using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using System.Globalization;
using TenantScoring.Core;
using TenantScoring.Data;
using TenantScoring.Models;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<TenantDbContext>(options =>
    options
        .UseSqlServer(builder.Configuration.GetConnectionString("TenantDatabase"))
        .ConfigureWarnings(warnings => warnings.Ignore(RelationalEventId.PendingModelChangesWarning)));
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
builder.Services.AddScoped<IKirayeeRepository, KirayeeRepository>();
builder.Services.AddScoped<IKirayeeService, KirayeeService>();
builder.Services.AddSingleton<IAuthenticationTokenStore, AuthenticationTokenStore>();

var app = builder.Build();
var agreementDocumentsDirectory = Path.Combine(app.Environment.ContentRootPath, "App_Data", "RentalAgreements");

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var dbContext = scope.ServiceProvider.GetRequiredService<TenantDbContext>();
    dbContext.Database.Migrate();
}

app.UseCors("Frontend");
var tokenStore = app.Services.GetRequiredService<IAuthenticationTokenStore>();

Guid? GetUserId(HttpRequest request)
{
    var authorization = request.Headers.Authorization.ToString();
    if (!authorization.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)) return null;
    var token = authorization["Bearer ".Length..].Trim();
    return tokenStore.TryGetTenantId(token, out var userId) && Guid.TryParse(userId, out var parsed) ? parsed : null;
}

app.MapPost("/api/kirayee/auth/register", async (KirayeeRegisterRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var result = await service.RegisterAsync(request, cancellationToken);
    return result.Response is null
        ? Results.BadRequest(new { message = result.ErrorMessage })
        : Results.Created($"/api/kirayee/users/{result.Response.UserId}", result.Response);
});

app.MapPost("/api/kirayee/auth/login", async (LoginRequest request, IKirayeeRepository repository, IKirayeeService service, IAuthenticationTokenStore tokenStore, CancellationToken cancellationToken) =>
{
    var user = await repository.FindUserByMobileAsync(request.MobileNumber.Trim(), cancellationToken);
    if (user is null || !BCrypt.Net.BCrypt.Verify(request.Pin, user.PinHash)) return Results.Unauthorized();
    var profileStatus = await service.GetProfileStatusAsync(user.UserId, cancellationToken);
    var profile = await service.GetProfileAsync(user.UserId, cancellationToken);
    return profileStatus is null || profile is null
        ? Results.NotFound()
        : Results.Ok(new AuthResponseDto(user.UserId, profile.FirstName, user.CurrentRole, tokenStore.CreateToken(user.UserId.ToString()), profileStatus));
});

app.MapGet("/api/user/me", async (HttpRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var profile = await service.GetProfileAsync(userId.Value, cancellationToken);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
});

app.MapGet("/api/kirayee/profile", async (HttpRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var profile = await service.GetProfileAsync(userId.Value, cancellationToken);
    return profile is null ? Results.NotFound() : Results.Ok(profile);
});

app.MapPut("/api/kirayee/profile", async (HttpRequest request, ProfileUpdateRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var profile = await service.UpdateProfileAsync(userId.Value, body, cancellationToken);
    return profile is null ? Results.BadRequest(new { message = "Profile details are invalid." }) : Results.Ok(profile);
});

app.MapPost("/api/kirayee/verification", async (HttpRequest request, VerificationRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    return await service.VerifyAsync(userId.Value, body, cancellationToken) ? Results.Ok(new { verified = true }) : Results.BadRequest(new { message = "Verification failed." });
});

app.MapGet("/api/kirayee/behavioral-assessment", async (HttpRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var assessment = await service.GetBehavioralAssessmentAsync(userId.Value, cancellationToken);
    return assessment is null ? Results.NotFound() : Results.Ok(assessment);
});

app.MapPost("/api/kirayee/behavioral-assessment", async (HttpRequest request, BehavioralAssessmentRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var assessment = await service.SubmitBehavioralAssessmentAsync(userId.Value, body, cancellationToken);
    return assessment is null ? Results.BadRequest(new { message = "Submit one Yes or No answer for every active assessment question." }) : Results.Ok(assessment);
});

app.MapPut("/api/kirayee/role", async (HttpRequest request, RoleUpdateRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    return await service.UpdateRoleAsync(userId.Value, body, cancellationToken) ? Results.NoContent() : Results.BadRequest(new { message = "Role must be Owner, Tenant, or Both." });
});

app.MapPost("/api/kirayee/score", async (HttpRequest request, ScoreRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var score = await service.RegenerateScoreAsync(userId.Value, body, cancellationToken);
    return score is null ? Results.BadRequest(new { message = "Score inputs must be between 0 and 100." }) : Results.Ok(score);
});

app.MapPost("/api/kirayee/bank-accounts", async (HttpRequest request, BankAccountRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var account = await service.AddBankAccountAsync(userId.Value, body, cancellationToken);
    return account is null ? Results.BadRequest(new { message = "Bank account details are invalid." }) : Results.Ok(account);
});

app.MapGet("/api/kirayee/properties", async (HttpRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    return userId is null ? Results.Unauthorized() : Results.Ok(await service.GetPropertiesAsync(userId.Value, cancellationToken));
});
app.MapPost("/api/kirayee/properties", async (HttpRequest request, PropertyRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var property = await service.AddPropertyAsync(userId.Value, body, cancellationToken);
    return property is null ? Results.BadRequest(new { message = "Property details are invalid." }) : Results.Created($"/api/kirayee/properties/{property.PropertyId}", property);
});

app.MapGet("/api/properties", async (HttpRequest request, Guid userId, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var authenticatedUserId = GetUserId(request);
    if (authenticatedUserId is null) return Results.Unauthorized();
    if (authenticatedUserId.Value != userId) return Results.Forbid();
    var properties = await service.GetOwnerPropertiesAsync(authenticatedUserId.Value, cancellationToken);
    return properties is null ? Results.Forbid() : Results.Ok(properties);
});

app.MapPost("/api/properties", async (HttpRequest request, PropertyManagementRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var profile = await service.GetProfileAsync(userId.Value, cancellationToken);
    if (profile is null) return Results.NotFound();
    if (profile.CurrentRole is not ("Owner" or "Both")) return Results.Forbid();
    var property = await service.CreateOwnerPropertyAsync(userId.Value, body, cancellationToken);
    return property is null ? Results.BadRequest(new { message = "Property details are invalid." }) : Results.Created($"/api/properties/{property.Id}", property);
});

app.MapPut("/api/properties/{propertyId:guid}", async (HttpRequest request, Guid propertyId, PropertyManagementRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var profile = await service.GetProfileAsync(userId.Value, cancellationToken);
    if (profile is null) return Results.NotFound();
    if (profile.CurrentRole is not ("Owner" or "Both")) return Results.Forbid();
    var property = await service.UpdateOwnerPropertyAsync(userId.Value, propertyId, body, cancellationToken);
    return property is null ? Results.BadRequest(new { message = "Property details are invalid or the property is unavailable." }) : Results.Ok(property);
});

app.MapGet("/api/tenants/search", async (HttpRequest request, string q, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var ownerId = GetUserId(request);
    if (ownerId is null) return Results.Unauthorized();
    var tenants = await service.SearchTenantsAsync(ownerId.Value, q, cancellationToken);
    return tenants is null ? Results.Forbid() : Results.Ok(tenants);
});

app.MapGet("/api/rental-requests", async (HttpRequest request, string? status, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var ownerId = GetUserId(request);
    if (ownerId is null) return Results.Unauthorized();
    var requests = await service.GetOwnerRentalRequestsAsync(ownerId.Value, status, cancellationToken);
    return requests is null ? Results.Forbid() : Results.Ok(requests);
});

app.MapGet("/api/tenant/rental-requests", async (HttpRequest request, string? status, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var tenantId = GetUserId(request);
    if (tenantId is null) return Results.Unauthorized();
    var requests = await service.GetTenantRentalRequestsAsync(tenantId.Value, status, cancellationToken);
    return requests is null ? Results.Forbid() : Results.Ok(requests);
});

app.MapGet("/api/tenant/rental-history", async (HttpRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var tenantId = GetUserId(request);
    if (tenantId is null) return Results.Unauthorized();
    var rentals = await service.GetTenantRentalHistoryAsync(tenantId.Value, cancellationToken);
    return rentals is null ? Results.Forbid() : Results.Ok(rentals);
});

app.MapPost("/api/rental-requests", async (HttpRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var ownerId = GetUserId(request);
    if (ownerId is null) return Results.Unauthorized();
    var profile = await service.GetProfileAsync(ownerId.Value, cancellationToken);
    if (profile is null) return Results.NotFound();
    if (profile.CurrentRole is not ("Owner" or "Both")) return Results.Forbid();

    var form = await request.ReadFormAsync(cancellationToken);
    if (!Guid.TryParse(form["propertyId"], out var propertyId)
        || !Guid.TryParse(form["tenantId"], out var tenantId)
        || !decimal.TryParse(form["rentAmount"], NumberStyles.Number, CultureInfo.InvariantCulture, out var rentAmount)
        || !DateTime.TryParse(form["startDate"], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var startDate)
        || !DateTime.TryParse(form["endDate"], CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var endDate))
    {
        return Results.BadRequest(new { message = "Select a property and tenant, and enter valid rent and rental dates." });
    }

    string? documentFileName = null;
    var document = form.Files.GetFile("agreementDocument");
    if (document is not null)
    {
        if (document.Length == 0 || document.Length > 5 * 1024 * 1024 || !string.Equals(Path.GetExtension(document.FileName), ".pdf", StringComparison.OrdinalIgnoreCase))
        {
            return Results.BadRequest(new { message = "Agreement documents must be PDF files no larger than 5 MB." });
        }

        await using var uploadStream = document.OpenReadStream();
        var pdfSignature = new byte[5];
        if (await uploadStream.ReadAsync(pdfSignature, cancellationToken) != pdfSignature.Length || !pdfSignature.AsSpan().SequenceEqual("%PDF-"u8))
        {
            return Results.BadRequest(new { message = "The agreement document is not a valid PDF file." });
        }
        uploadStream.Position = 0;

        Directory.CreateDirectory(agreementDocumentsDirectory);
        documentFileName = $"{Guid.NewGuid():N}.pdf";
        var documentPath = Path.Combine(agreementDocumentsDirectory, documentFileName);
        await using var fileStream = File.Create(documentPath);
        await uploadStream.CopyToAsync(fileStream, cancellationToken);
    }

    var created = await service.CreateRentalRequestAsync(ownerId.Value, propertyId, tenantId, rentAmount, startDate, endDate, documentFileName, cancellationToken);
    if (created is null)
    {
        if (documentFileName is not null) File.Delete(Path.Combine(agreementDocumentsDirectory, documentFileName));
        return Results.BadRequest(new { message = "The selected tenant or vacant property is not available for a rental request." });
    }

    return Results.Created($"/api/rental-requests/{created.LeaseId}", created);
});

app.MapGet("/api/rental-requests/{leaseId:guid}/document", async (HttpRequest request, Guid leaseId, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var document = await service.GetRentalRequestDocumentAsync(userId.Value, leaseId, cancellationToken);
    if (!document.IsAuthorized) return Results.Forbid();
    if (string.IsNullOrWhiteSpace(document.DocumentPath)) return Results.NotFound();
    var path = Path.GetFullPath(Path.Combine(agreementDocumentsDirectory, Path.GetFileName(document.DocumentPath)));
    if (!path.StartsWith(Path.GetFullPath(agreementDocumentsDirectory) + Path.DirectorySeparatorChar, StringComparison.OrdinalIgnoreCase) || !File.Exists(path)) return Results.NotFound();
    return Results.File(path, "application/pdf", "rental-agreement.pdf");
});

app.MapPost("/api/kirayee/leases", async (HttpRequest request, LeaseRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var lease = await service.CreateLeaseAsync(userId.Value, body, cancellationToken);
    return lease is null ? Results.BadRequest(new { message = "The property is unavailable." }) : Results.Created($"/api/kirayee/leases/{lease.LeaseId}", lease);
});

app.MapGet("/api/kirayee/leases", async (HttpRequest request, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    return userId is null ? Results.Unauthorized() : Results.Ok(await service.GetLeasesAsync(userId.Value, cancellationToken));
});

app.MapPost("/api/kirayee/leases/{leaseId:guid}/{action}", async (HttpRequest request, Guid leaseId, string action, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Json(new { message = "Your session has expired. Please sign in again." }, statusCode: StatusCodes.Status401Unauthorized);
    var lease = await service.ChangeLeaseStatusAsync(userId.Value, leaseId, action, cancellationToken);
    return lease is null ? Results.BadRequest(new { message = "The lease action is not permitted." }) : Results.Ok(lease);
});

app.MapPost("/api/kirayee/payments/{paymentId:guid}/pay", async (HttpRequest request, Guid paymentId, PaymentRequest body, IKirayeeService service, CancellationToken cancellationToken) =>
{
    var userId = GetUserId(request);
    if (userId is null) return Results.Unauthorized();
    var payment = await service.PayAsync(userId.Value, paymentId, body, cancellationToken);
    return payment is null ? Results.BadRequest(new { message = "Payment could not be completed." }) : Results.Ok(payment);
});

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
    IKirayeeRepository kirayeeRepository,
    IKirayeeService kirayeeService,
    IAuthenticationTokenStore tokenStore,
    CancellationToken cancellationToken) =>
{
    var validationErrors = AuthenticationValidator.ValidateLogin(request);
    if (validationErrors.Count > 0)
    {
        return Results.BadRequest(new { errors = validationErrors });
    }

    var normalizedUser = await kirayeeRepository.FindUserByMobileAsync(request.MobileNumber.Trim(), cancellationToken);
    if (normalizedUser is not null)
    {
        if (!BCrypt.Net.BCrypt.Verify(request.Pin, normalizedUser.PinHash)) return Results.BadRequest(new { message = "Invalid PIN. Please try again." });
        var profile = await kirayeeService.GetProfileAsync(normalizedUser.UserId, cancellationToken);
        return profile is null
            ? Results.NotFound(new { message = "Profile does not exist." })
            : Results.Ok(new AuthResponseDto(normalizedUser.UserId, profile.FirstName, normalizedUser.CurrentRole, tokenStore.CreateToken(normalizedUser.UserId.ToString()), profile.ProfileStatus));
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
    IKirayeeRepository kirayeeRepository,
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

    var summary = Guid.TryParse(tenantId, out var userId)
        ? await GetNormalizedDashboardSummaryAsync(userId, kirayeeRepository, cancellationToken)
        : await dashboardService.GetSummaryAsync(tenantId, cancellationToken);
    return summary is null ? Results.Unauthorized() : Results.Ok(summary);
});

static async Task<DashboardUserSummaryDto?> GetNormalizedDashboardSummaryAsync(Guid userId, IKirayeeRepository repository, CancellationToken cancellationToken)
{
    var user = await repository.FindUserAsync(userId, cancellationToken);
    var profile = await repository.FindProfileAsync(userId, cancellationToken);
    return user is null || profile is null
        ? null
        : new DashboardUserSummaryDto(user.UserId.ToString(), $"{profile.FirstName} {profile.LastName}".Trim(), [user.CurrentRole], user.MobileNumber, user.Email, (await repository.GetLatestScoreAsync(userId, cancellationToken))?.FinalScore);
}

app.Run();

public partial class Program;
