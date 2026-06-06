# Migrate from Terraform

This page describes how to migrate from the [Terraform Fabric Provider](https://registry.terraform.io/providers/microsoft/fabric/latest/docs) to udp-cicd: the differences between the two tools, the three adoption flows for bringing existing resources under management, and the step-by-step migration procedure.

---

## 1. Why migrate

| Capability | Terraform | udp-cicd |
|---|-----------|-----------|
| Language | HCL | YAML |
| State | Remote (S3, Blob, etc.) | Local JSON (stable); Azure Blob and OneLake/ADLS Gen2 (beta) |
| Learning curve | High (HCL, providers, modules) | Low (single YAML file) |
| Drift detection | `terraform plan` | `udp-cicd drift` |
| Rollback | Manual state manipulation | `udp-cicd rollback` |
| Fabric-specific | Generic provider | Purpose-built for Fabric |
| MCP support | No | Yes (14 tools) |
| Item types | ~15 | 45 |

---

## 2. Adoption flows

udp-cicd ships three complementary commands for bringing existing resources under management. Pick the one that matches your starting point:

| Command | Use when | Scope |
|---|---|---|
| [`udp-cicd import --from-terraform`](../cli/commands.md#import) | You already manage Fabric with Terraform and want to migrate in bulk. | Reads `terraform.tfstate`, extracts all `fabric_*` resources, and seeds udp-cicd state. |
| [`udp-cicd generate`](../cli/commands.md#generate) | You have a workspace but no declaration yet and want to reverse-engineer a `udp.yml`. | Scans a live workspace and writes `udp.yml` plus item content (notebook source, etc.). |
| [`udp-cicd bind`](../cli/commands.md#bind) | You wrote the declaration by hand and want to attach it to an existing item without recreating it. | Per-resource; binds one entry in `udp.yml` to one live item by ID. |

> **Compared to Databricks Asset Bundles**
>
> `databricks bundle generate` exists but is per-resource-type and requires the existing item's ID. `databricks bundle deployment bind` is the direct analog of `udp-cicd bind`. DABs have **no equivalent of `udp-cicd import --from-terraform`**; migrating from Terraform on Databricks is a manual `generate` + `bind` per resource. Bulk `tfstate` ingestion is unique to udp-cicd.

---

## 3. Step-by-step migration

### 3.1 Import existing state

```bash
udp-cicd import --from-terraform terraform.tfstate --target dev
```

This reads your Terraform state and creates a udp-cicd state file.

### 3.2 Generate udp.yml

```bash
udp-cicd generate --workspace "your-workspace-name"
```

Or manually map your Terraform resources:

| Terraform | udp.yml |
|-----------|------------|
| `fabric_workspace` | `targets.dev.workspace` |
| `fabric_lakehouse` | `resources.lakehouses` |
| `fabric_notebook` | `resources.notebooks` |
| `fabric_warehouse` | `resources.warehouses` |
| `fabric_environment` | `resources.environments` |
| `fabric_data_pipeline` | `resources.pipelines` |

### 3.3 Validate

```bash
udp-cicd validate
udp-cicd plan --target dev
```

### 3.4 Deploy

```bash
udp-cicd deploy --target dev
```

### 3.5 Remove Terraform

Once udp-cicd is managing your resources, remove the Terraform config. Keep the Terraform state file as a backup until you are confident.
