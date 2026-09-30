# PowerShell script to run tests and generate ReportGenerator HTML coverage report
$ErrorActionPreference = "Stop"

Write-Host "🧹 Cleaning previous test results..." -ForegroundColor Yellow
if (Test-Path "tests\ECart.Tests\TestResults") { Remove-Item -Recurse -Force "tests\ECart.Tests\TestResults" }

Write-Host "🧪 Running unit tests and collecting code coverage..." -ForegroundColor Cyan
dotnet test --collect:"XPlat Code Coverage"

Write-Host "📊 Generating HTML Code Coverage Report..." -ForegroundColor Cyan
reportgenerator `
  -reports:"**/coverage.cobertura.xml" `
  -targetdir:"coverage-report" `
  -reporttypes:Html `
  -classfilters:"+ECart.Api.Controllers.*;+ECart.Application.Services.*;+ECart.Infrastructure.Repositories.*"

Write-Host "✅ Report generated successfully at: coverage-report\index.html" -ForegroundColor Green
Start-Process "coverage-report\index.html"
