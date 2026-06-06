# Unified Data Platform Deployment

> **Public Preview**

Unified Data Platform Deployment (`udp-cicd`) is a declarative deployment tool for Microsoft Fabric projects, distributed as a .NET global tool. This page describes the deployment model, the gap the tool fills relative to existing tooling, and its principal capabilities.

---

## 1. Overview

Define an entire Fabric project in a single `udp.yml` file: lakehouses, notebooks, pipelines, semantic models, Data Agents, security roles, and environment targets. Validate, plan, and deploy with a single command.

The `udp.yml` file describes the **desired state** of the workspace. `udp-cicd` determines what to create, update, or delete to reach that state, applies the changes in dependency order, runs idempotently, and detects drift on every run. The model will be familiar to users of Terraform or Databricks Asset Deployments. See [the declarative model](guide/declarative-model.md) for details.

```bash
dotnet tool install --global udp-cicd
udp-cicd init --template medallion --name udp-project
udp-cicd deploy --target dev
```

Installation requires the .NET SDK 9.0 or later. See [Installation](getting-started/installation.md) for prerequisites and authentication setup.

---

## 2. Scope and positioning

Existing tools cover adjacent needs. The Fabric CLI can export and import items, `fabric-cicd` can deploy across workspaces, and Terraform or Bicep can provision infrastructure. None of them describe the project itself:

| Concern | Description |
|---|---|
| Resource inventory | Which resources the project requires |
| Dependencies | How those resources depend on each other |
| Environment configuration | How configuration varies across environments (dev, staging, prod) |
| Security | Which security roles and permissions are required |
| Ordering | How to deploy everything in the correct order |

Unified Data Platform Deployment fills that gap.

---

## 3. Features

| Feature | Description |
|---|---|
| 45 resource types | Every Fabric item type: Lakehouses, Notebooks, Pipelines, Warehouses, Semantic Models, Reports, Environments, Data Agents, KQL Databases, Eventhouses, dbt Jobs, and 34 more |
| Dependency resolution | Automatic topological sort for deployment ordering |
| Multi-environment | Dev, staging, and prod targets with variable overrides |
| State management | Tracks deployed resources and detects drift |
| Rollback support | Deployment history with point-in-time rollback |
| Security | Entra ID group, user, and service principal role assignments with Graph API resolution |
| Secrets | Environment variables and Azure Key Vault integration |
| CI/CD ready | GitHub Actions and Azure DevOps templates included |
| Policy enforcement | Configurable pre-deploy validation rules |
