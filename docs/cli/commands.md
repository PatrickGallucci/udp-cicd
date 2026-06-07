# CLI command reference

This topic provides a complete reference for all `udp-cicd` CLI commands, including syntax, options, usage examples, and example output. The CLI is distributed as a .NET global tool; see [Installation](../getting-started/installation.md) for setup.

---

## 1. Global options

The following options are available on all commands unless otherwise noted.

| Option | Description |
|---|---|
| `--version` | Print the installed version and exit. |
| `--help` | Show help text for any command. |

```bash
udp-cicd --version
udp-cicd --help
udp-cicd deploy --help
```

---

## 2. Command summary

| Category | Command | Description |
|---|---|---|
| **Project setup** | [init](#init) | Create a new deployment project from a template. |
| | [list](#list) | List available deployment templates. |
| | [diag](#diag) | Diagnose configuration issues. |
| | [check-update](#check-update) | Check if a newer version is available. |
| **Validation** | [validate](#validate) | Validate the deployment definition. |
| | [graph](#graph) | Visualize the dependency graph. |
| **Planning and deployment** | [plan](#plan) | Preview what changes would be made. |
| | [deploy](#deploy) | Deploy the deployment to a target workspace. |
| | [destroy](#destroy) | Tear down all deployment-managed resources. |
| | [promote](#promote) | Promote artifacts from one target to another. |
| **Operations** | [status](#status) | Show deployed resource health. |
| | [drift](#drift) | Detect drift between state and live workspace. |
| | [diff](#diff) | Show definition-level diff (local vs. deployed). |
| | [history](#history) | Show deployment history. |
| | [rollback](#rollback) | Roll back to a previous deployment. |
| | [watch](#watch) | Auto-deploy on file changes. |
| **Resource management** | [run](#run) | Run a notebook or pipeline. |
| | [export](#export) | Export definitions from a deployed workspace. |
| | [generate](#generate) | Generate udp.yml from an existing workspace. |
| | [bind](#bind) | Bind an existing workspace item to deployment management. |
| | [import](#import) | Import resources from Terraform state or a workspace. |
| **Admin** | [admin plan](#admin-plan) | Preview tenant (admin) setting changes against the live tenant. |
| | [admin apply](#admin-apply) | Apply org-wide tenant settings via the Fabric Admin API. |

---

## 3. Project setup commands

<a id="init"></a>

### 3.1 init

Create a new deployment project from a template.

#### Syntax

```
udp-cicd init [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--template` | `-t` | String | `blank` | Template name or path. Use `udp-cicd list` to see available templates. |
| `--name` | `-n` | String | *(prompted)* | Deployment project name. Used as the directory name and `deployment.name` value. |
| `--output` | `-o` | String | `.` | Output directory. If `.`, creates a subdirectory named after the project. |
| `--var` | | String (multiple) | | Template variables as `KEY=VALUE` pairs. Can be specified multiple times. |
| `--interactive` | `-i` | Flag | `false` | Launch the interactive setup wizard. Automatically enabled if no `--template` or `--name` is provided. |

#### Examples

**Create a project using the interactive wizard:**

```bash
udp-cicd init
```

**Example output:**

```
Unified Data Platform Deployment — Setup Wizard

Available templates:
  1. blank — Empty project with minimal structure
  2. medallion — Bronze/Silver/Gold lakehouse pattern

Select template: 2

Project name: sales-analytics

Fetching available capacities...
  1. MyCapacity (F4, West US 2)
Select capacity: 1

✓ Created project: sales-analytics/
  udp.yml
  notebooks/
  README.md
```

**Create a project non-interactively:**

```bash
udp-cicd init --template medallion --name sales-analytics --var capacity_id=abc-def-123
```

**Create a project in a specific directory:**

```bash
udp-cicd init --template blank --name udp-project --output /path/to/projects
```

> **Note**
>
> When using interactive mode, the wizard attempts to fetch available Fabric capacities using `az rest`. If Azure CLI is not authenticated, this step is skipped and you can set the capacity ID manually in `udp.yml` afterward.

---

<a id="list"></a>

### 3.2 list

List available deployment templates that can be used with `udp-cicd init`.

#### Syntax

```
udp-cicd list
```

#### Examples

```bash
udp-cicd list
```

**Example output:**

```
Available templates:

  blank
    Empty project with minimal structure

  medallion
    Bronze/Silver/Gold lakehouse pattern with ingestion notebooks

Usage: udp-cicd init --template <name> --name <project-name>
```

---

<a id="diag"></a>

### 3.3 diag

Run diagnostic checks to validate your environment, the .NET runtime, authentication, API connectivity, and deployment configuration.

#### Syntax

```
udp-cicd diag
```

#### Examples

```bash
udp-cicd diag
```

**Example output:**

```
udp-cicd diag

  ✓ .NET runtime 9.0.x
  ✓ Azure CLI installed
  ✓ Azure CLI authenticated
  ✓ Fabric API reachable
  ✓ udp.yml found
  ✓ Deployment validates

  6 passed, 0 failed
```

#### Checks performed

| Check | What it validates |
|---|---|
| .NET runtime | A compatible .NET 9 runtime is installed. |
| Azure CLI installed | The `az` binary is found in PATH. |
| Azure CLI authenticated | `az account show` succeeds (a valid session exists). |
| Fabric API reachable | The Fabric REST API responds to a workspace list request. |
| `udp.yml` found | A `udp.yml` or `udp.yaml` file exists in the current directory. |
| Deployment validates | The deployment definition parses and validates without errors. |

---

<a id="check-update"></a>

### 3.4 check-update

Check if a newer version of Unified Data Platform Deployment is available on NuGet.

#### Syntax

```
udp-cicd check-update
```

#### Examples

```bash
udp-cicd check-update
```

**Example output (update available):**

```
  Update available: 1.0.0b1 → 1.0.0b2
  Run: dotnet tool update --global udp-cicd
```

**Example output (up to date):**

```
  You're on the latest version: 1.0.0b2
```

---

## 4. Validation commands

<a id="validate"></a>

### 4.1 validate

Validate the deployment definition file for syntax errors, schema violations, unresolved variables, and dependency cycles.

#### Syntax

```
udp-cicd validate [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. If omitted, searches the current directory. |
| `--target` | `-t` | String | *(none)* | Target environment to validate against. Applies target-specific variable overrides and workspace config. |
| `--strict` | | Flag | `false` | Fail on unresolved variables and warnings in addition to errors. |

#### Examples

**Validate the deployment in the current directory:**

```bash
udp-cicd validate
```

**Example output:**

```
Deployment is valid.

  Deployment:    sales-analytics v1.0.0
  Desc:      Sales analytics pipeline
  Resources: 6
    environments: 1
    lakehouses: 2
    notebooks: 2
    pipelines: 1
  Targets:   dev, staging, prod

  Deployment order:
    1. [environments] spark_env
    2. [lakehouses] bronze_lakehouse
    3. [lakehouses] gold_lakehouse
    4. [notebooks] ingest_notebook (depends: bronze_lakehouse, spark_env)
    5. [notebooks] transform_notebook (depends: bronze_lakehouse, gold_lakehouse, spark_env)
    6. [pipelines] daily_pipeline (depends: ingest_notebook, transform_notebook)
```

**Validate against a specific target:**

```bash
udp-cicd validate --target prod
```

**Example output (with target):**

```
Deployment is valid.

  Deployment:    sales-analytics v1.0.0
  Resources: 6
    ...
  Targets:   dev, staging, prod
  Workspace: sales-analytics-prod
  Variables: 3

  Deployment order:
    ...
```

**Strict validation:**

```bash
udp-cicd validate --strict
```

**Example output (failure):**

```
Validation failed: Unresolved variable: ${source_connection}
  Variable 'source_connection' has no default value and no target override
```

> **Tip**
>
> Run `udp-cicd validate --strict -t <target>` in your CI pipeline to catch configuration errors before deployment.

---

<a id="graph"></a>

### 4.2 graph

Visualize the deployment dependency graph in Mermaid, DOT (Graphviz), or plain text format.

#### Syntax

```
udp-cicd graph [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--format` | | Choice: `mermaid`, `dot`, `text` | `mermaid` | Output format. |
| `--output` | `-o` | String | *(stdout)* | Output file path. If omitted, prints to stdout. |

#### Examples

**Generate a Mermaid diagram:**

```bash
udp-cicd graph
```

**Example output:**

```
graph TD
    spark_env["spark_env\n(environments)"]
    style spark_env fill:#457b9d,color:#fff
    bronze_lakehouse["bronze_lakehouse\n(lakehouses)"]
    style bronze_lakehouse fill:#2d6a4f,color:#fff
    ingest_notebook["ingest_notebook\n(notebooks)"]
    style ingest_notebook fill:#264653,color:#fff
    spark_env --> ingest_notebook
    bronze_lakehouse --> ingest_notebook
```

**Generate a DOT file for Graphviz:**

```bash
udp-cicd graph --format dot -o graph.dot
```

**Generate plain text:**

```bash
udp-cicd graph --format text
```

**Example output:**

```
  [environments] spark_env
  [lakehouses] bronze_lakehouse
  [notebooks] ingest_notebook ← spark_env, bronze_lakehouse
  [pipelines] daily_pipeline ← ingest_notebook
```

> **Tip**
>
> Paste Mermaid output into [mermaid.live](https://mermaid.live) or any Mermaid-compatible renderer (GitHub, GitLab, Notion, Confluence) to visualize the graph.

---

## 5. Planning and deployment commands

<a id="plan"></a>

### 5.1 plan

Preview what changes would be made to the target workspace without actually deploying. Compares the local deployment definition to the current workspace state.

#### Syntax

```
udp-cicd plan [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--auto-delete / --no-auto-delete` | | Flag | `false` | Include deletion of workspace items not in the deployment definition. |
| `--validate-api` | | Flag | `false` | After planning, validate each item definition against the Fabric API. |

#### Examples

**Preview changes for the default target:**

```bash
udp-cicd plan
```

**Example output:**

```
Plan: sales-analytics → sales-analytics-dev

  + [Lakehouse]      bronze_lakehouse          CREATE
  + [Lakehouse]      gold_lakehouse            CREATE
  + [Environment]    spark_env                 CREATE
  + [Notebook]       ingest_notebook           CREATE
  + [Notebook]       transform_notebook        CREATE
  + [DataPipeline]   daily_pipeline            CREATE

  Summary: 6 to create, 0 to update, 0 to delete, 0 unchanged
```

**Plan with auto-delete to remove unmanaged items:**

```bash
udp-cicd plan --target prod --auto-delete
```

**Example output:**

```
Plan: sales-analytics → sales-analytics-prod

  = [Lakehouse]      bronze_lakehouse          NO CHANGE
  ~ [Notebook]       ingest_notebook           UPDATE
  - [Notebook]       old_notebook              DELETE (unmanaged)

  Summary: 0 to create, 1 to update, 1 to delete, 1 unchanged
```

**Plan with API validation:**

```bash
udp-cicd plan --validate-api
```

**Example output:**

```
Plan: sales-analytics → sales-analytics-dev
  ...

Validating definitions against Fabric API...
  ✓ ingest_notebook: definition valid (2 parts)
  ✓ transform_notebook: definition valid (2 parts)
  - daily_pipeline: no definition (metadata only)
```

> **Note**
>
> If the workspace does not exist or is not reachable, the plan assumes an empty workspace and marks all items as CREATE.

---

<a id="deploy"></a>

### 5.2 deploy

Deploy the deployment to a target workspace. Creates, updates, or deletes items as needed to match the deployment definition.

#### Syntax

```
udp-cicd deploy [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--dry-run` | | Flag | `false` | Preview the plan without applying changes. Equivalent to `plan`. |
| `--auto-approve` | `-y` | Flag | `false` | Skip the interactive confirmation prompt. Required for CI/CD. |
| `--auto-delete / --no-auto-delete` | | Flag | `false` | Delete workspace items that are not defined in the deployment. |
| `--force` | | Flag | `false` | Override deployment locks and skip the definition cache. |

#### Examples

**Deploy to the default target (interactive):**

```bash
udp-cicd deploy
```

**Example output:**

```
Plan: sales-analytics → sales-analytics-dev

  + [Lakehouse]      bronze_lakehouse          CREATE
  + [Environment]    spark_env                 CREATE
  + [Notebook]       ingest_notebook           CREATE

  Summary: 3 to create, 0 to update, 0 to delete

Do you want to apply these changes? [y/N]: y

  ✓ Created: bronze_lakehouse (Lakehouse)
  ✓ Created: spark_env (Environment)
  ✓ Created: ingest_notebook (Notebook)

Deploy complete. 3 items deployed in 12.4s.
```

**Deploy to production with auto-approve (CI/CD):**

```bash
udp-cicd deploy --target prod -y
```

**Dry run:**

```bash
udp-cicd deploy --target staging --dry-run
```

**Force deploy (skip lock and cache):**

```bash
udp-cicd deploy --target dev --force --auto-approve
```

> **Warning**
>
> The `--auto-delete` flag permanently deletes workspace items that are not defined in your deployment. Use `udp-cicd plan --auto-delete` first to review which items would be removed.

> **Important**
>
> The `--force` flag overrides deployment locks. Use it only when a previous deployment was interrupted or left in an inconsistent state.

---

<a id="destroy"></a>

### 5.3 destroy

Delete all deployment-managed resources from the target workspace. Optionally delete the workspace itself.

#### Syntax

```
udp-cicd destroy [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--auto-approve` | `-y` | Flag | `false` | Skip the confirmation prompt. When not set, you must type the deployment name to confirm. |
| `--delete-workspace` | | Flag | `false` | Also delete the workspace after all items are removed. |

#### Examples

**Destroy resources in the dev environment:**

```bash
udp-cicd destroy --target dev
```

**Example output:**

```
WARNING: This will delete all deployment-managed resources in:
  Workspace: sales-analytics-dev
  Target:    dev

  Resources to destroy (reverse dependency order):
    1. - [pipelines] daily_pipeline
    2. - [notebooks] transform_notebook
    3. - [notebooks] ingest_notebook
    4. - [environments] spark_env
    5. - [lakehouses] gold_lakehouse
    6. - [lakehouses] bronze_lakehouse

Type the deployment name 'sales-analytics' to confirm destruction: sales-analytics

  - Deleted: daily_pipeline
  - Deleted: transform_notebook
  - Deleted: ingest_notebook
  - Deleted: spark_env
  - Deleted: gold_lakehouse
  - Deleted: bronze_lakehouse

Destroy complete. Deleted: 6 resources.
```

**Destroy with auto-approve and delete workspace (CI/CD cleanup):**

```bash
udp-cicd destroy --target dev -y --delete-workspace
```

> **Warning**
>
> This operation is irreversible. Destroyed resources cannot be recovered. The `--delete-workspace` flag deletes the entire Fabric workspace, including any items not managed by the deployment.

> **Note**
>
> Resources are destroyed in reverse dependency order (dependents first, then their dependencies) to avoid API errors from dangling references.

---

<a id="promote"></a>

### 5.4 promote

Promote deployed artifacts from one target workspace to another by copying item definitions.

#### Syntax

```
udp-cicd promote [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--from` | | String | **Required** | Source target name (for example, `staging`). |
| `--to` | | String | **Required** | Destination target name (for example, `prod`). |
| `--auto-approve` | `-y` | Flag | `false` | Skip the confirmation prompt. |

#### Examples

**Promote from staging to production:**

```bash
udp-cicd promote --from staging --to prod
```

**Example output:**

```
Promote: staging → prod
  Source:  sales-analytics-staging (abc123-...)
  Dest:    sales-analytics-prod (def456-...)

  6 items to promote
Proceed? [y/N]: y

  + Created: bronze_lakehouse
  + Created: gold_lakehouse
  ~ Updated: ingest_notebook
  ~ Updated: transform_notebook
  + Created: daily_pipeline
  + Created: spark_env

Promoted 6 items from staging to prod.
```

> **Note**
>
> If the destination workspace does not exist, `promote` creates it automatically and assigns the capacity configured in the target.

> **Important**
>
> Promote copies item *definitions* from the source workspace. It does not copy data. Lakehouse tables, warehouse data, and other runtime state are not transferred.

---

## 6. Operations commands

<a id="status"></a>

### 6.1 status

Show the deployed resource health and status for a target, including which items are deployed, missing, pending, or unmanaged.

#### Syntax

```
udp-cicd status [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |

#### Examples

```bash
udp-cicd status --target dev
```

**Example output:**

```
Status: sales-analytics
  Target:    dev
  Workspace: sales-analytics-dev (abc123-def456-...)
  Last deploy: 2025-03-15 14:30
  Items in workspace: 8
  Items in deployment:    6

Resource                   Type             Status       Item ID
bronze_lakehouse           lakehouses       deployed     abc123def456
gold_lakehouse             lakehouses       deployed     def789abc012
spark_env                  environments     deployed     ghi345jkl678
ingest_notebook            notebooks        deployed     mno901pqr234
transform_notebook         notebooks        deployed     stu567vwx890
daily_pipeline             pipelines        deployed     yza123bcd456
legacy_report              reports          unmanaged    efg789hij012
test_notebook              notebooks        unmanaged    klm345nop678

  Drift detected: 1 item(s)
```

#### Status meanings

| Status | Meaning |
|---|---|
| `deployed` | The item exists in the workspace and matches the state file. |
| `missing` | The item was previously deployed but is no longer in the workspace (deleted outside of udp-cicd). |
| `pending` | The item is defined in the deployment but has not been deployed yet. |
| `unmanaged` | The item exists in the workspace but is not defined in the deployment. |

---

<a id="drift"></a>

### 6.2 drift

Detect drift between the last deployed state and the live workspace. Reports items that were added, removed, or modified outside of `udp-cicd`. See [Drift Detection](../advanced/drift.md) for concepts, JSON output, and CI/CD integration.

#### Syntax

```
udp-cicd drift [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |

#### Examples

```bash
udp-cicd drift --target dev
```

**Example output (drift detected):**

```
Drift detected: 2 item(s)

  + new_notebook: added
  ~ ingest_notebook: modified

  Run 'udp-cicd deploy' to reconcile, or 'udp-cicd plan' to preview changes.
```

**Example output (no drift):**

```
No drift detected. Workspace matches deployed state.
```

> **Note**
>
> Drift detection requires a prior deployment. If no deployment state exists, the command prompts you to run `udp-cicd deploy` first.

---

<a id="diff"></a>

### 6.3 diff

Show a definition-level diff between local files and the deployed definitions in the workspace. Uses unified diff format.

#### Syntax

```
udp-cicd diff [OPTIONS] [RESOURCE_NAME]
```

#### Arguments

| Argument | Required | Description |
|---|---|---|
| `RESOURCE_NAME` | No | Specific resource to diff. If omitted, diffs all resources. |

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |

#### Examples

**Diff all resources:**

```bash
udp-cicd diff --target dev
```

**Example output:**

```
--- deployed/ingest_notebook/notebook-content.py
+++ local/ingest_notebook/notebook-content.py
@@ -10,6 +10,8 @@
 df = spark.read.format("csv").load(source_path)
+# Added data quality check
+df = df.filter(df["amount"] > 0)
 df.write.format("delta").save(target_path)
```

**Diff a single resource:**

```bash
udp-cicd diff --target dev ingest_notebook
```

**Example output (no differences):**

```
No differences found.
```

> **Note**
>
> Resources that have no exportable definition (for example, lakehouses created as metadata-only) are skipped.

---

<a id="history"></a>

### 6.4 history

Show the deployment history for a target environment.

#### Syntax

```
udp-cicd history [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--limit` | `-n` | Integer | `20` | Maximum number of history entries to display. |

#### Examples

```bash
udp-cicd history --target prod
```

**Example output:**

```
Deployment History (prod):

  deploy-abc123  2025-03-15 14:30  v1.2.0  6 resources  Update ingest_notebook
  deploy-def456  2025-03-10 09:15  v1.1.0  6 resources  Add transform_notebook
  deploy-ghi789  2025-03-01 11:00  v1.0.0  4 resources  Initial deployment
```

**Show only the last 5 entries:**

```bash
udp-cicd history --target prod -n 5
```

**Example output (no history):**

```
No deployment history found.
```

---

<a id="rollback"></a>

### 6.5 rollback

Roll back to a previous deployment by restoring the state file to a prior version.

#### Syntax

```
udp-cicd rollback [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--to` | | String | | Deploy ID to roll back to. Use `udp-cicd history` to find deploy IDs. |
| `--last` | | Flag | `false` | Roll back to the immediately previous deployment. |
| `--auto-approve` | `-y` | Flag | `false` | Skip the confirmation prompt. |

#### Examples

**Roll back to the previous deployment:**

```bash
udp-cicd rollback --target prod --last
```

**Example output:**

```
Rollback target: deploy-def456 (2025-03-10 09:15)
  Version: v1.1.0
  Resources: 6

Proceed with rollback? [y/N]: y
State rolled back. Run 'udp-cicd deploy' to apply.
```

**Roll back to a specific deployment:**

```bash
udp-cicd rollback --target prod --to deploy-ghi789 -y
```

> **Important**
>
> The `rollback` command restores the *state file* only. It does not modify the workspace. After rolling back, run `udp-cicd deploy` to apply the rolled-back state to the workspace.

> **Note**
>
> At least two deployment history entries are required for rollback. If only one entry exists, the command reports that there is not enough history.

---

<a id="watch"></a>

### 6.6 watch

Watch the project directory for file changes and automatically deploy to the target workspace.

#### Syntax

```
udp-cicd watch [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--interval` | | Integer | `5` | File check interval in seconds. |

#### Examples

**Watch and auto-deploy to dev:**

```bash
udp-cicd watch --target dev
```

**Example output:**

```
Watching for changes... (target: dev, interval: 5s)
  Press Ctrl+C to stop.

  [14:30:15] Changed: notebooks/ingest_notebook.py
  Deployed.

  [14:32:08] Changed: udp.yml, notebooks/transform_notebook.py
  Deployed.
```

**Watch with a faster interval:**

```bash
udp-cicd watch --target dev --interval 2
```

#### Watched file types

The watch command monitors files with the following extensions: `.py`, `.sql`, `.yml`, `.yaml`, `.json`, `.ipynb`, `.tmdl`, `.r`, `.scala`.

Directories named `.udp-cicd`, `__pycache__`, and `.venv` are excluded.

> **Warning**
>
> The `watch` command deploys changes automatically without a confirmation prompt. Use it only in development environments.

---

## 7. Resource management commands

<a id="run"></a>

### 7.1 run

Execute a specific notebook or pipeline in the target workspace.

#### Syntax

```
udp-cicd run RESOURCE_NAME [OPTIONS]
```

#### Arguments

| Argument | Required | Description |
|---|---|---|
| `RESOURCE_NAME` | Yes | The resource key of the notebook or pipeline to run, as defined in `udp.yml`. |

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--param` | `-p` | String (multiple) | | Execution parameters as `KEY=VALUE` pairs. Overrides default parameters from the deployment definition. |

#### Examples

**Run a notebook:**

```bash
udp-cicd run ingest_notebook --target dev
```

**Example output:**

```
Running [notebook]: ingest_notebook
  Workspace: sales-analytics-dev (abc123-...)
  Item ID:   def456-...

Job submitted. Waiting for completion...
Run complete.
```

**Run a notebook with parameters:**

```bash
udp-cicd run ingest_notebook --target dev -p start_date=2025-01-01 -p end_date=2025-12-31
```

**Example output:**

```
Running [notebook]: ingest_notebook
  Workspace: sales-analytics-dev (abc123-...)
  Item ID:   def456-...

  Parameters: {'start_date': '2025-01-01', 'end_date': '2025-12-31'}
Job submitted. Waiting for completion...
Run complete.
```

**Run a pipeline:**

```bash
udp-cicd run daily_pipeline --target prod
```

> **Note**
>
> Only `notebooks` and `pipelines` resource types are runnable. Attempting to run other resource types (such as lakehouses or semantic models) produces an error.

> **Important**
>
> The resource must already be deployed to the workspace. If it has not been deployed, run `udp-cicd deploy` first.

---

<a id="export"></a>

### 7.2 export

Export item definitions from a deployed workspace to local files.

#### Syntax

```
udp-cicd export [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |
| `--target` | `-t` | String | *(default target)* | Target environment. |
| `--output` | `-o` | String | `.` | Output directory for exported files. |
| `--resource` | `-r` | String | *(all)* | Export a specific resource by name. If omitted, exports all items. |

#### Examples

**Export all items from the dev workspace:**

```bash
udp-cicd export --target dev -o ./exported
```

**Example output:**

```
Exporting from workspace: sales-analytics-dev

  + ingest_notebook (Notebook): 2 files → exported/ingest_notebook
  + transform_notebook (Notebook): 2 files → exported/transform_notebook
  + daily_pipeline (DataPipeline): 1 files → exported/daily_pipeline
  = bronze_lakehouse (Lakehouse): no exportable definition
  = gold_lakehouse (Lakehouse): no exportable definition

Exported 3 item(s) to /Users/you/project/exported
```

**Export a single resource:**

```bash
udp-cicd export --target dev -r ingest_notebook -o ./exported
```

---

<a id="generate"></a>

### 7.3 generate

Generate a `udp.yml` deployment definition by scanning an existing Fabric workspace.

#### Syntax

```
udp-cicd generate [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--workspace` | `-w` | String | **Required** | Workspace name or GUID to scan. |
| `--output` | `-o` | String | `.` | Output directory for the generated `udp.yml` and item definitions. |

#### Examples

**Generate from a workspace name:**

```bash
udp-cicd generate -w "My Existing Workspace" -o ./generated
```

**Generate from a workspace GUID:**

```bash
udp-cicd generate -w "abc12345-def6-7890-abcd-ef1234567890" -o ./generated
```

**Example output:**

```
Scanning workspace: My Existing Workspace (abc12345-...)
  Found 8 items

  + Lakehouse: bronze_lake
  + Lakehouse: gold_lake
  + Notebook:  etl_step1
  + Notebook:  etl_step2
  + Pipeline:  nightly_run

Generated:
  ./generated/udp.yml
  ./generated/notebooks/etl_step1/notebook-content.py
  ./generated/notebooks/etl_step2/notebook-content.py
```

> **Tip**
>
> Use `generate` to bootstrap a deployment definition for an existing workspace, then customize the generated `udp.yml` to add variables, targets, security roles, and policies.

---

<a id="bind"></a>

### 7.4 bind

Bind an existing workspace item to deployment management. The item must already be defined in `udp.yml`.

#### Syntax

```
udp-cicd bind RESOURCE_NAME [OPTIONS]
```

#### Arguments

| Argument | Required | Description |
|---|---|---|
| `RESOURCE_NAME` | Yes | The resource key as defined in `udp.yml`. |

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--workspace` | `-w` | String | **Required** | Workspace name or GUID containing the item. |
| `--file` | `-f` | String | Auto-detected | Path to `udp.yml`. |

#### Examples

```bash
udp-cicd bind ingest_notebook -w "sales-analytics-dev"
```

**Example output:**

```
Bound: ingest_notebook
  Type:      Notebook
  Item ID:   abc123-def456-...
  Workspace: sales-analytics-dev
  Recorded to state. Visible in 'udp-cicd status'.

  This resource will be managed by the deployment on the next deploy.
  Changes to udp.yml will be applied to the existing item.
```

> **Important**
>
> The resource must be defined in `udp.yml` before you can bind it. Add the resource definition first, then run `bind` to associate it with the existing workspace item.

---

<a id="import"></a>

### 7.5 import

Import existing resources into udp-cicd management from a Terraform state file or a live Fabric workspace.

#### Syntax

```
udp-cicd import [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--from-terraform` | | String | | Path to a `terraform.tfstate` file. Extracts `fabric_*` resources. |
| `--workspace` | `-w` | String | | Workspace name or GUID to import from. |
| `--output` | `-o` | String | `.` | Output directory for state files. |
| `--target` | `-t` | String | `dev` | Target name to assign to the imported state. |

> **Note**
>
> You must specify exactly one of `--from-terraform` or `--workspace`.

#### Examples

**Import from Terraform state:**

```bash
udp-cicd import --from-terraform ./terraform.tfstate --target prod
```

**Example output:**

```
Found 4 Fabric resources in Terraform state
  lakehouse            bronze_lakehouse
  lakehouse            gold_lakehouse
  notebook             ingest_notebook
  datapipeline         daily_pipeline

Imported 4 resources to udp-cicd state.
```

**Import from a live workspace:**

```bash
udp-cicd import -w "My Workspace" --target dev
```

**Example output:**

```
Found 6 items in workspace 'My Workspace'
Imported 6 resources to udp-cicd state.
```

> **Tip**
>
> After importing, create or update your `udp.yml` to define the resources, then run `udp-cicd deploy` to bring them under full deployment management.

---

## 8. Admin commands

The `admin` command group manages **tenant-level (admin) settings** via the Fabric Admin API. Unlike every other command, these settings are **tenant-wide**, not scoped to a workspace or target — so they live outside the normal `deploy` flow. See [Admin / Tenant Settings](../guide/admin-settings.md) for the full guide and the `udp.yml` schema.

> **Permissions**
>
> The caller must be a **Fabric administrator**, or a service principal with the `Tenant.ReadWrite.All` delegated scope. The Update Tenant Setting API is in **preview** and rate-limited to 25 requests/minute (paced automatically). Changes can take up to 15 minutes to take effect.

Settings are keyed in `udp.yml` by their API `settingName` (for example `PublishToWeb`), not the portal display title. Both commands validate declared names against the live tenant; unknown names are reported, never silently ignored.

### 8.1 admin plan

Preview the difference between the tenant settings declared under `admin.tenant_settings` and the live tenant. Read-only — makes no changes.

#### Syntax

```
udp-cicd admin plan [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | (auto-detect) | Path to `udp.yml`. |

#### Example

```bash
udp-cicd admin plan
```

**Example output:**

```
Tenant Settings Plan

  ~  PublishToWeb  Publish to web
      enabled: enabled -> disabled
  =  DevelopmentTenantSettings  no change
  !  PublishToWebb  (unknown setting)
      setting not found in this tenant — check the settingName

  Summary: 1 to update, 1 unchanged, 1 unknown
```

Markers: `~` change, `=` no change, `!` unknown settingName.

### 8.2 admin apply

Apply the declared tenant settings to the tenant. Prompts for confirmation because the changes are organization-wide.

#### Syntax

```
udp-cicd admin apply [OPTIONS]
```

#### Options

| Option | Short | Type | Default | Description |
|---|---|---|---|---|
| `--file` | `-f` | String | (auto-detect) | Path to `udp.yml`. |
| `--dry-run` | | Flag | `false` | Show what would be applied without writing. |
| `--auto-approve` | `-y` | Flag | `false` | Skip the confirmation prompt (for CI). |

`admin apply` refuses to run while any declared `settingName` is **unknown**, so a typo cannot silently no-op.

#### Examples

```bash
# Preview, then apply with confirmation
udp-cicd admin apply

# Non-interactive (CI)
udp-cicd admin apply -y

# Preview without writing
udp-cicd admin apply --dry-run
```

**Example output:**

```
Tenant Settings Plan

  ~  PublishToWeb  Publish to web
      enabled: enabled -> disabled

  Summary: 1 to update, 0 unchanged

WARNING: These changes apply tenant-wide and affect every user in the organization.
Apply these tenant setting changes? [y/n]: y

  ~ Updated: PublishToWeb

Apply complete. Updated: 1 setting(s).
Note: tenant setting changes can take up to 15 minutes to take effect.
```

---

## 9. Exit codes

All `udp-cicd` commands use the following exit codes:

| Code | Meaning |
|---|---|
| `0` | Success. |
| `1` | Error. The command failed due to a validation error, API error, authentication error, or runtime exception. |

The `drift` command additionally distinguishes between detected drift (exit code `1`) and operational errors (exit code `2`). See [Drift Detection](../advanced/drift.md) for details.

---

## 10. Environment variables

The following environment variables affect `udp-cicd` behavior:

| Variable | Description |
|---|---|
| `AZURE_TENANT_ID` | Azure AD tenant ID for service principal authentication. |
| `AZURE_CLIENT_ID` | Application (client) ID for service principal authentication. |
| `AZURE_CLIENT_SECRET` | Client secret for service principal authentication. |
| `FABRIC_USE_BROWSER` | Set to `true` to authenticate with an interactive browser sign-in (`InteractiveBrowserCredential`). |
| `FABRIC_CAPACITY_ID` | Default Fabric capacity ID used when creating workspaces. |
| `AZURE_STORAGE_ACCOUNT_NAME` | Storage account name for the Azure Blob or ADLS Gen2 state backend. |
| `HTTPS_PROXY` | HTTP proxy URL for outbound connections. |

There is no environment variable for the deployment file path. When `--file` is not given, the CLI auto-detects by searching the current directory and its parents for `udp.yml`, `udp.yaml`, `.udp/deployment.yml`, or `.udp/deployment.yaml`.

When the three `AZURE_*` service principal variables are set, the CLI authenticates with `ClientSecretCredential`. Otherwise it falls back to `DefaultAzureCredential`. See [Environment Variables](../reference/environment-variables.md) for the full reference.

---

## 11. See also

- [Installation](../getting-started/installation.md) -- Install and configure Unified Data Platform Deployment.
- [udp.yml reference](../guide/udp-yml.md) -- Complete schema reference for deployment definitions.
- [Admin / Tenant Settings](../guide/admin-settings.md) -- Declaratively manage org-wide Fabric tenant settings.
