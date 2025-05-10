$ErrorActionPreference = "Stop"

Write-Host "Parando o ambiente DigiPay Transaction no Docker..." -ForegroundColor Cyan

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

# Parando os containers
docker-compose down

Write-Host "Ambiente DigiPay Transaction no Docker foi parado!" -ForegroundColor Green 