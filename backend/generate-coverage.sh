#!/usr/bin/env bash
set -e

# Ensure dotnet tools directory is in PATH
export PATH="$PATH:$HOME/.dotnet/tools"

echo "🧹 Cleaning previous test results..."
rm -rf tests/ECart.Tests/TestResults

echo "🧪 Running unit tests and collecting code coverage..."
dotnet test --collect:"XPlat Code Coverage"

echo "📊 Generating HTML Code Coverage Report..."
reportgenerator \
  -reports:"**/coverage.cobertura.xml" \
  -targetdir:"coverage-report" \
  -reporttypes:Html \
  -classfilters:"+ECart.Api.Controllers.*;+ECart.Application.Services.*;+ECart.Infrastructure.Repositories.*"

echo "✅ Report generated successfully at: backend/coverage-report/index.html"

# Open report in default browser if on macOS
if [[ "$OSTYPE" == "darwin"* ]]; then
  open coverage-report/index.html
fi
