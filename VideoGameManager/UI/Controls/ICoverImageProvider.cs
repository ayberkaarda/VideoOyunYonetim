using System.Drawing;
using System.Threading;
using System.Threading.Tasks;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// Supplies cover artwork to a <see cref="CoverImageBox"/>.
    /// </summary>
    /// <remarks>
    /// Only the contract lives in the UI layer. Any implementation touches the network,
    /// the disk or a cache, which makes it infrastructure: it belongs beside the other
    /// data access code, not here. The control depends on this interface so it stays
    /// pure presentation and can be exercised with a stub.
    /// </remarks>
    public interface ICoverImageProvider
    {
        /// <summary>
        /// Resolves a cover reference to an image.
        /// </summary>
        /// <param name="coverReference">Whatever identifies the artwork to the
        /// implementation: a URL, a cache key, a file name.</param>
        /// <param name="cancellationToken">Cancelled when the caller no longer wants the
        /// result, for example because the user selected a different game.</param>
        /// <returns>The image, or <see langword="null"/> when there is no artwork. The
        /// caller takes ownership of the returned image.</returns>
        Task<Image> GetCoverAsync(string coverReference, CancellationToken cancellationToken);
    }
}
