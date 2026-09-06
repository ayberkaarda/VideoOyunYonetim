using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// Base class for every window in the application: no system chrome, a dark
    /// <see cref="Controls.TitleBar"/> across the top, and the window management that a
    /// borderless form normally throws away put back by hand.
    /// </summary>
    /// <remarks>
    /// <para>
    /// A plain <c>FormBorderStyle.None</c> plus <c>ControlBox = false</c> form loses more
    /// than the caption. Measured on the forms this library replaces:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Alt+F4 did nothing, because without WS_SYSMENU the window has no
    /// system menu to send WM_CLOSE from. Restored here by adding WS_SYSMENU in
    /// <see cref="CreateParams"/>.</description></item>
    /// <item><description>Escape closed nothing, because no control was registered as
    /// <see cref="Form.CancelButton"/>. Restored here by wiring the title bar's Close
    /// button, which is a real visible <see cref="IButtonControl"/>.</description></item>
    /// <item><description>The taskbar button could not minimise or restore the window,
    /// because WS_MINIMIZEBOX was absent. Restored in <see cref="CreateParams"/>.</description></item>
    /// <item><description>One form's Minimize button had an empty handler body, so it was a
    /// dead pixel. <see cref="Controls.TitleBar"/> owns that behaviour now, so it cannot be
    /// forgotten per form.</description></item>
    /// <item><description>There was no drop shadow, so the window had no edge against a light
    /// desktop. CS_DROPSHADOW adds one.</description></item>
    /// </list>
    /// <para>
    /// <see cref="DefaultPadding"/> reserves the title strip height plus one XL step, so
    /// docked content starts below the strip instead of underneath it. Content positioned
    /// with absolute coordinates must still respect <see cref="ContentBounds"/>.
    /// </para>
    /// </remarks>
    [ToolboxItem(false)]
    [Description("Borderless base form with a dark title strip and working window commands.")]
    public class ChromelessForm : Form
    {
        private readonly TitleBar _titleBar;
        private bool _escapeCloses = true;

        /// <summary>Initialises a new chrome-less window.</summary>
        public ChromelessForm()
        {
            SetStyle(
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw,
                true);

            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = Theme.Surface;
            ForeColor = Theme.TextPrimary;
            Font = Theme.Fonts.Body;
            KeyPreview = true;
            ShowInTaskbar = true;
            AutoScaleMode = AutoScaleMode.None;

            _titleBar = new TitleBar();
            _titleBar.Name = "titleBar";
            _titleBar.TabStop = false;
            _titleBar.SetBounds(0, 0, ClientSize.Width, Theme.Metrics.TitleBar);
            Controls.Add(_titleBar);

            // Escape must reach a real, visible, enabled IButtonControl: Button.PerformClick
            // silently returns for a control that cannot be selected, which is why the usual
            // "hidden cancel button" trick does not work.
            CancelButton = _titleBar.CloseButton;
        }

        /// <summary>Gets the title strip, for setting the subtitle or hiding Minimize.</summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public TitleBar TitleStrip
        {
            get { return _titleBar; }
        }

        /// <summary>Gets or sets whether Escape closes the window.</summary>
        [Category("Behavior")]
        [DefaultValue(true)]
        [Description("Whether Escape closes the window.")]
        public bool EscapeCloses
        {
            get { return _escapeCloses; }
            set
            {
                if (_escapeCloses != value)
                {
                    _escapeCloses = value;
                    CancelButton = value ? _titleBar.CloseButton : null;
                }
            }
        }

        /// <summary>Gets or sets whether the Minimize caption button is shown.</summary>
        [Category("Appearance")]
        [DefaultValue(true)]
        [Description("Whether the Minimize caption button is shown.")]
        public bool ShowMinimizeButton
        {
            get { return _titleBar.ShowMinimize; }
            set { _titleBar.ShowMinimize = value; }
        }

        /// <summary>Gets or sets the context line shown under the window title.</summary>
        [Category("Appearance")]
        [DefaultValue("")]
        [Description("Context line shown under the window title.")]
        public string Subtitle
        {
            get { return _titleBar.Subtitle; }
            set { _titleBar.Subtitle = value; }
        }

        /// <summary>
        /// Gets the area available to content: the client rectangle minus the title strip
        /// and the form padding.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public Rectangle ContentBounds
        {
            get
            {
                Padding padding = Padding;
                return new Rectangle(
                    padding.Left,
                    padding.Top,
                    Math.Max(0, ClientSize.Width - padding.Horizontal),
                    Math.Max(0, ClientSize.Height - padding.Vertical));
            }
        }

        /// <inheritdoc/>
        protected override Padding DefaultPadding
        {
            get
            {
                return new Padding(
                    Theme.Space.XL,
                    Theme.Metrics.TitleBar + Theme.Space.XL,
                    Theme.Space.XL,
                    Theme.Space.XL);
            }
        }

        /// <inheritdoc/>
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams parameters = base.CreateParams;

                // WS_SYSMENU brings back Alt+F4 and Alt+Space; WS_MINIMIZEBOX makes the
                // taskbar button minimise and restore. Neither draws anything, because
                // WS_CAPTION is still absent.
                parameters.Style |= NativeMethods.WS_SYSMENU | NativeMethods.WS_MINIMIZEBOX;

                // A drop shadow gives the borderless window an edge on a light desktop.
                // WS_EX_COMPOSITED is deliberately not used: it flickers SplitContainer
                // and breaks child-control painting order.
                parameters.ClassStyle |= NativeMethods.CS_DROPSHADOW;

                return parameters;
            }
        }

        /// <summary>Applies the theme title to both the window and the strip.</summary>
        /// <inheritdoc/>
        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);

            // Form base-class work in the constructor can raise this before the strip exists.
            if (_titleBar != null)
            {
                _titleBar.Title = Text;
            }
        }

        /// <inheritdoc/>
        protected override void OnClientSizeChanged(EventArgs e)
        {
            base.OnClientSizeChanged(e);

            if (_titleBar != null)
            {
                _titleBar.SetBounds(0, 0, ClientSize.Width, Theme.Metrics.TitleBar);
            }
        }

        /// <inheritdoc/>
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);
            _titleBar.SetBounds(0, 0, ClientSize.Width, Theme.Metrics.TitleBar);
            _titleBar.Title = Text;

            if (string.IsNullOrEmpty(AccessibleName))
            {
                AccessibleName = Text;
            }
        }

        /// <inheritdoc/>
        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            if (e == null)
            {
                return;
            }

            // A borderless window has no frame of its own, so the 1 px outline is the only
            // thing separating it from whatever is behind it.
            using (Pen pen = new Pen(Theme.Border))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }
    }
}
