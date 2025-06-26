using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Drawing;

namespace killbotapp
{
    public class OverlayForm : Form
    {
        [DllImport("user32.dll")]
        static extern IntPtr GetDC(IntPtr hWnd);

        [DllImport("user32.dll")]
        static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

        [DllImport("user32.dll")]
        static extern bool InvertRect(IntPtr hDC, ref RECT rect);

        [StructLayout(LayoutKind.Sequential)]
        public struct RECT
        {
            public int Left, Top, Right, Bottom;
        }

        public OverlayForm()
        {
            this.FormBorderStyle = FormBorderStyle.None;
            this.Bounds = Screen.PrimaryScreen.Bounds;
            this.StartPosition = FormStartPosition.Manual;
            this.ShowInTaskbar = false;
            this.TopMost = true;

            this.BackColor = Color.Black;
            this.Opacity = 0; // fully transparent

            // Make the window click-through
            int initialStyle = GetWindowLong(this.Handle, GWL_EXSTYLE);
            SetWindowLong(this.Handle, GWL_EXSTYLE, initialStyle | WS_EX_LAYERED | WS_EX_TRANSPARENT | WS_EX_TOOLWINDOW);
        }

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            InvertScreenColors();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            RestoreScreenColors();
            base.OnFormClosing(e);
        }

        private void InvertScreenColors()
        {
            IntPtr hdc = GetDC(IntPtr.Zero); // entire screen DC

            RECT rect = new RECT
            {
                Left = 0,
                Top = 0,
                Right = Screen.PrimaryScreen.Bounds.Width,
                Bottom = Screen.PrimaryScreen.Bounds.Height
            };

            InvertRect(hdc, ref rect);
            ReleaseDC(IntPtr.Zero, hdc);
        }

        private void RestoreScreenColors()
        {
            // Invert again to restore original colors
            InvertScreenColors();
        }

        // Win32 constants and functions for layered window and transparency
        private const int GWL_EXSTYLE = -20;
        private const int WS_EX_LAYERED = 0x80000;
        private const int WS_EX_TRANSPARENT = 0x20;
        private const int WS_EX_TOOLWINDOW = 0x80;

        [DllImport("user32.dll")]
        static extern int GetWindowLong(IntPtr hWnd, int nIndex);

        [DllImport("user32.dll")]
        static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);
    }
}
