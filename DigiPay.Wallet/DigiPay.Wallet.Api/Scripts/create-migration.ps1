param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"

Write-Host "Creating migration: $MigrationName" -ForegroundColor Cyan

# Navegar até o diretório do projeto
$projectDir = (Get-Item $PSScriptRoot).Parent.FullName
Set-Location $projectDir

dotnet ef migrations add $MigrationName

Write-Host "Migration created successfully!" -ForegroundColor Green 