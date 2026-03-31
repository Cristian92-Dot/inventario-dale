USE [InventarioDb3];




IF OBJECT_ID(N'[AspNetUsers]') IS NULL OR OBJECT_ID(N'[Products]') IS NULL
BEGIN
    THROW 50001, 'Primero ejecuta inventario-db-schema.sql para crear el esquema.', 1;
END;


DECLARE @Now datetime2 = SYSUTCDATETIME();

DECLARE @AdminRoleId nvarchar(450) = N'11111111-1111-1111-1111-111111111111';
DECLARE @EmployeeRoleId nvarchar(450) = N'22222222-2222-2222-2222-222222222222';

DECLARE @AdminUserId nvarchar(450) = N'aaaaaaaa-aaaa-aaaa-aaaa-aaaaaaaaaaaa';
DECLARE @EmployeeUserId nvarchar(450) = N'bbbbbbbb-bbbb-bbbb-bbbb-bbbbbbbbbbbb';

DECLARE @CategoryComputo uniqueidentifier = '10000000-0000-0000-0000-000000000001';
DECLARE @CategoryPantallas uniqueidentifier = '10000000-0000-0000-0000-000000000002';
DECLARE @CategoryPerifericos uniqueidentifier = '10000000-0000-0000-0000-000000000003';
DECLARE @CategoryEscaneo uniqueidentifier = '10000000-0000-0000-0000-000000000004';
DECLARE @CategoryLogistica uniqueidentifier = '10000000-0000-0000-0000-000000000005';

DECLARE @ProductLaptop uniqueidentifier = '20000000-0000-0000-0000-000000000001';
DECLARE @ProductMonitor uniqueidentifier = '20000000-0000-0000-0000-000000000002';
DECLARE @ProductMouse uniqueidentifier = '20000000-0000-0000-0000-000000000003';
DECLARE @ProductTeclado uniqueidentifier = '20000000-0000-0000-0000-000000000004';
DECLARE @ProductScanner uniqueidentifier = '20000000-0000-0000-0000-000000000005';
DECLARE @ProductLector uniqueidentifier = '20000000-0000-0000-0000-000000000006';

DECLARE @Sale01 uniqueidentifier = '30000000-0000-0000-0000-000000000001';
DECLARE @Sale02 uniqueidentifier = '30000000-0000-0000-0000-000000000002';
DECLARE @Sale03 uniqueidentifier = '30000000-0000-0000-0000-000000000003';

BEGIN TRANSACTION;

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = @AdminRoleId)
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (@AdminRoleId, N'ADMIN', N'ADMIN', N'0c8d93e7-90f5-41d5-b7ce-8274a69b64aa');
END;

IF NOT EXISTS (SELECT 1 FROM [AspNetRoles] WHERE [Id] = @EmployeeRoleId)
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (@EmployeeRoleId, N'EMPLEADO', N'EMPLEADO', N'c03b68a2-9b1c-4ff6-a6ef-ef7dc2c9e81f');
END;

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @AdminUserId)
BEGIN
    INSERT INTO [AspNetUsers]
    (
        [Id], [DisplayName], [AvatarPath], [IsActive], [FailedLoginAttempts], [LockoutEndUtc], [CreatedAt],
        [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash],
        [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount]
    )
    VALUES
    (
        @AdminUserId, N'Administrador principal', NULL, 1, 0, NULL, DATEADD(day, -30, @Now),
        N'admin', N'ADMIN', N'admin@inventario.local', N'ADMIN@INVENTARIO.LOCAL', 1,
        N'AQAAAAIAAYagAAAAEFJwyxSIQOqP8X9egllDu/W+55f7E8sA3/QsWDRDhp2KfrOzKnavmvqjgFsn+16+dA==',
        N'17f8f3bd-22b9-45b8-8d5f-1e34cefa70be', N'35e3ef0b-2f08-4f19-a16c-42d640efdffe',
        NULL, 0, 0, NULL, 1, 0
    );
END;

IF NOT EXISTS (SELECT 1 FROM [AspNetUsers] WHERE [Id] = @EmployeeUserId)
BEGIN
    INSERT INTO [AspNetUsers]
    (
        [Id], [DisplayName], [AvatarPath], [IsActive], [FailedLoginAttempts], [LockoutEndUtc], [CreatedAt],
        [UserName], [NormalizedUserName], [Email], [NormalizedEmail], [EmailConfirmed], [PasswordHash],
        [SecurityStamp], [ConcurrencyStamp], [PhoneNumber], [PhoneNumberConfirmed], [TwoFactorEnabled],
        [LockoutEnd], [LockoutEnabled], [AccessFailedCount]
    )
    VALUES
    (
        @EmployeeUserId, N'Usuario operativo', NULL, 1, 0, NULL, DATEADD(day, -29, @Now),
        N'empleado', N'EMPLEADO', N'empleado@inventario.local', N'EMPLEADO@INVENTARIO.LOCAL', 1,
        N'AQAAAAIAAYagAAAAEPv2UdcMkTvupIfACPJzhqilNgDqFBzQUM9xKjcy6+kAfdG6OYw8WnRgBz03PSSDlg==',
        N'18a4e2c1-1f72-4e8d-9bd4-6016debf9a2e', N'9f934fb6-c15c-4332-b26d-fa815be92c47',
        NULL, 0, 0, NULL, 1, 0
    );
END;

IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = @AdminUserId AND [RoleId] = @AdminRoleId)
BEGIN
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
    VALUES (@AdminUserId, @AdminRoleId);
END;

IF NOT EXISTS (SELECT 1 FROM [AspNetUserRoles] WHERE [UserId] = @EmployeeUserId AND [RoleId] = @EmployeeRoleId)
BEGIN
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
    VALUES (@EmployeeUserId, @EmployeeRoleId);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Id] = @CategoryComputo)
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@CategoryComputo, N'Computo', N'Equipos principales para operacion administrativa y analitica.', 0, DATEADD(day, -21, @Now), NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Id] = @CategoryPantallas)
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@CategoryPantallas, N'Pantallas', N'Monitores y soluciones visuales para estaciones de trabajo.', 1, DATEADD(day, -21, @Now), NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Id] = @CategoryPerifericos)
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@CategoryPerifericos, N'Perifericos', N'Accesorios y dispositivos de apoyo para productividad diaria.', 2, DATEADD(day, -21, @Now), NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Id] = @CategoryEscaneo)
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@CategoryEscaneo, N'Escaneo', N'Equipos orientados a digitalizacion y captura documental.', 3, DATEADD(day, -21, @Now), NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Id] = @CategoryLogistica)
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@CategoryLogistica, N'Logistica', N'Herramientas para control de entradas, salidas y trazabilidad.', 4, DATEADD(day, -21, @Now), NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = @ProductLaptop)
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@ProductLaptop, N'Laptop Pro 14', N'Computo', N'DaLE Tech', N'Ultrabook empresarial con gran rendimiento y bateria prolongada.', N'Equipo pensado para operacion intensiva, reuniones, reportes y trabajo de oficina con desempeno estable durante toda la jornada.', 1450.00, N'/images/products/catalog/laptop-pro-14-main.jpg', 11, 4, 0, DATEADD(day, -20, @Now), DATEADD(day, -6, @Now), N'admin', N'empleado', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = @ProductMonitor)
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@ProductMonitor, N'Monitor 27 IPS', N'Pantallas', N'VisionPro', N'Pantalla amplia con excelente nitidez para estaciones de trabajo.', N'Monitor ideal para areas administrativas y operativas que requieren buena visibilidad, color consistente y una experiencia comoda en jornadas extensas.', 320.00, N'/images/products/catalog/monitor-27-ips-main.jpg', 6, 3, 0, DATEADD(day, -20, @Now), DATEADD(day, -3, @Now), N'admin', N'empleado', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = @ProductMouse)
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@ProductMouse, N'Mouse Ergonomico', N'Perifericos', N'ComfortLine', N'Accesorio ergonomico para uso prolongado.', N'Disenado para reducir fatiga en la mano, mejorar precision y acompanar el uso diario en puestos con alta interaccion digital.', 45.00, N'/images/products/catalog/mouse-ergonomico-main.jpg', 16, 6, 0, DATEADD(day, -20, @Now), DATEADD(day, -6, @Now), N'admin', N'empleado', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = @ProductTeclado)
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@ProductTeclado, N'Teclado Mecanico', N'Perifericos', N'KeyForge', N'Teclado robusto con respuesta rapida para operacion diaria.', N'Ofrece durabilidad, respuesta tactil confiable y mejor experiencia de escritura para puestos administrativos y de digitacion frecuente.', 95.00, N'/images/products/catalog/teclado-mecanico-main.jpg', 8, 4, 0, DATEADD(day, -20, @Now), DATEADD(day, -3, @Now), N'admin', N'empleado', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = @ProductScanner)
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@ProductScanner, N'Scanner Industrial', N'Escaneo', N'ScanCore', N'Equipo preparado para procesos de captura intensiva.', N'Pensado para areas de recepcion, inventario y digitalizacion de documentos con ritmos de trabajo exigentes.', 210.00, N'/images/products/catalog/scanner-industrial-main.jpg', 1, 3, 1, DATEADD(day, -20, @Now), DATEADD(day, -1, @Now), N'admin', N'admin', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Id] = @ProductLector)
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES (@ProductLector, N'Lector de Codigos', N'Logistica', N'QuickScan', N'Lector agil para inventario y ventas en punto de operacion.', N'Facilita procesos rapidos de entrada, salida y venta de productos al integrarse con flujos de inventario y trazabilidad.', 160.00, N'/images/products/catalog/lector-codigos-main.jpg', 0, 2, 1, DATEADD(day, -20, @Now), DATEADD(day, -1, @Now), N'admin', N'admin', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [Id] = '21000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES
    ('21000000-0000-0000-0000-000000000001', @ProductLaptop, N'/images/products/catalog/laptop-pro-14-gallery-1.jpg', 0, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000002', @ProductLaptop, N'/images/products/catalog/laptop-pro-14-gallery-2.jpg', 1, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000003', @ProductMonitor, N'/images/products/catalog/monitor-27-ips-gallery-1.jpg', 0, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000004', @ProductMonitor, N'/images/products/catalog/monitor-27-ips-gallery-2.jpg', 1, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000005', @ProductMouse, N'/images/products/catalog/mouse-ergonomico-gallery-1.jpg', 0, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000006', @ProductMouse, N'/images/products/catalog/mouse-ergonomico-gallery-2.jpg', 1, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000007', @ProductTeclado, N'/images/products/catalog/teclado-mecanico-gallery-1.jpg', 0, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000008', @ProductTeclado, N'/images/products/catalog/teclado-mecanico-gallery-2.jpg', 1, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000009', @ProductScanner, N'/images/products/catalog/scanner-industrial-gallery-1.jpg', 0, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000010', @ProductScanner, N'/images/products/catalog/scanner-industrial-gallery-2.jpg', 1, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000011', @ProductLector, N'/images/products/catalog/lector-codigos-gallery-1.jpg', 0, DATEADD(day, -20, @Now), NULL, N'admin', NULL),
    ('21000000-0000-0000-0000-000000000012', @ProductLector, N'/images/products/catalog/lector-codigos-gallery-2.jpg', 1, DATEADD(day, -20, @Now), NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [Sales] WHERE [Id] = @Sale01)
BEGIN
    INSERT INTO [Sales] ([Id], [Date], [UserId], [Total], [CorrelationId], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES (@Sale01, DATEADD(day, -6, @Now), @EmployeeUserId, 1540.00, N'SEED-SALE-001', DATEADD(day, -6, @Now), NULL, N'empleado', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [Sales] WHERE [Id] = @Sale02)
BEGIN
    INSERT INTO [Sales] ([Id], [Date], [UserId], [Total], [CorrelationId], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES (@Sale02, DATEADD(day, -3, @Now), @EmployeeUserId, 415.00, N'SEED-SALE-002', DATEADD(day, -3, @Now), NULL, N'empleado', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [Sales] WHERE [Id] = @Sale03)
BEGIN
    INSERT INTO [Sales] ([Id], [Date], [UserId], [Total], [CorrelationId], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES (@Sale03, DATEADD(day, -1, @Now), @AdminUserId, 370.00, N'SEED-SALE-003', DATEADD(day, -1, @Now), NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [SaleItems] WHERE [Id] = '31000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [SaleItems] ([Id], [SaleId], [ProductId], [Quantity], [UnitPrice], [Subtotal])
    VALUES
    ('31000000-0000-0000-0000-000000000001', @Sale01, @ProductLaptop, 1, 1450.00, 1450.00),
    ('31000000-0000-0000-0000-000000000002', @Sale01, @ProductMouse, 2, 45.00, 90.00),
    ('31000000-0000-0000-0000-000000000003', @Sale02, @ProductMonitor, 1, 320.00, 320.00),
    ('31000000-0000-0000-0000-000000000004', @Sale02, @ProductTeclado, 1, 95.00, 95.00),
    ('31000000-0000-0000-0000-000000000005', @Sale03, @ProductScanner, 1, 210.00, 210.00),
    ('31000000-0000-0000-0000-000000000006', @Sale03, @ProductLector, 1, 160.00, 160.00);
END;

IF NOT EXISTS (SELECT 1 FROM [AuditLogs] WHERE [Id] = '40000000-0000-0000-0000-000000000001')
BEGIN
    INSERT INTO [AuditLogs] ([Id], [UserId], [UserName], [ActionType], [EntityName], [OldValuesJson], [NewValuesJson], [AffectedFields], [IpAddress], [UserAgent], [CorrelationId], [CreatedAt])
    VALUES
    ('40000000-0000-0000-0000-000000000001', @EmployeeUserId, N'empleado', 6, N'Sale', NULL, N'{"Total":1540.00,"CorrelationId":"SEED-SALE-001"}', N'Total,CorrelationId', N'127.0.0.1', N'Seed Script', N'SEED-SALE-001', DATEADD(day, -6, @Now)),
    ('40000000-0000-0000-0000-000000000002', @EmployeeUserId, N'empleado', 6, N'Sale', NULL, N'{"Total":415.00,"CorrelationId":"SEED-SALE-002"}', N'Total,CorrelationId', N'127.0.0.1', N'Seed Script', N'SEED-SALE-002', DATEADD(day, -3, @Now)),
    ('40000000-0000-0000-0000-000000000003', @AdminUserId, N'admin', 6, N'Sale', NULL, N'{"Total":370.00,"CorrelationId":"SEED-SALE-003"}', N'Total,CorrelationId', N'127.0.0.1', N'Seed Script', N'SEED-SALE-003', DATEADD(day, -1, @Now));
END;

COMMIT TRANSACTION;

