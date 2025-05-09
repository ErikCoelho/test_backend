$ErrorActionPreference = "Stop"

Write-Host "Iniciando ambiente completo DigiPay Auth..." -ForegroundColor Cyan

# Iniciar PostgreSQL no Docker
$postgresScript = Join-Path -Path $PSScriptRoot -ChildPath "start-postgres.ps1"
& $postgresScript

if ($LASTEXITCODE -ne 0) {
    Write-Host "Falha ao iniciar PostgreSQL. Abortando." -ForegroundColor Red
    exit 1
}

# Aplicar migrações ao banco de dados
Write-Host "Aplicando migrações ao banco de dados..." -ForegroundColor Cyan
$updateDbScript = Join-Path -Path $PSScriptRoot -ChildPath "..\DigiPay.Auth.Api\Scripts\update-database.ps1"
& $updateDbScript

if ($LASTEXITCODE -ne 0) {
    Write-Host "Falha ao aplicar migrações. Abortando." -ForegroundColor Red
    exit 1
}

# Iniciar a API
Write-Host "Iniciando a API DigiPay Auth..." -ForegroundColor Cyan
$apiScript = Join-Path -Path $PSScriptRoot -ChildPath "..\DigiPay.Auth.Api\Scripts\run-api.ps1"
& $apiScript

Write-Host "Ambiente DigiPay Auth finalizado." -ForegroundColor Yellow 