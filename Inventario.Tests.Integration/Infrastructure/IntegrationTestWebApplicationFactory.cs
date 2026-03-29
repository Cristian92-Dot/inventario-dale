using Inventario.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Inventario.Tests.Integration.Infrastructure;

public class IntegrationTestWebApplicationFactory : WebApplicationFactory<Program>, IAsyncDisposable
{
    private readonly string _databaseName = $"InventarioTest_{Guid.NewGuid():N}";

    protected override void ConfigureWebHost(Microsoft.AspNetCore.Hosting.IWebHostBuilder builder)
    {
        builder.UseEnvironment("Development");
        var connectionString = $"Server=localhost,1433;Database={_databaseName};User Id=sa;Password=InventarioStrongPass123!;TrustServerCertificate=True;Encrypt=True;MultipleActiveResultSets=True";

        builder.UseSetting("ConnectionStrings:DefaultConnection", connectionString);

        builder.ConfigureAppConfiguration((_, configBuilder) =>
        {
            var configuration = new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = connectionString
            };

            configBuilder.AddInMemoryCollection(configuration);
        });

        builder.ConfigureServices(services =>
        {
            services.RemoveAll<DbContextOptions<ApplicationDbContext>>();
            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString, sql =>
                {
                    sql.MigrationsAssembly(typeof(ApplicationDbContext).Assembly.FullName);
                    sql.EnableRetryOnFailure(3);
                }));
        });
    }

    public override async ValueTask DisposeAsync()
    {
        using var scope = Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        try
        {
            await dbContext.Database.EnsureDeletedAsync();
        }
        catch
        {
        }

        await base.DisposeAsync();
    }
}
