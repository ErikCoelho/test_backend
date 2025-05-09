$ErrorActionPreference = "Stop"

Write-Host "Starting DigiPay Auth API..." -ForegroundColor Cyan

dotnet run --project DigiPay.Auth.Api.csproj

Write-Host "API stopped." -ForegroundColor Yellow 