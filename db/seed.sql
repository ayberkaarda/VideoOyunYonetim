/*
 * Video Game Manager - seed data
 * ----------------------------
 * This script is idempotent: it only inserts rows that are missing.
 *
 * Usage:
 *   sqlcmd -S localhost,1433 -U sa -P <password> -C -d VideoGameManager -i db/seed.sql
 *
 * The 15 rows were extracted from the data pages of the SQL Server backup
 * that used to live in this repository. Genre values and the two reviews were
 * translated from Turkish along with the rest of the application.
 *
 * One deviation: the review stored for FIFA 24 in the backup was keyboard-mash
 * placeholder text, so that row's earlier review (also found in the backup) is
 * used instead.
 */

USE [VideoGameManager];
GO

SET NOCOUNT ON;
SET IDENTITY_INSERT dbo.Game ON;

;WITH Source (Id, Name, Genre, [Platform], Score, CoverUrl, Comment) AS
(
    SELECT * FROM (VALUES
        ( 1, N'The Witcher 3: Wild Hunt',        N'RPG',        N'PC',          9.5, N'https://image.api.playstation.com/vulcan/ap/rnd/202211/0711/kh4MUIuMmHlktOHar3lVl6rY.png', CAST(NULL AS NVARCHAR(MAX))),
        ( 2, N'God of War',                      N'Action',     N'PlayStation', 9.8, N'https://cdn1.epicgames.com/offer/3ddd6a590da64e3686042d108968a6b2/EGS_GodofWar_SantaMonicaStudio_S2_1200x1600-fbdf3cbc2980749091d52751ffabb7b7_1200x1600-fbdf3cbc2980749091d52751ffabb7b7', NULL),
        ( 3, N'Minecraft',                       N'Sandbox',    N'PC',          8.9, N'https://image.api.playstation.com/vulcan/ap/rnd/202407/0401/670c294ded3baf4fa11068db2ec6758c63f7daeb266a35a1.png', NULL),
        ( 4, N'FIFA 24',                         N'Sports',     N'PlayStation', 7.5, N'https://gamefix.store/image/cache/catalog/photo-output-600x800.jpeg', N'Not realistic at all.'),
        ( 5, N'Hades',                           N'Roguelike',  N'Switch',      9.0, N'https://cdn.mos.cms.futurecdn.net/8pJ6jtcbVmYK2PwGXTo7pJ.jpg', NULL),
        ( 6, N'Alan Wake 2',                     N'Action',     N'PS5',         9.0, N'https://cdn1.epicgames.com/offer/c4763f236d08423eb47b4c3008779c84/EGS_AlanWake2_RemedyEntertainment_S2_1200x1600-c7c8091ddac0f9669c8e5905bca88aaa', NULL),
        ( 7, N'Battlefield 1',                   N'Action',     N'PC',          8.0, N'https://image.api.playstation.com/gs2-sec/appkgo/prod/CUSA02387_00/4/i_4f0735372ec946403df8905f3f520cfcec79b433a7fe90758500a025c3401d34/i/icon0_01.png', NULL),
        ( 8, N'Forza Horizon 5',                 N'Racing',     N'Xbox',        9.2, N'https://oyuncustore.net/wp-content/uploads/2021/11/forza5delux-1.jpg', N'Fewer classic cars than in the previous games.'),
        ( 9, N'Hollow Knight',                   N'Platformer', N'PC',          9.0, N'https://upload.wikimedia.org/wikipedia/en/thumb/0/04/Hollow_Knight_first_cover_art.webp/274px-Hollow_Knight_first_cover_art.webp.png', NULL),
        (10, N'Resident Evil Village',           N'Horror',     N'PlayStation', 8.8, N'https://image.api.playstation.com/vulcan/ap/rnd/202207/0706/D8YACd9U8RAcdtOVpXeXDpzg.png', NULL),
        (11, N'Age of Empires IV',               N'Strategy',   N'PC',          8.6, N'https://upload.wikimedia.org/wikipedia/tr/9/91/Age_of_Empires_4_kapak.jpg', NULL),
        (12, N'Super Mario Odyssey',             N'Adventure',  N'Switch',      9.5, N'https://cdn.akakce.com/nintendo/nintendo-super-mario-odyssey-z.jpg', NULL),
        (13, N'Celeste',                         N'Platformer', N'PC',          8.9, N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/0f/Celeste_box_art_full.png/1200px-Celeste_box_art_full.png', NULL),
        (14, N'Call of Duty: Modern Warfare II', N'Action',     N'Xbox',        8.2, N'https://upload.wikimedia.org/wikipedia/tr/c/c1/Modern_Warfare_2_kapak.PNG', NULL),
        (15, N'Stardew Valley',                  N'Simulation', N'Switch',      9.1, N'https://upload.wikimedia.org/wikipedia/tr/thumb/f/fd/Logo_of_Stardew_Valley.png/800px-Logo_of_Stardew_Valley.png', NULL)
    ) AS v (Id, Name, Genre, [Platform], Score, CoverUrl, Comment)
)
INSERT INTO dbo.Game (Id, Name, Genre, [Platform], Score, CoverUrl, Comment)
SELECT s.Id, s.Name, s.Genre, s.[Platform], s.Score, s.CoverUrl, s.Comment
FROM   Source AS s
WHERE  NOT EXISTS (SELECT 1 FROM dbo.Game AS g WHERE g.Id = s.Id);

DECLARE @inserted INT = @@ROWCOUNT;

SET IDENTITY_INSERT dbo.Game OFF;

/* Realign the IDENTITY seed with the highest existing Id. */
DBCC CHECKIDENT (N'dbo.Game', RESEED) WITH NO_INFOMSGS;

PRINT CONCAT(N'Rows inserted: ', @inserted);
GO
