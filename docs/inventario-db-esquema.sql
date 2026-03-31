IF DB_ID(N'InventarioDb') IS NULL
BEGIN
    CREATE DATABASE [InventarioDb];
END;


USE [InventarioDb];


IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;


BEGIN TRANSACTION;


CREATE TABLE [AspNetRoles] (
    [Id] nvarchar(450) NOT NULL,
    [Name] nvarchar(256) NULL,
    [NormalizedName] nvarchar(256) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoles] PRIMARY KEY ([Id])
);


CREATE TABLE [AspNetUsers] (
    [Id] nvarchar(450) NOT NULL,
    [IsActive] bit NOT NULL,
    [FailedLoginAttempts] int NOT NULL,
    [LockoutEndUtc] datetime2 NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UserName] nvarchar(256) NULL,
    [NormalizedUserName] nvarchar(256) NULL,
    [Email] nvarchar(256) NULL,
    [NormalizedEmail] nvarchar(256) NULL,
    [EmailConfirmed] bit NOT NULL,
    [PasswordHash] nvarchar(max) NULL,
    [SecurityStamp] nvarchar(max) NULL,
    [ConcurrencyStamp] nvarchar(max) NULL,
    [PhoneNumber] nvarchar(max) NULL,
    [PhoneNumberConfirmed] bit NOT NULL,
    [TwoFactorEnabled] bit NOT NULL,
    [LockoutEnd] datetimeoffset NULL,
    [LockoutEnabled] bit NOT NULL,
    [AccessFailedCount] int NOT NULL,
    CONSTRAINT [PK_AspNetUsers] PRIMARY KEY ([Id])
);


CREATE TABLE [AuditLogs] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] nvarchar(max) NULL,
    [UserName] nvarchar(max) NULL,
    [ActionType] int NOT NULL,
    [EntityName] nvarchar(150) NOT NULL,
    [OldValuesJson] nvarchar(max) NULL,
    [NewValuesJson] nvarchar(max) NULL,
    [AffectedFields] nvarchar(1000) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [CorrelationId] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_AuditLogs] PRIMARY KEY ([Id])
);


CREATE TABLE [ErrorLogs] (
    [Id] uniqueidentifier NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [StackTrace] nvarchar(max) NULL,
    [Source] nvarchar(max) NULL,
    [Path] nvarchar(500) NULL,
    [Method] nvarchar(20) NULL,
    [UserId] nvarchar(max) NULL,
    [CorrelationId] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_ErrorLogs] PRIMARY KEY ([Id])
);


CREATE TABLE [IdempotencyRecords] (
    [Id] uniqueidentifier NOT NULL,
    [IdempotencyKey] nvarchar(200) NOT NULL,
    [UserId] nvarchar(450) NULL,
    [Endpoint] nvarchar(500) NOT NULL,
    [RequestHash] nvarchar(256) NOT NULL,
    [ResponseBody] nvarchar(max) NOT NULL,
    [StatusCode] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    CONSTRAINT [PK_IdempotencyRecords] PRIMARY KEY ([Id])
);


CREATE TABLE [Products] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(150) NOT NULL,
    [Price] decimal(18,2) NOT NULL,
    [Stock] int NOT NULL,
    [MinStock] int NOT NULL,
    [RequiresRestock] bit NOT NULL,
    [RowVersion] rowversion NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_Products] PRIMARY KEY ([Id])
);


CREATE TABLE [RefreshTokens] (
    [Id] uniqueidentifier NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Token] nvarchar(256) NOT NULL,
    [ExpiresAt] datetime2 NOT NULL,
    [RevokedAt] datetime2 NULL,
    [CreatedByIp] nvarchar(max) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_RefreshTokens] PRIMARY KEY ([Id])
);


CREATE TABLE [Sales] (
    [Id] uniqueidentifier NOT NULL,
    [Date] datetime2 NOT NULL,
    [UserId] nvarchar(450) NOT NULL,
    [Total] decimal(18,2) NOT NULL,
    [CorrelationId] nvarchar(100) NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_Sales] PRIMARY KEY ([Id])
);


CREATE TABLE [AspNetRoleClaims] (
    [Id] int NOT NULL IDENTITY,
    [RoleId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetRoleClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetRoleClaims_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE
);


CREATE TABLE [AspNetUserClaims] (
    [Id] int NOT NULL IDENTITY,
    [UserId] nvarchar(450) NOT NULL,
    [ClaimType] nvarchar(max) NULL,
    [ClaimValue] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserClaims] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_AspNetUserClaims_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);


CREATE TABLE [AspNetUserLogins] (
    [LoginProvider] nvarchar(450) NOT NULL,
    [ProviderKey] nvarchar(450) NOT NULL,
    [ProviderDisplayName] nvarchar(max) NULL,
    [UserId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserLogins] PRIMARY KEY ([LoginProvider], [ProviderKey]),
    CONSTRAINT [FK_AspNetUserLogins_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);


CREATE TABLE [AspNetUserRoles] (
    [UserId] nvarchar(450) NOT NULL,
    [RoleId] nvarchar(450) NOT NULL,
    CONSTRAINT [PK_AspNetUserRoles] PRIMARY KEY ([UserId], [RoleId]),
    CONSTRAINT [FK_AspNetUserRoles_AspNetRoles_RoleId] FOREIGN KEY ([RoleId]) REFERENCES [AspNetRoles] ([Id]) ON DELETE CASCADE,
    CONSTRAINT [FK_AspNetUserRoles_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);


CREATE TABLE [AspNetUserTokens] (
    [UserId] nvarchar(450) NOT NULL,
    [LoginProvider] nvarchar(450) NOT NULL,
    [Name] nvarchar(450) NOT NULL,
    [Value] nvarchar(max) NULL,
    CONSTRAINT [PK_AspNetUserTokens] PRIMARY KEY ([UserId], [LoginProvider], [Name]),
    CONSTRAINT [FK_AspNetUserTokens_AspNetUsers_UserId] FOREIGN KEY ([UserId]) REFERENCES [AspNetUsers] ([Id]) ON DELETE CASCADE
);


CREATE TABLE [SaleItems] (
    [Id] uniqueidentifier NOT NULL,
    [SaleId] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [Quantity] int NOT NULL,
    [UnitPrice] decimal(18,2) NOT NULL,
    [Subtotal] decimal(18,2) NOT NULL,
    CONSTRAINT [PK_SaleItems] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_SaleItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE NO ACTION,
    CONSTRAINT [FK_SaleItems_Sales_SaleId] FOREIGN KEY ([SaleId]) REFERENCES [Sales] ([Id]) ON DELETE CASCADE
);


CREATE INDEX [IX_AspNetRoleClaims_RoleId] ON [AspNetRoleClaims] ([RoleId]);


CREATE UNIQUE INDEX [RoleNameIndex] ON [AspNetRoles] ([NormalizedName]) WHERE [NormalizedName] IS NOT NULL;


CREATE INDEX [IX_AspNetUserClaims_UserId] ON [AspNetUserClaims] ([UserId]);


CREATE INDEX [IX_AspNetUserLogins_UserId] ON [AspNetUserLogins] ([UserId]);


CREATE INDEX [IX_AspNetUserRoles_RoleId] ON [AspNetUserRoles] ([RoleId]);


CREATE INDEX [EmailIndex] ON [AspNetUsers] ([NormalizedEmail]);


CREATE UNIQUE INDEX [UserNameIndex] ON [AspNetUsers] ([NormalizedUserName]) WHERE [NormalizedUserName] IS NOT NULL;


CREATE UNIQUE INDEX [IX_IdempotencyRecords_IdempotencyKey_Endpoint_UserId] ON [IdempotencyRecords] ([IdempotencyKey], [Endpoint], [UserId]) WHERE [UserId] IS NOT NULL;


CREATE INDEX [IX_SaleItems_ProductId] ON [SaleItems] ([ProductId]);


CREATE INDEX [IX_SaleItems_SaleId] ON [SaleItems] ([SaleId]);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260328025227_InitialCreate', N'8.0.13');


COMMIT;


BEGIN TRANSACTION;


CREATE TABLE [RequestTraces] (
    [Id] uniqueidentifier NOT NULL,
    [TraceId] nvarchar(100) NOT NULL,
    [CorrelationId] nvarchar(100) NULL,
    [UserId] nvarchar(max) NULL,
    [UserName] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Path] nvarchar(500) NOT NULL,
    [Method] nvarchar(20) NOT NULL,
    [StatusCode] int NOT NULL,
    [DurationMs] bigint NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_RequestTraces] PRIMARY KEY ([Id])
);


CREATE TABLE [SecurityEvents] (
    [Id] uniqueidentifier NOT NULL,
    [EventType] int NOT NULL,
    [Severity] int NOT NULL,
    [Message] nvarchar(2000) NOT NULL,
    [UserId] nvarchar(max) NULL,
    [UserName] nvarchar(max) NULL,
    [IpAddress] nvarchar(max) NULL,
    [UserAgent] nvarchar(max) NULL,
    [Path] nvarchar(500) NULL,
    [Method] nvarchar(20) NULL,
    [CorrelationId] nvarchar(100) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_SecurityEvents] PRIMARY KEY ([Id])
);


CREATE INDEX [IX_RequestTraces_CorrelationId] ON [RequestTraces] ([CorrelationId]);


CREATE INDEX [IX_RequestTraces_CreatedAt] ON [RequestTraces] ([CreatedAt]);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260328034241_AddSecurityEventsAndRequestTraces', N'8.0.13');


COMMIT;


BEGIN TRANSACTION;


CREATE TABLE [HttpTransactionLogs] (
    [Id] uniqueidentifier NOT NULL,
    [RequestTraceId] uniqueidentifier NULL,
    [CorrelationId] nvarchar(100) NOT NULL,
    [TraceId] nvarchar(100) NOT NULL,
    [EndpointName] nvarchar(300) NULL,
    [RoutePattern] nvarchar(300) NULL,
    [Path] nvarchar(500) NOT NULL,
    [Method] nvarchar(20) NOT NULL,
    [QueryStringMasked] nvarchar(2000) NULL,
    [UserId] nvarchar(max) NULL,
    [UserName] nvarchar(max) NULL,
    [IpAddress] nvarchar(64) NULL,
    [UserAgent] nvarchar(1000) NULL,
    [RequestContentType] nvarchar(150) NULL,
    [RequestSizeBytes] bigint NULL,
    [RequestHeadersJson] nvarchar(max) NULL,
    [RequestBodyMasked] nvarchar(max) NULL,
    [RequestFingerprint] nvarchar(64) NULL,
    [RequestCaptureStatus] nvarchar(50) NOT NULL,
    [IsRequestTruncated] bit NOT NULL,
    [ResponseContentType] nvarchar(150) NULL,
    [ResponseSizeBytes] bigint NULL,
    [ResponseHeadersJson] nvarchar(max) NULL,
    [ResponseBodyMasked] nvarchar(max) NULL,
    [ResponseFingerprint] nvarchar(64) NULL,
    [ResponseCaptureStatus] nvarchar(50) NOT NULL,
    [IsResponseTruncated] bit NOT NULL,
    [StatusCode] int NOT NULL,
    [DurationMs] bigint NOT NULL,
    [IsIdempotencyReplay] bit NOT NULL,
    [IdempotencyKeyHash] nvarchar(64) NULL,
    [HasSecurityEvent] bit NOT NULL,
    [ExceptionType] nvarchar(300) NULL,
    [CreatedAt] datetime2 NOT NULL,
    CONSTRAINT [PK_HttpTransactionLogs] PRIMARY KEY ([Id])
);


CREATE INDEX [IX_HttpTransactionLogs_CorrelationId] ON [HttpTransactionLogs] ([CorrelationId]);


CREATE INDEX [IX_HttpTransactionLogs_CreatedAt] ON [HttpTransactionLogs] ([CreatedAt]);


CREATE INDEX [IX_HttpTransactionLogs_Path_Method_CreatedAt] ON [HttpTransactionLogs] ([Path], [Method], [CreatedAt]);


CREATE UNIQUE INDEX [IX_HttpTransactionLogs_RequestTraceId] ON [HttpTransactionLogs] ([RequestTraceId]) WHERE [RequestTraceId] IS NOT NULL;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260329012237_AddHttpTransactionLogs', N'8.0.13');


COMMIT;


BEGIN TRANSACTION;


ALTER TABLE [Products] ADD [ImagePath] nvarchar(300) NULL;


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260329144248_AddProductImageSupport', N'8.0.13');


COMMIT;


BEGIN TRANSACTION;


ALTER TABLE [Products] ADD [Brand] nvarchar(100) NOT NULL DEFAULT N'';


ALTER TABLE [Products] ADD [Category] nvarchar(100) NOT NULL DEFAULT N'';


ALTER TABLE [Products] ADD [Description] nvarchar(2000) NULL;


ALTER TABLE [Products] ADD [ShortDescription] nvarchar(240) NULL;


CREATE TABLE [ProductGalleryImages] (
    [Id] uniqueidentifier NOT NULL,
    [ProductId] uniqueidentifier NOT NULL,
    [ImagePath] nvarchar(300) NOT NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    CONSTRAINT [PK_ProductGalleryImages] PRIMARY KEY ([Id]),
    CONSTRAINT [FK_ProductGalleryImages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [Products] ([Id]) ON DELETE CASCADE
);


CREATE INDEX [IX_ProductGalleryImages_ProductId_SortOrder] ON [ProductGalleryImages] ([ProductId], [SortOrder]);



                UPDATE Products
                SET Category = CASE WHEN Category = '' THEN 'General' ELSE Category END,
                    Brand = CASE WHEN Brand = '' THEN 'Inventario' ELSE Brand END,
                    ShortDescription = CASE WHEN ShortDescription IS NULL THEN 'Producto migrado al catálogo enriquecido.' ELSE ShortDescription END
            


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260329145200_AddProductCatalogEnhancements', N'8.0.13');


COMMIT;


BEGIN TRANSACTION;


CREATE TABLE [ProductCategories] (
    [Id] uniqueidentifier NOT NULL,
    [Name] nvarchar(100) NOT NULL,
    [Description] nvarchar(240) NULL,
    [SortOrder] int NOT NULL,
    [CreatedAt] datetime2 NOT NULL,
    [UpdatedAt] datetime2 NULL,
    [CreatedBy] nvarchar(max) NULL,
    [UpdatedBy] nvarchar(max) NULL,
    [IsActive] bit NOT NULL,
    CONSTRAINT [PK_ProductCategories] PRIMARY KEY ([Id])
);


CREATE UNIQUE INDEX [IX_ProductCategories_Name] ON [ProductCategories] ([Name]);


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260329150058_AddProductCategoryMaster', N'8.0.13');


COMMIT;


BEGIN TRANSACTION;


ALTER TABLE [AspNetUsers] ADD [AvatarPath] nvarchar(300) NULL;


ALTER TABLE [AspNetUsers] ADD [DisplayName] nvarchar(150) NOT NULL DEFAULT N'';



                UPDATE AspNetUsers
                SET DisplayName = CASE
                    WHEN DisplayName = '' AND UserName IS NOT NULL THEN UserName
                    ELSE DisplayName
                END
            


INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
VALUES (N'20260329183104_AddUserProfileFields', N'8.0.13');


COMMIT;


