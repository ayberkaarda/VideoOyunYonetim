using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;

namespace LogoRenderer
{
    /// <summary>
    /// Writes the logo files: <c>app.ico</c> with every frame Windows asks for, and
    /// <c>logo-256.png</c>. With <c>--preview</c> it also writes one PNG per frame size, so
    /// the small frames can be inspected without opening the icon in another program.
    /// </summary>
    internal static class Program
    {
        private static readonly int[] IconSizes = { 16, 24, 32, 48, 64, 128, 256 };

        private static int Main(string[] args)
        {
            if (args.Length < 1 || args.Length > 2 || (args.Length == 2 && args[1] != "--preview"))
            {
                Console.Error.WriteLine("Usage: LogoRenderer <output-directory> [--preview]");
                return 2;
            }

            string output = Path.GetFullPath(args[0]);
            Directory.CreateDirectory(output);

            byte[][] frames = new byte[IconSizes.Length][];
            for (int i = 0; i < IconSizes.Length; i++)
            {
                using (Bitmap bitmap = LogoPainter.Render(IconSizes[i]))
                {
                    // Windows reads a 256 px frame as PNG; the smaller frames are stored as
                    // uncompressed DIBs, which every icon consumer understands.
                    frames[i] = IconSizes[i] >= 256 ? EncodePng(bitmap) : IconWriter.EncodeDib(bitmap);

                    if (args.Length == 2)
                    {
                        bitmap.Save(Path.Combine(output, "preview-" + IconSizes[i] + ".png"), ImageFormat.Png);
                    }
                }
            }

            File.WriteAllBytes(Path.Combine(output, "app.ico"), IconWriter.Build(IconSizes, frames));

            using (Bitmap logo = LogoPainter.Render(256))
            {
                File.WriteAllBytes(Path.Combine(output, "logo-256.png"), EncodePng(logo));
            }

            Console.WriteLine("Wrote app.ico and logo-256.png to " + output);
            return 0;
        }

        private static byte[] EncodePng(Bitmap bitmap)
        {
            using (MemoryStream stream = new MemoryStream())
            {
                bitmap.Save(stream, ImageFormat.Png);
                return stream.ToArray();
            }
        }
    }
}
