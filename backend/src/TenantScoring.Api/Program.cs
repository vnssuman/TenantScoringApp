using Microsoft.EntityFrameworkCore;
using TenantScoring.Core;
using TenantScoring.Data;
using TenantScoring.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TenantDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("TenantDatabase")));
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
        policy.WithOrigins("http://localhost:5173")
            .AllowAnyHeader()
            .AllowAnyMethod());
});
builder.Services.AddScoped<ITenantApplicationStore, TenantApplicationStore>();
builder.Services.AddScoped<ITenantIdGenerator, TenantIdGenerator>();
builder.Services.AddScoped<ITenantRegistrationService, TenantRegistrationService>();
builder.Services.AddScoped<IAuthenticationService, AuthenticationService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddSingleton<IAuthenticationTokenStore, AuthenticationTokenStore>();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
    dbContext.Database.EnsureCreated();
}

app.UseCors("Frontend");

app.MapPost("/api/tenants/register", async (
    RegisterTenantRequest request,
    ITenantRegistrationService registrationService,
    CancellationToken cancellationToken) =>
{
    var validationErrors = RegisterTenantValidator.Validate(request);
    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new { errors = validationErrors });
    }

    var result = await registrationService.RegisterAsync(request, cancellationToken);
    if (result.IsDuplicate)
    {
        return Results.BadRequest(new { message = "This mobile number is already registered." });
    }

    return Results.Created($"/api/tenants/{result.Response!.TenantId}", result.Response);
});

app.MapPost("/api/auth/login", async (
    LoginRequest request,
    IAuthenticationService authenticationService,
    CancellationToken cancellationToken) =>
{
    var validationErrors = AuthenticationValidator.ValidateLogin(request);
    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new { errors = validationErrors });
    }

    var result = await authenticationService.LoginAsync(request, cancellationToken);
    if (result.MobileNumberNotFound)
    {
        return Results.NotFound(new { message = "Mobile Number does not exist. Please register." });
    }

    if (result.InvalidPin)
    {
        return Results.BadRequest(new { message = "Invalid PIN. Please try again." });
    }

    return Results.Ok(result.Response);
});

app.MapPost("/api/auth/reset-pin", async (
    ResetPinRequest request,
    IAuthenticationService authenticationService,
    CancellationToken cancellationToken) =>
{
    var validationErrors = AuthenticationValidator.ValidateReset(request);
    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new { errors = validationErrors });
    }

    var result = await authenticationService.ResetPinAsync(request, cancellationToken);
    if (result.ValidationError is not null)
    {
        return Results.BadRequest(new { message = result.ValidationError });
    }

    if (result.MobileNumberNotFound)
    {
        return Results.NotFound(new { message = "Mobile Number does not exist. Please register." });
    }

    return Results.Ok(result.Response);
});

app.MapGet("/api/user/dashboard-summary", async (
    HttpRequest httpRequest,
    IAuthenticationTokenStore tokenStore,
    IDashboardService dashboardService,
    CancellationToken cancellationToken) =>
{
    var authorization = httpRequest.Headers.Authorization.ToString();
    if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
    {
        return Results.Unauthorized();
    }

    var token = authorization["Bearer ".Length..].Trim();
    if (token.Length == 0 || !tokenStore.TryGetTenantId(token, out var tenantId))
    {
        return Results.Unauthorized();
    }

    var summary = await dashboardService.GetSummaryAsync(tenantId, cancellationToken);
    return summary is null ? Results.Unauthorized() : Results.Ok(summary);
});

app.Run();

public partial class Program;
