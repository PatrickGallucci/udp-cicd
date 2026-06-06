# Your First Deployment

A minimal `udp.yml`:

```yaml
deployment:
  name: hello-udp
  version: "1.0.0"

workspace:
  capacity_id: "your-capacity-guid"

resources:
  lakehouses:
    my_lakehouse:
      description: "My first lakehouse"

  notebooks:
    hello_notebook:
      path: ./notebooks/hello.py
      description: "Hello world notebook"

targets:
  dev:
    default: true
    workspace:
      name: hello-udp-dev
```

Create the notebook:

```bash
mkdir notebooks
echo '# Hello from Unified Data Platform Deployment
print("It works!")' > notebooks/hello.py
```

Deploy:

```bash
udp-cicd validate
udp-cicd deploy --target dev
```
