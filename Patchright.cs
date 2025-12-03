#!/usr/bin/dotnet run
#:package Microsoft.CodeAnalysis.CSharp@5.0.0

using Microsoft.CodeAnalysis;
using Microsoft.CodeAnalysis.CSharp;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using System.Text.RegularExpressions;
using System.Xml.Linq;

Console.WriteLine($"Patching Playwright .NET to create Patchright");

var playwrightPath = args.Length > 0 ? args[0] : "playwright-dotnet";
if (!Directory.Exists(playwrightPath))
{
  Console.WriteLine($"Path to Playwright source '{playwrightPath}' not found, provide the path or run `git clone https://github.com/microsoft/playwright-dotnet.git` in this directory first.");
  return;
}

////////////////////////////////////////////////////////////////////////

try
{
  DisablePackageValidation();
  PatchProjectFile();
  PatchTargetsFile();
  PatchVersionPropsFile();
  ReplaceReadmeFile();
  PatchDriverDownloader();
  PatchScriptsHelper();
  PatchWorker();
  PatchJSHandle();
  PatchFrame();
  PatchLocator();
  PatchPage();
  PatchBrowserContext();
  PatchClock();
  PatchTracing();
  PatchOptionsClasses();
  PatchBrowser();
  PatchBrowserType();
}
catch (Exception ex)
{
  Console.ForegroundColor = ConsoleColor.Red;
  Console.WriteLine(ex);
  Console.ResetColor();
}

////////////////////////////////////////////////////////////////////////

// Update the project file details for the Patchright NuGet package.
void PatchProjectFile()
{
  var playwrightProjectPath = Path.Combine(playwrightPath, "src", "Playwright", "Playwright.csproj");

  Console.WriteLine($"Patching project file: {playwrightProjectPath}");

  var doc = XDocument.Load(playwrightProjectPath);

  // TODO: Won't need this if we XML inline document the new parameters on patched methods.
  doc.Descendants("TreatWarningsAsErrors").FirstOrDefault()?.Value = "false";

  // Update package metadata.
  doc.Descendants("Title").FirstOrDefault()?.Value = "Patchright";
  doc.Descendants("PackageId").FirstOrDefault()?.Value = "Patchright";
  doc.Descendants("Summary").FirstOrDefault()?.Value = "The .NET port of Playwright patched with Patchright, used to automate Chromium with a single API.";
  doc.Descendants("Description").FirstOrDefault()?.Value = "Undetected .NET version of the Playwright testing and automation library.";
  doc.Descendants("Authors").FirstOrDefault()?.Value = "Microsoft Corporation, patched by Werner van Deventer";
  doc.Descendants("RepositoryUrl").FirstOrDefault()?.Value = "https://github.com/DevEnterpriseSoftware/patchright-dotnet.git";

  doc.Save(playwrightProjectPath);
}

// Update the targets file to replace Nuget package ID Microsoft.Playwright with Patchright.
void PatchTargetsFile()
{
  var targetFilePath = Path.Combine(playwrightPath, "src", "Playwright", "build", "Microsoft.Playwright.targets");

  Console.WriteLine($"Patching Targets file: {targetFilePath}");

  var content = File.ReadAllText(targetFilePath)
      .Replace("<NuGetPackageId>Microsoft.Playwright</NuGetPackageId>", "<NuGetPackageId>Patchright</NuGetPackageId>");

  var newTargetFilePath = Path.Combine(playwrightPath, "src", "Playwright", "build", "Patchright.targets");
  File.WriteAllText(newTargetFilePath, content);
}

// Update the version properties file details for the Patchright NuGet package.
void PatchVersionPropsFile()
{
  var playwrightVersionPropsPath = Path.Combine(playwrightPath, "src", "Common", "Version.props");
  
  Console.WriteLine($"Patching Version Props file: {playwrightVersionPropsPath}");

  var doc = XDocument.Load(playwrightVersionPropsPath);

  // Update package metadata.
  doc.Descendants("Authors").FirstOrDefault()?.Value = "Microsoft Corporation, patched by Werner van Deventer";
  doc.Descendants("Owners").FirstOrDefault()?.Value = "DevEnterprise Software";
  doc.Descendants("PackageTags").FirstOrDefault()?.Value = "patchright,undetected,headless,chrome,playwright";
  doc.Descendants("PackageProjectUrl").FirstOrDefault()?.Value = "https://github.com/DevEnterpriseSoftware/patchright-dotnet";
  doc.Descendants("RepositoryUrl").FirstOrDefault()?.Value = "https://github.com/DevEnterpriseSoftware/patchright-dotnet.git";
  doc.Descendants("PackageLicenseExpression").FirstOrDefault()?.Value = "Apache-2.0";

  doc.Save(playwrightVersionPropsPath);
}

void ReplaceReadmeFile()
{
  var playwrightReadmeFilePath = Path.Combine(playwrightPath, "README.md");

  Console.WriteLine($"Replacing README file: {playwrightReadmeFilePath}");

  File.Copy("README.md", playwrightReadmeFilePath, overwrite: true);
  
  var readmeContent = File.ReadAllText(playwrightReadmeFilePath);
  
  // Extract H1 content and convert to markdown.
  var h1Match = Regex.Match(readmeContent, @"<h1[^>]*>(.*?)</h1>", RegexOptions.Singleline);
  if (h1Match.Success)
  {
    var h1Content = h1Match.Groups[1].Value.Trim();
    readmeContent = Regex.Replace(readmeContent, @"<h1[^>]*>.*?</h1>", $"# {h1Content}", RegexOptions.Singleline);
  }
  
  // Remove P tags and their contents (badges, etc.)
  readmeContent = Regex.Replace(readmeContent, @"<p[^>]*>.*?</p>", "", RegexOptions.Singleline);
  
  File.WriteAllText(playwrightReadmeFilePath, readmeContent);
}

// Disable package validation in all project files so that builds work with the new PackageId value.
void DisablePackageValidation()
{
  foreach (var projectFile in Directory.EnumerateFiles(playwrightPath, "*.csproj", SearchOption.AllDirectories))
  {
    var doc = XDocument.Load(projectFile);
    var element = doc.Descendants("EnablePackageValidation").FirstOrDefault(e => e.Value == "true");
    if (element is not null)
    {
      Console.WriteLine($"Disabling EnablePackageValidation in project file: {projectFile}");
      element.Value = "false";
      doc.Save(projectFile);
    }
  }
}

// Change the driver download URL to point to Patchright releases.
void PatchDriverDownloader()
{
  var driverDownloaderPath = Path.Combine(playwrightPath, "src", "tools", "Playwright.Tooling", "DriverDownloader.cs");

  Console.WriteLine($"Patching DriverDownloader file: {driverDownloaderPath}");

  const string OldUrl = "https://playwright.azureedge.net/builds/driver";
  const string NewUrl = "https://github.com/Kaliiiiiiiiii-Vinyzu/patchright/releases/download";
  const string OldUrlConstruction = "{cdn}/playwright-{driverVersion}-{platform}.zip";
  const string NewUrlConstruction = "{cdn}/v{driverVersion}/playwright-{driverVersion}-{platform}.zip";

  var content = File.ReadAllText(driverDownloaderPath)
      .Replace(OldUrl, NewUrl)
      .Replace(OldUrlConstruction, NewUrlConstruction);
  File.WriteAllText(driverDownloaderPath, content);
}

// Remove sourceURL from being appended to any scripts.
void PatchScriptsHelper()
{
  var scriptsHelperPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "ScriptsHelper.cs");
  var scriptsHelperCode = File.ReadAllText(scriptsHelperPath);

  Console.WriteLine($"Patching ScriptsHelper file: {scriptsHelperPath}");

  var tree = CSharpSyntaxTree.ParseText(scriptsHelperCode);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);
  var addSourceUrlMethod = root.GetMethods("AddSourceUrlToScript").First();

  var originalBody = addSourceUrlMethod.Body!;

  var newReturnStatement = SyntaxFactory.ParseStatement("return source;")
    .WithLeadingTrivia(originalBody.Statements[0].GetLeadingTrivia())
    .WithTrailingTrivia(lineEndingStyle);

  var newAddSourceUrlBody = SyntaxFactory.Block(
    SyntaxFactory.SingletonList(newReturnStatement))
    .WithOpenBraceToken(originalBody.OpenBraceToken)
    .WithCloseBraceToken(originalBody.CloseBraceToken);

  root = root.ReplaceNode(addSourceUrlMethod, addSourceUrlMethod.WithBody(newAddSourceUrlBody));

  File.WriteAllText(scriptsHelperPath, root.ToFullString());
}

// Add isolatedContext parameter to Frame methods.
void PatchWorker()
{
  var methodNamesToPatch = new[] { "EvaluateAsync", "EvaluateHandleAsync" };

  var workerPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "Worker.cs");
  var workerCode = File.ReadAllText(workerPath);

  Console.WriteLine($"Patching Worker file: {workerPath}");

  File.WriteAllText(workerPath, AddIsolatedContextToMethods(workerCode, "Worker", methodNamesToPatch));

  var workerGeneratedInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Generated", "IWorker.cs");
  var workerGeneratedInterfaceCode = File.ReadAllText(workerGeneratedInterfacePath);

  Console.WriteLine($"Patching generated IWorker file: {workerGeneratedInterfacePath}");

  File.WriteAllText(workerGeneratedInterfacePath, AddIsolatedContextToMethods(workerGeneratedInterfaceCode, "IWorker", methodNamesToPatch));
}

// Add isolatedContext parameter to JSHandle methods.
void PatchJSHandle()
{
  var methodNamesToPatch = new[] { "EvaluateAsync", "EvaluateHandleAsync" };

  var jsHandlePath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "JSHandle.cs");
  var jsHandleCode = File.ReadAllText(jsHandlePath);

  Console.WriteLine($"Patching JSHandle file: {jsHandlePath}");

  File.WriteAllText(jsHandlePath, AddIsolatedContextToMethods(jsHandleCode, "JSHandle", methodNamesToPatch));

  var jsHandleGeneratedInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Generated", "IJSHandle.cs");
  var jsHandleGeneratedInterfaceCode = File.ReadAllText(jsHandleGeneratedInterfacePath);

  Console.WriteLine($"Patching generated IJSHandle file: {jsHandleGeneratedInterfacePath}");

  File.WriteAllText(jsHandleGeneratedInterfacePath, AddIsolatedContextToMethods(jsHandleGeneratedInterfaceCode, "IJSHandle", methodNamesToPatch));

  var jsHandleSupplementsInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Supplements", "IJSHandle.cs");
  var jsHandleSupplementsInterfaceCode = File.ReadAllText(jsHandleSupplementsInterfacePath);

  Console.WriteLine($"Patching supplements IJSHandle file: {jsHandleSupplementsInterfacePath}");

  var newJsHandleSupplementsInterfaceCode = AddIsolatedContextToMethods(jsHandleSupplementsInterfaceCode, "IJSHandle", methodNamesToPatch);
  File.WriteAllText(jsHandleSupplementsInterfacePath, newJsHandleSupplementsInterfaceCode.Replace("EvaluateAsync{T}(string, object)", "EvaluateAsync{T}(string, object, bool)"));
}

// Add isolatedContext parameter to Frame methods.
void PatchFrame()
{
  // Playwright has a weird private methods with underscores that also needs to be patched.
  var methodNamesToPatch = new[] { "EvaluateAsync", "EvaluateHandleAsync", "EvalOnSelectorAsync", "EvalOnSelectorAllAsync", "_evalOnSelectorAsync", "_evalOnSelectorAllAsync" };

  var framePath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "Frame.cs");
  var frameCode = File.ReadAllText(framePath);

  Console.WriteLine($"Patching Frame file: {framePath}");

  File.WriteAllText(framePath, AddIsolatedContextToMethods(frameCode, "Frame", methodNamesToPatch));

  var frameGeneratedInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Generated", "IFrame.cs");
  var frameGeneratedInterfaceCode = File.ReadAllText(frameGeneratedInterfacePath);

  Console.WriteLine($"Patching generated IFrame file: {frameGeneratedInterfacePath}");

  File.WriteAllText(frameGeneratedInterfacePath, AddIsolatedContextToMethods(frameGeneratedInterfaceCode, "IFrame", methodNamesToPatch));

  var frameSupplementsInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Supplements", "IFrame.cs");
  var frameSupplementsInterfaceCode = File.ReadAllText(frameSupplementsInterfacePath);

  Console.WriteLine($"Patching supplements IFrame file: {frameSupplementsInterfacePath}");

  File.WriteAllText(frameSupplementsInterfacePath, AddIsolatedContextToMethods(frameSupplementsInterfaceCode, "IFrame", methodNamesToPatch));
}

// Add isolatedContext parameter to Locator methods.
void PatchLocator()
{
  var methodNamesToPatch = new[] { "EvaluateAsync", "EvaluateHandleAsync", "EvaluateAllAsync" };

  var locatorPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "Locator.cs");
  var locatorCode = File.ReadAllText(locatorPath);

  Console.WriteLine($"Patching Locator file: {locatorPath}");

  File.WriteAllText(locatorPath, AddIsolatedContextToMethods(locatorCode, "Locator", methodNamesToPatch));

  var locatorGeneratedInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Generated", "ILocator.cs");
  var locatorGeneratedInterfaceCode = File.ReadAllText(locatorGeneratedInterfacePath);

  Console.WriteLine($"Patching generated ILocator file: {locatorGeneratedInterfacePath}");

  File.WriteAllText(locatorGeneratedInterfacePath, AddIsolatedContextToMethods(locatorGeneratedInterfaceCode, "ILocator", methodNamesToPatch));

  var locatorSupplementsInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Supplements", "ILocator.cs");
  var locatorSupplementsInterfaceCode = File.ReadAllText(locatorSupplementsInterfacePath);

  Console.WriteLine($"Patching supplements ILocator file: {locatorSupplementsInterfacePath}");

  File.WriteAllText(locatorSupplementsInterfacePath, AddIsolatedContextToMethods(locatorSupplementsInterfaceCode, "ILocator", methodNamesToPatch));
}

// Add isolatedContext parameter to Page methods.
void PatchPage()
{
  var methodNamesToPatch = new[] { "EvaluateAsync", "EvaluateHandleAsync", "EvalOnSelectorAsync", "EvalOnSelectorAllAsync" };

  var pagePath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "Page.cs");
  var pageCode = File.ReadAllText(pagePath);

  Console.WriteLine($"Patching Page file: {pagePath}");

  File.WriteAllText(pagePath, AddIsolatedContextToMethods(pageCode, "Page", methodNamesToPatch));

  // Read the code again after the isolated context changes have been applied.
  pageCode = File.ReadAllText(pagePath);

  var tree = CSharpSyntaxTree.ParseText(pageCode);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);
  var pageClass = root.GetClass("Page");
  var indentation = pageClass.Members.First().GetIndentation();

  var routeInjectingPropertyExists = pageClass.Members
    .OfType<PropertyDeclarationSyntax>()
    .Any(p => p.Identifier.Text == "RouteInjecting");

  if (!routeInjectingPropertyExists)
  {
    var routeInjectingProperty = SyntaxFactory.ParseMemberDeclaration("public bool RouteInjecting { get; private set; }");
    var injectRouteMethod = SyntaxFactory.ParseMemberDeclaration("""
          [MethodImpl(MethodImplOptions.NoInlining)]
          public async Task InstallInjectRouteAsync()
          {
              if (RouteInjecting || Context.RouteInjecting)
              {
                  return;
              }

              await RouteAsync("**/*", async route =>
              {
                  try
                  {
                      var request = route.Request;
                      if (request.ResourceType.Equals("document", StringComparison.OrdinalIgnoreCase) &&
                          request.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                      {
                          var protocol = request.Url.Split(':')[0];
                          await route.FallbackAsync(new RouteFallbackOptions { Url = protocol + "://patchright-init-script-inject.internal/" }).ConfigureAwait(false);
                      }
                      else
                      {
                          await route.FallbackAsync().ConfigureAwait(false);
                      }
                  }
                  catch
                  {
                      await route.FallbackAsync().ConfigureAwait(false);
                  }
              }).ConfigureAwait(false);

              RouteInjecting = true;
          }
          """);

    root = root.ReplaceNode(pageClass, pageClass.AddMembers(
      routeInjectingProperty!.WithLeadingTrivia(indentation.Insert(0, lineEndingStyle)).WithTrailingTrivia(lineEndingStyle, lineEndingStyle),
      injectRouteMethod!.WithFullIndentation(indentation, lineEndingStyle).WithTrailingTrivia(lineEndingStyle)));

    // Get the class again from the new root.
    pageClass = root.GetClass("Page");

    // Add InstallInjectRouteAsync method to ExposeBindingAsync.
    var exposeBindingMethod = root.GetMethods("InnerExposeBindingAsync").First();
    var newExposeBindingMethod = PatchMethodWithInjectedStatement(exposeBindingMethod, "await InstallInjectRouteAsync().ConfigureAwait(false);", indentation, lineEndingStyle);

    root = root.ReplaceNode(pageClass, pageClass.ReplaceNode(exposeBindingMethod, newExposeBindingMethod));

    // Get the class again from the new root.
    pageClass = root.GetClass("Page");

    // Add InstallInjectRouteAsync method to AddInitScriptAsync.
    var addInitScriptMethod = root.GetMethods("AddInitScriptAsync").First();
    var newAddInitScripMethod = PatchMethodWithInjectedStatement(addInitScriptMethod, "await InstallInjectRouteAsync().ConfigureAwait(false);", indentation, lineEndingStyle);

    root = root.ReplaceNode(pageClass, pageClass.ReplaceNode(addInitScriptMethod, newAddInitScripMethod));

    File.WriteAllText(pagePath, root.ToFullString());
  }

  var pageGeneratedInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Generated", "IPage.cs");
  var pageGeneratedInterfaceCode = File.ReadAllText(pageGeneratedInterfacePath);

  Console.WriteLine($"Patching generated IPage file: {pageGeneratedInterfacePath}");

  File.WriteAllText(pageGeneratedInterfacePath, AddIsolatedContextToMethods(pageGeneratedInterfaceCode, "IPage", methodNamesToPatch));

  var pageSupplementsInterfacePath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Supplements", "IPage.cs");
  var pageSupplementsInterfaceCode = File.ReadAllText(pageSupplementsInterfacePath);

  Console.WriteLine($"Patching supplements IPage file: {pageSupplementsInterfacePath}");

  File.WriteAllText(pageSupplementsInterfacePath, AddIsolatedContextToMethods(pageSupplementsInterfaceCode, "IPage", methodNamesToPatch));
}

// Add route injection to BrowserContext class and call from relevant methods.
void PatchBrowserContext()
{
  var browserContextPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "BrowserContext.cs");
  var browserContextCode = File.ReadAllText(browserContextPath);

  Console.WriteLine($"Patching BrowserContext file: {browserContextPath}");

  var tree = CSharpSyntaxTree.ParseText(browserContextCode);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);
  var browserContextClass = root.GetClass("BrowserContext");
  var indentation = browserContextClass.Members.First().GetIndentation();

  var routeInjectingPropertyExists = browserContextClass.Members
    .OfType<PropertyDeclarationSyntax>()
    .Any(p => p.Identifier.Text == "RouteInjecting");

  if (!routeInjectingPropertyExists)
  {
    var routeInjectingProperty = SyntaxFactory.ParseMemberDeclaration("public bool RouteInjecting { get; private set; }");
    var injectRouteMethod = SyntaxFactory.ParseMemberDeclaration("""
          [MethodImpl(MethodImplOptions.NoInlining)]
          public async Task InstallInjectRouteAsync()
          {
              if (RouteInjecting)
              {
                  return;
              }

              await RouteAsync("**/*", async route =>
              {
                  try
                  {
                      var request = route.Request;
                      if (request.ResourceType.Equals("document", StringComparison.OrdinalIgnoreCase) &&
                          request.Url.StartsWith("http", StringComparison.OrdinalIgnoreCase))
                      {
                          var protocol = request.Url.Split(':')[0];
                          await route.FallbackAsync(new RouteFallbackOptions { Url = protocol + "://patchright-init-script-inject.internal/" }).ConfigureAwait(false);
                      }
                      else
                      {
                          await route.FallbackAsync().ConfigureAwait(false);
                      }
                  }
                  catch
                  {
                      await route.FallbackAsync().ConfigureAwait(false);
                  }
              }).ConfigureAwait(false);

              RouteInjecting = true;
          }
          """);

    root = root.ReplaceNode(browserContextClass, browserContextClass.AddMembers(
      routeInjectingProperty!.WithLeadingTrivia(indentation.Insert(0, lineEndingStyle)).WithTrailingTrivia(lineEndingStyle, lineEndingStyle),
      injectRouteMethod!.WithFullIndentation(indentation, lineEndingStyle).WithTrailingTrivia(lineEndingStyle)));

    // Get the class again from the new root.
    browserContextClass = root.GetClass("BrowserContext");

    // Add InstallInjectRouteAsync method to ExposeBindingAsync.
    var exposeBindingMethod = root.GetMethods("ExposeBindingAsync")
      .First(m => m.Modifiers.Any(mod => mod.IsKind(SyntaxKind.PrivateKeyword)));

    var newExposeBindingMethod = PatchMethodWithInjectedStatement(exposeBindingMethod, "await InstallInjectRouteAsync().ConfigureAwait(false);", indentation, lineEndingStyle);

    root = root.ReplaceNode(browserContextClass, browserContextClass.ReplaceNode(exposeBindingMethod, newExposeBindingMethod));

    // Get the class again from the new root.
    browserContextClass = root.GetClass("BrowserContext");

    // Add InstallInjectRouteAsync method to AddInitScriptAsync.
    var addInitScriptMethod = root.GetMethods("AddInitScriptAsync").First();
    var newAddInitScripMethod = PatchMethodWithInjectedStatement(addInitScriptMethod, "await InstallInjectRouteAsync().ConfigureAwait(false);", indentation, lineEndingStyle);

    root = root.ReplaceNode(browserContextClass, browserContextClass.ReplaceNode(addInitScriptMethod, newAddInitScripMethod));

    File.WriteAllText(browserContextPath, root.ToFullString());
  }
}

// Call the InstallInjectRouteAsync method when a clock is installed.
void PatchClock()
{
  var clockPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "Clock.cs");
  var clockCode = File.ReadAllText(clockPath);

  Console.WriteLine($"Patching Clock file: {clockPath}");

  var tree = CSharpSyntaxTree.ParseText(clockCode);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);
  var clockClass = root.GetClass("Clock");
  var installMethod = clockClass.GetMethods("InstallAsync").First();

  var originalBody = installMethod.Body!;
  var installMethodStatements = originalBody.Statements.ToList();

  if (!installMethodStatements[0].ToFullString().Contains("InstallInjectRouteAsync"))
  {
    installMethodStatements.Insert(0, SyntaxFactory.ParseStatement("await browserContext.InstallInjectRouteAsync().ConfigureAwait(false);")
      .WithLeadingTrivia(installMethodStatements[0].GetLeadingTrivia())
      .WithTrailingTrivia(lineEndingStyle, lineEndingStyle));

    var newInstallMethodBody = SyntaxFactory.Block(installMethodStatements)
      .WithOpenBraceToken(originalBody.OpenBraceToken)
      .WithCloseBraceToken(originalBody.CloseBraceToken);

    root = root.ReplaceNode(clockClass, clockClass.ReplaceNode(installMethod, installMethod.WithBody(newInstallMethodBody)));

    File.WriteAllText(clockPath, root.ToFullString());
  }
}

// Call the InstallInjectRouteAsync method when tracing is started.
void PatchTracing()
{
  var tracingPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "Tracing.cs");
  var tracingCode = File.ReadAllText(tracingPath);

  Console.WriteLine($"Patching Tracing file: {tracingPath}");

  var tree = CSharpSyntaxTree.ParseText(tracingCode);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);
  var clockClass = root.GetClass("Tracing");
  var startMethod = clockClass.GetMethods("StartAsync").First();

  var originalBody = startMethod.Body!;
  var startMethodStatements = originalBody.Statements.ToList();

  if (!startMethodStatements[0].ToFullString().Contains("InstallInjectRouteAsync"))
  {
    startMethodStatements.Insert(0, SyntaxFactory.ParseStatement("await ((BrowserContext)Parent!).InstallInjectRouteAsync().ConfigureAwait(false);")
      .WithLeadingTrivia(startMethodStatements[0].GetLeadingTrivia())
      .WithTrailingTrivia(lineEndingStyle, lineEndingStyle));

    var newStartMethodBody = SyntaxFactory.Block(startMethodStatements)
      .WithOpenBraceToken(originalBody.OpenBraceToken)
      .WithCloseBraceToken(originalBody.CloseBraceToken);

    root = root.ReplaceNode(clockClass, clockClass.ReplaceNode(startMethod, startMethod.WithBody(newStartMethodBody)));

    File.WriteAllText(tracingPath, root.ToFullString());
  }
}

// Add FocusControl property and clone statement to options classes.
void PatchOptionsClasses()
{
  foreach (var optionsClass in new[] { "BrowserTypeLaunchPersistentContextOptions", "BrowserTypeLaunchOptions", "BrowserNewPageOptions", "BrowserNewContextOptions" })
  {
    var optionsClassPath = Path.Combine(playwrightPath, "src", "Playwright", "API", "Generated", "Options", $"{optionsClass}.cs");
    var optionsClassCode = File.ReadAllText(optionsClassPath);

    Console.WriteLine($"Patching {optionsClass} file: {optionsClassPath}");

    File.WriteAllText(optionsClassPath, AddFocusControlToOptionsClass(optionsClassCode, optionsClass));
  }
}

// Add focusControl parameter to browser methods.
void PatchBrowser()
{
  var methodNamesToPatch = new[] { "NewContextAsync" };

  var browserPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "Browser.cs");
  var browserCode = File.ReadAllText(browserPath);

  Console.WriteLine($"Patching Browser file: {browserPath}");

  var updatedBrowserCode = AddDictionaryEntryToMethod(browserCode, "Browser", methodNamesToPatch, "focusControl", "options.FocusControl");

  // Parse the updated code to add FocusControl property assignment to NewPageAsync.
  var tree = CSharpSyntaxTree.ParseText(updatedBrowserCode);
  var root = tree.GetRoot();
  var browserClass = root.GetClass("Browser");
  var newPageMethod = browserClass.GetMethods("NewPageAsync").FirstOrDefault();

  if (newPageMethod is not null)
  {
    // Find the contextOptions object creation
    var objectCreation = newPageMethod.DescendantNodes()
      .OfType<ObjectCreationExpressionSyntax>()
      .FirstOrDefault(oc =>
        oc.Type is IdentifierNameSyntax ins &&
        ins.Identifier.Text == "BrowserNewContextOptions" &&
        oc.Initializer is not null);

    if (objectCreation?.Initializer is not null)
    {
      var expressions = objectCreation.Initializer.Expressions;

      // Check if FocusControl assignment already exists.
      var focusControlExists = expressions.Any(expr =>
        expr is AssignmentExpressionSyntax assignment &&
        assignment.Left is IdentifierNameSyntax identifierName &&
        identifierName.Identifier.Text == "FocusControl");

      if (!focusControlExists)
      {
        // Create the new FocusControl assignment.
        var newAssignment = SyntaxFactory.AssignmentExpression(
          SyntaxKind.SimpleAssignmentExpression,
          SyntaxFactory.IdentifierName("FocusControl"),
          SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            SyntaxFactory.IdentifierName("options"),
            SyntaxFactory.IdentifierName("FocusControl")))
          .WithOperatorToken(
            SyntaxFactory.Token(SyntaxKind.EqualsToken)
              .WithLeadingTrivia(SyntaxFactory.Space)
              .WithTrailingTrivia(SyntaxFactory.Space));

        // Get the first expression to copy trivia from
        var firstExpression = expressions.FirstOrDefault();
        if (firstExpression is not null)
        {
          newAssignment = newAssignment.WithTriviaFrom(firstExpression);
        }

        // Insert at the beginning.
        var newExpressions = expressions.Insert(0, newAssignment);

        // Build new separators list.
        var lineEndingStyle = DetectLineEndingStyle(root);
        var oldSeparators = expressions.GetSeparators().ToList();
        var newSeparators = new List<SyntaxToken> { SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(lineEndingStyle) };
        newSeparators.AddRange(oldSeparators);

        var newSeparatedList = SyntaxFactory.SeparatedList(newExpressions, newSeparators);
        var newInitializer = objectCreation.Initializer.WithExpressions(newSeparatedList);
        var newObjectCreation = objectCreation.WithInitializer(newInitializer);

        root = root.ReplaceNode(browserClass, browserClass.ReplaceNode(objectCreation, newObjectCreation));
        updatedBrowserCode = root.ToFullString();
      }
    }
  }

  File.WriteAllText(browserPath, updatedBrowserCode);
}

// Add focusControl parameter to browser type methods.
void PatchBrowserType()
{
  var methodNamesToPatch = new[] { "LaunchAsync", "LaunchPersistentContextAsync" };

  var browserPath = Path.Combine(playwrightPath, "src", "Playwright", "Core", "BrowserType.cs");
  var browserCode = File.ReadAllText(browserPath);

  Console.WriteLine($"Patching BrowserType file: {browserPath}");

  File.WriteAllText(browserPath, AddDictionaryEntryToMethod(browserCode, "BrowserType", methodNamesToPatch, "focusControl", "options.FocusControl"));
}

// Generic method to patch methods by adding async, replacing return with await, and injecting a statement at the beginning.
static MethodDeclarationSyntax PatchMethodWithInjectedStatement(MethodDeclarationSyntax methodDeclaration, string statementToInject, SyntaxTriviaList indentation, SyntaxTrivia lineEndingStyle)
{
  bool isArrowFunction = methodDeclaration.ExpressionBody is not null;
  var originalBody = methodDeclaration.Body!;

  // Handle arrow function syntax.
  if (isArrowFunction)
  {
    var arrowExpression = methodDeclaration.ExpressionBody!.Expression;

    if (arrowExpression is InvocationExpressionSyntax invocation &&
        invocation.Expression is SimpleNameSyntax simpleName &&
        simpleName.Identifier.Text == "SendMessageToServerAsync")
    {
      // Check if ConfigureAwait is already present.
      var hasConfigureAwait = invocation.DescendantNodes()
        .OfType<MemberAccessExpressionSyntax>()
        .Any(m => m.Name.Identifier.Text == "ConfigureAwait");

      if (!hasConfigureAwait)
      {
        // Add .ConfigureAwait(false)
        var configureAwaitInvocation = SyntaxFactory.InvocationExpression(
          SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            invocation,
            SyntaxFactory.IdentifierName("ConfigureAwait")),
          SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
              SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)))));

        arrowExpression = configureAwaitInvocation;
      }
    }

    // Convert arrow function to block body with await.
    var awaitExpression = SyntaxFactory.AwaitExpression(arrowExpression)
      .WithAwaitKeyword(
        SyntaxFactory.Token(SyntaxKind.AwaitKeyword)
          .WithTrailingTrivia(SyntaxFactory.Space));

    // Can't really figure out how to get the correct indentation here for the new code block statements.
    // Since there was only one statement in the original arrow function, we can just double indent all new statements to line them up.
    var doubleIndentation = SyntaxFactory.TriviaList(indentation).AddRange(indentation);

    // Create statements list with injected statement and await.
    var statements = new List<StatementSyntax>
        {
          SyntaxFactory.ParseStatement(statementToInject)
            .WithLeadingTrivia(doubleIndentation)
            .WithTrailingTrivia(lineEndingStyle, lineEndingStyle),
          SyntaxFactory.ExpressionStatement(awaitExpression)
            .WithLeadingTrivia(doubleIndentation)
            .WithTrailingTrivia(lineEndingStyle)
        };

    var newMethodBody = SyntaxFactory.Block(statements)
      .WithOpenBraceToken(
        SyntaxFactory.Token(SyntaxKind.OpenBraceToken)
          .WithLeadingTrivia(indentation)
          .WithTrailingTrivia(lineEndingStyle))
      .WithCloseBraceToken(
        SyntaxFactory.Token(SyntaxKind.CloseBraceToken)
          .WithLeadingTrivia(indentation)
          .WithTrailingTrivia(lineEndingStyle));

    // Remove expression body and add block body.
    methodDeclaration = methodDeclaration
      .WithExpressionBody(null)
      .WithSemicolonToken(default)
      .WithBody(newMethodBody);
  }
  else
  {
    // Handle regular method body.
    var methodStatements = methodDeclaration.Body!.Statements.ToList();

    // Insert the injected statement at the beginning.
    methodStatements.Insert(0, SyntaxFactory.ParseStatement(statementToInject)
      .WithLeadingTrivia(methodStatements[0].GetLeadingTrivia())
      .WithTrailingTrivia(lineEndingStyle, lineEndingStyle));

    var newMethodBody = SyntaxFactory.Block(methodStatements);

    // Find and replace any return statement with and await statement.
    var returnStatement = newMethodBody.DescendantNodes()
      .OfType<ReturnStatementSyntax>()
      .FirstOrDefault(r => r.Expression is InvocationExpressionSyntax inv &&
        inv.Expression is SimpleNameSyntax simpleName &&
        simpleName.Identifier.Text == "SendMessageToServerAsync");

    if (returnStatement is not null)
    {
      var invocation = (InvocationExpressionSyntax)returnStatement.Expression!;

      // Check if ConfigureAwait is already present.
      var hasConfigureAwait = invocation.DescendantNodes()
        .OfType<MemberAccessExpressionSyntax>()
        .Any(m => m.Name.Identifier.Text == "ConfigureAwait");

      if (!hasConfigureAwait)
      {
        // Add .ConfigureAwait(false)
        var configureAwaitInvocation = SyntaxFactory.InvocationExpression(
          SyntaxFactory.MemberAccessExpression(
            SyntaxKind.SimpleMemberAccessExpression,
            invocation,
            SyntaxFactory.IdentifierName("ConfigureAwait")),
          SyntaxFactory.ArgumentList(
            SyntaxFactory.SingletonSeparatedList(
              SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(SyntaxKind.FalseLiteralExpression)))));

        // Create await expression statement.
        var awaitStatement = SyntaxFactory.ExpressionStatement(
          SyntaxFactory.AwaitExpression(configureAwaitInvocation)
            .WithAwaitKeyword(
              SyntaxFactory.Token(SyntaxKind.AwaitKeyword)
                .WithTrailingTrivia(SyntaxFactory.Space)))
          .WithTriviaFrom(returnStatement);

        newMethodBody = newMethodBody.ReplaceNode(returnStatement, awaitStatement);
      }
    }

    methodDeclaration = methodDeclaration.WithBody(newMethodBody
      .WithOpenBraceToken(originalBody.OpenBraceToken)
      .WithCloseBraceToken(originalBody.CloseBraceToken));
  }

  // Add async modifier if not present.
  var newMethodModifiers = methodDeclaration.Modifiers;
  if (!newMethodModifiers.Any(m => m.IsKind(SyntaxKind.AsyncKeyword)))
  {
    newMethodModifiers = newMethodModifiers.Add(SyntaxFactory.Token(SyntaxKind.AsyncKeyword).WithTrailingTrivia(SyntaxFactory.Space));
    methodDeclaration = methodDeclaration.WithModifiers(newMethodModifiers);
  }

  return methodDeclaration;
}

// Method to add isolatedContext parameter to specified methods provided.
// This will also patch pass-through calls to private methods as needed and add the dictionary entry to SendMessageToServerAsync calls.
static string AddIsolatedContextToMethods(string code, string typeName, IEnumerable<string> methodNames)
{
  var tree = CSharpSyntaxTree.ParseText(code);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);
  var methodMissingIsolatedContextParam = new Func<MethodDeclarationSyntax, bool>(method => !method.ParameterList.Parameters.Any(p => p.Identifier.Text == "isolatedContext"));

  foreach (var methodName in methodNames)
  {
    // Find all overloads of the method in the current class.
    var methods = root
      .GetType(typeName)
      .GetMethods(methodName)
      .Where(methodMissingIsolatedContextParam);

    foreach (var method in methods)
    {
      var currentMethod = root
        .GetType(typeName)
        .GetMethods(methodName)
        .First(methodMissingIsolatedContextParam);

      // Find the first invocation of SendMessageToServerAsync and patch the dictionary parameter.
      var sendInvocationExpression = currentMethod.DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .FirstOrDefault(inv => inv.Expression is SimpleNameSyntax simpleName && simpleName.Identifier.Text == "SendMessageToServerAsync");

      if (sendInvocationExpression is not null)
      {
        var dictObjCreation = sendInvocationExpression.ArgumentList.Arguments.FirstOrDefault(a => a.Expression is ObjectCreationExpressionSyntax);
        if (dictObjCreation is not null)
        {
          var objectCreation = dictObjCreation.Expression as ObjectCreationExpressionSyntax;

          // Create the new assignment expression.
          // New Code: ["isolatedContext"] = isolatedContext
          var newEntry = SyntaxFactory.AssignmentExpression(
             SyntaxKind.SimpleAssignmentExpression,
             SyntaxFactory.ImplicitElementAccess(
               SyntaxFactory.BracketedArgumentList(
                 SyntaxFactory.SingletonSeparatedList(
                   SyntaxFactory.Argument(
                     SyntaxFactory.LiteralExpression(
                       SyntaxKind.StringLiteralExpression,
                       SyntaxFactory.Literal("isolatedContext")))))),
             SyntaxFactory.IdentifierName("isolatedContext"))
             .WithOperatorToken(
               SyntaxFactory.Token(SyntaxKind.EqualsToken)
                .WithLeadingTrivia(SyntaxFactory.Space)
                .WithTrailingTrivia(SyntaxFactory.Space));

          // Get existing expressions and determine the trivia to use.
          var existingExpressions = objectCreation!.Initializer!.Expressions;

          // Add proper leading trivia (newline and indentation) to match existing entries.
          if (existingExpressions.Count > 0)
          {
            var lastExpression = existingExpressions.Last();
            newEntry = newEntry.WithTriviaFrom(lastExpression);
          }

          // Create a new separated syntax list with the new entry.
          var separator = SyntaxFactory.Token(SyntaxKind.CommaToken);
          var newExpressions = existingExpressions.Add(newEntry);
          var newSeparatedList = SyntaxFactory.SeparatedList(newExpressions, existingExpressions.GetSeparators().Append(separator.WithTrailingTrivia(lineEndingStyle)));

          var newInitializer = objectCreation.Initializer.WithExpressions(newSeparatedList);
          var newObjectCreation = objectCreation.WithInitializer(newInitializer);

          root = root.ReplaceNode(currentMethod, currentMethod.ReplaceNode(objectCreation, newObjectCreation));

          // Refresh the current method reference after root update.
          currentMethod = root
            .GetType(typeName)
            .GetMethods(methodName)
            .First(methodMissingIsolatedContextParam);
        }
      }

      // The Playwright code has some passthrough calls to private methods or chain calls to frames which then call SendMessageToServerAsync (handled above).
      // Need to detect these and pass-through the isolatedContext parameter.
      var passthroughInvocationExpression = currentMethod.DescendantNodes()
        .OfType<InvocationExpressionSyntax>()
        .FirstOrDefault(inv =>
        {
          // Direct method call (e.g., EvaluateAsync(...))
          if (inv.Expression is SimpleNameSyntax simpleName)
          {
            return simpleName.Identifier.Text.StartsWith("_eval") || simpleName.Identifier.Text.StartsWith("Eval");
          }

          // Member access calls (e.g., h.EvaluateAsync(...) or _frame.EvalOnSelectorAllAsync(...))
          if (inv.Expression is MemberAccessExpressionSyntax memberAccess)
          {
            return memberAccess.Name.Identifier.Text.StartsWith("Eval");
          }

          return false;
        });

      if (passthroughInvocationExpression is not null)
      {
        // Create the new named argument.
        // New Code: isolatedContext: isolatedContext
        var newArgument = SyntaxFactory.Argument(
            SyntaxFactory.IdentifierName("isolatedContext"))
          .WithNameColon(
            SyntaxFactory.NameColon(
              SyntaxFactory.IdentifierName("isolatedContext"))
            .WithLeadingTrivia(SyntaxFactory.Space)
            .WithTrailingTrivia(SyntaxFactory.Space));

        // Create new argument list with the additional argument.
        var newArgumentList = passthroughInvocationExpression.ArgumentList.AddArguments(newArgument);

        // Create the new invocation expression.
        var newPassthroughInvocationExpression = passthroughInvocationExpression.WithArgumentList(newArgumentList);

        // Replace in the syntax tree.
        root = root.ReplaceNode(currentMethod, currentMethod.ReplaceNode(passthroughInvocationExpression, newPassthroughInvocationExpression));

        // Refresh the current method reference after root update.
        currentMethod = root
          .GetType(typeName)
          .GetMethods(methodName)
          .First(methodMissingIsolatedContextParam);
      }

      // Add isolatedContext parameter with default value of true.
      // New Code: bool isolatedContext = true
      var isolatedContextParam = SyntaxFactory.Parameter(
        SyntaxFactory.Identifier("isolatedContext"))
        .WithType(
          SyntaxFactory.PredefinedType(
            SyntaxFactory.Token(SyntaxKind.BoolKeyword))
        .WithTrailingTrivia(SyntaxFactory.Space))
        .WithDefault(
          SyntaxFactory.EqualsValueClause(
            SyntaxFactory.Token(SyntaxKind.EqualsToken)
              .WithLeadingTrivia(SyntaxFactory.Space)
              .WithTrailingTrivia(SyntaxFactory.Space),
            SyntaxFactory.LiteralExpression(SyntaxKind.TrueLiteralExpression)));

      // Add leading space if there are existing parameters.
      var newParameterList = currentMethod.ParameterList.Parameters.Count > 0
          ? currentMethod.ParameterList.AddParameters(isolatedContextParam.WithLeadingTrivia(SyntaxFactory.Space))
          : currentMethod.ParameterList.AddParameters(isolatedContextParam);

      // Replace the method in the syntax tree and update root.
      root = root.ReplaceNode(currentMethod, currentMethod.WithParameterList(newParameterList));
    }
  }

  return root.ToFullString();
}

// Method to add FocusControl property and clone statement to options classes.
static string AddFocusControlToOptionsClass(string code, string className)
{
  var tree = CSharpSyntaxTree.ParseText(code);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);
  var optionsClass = root.GetClass(className);

  var focusControlPropertyExists = optionsClass.Members
    .OfType<PropertyDeclarationSyntax>()
    .Any(p => p.Identifier.Text == "FocusControl");

  if (!focusControlPropertyExists)
  {
    var indentation = optionsClass.Members.First().GetIndentation();

    // Create the FocusControl property with JsonPropertyName attribute.
    var focusControlProperty = SyntaxFactory.ParseMemberDeclaration("""
          [JsonPropertyName("focusControl")]
          public bool? FocusControl { get; set; }
          """)!
      .WithFullIndentation(indentation, lineEndingStyle)
      .WithLeadingTrivia(SyntaxFactory.TriviaList().Add(lineEndingStyle).AddRange(indentation))
      .WithTrailingTrivia(lineEndingStyle);

    root = root.ReplaceNode(optionsClass, optionsClass.AddMembers(focusControlProperty));

    // Get the class again from the new root.
    optionsClass = root.GetClass(className);

    // Find the copy constructor (constructor with single parameter matching the class name).
    var copyConstructor = optionsClass.Members
      .OfType<ConstructorDeclarationSyntax>()
      .FirstOrDefault(c =>
        c.ParameterList.Parameters.Count == 1 &&
        c.ParameterList.Parameters[0].Type is IdentifierNameSyntax typeName &&
        typeName.Identifier.Text == className);

    if (copyConstructor?.Body is not null)
    {
      var constructorStatements = copyConstructor.Body.Statements.ToList();

      // Create the FocusControl assignment statement.
      var focusControlAssignment = SyntaxFactory.ParseStatement("FocusControl = clone.FocusControl;").WithTriviaFrom(constructorStatements[^1]);

      constructorStatements.Insert(constructorStatements.Count, focusControlAssignment);

      var newConstructorBody = SyntaxFactory.Block(constructorStatements)
        .WithOpenBraceToken(copyConstructor.Body.OpenBraceToken)
        .WithCloseBraceToken(copyConstructor.Body.CloseBraceToken);

      root = root.ReplaceNode(optionsClass, optionsClass.ReplaceNode(copyConstructor, copyConstructor.WithBody(newConstructorBody)));
    }
  }

  return root.ToFullString();
}

// Method to add a dictionary entry to the first Dictionary initialization found in a method.
// Supports both collection initializer syntax and object initializer syntax.
static string AddDictionaryEntryToMethod(string code, string className, IEnumerable<string> methodNames, string key, string value)
{
  var tree = CSharpSyntaxTree.ParseText(code);
  var root = tree.GetRoot();
  var lineEndingStyle = DetectLineEndingStyle(root);

  foreach (var methodName in methodNames)
  {
    var targetClass = root.GetClass(className);
    var targetMethod = targetClass.GetMethods(methodName).FirstOrDefault();

    if (targetMethod is null)
    {
      continue;
    }

    // Find the first dictionary creation in the method.
    var dictionaryCreation = targetMethod.DescendantNodes()
      .OfType<ObjectCreationExpressionSyntax>()
      .FirstOrDefault(oc =>
        oc.Type is GenericNameSyntax gns &&
        gns.Identifier.Text == "Dictionary" &&
        oc.Initializer is not null);

    if (dictionaryCreation?.Initializer is null)
    {
      continue;
    }

    var initializer = dictionaryCreation.Initializer;
    var expressions = initializer.Expressions;

    // Check if the key already exists.
    var keyExists = expressions.Any(expr =>
    {
      if (expr is AssignmentExpressionSyntax assignment &&
          assignment.Left is ImplicitElementAccessSyntax elementAccess)
      {
        var arg = elementAccess.ArgumentList.Arguments.FirstOrDefault();
        return arg?.Expression is LiteralExpressionSyntax literal &&
               literal.Token.ValueText == key;
      }
      else if (expr is InitializerExpressionSyntax initExpr &&
               initExpr.Expressions.Count == 2 &&
               initExpr.Expressions[0] is LiteralExpressionSyntax literal)
      {
        return literal.Token.ValueText == key;
      }

      return false;
    });

    if (keyExists)
    {
      continue;
    }

    // Determine which syntax style is being used.
    var firstExpression = expressions.FirstOrDefault();
    var isImplicitElementAccess = firstExpression is AssignmentExpressionSyntax assignment && assignment.Left is ImplicitElementAccessSyntax;

    ExpressionSyntax newEntry;

    if (isImplicitElementAccess)
    {
      // New syntax: ["key"] = value
      newEntry = SyntaxFactory.AssignmentExpression(
        SyntaxKind.SimpleAssignmentExpression,
        SyntaxFactory.ImplicitElementAccess(
          SyntaxFactory.BracketedArgumentList(
            SyntaxFactory.SingletonSeparatedList(
              SyntaxFactory.Argument(
                SyntaxFactory.LiteralExpression(
                  SyntaxKind.StringLiteralExpression,
                  SyntaxFactory.Literal(key)))))),
        SyntaxFactory.ParseExpression(value))
        .WithOperatorToken(
          SyntaxFactory.Token(SyntaxKind.EqualsToken)
            .WithLeadingTrivia(SyntaxFactory.Space)
            .WithTrailingTrivia(SyntaxFactory.Space));
    }
    else
    {
      // Old syntax: { "key", value }
      newEntry = SyntaxFactory.InitializerExpression(
        SyntaxKind.ComplexElementInitializerExpression,
        SyntaxFactory.SeparatedList(
        [
          SyntaxFactory.LiteralExpression(
              SyntaxKind.StringLiteralExpression,
              SyntaxFactory.Literal(key)
                .WithLeadingTrivia(SyntaxFactory.Space)),
            SyntaxFactory.ParseExpression(value)
              .WithLeadingTrivia(SyntaxFactory.Space)
              .WithTrailingTrivia(SyntaxFactory.Space)
        ]));
    }

    // Insert the new entry at the beginning.
    var newExpressions = expressions.Insert(0, newEntry.WithTriviaFrom(firstExpression!));

    // Build new separators list: insert a comma after the new entry, keep all existing separators.
    var oldSeparators = expressions.GetSeparators().ToList();
    var newSeparators = new List<SyntaxToken> { SyntaxFactory.Token(SyntaxKind.CommaToken).WithTrailingTrivia(lineEndingStyle) };
    newSeparators.AddRange(oldSeparators);

    var newSeparatedList = SyntaxFactory.SeparatedList(newExpressions, newSeparators);
    var newInitializer = initializer.WithExpressions(newSeparatedList);
    var newDictionaryCreation = dictionaryCreation.WithInitializer(newInitializer);


    root = root.ReplaceNode(targetClass, targetClass.ReplaceNode(dictionaryCreation, newDictionaryCreation));
  }

  return root.ToFullString();
}

// Analyze the existing file's line ending style to keep formatting consistent.
static SyntaxTrivia DetectLineEndingStyle(SyntaxNode root)
{
  var endOfLineTrivia = root.DescendantTrivia().FirstOrDefault(t => t.IsKind(SyntaxKind.EndOfLineTrivia));

  if (endOfLineTrivia != default)
  {
    return endOfLineTrivia.ToString() == "\r\n" ? SyntaxFactory.CarriageReturnLineFeed : SyntaxFactory.LineFeed;
  }

  // Use default if no line endings found.
  return SyntaxFactory.CarriageReturnLineFeed;
}

static class Extensions
{
  public static TypeDeclarationSyntax GetType(this SyntaxNode root, string typeName)
      => typeName.StartsWith('I') ? root.GetInterface(typeName) : root.GetClass(typeName);

  public static InterfaceDeclarationSyntax GetInterface(this SyntaxNode root, string interfaceName)
      => root.DescendantNodes()
          .OfType<InterfaceDeclarationSyntax>()
          .First(c => c.Identifier.Text == interfaceName);

  public static ClassDeclarationSyntax GetClass(this SyntaxNode root, string className)
      => root.DescendantNodes()
          .OfType<ClassDeclarationSyntax>()
          .First(c => c.Identifier.Text == className);

  public static IEnumerable<MethodDeclarationSyntax> GetMethods(this SyntaxNode @class, string methodName)
      => @class.DescendantNodes()
          .OfType<MethodDeclarationSyntax>()
          .Where(c => c.Identifier.Text == methodName);

  public static SyntaxTriviaList GetIndentation(this SyntaxNode node)
  {
    var leadingTrivia = node.GetLeadingTrivia();

    // Find the last whitespace trivia (which is typically the indentation)
    var lastWhitespace = leadingTrivia
      .Reverse()
      .FirstOrDefault(t => t.IsKind(SyntaxKind.WhitespaceTrivia));

    return lastWhitespace != default
      ? SyntaxFactory.TriviaList(lastWhitespace)
      : SyntaxFactory.TriviaList(SyntaxFactory.Whitespace("        "));
  }

  public static MemberDeclarationSyntax WithFullIndentation(this MemberDeclarationSyntax member, SyntaxTriviaList indentation, SyntaxTrivia lineEndingStyle)
  {
    var lines = member.ToFullString().Split(["\r\n", "\r", "\n"], StringSplitOptions.None);
    var indentString = indentation.ToString();

    // Find the minimum indentation of non-empty lines (this is the base indentation to remove).
    var nonEmptyLines = lines.Where(line => !string.IsNullOrWhiteSpace(line)).ToList();
    var minIndent = nonEmptyLines.Count != 0 ? nonEmptyLines.Min(line => line.Length - line.TrimStart().Length) : 0;

    // Indent all lines: remove the base indentation, then add the target indentation.
    var indentedLines = lines.Select(line =>
    {
      // Do not change empty lines.
      if (string.IsNullOrWhiteSpace(line))
      {
        return string.Empty;
      }

      // Remove the minimum indentation (the relative base), then add our target indentation.
      var trimmedLine = line.Length >= minIndent ? line[minIndent..] : line.TrimStart();
      return indentString + trimmedLine;
    });

    var indentedCode = string.Join(lineEndingStyle.ToString(), indentedLines);

    return SyntaxFactory.ParseMemberDeclaration(indentedCode)!;
  }
}
