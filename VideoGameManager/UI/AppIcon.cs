using System;
using System.Drawing;
using System.IO;

namespace VideoGameManager.UI
{
    /// <summary>
    /// The application icon, as every window shows it in the taskbar and the window switcher.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The icon is assets/app.ico, embedded once in this assembly. The same file is the
    /// project's <c>ApplicationIcon</c>, so the executable and its windows cannot drift apart
    /// the way they could when every form carried its own copy in a .resx file.
    /// </para>
    /// <para>
    /// One instance is shared by every window. A form does not dispose the icon it is given,
    /// only the small copy it derives from it, so sharing is safe and saves decoding the same
    /// file for each window.
    /// </para>
    /// </remarks>
    internal static class AppIcon
    {
        /// <summary>The manifest resource name the icon is embedded under.</summary>
        internal const string ResourceName = "VideoGameManager.app.ico";

        private static readonly Lazy<Icon> Shared = new Lazy<Icon>(Load);

        /// <summary>Gets the icon every window uses.</summary>
        public static Icon Window => Shared.Value;

        /// <summary>Opens the embedded icon file.</summary>
        /// <returns>A stream over the .ico bytes; the caller disposes it.</returns>
        /// <exception cref="InvalidOperationException">The resource is missing from the
        /// assembly, which means the project file no longer embeds it.</exception>
        internal static Stream OpenStream()
        {
            Stream? stream = typeof(AppIcon).Assembly.GetManifestResourceStream(ResourceName);
            return stream ?? throw new InvalidOperationException(
                "The application icon resource '" + ResourceName + "' is missing.");
        }

        private static Icon Load()
        {
            using (Stream stream = OpenStream())
            {
                return new Icon(stream);
            }
        }
    }
}
