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
[PlaywrightTest("browsercontext-events.spec.ts", "weberror event should work")]  

### Websocket Routing (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-route-web-socket.spec.ts", "should pattern match")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with baseURL")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with baseURL")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work without server")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with server")]  
[PlaywrightTest("page-route-web-socket.spec.ts", "should work with ws.close")]  

### Mismatching Request Headers (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-network-request.spec.ts", "should report raw headers")]  

### Selectors (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/148)
[PlaywrightTest("page-wait-for-selector-1.spec.ts", "should throw when frame is detached")]  
[PlaywrightTest("page-wait-for-selector-2.spec.ts", "should throw when frame is detached xpath")]  

### Expose Functions (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/144)
[PlaywrightTest("browsercontext-expose-function.spec.ts", "should be callable from-inside addInitScript")]  
[PlaywrightTest("browsercontext-expose-function.spec.ts", "exposeBindingHandle should work")]  
[PlaywrightTest("page-expose-function.spec.ts", "exposeBindingHandle should work")]  

### Init Scripts (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-expose-function.spec.ts", "should be callable from-inside addInitScript")]  
[PlaywrightTest("popup.spec.ts", "BrowserContext.addInitScript should apply to an in-process popup")]  

### Atomic Checks (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/145)
[PlaywrightTest("elementhandle-convenience.spec.ts", "innerHTML should be atomic")]  
[PlaywrightTest("elementhandle-convenience.spec.ts", "getAttribute should be atomic")]  
[PlaywrightTest("selector-register.spec.ts", "textContent should be atomic")]  

### Add Script Source (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/30)
[PlaywrightTest("page-add-script-tag.spec.ts", "should include sourceURL when path is provided")]  
[PlaywrightTest("page-event-pageerror.spec.ts", "should contain sourceURL")]  

### Wrong Error Messages (https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/issues/146)
[PlaywrightTest("elementhandle-eval-on-selector.spec.ts", "should throw in case of missing selector")]  
[PlaywrightTest("eval-on-selector.spec.ts", "should throw error if no element is found")]  
[PlaywrightTest("page-event-crash.spec.ts", "should throw on any action after page crashes")]  

