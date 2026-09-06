/*
 * 0003 - move reviews into their own table.
 *
 * WHAT
 *   Adds dbo.Review, moves the single dbo.Game.Comment value of every game into
 *   it, drops that column, and puts the score range on dbo.Game as a constraint.
 *
 * WHY
 *   One nullable Comment column allowed exactly one opinion per game and had no
 *   score and no date of its own. A row per review lifts all three limits.
 *
 * HOW THE OLD VALUES ARE CARRIED OVER
 *   Score is left NULL on every migrated row on purpose. The old column held text
 *   and nothing else; dbo.Game.Score is the game's own rating, not the rating of
 *   the person who wrote that text. Copying it across would attribute a number to
 *   a review that never carried one, which is inventing data.
 *
 *   CreatedAt has no source either -- the old column recorded no date -- so the
 *   moment of the migration is used. It is the only honest answer available: it
 *   says when the row came into being, and it does not pretend to be the moment
 *   the text was written.
 *
 *   A game that already has a review is skipped, so a replay adds nothing twice.
 *
 * The statements that name Game.Comment sit inside EXEC because T-SQL resolves
 * column names when it compiles a batch, so a guarded statement naming a dropped
 * column would still fail to compile on a re-run. The literal is fixed text.
 */

IF OBJECT_ID(N'dbo.Review', N'U') IS NULL
BEGIN
    PRINT N'0003: creating table dbo.Review...';

    /* Reviews belong to their game and have no meaning without it, so deleting a
       game takes them along. DATETIMEOFFSET keeps the writer's UTC offset, which
       a plain DATETIME2 would silently discard. */
    CREATE TABLE dbo.Review
    (
        Id        INT               IDENTITY(1, 1) NOT NULL,
        GameId    INT               NOT NULL,
        Score     FLOAT             NULL,
        Body      NVARCHAR(MAX)     NOT NULL,
        CreatedAt DATETIMEOFFSET(3) NOT NULL
            CONSTRAINT DF_Review_CreatedAt DEFAULT SYSDATETIMEOFFSET(),
        CONSTRAINT PK_Review PRIMARY KEY CLUSTERED (Id),
        CONSTRAINT FK_Review_Game
            FOREIGN KEY (GameId) REFERENCES dbo.Game (Id) ON DELETE CASCADE
    );
END
GO

/* WITH CHECK, so the constraint validates the rows already there instead of
   trusting them. A trusted constraint is also usable by the query optimiser. */
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = N'CK_Review_Score' AND parent_object_id = OBJECT_ID(N'dbo.Review'))
    ALTER TABLE dbo.Review WITH CHECK
        ADD CONSTRAINT CK_Review_Score CHECK (Score IS NULL OR (Score >= 0 AND Score <= 10));
GO

/* ------------------------------------------------ carry the old text across */

IF COL_LENGTH(N'dbo.Game', N'Comment') IS NOT NULL
    EXEC (N'
INSERT INTO dbo.Review (GameId, Score, Body, CreatedAt)
SELECT g.Id, NULL, g.Comment, SYSDATETIMEOFFSET()
FROM   dbo.Game AS g
WHERE  g.Comment IS NOT NULL
  AND  LEN(LTRIM(RTRIM(g.Comment))) > 0
  AND  NOT EXISTS (SELECT 1 FROM dbo.Review AS existing WHERE existing.GameId = g.Id);');
GO

IF COL_LENGTH(N'dbo.Game', N'Comment') IS NOT NULL
BEGIN
    PRINT N'0003: dropping column dbo.Game.Comment...';
    EXEC (N'ALTER TABLE dbo.Game DROP COLUMN Comment;');
END
GO

/* ------------------------------------------------- the score range on Game */

IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = N'CK_Game_Score' AND parent_object_id = OBJECT_ID(N'dbo.Game'))
    ALTER TABLE dbo.Game WITH CHECK
        ADD CONSTRAINT CK_Game_Score CHECK (Score IS NULL OR (Score >= 0 AND Score <= 10));
GO
