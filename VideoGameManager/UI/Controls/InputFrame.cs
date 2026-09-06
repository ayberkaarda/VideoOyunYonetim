using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// Wraps a single editable control (usually a <see cref="TextBox"/> or a
    /// <see cref="ComboBox"/>) and draws the rounded frame and focus ring around it.
    /// </summary>
    /// <remarks>
    /// Stock WinForms fields can only be drawn with the classic sunken 3-D border or
    /// with no border at all. This frame supplies the third option: a 1 px rounded
    /// border that thickens into an accent focus ring when the wrapped control takes
    /// focus, so keyboard users can always see where they are.
    /// </remarks>
    [ToolboxItem(true)]
    [Description("Draws a rounded border and focus ring around a wrapped TextBox or ComboBox.")]
    public class InputFrame : Panel
    {
        private int _cornerRadius = Theme.Radius.Control;
        private bool _focused;
        private bool _hovered;
        private bool _invalid;

        /// <summary>Initialises a new input frame.</summary>
        public InputFrame()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Color.Transparent;
            Size = new Size(220, Theme.Metrics.Input);
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

        /// <summary>
        /// Gets or sets whether the frame is drawn in the error colour. Purely visual:
        /// the frame never decides what "invalid" means, the caller does.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Draws the frame in the error colour.")]
        public bool Invalid
        {
            get { return _invalid; }
            set
            {
                if (_invalid != value)
                {
                    _invalid = value;
                    Invalidate();
                }
            }
        }

        /// <inheritdoc/>
        protected override Size DefaultSize
        {
            get { return new Size(220, Theme.Metrics.Input); }
        }

        /// <inheritdoc/>
        protected override Padding DefaultPadding
        {
            get { return new Padding(Theme.Space.M, Theme.Space.S, Theme.Space.M, Theme.Space.S); }
        }

        /// <inheritdoc/>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e == null || e.Control == null)
            {
                return;
            }

            Control child = e.Control;
            child.BackColor = Enabled ? Theme.SurfaceRaised : Theme.SurfaceDisabled;
            child.ForeColor = Enabled ? Theme.TextPrimary : Theme.TextDisabled;

            TextBoxBase textBox = child as TextBoxBase;
            if (textBox != null)
            {
                textBox.BorderStyle = BorderStyle.None;
            }

            ComboBox comboBox = child as ComboBox;
            if (comboBox != null)
            {
                comboBox.FlatStyle = FlatStyle.Flat;
            }

            // Panel raises Enter/Leave for itself, but a ComboBox drop-down steals focus
            // to a native list window, so the child's own events are hooked as well.
            child.GotFocus += ChildFocusChanged;
            child.LostFocus += ChildFocusChanged;
            child.MouseEnter += ChildHoverChanged;
            child.MouseLeave += ChildHoverChanged;

            PerformLayout();
        }

        /// <inheritdoc/>
        protected override void OnControlRemoved(ControlEventArgs e)
        {
            if (e != null && e.Control != null)
            {
                e.Control.GotFocus -= ChildFocusChanged;
                e.Control.LostFocus -= ChildFocusChanged;
                e.Control.MouseEnter -= ChildHoverChanged;
                e.Control.MouseLeave -= ChildHoverChanged;
            }

            base.OnControlRemoved(e);
        }

        /// <inheritdoc/>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            Rectangle inner = DisplayRectangle;
            for (int i = 0; i < Controls.Count; i++)
            {
                Control child = Controls[i];
                if (child.Dock != DockStyle.None)
                {
                    continue;
                }

                // A TextBox or ComboBox height is dictated by its font, so it is
                // centred inside the frame rather than stretched to fill it.
                int height = Math.Min(child.Height, inner.Height);
                child.SetBounds(
                    inner.X,
                    inner.Y + ((inner.Height - height) / 2),
                    inner.Width,
                    height);
            }
        }

        /// <inheritdoc/>
        protected override void OnEnter(EventArgs e)
        {
            _focused = true;
            Invalidate();
            base.OnEnter(e);
        }

        /// <inheritdoc/>
        protected override void OnLeave(EventArgs e)
        {
            _focused = false;
            Invalidate();
            base.OnLeave(e);
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
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e)
        {
            Color fill = Enabled ? Theme.SurfaceRaised : Theme.SurfaceDisabled;
            Color ink = Enabled ? Theme.TextPrimary : Theme.TextDisabled;

            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].BackColor = fill;
                Controls[i].ForeColor = ink;
            }

            Invalidate();
            base.OnEnabledChanged(e);
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
            Color fill = Enabled ? Theme.SurfaceRaised : Theme.SurfaceDisabled;
            g.FillRoundedRect(fill, bounds, _cornerRadius);

            Color border;
            int thickness;

            if (!Enabled)
            {
                border = Theme.BorderDisabled;
                thickness = Theme.Metrics.BorderThickness;
            }
            else if (_invalid)
            {
                border = Theme.Danger;
                thickness = _focused ? Theme.Metrics.FocusRing : Theme.Metrics.BorderThickness;
            }
            else if (_focused)
            {
                border = Theme.Accent;
                thickness = Theme.Metrics.FocusRing;
            }
            else
            {
                border = _hovered ? Theme.TextSecondary : Theme.InputBorder;
                thickness = Theme.Metrics.BorderThickness;
            }

            g.DrawRoundedRect(border, bounds, _cornerRadius, thickness);

            base.OnPaint(e);
        }

        private void ChildFocusChanged(object sender, EventArgs e)
        {
            bool focused = ContainsFocus;
            if (_focused != focused)
            {
                _focused = focused;
                Invalidate();
            }
        }

        private void ChildHoverChanged(object sender, EventArgs e)
        {
            Invalidate();
        }
    }
}
