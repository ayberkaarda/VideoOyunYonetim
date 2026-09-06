/*
 * VideoOyunYonetim — örnek veri / seed data
 * ----------------------------------------
 * Bu script idempotenttir: yalnızca eksik satırları ekler.
 * This script is idempotent: it only inserts rows that are missing.
 *
 * Kullanım / Usage (dosya UTF-8'dir, -f 65001 gereklidir):
 *   sqlcmd -S <sunucu\instance> -E -d VideoOyun -i db/seed.sql -f 65001
 *
 * Veri, depoda daha önce bulunan VideoOyun.bak yedeğinin veri sayfalarından
 * birebir çıkarılmıştır (15 satır, Id 1-15).
 * The data was extracted verbatim from the data pages of the VideoOyun.bak
 * backup that used to live in this repository (15 rows, Id 1-15).
 *
 * Tek sapma / Single deviation:
 *   Yedekteki FIFA 24 yorumu klavye karalaması bir test metniydi
 *   ("POASPODPOSAIDSAO..."). Yerine aynı satırın yedekte bulunan bir önceki
 *   yorumu ("hiç gerçekçi değil") kullanıldı.
 *   The FIFA 24 review in the backup was keyboard-mash placeholder text; it was
 *   replaced with that row's earlier review found in the same backup.
 */

USE [VideoOyun];
GO

SET NOCOUNT ON;
SET IDENTITY_INSERT dbo.Oyunlar ON;

;WITH Kaynak (Id, Ad, Tur, [Platform], Puan, ResimLink, Yorum) AS
(
    SELECT * FROM (VALUES
        ( 1, N'The Witcher 3: Wild Hunt',        N'RPG',        N'PC',          9.5, N'https://image.api.playstation.com/vulcan/ap/rnd/202211/0711/kh4MUIuMmHlktOHar3lVl6rY.png', CAST(NULL AS NVARCHAR(MAX))),
        ( 2, N'God of War',                      N'Aksiyon',    N'PlayStation', 9.8, N'https://cdn1.epicgames.com/offer/3ddd6a590da64e3686042d108968a6b2/EGS_GodofWar_SantaMonicaStudio_S2_1200x1600-fbdf3cbc2980749091d52751ffabb7b7_1200x1600-fbdf3cbc2980749091d52751ffabb7b7', NULL),
        ( 3, N'Minecraft',                       N'Sandbox',    N'PC',          8.9, N'https://image.api.playstation.com/vulcan/ap/rnd/202407/0401/670c294ded3baf4fa11068db2ec6758c63f7daeb266a35a1.png', NULL),
        ( 4, N'FIFA 24',                         N'Spor',       N'PlayStation', 7.5, N'https://gamefix.store/image/cache/catalog/photo-output-600x800.jpeg', N'hiç gerçekçi değil'),
        ( 5, N'Hades',                           N'Roguelike',  N'Switch',      9.0, N'https://cdn.mos.cms.futurecdn.net/8pJ6jtcbVmYK2PwGXTo7pJ.jpg', NULL),
        ( 6, N'Alan Wake 2',                     N'Aksiyon',    N'PS5',         9.0, N'https://cdn1.epicgames.com/offer/c4763f236d08423eb47b4c3008779c84/EGS_AlanWake2_RemedyEntertainment_S2_1200x1600-c7c8091ddac0f9669c8e5905bca88aaa', NULL),
        ( 7, N'Battlefield 1',                   N'Aksiyon',    N'PC',          8.0, N'https://image.api.playstation.com/gs2-sec/appkgo/prod/CUSA02387_00/4/i_4f0735372ec946403df8905f3f520cfcec79b433a7fe90758500a025c3401d34/i/icon0_01.png', NULL),
        ( 8, N'Forza Horizon 5',                 N'Yarış',      N'Xbox',        9.2, N'https://oyuncustore.net/wp-content/uploads/2021/11/forza5delux-1.jpg', N'Eski araç sayısında önceki oyunlara göre azalma mevcut.'),
        ( 9, N'Hollow Knight',                   N'Platform',   N'PC',          9.0, N'https://upload.wikimedia.org/wikipedia/en/thumb/0/04/Hollow_Knight_first_cover_art.webp/274px-Hollow_Knight_first_cover_art.webp.png', NULL),
        (10, N'Resident Evil Village',           N'Korku',      N'PlayStation', 8.8, N'https://image.api.playstation.com/vulcan/ap/rnd/202207/0706/D8YACd9U8RAcdtOVpXeXDpzg.png', NULL),
        (11, N'Age of Empires IV',               N'Strateji',   N'PC',          8.6, N'https://upload.wikimedia.org/wikipedia/tr/9/91/Age_of_Empires_4_kapak.jpg', NULL),
        (12, N'Super Mario Odyssey',             N'Macera',     N'Switch',      9.5, N'https://cdn.akakce.com/nintendo/nintendo-super-mario-odyssey-z.jpg', NULL),
        (13, N'Celeste',                         N'Platform',   N'PC',          8.9, N'https://upload.wikimedia.org/wikipedia/commons/thumb/0/0f/Celeste_box_art_full.png/1200px-Celeste_box_art_full.png', NULL),
        (14, N'Call of Duty: Modern Warfare II', N'Aksiyon',    N'Xbox',        8.2, N'https://upload.wikimedia.org/wikipedia/tr/c/c1/Modern_Warfare_2_kapak.PNG', NULL),
        (15, N'Stardew Valley',                  N'Simülasyon', N'Switch',      9.1, N'https://upload.wikimedia.org/wikipedia/tr/thumb/f/fd/Logo_of_Stardew_Valley.png/800px-Logo_of_Stardew_Valley.png', NULL)
    ) AS v (Id, Ad, Tur, [Platform], Puan, ResimLink, Yorum)
)
INSERT INTO dbo.Oyunlar (Id, Ad, Tur, [Platform], Puan, ResimLink, Yorum)
SELECT k.Id, k.Ad, k.Tur, k.[Platform], k.Puan, k.ResimLink, k.Yorum
FROM   Kaynak AS k
WHERE  NOT EXISTS (SELECT 1 FROM dbo.Oyunlar AS o WHERE o.Id = k.Id);

DECLARE @eklenen INT = @@ROWCOUNT;

SET IDENTITY_INSERT dbo.Oyunlar OFF;

/* IDENTITY tohumunu mevcut en yüksek Id'ye hizala.
   Realign the IDENTITY seed with the highest existing Id. */
DBCC CHECKIDENT (N'dbo.Oyunlar', RESEED) WITH NO_INFOMSGS;

PRINT CONCAT(N'Eklenen satır / rows inserted: ', @eklenen);
GO
