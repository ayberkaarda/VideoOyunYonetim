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
