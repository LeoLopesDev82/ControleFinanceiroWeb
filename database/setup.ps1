<#
.SYNOPSIS
    Creates the ControleFinanceiroWeb Firebird database from the versioned
    SQL scripts.

.DESCRIPTION
    Runs schema.sql to create DATABASE.FDB inside the web project folder and,
    unless -NoSeed is given, loads the fictitious demo data from seed.sql.

    Requires Firebird 3.0 installed and its service running.

.PARAMETER Force
    Overwrite an existing DATABASE.FDB. Without it the script refuses to
    touch a database that is already there.

.PARAMETER NoSeed
    Create an empty schema without the demo data.

.EXAMPLE
    .\database\setup.ps1
    Creates the database and loads the demo data.

.EXAMPLE
    .\database\setup.ps1 -Force -NoSeed
    Recreates an empty database, discarding the current one.
#>

[CmdletBinding()]
param(
    [switch]$Force,
    [switch]$NoSeed
)

$ErrorActionPreference = 'Stop'

$scriptRoot = $PSScriptRoot
$projectDir = Join-Path (Split-Path $scriptRoot -Parent) 'ControleFinanceiroWeb'
$databasePath = Join-Path $projectDir 'DATABASE.FDB'

# Locate isql.exe: PATH first, then the default Firebird install folders.
$isql = (Get-Command isql.exe -ErrorAction SilentlyContinue).Source

if (-not $isql) {
    $candidates = @(
        "$env:ProgramFiles\Firebird\*\isql.exe"
        "${env:ProgramFiles(x86)}\Firebird\*\isql.exe"
    )
    $isql = Get-ChildItem -Path $candidates -ErrorAction SilentlyContinue |
            Select-Object -First 1 -ExpandProperty FullName
}

if (-not $isql) {
    throw "isql.exe not found. Install Firebird 3.0 or add its bin folder to PATH."
}

Write-Host "Using $isql"

if (Test-Path $databasePath) {
    if (-not $Force) {
        throw "$databasePath already exists. Pass -Force to replace it."
    }

    Write-Host "Removing the existing database."
    Remove-Item $databasePath -Force
}

if (-not (Test-Path $projectDir)) {
    throw "Project folder not found at $projectDir."
}

# Both scripts reference DATABASE.FDB by a relative name, so they must run
# with the project folder as the working directory.
Push-Location $projectDir

try {
    Write-Host "Creating the schema."
    & $isql -q -i (Join-Path $scriptRoot 'schema.sql')

    if ($LASTEXITCODE -ne 0) {
        throw "schema.sql failed with exit code $LASTEXITCODE."
    }

    if (-not $NoSeed) {
        Write-Host "Loading the demo data."
        & $isql -q -ch ISO8859_1 -i (Join-Path $scriptRoot 'seed.sql')

        if ($LASTEXITCODE -ne 0) {
            throw "seed.sql failed with exit code $LASTEXITCODE."
        }
    }
}
finally {
    Pop-Location
}

Write-Host ""
Write-Host "Database ready at $databasePath"
Write-Host "Run the application with: dotnet run --project ControleFinanceiroWeb"
