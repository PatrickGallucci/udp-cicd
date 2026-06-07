namespace UdpCicd.Core.Models;

/// <summary>
/// Metadata for a single resource type. <see cref="FieldName"/> is the
/// snake_case key used in <c>udp.yml</c> (and reported by diagnostics);
/// <see cref="PropertyName"/> is the corresponding <see cref="ResourcesConfig"/>
/// property; <see cref="FabricType"/> is the Microsoft Fabric item-type name.
/// </summary>
public sealed record ResourceTypeInfo(
    string FieldName,
    string PropertyName,
    string FabricType,
    bool StrictNaming);

/// <summary>
/// Single source of truth for the 45 supported resource types. Centralizes the
/// list that the Python code duplicated across the models, the resolver/planner,
/// and <c>ITEM_TYPE_MAP</c> in the Fabric API provider.
/// </summary>
public static class ResourceTypeRegistry
{
    // Strict-naming types: only letters, numbers and underscores allowed
    // (mirrors strict_name_types in ResourcesConfig.validate_resource_names).
    public static readonly IReadOnlyList<ResourceTypeInfo> All =
    [
        new("lakehouses", "Lakehouses", "Lakehouse", StrictNaming: true),
        new("notebooks", "Notebooks", "Notebook", false),
        new("pipelines", "Pipelines", "DataPipeline", false),
        new("warehouses", "Warehouses", "Warehouse", StrictNaming: true),
        new("semantic_models", "SemanticModels", "SemanticModel", false),
        new("reports", "Reports", "Report", false),
        new("data_agents", "DataAgents", "DataAgent", false),
        new("environments", "Environments", "Environment", false),
        new("eventhouses", "Eventhouses", "Eventhouse", StrictNaming: true),
        new("eventstreams", "Eventstreams", "Eventstream", false),
        new("ml_models", "MlModels", "MLModel", false),
        new("ml_experiments", "MlExperiments", "MLExperiment", false),
        new("kql_databases", "KqlDatabases", "KQLDatabase", StrictNaming: true),
        new("kql_dashboards", "KqlDashboards", "KQLDashboard", false),
        new("kql_querysets", "KqlQuerysets", "KQLQueryset", false),
        new("dataflows", "Dataflows", "Dataflow", false),
        new("graphql_apis", "GraphqlApis", "GraphQLApi", false),
        new("spark_job_definitions", "SparkJobDefinitions", "SparkJobDefinition", false),
        new("sql_databases", "SqlDatabases", "SQLDatabase", StrictNaming: true),
        new("mirrored_databases", "MirroredDatabases", "MirroredDatabase", false),
        new("copy_jobs", "CopyJobs", "CopyJob", false),
        new("airflow_jobs", "AirflowJobs", "ApacheAirflowJob", false),
        new("reflex", "Reflex", "Reflex", false),
        new("mounted_data_factories", "MountedDataFactories", "MountedDataFactory", false),
        new("user_data_functions", "UserDataFunctions", "UserDataFunction", false),
        new("variable_libraries", "VariableLibraries", "VariableLibrary", false),
        new("ontologies", "Ontologies", "Ontology", false),
        new("graphs", "Graphs", "Graph", false),
        new("dbt_jobs", "DbtJobs", "DataBuildToolJob", false),
        new("datamarts", "Datamarts", "Datamart", false),
        new("paginated_reports", "PaginatedReports", "PaginatedReport", false),
        new("dashboards", "Dashboards", "Dashboard", false),
        new("mirrored_warehouses", "MirroredWarehouses", "MirroredWarehouse", false),
        new("snowflake_databases", "SnowflakeDatabases", "SnowflakeDatabase", false),
        new("cosmosdb_databases", "CosmosdbDatabases", "CosmosDBDatabase", false),
        new("mirrored_databricks_catalogs", "MirroredDatabricksCatalogs", "MirroredAzureDatabricksCatalog", false),
        new("operations_agents", "OperationsAgents", "OperationsAgent", false),
        new("anomaly_detectors", "AnomalyDetectors", "AnomalyDetector", false),
        new("digital_twin_builders", "DigitalTwinBuilders", "DigitalTwinBuilder", false),
        new("digital_twin_builder_flows", "DigitalTwinBuilderFlows", "DigitalTwinBuilderFlow", false),
        new("event_schema_sets", "EventSchemaSets", "EventSchemaSet", false),
        new("graph_query_sets", "GraphQuerySets", "GraphQuerySet", false),
        new("map_items", "MapItems", "Map", false),
        new("graph_models", "GraphModels", "GraphModel", false),
        new("hls_cohorts", "HlsCohorts", "HLSCohort", false),
    ];

    /// <summary>Fabric item type name for a snake_case resource field name.</summary>
    public static readonly IReadOnlyDictionary<string, string> ItemTypeMap =
        All.ToDictionary(r => r.FieldName, r => r.FabricType);

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
