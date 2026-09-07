using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using VideoGameManager.UI;
using Xunit;

namespace VideoGameManager.Tests.Desktop.UI
{
    /// <summary>
    /// Covers what the cover cache promises: artwork is scaled to the size the screens
    /// draw and never enlarged, a cache entry belongs to the size it was stored at, an
    /// address that failed is not asked for twice, and the memory cache stops growing.
    /// </summary>
    /// <remarks>
    /// Nothing here reaches the network. The provider takes its download step as a
    /// delegate, so every test feeds it byte arrays it built itself and points the disk
    /// cache at a temporary folder that is removed again afterwards.
    /// </remarks>
    public sealed class CachedCoverImageProviderTests : IDisposable
    {
        private readonly string _cacheDirectory;

        public CachedCoverImageProviderTests()
        {
            _cacheDirectory = Path.Combine(
                Path.GetTempPath(),
                "VideoGameManager.Tests.Desktop",
                Guid.NewGuid().ToString("n"));
        }

        public void Dispose()
        {
            try
            {
                if (Directory.Exists(_cacheDirectory))
                {
                    Directory.Delete(_cacheDirectory, recursive: true);
                }
            }
            catch (IOException)
            {
                // A leftover temporary folder is not worth failing a test run over.
            }
        }

        // ------------------------------------------------------------------
        // Scaling
        // ------------------------------------------------------------------

        [Fact]
        public async Task Oversized_artwork_is_scaled_down_without_changing_its_proportions()
        {
            byte[] original = PngOfSize(1000, 1500);

            using (CachedCoverImageProvider provider = ProviderReturning(original))
            {
                // The cache owns what it hands back; the test only reads it.
                Image? cover = await provider.GetCoverAsync("https://example.invalid/cover.png", CancellationToken.None);

                cover.Should().NotBeNull();

                // 1000x1500 into 480x848: the width is the binding edge, so the scale is
                // 0.48 and the height follows it rather than being stretched to the box.
                cover!.Width.Should().Be(480);
                cover.Height.Should().Be(720);

                double originalRatio = 1000d / 1500d;
                double storedRatio = (double)cover.Width / cover.Height;
                storedRatio.Should().BeApproximately(originalRatio, 0.01);
            }
        }

        [Fact]
        public async Task Artwork_that_is_already_small_enough_is_left_exactly_as_it_arrived()
        {
            byte[] original = PngOfSize(100, 150);
            const string reference = "https://example.invalid/small.png";

            using (CachedCoverImageProvider provider = ProviderReturning(original))
            {
                Image? cover = await provider.GetCoverAsync(reference, CancellationToken.None);

                cover.Should().NotBeNull();
                cover!.Width.Should().Be(100);
                cover.Height.Should().Be(150);
            }

            // Not re-encoded either: the bytes on disk are the ones that came down.
            string path = Path.Combine(
                _cacheDirectory,
                CachedCoverImageProvider.DiskCacheFileName(reference, 480, 848));

            File.ReadAllBytes(path).Should().Equal(original);
        }

        [Fact]
        public void Scaling_never_enlarges_and_always_keeps_the_ratio()
        {
            CachedCoverImageProvider.ScaleToFit(new Size(40, 60), 480, 848)
                .Should().Be(new Size(40, 60), "a picture smaller than the box is left alone");

            CachedCoverImageProvider.ScaleToFit(new Size(2000, 1000), 480, 848)
                .Should().Be(new Size(480, 240), "a landscape source is bound by its width");

            CachedCoverImageProvider.ScaleToFit(new Size(1000, 4000), 480, 848)
                .Should().Be(new Size(212, 848), "a very tall source is bound by its height");
        }

        // ------------------------------------------------------------------
        // Stored format
        // ------------------------------------------------------------------

        [Fact]
        public async Task Artwork_with_nothing_to_see_through_is_stored_lossily()
        {
            const string reference = "https://example.invalid/opaque.png";
            byte[] original = OpaqueDetailedPng(1200, 1600);

            using (CachedCoverImageProvider provider = ProviderReturning(original))
            {
                (await provider.GetCoverAsync(reference, CancellationToken.None)).Should().NotBeNull();
            }

            byte[] stored = File.ReadAllBytes(CachePathFor(reference));

            StoredFormatOf(stored).Should().Be(ImageFormat.Jpeg,
                "a cover with no transparency has nothing to gain from a lossless format");

            // The point of the exercise: a lossless copy of the very same scaled picture
            // is the thing this is meant to be smaller than. Comparing against the
            // download would prove nothing, because the download is a different size.
            stored.Length.Should().BeLessThan(LosslessSizeOf(stored) / 2,
                "photographic cover art is what this format exists for");
        }

        [Fact]
        public async Task Artwork_that_is_see_through_is_stored_losslessly_and_stays_see_through()
        {
            const string reference = "https://example.invalid/transparent.png";
            byte[] original = PngWithTransparentCorner(1200, 1600);

            using (CachedCoverImageProvider provider = ProviderReturning(original))
            {
                Image? cover = await provider.GetCoverAsync(reference, CancellationToken.None);

                cover.Should().NotBeNull();

                // Flattening this onto a solid background is the failure being guarded
                // against: it is invisible to a build and to every size assertion, and it
                // shows up on screen as a black or white block where the artwork was cut out.
                using (Bitmap drawn = new Bitmap(cover!))
                {
                    drawn.GetPixel(2, 2).A.Should().Be(0, "the transparent corner survived the round trip");
                }
            }

            byte[] stored = File.ReadAllBytes(CachePathFor(reference));

            StoredFormatOf(stored).Should().Be(ImageFormat.Png,
                "the lossy format cannot record transparency at all");
        }

        // ------------------------------------------------------------------
        // Entries the current build cannot read
        // ------------------------------------------------------------------

        [Fact]
        public async Task Cache_files_left_by_the_previous_naming_scheme_are_cleared_out()
        {
            Directory.CreateDirectory(_cacheDirectory);

            string firstOrphan = Path.Combine(_cacheDirectory, "0f9a2b.img");
            string secondOrphan = Path.Combine(_cacheDirectory, "c41d77.IMG");

            File.WriteAllBytes(firstOrphan, new byte[] { 1, 2, 3 });
            File.WriteAllBytes(secondOrphan, new byte[] { 4, 5, 6 });

            using (CachedCoverImageProvider provider = ProviderReturning(PngOfSize(64, 96)))
            {
                (await provider.GetCoverAsync("https://example.invalid/any.png", CancellationToken.None))
                    .Should().NotBeNull();
            }

            File.Exists(firstOrphan).Should().BeFalse();
            File.Exists(secondOrphan).Should().BeFalse("the extension is matched without regard to case");
        }

        [Fact]
        public async Task Clearing_out_stale_entries_touches_nothing_else()
        {
            Directory.CreateDirectory(_cacheDirectory);

            string nested = Path.Combine(_cacheDirectory, "nested");
            Directory.CreateDirectory(nested);

            string[] keep =
            {
                Path.Combine(_cacheDirectory, "3ab19c.cover"),
                Path.Combine(_cacheDirectory, "3ab19c.imgx"),
                Path.Combine(_cacheDirectory, "3ab19c.image"),
                Path.Combine(_cacheDirectory, "img"),
                Path.Combine(_cacheDirectory, "notes.txt"),
                Path.Combine(nested, "deep.img"),
            };

            foreach (string path in keep)
            {
                File.WriteAllBytes(path, new byte[] { 7 });
            }

            using (CachedCoverImageProvider provider = ProviderReturning(PngOfSize(64, 96)))
            {
                (await provider.GetCoverAsync("https://example.invalid/any.png", CancellationToken.None))
                    .Should().NotBeNull();
            }

            foreach (string path in keep)
            {
                File.Exists(path).Should().BeTrue(
                    "only files in this folder whose extension is exactly the stale one may be removed, and " +
                    path + " is not one of them");
            }
        }

        [Fact]
        public async Task A_cover_written_by_this_build_is_still_there_after_the_sweep()
        {
            const string reference = "https://example.invalid/kept.png";
            byte[] original = OpaqueDetailedPng(1200, 1600);

            // Written by one provider, so the sweep the next one runs meets a real entry
            // rather than a file the test invented.
            using (CachedCoverImageProvider first = ProviderReturning(original))
            {
                (await first.GetCoverAsync(reference, CancellationToken.None)).Should().NotBeNull();
            }

            string path = CachePathFor(reference);
            File.Exists(path).Should().BeTrue();

            int downloads = 0;

            using (CachedCoverImageProvider second = ProviderUsing((r, t) =>
            {
                downloads++;
                return Task.FromResult<byte[]?>(original);
            }))
            {
                (await second.GetCoverAsync(reference, CancellationToken.None)).Should().NotBeNull();
            }

            File.Exists(path).Should().BeTrue();
            downloads.Should().Be(0, "the entry was read from disk, so the sweep left it usable");
        }

        // ------------------------------------------------------------------
        // Cache key
        // ------------------------------------------------------------------

        [Fact]
        public void A_cache_entry_is_named_for_the_size_it_holds_as_well_as_the_address()
        {
            const string reference = "https://example.invalid/cover.png";

            string atOneSize = CachedCoverImageProvider.DiskCacheFileName(reference, 480, 848);
            string atAnotherWidth = CachedCoverImageProvider.DiskCacheFileName(reference, 960, 848);
            string atAnotherHeight = CachedCoverImageProvider.DiskCacheFileName(reference, 480, 1696);

            atOneSize.Should().NotBe(atAnotherWidth);
            atOneSize.Should().NotBe(atAnotherHeight);
            atAnotherWidth.Should().NotBe(atAnotherHeight);

            // Same address, same size, same file: the entry has to be found again.
            CachedCoverImageProvider.DiskCacheFileName(reference, 480, 848).Should().Be(atOneSize);
        }

        // ------------------------------------------------------------------
        // Failures
        // ------------------------------------------------------------------

        [Fact]
        public async Task An_address_that_failed_is_not_fetched_a_second_time()
        {
            int attempts = 0;

            using (CachedCoverImageProvider provider = ProviderUsing((reference, token) =>
            {
                attempts++;
                return Task.FromResult<byte[]?>(null);
            }))
            {
                (await provider.GetCoverAsync("https://example.invalid/gone.png", CancellationToken.None))
                    .Should().BeNull();
                (await provider.GetCoverAsync("https://example.invalid/gone.png", CancellationToken.None))
                    .Should().BeNull();
                (await provider.GetCoverAsync("https://example.invalid/gone.png", CancellationToken.None))
                    .Should().BeNull();
            }

            attempts.Should().Be(1, "the refusal is remembered, so the wait is paid once");
        }

        [Fact]
        public async Task A_download_that_is_not_an_image_counts_as_a_failure()
        {
            int attempts = 0;

            using (CachedCoverImageProvider provider = ProviderUsing((reference, token) =>
            {
                attempts++;
                return Task.FromResult<byte[]?>(new byte[] { 1, 2, 3, 4 });
            }))
            {
                (await provider.GetCoverAsync("https://example.invalid/notanimage", CancellationToken.None))
                    .Should().BeNull();
                (await provider.GetCoverAsync("https://example.invalid/notanimage", CancellationToken.None))
                    .Should().BeNull();
            }

            attempts.Should().Be(1);
        }

        [Fact]
        public async Task A_cover_that_was_resolved_once_is_served_from_memory()
        {
            int attempts = 0;
            byte[] original = PngOfSize(600, 900);

            using (CachedCoverImageProvider provider = ProviderUsing((reference, token) =>
            {
                attempts++;
                return Task.FromResult<byte[]?>(original);
            }))
            {
                Image? first = await provider.GetCoverAsync("https://example.invalid/a.png", CancellationToken.None);
                Image? second = await provider.GetCoverAsync("https://example.invalid/a.png", CancellationToken.None);

                first.Should().NotBeNull();
                second.Should().BeSameAs(first);
            }

            attempts.Should().Be(1);
        }

        // ------------------------------------------------------------------
        // Memory cache bound
        // ------------------------------------------------------------------

        [Fact]
        public async Task The_memory_cache_stops_growing_once_it_is_full()
        {
            byte[] original = PngOfSize(64, 96);
            const int distinctCovers = 60;

            using (CachedCoverImageProvider provider = ProviderReturning(original))
            {
                int highWaterMark = 0;

                for (int i = 0; i < distinctCovers; i++)
                {
                    string reference = "https://example.invalid/cover-" +
                                       i.ToString(CultureInfo.InvariantCulture) + ".png";

                    (await provider.GetCoverAsync(reference, CancellationToken.None)).Should().NotBeNull();

                    highWaterMark = Math.Max(highWaterMark, provider.MemoryCacheCount);
                }

                // The exact ceiling is the provider's business; what matters is that it has
                // one, and that it is nowhere near the number of covers that went through.
                highWaterMark.Should().BeLessThan(distinctCovers);
                provider.MemoryCacheCount.Should().Be(highWaterMark);
            }
        }

        [Fact]
        public async Task The_cover_most_recently_asked_for_survives_every_eviction()
        {
            byte[] original = PngOfSize(64, 96);
            const int distinctCovers = 60;
            List<Image> served = new List<Image>();

            using (CachedCoverImageProvider provider = ProviderReturning(original))
            {
                for (int i = 0; i < distinctCovers; i++)
                {
                    string reference = "https://example.invalid/live-" +
                                       i.ToString(CultureInfo.InvariantCulture) + ".png";

                    Image? cover = await provider.GetCoverAsync(reference, CancellationToken.None);
                    cover.Should().NotBeNull();
                    served.Add(cover!);
                }

                // Evicting the oldest entry disposes it, which is only safe while the entry
                // a screen is drawing is the newest one. Reading the last cover's size would
                // throw if the provider had disposed it.
                Image latest = served[served.Count - 1];
                latest.Size.Should().Be(new Size(64, 96));
            }
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private CachedCoverImageProvider ProviderReturning(byte[] bytes)
        {
            return ProviderUsing((reference, token) => Task.FromResult<byte[]?>(bytes));
        }

        private CachedCoverImageProvider ProviderUsing(Func<string, CancellationToken, Task<byte[]?>> download)
        {
            return new CachedCoverImageProvider(
                _cacheDirectory,
                NullLogger<CachedCoverImageProvider>.Instance,
                download);
        }

        /// <summary>Where the provider under test keeps the entry for one address.</summary>
        private string CachePathFor(string reference)
        {
            return Path.Combine(
                _cacheDirectory,
                CachedCoverImageProvider.DiskCacheFileName(reference, 480, 848));
        }

        /// <summary>The format a decoder sees when it opens these bytes.</summary>
        private static ImageFormat StoredFormatOf(byte[] stored)
        {
            using (MemoryStream buffer = new MemoryStream(stored, writable: false))
            using (Image image = Image.FromStream(buffer))
            {
                return image.RawFormat;
            }
        }

        /// <summary>What the same picture would take if it were stored without losing anything.</summary>
        private static int LosslessSizeOf(byte[] stored)
        {
            using (MemoryStream buffer = new MemoryStream(stored, writable: false))
            using (Image image = Image.FromStream(buffer))
            using (MemoryStream output = new MemoryStream())
            {
                image.Save(output, ImageFormat.Png);
                return (int)output.Length;
            }
        }

        /// <summary>
        /// Builds an opaque picture with the sort of detail real cover art has. A flat fill
        /// would compress to almost nothing either way and prove nothing about the choice.
        /// </summary>
        private static byte[] OpaqueDetailedPng(int width, int height)
        {
            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format24bppRgb))
            {
                // Fixed seed: the sizes this feeds into are asserted on, so the picture has
                // to be the same one on every run.
                Random random = new Random(20240117);

                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        bitmap.SetPixel(x, y, Color.FromArgb(
                            random.Next(256), random.Next(256), random.Next(256)));
                    }
                }

                using (MemoryStream output = new MemoryStream())
                {
                    bitmap.Save(output, ImageFormat.Png);
                    return output.ToArray();
                }
            }
        }

        /// <summary>
        /// Builds artwork whose top-left quarter is cut out entirely, the shape that a
        /// lossy format would silently fill in with a solid colour.
        /// </summary>
        private static byte[] PngWithTransparentCorner(int width, int height)
        {
            using (Bitmap bitmap = new Bitmap(width, height, PixelFormat.Format32bppArgb))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (MemoryStream output = new MemoryStream())
            {
                graphics.Clear(Color.Transparent);
                graphics.FillRectangle(Brushes.MediumVioletRed, width / 2, 0, width / 2, height);
                graphics.FillRectangle(Brushes.SeaGreen, 0, height / 2, width, height / 2);

                bitmap.Save(output, ImageFormat.Png);
                return output.ToArray();
            }
        }

        /// <summary>Builds a PNG of the requested size, so a test can state the size it means.</summary>
        private static byte[] PngOfSize(int width, int height)
        {
            using (Bitmap bitmap = new Bitmap(width, height))
            using (Graphics graphics = Graphics.FromImage(bitmap))
            using (MemoryStream output = new MemoryStream())
            {
                // Two blocks rather than a flat fill, so that a scaled copy is visibly
                // derived from this one rather than from an empty bitmap.
                graphics.Clear(Color.CornflowerBlue);
                graphics.FillRectangle(Brushes.Goldenrod, 0, 0, width / 2, height / 2);

                bitmap.Save(output, ImageFormat.Png);
                return output.ToArray();
            }
        }
    }
}
