$ErrorActionPreference = "Stop"

Write-Host "Parando RabbitMQ para DigiPay Transaction..." -ForegroundColor Cyan

# Verificando se o Docker está em execução
try {
    docker info > $null
}
catch {
    Write-Host "Erro: Docker não está em execução." -ForegroundColor Red
    exit 1
}

# Verificando se o contêiner existe e está em execução
$containerName = "digipay-transaction-rabbitmq"
$containerRunning = docker ps --filter "name=$containerName" --format "{{.Names}}"

if ($containerRunning) {
    Write-Host "Parando contêiner RabbitMQ..." -ForegroundColor Yellow
    docker stop $containerName
    Write-Host "RabbitMQ parado com sucesso." -ForegroundColor Green
}
else {
    Write-Host "O contêiner RabbitMQ não está em execução." -ForegroundColor Yellow
} 