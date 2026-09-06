/*
 * VideoOyunYonetim — veritabanı şeması / database schema
 * -----------------------------------------------------
 * Bu script idempotenttir: birden fazla kez çalıştırılabilir.
 * This script is idempotent: it can be run more than once.
 *
 * Kullanım / Usage:
 *   sqlcmd -S <sunucu\instance> -E -i db/schema.sql
 *
 * Not: Şema, depoda daha önce bulunan VideoOyun.bak yedeğinden çıkarılmıştır.
 * Note: The schema was reverse-engineered from the VideoOyun.bak backup that
 *       used to live in this repository.
 *
 * Bilinçli sapma / Intentional deviation:
 *   Orijinal yedekte `Yorum` sütunu `TEXT` tipindeydi. `TEXT` kullanımdan
 *   kaldırılmış ve Unicode desteklemeyen bir tiptir; Türkçe karakterler
 *   veritabanı kod sayfasına göre bozulabiliyordu. Burada NVARCHAR(MAX)
 *   kullanıldı — uygulama kodunda değişiklik gerektirmez.
 *   The original backup declared `Yorum` as `TEXT`, a deprecated non-Unicode
 *   type that mangled Turkish characters. It is NVARCHAR(MAX) here; no
 *   application change is required.
 */

IF DB_ID(N'VideoOyun') IS NULL
BEGIN
    PRINT N'VideoOyun veritabanı oluşturuluyor... / Creating database VideoOyun...';
    EXEC (N'CREATE DATABASE [VideoOyun] COLLATE Turkish_CI_AS;');
END
ELSE
    PRINT N'VideoOyun veritabanı zaten mevcut. / Database VideoOyun already exists.';
GO

USE [VideoOyun];
GO

IF OBJECT_ID(N'dbo.Oyunlar', N'U') IS NULL
BEGIN
    PRINT N'dbo.Oyunlar tablosu oluşturuluyor... / Creating table dbo.Oyunlar...';

    CREATE TABLE dbo.Oyunlar
    (
        Id        INT            IDENTITY(1, 1) NOT NULL,
        Ad        NVARCHAR(100)  NULL,
        Tur       NVARCHAR(50)   NULL,
        [Platform] NVARCHAR(50)  NULL,
        Puan      FLOAT          NULL,
        ResimLink NVARCHAR(MAX)  NULL,
        Yorum     NVARCHAR(MAX)  NULL,
        CONSTRAINT PK_Oyunlar PRIMARY KEY CLUSTERED (Id)
    );
END
ELSE
    PRINT N'dbo.Oyunlar tablosu zaten mevcut. / Table dbo.Oyunlar already exists.';
GO

/* Eski kurulumlar için eksik sütunları tamamla.
   Backfill columns that older installations may be missing. */
IF COL_LENGTH(N'dbo.Oyunlar', N'ResimLink') IS NULL
    ALTER TABLE dbo.Oyunlar ADD ResimLink NVARCHAR(MAX) NULL;
GO

IF COL_LENGTH(N'dbo.Oyunlar', N'Yorum') IS NULL
    ALTER TABLE dbo.Oyunlar ADD Yorum NVARCHAR(MAX) NULL;
GO

PRINT N'Şema hazır. / Schema ready.';
GO
