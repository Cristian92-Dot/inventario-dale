using System.Text;
using System.Diagnostics;
using System.Threading.RateLimiting;
using System.Reflection;
using System.Globalization;
using Asp.Versioning;
using Inventario.Application;
using Inventario.Application.Common;
using Inventario.Infrastructure;
using Inventario.Infrastructure.Identity;
using Inventario.Infrastructure.Persistence;
using Inventario.Infrastructure.Services;
using Inventario.Web.Swagger;
using Inventario.Web.Middleware;
using Inventario.Web.Services;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using QuestPDF.Infrastructure;
using Serilog;

var builder = WebApplication.CreateBuilder(args);
QuestPDF.Settings.License = LicenseType.Community;

var lempiraCulture = new CultureInfo("es-HN");
lempiraCulture.NumberFormat.CurrencySymbol = "L";
lempiraCulture.NumberFormat.CurrencyPositivePattern = 0;
lempiraCulture.NumberFormat.CurrencyNegativePattern = 1;

CultureInfo.DefaultThreadCurrentCulture = lempiraCulture;
CultureInfo.DefaultThreadCurrentUICulture = lempiraCulture;

builder.WebHost.ConfigureKestrel(options =>
{
    options.AddServerHeader = false;
    options.Limits.MaxRequestBodySize = 15 * 1024 * 1024;
});

builder.Host.UseSerilog((context, services, configuration) => configuration
    .ReadFrom.Configuration(context.Configuration)
    .ReadFrom.Services(services)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File("logs/inventario-.log", rollingInterval: RollingInterval.Day));

builder.Services.AddApplication();
builder.Services.AddInfrastructure(builder.Configuration);

builder.Services.AddControllers();
builder.Services.AddRazorPages(options =>
{
    options.Conventions.AuthorizeFolder("/");
    options.Conventions.AuthorizeFolder("/Dashboard", "AdminOrEmployee");
    options.Conventions.AuthorizeFolder("/Reports", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Products/Index", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Products/Details", "AdminOrEmployee");
    options.Conventions.AuthorizePage("/Products/Create", "AdminOnly");
    options.Conventions.AuthorizePage("/Products/Edit", "AdminOnly");
    options.Conventions.AuthorizePage("/Sales/Create", "AdminOrEmployee");
    options.Conventions.AuthorizeFolder("/Categories", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Users", "AdminOnly");
    options.Conventions.AuthorizeFolder("/Audit", "AdminOnly");
    options.Conventions.AllowAnonymousToPage("/Index");
    options.Conventions.AllowAnonymousToPage("/Account/Login");
    options.Conventions.AllowAnonymousToPage("/AccessDenied");
    options.Conventions.AllowAnonymousToPage("/Error");
});
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();
builder.Services.AddAntiforgery();
builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddScoped<IProductImageStorage, ProductImageStorage>();
builder.Services.AddScoped<IUserAvatarStorage, UserAvatarStorage>();

builder.Services.Configure<ApiBehaviorOptions>(options =>
{
    options.InvalidModelStateResponseFactory = context =>
    {
        var errors = context.ModelState
            .Values
            .SelectMany(value => value.Errors)
            .Select(error => string.IsNullOrWhiteSpace(error.ErrorMessage) ? "Solicitud inválida." : error.ErrorMessage)
            .Distinct()
            .ToArray();

        var response = ApiResponse<object>.Fail(errors, "Se detectaron errores de validación.") with
        {
            TraceId = Activity.Current?.TraceId.ToString() ?? string.Empty,
            CorrelationId = context.HttpContext.TraceIdentifier,
            Timestamp = DateTime.UtcNow
        };

        return new BadRequestObjectResult(response);
    };
});

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/AccessDenied";
    options.Cookie.Name = "Inventario.Auth";
    options.Cookie.HttpOnly = true;
    options.Cookie.SameSite = SameSiteMode.Lax;
    options.Cookie.SecurePolicy = CookieSecurePolicy.SameAsRequest;
    options.SlidingExpiration = true;
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
    options.Events.OnRedirectToLogin = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status401Unauthorized;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnRedirectToAccessDenied = context =>
    {
        if (context.Request.Path.StartsWithSegments("/api"))
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return Task.CompletedTask;
        }

        context.Response.Redirect(context.RedirectUri);
        return Task.CompletedTask;
    };
    options.Events.OnValidatePrincipal = async context =>
    {
        if (context.Principal is null)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
            return;
        }

        var userManager = context.HttpContext.RequestServices.GetRequiredService<UserManager<ApplicationUser>>();
        var user = await userManager.GetUserAsync(context.Principal);
        if (user is null || !user.IsActive)
        {
            context.RejectPrincipal();
            await context.HttpContext.SignOutAsync(IdentityConstants.ApplicationScheme);
        }
    };
});

var corsOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? Array.Empty<string>();

builder.Services.AddCors(options =>
{
    options.AddPolicy("DefaultCors", policy =>
    {
        if (corsOrigins.Length == 0)
        {
            policy.WithOrigins("http://localhost:5150", "https://localhost:7049")
                .AllowAnyHeader()
                .AllowAnyMethod()
                .WithExposedHeaders(CorrelationIdMiddleware.HeaderName);

            return;
        }

        policy.WithOrigins(corsOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .WithExposedHeaders(CorrelationIdMiddleware.HeaderName);
    });
});

builder.Services.Configure<RequestLocalizationOptions>(options =>
{
    options.DefaultRequestCulture = new Microsoft.AspNetCore.Localization.RequestCulture(lempiraCulture);
    options.SupportedCultures = new List<CultureInfo> { lempiraCulture };
    options.SupportedUICultures = new List<CultureInfo> { lempiraCulture };
});

builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
});

builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Inventario API",
        Version = "v1",
        Description = "API REST empresarial para la gestion integral de inventario, productos, ventas y trazabilidad operativa. " +
                      "Esta plataforma expone endpoints versionados para autenticacion con JWT, rotacion de refresh tokens, administracion del catalogo, registro transaccional de ventas, reporte de stock bajo y consulta de auditoria. " +
                      "La API incorpora respuestas uniformes, identificadores de correlacion y traza, controles de autorizacion por roles, validaciones de negocio, idempotencia en operaciones sensibles, auditoria automatica, bitacora tecnica, eventos de seguridad, trazabilidad por request, rate limiting y protecciones de seguridad HTTP orientadas a un entorno empresarial. " +
                      "Para soporte funcional o tecnico relacionado con esta integracion, el contacto responsable es Cristian Wilfredo Flores Pacheco, correo cristianwilfredo54@gmail.com y telefono 99120949.",
        Contact = new OpenApiContact
        {
            Name = "Cristian Wilfredo Flores Pacheco",
            Email = "cristianwilfredo54@gmail.com",
            Url = new Uri("mailto:cristianwilfredo54@gmail.com")
        }
    });

    var webXmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var webXmlPath = Path.Combine(AppContext.BaseDirectory, webXmlFile);
    if (File.Exists(webXmlPath))
    {
        options.IncludeXmlComments(webXmlPath, includeControllerXmlComments: true);
    }

    var applicationXmlFile = $"{typeof(Inventario.Application.DependencyInjection).Assembly.GetName().Name}.xml";
    var applicationXmlPath = Path.Combine(AppContext.BaseDirectory, applicationXmlFile);
    if (File.Exists(applicationXmlPath))
    {
        options.IncludeXmlComments(applicationXmlPath);
    }

    options.OperationFilter<InventorySwaggerOperationFilter>();
    options.SchemaFilter<InventorySwaggerSchemaFilter>();

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Token de acceso JWT requerido para consumir operaciones protegidas. Debe enviarse en el encabezado Authorization con el formato exacto: `Bearer {token}`."
    });
    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var jwtSection = builder.Configuration.GetSection("Jwt");
var jwtKey = jwtSection["Key"] ?? "ChangeThisInDevelopmentOnly1234567890";

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultScheme = "SmartScheme";
        options.DefaultAuthenticateScheme = "SmartScheme";
        options.DefaultChallengeScheme = "SmartScheme";
    })
    .AddPolicyScheme("SmartScheme", "JWT or Cookie", options =>
    {
        options.ForwardDefaultSelector = context =>
        {
            if (context.Request.Path.StartsWithSegments("/api"))
            {
                return JwtBearerDefaults.AuthenticationScheme;
            }

            var authHeader = context.Request.Headers.Authorization.ToString();
            return authHeader.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase)
                ? JwtBearerDefaults.AuthenticationScheme
                : IdentityConstants.ApplicationScheme;
        };
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSection["Issuer"],
            ValidAudience = jwtSection["Audience"],
            NameClaimType = System.Security.Claims.ClaimTypes.Name,
            RoleClaimType = System.Security.Claims.ClaimTypes.Role,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization(options =>
{
    options.FallbackPolicy = new AuthorizationPolicyBuilder()
        .RequireAuthenticatedUser()
        .Build();
    options.AddPolicy("AdminOnly", policy => policy.RequireRole(DataSeeder.AdminRole));
    options.AddPolicy("AdminOrEmployee", policy => policy.RequireRole(DataSeeder.AdminRole, DataSeeder.EmployeeRole));
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 120,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("auth-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 10,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));

    options.AddPolicy("sales-policy", httpContext =>
        RateLimitPartition.GetFixedWindowLimiter(
            partitionKey: httpContext.User.Identity?.Name ?? httpContext.Connection.RemoteIpAddress?.ToString() ?? "unknown",
            factory: _ => new FixedWindowRateLimiterOptions
            {
                PermitLimit = 30,
                Window = TimeSpan.FromMinutes(1),
                QueueLimit = 0,
                AutoReplenishment = true
            }));
});

var app = builder.Build();
var swaggerEnabled = app.Environment.IsDevelopment() || app.Environment.IsStaging() || app.Configuration.GetValue<bool>("Features:Swagger:Enabled");

app.UseRequestLocalization();

app.UseSerilogRequestLogging();
app.UseMiddleware<CorrelationIdMiddleware>();
app.UseMiddleware<HttpTransactionLoggingMiddleware>();
app.UseMiddleware<RequestTraceMiddleware>();
app.UseMiddleware<GlobalExceptionMiddleware>();

if (swaggerEnabled)
{
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.DocumentTitle = "Inventario API - Documentación";
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventario API v1");
        options.DisplayRequestDuration();
    });
}
else
{
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseMiddleware<SecurityHeadersMiddleware>();
app.UseStaticFiles();
app.UseRouting();
app.UseCors("DefaultCors");
app.UseAuthentication();
app.UseMiddleware<IdempotencyMiddleware>();
app.UseAuthorization();
app.UseRateLimiter();

app.MapControllers();
app.MapRazorPages();
app.MapGet("/health/live", () => Results.Ok(new { status = "live" })).AllowAnonymous();
app.MapGet("/health/ready", () => Results.Ok(new { status = "ready" })).AllowAnonymous();

using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await dbContext.Database.MigrateAsync();
    var seeder = scope.ServiceProvider.GetRequiredService<DataSeeder>();
    await seeder.SeedAsync();
}

app.Run();

public partial class Program;
