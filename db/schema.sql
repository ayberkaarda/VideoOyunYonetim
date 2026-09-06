/*
 * Video Game Manager - database schema
 * ------------------------------------
 * This script is idempotent: it can be run more than once.
 *
 * Usage:
 *   sqlcmd -S localhost,1433 -U sa -P <password> -C -i db/schema.sql
 *
 * The schema was reverse-engineered from the SQL Server backup that used to
 * live in this repository.
 *
 * Intentional deviations from that backup:
 *
 *   1. `Comment` is NVARCHAR(MAX). The backup declared it as `TEXT`, a
 *      deprecated non-Unicode type that mangled non-ASCII characters.
 *
 *   2. The collation is Latin1_General_100_CI_AI, not Turkish_CI_AS. Under a
 *      Turkish collation `I` and `i` are different letters, so
 *      `Name LIKE '%fifa%'` does NOT match 'FIFA 24' -- search silently
 *      returns nothing, with no error. Accent insensitivity is a bonus:
 *      'pokemon' matches 'Pokemon'.
 *
 *   3. Identifiers are English. The backup used Turkish ones.
 *
 * Table names are singular so they line up with the normalised schema that
 * replaces this one later: Genre, Platform, Review, GamePlatform.
 */

IF DB_ID(N'VideoGameManager') IS NULL
BEGIN
    PRINT N'Creating database VideoGameManager...';
    EXEC (N'CREATE DATABASE [VideoGameManager] COLLATE Latin1_General_100_CI_AI;');
END
ELSE IF CONVERT(NVARCHAR(128), DATABASEPROPERTYEX(N'VideoGameManager', 'Collation')) <> N'Latin1_General_100_CI_AI'
    RAISERROR(N'VideoGameManager already exists with a different collation. Drop it (docker compose -f db/docker-compose.yml down -v) and run this script again.', 16, 1);
ELSE
    PRINT N'Database VideoGameManager already exists.';
GO

USE [VideoGameManager];
GO

IF OBJECT_ID(N'dbo.Game', N'U') IS NULL
BEGIN
    PRINT N'Creating table dbo.Game...';

    CREATE TABLE dbo.Game
    (
        Id         INT            IDENTITY(1, 1) NOT NULL,
        Name       NVARCHAR(100)  NULL,
        Genre      NVARCHAR(50)   NULL,
        [Platform] NVARCHAR(50)   NULL,
        Score      FLOAT          NULL,
        CoverUrl   NVARCHAR(MAX)  NULL,
        Comment    NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_Game PRIMARY KEY CLUSTERED (Id)
    );
END
ELSE
    PRINT N'Table dbo.Game already exists.';
GO

/* Backfill columns that older installations may be missing. */
IF COL_LENGTH(N'dbo.Game', N'CoverUrl') IS NULL
    ALTER TABLE dbo.Game ADD CoverUrl NVARCHAR(MAX) NULL;
GO

IF COL_LENGTH(N'dbo.Game', N'Comment') IS NULL
    ALTER TABLE dbo.Game ADD Comment NVARCHAR(MAX) NULL;
GO

PRINT N'Schema ready.';
GO
