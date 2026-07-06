param(
    [switch]$SkipTests
)

Write-Host "=== Restoring packages ===" -ForegroundColor Cyan
dotnet restore

Write-Host "`n=== Building solution ===" -ForegroundColor Cyan
dotnet build --configuration Release --no-restore

if (-not $SkipTests) {
    Write-Host "`n=== Running tests ===" -ForegroundColor Cyan
    dotnet test --configuration Release --no-build --verbosity normal
}

Write-Host "`n=== Packing NuGet package ===" -ForegroundColor Cyan
dotnet pack src/LogParser/LogParser.csproj `
    --configuration Release `
    --no-build `
    --output nupkg

Write-Host "`n=== Done ===" -ForegroundColor Green
Write-Host "NuGet packages are in ./nupkg/"
