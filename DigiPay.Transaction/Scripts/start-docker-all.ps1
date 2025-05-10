$ErrorActionPreference = "Stop"

Write-Host "Iniciando todo o ambiente DigiPay Transaction no Docker..." -ForegroundColor Cyan

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
        docker exec digipay-transaction-postgres pg_isready > $null
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
        $rabbitStatus = docker exec digipay-transaction-rabbitmq rabbitmqctl status
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

# Mostrar logs
Write-Host "Ambiente DigiPay Transaction está em execução!" -ForegroundColor Green
Write-Host "PostgreSQL: localhost:5434" -ForegroundColor Cyan
Write-Host "RabbitMQ: localhost:5672 (AMQP), localhost:15672 (Management UI)" -ForegroundColor Cyan
Write-Host "API: http://localhost:5002" -ForegroundColor Cyan
Write-Host "As migrações serão aplicadas automaticamente pela aplicação" -ForegroundColor Green
Write-Host "Para ver os logs, execute: docker-compose logs -f" -ForegroundColor Yellow
Write-Host "Para parar o ambiente, execute: .\Scripts\stop-docker-all.ps1" -ForegroundColor Yellow 