param(
    [string]$CsvPath = ".\SPI mockados.csv",
    [string]$BackendPath = ".\backend-dotnet",
    [string]$Environment = "Development"
)

$ErrorActionPreference = "Stop"

$root = Resolve-Path (Join-Path $PSScriptRoot "..")
$csvFullPath = Resolve-Path (Join-Path $root $CsvPath)
$backendFullPath = Resolve-Path (Join-Path $root $BackendPath)
$projectPath = Join-Path $backendFullPath "tools\SPI.DemoDataSeeder\SPI.DemoDataSeeder.csproj"
$apiPath = Join-Path $backendFullPath "src\SPI.API"

dotnet run --project $projectPath -- `
    --csv "$csvFullPath" `
    --api "$apiPath" `
    --environment "$Environment"
