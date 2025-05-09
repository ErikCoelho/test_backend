$ErrorActionPreference = "Stop"

Write-Host "Iniciando todo o ambiente DigiPay Auth no Docker..." -ForegroundColor Cyan

# Navegando para o diretório do docker-compose
Set-Location -Path (Split-Path -Parent $PSScriptRoot)

# Verificando se o Docker está em execução
try {
    docker info > $null
}
catch {
    Write-Host "Erro: Docker não está em execução. Por favor, inicie o Docker Desktop e tente novamente." -ForegroundColor Red
    exit 1
}

# # Parar containers existentes
# docker-compose down

# # Reconstruir imagens
# docker-compose build --no-cache

# Iniciando os containers
docker-compose up 

Write-Host "Ambiente DigiPay Auth está em execução!" -ForegroundColor Green
Write-Host "PostgreSQL: localhost:5432" -ForegroundColor Cyan
Write-Host "API: http://localhost:5000" -ForegroundColor Cyan
Write-Host "Para parar o ambiente, execute: .\Scripts\stop-docker-all.ps1" -ForegroundColor Yellow 