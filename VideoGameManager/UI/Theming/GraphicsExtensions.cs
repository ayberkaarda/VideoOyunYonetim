using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace VideoGameManager.UI.Theming
{
    /// <summary>
    /// Drawing primitives shared by every owner-drawn control: rounded rectangles
    /// and the aspect-preserving fit used by <see cref="Controls.CoverImageBox"/>.
    /// </summary>
    public static class GraphicsExtensions
    {
        /// <summary>
        /// Finds the colour actually painted behind a control.
        /// </summary>
        /// <remarks>
        /// An owner-drawn control with rounded corners has to fill the area outside the
        /// rounded path with whatever is behind it. <see cref="Control.BackColor"/> of the
        /// immediate parent is not enough: a parent may itself be transparent, and clearing
        /// to <see cref="Color.Transparent"/> writes alpha zero into an opaque buffer, which
        /// shows up as a hard white or black square rather than as transparency.
        /// </remarks>
        /// <param name="control">The control asking what is behind it.</param>
        /// <returns>The nearest opaque ancestor colour, or the theme surface.</returns>
        public static Color ResolveBackColor(this Control control)
        {
            Control? current = control != null ? control.Parent : null;

            while (current != null)
            {
                if (current.BackColor.A == 255)
                {
                    return current.BackColor;
                }

                current = current.Parent;
            }

            return Theme.Surface;
        }

        /// <summary>
        /// Builds a rounded-rectangle path.
        /// </summary>
        /// <param name="bounds">The rectangle to round.</param>
        /// <param name="radius">Corner radius in pixels. Zero produces a plain rectangle;
        /// a radius larger than half the shortest side is clamped.</param>
        /// <returns>A new <see cref="GraphicsPath"/>. The caller owns it and must dispose it.</returns>
        public static GraphicsPath RoundedRect(this Rectangle bounds, int radius)
        {
            GraphicsPath path = new GraphicsPath();

            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return path;
            }

            int max = Math.Min(bounds.Width, bounds.Height) / 2;
            int r = Math.Max(0, Math.Min(radius, max));

            if (r == 0)
            {
                path.AddRectangle(bounds);
                path.CloseFigure();
                return path;
            }

            int d = r * 2;
            path.AddArc(bounds.Left, bounds.Top, d, d, 180f, 90f);
            path.AddArc(bounds.Right - d, bounds.Top, d, d, 270f, 90f);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0f, 90f);
            path.AddArc(bounds.Left, bounds.Bottom - d, d, d, 90f, 90f);
            path.CloseFigure();
            return path;
        }

        /// <summary>
        /// Fills a rounded rectangle with a solid colour.
        /// </summary>
        /// <param name="g">The surface to draw on.</param>
        /// <param name="color">Fill colour.</param>
        /// <param name="bounds">The rectangle to fill.</param>
        /// <param name="radius">Corner radius in pixels.</param>
        public static void FillRoundedRect(this Graphics g, Color color, Rectangle bounds, int radius)
        {
            if (g == null || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            using (GraphicsPath path = bounds.RoundedRect(radius))
            using (SolidBrush brush = new SolidBrush(color))
            {
                SmoothingMode previous = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPath(brush, path);
                g.SmoothingMode = previous;
            }
        }

        /// <summary>
        /// Strokes a rounded rectangle. The path is inset by half the pen width so the
        /// stroke lands inside <paramref name="bounds"/> instead of straddling its edge
        /// and being clipped.
        /// </summary>
        /// <param name="g">The surface to draw on.</param>
        /// <param name="color">Stroke colour.</param>
        /// <param name="bounds">The rectangle to stroke.</param>
        /// <param name="radius">Corner radius in pixels.</param>
        /// <param name="thickness">Stroke width in pixels.</param>
        public static void DrawRoundedRect(this Graphics g, Color color, Rectangle bounds, int radius, int thickness)
        {
            if (g == null || thickness <= 0 || bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            int inset = thickness / 2;
            Rectangle stroked = new Rectangle(
                bounds.X + inset,
                bounds.Y + inset,
                Math.Max(0, bounds.Width - thickness),
                Math.Max(0, bounds.Height - thickness));

            if (stroked.Width <= 0 || stroked.Height <= 0)
            {
                return;
            }

            using (GraphicsPath path = stroked.RoundedRect(Math.Max(0, radius - inset)))
            using (Pen pen = new Pen(color, thickness))
            {
                pen.Alignment = PenAlignment.Center;
                SmoothingMode previous = g.SmoothingMode;
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.DrawPath(pen, path);
                g.SmoothingMode = previous;
            }
        }

        /// <summary>
        /// Computes the <see cref="System.Windows.Forms.PictureBoxSizeMode.Zoom"/> rectangle:
        /// the largest rectangle with the aspect ratio of <paramref name="content"/> that fits
        /// inside <paramref name="container"/>, centred. The content is never enlarged past
        /// the container, but it <em>is</em> enlarged to fill it, matching Zoom semantics.
        /// </summary>
        /// <param name="content">Natural size of the content, e.g. an image.</param>
        /// <param name="container">The area to fit into.</param>
        /// <returns>The centred, aspect-preserved destination rectangle, or
        /// <see cref="Rectangle.Empty"/> if either input is degenerate.</returns>
        public static Rectangle FitInside(this Size content, Rectangle container)
        {
            if (content.Width <= 0 || content.Height <= 0 || container.Width <= 0 || container.Height <= 0)
            {
                return Rectangle.Empty;
            }

            double scale = Math.Min(
                (double)container.Width / content.Width,
                (double)container.Height / content.Height);

            int width = Math.Max(1, (int)Math.Round(content.Width * scale));
            int height = Math.Max(1, (int)Math.Round(content.Height * scale));

            return new Rectangle(
                container.X + ((container.Width - width) / 2),
                container.Y + ((container.Height - height) / 2),
                width,
                height);
        }
    }
}
