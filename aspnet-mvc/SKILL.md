---
name: aspnet-mvc
version: 1.0.0
description: "Use for classic ASP.NET MVC 5 or ASP.NET Core MVC applications built with controllers, Razor views, model binding, filters, and server-rendered HTML."
author: mag1art
license: Apache-2.0
tags: [aspnet, aspnet-mvc, aspnetcore, mvc, razor, controllers, views, model-binding]
triggers:
  - ASP.NET MVC
  - MVC 5
  - ASP.NET Core MVC
  - Razor view
  - controller and view
  - model binding
  - action filter
  - area
metadata:
  hermes:
    tags: [aspnet, aspnet-mvc, aspnetcore, mvc, razor, controllers, views]
---

# ASP.NET MVC

## When to Use

Use for server-rendered MVC applications, controllers, Razor views, view models, routing, model binding, validation, filters, areas, layouts, and migration from classic ASP.NET MVC to ASP.NET Core MVC.

## When Not to Use

Do not use for a pure Web API, Blazor-only UI, Razor Pages-only project, or frontend code with no MVC boundary.

## Identify the Runtime

Classic ASP.NET MVC 5 usually has System.Web, Global.asax, Web.config, App_Start, and packages.config. ASP.NET Core MVC usually has a Microsoft.NET.Sdk.Web project, Program.cs, appsettings.json, and middleware registration.

Do not mix APIs between the two generations. System.Web.Mvc attributes and HttpContext types are not interchangeable with Microsoft.AspNetCore.Mvc.

## Workflow

1. Identify runtime, target framework, hosting model, route style, view engine, authentication, and dependency injection setup.
2. Preserve existing conventions before introducing newer patterns.
3. Separate request models, domain models, and view models.
4. Keep controller actions focused on HTTP/input orchestration; put business logic in services.
5. Validate model state and authorization before side effects.
6. Keep Razor views focused on presentation and use partials or view components for reusable UI.
7. Test routing, binding, validation, authorization, status/redirect behavior, and important rendered output.
8. Run build, tests, and a smoke test through the real host.

## Controller and Action Rules

- Use explicit action names, route constraints, and appropriate HTTP verbs.
- Return the correct result: View, PartialView, Redirect, File, NotFound, BadRequest, or ProblemDetails.
- Never bind domain entities directly for writes; use input models and explicit mapping.
- Protect POST forms with antiforgery tokens when cookie authentication is used.
- Validate authorization at resource/action level.
- Keep GET actions side-effect free.
- Do not expose exception details or internal IDs unnecessarily.

## Razor and Model Binding

- Use strongly typed views and display/editor templates.
- Encode output by default; treat Html.Raw as a security-sensitive exception.
- Validate both client-side and server-side.
- Do not trust hidden fields for authorization or ownership.
- Keep localization, date, number, and culture behavior explicit.
- Avoid database calls from views.

## ASP.NET Core MVC Setup

Inspect Program.cs for AddControllersWithViews, authentication, authorization, static files, routing, and MapControllerRoute. Keep middleware order explicit and configure antiforgery, HTTPS, error handling, caching, and security headers deliberately.

## Classic MVC 5 Notes

Inspect Web.config, Global.asax, FilterConfig, RouteConfig, BundleConfig, Areas, and dependency-injection bootstrap. Respect IIS hosting, System.Web lifecycle, machine-key, Forms Authentication, and MVC 5 package constraints.

## Migration Notes

- Replace System.Web and HttpModules/HttpHandlers with ASP.NET Core middleware and services.
- Replace Web.config configuration with appsettings, options, environment variables, and secret providers.
- Rework routing, filters, authentication, session, caching, bundling, and static-file behavior instead of only changing namespaces.
- Port views incrementally and test encoding, model binding, antiforgery, authorization, and URL generation.

## Quality Gate

Verify runtime generation, routing, binding and validation, antiforgery, authorization, output encoding, error handling, performance, tests, and production configuration.
## Example: MVC POST with Validation and Anti-Forgery

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(
    CreateOrderViewModel model,
    CancellationToken ct)
{
    if (!ModelState.IsValid)
        return View(model);

    var orderId = await _orders.CreateAsync(model, ct);
    return RedirectToAction(nameof(Details), new { id = orderId });
}
```

For classic MVC, verify anti-forgery configuration and binding behavior. For ASP.NET Core MVC, prefer validated request models, explicit authorization policies, async actions, and a service layer. Do not bind domain entities directly from form input.

