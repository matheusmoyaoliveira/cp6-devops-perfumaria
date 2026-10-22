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
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022183357_CriacaoInicialSqlServer'
)
BEGIN
    CREATE TABLE [T_PRF_NOTAS_OLFATIVAS] (
        [ID] int NOT NULL IDENTITY,
        [NOME] nvarchar(100) NOT NULL,
        [FAMILIA] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_T_PRF_NOTAS_OLFATIVAS] PRIMARY KEY ([ID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022183357_CriacaoInicialSqlServer'
)
BEGIN
    CREATE TABLE [T_PRF_PERFUMES] (
        [ID] int NOT NULL IDENTITY,
        [NOME] nvarchar(150) NOT NULL,
        [MARCA] nvarchar(100) NOT NULL,
        [GENERO] nvarchar(20) NOT NULL,
        [VOLUME_ML] int NOT NULL,
        [PRECO] decimal(10,2) NOT NULL,
        [ANO_LANCAMENTO] int NOT NULL,
        CONSTRAINT [PK_T_PRF_PERFUMES] PRIMARY KEY ([ID])
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022183357_CriacaoInicialSqlServer'
)
BEGIN
    CREATE TABLE [T_PRF_PERFUME_NOTA] (
        [PERFUME_ID] int NOT NULL,
        [NOTA_OLFATIVA_ID] int NOT NULL,
        [POSICAO] nvarchar(20) NOT NULL,
        CONSTRAINT [PK_T_PRF_PERFUME_NOTA] PRIMARY KEY ([PERFUME_ID], [NOTA_OLFATIVA_ID]),
        CONSTRAINT [FK_T_PRF_PERFUME_NOTA_T_PRF_NOTAS_OLFATIVAS_NOTA_OLFATIVA_ID] FOREIGN KEY ([NOTA_OLFATIVA_ID]) REFERENCES [T_PRF_NOTAS_OLFATIVAS] ([ID]) ON DELETE CASCADE,
        CONSTRAINT [FK_T_PRF_PERFUME_NOTA_T_PRF_PERFUMES_PERFUME_ID] FOREIGN KEY ([PERFUME_ID]) REFERENCES [T_PRF_PERFUMES] ([ID]) ON DELETE CASCADE
    );
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022183357_CriacaoInicialSqlServer'
)
BEGIN
    CREATE INDEX [IX_T_PRF_PERFUME_NOTA_NOTA_OLFATIVA_ID] ON [T_PRF_PERFUME_NOTA] ([NOTA_OLFATIVA_ID]);
END;
GO

IF NOT EXISTS (
    SELECT * FROM [__EFMigrationsHistory]
    WHERE [MigrationId] = N'20261022183357_CriacaoInicialSqlServer'
)
BEGIN
    INSERT INTO [__EFMigrationsHistory] ([MigrationId], [ProductVersion])
    VALUES (N'20261022183357_CriacaoInicialSqlServer', N'8.0.11');
END;
GO

COMMIT;
GO

