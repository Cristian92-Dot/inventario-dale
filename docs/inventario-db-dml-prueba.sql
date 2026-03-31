-- Datos de prueba para SQL Server / InventarioDb
-- Usuarios de acceso:
--   admin / Admin123*
--   empleado / Empleado123*

USE [InventarioDb];



BEGIN TRANSACTION;


DECLARE @AdminRoleId nvarchar(450) = N'role-admin';
DECLARE @EmployeeRoleId nvarchar(450) = N'role-empleado';
DECLARE @AdminUserId nvarchar(450) = N'user-admin';
DECLARE @EmployeeUserId nvarchar(450) = N'user-empleado';

IF NOT EXISTS (
    SELECT 1
    FROM [AspNetRoles]
    WHERE [NormalizedName] = N'ADMIN'
)
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (@AdminRoleId, N'ADMIN', N'ADMIN', N'2d6ddaf1-8d5f-4b23-a6e3-cff46c35c4d1');
END
ELSE
BEGIN
    SELECT @AdminRoleId = [Id]
    FROM [AspNetRoles]
    WHERE [NormalizedName] = N'ADMIN';
END;

IF NOT EXISTS (
    SELECT 1
    FROM [AspNetRoles]
    WHERE [NormalizedName] = N'EMPLEADO'
)
BEGIN
    INSERT INTO [AspNetRoles] ([Id], [Name], [NormalizedName], [ConcurrencyStamp])
    VALUES (@EmployeeRoleId, N'EMPLEADO', N'EMPLEADO', N'61d6f3c0-1982-4ce8-9512-4b33e7176d4b');
END
ELSE
BEGIN
    SELECT @EmployeeRoleId = [Id]
    FROM [AspNetRoles]
    WHERE [NormalizedName] = N'EMPLEADO';
END;

IF NOT EXISTS (
    SELECT 1
    FROM [AspNetUsers]
    WHERE [NormalizedUserName] = N'ADMIN'
)
BEGIN
    INSERT INTO [AspNetUsers] (
        [Id],
        [DisplayName],
        [AvatarPath],
        [IsActive],
        [FailedLoginAttempts],
        [LockoutEndUtc],
        [CreatedAt],
        [UserName],
        [NormalizedUserName],
        [Email],
        [NormalizedEmail],
        [EmailConfirmed],
        [PasswordHash],
        [SecurityStamp],
        [ConcurrencyStamp],
        [PhoneNumber],
        [PhoneNumberConfirmed],
        [TwoFactorEnabled],
        [LockoutEnd],
        [LockoutEnabled],
        [AccessFailedCount]
    )
    VALUES (
        @AdminUserId,
        N'Administrador principal',
        N'/images/avatars/admin.jpg',
        1,
        0,
        NULL,
        '2026-03-31T09:00:00',
        N'admin',
        N'ADMIN',
        N'admin@inventario.local',
        N'ADMIN@INVENTARIO.LOCAL',
        1,
        N'AQAAAAIAAYagAAAAEIrFWvEx5FulQ/wWfHttA86FWBPtHiHMjtvmvymyZdVHaPaJAZ9djVHa22xDWlWD0A==',
        N'be0b90ee-6504-41f2-bdba-63b48fd11762',
        N'447a0984-25b5-4e25-b781-5f94be955feb',
        NULL,
        0,
        0,
        NULL,
        1,
        0
    );
END
ELSE
BEGIN
    SELECT @AdminUserId = [Id]
    FROM [AspNetUsers]
    WHERE [NormalizedUserName] = N'ADMIN';
END;

IF NOT EXISTS (
    SELECT 1
    FROM [AspNetUsers]
    WHERE [NormalizedUserName] = N'EMPLEADO'
)
BEGIN
    INSERT INTO [AspNetUsers] (
        [Id],
        [DisplayName],
        [AvatarPath],
        [IsActive],
        [FailedLoginAttempts],
        [LockoutEndUtc],
        [CreatedAt],
        [UserName],
        [NormalizedUserName],
        [Email],
        [NormalizedEmail],
        [EmailConfirmed],
        [PasswordHash],
        [SecurityStamp],
        [ConcurrencyStamp],
        [PhoneNumber],
        [PhoneNumberConfirmed],
        [TwoFactorEnabled],
        [LockoutEnd],
        [LockoutEnabled],
        [AccessFailedCount]
    )
    VALUES (
        @EmployeeUserId,
        N'Usuario operativo',
        N'/images/avatars/empleado.jpg',
        1,
        0,
        NULL,
        '2026-03-31T09:05:00',
        N'empleado',
        N'EMPLEADO',
        N'empleado@inventario.local',
        N'EMPLEADO@INVENTARIO.LOCAL',
        1,
        N'AQAAAAIAAYagAAAAEMNt1KCsyN/MW5bo3YRJ2KtsOfzqjil1LKbZMquzqXsoxTXiCuVAMq2QM3kRqRz3cg==',
        N'2d080531-dbe8-4dff-ac1e-41a2823d7dca',
        N'b5fd9e72-f8e8-4296-b7df-791c60fa5ca6',
        NULL,
        0,
        0,
        NULL,
        1,
        0
    );
END
ELSE
BEGIN
    SELECT @EmployeeUserId = [Id]
    FROM [AspNetUsers]
    WHERE [NormalizedUserName] = N'EMPLEADO';
END;

IF NOT EXISTS (
    SELECT 1
    FROM [AspNetUserRoles]
    WHERE [UserId] = @AdminUserId
      AND [RoleId] = @AdminRoleId
)
BEGIN
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
    VALUES (@AdminUserId, @AdminRoleId);
END;

IF NOT EXISTS (
    SELECT 1
    FROM [AspNetUserRoles]
    WHERE [UserId] = @EmployeeUserId
      AND [RoleId] = @EmployeeRoleId
)
BEGIN
    INSERT INTO [AspNetUserRoles] ([UserId], [RoleId])
    VALUES (@EmployeeUserId, @EmployeeRoleId);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Name] = N'Computo')
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('b1000000-0000-0000-0000-000000000001', N'Computo', N'Equipos principales para operacion administrativa y analitica.', 0, '2026-03-31T09:10:00', NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Name] = N'Pantallas')
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('b1000000-0000-0000-0000-000000000002', N'Pantallas', N'Monitores y soluciones visuales para estaciones de trabajo.', 1, '2026-03-31T09:11:00', NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Name] = N'Perifericos')
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('b1000000-0000-0000-0000-000000000003', N'Perifericos', N'Accesorios y dispositivos de apoyo para productividad diaria.', 2, '2026-03-31T09:12:00', NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Name] = N'Escaneo')
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('b1000000-0000-0000-0000-000000000004', N'Escaneo', N'Equipos orientados a digitalizacion y captura documental.', 3, '2026-03-31T09:13:00', NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductCategories] WHERE [Name] = N'Logistica')
BEGIN
    INSERT INTO [ProductCategories] ([Id], [Name], [Description], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('b1000000-0000-0000-0000-000000000005', N'Logistica', N'Herramientas para control de entradas, salidas y trazabilidad.', 4, '2026-03-31T09:14:00', NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Name] = N'Laptop Pro 14')
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('a1000000-0000-0000-0000-000000000001', N'Laptop Pro 14', N'Computo', N'DaLE Tech', N'Ultrabook empresarial con gran rendimiento y bateria prolongada.', N'Equipo pensado para operacion intensiva, reuniones, reportes y trabajo de oficina con desempeno estable durante toda la jornada.', 1450.00, N'/images/products/catalog/laptop-pro-14-main.jpg', 11, 4, 0, '2026-03-31T09:20:00', '2026-03-31T11:05:00', N'admin', N'admin', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Name] = N'Monitor 27 IPS')
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('a1000000-0000-0000-0000-000000000002', N'Monitor 27 IPS', N'Pantallas', N'VisionPro', N'Pantalla amplia con excelente nitidez para estaciones de trabajo.', N'Monitor ideal para areas administrativas y operativas que requieren buena visibilidad, color consistente y una experiencia comoda en jornadas extensas.', 320.00, N'/images/products/catalog/monitor-27-ips-main.jpg', 6, 3, 0, '2026-03-31T09:21:00', '2026-03-31T11:05:00', N'admin', N'admin', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Name] = N'Mouse Ergonomico')
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('a1000000-0000-0000-0000-000000000003', N'Mouse Ergonomico', N'Perifericos', N'ComfortLine', N'Accesorio ergonomico para uso prolongado.', N'Disenado para reducir fatiga en la mano, mejorar precision y acompanar el uso diario en puestos con alta interaccion digital.', 45.00, N'/images/products/catalog/mouse-ergonomico-main.jpg', 16, 6, 0, '2026-03-31T09:22:00', '2026-03-31T10:40:00', N'admin', N'empleado', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Name] = N'Teclado Mecanico')
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('a1000000-0000-0000-0000-000000000004', N'Teclado Mecanico', N'Perifericos', N'KeyForge', N'Teclado robusto con respuesta rapida para operacion diaria.', N'Ofrece durabilidad, respuesta tactil confiable y mejor experiencia de escritura para puestos administrativos y de digitacion frecuente.', 95.00, N'/images/products/catalog/teclado-mecanico-main.jpg', 8, 4, 0, '2026-03-31T09:23:00', '2026-03-31T11:05:00', N'admin', N'admin', 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Name] = N'Scanner Industrial')
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('a1000000-0000-0000-0000-000000000005', N'Scanner Industrial', N'Escaneo', N'ScanCore', N'Equipo preparado para procesos de captura intensiva.', N'Pensado para areas de recepcion, inventario y digitalizacion de documentos con ritmos de trabajo exigentes.', 210.00, N'/images/products/catalog/scanner-industrial-main.jpg', 2, 3, 1, '2026-03-31T09:24:00', NULL, N'admin', NULL, 1);
END;

IF NOT EXISTS (SELECT 1 FROM [Products] WHERE [Name] = N'Lector de Codigos')
BEGIN
    INSERT INTO [Products] ([Id], [Name], [Category], [Brand], [ShortDescription], [Description], [Price], [ImagePath], [Stock], [MinStock], [RequiresRestock], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy], [IsActive])
    VALUES ('a1000000-0000-0000-0000-000000000006', N'Lector de Codigos', N'Logistica', N'QuickScan', N'Lector agil para inventario y ventas en punto de operacion.', N'Facilita procesos rapidos de entrada, salida y venta de productos al integrarse con flujos de inventario y trazabilidad.', 160.00, N'/images/products/catalog/lector-codigos-main.jpg', 0, 2, 1, '2026-03-31T09:25:00', '2026-03-31T10:40:00', N'admin', N'empleado', 1);
END;

DECLARE @LaptopId uniqueidentifier;
DECLARE @MonitorId uniqueidentifier;
DECLARE @MouseId uniqueidentifier;
DECLARE @KeyboardId uniqueidentifier;
DECLARE @ScannerId uniqueidentifier;
DECLARE @ReaderId uniqueidentifier;

SELECT @LaptopId = [Id] FROM [Products] WHERE [Name] = N'Laptop Pro 14';
SELECT @MonitorId = [Id] FROM [Products] WHERE [Name] = N'Monitor 27 IPS';
SELECT @MouseId = [Id] FROM [Products] WHERE [Name] = N'Mouse Ergonomico';
SELECT @KeyboardId = [Id] FROM [Products] WHERE [Name] = N'Teclado Mecanico';
SELECT @ScannerId = [Id] FROM [Products] WHERE [Name] = N'Scanner Industrial';
SELECT @ReaderId = [Id] FROM [Products] WHERE [Name] = N'Lector de Codigos';

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @LaptopId AND [ImagePath] = N'/images/products/catalog/laptop-pro-14-gallery-1.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000001', @LaptopId, N'/images/products/catalog/laptop-pro-14-gallery-1.jpg', 0, '2026-03-31T09:30:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @LaptopId AND [ImagePath] = N'/images/products/catalog/laptop-pro-14-gallery-2.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000002', @LaptopId, N'/images/products/catalog/laptop-pro-14-gallery-2.jpg', 1, '2026-03-31T09:30:01', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @MonitorId AND [ImagePath] = N'/images/products/catalog/monitor-27-ips-gallery-1.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000003', @MonitorId, N'/images/products/catalog/monitor-27-ips-gallery-1.jpg', 0, '2026-03-31T09:31:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @MonitorId AND [ImagePath] = N'/images/products/catalog/monitor-27-ips-gallery-2.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000004', @MonitorId, N'/images/products/catalog/monitor-27-ips-gallery-2.jpg', 1, '2026-03-31T09:31:01', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @MouseId AND [ImagePath] = N'/images/products/catalog/mouse-ergonomico-gallery-1.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000005', @MouseId, N'/images/products/catalog/mouse-ergonomico-gallery-1.jpg', 0, '2026-03-31T09:32:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @MouseId AND [ImagePath] = N'/images/products/catalog/mouse-ergonomico-gallery-2.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000006', @MouseId, N'/images/products/catalog/mouse-ergonomico-gallery-2.jpg', 1, '2026-03-31T09:32:01', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @KeyboardId AND [ImagePath] = N'/images/products/catalog/teclado-mecanico-gallery-1.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000007', @KeyboardId, N'/images/products/catalog/teclado-mecanico-gallery-1.jpg', 0, '2026-03-31T09:33:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @KeyboardId AND [ImagePath] = N'/images/products/catalog/teclado-mecanico-gallery-2.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000008', @KeyboardId, N'/images/products/catalog/teclado-mecanico-gallery-2.jpg', 1, '2026-03-31T09:33:01', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @ScannerId AND [ImagePath] = N'/images/products/catalog/scanner-industrial-gallery-1.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000009', @ScannerId, N'/images/products/catalog/scanner-industrial-gallery-1.jpg', 0, '2026-03-31T09:34:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @ScannerId AND [ImagePath] = N'/images/products/catalog/scanner-industrial-gallery-2.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000010', @ScannerId, N'/images/products/catalog/scanner-industrial-gallery-2.jpg', 1, '2026-03-31T09:34:01', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @ReaderId AND [ImagePath] = N'/images/products/catalog/lector-codigos-gallery-1.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000011', @ReaderId, N'/images/products/catalog/lector-codigos-gallery-1.jpg', 0, '2026-03-31T09:35:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [ProductGalleryImages] WHERE [ProductId] = @ReaderId AND [ImagePath] = N'/images/products/catalog/lector-codigos-gallery-2.jpg')
BEGIN
    INSERT INTO [ProductGalleryImages] ([Id], [ProductId], [ImagePath], [SortOrder], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('c1000000-0000-0000-0000-000000000012', @ReaderId, N'/images/products/catalog/lector-codigos-gallery-2.jpg', 1, '2026-03-31T09:35:01', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [Sales] WHERE [CorrelationId] = N'SALE-20260331-0001')
BEGIN
    INSERT INTO [Sales] ([Id], [Date], [UserId], [Total], [CorrelationId], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('d1000000-0000-0000-0000-000000000001', '2026-03-31T10:40:00', @EmployeeUserId, 250.00, N'SALE-20260331-0001', '2026-03-31T10:40:00', NULL, N'empleado', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [Sales] WHERE [CorrelationId] = N'SALE-20260331-0002')
BEGIN
    INSERT INTO [Sales] ([Id], [Date], [UserId], [Total], [CorrelationId], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('d1000000-0000-0000-0000-000000000002', '2026-03-31T11:05:00', @AdminUserId, 1865.00, N'SALE-20260331-0002', '2026-03-31T11:05:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (SELECT 1 FROM [SaleItems] WHERE [SaleId] = 'd1000000-0000-0000-0000-000000000001' AND [ProductId] = @ReaderId)
BEGIN
    INSERT INTO [SaleItems] ([Id], [SaleId], [ProductId], [Quantity], [UnitPrice], [Subtotal])
    VALUES ('e1000000-0000-0000-0000-000000000001', 'd1000000-0000-0000-0000-000000000001', @ReaderId, 1, 160.00, 160.00);
END;

IF NOT EXISTS (SELECT 1 FROM [SaleItems] WHERE [SaleId] = 'd1000000-0000-0000-0000-000000000001' AND [ProductId] = @MouseId)
BEGIN
    INSERT INTO [SaleItems] ([Id], [SaleId], [ProductId], [Quantity], [UnitPrice], [Subtotal])
    VALUES ('e1000000-0000-0000-0000-000000000002', 'd1000000-0000-0000-0000-000000000001', @MouseId, 2, 45.00, 90.00);
END;

IF NOT EXISTS (SELECT 1 FROM [SaleItems] WHERE [SaleId] = 'd1000000-0000-0000-0000-000000000002' AND [ProductId] = @LaptopId)
BEGIN
    INSERT INTO [SaleItems] ([Id], [SaleId], [ProductId], [Quantity], [UnitPrice], [Subtotal])
    VALUES ('e1000000-0000-0000-0000-000000000003', 'd1000000-0000-0000-0000-000000000002', @LaptopId, 1, 1450.00, 1450.00);
END;

IF NOT EXISTS (SELECT 1 FROM [SaleItems] WHERE [SaleId] = 'd1000000-0000-0000-0000-000000000002' AND [ProductId] = @MonitorId)
BEGIN
    INSERT INTO [SaleItems] ([Id], [SaleId], [ProductId], [Quantity], [UnitPrice], [Subtotal])
    VALUES ('e1000000-0000-0000-0000-000000000004', 'd1000000-0000-0000-0000-000000000002', @MonitorId, 1, 320.00, 320.00);
END;

IF NOT EXISTS (SELECT 1 FROM [SaleItems] WHERE [SaleId] = 'd1000000-0000-0000-0000-000000000002' AND [ProductId] = @KeyboardId)
BEGIN
    INSERT INTO [SaleItems] ([Id], [SaleId], [ProductId], [Quantity], [UnitPrice], [Subtotal])
    VALUES ('e1000000-0000-0000-0000-000000000005', 'd1000000-0000-0000-0000-000000000002', @KeyboardId, 1, 95.00, 95.00);
END;

IF NOT EXISTS (SELECT 1 FROM [RefreshTokens] WHERE [Token] = N'refresh-token-admin-demo-20260331')
BEGIN
    INSERT INTO [RefreshTokens] ([Id], [UserId], [Token], [ExpiresAt], [RevokedAt], [CreatedByIp], [CreatedAt], [UpdatedAt], [CreatedBy], [UpdatedBy])
    VALUES ('f1000000-0000-0000-0000-000000000001', @AdminUserId, N'refresh-token-admin-demo-20260331', '2026-04-15T12:00:00', NULL, N'127.0.0.1', '2026-03-31T11:10:00', NULL, N'admin', NULL);
END;

IF NOT EXISTS (
    SELECT 1
    FROM [IdempotencyRecords]
    WHERE [IdempotencyKey] = N'idem-sale-20260331-0002'
      AND [Endpoint] = N'/api/sales'
      AND [UserId] = @AdminUserId
)
BEGIN
    INSERT INTO [IdempotencyRecords] ([Id], [IdempotencyKey], [UserId], [Endpoint], [RequestHash], [ResponseBody], [StatusCode], [CreatedAt], [ExpiresAt])
    VALUES ('f1000000-0000-0000-0000-000000000002', N'idem-sale-20260331-0002', @AdminUserId, N'/api/sales', N'2f6459e17d7a9ec7fb8c3c4df42f4bf94476c4fd6f4517488db2d5b55b1ad880', N'{"saleId":"d1000000-0000-0000-0000-000000000002","total":1865.00}', 201, '2026-03-31T11:05:00', '2026-04-01T11:05:00');
END;

IF NOT EXISTS (SELECT 1 FROM [RequestTraces] WHERE [TraceId] = N'trace-20260331-0002')
BEGIN
    INSERT INTO [RequestTraces] ([Id], [TraceId], [CorrelationId], [UserId], [UserName], [IpAddress], [UserAgent], [Path], [Method], [StatusCode], [DurationMs], [CreatedAt])
    VALUES ('f1000000-0000-0000-0000-000000000003', N'trace-20260331-0002', N'SALE-20260331-0002', @AdminUserId, N'admin', N'127.0.0.1', N'SwaggerClient/1.0', N'/api/sales', N'POST', 201, 182, '2026-03-31T11:05:00');
END;

IF NOT EXISTS (SELECT 1 FROM [HttpTransactionLogs] WHERE [RequestTraceId] = 'f1000000-0000-0000-0000-000000000003')
BEGIN
    INSERT INTO [HttpTransactionLogs] (
        [Id],
        [RequestTraceId],
        [CorrelationId],
        [TraceId],
        [EndpointName],
        [RoutePattern],
        [Path],
        [Method],
        [QueryStringMasked],
        [UserId],
        [UserName],
        [IpAddress],
        [UserAgent],
        [RequestContentType],
        [RequestSizeBytes],
        [RequestHeadersJson],
        [RequestBodyMasked],
        [RequestFingerprint],
        [RequestCaptureStatus],
        [IsRequestTruncated],
        [ResponseContentType],
        [ResponseSizeBytes],
        [ResponseHeadersJson],
        [ResponseBodyMasked],
        [ResponseFingerprint],
        [ResponseCaptureStatus],
        [IsResponseTruncated],
        [StatusCode],
        [DurationMs],
        [IsIdempotencyReplay],
        [IdempotencyKeyHash],
        [HasSecurityEvent],
        [ExceptionType],
        [CreatedAt]
    )
    VALUES (
        'f1000000-0000-0000-0000-000000000004',
        'f1000000-0000-0000-0000-000000000003',
        N'SALE-20260331-0002',
        N'trace-20260331-0002',
        N'Registrar venta',
        N'/api/sales',
        N'/api/sales',
        N'POST',
        NULL,
        @AdminUserId,
        N'admin',
        N'127.0.0.1',
        N'SwaggerClient/1.0',
        N'application/json',
        248,
        N'{"Content-Type":"application/json","X-Correlation-Id":"SALE-20260331-0002"}',
        N'{"items":[{"product":"Laptop Pro 14","quantity":1},{"product":"Monitor 27 IPS","quantity":1},{"product":"Teclado Mecanico","quantity":1}]}',
        N'ef31c9c45582d7ee7fa4c2f8bfd294d29584fba98ba88f64b7e2d7c4c7440704',
        N'captured',
        0,
        N'application/json',
        94,
        N'{"Content-Type":"application/json"}',
        N'{"saleId":"d1000000-0000-0000-0000-000000000002","total":1865.00}',
        N'4d7cefd5c89af4de6a1f13d265b4c8cc4705373f0ce6ca75ab25d91df4fd4d30',
        N'captured',
        0,
        201,
        182,
        0,
        N'4d6ea6e7db7dc3ffdbf8cf0b67f67958b8063ff9415f18f76363dbda0d2ed31c',
        0,
        NULL,
        '2026-03-31T11:05:00'
    );
END;

IF NOT EXISTS (
    SELECT 1
    FROM [SecurityEvents]
    WHERE [CorrelationId] = N'LOGIN-20260331-0001'
      AND [Message] = N'Inicio de sesion exitoso del usuario admin.'
)
BEGIN
    INSERT INTO [SecurityEvents] ([Id], [EventType], [Severity], [Message], [UserId], [UserName], [IpAddress], [UserAgent], [Path], [Method], [CorrelationId], [CreatedAt])
    VALUES ('f1000000-0000-0000-0000-000000000005', 1, 1, N'Inicio de sesion exitoso del usuario admin.', @AdminUserId, N'admin', N'127.0.0.1', N'SwaggerClient/1.0', N'/api/auth/login', N'POST', N'LOGIN-20260331-0001', '2026-03-31T08:59:00');
END;

IF NOT EXISTS (
    SELECT 1
    FROM [AuditLogs]
    WHERE [CorrelationId] = N'SALE-20260331-0002'
      AND [EntityName] = N'Sale'
      AND [ActionType] = 6
)
BEGIN
    INSERT INTO [AuditLogs] ([Id], [UserId], [UserName], [ActionType], [EntityName], [OldValuesJson], [NewValuesJson], [AffectedFields], [IpAddress], [UserAgent], [CorrelationId], [CreatedAt])
    VALUES ('f1000000-0000-0000-0000-000000000006', @AdminUserId, N'admin', 6, N'Sale', NULL, N'{"Id":"d1000000-0000-0000-0000-000000000002","Total":1865.00}', N'Total,Items', N'127.0.0.1', N'SwaggerClient/1.0', N'SALE-20260331-0002', '2026-03-31T11:05:00');
END;

IF NOT EXISTS (
    SELECT 1
    FROM [ErrorLogs]
    WHERE [CorrelationId] = N'ERR-20260331-0001'
      AND [Path] = N'/api/products/stock-adjustment'
)
BEGIN
    INSERT INTO [ErrorLogs] ([Id], [Message], [StackTrace], [Source], [Path], [Method], [UserId], [CorrelationId], [CreatedAt])
    VALUES ('f1000000-0000-0000-0000-000000000007', N'Intento de ajuste con cantidad invalida.', N'InvalidOperationException: La cantidad debe ser mayor que cero.', N'Inventario.Application.Services.ProductService', N'/api/products/stock-adjustment', N'POST', @EmployeeUserId, N'ERR-20260331-0001', '2026-03-31T11:20:00');
END;

COMMIT;


