using System.Net;
using FluentAssertions;
using Inventario.Infrastructure.Persistence;
using Inventario.Tests.Integration.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace Inventario.Tests.Integration.Pages;

public class PageAuthorizationTests
{
    [Fact]
    public async Task AdminPage_WithoutAuthentication_ShouldRedirectToLogin()
    {
        await using var factory = new IntegrationTestWebApplicationFactory();
        using var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });

        var response = await client.GetAsync("/Products/Create");

        response.StatusCode.Should().Be(HttpStatusCode.Redirect);
        response.Headers.Location.Should().NotBeNull();
        response.Headers.Location!.OriginalString.Should().Contain("/Account/Login");
    }

    [Fact]
    public async Task Employee_ShouldReceiveForbidden_WhenOpeningAdminOnlyPages()
    {
        await using var factory = new PageAuthorizationWebApplicationFactory();
        using var client = CreateClient(factory, "empleado", "EMPLEADO");

        var usersResponse = await client.GetAsync("/Users/Index");
        var auditResponse = await client.GetAsync("/Audit/Index");
        var createProductResponse = await client.GetAsync("/Products/Create");
        var categoriesResponse = await client.GetAsync("/Categories/Index");

        usersResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        auditResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        createProductResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        categoriesResponse.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_ShouldReceiveForbidden_WhenCallingAdminOnlyPageHandlers()
    {
        await using var factory = new PageAuthorizationWebApplicationFactory();
        using var client = CreateClient(factory, "empleado", "EMPLEADO");

        var productExport = await client.GetAsync("/Products/Index?handler=Export");
        var dashboardExport = await client.GetAsync("/Dashboard/Index?handler=ExportProducts");
        var reportExport = await client.GetAsync("/Reports/LowStock?handler=Export");

        productExport.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        dashboardExport.StatusCode.Should().Be(HttpStatusCode.Forbidden);
        reportExport.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Employee_ShouldAccessSharedProductDetailPage()
    {
        await using var factory = new PageAuthorizationWebApplicationFactory();
        using var scope = factory.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var productId = dbContext.Products.Select(x => x.Id).First();

        using var client = CreateClient(factory, "empleado", "EMPLEADO");
        var response = await client.GetAsync($"/Products/Details/{productId}");

        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory, string userName, string role)
    {
        var client = factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-User", userName);
        client.DefaultRequestHeaders.Add("X-Test-Role", role);
        return client;
    }
}
