# udp.yml Editor (Windows)

The **udp.yml Editor** is a standalone Windows desktop application for creating
and updating [`udp.yml`](udp-yml.md) deployment definitions through a graphical
tree + property-grid interface — no hand-editing of YAML required.

It is built on the same `UdpCicd.Core` model and serializer that power the
`udp-cicd` CLI and MCP server, so:

- it understands **all 45 supported resource types** automatically;
- the files it writes are byte-compatible with `udp-cicd deploy`/`validate`;
- it runs the **same validation** as `udp-cicd validate`.

!!! note "Windows only"
    The editor targets `net9.0-windows` and uses Windows Forms. The CLI and MCP
    server remain cross-platform; only this GUI is Windows-specific.

## Installing and running

The editor ships in the repository as the `UdpCicd.Editor` project.

```powershell
# from the dotnet/ folder of the repo
dotnet run --project src/UdpCicd.Editor

# open an existing file directly
dotnet run --project src/UdpCicd.Editor -- ..\examples\02-medallion-lakehouse\udp.yml
```

To produce a runnable build:

```powershell
dotnet build src/UdpCicd.Editor -c Release
# → src/UdpCicd.Editor/bin/Release/net9.0-windows/UdpCicd.Editor.exe
```

You can also launch the `.exe` directly and pass a `udp.yml` path as the first
argument.

## The window

```
┌──────────────────────────────┬───────────────────────────────────────┐
│ Deployment: medallion         │  Name            medallion-analytics   │
│ Workspace                     │  Version         1.0.0                  │
│ Variables (2)                 │  Description      Bronze/Silver/Gold…   │
│ Resources                     │  Folders By Type  True                  │
│   notebooks (3)               │                                         │
│   lakehouses (3)              │   ← property grid edits the item        │
│   pipelines (1)               │     selected on the left                │
│ Security roles (2)            │                                         │
│ Connections (0)               │                                         │
│ Targets (3)                   │                                         │
│ Admin / tenant settings (0)   │                                         │
│ Advanced                      │                                         │
└──────────────────────────────┴───────────────────────────────────────┘
```

- **Left — tree.** Every section of the deployment: `Deployment`, `Workspace`,
  `Variables`, `Resources` (grouped by type, then by key), `Security`,
  `Connections`, `Targets`, `Admin / tenant settings`, and an `Advanced` group
  (`Policies`, `Notifications`, `State`).
- **Right — property grid.** Edits the object selected in the tree: scalars,
  enums (drop-downs), lists (collection editor), string maps (key/value dialog),
  and nested objects (inline or via a dialog that can create a missing section).

## Editing

| Task | How |
|---|---|
| Edit a field | Select a node, change the value in the property grid |
| Add a resource | **Edit ▸ Add Resource…** (`Ctrl+R`), pick a type and key |
| Add a variable / connection / target / tenant setting | **Edit ▸ Add …** |
| Rename a key | Select the node and press **F2** (or right-click ▸ Rename) |
| Remove an item | Select the node and press **Delete** |
| Edit a list (e.g. shortcuts, security roles) | Click the **…** collection editor on the property |
| Edit a key/value map (e.g. spark properties, parameters) | Click the **…** map editor on the property |

## Tools

- **Tools ▸ Validate** (`F5`) — runs the same cross-reference and naming checks
  as `udp-cicd validate` and lists any issues.
- **Tools ▸ View YAML** — previews exactly what will be written to disk.
- **Tools ▸ Group items into workspace folders by type** — sets
  `workspace.folders_by_type` so `deploy` organizes items into Fabric workspace
  folders by type. See [Workspace folders by type](#workspace-folders-by-type).

## Workspace folders by type

Toggling **Group items into workspace folders by type** sets
`workspace.folders_by_type: true`. On deploy, each item is created inside a
Fabric workspace folder named after its type category. A per-item `folder:`
value always overrides the type folder. See the
[`folders_by_type` reference](udp-yml.md#44-workspace-folders-by-type) for the
full type → folder mapping.

## How it maps to `udp.yml`

The editor round-trips the **literal** source:

- It does **not** resolve `${var.*}` references or merge `include` / `extends` —
  those stay verbatim, as an editor should preserve them.
- Output omits nulls and empty collections, so unused resource maps never
  clutter the file; meaningful defaults (e.g. `enable_schemas: true`) are kept.

Because it uses the production model, anything the editor saves can be deployed
directly:

```powershell
udp-cicd validate
udp-cicd plan -t dev
udp-cicd deploy -t dev
```

## Limitations

- Windows only (Windows Forms).
- Loading a file that contains a value the model cannot represent (for example
  an unknown `connections.*.type`) reports a precise error rather than silently
  dropping it — fix the value and reopen.
- Folder assignment applies when items are **created**; existing items are not
  moved into folders on update.
