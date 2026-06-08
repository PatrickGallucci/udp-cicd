namespace UdpCicd.Core.Models;

/// <summary>
/// Metadata for a single resource type. <see cref="FieldName"/> is the
/// snake_case key used in <c>udp.yml</c> (and reported by diagnostics);
/// <see cref="PropertyName"/> is the corresponding <see cref="ResourcesConfig"/>
/// property; <see cref="FabricType"/> is the Microsoft Fabric item-type name;
/// <see cref="Folder"/> is the workspace folder the type is grouped under when
/// <c>workspace.folders_by_type</c> is enabled.
/// </summary>
/// <remarks>
/// <see cref="Folder"/> defaults to <c>"Other"</c>, so a newly added resource
/// type is always assigned a folder even if the author forgets to pick one —
/// no type can ever be left unfoldered.
/// </remarks>
public sealed record ResourceTypeInfo(
    string FieldName,
    string PropertyName,
    string FabricType,
    bool StrictNaming,
    string Folder = "Other");

/// <summary>
/// Single source of truth for the 45 supported resource types. Centralizes the
/// list that the Python code duplicated across the models, the resolver/planner,
/// and <c>ITEM_TYPE_MAP</c> in the Fabric API provider.
/// </summary>
public static class ResourceTypeRegistry
{
    // Strict-naming types: only letters, numbers and underscores allowed
    // (mirrors strict_name_types in ResourcesConfig.validate_resource_names).
    // Folder: the workspace folder each type is grouped under (folders_by_type).
    public static readonly IReadOnlyList<ResourceTypeInfo> All =
    [
        new("lakehouses", "Lakehouses", "Lakehouse", StrictNaming: true, Folder: "Lakehouses"),
        new("notebooks", "Notebooks", "Notebook", false, Folder: "Notebooks"),
        new("pipelines", "Pipelines", "DataPipeline", false, Folder: "Pipelines"),
        new("warehouses", "Warehouses", "Warehouse", StrictNaming: true, Folder: "Warehouses"),
        new("semantic_models", "SemanticModels", "SemanticModel", false, Folder: "Models"),
        new("reports", "Reports", "Report", false, Folder: "Reports"),
        new("data_agents", "DataAgents", "DataAgent", false, Folder: "Agents"),
        new("environments", "Environments", "Environment", false, Folder: "Environments"),
        new("eventhouses", "Eventhouses", "Eventhouse", StrictNaming: true, Folder: "Real-Time"),
        new("eventstreams", "Eventstreams", "Eventstream", false, Folder: "Real-Time"),
        new("ml_models", "MlModels", "MLModel", false, Folder: "Models"),
        new("ml_experiments", "MlExperiments", "MLExperiment", false, Folder: "Models"),
        new("kql_databases", "KqlDatabases", "KQLDatabase", StrictNaming: true, Folder: "Databases"),
        new("kql_dashboards", "KqlDashboards", "KQLDashboard", false, Folder: "Real-Time"),
        new("kql_querysets", "KqlQuerysets", "KQLQueryset", false, Folder: "Real-Time"),
        new("dataflows", "Dataflows", "Dataflow", false, Folder: "Data Factory"),
        new("graphql_apis", "GraphqlApis", "GraphQLApi", false, Folder: "Data Engineering"),
        new("spark_job_definitions", "SparkJobDefinitions", "SparkJobDefinition", false, Folder: "Data Engineering"),
        new("sql_databases", "SqlDatabases", "SQLDatabase", StrictNaming: true, Folder: "Databases"),
        new("mirrored_databases", "MirroredDatabases", "MirroredDatabase", false, Folder: "Databases"),
        new("copy_jobs", "CopyJobs", "CopyJob", false, Folder: "Data Factory"),
        new("airflow_jobs", "AirflowJobs", "ApacheAirflowJob", false, Folder: "Data Factory"),
        new("reflex", "Reflex", "Reflex", false, Folder: "Real-Time"),
        new("mounted_data_factories", "MountedDataFactories", "MountedDataFactory", false, Folder: "Data Factory"),
        new("user_data_functions", "UserDataFunctions", "UserDataFunction", false, Folder: "Data Engineering"),
        new("variable_libraries", "VariableLibraries", "VariableLibrary", false, Folder: "Variables"),
        new("ontologies", "Ontologies", "Ontology", false, Folder: "Real-Time"),
        new("graphs", "Graphs", "Graph", false, Folder: "Graph"),
        new("dbt_jobs", "DbtJobs", "DataBuildToolJob", false, Folder: "Data Factory"),
        new("datamarts", "Datamarts", "Datamart", false, Folder: "Databases"),
        new("paginated_reports", "PaginatedReports", "PaginatedReport", false, Folder: "Reports"),
        new("dashboards", "Dashboards", "Dashboard", false, Folder: "Reports"),
        new("mirrored_warehouses", "MirroredWarehouses", "MirroredWarehouse", false, Folder: "Databases"),
        new("snowflake_databases", "SnowflakeDatabases", "SnowflakeDatabase", false, Folder: "Databases"),
        new("cosmosdb_databases", "CosmosdbDatabases", "CosmosDBDatabase", false, Folder: "Databases"),
        new("mirrored_databricks_catalogs", "MirroredDatabricksCatalogs", "MirroredAzureDatabricksCatalog", false, Folder: "Databases"),
        new("operations_agents", "OperationsAgents", "OperationsAgent", false, Folder: "Agents"),
        new("anomaly_detectors", "AnomalyDetectors", "AnomalyDetector", false, Folder: "Real-Time"),
        new("digital_twin_builders", "DigitalTwinBuilders", "DigitalTwinBuilder", false, Folder: "Real-Time"),
        new("digital_twin_builder_flows", "DigitalTwinBuilderFlows", "DigitalTwinBuilderFlow", false, Folder: "Real-Time"),
        new("event_schema_sets", "EventSchemaSets", "EventSchemaSet", false, Folder: "Real-Time"),
        new("graph_query_sets", "GraphQuerySets", "GraphQuerySet", false, Folder: "Graph"),
        new("map_items", "MapItems", "Map", false, Folder: "Maps"),
        new("graph_models", "GraphModels", "GraphModel", false, Folder: "Graph"),
        new("hls_cohorts", "HlsCohorts", "HLSCohort", false, Folder: "Healthcare"),
    ];

    /// <summary>Fabric item type name for a snake_case resource field name.</summary>
    public static readonly IReadOnlyDictionary<string, string> ItemTypeMap =
        All.ToDictionary(r => r.FieldName, r => r.FabricType);

    /// <summary>
    /// Workspace folder name each resource type is grouped under when
    /// <c>workspace.folders_by_type</c> is enabled, keyed by snake_case field
    /// name. Derived from <see cref="ResourceTypeInfo.Folder"/>, so every type —
    /// including any added later — is always present.
    /// </summary>
    public static readonly IReadOnlyDictionary<string, string> TypeFolders =
        All.ToDictionary(r => r.FieldName, r => r.Folder);

    /// <summary>The per-type workspace folder for a snake_case field name.
    /// Falls back to <c>"Other"</c> for an unknown field name.</summary>
    public static string FolderFor(string fieldName) => TypeFolders.GetValueOrDefault(fieldName, "Other");

    /// <summary>Item types that are list-only — cannot be created/deleted via API.</summary>
    public static readonly IReadOnlySet<string> ListOnlyTypes = new HashSet<string>
    {
        "Datamart", "MirroredWarehouse", "SQLEndpoint", "Dashboard", "PaginatedReport",
    };

    /// <summary>Item types that REQUIRE a definition to create (cannot create empty).</summary>
    public static readonly IReadOnlySet<string> DefinitionRequiredTypes = new HashSet<string>
    {
        "MountedDataFactory", "MirroredDatabase", "Report", "SemanticModel",
    };

    /// <summary>Item types where definition upload is not supported.</summary>
    public static readonly IReadOnlySet<string> NoDefinitionTypes = new HashSet<string>
    {
        "MLModel", "MLExperiment", "Warehouse",
    };
}
