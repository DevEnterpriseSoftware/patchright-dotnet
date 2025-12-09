#!/usr/bin/env pwsh
<#
.SYNOPSIS
    Test workflow for Patchright .NET
.DESCRIPTION
    This script builds Patchright .NET, applies test modifications, and runs tests.
.EXAMPLE
    .\test.ps1
    .\test.ps1 -DriverVersion 1.57.0
#>

param(
    [string]$DriverVersion
)

$ErrorActionPreference = "Stop"

# Define replacement dictionary (see BUGS.md for details)
# Key: string to find, Value: string to replace with
$replacements = @{
    ' [PlaywrightTest("", "")]' = '//[PlaywrightTest("", "")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup 2")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup 2")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "console event should work in immediately closed popup")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in immediately closed popup")]'
    ' [PlaywrightTest("browsercontext-service-worker-policy.spec.ts", "blocks service worker registration")]' = ' //[PlaywrightTest("browsercontext-service-worker-policy.spec.ts", "blocks service worker registration")]'
    ' [PlaywrightTest("browsercontext-viewport-mobile.spec.ts", "should fire orientationchange event")]' = ' //[PlaywrightTest("browsercontext-viewport-mobile.spec.ts", "should fire orientationchange event")]'
    ' [PlaywrightTest("geolocation.spec.ts", "watchPosition should be notified")]' = ' //[PlaywrightTest("geolocation.spec.ts", "watchPosition should be notified")]'
    ' [PlaywrightTest("page-click.spec.ts", "should click offscreen buttons")]' = ' //[PlaywrightTest("page-click.spec.ts", "should click offscreen buttons")] '
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
    ' [PlaywrightTest("workers.spec.ts", "should report console event on the worker")]' = '//[PlaywrightTest("workers.spec.ts", "should report console event on the worker")]'
    ' [PlaywrightTest("workers.spec.ts", "should report console event on the worker when not listening on page or context")]' = '//[PlaywrightTest("workers.spec.ts", "should report console event on the worker when not listening on page or context")]'
    ' [PlaywrightTest("browsercontext-events.spec.ts", "weberror event should work")]' = ' //[PlaywrightTest("browsercontext-events.spec.ts", "weberror event should work")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should pattern match")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should pattern match")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work with baseURL")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work with baseURL")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work without server")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work without server")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work with server")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work with server")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work with ws.close")]' = ' //[PlaywrightTest("page-route-web-socket.spec.ts", "should work with ws.close")]'
    ' [PlaywrightTest("page-route-web-socket.spec.ts", "should work with no trailing slash")]' = '//[PlaywrightTest("page-route-web-socket.spec.ts", "should work with no trailing slash")]'
    ' [PlaywrightTest("page-network-request.spec.ts", "should report raw headers")]' = ' //[PlaywrightTest("page-network-request.spec.ts", "should report raw headers")]'
    ' [PlaywrightTest("page-expose-function.spec.ts", "should be callable from-inside addInitScript")]' = ' //[PlaywrightTest("page-expose-function.spec.ts", "should be callable from-inside addInitScript")]'
    ' [PlaywrightTest("popup.spec.ts", "BrowserContext.addInitScript should apply to an in-process popup")]' = ' //[PlaywrightTest("popup.spec.ts", "BrowserContext.addInitScript should apply to an in-process popup")]'
    ' [PlaywrightTest("popup.spec.ts", "should expose function from browser context")]' = '//[PlaywrightTest("popup.spec.ts", "should expose function from browser context")]'
    ' [PlaywrightTest("browsercontext-basic.spec.ts", "should disable javascript")]' = '//[PlaywrightTest("browsercontext-basic.spec.ts", "should disable javascript")]'
    ' [PlaywrightTest("page-add-script-tag.spec.ts", "should include sourceURL when path is provided")]' = ' //[PlaywrightTest("page-add-script-tag.spec.ts", "should include sourceURL when path is provided")]'
    ' [PlaywrightTest("page-wait-for-function.spec.tsPageWaitForFunctionTests", "should work when resolved right before execution context disposal")]' = '//[PlaywrightTest("page-wait-for-function.spec.tsPageWaitForFunctionTests", "should work when resolved right before execution context disposal")]'
    ' [PlaywrightTest("page-add-init-script.spec.ts", "should work with CSP")]' = '//[PlaywrightTest("page-add-init-script.spec.ts", "should work with CSP")]'
    ' [PlaywrightTest("page-event-pageerror.spec.ts", "should contain sourceURL")]' = ' //[PlaywrightTest("page-event-pageerror.spec.ts", "should contain sourceURL")]'

# Replacements
    'window.open(''about:blank'')' = 'window.open(''https://www.google.com/blank.html'')'
    'GotoAsync("about:blank")' = 'GotoAsync("https://www.google.com/blank.html")'
    'GotoAsync("data:text/html,")' = 'GotoAsync("https://www.google.com/blank.html")'
    'Assert.AreEqual("about:blank", popup.Url)' = 'Assert.AreEqual("https://www.google.com/blank.html", popup.Url)'
    'Assert.AreEqual("about:blank", await page.EvaluateAsync<string>("window.location.href"));' = 'Assert.AreEqual("https://www.google.com/blank.html", await page.EvaluateAsync<string>("window.location.href"));'
    'var response = await Page.GotoAsync("https://www.google.com/blank.html")' = 'var response = await Page.GotoAsync("about:blank")'
    ' === div' = ' === document.querySelector(''div'')'
    
# Broken Assertions
    ' [PlaywrightTest("playwright-test/playwright.expect.spec.ts", "should support toBeEnabled")]' = '//[PlaywrightTest("playwright-test/playwright.expect.spec.ts", "should support toBeEnabled")]'
    ' [PlaywrightTest("playwright-test/playwright.expect.spec.ts", "should support toBeVisible")]' = '//[PlaywrightTest("playwright-test/playwright.expect.spec.ts", "should support toBeVisible")]'
    ' [PlaywrightTest("page/expect-misc.spec.ts", "strict mode violation error format")]' = '//[PlaywrightTest("page/expect-misc.spec.ts", "strict mode violation error format")]'
    ' [PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > eventually with not")]' = '//[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > eventually with not")]'
    ' [PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail")]' = '//[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail")]'
    ' [PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail with not")]' = '//[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail with not")]'
    ' [PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail with impossible timeout .not")]' = '//[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail with impossible timeout .not")]'
    ' [PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > over navigation")]' = '//[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > over navigation")]'
    ' [PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > with not")]' = '//[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > with not")]'
    ' [PlaywrightTest("locator-frame.spec.ts", "should not wait for frame")]' = '//[PlaywrightTest("locator-frame.spec.ts", "should not wait for frame")]'
    ' [PlaywrightTest("locator-frame.spec.ts", "should not wait for frame 2")]' = '//[PlaywrightTest("locator-frame.spec.ts", "should not wait for frame 2")]'
    ' [PlaywrightTest("locator-frame.spec.ts", "should not wait for frame 3")]' = '//[PlaywrightTest("locator-frame.spec.ts", "should not wait for frame 3")]'
    ' [PlaywrightTest("locator-frame.spec.ts", "should wait for frame to go")]' = '//[PlaywrightTest("locator-frame.spec.ts", "should wait for frame to go")]'
    ' [PlaywrightTest("locator-query.spec.ts", "should filter by regex with a single quote")]' = '//[PlaywrightTest("locator-query.spec.ts", "should filter by regex with a single quote")]'
    
# Inconsistent tests
    ' [PlaywrightTest("resource-timing.spec.ts", "should work when serving from memory cache")]' = '//[PlaywrightTest("resource-timing.spec.ts", "should work when serving from memory cache")]'
    ' [PlaywrightTest("tracing.spec.ts", "should collect trace with resources, but no js")]' = '//[PlaywrightTest("tracing.spec.ts", "should collect trace with resources, but no js")]'
    ' [PlaywrightTest("tracing.spec.ts", "should respect tracesDir and name")]' = '//[PlaywrightTest("tracing.spec.ts", "should respect tracesDir and name")]'
    ' [PlaywrightTest("popup.spec.ts", "should inherit viewport size from browser context")]' = '//[PlaywrightTest("popup.spec.ts", "should inherit viewport size from browser context")]'
    ' [PlaywrightTest("page-wait-for-selector-1.spec.ts", "should report logs while waiting for visible")]' = '//[PlaywrightTest("page-wait-for-selector-1.spec.ts", "should report logs while waiting for visible")]'
    ' [PlaywrightTest("page-wait-for-selector-1.spec.ts", "should report logs while waiting for hidden")]' = '//[PlaywrightTest("page-wait-for-selector-1.spec.ts", "should report logs while waiting for hidden")]'
    ' [PlaywrightTest("page-wait-for-selector-1.spec.ts", "elementHandle.waitForSelector should throw on navigation")]' = '//[PlaywrightTest("page-wait-for-selector-1.spec.ts", "elementHandle.waitForSelector should throw on navigation")]'

# Expose Functions (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/144)
    ' [PlaywrightTest("browsercontext-expose-function.spec.ts", "should be callable from-inside addInitScript")]' = '//[PlaywrightTest("browsercontext-expose-function.spec.ts", "should be callable from-inside addInitScript")]'
    ' [PlaywrightTest("browsercontext-expose-function.spec.ts", "exposeBindingHandle should work")]' = '//[PlaywrightTest("browsercontext-expose-function.spec.ts", "exposeBindingHandle should work")]'
    ' [PlaywrightTest("browsercontext-expose-function.spec.ts", "expose binding should work")]' = '//[PlaywrightTest("browsercontext-expose-function.spec.ts", "expose binding should work")]  '
    ' [PlaywrightTest("page-expose-function.spec.ts", "exposeBindingHandle should work")]' = '//[PlaywrightTest("page-expose-function.spec.ts", "exposeBindingHandle should work")]'
# Atomic Checks (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/145)
    ' [PlaywrightTest("elementhandle-convenience.spec.ts", "innerHTML should be atomic")]' = '//[PlaywrightTest("elementhandle-convenience.spec.ts", "innerHTML should be atomic")]'
    ' [PlaywrightTest("elementhandle-convenience.spec.ts", "innerText should be atomic")]' = '//[PlaywrightTest("elementhandle-convenience.spec.ts", "innerText should be atomic")]'
    ' [PlaywrightTest("elementhandle-convenience.spec.ts", "getAttribute should be atomic")]' = '//[PlaywrightTest("elementhandle-convenience.spec.ts", "getAttribute should be atomic")]'
    ' [PlaywrightTest("selector-register.spec.ts", "textContent should be atomic")]' = '//[PlaywrightTest("selector-register.spec.ts", "textContent should be atomic")]'
}

Write-Host "`nPatchright .NET Test Script" -ForegroundColor Yellow
Write-Host "===========================`n" -ForegroundColor Yellow

# Step 1: Run build script with cleanup
Write-Host "Step 1: Running build script..." -ForegroundColor Cyan

if ($DriverVersion) {
    Write-Host "Using driver version: $DriverVersion" -ForegroundColor Cyan
    & .\build.ps1 -Cleanup -DriverVersion $DriverVersion -IsolatedContextDefault $false
}
else {
    & .\build.ps1 -Cleanup -IsolatedContextDefault $false
}
if ($LASTEXITCODE -ne 0) {
    Write-Error "Build script failed"
    exit 1
}    
Write-Host "✓ Build completed" -ForegroundColor Green

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

# Step 3: Install Playwright browsers
Write-Host "`nStep 3: Installing Playwright browsers..." -ForegroundColor Cyan
Push-Location ".\playwright-dotnet"
try {
    & .\src\Playwright\bin\Release\netstandard2.0\playwright.ps1 install
    if ($LASTEXITCODE -ne 0) {
        Write-Error "Failed to install Playwright browsers"
        exit 1
    }
    Write-Host "✓ Playwright browsers installed" -ForegroundColor Green
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
