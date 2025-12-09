# Current Bugs of Patchright

This is the .NET version of the items listed in https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30

The tests below can be ignored and may or may not be fixed one day.

You can perform the following find/replace operations to make a lot of tests pass:
```
window.open('about:blank') -> window.open('https://www.google.com/blank.html')
GotoAsync("about:blank") -> GotoAsync("https://www.google.com/blank.html")
GotoAsync("data:text/html,") -> GotoAsync("https://www.google.com/blank.html")
```

### Console (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("browsercontext-events.spec.ts", "console event should work")]  
[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup")]  
[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in popup 2")]  
[PlaywrightTest("browsercontext-events.spec.ts", "console event should work in immediately closed popup")]  
[PlaywrightTest("browsercontext-service-worker-policy.spec.ts", "blocks service worker registration")]  
[PlaywrightTest("browsercontext-viewport-mobile.spec.ts", "should fire orientationchange event")]  
[PlaywrightTest("geolocation.spec.ts", "watchPosition should be notified")]  
[PlaywrightTest("page-click.spec.ts", "should click offscreen buttons")]  
[PlaywrightTest("page-event-console.spec.ts", "consoleMessages should work")]  
[PlaywrightTest("page-event-console.spec.ts", "should emit same log twice")]  
[PlaywrightTest("page-event-console.spec.ts", "should have location for console API calls")]  
[PlaywrightTest("page-event-console.spec.ts", "should not fail for window object")]  
[PlaywrightTest("page-event-console.spec.ts", "should work")]  
[PlaywrightTest("page-event-console.spec.ts", "should work for different console API calls")]  
[PlaywrightTest("page-event-pageerror.spec.ts", "should fire")]  
[PlaywrightTest("page-event-pageerror.spec.ts", "should handle odd values")]  
[PlaywrightTest("page-event-pageerror.spec.ts", "should handle object")]  
[PlaywrightTest("page-event-pageerror.spec.ts", "should handle window")]  
[PlaywrightTest("page-event-pageerror.spec.ts", "pageErrors should work")]  
[PlaywrightTest("workers.spec.ts", "should have JSHandles for console logs")]  
[PlaywrightTest("workers.spec.ts", "should report console logs")]  
[PlaywrightTest("workers.spec.ts", "should report errors")]  
[PlaywrightTest("workers.spec.ts", "should report console event on the worker")]  
[PlaywrightTest("workers.spec.ts", "should report console event on the worker when not listening on page or context")]  
[PlaywrightTest("browsercontext-events.spec.ts", "weberror event should work")]  

### Websocket Routing (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-route-web-socket.spec.ts", "should pattern match")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with baseURL")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work without server")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with server")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with ws.close")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with no trailing slash")]  

### Mismatching Request Headers (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-network-request.spec.ts", "should report raw headers")]  

### Expose Functions (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/144)
[PlaywrightTest("browsercontext-expose-function.spec.ts", "should be callable from-inside addInitScript")]  
[PlaywrightTest("browsercontext-expose-function.spec.ts", "exposeBindingHandle should work")]  
[PlaywrightTest("browsercontext-expose-function.spec.ts", "expose binding should work")]  
[PlaywrightTest("page-expose-function.spec.ts", "exposeBindingHandle should work")]  
[PlaywrightTest("page-expose-function.spec.ts", "should work with handles and complex objects")]  

### Init Scripts (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-expose-function.spec.ts", "should be callable from-inside addInitScript")]  
[PlaywrightTest("popup.spec.ts", "BrowserContext.addInitScript should apply to an in-process popup")]  
[PlaywrightTest("popup.spec.ts", "should expose function from browser context")]
[PlaywrightTest("browsercontext-basic.spec.ts", "should disable javascript")]  
[PlaywrightTest("page-wait-for-function.spec.tsPageWaitForFunctionTests", "should work when resolved right before execution context disposal")]  
[PlaywrightTest("page-add-init-script.spec.ts", "should work with CSP")] --> Routing stops this from failing.  

### Atomic Checks (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/145)
[PlaywrightTest("elementhandle-convenience.spec.ts", "innerHTML should be atomic")]  
[PlaywrightTest("elementhandle-convenience.spec.ts", "innerText should be atomic")]  
[PlaywrightTest("elementhandle-convenience.spec.ts", "getAttribute should be atomic")]  
[PlaywrightTest("selector-register.spec.ts", "textContent should be atomic")]  

### Add Script Source (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-add-script-tag.spec.ts", "should include sourceURL when path is provided")]  
[PlaywrightTest("page-event-pageerror.spec.ts", "should contain sourceURL")]  

### .NET Playwright Assertion Library
All of these tests actually pass if you directly check the value instead of going through the Expect().ToBe* pattern.

[PlaywrightTest("playwright-test/playwright.expect.spec.ts", "should support toBeEnabled")]  
[PlaywrightTest("playwright-test/playwright.expect.spec.ts", "should support toBeVisible")]  
[PlaywrightTest("page/expect-misc.spec.ts", "strict mode violation error format")]  
[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > eventually with not")]  
[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail")]  
[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail with not")]  
[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > fail with impossible timeout .not")]  
[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > over navigation")]  
[PlaywrightTest("page/expect-boolean.spec.ts", "toBeAttached > with not")]  
[PlaywrightTest("locator-frame.spec.ts", "should not wait for frame")]  
[PlaywrightTest("locator-frame.spec.ts", "should not wait for frame 2")]  
[PlaywrightTest("locator-frame.spec.ts", "should not wait for frame 3")]  
[PlaywrightTest("locator-frame.spec.ts", "should wait for frame to go")]  
[PlaywrightTest("locator-query.spec.ts", "should filter by regex with a single quote")]  

### Inconsistent Tests
[PlaywrightTest("resource-timing.spec.ts", "should work when serving from memory cache")]  
[PlaywrightTest("tracing.spec.ts", "should collect trace with resources, but no js")]  
[PlaywrightTest("tracing.spec.ts", "should respect tracesDir and name")]  
[PlaywrightTest("page-wait-for-selector-1.spec.ts", "should report logs while waiting for visible")]  
[PlaywrightTest("page-wait-for-selector-1.spec.ts", "should report logs while waiting for hidden")]  
