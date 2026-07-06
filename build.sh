#!/usr/bin/env bash
set -euo pipefail

echo "=== Restoring packages ==="
dotnet restore

echo ""
echo "=== Building solution ==="
dotnet build --configuration Release --no-restore

echo ""
echo "=== Running tests ==="
dotnet test --configuration Release --no-build --verbosity normal

echo ""
echo "=== Packing NuGet package ==="
dotnet pack src/LogParser/LogParser.csproj \
    --configuration Release \
    --no-build \
    --output nupkg

echo ""
echo "=== Done ==="
echo "NuGet packages are in ./nupkg/"
