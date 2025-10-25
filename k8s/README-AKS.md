# AKS deployment notes — biursite backend + redis

This folder contains Kubernetes manifests and a PowerShell helper to deploy only the backend API and Redis to Azure Kubernetes Service (AKS).

Files added:

- `aks-backend-deployment.yaml` — Deployment for the .NET backend (2 replicas). The container expects to listen on port 8080.
- `aks-backend-service.yaml` — Service of type LoadBalancer with the annotation `service.beta.kubernetes.io/azure-dns-label-name: "biursitebackend"` to get the DNS `biursitebackend.<location>.cloudapp.azure.com`.
- `redis-deployment.yaml` — Single-replica Redis Deployment using `redis:7-alpine` (appendonly enabled). This is ephemeral by default; add a PVC if you want persistence.
- `redis-service.yaml` — ClusterIP Service for redis (reachable at `biursite-redis:6379`).
- `deploy-aks.ps1` — PowerShell helper to create AKS (optional), create `biursite-secrets` from the repo `.env`, apply manifests, and show the external endpoint.

Quick steps (summary):

1. Build and push your backend image to a registry (Docker Hub example):

   - Ensure `backend` image is built and pushed with tag matching image in `aks-backend-deployment.yaml` (the manifest now defaults to `kai1409/biursite-backend:latest`). You can override by passing `-DockerHubUser` to the script to set the image.

2. Create AKS (optional) and deploy:

   - From the `k8s` directory run (PowerShell):
     .\deploy-aks.ps1 -ResourceGroup "biursite-rg" -Location "eastus" -ClusterName "biursite-aks" -Namespace "biursite" -DockerHubUser "kai1409" -CreateCluster

   - If you already have an AKS cluster, omit `-CreateCluster`.

3. Secrets:

   - The script will create a Kubernetes secret `biursite-secrets` from the repo `.env` (path `../.env` relative to this script). If you don't want to keep `.env` in the repo, run:

     kubectl create secret generic biursite-secrets --from-literal=JWT_SECRET_KEY="..." --from-literal=MONGODB_CONNECTION_STRING="..." -n biursite

4. DNS & public endpoint:

   - The backend service uses the DNS label `biursitebackend`. AKS/Azure allocates a public IP and creates the hostname `biursitebackend.<location>.cloudapp.azure.com` (for example `biursitebackend.eastus.cloudapp.azure.com`).

Notes & next steps

- The `aks-backend-deployment.yaml` manifest is set to `kai1409/biursite-backend:latest` by default. You can change it directly in the YAML or override at deploy time by passing `-DockerHubUser` to `deploy-aks.ps1`.
- For production, use:
  - Private image registry and `imagePullSecrets`.
  - Persistent storage (PVC) for Redis if durability is required.
  - Managed Redis (Azure Cache for Redis) instead of in-cluster Redis for better reliability.
  - TLS/HTTPS and API authentication, and optionally an Ingress with cert-manager for TLS.
