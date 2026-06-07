# Troubleshooting

This page lists common errors by category, with the cause and resolution for each. For a general environment check, run `udp-cicd diag`, which validates the .NET runtime, Azure CLI status, and Fabric API connectivity.

---

## 1. Authentication

### 1.1 "DefaultAzureCredential failed to retrieve a token"

**Cause:** Not authenticated with Azure. When the service principal variables are not set, the tool falls back to `DefaultAzureCredential`, which requires a managed identity, environment credentials, or an active `az login` session.

**Resolution:**

```bash
# Interactive login
az login

# Or set service principal env vars (ClientSecretCredential)
export AZURE_TENANT_ID="your-tenant-guid"
export AZURE_CLIENT_ID="your-client-guid"
export AZURE_CLIENT_SECRET="your-secret"
```

### 1.2 "Invalid client secret provided"

**Cause:** The `AZURE_CLIENT_SECRET` value is wrong. A common mistake is copying the Secret ID instead of the Secret Value.

**Resolution:** In the Azure Portal, go to App registrations, select the app, open Certificates & secrets, create a new secret, and copy the **Value** (not the ID).

### 1.3 "AADSTS7000215: Invalid client secret"

**Cause:** Same as 1.2, or the secret has expired.

**Resolution:** Create a new client secret and update the GitHub secrets or environment variables.

---

## 2. Capacity

### 2.1 "capacity_id is not a valid GUID"

**Cause:** The `capacity_id` in `udp.yml` is not in the correct format.

**Resolution:** Find the capacity GUID:

```bash
az rest --method get \
  --url "https://api.fabric.microsoft.com/v1/capacities" \
  --resource "https://api.fabric.microsoft.com"
```

Copy the `id` field (format: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`).

### 2.2 "Capacity not found" or deploy creates workspace but items fail

**Cause:** The capacity is paused or inactive, or the identity does not have access to it.

**Resolution:** In the Fabric Admin Portal, open Capacities and confirm the capacity shows "Active".

---

## 3. Item creation

### 3.1 "DisplayName is Invalid for ArtifactType"

**Cause:** Lakehouses, warehouses, and some other item types cannot have hyphens or spaces in their names.

**Resolution:** Use underscores instead of hyphens:

```yaml
# Wrong
lakehouses:
  bronze-lakehouse:  # ← hyphens not allowed

# Right
lakehouses:
  bronze_lakehouse:  # ← underscores only
```

### 3.2 "ItemDisplayNameNotAvailableYet"

**Cause:** An item with the same name was recently deleted. Fabric reserves the name for a few minutes.

**Resolution:** Wait 2-5 minutes and retry. udp-cicd automatically retries retriable errors, but if the wait exceeds the retry window, run deploy again.

### 3.3 "NotebookId cannot be null"

**Cause:** Pipeline activities reference notebooks by ID, but the notebook was not found in the workspace.

**Resolution:** Ensure notebooks are deployed before pipelines. udp-cicd handles this automatically through dependency ordering, but if a pipeline references notebooks from a different deployment, deploy those notebooks first.

### 3.4 "Artifact definition parts count should be 1"

**Cause:** Spark Job Definitions only accept a single definition part.

**Resolution:** This was a bug in earlier versions. Update to the latest version: `dotnet tool update --global udp-cicd`

### 3.5 "InvalidDefinitionFormat"

**Cause:** The notebook definition format is not recognized by the Fabric API.

**Resolution:** Ensure the notebook files are valid `.py` or `.ipynb` files. udp-cicd wraps `.py` files in ipynb format automatically.

### 3.6 "MissingDefinition"

**Cause:** Semantic Models and Reports require definition files (TMDL/PBIR) that do not exist locally.

**Resolution:**

| Item type | Action |
|---|---|
| Semantic Models | Export TMDL files from Power BI Desktop or the Fabric portal and place them in the `path` directory. |
| Reports | Export PBIR files from Power BI Desktop and place them in the `path` directory. |
| Either | Alternatively, remove the items from `udp.yml` and create them in the portal. |

### 3.7 "The feature is not available"

**Cause:** The item type requires a capacity feature that is not enabled (for example, dbt or EventSchemaSet).

**Resolution:** Contact your Fabric admin to enable the feature, or remove the item from `udp.yml`.

---

## 4. OneLake security

### 4.1 "UniversalSecurityFeatureDisabledForArtifactType"

**Cause:** OneLake data access roles require the security feature to be enabled per lakehouse.

**Resolution:**

1. Open the lakehouse in the Fabric portal.
2. Click **Manage OneLake security (preview)** in the ribbon.
3. Enable the feature.
4. Run `udp-cicd deploy` again.

> **Note**
>
> This is a per-item setting, not a tenant admin setting.

---

## 5. Environment

### 5.1 "There is a publish operation in progress"

**Cause:** A previous environment publish is still running. Environment publishes can take 5-10 minutes.

**Resolution:** Wait for the current publish to complete. Check status in the Fabric portal under the environment's details. udp-cicd publishes are fire-and-forget, so the deploy itself succeeds.

---

## 6. Variables

### 6.1 "Unresolved variables: ${var.missing}"

**Cause:** A variable referenced in `udp.yml` has no value defined.

**Resolution:** Provide a value through one of the following:

| Approach | Example |
|---|---|
| Default value | `variables: { missing: { default: "value" } }` |
| Target override | `targets: { dev: { variables: { missing: "value" } } }` |
| Environment variable | Set the variable when using `${env.MISSING}` |
| Secret | Set the secret when using `${secret.MISSING}` |

In deploy mode, unresolved variables cause a hard failure. In validate mode, use `--strict` to catch them.

---

## 7. Deployment

### 7.1 "Deployment locked by user@host"

**Cause:** A previous deployment did not release its lock (for example, it crashed mid-deploy).

**Resolution:**

```bash
udp-cicd deploy --target dev --force  # Override the lock
```

Or manually delete the lock file:

```bash
rm .udp-cicd/lock-dev.lock
```

### 7.2 Partial deployment / rollback

**Cause:** Some items failed during creation, triggering a rollback of already-created items.

**Resolution:** Check the error messages for the failed items. Fix the issues (naming, definitions, permissions) and run deploy again. Successfully created items from previous runs will show as "update" instead of "create".

---

## 8. CI/CD

### 8.1 GitHub Actions: "Process completed with exit code 1"

**Cause:** A pipeline step failed. Common causes:

| Cause | Detail |
|---|---|
| Authentication | Secrets not configured or expired |
| Capacity | Not active, or wrong GUID |
| Naming | Hyphens in resource names |

**Resolution:** Check the full log output in the Actions run. Run `udp-cicd diag` locally to diagnose.

### 8.2 "udp-cicd: command not found" in CI

**Cause:** udp-cicd is not installed in the CI environment.

**Resolution:** Add `dotnet tool install --global udp-cicd` before running udp-cicd commands.

---

## 9. Admin / tenant settings

### 9.1 `admin plan` reports a setting as "unknown"

**Cause:** The key under `admin.tenant_settings` is not a valid `settingName` for your tenant. The [tenant settings index](https://learn.microsoft.com/en-us/fabric/admin/tenant-settings-index) lists display titles, not the API identifiers udp-cicd needs.

**Resolution:** List the real identifiers and match by title:

```bash
az rest --method get \
  --url "https://api.fabric.microsoft.com/v1/admin/tenantsettings" \
  --resource "https://api.fabric.microsoft.com" \
  --query "tenantSettings[].{name:settingName, title:title}" -o table
```

Use the `name` column value as the key in `udp.yml`.

### 9.2 "Could not read tenant settings" / 403 Forbidden

**Cause:** The caller is not a Fabric administrator. Tenant settings require elevated permissions.

**Resolution:** Sign in as a **Fabric administrator**, or use a service principal granted the `Tenant.ReadWrite.All` delegated scope. The service principal must also be enabled to call admin APIs (Admin portal → *Service principals can access admin APIs used for updates*).

### 9.3 A change applied but isn't visible yet

**Cause:** Tenant setting changes propagate gradually.

**Resolution:** Wait — changes can take **up to 15 minutes** to take effect across the organization.

### 9.4 Declared security groups are ignored

**Cause:** The setting does not support per-group scoping (`canSpecifySecurityGroups` is false). `admin plan` prints a warning for this.

**Resolution:** Apply the setting org-wide (`enabled: true`/`false`) without `enabled_security_groups`.

---

## 10. Getting help

- Run `udp-cicd diag` to diagnose common issues.
- Check [GitHub Issues](https://github.com/PatrickGallucci/udp-cicd/issues) for known bugs.
- File a new issue with the full error message and your `udp.yml` (redact secrets).
