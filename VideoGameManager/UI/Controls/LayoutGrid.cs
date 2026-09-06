using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// A <see cref="TableLayoutPanel"/> that is actually double buffered.
    /// </summary>
    /// <remarks>
    /// The stock panel leaves <see cref="Control.DoubleBuffered"/> off, so resizing a
    /// form built on one repaints every cell straight to the screen and the layout
    /// visibly tears. The property is protected on <see cref="Control"/>, so the only
    /// way to turn it on is a subclass; this is that subclass, plus theme-consistent
    /// defaults for background and cell padding.
    /// </remarks>
    [ToolboxItem(true)]
    [Description("A double-buffered TableLayoutPanel with theme defaults.")]
    public class LayoutGrid : TableLayoutPanel
    {
        /// <summary>Initialises a new layout grid.</summary>
        public LayoutGrid()
        {
            DoubleBuffered = true;
            ResizeRedraw = true;
            BackColor = Color.Transparent;
            Margin = Padding.Empty;
        }

        /// <inheritdoc/>
        protected override Padding DefaultPadding
        {
            get { return Padding.Empty; }
        }

        /// <inheritdoc/>
        protected override Padding DefaultMargin
        {
            get { return Padding.Empty; }
        }

        /// <summary>
        /// Gets or sets the gap placed around each child, expressed in theme spacing
        /// steps rather than raw pixels.
        /// </summary>
        [Category("Layout")]
        [DefaultValue(Theme.Space.M)]
        [Description("Gap around each child control, in pixels, taken from the theme spacing scale.")]
        public int CellSpacing
        {
            get { return _cellSpacing; }
            set
            {
                int clamped = value < 0 ? 0 : value;
                if (_cellSpacing != clamped)
                {
                    _cellSpacing = clamped;
                    ApplyCellSpacing();
                }
            }
        }

        private int _cellSpacing = Theme.Space.M;

        /// <inheritdoc/>
        protected override void OnControlAdded(ControlEventArgs e)
        {
            base.OnControlAdded(e);

            if (e != null && e.Control != null)
            {
                e.Control.Margin = new Padding(_cellSpacing / 2);
            }
        }

        private void ApplyCellSpacing()
        {
            SuspendLayout();
            for (int i = 0; i < Controls.Count; i++)
            {
                Controls[i].Margin = new Padding(_cellSpacing / 2);
            }

            ResumeLayout(true);
        }
    }
}
