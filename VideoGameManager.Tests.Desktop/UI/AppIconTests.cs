using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.IO;
using System.Resources;
using FluentAssertions;
using VideoGameManager.UI;
using VideoGameManager.UI.Controls;
using Xunit;

namespace VideoGameManager.Tests.Desktop.UI
{
    /// <summary>
    /// Covers the application icon: the embedded file holds every frame Windows asks for,
    /// every window shows it, the executable carries the same picture, and no form brings
    /// back a copy of its own through its .resx file.
    /// </summary>
    public sealed class AppIconTests
    {
        private static readonly int[] ExpectedSizes = { 16, 24, 32, 48, 64, 128, 256 };

        [Fact]
        public void The_embedded_icon_holds_every_frame_size_at_32_bits()
        {
            List<IconFrame> frames = ReadFrames();

            frames.Should().HaveCount(ExpectedSizes.Length);
            frames.ConvertAll(frame => frame.Size).Should().Equal(ExpectedSizes);
            frames.Should().OnlyContain(frame => frame.BitsPerPixel == 32);
        }

        [Fact]
        public void The_256_px_frame_is_png_and_the_smaller_frames_are_bitmaps()
        {
            foreach (IconFrame frame in ReadFrames())
            {
                if (frame.Size == 256)
                {
                    frame.IsPng.Should().BeTrue("Windows expects the largest frame to be PNG-compressed");
                }
                else
                {
                    frame.IsPng.Should().BeFalse("a {0} px frame is stored as an uncompressed bitmap", frame.Size);
                }
            }
        }

        [Fact]
        public void A_window_shows_the_application_icon()
        {
            using (ChromelessForm form = new ChromelessForm())
            {
                form.Icon.Should().NotBeNull();
                form.Icon.Should().BeSameAs(AppIcon.Window);
            }
        }

        [Theory]
        [InlineData(typeof(MainForm))]
        [InlineData(typeof(AddGameForm))]
        [InlineData(typeof(BrowseGamesForm))]
        [InlineData(typeof(RecommendationForm))]
        [InlineData(typeof(ReviewGameForm))]
        [InlineData(typeof(StatisticsForm))]
        public void No_form_carries_an_icon_of_its_own(Type formType)
        {
            // A form that set its own icon in the designer would override the shared one,
            // and its .resx would carry the bytes again.
            ComponentResourceManager resources = new ComponentResourceManager(formType);

            object? icon;
            try
            {
                icon = resources.GetObject("$this.Icon");
            }
            catch (MissingManifestResourceException)
            {
                icon = null;
            }

            icon.Should().BeNull();
        }

        [Fact]
        public void The_executable_carries_the_same_icon_as_the_windows()
        {
            // ApplicationIcon is compiled into the assembly as a Win32 resource. Reading it
            // back through the shell and comparing its 32 px frame with the embedded file's
            // proves the two have not drifted apart.
            string assemblyPath = typeof(AppIcon).Assembly.Location;

            using (Icon? fromExecutable = Icon.ExtractAssociatedIcon(assemblyPath))
            using (Stream stream = AppIcon.OpenStream())
            using (Icon fromResource = new Icon(stream, 32, 32))
            {
                fromExecutable.Should().NotBeNull();
                using (Bitmap actual = fromExecutable!.ToBitmap())
                using (Bitmap expected = fromResource.ToBitmap())
                {
                    actual.Size.Should().Be(expected.Size);
                    for (int y = 0; y < expected.Height; y++)
                    {
                        for (int x = 0; x < expected.Width; x++)
                        {
                            actual.GetPixel(x, y).Should().Be(expected.GetPixel(x, y), "pixel ({0}, {1}) differs", x, y);
                        }
                    }
                }
            }
        }

        private static List<IconFrame> ReadFrames()
        {
            using (Stream stream = AppIcon.OpenStream())
            using (MemoryStream copy = new MemoryStream())
            {
                stream.CopyTo(copy);
                byte[] bytes = copy.ToArray();

                BitConverter.ToUInt16(bytes, 0).Should().Be(0, "the ICONDIR reserved field is zero");
                BitConverter.ToUInt16(bytes, 2).Should().Be(1, "type 1 is an icon, 2 would be a cursor");
                int count = BitConverter.ToUInt16(bytes, 4);

                List<IconFrame> frames = new List<IconFrame>(count);
                for (int i = 0; i < count; i++)
                {
                    int entry = 6 + (i * 16);
                    int width = bytes[entry] == 0 ? 256 : bytes[entry];
                    int height = bytes[entry + 1] == 0 ? 256 : bytes[entry + 1];
                    int bitsPerPixel = BitConverter.ToUInt16(bytes, entry + 6);
                    int length = BitConverter.ToInt32(bytes, entry + 8);
                    int offset = BitConverter.ToInt32(bytes, entry + 12);

                    width.Should().Be(height, "every frame is square");
                    (offset + length).Should().BeLessThanOrEqualTo(bytes.Length, "frame {0} lies inside the file", i);

                    bool isPng = bytes[offset] == 0x89 && bytes[offset + 1] == (byte)'P'
                        && bytes[offset + 2] == (byte)'N' && bytes[offset + 3] == (byte)'G';
                    frames.Add(new IconFrame(width, bitsPerPixel, isPng));
                }

                return frames;
            }
        }

        private sealed record IconFrame(int Size, int BitsPerPixel, bool IsPng);
    }
}
