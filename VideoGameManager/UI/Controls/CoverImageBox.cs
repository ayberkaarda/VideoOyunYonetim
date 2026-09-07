using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// Shows cover artwork inside a rounded, sunken frame. The image is fitted with
    /// Zoom semantics, so a portrait cover in a landscape box keeps its proportions
    /// instead of being stretched.
    /// </summary>
    /// <remarks>
    /// The box has three states and draws all of them: empty (a glyph and a caption),
    /// loading (a rotating arc), and loaded. A stock <see cref="PictureBox"/> has no
    /// loading state at all, which is why the old screens showed a blank grey rectangle
    /// while artwork downloaded and gave the user nothing to read.
    /// </remarks>
    [ToolboxItem(true)]
    [DefaultProperty("Image")]
    [Description("Cover artwork in a rounded frame, with empty and loading states.")]
    public class CoverImageBox : Control
    {
        private const string EmptyGlyph = "\uE91B";
        private const int SpinnerIntervalMilliseconds = 40;
        private const int SpinnerDiameter = 32;
        private const int SpinnerThickness = 3;
        private const int SpinnerStepDegrees = 24;

        private readonly System.Windows.Forms.Timer _spinnerTimer;

        private Image? _image;
        private bool _isLoading;
        private int _spinnerAngle;
        private int _cornerRadius = Theme.Radius.Card;
        private string _placeholderText = "No cover";
        private ICoverImageProvider? _provider;
        private CancellationTokenSource? _pendingLoad;

        /// <summary>Initialises a new, empty cover box.</summary>
        public CoverImageBox()
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

            _spinnerTimer = new System.Windows.Forms.Timer();
            _spinnerTimer.Interval = SpinnerIntervalMilliseconds;
            _spinnerTimer.Tick += SpinnerTick;
        }

        /// <summary>
        /// Gets or sets the artwork. Setting a new image does not dispose the previous
        /// one; the caller owns whatever it hands over.
        /// </summary>
        [Category("Appearance")]
        [DefaultValue(null)]
        [Description("The cover artwork to display.")]
        public Image? Image
        {
            get { return _image; }
            set
            {
                if (!ReferenceEquals(_image, value))
                {
                    _image = value;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets whether the loading indicator is shown.</summary>
        [Category("Appearance")]
        [DefaultValue(false)]
        [Description("Shows the loading indicator.")]
        public bool IsLoading
        {
            get { return _isLoading; }
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    _spinnerTimer.Enabled = value && !DesignMode;
                    Invalidate();
                }
            }
        }

        /// <summary>Gets or sets the caption shown when there is no artwork.</summary>
        [Category("Appearance")]
        [DefaultValue("No cover")]
        [Description("Caption shown when there is no artwork.")]
        public string PlaceholderText
        {
            get { return _placeholderText; }
            set
            {
                string text = value ?? string.Empty;
                if (!string.Equals(_placeholderText, text, StringComparison.Ordinal))
                {
                    _placeholderText = text;
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

        /// <summary>
        /// Gets or sets the artwork source used by <see cref="LoadCoverAsync"/>. Left
        /// null, the box is a passive display and the caller assigns <see cref="Image"/>.
        /// </summary>
        [Browsable(false)]
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public ICoverImageProvider? Provider
        {
            get { return _provider; }
            set { _provider = value; }
        }

        /// <inheritdoc/>
        protected override Size DefaultSize
        {
            get { return new Size(180, 240); }
        }

        /// <summary>
        /// Asks <see cref="Provider"/> for artwork and shows it, running the loading state
        /// for the duration. A second call cancels the first, so quickly clicking through a
        /// list cannot leave the wrong cover on screen.
        /// </summary>
        /// <param name="coverReference">The reference to hand to the provider.</param>
        /// <returns>A task that completes when the box has settled on a state.</returns>
        public async Task LoadCoverAsync(string? coverReference)
        {
            if (_provider == null)
            {
                return;
            }

            CancellationTokenSource? previous = _pendingLoad;
            CancellationTokenSource current = new CancellationTokenSource();
            _pendingLoad = current;

            if (previous != null)
            {
                // Cancel but do not dispose: the superseded call still holds the token and
                // disposes its own source in its finally block.
                previous.Cancel();
            }

            Image = null;
            IsLoading = true;

            try
            {
                Image? loaded = await _provider.GetCoverAsync(coverReference, current.Token)
                    .ConfigureAwait(true);

                if (!current.IsCancellationRequested && ReferenceEquals(_pendingLoad, current))
                {
                    Image = loaded;
                }
            }
            catch (OperationCanceledException)
            {
                // A superseded request. The newer call owns the box now, so there is
                // nothing to report and nothing to draw.
            }
            finally
            {
                if (ReferenceEquals(_pendingLoad, current))
                {
                    _pendingLoad = null;
                    IsLoading = false;
                }

                current.Dispose();
            }
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

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _spinnerTimer.Tick -= SpinnerTick;
                _spinnerTimer.Dispose();

                if (_pendingLoad != null)
                {
                    // The in-flight call disposes the source itself once it unwinds.
                    _pendingLoad.Cancel();
                    _pendingLoad = null;
                }
            }

            base.Dispose(disposing);
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
            g.FillRoundedRect(Theme.SurfaceSunken, bounds, _cornerRadius);

            if (_image != null)
            {
                Rectangle destination = _image.Size.FitInside(bounds);
                if (destination.Width > 0 && destination.Height > 0)
                {
                    using (GraphicsPath clip = bounds.RoundedRect(_cornerRadius))
                    {
                        Region previousClip = g.Clip;
                        g.SetClip(clip, CombineMode.Intersect);
                        InterpolationMode previousInterpolation = g.InterpolationMode;
                        g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                        g.PixelOffsetMode = PixelOffsetMode.HighQuality;
                        g.DrawImage(_image, destination);
                        g.InterpolationMode = previousInterpolation;
                        g.Clip = previousClip;
                    }
                }
            }
            else if (!_isLoading)
            {
                DrawPlaceholder(g, bounds);
            }

            if (_isLoading)
            {
                DrawSpinner(g, bounds);
            }

            g.DrawRoundedRect(Theme.Border, bounds, _cornerRadius, Theme.Metrics.BorderThickness);
        }

        private void DrawPlaceholder(Graphics g, Rectangle bounds)
        {
            int glyphHeight = Theme.Fonts.Glyph20.Height;
            int captionHeight = Font.Height;
            int blockHeight = glyphHeight + Theme.Space.M + captionHeight;
            int top = bounds.Y + ((bounds.Height - blockHeight) / 2);

            TextRenderer.DrawText(
                g,
                EmptyGlyph,
                Theme.Fonts.Glyph20,
                new Rectangle(bounds.X, top, bounds.Width, glyphHeight),
                Theme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

            TextRenderer.DrawText(
                g,
                _placeholderText,
                Font,
                new Rectangle(bounds.X, top + glyphHeight + Theme.Space.M, bounds.Width, captionHeight),
                Theme.TextSecondary,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                TextFormatFlags.SingleLine | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }

        private void DrawSpinner(Graphics g, Rectangle bounds)
        {
            int diameter = Math.Min(SpinnerDiameter, Math.Min(bounds.Width, bounds.Height) / 2);
            if (diameter < SpinnerThickness * 3)
            {
                return;
            }

            Rectangle arc = new Rectangle(
                bounds.X + ((bounds.Width - diameter) / 2),
                bounds.Y + ((bounds.Height - diameter) / 2),
                diameter,
                diameter);

            SmoothingMode previous = g.SmoothingMode;
            g.SmoothingMode = SmoothingMode.AntiAlias;

            using (Pen track = new Pen(Theme.Border, SpinnerThickness))
            {
                g.DrawEllipse(track, arc);
            }

            using (Pen head = new Pen(Theme.Accent, SpinnerThickness))
            {
                head.StartCap = LineCap.Round;
                head.EndCap = LineCap.Round;
                g.DrawArc(head, arc, _spinnerAngle, 90f);
            }

            g.SmoothingMode = previous;
        }

        private void SpinnerTick(object? sender, EventArgs e)
        {
            _spinnerAngle = (_spinnerAngle + SpinnerStepDegrees) % 360;
            Invalidate();
        }
    }
}
