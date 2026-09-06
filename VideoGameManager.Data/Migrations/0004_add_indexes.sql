/*
 * 0004 - indexes for the queries the application actually runs.
 *
 * WHAT AND WHY, one line each:
 *
 *   IX_Game_Name               the list is ordered by name and searched by it.
 *   IX_Game_Score              "top rated" pages read the score descending; Id
 *                              is carried along so paging has a stable tie-break.
 *   IX_Game_GenreId            filtering by genre, then ordering the hits by name.
 *   IX_GamePlatform_PlatformId the link table is stored by (GameId, PlatformId),
 *                              so asking "which games run on this platform" has
 *                              no useful order without the columns reversed.
 *   IX_Review_GameId_CreatedAt a game's reviews, newest first; Id descending
 *                              breaks ties between reviews saved in the same
 *                              millisecond.
 *
 * Each one is guarded so a replay against a database that already has them does
 * nothing rather than failing.
 */

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Game_Name' AND object_id = OBJECT_ID(N'dbo.Game'))
    CREATE INDEX IX_Game_Name ON dbo.Game (Name);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Game_Score' AND object_id = OBJECT_ID(N'dbo.Game'))
    CREATE INDEX IX_Game_Score ON dbo.Game (Score DESC, Id);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Game_GenreId' AND object_id = OBJECT_ID(N'dbo.Game'))
    CREATE INDEX IX_Game_GenreId ON dbo.Game (GenreId, Name);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_GamePlatform_PlatformId' AND object_id = OBJECT_ID(N'dbo.GamePlatform'))
    CREATE INDEX IX_GamePlatform_PlatformId ON dbo.GamePlatform (PlatformId, GameId);
GO

IF NOT EXISTS (SELECT 1 FROM sys.indexes
               WHERE name = N'IX_Review_GameId_CreatedAt' AND object_id = OBJECT_ID(N'dbo.Review'))
    CREATE INDEX IX_Review_GameId_CreatedAt ON dbo.Review (GameId, CreatedAt DESC, Id DESC);
GO
