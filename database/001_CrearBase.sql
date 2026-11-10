USE [master];
GO

IF DB_ID(N'Kepler7') IS NULL
BEGIN
    EXEC (N'CREATE DATABASE [Kepler7];');
END;
GO
