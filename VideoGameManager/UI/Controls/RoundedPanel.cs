using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// A container with rounded corners and a hairline border: the card surface
    /// everything else sits on.
    /// </summary>
    /// <remarks>
    /// The panel paints the nearest opaque ancestor colour first and then the rounded
    /// fill, so the corners show the page behind them instead of a square of card
    /// colour. <see cref="Control.BackColor"/> is the fill, which keeps child controls
    /// that ask their parent what is behind them on the card colour rather than on the
    /// page colour.
    /// </remarks>
    [ToolboxItem(true)]
    [Description("A container with rounded corners and a hairline border.")]
    public class RoundedPanel : Panel
    {
        private int _cornerRadius = Theme.Radius.Card;
        private Color _borderColor = Theme.Border;
        private int _borderThickness = Theme.Metrics.BorderThickness;

        /// <summary>Initialises a new rounded panel.</summary>
        public RoundedPanel()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Theme.SurfaceRaised;
        }

        /// <summary>Gets or sets the corner radius in pixels.</summary>
        [Category("Appearance")]
        [DefaultValue(Theme.Radius.Card)]
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

        /// <summary>Gets or sets the fill colour of the card. An alias for
        /// <see cref="Control.BackColor"/>, which is what children inherit.</summary>
        [Category("Appearance")]
        [Description("Fill colour of the card.")]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Color FillColor
        {
            get { return BackColor; }
            set { BackColor = value; }
        }

        /// <summary>Gets or sets the border colour. <see cref="Color.Empty"/> hides the border.</summary>
        [Category("Appearance")]
        [Description("Border colour. Set to Empty to hide the border.")]
        public Color BorderColor
        {
            get { return _borderColor; }
            set
            {
                if (_borderColor != value)
                {
                    _borderColor = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets the border width in pixels.</summary>
        [Category("Appearance")]
        [DefaultValue(Theme.Metrics.BorderThickness)]
        [Description("Border width in pixels.")]
        public int BorderThickness
        {
            get { return _borderThickness; }
            set
            {
                int clamped = value < 0 ? 0 : value;
                if (_borderThickness != clamped)
                {
                    _borderThickness = clamped;
                    Invalidate();
                }
            }
        }

        /// <inheritdoc/>
        protected override Padding DefaultPadding
        {
            get { return new Padding(Theme.Space.L); }
        }

        /// <summary>Tells the designer whether <see cref="Control.BackColor"/> was customised.</summary>
        /// <returns><see langword="true"/> when it differs from the theme default.</returns>
        public bool ShouldSerializeBackColor()
        {
            return BackColor != Theme.SurfaceRaised;
        }

        /// <summary>Restores the theme fill colour.</summary>
        public override void ResetBackColor()
        {
            BackColor = Theme.SurfaceRaised;
        }

        /// <summary>Tells the designer whether <see cref="BorderColor"/> was customised.</summary>
        /// <returns><see langword="true"/> when it differs from the theme default.</returns>
        public bool ShouldSerializeBorderColor()
        {
            return _borderColor != Theme.Border;
        }

        /// <summary>Restores the theme border colour.</summary>
        public void ResetBorderColor()
        {
            BorderColor = Theme.Border;
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
            g.FillRoundedRect(BackColor, bounds, _cornerRadius);

            if (_borderThickness > 0 && _borderColor != Color.Empty)
            {
                g.DrawRoundedRect(_borderColor, bounds, _cornerRadius, _borderThickness);
            }

            base.OnPaint(e);
        }
    }
}
