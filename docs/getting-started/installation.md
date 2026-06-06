# Installation

This topic describes how to install, configure, and verify Unified Data Platform Deployment on your local machine or CI/CD environment.

---

## Prerequisites

Before you install Unified Data Platform Deployment, make sure your environment meets the following requirements.

### System requirements

| Requirement | Minimum version | Notes |
|---|---|---|
| **.NET SDK** | 9.0 | Required to install and run the tool. Download from [https://dotnet.microsoft.com/download](https://dotnet.microsoft.com/download). |
| **Azure CLI** | 2.50+ | Required for interactive authentication. Install from [https://aka.ms/installazurecli](https://aka.ms/installazurecli). |
| **Operating system** | Windows 10+, macOS 12+, Linux | Any OS supported by the .NET 9 runtime. |

### Microsoft Fabric requirements

| Requirement | Details |
|---|---|
| **Fabric capacity** | An active Fabric capacity (F2 or higher). You need the capacity GUID from the Fabric admin portal. |
| **Workspace permissions** | Admin or Contributor role on the target workspace, or permissions to create new workspaces. |
| **Entra ID (Azure AD) access** | The authenticated identity must have Fabric API permissions. |

> **Important**
>
> Unified Data Platform Deployment calls the Microsoft Fabric REST API. If your organization uses Conditional Access policies or tenant restrictions, confirm that API access is allowed for your identity before proceeding.

---

## Step 1: Install the tool

Unified Data Platform Deployment ships as a [.NET global tool](https://learn.microsoft.com/dotnet/core/tools/global-tools). Install it with the .NET SDK:

```bash
dotnet tool install --global udp-cicd
```

This installs the `udp-cicd` CLI. To use the MCP server (for AI-assisted authoring in Claude Code, Cursor, etc.), install the companion tool:

```bash
dotnet tool install --global udp-cicd-mcp
```

> **Note**
>
> Global tools are installed to `~/.dotnet/tools` (Linux/macOS) or `%USERPROFILE%\.dotnet\tools` (Windows). Ensure this directory is on your `PATH` — the .NET SDK installer normally adds it for you.

### Install from source

To build and install from the GitHub repository (for example, to use an unreleased feature):

```bash
git clone https://github.com/PatrickGallucci/udp-cicd.git
cd udp-cicd/dotnet
dotnet pack -c Release
dotnet tool install --global --add-source ./src/UdpCicd.Cli/bin/Release udp-cicd
```

---

## Step 2: Verify the installation

### Check the version

```bash
udp-cicd --version
```

### Run the diagnostic check

The `doctor` command validates your environment, authentication, and Fabric API connectivity in a single step:

```bash
udp-cicd doctor
```

**Example output (healthy environment):**

```
udp-cicd doctor

  ✓ .NET runtime 9.0.0
  ✓ Azure CLI installed
  ✓ Azure CLI authenticated
  ✓ Fabric API reachable
  ✓ udp.yml found
  ✓ Deployment validates

  6 passed, 0 failed
```

> **Note**
>
> The `doctor` command does not require a `udp.yml` file. If one is not found, it skips the deployment validation check. All other checks still run.

---

## Step 3: Set up authentication

Unified Data Platform Deployment uses the `Azure.Identity` library and supports two authentication methods: interactive login for development and service principal for CI/CD.

### Interactive login (development)

Use the Azure CLI to sign in interactively. This is the recommended method for local development.

```bash
az login
```

Then run commands against your development environment:

```bash
udp-cicd plan --target dev
udp-cicd deploy --target dev
```

> **Note**
>
> If you have access to multiple Azure tenants, specify the tenant explicitly: `az login --tenant your-tenant-id`.

### Service principal (CI/CD)

For automated pipelines, use a service principal with the following environment variables:

```bash
export AZURE_TENANT_ID="your-tenant-id"
export AZURE_CLIENT_ID="your-client-id"
export AZURE_CLIENT_SECRET="your-client-secret"
```

Then run commands non-interactively using the `--auto-approve` flag:

```bash
udp-cicd deploy --target prod --auto-approve
```

**Setting up a service principal:**

1. Register an application in Entra ID (Azure AD).
2. Create a client secret or configure certificate-based auth.
3. Grant the service principal the **Contributor** role on the target Fabric workspace.
4. If creating workspaces, grant the service principal permission to create workspaces in the Fabric tenant.

> **Warning**
>
> Never commit service principal credentials to source control. Use your CI/CD platform's secret management (for example, GitHub Actions secrets, Azure DevOps variable groups, or Azure Key Vault) to inject these values at runtime.

### GitHub Actions example

```yaml
# .github/workflows/deploy.yml
name: Deploy Fabric Deployment
on:
  push:
    branches: [main]

jobs:
  deploy:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4

      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '9.0.x'

      - run: dotnet tool install --global udp-cicd

      - run: udp-cicd deploy --target prod --auto-approve
        env:
          AZURE_TENANT_ID: ${{ secrets.AZURE_TENANT_ID }}
          AZURE_CLIENT_ID: ${{ secrets.AZURE_CLIENT_ID }}
          AZURE_CLIENT_SECRET: ${{ secrets.AZURE_CLIENT_SECRET }}
```

See [GitHub Actions](../cicd/github-actions.md) and [Azure DevOps](../cicd/azure-devops.md) for complete pipeline templates.

---

## Upgrading

### Check for updates

```bash
udp-cicd check-update
```

### Upgrade to the latest version

```bash
dotnet tool update --global udp-cicd
```

### Pin a specific version

For reproducible CI/CD pipelines, pin the version when installing:

```bash
dotnet tool install --global udp-cicd --version 1.0.1
```

Or commit a [tool manifest](https://learn.microsoft.com/dotnet/core/tools/local-tools) (`.config/dotnet-tools.json`) and run `dotnet tool restore`.

---

## Troubleshooting installation issues

### `udp-cicd: command not found`

**Cause:** The .NET global tools directory is not on your `PATH`.

**Solution:** Add it to your shell profile.

```bash
# macOS / Linux (~/.zshrc or ~/.bashrc)
export PATH="$PATH:$HOME/.dotnet/tools"
```

```powershell
# Windows (PowerShell profile)
$env:PATH += ";$env:USERPROFILE\.dotnet\tools"
```

### `az login` errors

**Cause:** The Azure CLI is not installed or not authenticated.

**Solution:**

```bash
az --version           # confirm the CLI is installed (see https://aka.ms/installazurecli)
az login               # sign in
az account show --query '{name:name, tenantId:tenantId}' -o table
```

### Proxy or firewall issues

If you are behind a corporate proxy, configure NuGet and the Azure CLI:

```bash
export HTTPS_PROXY=http://proxy.example.com:8080
dotnet tool install --global udp-cicd
az login
```

If your firewall blocks nuget.org, install from a private NuGet feed with `--add-source <feed-url>`.

---

## Next steps

- [Quick start tutorial](../getting-started/quickstart.md) -- Create and deploy your first deployment.
- [CLI commands reference](../cli/commands.md) -- Full reference for all `udp-cicd` commands.
- [udp.yml reference](../guide/udp-yml.md) -- Complete schema reference for deployment definitions.
