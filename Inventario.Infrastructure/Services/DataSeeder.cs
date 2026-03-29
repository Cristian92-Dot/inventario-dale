using Inventario.Domain.Entities;
using Inventario.Infrastructure.Identity;
using Inventario.Infrastructure.Persistence;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Inventario.Infrastructure.Services;

public class DataSeeder
{
    public const string AdminRole = "ADMIN";
    public const string EmployeeRole = "EMPLEADO";
    public const string DefaultAdminUserName = "admin";
    public const string DefaultAdminEmail = "admin@inventario.local";
    public const string DefaultAdminPassword = "Admin123*";
    public const string DefaultEmployeeUserName = "empleado";
    public const string DefaultEmployeeEmail = "empleado@inventario.local";
    public const string DefaultEmployeePassword = "Empleado123*";

    private readonly RoleManager<ApplicationRole> _roleManager;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly ApplicationDbContext _dbContext;

    public DataSeeder(
        RoleManager<ApplicationRole> roleManager,
        UserManager<ApplicationUser> userManager,
        ApplicationDbContext dbContext)
    {
        _roleManager = roleManager;
        _userManager = userManager;
        _dbContext = dbContext;
    }

    public async Task SeedAsync()
    {
        if (!await _roleManager.RoleExistsAsync(AdminRole))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = AdminRole });
        }

        if (!await _roleManager.RoleExistsAsync(EmployeeRole))
        {
            await _roleManager.CreateAsync(new ApplicationRole { Name = EmployeeRole });
        }

        var admin = await _userManager.FindByNameAsync(DefaultAdminUserName);
        if (admin is null)
        {
            admin = new ApplicationUser
            {
                UserName = DefaultAdminUserName,
                DisplayName = "Administrador principal",
                Email = DefaultAdminEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createResult = await _userManager.CreateAsync(admin, DefaultAdminPassword);
            if (!createResult.Succeeded)
            {
                var errors = string.Join(", ", createResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"No fue posible sembrar el usuario administrador: {errors}");
            }
        }

        if (!await _userManager.IsInRoleAsync(admin, AdminRole))
        {
            await _userManager.AddToRoleAsync(admin, AdminRole);
        }

        if (string.IsNullOrWhiteSpace(admin.DisplayName))
        {
            admin.DisplayName = "Administrador principal";
            await _userManager.UpdateAsync(admin);
        }

        var employee = await _userManager.FindByNameAsync(DefaultEmployeeUserName);
        if (employee is null)
        {
            employee = new ApplicationUser
            {
                UserName = DefaultEmployeeUserName,
                DisplayName = "Usuario operativo",
                Email = DefaultEmployeeEmail,
                EmailConfirmed = true,
                CreatedAt = DateTime.UtcNow
            };

            var createEmployeeResult = await _userManager.CreateAsync(employee, DefaultEmployeePassword);
            if (!createEmployeeResult.Succeeded)
            {
                var errors = string.Join(", ", createEmployeeResult.Errors.Select(x => x.Description));
                throw new InvalidOperationException($"No fue posible sembrar el usuario empleado: {errors}");
            }
        }

        if (!await _userManager.IsInRoleAsync(employee, EmployeeRole))
        {
            await _userManager.AddToRoleAsync(employee, EmployeeRole);
        }

        if (string.IsNullOrWhiteSpace(employee.DisplayName))
        {
            employee.DisplayName = "Usuario operativo";
            await _userManager.UpdateAsync(employee);
        }

        await SeedCategoriesAsync(admin.UserName ?? DefaultAdminUserName);
        await SeedProductsAsync(admin.UserName ?? DefaultAdminUserName);
        await SeedProductMediaAsync(admin.UserName ?? DefaultAdminUserName);
    }

    private async Task SeedCategoriesAsync(string createdBy)
    {
        if (await _dbContext.ProductCategories.AnyAsync())
        {
            return;
        }

        var categories = new[]
        {
            CreateCategory("Computo", "Equipos principales para operación administrativa y analítica.", 0, createdBy),
            CreateCategory("Pantallas", "Monitores y soluciones visuales para estaciones de trabajo.", 1, createdBy),
            CreateCategory("Perifericos", "Accesorios y dispositivos de apoyo para productividad diaria.", 2, createdBy),
            CreateCategory("Escaneo", "Equipos orientados a digitalización y captura documental.", 3, createdBy),
            CreateCategory("Logistica", "Herramientas para control de entradas, salidas y trazabilidad.", 4, createdBy)
        };

        await _dbContext.ProductCategories.AddRangeAsync(categories);
        await _dbContext.SaveChangesAsync();
    }

    private async Task SeedProductsAsync(string createdBy)
    {
        if (await _dbContext.Products.AnyAsync())
        {
            return;
        }

        var products = new[]
        {
            CreateProduct("Laptop Pro 14", "Computo", "DaLE Tech", "Ultrabook empresarial con gran rendimiento y bateria prolongada.", "Equipo pensado para operación intensiva, reuniones, reportes y trabajo de oficina con desempeño estable durante toda la jornada.", 1450m, 12, 4, createdBy),
            CreateProduct("Monitor 27 IPS", "Pantallas", "VisionPro", "Pantalla amplia con excelente nitidez para estaciones de trabajo.", "Monitor ideal para áreas administrativas y operativas que requieren buena visibilidad, color consistente y una experiencia cómoda en jornadas extensas.", 320m, 7, 3, createdBy),
            CreateProduct("Mouse Ergonomico", "Perifericos", "ComfortLine", "Accesorio ergonómico para uso prolongado.", "Diseñado para reducir fatiga en la mano, mejorar precisión y acompañar el uso diario en puestos con alta interacción digital.", 45m, 18, 6, createdBy),
            CreateProduct("Teclado Mecanico", "Perifericos", "KeyForge", "Teclado robusto con respuesta rápida para operación diaria.", "Ofrece durabilidad, respuesta táctil confiable y mejor experiencia de escritura para puestos administrativos y de digitación frecuente.", 95m, 9, 4, createdBy),
            CreateProduct("Scanner Industrial", "Escaneo", "ScanCore", "Equipo preparado para procesos de captura intensiva.", "Pensado para áreas de recepción, inventario y digitalización de documentos con ritmos de trabajo exigentes.", 210m, 2, 3, createdBy),
            CreateProduct("Lector de Codigos", "Logistica", "QuickScan", "Lector ágil para inventario y ventas en punto de operación.", "Facilita procesos rápidos de entrada, salida y venta de productos al integrarse con flujos de inventario y trazabilidad.", 160m, 1, 2, createdBy)
        };

        await _dbContext.Products.AddRangeAsync(products);
        await _dbContext.SaveChangesAsync();
    }

    private static Product CreateProduct(string name, string category, string brand, string shortDescription, string description, decimal price, int stock, int minStock, string createdBy)
    {
        var product = new Product
        {
            Name = name,
            Category = category,
            Brand = brand,
            ShortDescription = shortDescription,
            Description = description,
            Price = price,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        product.ConfigureInventory(stock, minStock);
        return product;
    }

    private async Task SeedProductMediaAsync(string updatedBy)
    {
        _dbContext.ChangeTracker.Clear();

        var mediaMap = new Dictionary<string, (string MainImagePath, string[] GalleryImages)>(StringComparer.OrdinalIgnoreCase)
        {
            ["Laptop Pro 14"] =
            (
                "/images/products/catalog/laptop-pro-14-main.jpg",
                [
                    "/images/products/catalog/laptop-pro-14-gallery-1.jpg",
                    "/images/products/catalog/laptop-pro-14-gallery-2.jpg"
                ]
            ),
            ["Monitor 27 IPS"] =
            (
                "/images/products/catalog/monitor-27-ips-main.jpg",
                [
                    "/images/products/catalog/monitor-27-ips-gallery-1.jpg",
                    "/images/products/catalog/monitor-27-ips-gallery-2.jpg"
                ]
            ),
            ["Mouse Ergonomico"] =
            (
                "/images/products/catalog/mouse-ergonomico-main.jpg",
                [
                    "/images/products/catalog/mouse-ergonomico-gallery-1.jpg",
                    "/images/products/catalog/mouse-ergonomico-gallery-2.jpg"
                ]
            ),
            ["Teclado Mecanico"] =
            (
                "/images/products/catalog/teclado-mecanico-main.jpg",
                [
                    "/images/products/catalog/teclado-mecanico-gallery-1.jpg",
                    "/images/products/catalog/teclado-mecanico-gallery-2.jpg"
                ]
            ),
            ["Scanner Industrial"] =
            (
                "/images/products/catalog/scanner-industrial-main.jpg",
                [
                    "/images/products/catalog/scanner-industrial-gallery-1.jpg",
                    "/images/products/catalog/scanner-industrial-gallery-2.jpg"
                ]
            ),
            ["Lector de Codigos"] =
            (
                "/images/products/catalog/lector-codigos-main.jpg",
                [
                    "/images/products/catalog/lector-codigos-gallery-1.jpg",
                    "/images/products/catalog/lector-codigos-gallery-2.jpg"
                ]
            )
        };

        var products = await _dbContext.Products
            .Select(product => new { product.Id, product.Name, product.ImagePath })
            .ToListAsync();

        var pendingGalleryImages = new List<ProductGalleryImage>();
        foreach (var product in products)
        {
            if (!mediaMap.TryGetValue(product.Name, out var media))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(product.ImagePath) || IsCatalogManagedImage(product.ImagePath) && !string.Equals(product.ImagePath, media.MainImagePath, StringComparison.OrdinalIgnoreCase))
            {
                await _dbContext.Products
                    .Where(item => item.Id == product.Id)
                    .ExecuteUpdateAsync(setters => setters
                        .SetProperty(item => item.ImagePath, media.MainImagePath)
                        .SetProperty(item => item.UpdatedAt, DateTime.UtcNow)
                        .SetProperty(item => item.UpdatedBy, updatedBy));
            }

            var galleryImagePaths = await _dbContext.ProductGalleryImages
                .Where(image => image.ProductId == product.Id)
                .OrderBy(image => image.SortOrder)
                .Select(image => image.ImagePath)
                .ToListAsync();

            var needsGalleryRefresh = galleryImagePaths.Count == 0 ||
                galleryImagePaths.All(IsCatalogManagedImage) && !galleryImagePaths.SequenceEqual(media.GalleryImages, StringComparer.OrdinalIgnoreCase);

            if (needsGalleryRefresh)
            {
                if (galleryImagePaths.Count > 0)
                {
                    await _dbContext.ProductGalleryImages
                        .Where(image => image.ProductId == product.Id)
                        .ExecuteDeleteAsync();
                }

                pendingGalleryImages.AddRange(media.GalleryImages.Select((imagePath, index) => new ProductGalleryImage
                {
                    ProductId = product.Id,
                    ImagePath = imagePath,
                    SortOrder = index,
                    CreatedAt = DateTime.UtcNow,
                    CreatedBy = updatedBy
                }));
            }
        }

        if (pendingGalleryImages.Count > 0)
        {
            await _dbContext.ProductGalleryImages.AddRangeAsync(pendingGalleryImages);
            await _dbContext.SaveChangesAsync();
        }
    }

    private static bool IsCatalogManagedImage(string? imagePath)
    {
        return !string.IsNullOrWhiteSpace(imagePath)
            && imagePath.StartsWith("/images/products/catalog/", StringComparison.OrdinalIgnoreCase);
    }

    private static ProductCategory CreateCategory(string name, string description, int sortOrder, string createdBy)
    {
        return new ProductCategory
        {
            Name = name,
            Description = description,
            SortOrder = sortOrder,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = createdBy
        };
    }

}
