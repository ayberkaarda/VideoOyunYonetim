using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// Visual weight of a <see cref="FlatButton"/>.
    /// </summary>
    public enum ButtonKind
    {
        /// <summary>Filled accent. One per screen: the action the user most likely wants.</summary>
        Primary,

        /// <summary>Outlined neutral. Everything that is not the primary action.</summary>
        Secondary,

        /// <summary>Filled red. Destructive or irreversible actions only.</summary>
        Danger
    }

    /// <summary>
    /// A flat, rounded, owner-drawn button.
    /// </summary>
    /// <remarks>
    /// Derives from <see cref="Button"/> rather than <see cref="Control"/> so that
    /// mnemonics, Space/Enter activation, <see cref="Form.AcceptButton"/>,
    /// <see cref="Form.CancelButton"/> and <see cref="IButtonControl.DialogResult"/>
    /// keep working; only the painting is replaced.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultProperty("Text")]
    [DefaultEvent("Click")]
    [Description("A flat, rounded button with primary, secondary and danger variants.")]
    public class FlatButton : Button
    {
        private ButtonKind _kind = ButtonKind.Primary;
        private int _cornerRadius = Theme.Radius.Control;
        private bool _hovered;
        private bool _pressed;

        /// <summary>Initialises a new button with the default primary appearance.</summary>
        public FlatButton()
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

        /// <summary>Gets or sets the visual weight of the button.</summary>
        [Category("Appearance")]
        [DefaultValue(ButtonKind.Primary)]
        [Description("Visual weight: filled accent, outlined neutral, or filled red.")]
        public ButtonKind Kind
        {
            get { return _kind; }
            set
            {
                if (_kind != value)
                {
                    _kind = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets the corner radius in pixels.</summary>
        [Category("Appearance")]
        [DefaultValue(Theme.Radius.Control)]
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
            get { return new Size(160, Theme.Metrics.Button); }
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
        protected override void OnMouseEnter(System.EventArgs e)
        {
            _hovered = true;
            Invalidate();
            base.OnMouseEnter(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(System.EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            if (mevent != null && mevent.Button == MouseButtons.Left)
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
        protected override void OnEnabledChanged(System.EventArgs e)
        {
            _hovered = false;
            _pressed = false;
            Cursor = Enabled ? Cursors.Hand : Cursors.Default;
            base.OnEnabledChanged(e);
        }

        /// <inheritdoc/>
        protected override void OnGotFocus(System.EventArgs e)
        {
            Invalidate();
            base.OnGotFocus(e);
        }

        /// <inheritdoc/>
        protected override void OnLostFocus(System.EventArgs e)
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
            Color fill;
            Color foreground;
            Color border = Color.Empty;

            if (!Enabled)
            {
                fill = _kind == ButtonKind.Secondary ? Theme.SurfaceDisabled : Theme.AccentDisabled;
                foreground = Theme.TextDisabled;
                border = _kind == ButtonKind.Secondary ? Theme.BorderDisabled : Color.Empty;
            }
            else
            {
                switch (_kind)
                {
                    case ButtonKind.Secondary:
                        fill = _pressed ? Theme.SurfacePressed : (_hovered ? Theme.SurfaceHover : Theme.SurfaceRaised);
                        foreground = Theme.TextPrimary;
                        border = Theme.InputBorder;
                        break;

                    case ButtonKind.Danger:
                        fill = _pressed ? Theme.DangerPressed : (_hovered ? Theme.DangerHover : Theme.Danger);
                        foreground = Theme.OnAccent;
                        break;

                    default:
                        fill = _pressed ? Theme.AccentPressed : (_hovered ? Theme.AccentHover : Theme.Accent);
                        foreground = Theme.OnAccent;
                        break;
                }
            }

            g.FillRoundedRect(fill, bounds, _cornerRadius);

            if (border != Color.Empty)
            {
                g.DrawRoundedRect(border, bounds, _cornerRadius, Theme.Metrics.BorderThickness);
            }

            if (Focused && Enabled)
            {
                // The ring is drawn inside the fill so it stays visible on a filled
                // button, where an outer ring would blend into the form background.
                // ShowFocusCues is deliberately not consulted: it returns false until the
                // user touches the keyboard, which hides the ring from anyone checking
                // that focus is visible at all.
                Color ring = _kind == ButtonKind.Secondary ? Theme.Accent : Theme.OnAccent;
                Rectangle inner = Rectangle.Inflate(bounds, -Theme.Space.S, -Theme.Space.S);
                g.DrawRoundedRect(ring, inner, _cornerRadius, Theme.Metrics.FocusRing);
            }

            TextRenderer.DrawText(
                g,
                Text,
                Font,
                bounds,
                foreground,
                TextFormatFlags.HorizontalCenter |
                TextFormatFlags.VerticalCenter |
                TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine);
        }
    }
}
