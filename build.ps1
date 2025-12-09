#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Build and run workflow for Patchright .NET
.DESCRIPTION
    This script clones playwright-dotnet, patches it with Patchright, and builds the project.
.PARAMETER Cleanup
    Remove existing playwright-dotnet directory before starting
.PARAMETER DriverVersion
    Specific driver version to use
.PARAMETER PackageVersion
    Target package version to build (overrides Version.props)
.PARAMETER IsolatedContextDefault
    The default isolated context behavior to use when running the patch.
.EXAMPLE
    .\build.ps1
    .\build.ps1 -Cleanup
    .\build.ps1 -Cleanup -DriverVersion 1.57.0 -PackageVersion 1.57.0
    .\build.ps1 -Cleanup -DriverVersion 1.57.0 -PackageVersion 1.57.0 -IsolatedContextDefault $false
#>

param(
    [switch]$Cleanup,
    [string]$DriverVersion,
    [string]$PackageVersion,
    [bool]$IsolatedContextDefault = $true  # Default to true, can be overridden for unit tests
)

$ErrorActionPreference = "Stop"

# Check prerequisites
function Test-Prerequisites {
    Write-Host "Checking prerequisites..." -ForegroundColor Cyan
    
    # Check git
    if (-not (Get-Command git -ErrorAction SilentlyContinue)) {
        Write-Error "git is not installed or not in PATH. Please install git first."
        exit 1
    }
    
    # Check dotnet
    if (-not (Get-Command dotnet -ErrorAction SilentlyContinue)) {
        Write-Error ".NET SDK is not installed or not in PATH. Please install .NET 10 SDK first."
        exit 1
    }
    
    # Check .NET version
    $dotnetVersion = dotnet --version
    $majorVersion = [int]($dotnetVersion -split '\.')[0]
    
    if ($majorVersion -lt 10) {
        Write-Error ".NET SDK version $dotnetVersion found, but .NET 10 SDK or higher is required."
        exit 1
    }
    
    Write-Host "✓ git: $(git --version)" -ForegroundColor Green
    Write-Host "✓ .NET SDK: $dotnetVersion" -ForegroundColor Green
}

# Get latest release version from GitHub
function Get-LatestPlaywrightVersion {
    Write-Host "`nFetching latest Playwright .NET release version..." -ForegroundColor Cyan
    
    try {
        $apiUrl = "https://api.github.com/repos/microsoft/playwright-dotnet/releases/latest"
        $response = Invoke-RestMethod -Uri $apiUrl -Headers @{ "User-Agent" = "Patchright-Build-Script" }
        $tagName = $response.tag_name
        
        # Remove 'v' prefix if present and strip build number (keep only major.minor)
        $version = $tagName -replace '^v', ''
        $versionParts = $version -split '\.'
        $majorMinor = "$($versionParts[0]).$($versionParts[1])"
        
        Write-Host "✓ Latest version: $tagName (using release-$majorMinor)" -ForegroundColor Green
        return $majorMinor
    }
    catch {
        Write-Error "Failed to fetch latest release version from GitHub: $_"
        exit 1
    }
}

# Main workflow
Write-Host "`nPatchright .NET Build Script" -ForegroundColor Yellow
Write-Host "============================`n" -ForegroundColor Yellow

Test-Prerequisites

$version = Get-LatestPlaywrightVersion
$playwrightDir = "playwright-dotnet"

# Cleanup if requested
if ($Cleanup -and (Test-Path $playwrightDir)) {
    Write-Host "`nCleaning up existing playwright-dotnet directory..." -ForegroundColor Cyan
    Remove-Item -Path $playwrightDir -Recurse -Force
    Write-Host "✓ Cleanup complete" -ForegroundColor Green
}

# Clone repository
if (-not (Test-Path $playwrightDir)) {
    Write-Host "`nCloning playwright-dotnet repository..." -ForegroundColor Cyan
    git clone https://github.com/microsoft/playwright-dotnet.git
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to clone repository"
        exit 1
    }
    Write-Host "✓ Repository cloned" -ForegroundColor Green
}
else {
    Write-Host "`nPlaywright-dotnet directory already exists, skipping clone" -ForegroundColor Yellow
}

# Checkout release branch
Write-Host "`nChecking out release-$version..." -ForegroundColor Cyan
Push-Location $playwrightDir
try {
    git checkout "release-$version"
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to checkout release-$version"
        exit 1
    }
    Write-Host "✓ Checked out release-$version" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Run Patchright
Write-Host "`nRunning Patchright..." -ForegroundColor Cyan
dotnet run .\Patchright.cs $IsolatedContextDefault $DriverVersion $PackageVersion

if ($LASTEXITCODE -ne 0) {
    Write-Error "Failed to run Patchright"
    exit 1
}
Write-Host "✓ Patchright executed successfully" -ForegroundColor Green

# Download drivers
Write-Host "`nDownloading Playwright drivers..." -ForegroundColor Cyan
Push-Location $playwrightDir
try {
    dotnet run --project ./src/tools/Playwright.Tooling/Playwright.Tooling.csproj -- download-drivers --basepath .
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to download drivers"
        exit 1
    }
    Write-Host "✓ Drivers downloaded" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Build solution
Write-Host "`nBuilding Playwright solution..." -ForegroundColor Cyan
Push-Location $playwrightDir
try {
    dotnet build --configuration Release ./src/Playwright.sln
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to build solution"
        exit 1
    }
    Write-Host "✓ Solution built successfully" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Create patch file
Write-Host "`nCreating patch file..." -ForegroundColor Cyan
Push-Location $playwrightDir
try {
    git diff -- . ":(exclude)README.md" > ../patchright.patch
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to create patch file"
        exit 1
    }
    Write-Host "✓ Patch file created successfully" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Pack project
Write-Host "`nPacking Playwright project..." -ForegroundColor Cyan
Push-Location $playwrightDir
try {
    dotnet pack --no-build --configuration Release --output ../nuget ./src/Playwright/Playwright.csproj
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to pack project"
        exit 1
    }
    Write-Host "✓ Project packed successfully" -ForegroundColor Green
}
finally {
    Pop-Location
}

Write-Host "`n============================`n" -ForegroundColor Yellow
Write-Host "Build completed successfully!" -ForegroundColor Green
Write-Host ""
