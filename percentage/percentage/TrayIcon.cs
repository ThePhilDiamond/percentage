using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace percentage
{
    class TrayIcon
    {
        [DllImport("user32.dll", CharSet = CharSet.Auto)]
        static extern bool DestroyIcon(IntPtr handle);

        private const string iconFont = "Roboto";
        private const int iconFontSize = 28;
        private const int refreshInterval = 1000; // in miliseconds

        private string batteryPercentage = "?";
        private readonly NotifyIcon notifyIcon;

        public TrayIcon()
        {
            // initialize menuItem
            MenuItem menuItem = new MenuItem
            {
                Index = 0,
                Text = "Exit"
            };
            menuItem.Click += new EventHandler(menuItem_Click);

            // initialize contextMenu
            ContextMenu contextMenu = new ContextMenu();
            contextMenu.MenuItems.AddRange(new MenuItem[] { menuItem });

            notifyIcon = new NotifyIcon
            {
                ContextMenu = contextMenu,
                Visible = true,
            };

            Timer timer = new Timer { Interval = refreshInterval };
            timer.Tick += new EventHandler(timer_Tick);
            timer.Start();
        }

        private void timer_Tick(object sender, EventArgs e)
        {
            PowerStatus powerStatus = SystemInformation.PowerStatus;
            batteryPercentage = $"{(powerStatus.BatteryLifePercent * 100)}";


            using (Bitmap bitmap = new Bitmap(DrawText(batteryPercentage, new Font(iconFont, iconFontSize), Color.White, Color.Transparent)))
            {
                IntPtr intPtr = bitmap.GetHicon();
                try
                {
                    using (Icon icon = Icon.FromHandle(intPtr))
                    {
                        notifyIcon.Icon = icon;

                        // Offline means the laptop is not connected to a power source
                        if (powerStatus.PowerLineStatus == PowerLineStatus.Offline)
                        {
                            var ts = TimeSpan.FromSeconds(powerStatus.BatteryLifeRemaining);
                            // if you don't want the leading zeros, you can replace the format by '{0}:{1}'
                            notifyIcon.Text = string.Format("{0:00}:{1:00} remaining", ts.Hours, ts.Minutes);
                        }
                        else
                        {
                            notifyIcon.Text = "Charging";
                        }
                    }
                }
                finally
                {
                    DestroyIcon(intPtr);
                }
            }
        }

        private void menuItem_Click(object sender, EventArgs e)
        {
            notifyIcon.Visible = false;
            notifyIcon.Dispose();
            Application.Exit();
        }

        private Image DrawText(string text, Font font, Color textColor, Color backColor)
        {
            Image image = new Bitmap(55, 42);
            using (Graphics graphics = Graphics.FromImage(image))
            {
                // paint the background
                graphics.Clear(backColor);
                graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.SystemDefault;

                // create a brush for the text
                using (Brush textBrush = new SolidBrush(textColor))
                {
                    graphics.DrawString(text, font, textBrush, 0, 0);
                    graphics.Save();
                }
            }

            return image;
        }
    }
}
