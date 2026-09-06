using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// A small chip showing a 0-10 score, coloured by band: 8 and above reads as
    /// success, 6 and above as warning, anything lower as danger. A null score
    /// renders as an em dash, so "no score yet" never looks like a bad score.
    /// </summary>
    /// <remarks>
    /// The three band colours are the dark, measured variants from <see cref="Theme"/>,
    /// each at least 5.8:1 against the white numerals. A pastel green chip with white
    /// text would fail at roughly 1.6:1 and is exactly the mistake this control exists
    /// to prevent.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultProperty("Score")]
    [Description("A 0-10 score chip coloured by band.")]
    public class RatingBadge : Control
    {
        /// <summary>Score at or above which the badge reads as success.</summary>
        public const double SuccessThreshold = 8d;

        /// <summary>Score at or above which the badge reads as a warning.</summary>
        public const double WarningThreshold = 6d;

        private const string EmptyScoreText = "\u2014";

        private double? _score;
        private int _cornerRadius = Theme.Radius.Badge;

        /// <summary>Initialises a new rating badge with no score.</summary>
        public RatingBadge()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Font = Theme.Fonts.BodyStrong;
            TabStop = false;
        }

        /// <summary>
        /// Gets or sets the score to display, or <see langword="null"/> when the item
        /// has not been scored.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(null)]
        [Description("The 0-10 score, or null when the item has no score.")]
        public double? Score
        {
            get { return _score; }
            set
            {
                if (_score != value)
                {
                    _score = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets the corner radius in pixels.</summary>
        [Category("Appearance")]
        [DefaultValue(Theme.Radius.Badge)]
        [Description("Corner radius in pixels.")]
        public int CornerRadius
        {
            get { return _cornerRadius; }
            set
            {
                int clamped = value < 0 ? 0 : value;
                if (_cornerRadius != clamped)
                {
                    _cornerRadius = clamped;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets the background colour the current <see cref="Score"/> maps to.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color BandColor
        {
            get { return BandColorFor(_score); }
        }

        /// <inheritdoc/>
        protected override Size DefaultSize
        {
            get { return new Size(Theme.Metrics.BadgeMinWidth, Theme.Metrics.Badge); }
        }

        /// <summary>
        /// Maps a score to its band colour. Exposed so a list row can paint the same
        /// chip without instantiating a control per row.
        /// </summary>
        /// <param name="score">The score, or <see langword="null"/>.</param>
        /// <returns>Success, warning, danger, or the neutral colour for a null score.</returns>
        public static Color BandColorFor(double? score)
        {
            if (!score.HasValue)
            {
                return Theme.TextSecondary;
            }

            if (score.Value >= SuccessThreshold)
            {
                return Theme.Success;
            }

            return score.Value >= WarningThreshold ? Theme.Warning : Theme.Danger;
        }

        /// <summary>Formats a score the way the badge shows it.</summary>
        /// <param name="score">The score, or <see langword="null"/>.</param>
        /// <returns>One decimal place, or an em dash when there is no score.</returns>
        public static string FormatScore(double? score)
        {
            return score.HasValue
                ? score.Value.ToString("0.0", CultureInfo.InvariantCulture)
                : EmptyScoreText;
        }

        /// <summary>Tells the designer whether <see cref="Control.Font"/> was customised.</summary>
        /// <returns><see langword="true"/> when the font differs from the theme default.</returns>
        public bool ShouldSerializeFont()
        {
            return !Equals(Font, Theme.Fonts.BodyStrong);
        }

        /// <summary>Restores the theme font.</summary>
        public override void ResetFont()
        {
            Font = Theme.Fonts.BodyStrong;
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
            Color fill = Enabled ? BandColorFor(_score) : Theme.SurfaceDisabled;
            Color foreground = Enabled ? Theme.OnAccent : Theme.TextDisabled;

            g.FillRoundedRect(fill, bounds, _cornerRadius);

            TextRenderer.DrawText(
                g,
                FormatScore(_score),
                Font,
                bounds,
                foreground,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }

        /// <inheritdoc/>
        protected override void OnTextChanged(EventArgs e)
        {
            // The chip renders Score, never Text, so a stray Text assignment is ignored.
            base.OnTextChanged(e);
            Invalidate();
        }
    }
}
