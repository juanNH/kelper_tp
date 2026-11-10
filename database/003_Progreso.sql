USE [Kepler7];

IF OBJECT_ID(N'dbo.Partidas', N'U') IS NULL
    THROW 50000, 'Ejecute 002_Partidas.sql antes de preparar el progreso.', 1;

SET XACT_ABORT ON;
BEGIN TRANSACTION;

IF OBJECT_ID(N'dbo.ProgresoPartida', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.ProgresoPartida
    (
        PartidaId bigint NOT NULL CONSTRAINT PK_ProgresoPartida PRIMARY KEY,
        Experiencia int NOT NULL,

        CONSTRAINT FK_ProgresoPartida_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas (Id),
        CONSTRAINT CK_ProgresoPartida_Experiencia CHECK (Experiencia >= 0)
    );
END;

IF OBJECT_ID(N'dbo.LogrosPatron', N'U') IS NULL
BEGIN
    CREATE TABLE dbo.LogrosPatron
    (
        Patron tinyint NOT NULL CONSTRAINT PK_LogrosPatron PRIMARY KEY,
        PartidaId bigint NOT NULL,
        DesbloqueadoUtc datetime2(7) NOT NULL CONSTRAINT DF_LogrosPatron_DesbloqueadoUtc DEFAULT SYSUTCDATETIME(),

        CONSTRAINT FK_LogrosPatron_Partidas FOREIGN KEY (PartidaId) REFERENCES dbo.Partidas (Id),
        CONSTRAINT CK_LogrosPatron_Patron CHECK (Patron BETWEEN 1 AND 12)
    );
END;

COMMIT TRANSACTION;
