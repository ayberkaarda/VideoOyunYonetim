using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// The 48 px dark strip at the top of a <see cref="ChromelessForm"/>: window title
    /// on the left, caption buttons on the right, and the whole remaining area acting
    /// as a drag handle.
    /// </summary>
    /// <remarks>
    /// Dragging is handed to Windows through WM_NCLBUTTONDOWN / HTCAPTION rather than
    /// tracked by hand in MouseMove. That single message is what keeps Aero Snap,
    /// shake, and multi-monitor drags behaving like a normal window; a hand-rolled
    /// "move the form by the mouse delta" loop breaks all three.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultProperty("Title")]
    [Description("The dark title strip with caption buttons and a drag handle.")]
    public class TitleBar : Control
    {
        private readonly CaptionButton _closeButton;
        private readonly CaptionButton _minimizeButton;

        private string _title = string.Empty;
        private string _subtitle = string.Empty;
        private bool _showMinimize = true;

        /// <summary>Initialises a new title strip with Close and Minimize buttons.</summary>
        public TitleBar()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            BackColor = Theme.TitleBar;
            ForeColor = Theme.TitleBarText;
            Font = Theme.Fonts.BodyStrong;
            Height = Theme.Metrics.TitleBar;
            TabStop = false;

            _closeButton = new CaptionButton();
            _closeButton.Kind = CaptionButtonKind.Close;
            _closeButton.Name = "closeButton";
            _closeButton.Click += CloseButtonClick;

            _minimizeButton = new CaptionButton();
            _minimizeButton.Kind = CaptionButtonKind.Minimize;
            _minimizeButton.Name = "minimizeButton";
            _minimizeButton.Click += MinimizeButtonClick;

            Controls.Add(_closeButton);
            Controls.Add(_minimizeButton);
        }

        /// <summary>Gets the Close button, so a form can wire it to <see cref="Form.CancelButton"/>.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CaptionButton CloseButton
        {
            get { return _closeButton; }
        }

        /// <summary>Gets the Minimize button.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public CaptionButton MinimizeButton
        {
            get { return _minimizeButton; }
        }

        /// <summary>Gets or sets the window title shown on the left of the strip.</summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("Window title shown on the left of the strip.")]
        public string Title
        {
            get { return _title; }
            set
            {
                string text = value ?? string.Empty;
                if (!string.Equals(_title, text, StringComparison.Ordinal))
                {
                    _title = text;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets an optional second line of context under the title.</summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("Optional context line shown under the title.")]
        public string Subtitle
        {
            get { return _subtitle; }
            set
            {
                string text = value ?? string.Empty;
                if (!string.Equals(_subtitle, text, StringComparison.Ordinal))
                {
                    _subtitle = text;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets whether the Minimize button is shown.</summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Shows or hides the Minimize button.")]
        public bool ShowMinimize
        {
            get { return _showMinimize; }
            set
            {
                if (_showMinimize != value)
                {
                    _showMinimize = value;
                    _minimizeButton.Visible = value;
                    PerformLayout();
                    Invalidate();
                }
            }
        }

        /// <inheritdoc/>
        protected override Size DefaultSize
        {
            get { return new Size(600, Theme.Metrics.TitleBar); }
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
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            // Setting Font in the constructor triggers a layout pass before the caption
            // buttons exist, so the fields have to be checked rather than assumed.
            if (_closeButton == null || _minimizeButton == null)
            {
                return;
            }

            int right = Width;
            _closeButton.SetBounds(right - Theme.Metrics.CaptionButton, 0, Theme.Metrics.CaptionButton, Height);
            right -= Theme.Metrics.CaptionButton;

            _minimizeButton.Visible = _showMinimize;
            if (_showMinimize)
            {
                _minimizeButton.SetBounds(right - Theme.Metrics.CaptionButton, 0, Theme.Metrics.CaptionButton, Height);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e == null || e.Button != MouseButtons.Left || DesignMode)
            {
                return;
            }

            Form? form = FindForm();
            if (form == null)
            {
                return;
            }

            NativeMethods.ReleaseCapture();
            NativeMethods.SendMessage(
                form.Handle,
                NativeMethods.WM_NCLBUTTONDOWN,
                new IntPtr(NativeMethods.HTCAPTION),
                IntPtr.Zero);
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            if (e == null)
            {
                return;
            }

            Graphics g = e.Graphics;
            using (SolidBrush brush = new SolidBrush(Theme.TitleBar))
            {
                g.FillRectangle(brush, new Rectangle(0, 0, Width, Height));
            }

            int left = Theme.Space.L;
            int reserved = Theme.Metrics.CaptionButton * (_showMinimize ? 2 : 1);
            int available = Width - left - reserved - Theme.Space.M;
            if (available <= 0)
            {
                return;
            }

            bool hasSubtitle = _subtitle.Length > 0;
            int titleHeight = Font.Height;
            int subtitleHeight = hasSubtitle ? Theme.Fonts.Caption.Height : 0;
            int blockHeight = titleHeight + (hasSubtitle ? subtitleHeight : 0);
            int top = (Height - blockHeight) / 2;

            TextRenderer.DrawText(
                g,
                _title,
                Font,
                new Rectangle(left, top, available, titleHeight),
                Theme.TitleBarText,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            if (hasSubtitle)
            {
                TextRenderer.DrawText(
                    g,
                    _subtitle,
                    Theme.Fonts.Caption,
                    new Rectangle(left, top + titleHeight, available, subtitleHeight),
                    Theme.TitleBarTextSecondary,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        private void CloseButtonClick(object? sender, EventArgs e)
        {
            Form? form = FindForm();
            if (form != null)
            {
                form.Close();
            }
        }

        private void MinimizeButtonClick(object? sender, EventArgs e)
        {
            Form? form = FindForm();
            if (form != null)
            {
                form.WindowState = FormWindowState.Minimized;
            }
        }
    }
}
