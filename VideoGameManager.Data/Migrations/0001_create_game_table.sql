/*
 * 0001 - baseline: the flat Game table.
 *
 * WHAT
 *   Creates dbo.Game in the shape the catalogue had before the schema was
 *   normalised: genre and platform held as free text on the row, and a single
 *   Comment column instead of a review table. The scripts that follow migrate
 *   that shape forward.
 *
 * WHY IT IS GUARDED
 *   Installations that predate this chain already hold dbo.Game but have no
 *   journal table, so the first run of the migrator replays the whole chain
 *   against a database that is partly there. Every script therefore has to be
 *   safe to run against work that is already done. Here that means creating the
 *   table only when it is missing and touching nothing otherwise.
 *
 * There is no USE statement: the migrator runs each script on the connection's
 * own database, which is how a test can point the same chain at a throwaway one.
 */

IF OBJECT_ID(N'dbo.Game', N'U') IS NULL
BEGIN
    PRINT N'0001: creating table dbo.Game...';

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
    PRINT N'0001: table dbo.Game is already present, nothing to do.';
GO
