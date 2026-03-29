using Inventario.Application.Features.Auth.Dtos;
using Inventario.Application.Features.Products.Dtos;
using Inventario.Application.Features.Reports.Dtos;
using Inventario.Application.Features.Sales.Dtos;
using Inventario.Application.Features.Users.Dtos;
using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace Inventario.Web.Swagger;

public class InventorySwaggerSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(LoginRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["userName"] = new OpenApiString("admin"),
                ["password"] = new OpenApiString("Admin123*")
            };
        }

        if (context.Type == typeof(RefreshTokenRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["refreshToken"] = new OpenApiString("k6xA4fP4q8qQv1u7C0rZbVq2M4o4mXb6n1DkK0tI7LQ=")
            };
        }

        if (context.Type == typeof(CreateProductRequest) || context.Type == typeof(UpdateProductRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["name"] = new OpenApiString("Monitor 27 IPS"),
                ["category"] = new OpenApiString("Pantallas"),
                ["brand"] = new OpenApiString("VisionPro"),
                ["shortDescription"] = new OpenApiString("Pantalla amplia con excelente nitidez para estaciones de trabajo."),
                ["description"] = new OpenApiString("Monitor ideal para áreas administrativas y operativas que requieren buena visibilidad y una experiencia cómoda en jornadas extensas."),
                ["price"] = new OpenApiDouble(320.00),
                ["stock"] = new OpenApiInteger(12),
                ["minStock"] = new OpenApiInteger(4),
                ["imagePath"] = new OpenApiString("/images/products/catalog/monitor-27-ips-main.jpg"),
                ["galleryImagePaths"] = new OpenApiArray
                {
                    new OpenApiString("/images/products/catalog/monitor-27-ips-gallery-1.jpg"),
                    new OpenApiString("/images/products/catalog/monitor-27-ips-gallery-2.jpg")
                }
            };
        }

        if (context.Type == typeof(RegisterSaleRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["items"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["productId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                        ["quantity"] = new OpenApiInteger(2)
                    },
                    new OpenApiObject
                    {
                        ["productId"] = new OpenApiString("22222222-2222-2222-2222-222222222222"),
                        ["quantity"] = new OpenApiInteger(1)
                    }
                }
            };
        }

        if (context.Type == typeof(AuthTokenResponse))
        {
            schema.Example = new OpenApiObject
            {
                ["accessToken"] = new OpenApiString("eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9..."),
                ["refreshToken"] = new OpenApiString("wGZ4Jmx0R3N4dVFtUEtkV21ndDd6T0tXR3QxMG1kdHc="),
                ["expiresAt"] = new OpenApiDateTime(DateTime.UtcNow.AddMinutes(20)),
                ["userName"] = new OpenApiString("admin"),
                ["roles"] = new OpenApiArray { new OpenApiString("ADMIN") }
            };
        }

        if (context.Type == typeof(ProductDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("c2c97d12-b6d1-4d5b-8b5b-274fe4b9f2aa"),
                ["name"] = new OpenApiString("Monitor 27 IPS"),
                ["category"] = new OpenApiString("Pantallas"),
                ["brand"] = new OpenApiString("VisionPro"),
                ["shortDescription"] = new OpenApiString("Pantalla amplia con excelente nitidez para estaciones de trabajo."),
                ["description"] = new OpenApiString("Monitor ideal para áreas administrativas y operativas que requieren buena visibilidad y una experiencia cómoda en jornadas extensas."),
                ["price"] = new OpenApiDouble(320.00),
                ["imagePath"] = new OpenApiString("/images/products/catalog/monitor-27-ips-main.jpg"),
                ["galleryImages"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["id"] = new OpenApiString("1f0cb738-f8fd-4c8e-9dd1-b86a8d82aa70"),
                        ["imagePath"] = new OpenApiString("/images/products/catalog/monitor-27-ips-gallery-1.jpg"),
                        ["sortOrder"] = new OpenApiInteger(0)
                    }
                },
                ["stock"] = new OpenApiInteger(7),
                ["minStock"] = new OpenApiInteger(3),
                ["requiresRestock"] = new OpenApiBoolean(false),
                ["isActive"] = new OpenApiBoolean(true),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow.AddDays(-5)),
                ["updatedAt"] = new OpenApiNull()
            };
        }

        if (context.Type == typeof(SaleDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("6b34bf58-1650-4d7e-b6d5-f4c0ea4b1a0a"),
                ["date"] = new OpenApiDateTime(DateTime.UtcNow),
                ["userId"] = new OpenApiString("f4a6e170-6d96-41de-80e3-e5c3f64a64f0"),
                ["total"] = new OpenApiDouble(729.99),
                ["correlationId"] = new OpenApiString("1cb5e89e1ebd46fdbcc1d9e3be3ef6af"),
                ["items"] = new OpenApiArray
                {
                    new OpenApiObject
                    {
                        ["productId"] = new OpenApiString("11111111-1111-1111-1111-111111111111"),
                        ["productName"] = new OpenApiString("Monitor 27 IPS"),
                        ["quantity"] = new OpenApiInteger(2),
                        ["unitPrice"] = new OpenApiDouble(320.00),
                        ["subtotal"] = new OpenApiDouble(640.00)
                    }
                }
            };
        }

        if (context.Type == typeof(CreateUserRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["userName"] = new OpenApiString("maria.operaciones"),
                ["displayName"] = new OpenApiString("María Gómez"),
                ["email"] = new OpenApiString("maria@inventario.local"),
                ["password"] = new OpenApiString("Maria123*"),
                ["role"] = new OpenApiString("EMPLEADO"),
                ["avatarPath"] = new OpenApiString("/uploads/users/avatars/maria-gomez.jpg")
            };
        }

        if (context.Type == typeof(UpdateUserProfileRequest))
        {
            schema.Example = new OpenApiObject
            {
                ["userName"] = new OpenApiString("maria.operaciones"),
                ["displayName"] = new OpenApiString("María Gómez"),
                ["email"] = new OpenApiString("maria@inventario.local"),
                ["avatarPath"] = new OpenApiString("/uploads/users/avatars/maria-gomez.jpg")
            };
        }

        if (context.Type == typeof(UserManagementDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("f4a6e170-6d96-41de-80e3-e5c3f64a64f0"),
                ["userName"] = new OpenApiString("maria.operaciones"),
                ["displayName"] = new OpenApiString("María Gómez"),
                ["email"] = new OpenApiString("maria@inventario.local"),
                ["avatarPath"] = new OpenApiString("/uploads/users/avatars/maria-gomez.jpg"),
                ["role"] = new OpenApiString("EMPLEADO"),
                ["isActive"] = new OpenApiBoolean(true),
                ["failedLoginAttempts"] = new OpenApiInteger(0),
                ["lockoutEndUtc"] = new OpenApiNull(),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow.AddDays(-10))
            };
        }

        if (context.Type == typeof(DashboardMetricsDto))
        {
            schema.Example = new OpenApiObject
            {
                ["totalProducts"] = new OpenApiInteger(25),
                ["totalSales"] = new OpenApiInteger(14),
                ["lowStockProducts"] = new OpenApiInteger(3),
                ["totalSoldToday"] = new OpenApiDouble(1850.50)
            };
        }

        if (context.Type == typeof(AuditLogDto))
        {
            schema.Example = new OpenApiObject
            {
                ["id"] = new OpenApiString("2d5108a4-3e35-4cff-b15a-69fd6005a2f0"),
                ["actionType"] = new OpenApiString("SaleRegistered"),
                ["entityName"] = new OpenApiString("Sale"),
                ["userName"] = new OpenApiString("empleado"),
                ["correlationId"] = new OpenApiString("54e1921d7e284a6eab1dfd0ee2e26f87"),
                ["createdAt"] = new OpenApiDateTime(DateTime.UtcNow)
            };
        }
    }
}
