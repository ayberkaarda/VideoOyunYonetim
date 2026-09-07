using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;
using VideoGameManager.UI.Controls;

namespace VideoGameManager.UI
{
    /// <summary>
    /// Resolves game cover artwork from a URL, backed by a bounded in-memory cache and a
    /// disk cache under the user's local application data folder.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This class touches the network and the file system, which is why it lives
    /// beside the forms rather than inside <c>UI.Controls</c>: the control layer only
    /// knows the <see cref="ICoverImageProvider"/> contract, never how artwork is
    /// actually fetched. A single static <see cref="HttpClient"/> is shared by every
    /// instance, per the guidance against creating one per request (socket exhaustion
    /// under load).
    /// </para>
    /// <para>
    /// Artwork published for a store page is far larger than a cover slot a few hundred
    /// pixels wide - several megabytes is normal. Downloading that once is unavoidable,
    /// but keeping it is not: what gets cached is the picture scaled down to the size the
    /// screens actually draw, so every later read moves a fraction of the bytes and the
    /// cache folder stays small.
    /// </para>
    /// </remarks>
    public sealed class CachedCoverImageProvider : ICoverImageProvider, IDisposable
    {
        /// <summary>
        /// How long one artwork request may take end to end, body included.
        /// </summary>
        /// <remarks>
        /// Short on purpose. A cover is decoration: a screen that waits on one is a screen
        /// the user is watching do nothing. An address that no longer answers costs this
        /// much exactly once per run, because the failure is remembered, and a healthy
        /// download of a few megabytes finishes well inside it on any ordinary connection.
        /// </remarks>
        private const int RequestTimeoutSeconds = 6;

        private const int CopyBufferSize = 81920;

        /// <summary>
        /// Quality passed to the JPEG encoder for artwork that has no transparency.
        /// </summary>
        /// <remarks>
        /// Chosen by encoding the catalogue's covers across the range. Below this the
        /// savings flatten out while the artefacts do not: cover art carries titles,
        /// logos and studio marks, and it is exactly those hard edges that pick up
        /// ringing first, so a number tuned on photographs would be too low here. Above
        /// it the file grows far faster than the picture improves - the same covers cost
        /// roughly 40% more at 95 and three times as much at 100, for a difference no
        /// one is going to see in a slot a couple of hundred pixels wide.
        /// </remarks>
        private const long JpegQuality = 90L;

        /// <summary>
        /// Extension used by cache files this build can no longer read.
        /// </summary>
        /// <remarks>
        /// Entries used to be named for the address alone. They are now named for the
        /// address and the stored size together, under a different extension, so nothing
        /// written under the old scheme will ever be looked up again - it would only sit
        /// in the cache folder taking up space. See <see cref="RemoveSupersededEntries"/>.
        /// </remarks>
        private const string SupersededCacheExtension = ".img";

        /// <summary>
        /// Widest and tallest the cover slot is drawn on any screen, in pixels. Both
        /// windows are fixed size, so these do not move at runtime: the browse screen
        /// draws into 236x424 and the recommendation screen into 232x290, rounded up here
        /// to the next round number.
        /// </summary>
        private const int DisplayWidth = 240;
        private const int DisplayHeight = 424;

        /// <summary>
        /// How much larger than the drawn size artwork is kept. One extra factor of two
        /// costs four times the pixels of a pixel-exact copy - still a small fraction of
        /// the original - and buys a picture that survives being drawn slightly larger,
        /// on a scaled display for instance, without turning soft.
        /// </summary>
        private const int OversampleFactor = 2;

        private const int TargetWidth = DisplayWidth * OversampleFactor;
        private const int TargetHeight = DisplayHeight * OversampleFactor;

        /// <summary>
        /// How many decoded covers are held in memory at once.
        /// </summary>
        /// <remarks>
        /// Every entry pins a GDI+ bitmap handle, so the cache cannot be allowed to grow
        /// with the catalogue. Thirty-two covers at the size stored here is a few tens of
        /// megabytes at worst and comfortably more than one screenful, so paging back and
        /// forth through a list still hits memory.
        /// </remarks>
        private const int MemoryCacheCapacity = 32;

        /// <summary>How many failed references are remembered before the oldest is forgotten.</summary>
        private const int FailureCacheCapacity = 64;

        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        /// <summary>
        /// The JPEG encoder, looked up once. Null if the platform does not offer one, in
        /// which case everything is stored as PNG exactly as it was before.
        /// </summary>
        private static readonly ImageCodecInfo? JpegCodec = FindJpegCodec();

        /// <summary>
        /// Guards both caches. They are updated together often enough, and briefly enough,
        /// that one lock is simpler to reason about than two lock-free structures.
        /// </summary>
        private readonly object _cacheLock = new object();

        /// <summary>Most recently used first. The head is what the screens are showing.</summary>
        private readonly LinkedList<MemoryEntry> _memoryOrder = new LinkedList<MemoryEntry>();

        private readonly Dictionary<string, LinkedListNode<MemoryEntry>> _memoryIndex =
            new Dictionary<string, LinkedListNode<MemoryEntry>>(StringComparer.OrdinalIgnoreCase);

        private readonly HashSet<string> _failedReferences =
            new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        private readonly Queue<string> _failureOrder = new Queue<string>();

        private readonly string _diskCacheDirectory;
        private readonly ILogger<CachedCoverImageProvider> _logger;
        private readonly Func<string, CancellationToken, Task<byte[]?>> _download;

        private bool _disposed;

        /// <summary>
        /// Zero until the one-off sweep for unreadable cache entries has been started.
        /// </summary>
        private int _sweptSupersededEntries;

        /// <summary>Initialises a provider that caches under the user's local application data folder.</summary>
        public CachedCoverImageProvider()
            : this(DefaultDiskCacheDirectory(), NullLogger<CachedCoverImageProvider>.Instance)
        {
        }

        /// <summary>Initialises a provider that caches under the user's local application data folder and logs to <paramref name="logger"/>.</summary>
        /// <param name="logger">Receives a warning whenever fetching or caching a cover fails.</param>
        public CachedCoverImageProvider(ILogger<CachedCoverImageProvider> logger)
            : this(DefaultDiskCacheDirectory(), logger)
        {
        }

        /// <summary>Initialises a provider that caches under an explicit directory. Exposed for testing.</summary>
        /// <param name="diskCacheDirectory">Directory the disk cache files are written to.</param>
        public CachedCoverImageProvider(string diskCacheDirectory)
            : this(diskCacheDirectory, NullLogger<CachedCoverImageProvider>.Instance)
        {
        }

        /// <summary>Initialises a provider that caches under an explicit directory and logs to <paramref name="logger"/>. Exposed for testing.</summary>
        /// <param name="diskCacheDirectory">Directory the disk cache files are written to.</param>
        /// <param name="logger">Receives a warning whenever fetching or caching a cover fails.</param>
        public CachedCoverImageProvider(string diskCacheDirectory, ILogger<CachedCoverImageProvider> logger)
            : this(diskCacheDirectory, logger, DownloadAsync)
        {
        }

        /// <summary>
        /// Initialises a provider whose downloads go through <paramref name="download"/>
        /// instead of the network. This is the seam the tests use, so that caching,
        /// scaling and failure handling can be exercised from byte arrays alone.
        /// </summary>
        internal CachedCoverImageProvider(
            string diskCacheDirectory,
            ILogger<CachedCoverImageProvider> logger,
            Func<string, CancellationToken, Task<byte[]?>> download)
        {
            _diskCacheDirectory = diskCacheDirectory;
            _logger = logger ?? NullLogger<CachedCoverImageProvider>.Instance;
            _download = download ?? DownloadAsync;
        }

        /// <summary>How many covers are held in memory right now. Exposed for testing.</summary>
        internal int MemoryCacheCount
        {
            get
            {
                lock (_cacheLock)
                {
                    return _memoryIndex.Count;
                }
            }
        }

        private static string DefaultDiskCacheDirectory()
        {
            return Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VideoGameManager",
                "covers");
        }

        /// <inheritdoc/>
        public async Task<Image?> GetCoverAsync(string? coverReference, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(coverReference))
            {
                return null;
            }

            Image? memoryHit = TakeFromMemory(coverReference);
            if (memoryHit != null)
            {
                return memoryHit;
            }

            if (HasFailedBefore(coverReference))
            {
                // Asked for and refused already during this run. Repeating the attempt
                // would cost another timeout and end the same way. The record is kept in
                // memory only, so a restart tries again: an address that failed once may
                // simply have been unreachable at the time.
                return null;
            }

            // First time this run that anything is about to be read from or written to the
            // cache folder, and therefore the cheapest moment to notice what is stale in it.
            RemoveSupersededEntries();

            string cacheFilePath = GetDiskCachePath(coverReference);

            try
            {
                byte[]? bytes = await ReadFromDiskAsync(cacheFilePath, cancellationToken).ConfigureAwait(false);

                if (bytes == null)
                {
                    byte[]? downloaded = await _download(coverReference, cancellationToken).ConfigureAwait(false);

                    if (downloaded != null)
                    {
                        bytes = ReduceForDisplay(downloaded);

                        if (bytes != null)
                        {
                            await WriteToDiskAsync(cacheFilePath, bytes, cancellationToken).ConfigureAwait(false);
                        }
                    }
                }

                Image? decoded = bytes == null ? null : DecodeImage(bytes);

                if (decoded == null)
                {
                    RememberFailure(coverReference);
                    return null;
                }

                return StoreInMemory(coverReference, decoded);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                // The caller (typically a new selection superseding this one) no longer
                // wants the result. Propagate so it can tell the difference between
                // "cancelled" and "failed".
                throw;
            }
            catch (Exception ex)
            {
                // Network failure, disk I/O failure or a malformed image: none of these
                // should crash the UI thread. A request that ran out of time arrives here
                // too, as a cancellation the caller never asked for, which is why the
                // filter above checks the caller's token rather than the exception type.
                // The caller falls back to its placeholder state, exactly as it would for
                // a missing cover, but the detail is not thrown away -- it goes to the log.
                _logger.LogWarning(ex, "Fetching cover art failed for {CoverReference}", coverReference);
                RememberFailure(coverReference);
                return null;
            }
        }

        /// <summary>Releases every cover held in memory.</summary>
        public void Dispose()
        {
            List<Image> held;

            lock (_cacheLock)
            {
                if (_disposed)
                {
                    return;
                }

                _disposed = true;
                held = new List<Image>(_memoryOrder.Count);

                foreach (MemoryEntry entry in _memoryOrder)
                {
                    held.Add(entry.Image);
                }

                _memoryOrder.Clear();
                _memoryIndex.Clear();
            }

            foreach (Image image in held)
            {
                image.Dispose();
            }
        }

        // ------------------------------------------------------------------
        // Memory cache
        // ------------------------------------------------------------------

        private Image? TakeFromMemory(string coverReference)
        {
            lock (_cacheLock)
            {
                LinkedListNode<MemoryEntry>? node;
                if (!_memoryIndex.TryGetValue(coverReference, out node))
                {
                    return null;
                }

                _memoryOrder.Remove(node);
                _memoryOrder.AddFirst(node);
                return node.Value.Image;
            }
        }

        /// <summary>
        /// Puts a freshly decoded cover in the cache and returns the one the caller should
        /// display, which is the cached instance rather than necessarily the one passed in.
        /// </summary>
        /// <remarks>
        /// Dropping the oldest entry disposes its bitmap, which is only safe because the
        /// entry being dropped cannot be on screen. Every screen that draws a cover is
        /// modal, so at most one cover box exists at a time, and that box is always showing
        /// either nothing or the cover that was fetched last - which is this list's head.
        /// The head is the one entry an eviction never reaches while the capacity is above
        /// one. Without that guarantee a bitmap could be disposed mid-paint, which is a
        /// crash rather than a leak, so the capacity must stay well above the number of
        /// cover boxes that can be visible together.
        /// </remarks>
        private Image StoreInMemory(string coverReference, Image image)
        {
            List<Image> discarded = new List<Image>();
            Image kept;

            lock (_cacheLock)
            {
                if (_disposed)
                {
                    return image;
                }

                LinkedListNode<MemoryEntry>? existing;
                if (_memoryIndex.TryGetValue(coverReference, out existing))
                {
                    // Two loads of the same cover overlapped. The one that arrived first is
                    // already the one on screen, so it stays; the copy that lost the race
                    // was never handed to anybody and can go.
                    _memoryOrder.Remove(existing);
                    _memoryOrder.AddFirst(existing);
                    kept = existing.Value.Image;

                    if (!ReferenceEquals(kept, image))
                    {
                        discarded.Add(image);
                    }
                }
                else
                {
                    _memoryIndex.Add(coverReference, _memoryOrder.AddFirst(new MemoryEntry(coverReference, image)));
                    kept = image;

                    while (_memoryIndex.Count > MemoryCacheCapacity && _memoryOrder.Last != null)
                    {
                        MemoryEntry oldest = _memoryOrder.Last.Value;
                        _memoryOrder.RemoveLast();
                        _memoryIndex.Remove(oldest.Key);
                        discarded.Add(oldest.Image);
                    }
                }
            }

            // Disposing outside the lock: releasing a GDI+ handle is not instant and no
            // other caller should be made to wait behind it.
            foreach (Image stale in discarded)
            {
                stale.Dispose();
            }

            return kept;
        }

        private bool HasFailedBefore(string coverReference)
        {
            lock (_cacheLock)
            {
                return _failedReferences.Contains(coverReference);
            }
        }

        private void RememberFailure(string coverReference)
        {
            lock (_cacheLock)
            {
                if (!_failedReferences.Add(coverReference))
                {
                    return;
                }

                _failureOrder.Enqueue(coverReference);

                // Bounded like the image cache, for the same reason: a long-lived provider
                // must not accumulate strings for the life of the process.
                while (_failureOrder.Count > FailureCacheCapacity)
                {
                    _failedReferences.Remove(_failureOrder.Dequeue());
                }
            }
        }

        // ------------------------------------------------------------------
        // Network
        // ------------------------------------------------------------------

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);

            // Several of the hosts that serve cover artwork - Wikimedia among them - refuse
            // anonymous requests outright and ask that a client identify itself by name,
            // version and a way to get in touch. Without this the thumbnail endpoints
            // answer with an error rather than an image.
            Version? version = typeof(CachedCoverImageProvider).Assembly.GetName().Version;
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("VideoGameManager", version == null ? "1.0" : version.ToString(3)));
            client.DefaultRequestHeaders.UserAgent.Add(
                new ProductInfoHeaderValue("(+https://github.com/ayberkaarda/VideoOyunYonetim)"));

            return client;
        }

        private static async Task<byte[]?> DownloadAsync(string coverReference, CancellationToken cancellationToken)
        {
            Uri? coverUri;
            if (!Uri.TryCreate(coverReference, UriKind.Absolute, out coverUri))
            {
                return null;
            }

            using (HttpResponseMessage response = await SharedHttpClient
                .GetAsync(coverUri, HttpCompletionOption.ResponseHeadersRead, cancellationToken)
                .ConfigureAwait(false))
            {
                if (!response.IsSuccessStatusCode)
                {
                    return null;
                }

                return await response.Content.ReadAsByteArrayAsync(cancellationToken).ConfigureAwait(false);
            }
        }

        // ------------------------------------------------------------------
        // Disk cache
        // ------------------------------------------------------------------

        private string GetDiskCachePath(string coverReference)
        {
            return Path.Combine(_diskCacheDirectory, DiskCacheFileName(coverReference, TargetWidth, TargetHeight));
        }

        /// <summary>
        /// Names the cache file for one cover reference at one stored size.
        /// </summary>
        /// <remarks>
        /// The size is part of what gets hashed, not just part of the address. A file
        /// holds a picture scaled for a particular slot, so an entry written for one size
        /// must not be handed back after that size changes - the screen would quietly show
        /// artwork at the wrong resolution, and nothing would look broken enough to notice.
        /// Changing either constant therefore renames every entry, and the old ones are
        /// simply never read again.
        /// </remarks>
        internal static string DiskCacheFileName(string coverReference, int width, int height)
        {
            string key = coverReference + "|" +
                         width.ToString(CultureInfo.InvariantCulture) + "x" +
                         height.ToString(CultureInfo.InvariantCulture);

            return ComputeSha256Hex(key) + ".cover";
        }

        private static string ComputeSha256Hex(string value)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(value));
                StringBuilder builder = new StringBuilder(hash.Length * 2);

                for (int i = 0; i < hash.Length; i++)
                {
                    builder.Append(hash[i].ToString("x2"));
                }

                return builder.ToString();
            }
        }

        /// <summary>
        /// Deletes cache files this build can no longer read, once per provider.
        /// </summary>
        /// <remarks>
        /// <para>
        /// Kept deliberately narrow, because it is the only thing here that destroys
        /// anything: it looks in this provider's own cache folder and nowhere below it,
        /// and it removes only files whose extension is exactly the superseded one. That
        /// last condition is spelled out in the loop rather than left to the search
        /// pattern alone. A pattern is a filter the file system interprets, and what it
        /// admits has varied with the platform and the volume - short-name matching being
        /// the usual reason a pattern returns more than it appears to ask for. Stating the
        /// rule for deletion where the deletion happens costs one comparison per candidate
        /// and makes the bound on this method readable without knowing any of that.
        /// </para>
        /// <para>
        /// It runs at most once per instance, on the first request that reaches the disk,
        /// so listing the folder is not repeated for every cover on a screen. Failure is
        /// not propagated in any form: this reclaims space, and a cache folder that cannot
        /// be tidied is not a reason for a screen to stop showing artwork.
        /// </para>
        /// </remarks>
        private void RemoveSupersededEntries()
        {
            if (Interlocked.Exchange(ref _sweptSupersededEntries, 1) != 0)
            {
                return;
            }

            List<string> removed = new List<string>();

            try
            {
                if (!Directory.Exists(_diskCacheDirectory))
                {
                    return;
                }

                foreach (string path in Directory.EnumerateFiles(
                    _diskCacheDirectory, "*" + SupersededCacheExtension, SearchOption.TopDirectoryOnly))
                {
                    if (!string.Equals(
                        Path.GetExtension(path), SupersededCacheExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        continue;
                    }

                    try
                    {
                        File.Delete(path);
                        removed.Add(Path.GetFileName(path));
                    }
                    catch (IOException)
                    {
                        // Held open by something else. It will still be here next run.
                    }
                    catch (UnauthorizedAccessException)
                    {
                        // Read-only or not ours to delete. Left where it is.
                    }
                }
            }
            catch (IOException)
            {
                // The folder could not be listed. Whatever is in it simply stays.
            }
            catch (UnauthorizedAccessException)
            {
                // As above: no permission to look, so nothing is reclaimed.
            }

            if (removed.Count > 0)
            {
                _logger.LogInformation(
                    "Removed {Count} unreadable cover cache {Entries} from {Directory}: {Files}",
                    removed.Count,
                    removed.Count == 1 ? "entry" : "entries",
                    _diskCacheDirectory,
                    string.Join(", ", removed));
            }
        }

        private static async Task<byte[]?> ReadFromDiskAsync(string path, CancellationToken cancellationToken)
        {
            if (!File.Exists(path))
            {
                return null;
            }

            try
            {
                using (FileStream fileStream = new FileStream(
                    path, FileMode.Open, FileAccess.Read, FileShare.Read, CopyBufferSize, useAsync: true))
                using (MemoryStream buffer = new MemoryStream())
                {
                    await fileStream.CopyToAsync(buffer, CopyBufferSize, cancellationToken).ConfigureAwait(false);
                    return buffer.ToArray();
                }
            }
            catch (IOException)
            {
                // A missing, locked or otherwise unreadable cache entry is not fatal:
                // the caller falls back to downloading the artwork again.
                return null;
            }
            catch (UnauthorizedAccessException)
            {
                return null;
            }
        }

        private static async Task WriteToDiskAsync(string path, byte[] bytes, CancellationToken cancellationToken)
        {
            try
            {
                string? directory = Path.GetDirectoryName(path);
                if (!string.IsNullOrEmpty(directory))
                {
                    Directory.CreateDirectory(directory);
                }

                using (FileStream fileStream = new FileStream(
                    path, FileMode.Create, FileAccess.Write, FileShare.None, CopyBufferSize, useAsync: true))
                {
                    await fileStream.WriteAsync(bytes, 0, bytes.Length, cancellationToken).ConfigureAwait(false);
                }
            }
            catch (IOException)
            {
                // The disk cache is an optimisation, not a requirement: a failed write
                // just means the next load downloads again instead of reading from disk.
            }
            catch (UnauthorizedAccessException) // bypass-ok: caching is best-effort; a permissions failure on the cache folder falls back to re-downloading on the next call, same as the IOException case above, so there is nothing actionable to log or show.
            {
            }
        }

        // ------------------------------------------------------------------
        // Decoding and scaling
        // ------------------------------------------------------------------

        /// <summary>
        /// Scales artwork down to the size the screens draw it at and encodes the result,
        /// or hands the bytes back untouched when they are already small enough.
        /// </summary>
        /// <param name="originalBytes">The picture exactly as it was downloaded.</param>
        /// <returns>The bytes to store, or <see langword="null"/> when the download was not
        /// something that can be read as an image.</returns>
        /// <remarks>
        /// Proportions are kept, so a portrait cover stays portrait, and nothing is ever
        /// enlarged: scaling a small picture up would cost space and add no detail. How the
        /// scaled result is written depends on what is in it - see <see cref="Encode"/>. A
        /// picture that needed no scaling is stored in whatever format it arrived in,
        /// because re-encoding it could only lose quality or add bytes.
        /// </remarks>
        internal static byte[]? ReduceForDisplay(byte[] originalBytes)
        {
            try
            {
                using (MemoryStream source = new MemoryStream(originalBytes, writable: false))
                using (Bitmap original = new Bitmap(source))
                {
                    Size target = ScaleToFit(original.Size, TargetWidth, TargetHeight);

                    if (target == original.Size)
                    {
                        return originalBytes;
                    }

                    using (Bitmap reduced = new Bitmap(target.Width, target.Height, PixelFormat.Format32bppArgb))
                    {
                        using (Graphics graphics = Graphics.FromImage(reduced))
                        {
                            // SourceCopy rather than SourceOver: the destination starts out
                            // transparent, and blending onto it would darken the edges of
                            // artwork that has transparency of its own.
                            graphics.CompositingMode = CompositingMode.SourceCopy;
                            graphics.CompositingQuality = CompositingQuality.HighQuality;
                            graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
                            graphics.SmoothingMode = SmoothingMode.HighQuality;
                            graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;

                            using (ImageAttributes attributes = new ImageAttributes())
                            {
                                // The bicubic kernel reaches past the edge of the source.
                                // Left to wrap around, it blends the far edge back in and
                                // leaves a faint border; mirroring the edge instead does not.
                                attributes.SetWrapMode(WrapMode.TileFlipXY);

                                graphics.DrawImage(
                                    original,
                                    new Rectangle(Point.Empty, target),
                                    0,
                                    0,
                                    original.Width,
                                    original.Height,
                                    GraphicsUnit.Pixel,
                                    attributes);
                            }
                        }

                        return Encode(reduced);
                    }
                }
            }
            catch (ArgumentException)
            {
                // Not a format GDI+ recognises as an image.
                return null;
            }
            catch (ExternalException)
            {
                // GDI+ refused to encode the result. Treated the same as an unreadable
                // download: no artwork, and the caller shows its placeholder.
                return null;
            }
        }

        /// <summary>
        /// Encodes a scaled cover in whichever format suits what is actually in it: JPEG
        /// when it is opaque, PNG when any part of it is see-through.
        /// </summary>
        /// <param name="reduced">The scaled artwork, always 32 bits per pixel with alpha.</param>
        /// <returns>The bytes to store.</returns>
        /// <remarks>
        /// <para>
        /// PNG is lossless, and lossless is the wrong trade for a photograph or a painted
        /// cover: reducing one to a fraction of its pixel count can still produce a file
        /// several times larger than the original arrived as, so scaling down would cost
        /// disk space rather than save it. JPEG is what those covers were published as and
        /// what they compress well into. PNG is kept only where it earns its size, which is
        /// artwork that is transparent somewhere - JPEG cannot store transparency at all,
        /// and flattening it silently paints the see-through parts onto a solid background.
        /// </para>
        /// <para>
        /// Which of the two applies is decided by looking at the pixels rather than at the
        /// format the artwork declares. A picture may carry an alpha channel and use none
        /// of it, and that is not a rare corner: covers published as PNG routinely arrive
        /// with a fully opaque alpha channel, and trusting the declaration alone would keep
        /// the very largest of them lossless for nothing. The check costs well under a
        /// millisecond, once, on the same code path that just finished a download.
        /// </para>
        /// </remarks>
        private static byte[] Encode(Bitmap reduced)
        {
            bool lossless = JpegCodec == null || HasTransparentPixels(reduced);

            using (MemoryStream output = new MemoryStream())
            {
                if (lossless)
                {
                    reduced.Save(output, ImageFormat.Png);
                }
                else
                {
                    using (EncoderParameters parameters = new EncoderParameters(1))
                    // Fully qualified: System.Text is in scope here and has an Encoder of
                    // its own, which is an unrelated type.
                    using (EncoderParameter quality =
                        new EncoderParameter(System.Drawing.Imaging.Encoder.Quality, JpegQuality))
                    {
                        parameters.Param[0] = quality;
                        reduced.Save(output, JpegCodec!, parameters);
                    }
                }

                return output.ToArray();
            }
        }

        /// <summary>
        /// Reports whether any pixel is less than fully opaque.
        /// </summary>
        /// <remarks>
        /// Reads the alpha byte of every pixel and stops at the first one that is not
        /// solid, so artwork with a transparent border - the common case - is settled
        /// almost immediately and only a fully opaque picture is read all the way
        /// through. The rows are copied out one at a time instead of being addressed
        /// directly, which keeps this ordinary verifiable code rather than pointer
        /// arithmetic, for a cost that does not matter at this size.
        /// </remarks>
        private static bool HasTransparentPixels(Bitmap reduced)
        {
            BitmapData data = reduced.LockBits(
                new Rectangle(Point.Empty, reduced.Size),
                ImageLockMode.ReadOnly,
                PixelFormat.Format32bppArgb);

            try
            {
                byte[] row = new byte[Math.Abs(data.Stride)];

                for (int y = 0; y < data.Height; y++)
                {
                    // Signed arithmetic on purpose: a bottom-up bitmap reports a negative
                    // stride, with Scan0 pointing at the last row rather than the first.
                    Marshal.Copy(data.Scan0 + (y * data.Stride), row, 0, row.Length);

                    // Alpha is the fourth byte of each pixel in this layout.
                    for (int offset = 3; offset < data.Width * 4; offset += 4)
                    {
                        if (row[offset] != byte.MaxValue)
                        {
                            return true;
                        }
                    }
                }
            }
            finally
            {
                reduced.UnlockBits(data);
            }

            return false;
        }

        private static ImageCodecInfo? FindJpegCodec()
        {
            foreach (ImageCodecInfo codec in ImageCodecInfo.GetImageEncoders())
            {
                if (codec.FormatID == ImageFormat.Jpeg.Guid)
                {
                    return codec;
                }
            }

            return null;
        }

        /// <summary>
        /// Fits a size inside a box without changing its proportions, and without ever
        /// making it larger than it already is.
        /// </summary>
        internal static Size ScaleToFit(Size source, int maxWidth, int maxHeight)
        {
            if (source.Width <= 0 || source.Height <= 0 || maxWidth <= 0 || maxHeight <= 0)
            {
                return source;
            }

            double scale = Math.Min((double)maxWidth / source.Width, (double)maxHeight / source.Height);

            if (scale >= 1.0)
            {
                return source;
            }

            return new Size(
                Math.Max(1, (int)Math.Round(source.Width * scale)),
                Math.Max(1, (int)Math.Round(source.Height * scale)));
        }

        private static Image? DecodeImage(byte[] bytes)
        {
            try
            {
                using (MemoryStream temp = new MemoryStream(bytes))
                using (Bitmap streamBound = new Bitmap(temp))
                {
                    // System.Drawing keeps a GDI+ bitmap tied to the stream it was
                    // constructed from for its entire lifetime. Copying into a fresh
                    // Bitmap here detaches it, so the MemoryStream can be disposed
                    // immediately instead of leaking for as long as the cover is shown.
                    return new Bitmap(streamBound);
                }
            }
            catch (ArgumentException)
            {
                // Not a format GDI+ recognises as an image.
                return null;
            }
        }

        /// <summary>One cover held in memory, paired with the reference it was fetched for.</summary>
        private sealed class MemoryEntry
        {
            internal MemoryEntry(string key, Image image)
            {
                Key = key;
                Image = image;
            }

            internal string Key { get; }

            internal Image Image { get; }
        }
    }
}
