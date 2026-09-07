using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// What a <see cref="GameListBox"/> row needs in order to draw itself.
    /// </summary>
    /// <remarks>
    /// This is a presentation contract, not a domain type: three already-formatted
    /// values. An item that does not implement it still lists fine, it just gets the
    /// single-line rendering from <see cref="ListBox.GetItemText"/>.
    /// </remarks>
    public interface IGameListItem
    {
        /// <summary>Line one. The game title.</summary>
        string PrimaryText { get; }

        /// <summary>Line two. Context such as platform and genre. May be empty.</summary>
        string SecondaryText { get; }

        /// <summary>The 0-10 score shown as a chip, or <see langword="null"/> when unscored.</summary>
        double? Score { get; }
    }

    /// <summary>
    /// An owner-drawn list of games: two lines of text per row and a score chip on
    /// the right, with hover and selection states that follow the theme instead of
    /// the system highlight colour.
    /// </summary>
    /// <remarks>
    /// <see cref="ControlStyles.UserPaint"/> is deliberately <em>not</em> set here.
    /// <see cref="ListBox"/> is a native control; taking over its painting entirely
    /// breaks scrolling and item hit-testing. Setting
    /// <see cref="Control.DoubleBuffered"/> gives the flicker-free redraw without
    /// touching the native paint cycle.
    /// </remarks>
    [ToolboxItem(true)]
    [Description("An owner-drawn game list with two-line rows and a score chip.")]
    public class GameListBox : ListBox
    {
        private int _hoveredIndex = -1;

        /// <summary>Initialises a new game list.</summary>
        public GameListBox()
        {
            DoubleBuffered = true;
            DrawMode = DrawMode.OwnerDrawFixed;
            BorderStyle = BorderStyle.None;
            IntegralHeight = false;
            ItemHeight = Theme.Metrics.ListRow;
            BackColor = Theme.SurfaceRaised;
            ForeColor = Theme.TextPrimary;
            Font = Theme.Fonts.BodyStrong;
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
        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);

            if (e == null)
            {
                return;
            }

            int index = IndexFromPoint(e.Location);
            if (index != _hoveredIndex)
            {
                int previous = _hoveredIndex;
                _hoveredIndex = index;
                InvalidateRow(previous);
                InvalidateRow(index);
            }
        }

        /// <inheritdoc/>
        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            if (_hoveredIndex != -1)
            {
                int previous = _hoveredIndex;
                _hoveredIndex = -1;
                InvalidateRow(previous);
            }
        }

        /// <inheritdoc/>
        protected override void OnDrawItem(DrawItemEventArgs e)
        {
            if (e == null || e.Index < 0 || e.Index >= Items.Count)
            {
                return;
            }

            Graphics g = e.Graphics;
            Rectangle row = e.Bounds;
            bool selected = (e.State & DrawItemState.Selected) == DrawItemState.Selected;
            bool hovered = e.Index == _hoveredIndex && !selected;

            Color background = selected
                ? Theme.Accent
                : (hovered ? Theme.AccentSubtle : Theme.SurfaceRaised);

            Rectangle fill = new Rectangle(
                row.X + Theme.Space.S,
                row.Y + 1,
                Math.Max(0, row.Width - (Theme.Space.S * 2)),
                Math.Max(0, row.Height - 2));

            using (SolidBrush surface = new SolidBrush(Theme.SurfaceRaised))
            {
                g.FillRectangle(surface, row);
            }

            g.FillRoundedRect(background, fill, Theme.Radius.Control);

            object? item = Items[e.Index];
            IGameListItem? presentable = item as IGameListItem;

            string primary = presentable != null ? presentable.PrimaryText : (GetItemText(item) ?? string.Empty);
            string secondary = presentable != null ? presentable.SecondaryText : string.Empty;
            double? score = presentable != null ? presentable.Score : null;

            Color primaryColor = selected ? Theme.OnAccent : Theme.TextPrimary;
            Color secondaryColor = selected ? Theme.AccentSubtle : Theme.TextSecondary;

            int chipWidth = Theme.Metrics.BadgeMinWidth;
            int chipHeight = Theme.Metrics.Badge;
            bool showChip = presentable != null;

            int textLeft = fill.X + Theme.Space.M;
            int textRight = fill.Right - Theme.Space.M - (showChip ? chipWidth + Theme.Space.M : 0);
            int textWidth = Math.Max(0, textRight - textLeft);

            if (textWidth > 0)
            {
                bool hasSecondary = !string.IsNullOrEmpty(secondary);
                int primaryHeight = Font.Height;
                int secondaryHeight = hasSecondary ? Theme.Fonts.Caption.Height : 0;
                int blockHeight = primaryHeight + secondaryHeight;
                int top = fill.Y + ((fill.Height - blockHeight) / 2);

                TextRenderer.DrawText(
                    g,
                    primary,
                    Font,
                    new Rectangle(textLeft, top, textWidth, primaryHeight),
                    primaryColor,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);

                if (hasSecondary)
                {
                    TextRenderer.DrawText(
                        g,
                        secondary,
                        Theme.Fonts.Caption,
                        new Rectangle(textLeft, top + primaryHeight, textWidth, secondaryHeight),
                        secondaryColor,
                        TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis |
                        TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
                }
            }

            if (showChip)
            {
                Rectangle chip = new Rectangle(
                    fill.Right - Theme.Space.M - chipWidth,
                    fill.Y + ((fill.Height - chipHeight) / 2),
                    chipWidth,
                    chipHeight);

                // The chip keeps its own band colour even on a selected row: it is an
                // opaque surface, so it never has to contrast with the accent behind it.
                g.FillRoundedRect(RatingBadge.BandColorFor(score), chip, Theme.Radius.Badge);

                TextRenderer.DrawText(
                    g,
                    RatingBadge.FormatScore(score),
                    Theme.Fonts.BodyStrong,
                    chip,
                    Theme.OnAccent,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }

            if ((e.State & DrawItemState.Focus) == DrawItemState.Focus && Focused)
            {
                g.DrawRoundedRect(
                    selected ? Theme.OnAccent : Theme.Accent,
                    Rectangle.Inflate(fill, -2, -2),
                    Theme.Radius.Control,
                    Theme.Metrics.BorderThickness);
            }
        }

        private void InvalidateRow(int index)
        {
            if (index >= 0 && index < Items.Count)
            {
                Invalidate(GetItemRectangle(index));
            }
        }
    }
}
