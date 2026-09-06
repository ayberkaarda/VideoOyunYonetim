/*
 * 0002 - normalise genre and platform.
 *
 * WHAT
 *   Adds dbo.Genre, dbo.Platform and the dbo.GamePlatform link table, copies the
 *   free-text values off dbo.Game into them, points dbo.Game at a genre through
 *   GenreId, and finally drops the two text columns.
 *
 * WHY
 *   Genre and platform were repeated on every row as text, so a typo produced a
 *   new "genre" and nothing could be counted or weighted reliably. A game also
 *   only ever had one platform, which is not true of the catalogue. The link
 *   table lifts that limit without changing dbo.Game again later.
 *
 * WHY THE ORDER MATTERS
 *   The text columns are read by three steps and dropped only after the last of
 *   them. Dropping earlier would lose the data the copy depends on.
 *
 * WHY THE DYNAMIC SQL
 *   Every statement that names Game.Genre or Game.[Platform] sits inside EXEC.
 *   T-SQL resolves column names when it compiles a batch, not when it reaches the
 *   IF, so a guarded statement that names a dropped column still fails to compile
 *   on a re-run. The literal inside EXEC is fixed text with no value pasted into
 *   it, so nothing here is assembled from input.
 *
 * WHY IT IS GUARDED AT ALL
 *   Installations that predate this chain have no journal table, so the first run
 *   replays every script. Each step therefore checks whether its work is already
 *   done rather than assuming a clean database.
 */

/* ---------------------------------------------------------------- lookups */

IF OBJECT_ID(N'dbo.Genre', N'U') IS NULL
BEGIN
    PRINT N'0002: creating table dbo.Genre...';

    CREATE TABLE dbo.Genre
    (
        Id   INT          IDENTITY(1, 1) NOT NULL,
        Name NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_Genre PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Genre_Name UNIQUE (Name)
    );
END
GO

IF OBJECT_ID(N'dbo.Platform', N'U') IS NULL
BEGIN
    PRINT N'0002: creating table dbo.Platform...';

    CREATE TABLE dbo.[Platform]
    (
        Id   INT          IDENTITY(1, 1) NOT NULL,
        Name NVARCHAR(50) NOT NULL,
        CONSTRAINT PK_Platform PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT UQ_Platform_Name UNIQUE (Name)
    );
END
GO

IF OBJECT_ID(N'dbo.GamePlatform', N'U') IS NULL
BEGIN
    PRINT N'0002: creating table dbo.GamePlatform...';

    /* Deleting a game takes its links with it; deleting a platform that is still
       in use is refused, because that is a catalogue mistake and not a cascade. */
    CREATE TABLE dbo.GamePlatform
    (
        GameId     INT NOT NULL,
        PlatformId INT NOT NULL,
        CONSTRAINT PK_GamePlatform PRIMARY KEY CLUSTERED (GameId, PlatformId),
        CONSTRAINT FK_GamePlatform_Game
            FOREIGN KEY (GameId) REFERENCES dbo.Game (Id) ON DELETE CASCADE,
        CONSTRAINT FK_GamePlatform_Platform
            FOREIGN KEY (PlatformId) REFERENCES dbo.[Platform] (Id)
    );
END
GO

/* ------------------------------------------------------ Game.GenreId column */

IF COL_LENGTH(N'dbo.Game', N'GenreId') IS NULL
BEGIN
    PRINT N'0002: adding column dbo.Game.GenreId...';
    ALTER TABLE dbo.Game ADD GenreId INT NULL;
END
GO

IF NOT EXISTS (SELECT 1 FROM sys.foreign_keys
               WHERE name = N'FK_Game_Genre' AND parent_object_id = OBJECT_ID(N'dbo.Game'))
    ALTER TABLE dbo.Game WITH CHECK
        ADD CONSTRAINT FK_Game_Genre FOREIGN KEY (GenreId) REFERENCES dbo.Genre (Id);
GO

/* --------------------------------------------------------- copy the values */

/* Distinct trimmed genre names. Blank and NULL text is skipped: it carries no
   genre, and inventing one would be making data up. */
IF COL_LENGTH(N'dbo.Game', N'Genre') IS NOT NULL
    EXEC (N'
INSERT INTO dbo.Genre (Name)
SELECT DISTINCT LTRIM(RTRIM(g.Genre))
FROM   dbo.Game AS g
WHERE  g.Genre IS NOT NULL
  AND  LEN(LTRIM(RTRIM(g.Genre))) > 0
  AND  NOT EXISTS (SELECT 1 FROM dbo.Genre AS existing
                   WHERE existing.Name = LTRIM(RTRIM(g.Genre)));');
GO

IF COL_LENGTH(N'dbo.Game', N'Platform') IS NOT NULL
    EXEC (N'
INSERT INTO dbo.[Platform] (Name)
SELECT DISTINCT LTRIM(RTRIM(g.[Platform]))
FROM   dbo.Game AS g
WHERE  g.[Platform] IS NOT NULL
  AND  LEN(LTRIM(RTRIM(g.[Platform]))) > 0
  AND  NOT EXISTS (SELECT 1 FROM dbo.[Platform] AS existing
                   WHERE existing.Name = LTRIM(RTRIM(g.[Platform])));');
GO

/* Point each game at its genre row. Matching on the name is correct here and
   only here: the name is the only link the old flat column left behind, and the
   name is unique in dbo.Genre by constraint. */
IF COL_LENGTH(N'dbo.Game', N'Genre') IS NOT NULL
    EXEC (N'
UPDATE      g
SET         g.GenreId = lookup.Id
FROM        dbo.Game  AS g
INNER JOIN  dbo.Genre AS lookup ON lookup.Name = LTRIM(RTRIM(g.Genre))
WHERE       g.GenreId IS NULL;');
GO

/* One link row per game, from the single platform the flat column held. */
IF COL_LENGTH(N'dbo.Game', N'Platform') IS NOT NULL
    EXEC (N'
INSERT INTO dbo.GamePlatform (GameId, PlatformId)
SELECT      g.Id, p.Id
FROM        dbo.Game       AS g
INNER JOIN  dbo.[Platform] AS p ON p.Name = LTRIM(RTRIM(g.[Platform]))
WHERE       NOT EXISTS (SELECT 1 FROM dbo.GamePlatform AS link
                        WHERE link.GameId = g.Id AND link.PlatformId = p.Id);');
GO

/* ------------------------------------------------------------- tighten Game */

/* A game without a name cannot be shown or searched for. Stop rather than guess
   a value: the operator has to decide what those rows are. */
IF EXISTS (SELECT 1 FROM dbo.Game WHERE Name IS NULL)
    RAISERROR(N'dbo.Game holds rows whose Name is NULL. Give every row a name, then run the migrator again.', 16, 1);
GO

IF EXISTS (SELECT 1 FROM sys.columns
           WHERE object_id = OBJECT_ID(N'dbo.Game') AND name = N'Name' AND is_nullable = 1)
BEGIN
    PRINT N'0002: making dbo.Game.Name NOT NULL...';
    ALTER TABLE dbo.Game ALTER COLUMN Name NVARCHAR(100) NOT NULL;
END
GO

/* ------------------------------------------- drop the columns that moved out */

IF COL_LENGTH(N'dbo.Game', N'Genre') IS NOT NULL
BEGIN
    PRINT N'0002: dropping column dbo.Game.Genre...';
    EXEC (N'ALTER TABLE dbo.Game DROP COLUMN Genre;');
END
GO

IF COL_LENGTH(N'dbo.Game', N'Platform') IS NOT NULL
BEGIN
    PRINT N'0002: dropping column dbo.Game.Platform...';
    EXEC (N'ALTER TABLE dbo.Game DROP COLUMN [Platform];');
END
GO
