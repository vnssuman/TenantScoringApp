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

app.Run();

public partial class Program;
