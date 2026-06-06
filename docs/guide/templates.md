# Templates

Templates are starter projects that generate a complete `udp.yml`, directory structure, and sample definition files. They save you from writing boilerplate and give you a working project in seconds.

## Using templates

```bash
udp-cicd init --template <template_name> --name <project_name>
```

This creates a new directory with the project name containing a fully configured deployment.

## Built-in templates

### `blank`

A minimal starting point with an empty `udp.yml` and the standard directory structure.

```bash
udp-cicd init --template blank --name udp-project
```

Creates:

```
udp-project/
├── udp.yml
├── notebooks/
├── sql/
├── pipelines/
├── agent/
└── .gitignore
```

The generated `udp.yml` contains the `deployment`, `workspace`, `variables`, `resources`, and `targets` sections with placeholder comments. Add your own resources and fill in the workspace details.

### `medallion`

A Bronze/Silver/Gold lakehouse architecture with ETL notebooks, data pipelines, and a Data Agent. This is the most common pattern for data engineering projects on Fabric.

```bash
udp-cicd init --template medallion --name contoso-analytics
```

Creates:

```
contoso-analytics/
├── udp.yml
├── notebooks/
│   ├── ingest_to_bronze.py
│   ├── bronze_to_silver.py
│   └── silver_to_gold.py
├── sql/
│   └── gold_views.sql
├── pipelines/
│   └── daily_ingest.yml
├── agent/
│   └── analytics_agent.yml
└── .gitignore
```

Resources defined in `udp.yml`:

| Resource | Type | Description |
|----------|------|-------------|
| `bronze` | Lakehouse | Raw data landing zone |
| `silver` | Lakehouse | Cleansed and conformed data |
| `gold` | Lakehouse | Business-ready aggregates |
| `ingest_to_bronze` | Notebook | Ingests raw data into bronze |
| `bronze_to_silver` | Notebook | Transforms bronze to silver |
| `silver_to_gold` | Notebook | Aggregates silver into gold |
| `daily_ingest` | Data Pipeline | Orchestrates the ETL flow |
| `analytics_agent` | Data Agent | Natural language query agent over gold |
| `spark_env` | Environment | Spark runtime with library dependencies |

The template includes dev and prod targets with variable overrides for database connections.


## Creating custom templates

A custom template is a directory containing a `template.yml` manifest and a set of files that will be copied into the new project. File contents and names can include Jinja2 template variables.

### Directory structure

```
udp-custom-template/
├── template.yml
├── udp.yml
├── notebooks/
│   └── setup.py
├── sql/
│   └── init.sql
└── .gitignore
```

### `template.yml` format

```yaml
name: udp-custom-template
description: "A custom template for our team's standard project layout."
version: "1.0.0"
author: "Data Platform Team"

variables:
  project_name:
    description: "Project name (used in resource naming)"
    default: "udp-project"

  lakehouse_count:
    description: "Number of lakehouses to create"
    type: number
    default: 2

  include_agent:
    description: "Include a Data Agent"
    type: boolean
    default: true
```

When a user runs `udp-cicd init --template ./udp-custom-template`, they are prompted for each variable (or the default is used).

### Jinja2 variables in template files

All files in the template directory are processed as Jinja2 templates. Use `{{ variable_name }}` for substitution and `{% if %}` / `{% for %}` for conditional and repeated blocks.

In `udp.yml`:

```yaml
deployment:
  name: {{ project_name }}
  version: "1.0.0"

resources:
  lakehouses:
    {% for i in range(lakehouse_count) %}
    layer_{{ i }}:
      display_name: "{{ project_name }}_layer_{{ i }}"
      description: "Data layer {{ i }}"
    {% endfor %}

  {% if include_agent %}
  data_agents:
    assistant:
      display_name: "{{ project_name }}_agent"
      description: "Data Agent for {{ project_name }}"
      instruction_file: agent/assistant.yml
  {% endif %}
```

### Built-in template variables

These variables are always available in addition to the ones you define in `template.yml`:

| Variable | Description |
|----------|-------------|
| `project_name` | The `--name` value passed to `udp-cicd init` |
| `timestamp` | ISO 8601 timestamp of project creation |
| `udp_deployment_version` | Version of the installed udp-cicd package |

## Remote templates

Templates can be loaded from a URL or a GitHub repository. This allows teams to share standard templates without copying files.

### URL-based templates

Point to a `.tar.gz` or `.zip` archive containing the template directory:

```bash
udp-cicd init --template https://example.com/templates/data-mesh-v2.tar.gz --name udp-project
```

### GitHub shorthand

Use the `github:` prefix to reference a template in a GitHub repository:

```bash
# Uses the repository root as the template
udp-cicd init --template github:contoso/udp-templates --name udp-project

# Uses a subdirectory within the repository
udp-cicd init --template github:contoso/udp-templates/medallion-v2 --name udp-project

# Uses a specific branch or tag
udp-cicd init --template github:contoso/udp-templates@v3.0 --name udp-project
```

The repository must contain a `template.yml` at the root (or specified subdirectory). Public repositories are accessible without authentication. For private repositories, udp-cicd uses the `GITHUB_TOKEN` environment variable.

## Template variables at init time

When running `udp-cicd init`, variables are resolved in this order:

1. **Command-line flags**: `--var project_name=udp-project`
2. **Interactive prompt**: If a required variable has no default and was not provided on the command line, udp-cicd prompts for it.
3. **Default values**: From `template.yml`.

```bash
# Provide all variables on the command line (no prompts)
udp-cicd init \
  --template medallion \
  --name contoso-analytics \
  --var lakehouse_prefix=contoso \
  --var include_agent=true
```
