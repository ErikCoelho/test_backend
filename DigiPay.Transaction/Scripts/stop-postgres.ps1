$ErrorActionPreference = "Stop"

Write-Host "Parando PostgreSQL para DigiPay Transaction..." -ForegroundColor Cyan

# Verificando se o Docker está em execução
try {
    docker info > $null
}
catch {
    Write-Host "Erro: Docker não está em execução." -ForegroundColor Red
    exit 1
}

# Verificando se o contêiner existe e está em execução
$containerName = "digipay-transaction-postgres"
$containerRunning = docker ps --filter "name=$containerName" --format "{{.Names}}"

if ($containerRunning) {
    Write-Host "Parando contêiner PostgreSQL..." -ForegroundColor Yellow
    docker stop $containerName
    Write-Host "PostgreSQL parado com sucesso." -ForegroundColor Green
}
else {
    Write-Host "O contêiner PostgreSQL não está em execução." -ForegroundColor Yellow
} 