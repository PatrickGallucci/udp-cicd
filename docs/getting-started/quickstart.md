# Quick Start

This tutorial creates a Fabric project from a built-in template and deploys it to a development workspace. It assumes the tool is installed and authentication is configured; see [Installation](installation.md) if not.

---

## 1. Create a new project

```bash
udp-cicd init --template medallion --name udp-analytics
cd udp-analytics
```

Project templates are rendered from `Assets/templates/` using `${{ variable }}` placeholder substitution. Two templates are available:

| Template | Contents |
|---|---|
| `medallion` | A complete medallion-architecture project (see below). |
| `blank` | A minimal `udp.yml` with no resources, for starting from scratch. |

The `medallion` template creates a project containing:

| Resource | Count | Purpose |
|---|---|---|
| Lakehouses | 3 | Bronze, silver, and gold layers |
| Notebooks | 3 | ETL processing per layer |
| Data pipelines | 1 | Orchestration with scheduling |
| Spark environments | 1 | Shared compute configuration |
| Data agents | 1 | Conversational access to gold data |
| Targets | 3 | Dev, staging, and prod environment definitions |

---

## 2. Configure your capacity

Find the Fabric capacity GUID:

```bash
az rest --method get \
  --url "https://api.fabric.microsoft.com/v1/capacities" \
  --resource "https://api.fabric.microsoft.com"
```

Update `udp.yml` with the capacity ID:

```yaml
workspace:
  capacity_id: "your-capacity-guid-here"
```

---

## 3. Validate, plan, and deploy

Run the three commands in sequence:

| Command | Purpose |
|---|---|
| `udp-cicd validate` | Parses `udp.yml`, resolves variables, and checks the definition against the schema. No API calls are made. |
| `udp-cicd plan --target dev` | Dry run. Compares desired state against actual workspace state and prints the create, update, and delete actions that a deploy would perform. |
| `udp-cicd deploy --target dev` | Executes the plan against the Fabric API and records the result in deployment state. |

```bash
udp-cicd validate
udp-cicd plan --target dev
udp-cicd deploy --target dev
```

---

## 4. Check status and drift

```bash
udp-cicd status --target dev
udp-cicd drift --target dev
```

`status` reports what is deployed according to the recorded state. `drift` compares recorded state against the live workspace and reports out-of-band changes.

---

## 5. Quick start with MCP (GitHub Copilot or Claude Code)

With GitHub Copilot or Claude Code, Fabric can be managed conversationally instead of through CLI commands.

### 5.1 Install with MCP support

```bash
dotnet tool install --global udp-cicd-mcp
```

### 5.2 Authenticate

```bash
az login
```

### 5.3 Add the MCP server

**GitHub Copilot** -- add to `.vscode/mcp.json` in the project root (or VS Code user `settings.json` under `"mcp.servers"` for global use):

```json
{
  "servers": {
    "udp-cicd": {
      "command": "udp-cicd-mcp"
    }
  }
}
```

**Claude Code** -- add to `.claude/settings.json` in the project root (or `~/.claude/settings.json` for global use):

```json
{
  "mcpServers": {
    "udp-cicd": {
      "command": "udp-cicd-mcp"
    }
  }
}
```

**Claude Desktop** -- add to `~/Library/Application Support/Claude/claude_desktop_config.json` (macOS):

```json
{
  "mcpServers": {
    "udp-cicd": {
      "command": "udp-cicd-mcp"
    }
  }
}
```

### 5.4 Add AI instructions to your project (optional, recommended)

Copy the instruction file for your assistant into the project. These files give the AI assistant context about the project structure and available tools.

| Assistant | Source | Destination |
|---|---|---|
| GitHub Copilot | `examples/.github/copilot-instructions.md` | `.github/copilot-instructions.md` |
| Claude Code | `examples/CLAUDE.md` | `CLAUDE.md` (project root) |

### 5.5 Start talking

```
You: "Create a new Fabric project for sales analytics"
You: "What capacities do I have?"
You: "Deploy to dev"
You: "Run the ingest notebook"
You: "Check for drift in prod"
You: "Show me what's deployed"
```

The assistant uses the 14 MCP tools (`udp_validate`, `udp_plan`, `udp_deploy`, `udp_status`, `udp_drift`, `udp_run`, and others) to execute requests against the live Fabric API.

### 5.6 Combine with the Fabric VS Code extension

For the best experience, also install the [Fabric Data Engineering VS Code Extension](https://marketplace.visualstudio.com/items?itemName=ms-udp.udpdataengineering). This combination supports:

- Editing notebooks in VS Code with AI assistance
- Running cells on remote Fabric Spark compute
- Managing infrastructure through the udp-cicd MCP server
- Completing the full workflow without leaving VS Code

See [Development Workflows](../guide/development-workflows.md) for detailed patterns.
