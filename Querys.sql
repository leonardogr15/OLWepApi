/* ============================================================
   01 - CREACIÓN DE TABLAS
   Descripción: Se crean las tablas Usuario, Comerciante y Establecimiento.
   Se definen los identificadores como columnas de identidad y se
   incluyen los campos de auditoría en Comerciante y Establecimiento.
============================================================ */

-- Tabla de Usuarios
CREATE TABLE dbo.Usuario
(
    IdUsuario INT IDENTITY(1,1) PRIMARY KEY,
    Nombre NVARCHAR(100) NOT NULL,
    CorreoElectronico NVARCHAR(150) NOT NULL,
    Contraseña NVARCHAR(100) NOT NULL,
    Rol NVARCHAR(50) NOT NULL CHECK (Rol IN ('Administrador', 'Auxiliar de Registro'))
);

-- Tabla de Comerciantes
CREATE TABLE dbo.Comerciante
(
    IdComerciante INT IDENTITY(1,1) PRIMARY KEY,
    NombreRazonSocial NVARCHAR(150) NOT NULL,
    Municipio NVARCHAR(100) NOT NULL,
    Telefono NVARCHAR(20) NULL,
    CorreoElectronico NVARCHAR(150) NULL,
    FechaRegistro DATETIME NOT NULL DEFAULT GETDATE(),
    Estado NVARCHAR(10) NOT NULL CHECK (Estado IN ('Activo','Inactivo')),
    FechaActualizacion DATETIME NULL,
    Usuario NVARCHAR(100) NULL
);

-- Tabla de Establecimientos
CREATE TABLE dbo.Establecimiento
(
    IdEstablecimiento INT IDENTITY(1,1) PRIMARY KEY,
    NombreEstablecimiento NVARCHAR(150) NOT NULL,
    Ingresos DECIMAL(18,2) NOT NULL,
    NumeroEmpleados INT NOT NULL,
    IdComerciante INT NOT NULL,
    -- Campos de auditoría
    FechaActualizacion DATETIME NULL,
    Usuario NVARCHAR(100) NULL,
    CONSTRAINT FK_Establecimiento_Comerciante FOREIGN KEY (IdComerciante)
        REFERENCES dbo.Comerciante(IdComerciante)
);

/* ============================================================
   02 - CREACIÓN DE TRIGGERS DE AUDITORÍA
   Descripción: Se crean triggers para las tablas Comerciante y Establecimiento.
   Cada trigger se dispara después de una operación INSERT o UPDATE para
   actualizar los campos de auditoría (FechaActualizacion y Usuario).
============================================================ */

-- Trigger para la tabla Comerciante
IF OBJECT_ID('dbo.trg_Auditoria_Comerciante', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_Comerciante;
GO
CREATE TRIGGER dbo.trg_Auditoria_Comerciante
ON dbo.Comerciante
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF (TRIGGER_NESTLEVEL() > 1) RETURN;
    
    UPDATE c
    SET c.FechaActualizacion = GETDATE(),
        c.Usuario = SUSER_SNAME()
    FROM dbo.Comerciante c
    INNER JOIN inserted i ON c.IdComerciante = i.IdComerciante;
END;
GO

-- Trigger para la tabla Establecimiento
IF OBJECT_ID('dbo.trg_Auditoria_Establecimiento', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_Establecimiento;
GO
CREATE TRIGGER dbo.trg_Auditoria_Establecimiento
ON dbo.Establecimiento
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    IF (TRIGGER_NESTLEVEL() > 1) RETURN;
    
    UPDATE e
    SET e.FechaActualizacion = GETDATE(),
        e.Usuario = SUSER_SNAME()
    FROM dbo.Establecimiento e
    INNER JOIN inserted i ON e.IdEstablecimiento = i.IdEstablecimiento;
END;
GO

/* ============================================================
   03 - INSERCIÓN DE DATOS SEMILLA
   Descripción: Se insertan datos de prueba (seed data) en las tablas:
   - Usuarios: 2 registros (uno por cada rol).
   - Comerciantes: 5 registros.
   - Establecimientos: 10 registros, distribuidos de forma aleatoria entre
     los comerciantes (la cantidad de establecimientos por comerciante es variable).
============================================================ */

-- Insertar Usuarios
INSERT INTO dbo.Usuario (Nombre, CorreoElectronico, Contraseña, Rol)
VALUES 
('Admin User', 'admin@empresa.com', 'adminpass', 'Administrador'),
('Auxiliar User', 'auxiliar@empresa.com', 'auxiliarpass', 'Auxiliar de Registro');

-- Insertar Comerciantes (5 registros)
INSERT INTO dbo.Comerciante (NombreRazonSocial, Municipio, Telefono, CorreoElectronico, FechaRegistro, Estado)
VALUES 
('La Esquina Comercial', 'Guadalajara', '3312345678', 'contacto@laesquina.com', GETDATE(), 'Activo'),
('Supermercado Central', 'Monterrey', '8123456789', 'info@supercentral.com', GETDATE(), 'Activo'),
('Panadería El Buen Pan', 'Ciudad de México', '5551234567', 'ventas@elbuenpan.com', GETDATE(), 'Activo'),
('Ferretería San José', 'Puebla', '2229876543', 'servicio@ferreteriasanjose.com', GETDATE(), 'Inactivo'),
('Boutique de Moda Bella', 'Cancún', '9987654321', 'contacto@boutiquebella.com', GETDATE(), 'Activo');

-- Insertar Establecimientos (10 registros)
-- Se asigna aleatoriamente el IdComerciante (valores entre 1 y 5)
INSERT INTO dbo.Establecimiento (NombreEstablecimiento, Ingresos, NumeroEmpleados, IdComerciante)
VALUES 
('Tienda La Esquina - Plaza Centro', 1520.75, 8, 10),
('Sucursal Supermercado Central - Norte', 2780.90, 12, 12),
('Panadería El Buen Pan - Sucursal Reforma', 3450.50, 15, 13),
('Tienda La Esquina - Zapopan', 1980.25, 10, 11),
('Sucursal Supermercado Central - Sur', 2230.80, 9, 12),
('Panadería El Buen Pan - Sucursal Roma', 3120.60, 7, 13),
('Boutique de Moda Bella - Outlet', 3320.00, 20, 15),
('Boutique de Moda Bella - Centro Comercial', 1890.40, 11, 15),
('Sucursal Supermercado Central - Este', 4150.00, 25, 12),
('Tienda La Esquina - Lomas', 2290.60, 13, 11);
GO

/* ============================================================
   04 - PROCEDIMIENTO ALMACENADO PARA REPORTE DE COMERCIANTES
   Descripción: Se crea un procedimiento almacenado que retorna la
   información de los comerciantes activos. La salida incluye:
   - Nombre o razón social, Municipio, Teléfono, Correo Electrónico,
     Fecha de Registro, Estado.
   - Cantidad de Establecimientos, Total Ingresos y Cantidad de Empleados
     (calculados en base a los establecimientos asociados).
   Los registros se ordenan de forma descendente según la cantidad de establecimientos.
============================================================ */

IF OBJECT_ID('dbo.sp_ReporteComerciantes', 'P') IS NOT NULL
    DROP PROCEDURE dbo.sp_ReporteComerciantes;
GO
CREATE PROCEDURE dbo.sp_ReporteComerciantes
AS
BEGIN
    SET NOCOUNT ON;
    
    SELECT 
        c.NombreRazonSocial,
        c.Municipio,
        c.Telefono,
        c.CorreoElectronico,
        c.FechaRegistro,
        c.Estado,
        COUNT(e.IdEstablecimiento) AS CantidadEstablecimientos,
        ISNULL(SUM(e.Ingresos), 0) AS TotalIngresos,
        ISNULL(SUM(e.NumeroEmpleados), 0) AS CantidadEmpleados
    FROM dbo.Comerciante c
    LEFT JOIN dbo.Establecimiento e ON c.IdComerciante = e.IdComerciante
    WHERE c.Estado = 'Activo'
    GROUP BY 
        c.NombreRazonSocial,
        c.Municipio,
        c.Telefono,
        c.CorreoElectronico,
        c.FechaRegistro,
        c.Estado
    ORDER BY COUNT(e.IdEstablecimiento) DESC;
END;
GO

IF OBJECT_ID('dbo.trg_Auditoria_Comerciante', 'TR') IS NOT NULL
    DROP TRIGGER dbo.trg_Auditoria_Comerciante;
GO

CREATE TRIGGER dbo.trg_Auditoria_Comerciante
ON dbo.Comerciante
AFTER INSERT, UPDATE
AS
BEGIN
    SET NOCOUNT ON;
    
    -- Evitar recursividad en caso de actualizaciones internas
    IF (TRIGGER_NESTLEVEL() > 1)
        RETURN;
    
    UPDATE c
    SET c.FechaActualizacion = GETDATE(),
        c.Usuario = SUSER_SNAME()
    FROM dbo.Comerciante c
    INNER JOIN inserted i ON c.IdComerciante = i.IdComerciante;
END;
GO



IF OBJECT_ID('dbo.InsertComerciante', 'P') IS NOT NULL
    DROP PROCEDURE dbo.InsertComerciante;
GO

CREATE PROCEDURE dbo.InsertComerciante
    @NombreRazonSocial NVARCHAR(150),
    @Municipio NVARCHAR(100),
    @Telefono NVARCHAR(20),
    @CorreoElectronico NVARCHAR(150),
    @FechaRegistro DATETIME,
    @Estado NVARCHAR(10),
    @FechaActualizacion DATETIME,
    @Usuario NVARCHAR(100),
    @NewId INT OUTPUT
AS
BEGIN
    SET NOCOUNT ON;

    INSERT INTO dbo.Comerciante
        (NombreRazonSocial, Municipio, Telefono, CorreoElectronico, FechaRegistro, Estado, FechaActualizacion, Usuario)
    VALUES
        (@NombreRazonSocial, @Municipio, @Telefono, @CorreoElectronico, @FechaRegistro, @Estado, @FechaActualizacion, @Usuario);

    SET @NewId = SCOPE_IDENTITY();
END;
GO



IF OBJECT_ID('dbo.UpdateComerciante', 'P') IS NOT NULL
    DROP PROCEDURE dbo.UpdateComerciante;
GO

CREATE PROCEDURE dbo.UpdateComerciante
    @IdComerciante INT,
    @NombreRazonSocial NVARCHAR(150),
    @Municipio NVARCHAR(100),
    @Telefono NVARCHAR(20),
    @CorreoElectronico NVARCHAR(150),
    @FechaRegistro DATETIME,
    @Estado NVARCHAR(10),
    @FechaActualizacion DATETIME,
    @Usuario NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Comerciante
    SET NombreRazonSocial = @NombreRazonSocial,
        Municipio = @Municipio,
        Telefono = @Telefono,
        CorreoElectronico = @CorreoElectronico,
        FechaRegistro = @FechaRegistro,
        Estado = @Estado,
        FechaActualizacion = @FechaActualizacion,
        Usuario = @Usuario
    WHERE IdComerciante = @IdComerciante;
END;
GO


IF OBJECT_ID('dbo.UpdateEstadoComerciante', 'P') IS NOT NULL
    DROP PROCEDURE dbo.UpdateEstadoComerciante;
GO

CREATE PROCEDURE dbo.UpdateEstadoComerciante
    @IdComerciante INT,
    @Estado NVARCHAR(10),
    @FechaActualizacion DATETIME,
    @Usuario NVARCHAR(100)
AS
BEGIN
    SET NOCOUNT ON;

    UPDATE dbo.Comerciante
    SET Estado = @Estado,
        FechaActualizacion = @FechaActualizacion,
        Usuario = @Usuario
    WHERE IdComerciante = @IdComerciante;
END;
GO
