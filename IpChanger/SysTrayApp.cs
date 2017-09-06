using System;
using System.Drawing;
using System.Windows.Forms;

namespace IpChanger
{
    public class SysTrayApp : Form
    {
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        public static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new SysTrayApp());
        }

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private static Timer mainTimer = new Timer();

        private SysTrayApp()
        {
            trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add("Exit", OnExit);

            trayIcon = new NotifyIcon();
            trayIcon.Text = "Legagladio Ip Changer";
            trayIcon.Icon = new Icon(SystemIcons.WinLogo, 32, 32);

            trayIcon.ContextMenu = trayMenu;
            trayIcon.Visible = true;

            mainTimer.Tick += new EventHandler(TimerEventProcessor);
            mainTimer.Interval = 3000;
            mainTimer.Start();
        }

        private void TimerEventProcessor(object sender, EventArgs e)
        {
            mainTimer.Stop();

            trayIcon.Icon = new Icon(SystemIcons.Shield, 32, 32);
            trayIcon.Text = "Processing...";

            // Call class to do the ip update logic


            trayIcon.Icon = new Icon(SystemIcons.WinLogo, 32, 32);
            trayIcon.Text = "Legagladio Ip Changer";

            mainTimer.Enabled = true;
        }

        protected override void OnLoad(EventArgs e)
        {
            Visible = false;
            ShowInTaskbar = false;

            base.OnLoad(e);
        }

        private void OnExit(object sender, EventArgs e)
        {
            Application.Exit();
        }

        protected override void Dispose(bool isDisposing)
        {
            if (isDisposing)
            {
                trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }
    }
}