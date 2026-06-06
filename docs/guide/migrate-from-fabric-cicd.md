# Migrate from fabric-cicd

This page describes how to migrate from [fabric-cicd](https://github.com/microsoft/fabric-cicd) to udp-cicd: the functional differences between the two tools, the criteria for migrating, and the migration steps.

---

## 1. Comparison

| Capability | fabric-cicd | udp-cicd |
|---|------------|-----------|
| Approach | Git sync-based deployment | Declarative YAML + API |
| Config | Python code | udp.yml |
| Item support | Git-synced items only | 45 item types |
| Creates workspaces | No | Yes |
| Creates lakehouses | No | Yes |
| Creates environments | No | Yes |
| Security roles | No | Yes |
| Drift detection | No | Yes |
| Rollback | No | Yes |
| MCP server | No | Yes |
| State tracking | No | Yes |

udp-cicd is distributed as a .NET global tool. Install it with:

```bash
dotnet tool install --global udp-cicd
```

---

## 2. When to migrate

| Migrate to udp-cicd if you need | Stay with fabric-cicd if |
|---|---|
| Infrastructure creation (workspaces, lakehouses, environments) | You only need to promote git-synced content between workspaces |
| Security role automation | Your infrastructure is already created and managed manually |
| Drift detection | |
| Full item type coverage beyond git-synced items | |
| Declarative YAML instead of Python code | |

---

## 3. Migration steps

### 3.1 Export your workspace

```bash
udp-cicd generate --workspace "your-dev-workspace"
```

This runs reverse generation: it scans the live workspace through the Fabric REST API and writes `udp.yml` plus the source files for each item.

### 3.2 Review the generated udp.yml

The generated file captures all items in your workspace. Edit it to:

- Add targets for staging/prod.
- Add security roles.
- Add variable overrides per target.
- Remove items you do not want managed.

### 3.3 Set up CI/CD

Replace your fabric-cicd pipeline with udp-cicd:

**Before (fabric-cicd):**

```python
from fabric_cicd import FabricWorkspace
ws = FabricWorkspace(workspace_id="...", repository_directory=".")
ws.publish_all_items()
```

**After (udp-cicd):**

```bash
udp-cicd deploy --target prod -y
```

### 3.4 Test

```bash
udp-cicd validate
udp-cicd plan --target staging
udp-cicd deploy --target staging
```
