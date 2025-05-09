param(
    [Parameter(Mandatory=$true)]
    [string]$MigrationName
)

$ErrorActionPreference = "Stop"

Write-Host "Creating migration: $MigrationName" -ForegroundColor Cyan

dotnet ef migrations add $MigrationName --project DigiPay.Auth.Api.csproj

Write-Host "Migration created successfully!" -ForegroundColor Green 