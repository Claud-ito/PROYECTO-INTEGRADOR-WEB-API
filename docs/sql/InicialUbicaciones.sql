IF OBJECT_ID(N'[__EFMigrationsHistory]') IS NULL
BEGIN
    CREATE TABLE [__EFMigrationsHistory] (
        [MigrationId] nvarchar(150) NOT NULL,
        [ProductVersion] nvarchar(32) NOT NULL,
        CONSTRAINT [PK___EFMigrationsHistory] PRIMARY KEY ([MigrationId])
    );
END;
GO

BEGIN TRANSACTION;
IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120022734_InicialUbicaciones'
)
BEGIN
    CREATE TABLE [Modulos] (
        [ModuloId] int NOT NULL IDENTITY,
        [Codigo] nvarchar(20) COLLATE Latin1_General_100_CI_AS NOT NULL,
        [Nombre] nvarchar(100) NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Modulos] PRIMARY KEY ([ModuloId]),
        CONSTRAINT [CK_Modulos_Codigo_NoVacio] CHECK (LEN(LTRIM(RTRIM([Codigo]))) > 0),
        CONSTRAINT [CK_Modulos_Nombre_NoVacio] CHECK (LEN(LTRIM(RTRIM([Nombre]))) > 0)
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120022734_InicialUbicaciones'
)
BEGIN
    CREATE TABLE [Pisos] (
        [PisoId] int NOT NULL IDENTITY,
        [ModuloId] int NOT NULL,
        [Denominacion] nvarchar(50) COLLATE Latin1_General_100_CI_AS NOT NULL,
        [Activo] bit NOT NULL DEFAULT CAST(1 AS bit),
        CONSTRAINT [PK_Pisos] PRIMARY KEY ([PisoId]),
        CONSTRAINT [CK_Pisos_Denominacion_NoVacia] CHECK (LEN(LTRIM(RTRIM([Denominacion]))) > 0),
        CONSTRAINT [FK_Pisos_Modulos_ModuloId] FOREIGN KEY ([ModuloId]) REFERENCES [Modulos] ([ModuloId]) ON DELETE NO ACTION
    );
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120022734_InicialUbicaciones'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Modulos_Codigo] ON [Modulos] ([Codigo]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120022734_InicialUbicaciones'
)
BEGIN
    CREATE UNIQUE INDEX [IX_Pisos_ModuloId_Denominacion] ON [Pisos] ([ModuloId], [Denominacion]);
END;

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261120022734_InicialUbicaciones'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261120022734_InicialUbicaciones', N'10.0.13');
END;

COMMIT;
GO

