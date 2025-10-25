<#
deploy-aks.ps1 — creates AKS + ACR (optional), builds image in ACR and deploys manifests.

Usage example:
  .\deploy-aks.ps1 -ResourceGroup "biursite-rg" -Location "eastus" -ClusterName "biursite-aks" -Namespace "biursite" -UseAcr -CreateCluster
#>

param(
    [string]$ResourceGroup = "biursite-rg",
    [string]$Location = "eastus",
    [string]$ClusterName = "biursite-aks",
    [string]$Namespace = "biursite",
    [string]$DockerHubUser = "kai1409",
    [switch]$CreateCluster,
    [switch]$UseAcr
)

function Ensure-CommandExists([string]$cmd) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Error "Required command '$cmd' is not available. Please install it and re-run the script."
        exit 1
    }
}

Ensure-CommandExists -cmd az
Ensure-CommandExists -cmd kubectl

Write-Host "ResourceGroup: $ResourceGroup | Location: $Location | Cluster: $ClusterName | Namespace: $Namespace"

# Ensure user is logged in to Azure
az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please authenticate to Azure. A browser window will open..."
    az login | Out-Null
}

if ($CreateCluster) {
    Write-Host "Creating resource group (if missing)..."
    az group create --name $ResourceGroup --location $Location | Out-Null

    if ($UseAcr) {
        $rand = Get-Random -Maximum 99999
        $AcrName = ((($ClusterName + "acr" + $rand) -replace '[^a-zA-Z0-9]','') ).ToLower()
        Write-Host "Creating Azure Container Registry: $AcrName"
        az acr create --resource-group $ResourceGroup --name $AcrName --sku Basic --location $Location --admin-enabled false | Out-Null

        Write-Host "Creating AKS cluster and attaching ACR (this can take 10+ minutes)."
        az aks create --resource-group $ResourceGroup --name $ClusterName --node-count 2 --attach-acr $AcrName --generate-ssh-keys --location $Location | Out-Null
    } else {
        Write-Host "Creating AKS cluster (this can take 10+ minutes)."
        az aks create --resource-group $ResourceGroup --name $ClusterName --node-count 2 --generate-ssh-keys --location $Location | Out-Null
    }
}

Write-Host "Getting AKS credentials..."
az aks get-credentials --resource-group $ResourceGroup --name $ClusterName --overwrite-existing | Out-Null

Write-Host "Creating namespace $Namespace (if not exists)"
kubectl get namespace $Namespace -o name 2>$null
if ($LASTEXITCODE -ne 0) { kubectl create namespace $Namespace }

Write-Host "Creating Kubernetes secret 'biursite-secrets' from repository .env"
if (-Not (Test-Path "../.env")) {
    Write-Host "WARNING: .env not found at repository root. Create secrets manually or place a .env file at repo root before running this script." -ForegroundColor Yellow
} else {
    kubectl delete secret biursite-secrets -n $Namespace 2>$null | Out-Null
    kubectl create secret generic biursite-secrets --from-env-file="../.env" -n $Namespace
}

Write-Host "Applying Redis manifests"
kubectl apply -f ./redis-deployment.yaml -n $Namespace
kubectl apply -f ./redis-service.yaml -n $Namespace

if ($UseAcr) {
    if (-not $AcrName) {
        $possibleAcr = az acr list --resource-group $ResourceGroup --query "[0].name" -o tsv 2>$null
        if ($possibleAcr) { $AcrName = $possibleAcr } else { Write-Error "ACR name not found. Create ACR or run with -CreateCluster -UseAcr"; exit 1 }
    }

    $acrImage = "$AcrName.azurecr.io/biursite-backend:latest"
    Write-Host "Starting ACR build for image $acrImage (this runs in Azure - may take several minutes)."
    az acr build --registry $AcrName --image "biursite-backend:latest" ../backend | Out-Null

    Write-Host "Applying backend manifests"
    kubectl apply -f ./aks-backend-deployment.yaml -n $Namespace
    kubectl apply -f ./aks-backend-service.yaml -n $Namespace

    Write-Host "Setting backend image to $acrImage"
    kubectl set image deployment/biursite-backend biursite-backend=$acrImage -n $Namespace --record 2>$null | Out-Null
} else {
    Write-Host "Applying backend manifests (DockerHub flow)"
    kubectl apply -f ./aks-backend-deployment.yaml -n $Namespace
    kubectl apply -f ./aks-backend-service.yaml -n $Namespace

    if ($DockerHubUser -and $DockerHubUser -ne "") {
        $image = "${DockerHubUser}/biursite-backend:latest"
        Write-Host "Setting backend image to $image"
        kubectl set image deployment/biursite-backend biursite-backend=$image -n $Namespace --record 2>$null | Out-Null
    }
}

Write-Host "Waiting for external IP to be provisioned for the backend service (this may take 1-3 minutes)..."
for ($i=0; $i -lt 60; $i++) {
    $svcJson = kubectl get svc biursite-backend-svc -n $Namespace -o json 2>$null
    if (-not $svcJson) { Start-Sleep -Seconds 3; continue }
    $svc = $svcJson | ConvertFrom-Json
    $ip = $null; $fqdn = $null
    if ($svc.status -and $svc.status.loadBalancer -and $svc.status.loadBalancer.ingress) {
        if ($svc.status.loadBalancer.ingress.Count -gt 0) {
            $entry = $svc.status.loadBalancer.ingress[0]
            if ($entry.ip) { $ip = $entry.ip }
            if ($entry.hostname) { $fqdn = $entry.hostname }
        }
    }

    if ($ip -or $fqdn) {
        if ($ip) { Write-Host "Service External: $ip" } else { Write-Host "Service External: $fqdn" }
        break
    }
    Start-Sleep -Seconds 6
}

Write-Host "If you used the annotation 'biursitebackend' the full DNS will be: biursitebackend.$Location.cloudapp.azure.com"
Write-Host "You can test the health endpoint once the service is up:"
Write-Host "  curl http://biursitebackend.$Location.cloudapp.azure.com/health"
<#
<#
deploy-aks.ps1 — creates AKS + ACR (optional), builds image in ACR and deploys manifests.

Usage example:
  .\deploy-aks.ps1 -ResourceGroup "biursite-rg" -Location "eastus" -ClusterName "biursite-aks" -Namespace "biursite" -UseAcr -CreateCluster
#>

param(
    [string]$ResourceGroup = "biursite-rg",
    [string]$Location = "eastus",
    [string]$ClusterName = "biursite-aks",
    [string]$Namespace = "biursite",
    [string]$DockerHubUser = "kai1409",
    [switch]$CreateCluster,
    [switch]$UseAcr
)

function Ensure-CommandExists([string]$cmd) {
    if (-not (Get-Command $cmd -ErrorAction SilentlyContinue)) {
        Write-Error "Required command '$cmd' is not available. Please install it and re-run the script."
        exit 1
    }
}

Ensure-CommandExists -cmd az
Ensure-CommandExists -cmd kubectl

Write-Host "ResourceGroup: $ResourceGroup | Location: $Location | Cluster: $ClusterName | Namespace: $Namespace"

# Ensure user is logged in to Azure
az account show 2>$null
if ($LASTEXITCODE -ne 0) {
    Write-Host "Please authenticate to Azure. A browser window will open..."
    az login | Out-Null
}

if ($CreateCluster) {
    Write-Host "Creating resource group (if missing)..."
    az group create --name $ResourceGroup --location $Location | Out-Null

    if ($UseAcr) {
        $rand = Get-Random -Maximum 99999
        $AcrName = ((($ClusterName + "acr" + $rand) -replace '[^a-zA-Z0-9]','') ).ToLower()
        Write-Host "Creating Azure Container Registry: $AcrName"
        az acr create --resource-group $ResourceGroup --name $AcrName --sku Basic --location $Location --admin-enabled false | Out-Null

        Write-Host "Creating AKS cluster and attaching ACR (this can take 10+ minutes)."
        az aks create --resource-group $ResourceGroup --name $ClusterName --node-count 2 --attach-acr $AcrName --generate-ssh-keys --location $Location | Out-Null
    } else {
        Write-Host "Creating AKS cluster (this can take 10+ minutes)."
        az aks create --resource-group $ResourceGroup --name $ClusterName --node-count 2 --generate-ssh-keys --location $Location | Out-Null
    }
}

Write-Host "Getting AKS credentials..."
az aks get-credentials --resource-group $ResourceGroup --name $ClusterName --overwrite-existing | Out-Null

Write-Host "Creating namespace $Namespace (if not exists)"
kubectl get namespace $Namespace -o name 2>$null
if ($LASTEXITCODE -ne 0) { kubectl create namespace $Namespace }

Write-Host "Creating Kubernetes secret 'biursite-secrets' from repository .env"
if (-Not (Test-Path "../.env")) {
    Write-Host "WARNING: .env not found at repository root. Create secrets manually or place a .env file at repo root before running this script." -ForegroundColor Yellow
} else {
    kubectl delete secret biursite-secrets -n $Namespace 2>$null | Out-Null
    kubectl create secret generic biursite-secrets --from-env-file="../.env" -n $Namespace
}

Write-Host "Applying Redis manifests"
kubectl apply -f ./redis-deployment.yaml -n $Namespace
kubectl apply -f ./redis-service.yaml -n $Namespace

if ($UseAcr) {
    if (-not $AcrName) {
        $possibleAcr = az acr list --resource-group $ResourceGroup --query "[0].name" -o tsv 2>$null
        if ($possibleAcr) { $AcrName = $possibleAcr } else { Write-Error "ACR name not found. Create ACR or run with -CreateCluster -UseAcr"; exit 1 }
    }

    $acrImage = "$AcrName.azurecr.io/biursite-backend:latest"
    Write-Host "Starting ACR build for image $acrImage (this runs in Azure - may take several minutes)."
    az acr build --registry $AcrName --image "biursite-backend:latest" ../backend | Out-Null

    Write-Host "Applying backend manifests"
    kubectl apply -f ./aks-backend-deployment.yaml -n $Namespace
    kubectl apply -f ./aks-backend-service.yaml -n $Namespace

    Write-Host "Setting backend image to $acrImage"
    kubectl set image deployment/biursite-backend biursite-backend=$acrImage -n $Namespace --record 2>$null | Out-Null
} else {
    Write-Host "Applying backend manifests (DockerHub flow)"
    kubectl apply -f ./aks-backend-deployment.yaml -n $Namespace
    kubectl apply -f ./aks-backend-service.yaml -n $Namespace

    if ($DockerHubUser -and $DockerHubUser -ne "") {
        $image = "${DockerHubUser}/biursite-backend:latest"
        Write-Host "Setting backend image to $image"
        kubectl set image deployment/biursite-backend biursite-backend=$image -n $Namespace --record 2>$null | Out-Null
    }
}

Write-Host "Waiting for external IP to be provisioned for the backend service (this may take 1-3 minutes)..."
for ($i=0; $i -lt 30; $i++) {
    $svcJson = kubectl get svc biursite-backend-svc -n $Namespace -o json 2>$null
    if (-not $svcJson) { Start-Sleep -Seconds 3; continue }
    $svc = $svcJson | ConvertFrom-Json
    $ip = $null; $fqdn = $null
    if ($svc.status -and $svc.status.loadBalancer -and $svc.status.loadBalancer.ingress) {
        if ($svc.status.loadBalancer.ingress.Count -gt 0) {
            $entry = $svc.status.loadBalancer.ingress[0]
            if ($entry.ip) { $ip = $entry.ip }
            if ($entry.hostname) { $fqdn = $entry.hostname }
        }
    }

    if ($ip -or $fqdn) {
        if ($ip) { Write-Host "Service External: $ip" } else { Write-Host "Service External: $fqdn" }
        break
    }
    Start-Sleep -Seconds 6
}

Write-Host "If you used the annotation 'biursitebackend' the full DNS will be: biursitebackend.$Location.cloudapp.azure.com"
Write-Host "You can test the health endpoint once the service is up:"
Write-Host "  curl http://biursitebackend.$Location.cloudapp.azure.com/health"

    az group create --name $ResourceGroup --location $Location | Out-Null

    if ($UseAcr) {
        # generate a reasonably unique ACR name (lowercase, 5-50 chars)
        $rand = Get-Random -Maximum 99999
        $AcrName = ((($ClusterName + "acr" + $rand) -replace '[^a-zA-Z0-9]','') ).ToLower()
        Write-Host "Creating Azure Container Registry: $AcrName"
        az acr create --resource-group $ResourceGroup --name $AcrName --sku Basic --location $Location --admin-enabled false | Out-Null

        Write-Host "Creating AKS cluster and attaching ACR (this can take 10+ minutes)."
        az aks create --resource-group $ResourceGroup --name $ClusterName --node-count 2 --attach-acr $AcrName --generate-ssh-keys --location $Location | Out-Null
    }
    else {
        Write-Host "Creating AKS cluster (this can take 10+ minutes)."
        az aks create --resource-group $ResourceGroup --name $ClusterName --node-count 2 --generate-ssh-keys --location $Location | Out-Null
    }
}

Write-Host "Getting AKS credentials..."
az aks get-credentials --resource-group $ResourceGroup --name $ClusterName --overwrite-existing | Out-Null

Write-Host "Creating namespace $Namespace (if not exists)"
kubectl get namespace $Namespace -o name 2>$null
if ($LASTEXITCODE -ne 0) { kubectl create namespace $Namespace }

Write-Host "Creating Kubernetes secret 'biursite-secrets' from repository .env"
if (-Not (Test-Path "../.env")) {
    Write-Host "WARNING: .env not found at repository root. Create secrets manually or place a .env file at repo root before running this script." -ForegroundColor Yellow
} else {
    kubectl delete secret biursite-secrets -n $Namespace 2>$null | Out-Null
    kubectl create secret generic biursite-secrets --from-env-file="../.env" -n $Namespace
}

Write-Host "Applying Redis manifests"
kubectl apply -f ./redis-deployment.yaml -n $Namespace
kubectl apply -f ./redis-service.yaml -n $Namespace

if ($UseAcr) {
    if (-not $AcrName) {
        # If ACR was not created in this run, try to detect an existing ACR in the resource group
        $possibleAcr = az acr list --resource-group $ResourceGroup --query "[0].name" -o tsv 2>$null
        if ($possibleAcr) { $AcrName = $possibleAcr }
        else { Write-Error "ACR name not found. Create ACR or run with -CreateCluster -UseAcr"; exit 1 }
    }

    # Build the backend image inside Azure and push to ACR (no DockerHub required).
    $acrImage = "$AcrName.azurecr.io/biursite-backend:latest"
    Write-Host "Starting ACR build for image $acrImage (this runs in Azure - may take several minutes)."
    az acr build --registry $AcrName --image "biursite-backend:latest" ../backend | Out-Null

    Write-Host "Applying backend manifests"
    kubectl apply -f ./aks-backend-deployment.yaml -n $Namespace
    kubectl apply -f ./aks-backend-service.yaml -n $Namespace

    Write-Host "Setting backend image to $acrImage"
    kubectl set image deployment/biursite-backend biursite-backend=$acrImage -n $Namespace --record 2>$null | Out-Null
}
else {
    Write-Host "Applying backend manifests (please replace image placeholder with your Docker image if needed)"
    kubectl apply -f ./aks-backend-deployment.yaml -n $Namespace
    kubectl apply -f ./aks-backend-service.yaml -n $Namespace

    if ($DockerHubUser -and $DockerHubUser -ne "") {
        $image = "${DockerHubUser}/biursite-backend:latest"
        Write-Host "Setting backend image to $image"
        kubectl set image deployment/biursite-backend biursite-backend=$image -n $Namespace --record 2>$null | Out-Null
    }
}

Write-Host "Waiting for external IP to be provisioned for the backend service (this may take 1-3 minutes)..."
for ($i=0; $i -lt 30; $i++) {
    $svc = kubectl get svc biursite-backend-svc -n $Namespace -o json | ConvertFrom-Json
    $ip = $svc.status.loadBalancer.ingress[0].ip 2>$null
    $fqdn = $svc.status.loadBalancer.ingress[0].hostname 2>$null
    if ($ip -or $fqdn) {
        Write-Host "Service External: $($ip ? $ip : $fqdn)"
        break
    }
    Start-Sleep -Seconds 6
}

Write-Host "If you used the annotation 'biursitebackend' the full DNS will be: biursitebackend.$Location.cloudapp.azure.com"
Write-Host "You can test the health endpoint once the service is up:"
Write-Host "  curl http://biursitebackend.$Location.cloudapp.azure.com/health"
