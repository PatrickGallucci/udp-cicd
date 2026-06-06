# Azure DevOps

This page provides a complete `azure-pipelines.yml` for deploying Unified Data Platform Deployment with Azure DevOps, along with the variable group and environment setup it requires. The pipeline covers validate, staging, and production stages with approval gates.

---

## 1. Pipeline definition

The pipeline runs the following stages:

| Stage | Environment | Trigger condition | Approval |
|-------|-------------|-------------------|----------|
| Validate | (none) | Every PR and push | None |
| DeployDev | `dev` | Merge to `main`, after Validate | None |
| DeployStaging | `staging` | After DeployDev succeeds | None |
| DeployProd | `production` | After DeployStaging succeeds | Required approvers |

Create `azure-pipelines.yml` in your repo root:

```yaml
trigger:
  branches:
    include:
      - main
  paths:
    include:
      - udp.yml
      - notebooks/*
      - sql/*
      - agent/*

pool:
  vmImage: 'ubuntu-latest'

variables:
  - group: udp-credentials

stages:
  # ──────────────────────────────────────────
  # Validate (runs on every PR and push)
  # ──────────────────────────────────────────
  - stage: Validate
    displayName: 'Validate Deployment'
    jobs:
      - job: Validate
        steps:
          - task: UseDotNet@2
            inputs:
              packageType: 'sdk'
              version: '9.x'

          - script: dotnet tool install --global udp-cicd
            displayName: 'Install udp-cicd'

          - script: udp-cicd validate
            displayName: 'Validate udp.yml'

          - script: udp-cicd plan --target dev
            displayName: 'Plan dev deployment'
            env:
              AZURE_TENANT_ID: $(AZURE_TENANT_ID)
              AZURE_CLIENT_ID: $(AZURE_CLIENT_ID)
              AZURE_CLIENT_SECRET: $(AZURE_CLIENT_SECRET)

  # ──────────────────────────────────────────
  # Deploy to Dev (auto on merge to main)
  # ──────────────────────────────────────────
  - stage: DeployDev
    displayName: 'Deploy to Dev'
    dependsOn: Validate
    condition: and(succeeded(), eq(variables['Build.SourceBranch'], 'refs/heads/main'))
    jobs:
      - deployment: DeployDev
        environment: 'dev'
        strategy:
          runOnce:
            deploy:
              steps:
                - checkout: self

                - task: UseDotNet@2
                  inputs:
                    packageType: 'sdk'
                    version: '9.x'

                - script: dotnet tool install --global udp-cicd
                  displayName: 'Install udp-cicd'

                - script: udp-cicd deploy --target dev -y
                  displayName: 'Deploy to dev'
                  env:
                    AZURE_TENANT_ID: $(AZURE_TENANT_ID)
                    AZURE_CLIENT_ID: $(AZURE_CLIENT_ID)
                    AZURE_CLIENT_SECRET: $(AZURE_CLIENT_SECRET)

                - script: udp-cicd status --target dev
                  displayName: 'Check status'
                  env:
                    AZURE_TENANT_ID: $(AZURE_TENANT_ID)
                    AZURE_CLIENT_ID: $(AZURE_CLIENT_ID)
                    AZURE_CLIENT_SECRET: $(AZURE_CLIENT_SECRET)

  # ──────────────────────────────────────────
  # Deploy to Staging (with quality gate)
  # ──────────────────────────────────────────
  - stage: DeployStaging
    displayName: 'Deploy to Staging'
    dependsOn: DeployDev
    jobs:
      - deployment: DeployStaging
        environment: 'staging'
        strategy:
          runOnce:
            deploy:
              steps:
                - checkout: self

                - task: UseDotNet@2
                  inputs:
                    packageType: 'sdk'
                    version: '9.x'

                - script: dotnet tool install --global udp-cicd
                  displayName: 'Install udp-cicd'

                - script: udp-cicd deploy --target staging -y
                  displayName: 'Deploy to staging'
                  env:
                    AZURE_TENANT_ID: $(AZURE_TENANT_ID)
                    AZURE_CLIENT_ID: $(AZURE_CLIENT_ID)
                    AZURE_CLIENT_SECRET: $(AZURE_CLIENT_SECRET)

  # ──────────────────────────────────────────
  # Deploy to Prod (manual approval)
  # ──────────────────────────────────────────
  - stage: DeployProd
    displayName: 'Deploy to Production'
    dependsOn: DeployStaging
    jobs:
      - deployment: DeployProd
        environment: 'production'  # Configure approval in ADO Environments
        strategy:
          runOnce:
            deploy:
              steps:
                - checkout: self

                - task: UseDotNet@2
                  inputs:
                    packageType: 'sdk'
                    version: '9.x'

                - script: dotnet tool install --global udp-cicd
                  displayName: 'Install udp-cicd'

                - script: udp-cicd deploy --target prod -y
                  displayName: 'Deploy to production'
                  env:
                    AZURE_TENANT_ID: $(AZURE_TENANT_ID)
                    AZURE_CLIENT_ID: $(AZURE_CLIENT_ID)
                    AZURE_CLIENT_SECRET: $(AZURE_CLIENT_SECRET)

                - script: udp-cicd status --target prod
                  displayName: 'Verify deployment'
                  env:
                    AZURE_TENANT_ID: $(AZURE_TENANT_ID)
                    AZURE_CLIENT_ID: $(AZURE_CLIENT_ID)
                    AZURE_CLIENT_SECRET: $(AZURE_CLIENT_SECRET)
```

---

## 2. Setup

### 2.1 Create a variable group

Go to Pipelines > Library and create a variable group named `udp-credentials`:

| Variable | Value | Secret? |
|----------|-------|---------|
| `AZURE_TENANT_ID` | Your Entra tenant GUID | No |
| `AZURE_CLIENT_ID` | Service principal app ID | No |
| `AZURE_CLIENT_SECRET` | Service principal secret | Yes |

The pipeline references this group in its `variables` block. See [Service Principal Setup](../guide/service-principal.md) for creating the service principal.

### 2.2 Create environments

Go to Pipelines > Environments and create:

| Environment | Approvals |
|-------------|-----------|
| `dev` | None |
| `staging` | None |
| `production` | Required approvers |

### 2.3 Create the pipeline

Go to Pipelines > New Pipeline, select your repo, choose "Existing Azure Pipelines YAML file", and select `azure-pipelines.yml`.
