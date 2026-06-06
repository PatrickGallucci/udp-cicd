# ${{project_name}}

A Microsoft Fabric project managed by [Unified Data Platform Deployment](https://github.com/PatrickGallucci/udp-cicd).

## Project Structure

```
${{project_name}}/
├── udp.yml              # Deployment definition — all resources, targets, security
├── notebooks/                    # Notebooks and source code
│   └── sample_notebook.py  # Sample PySpark notebook
├── resources/              # Pipeline configs, SQL scripts, agent instructions
├── tests/                  # Validation tests
│   └── test_validate.py    # Checks udp.yml is valid
└── README.md               # This file
```

## Getting Started

### Prerequisites

- Python 3.10+
- Azure CLI (`az login`)
- A Microsoft Fabric capacity

### Setup

```bash
# Install udp-cicd
dotnet tool install --global udp-cicd

# Find your capacity GUID
az rest --method get \
  --url "https://api.fabric.microsoft.com/v1/capacities" \
  --resource "https://api.fabric.microsoft.com"

# Update udp.yml with your capacity_id
```

### Deploy

```bash
udp-cicd validate          # Check for errors
udp-cicd plan -t dev       # Preview changes
udp-cicd deploy -t dev     # Deploy to dev workspace
```

### Develop

Edit notebooks in `notebooks/`, then redeploy:

```bash
udp-cicd deploy -t dev
```

Or use the [Fabric VS Code Extension](https://learn.microsoft.com/en-us/udpric/data-engineering/setup-vs-code-extension) to edit and run notebooks on remote Spark compute.

### CI/CD

See the [CI/CD guide](https://PatrickGallucci.github.io/udp-cicd/cicd/overview/) for GitHub Actions and Azure DevOps templates.

## Commands

| Command | Description |
|---------|-------------|
| `udp-cicd validate` | Validate udp.yml |
| `udp-cicd plan -t dev` | Preview changes |
| `udp-cicd deploy -t dev` | Deploy to dev |
| `udp-cicd destroy -t dev` | Tear down dev |
| `udp-cicd status -t dev` | Show deployed resources |
| `udp-cicd drift -t dev` | Detect portal changes |
| `udp-cicd run <notebook>` | Run a notebook |
