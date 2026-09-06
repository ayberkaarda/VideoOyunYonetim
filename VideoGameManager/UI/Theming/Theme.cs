using System.Drawing;
using System.Drawing.Text;

namespace VideoGameManager.UI.Theming
{
    /// <summary>
    /// Semantic role of a card accent. Used by <see cref="Controls.FlatCardButton"/>
    /// to pick the icon disc colours without hard-coding an RGB value at the call site.
    /// </summary>
    public enum CardTone
    {
        /// <summary>The single brand accent.</summary>
        Accent,

        /// <summary>Positive / confirming action.</summary>
        Success,

        /// <summary>Action that needs attention.</summary>
        Warning,

        /// <summary>Destructive or exiting action.</summary>
        Danger
    }

    /// <summary>
    /// The single source of truth for every colour, font, spacing step, corner
    /// radius and control metric used by the UI layer.
    /// </summary>
    /// <remarks>
    /// <para>
    /// Every palette entry below was chosen by measuring the WCAG 2.1 contrast
    /// ratio of the text/background pair it takes part in, not by eye. Normal
    /// text pairs clear 4.5:1, large and bold text pairs clear 3:1. The measured
    /// ratios are documented next to each colour so a future edit cannot silently
    /// drop a pair below the threshold.
    /// </para>
    /// <para>
    /// The project currently ships as DPI unaware, so all metrics are plain
    /// pixel constants. They are collected here rather than spread over the
    /// controls so that moving to PerMonitorV2 later is a mechanical change:
    /// scale the values in <see cref="Metrics"/>, <see cref="Space"/> and
    /// <see cref="Radius"/> once, and every control follows.
    /// </para>
    /// </remarks>
    public static class Theme
    {
        // ------------------------------------------------------------------
        // Surfaces
        // ------------------------------------------------------------------

        /// <summary>Page background. The lowest surface in the elevation scale.</summary>
        public static readonly Color Surface = FromRgb(0xF4F5F7);

        /// <summary>Raised surface: cards, inputs, list bodies.</summary>
        public static readonly Color SurfaceRaised = FromRgb(0xFFFFFF);

        /// <summary>Sunken surface: wells, image placeholders, inert areas.</summary>
        public static readonly Color SurfaceSunken = FromRgb(0xECEEF1);

        /// <summary>Background of a disabled control.</summary>
        public static readonly Color SurfaceDisabled = FromRgb(0xF0F1F4);

        /// <summary>Hover wash for a neutral (secondary) surface.</summary>
        public static readonly Color SurfaceHover = FromRgb(0xF1F3F6);

        /// <summary>Pressed wash for a neutral (secondary) surface.</summary>
        public static readonly Color SurfacePressed = FromRgb(0xE4E7EC);

        /// <summary>Decorative hairline that separates surfaces of the same elevation.
        /// 1.29:1 on <see cref="SurfaceRaised"/>: it never carries meaning on its own —
        /// removing it would not make any control impossible to identify — so WCAG 1.4.11
        /// does not apply to it.</summary>
        public static readonly Color Border = FromRgb(0xDFE3E8);

        /// <summary>Border of an editable field. This one <em>does</em> carry meaning: it is
        /// the only thing marking the boundary of a text field, so it clears the 3:1 that
        /// WCAG 1.4.11 asks of non-text UI components — 3.27:1 on <see cref="SurfaceRaised"/>.</summary>
        public static readonly Color InputBorder = FromRgb(0x878F99);

        /// <summary>Border of a disabled control.</summary>
        public static readonly Color BorderDisabled = FromRgb(0xE2E5EA);

        // ------------------------------------------------------------------
        // Text
        // ------------------------------------------------------------------

        /// <summary>Primary text. 16.56:1 on <see cref="SurfaceRaised"/>, 15.18:1 on <see cref="Surface"/>.</summary>
        public static readonly Color TextPrimary = FromRgb(0x1B1F24);

        /// <summary>Secondary text: captions, sub-titles, field labels.
        /// 6.00:1 on <see cref="SurfaceRaised"/>, 5.50:1 on <see cref="Surface"/>.</summary>
        public static readonly Color TextSecondary = FromRgb(0x5A6472);

        /// <summary>Disabled text. 4.05:1 on <see cref="SurfaceRaised"/> and 3.59:1 on
        /// <see cref="SurfaceDisabled"/> — deliberately kept above 3:1 even though
        /// WCAG 1.4.3 exempts disabled controls, so a greyed-out field is still readable.</summary>
        public static readonly Color TextDisabled = FromRgb(0x767F8C);

        // ------------------------------------------------------------------
        // Accent — one hue for the whole application
        // ------------------------------------------------------------------

        /// <summary>The single accent. 5.69:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color Accent = FromRgb(0x0B63CE);

        /// <summary>Accent under the pointer. 6.99:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color AccentHover = FromRgb(0x0A56B4);

        /// <summary>Accent while pressed. 9.00:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color AccentPressed = FromRgb(0x08488F);

        /// <summary>Accent tint for icon discs and selected-row washes.
        /// 4.95:1 with <see cref="Accent"/>, 14.41:1 with <see cref="TextPrimary"/>.</summary>
        public static readonly Color AccentSubtle = FromRgb(0xE7F0FC);

        /// <summary>Accent tint under the pointer.</summary>
        public static readonly Color AccentSubtleHover = FromRgb(0xD7E6FA);

        /// <summary>Text and glyphs drawn on top of <see cref="Accent"/>.</summary>
        public static readonly Color OnAccent = FromRgb(0xFFFFFF);

        /// <summary>Background of a disabled accent button.</summary>
        public static readonly Color AccentDisabled = FromRgb(0xE3E6EB);

        // ------------------------------------------------------------------
        // Semantic
        // ------------------------------------------------------------------

        /// <summary>Success. 5.82:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color Success = FromRgb(0x17743F);

        /// <summary>Success tint. 5.07:1 with <see cref="Success"/>.</summary>
        public static readonly Color SuccessSubtle = FromRgb(0xE4F3E9);

        /// <summary>Warning. A dark amber rather than a bright yellow: 5.93:1 with
        /// <see cref="OnAccent"/>, where the previous #ECD540 managed only 1.48:1.</summary>
        public static readonly Color Warning = FromRgb(0x8A5A00);

        /// <summary>Warning tint. 5.24:1 with <see cref="Warning"/>.</summary>
        public static readonly Color WarningSubtle = FromRgb(0xFBF0DA);

        /// <summary>Danger. 6.54:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color Danger = FromRgb(0xB3261E);

        /// <summary>Danger under the pointer. 8.01:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color DangerHover = FromRgb(0x9C1F18);

        /// <summary>Danger while pressed. 10.24:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color DangerPressed = FromRgb(0x7F1913);

        /// <summary>Danger tint. 5.58:1 with <see cref="Danger"/>.</summary>
        public static readonly Color DangerSubtle = FromRgb(0xFBE9E7);

        // ------------------------------------------------------------------
        // Title bar
        // ------------------------------------------------------------------

        /// <summary>The 48 px dark strip at the top of every window.</summary>
        public static readonly Color TitleBar = FromRgb(0x23272E);

        /// <summary>Caption button under the pointer. 10.57:1 with <see cref="TitleBarText"/>.</summary>
        public static readonly Color TitleBarHover = FromRgb(0x333941);

        /// <summary>Caption button while pressed.</summary>
        public static readonly Color TitleBarPressed = FromRgb(0x3D444E);

        /// <summary>Close button under the pointer. 5.66:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color CloseHover = FromRgb(0xC42B1C);

        /// <summary>Close button while pressed. 8.92:1 with <see cref="OnAccent"/>.</summary>
        public static readonly Color ClosePressed = FromRgb(0x8E1F13);

        /// <summary>Window title text. 13.60:1 on <see cref="TitleBar"/>.</summary>
        public static readonly Color TitleBarText = FromRgb(0xF2F4F7);

        /// <summary>Secondary text on the title strip. 8.18:1 on <see cref="TitleBar"/>.</summary>
        public static readonly Color TitleBarTextSecondary = FromRgb(0xB9C0CA);

        // ------------------------------------------------------------------
        // Card accent tones
        // ------------------------------------------------------------------

        /// <summary>Glyph colour of a <see cref="CardTone"/> icon disc.</summary>
        /// <param name="tone">The semantic tone.</param>
        /// <returns>A colour that clears 4.5:1 against <see cref="CardToneSurface"/> for the same tone.</returns>
        public static Color CardToneForeground(CardTone tone)
        {
            switch (tone)
            {
                case CardTone.Success: return Success;
                case CardTone.Warning: return Warning;
                case CardTone.Danger: return Danger;
                default: return Accent;
            }
        }

        /// <summary>Disc fill of a <see cref="CardTone"/> icon.</summary>
        /// <param name="tone">The semantic tone.</param>
        /// <returns>The tint that pairs with <see cref="CardToneForeground"/>.</returns>
        public static Color CardToneSurface(CardTone tone)
        {
            switch (tone)
            {
                case CardTone.Success: return SuccessSubtle;
                case CardTone.Warning: return WarningSubtle;
                case CardTone.Danger: return DangerSubtle;
                default: return AccentSubtle;
            }
        }

        // ------------------------------------------------------------------
        // Type scale
        // ------------------------------------------------------------------

        /// <summary>The Segoe UI type scale: 8.25 / 9.75 / 12 / 15.75 pt.</summary>
        public static class Fonts
        {
            private const string UiFamily = "Segoe UI";
            private const string StrongFamily = "Segoe UI Semibold";
            private const string GlyphFamily = "Segoe MDL2 Assets";
            private const string GlyphFallbackFamily = "Segoe UI Symbol";

            /// <summary>11 px. Sub-titles, helper text, list row secondary line.</summary>
            public static readonly Font Caption;

            /// <summary>13 px. The default body size.</summary>
            public static readonly Font Body;

            /// <summary>13 px semibold. Field labels, list row primary line.</summary>
            public static readonly Font BodyStrong;

            /// <summary>16 px. Button labels and card titles.</summary>
            public static readonly Font BodyLarge;

            /// <summary>16 px semibold. Primary button labels.</summary>
            public static readonly Font BodyLargeStrong;

            /// <summary>21 px semibold. Page and window titles.</summary>
            public static readonly Font Title;

            /// <summary>10 pt icon font. Caption buttons and inline affordances.</summary>
            public static readonly Font Glyph10;

            /// <summary>16 pt icon font. Card icon discs.</summary>
            public static readonly Font Glyph16;

            /// <summary>20 pt icon font. Empty-state illustrations.</summary>
            public static readonly Font Glyph20;

            static Fonts()
            {
                bool hasStrong = IsInstalled(StrongFamily);
                string glyphFamily = IsInstalled(GlyphFamily)
                    ? GlyphFamily
                    : (IsInstalled(GlyphFallbackFamily) ? GlyphFallbackFamily : UiFamily);

                Caption = new Font(UiFamily, 8.25f, FontStyle.Regular, GraphicsUnit.Point);
                Body = new Font(UiFamily, 9.75f, FontStyle.Regular, GraphicsUnit.Point);
                BodyLarge = new Font(UiFamily, 12f, FontStyle.Regular, GraphicsUnit.Point);

                // "Segoe UI Semibold" is a separate family, so it must be requested with
                // FontStyle.Regular. On a machine that lacks it, fall back to bold Segoe UI.
                BodyStrong = hasStrong
                    ? new Font(StrongFamily, 9.75f, FontStyle.Regular, GraphicsUnit.Point)
                    : new Font(UiFamily, 9.75f, FontStyle.Bold, GraphicsUnit.Point);
                BodyLargeStrong = hasStrong
                    ? new Font(StrongFamily, 12f, FontStyle.Regular, GraphicsUnit.Point)
                    : new Font(UiFamily, 12f, FontStyle.Bold, GraphicsUnit.Point);
                Title = hasStrong
                    ? new Font(StrongFamily, 15.75f, FontStyle.Regular, GraphicsUnit.Point)
                    : new Font(UiFamily, 15.75f, FontStyle.Bold, GraphicsUnit.Point);

                Glyph10 = new Font(glyphFamily, 10f, FontStyle.Regular, GraphicsUnit.Point);
                Glyph16 = new Font(glyphFamily, 16f, FontStyle.Regular, GraphicsUnit.Point);
                Glyph20 = new Font(glyphFamily, 20f, FontStyle.Regular, GraphicsUnit.Point);
            }

            private static bool IsInstalled(string familyName)
            {
                using (InstalledFontCollection installed = new InstalledFontCollection())
                {
                    FontFamily[] families = installed.Families;
                    for (int i = 0; i < families.Length; i++)
                    {
                        if (string.Equals(families[i].Name, familyName, System.StringComparison.OrdinalIgnoreCase))
                        {
                            return true;
                        }
                    }
                }

                return false;
            }
        }

        /// <summary>The 4/8 based spacing scale. Nothing in the UI may use a gap that is not one of these.</summary>
        public static class Space
        {
            /// <summary>4 px. Gap inside a compound control, e.g. glyph to label.</summary>
            public const int S = 4;

            /// <summary>8 px. Gap between tightly related controls.</summary>
            public const int M = 8;

            /// <summary>16 px. Gap between control groups.</summary>
            public const int L = 16;

            /// <summary>24 px. Form padding and gaps between sections.</summary>
            public const int XL = 24;
        }

        /// <summary>The 4/6/10 corner radius scale.</summary>
        public static class Radius
        {
            /// <summary>6 px. Buttons, inputs, list rows.</summary>
            public const int Control = 6;

            /// <summary>10 px. Cards and panels.</summary>
            public const int Card = 10;

            /// <summary>4 px. Badges and chips.</summary>
            public const int Badge = 4;
        }

        /// <summary>Fixed pixel sizes. The one place to touch when moving off DPI-unaware.</summary>
        public static class Metrics
        {
            /// <summary>Height of the dark title strip.</summary>
            public const int TitleBar = 48;

            /// <summary>Width of one caption button. Its height is <see cref="TitleBar"/>.</summary>
            public const int CaptionButton = 46;

            /// <summary>Height of a single-line input frame.</summary>
            public const int Input = 34;

            /// <summary>Height of a standard button.</summary>
            public const int Button = 36;

            /// <summary>Height of a card button.</summary>
            public const int Card = 104;

            /// <summary>Height of a row in a game list.</summary>
            public const int ListRow = 46;

            /// <summary>Height of a rating badge.</summary>
            public const int Badge = 26;

            /// <summary>Minimum width of a rating badge.</summary>
            public const int BadgeMinWidth = 48;

            /// <summary>Diameter of the icon disc on a card button.</summary>
            public const int IconDisc = 44;

            /// <summary>Width of a hairline border.</summary>
            public const int BorderThickness = 1;

            /// <summary>Width of the ring drawn around a focused control.</summary>
            public const int FocusRing = 2;
        }

        private static Color FromRgb(int rgb)
        {
            return Color.FromArgb(unchecked((int)0xFF000000) | rgb);
        }
    }
}
