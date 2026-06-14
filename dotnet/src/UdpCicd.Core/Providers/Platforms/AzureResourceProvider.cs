using System.Text;
using Spectre.Console;
using UdpCicd.Core.Engine;
using UdpCicd.Core.Models;

namespace UdpCicd.Core.Providers.Platforms;

/// <summary>
/// Deploys Azure (ARM) resources through Bicep via the <c>az</c> CLI. Resource
/// groups and storage accounts are emitted as generated Bicep templates;
/// arbitrary resources use an author-supplied <c>.bicep</c> file. Deployments are
/// incremental and idempotent (re-applying converges), so create and update share
/// a path.
/// </summary>
public sealed class AzureResourceProvider : IResourcePlatformProvider
{
    private readonly AzureCli _az;

    public AzureResourceProvider(AzureCli? az = null) => _az = az ?? new AzureCli();

    public ResourcePlatform Platform => ResourcePlatform.Azure;

    public bool? Apply(PlanItem item, PlatformDeployContext ctx) => item.ResourceType switch
    {
        "Microsoft.Resources/resourceGroups" => ApplyResourceGroup(item, ctx),
        "Microsoft.Storage/storageAccounts" => ApplyStorageAccount(item, ctx),
        "Microsoft.Resources/deployments" => ApplyBicep(item, ctx),
        _ => Skip(ctx, item, $"unsupported Azure type '{item.ResourceType}'"),
    };

    // ----- Resource groups (subscription-scope Bicep) -----

    private bool? ApplyResourceGroup(PlanItem item, PlatformDeployContext ctx)
    {
        var name = item.ResourceKey;
        ctx.Deployment.Resources.AzureResourceGroups.TryGetValue(name, out var rg);
        rg ??= new AzureResourceGroupResource();
        var sub = rg.Subscription ?? ctx.Deployment.Azure.Subscription;
        var location = rg.Location ?? ctx.Deployment.Azure.Location;

        if (item.Action == PlanAction.Delete)
        {
            return DeleteRg(ctx, name, sub);
        }
        if (string.IsNullOrEmpty(location))
        {
            return Skip(ctx, item, "no location (set azure.location or the resource's location)");
        }
        if (ctx.DryRun)
        {
            return Would(ctx, "resource group", $"{name} in {location}");
        }

        var bicep = new StringBuilder()
            .AppendLine("targetScope = 'subscription'")
            .AppendLine($"resource rg 'Microsoft.Resources/resourceGroups@2021-04-01' = {{")
            .AppendLine($"  name: '{Esc(name)}'")
            .AppendLine($"  location: '{Esc(location)}'")
            .AppendLine($"  tags: {TagsBicep(rg.Tags)}")
            .AppendLine("}")
            .ToString();

        return DeployTemplate(ctx, bicep, name, sub, args =>
        {
            args.Add("deployment");
            args.Add("sub");
            args.Add("create");
            args.Add("--location");
            args.Add(location);
        }, recordKey: name);
    }

    // ----- Storage accounts (resource-group-scope Bicep) -----

    private bool? ApplyStorageAccount(PlanItem item, PlatformDeployContext ctx)
    {
        var name = item.ResourceKey;
        ctx.Deployment.Resources.AzureStorageAccounts.TryGetValue(name, out var sa);
        if (sa is null || string.IsNullOrEmpty(sa.ResourceGroup))
        {
            return Skip(ctx, item, "storage account requires a resource_group");
        }
        var sub = sa.Subscription ?? ctx.Deployment.Azure.Subscription;
        var location = sa.Location ?? ctx.Deployment.Azure.Location;

        if (item.Action == PlanAction.Delete)
        {
            return DeleteStorage(ctx, name, sa.ResourceGroup, sub);
        }
        if (string.IsNullOrEmpty(location))
        {
            return Skip(ctx, item, "no location (set azure.location or the resource's location)");
        }
        if (ctx.DryRun)
        {
            return Would(ctx, "storage account", $"{name} in {sa.ResourceGroup}");
        }

        var bicep = new StringBuilder()
            .AppendLine($"resource sa 'Microsoft.Storage/storageAccounts@2023-01-01' = {{")
            .AppendLine($"  name: '{Esc(name)}'")
            .AppendLine($"  location: '{Esc(location)}'")
            .AppendLine($"  sku: {{ name: '{Esc(sa.Sku)}' }}")
            .AppendLine($"  kind: '{Esc(sa.Kind)}'")
            .AppendLine("  properties: {")
            .AppendLine($"    accessTier: '{Esc(sa.AccessTier)}'")
            .AppendLine($"    supportsHttpsTrafficOnly: {(sa.HttpsOnly ? "true" : "false")}")
            .AppendLine("  }")
            .AppendLine($"  tags: {TagsBicep(sa.Tags)}")
            .AppendLine("}")
            .ToString();

        return DeployTemplate(ctx, bicep, name, sub, args =>
        {
            args.Add("deployment");
            args.Add("group");
            args.Add("create");
            args.Add("--resource-group");
            args.Add(sa.ResourceGroup);
        }, recordKey: null);
    }

    // ----- Generic author-supplied Bicep -----

    private bool? ApplyBicep(PlanItem item, PlatformDeployContext ctx)
    {
        var name = item.ResourceKey;
        ctx.Deployment.Resources.AzureDeployments.TryGetValue(name, out var dep);
        if (dep is null || string.IsNullOrEmpty(dep.TemplateFile))
        {
            return Skip(ctx, item, "generic Azure deployment requires a template_file");
        }
        if (item.Action == PlanAction.Delete)
        {
            return Skip(ctx, item, "delete is not supported for generic Bicep deployments — remove the resources by hand or via their resource group");
        }

        var sub = dep.Subscription ?? ctx.Deployment.Azure.Subscription;
        var templatePath = Path.IsPathRooted(dep.TemplateFile)
            ? dep.TemplateFile
            : Path.Combine(ctx.ProjectDir, dep.TemplateFile);
        if (!File.Exists(templatePath))
        {
            return Skip(ctx, item, $"template file not found: {templatePath}");
        }

        var isSub = string.Equals(dep.Scope, "subscription", StringComparison.OrdinalIgnoreCase);
        if (isSub && string.IsNullOrEmpty(dep.Location ?? ctx.Deployment.Azure.Location))
        {
            return Skip(ctx, item, "subscription-scope deployment needs a location");
        }
        if (!isSub && string.IsNullOrEmpty(dep.ResourceGroup))
        {
            return Skip(ctx, item, "group-scope deployment needs a resource_group");
        }
        if (ctx.DryRun)
        {
            return Would(ctx, "bicep deployment", $"{name} ({dep.Scope})");
        }

        var args = new List<string> { "deployment", isSub ? "sub" : "group", "create" };
        if (isSub)
        {
            args.Add("--location");
            args.Add(dep.Location ?? ctx.Deployment.Azure.Location!);
        }
        else
        {
            args.Add("--resource-group");
            args.Add(dep.ResourceGroup!);
        }
        AddCommon(args, sub, name, templatePath);
        foreach (var (k, v) in dep.Parameters)
        {
            args.Add("--parameters");
            args.Add($"{k}={v}");
        }

        _az.RunChecked(args);
        ctx.Console.MarkupLine($"  [green]+[/] Deployed Azure (bicep): {Markup.Escape(name)}");
        return true;
    }

    // ----- Shared helpers -----

    private bool? DeployTemplate(PlatformDeployContext ctx, string bicep, string name, string? sub,
        Action<List<string>> scopeArgs, string? recordKey)
    {
        var tempPath = Path.Combine(Path.GetTempPath(), $"udp-{Sanitize(name)}-{Guid.NewGuid():N}.bicep");
        File.WriteAllText(tempPath, bicep);
        try
        {
            var args = new List<string>();
            scopeArgs(args);
            AddCommon(args, sub, name, tempPath);
            _az.RunChecked(args);
            if (recordKey is not null)
            {
                ctx.RecordRollback(recordKey, name);
            }
            ctx.Console.MarkupLine($"  [green]+[/] Deployed Azure: {Markup.Escape(name)}");
            return true;
        }
        finally
        {
            try { File.Delete(tempPath); } catch { /* best effort */ }
        }
    }

    private static void AddCommon(List<string> args, string? sub, string name, string templatePath)
    {
        args.Add("--name");
        args.Add($"udp-{Sanitize(name)}");
        args.Add("--template-file");
        args.Add(templatePath);
        if (!string.IsNullOrEmpty(sub))
        {
            args.Add("--subscription");
            args.Add(sub);
        }
        args.Add("--only-show-errors");
    }

    private bool? DeleteRg(PlatformDeployContext ctx, string name, string? sub)
    {
        if (ctx.DryRun)
        {
            ctx.Console.MarkupLine($"  [red]-[/] Would delete Azure resource group: {Markup.Escape(name)}");
            return true;
        }
        var args = new List<string> { "group", "delete", "--name", name, "--yes" };
        if (!string.IsNullOrEmpty(sub))
        {
            args.Add("--subscription");
            args.Add(sub);
        }
        _az.RunChecked(args);
        ctx.Console.MarkupLine($"  [red]-[/] Deleted Azure resource group: {Markup.Escape(name)}");
        return true;
    }

    private bool? DeleteStorage(PlatformDeployContext ctx, string name, string rg, string? sub)
    {
        if (ctx.DryRun)
        {
            ctx.Console.MarkupLine($"  [red]-[/] Would delete Azure storage account: {Markup.Escape(name)}");
            return true;
        }
        var args = new List<string> { "storage", "account", "delete", "--name", name, "--resource-group", rg, "--yes" };
        if (!string.IsNullOrEmpty(sub))
        {
            args.Add("--subscription");
            args.Add(sub);
        }
        _az.RunChecked(args);
        ctx.Console.MarkupLine($"  [red]-[/] Deleted Azure storage account: {Markup.Escape(name)}");
        return true;
    }

    private static bool? Would(PlatformDeployContext ctx, string kind, string detail)
    {
        ctx.Console.MarkupLine($"  [green]+[/] Would deploy Azure {kind}: {Markup.Escape(detail)}");
        return true;
    }

    private static bool? Skip(PlatformDeployContext ctx, PlanItem item, string why)
    {
        ctx.Console.MarkupLine($"  [yellow]![/] {Markup.Escape(item.ResourceKey)}: {Markup.Escape(why)} — skipping");
        return null;
    }

    private static string TagsBicep(Dictionary<string, string> tags) =>
        tags.Count == 0
            ? "{}"
            : "{\n" + string.Join("\n", tags.Select(t => $"    '{Esc(t.Key)}': '{Esc(t.Value)}'")) + "\n  }";

    private static string Esc(string value) => value.Replace("'", "\\'");

    private static string Sanitize(string name)
    {
        var cleaned = new string(name.Select(c => char.IsLetterOrDigit(c) || c is '-' or '_' ? c : '-').ToArray());
        return string.IsNullOrEmpty(cleaned) ? "deployment" : cleaned;
    }
}
