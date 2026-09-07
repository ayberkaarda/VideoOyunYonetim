using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Text;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// One bar of a <see cref="BarChart"/>: a label, the number that sets the bar length,
    /// and the two pieces of text drawn beside it.
    /// </summary>
    /// <remarks>
    /// The chart draws <see cref="ValueText"/> and <see cref="Annotation"/> exactly as given
    /// and never formats a number itself. Formatting depends on a culture, and the caller is
    /// the only place that knows which one applies, so the decision is left there rather than
    /// being made twice.
    /// </remarks>
    public sealed class BarChartItem
    {
        /// <summary>Initialises a new bar.</summary>
        /// <param name="label">Text drawn to the left of the bar. Never <see langword="null"/> after construction.</param>
        /// <param name="value">The magnitude the bar length is proportional to. Values at or below
        /// zero draw a short stub so the row stays visible instead of vanishing.</param>
        /// <param name="valueText">The value as the caller wants it read, drawn inside the bar
        /// when it fits and immediately after it when it does not.</param>
        /// <param name="annotation">Optional secondary figure drawn in a fixed column on the right.</param>
        public BarChartItem(string label, double value, string valueText, string annotation)
        {
            Label = label ?? string.Empty;
            Value = value;
            ValueText = valueText ?? string.Empty;
            Annotation = annotation ?? string.Empty;
        }

        /// <summary>Gets the text drawn to the left of the bar.</summary>
        public string Label { get; }

        /// <summary>Gets the magnitude the bar length is proportional to.</summary>
        public double Value { get; }

        /// <summary>Gets the already formatted value text.</summary>
        public string ValueText { get; }

        /// <summary>Gets the already formatted secondary figure, or an empty string.</summary>
        public string Annotation { get; }
    }

    /// <summary>
    /// A horizontal bar chart drawn by hand with GDI+.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The bars run horizontally because the labels are words of unpredictable length. A
    /// vertical chart would have to rotate them, abbreviate them or let them collide; a
    /// horizontal one gives every label a full line of its own and grows downwards, which
    /// is the direction a list of categories is read in anyway.
    /// </para>
    /// <para>
    /// Every bar uses the one accent colour rather than a colour per category. The bar
    /// length already says which category is larger, so a second encoding would add no
    /// information while introducing colours whose contrast nobody has measured.
    /// </para>
    /// <para>
    /// The degenerate inputs that break a hand-drawn chart are all handled explicitly: an
    /// empty set draws <see cref="EmptyText"/>; a largest value of zero draws every bar as
    /// a stub instead of dividing by it; a value of zero keeps its stub so the row is still
    /// there to read; values that are all equal legitimately draw as full-length bars; and
    /// a label longer than its bar is ellipsised inside its own fixed column, so no text is
    /// ever painted outside the control.
    /// </para>
    /// </remarks>
    [ToolboxItem(true)]
    [Description("A horizontal bar chart of labelled values, drawn with GDI+.")]
    public class BarChart : Control
    {
        private const string DefaultEmptyText = "Nothing to chart yet.";
        private const int DefaultLabelWidth = 128;
        private const int DefaultAnnotationWidth = 48;
        private const int DefaultBarThickness = 18;

        /// <summary>Shortest row that still leaves the label legible.</summary>
        private const int MinimumRowHeight = 20;

        /// <summary>Length of the stub drawn for a value of zero, so the row does not vanish.</summary>
        private const int MinimumBarLength = 3;

        /// <summary>Narrowest track worth drawing a bar in.</summary>
        private const int MinimumTrackWidth = 24;

        private static readonly BarChartItem[] NoItems = new BarChartItem[0];

        private BarChartItem[] _items = NoItems;
        private string _emptyText = DefaultEmptyText;
        private int _labelWidth = DefaultLabelWidth;
        private int _annotationWidth = DefaultAnnotationWidth;
        private int _barThickness = DefaultBarThickness;
        private Color _barColor = Theme.Accent;
        private Color _trackColor = Theme.SurfaceSunken;

        /// <summary>Initialises a new, empty chart.</summary>
        public BarChart()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Font = Theme.Fonts.Caption;
            TabStop = false;
        }

        /// <summary>Gets the bars currently drawn, in the order they were given.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public IReadOnlyList<BarChartItem> Items
        {
            get { return _items; }
        }

        /// <summary>Gets or sets the message drawn when there is nothing to chart.</summary>
        [Category("Appearance")]
        [DefaultValue(DefaultEmptyText)]
        [Description("Message drawn when there are no bars.")]
        public string EmptyText
        {
            get { return _emptyText; }
            set
            {
                string text = value ?? string.Empty;
                if (!string.Equals(_emptyText, text, StringComparison.Ordinal))
                {
                    _emptyText = text;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the label column in pixels. Treated as an upper bound:
        /// a label column is never allowed to take more than a third of the control, so a
        /// generous setting cannot squeeze the bars out of a narrow chart.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(DefaultLabelWidth)]
        [Description("Width of the label column in pixels, capped at a third of the control.")]
        public int LabelWidth
        {
            get { return _labelWidth; }
            set
            {
                int clamped = value < 0 ? 0 : value;
                if (_labelWidth != clamped)
                {
                    _labelWidth = clamped;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the width of the right-hand annotation column in pixels. Capped at a
        /// fifth of the control for the same reason as <see cref="LabelWidth"/>.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(DefaultAnnotationWidth)]
        [Description("Width of the annotation column in pixels, capped at a fifth of the control.")]
        public int AnnotationWidth
        {
            get { return _annotationWidth; }
            set
            {
                int clamped = value < 0 ? 0 : value;
                if (_annotationWidth != clamped)
                {
                    _annotationWidth = clamped;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the thickness of a bar in pixels. It is also the height a row prefers,
        /// so a chart with two bars draws them at the same weight as a chart with ten.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(DefaultBarThickness)]
        [Description("Thickness of a bar in pixels.")]
        public int BarThickness
        {
            get { return _barThickness; }
            set
            {
                int clamped = value < 1 ? 1 : value;
                if (_barThickness != clamped)
                {
                    _barThickness = clamped;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets the fill colour of a bar.</summary>
        [Category("Appearance")]
        [Description("Fill colour of a bar.")]
        public Color BarColor
        {
            get { return _barColor; }
            set
            {
                if (_barColor != value)
                {
                    _barColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets the colour of the empty track a bar sits in.</summary>
        [Category("Appearance")]
        [Description("Colour of the track behind a bar.")]
        public Color TrackColor
        {
            get { return _trackColor; }
            set
            {
                if (_trackColor != value)
                {
                    _trackColor = value;
                    Invalidate();
                }
            }
        }

        /// <inheritdoc/>
        protected override Size DefaultSize
        {
            get { return new Size(360, 200); }
        }

        /// <summary>
        /// Replaces the bars and repaints.
        /// </summary>
        /// <param name="items">The bars to draw. <see langword="null"/> and null entries are
        /// treated as "nothing to chart" rather than as a failure, so a caller can pass a
        /// partially filled result straight through.</param>
        public void SetItems(IEnumerable<BarChartItem>? items)
        {
            List<BarChartItem> accepted = new List<BarChartItem>();

            if (items != null)
            {
                foreach (BarChartItem item in items)
                {
                    if (item != null)
                    {
                        accepted.Add(item);
                    }
                }
            }

            _items = accepted.Count == 0 ? NoItems : accepted.ToArray();

            // A painted chart says nothing to a screen reader, so the same figures are
            // published as text on the control itself.
            AccessibleDescription = DescribeItems(_items);

            Invalidate();
        }

        /// <summary>Tells the designer whether <see cref="Control.Font"/> was customised.</summary>
        /// <returns><see langword="true"/> when the font differs from the theme default.</returns>
        public bool ShouldSerializeFont()
        {
            return !Equals(Font, Theme.Fonts.Caption);
        }

        /// <summary>Restores the theme font.</summary>
        public override void ResetFont()
        {
            Font = Theme.Fonts.Caption;
        }

        /// <summary>Tells the designer whether <see cref="BarColor"/> was customised.</summary>
        /// <returns><see langword="true"/> when it differs from the theme accent.</returns>
        public bool ShouldSerializeBarColor()
        {
            return _barColor != Theme.Accent;
        }

        /// <summary>Restores the theme accent as the bar colour.</summary>
        public void ResetBarColor()
        {
            BarColor = Theme.Accent;
        }

        /// <summary>Tells the designer whether <see cref="TrackColor"/> was customised.</summary>
        /// <returns><see langword="true"/> when it differs from the theme sunken surface.</returns>
        public bool ShouldSerializeTrackColor()
        {
            return _trackColor != Theme.SurfaceSunken;
        }

        /// <summary>Restores the theme sunken surface as the track colour.</summary>
        public void ResetTrackColor()
        {
            TrackColor = Theme.SurfaceSunken;
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            Graphics g = e.Graphics;
            g.Clear(this.ResolveBackColor());

            Rectangle bounds = new Rectangle(0, 0, Width, Height);
            if (bounds.Width <= 0 || bounds.Height <= 0)
            {
                return;
            }

            BarChartItem[] items = _items;
            if (items.Length == 0)
            {
                DrawCentred(g, _emptyText, bounds);
                return;
            }

            // Rows prefer BarThickness plus one spacing step. When that many rows do not fit
            // they are compressed to MinimumRowHeight, and when even that is not enough the
            // last usable row is spent saying how many bars are not shown, rather than
            // drawing them past the bottom edge.
            int preferredRow = _barThickness + Theme.Space.M;
            int rowHeight = preferredRow;
            int visible = items.Length;
            int hidden = 0;

            if (bounds.Height < MinimumRowHeight)
            {
                DrawCentred(g, DescribeCount(items.Length), bounds);
                return;
            }

            if (bounds.Height / rowHeight < visible)
            {
                rowHeight = MinimumRowHeight;
                int fits = bounds.Height / rowHeight;

                if (fits < visible)
                {
                    visible = fits - 1;
                    if (visible < 1)
                    {
                        DrawCentred(g, DescribeCount(items.Length), bounds);
                        return;
                    }

                    hidden = items.Length - visible;
                }
            }

            // Neither side column may crowd out the bars, however wide it was asked to be.
            int labelWidth = Math.Min(_labelWidth, bounds.Width / 3);
            int annotationWidth = Math.Min(_annotationWidth, bounds.Width / 5);
            int trackLeft = bounds.Left + labelWidth + Theme.Space.M;
            int trackWidth = bounds.Width - labelWidth - annotationWidth - (Theme.Space.M * 2);

            if (trackWidth < MinimumTrackWidth)
            {
                DrawCentred(g, DescribeCount(items.Length), bounds);
                return;
            }

            double largest = 0d;
            for (int i = 0; i < items.Length; i++)
            {
                if (items[i].Value > largest)
                {
                    largest = items[i].Value;
                }
            }

            int barThickness = Math.Min(_barThickness, rowHeight - Theme.Space.S);
            if (barThickness < 1)
            {
                barThickness = 1;
            }

            for (int i = 0; i < visible; i++)
            {
                BarChartItem item = items[i];
                int rowTop = bounds.Top + (i * rowHeight);

                DrawRow(
                    g,
                    item,
                    largest,
                    new Rectangle(bounds.Left, rowTop, labelWidth, rowHeight),
                    new Rectangle(trackLeft, rowTop + ((rowHeight - barThickness) / 2), trackWidth, barThickness),
                    new Rectangle(trackLeft + trackWidth + Theme.Space.M, rowTop, annotationWidth, rowHeight));
            }

            if (hidden > 0)
            {
                TextRenderer.DrawText(
                    g,
                    DescribeHidden(hidden),
                    Font,
                    new Rectangle(trackLeft, bounds.Top + (visible * rowHeight), trackWidth, rowHeight),
                    Theme.TextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        /// <summary>Draws one label, one bar and its two pieces of text.</summary>
        /// <param name="g">The surface to draw on.</param>
        /// <param name="item">The bar to draw.</param>
        /// <param name="largest">The largest value in the set. Zero or less means every bar
        /// draws as a stub, which is what keeps this method from dividing by it.</param>
        /// <param name="labelBounds">The label column for this row.</param>
        /// <param name="track">The full-length track the bar is drawn inside.</param>
        /// <param name="annotationBounds">The annotation column for this row.</param>
        private void DrawRow(
            Graphics g,
            BarChartItem item,
            double largest,
            Rectangle labelBounds,
            Rectangle track,
            Rectangle annotationBounds)
        {
            if (labelBounds.Width > 0)
            {
                TextRenderer.DrawText(
                    g,
                    item.Label,
                    Font,
                    labelBounds,
                    Theme.TextSecondary,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }

            g.FillRoundedRect(_trackColor, track, Theme.Radius.Badge);

            int length = MinimumBarLength;
            if (largest > 0d && item.Value > 0d)
            {
                length = (int)Math.Round(item.Value / largest * track.Width, MidpointRounding.AwayFromZero);
                length = Math.Max(MinimumBarLength, Math.Min(track.Width, length));
            }

            Rectangle bar = new Rectangle(track.X, track.Y, length, track.Height);
            g.FillRoundedRect(_barColor, bar, Theme.Radius.Badge);

            if (item.ValueText.Length > 0)
            {
                Size valueSize = TextRenderer.MeasureText(
                    g,
                    item.ValueText,
                    Font,
                    new Size(int.MaxValue, track.Height),
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

                bool insideBar = bar.Width >= valueSize.Width + (Theme.Space.M * 2);

                if (insideBar)
                {
                    TextRenderer.DrawText(
                        g,
                        item.ValueText,
                        Font,
                        Rectangle.Inflate(bar, -Theme.Space.M, 0),
                        Theme.OnAccent,
                        TextFormatFlags.Right | TextFormatFlags.VerticalCenter |
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                }
                else
                {
                    // The text is clipped to what is left of the track, so a bar that runs
                    // almost to the end pushes its value into an ellipsis instead of over
                    // the annotation column.
                    int left = bar.Right + Theme.Space.S;
                    int available = track.Right - left;

                    if (available > 0)
                    {
                        TextRenderer.DrawText(
                            g,
                            item.ValueText,
                            Font,
                            new Rectangle(left, track.Y, available, track.Height),
                            Theme.TextSecondary,
                            TextFormatFlags.Left | TextFormatFlags.VerticalCenter |
                            TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                    }
                }
            }

            if (item.Annotation.Length > 0 && annotationBounds.Width > 0)
            {
                TextRenderer.DrawText(
                    g,
                    item.Annotation,
                    Font,
                    annotationBounds,
                    Theme.TextSecondary,
                    TextFormatFlags.Right | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.EndEllipsis | TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        /// <summary>Draws a single line of explanation in the middle of the control.</summary>
        private void DrawCentred(Graphics g, string text, Rectangle bounds)
        {
            if (string.IsNullOrEmpty(text))
            {
                return;
            }

            TextRenderer.DrawText(
                g,
                text,
                Font,
                bounds,
                Theme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.WordBreak | TextFormatFlags.EndEllipsis);
        }

        private static string DescribeCount(int count)
        {
            return string.Format(
                CultureInfo.InvariantCulture,
                count == 1 ? "{0} entry, too little room to chart it." : "{0} entries, too little room to chart them.",
                count);
        }

        private static string DescribeHidden(int hidden)
        {
            return string.Format(CultureInfo.InvariantCulture, "and {0} more", hidden);
        }

        private static string DescribeItems(BarChartItem[] items)
        {
            if (items.Length == 0)
            {
                return string.Empty;
            }

            StringBuilder text = new StringBuilder();

            for (int i = 0; i < items.Length; i++)
            {
                if (i > 0)
                {
                    text.Append("; ");
                }

                text.Append(items[i].Label).Append(' ').Append(items[i].ValueText);

                if (items[i].Annotation.Length > 0)
                {
                    text.Append(", ").Append(items[i].Annotation);
                }
            }

            return text.ToString();
        }
    }
}
