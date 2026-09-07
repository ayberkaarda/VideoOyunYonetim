using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// A large, card-shaped button: a tinted icon disc on the left, a title and a
    /// one-line description on the right. Built for menu screens, where a row of
    /// identical coloured rectangles gives the user nothing to aim at.
    /// </summary>
    /// <remarks>
    /// The tone only colours the icon disc, never the whole card. That is what keeps
    /// the label readable: the title is always <see cref="Theme.TextPrimary"/> on
    /// <see cref="Theme.SurfaceRaised"/> (16.56:1), instead of white on a saturated
    /// fill that cannot clear 4.5:1.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [Description("A card-shaped button with a tinted icon disc, a title and a description.")]
    public class FlatCardButton : Button
    {
        private CardTone _tone = CardTone.Accent;
        private string _description = string.Empty;
        private string _glyph = string.Empty;
        private int _cornerRadius = Theme.Radius.Card;
        private bool _hovered;
        private bool _pressed;

        /// <summary>Initialises a new card button.</summary>
        public FlatCardButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Color.Transparent;
            Font = Theme.Fonts.BodyLargeStrong;
            Cursor = Cursors.Hand;
            UseVisualStyleBackColor = false;
            AutoSize = false;
        }

        /// <summary>Gets or sets the semantic tone of the icon disc.</summary>
        [Category("Appearance")]
        [DefaultValue(CardTone.Accent)]
        [Description("Semantic tone of the icon disc.")]
        public CardTone Tone
        {
            get { return _tone; }
            set
            {
                if (_tone != value)
                {
                    _tone = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets the secondary line under the title.</summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("One-line description shown under the title.")]
        public string Description
        {
            get { return _description; }
            set
            {
                string text = value ?? string.Empty;
                if (!string.Equals(_description, text, StringComparison.Ordinal))
                {
                    _description = text;
                    Invalidate();
                }
            }
        }

        /// <summary>
        /// Gets or sets the character drawn in the icon disc. Expects a Segoe MDL2 Assets
        /// code point such as U+E710; any string is rendered as-is if the icon font is missing.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("Segoe MDL2 Assets code point drawn in the icon disc.")]
        public string Glyph
        {
            get { return _glyph; }
            set
            {
                string text = value ?? string.Empty;
                if (!string.Equals(_glyph, text, StringComparison.Ordinal))
                {
                    _glyph = text;
                    Invalidate();
                }
            }
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

        /// <inheritdoc/>
        protected override Size DefaultSize
        {
            get { return new Size(260, Theme.Metrics.Card); }
        }

        /// <summary>Keeps the designer from serialising the theme font.</summary>
        /// <returns><see langword="true"/> when the font differs from the theme default.</returns>
        public bool ShouldSerializeFont()
        {
            return !Equals(Font, Theme.Fonts.BodyLargeStrong);
        }

        /// <summary>Restores the theme font.</summary>
        public override void ResetFont()
        {
            Font = Theme.Fonts.BodyLargeStrong;
        }

        /// <inheritdoc/>
        protected override void OnMouseEnter(EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent.Button == MouseButtons.Left)
            {
                _pressed = true;
                Invalidate();
            }

            base.OnMouseDown(mevent);
        }

        /// <inheritdoc/>
        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            _pressed = false;
            Invalidate();
            base.OnMouseUp(mevent);
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            base.OnEnabledChanged(e);
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(EventArgs e)
        {
            Invalidate();
            base.OnLostFocus(e);
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (pevent == null)
            {
                return;
            }

            Graphics g = pevent.Graphics;
            g.Clear(this.ResolveBackColor());

            Rectangle bounds = new Rectangle(0, 0, Width, Height);
            bool enabled = Enabled;

            Color fill = !enabled
                ? Theme.SurfaceDisabled
                : (_pressed ? Theme.SurfacePressed : (_hovered ? Theme.SurfaceHover : Theme.SurfaceRaised));
            Color border = enabled
                ? (_hovered || _pressed || Focused ? Theme.Accent : Theme.Border)
                : Theme.BorderDisabled;
            int borderWidth = enabled && (_hovered || _pressed || Focused)
                ? Theme.Metrics.FocusRing
                : Theme.Metrics.BorderThickness;

            g.FillRoundedRect(fill, bounds, _cornerRadius);
            g.DrawRoundedRect(border, bounds, _cornerRadius, borderWidth);

            // Icon disc.
            int disc = Theme.Metrics.IconDisc;
            Rectangle discBounds = new Rectangle(
                Theme.Space.L,
                (Height - disc) / 2,
                disc,
                disc);

            Color discFill = enabled ? Theme.CardToneSurface(_tone) : Theme.SurfaceSunken;
            Color discGlyph = enabled ? Theme.CardToneForeground(_tone) : Theme.TextDisabled;

            System.Drawing.Drawing2D.SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (SolidBrush discBrush = new SolidBrush(discFill))
            {
                g.FillEllipse(discBrush, discBounds);
            }

            g.SmoothingMode = previous;

            TextRenderer.DrawText(
                g,
                _glyph,
                Theme.Fonts.Glyph16,
                discBounds,
                discGlyph,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            // Text block.
            int textLeft = discBounds.Right + Theme.Space.L;
            int textWidth = Width - textLeft - Theme.Space.L;
            if (textWidth <= 0)
            {
                return;
            }

            Color titleColor = enabled ? Theme.TextPrimary : Theme.TextDisabled;
            Color descriptionColor = enabled ? Theme.TextSecondary : Theme.TextDisabled;

            bool hasDescription = _description.Length > 0;
            int titleHeight = Font.Height;
            int descriptionHeight = hasDescription ? Theme.Fonts.Caption.Height : 0;
            int blockHeight = titleHeight + (hasDescription ? Theme.Space.S + descriptionHeight : 0);
            int top = (Height - blockHeight) / 2;

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                new Rectangle(textLeft, top, textWidth, titleHeight),
                titleColor,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            if (hasDescription)
            {
                TextRenderer.DrawText(
                    g,
                    _description,
                    Theme.Fonts.Caption,
                    new Rectangle(textLeft, top + titleHeight + Theme.Space.S, textWidth, descriptionHeight),
                    descriptionColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }
    }
}
