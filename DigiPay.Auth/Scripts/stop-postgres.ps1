$ErrorActionPreference = "Stop"

Write-Host "Parando PostgreSQL no Docker..." -ForegroundColor Cyan

# Navegando para o diretório do docker-compose
Set-Location -Path (Split-Path -Parent $PSScriptRoot)

# Verificando se o Docker está em execução
try {
    docker info > $null
}
catch {
    Write-Host "Erro: Docker não está em execução." -ForegroundColor Red
    exit 1
}

# Parando o container
docker-compose down postgres

Write-Host "PostgreSQL no Docker foi parado!" -ForegroundColor Green 