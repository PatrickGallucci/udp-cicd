# <img src="assets/udp-icon.png" width="32" height="32" align="top" /> Unified Data Platform Deployment

> **Public Preview**

**Project definition for Microsoft Unified Data Platform.**

Define your entire Fabric project in a single `udp.yml` — lakehouses, notebooks, pipelines, semantic models, Data Agents, security roles, and environment targets — then validate, plan, and deploy with a single command.

You describe the **desired state** of your workspace. `udp-cicd` figures out what to create, update, or delete to make it so — in the right order, idempotently, with drift detection on every run. If you've used Terraform or Databricks Asset Deployments, you already know the model. → [Read about the declarative model](guide/declarative-model.md).

```bash
dotnet tool install --global udp-cicd
udp-cicd init --template medallion --name udp-project
udp-cicd deploy --target dev
```

## Why?

Project definition for Microsoft Unified Data Platform. The Fabric CLI can export/import items, `fabric-cicd` can deploy across workspaces, and Terraform/Bicep can provision infrastructure — but none of them describe:

- What resources your project needs
- How those resources depend on each other
- How configuration varies across environments
- What security roles and permissions are required
- How to deploy everything in the correct order

**Unified Data Platform Deployment fills that gap.**

## Features

- **45 resource types** — Every Fabric item type: Lakehouses, Notebooks, Pipelines, Warehouses, Semantic Models, Reports, Environments, Data Agents, KQL Databases, Eventhouses, dbt Jobs, and 34 more
- **Dependency resolution** — automatic topological sort for deployment ordering
- **Multi-environment** — dev, staging, prod targets with variable overrides
- **State management** — tracks deployed resources, detects drift
- **Rollback support** — deployment history with point-in-time rollback
- **Security** — Entra ID group/user/SP role assignments with Graph API resolution
- **Secrets** — Environment variables and Azure KeyVault integration
- **CI/CD ready** — GitHub Actions and Azure DevOps templates included
- **Policy enforcement** — configurable pre-deploy validation rules
