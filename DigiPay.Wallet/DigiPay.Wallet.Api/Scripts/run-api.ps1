$ErrorActionPreference = "Stop"

Write-Host "Starting DigiPay Wallet API..." -ForegroundColor Cyan

dotnet run --project DigiPay.Wallet.Api.csproj

Write-Host "API stopped." -ForegroundColor Yellow 