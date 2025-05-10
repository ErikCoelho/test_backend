$ErrorActionPreference = "Stop"

Write-Host "Resetando completamente o ambiente DigiPay Wallet..." -ForegroundColor Cyan

Write-Host "AVISO: Este script irá remover todos os contêineres e volumes do DigiPay Wallet!" -ForegroundColor Red
Write-Host "Todos os dados serão perdidos. Esta operação não pode ser desfeita." -ForegroundColor Red
Write-Host ""
$confirmation = Read-Host "Tem certeza que deseja continuar? (s/n)"

if ($confirmation -ne "s") {
    Write-Host "Operação cancelada." -ForegroundColor Yellow
    exit 0
}

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

# Parar todos os containers
Write-Host "Parando containers..." -ForegroundColor Yellow
docker-compose down

# Remover volume do PostgreSQL
Write-Host "Removendo volume do PostgreSQL..." -ForegroundColor Yellow
docker volume rm digipay-wallet-postgres-data --force

# Verificando se a rede existe
try {
    $networkExists = docker network ls --filter name=digipay-network --format "{{.Name}}" | Select-String -Pattern "digipay-network"
    if (-not $networkExists) {
        Write-Host "Criando rede digipay-network..." -ForegroundColor Yellow
        docker network create digipay-network
    }
}
catch {
    Write-Host "Erro ao verificar/criar a rede Docker: $_" -ForegroundColor Red
    exit 1
}

# Reconstruir as imagens
Write-Host "Reconstruindo imagens..." -ForegroundColor Cyan
docker-compose build --no-cache

# Iniciar containers
Write-Host "Iniciando containers..." -ForegroundColor Cyan
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

# Aguardar RabbitMQ ficar pronto
Write-Host "Aguardando RabbitMQ inicializar..." -ForegroundColor Yellow
$attempts = 0
$maxAttempts = 30
while ($attempts -lt $maxAttempts) {
    try {
        $rabbitStatus = docker exec digipay-rabbitmq rabbitmqctl status
        if ($LASTEXITCODE -eq 0) {
            Write-Host "RabbitMQ está pronto!" -ForegroundColor Green
            break
        }
    }
    catch {
        # Ignorar erros e continuar tentando
    }
    
    $attempts++
    Write-Host "Aguardando RabbitMQ iniciar... ($attempts/$maxAttempts)" -ForegroundColor Yellow
    Start-Sleep -Seconds 1
}

Write-Host "Ambiente DigiPay Wallet foi completamente resetado e está em execução!" -ForegroundColor Green
Write-Host "PostgreSQL: localhost:5433" -ForegroundColor Cyan
Write-Host "RabbitMQ: localhost:5672 (AMQP), localhost:15672 (Management UI)" -ForegroundColor Cyan
Write-Host "API: http://localhost:5001" -ForegroundColor Cyan
Write-Host "As migrações serão aplicadas automaticamente pela aplicação" -ForegroundColor Green

# Retorne ao diretório original
Set-Location -Path (Split-Path -Parent $PSScriptRoot) 