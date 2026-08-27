---
name: dotnet-file-based-apps
version: 1.0.0
description: "Use for .NET 10 file-based C# applications that run from a single .cs file without a traditional .csproj project file."
author: mag1art
license: Apache-2.0
tags: [dotnet, csharp, file-based-apps, sdk, scripting, native-aot, dotnet-tools]
triggers:
  - file-based app
  - single C# file
  - dotnet run file.cs
  - C# script application
  - dotnet project convert
  - Native AOT file app
metadata:
  hermes:
    tags: [dotnet, csharp, file-based-apps, sdk, scripting, native-aot]
---

# .NET File-Based Apps

## When to Use

Use for .NET 10 SDK file-based apps: scripts, command-line utilities, prototypes, small tools, one-off automation, and applications that benefit from a single self-contained C# source file.

## When Not to Use

Do not use this model for a large multi-project solution, a service with complex build configuration, a shared library, or an application that needs extensive IDE/project tooling. Convert it to a traditional project when the source, dependencies, tests, or deployment process outgrow one file.

## Requirements

File-based apps require the .NET 10 SDK or later. Check the active SDK and repository-level global.json before using them:

~~~bash
dotnet --info
dotnet --list-sdks
~~~

The SDK creates a virtual project from the directives in the source file. A traditional .csproj is not required.

## Minimal File

~~~csharp
Console.WriteLine("Hello from a file-based app");
~~~

Run it with an explicit file option:

~~~bash
dotnet run --file app.cs
~~~

The shorthand forms are dotnet run app.cs and dotnet app.cs. If a project file exists in the current directory, prefer --file because dotnet run app.cs may run the existing project and pass app.cs as an application argument.

Pass arguments after the separator:

~~~bash
dotnet run --file app.cs -- input.json --verbose
~~~

## Configuration Directives

Place directives at the top of the C# file. Supported directives are:

- #:package Package.Name@1.2.3 — add an explicit NuGet package reference;
- #:project ../Shared/Shared.csproj — reference another project;
- #:property TargetFramework=net10.0 — set an MSBuild property;
- #:sdk Microsoft.NET.Sdk.Web — select an SDK such as the web SDK;
- #:include helpers.cs — include additional source/resource files where supported by the SDK.

Prefer explicit package versions. A version can be omitted only when central package management is configured; otherwise use a version or the documented @* form.

Example:

~~~csharp
#:
#:sdk Microsoft.NET.Sdk.Web
#:package Spectre.Console@0.49.1
#:property PublishAot=false

using Spectre.Console;

AnsiConsole.MarkupLine("[green]Ready[/]");
~~~

Do not place application statements before directives. Included .cs files can add declarations, but cannot add another set of top-level statements.

## Build, Publish, and Pack

~~~bash
dotnet restore app.cs
dotnet build app.cs --output ./build
dotnet publish app.cs --output ./publish
dotnet pack app.cs
~~~

Native AOT publishing and PackAsTool are enabled by default. Disable them explicitly when the application or dependency graph is not compatible:

~~~csharp
#:property PublishAot=false
#:property PackAsTool=false
~~~

Test trimming, reflection, serializers, dynamic loading, and platform-specific dependencies before relying on Native AOT.

## Convert to a Traditional Project

Use the SDK conversion command when the app needs a normal project structure:

~~~bash
dotnet project convert app.cs
~~~

The command creates a copy and a .csproj with equivalent SDK items, properties, and package references. It leaves the original .cs file untouched. Review the generated project before committing it.

## Configuration and Secrets

File-based apps respect global.json, Directory.Build.props, Directory.Build.targets, Directory.Packages.props, and nuget.config in their directory or parent directories. Isolate file-based apps from a nearby .csproj project when implicit settings could interfere.

Use user secrets instead of source-controlled credentials:

~~~bash
dotnet user-secrets set "ApiKey" "value" --file app.cs
dotnet user-secrets list --file app.cs
~~~

Never put real secrets in the source file or command history.

## Launch Profiles

For app.cs, a flat app.run.json next to the source can define development profiles. A traditional Properties/launchSettings.json takes priority when both exist. Use launch profiles for local behavior, not production secrets.

## Caching and Concurrency

The SDK caches file-based app build outputs under a temporary runfile directory. Changes to implicit build files or moving files can be confusing during development. If concurrent invocations contend for the same output, build once and run without rebuilding:

~~~bash
dotnet build app.cs
dotnet run app.cs --no-build
~~~

Clear stale file-based caches with dotnet clean file-based-apps or clean the individual file before rebuilding.

## Layout Rules

- Keep each independent file-based app in an isolated directory when it needs different build settings.
- Do not place a file-based app inside the directory cone of an unrelated .csproj.
- Keep included files and resources close to the entry file and document their role.
- Use a traditional project when tests, multiple executables, analyzers, packaging, or deployment settings become substantial.

## Quality Gate

Verify SDK version, directives, package versions, implicit build files, restore, build, run arguments, publish output, Native AOT compatibility, secrets handling, cache behavior, and the conversion path to .csproj.

Official reference:
https://learn.microsoft.com/en-us/dotnet/core/sdk/file-based-apps
## Example: Single-File Utility

```csharp
#:package Spectre.Console@0.49.1

using Spectre.Console;

AnsiConsole.MarkupLine("[green]File-based app started[/]");
```

Run it explicitly when the directory also contains a project:

```bash
dotnet run --file tools/report.cs -- --input data.json
dotnet project convert tools/report.cs
```

Keep package versions intentional and convert the file to a normal project when it needs tests, multiple source files, complex build properties, or team-scale maintenance.

