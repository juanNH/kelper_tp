USE [Kepler7];

IF DB_NAME() <> N'Kepler7'
    THROW 50000, 'Ejecute 001_CrearBase.sql antes de preparar la tabla Partidas.', 1;

IF OBJECT_ID(N'dbo.Partidas', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.Partidas
    (
        Id bigint IDENTITY(1, 1) NOT NULL CONSTRAINT PK_Partidas PRIMARY KEY,
        Nombre nvarchar(120) NOT NULL,
        Etapa tinyint NOT NULL,
        Estado tinyint NOT NULL,
        VersionFormato int NOT NULL,
        Datos nvarchar(max) NOT NULL,
        ActualizadaUtc datetime2(7) NOT NULL CONSTRAINT DF_Partidas_ActualizadaUtc DEFAULT SYSUTCDATETIME(),
        Revision bigint NOT NULL CONSTRAINT DF_Partidas_Revision DEFAULT 1,

        CONSTRAINT CK_Partidas_Nombre CHECK (LEN(LTRIM(RTRIM(Nombre))) > 0),
        CONSTRAINT CK_Partidas_EtapaEstado CHECK
        (
            (Etapa BETWEEN 1 AND 6 AND Estado = 1)
            OR (Etapa = 7 AND Estado BETWEEN 2 AND 4)
        ),
        CONSTRAINT CK_Partidas_VersionFormato CHECK (VersionFormato > 0),
        CONSTRAINT CK_Partidas_DatosJson CHECK (ISJSON(Datos) = 1),
        CONSTRAINT CK_Partidas_Revision CHECK (Revision > 0)
    );
END;

IF NOT EXISTS
(
    SELECT 1
    FROM sys.indexes
    WHERE object_id = OBJECT_ID(N'dbo.Partidas', N'U')
        AND name = N'IX_Partidas_Estado_ActualizadaUtc'
)
BEGIN
    CREATE INDEX IX_Partidas_Estado_ActualizadaUtc
        ON dbo.Partidas (Estado, ActualizadaUtc DESC, Id DESC)
        INCLUDE (Nombre, Etapa, VersionFormato, Revision);
END;
