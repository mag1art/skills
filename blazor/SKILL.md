---
name: blazor
version: 1.0.0
description: "Use for Blazor Web Apps and Blazor components, including static SSR, interactive Server/WebAssembly/Auto render modes, forms, state, and JavaScript interop."
author: mag1art
license: Apache-2.0
tags: [blazor, aspnetcore, razor-components, webassembly, server, ssr, render-modes, frontend]
triggers:
  - Blazor
  - Blazor Web App
  - InteractiveServer
  - InteractiveWebAssembly
  - InteractiveAuto
  - Razor component
  - render mode
  - JS interop
metadata:
  hermes:
    tags: [blazor, aspnetcore, razor-components, webassembly, server, ssr, render-modes]
---

# Blazor

## When to Use

Use for Blazor Web Apps, Razor components, render modes, component lifecycle, forms, validation, routing, authentication, state management, JavaScript interop, and performance.

## When Not to Use

Do not use for MVC/Razor Views-only work, a standalone JavaScript SPA, or backend API changes with no component boundary.

## Identify the Hosting Model

For .NET 8 and later, inspect whether the project is a Blazor Web App and which components use Static, Interactive Server, Interactive WebAssembly, or Interactive Auto rendering. For older projects, distinguish Blazor Server and standalone Blazor WebAssembly before changing hosting or state assumptions.

Every component render mode determines where it renders and whether it is interactive. Prerendering can run before the interactive runtime starts, so initialization code must tolerate two phases.

## Workflow

1. Inspect TargetFramework, project template, render modes, authentication, persistence, JavaScript dependencies, and deployment model.
2. Identify the component boundary and decide whether behavior belongs in a component, service, API, or database.
3. Choose the smallest render mode that satisfies the interaction.
4. Keep server-only services and secrets out of WebAssembly components.
5. Implement loading, error, empty, and disconnected states.
6. Add component, integration, accessibility, and end-to-end tests where warranted.
7. Verify prerendering, hydration, refresh, navigation, authorization, and production publishing.

## Render Modes

- Static SSR: server renders HTML without an interactive circuit.
- Interactive Server: events execute on the server over a circuit; manage connection loss and circuit state.
- Interactive WebAssembly: component code runs in the browser; protect API boundaries and account for download size.
- Interactive Auto: use only when the project is configured for it and the client/server transition is understood.
- Apply a render mode at the smallest practical component boundary; global interactivity increases resource use and coupling.

## Components and State

- Keep components focused and use parameters/events for local composition.
- Use cascading values deliberately and avoid hidden global mutable state.
- Treat scoped services differently in Server and WebAssembly hosting models.
- Do not assume a scoped service has the same lifetime or process location across render modes.
- Persist state explicitly when it must survive prerendering, refresh, reconnect, or navigation.
- Use clearly owned state transitions for complex screens.

## Forms and Security

- Use EditForm and explicit server-side validation.
- Do not trust client-side validation, hidden fields, or component parameters for authorization.
- Enforce resource authorization on the server/API.
- Protect antiforgery and cookie-authenticated actions according to the hosting model.
- Never ship secrets, database credentials, or privileged connection strings to WebAssembly.

## JavaScript Interop

- Keep JS interop at a narrow boundary and provide a C# abstraction for reusable behavior.
- Do not call browser-only APIs during prerendering unless guarded.
- Dispose JS object references and event subscriptions.
- Avoid synchronous JS interop in Server scenarios.
- Treat data crossing the JS boundary as untrusted and validate it.

## Performance

- Avoid making every page globally interactive without a reason.
- Virtualize large lists, paginate server data, and cancel stale requests.
- Minimize component rerenders and expensive lifecycle work.
- Measure WebAssembly download size, startup time, circuit count, memory, and API latency.
- Use streaming or incremental rendering where supported and beneficial.

## Quality Gate

Verify render mode, prerender/hydration behavior, state lifetime, authorization, antiforgery, reconnect behavior, JS disposal, accessibility, download size, tests, and deployment configuration.

Official references:
- https://learn.microsoft.com/aspnet/core/blazor/components/render-modes
- https://learn.microsoft.com/aspnet/core/blazor/fundamentals
- https://learn.microsoft.com/aspnet/core/blazor/hosting-models
