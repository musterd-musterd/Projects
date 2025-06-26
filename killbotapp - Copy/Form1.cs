using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Drawing;

namespace killbotapp
{
    public partial class Form1 : Form
    {
        private const int HOTKEY_START_ID = 1;
        private const int HOTKEY_STOP_ID = 2;
        private const int HOTKEY_TOGGLE_OVERLAY_ID = 3;

        private CancellationTokenSource cts;
        private OverlayForm overlay;
        private bool overlayActive = false;

        [DllImport("user32.dll")]
        public static extern bool RegisterHotKey(IntPtr hWnd, int id, int fsModifiers, int vk);
        [DllImport("user32.dll")]
        public static extern bool UnregisterHotKey(IntPtr hWnd, int id);

        public Form1()
        {
            InitializeComponent();

            this.Load += Form1_Load;
            this.Load += Form1_Load_1;  // Keep this if event subscribed, else remove both
            this.FormClosing += Form1_FormClosing;

            this.BackColor = Color.Black;

            Label titleLabel = new Label();
            titleLabel.Text = "KILLBOT CPU STRESS TOOL";
            titleLabel.ForeColor = Color.Red;
            titleLabel.Font = new Font("Segoe UI", 16, FontStyle.Bold);
            titleLabel.AutoSize = true;
            titleLabel.Location = new Point(30, 30);
            this.Controls.Add(titleLabel);

            Label instructionsLabel = new Label();
            instructionsLabel.Text = "Press Ctrl + Q to START CPU load\nPress Ctrl + P to STOP CPU load\nPress Ctrl + I to TOGGLE Contrast Overlay";
            instructionsLabel.ForeColor = Color.White;
            instructionsLabel.Font = new Font("Segoe UI", 12);
            instructionsLabel.AutoSize = true;
            instructionsLabel.Location = new Point(30, 80);
            this.Controls.Add(instructionsLabel);
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            try
            {
                Process.GetCurrentProcess().PriorityClass = ProcessPriorityClass.High;
            }
            catch { }

            RegisterHotKey(this.Handle, HOTKEY_START_ID, 0x0002, (int)Keys.Q); // Ctrl + Q
            RegisterHotKey(this.Handle, HOTKEY_STOP_ID, 0x0002, (int)Keys.P);  // Ctrl + P
            RegisterHotKey(this.Handle, HOTKEY_TOGGLE_OVERLAY_ID, 0x0002, (int)Keys.I); // Ctrl + I
        }

        // Empty method just to satisfy event if subscribed
        private void Form1_Load_1(object sender, EventArgs e)
        {
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            UnregisterHotKey(this.Handle, HOTKEY_START_ID);
            UnregisterHotKey(this.Handle, HOTKEY_STOP_ID);
            UnregisterHotKey(this.Handle, HOTKEY_TOGGLE_OVERLAY_ID);

            cts?.Cancel();

            HideOverlay();
        }

        protected override void WndProc(ref Message m)
        {
            const int WM_HOTKEY = 0x0312;
            if (m.Msg == WM_HOTKEY)
            {
                int id = m.WParam.ToInt32();

                if (id == HOTKEY_START_ID)       // Ctrl + Q
                {
                    StartCPULoad(0.5);           // 50% CPU load
                }
                else if (id == HOTKEY_STOP_ID)  // Ctrl + P
                {
                    StopCPULoad();
                }
                else if (id == HOTKEY_TOGGLE_OVERLAY_ID)  // Ctrl + I
                {
                    if (overlayActive)
                    {
                        HideOverlay();
                        overlayActive = false;
                    }
                    else
                    {
                        ShowOverlay();
                        overlayActive = true;
                    }
                }
            }
            base.WndProc(ref m);
        }

        private void StartCPULoad(double loadPercent)
        {
            if (cts != null && !cts.IsCancellationRequested)
                return;

            cts = new CancellationTokenSource();
            int coreCount = Environment.ProcessorCount;

            for (int i = 0; i < coreCount; i++)
            {
                _ = Task.Run(() =>
                {
                    Stopwatch sw = new Stopwatch();
                    while (!cts.IsCancellationRequested)
                    {
                        sw.Restart();
                        while (sw.ElapsedMilliseconds < loadPercent * 100) { }
                        Thread.Sleep((int)((1 - loadPercent) * 100));
                    }
                }, cts.Token);
            }
        }

        private void StopCPULoad()
        {
            if (cts != null)
            {
                cts.Cancel();
                cts = null;
            }
        }

        private void ShowOverlay()
        {
            if (overlay == null || overlay.IsDisposed)
            {
                overlay = new OverlayForm();
                overlay.Show();
            }
        }

        private void HideOverlay()
        {
            if (overlay != null && !overlay.IsDisposed)
            {
                overlay.Close();
                overlay.Dispose();
                overlay = null;
            }
        }
    }
}
