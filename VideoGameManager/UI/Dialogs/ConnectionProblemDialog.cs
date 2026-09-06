using System;
using System.ComponentModel;
using System.Drawing;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Extensions.Logging;
using VideoGameManager.Services;
using VideoGameManager.UI.Controls;
using VideoGameManager.UI.Theming;

namespace VideoGameManager.UI.Dialogs
{
    /// <summary>
    /// Shown at startup when the database cannot be reached, so that a stopped server is
    /// a question the user answers rather than a window that never appears.
    /// </summary>
    /// <remarks>
    /// <para>
    /// The outcome is reported through <see cref="Form.DialogResult"/> rather than a
    /// property of its own:
    /// </para>
    /// <list type="bullet">
    /// <item><description><see cref="DialogResult.OK"/> — a retry succeeded and the
    /// database is now reachable.</description></item>
    /// <item><description><see cref="DialogResult.Continue"/> — the user chose to open
    /// the application anyway; each screen will report its own failure.</description></item>
    /// <item><description><see cref="DialogResult.Cancel"/> — the user quit, which is
    /// also what closing the window or pressing Escape means.</description></item>
    /// </list>
    /// <para>
    /// The window is built in code and has no designer file. It is fixed size and pixel
    /// aligned like every other window in the application.
    /// </para>
    /// </remarks>
    [DesignerCategory("Code")]
    internal sealed class ConnectionProblemDialog : ChromelessForm
    {
        private const int DialogWidth = 520;

        private const int Gutter = Theme.Space.XL;
        private const int ContentWidth = DialogWidth - (Gutter * 2);
        private const int ContentTop = Theme.Metrics.TitleBar + Theme.Space.XL;

        private const int TargetPanelHeight = 64;
        private const int TargetRowHeight = 20;
        private const int TargetLabelWidth = 80;
        private const int TargetInset = Theme.Space.L;
        private const int FirstTargetRowTop = Theme.Space.M + 2;
        private const int SecondTargetRowTop = FirstTargetRowTop + TargetRowHeight + Theme.Space.S;

        private const int RetryWidth = 96;
        private const int ContinueWidth = 150;
        private const int QuitWidth = 96;

        /// <summary>
        /// A label drawn with GDI keeps a few pixels either side of its text, so the width
        /// the text really wraps at is narrower than the control. Measuring against the
        /// narrower width errs towards a box that is one line too tall, which is the
        /// harmless direction to be wrong in.
        /// </summary>
        private const int LabelTextInset = 6;

        /// <summary>Ceiling passed to the text measurement instead of an unbounded height.</summary>
        private const int MeasureHeightCap = 4096;

        /// <summary>
        /// How tall the message block is allowed to grow. The message comes from the
        /// failure report, so its length is not something this window controls; past this
        /// many lines the label stops growing and ellipsises instead, which keeps the
        /// window on the screen.
        /// </summary>
        private const int MaxMessageLines = 6;

        private const int MinStatusLines = 2;
        private const int MaxStatusLines = 3;

        private const TextFormatFlags MeasureFlags = TextFormatFlags.WordBreak;

        private const string WindowTitle = "Connection problem";
        private const string WindowSubtitle = "The game library could not be opened";

        private const string UnknownTarget = "(not configured)";

        private const string FallbackMessage =
            "The database is not reachable, so the game library cannot be loaded.";

        private const string HintMessage =
            "Start the database and choose Retry, or continue without it and see the " +
            "problem again on each screen.";

        private const string CheckingMessage = "Checking the connection...";

        private const string StillUnreachableMessage =
            "Still not reachable. Check that the database is running, then try again.";

        private const string RetryFailedMessage =
            "The check could not be completed. The details were written to the log file.";

        private readonly Func<Task<DatabaseStatus>> _probe;
        private readonly ILogger _logger;
        private readonly Label _statusLabel;
        private readonly FlatButton _retryButton;
        private readonly FlatButton _continueButton;
        private readonly FlatButton _quitButton;

        /// <summary>
        /// Builds the dialog.
        /// </summary>
        /// <param name="message">A message written for the user. The raw failure text never
        /// belongs here: it names drivers and hosts, which helps nobody in front of the screen.</param>
        /// <param name="target">Where the application was looking, shown so the user can tell
        /// a stopped service apart from a wrong setting. Credentials are not part of it.</param>
        /// <param name="probe">Runs the reachability check again. Called on a background
        /// thread, so it must be safe to call more than once.</param>
        /// <param name="logger">Where a failed retry is recorded. May be null.</param>
        public ConnectionProblemDialog(
            string message,
            ConnectionTarget target,
            Func<Task<DatabaseStatus>> probe,
            ILogger logger)
        {
            if (probe == null)
            {
                throw new ArgumentNullException("probe");
            }

            _probe = probe;
            _logger = logger;

            Text = WindowTitle;
            Subtitle = WindowSubtitle;
            ShowMinimizeButton = false;
            ShowInTaskbar = true;
            StartPosition = FormStartPosition.CenterScreen;
            AccessibleName = WindowTitle;
            AccessibleDescription = WindowSubtitle;

            // The window is sized around its text rather than the other way round. The
            // wording here is English, the message can arrive from the failure report,
            // and both are longer than a height picked by hand tends to allow; a block
            // measured at run time cannot be cut off by an edit to a sentence.
            string body =
                (string.IsNullOrWhiteSpace(message) ? FallbackMessage : message.Trim()) +
                Environment.NewLine + Environment.NewLine +
                HintMessage;

            int messageHeight = MeasureBlock(body, Theme.Fonts.Body, 1, MaxMessageLines);
            int targetTop = ContentTop + messageHeight + Theme.Space.L;
            int statusTop = targetTop + TargetPanelHeight + Theme.Space.L;
            int statusHeight = MeasureStatusBlock();
            int buttonTop = statusTop + statusHeight + Theme.Space.M;

            ClientSize = new Size(DialogWidth, buttonTop + Theme.Metrics.Button + Gutter);
            MinimumSize = Size;
            MaximumSize = Size;

            SuspendLayout();

            AddMessage(body, messageHeight);
            AddTargetPanel(target, targetTop);
            _statusLabel = AddStatus(statusTop, statusHeight);
            AddButtons(buttonTop, out _retryButton, out _continueButton, out _quitButton);

            ResumeLayout(false);

            AcceptButton = _retryButton;
        }

        // ------------------------------------------------------------------
        // Measurement
        // ------------------------------------------------------------------

        /// <summary>
        /// Measures how tall a wrapped block of text needs to be at the content width.
        /// </summary>
        /// <param name="text">The text that will be shown.</param>
        /// <param name="font">The font it will be drawn in.</param>
        /// <param name="minimumLines">Lines to reserve even when the text is shorter, so
        /// that later text of that length does not move the controls below it.</param>
        /// <param name="maximumLines">Lines beyond which the block stops growing.</param>
        /// <returns>A height in pixels.</returns>
        private static int MeasureBlock(string text, Font font, int minimumLines, int maximumLines)
        {
            Size measured = TextRenderer.MeasureText(
                text,
                font,
                new Size(ContentWidth - LabelTextInset, MeasureHeightCap),
                MeasureFlags);

            // One spacing step of slack absorbs the difference between what the measuring
            // call reports and what the label's own painting flags produce.
            int height = measured.Height + Theme.Space.S;
            int minimum = (font.Height * minimumLines) + Theme.Space.S;
            int maximum = (font.Height * maximumLines) + Theme.Space.S;

            if (height < minimum)
            {
                height = minimum;
            }

            return height > maximum ? maximum : height;
        }

        /// <summary>
        /// Reserves room for the tallest line of status text the dialog can show, so that
        /// a retry never moves the buttons under the pointer.
        /// </summary>
        /// <returns>A height in pixels.</returns>
        private static int MeasureStatusBlock()
        {
            string[] candidates = { CheckingMessage, StillUnreachableMessage, RetryFailedMessage };

            int height = 0;
            for (int i = 0; i < candidates.Length; i++)
            {
                int candidate = MeasureBlock(candidates[i], Theme.Fonts.Caption, MinStatusLines, MaxStatusLines);
                if (candidate > height)
                {
                    height = candidate;
                }
            }

            return height;
        }

        /// <inheritdoc/>
        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);

            // Retry is the action the dialog exists for, so it starts focused and Enter
            // triggers it rather than the caption strip.
            _retryButton.Focus();
        }

        // ------------------------------------------------------------------
        // Layout
        // ------------------------------------------------------------------

        private void AddMessage(string body, int height)
        {
            Label label = new Label();
            label.Name = "messageLabel";
            label.AutoSize = false;
            label.SetBounds(Gutter, ContentTop, ContentWidth, height);
            label.Font = Theme.Fonts.Body;
            label.ForeColor = Theme.TextPrimary;
            label.BackColor = Color.Transparent;
            label.TextAlign = ContentAlignment.TopLeft;

            // The height was measured for this exact text, so this only matters for a
            // message longer than the cap: it then ends in an ellipsis instead of being
            // cut off in the middle of a word with nothing to show that it was.
            label.AutoEllipsis = true;
            label.Text = body;
            Controls.Add(label);
        }

        private void AddTargetPanel(ConnectionTarget target, int top)
        {
            RoundedPanel panel = new RoundedPanel();
            panel.Name = "targetPanel";
            panel.SetBounds(Gutter, top, ContentWidth, TargetPanelHeight);
            panel.BackColor = Theme.SurfaceSunken;
            panel.BorderColor = Theme.Border;
            panel.TabStop = false;
            Controls.Add(panel);

            AddTargetRow(
                panel,
                "Server",
                target == null ? null : target.Server,
                FirstTargetRowTop);

            AddTargetRow(
                panel,
                "Database",
                target == null ? null : target.Database,
                SecondTargetRowTop);
        }

        private static void AddTargetRow(Control parent, string caption, string value, int top)
        {
            // The captions are aligned by giving them a fixed width rather than by
            // computing an x for each one from its text width: an edit to the wording
            // would otherwise silently push the values out of line.
            Label captionLabel = new Label();
            captionLabel.AutoSize = false;
            captionLabel.SetBounds(TargetInset, top, TargetLabelWidth, TargetRowHeight);
            captionLabel.Font = Theme.Fonts.Caption;
            captionLabel.ForeColor = Theme.TextSecondary;
            captionLabel.BackColor = Color.Transparent;
            captionLabel.TextAlign = ContentAlignment.MiddleLeft;
            captionLabel.Text = caption;
            parent.Controls.Add(captionLabel);

            int valueLeft = TargetInset + TargetLabelWidth + Theme.Space.M;

            Label valueLabel = new Label();
            valueLabel.AutoSize = false;
            valueLabel.SetBounds(
                valueLeft,
                top,
                Math.Max(0, parent.Width - valueLeft - TargetInset),
                TargetRowHeight);
            valueLabel.Font = Theme.Fonts.BodyStrong;
            valueLabel.ForeColor = Theme.TextPrimary;
            valueLabel.BackColor = Color.Transparent;
            valueLabel.TextAlign = ContentAlignment.MiddleLeft;
            valueLabel.AutoEllipsis = true;
            valueLabel.Text = string.IsNullOrEmpty(value) ? UnknownTarget : value;
            valueLabel.AccessibleName = caption;
            parent.Controls.Add(valueLabel);
        }

        private Label AddStatus(int top, int height)
        {
            Label label = new Label();
            label.Name = "statusLabel";
            label.AutoSize = false;
            label.SetBounds(Gutter, top, ContentWidth, height);
            label.Font = Theme.Fonts.Caption;
            label.ForeColor = Theme.TextSecondary;
            label.BackColor = Color.Transparent;
            label.TextAlign = ContentAlignment.TopLeft;

            // A failed retry can report a message this window did not write, so anything
            // past the reserved height ends in an ellipsis rather than disappearing.
            label.AutoEllipsis = true;
            label.Text = string.Empty;
            Controls.Add(label);
            return label;
        }

        private void AddButtons(int top, out FlatButton retry, out FlatButton keepGoing, out FlatButton quit)
        {
            int total = RetryWidth + Theme.Space.M + ContinueWidth + Theme.Space.M + QuitWidth;
            int left = DialogWidth - Gutter - total;

            retry = CreateButton("retryButton", "Retry", ButtonKind.Primary, left, top, RetryWidth);
            retry.Click += RetryClick;
            left += RetryWidth + Theme.Space.M;

            keepGoing = CreateButton("continueButton", "Continue anyway", ButtonKind.Secondary, left, top, ContinueWidth);

            // A button that carries a DialogResult ends the modal loop by itself, so
            // these two need no handler. Retry deliberately carries none, because it has
            // to leave the window open when the database is still down.
            keepGoing.DialogResult = DialogResult.Continue;
            left += ContinueWidth + Theme.Space.M;

            quit = CreateButton("quitButton", "Quit", ButtonKind.Secondary, left, top, QuitWidth);
            quit.DialogResult = DialogResult.Cancel;
        }

        private FlatButton CreateButton(string name, string text, ButtonKind kind, int left, int top, int width)
        {
            FlatButton button = new FlatButton();
            button.Name = name;
            button.Kind = kind;
            button.Text = text;
            button.SetBounds(left, top, width, Theme.Metrics.Button);
            button.AccessibleName = text;
            Controls.Add(button);
            return button;
        }

        // ------------------------------------------------------------------
        // Behaviour
        // ------------------------------------------------------------------

        private async void RetryClick(object sender, EventArgs e)
        {
            // An event handler is the one place where a method may return void and still
            // await. The whole body is guarded so a failure inside the check cannot
            // escape into the application-wide handler as a crash.
            try
            {
                SetBusy(true);
                _statusLabel.Text = CheckingMessage;

                // The check runs off the message loop: the window has to keep repainting
                // while a connection attempt sits in its timeout.
                DatabaseStatus status = await Task.Run(_probe).ConfigureAwait(true);

                if (status != null && status.IsReachable)
                {
                    if (_logger != null)
                    {
                        _logger.LogInformation("Database became reachable after a retry from the startup dialog.");
                    }

                    // Assigning this ends the modal loop, so nothing after it runs except
                    // the finally block below.
                    DialogResult = DialogResult.OK;
                    return;
                }

                if (_logger != null)
                {
                    _logger.LogError(
                        status == null ? null : status.Failure,
                        "Database is still unreachable after a retry from the startup dialog.");
                }

                string reported = status == null ? null : status.Message;
                _statusLabel.Text = string.IsNullOrWhiteSpace(reported)
                    ? StillUnreachableMessage
                    : reported.Trim();
            }
            catch (Exception exception)
            {
                if (_logger != null)
                {
                    _logger.LogError(exception, "The startup connection retry could not be completed.");
                }

                _statusLabel.Text = RetryFailedMessage;
            }
            finally
            {
                if (!IsDisposed)
                {
                    SetBusy(false);
                }
            }
        }

        private void SetBusy(bool busy)
        {
            _retryButton.Enabled = !busy;
            _continueButton.Enabled = !busy;
            _quitButton.Enabled = !busy;
            UseWaitCursor = busy;
        }
    }
}
