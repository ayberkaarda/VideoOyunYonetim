using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// A rounded search field: magnifier glyph, placeholder, clear button, and a
    /// debounced change event.
    /// </summary>
    /// <remarks>
    /// <see cref="SearchTextChanged"/> fires once the user has stopped typing for
    /// <see cref="DebounceInterval"/> milliseconds, never on every keystroke. A filter
    /// wired straight to <see cref="Control.TextChanged"/> issues one query per
    /// character, which on a remote database means eight round trips for the word
    /// "Hollow" before the user has finished the word.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultProperty("Text")]
    [DefaultEvent("SearchTextChanged")]
    [Description("A rounded search field with a debounced change event.")]
    public class SearchBox : Control
    {
        private const string SearchGlyph = "\uE721";
        private const string ClearGlyph = "\uE711";

        /// <summary>The default debounce window in milliseconds.</summary>
        public const int DefaultDebounceInterval = 300;

        private readonly TextBox _input;
        private readonly Timer _debounceTimer;

        private int _cornerRadius = Theme.Radius.Control;
        private bool _focused;
        private bool _hovered;
        private bool _clearHovered;

        /// <summary>Initialises a new search box.</summary>
        public SearchBox()
        {
            SetStyle(
                ControlStyles.UserPaint |
                ControlStyles.AllPaintingInWmPaint |
                ControlStyles.OptimizedDoubleBuffer |
                ControlStyles.ResizeRedraw |
                ControlStyles.SupportsTransparentBackColor,
                true);

            BackColor = Color.Transparent;
            Size = new Size(280, Theme.Metrics.Input);

            _input = new TextBox();
            _input.Name = "input";
            _input.BorderStyle = BorderStyle.None;
            _input.BackColor = Theme.SurfaceRaised;
            _input.ForeColor = Theme.TextPrimary;
            _input.Font = Theme.Fonts.Body;
            _input.PlaceholderText = "Search";
            _input.AccessibleName = "Search";
            _input.TextChanged += InputTextChanged;
            _input.GotFocus += InputFocusChanged;
            _input.LostFocus += InputFocusChanged;
            Controls.Add(_input);

            _debounceTimer = new Timer();
            _debounceTimer.Interval = DefaultDebounceInterval;
            _debounceTimer.Tick += DebounceElapsed;
        }

        /// <summary>
        /// Raised once the user has paused typing for <see cref="DebounceInterval"/>
        /// milliseconds, and when the field is cleared.
        /// </summary>
        [Category("Action")]
        [Description("Raised once the user has paused typing.")]
        public event EventHandler SearchTextChanged;

        /// <summary>Gets or sets the current search text.</summary>
        [Browsable(true)]
        [EditorBrowsable(EditorBrowsableState.Always)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Visible)]
        [Description("The current search text.")]
        public override string Text
        {
            get { return _input.Text; }
            set { _input.Text = value ?? string.Empty; }
        }

        /// <summary>Gets or sets the greyed-out prompt shown while the field is empty.</summary>
        [Category("Appearance")]
        [DefaultValue("Search")]
        [Description("Prompt shown while the field is empty.")]
        public string PlaceholderText
        {
            get { return _input.PlaceholderText; }
            set { _input.PlaceholderText = value ?? string.Empty; }
        }

        /// <summary>Gets or sets how long the field waits after the last keystroke.</summary>
        [Category("Behavior")]
        [DefaultValue(DefaultDebounceInterval)]
        [Description("Milliseconds to wait after the last keystroke before raising SearchTextChanged.")]
        public int DebounceInterval
        {
            get { return _debounceTimer.Interval; }
            set { _debounceTimer.Interval = value < 1 ? 1 : value; }
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
            get { return new Size(280, Theme.Metrics.Input); }
        }

        /// <summary>Clears the field and raises <see cref="SearchTextChanged"/> immediately.</summary>
        public void Clear()
        {
            _debounceTimer.Stop();
            _input.Text = string.Empty;
            OnSearchTextChanged(EventArgs.Empty);
        }

        /// <summary>Moves keyboard focus into the text field.</summary>
        /// <returns><see langword="true"/> if the field took focus.</returns>
        public new bool Focus()
        {
            return _input.Focus();
        }

        /// <summary>Raises <see cref="SearchTextChanged"/>.</summary>
        /// <param name="e">Always <see cref="EventArgs.Empty"/>.</param>
        protected virtual void OnSearchTextChanged(EventArgs e)
        {
            EventHandler handler = SearchTextChanged;
            if (handler != null)
            {
                handler(this, e);
            }
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _debounceTimer.Tick -= DebounceElapsed;
                _debounceTimer.Dispose();
                _input.TextChanged -= InputTextChanged;
                _input.GotFocus -= InputFocusChanged;
                _input.LostFocus -= InputFocusChanged;
            }

            base.Dispose(disposing);
        }

        /// <inheritdoc/>
        protected override void OnLayout(LayoutEventArgs levent)
        {
            base.OnLayout(levent);

            // Sizing the control in the constructor lays it out before the field exists.
            if (_input == null)
            {
                return;
            }

            int glyphColumn = Theme.Metrics.Input;
            int left = glyphColumn;
            int right = Width - (HasText() ? glyphColumn : Theme.Space.M);
            int width = Math.Max(0, right - left);
            int height = Math.Min(_input.PreferredHeight, Math.Max(0, Height - (Theme.Space.S * 2)));

            _input.SetBounds(left, (Height - height) / 2, width, height);
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
            _clearHovered = false;
            Invalidate();
            base.OnMouseLeave(e);
        }

        /// <inheritdoc/>
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e == null)
            {
                return;
            }

            bool overClear = HasText() && ClearButtonBounds().Contains(e.Location);
            if (overClear != _clearHovered)
            {
                _clearHovered = overClear;
                Cursor = overClear ? Cursors.Hand : Cursors.IBeam;
                Invalidate();
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);

            if (e == null || e.Button != MouseButtons.Left)
            {
                return;
            }

            if (HasText() && ClearButtonBounds().Contains(e.Location))
            {
                Clear();
                _input.Focus();
                return;
            }

            _input.Focus();
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

            TextRenderer.DrawText(
                g,
                SearchGlyph,
                Theme.Fonts.Glyph10,
                new Rectangle(0, 0, Theme.Metrics.Input, Height),
                Enabled ? Theme.TextSecondary : Theme.TextDisabled,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            if (HasText())
            {
                Rectangle clear = ClearButtonBounds();
                if (_clearHovered)
                {
                    g.FillRoundedRect(Theme.SurfaceHover, Rectangle.Inflate(clear, -Theme.Space.S, -Theme.Space.S), Theme.Radius.Badge);
                }

                TextRenderer.DrawText(
                    g,
                    ClearGlyph,
                    Theme.Fonts.Glyph10,
                    clear,
                    _clearHovered ? Theme.TextPrimary : Theme.TextSecondary,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        /// <inheritdoc/>
        protected override void OnEnabledChanged(EventArgs e)
        {
            if (_input == null)
            {
                base.OnEnabledChanged(e);
                return;
            }

            _input.Enabled = Enabled;
            _input.BackColor = Enabled ? Theme.SurfaceRaised : Theme.SurfaceDisabled;
            _input.ForeColor = Enabled ? Theme.TextPrimary : Theme.TextDisabled;
            Invalidate();
            base.OnEnabledChanged(e);
        }

        private bool HasText()
        {
            return _input != null && _input.Text.Length > 0;
        }

        private Rectangle ClearButtonBounds()
        {
            return new Rectangle(Width - Theme.Metrics.Input, 0, Theme.Metrics.Input, Height);
        }

        private void InputTextChanged(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            _debounceTimer.Start();
            PerformLayout();
            Invalidate();
        }

        private void InputFocusChanged(object sender, EventArgs e)
        {
            _focused = _input.Focused;
            Invalidate();
        }

        private void DebounceElapsed(object sender, EventArgs e)
        {
            _debounceTimer.Stop();
            OnSearchTextChanged(EventArgs.Empty);
        }
    }
}
