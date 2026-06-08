# UDP-CICD `udp.yml` Editor

A standalone Windows Forms application for **creating and updating `udp.yml`
deployment definitions** with a tree + property-grid UI — no YAML hand-editing
required.

It references `UdpCicd.Core` and reuses the project's own model graph and
YamlDotNet configuration, so the files it writes are byte-compatible with the
`udp-cicd` CLI and MCP server, and it automatically supports **all 45 Fabric
resource types**.

## Running

```powershell
# from the dotnet/ folder
dotnet run --project src/UdpCicd.Editor

# or open a file directly
dotnet run --project src/UdpCicd.Editor -- ..\examples\02-medallion-lakehouse\udp.yml
```

A published single build:

```powershell
dotnet build src/UdpCicd.Editor -c Release
# -> src/UdpCicd.Editor/bin/Release/net9.0-windows/UdpCicd.Editor.exe
```

> Windows-only: this project targets `net9.0-windows` and uses Windows Forms.

## What you can edit

- **Deployment** metadata, **Workspace** (incl. Git integration), **Variables**
  (literal or description/default form).
- **Resources** — every supported type (lakehouses, notebooks, pipelines,
  warehouses, semantic models, reports, data agents, eventhouses/streams, KQL,
  ML, graphs, mirrored databases, …). Add via **Edit ▸ Add Resource**.
- **Security** roles + OneLake role bindings, **Connections**, **Targets**
  (per-environment workspace/variables/run-as/strategy), **Admin** tenant
  settings, and the advanced **Policies / Notifications / State** sections.

The right-hand property grid edits scalars, enums, lists (collection editor),
string maps (key/value dialog), and nested objects (inline or via a dialog that
can create a missing section). Rename a key with **F2** / right-click; remove
with **Delete**.

## Tools

- **Tools ▸ Validate** (`F5`) runs the same cross-reference and naming checks as
  `udp-cicd validate`.
- **Tools ▸ View YAML** previews exactly what will be written.
- **Tools ▸ Group items into workspace folders by type** sets
  `workspace.folders_by_type`, so `deploy` organizes items into Fabric workspace
  folders by type (Notebooks, Pipelines, Lakehouses, Reports, Models, Databases,
  Warehouses, Environments, Agents, Variables). A per-item `folder` overrides the
  type folder.

## Notes on output

- The editor **does not** resolve `${var.*}` references or merge `include` /
  `extends` — it round-trips the literal source, as an editor should.
- Output omits nulls and empty collections, so unused resource maps don't clutter
  the file; meaningful defaults (e.g. `enable_schemas: true`) are kept.
