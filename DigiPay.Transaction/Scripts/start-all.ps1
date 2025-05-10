$ErrorActionPreference = "Stop"

Write-Host "Iniciando todos os serviços para DigiPay Transaction..." -ForegroundColor Cyan

# Obtendo o diretório do script atual
$scriptDir = $PSScriptRoot

# Iniciando PostgreSQL
Write-Host "Iniciando PostgreSQL..." -ForegroundColor Yellow
& "$scriptDir\start-postgres.ps1"

# Iniciando RabbitMQ
Write-Host "Iniciando RabbitMQ..." -ForegroundColor Yellow
& "$scriptDir\start-rabbitmq.ps1"

Write-Host "Todos os serviços necessários para DigiPay Transaction estão em execução!" -ForegroundColor Green
Write-Host "Você pode iniciar a aplicação agora." -ForegroundColor Cyan 