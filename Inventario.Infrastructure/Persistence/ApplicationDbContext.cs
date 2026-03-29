using Inventario.Application.Abstractions;
using Inventario.Domain.Entities;
using Inventario.Infrastructure.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System.Text.Json;

namespace Inventario.Infrastructure.Persistence;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, string>, IUnitOfWork
{
    private readonly ICurrentUserService? _currentUserService;
    private readonly IDateTimeProvider? _dateTimeProvider;

    public ApplicationDbContext(
        DbContextOptions<ApplicationDbContext> options,
        ICurrentUserService? currentUserService = null,
        IDateTimeProvider? dateTimeProvider = null)
        : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeProvider = dateTimeProvider;
    }

    public DbSet<Product> Products => Set<Product>();
    public DbSet<ProductCategory> ProductCategories => Set<ProductCategory>();
    public DbSet<ProductGalleryImage> ProductGalleryImages => Set<ProductGalleryImage>();
    public DbSet<Sale> Sales => Set<Sale>();
    public DbSet<SaleItem> SaleItems => Set<SaleItem>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ErrorLog> ErrorLogs => Set<ErrorLog>();
    public DbSet<IdempotencyRecord> IdempotencyRecords => Set<IdempotencyRecord>();
    public DbSet<SecurityEvent> SecurityEvents => Set<SecurityEvent>();
    public DbSet<RequestTrace> RequestTraces => Set<RequestTrace>();
    public DbSet<HttpTransactionLog> HttpTransactionLogs => Set<HttpTransactionLog>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Product>(entity =>
        {
            entity.ToTable("Products");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(150).IsRequired();
            entity.Property(x => x.Category).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Brand).HasMaxLength(100).IsRequired();
            entity.Property(x => x.ShortDescription).HasMaxLength(240);
            entity.Property(x => x.Description).HasMaxLength(2000);
            entity.Property(x => x.Price).HasColumnType("decimal(18,2)");
            entity.Property(x => x.ImagePath).HasMaxLength(300);
            entity.Property(x => x.RowVersion).IsRowVersion();
            entity.HasMany(x => x.GalleryImages)
                .WithOne(x => x.Product)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        builder.Entity<ProductGalleryImage>(entity =>
        {
            entity.ToTable("ProductGalleryImages");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.ImagePath).HasMaxLength(300).IsRequired();
            entity.HasIndex(x => new { x.ProductId, x.SortOrder });
        });

        builder.Entity<ProductCategory>(entity =>
        {
            entity.ToTable("ProductCategories");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Name).HasMaxLength(100).IsRequired();
            entity.Property(x => x.Description).HasMaxLength(240);
            entity.HasIndex(x => x.Name).IsUnique();
        });

        builder.Entity<Sale>(entity =>
        {
            entity.ToTable("Sales");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Total).HasColumnType("decimal(18,2)");
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
        });

        builder.Entity<SaleItem>(entity =>
        {
            entity.ToTable("SaleItems");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.UnitPrice).HasColumnType("decimal(18,2)");
            entity.Property(x => x.Subtotal).HasColumnType("decimal(18,2)");
            entity.HasOne(x => x.Sale)
                .WithMany(x => x.Items)
                .HasForeignKey(x => x.SaleId)
                .OnDelete(DeleteBehavior.Cascade);
            entity.HasOne(x => x.Product)
                .WithMany(x => x.SaleItems)
                .HasForeignKey(x => x.ProductId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        builder.Entity<RefreshToken>(entity =>
        {
            entity.ToTable("RefreshTokens");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Token).HasMaxLength(256).IsRequired();
            entity.Property(x => x.UserId).HasMaxLength(450).IsRequired();
        });

        builder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.EntityName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.AffectedFields).HasMaxLength(1000);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
        });

        builder.Entity<ErrorLog>(entity =>
        {
            entity.ToTable("ErrorLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Path).HasMaxLength(500);
            entity.Property(x => x.Method).HasMaxLength(20);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
        });

        builder.Entity<IdempotencyRecord>(entity =>
        {
            entity.ToTable("IdempotencyRecords");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.IdempotencyKey).HasMaxLength(200).IsRequired();
            entity.Property(x => x.Endpoint).HasMaxLength(500).IsRequired();
            entity.Property(x => x.RequestHash).HasMaxLength(256).IsRequired();
            entity.HasIndex(x => new { x.IdempotencyKey, x.Endpoint, x.UserId }).IsUnique();
        });

        builder.Entity<SecurityEvent>(entity =>
        {
            entity.ToTable("SecurityEvents");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.Message).HasMaxLength(2000).IsRequired();
            entity.Property(x => x.Path).HasMaxLength(500);
            entity.Property(x => x.Method).HasMaxLength(20);
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
        });

        builder.Entity<RequestTrace>(entity =>
        {
            entity.ToTable("RequestTraces");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.TraceId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.CorrelationId).HasMaxLength(100);
            entity.Property(x => x.Path).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Method).HasMaxLength(20).IsRequired();
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.CorrelationId);
        });

        builder.Entity<HttpTransactionLog>(entity =>
        {
            entity.ToTable("HttpTransactionLogs");
            entity.HasKey(x => x.Id);
            entity.Property(x => x.CorrelationId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.TraceId).HasMaxLength(100).IsRequired();
            entity.Property(x => x.EndpointName).HasMaxLength(300);
            entity.Property(x => x.RoutePattern).HasMaxLength(300);
            entity.Property(x => x.Path).HasMaxLength(500).IsRequired();
            entity.Property(x => x.Method).HasMaxLength(20).IsRequired();
            entity.Property(x => x.QueryStringMasked).HasMaxLength(2000);
            entity.Property(x => x.IpAddress).HasMaxLength(64);
            entity.Property(x => x.UserAgent).HasMaxLength(1000);
            entity.Property(x => x.RequestContentType).HasMaxLength(150);
            entity.Property(x => x.ResponseContentType).HasMaxLength(150);
            entity.Property(x => x.RequestFingerprint).HasMaxLength(64);
            entity.Property(x => x.ResponseFingerprint).HasMaxLength(64);
            entity.Property(x => x.RequestCaptureStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.ResponseCaptureStatus).HasMaxLength(50).IsRequired();
            entity.Property(x => x.IdempotencyKeyHash).HasMaxLength(64);
            entity.Property(x => x.ExceptionType).HasMaxLength(300);
            entity.HasIndex(x => x.CreatedAt);
            entity.HasIndex(x => x.CorrelationId);
            entity.HasIndex(x => new { x.Path, x.Method, x.CreatedAt });
            entity.HasIndex(x => x.RequestTraceId).IsUnique();
        });

        builder.Entity<ApplicationUser>(entity =>
        {
            entity.Property(x => x.DisplayName).HasMaxLength(150).IsRequired();
            entity.Property(x => x.AvatarPath).HasMaxLength(300);
        });
    }

    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        var now = _dateTimeProvider?.UtcNow ?? DateTime.UtcNow;
        var requestContext = _currentUserService?.GetCurrent();
        var auditLogs = BuildAuditLogs(now, requestContext);

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is not Domain.Common.AuditableEntity auditableEntity)
            {
                continue;
            }

            if (entry.State == EntityState.Added)
            {
                auditableEntity.CreatedAt = now;
                auditableEntity.CreatedBy ??= requestContext?.UserName ?? "system";
            }

            if (entry.State == EntityState.Modified)
            {
                auditableEntity.UpdatedAt = now;
                auditableEntity.UpdatedBy = requestContext?.UserName ?? "system";
            }
        }

        if (auditLogs.Count > 0)
        {
            AuditLogs.AddRange(auditLogs);
        }

        return await base.SaveChangesAsync(cancellationToken);
    }

    private List<AuditLog> BuildAuditLogs(DateTime now, Inventario.Application.Common.CurrentRequestContext? requestContext)
    {
        var result = new List<AuditLog>();

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.Entity is AuditLog or ErrorLog or IdempotencyRecord or SecurityEvent or RequestTrace or HttpTransactionLog || entry.State == EntityState.Unchanged || entry.State == EntityState.Detached)
            {
                continue;
            }

            if (entry.Entity is not Domain.Common.AuditableEntity && entry.Entity is not SaleItem)
            {
                continue;
            }

            var actionType = entry.State switch
            {
                EntityState.Added => Domain.Enums.AuditActionType.Create,
                EntityState.Modified => Domain.Enums.AuditActionType.Update,
                EntityState.Deleted => Domain.Enums.AuditActionType.Delete,
                _ => Domain.Enums.AuditActionType.Update
            };

            result.Add(new AuditLog
            {
                UserId = requestContext?.UserId,
                UserName = requestContext?.UserName,
                ActionType = entry.Entity is Sale ? Domain.Enums.AuditActionType.SaleRegistered : actionType,
                EntityName = entry.Metadata.ClrType.Name,
                OldValuesJson = SerializeValues(entry.OriginalValues),
                NewValuesJson = SerializeValues(entry.CurrentValues),
                AffectedFields = string.Join(",", entry.Properties.Where(p => p.IsModified).Select(p => p.Metadata.Name)),
                IpAddress = requestContext?.IpAddress,
                UserAgent = requestContext?.UserAgent,
                CorrelationId = requestContext?.CorrelationId,
                CreatedAt = now
            });
        }

        return result;
    }

    private static string? SerializeValues(PropertyValues? values)
    {
        if (values is null)
        {
            return null;
        }

        var dictionary = values.Properties.ToDictionary(p => p.Name, p => values[p]);
        return JsonSerializer.Serialize(dictionary);
    }
}
