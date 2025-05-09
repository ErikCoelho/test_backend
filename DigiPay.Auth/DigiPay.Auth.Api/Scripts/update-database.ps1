$ErrorActionPreference = "Stop"

Write-Host "Applying migrations to database..." -ForegroundColor Cyan

dotnet ef database update --project DigiPay.Auth.Api.csproj

Write-Host "Database updated successfully!" -ForegroundColor Green 