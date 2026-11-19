using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;

namespace LogoRenderer
{
    /// <summary>
    /// Draws the application logo: three game cases fanned out like a collection, the front
    /// one carrying a controller on its cover, on a rounded tile in the accent blue.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every coordinate is on a 256 x 256 design grid and is scaled to the requested size,
    /// so one drawing serves every frame of the icon. The main edges sit on multiples of 16
    /// so they land on whole pixels at 16, 32, 64, 128 and 256 px. assets/logo.svg uses the
    /// same numbers; when one changes, the other has to follow.
    /// </para>
    /// <para>
    /// Frames below 32 px drop the spine line and the controller's cut-outs, and draw the
    /// controller as a solid silhouette on whole pixels: at that size the detail only turns
    /// into grey speckle.
    /// </para>
    /// <para>
    /// The colours come from the application theme (UI/Theming/Theme.cs): the tile runs
    /// from a lighter step of Accent (#0B63CE) down to AccentHover (#0A56B4), the controller
    /// is Accent, the nearer back case is AccentSubtle (#E7F0FC) and the farther one a step
    /// between the two.
    /// </para>
    /// </remarks>
    internal static class LogoPainter
    {
        /// <summary>The side of the design grid every coordinate is measured on.</summary>
        public const float Grid = 256f;

        /// <summary>Frames smaller than this are drawn without fine detail.</summary>
        public const int DetailThreshold = 32;

        private static readonly Color TileTop = Color.FromArgb(0x2A, 0x7D, 0xE1);
        private static readonly Color TileBottom = Color.FromArgb(0x0A, 0x56, 0xB4);
        private static readonly Color Cover = Color.FromArgb(0xFF, 0xFF, 0xFF);
        private static readonly Color CaseNear = Color.FromArgb(0xC6, 0xDC, 0xF8);
        private static readonly Color Spine = Color.FromArgb(0xE7, 0xF0, 0xFC);
        private static readonly Color CaseFar = Color.FromArgb(0x8F, 0xBA, 0xEE);
        private static readonly Color Ink = Color.FromArgb(0x0B, 0x63, 0xCE);

        private static readonly RectangleF Tile = new RectangleF(8f, 8f, 240f, 240f);
        private const float TileRadius = 56f;

        // The front case. The two behind it are the same rectangle turned about its
        // bottom-right corner, so they fan up and to the right and never leave the tile.
        private static readonly RectangleF Case = new RectangleF(32f, 64f, 128f, 160f);
        private const float CaseRadius = 14f;
        private const float NearAngle = 12f;
        private const float FarAngle = 24f;

        /// <summary>Renders the logo into a new 32-bit ARGB bitmap.</summary>
        /// <param name="size">The width and height of the bitmap in pixels.</param>
        /// <returns>The rendered bitmap; the caller owns it.</returns>
        public static Bitmap Render(int size)
        {
            Bitmap bitmap = new Bitmap(size, size, PixelFormat.Format32bppArgb);
            using (Graphics g = Graphics.FromImage(bitmap))
            {
                g.Clear(Color.Transparent);
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                g.CompositingQuality = CompositingQuality.HighQuality;
                g.ScaleTransform(size / Grid, size / Grid);
                Paint(g, size >= DetailThreshold);
            }

            return bitmap;
        }

        private static void Paint(Graphics g, bool detailed)
        {
            using (LinearGradientBrush tileBrush = new LinearGradientBrush(
                new PointF(0f, Tile.Top), new PointF(0f, Tile.Bottom), TileTop, TileBottom))
            using (GraphicsPath tilePath = RoundedRectangle(Tile, TileRadius))
            {
                g.FillPath(tileBrush, tilePath);
            }

            PointF pivot = new PointF(Case.Right, Case.Bottom);
            FillCase(g, CaseFar, pivot, FarAngle);
            FillCase(g, CaseNear, pivot, NearAngle);
            FillCase(g, Cover, pivot, 0f);

            using (SolidBrush ink = new SolidBrush(Ink))
            using (SolidBrush cover = new SolidBrush(Cover))
            using (SolidBrush spine = new SolidBrush(Spine))
            {
                if (detailed)
                {
                    g.FillRectangle(spine, 40f, 80f, 8f, 128f);

                    // A controller: a pill-shaped body and two round grips, with a
                    // direction pad and two face buttons cut out of it.
                    FillRounded(g, ink, new RectangleF(56f, 120f, 96f, 40f), 20f);
                    g.FillEllipse(ink, 56f, 136f, 40f, 40f);
                    g.FillEllipse(ink, 112f, 136f, 40f, 40f);

                    g.FillRectangle(cover, 66f, 135f, 28f, 10f);
                    g.FillRectangle(cover, 75f, 126f, 10f, 28f);
                    g.FillEllipse(cover, 115f, 140f, 12f, 12f);
                    g.FillEllipse(cover, 129f, 127f, 12f, 12f);
                }
                else
                {
                    FillRounded(g, ink, new RectangleF(48f, 128f, 96f, 32f), 16f);
                    g.FillEllipse(ink, 48f, 144f, 32f, 32f);
                    g.FillEllipse(ink, 112f, 144f, 32f, 32f);
                }
            }
        }

        private static void FillCase(Graphics g, Color color, PointF pivot, float angle)
        {
            GraphicsState state = g.Save();
            g.TranslateTransform(pivot.X, pivot.Y);
            g.RotateTransform(angle);
            g.TranslateTransform(-pivot.X, -pivot.Y);
            using (SolidBrush brush = new SolidBrush(color))
            {
                FillRounded(g, brush, Case, CaseRadius);
            }

            g.Restore(state);
        }

        private static void FillRounded(Graphics g, Brush brush, RectangleF bounds, float radius)
        {
            using (GraphicsPath path = RoundedRectangle(bounds, radius))
            {
                g.FillPath(brush, path);
            }
        }

        private static GraphicsPath RoundedRectangle(RectangleF bounds, float radius)
        {
            float diameter = radius * 2f;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(bounds.Left, bounds.Top, diameter, diameter, 180f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Top, diameter, diameter, 270f, 90f);
            path.AddArc(bounds.Right - diameter, bounds.Bottom - diameter, diameter, diameter, 0f, 90f);
            path.AddArc(bounds.Left, bounds.Bottom - diameter, diameter, diameter, 90f, 90f);
            path.CloseFigure();
            return path;
        }
    }
}
