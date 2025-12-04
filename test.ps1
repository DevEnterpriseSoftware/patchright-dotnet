#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test workflow for Patchright .NET
.DESCRIPTION
    This script builds Patchright .NET, applies test modifications, and runs tests.
.EXAMPLE
    .\test.ps1
    .\test.ps1 -Cleanup
#>

param(
    [switch]$Cleanup,
    [string]$DriverVersion
)

$ErrorActionPreference = "Stop"

# Define replacement dictionary
# Key: string to find, Value: string to replace with
$replacements = @{
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup 2")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup 2")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work in immediately closed popup")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in immediately closed popup")]'
    ' [PlaywrightTest("browsercontext-service-worker-policy.spec.ts", "blocks service worker registration")]' = ' //[PlaywrightTest("browsercontext-service-worker-policy.spec.ts", "blocks service worker registration")]'
    ' [PlaywrightTest("browsercontext-viewport-mobile.spec.ts", "should fire orientationchange event")]' = ' //[PlaywrightTest("browsercontext-viewport-mobile.spec.ts", "should fire orientationchange event")]'
    ' [PlaywrightTest("geolocation.spec.ts", "watchPosition should be notified")]' = ' //[PlaywrightTest("geolocation.spec.ts", "watchPosition should be notified")]'
    ' [PlaywrightTest("page-click.spec.ts", "should click offscreen buttons")] ' = ' //[PlaywrightTest("page-click.spec.ts", "should click offscreen buttons")] '
    ' [PlaywrightTest("page-event-console.spec.ts", "consoleMessages should work")]' = ' //[PlaywrightTest("page-event-console.spec.ts", "consoleMessages should work")]'
    ' [PlaywrightTest("page-event-console.spec.ts", "should emit same log twice")]' = ' //[PlaywrightTest("page-event-console.spec.ts", "should emit same log twice")]'
    ' [PlaywrightTest("page-event-console.spec.ts", "should have location for console API calls")]' = ' //[PlaywrightTest("page-event-console.spec.ts", "should have location for console API calls")]'
    ' [PlaywrightTest("page-event-console.spec.ts", "should not fail for window object")]' = ' //[PlaywrightTest("page-event-console.spec.ts", "should not fail for window object")]'
    ' [PlaywrightTest("page-event-console.spec.ts", "should work")]' = ' //[PlaywrightTest("page-event-console.spec.ts", "should work")]'
    ' [PlaywrightTest("page-event-console.spec.ts", "should work for different console API calls")]' = ' //[PlaywrightTest("page-event-console.spec.ts", "should work for different console API calls")]'
    ' [PlaywrightTest("page-event-pageerror.spec.ts", "should fire")]' = ' //[PlaywrightTest("page-event-pageerror.spec.ts", "should fire")]'
    ' [PlaywrightTest("page-event-pageerror.spec.ts", "should handle odd values")]' = ' //[PlaywrightTest("page-event-pageerror.spec.ts", "should handle odd values")]'
    ' [PlaywrightTest("page-event-pageerror.spec.ts", "should handle object")]' = ' //[PlaywrightTest("page-event-pageerror.spec.ts", "should handle object")]'
    ' [PlaywrightTest("page-event-pageerror.spec.ts", "should handle window")]' = ' //[PlaywrightTest("page-event-pageerror.spec.ts", "should handle window")]'
    ' [PlaywrightTest("page-event-pageerror.spec.ts", "pageErrors should work")]' = ' //[PlaywrightTest("page-event-pageerror.spec.ts", "pageErrors should work")]'
    ' [PlaywrightTest("workers.spec.ts", "should have JSHandles for console logs")]' = ' //[PlaywrightTest("workers.spec.ts", "should have JSHandles for console logs")]'
    ' [PlaywrightTest("workers.spec.ts", "should report console logs")]' = ' //[PlaywrightTest("workers.spec.ts", "should report console logs")]'
    ' [PlaywrightTest("workers.spec.ts", "should report errors")]' = ' //[PlaywrightTest("workers.spec.ts", "should report errors")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "weberror event should work")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "weberror event should work")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should pattern match")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should pattern match")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work with baseURL")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work with baseURL")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work without server")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work without server")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work with server")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work with server")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work with ws.close")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work with ws.close")]'
    ' [PlaywrightTest("page-network-request.spec.ts", "should report raw headers")]' = ' //[PlaywrightTest("page-network-request.spec.ts", "should report raw headers")]'
    ' [PlaywrightTest("page-expose-function.spec.ts", "should be callable from-inside addInitScript")]' = ' //[PlaywrightTest("page-expose-function.spec.ts", "should be callable from-inside addInitScript")]'
    ' [PlaywrightTest("popup.spec.ts", "BrowserContext.addInitScript should apply to an in-process popup")]' = ' //[PlaywrightTest("popup.spec.ts", "BrowserContext.addInitScript should apply to an in-process popup")]'
    ' [PlaywrightTest("page-add-script-tag.spec.ts", "should include sourceURL when path is provided")]' = ' //[PlaywrightTest("page-add-script-tag.spec.ts", "should include sourceURL when path is provided")]'
    ' [PlaywrightTest("page-event-pageerror.spec.ts", "should contain sourceURL")]' = ' //[PlaywrightTest("page-event-pageerror.spec.ts", "should contain sourceURL")]'
    'window.open(''about:blank'')' = 'window.open(''https://www.google.com/blank.html'')'
    'GotoAsync("about:blank")' = 'GotoAsync("https://www.google.com/blank.html")'
    'GotoAsync("data:text/html,")' = 'GotoAsync("https://www.google.com/blank.html")'
}

Write-Host "`nPatchright .NET Test Script" -ForegroundColor Yellow
Write-Host "===========================`n" -ForegroundColor Yellow

# Step 1: Run build script with cleanup
Write-Host "Step 1: Running build script with cleanup..." -ForegroundColor Cyan
if ($Cleanup) {
    $buildArgs = @{
        Cleanup = $true
    }
    if ($DriverVersion) {
        $buildArgs.DriverVersion = $DriverVersion
        Write-Host "Using driver version: $DriverVersion" -ForegroundColor Cyan
    }
    & .\build.ps1 @buildArgs
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Build script failed"
        exit 1
    }
    Write-Host "✓ Build completed" -ForegroundColor Green
}
else {
  Write-Host "✓ Cleanup skipped" -ForegroundColor Green
}

# Step 2: Apply replacements to test files
Write-Host "`nStep 2: Applying test file modifications..." -ForegroundColor Cyan
$testDir = ".\playwright-dotnet\src\Playwright.Tests"
$csFiles = Get-ChildItem -Path $testDir -Filter "*.cs" -Recurse

$totalReplacements = 0
foreach ($file in $csFiles) {
    $content = Get-Content -Path $file.FullName -Raw -Encoding UTF8
    $originalContent = $content
    $fileReplacements = 0
    
    foreach ($find in $replacements.Keys) {
        $replace = $replacements[$find]
        if ($content -match [regex]::Escape($find)) {
            $content = $content -replace [regex]::Escape($find), $replace
            $fileReplacements++
        }
    }
    
    if ($content -ne $originalContent) {
        Set-Content -Path $file.FullName -Value $content -Encoding UTF8 -NoNewline
        Write-Host "  Modified: $($file.Name) ($fileReplacements replacements)" -ForegroundColor Gray
        $totalReplacements += $fileReplacements
    }
}

Write-Host "✓ Applied $totalReplacements replacements across $($csFiles.Count) files" -ForegroundColor Green

# Step 3: Install Chrome browser with dependencies
Write-Host "`nStep 3: Installing Chrome browser with dependencies..." -ForegroundColor Cyan
Push-Location ".\playwright-dotnet"
try {
    & .\src\Playwright\bin\Release\netstandard2.0\playwright.ps1 install --with-deps chrome
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install Chrome"
        exit 1
    }
    Write-Host "✓ Chrome installed" -ForegroundColor Green
}
finally {
    Pop-Location
}

# Step 4: Run tests
Write-Host "`nStep 4: Running tests..." -ForegroundColor Cyan
Push-Location ".\playwright-dotnet"
try {
    dotnet test -f net8.0 .\src\Playwright.Tests\Playwright.Tests.csproj --logger:"console;verbosity=normal"
    $testExitCode = $LASTEXITCODE
    
    if ($testExitCode -eq 0) {
        Write-Host "✓ All tests passed" -ForegroundColor Green
    }
    else {
        Write-Host "⚠ Tests completed with failures (exit code: $testExitCode)" -ForegroundColor Yellow
    }
}
finally {
    Pop-Location
}

Write-Host "`n===========================`n" -ForegroundColor Yellow
Write-Host "Test workflow completed!" -ForegroundColor Green
Write-Host ""

exit $testExitCode
