/*
    ============================================================
    MedicalCare - Base de datos para gestión de atenciones médicas
    Compatible con SQL Server 2012+
    ============================================================
*/

---------------------------------------------------------------
-- 1. CREAR BASE DE DATOS
---------------------------------------------------------------

IF DB_ID('MedicalCare') IS NULL
BEGIN
    CREATE DATABASE MedicalCare;
END
GO

USE MedicalCare;
GO

---------------------------------------------------------------
-- 2. TABLA: Pacientes
---------------------------------------------------------------

IF OBJECT_ID('dbo.Pacientes', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Pacientes
    (
        Id INT IDENTITY(1,1) NOT NULL,
        Documento VARCHAR(20) NOT NULL,
        Nombre VARCHAR(100) NOT NULL,
        Apellido VARCHAR(100) NOT NULL,
        FechaNacimiento DATE NULL,
        Activo BIT NOT NULL CONSTRAINT DF_Pacientes_Activo DEFAULT (1),

        CONSTRAINT PK_Pacientes PRIMARY KEY (Id),
        CONSTRAINT UQ_Pacientes_Documento UNIQUE (Documento)
    );
END
GO

---------------------------------------------------------------
-- 3. TABLA: Servicios Médicos
---------------------------------------------------------------

IF OBJECT_ID('dbo.ServiciosMedicos', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.ServiciosMedicos
    (
        Id INT IDENTITY(1,1) NOT NULL,
        Nombre VARCHAR(100) NOT NULL,
        Descripcion VARCHAR(250) NULL,
        Activo BIT NOT NULL CONSTRAINT DF_ServiciosMedicos_Activo DEFAULT (1),

        CONSTRAINT PK_ServiciosMedicos PRIMARY KEY (Id)
    );
END
GO

---------------------------------------------------------------
-- 4. TABLA: Atenciones
---------------------------------------------------------------

IF OBJECT_ID('dbo.Atenciones', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.Atenciones
    (
        Id INT IDENTITY(1,1) NOT NULL,
        PacienteId INT NOT NULL,
        FechaAtencion DATE NOT NULL,
        FechaRegistro DATETIME NOT NULL CONSTRAINT DF_Atenciones_FechaRegistro DEFAULT (GETDATE()),

        CONSTRAINT PK_Atenciones PRIMARY KEY (Id),

        CONSTRAINT FK_Atenciones_Pacientes
            FOREIGN KEY (PacienteId)
            REFERENCES dbo.Pacientes(Id)
    );
END
GO

---------------------------------------------------------------
-- 5. TABLA: Detalle de servicios de una atención
---------------------------------------------------------------

IF OBJECT_ID('dbo.AtencionServicios', 'U') IS NULL
BEGIN
    CREATE TABLE dbo.AtencionServicios
    (
        Id INT IDENTITY(1,1) NOT NULL,
        AtencionId INT NOT NULL,
        ServicioId INT NOT NULL,

        CONSTRAINT PK_AtencionServicios PRIMARY KEY (Id),

        CONSTRAINT FK_AtencionServicios_Atenciones
            FOREIGN KEY (AtencionId)
            REFERENCES dbo.Atenciones(Id),

        CONSTRAINT FK_AtencionServicios_Servicios
            FOREIGN KEY (ServicioId)
            REFERENCES dbo.ServiciosMedicos(Id),

        CONSTRAINT UQ_AtencionServicios
            UNIQUE (AtencionId, ServicioId)
    );
END
GO

---------------------------------------------------------------
-- 6. ÍNDICES
---------------------------------------------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE name = 'IX_Atenciones_Paciente_Fecha'
      AND object_id = OBJECT_ID('dbo.Atenciones')
)
BEGIN
    CREATE INDEX IX_Atenciones_Paciente_Fecha
        ON dbo.Atenciones(PacienteId, FechaAtencion);
END
GO

---------------------------------------------------------------
-- 7. TIPO DE TABLA PARA RECIBIR MÚLTIPLES SERVICIOS
---------------------------------------------------------------

IF TYPE_ID('dbo.ServicioIdTable') IS NULL
BEGIN
    CREATE TYPE dbo.ServicioIdTable AS TABLE
    (
        ServicioId INT NOT NULL
    );
END
GO

---------------------------------------------------------------
-- 8. DATOS INICIALES - PACIENTES
---------------------------------------------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.Pacientes
)
BEGIN
    INSERT INTO dbo.Pacientes
    (
        Documento,
        Nombre,
        Apellido,
        FechaNacimiento,
        Activo
    )
    VALUES
        ('1001001001', 'Juan', 'Perez', '1990-05-10', 1),
        ('1001001002', 'Maria', 'Gomez', '1985-08-20', 1),
        ('1001001003', 'Carlos', 'Rodriguez', '1995-02-15', 1);
END
GO

---------------------------------------------------------------
-- 9. DATOS INICIALES - SERVICIOS MÉDICOS
---------------------------------------------------------------

IF NOT EXISTS
(
    SELECT 1
    FROM dbo.ServiciosMedicos
)
BEGIN
    INSERT INTO dbo.ServiciosMedicos
    (
        Nombre,
        Descripcion,
        Activo
    )
    VALUES
        ('Medicina General', 'Consulta médica general', 1),
        ('Odontologia', 'Consulta y atención odontológica', 1),
        ('Pediatria', 'Atención médica pediátrica', 1),
        ('Laboratorio Clinico', 'Servicios de laboratorio clínico', 1),
        ('Cardiologia', 'Consulta especializada en cardiología', 0);
END
GO

---------------------------------------------------------------
-- 10. SP: LISTAR PACIENTES ACTIVOS
---------------------------------------------------------------

CREATE OR ALTER PROCEDURE dbo.sp_Paciente_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Documento,
        Nombre,
        Apellido,
        FechaNacimiento
    FROM dbo.Pacientes
    WHERE Activo = 1
    ORDER BY Nombre, Apellido;
END
GO

---------------------------------------------------------------
-- 11. SP: LISTAR SERVICIOS ACTIVOS
---------------------------------------------------------------

CREATE OR ALTER PROCEDURE dbo.sp_Servicio_ListarActivos
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        Id,
        Nombre,
        Descripcion
    FROM dbo.ServiciosMedicos
    WHERE Activo = 1
    ORDER BY Nombre;
END
GO

---------------------------------------------------------------
-- 12. SP: CREAR ATENCIÓN
---------------------------------------------------------------

CREATE OR ALTER PROCEDURE dbo.sp_Atencion_Crear
(
    @PacienteId INT,
    @FechaAtencion DATE,
    @Servicios dbo.ServicioIdTable READONLY
)
AS
BEGIN
    SET NOCOUNT ON;
    SET XACT_ABORT ON;

    BEGIN TRY

        -------------------------------------------------------
        -- Validar paciente
        -------------------------------------------------------

        IF NOT EXISTS
        (
            SELECT 1
            FROM dbo.Pacientes
            WHERE Id = @PacienteId
              AND Activo = 1
        )
        BEGIN
            RAISERROR(
                'El paciente no existe o se encuentra inactivo.',
                16,
                1
            );
            RETURN;
        END;

        -------------------------------------------------------
        -- Validar que exista al menos un servicio
        -------------------------------------------------------

        IF NOT EXISTS
        (
            SELECT 1
            FROM @Servicios
        )
        BEGIN
            RAISERROR(
                'La atención debe tener al menos un servicio.',
                16,
                1
            );
            RETURN;
        END;

        -------------------------------------------------------
        -- Validar que todos los servicios existan y estén activos
        -------------------------------------------------------

        IF EXISTS
        (
            SELECT 1
            FROM @Servicios ts
            LEFT JOIN dbo.ServiciosMedicos s
                ON s.Id = ts.ServicioId
            WHERE s.Id IS NULL
               OR s.Activo = 0
        )
        BEGIN
            RAISERROR(
                'Uno o más servicios no existen o se encuentran inactivos.',
                16,
                1
            );
            RETURN;
        END;

        -------------------------------------------------------
        -- Validar duplicidad:
        -- mismo paciente + misma fecha + mismo servicio
        -------------------------------------------------------

        IF EXISTS
        (
            SELECT 1
            FROM dbo.Atenciones a
            INNER JOIN dbo.AtencionServicios ats
                ON ats.AtencionId = a.Id
            INNER JOIN @Servicios ts
                ON ts.ServicioId = ats.ServicioId
            WHERE a.PacienteId = @PacienteId
              AND a.FechaAtencion = @FechaAtencion
        )
        BEGIN
            RAISERROR(
                'El paciente ya tiene registrado uno de los servicios seleccionados para la fecha indicada.',
                16,
                1
            );
            RETURN;
        END;

        -------------------------------------------------------
        -- Registrar atención y servicios
        -------------------------------------------------------

        BEGIN TRANSACTION;

        DECLARE @AtencionId INT;

        INSERT INTO dbo.Atenciones
        (
            PacienteId,
            FechaAtencion
        )
        VALUES
        (
            @PacienteId,
            @FechaAtencion
        );

        SET @AtencionId = SCOPE_IDENTITY();

        INSERT INTO dbo.AtencionServicios
        (
            AtencionId,
            ServicioId
        )
        SELECT
            @AtencionId,
            ServicioId
        FROM @Servicios;

        COMMIT TRANSACTION;

        -------------------------------------------------------
        -- Retornar identificador generado
        -------------------------------------------------------

        SELECT @AtencionId AS AtencionId;

    END TRY
    BEGIN CATCH

        IF XACT_STATE() <> 0
        BEGIN
            ROLLBACK TRANSACTION;
        END;

        RAISERROR(
            'No es posible registrar la atención: el paciente ya tiene registrado este servicio para la fecha indicada.',
            16,
            1
        );

    END CATCH;
END
GO

---------------------------------------------------------------
-- 13. SP: LISTAR ATENCIONES
---------------------------------------------------------------

CREATE OR ALTER PROCEDURE dbo.sp_Atencion_Listar
AS
BEGIN
    SET NOCOUNT ON;

    SELECT
        a.Id AS AtencionId,
        p.Documento,
        p.Nombre + ' ' + p.Apellido AS Paciente,
        a.FechaAtencion,
        a.FechaRegistro,
        s.Nombre AS Servicio
    FROM dbo.Atenciones a
    INNER JOIN dbo.Pacientes p
        ON p.Id = a.PacienteId
    INNER JOIN dbo.AtencionServicios ats
        ON ats.AtencionId = a.Id
    INNER JOIN dbo.ServiciosMedicos s
        ON s.Id = ats.ServicioId
    ORDER BY
        a.FechaAtencion DESC,
        a.Id DESC;
END
GO

---------------------------------------------------------------
-- FIN DEL SCRIPT
---------------------------------------------------------------