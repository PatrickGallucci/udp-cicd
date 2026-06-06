using System.Net;
using System.Net.Http.Headers;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Azure.Core;
using Azure.Identity;

namespace UdpCicd.Core.Providers;

/// <summary>
/// Microsoft Graph API client — resolves Entra ID display names to object GUIDs.
/// Used by the deployer to convert human-readable security role references
/// (e.g. <c>sg-data-engineering</c>) into the GUIDs the Fabric API requires.
/// Mirrors <c>providers/graph_api.py</c>.
/// </summary>
public sealed partial class GraphClient
{
    public const string GraphApiBase = "https://graph.microsoft.com/v1.0";
    public const string GraphScope = "https://graph.microsoft.com/.default";

    [GeneratedRegex("^[0-9a-f]{8}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{4}-[0-9a-f]{12}$", RegexOptions.IgnoreCase)]
    private static partial Regex GuidPattern();

    public static bool IsGuid(string value) => GuidPattern().IsMatch(value);

    private readonly TokenCredential _credential;
    private readonly HttpClient _http;
    private string? _token;
    private readonly Dictionary<string, string?> _cache = [];

    public GraphClient(TokenCredential? credential = null, HttpClient? httpClient = null)
    {
        _credential = credential ?? new DefaultAzureCredential();
        _http = httpClient ?? new HttpClient { Timeout = TimeSpan.FromSeconds(30) };
    }

    private string GetToken() =>
        _token ??= _credential.GetToken(new TokenRequestContext([GraphScope]), CancellationToken.None).Token;

    private JsonNode? Request(string method, string path, IDictionary<string, string>? queryParams = null)
    {
        var url = GraphApiBase + path;
        if (queryParams is { Count: > 0 })
        {
            url += "?" + string.Join("&", queryParams.Select(kv =>
                $"{Uri.EscapeDataString(kv.Key)}={Uri.EscapeDataString(kv.Value)}"));
        }

        try
        {
            var resp = Send(method, url);
            if (resp.StatusCode == HttpStatusCode.Unauthorized)
            {
                _token = null;
                resp.Dispose();
                resp = Send(method, url);
            }
            using (resp)
            {
                if ((int)resp.StatusCode >= 400)
                {
                    return null;
                }
                var text = resp.Content.ReadAsStringAsync().GetAwaiter().GetResult();
                return string.IsNullOrEmpty(text) ? null : JsonNode.Parse(text);
            }
        }
        catch (HttpRequestException)
        {
            return null;
        }
    }

    private HttpResponseMessage Send(string method, string url)
    {
        var request = new HttpRequestMessage(new HttpMethod(method), url);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", GetToken());
        return _http.Send(request);
    }

    /// <summary>Resolve a group display name to its object ID.</summary>
    public string? ResolveGroup(string displayName)
    {
        if (IsGuid(displayName))
        {
            return displayName;
        }
        var cacheKey = $"group:{displayName}";
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }
        var result = Request("GET", "/groups",
            new Dictionary<string, string> { ["$filter"] = $"displayName eq '{displayName}'", ["$select"] = "id" });
        var guid = (result?["value"] as JsonArray)?.FirstOrDefault()?["id"]?.GetValue<string>();
        _cache[cacheKey] = guid;
        return guid;
    }

    /// <summary>Resolve a UPN or display name to a user object ID.</summary>
    public string? ResolveUser(string userPrincipalName)
    {
        if (IsGuid(userPrincipalName))
        {
            return userPrincipalName;
        }
        var cacheKey = $"user:{userPrincipalName}";
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var byUpn = Request("GET", $"/users/{userPrincipalName}",
            new Dictionary<string, string> { ["$select"] = "id" });
        if (byUpn?["id"] is { } idNode)
        {
            var id = idNode.GetValue<string>();
            _cache[cacheKey] = id;
            return id;
        }

        var byName = Request("GET", "/users",
            new Dictionary<string, string> { ["$filter"] = $"displayName eq '{userPrincipalName}'", ["$select"] = "id" });
        var guid = (byName?["value"] as JsonArray)?.FirstOrDefault()?["id"]?.GetValue<string>();
        _cache[cacheKey] = guid;
        return guid;
    }

    /// <summary>Resolve a service principal display name or app ID to its object ID.</summary>
    public string? ResolveServicePrincipal(string displayName)
    {
        if (IsGuid(displayName))
        {
            return displayName;
        }
        var cacheKey = $"sp:{displayName}";
        if (_cache.TryGetValue(cacheKey, out var cached))
        {
            return cached;
        }

        var byName = Request("GET", "/servicePrincipals",
            new Dictionary<string, string> { ["$filter"] = $"displayName eq '{displayName}'", ["$select"] = "id" });
        var byNameId = (byName?["value"] as JsonArray)?.FirstOrDefault()?["id"]?.GetValue<string>();
        if (byNameId is not null)
        {
            _cache[cacheKey] = byNameId;
            return byNameId;
        }

        var byApp = Request("GET", "/servicePrincipals",
            new Dictionary<string, string> { ["$filter"] = $"appId eq '{displayName}'", ["$select"] = "id" });
        var guid = (byApp?["value"] as JsonArray)?.FirstOrDefault()?["id"]?.GetValue<string>();
        _cache[cacheKey] = guid;
        return guid;
    }

    /// <summary>Resolve any principal type to its GUID.</summary>
    public string? ResolvePrincipal(string value, string principalType = "Group") => principalType switch
    {
        "User" => ResolveUser(value),
        "ServicePrincipal" => ResolveServicePrincipal(value),
        _ => ResolveGroup(value),
    };
}
