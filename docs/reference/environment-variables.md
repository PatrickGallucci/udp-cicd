# Environment Variables

## Authentication

| Variable | Required | Description |
|----------|----------|-------------|
| `AZURE_TENANT_ID` | For SP auth | Azure AD tenant GUID |
| `AZURE_CLIENT_ID` | For SP auth | Service principal app ID |
| `AZURE_CLIENT_SECRET` | For SP auth | Service principal client secret |

When these are set, `DefaultAzureCredential` uses `EnvironmentCredential` automatically. Otherwise falls back to `az login` session.

## udp.yml Variables

Reference in udp.yml with `${env.VARIABLE_NAME}`:

```yaml
resources:
  notebooks:
    etl:
      path: ./notebooks/etl.py
      parameters:
        db_host: "${env.DB_HOST}"
```

## Secret Variables

Reference with `${secret.NAME}`:

```yaml
targets:
  prod:
    variables:
      db_password: "${secret.DB_PASSWORD}"
```

Secrets are resolved from environment variables at deploy time. The `secret.` prefix is a convention — it reads from the same environment.

## KeyVault Variables

Reference with `${keyvault.VAULT_NAME.SECRET_NAME}`:

```yaml
connections:
  my_db:
    properties:
      password: "${keyvault.udp-vault.db-password}"
```

Requires: `dotnet tool install --global udp-cicd`

## Deployment Variables

Built-in variables available in udp.yml:

| Variable | Value |
|----------|-------|
| `${deployment.name}` | Deployment name from `deployment.name` |
| `${deployment.version}` | Deployment version from `deployment.version` |

## State Backend Config

For remote state (in udp.yml):

```yaml
state:
  backend: onelake  # or: azureblob, adls, local
  config:
    workspace_id: "guid"      # OneLake
    lakehouse_id: "guid"      # OneLake
    account_name: "storage"   # azureblob / adls
    container_name: "state"   # azureblob
    filesystem: "state"       # adls
    account_key: "..."        # azureblob (optional, uses DefaultAzureCredential)
```
