using System.Collections;
using System.Text.RegularExpressions;

namespace UdpCicd.Core.Models;

/// <summary>All resource definitions in the deployment.</summary>
public sealed partial class ResourcesConfig
{
    public Dictionary<string, LakehouseResource> Lakehouses { get; set; } = [];
    public Dictionary<string, NotebookResource> Notebooks { get; set; } = [];
    public Dictionary<string, PipelineResource> Pipelines { get; set; } = [];
    public Dictionary<string, WarehouseResource> Warehouses { get; set; } = [];
    public Dictionary<string, SemanticModelResource> SemanticModels { get; set; } = [];
    public Dictionary<string, ReportResource> Reports { get; set; } = [];
    public Dictionary<string, DataAgentInstructions> DataAgents { get; set; } = [];
    public Dictionary<string, EnvironmentResource> Environments { get; set; } = [];
    public Dictionary<string, EventhouseResource> Eventhouses { get; set; } = [];
    public Dictionary<string, EventstreamResource> Eventstreams { get; set; } = [];
    public Dictionary<string, MLModelResource> MlModels { get; set; } = [];
    public Dictionary<string, MLExperimentResource> MlExperiments { get; set; } = [];
    public Dictionary<string, KQLDatabaseResource> KqlDatabases { get; set; } = [];
    public Dictionary<string, KQLDashboardResource> KqlDashboards { get; set; } = [];
    public Dictionary<string, KQLQuerysetResource> KqlQuerysets { get; set; } = [];
    public Dictionary<string, DataflowResource> Dataflows { get; set; } = [];
    public Dictionary<string, GraphQLApiResource> GraphqlApis { get; set; } = [];
    public Dictionary<string, SparkJobDefinitionResource> SparkJobDefinitions { get; set; } = [];
    public Dictionary<string, SQLDatabaseResource> SqlDatabases { get; set; } = [];
    public Dictionary<string, MirroredDatabaseResource> MirroredDatabases { get; set; } = [];
    public Dictionary<string, CopyJobResource> CopyJobs { get; set; } = [];
    public Dictionary<string, ApacheAirflowJobResource> AirflowJobs { get; set; } = [];
    public Dictionary<string, ReflexResource> Reflex { get; set; } = [];
    public Dictionary<string, MountedDataFactoryResource> MountedDataFactories { get; set; } = [];
    public Dictionary<string, UserDataFunctionResource> UserDataFunctions { get; set; } = [];
    public Dictionary<string, VariableLibraryResource> VariableLibraries { get; set; } = [];
    public Dictionary<string, OntologyResource> Ontologies { get; set; } = [];
    public Dictionary<string, GraphResource> Graphs { get; set; } = [];
    public Dictionary<string, DataBuildToolJobResource> DbtJobs { get; set; } = [];
    public Dictionary<string, DatamartResource> Datamarts { get; set; } = [];
    public Dictionary<string, PaginatedReportResource> PaginatedReports { get; set; } = [];
    public Dictionary<string, DashboardResource> Dashboards { get; set; } = [];
    public Dictionary<string, MirroredWarehouseResource> MirroredWarehouses { get; set; } = [];
    public Dictionary<string, SnowflakeDatabaseResource> SnowflakeDatabases { get; set; } = [];
    public Dictionary<string, CosmosDBDatabaseResource> CosmosdbDatabases { get; set; } = [];
    public Dictionary<string, MirroredDatabricksCatalogResource> MirroredDatabricksCatalogs { get; set; } = [];
    public Dictionary<string, OperationsAgentResource> OperationsAgents { get; set; } = [];
    public Dictionary<string, AnomalyDetectorResource> AnomalyDetectors { get; set; } = [];
    public Dictionary<string, DigitalTwinBuilderResource> DigitalTwinBuilders { get; set; } = [];
    public Dictionary<string, DigitalTwinBuilderFlowResource> DigitalTwinBuilderFlows { get; set; } = [];
    public Dictionary<string, EventSchemaSetResource> EventSchemaSets { get; set; } = [];
    public Dictionary<string, GraphQuerySetResource> GraphQuerySets { get; set; } = [];
    public Dictionary<string, MapResource> MapItems { get; set; } = [];
    public Dictionary<string, GraphModelResource> GraphModels { get; set; } = [];
    public Dictionary<string, HLSCohortResource> HlsCohorts { get; set; } = [];

    private static readonly Dictionary<string, System.Reflection.PropertyInfo> PropByField =
        ResourceTypeRegistry.All.ToDictionary(
            r => r.FieldName,
            r => typeof(ResourcesConfig).GetProperty(r.PropertyName)!);

    private IDictionary DictFor(ResourceTypeInfo info) =>
        (IDictionary)PropByField[info.FieldName].GetValue(this)!;

    /// <summary>Get the resource object stored under a snake_case field name + key, or null.</summary>
    public object? GetResourceObject(string fieldName, string key)
    {
        if (!PropByField.TryGetValue(fieldName, out var prop))
        {
            return null;
        }
        var dict = (IDictionary)prop.GetValue(this)!;
        return dict.Contains(key) ? dict[key] : null;
    }

    /// <summary>Return all resource keys across all types.</summary>
    public HashSet<string> AllResourceKeys()
    {
        var keys = new HashSet<string>();
        foreach (var info in ResourceTypeRegistry.All)
        {
            foreach (var key in DictFor(info).Keys)
            {
                keys.Add((string)key);
            }
        }
        return keys;
    }

    /// <summary>Return the resource type field name for a given key.</summary>
    public string? GetResourceType(string key)
    {
        foreach (var info in ResourceTypeRegistry.All)
        {
            if (DictFor(info).Contains(key))
            {
                return info.FieldName;
            }
        }
        return null;
    }

    [GeneratedRegex("^[a-zA-Z0-9_]+$")]
    private static partial Regex StrictNamePattern();

    [GeneratedRegex("^[a-zA-Z0-9_ -]+$")]
    private static partial Regex GeneralNamePattern();

    /// <summary>
    /// Validate resource names follow Fabric naming rules per item type.
    /// Returns the list of warnings (same text as the Python implementation).
    /// </summary>
    public List<string> ValidateResourceNames()
    {
        var warnings = new List<string>();
        foreach (var info in ResourceTypeRegistry.All)
        {
            foreach (var keyObj in DictFor(info).Keys)
            {
                var key = (string)keyObj;
                if (key.Length > 256)
                {
                    warnings.Add($"'{key}' exceeds 256 character limit");
                }
                if (key != key.Trim())
                {
                    warnings.Add($"'{key}' has leading/trailing whitespace");
                }
                if (info.StrictNaming)
                {
                    if (!StrictNamePattern().IsMatch(key))
                    {
                        warnings.Add(
                            $"'{key}' ({info.FieldName}): only letters, numbers, and underscores allowed. " +
                            "Hyphens and spaces are not supported.");
                    }
                }
                else if (!GeneralNamePattern().IsMatch(key))
                {
                    warnings.Add($"'{key}' ({info.FieldName}): contains invalid characters");
                }
            }
        }
        return warnings;
    }
}
