using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;

namespace LogoRenderer
{
    /// <summary>
    /// Assembles a Windows .ico file from already encoded frames.
    /// </summary>
    /// <remarks>
    /// The layout is the ICONDIR header (6 bytes), one ICONDIRENTRY per frame (16 bytes
    /// each), then the frame data in the same order. A width or height of 256 is written as
    /// 0, which is how the format spells it.
    /// </remarks>
    internal static class IconWriter
    {
        private const int HeaderSize = 6;
        private const int EntrySize = 16;
        private const int InfoHeaderSize = 40;

        /// <summary>Builds the icon file.</summary>
        /// <param name="sizes">The side of each frame in pixels.</param>
        /// <param name="frames">The encoded frame data, in the same order as <paramref name="sizes"/>.</param>
        /// <returns>The bytes of the .ico file.</returns>
        public static byte[] Build(int[] sizes, byte[][] frames)
        {
            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write((ushort)0);            // reserved
                writer.Write((ushort)1);            // type: icon
                writer.Write((ushort)sizes.Length); // frame count

                int offset = HeaderSize + (EntrySize * sizes.Length);
                for (int i = 0; i < sizes.Length; i++)
                {
                    byte side = sizes[i] >= 256 ? (byte)0 : (byte)sizes[i];
                    writer.Write(side);                 // width
                    writer.Write(side);                 // height
                    writer.Write((byte)0);              // palette size: none
                    writer.Write((byte)0);              // reserved
                    writer.Write((ushort)1);            // colour planes
                    writer.Write((ushort)32);           // bits per pixel
                    writer.Write(frames[i].Length);     // data size
                    writer.Write(offset);               // data offset
                    offset += frames[i].Length;
                }

                foreach (byte[] frame in frames)
                {
                    writer.Write(frame);
                }

                writer.Flush();
                return stream.ToArray();
            }
        }

        /// <summary>
        /// Encodes a 32-bit ARGB bitmap as an icon DIB: a BITMAPINFOHEADER with the height
        /// doubled, the BGRA rows bottom-up, then a 1-bit AND mask.
        /// </summary>
        /// <param name="bitmap">The frame to encode.</param>
        /// <returns>The DIB bytes.</returns>
        public static byte[] EncodeDib(Bitmap bitmap)
        {
            int width = bitmap.Width;
            int height = bitmap.Height;
            int maskStride = ((width + 31) / 32) * 4;

            byte[] pixels = new byte[width * height * 4];
            BitmapData data = bitmap.LockBits(
                new Rectangle(0, 0, width, height), ImageLockMode.ReadOnly, PixelFormat.Format32bppArgb);
            try
            {
                for (int y = 0; y < height; y++)
                {
                    IntPtr row = data.Scan0 + (y * data.Stride);
                    Marshal.Copy(row, pixels, y * width * 4, width * 4);
                }
            }
            finally
            {
                bitmap.UnlockBits(data);
            }

            using (MemoryStream stream = new MemoryStream())
            using (BinaryWriter writer = new BinaryWriter(stream))
            {
                writer.Write(InfoHeaderSize);       // biSize
                writer.Write(width);                // biWidth
                writer.Write(height * 2);           // biHeight: XOR image plus AND mask
                writer.Write((ushort)1);            // biPlanes
                writer.Write((ushort)32);           // biBitCount
                writer.Write(0);                    // biCompression: BI_RGB
                writer.Write((width * height * 4) + (maskStride * height)); // biSizeImage
                writer.Write(0);                    // biXPelsPerMeter
                writer.Write(0);                    // biYPelsPerMeter
                writer.Write(0);                    // biClrUsed
                writer.Write(0);                    // biClrImportant

                for (int y = height - 1; y >= 0; y--)
                {
                    writer.Write(pixels, y * width * 4, width * 4);
                }

                // The AND mask marks fully transparent pixels. 32-bit consumers read the
                // alpha channel instead, but older ones still look at the mask.
                for (int y = height - 1; y >= 0; y--)
                {
                    byte[] maskRow = new byte[maskStride];
                    for (int x = 0; x < width; x++)
                    {
                        if (pixels[(((y * width) + x) * 4) + 3] == 0)
                        {
                            maskRow[x / 8] |= (byte)(0x80 >> (x % 8));
                        }
                    }

                    writer.Write(maskRow);
                }

                writer.Flush();
                return stream.ToArray();
            }
        }
    }
}
