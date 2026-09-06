using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// Which window command a <see cref="CaptionButton"/> stands for.
    /// </summary>
    public enum CaptionButtonKind
    {
        /// <summary>Minimise the window to the taskbar.</summary>
        Minimize,

        /// <summary>Maximise or restore the window.</summary>
        Maximize,

        /// <summary>Close the window.</summary>
        Close
    }

    /// <summary>
    /// One button in the dark title strip, drawn to the Windows 11 caption metrics:
    /// a 46 x 48 hit target, a neutral hover wash, and the red hover reserved for Close.
    /// </summary>
    /// <remarks>
    /// Derives from <see cref="Button"/> so it satisfies <see cref="IButtonControl"/>.
    /// <see cref="ChromelessForm"/> assigns the Close instance to
    /// <see cref="Form.CancelButton"/>, which is what makes Escape close the window:
    /// <see cref="Button.PerformClick"/> requires a visible, enabled control, so an
    /// invisible dummy button would silently do nothing.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultEvent("Click")]
    [Description("A minimise, maximise or close button for the title strip.")]
    public class CaptionButton : Button
    {
        private const string MinimizeGlyph = "\uE921";
        private const string MaximizeGlyph = "\uE922";
        private const string RestoreGlyph = "\uE923";
        private const string CloseGlyph = "\uE8BB";

        private CaptionButtonKind _kind = CaptionButtonKind.Close;
        private bool _hovered;
        private bool _pressed;

        /// <summary>Initialises a new caption button.</summary>
        public CaptionButton()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            BackColor = Theme.TitleBar;
            ForeColor = Theme.TitleBarText;
            Font = Theme.Fonts.Glyph10;
            TabStop = false;
            UseVisualStyleBackColor = false;
            Size = new Size(Theme.Metrics.CaptionButton, Theme.Metrics.TitleBar);
            UpdateAccessibleName();
        }

        /// <summary>Gets or sets which window command this button stands for.</summary>
        [Category("Behavior")]
        [DefaultValue(CaptionButtonKind.Close)]
        [Description("Which window command this button stands for.")]
        public CaptionButtonKind Kind
        {
            get { return _kind; }
            set
            {
                if (_kind != value)
                {
                    _kind = value;
                    UpdateAccessibleName();
                    Invalidate();
                }
            }
        }

        /// <inheritdoc/>
        protected override Size DefaultSize
        {
            get { return new Size(Theme.Metrics.CaptionButton, Theme.Metrics.TitleBar); }
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
        protected override void OnPaint(PaintEventArgs pevent)
        {
            if (pevent == null)
            {
                return;
            }

            Graphics g = pevent.Graphics;
            Rectangle bounds = new Rectangle(0, 0, Width, Height);

            Color fill;
            Color foreground = Theme.TitleBarText;

            if (_kind == CaptionButtonKind.Close)
            {
                fill = _pressed ? Theme.ClosePressed : (_hovered ? Theme.CloseHover : Theme.TitleBar);
            }
            else
            {
                fill = _pressed ? Theme.TitleBarPressed : (_hovered ? Theme.TitleBarHover : Theme.TitleBar);
            }

            using (SolidBrush brush = new SolidBrush(fill))
            {
                g.FillRectangle(brush, bounds);
            }

            TextRenderer.DrawText(
                g,
                GlyphFor(_kind),
                Font,
                bounds,
                foreground,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
        }

        private string GlyphFor(CaptionButtonKind kind)
        {
            switch (kind)
            {
                case CaptionButtonKind.Minimize:
                    return MinimizeGlyph;

                case CaptionButtonKind.Maximize:
                    Form form = FindForm();
                    return form != null && form.WindowState == FormWindowState.Maximized
                        ? RestoreGlyph
                        : MaximizeGlyph;

                default:
                    return CloseGlyph;
            }
        }

        private void UpdateAccessibleName()
        {
            switch (_kind)
            {
                case CaptionButtonKind.Minimize:
                    AccessibleName = "Minimize";
                    break;

                case CaptionButtonKind.Maximize:
                    AccessibleName = "Maximize";
                    break;

                default:
                    AccessibleName = "Close";
                    break;
            }

            AccessibleRole = AccessibleRole.PushButton;
        }
    }
}
