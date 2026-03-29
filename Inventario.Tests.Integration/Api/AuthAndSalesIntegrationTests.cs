using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using FluentAssertions;
using Inventario.Infrastructure.Persistence;
using Inventario.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Tests.Integration.Api;

public class AuthAndSalesIntegrationTests
{
    [Fact]
    public async Task Login_ShouldReturnAccessTokenAndRefreshToken()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/v1/Auth/login", new
        {
            userName = "admin",
            password = "Admin123*"
        });

        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        json.RootElement.GetProperty("success").GetBoolean().Should().BeTrue();
        json.RootElement.GetProperty("data").GetProperty("accessToken").GetString().Should().NotBeNullOrWhiteSpace();
        json.RootElement.GetProperty("data").GetProperty("refreshToken").GetString().Should().NotBeNullOrWhiteSpace();
    }

    [Fact]
    public async Task Refresh_ShouldRotateRefreshToken_AndInvalidatePreviousOne()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login", new
        {
            userName = "admin",
            password = "Admin123*"
        });

        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var originalRefreshToken = loginJson.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/Auth/refresh", new
        {
            refreshToken = originalRefreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var refreshJson = JsonDocument.Parse(await refreshResponse.Content.ReadAsStringAsync());
        var rotatedRefreshToken = refreshJson.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();

        rotatedRefreshToken.Should().NotBeNullOrWhiteSpace();
        rotatedRefreshToken.Should().NotBe(originalRefreshToken);

        var invalidReuseResponse = await client.PostAsJsonAsync("/api/v1/Auth/refresh", new
        {
            refreshToken = originalRefreshToken
        });

        invalidReuseResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Logout_ShouldRevokeRefreshToken_AndBlockFutureRefresh()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var loginResponse = await client.PostAsJsonAsync("/api/v1/Auth/login", new
        {
            userName = "empleado",
            password = "Empleado123*"
        });

        using var loginJson = JsonDocument.Parse(await loginResponse.Content.ReadAsStringAsync());
        var refreshToken = loginJson.RootElement.GetProperty("data").GetProperty("refreshToken").GetString();

        var logoutResponse = await client.PostAsJsonAsync("/api/v1/Auth/logout", new
        {
            refreshToken
        });

        logoutResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        var refreshResponse = await client.PostAsJsonAsync("/api/v1/Auth/refresh", new
        {
            refreshToken
        });

        refreshResponse.StatusCode.Should().Be(HttpStatusCode.Unauthorized);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var persistedToken = await dbContext.RefreshTokens.FirstAsync(x => x.Token == refreshToken);
        persistedToken.RevokedAt.Should().NotBeNull();
    }

    [Fact]
    public async Task ProtectedProductsEndpoint_WithoutToken_ShouldReturnUnauthorized()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });

        var response = await client.GetAsync("/api/v1/Products");

        response.StatusCode.Should().Be(HttpStatusCode.Unauthorized);
    }

    [Fact]
    public async Task Employee_ShouldReceiveForbidden_WhenCallingAdminOnlyApiEndpoints()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var employeeToken = await LoginAndGetAccessTokenAsync(client, "empleado", "Empleado123*");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", employeeToken);

        var reportsResponse = await client.GetAsync("/api/v1/Reports/dashboard");
        var createProductResponse = await client.PostAsJsonAsync("/api/v1/Products", new
        {
            name = $"Producto protegido {Guid.NewGuid():N}",
            category = "Seguridad",
            brand = "PolicyGuard",
            price = 10m,
            stock = 2,
            minStock = 1
        });

        reportsResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        createProductResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Admin_ShouldGeneratePurchaseRequestPdf()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var adminToken = await LoginAndGetAccessTokenAsync(client, "admin", "Admin123*");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync("/api/v1/Reports/purchase-request/export-pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        (await response.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Admin_ShouldGenerateMonthlySalesPdf()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();

        var adminToken = await LoginAndGetAccessTokenAsync(client, "admin", "Admin123*");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var response = await client.GetAsync("/api/v1/Reports/monthly-sales/export-pdf");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
        response.Content.Headers.ContentType?.MediaType.Should().Be("application/pdf");
        (await response.Content.ReadAsByteArrayAsync()).Length.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task RegisterSale_ShouldPersistSaleAndDeductStock()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();
        var uniqueName = $"Monitor 27 {Guid.NewGuid():N}";

        var adminToken = await LoginAndGetAccessTokenAsync(client, "admin", "Admin123*");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createProductResponse = await client.PostAsJsonAsync("/api/v1/Products", new
        {
            name = uniqueName,
            category = "Pantallas",
            brand = "VisionPro",
            shortDescription = "Monitor para operación comercial.",
            description = "Monitor diseñado para estaciones de trabajo de alto uso.",
            price = 199.99m,
            stock = 10,
            minStock = 3
        });

        createProductResponse.StatusCode.Should().Be(HttpStatusCode.Created);
        var productId = await ExtractGuidFromResponseAsync(createProductResponse);

        var employeeToken = await LoginAndGetAccessTokenAsync(client, "empleado", "Empleado123*");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", employeeToken);

        var saleResponse = await client.PostAsJsonAsync("/api/v1/Sales", new
        {
            items = new[]
            {
                new { productId, quantity = 4 }
            }
        });

        saleResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var product = await dbContext.Products.FindAsync(productId);
        var salesCount = await dbContext.Sales.CountAsync();

        product.Should().NotBeNull();
        product!.Stock.Should().Be(6);
        salesCount.Should().Be(1);
    }

    [Fact]
    public async Task RegisterSale_WithSameIdempotencyKey_ShouldReplayPreviousResponse()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient();
        var uniqueName = $"Scanner Industrial {Guid.NewGuid():N}";

        var adminToken = await LoginAndGetAccessTokenAsync(client, "admin", "Admin123*");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", adminToken);

        var createProductResponse = await client.PostAsJsonAsync("/api/v1/Products", new
        {
            name = uniqueName,
            category = "Escaneo",
            brand = "ScanCore",
            shortDescription = "Scanner para inventario.",
            description = "Equipo preparado para operación intensiva y control de inventario.",
            price = 89.5m,
            stock = 8,
            minStock = 2
        });

        var productId = await ExtractGuidFromResponseAsync(createProductResponse);

        var employeeToken = await LoginAndGetAccessTokenAsync(client, "empleado", "Empleado123*");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", employeeToken);

        var idempotencyKey = Guid.NewGuid().ToString("N");
        var payload = JsonContent.Create(new
        {
            items = new[]
            {
                new { productId, quantity = 2 }
            }
        });

        using var firstRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Sales")
        {
            Content = payload
        };
        firstRequest.Headers.Add("Idempotency-Key", idempotencyKey);

        using var firstResponse = await client.SendAsync(firstRequest);
        var firstBody = await firstResponse.Content.ReadAsStringAsync();

        using var secondRequest = new HttpRequestMessage(HttpMethod.Post, "/api/v1/Sales")
        {
            Content = JsonContent.Create(new
            {
                items = new[]
                {
                    new { productId, quantity = 2 }
                }
            })
        };
        secondRequest.Headers.Add("Idempotency-Key", idempotencyKey);

        using var secondResponse = await client.SendAsync(secondRequest);
        var secondBody = await secondResponse.Content.ReadAsStringAsync();

        secondResponse.StatusCode.Should().Be(firstResponse.StatusCode);
        secondBody.Should().Be(firstBody);

        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        (await dbContext.Sales.CountAsync()).Should().Be(1);
        (await dbContext.IdempotencyRecords.CountAsync()).Should().Be(1);
    }

    private static async Task<string> LoginAndGetAccessTokenAsync(HttpClient client, string userName, string password)
    {
        var response = await client.PostAsJsonAsync("/api/v1/Auth/login", new { userName, password });
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").GetProperty("accessToken").GetString()!;
    }

    private static async Task<Guid> ExtractGuidFromResponseAsync(HttpResponseMessage response)
    {
        response.EnsureSuccessStatusCode();

        using var json = JsonDocument.Parse(await response.Content.ReadAsStringAsync());
        return json.RootElement.GetProperty("data").GetProperty("id").GetGuid();
    }
}
