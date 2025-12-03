<h1 align="center">
    🎭 Patchright .NET
</h1>


<p align="center">
    <a href="https://github.com/DevEnterpriseSoftware/patchright-dotnet/blob/main/LICENSE">
        <img src="https://img.shields.io/badge/License-Apache%202.0-green">
    </a>
    <a>
        <img src="https://img.shields.io/badge/Based%20on-Playwright-goldenrod">
    </a>
    <a>
        <img src="https://img.shields.io/badge/Driver-Patched-blue">
    </a>
    <br/>
    <a href="https://github.com/DevEnterpriseSoftware/patchright-dotnet/releases/latest">
        <img alt="Patchright Version" src="https://img.shields.io/github/v/release/microsoft/playwright-dotnet?display_name=release&label=Version">
    </a>
</p>

#### Patchright is a patched and undetected version of the Playwright Testing and Automation Framework for .NET. </br> It can be used as a drop-in replacement for Playwright.

> [!NOTE]  
> This repository serves the Patchright .NET Package. To use Patchright with Python or NodeJS, check out the [Python Package](https://github.com/Kaliiiiiiiiii-Vinyzu/patchright-python) or the [NodeJS Package](https://github.com/Kaliiiiiiiiii-Vinyzu/patchright-nodejs).
> Also check out the main [Patchright Driver Repository](https://github.com/Kaliiiiiiiiii-Vinyzu/patchright)

---

## Install it from NuGet

```bash
# Install Patchright with NuGet
dotnet add package Patchright
```

---

## Usage
#### Instead of using the Microsoft.Playwright package just install the Patchright package instead. Patchright is a drop-in-replacement for Playwright!

> [!WARNING]  
> Patchright only patches CHROMIUM based browsers. Firefox and Webkit are not supported.

```csharp
using Microsoft.Playwright;
using System.Threading.Tasks;

class Program
{
    public static async Task Main()
    {
        using var playwright = await Playwright.CreateAsync();
        await using var browser = await playwright.Chromium.LaunchAsync();
        var page = await browser.NewPageAsync();
        await page.GotoAsync("https://playwright.dev");
        await page.ScreenshotAsync(new() { Path = "screenshot.png" });
    }
}
```

### Best Practice - use Chrome without Fingerprint Injection

To be completely undetected, use the following configuration:
```csharp
await using var context = await playwright.Chromium.LaunchPersistentContextAsync(
    userDataDir: "...",
    new BrowserTypeLaunchPersistentContextOptions 
    {
        Channel = "chrome",
        Headless = false,
        NoViewport = true,
        // do NOT add custom browser headers or userAgent
    });
```

> [!NOTE]  
> We recommend using Google Chrome instead of Chromium.
> You can install it via `./playwright.ps1 install --with-deps chrome` (or via any other installation method) and use it with `Channel = "chrome"`.

---

## Patches

### [Runtime.enable](https://vanilla.aslushnikov.com/?Runtime.enable) Leak
This is the biggest Patch Patchright uses. To avoid detection by this leak, patchright avoids using [Runtime.enable](https://vanilla.aslushnikov.com/?Runtime.enable) by executing Javascript in (isolated) ExecutionContexts.

### [Console.enable](https://vanilla.aslushnikov.com/?Console.enable) Leak
Patchright patches this leak by disabling the Console API all together. This means, console functionality will not work in Patchright. If you really need the console, you might be better off using Javascript loggers, although they also can be easily detected.

### Command Flags Leaks
Patchright tweaks the Playwright Default Args to avoid detection by Command Flag Leaks. This (most importantly) affects:
- `--disable-blink-features=AutomationControlled` (added) to avoid navigator.webdriver detection.
- `--enable-automation` (removed) to avoid navigator.webdriver detection.
- `--disable-popup-blocking` (removed) to avoid popup crashing.
- `--disable-component-update` (removed) to avoid detection as a Stealth Driver.
- `--disable-default-apps` (removed) to enable default apps.
- `--disable-extensions` (removed) to enable extensions

### General Leaks
Patchright patches some general leaks in the Playwright codebase. This mainly includes poor setups and obvious detection points.

### Closed Shadow Roots
Patchright is able to interact with elements in Closed Shadow Roots. Just use normal locators and Patchright will do the rest.
<br/>
Patchright is now also able to use XPaths in Closed Shadow Roots.

---

## Stealth

With the right setup, Patchright currently is considered undetectable.
Patchright passes:
- [Brotector](https://kaliiiiiiiiii.github.io/brotector/) ✅
- [Cloudflare](https://cloudflare.com/) ✅
- [Kasada](https://www.kasada.io/) ✅
- [Akamai](https://www.akamai.com/products/bot-manager/) ✅
- [Shape/F5](https://www.f5.com/) ✅
- [Bet365](https://bet365.com/) ✅
- [Datadome](https://datadome.co/products/bot-protection/) ✅
- [Fingerprint.com](https://fingerprint.com/products/bot-detection/) ✅
- [CreepJS](https://abrahamjuliot.github.io/creepjs/) ✅
- [Sannysoft](https://bot.sannysoft.com/) ✅
- [Incolumitas](https://bot.incolumitas.com/) ✅
- [IPHey](https://iphey.com/) ✅
- [Browserscan](https://browserscan.net/) ✅
- [Pixelscan](https://pixelscan.net/) ✅


## Documentation and API Reference
See the original [Playwright .NET Documentation](https://playwright.dev/dotnet/docs/intro) and [API Reference](https://playwright.dev/dotnet/docs/api/class-playwright)

## Extended Patchright API

#### **`EvaluateAsync`** Method <sub>([`IFrame.EvaluateAsync`](https://playwright.dev/dotnet/docs/api/class-frame#frame-evaluate),  [`IJSHandle.EvaluateAsync`](https://playwright.dev/dotnet/docs/api/class-jshandle#js-handle-evaluate),  [`ILocator.EvaluateAsync`](https://playwright.dev/dotnet/docs/api/class-locator#locator-evaluate),  [`IPage.EvaluateAsync`](https://playwright.dev/dotnet/docs/api/class-page#page-evaluate),  [`IWorker.EvaluateAsync`](https://playwright.dev/dotnet/docs/api/class-worker#worker-evaluate))</sub>
- Added `isolatedContext` to choose Execution Context (Main/Isolated). `bool` (*optional*, Defaults to `true`)
```diff
Task<T> EvaluateAsync<T>(
    string expression, 
    object? arg = default,
+   bool isolatedContext = true);
```

#### **`EvaluateHandleAsync`** Method <sub>([`IFrame.EvaluateHandleAsync`](https://playwright.dev/dotnet/docs/api/class-frame#frame-evaluate-handle),  [`IJSHandle.EvaluateHandleAsync`](https://playwright.dev/dotnet/docs/api/class-jshandle#js-handle-evaluate-handle),  [`ILocator.EvaluateHandleAsync`](https://playwright.dev/dotnet/docs/api/class-locator#locator-evaluate-handle),  [`IPage.EvaluateHandleAsync`](https://playwright.dev/dotnet/docs/api/class-page#page-evaluate-handle),  [`IWorker.EvaluateHandleAsync`](https://playwright.dev/dotnet/docs/api/class-worker#worker-evaluate-handle))</sub>
- Added `isolatedContext` to choose Execution Context (Main/Isolated). `bool` (*optional*, Defaults to `true`)
```diff
Task<IJSHandle> EvaluateHandleAsync(
    string expression, 
    object? arg = default,
+   bool isolatedContext = true);
```

#### **`EvaluateAllAsync`** Method <sub>([`ILocator.EvaluateAllAsync`](https://playwright.dev/dotnet/docs/api/class-locator#locator-evaluate-all))</sub>
- Added `isolatedContext` to choose Execution Context (Main/Isolated). `bool` (*optional*, Defaults to `true`)
```diff
Task<IJSHandle> EvaluateAllAsync(
    string expression, 
    object? arg = default,
+   bool isolatedContext = true);
```

#### **`EvalOnSelectorAsync`** Method <sub>([`IPage.EvalOnSelectorAsync`](https://playwright.dev/dotnet/docs/api/class-page#page-eval-on-selector), [`IFrame.EvalOnSelectorAsync`](https://playwright.dev/dotnet/docs/api/class-frame#frame-eval-on-selector))</sub>
- Added `isolatedContext` to choose Execution Context (Main/Isolated). `bool` (*optional*, Defaults to `true`)
```diff
Task<T> EvalOnSelectorAsync<T>(
    string selector,
    string expression,
    object? arg = default,
    EvalOnSelectorOptions? options = default,
+   bool isolatedContext = true);
```

#### **`EvalOnSelectorAllAsync`** Method <sub>([`IPage.EvalOnSelectorAllAsync`](https://playwright.dev/dotnet/docs/api/class-page#page-eval-on-selector-all), [`IFrame.EvalOnSelectorAllAsync`](https://playwright.dev/dotnet/docs/api/class-frame#frame-eval-on-selector-all))</sub>
- Added `isolatedContext` to choose Execution Context (Main/Isolated). `bool` (*optional*, Defaults to `true`)
```diff
Task<T> EvalOnSelectorAllAsync<T>(
    string selector,
    string expression,
    object? arg = default,
+   bool isolatedContext = true);
```

---

## Bugs
#### The bugs are documented in the [Patchright Driver Repository](https://github.com/Kaliiiiiiiiii-Vinyzu/patchright#bugs).

---

### TODO
#### The TODO is documented in the [Patchright Driver Repository](https://github.com/Kaliiiiiiiiii-Vinyzu/patchright#todo).

---

## Development

Deployment of new Patchright versions are not automatic (yet), and bugs due to Playwright codebase changes may occur. Fixes for these bugs might take a few days to be released manually.

---

## Support our work

If you choose to support our work, please contact [@vinyzu](https://discord.com/users/935224495126487150) or [@steve_abcdef](https://discord.com/users/936292409426477066) on Discord.

---

## Copyright and License
© [Vinyzu](https://github.com/Vinyzu/)

Patchright is licensed [Apache 2.0](https://choosealicense.com/licenses/apache-2.0/)

---

## Disclaimer

This repository is provided for **educational purposes only**. \
No warranties are provided regarding accuracy, completeness, or suitability for any purpose. **Use at your own risk**—the authors and maintainers assume **no liability** for **any damages**, **legal issues**, or **warranty breaches** resulting from use, modification, or distribution of this code.\
**Any misuse or legal violations are the sole responsibility of the user**. 

---

## Authors

#### Active Maintainer: [Werner van Deventer](https://github.com/brutaldev/)
