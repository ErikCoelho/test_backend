$ErrorActionPreference = "Stop"

Write-Host "Iniciando RabbitMQ para DigiPay Transaction..." -ForegroundColor Cyan

# Verificando se o Docker está em execução
try {
    docker info > $null
}
catch {
    Write-Host "Erro: Docker não está em execução. Por favor, inicie o Docker Desktop e tente novamente." -ForegroundColor Red
    exit 1
}

# Verificando se o contêiner já existe
$containerName = "digipay-transaction-rabbitmq"
$containerExists = docker ps -a --filter "name=$containerName" --format "{{.Names}}"

if ($containerExists) {
    # Verificando se o contêiner está em execução
    $containerRunning = docker ps --filter "name=$containerName" --format "{{.Names}}"
    
    if ($containerRunning) {
        Write-Host "O contêiner RabbitMQ já está em execução." -ForegroundColor Green
    }
    else {
        Write-Host "Iniciando contêiner RabbitMQ existente..." -ForegroundColor Yellow
        docker start $containerName
    }
}
else {
    Write-Host "Criando e iniciando novo contêiner RabbitMQ..." -ForegroundColor Yellow
    docker run -d `
        --name $containerName `
        -e RABBITMQ_DEFAULT_USER=guest `
        -e RABBITMQ_DEFAULT_PASS=guest `
        -p 5673:5672 `
        -p 15673:15672 `
        --network digipay-network `
        rabbitmq:3-management
}

# Aguardar o RabbitMQ ficar pronto
Write-Host "Aguardando RabbitMQ inicializar..." -ForegroundColor Yellow
$attempts = 0
$maxAttempts = 30
while ($attempts -lt $maxAttempts) {
    try {
        $status = docker exec $containerName rabbitmqctl status
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

Write-Host "RabbitMQ está em execução em:" -ForegroundColor Cyan
Write-Host "  AMQP: localhost:5673" -ForegroundColor Cyan
Write-Host "  Management UI: http://localhost:15673" -ForegroundColor Cyan
Write-Host "  Usuário: guest" -ForegroundColor Cyan
Write-Host "  Senha: guest" -ForegroundColor Cyan 