### Environment Variables in Docker and Kubernetes

As your instructor on Docker and Kubernetes, today we'll cover "environment variables," which are essential for configuring applications without hardcoding values into code. Environment variables allow you to pass dynamic settings like API keys, database URLs, or feature flags into containers, promoting portability across environments (dev, staging, prod). We'll explore how they work in Docker for straightforward setups and Kubernetes for managed orchestration, with real-world examples integrating with technologies like CI/CD pipelines, databases, cloud services, and monitoring tools. Diagrams will help visualize the concepts.

#### Environment Variables in Docker: Flexible Configuration via Flags and Files

In Docker, environment variables are set when running containers using the `-e` or `--env` flag, or loaded from a `.env` file. This injects key-value pairs into the container's process environment, accessible via `process.env` in Node.js or `os.Getenv` in Go. Docker also supports `ENV` in Dockerfiles for build-time defaults, which can be overridden at runtime.

Here's how it works:
- **Key Components**: Use `-e KEY=VALUE` for single vars, or `--env-file .env` for multiple. Variables can reference host env vars (e.g., `-e DB_HOST=${HOST_DB}`) for dynamic injection.
- **Workflow**: Build an image with defaults, then run with overrides: `docker run -e APP_ENV=production my-image`. In multi-container apps, Docker Compose uses `environment` in YAML or loads from `.env` files.




**Real-World Example: A Microservice API with Docker and Integration with Other Technologies**
Consider a payment processing service like one at Stripe, where sensitive configs vary by environment. A Node.js app might need a database URL and API key.

In a Dockerfile:
```dockerfile
FROM node:18
ENV APP_ENV=development  # Default
WORKDIR /app
COPY . .
CMD ["node", "app.js"]
```

Run with vars: `docker run -e DB_URL=postgres://user:pass@host:5432/db -e API_KEY=sk_test_123 my-payment-image`.

This integrates with technologies like:
- **CI/CD Pipelines (e.g., GitHub Actions or Jenkins)**: At companies like Netflix, GitHub Actions builds Docker images and injects env vars from secrets during deployment. For instance, in a workflow YAML, use `${{ secrets.DB_PASSWORD }}` to set `-e DB_PASS` securely, enabling automated testing and promotion to production without exposing creds.
- **Cloud Services (e.g., AWS ECS or Google Cloud Run)**: In AWS ECS task definitions, map env vars to pull from SSM Parameter Store. This allows dynamic scaling for e-commerce spikes, where vars like `MAX_CONNECTIONS` adjust based on load, integrating with AWS Lambda for serverless triggers.

Using `.env` files keeps configs separate, as seen in open-source projects where developers override defaults for local runs without committing secrets.

#### Environment Variables in Kubernetes: Declarative Management with ConfigMaps and Secrets

Kubernetes handles environment variables in Pod specs under the `env` field, allowing direct values, references to ConfigMaps (for non-sensitive data), or Secrets (for sensitive info). This declarative approach ensures consistency across replicas and environments, with K8s injecting vars at runtime.

Here's how it works:
- **Key Components**: Define `env` as a list of name-value pairs, or use `envFrom` to pull from ConfigMaps/Secrets. Vars can reference downward API (e.g., Pod name) or host env.
- **Workflow**: Create a ConfigMap/Secret, reference it in the Pod YAML, and apply: `kubectl apply -f pod.yaml`. Updates require rolling restarts for changes to propagate.




**Real-World Example: A Scalable Web App in Kubernetes and Integration with Other Technologies**
Think of a social media platform like Twitter (now X), where user services need configurable endpoints for databases and caches.

Pod YAML snippet:
```yaml
apiVersion: v1
kind: Pod
metadata:
  name: web-pod
spec:
  containers:
  - name: app
    image: web-app:latest
    env:
      - name: DB_URL
        value: "mysql://user:pass@db-service:3306/app"
      - name: LOG_LEVEL
        valueFrom:
          configMapKeyRef:
            name: app-config
            key: log_level
    envFrom:
      - secretRef:
          name: api-secrets  # Pulls all keys as env vars
```

This multi-var setup integrates with technologies like:
- **Databases and Monitoring (e.g., Prometheus and PostgreSQL)**: At enterprises like Google, env vars set Prometheus scrape intervals (e.g., `SCRAPE_INTERVAL=15s`), while database connections pull from Secrets. This enables auto-scaling Pods to monitor query performance, integrating with Grafana for dashboards.
- **Cloud Orchestration (e.g., Azure AKS)**: In AKS, use Azure Key Vault to sync secrets as env vars. For apps like banking systems at JPMorgan, vars control feature toggles, allowing A/B testing without redeploys, while integrating with Kafka for event-driven updates.

In large clusters like those at Uber, ConfigMaps manage app configs across namespaces, reducing misconfigurations in hybrid cloud setups.

#### Key Differences and Best Practices
- **Docker**: Runtime-focused, simple for local use; vars are ephemeral unless in images.
- **Kubernetes**: Declarative, scalable; prefers external sources like ConfigMaps for separation of concerns.

| Aspect | Docker Env Vars | Kubernetes Env Vars |
|--------|-----------------|---------------------|
| Setting Method | -e flag or .env file | env field in YAML, ConfigMap/Secret refs |
| Scope | Per container run | Pod-wide, injectable across replicas |
| Security | Avoid sensitive in images; use --env-file | Use Secrets for encryption at rest |
| Use Case | Quick dev with local Redis | Prod with AWS Secrets Manager integration |

Best practices: Never hardcode secrets, use external managers like HashiCorp Vault, and validate vars with tools like envsubst in CI/CD.

If you'd like hands-on exercises, such as injecting vars into a sample app or managing Secrets in Minikube, let me know—we can build on this!