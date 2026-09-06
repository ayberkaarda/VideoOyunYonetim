using System;
using System.Runtime.InteropServices;

namespace VideoGameManager.UI.Controls
{
    /// <summary>
    /// The small set of Win32 entry points the chrome-less window needs.
    /// Nothing here touches application state; it is window management only.
    /// </summary>
    internal static class NativeMethods
    {
        /// <summary>Window style: the window has a Minimize box.</summary>
        internal const int WS_MINIMIZEBOX = 0x00020000;

        /// <summary>Window style: the window has a system menu, which is what makes
        /// Alt+F4 and Alt+Space work. Without it a borderless form cannot be closed
        /// from the keyboard at all.</summary>
        internal const int WS_SYSMENU = 0x00080000;

        /// <summary>Class style: Windows paints a drop shadow behind the window.</summary>
        internal const int CS_DROPSHADOW = 0x00020000;

        /// <summary>Sent when the user presses the left button in a non-client area.</summary>
        internal const int WM_NCLBUTTONDOWN = 0x00A1;

        /// <summary>Hit-test result meaning "the title bar". Handing this to
        /// <see cref="WM_NCLBUTTONDOWN"/> lets Windows run its own window-drag loop,
        /// which keeps snap and shake gestures working.</summary>
        internal const int HTCAPTION = 0x0002;

        /// <summary>Releases the mouse capture so the drag loop can take over.</summary>
        /// <returns>Non-zero on success.</returns>
        [DllImport("user32.dll", ExactSpelling = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        internal static extern bool ReleaseCapture();

        /// <summary>Posts a message to a window and waits for it to be processed.</summary>
        /// <param name="hWnd">Target window.</param>
        /// <param name="msg">Message id.</param>
        /// <param name="wParam">First parameter.</param>
        /// <param name="lParam">Second parameter.</param>
        /// <returns>The message-specific result.</returns>
        [DllImport("user32.dll", CharSet = CharSet.Unicode, EntryPoint = "SendMessageW")]
        internal static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
    }
}
