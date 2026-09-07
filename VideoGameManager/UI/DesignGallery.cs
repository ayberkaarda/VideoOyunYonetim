using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using VideoGameManager.UI.Controls;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI
{
    /// <summary>
    /// A visual test page for the control library: every control, in every state it
    /// can be caught in, on one screen.
    /// </summary>
    /// <remarks>
    /// <para>
    /// It reaches no database and imports no forms. Everything it shows is a literal in
    /// this file, including the sample cover, which is drawn in memory. That makes it
    /// runnable with no SQL Server, no seed data and no network, which is the only way a
    /// visual check is worth anything: if the page looks wrong, the library is wrong.
    /// </para>
    /// <para>
    /// Launch with <c>VideoGameManager.exe --gallery</c>.
    /// </para>
    /// </remarks>
    [DesignerCategory("Code")]
    public sealed class DesignGallery : ChromelessForm
    {
        private const int ColumnOneX = 24;
        private const int ColumnOneWidth = 520;
        private const int ColumnTwoX = 564;
        private const int ColumnTwoWidth = 400;
        private const int ColumnThreeX = 984;
        private const int ColumnThreeWidth = 400;
        private const int ContentTop = Theme.Metrics.TitleBar + Theme.Space.XL;
        private const int CardPadding = Theme.Space.L;
        private const int HeaderHeight = 20;

        private readonly PreviewButton? _focusedButton;
        private readonly SearchBox _searchBox;
        private readonly Label _searchEcho;
        private readonly Image _sampleCover;
        private readonly PreviewButton[] _hoverPreviews;
        private readonly PreviewButton[] _pressedPreviews;
        private readonly PreviewCardButton _hoverCard;
        private readonly PreviewCaptionButton[] _captionPreviews;

        /// <summary>Builds the gallery.</summary>
        public DesignGallery()
        {
            Text = "Design Gallery";
            Subtitle = "Every control, every state. No data access.";
            ClientSize = new Size(1408, 900);
            AccessibleName = "Design gallery";

            _sampleCover = BuildSampleCover(300, 400);
            _hoverPreviews = new PreviewButton[3];
            _pressedPreviews = new PreviewButton[3];
            _captionPreviews = new PreviewCaptionButton[3];

            SuspendLayout();

            _focusedButton = BuildButtonCard();
            BuildCardButtonCard(out _hoverCard);
            _searchEcho = new Label();
            int columnTwoY = ContentTop;
            _searchBox = BuildInputCard(ref columnTwoY);
            BuildBadgeCard(ref columnTwoY);
            BuildCoverCard(ref columnTwoY);
            BuildPaletteCard(ref columnTwoY);
            BuildListCard();
            BuildChromeCard();
            BuildLayoutGridCard();

            ResumeLayout(false);
        }

        /// <inheritdoc/>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Hover and pressed cannot be captured from a screenshot of a real pointer,
            // so the preview instances are pinned into those states once the handles
            // exist. Only the previews are pinned; the neighbouring controls stay live.
            for (int i = 0; i < _hoverPreviews.Length; i++)
            {
                _hoverPreviews[i].LockHover();
                _pressedPreviews[i].LockPressed();
            }

            _hoverCard.LockHover();
            _captionPreviews[0].LockHover();
            _captionPreviews[1].LockHover();
            _captionPreviews[2].LockPressed();

            _focusedButton?.Focus();
        }

        /// <inheritdoc/>
        protected override void Dispose(bool disposing)
        {
            if (disposing && _sampleCover != null)
            {
                _sampleCover.Dispose();
            }

            base.Dispose(disposing);
        }

        // ------------------------------------------------------------------
        // Column one
        // ------------------------------------------------------------------

        private PreviewButton? BuildButtonCard()
        {
            const int cardHeight = 274;
            const int buttonWidth = 88;
            const int columnGap = Theme.Space.M;

            RoundedPanel card = AddCard("Buttons", ColumnOneX, ContentTop, ColumnOneWidth, cardHeight);

            int left = ColumnOneX + CardPadding;
            int headerY = ContentTop + CardPadding + 20 + Theme.Space.M;
            string[] states = { "Default", "Hover", "Pressed", "Disabled", "Focused" };

            for (int column = 0; column < states.Length; column++)
            {
                AddCaption(card, states[column], left + (column * (buttonWidth + columnGap)) - ColumnOneX, headerY - ContentTop, buttonWidth);
            }

            ButtonKind[] kinds = { ButtonKind.Primary, ButtonKind.Secondary, ButtonKind.Danger };
            string[] labels = { "Save", "Cancel", "Delete" };
            PreviewButton? focused = null;

            int rowY = headerY + Theme.Fonts.Caption.Height + Theme.Space.S;
            for (int row = 0; row < kinds.Length; row++)
            {
                int y = rowY + (row * (Theme.Metrics.Button + 10));

                for (int column = 0; column < states.Length; column++)
                {
                    PreviewButton button = new PreviewButton();
                    button.Kind = kinds[row];
                    button.Text = labels[row];
                    button.Font = Theme.Fonts.BodyStrong;
                    button.AccessibleName = labels[row] + " " + states[column];
                    button.SetBounds(
                        left + (column * (buttonWidth + columnGap)) - ColumnOneX,
                        y - ContentTop,
                        buttonWidth,
                        Theme.Metrics.Button);

                    if (column == 1)
                    {
                        _hoverPreviews[row] = button;
                    }
                    else if (column == 2)
                    {
                        _pressedPreviews[row] = button;
                    }
                    else if (column == 3)
                    {
                        button.Enabled = false;
                    }
                    else if (column == 4 && row == 0)
                    {
                        focused = button;
                    }

                    card.Controls.Add(button);
                }
            }

            AddCaption(
                card,
                "Focused shows a ring only on the control that actually has focus, so one sample per page.",
                CardPadding,
                cardHeight - CardPadding - Theme.Fonts.Caption.Height,
                ColumnOneWidth - (CardPadding * 2));

            return focused;
        }

        private void BuildCardButtonCard(out PreviewCardButton hoverCard)
        {
            const int cardHeight = 500;
            RoundedPanel card = AddCard(
                "Card buttons",
                ColumnOneX,
                ContentTop + 274 + Theme.Space.XL,
                ColumnOneWidth,
                cardHeight);

            int width = ColumnOneWidth - (CardPadding * 2);
            int top = CardPadding + 20 + Theme.Space.M;

            hoverCard = new PreviewCardButton();
            AddCardButton(card, hoverCard, "Add Game", "Record a new title in the library", "\uE710", CardTone.Accent, CardPadding, top, width);

            PreviewCardButton browse = new PreviewCardButton();
            AddCardButton(card, browse, "Browse Games", "Search and filter everything you own", "\uE8FD", CardTone.Success, CardPadding, top + Theme.Metrics.Card + Theme.Space.M, width);

            PreviewCardButton review = new PreviewCardButton();
            AddCardButton(card, review, "Review Game", "Score a title you have finished", "\uE734", CardTone.Warning, CardPadding, top + ((Theme.Metrics.Card + Theme.Space.M) * 2), width);

            PreviewCardButton exit = new PreviewCardButton();
            AddCardButton(card, exit, "Exit", "Disabled state", "\uE7E8", CardTone.Danger, CardPadding, top + ((Theme.Metrics.Card + Theme.Space.M) * 3), width);
            exit.Enabled = false;
        }

        // ------------------------------------------------------------------
        // Column two
        // ------------------------------------------------------------------

        private SearchBox BuildInputCard(ref int top)
        {
            const int labelWidth = 96;
            const int rowGap = Theme.Space.M;

            int rowsTop = CardPadding + HeaderHeight + Theme.Space.M;
            int rowsHeight = (Theme.Metrics.Input * 5) + (rowGap * 4);
            int echoTop = rowsTop + rowsHeight + Theme.Space.M;
            int cardHeight = echoTop + Theme.Fonts.Caption.Height + CardPadding;

            RoundedPanel card = AddCard("Inputs", ColumnTwoX, top, ColumnTwoWidth, cardHeight);

            int fieldLeft = CardPadding + labelWidth + Theme.Space.M;
            int fieldWidth = ColumnTwoWidth - (CardPadding * 2) - labelWidth - Theme.Space.M;
            int y = rowsTop;

            AddFieldLabel(card, "Game name", CardPadding, y, labelWidth);
            InputFrame nameFrame = new InputFrame();
            nameFrame.SetBounds(fieldLeft, y, fieldWidth, Theme.Metrics.Input);
            TextBox nameBox = new TextBox();
            nameBox.Text = "Hollow Knight";
            nameBox.AccessibleName = "Game name";
            nameFrame.Controls.Add(nameBox);
            card.Controls.Add(nameFrame);

            y += Theme.Metrics.Input + rowGap;
            AddFieldLabel(card, "Platform", CardPadding, y, labelWidth);
            InputFrame platformFrame = new InputFrame();
            platformFrame.SetBounds(fieldLeft, y, fieldWidth, Theme.Metrics.Input);
            ComboBox platformBox = new ComboBox();
            platformBox.DropDownStyle = ComboBoxStyle.DropDownList;
            platformBox.Items.AddRange(new object[] { "PC", "PlayStation 5", "Xbox Series X", "Nintendo Switch" });
            platformBox.SelectedIndex = 0;
            platformBox.AccessibleName = "Platform";
            platformFrame.Controls.Add(platformBox);
            card.Controls.Add(platformFrame);

            y += Theme.Metrics.Input + rowGap;
            AddFieldLabel(card, "Score", CardPadding, y, labelWidth);
            InputFrame invalidFrame = new InputFrame();
            invalidFrame.Invalid = true;
            invalidFrame.SetBounds(fieldLeft, y, fieldWidth, Theme.Metrics.Input);
            TextBox invalidBox = new TextBox();
            invalidBox.Text = "12.5";
            invalidBox.AccessibleName = "Score, invalid";
            invalidFrame.Controls.Add(invalidBox);
            card.Controls.Add(invalidFrame);

            y += Theme.Metrics.Input + rowGap;
            AddFieldLabel(card, "Owner", CardPadding, y, labelWidth);
            InputFrame disabledFrame = new InputFrame();
            disabledFrame.SetBounds(fieldLeft, y, fieldWidth, Theme.Metrics.Input);
            TextBox disabledBox = new TextBox();
            disabledBox.Text = "Read only";
            disabledBox.AccessibleName = "Owner, disabled";
            disabledFrame.Controls.Add(disabledBox);
            disabledFrame.Enabled = false;
            card.Controls.Add(disabledFrame);

            y += Theme.Metrics.Input + rowGap;
            AddFieldLabel(card, "Search", CardPadding, y, labelWidth);
            SearchBox search = new SearchBox();
            search.PlaceholderText = "Filter by title";
            search.Text = "hollow";
            search.SetBounds(fieldLeft, y, fieldWidth, Theme.Metrics.Input);
            search.SearchTextChanged += SearchDebounced;
            card.Controls.Add(search);

            _searchEcho.AutoSize = false;
            _searchEcho.Font = Theme.Fonts.Caption;
            _searchEcho.ForeColor = Theme.TextSecondary;
            _searchEcho.BackColor = Theme.SurfaceRaised;
            _searchEcho.Text = "SearchTextChanged fires 300 ms after the last keystroke.";
            _searchEcho.SetBounds(
                CardPadding,
                echoTop,
                ColumnTwoWidth - (CardPadding * 2),
                Theme.Fonts.Caption.Height);
            card.Controls.Add(_searchEcho);

            top += cardHeight + Theme.Space.XL;
            return search;
        }

        private void BuildBadgeCard(ref int top)
        {
            int badgeTop = CardPadding + HeaderHeight + Theme.Space.M;
            int captionTop = badgeTop + Theme.Metrics.Badge + Theme.Space.S;
            int cardHeight = captionTop + Theme.Fonts.Caption.Height + CardPadding;

            RoundedPanel card = AddCard("Rating badges", ColumnTwoX, top, ColumnTwoWidth, cardHeight);

            double?[] scores = { 9.4d, 7.2d, 4.1d, null };
            string[] captions = { "Success", "Warning", "Danger", "No score" };
            int step = Theme.Metrics.BadgeMinWidth + Theme.Space.XL;
            int captionWidth = Theme.Metrics.BadgeMinWidth + Theme.Space.L;

            for (int i = 0; i < scores.Length; i++)
            {
                int x = CardPadding + (i * step);

                RatingBadge badge = new RatingBadge();
                badge.Score = scores[i];
                badge.AccessibleName = captions[i] + " rating";
                badge.SetBounds(x, badgeTop, Theme.Metrics.BadgeMinWidth, Theme.Metrics.Badge);
                card.Controls.Add(badge);

                AddCaption(card, captions[i], x, captionTop, captionWidth);
            }

            top += cardHeight + Theme.Space.XL;
        }

        private void BuildCoverCard(ref int top)
        {
            const int boxHeight = 150;

            int boxTop = CardPadding + HeaderHeight + Theme.Space.M;
            int captionTop = boxTop + boxHeight + Theme.Space.S;
            int cardHeight = captionTop + Theme.Fonts.Caption.Height + CardPadding;

            RoundedPanel card = AddCard("Cover artwork", ColumnTwoX, top, ColumnTwoWidth, cardHeight);

            int boxWidth = (ColumnTwoWidth - (CardPadding * 2) - (Theme.Space.L * 2)) / 3;
            string[] captions = { "Loaded, zoom fit", "Empty", "Loading" };

            for (int i = 0; i < 3; i++)
            {
                int x = CardPadding + (i * (boxWidth + Theme.Space.L));

                CoverImageBox box = new CoverImageBox();
                box.SetBounds(x, boxTop, boxWidth, boxHeight);
                box.AccessibleName = "Cover, " + captions[i];

                if (i == 0)
                {
                    box.Image = _sampleCover;
                }
                else if (i == 2)
                {
                    box.IsLoading = true;
                }

                card.Controls.Add(box);
                AddCaption(card, captions[i], x, captionTop, boxWidth + Theme.Space.L);
            }

            top += cardHeight + Theme.Space.XL;
        }

        private void BuildPaletteCard(ref int top)
        {
            const int swatchHeight = 32;

            int swatchTop = CardPadding + HeaderHeight + Theme.Space.M;
            int captionTop = swatchTop + swatchHeight + Theme.Space.S;
            int cardHeight = captionTop + Theme.Fonts.Caption.Height + CardPadding;

            RoundedPanel card = AddCard("Measured contrast", ColumnTwoX, top, ColumnTwoWidth, cardHeight);

            Color[] fills = { Theme.Accent, Theme.Success, Theme.Warning, Theme.Danger, Theme.TitleBar, Theme.SurfaceRaised };
            Color[] inks = { Theme.OnAccent, Theme.OnAccent, Theme.OnAccent, Theme.OnAccent, Theme.TitleBarText, Theme.TextPrimary };
            string[] ratios = { "5.69", "5.82", "5.93", "6.54", "13.6", "16.6" };
            string[] names = { "Accent", "Success", "Warning", "Danger", "Title", "Text" };

            int swatchWidth = (ColumnTwoWidth - (CardPadding * 2) - (Theme.Space.M * 5)) / 6;

            for (int i = 0; i < fills.Length; i++)
            {
                int x = CardPadding + (i * (swatchWidth + Theme.Space.M));

                Swatch swatch = new Swatch();
                swatch.Fill = fills[i];
                swatch.Ink = inks[i];
                swatch.Ratio = ratios[i];
                swatch.SetBounds(x, swatchTop, swatchWidth, swatchHeight);
                card.Controls.Add(swatch);

                AddCaption(card, names[i], x, captionTop, swatchWidth + Theme.Space.M);
            }

            top += cardHeight + Theme.Space.XL;
        }

        // ------------------------------------------------------------------
        // Column three
        // ------------------------------------------------------------------

        private void BuildListCard()
        {
            const int cardHeight = 336;
            RoundedPanel card = AddCard("Game list", ColumnThreeX, ContentTop, ColumnThreeWidth, cardHeight);

            GameListBox list = new GameListBox();
            list.SetBounds(
                CardPadding,
                CardPadding + 20 + Theme.Space.M,
                ColumnThreeWidth - (CardPadding * 2),
                Theme.Metrics.ListRow * 6);
            list.AccessibleName = "Games";
            list.Items.AddRange(new object[]
            {
                new SampleGame("The Witcher 3: Wild Hunt", "PC  -  RPG", 9.5d),
                new SampleGame("Hollow Knight", "PC  -  Metroidvania", 9.2d),
                new SampleGame("Forza Horizon 5", "Xbox Series X  -  Racing", 7.4d),
                new SampleGame("Battlefield 1", "PC  -  Shooter", 5.8d),
                new SampleGame("Celeste", "Nintendo Switch  -  Platformer", null),
                new SampleGame("Stardew Valley", "PC  -  Simulation", 8.8d)
            });
            list.SelectedIndex = 1;

            card.Controls.Add(list);
        }

        private void BuildChromeCard()
        {
            const int cardHeight = 240;
            int top = ContentTop + 336 + Theme.Space.XL;
            RoundedPanel card = AddCard("Window chrome", ColumnThreeX, top, ColumnThreeWidth, cardHeight);

            int y = CardPadding + 20 + Theme.Space.M;
            string[] captions = { "Default", "Hover", "Pressed" };
            CaptionButtonKind[] kinds = { CaptionButtonKind.Minimize, CaptionButtonKind.Maximize, CaptionButtonKind.Close };

            for (int i = 0; i < 3; i++)
            {
                int x = CardPadding + (i * (Theme.Metrics.CaptionButton + Theme.Space.L));

                PreviewCaptionButton button = new PreviewCaptionButton();
                button.Kind = kinds[i];
                button.SetBounds(x, y, Theme.Metrics.CaptionButton, Theme.Metrics.TitleBar);
                card.Controls.Add(button);
                _captionPreviews[i] = button;

                AddCaption(card, captions[i], x, y + Theme.Metrics.TitleBar + Theme.Space.S, Theme.Metrics.CaptionButton + Theme.Space.L);
            }

            string[] lines =
            {
                "Alt+F4 closes the window (WS_SYSMENU).",
                "Escape closes the window (CancelButton).",
                "The taskbar button minimises and restores (WS_MINIMIZEBOX).",
                "Dragging uses WM_NCLBUTTONDOWN, so Aero Snap keeps working.",
                "CS_DROPSHADOW gives the borderless window an edge."
            };

            int textY = y + Theme.Metrics.TitleBar + Theme.Space.S + Theme.Fonts.Caption.Height + Theme.Space.M;
            for (int i = 0; i < lines.Length; i++)
            {
                AddCaption(
                    card,
                    lines[i],
                    CardPadding,
                    textY + (i * (Theme.Fonts.Caption.Height + 3)),
                    ColumnThreeWidth - (CardPadding * 2));
            }
        }

        private void BuildLayoutGridCard()
        {
            const int cardHeight = 188;
            int top = ContentTop + 336 + Theme.Space.XL + 240 + Theme.Space.XL;
            RoundedPanel card = AddCard("Layout grid", ColumnThreeX, top, ColumnThreeWidth, cardHeight);

            LayoutGrid grid = new LayoutGrid();
            grid.ColumnCount = 2;
            grid.RowCount = 2;
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            grid.RowStyles.Add(new RowStyle(SizeType.Percent, 50f));
            grid.SetBounds(
                CardPadding,
                CardPadding + 20 + Theme.Space.M,
                ColumnThreeWidth - (CardPadding * 2),
                cardHeight - CardPadding - (CardPadding + 20 + Theme.Space.M));

            string[] labels = { "One", "Two", "Three", "Four" };
            for (int i = 0; i < labels.Length; i++)
            {
                FlatButton button = new FlatButton();
                button.Kind = i == 0 ? ButtonKind.Primary : ButtonKind.Secondary;
                button.Text = labels[i];
                button.Font = Theme.Fonts.BodyStrong;
                button.Dock = DockStyle.Fill;
                grid.Controls.Add(button, i % 2, i / 2);
            }

            card.Controls.Add(grid);
        }

        // ------------------------------------------------------------------
        // Helpers
        // ------------------------------------------------------------------

        private RoundedPanel AddCard(string title, int x, int y, int width, int height)
        {
            RoundedPanel card = new RoundedPanel();
            card.SetBounds(x, y, width, height);
            card.AccessibleName = title;
            Controls.Add(card);

            Label header = new Label();
            header.AutoSize = false;
            header.Text = title;
            header.Font = Theme.Fonts.BodyStrong;
            header.ForeColor = Theme.TextPrimary;
            header.BackColor = Theme.SurfaceRaised;
            header.TextAlign = ContentAlignment.MiddleLeft;
            header.SetBounds(CardPadding, CardPadding, width - (CardPadding * 2), HeaderHeight);
            card.Controls.Add(header);

            return card;
        }

        private static void AddCaption(Control parent, string text, int x, int y, int width)
        {
            Label caption = new Label();
            caption.AutoSize = false;
            caption.Text = text;
            caption.Font = Theme.Fonts.Caption;
            caption.ForeColor = Theme.TextSecondary;
            caption.BackColor = Theme.SurfaceRaised;
            caption.TextAlign = ContentAlignment.MiddleLeft;
            caption.SetBounds(x, y, width, Theme.Fonts.Caption.Height);
            parent.Controls.Add(caption);
        }

        private static void AddFieldLabel(Control parent, string text, int x, int y, int width)
        {
            Label label = new Label();
            label.AutoSize = false;
            label.Text = text;
            label.Font = Theme.Fonts.BodyStrong;
            label.ForeColor = Theme.TextSecondary;
            label.BackColor = Theme.SurfaceRaised;
            label.TextAlign = ContentAlignment.MiddleRight;
            label.SetBounds(x, y, width, Theme.Metrics.Input);
            parent.Controls.Add(label);
        }

        private static void AddCardButton(
            Control parent,
            FlatCardButton button,
            string title,
            string description,
            string glyph,
            CardTone tone,
            int x,
            int y,
            int width)
        {
            button.Text = title;
            button.Description = description;
            button.Glyph = glyph;
            button.Tone = tone;
            button.AccessibleName = title;
            button.SetBounds(x, y, width, Theme.Metrics.Card);
            parent.Controls.Add(button);
        }

        private static Image BuildSampleCover(int width, int height)
        {
            Bitmap bitmap = new Bitmap(width, height);

            using (Graphics g = Graphics.FromImage(bitmap))
            using (LinearGradientBrush background = new LinearGradientBrush(
                new Rectangle(0, 0, width, height),
                Color.FromArgb(255, 20, 32, 58),
                Color.FromArgb(255, 96, 44, 110),
                LinearGradientMode.ForwardDiagonal))
            {
                g.FillRectangle(background, 0, 0, width, height);
                g.SmoothingMode = SmoothingMode.AntiAlias;

                using (Pen ring = new Pen(Color.FromArgb(90, 255, 255, 255), 6f))
                {
                    g.DrawEllipse(ring, (width / 2) - 70, (height / 2) - 110, 140, 140);
                }

                TextRenderer.DrawText(
                    g,
                    "SAMPLE",
                    Theme.Fonts.Title,
                    new Rectangle(0, (height / 2) + 60, width, 40),
                    Color.White,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                TextRenderer.DrawText(
                    g,
                    "COVER 3:4",
                    Theme.Fonts.BodyLarge,
                    new Rectangle(0, (height / 2) + 100, width, 30),
                    Color.FromArgb(200, 255, 255, 255),
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
            }

            return bitmap;
        }

        private void SearchDebounced(object? sender, EventArgs e)
        {
            _searchEcho.Text = "SearchTextChanged: \"" + _searchBox.Text + "\"";
        }

        // ------------------------------------------------------------------
        // Preview helpers: controls pinned into a state a screenshot cannot catch
        // ------------------------------------------------------------------

        private sealed class PreviewButton : FlatButton
        {
            private bool _locked;

            public void LockHover()
            {
                base.OnMouseEnter(EventArgs.Empty);
                _locked = true;
            }

            public void LockPressed()
            {
                base.OnMouseEnter(EventArgs.Empty);
                base.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                _locked = true;
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (!_locked)
                {
                    base.OnMouseEnter(e);
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (!_locked)
                {
                    base.OnMouseLeave(e);
                }
            }

            protected override void OnMouseDown(MouseEventArgs mevent)
            {
                if (!_locked)
                {
                    base.OnMouseDown(mevent);
                }
            }

            protected override void OnMouseUp(MouseEventArgs mevent)
            {
                if (!_locked)
                {
                    base.OnMouseUp(mevent);
                }
            }
        }

        private sealed class PreviewCardButton : FlatCardButton
        {
            private bool _locked;

            public void LockHover()
            {
                base.OnMouseEnter(EventArgs.Empty);
                _locked = true;
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (!_locked)
                {
                    base.OnMouseEnter(e);
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (!_locked)
                {
                    base.OnMouseLeave(e);
                }
            }
        }

        private sealed class PreviewCaptionButton : Controls.CaptionButton
        {
            private bool _locked;

            public void LockHover()
            {
                base.OnMouseEnter(EventArgs.Empty);
                _locked = true;
            }

            public void LockPressed()
            {
                base.OnMouseEnter(EventArgs.Empty);
                base.OnMouseDown(new MouseEventArgs(MouseButtons.Left, 1, 0, 0, 0));
                _locked = true;
            }

            protected override void OnMouseEnter(EventArgs e)
            {
                if (!_locked)
                {
                    base.OnMouseEnter(e);
                }
            }

            protected override void OnMouseLeave(EventArgs e)
            {
                if (!_locked)
                {
                    base.OnMouseLeave(e);
                }
            }

            protected override void OnMouseDown(MouseEventArgs mevent)
            {
                if (!_locked)
                {
                    base.OnMouseDown(mevent);
                }
            }

            protected override void OnMouseUp(MouseEventArgs mevent)
            {
                if (!_locked)
                {
                    base.OnMouseUp(mevent);
                }
            }
        }

        private sealed class Swatch : Control
        {
            private Color _fill = Theme.Accent;
            private Color _ink = Theme.OnAccent;
            private string _ratio = string.Empty;

            public Swatch()
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
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color Fill
            {
                get { return _fill; }
                set { _fill = value; Invalidate(); }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public Color Ink
            {
                get { return _ink; }
                set { _ink = value; Invalidate(); }
            }

            [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
            public string Ratio
            {
                get { return _ratio; }
                set { _ratio = value ?? string.Empty; Invalidate(); }
            }

            protected override void OnPaint(PaintEventArgs e)
            {
                Graphics g = e.Graphics;
                g.Clear(this.ResolveBackColor());

                Rectangle bounds = new Rectangle(0, 0, Width, Height);
                g.FillRoundedRect(_fill, bounds, Theme.Radius.Badge);
                g.DrawRoundedRect(Theme.Border, bounds, Theme.Radius.Badge, Theme.Metrics.BorderThickness);

                TextRenderer.DrawText(
                    g,
                    _ratio,
                    Font,
                    bounds,
                    _ink,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter |
                    TextFormatFlags.SingleLine | TextFormatFlags.NoPadding);
            }
        }

        private sealed class SampleGame : IGameListItem
        {
            private readonly string _primary;
            private readonly string _secondary;
            private readonly double? _score;

            public SampleGame(string primary, string secondary, double? score)
            {
                _primary = primary;
                _secondary = secondary;
                _score = score;
            }

            public string PrimaryText
            {
                get { return _primary; }
            }

            public string SecondaryText
            {
                get { return _secondary; }
            }

            public double? Score
            {
                get { return _score; }
            }

            public override string ToString()
            {
                return _primary;
            }
        }
    }
}
