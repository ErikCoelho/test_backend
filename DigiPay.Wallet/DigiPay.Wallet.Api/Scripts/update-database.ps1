$ErrorActionPreference = "Stop"

Write-Host "Applying migrations to database..." -ForegroundColor Cyan

# Navegar até o diretório do projeto
$projectDir = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $projectDir

dotnet ef database update

Write-Host "Database updated successfully!" -ForegroundColor Green 