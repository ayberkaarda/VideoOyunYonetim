using System;
using System.Collections.Concurrent;
using System.Drawing;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using VideoGameManager.UI.Controls;

namespace VideoGameManager.UI
{
    /// <summary>
    /// Resolves game cover artwork from a URL, backed by an in-memory cache and a
    /// disk cache under the user's local application data folder.
    /// </summary>
    /// <remarks>
    /// This class touches the network and the file system, which is why it lives
    /// beside the forms rather than inside <c>UI.Controls</c>: the control layer only
    /// knows the <see cref="ICoverImageProvider"/> contract, never how artwork is
    /// actually fetched. A single static <see cref="HttpClient"/> is shared by every
    /// instance, per the guidance against creating one per request (socket exhaustion
    /// under load).
    /// </remarks>
    public sealed class CachedCoverImageProvider : ICoverImageProvider
    {
        private const int RequestTimeoutSeconds = 10;
        private const int CopyBufferSize = 81920;

        private static readonly HttpClient SharedHttpClient = CreateHttpClient();

        private readonly ConcurrentDictionary<string, Image> _memoryCache =
            new ConcurrentDictionary<string, Image>(StringComparer.OrdinalIgnoreCase);

        private readonly string _diskCacheDirectory;

        /// <summary>Initialises a provider that caches under the user's local application data folder.</summary>
        public CachedCoverImageProvider()
            : this(Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "VideoGameManager",
                "covers"))
        {
        }

        /// <summary>Initialises a provider that caches under an explicit directory. Exposed for testing.</summary>
        /// <param name="diskCacheDirectory">Directory the disk cache files are written to.</param>
        public CachedCoverImageProvider(string diskCacheDirectory)
        {
            _diskCacheDirectory = diskCacheDirectory;
        }

        /// <inheritdoc/>
        public async Task<Image> GetCoverAsync(string coverReference, CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(coverReference))
            {
                return null;
            }

            Image memoryHit;
            if (_memoryCache.TryGetValue(coverReference, out memoryHit))
            {
                return memoryHit;
            }

            string cacheFilePath = GetDiskCachePath(coverReference);

            try
            {
                byte[] bytes = await ReadFromDiskAsync(cacheFilePath, cancellationToken).ConfigureAwait(false);

                if (bytes == null)
                {
                    bytes = await DownloadAsync(coverReference, cancellationToken).ConfigureAwait(false);

                    if (bytes != null)
                    {
                        await WriteToDiskAsync(cacheFilePath, bytes, cancellationToken).ConfigureAwait(false);
                    }
                }

                if (bytes == null)
                {
                    return null;
                }

                Image decoded = DecodeImage(bytes);
                if (decoded != null)
                {
                    _memoryCache[coverReference] = decoded;
                }

                return decoded;
            }
            catch (OperationCanceledException)
            {
                // The caller (typically a new selection superseding this one) no longer
                // wants the result. Propagate so it can tell the difference between
                // "cancelled" and "failed".
                throw;
            }
            catch (Exception)
            {
                // Network failure, disk I/O failure or a malformed image: none of these
                // should crash the UI thread. The caller falls back to its placeholder
                // state, exactly as it would for a missing cover.
                return null;
            }
        }

        private static HttpClient CreateHttpClient()
        {
            HttpClient client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(RequestTimeoutSeconds);
            return client;
        }

        private string GetDiskCachePath(string coverReference)
        {
            string fileName = ComputeSha256Hex(coverReference) + ".img";
            return Path.Combine(_diskCacheDirectory, fileName);
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

        private static async Task<byte[]> ReadFromDiskAsync(string path, CancellationToken cancellationToken)
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

        private static async Task<byte[]> DownloadAsync(string coverReference, CancellationToken cancellationToken)
        {
            Uri coverUri;
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

        private static async Task WriteToDiskAsync(string path, byte[] bytes, CancellationToken cancellationToken)
        {
            try
            {
                string directory = Path.GetDirectoryName(path);
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
            catch (UnauthorizedAccessException) // error-guard: bypass-ok caching is best-effort; a permissions failure on the cache folder falls back to re-downloading on the next call, same as the IOException case above, so there is nothing actionable to log or show.
            {
            }
        }

        private static Image DecodeImage(byte[] bytes)
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
    }
}
