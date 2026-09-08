/*
 * Video Game Manager - seed data
 * ------------------------------
 * This script is idempotent: it only inserts rows that are missing.
 *
 * It expects the normalised schema, so it runs AFTER the migrator has created
 * the tables. Against an empty database on its own it fails, because none of
 * the tables it writes to exist yet.
 *
 * Usage:
 *   dotnet run --project VideoGameManager.Migrator -- \
 *     "Server=localhost,1433;Database=VideoGameManager;User Id=sa;Password=<password>;TrustServerCertificate=True"
 *   sqlcmd -S localhost,1433 -U sa -P <password> -C -d VideoGameManager -f 65001 -i db/seed.sql
 *
 * `-f 65001` is not optional. This file is UTF-8 and sqlcmd does not assume
 * that; without the flag the non-ASCII characters are stored corrupted and no
 * error is raised.
 *
 * There is no USE statement: the target comes from `-d`, so the same file can
 * seed a throwaway database without editing.
 *
 * The 15 rows were extracted from the data pages of the SQL Server backup that
 * used to live in this repository. Genre values and the two reviews were
 * translated from Turkish along with the rest of the application.
 *
 * Two deviations from that backup were deliberate:
 *
 *   1. Review text is NVARCHAR(MAX) in its own Review table. The backup held it
 *      in a single `TEXT` column on the game row -- a deprecated non-Unicode
 *      type that mangled non-ASCII characters, and one opinion per game.
 *
 *   2. The review stored for FIFA 24 in the backup was keyboard-mash
 *      placeholder text, so that row's earlier review (also found in the
 *      backup) is used instead.
 *
 * Review scores are left NULL: the backup recorded review text and nothing
 * else, and the game's own Score is not the score of the person who wrote that
 * text.
 */

SET NOCOUNT ON;

/* The catalogue in one place, so the game rows, the two lookup tables and the
   link rows are all built from the same list. */
DECLARE @Source TABLE
(
    Id         INT           NOT NULL PRIMARY KEY,
    Name       NVARCHAR(100) NOT NULL,
    Genre      NVARCHAR(50)  NOT NULL,
    [Platform] NVARCHAR(50)  NOT NULL,
    Score      FLOAT         NULL,
    CoverUrl   NVARCHAR(MAX) NULL
);

INSERT INTO @Source (Id, Name, Genre, [Platform], Score, CoverUrl) VALUES
    ( 1, N'The Witcher 3: Wild Hunt',        N'RPG',        N'PC',          9.5, N'https://image.api.playstation.com/vulcan/ap/rnd/202211/0711/kh4MUIuMmHlktOHar3lVl6rY.png'),
    ( 2, N'God of War',                      N'Action',     N'PlayStation', 9.8, N'https://cdn1.epicgames.com/offer/3ddd6a590da64e3686042d108968a6b2/EGS_GodofWar_SantaMonicaStudio_S2_1200x1600-fbdf3cbc2980749091d52751ffabb7b7_1200x1600-fbdf3cbc2980749091d52751ffabb7b7'),
    ( 3, N'Minecraft',                       N'Sandbox',    N'PC',          8.9, N'https://image.api.playstation.com/vulcan/ap/rnd/202407/0401/670c294ded3baf4fa11068db2ec6758c63f7daeb266a35a1.png'),
    ( 4, N'FIFA 24',                         N'Sports',     N'PlayStation', 7.5, N'https://cdn.cloudflare.steamstatic.com/steam/apps/2195250/library_600x900.jpg'),
    ( 5, N'Hades',                           N'Roguelike',  N'Switch',      9.0, N'https://cdn.mos.cms.futurecdn.net/8pJ6jtcbVmYK2PwGXTo7pJ.jpg'),
    ( 6, N'Alan Wake 2',                     N'Action',     N'PS5',         9.0, N'https://cdn1.epicgames.com/offer/c4763f236d08423eb47b4c3008779c84/EGS_AlanWake2_RemedyEntertainment_S2_1200x1600-c7c8091ddac0f9669c8e5905bca88aaa'),
    ( 7, N'Battlefield 1',                   N'Action',     N'PC',          8.0, N'https://image.api.playstation.com/gs2-sec/appkgo/prod/CUSA02387_00/4/i_4f0735372ec946403df8905f3f520cfcec79b433a7fe90758500a025c3401d34/i/icon0_01.png'),
    ( 8, N'Forza Horizon 5',                 N'Racing',     N'Xbox',        9.2, N'https://oyuncustore.net/wp-content/uploads/2021/11/forza5delux-1.jpg'),
    ( 9, N'Hollow Knight',                   N'Platformer', N'PC',          9.0, N'https://cdn.cloudflare.steamstatic.com/steam/apps/367520/library_600x900.jpg'),
    (10, N'Resident Evil Village',           N'Horror',     N'PlayStation', 8.8, N'https://image.api.playstation.com/vulcan/ap/rnd/202207/0706/D8YACd9U8RAcdtOVpXeXDpzg.png'),
    (11, N'Age of Empires IV',               N'Strategy',   N'PC',          8.6, N'https://upload.wikimedia.org/wikipedia/tr/9/91/Age_of_Empires_4_kapak.jpg'),
    (12, N'Super Mario Odyssey',             N'Adventure',  N'Switch',      9.5, N'https://cdn.akakce.com/nintendo/nintendo-super-mario-odyssey-z.jpg'),
    (13, N'Celeste',                         N'Platformer', N'PC',          8.9, N'https://upload.wikimedia.org/wikipedia/commons/0/0f/Celeste_box_art_full.png'),
    (14, N'Call of Duty: Modern Warfare II', N'Action',     N'Xbox',        8.2, N'https://upload.wikimedia.org/wikipedia/tr/c/c1/Modern_Warfare_2_kapak.PNG'),
    (15, N'Stardew Valley',                  N'Simulation', N'Switch',      9.1, N'https://upload.wikimedia.org/wikipedia/tr/f/fd/Logo_of_Stardew_Valley.png');

/* Lookup rows first: a game cannot point at a genre that is not there yet. */
INSERT INTO dbo.Genre (Name)
SELECT DISTINCT s.Genre
FROM   @Source AS s
WHERE  NOT EXISTS (SELECT 1 FROM dbo.Genre AS existing WHERE existing.Name = s.Genre);

INSERT INTO dbo.[Platform] (Name)
SELECT DISTINCT s.[Platform]
FROM   @Source AS s
WHERE  NOT EXISTS (SELECT 1 FROM dbo.[Platform] AS existing WHERE existing.Name = s.[Platform]);

/* The Ids are pinned so the rows keep the identities the backup gave them and a
   re-run recognises what is already there. */
SET IDENTITY_INSERT dbo.Game ON;

INSERT INTO dbo.Game (Id, Name, GenreId, Score, CoverUrl)
SELECT     s.Id, s.Name, genre.Id, s.Score, s.CoverUrl
FROM       @Source  AS s
INNER JOIN dbo.Genre AS genre ON genre.Name = s.Genre
WHERE      NOT EXISTS (SELECT 1 FROM dbo.Game AS existing WHERE existing.Id = s.Id);

DECLARE @inserted INT = @@ROWCOUNT;

SET IDENTITY_INSERT dbo.Game OFF;

/* Realign the IDENTITY seed with the highest existing Id. */
DBCC CHECKIDENT (N'dbo.Game', RESEED) WITH NO_INFOMSGS;

INSERT INTO dbo.GamePlatform (GameId, PlatformId)
SELECT     s.Id, p.Id
FROM       @Source        AS s
INNER JOIN dbo.[Platform] AS p ON p.Name = s.[Platform]
WHERE      EXISTS (SELECT 1 FROM dbo.Game AS g WHERE g.Id = s.Id)
  AND      NOT EXISTS (SELECT 1 FROM dbo.GamePlatform AS link
                       WHERE link.GameId = s.Id AND link.PlatformId = p.Id);

/* The two reviews the backup held, each attached to the game it was written
   for. A game that already has a review is left alone. */
DECLARE @Reviews TABLE
(
    GameId INT           NOT NULL PRIMARY KEY,
    Body   NVARCHAR(MAX) NOT NULL
);

INSERT INTO @Reviews (GameId, Body) VALUES
    (4, N'Not realistic at all.'),
    (8, N'Fewer classic cars than in the previous games.');

INSERT INTO dbo.Review (GameId, Score, Body, CreatedAt)
SELECT r.GameId, NULL, r.Body, SYSDATETIMEOFFSET()
FROM   @Reviews AS r
WHERE  EXISTS (SELECT 1 FROM dbo.Game AS g WHERE g.Id = r.GameId)
  AND  NOT EXISTS (SELECT 1 FROM dbo.Review AS existing WHERE existing.GameId = r.GameId);

PRINT CONCAT(N'Rows inserted: ', @inserted);
GO
