$ErrorActionPreference = "Stop"

Write-Host "Iniciando PostgreSQL para DigiPay Transaction..." -ForegroundColor Cyan

# Verificando se o Docker está em execução
try {
    docker info > $null
}
catch {
    Write-Host "Erro: Docker não está em execução. Por favor, inicie o Docker Desktop e tente novamente." -ForegroundColor Red
    exit 1
}

# Verificando se o contêiner já existe
$containerName = "digipay-transaction-postgres"
$containerExists = docker ps -a --filter "name=$containerName" --format "{{.Names}}"

if ($containerExists) {
    # Verificando se o contêiner está em execução
    $containerRunning = docker ps --filter "name=$containerName" --format "{{.Names}}"
    
    if ($containerRunning) {
        Write-Host "O contêiner PostgreSQL já está em execução." -ForegroundColor Green
    }
    else {
        Write-Host "Iniciando contêiner PostgreSQL existente..." -ForegroundColor Yellow
        docker start $containerName
    }
}
else {
    Write-Host "Criando e iniciando novo contêiner PostgreSQL..." -ForegroundColor Yellow
    docker run -d `
        --name $containerName `
        -e POSTGRES_DB=digipay_transaction `
        -e POSTGRES_USER=postgres `
        -e POSTGRES_PASSWORD=postgres `
        -p 5434:5432 `
        -v digipay-transaction-postgres-data:/var/lib/postgresql/data `
        postgres:15
}

# Aguardar o PostgreSQL ficar pronto
Write-Host "Aguardando PostgreSQL inicializar..." -ForegroundColor Yellow
$attempts = 0
$maxAttempts = 30
while ($attempts -lt $maxAttempts) {
    try {
        $isReady = docker exec $containerName pg_isready -U postgres
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

Write-Host "PostgreSQL está em execução em localhost:5434" -ForegroundColor Cyan
Write-Host "Banco de dados: digipay_transaction" -ForegroundColor Cyan
Write-Host "Usuário: postgres" -ForegroundColor Cyan
Write-Host "Senha: postgres" -ForegroundColor Cyan 