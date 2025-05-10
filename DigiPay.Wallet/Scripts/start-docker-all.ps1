$ErrorActionPreference = "Stop"

Write-Host "Iniciando todo o ambiente DigiPay Wallet no Docker..." -ForegroundColor Cyan

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

# Parar containers existentes
Write-Host "Parando containers existentes..." -ForegroundColor Yellow
docker-compose down

# Iniciando os containers em background
Write-Host "Iniciando os containers em background..." -ForegroundColor Cyan
docker-compose up -d

# Aguardar o PostgreSQL ficar pronto
Write-Host "Aguardando PostgreSQL inicializar..." -ForegroundColor Yellow
$attempts = 0
$maxAttempts = 30
while ($attempts -lt $maxAttempts) {
    try {
        docker exec digipay-wallet-postgres pg_isready > $null
        if ($LASTEXITCODE -eq 0) {
            Write-Host "PostgreSQL está pronto!" -ForegroundColor Green
            break
        }
    }
    catch {
        # Ignorar erros e continuar tentando
    }
    
    $attempts++
    Write-Host "Aguardando PostgreSQL iniciar... ($attempts/$maxAttempts)" -ForegroundColor Yellow
    Start-Sleep -Seconds 1
}

# Mostrar logs
Write-Host "Ambiente DigiPay Wallet está em execução!" -ForegroundColor Green
Write-Host "PostgreSQL: localhost:5433" -ForegroundColor Cyan
Write-Host "API: http://localhost:5001" -ForegroundColor Cyan
Write-Host "As migrações serão aplicadas automaticamente pela aplicação" -ForegroundColor Green
Write-Host "Para ver os logs, execute: docker-compose logs -f" -ForegroundColor Yellow
Write-Host "Para parar o ambiente, execute: .\Scripts\stop-docker-all.ps1" -ForegroundColor Yellow

# Retorne ao diretório original
Set-Location -Path (Split-Path -Parent $PSScriptRoot) 