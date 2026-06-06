# Migrate from Terraform

If you're using the [Terraform Fabric Provider](https://registry.terraform.io/providers/microsoft/udp/latest/docs), here's how to migrate to udp-cicd.

## Why Migrate?

| | Terraform | udp-cicd |
|---|-----------|-----------|
| Language | HCL | YAML |
| State | Remote (S3, Blob, etc.) | Local or OneLake |
| Learning curve | High (HCL, providers, modules) | Low (single YAML file) |
| Drift detection | `terraform plan` | `udp-cicd drift` |
| Rollback | Manual state manipulation | `udp-cicd rollback` |
| Fabric-specific | Generic provider | Purpose-built for Fabric |
| MCP support | No | Yes (12 tools) |
| Item types | ~15 | 45 |

## Adoption flows

udp-cicd ships three complementary commands for bringing existing resources under management. Pick the one that matches your starting point:

| Command | Use when | Scope |
|---|---|---|
| [`udp-cicd import --from-terraform`](../cli/commands.md#import) | You already manage Fabric with Terraform and want to migrate in bulk. | Reads `terraform.tfstate`, extracts all `microsoft_udp_*` resources, and seeds udp-cicd state. |
| [`udp-cicd generate`](../cli/commands.md#generate) | You have a workspace but no declaration yet and want to reverse-engineer a `udp.yml`. | Scans a live workspace and writes `udp.yml` plus item content (notebook source, etc.). |
| [`udp-cicd bind`](../cli/commands.md#bind) | You wrote the declaration by hand and want to attach it to an existing item without recreating it. | Per-resource; binds one entry in `udp.yml` to one live item by ID. |

> **Compared to Databricks Asset Deployments**
>
> `databricks deployment generate` exists but is per-resource-type and requires the existing item's ID. `databricks deployment deployment bind` is the direct analog of `udp-cicd bind`. DAB has **no equivalent of `udp-cicd import --from-terraform`** — migrating from Terraform on Databricks is a manual `generate` + `bind` per resource. Bulk `tfstate` ingestion is unique to udp-cicd.

## Step-by-step Migration

### 1. Import existing state

```bash
udp-cicd import --from-terraform terraform.tfstate --target dev
```

This reads your Terraform state and creates a udp-cicd state file.

### 2. Generate udp.yml

```bash
udp-cicd generate --workspace "your-workspace-name"
```

Or manually map your Terraform resources:

| Terraform | udp.yml |
|-----------|------------|
| `microsoft_udp_workspace` | `targets.dev.workspace` |
| `microsoft_udp_lakehouse` | `resources.lakehouses` |
| `microsoft_udp_notebook` | `resources.notebooks` |
| `microsoft_udp_warehouse` | `resources.warehouses` |
| `microsoft_udp_spark_environment` | `resources.environments` |
| `microsoft_udp_data_pipeline` | `resources.pipelines` |

### 3. Validate

```bash
udp-cicd validate
udp-cicd plan --target dev
```

### 4. Deploy

```bash
udp-cicd deploy --target dev
```

### 5. Remove Terraform

Once udp-cicd is managing your resources, remove the Terraform config. Keep the Terraform state file as a backup until you're confident.
