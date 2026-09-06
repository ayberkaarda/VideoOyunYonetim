<div align="center">

# 🎮 VideoOyunYonetim

**Video oyunlarını kataloglamak, puanlamak ve keşfetmek için bir Windows masaüstü uygulaması.**

[![Platform](https://img.shields.io/badge/platform-Windows-0078D6)](#)
[![Dil](https://img.shields.io/badge/C%23-.NET%2010-512BD4)](#)
[![Veritabanı](https://img.shields.io/badge/veritaban%C4%B1-SQL%20Server-CC2927)](#)
[![Lisans](https://img.shields.io/badge/lisans-MIT-green)](LICENSE)

[English](README.md) · **Türkçe**

<img src="screenshots/anasayfa.png" alt="Ana menü" width="620">

</div>

---

## Genel Bakış

VideoOyunYonetim, arka planında SQL Server çalışan bir Windows Forms uygulamasıdır.
Oyunları tür, platform, puan ve kapak görseli ile kişisel kataloğunuza eklersiniz;
kataloğu tüm detaylarıyla gezersiniz; herhangi bir oyuna yazılı yorum bırakırsınız ve
ne oynayacağınıza karar veremediğinizde rastgele bir öneri alırsınız.

> **Not** — Deponun varsayılan README dosyası İngilizcedir: [README.md](README.md).

## Özellikler

| | |
|---|---|
| ➕ **Oyun ekleme** | İsim, tür, platform, puan (1–10) ve kapak görseli bağlantısı |
| 📃 **Listeleme ve detay** | Listeden seçtiğiniz oyunun tüm alanlarını ve kapak görselini görün |
| 🗣️ **Değerlendirme** | Katalogdaki herhangi bir oyuna yazılı yorum ekleyin |
| 🎲 **Öneri** | Katalogdan rastgele bir oyunu kapağıyla birlikte çekin |
| 🪟 **Özel pencere** | Kenarlıksız formlar, elle yazılmış küçült/kapat düğmeleri |

## Ekranlar

<table>
<tr>
<td width="50%">

**Oyun ekleme** — `OyunEkleForm`

<img src="screenshots/oyun_ekle.png" alt="Oyun ekleme ekranı" width="100%">

</td>
<td width="50%">

**Oyunları listeleme** — `OyunListeleForm`

<img src="screenshots/oyunlari_listele.png" alt="Oyun listeleme ekranı" width="100%">

</td>
</tr>
<tr>
<td width="50%">

**Oyun önerisi** — `OyunOneriForm`

<img src="screenshots/oyun_oneri.png" alt="Oyun öneri ekranı" width="100%">

</td>
<td width="50%">

**Oyun değerlendirme** — `OyunDegerlendirForm`

<img src="screenshots/oyun_degerlendir.png" alt="Oyun değerlendirme ekranı" width="100%">

</td>
</tr>
</table>

## Kullanılan Teknolojiler

- **C# / Windows Forms**, **.NET 10** üzerinde (SDK-style proje)
- Veri saklama için **SQL Server** — depoda hazır bir Docker Compose dosyası var
- Parametreli komutlarla [`Microsoft.Data.SqlClient`](https://github.com/dotnet/SqlClient) üzerinden **ADO.NET**

## Kurulum

### Gereksinimler

- Windows 10/11
- [.NET 10 SDK](https://dotnet.microsoft.com/download/dotnet/10.0) — `winget install Microsoft.DotNet.SDK.10`
- Docker Desktop (önerilen) **veya** halihazırda kullandığınız herhangi bir SQL Server örneği
- `sqlcmd` — `winget install Microsoft.Sqlcmd`

Visual Studio zorunlu değil; solution tamamen komut satırından derlenip çalıştırılabilir.

### 1. SQL Server'ı başlatın

```powershell
# bir şifre belirleyip db/.env dosyasına yazın (git tarafından yok sayılır)
Copy-Item db/.env.example db/.env    # sonra şifreyi düzenleyin

docker compose -f db/docker-compose.yml up -d
docker compose -f db/docker-compose.yml ps    # "healthy" olmasını bekleyin
```

Zaten çalışan bir SQL Server'ınız mı var (Express, Developer, LocalDB)? Bu adımı atlayıp
aşağıdaki komutlarda kendi sunucu adınızı kullanın.

### 2. Veritabanını oluşturun

Veritabanı, sürüm kontrolündeki SQL script'lerinden oluşturulur. Her ikisi de idempotenttir;
tekrar çalıştırmak güvenlidir.

```powershell
$sa = (Get-Content db/.env | Select-String 'MSSQL_SA_PASSWORD=(.*)').Matches.Groups[1].Value

# şema — VideoOyun veritabanını ve Oyunlar tablosunu oluşturur
sqlcmd -S localhost,1433 -U sa -P $sa -C -i db/schema.sql

# örnek veri — 15 oyun, yalnızca eksik satırları ekler
sqlcmd -S localhost,1433 -U sa -P $sa -C -d VideoOyun -i db/seed.sql -f 65001

# doğrulama
sqlcmd -S localhost,1433 -U sa -P $sa -C -d VideoOyun -Q "SELECT COUNT(*) FROM dbo.Oyunlar"   # 15
```

`-f 65001` bayrağı `seed.sql` dosyasının UTF-8 olduğunu `sqlcmd`'ye bildirir; Türkçe
karakterlerin bozulmamasını sağlar. Arayüz tercih ederseniz her iki dosyayı SQL Server
Management Studio veya Azure Data Studio ile açıp bu sırayla çalıştırın.

### 3. Uygulamayı sunucunuza yönlendirin

[`VideoOyunY/DatabaseHelper.cs`](VideoOyunY/DatabaseHelper.cs) dosyasını açın ve bağlantı
cümlesini ayarlayın:

```csharp
private static string connectionString =
    "Server=localhost,1433;Database=VideoOyun;User Id=sa;Password=SIFRENIZ;TrustServerCertificate=True;";
```

> Bağlantı cümlesi şu anda kod içine gömülüdür. `appsettings.json` dosyasına taşınması
> [yol haritasının](#yol-haritası) 2. fazında planlanmıştır.

### 4. Derleyin ve çalıştırın

```powershell
# depo kök dizininden
dotnet build VideoOyunY.sln
dotnet run --project VideoOyunY
```

Ya da `VideoOyunY.sln` dosyasını Visual Studio ile açıp <kbd>F5</kbd> tuşuna basın.

## Depo Yapısı

```
VideoOyunYonetim/
├── db/
│   ├── docker-compose.yml    # yerel SQL Server 2022 konteyneri
│   ├── .env.example          # SA şifresi için şablon
│   ├── schema.sql            # veritabanı + tablo tanımı (idempotent)
│   └── seed.sql              # 15 örnek oyun (idempotent)
├── screenshots/              # README dosyalarının kullandığı görseller
├── VideoOyunY/
│   ├── Program.cs            # giriş noktası
│   ├── Form1.cs              # ana menü
│   ├── OyunEkleForm.cs       # oyun ekleme
│   ├── OyunListeleForm.cs    # katalog listeleme
│   ├── OyunOneriForm.cs      # rastgele öneri
│   ├── OyunDegerlendirForm.cs# yorum yazma
│   ├── DatabaseHelper.cs     # ADO.NET yardımcısı
│   ├── Oyun.cs               # oyun modeli
│   └── Oyuncu.cs             # oyuncu modeli
└── VideoOyunY.sln
```

### Veritabanı Şeması

| Sütun | Tip | Açıklama |
|---|---|---|
| `Id` | `INT IDENTITY` | Birincil anahtar |
| `Ad` | `NVARCHAR(100)` | Oyun adı |
| `Tur` | `NVARCHAR(50)` | Tür |
| `Platform` | `NVARCHAR(50)` | PC / PlayStation / Xbox / Switch |
| `Puan` | `FLOAT` | Puan, 1–10 |
| `ResimLink` | `NVARCHAR(MAX)` | Kapak görseli bağlantısı |
| `Yorum` | `NVARCHAR(MAX)` | Kullanıcı yorumu |

## Yol Haritası

Proje, öğrenci seviyesindeki bir prototipten sürdürülebilir bir uygulamaya numaralı
fazlarla taşınıyor:

| Faz | Kapsam | Durum |
|---|---|---|
| 0 | Repo hijyeni — `.gitignore`, `.bak` yerine SQL script'leri, SDK-style .NET 10 projesi | ✅ tamamlandı |
| 1 | Katmanlı mimari — Domain / Data / Services / WinForms, MVP, bağımlılık enjeksiyonu | ⏳ planlandı |
| 2 | Yapılandırma ve hata yönetimi — `appsettings.json`, Serilog, doğrulama, `async/await` | ⏳ planlandı |
| 3 | Veritabanı — normalizasyon, indeksler, migration'lar | ⏳ planlandı |
| 4 | Testler — xUnit, FluentAssertions, NSubstitute | ⏳ planlandı |
| 5 | Özellikler — arama, sayfalama, gelişmiş öneri, resim önbelleği, dışa aktarma, istatistik | ⏳ planlandı |
| 6 | CI ve dokümantasyon — GitHub Actions, `.editorconfig`, `CHANGELOG.md` | ⏳ planlandı |

## Katkı

Issue ve pull request'ler memnuniyetle karşılanır. Lütfen her fazı ayrı bir commit'te
tutun ve PR açmadan önce solution'ın derlendiğinden emin olun.

## Geliştirici

**Ayberk Arda** — [@ayberkaarda](https://github.com/ayberkaarda)

## Lisans

[MIT Lisansı](LICENSE) ile yayımlanmıştır.
