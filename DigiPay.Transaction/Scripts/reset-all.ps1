$ErrorActionPreference = "Stop"

Write-Host "AVISO: Este script irá remover todos os contêineres e volumes do DigiPay Transaction!" -ForegroundColor Red
Write-Host "Todos os dados serão perdidos. Esta operação não pode ser desfeita." -ForegroundColor Red
Write-Host ""
$confirmation = Read-Host "Tem certeza que deseja continuar? (s/n)"

if ($confirmation -ne "s") {
    Write-Host "Operação cancelada." -ForegroundColor Yellow
    exit 0
}

# Verificando se o Docker está em execução
try {
    docker info > $null
}
catch {
    Write-Host "Erro: Docker não está em execução." -ForegroundColor Red
    exit 1
}

# Parando e removendo contêineres
$containers = @("digipay-transaction-postgres", "digipay-transaction-rabbitmq", "digipay-transaction-api")

foreach ($container in $containers) {
    $containerExists = docker ps -a --filter "name=$container" --format "{{.Names}}"
    if ($containerExists) {
        Write-Host "Removendo contêiner $container..." -ForegroundColor Yellow
        docker rm -f $container
    }
}

# Removendo volumes
$volumes = @("digipay-transaction-postgres-data")

foreach ($volume in $volumes) {
    $volumeExists = docker volume ls --filter "name=$volume" --format "{{.Name}}"
    if ($volumeExists) {
        Write-Host "Removendo volume $volume..." -ForegroundColor Yellow
        docker volume rm $volume
    }
}

Write-Host "Todos os recursos Docker do DigiPay Transaction foram removidos com sucesso!" -ForegroundColor Green
Write-Host "Para iniciar um ambiente limpo, execute: .\Scripts\start-docker-all.ps1" -ForegroundColor Cyan 