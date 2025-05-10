$ErrorActionPreference = "Stop"

Write-Host "Starting PostgreSQL in Docker..." -ForegroundColor Cyan

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

# Iniciando o container
docker-compose up -d postgres

Write-Host "Verificando se o PostgreSQL está pronto para conexões..." -ForegroundColor Cyan
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

if ($attempts -ge $maxAttempts) {
    Write-Host "Tempo limite excedido aguardando pelo PostgreSQL." -ForegroundColor Red
    exit 1
}

Write-Host "PostgreSQL está em execução no Docker!" -ForegroundColor Green
Write-Host "Conexão: Host=localhost;Port=5433;Database=digipay_wallet;Username=postgres;Password=postgres" -ForegroundColor Cyan 