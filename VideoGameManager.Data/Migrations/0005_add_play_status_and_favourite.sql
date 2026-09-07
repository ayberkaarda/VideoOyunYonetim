/*
 * 0005 - track how far a game has been played, and which games are favourites.
 *
 * WHAT
 *   Adds two columns to dbo.Game: Status, a small number saying whether a game is
 *   in the backlog (0), being played (1) or finished (2), and IsFavourite, a flag
 *   the owner sets by hand. Both are NOT NULL with a default, both get a check or
 *   a type that keeps them honest, and two indexes serve the listings that filter
 *   on them.
 *
 * WHY THE DEFAULTS ARE WHAT THEY ARE
 *   A catalogue kept before these columns existed says nothing about what its
 *   owner played or liked. Backlog and "not a favourite" are the only answers that
 *   invent nothing, and because they are also the zero of each type, every row that
 *   already exists is filled in by the default rather than by a guess.
 *
 *   Nothing else about an existing row is touched: no row is removed, no value is
 *   rewritten and no order is changed.
 *
 * WHY THE CONSTRAINT IS SEPARATE FROM THE COLUMN
 *   T-SQL resolves column names when it compiles a batch, not when it reaches the
 *   IF that guards a statement. A constraint naming Status in the same batch that
 *   adds Status would therefore fail to compile on a database where the column is
 *   still absent. GO ends the batch, so the constraint is compiled only once the
 *   column is really there.
 *
 * WHY IT IS GUARDED AT ALL
 *   Installations that predate the journal replay the whole chain on their first
 *   migration, so every step checks whether its work is already done rather than
 *   assuming a database nobody has touched.
 *
 * THE INDEXES
 *   IX_Game_Status       listing one play state, in name order.
 *   IX_Game_IsFavourite  the favourites list, in name order. Two-valued columns
 *                        make poor leading keys in general, but this index is far
 *                        narrower than the table it spares, so even a scan of it
 *                        reads a fraction of the pages the clustered index would.
 */

IF COL_LENGTH(N'dbo.Game', N'Status') IS NULL
BEGIN
    PRINT N'0005: adding column dbo.Game.Status...';

    /* TINYINT holds 0 to 255, which is far more than the three states need and is
       the smallest type that can carry them. The default is applied to the rows
       that are already there as the column is created. */
    ALTER TABLE dbo.Game
        ADD [Status] TINYINT NOT NULL
            CONSTRAINT DF_Game_Status DEFAULT (0);
END
ELSE
    PRINT N'0005: column dbo.Game.Status is already present, nothing to do.';
GO

IF COL_LENGTH(N'dbo.Game', N'IsFavourite') IS NULL
BEGIN
    PRINT N'0005: adding column dbo.Game.IsFavourite...';

    ALTER TABLE dbo.Game
        ADD IsFavourite BIT NOT NULL
            CONSTRAINT DF_Game_IsFavourite DEFAULT (0);
END
ELSE
    PRINT N'0005: column dbo.Game.IsFavourite is already present, nothing to do.';
GO

/* WITH CHECK, so the constraint validates the rows already there instead of
   trusting them, and so the optimiser may rely on it. The three numbers are the
   storage format of the play state: a fourth would need this list widened here
   before any code could write it. */
IF NOT EXISTS (SELECT 1 FROM sys.check_constraints
               WHERE name = N'CK_Game_Status' AND parent_object_id = OBJECT_ID(N'dbo.Game'))
    ALTER TABLE dbo.Game WITH CHECK
        ADD CONSTRAINT CK_Game_Status CHECK ([Status] IN (0, 1, 2));
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Game_Status' AND object_id = OBJECT_ID(N'dbo.Game'))
    CREATE INDEX IX_Game_Status ON dbo.Game ([Status], Name);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Game_IsFavourite' AND object_id = OBJECT_ID(N'dbo.Game'))
    CREATE INDEX IX_Game_IsFavourite ON dbo.Game (IsFavourite, Name);
GO
