# State Management

udp-cicd tracks what's deployed using a state file, similar to Terraform's `terraform.tfstate` or Databricks Asset Deployments' internal state. The state enables incremental deploys, drift detection, and rollback.

## How It Works

Every time you run `udp-cicd deploy`, the state file is updated with:

- **What resources exist** — name, item ID, type
- **Definition hashes** — SHA-256 of each resource's definition (for incremental deploy)
- **Workspace ID** — which workspace was deployed to
- **Timestamp** — when the deployment happened

On the next deploy, udp-cicd compares local definitions against stored hashes. **Unchanged resources are skipped** — only modified resources are re-uploaded.

## State File Location

By default, state is stored locally:

```
udp-project/
├── udp.yml
├── .udp-cicd/                    # State directory
│   ├── state-dev.json              # State for dev target
│   ├── state-staging.json          # State for staging target
│   ├── state-prod.json             # State for prod target
│   ├── lock-dev.json               # Deployment lock (temporary)
│   ├── history/                    # Deployment history
│   │   ├── 1774451090-dev.json
│   │   └── 1774451200-dev.json
│   ├── audit.jsonl                 # Structured audit log
│   └── metrics.json                # Deployment metrics
└── .gitignore                      # Should include .udp-cicd/
```

!!! warning "Don't commit state files"
    Add `.udp-cicd/` to your `.gitignore`. State is environment-specific — each developer and CI runner has their own.

## State File Format

```json
{
  "version": 1,
  "deployment_name": "contoso-analytics",
  "deployment_version": "1.0.0",
  "target_name": "dev",
  "workspace_id": "c2410443-5bce-4cce-8065-b453dd6b2f1d",
  "workspace_name": "contoso-analytics-dev",
  "last_deployed": 1774451090.617839,
  "resources": {
    "bronze": {
      "item_id": "f207dd97-d7ae-4fed-88c0-5f4a38ff934b",
      "item_type": "Lakehouse",
      "resource_key": "bronze",
      "definition_hash": "a3f8c2d1e5b7...",
      "last_deployed": 1774451090.617839
    },
    "ingest_to_bronze": {
      "item_id": "f1714aa7-636b-48ca-8ef2-b85d5c59583c",
      "item_type": "Notebook",
      "resource_key": "ingest_to_bronze",
      "definition_hash": "b4e9d3f2a6c8...",
      "last_deployed": 1774451090.617839
    }
  }
}
```

## Remote State Backends

For teams and CI/CD, local state doesn't work — each runner gets its own copy. Use a remote backend to share state across machines.

### OneLake (Recommended)

Store state in a Fabric lakehouse — state lives alongside your data.

```yaml
state:
  backend: onelake
  config:
    workspace_id: "your-workspace-guid"
    lakehouse_id: "your-lakehouse-guid"
    path: ".udp-cicd-state"   # Optional, default: .udp-cicd-state
```

State files are stored in `Files/.udp-cicd-state/` in the lakehouse. Uses the OneLake ADLS-compatible endpoint (`onelake.dfs.udp.microsoft.com`).

Requires: `pip install azure-storage-file-datalake`

### Azure Blob Storage

Like Terraform's `azurerm` backend.

```yaml
state:
  backend: azureblob
  config:
    account_name: mystorageaccount
    container_name: udp-cicd-state
    # Optional: uses DefaultAzureCredential if no key provided
    account_key: "${secret.STORAGE_KEY}"
```

Requires: `dotnet tool install --global udp-cicd`

### Azure Data Lake Storage Gen2

```yaml
state:
  backend: adls
  config:
    account_name: mydatalake
    filesystem: udp-cicd-state
```

Requires: `dotnet tool install --global udp-cicd`

### Local (Default)

No configuration needed. State stored in `.udp-cicd/`.

```yaml
# No state config = local
```

## Comparison with Other Tools

| | Terraform | Databricks (DABs) | udp-cicd |
|---|-----------|-------------------|-----------|
| State file | `terraform.tfstate` | `.databricks/deployment/{target}/terraform.tfstate` | `.udp-cicd/state-{target}.json` |
| Remote backends | S3, Azure Blob, GCS, etc. | Managed by Databricks | **OneLake**, Azure Blob, ADLS |
| Locking | DynamoDB / Blob lease | Terraform under the hood | Blob lease / file lock |
| Incremental | Yes (hash comparison) | Yes | Yes (definition hash) |
| Drift detection | `terraform plan` | No | `udp-cicd drift` |
| Rollback | Manual state manipulation | No | `udp-cicd rollback` |
| History | No (state is overwritten) | No | Yes (timestamped snapshots) |

## Distributed Locking

When using a remote backend, udp-cicd uses **Azure Blob leases** to prevent concurrent deployments. If two CI runners try to deploy to the same target simultaneously, the second one will see:

```
Deployment locked by runner@github-actions (CI: 12345678)
  Use --force to override.
```

Locks automatically expire after 30 minutes (stale lock protection).

## Commands

```bash
# View current state
udp-cicd status --target dev

# View deployment history
udp-cicd history --target dev

# Detect drift (compare state vs live workspace)
udp-cicd drift --target dev

# Rollback to previous deployment
udp-cicd rollback --last --target dev

# Override a stale lock
udp-cicd deploy --target dev --force
```

## Importing Existing Resources

If you have resources already deployed (by Terraform, fabric-cicd, or manually), import them into udp-cicd state:

```bash
# From a live workspace
udp-cicd import --workspace "udp-existing-workspace" --target dev

# From Terraform state
udp-cicd import --from-terraform terraform.tfstate --target dev
```

This creates the state file without redeploying — udp-cicd will manage the resources going forward.
