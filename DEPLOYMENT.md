# Deploying Gulf Info Tracker to Azure

This guide walks through deploying the full stack — ASP.NET Core API, React frontend, PostgreSQL, and Redis — to Azure Container Apps using the Azure Developer CLI (`azd`).

The same `dotnet run` command used for local development continues to work unchanged. The AppHost detects whether it is running locally or being published and automatically selects the right infrastructure.

| Mode | Postgres | Redis | Secrets |
|---|---|---|---|
| `dotnet run` (local) | Docker container | Docker container | `appsettings.Development.json` |
| `azd deploy` (Azure) | PostgreSQL Flexible Server | Azure Cache for Redis | Azure Key Vault |

---

## Prerequisites

Install and verify each tool before starting:

```bash
# Azure CLI
brew install azure-cli
az --version          # must show 2.60+

# Azure Developer CLI
brew install azd
azd version           # must show 1.9+

# Docker Desktop — must be running
docker info

# .NET 10 SDK and Node.js 20+ are already required for local dev
dotnet --version
node --version
```

You also need an **Azure subscription**. A free account at [portal.azure.com](https://portal.azure.com) is sufficient for testing.

---

## Step 1 — Sign in

```bash
az login          # opens a browser for Azure CLI auth
azd auth login    # opens a browser for azd auth
```

Verify you are targeting the correct subscription:

```bash
az account show --query "{name:name, id:id}" -o table
```

To switch subscriptions:

```bash
az account set --subscription "<subscription-name-or-id>"
```

---

## Step 2 — Initialise the `azd` project

Run from the repository root:

```bash
azd init
```

At the prompts:
- **How do you want to initialise your app?** → `Use code in the current directory`
- **Environment name** → e.g. `gulf-info-prod` (used to name Azure resource groups)

This generates two things:
- `azure.yaml` — declares which Aspire services map to which Azure resources
- `infra/` — Bicep templates for Container Apps, PostgreSQL, Redis, Key Vault, Container Registry, and Log Analytics

> The `infra/` directory is auto-generated and can be regenerated at any time. You do not need to edit it unless you want to customise SKUs or regions beyond what `azd` defaults to.

---

## Step 3 — Provision Azure resources

```bash
azd provision
```

This creates all Azure resources in a new resource group (named after your environment). It takes approximately 5–10 minutes. When it completes you will see a summary of created resources and their URLs.

The provisioned resources are:

| Resource | Type |
|---|---|
| Container Apps Environment | Hosts API and frontend containers |
| Container Registry | Stores built container images |
| Azure PostgreSQL Flexible Server | Replaces the local Docker Postgres |
| Azure Cache for Redis | Replaces the local Docker Redis |
| Azure Key Vault | Stores all application secrets |
| Log Analytics Workspace | Collects logs and traces |

Resource connection details are saved to `.azure/<env-name>/.env` and are referenced automatically by subsequent `azd` commands.

---

## Step 4 — Load secrets into Key Vault

Find the Key Vault name that was just created:

```bash
azd env get-values | grep AZURE_KEY_VAULT_NAME
```

Then set each secret. Key Vault uses `--` as the section separator, which maps to `:` in .NET configuration (e.g. `Claude--ApiKey` → `Claude:ApiKey`):

```bash
KV=<your-key-vault-name>

# AI provider and keys (set only the ones you use)
az keyvault secret set --vault-name $KV --name AiProvider      --value "Claude"
az keyvault secret set --vault-name $KV --name Claude--ApiKey  --value "sk-ant-..."
az keyvault secret set --vault-name $KV --name OpenAi--ApiKey  --value "sk-..."

# API bearer token (must match VITE_API_BEARER_TOKEN in the frontend)
az keyvault secret set --vault-name $KV --name Api--BearerToken --value "your-strong-token"

# X (Twitter) API key — only needed if you have X source plugins enabled
az keyvault secret set --vault-name $KV --name X--BearerToken  --value "..."
```

These secrets are automatically injected into the API container at startup. No changes to `appsettings.json` are needed.

---

## Step 5 — Set the frontend bearer token

The React frontend reads `VITE_API_BEARER_TOKEN` at **build time** (Vite bakes environment variables into the JS bundle). Set it in the `azd` environment so it is available when the frontend container is built:

```bash
azd env set VITE_API_BEARER_TOKEN "your-strong-token"
```

Use the same value you set for `Api--BearerToken` in the previous step.

---

## Step 6 — Build and deploy

```bash
azd deploy
```

This:
1. Builds the .NET API as a container image
2. Runs `npm run build` for the React frontend and packages it as a container image
3. Pushes both images to the Azure Container Registry
4. Deploys the new images to their Container Apps
5. Prints the live URLs

Total time: approximately 3–5 minutes.

---

## Step 7 — Verify the deployment

```bash
# Get the API URL
azd env get-values | grep SERVICE_API_ENDPOINT

# Should return 401 — authentication is working
curl https://<api-url>/api/articles

# Should return 200 with JSON — full stack is working
curl -H "Authorization: Bearer your-strong-token" https://<api-url>/api/articles

# Should return source health data
curl -H "Authorization: Bearer your-strong-token" https://<api-url>/api/sources
```

Open the frontend URL in a browser and confirm articles are loading.

---

## Step 8 — Set up CI/CD (optional)

To automatically deploy on every push to `main`:

```bash
azd pipeline config
```

This interactive wizard:
1. Detects your GitHub remote
2. Creates an Azure service principal for the pipeline
3. Stores credentials as GitHub Actions secrets
4. Writes `.github/workflows/azure-dev.yml`

After this, every push to `main` triggers:
1. `dotnet build` + `dotnet test` (all 20 tests must pass)
2. `azd deploy` (if tests pass)

The pipeline uses the `VITE_API_BEARER_TOKEN` secret you set in Step 5.

---

## Updating the deployment

After making code changes locally:

```bash
# Test locally first
dotnet test GulfInfoTracker.sln

# Deploy the changes
azd deploy
```

Only the containers that changed are rebuilt and redeployed. If you change infrastructure (e.g. SKUs in the Bicep), run `azd provision` again before `azd deploy`.

---

## Rotating secrets

To update a secret (e.g. rolling the API bearer token):

```bash
# 1. Update Key Vault
az keyvault secret set --vault-name $KV --name Api--BearerToken --value "new-token"

# 2. Update the frontend build env and redeploy
azd env set VITE_API_BEARER_TOKEN "new-token"
azd deploy
```

The API picks up the new Key Vault value on the next container restart (triggered automatically by `azd deploy`).

---

## Teardown

To delete all Azure resources and stop incurring costs:

```bash
azd down          # deletes the resource group; keeps local azd config
azd down --purge  # also permanently purges the Key Vault (bypasses soft-delete)
```

Local development is unaffected — `dotnet run --project src/GulfInfoTracker.AppHost` continues to work.

---

## Estimated monthly cost

For a low-traffic deployment using the smallest available SKUs:

| Resource | SKU | Est. monthly |
|---|---|---|
| Container Apps — API | Consumption (0.5 vCPU / 1 GiB) | ~$5–10 |
| Container Apps — Web | Consumption (0.25 vCPU / 0.5 GiB) | ~$2–5 |
| Azure PostgreSQL Flexible | Burstable B1ms, 32 GiB | ~$15 |
| Azure Cache for Redis | C0 Basic (250 MB) | ~$16 |
| Container Registry | Basic | ~$5 |
| Key Vault | Standard | ~$1 |
| Log Analytics | Pay-per-GB | ~$2 |
| **Total** | | **~$46–54 / month** |

Scale up the Container Apps SKU or PostgreSQL tier to handle higher traffic without any code changes.

---

## Troubleshooting

**`azd provision` fails with "subscription not found"**
Run `az account show` to confirm you are targeting the correct subscription, then `az account set --subscription <id>`.

**API returns 500 after deploy**
Check logs in the Aspire dashboard or Azure Portal → Container Apps → Log stream. The most common cause is a missing Key Vault secret — verify all secrets from Step 4 are set.

**API returns 401 when using the correct token**
Confirm `Api--BearerToken` in Key Vault exactly matches `VITE_API_BEARER_TOKEN` set in Step 5 (case-sensitive, no leading/trailing spaces).

**Frontend shows no articles**
Open browser DevTools → Network. If requests to `/api/articles` return 401, the `VITE_API_BEARER_TOKEN` was not set before `azd deploy`. Re-run Step 5 and Step 6.

**EF Core migration errors on startup**
Migrations run automatically on API startup (`db.Database.MigrateAsync()`). If the container exits immediately, check the Log stream for "relation does not exist" or connection errors — the PostgreSQL Flexible Server may still be warming up. Restart the Container App from the Azure Portal.
