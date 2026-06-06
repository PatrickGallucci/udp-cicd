using UdpCicd.Core.Engine;
using UdpCicd.Core.Models;

namespace UdpCicd.Core.Tests;

public class LoaderTests
{
    /// <summary>Locate the repo root by walking up to the folder containing 'examples'.</summary>
    private static string RepoRoot()
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !Directory.Exists(Path.Combine(dir.FullName, "examples")))
        {
            dir = dir.Parent;
        }
        return dir?.FullName ?? throw new DirectoryNotFoundException("Could not locate repo root (examples/).");
    }

    private static string ExamplePath(string rel) =>
        Path.Combine(RepoRoot(), "examples", rel.Replace('/', Path.DirectorySeparatorChar));

    [Fact]
    public void Loads_Medallion_Example()
    {
        var path = ExamplePath("02-medallion-lakehouse/udp.yml");
        var deployment = Loader.LoadDeployment(path, target: "dev");

        Assert.Equal("medallion-analytics", deployment.Deployment.Name);
        Assert.Equal("1.0.0", deployment.Deployment.Version);

        // Resources bound correctly.
        Assert.Equal(3, deployment.Resources.Lakehouses.Count);
        Assert.True(deployment.Resources.Lakehouses["bronze"].EnableSchemas);
        Assert.Equal(3, deployment.Resources.Notebooks.Count);
        Assert.Equal("spark_env", deployment.Resources.Notebooks["ingest_to_bronze"].Environment);
        Assert.Equal("bronze", deployment.Resources.Notebooks["ingest_to_bronze"].DefaultLakehouse);

        // Pipeline activities + depends_on.
        var pipeline = deployment.Resources.Pipelines["daily_etl"];
        Assert.Equal(3, pipeline.Activities.Count);
        Assert.Equal("0 6 * * *", pipeline.Schedule!.Cron);
        Assert.Contains("ingest", pipeline.Activities[1].DependsOn);

        // Security roles + enum mapping (contributor / read).
        Assert.Equal(2, deployment.Security.Roles.Count);
        Assert.Equal(WorkspaceRole.Contributor, deployment.Security.Roles[0].WorkspaceRole);
        Assert.Equal(WorkspaceRole.Viewer, deployment.Security.Roles[1].WorkspaceRole);
        Assert.Contains(OneLakePermission.Read, deployment.Security.Roles[1].OnelakeRoles[0].Permissions);

        // Variable substitution: dev target injects capacity_id default into workspace.
        var ws = deployment.GetEffectiveWorkspace("dev");
        Assert.Equal("medallion-dev", ws.Name);
        Assert.Equal("REPLACE-WITH-YOUR-CAPACITY-GUID", ws.CapacityId);
    }

    [Fact]
    public void Loads_Minimal_Example()
    {
        var path = ExamplePath("01-minimal/udp.yml");
        var deployment = Loader.LoadDeployment(path);
        Assert.False(string.IsNullOrEmpty(deployment.Deployment.Name));
    }
}
