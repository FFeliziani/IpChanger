using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using IpChanger.Properties;
using OvhApi.Models.Domain.Zone;
using OvhApi.Models.Sms;
using OVHApi;
using Exception = System.Exception;
using IpChangerLib = IPChangerLib.IPChangerLib;

namespace IPChangerTray
{
    //Environment.SetEnvironmentVariable("OVH_ENDPOINT", "ovh-eu");
    //Environment.SetEnvironmentVariable("OVH_APPLICATION_KEY", "J1MXVC7BkTJzoTi4");
    //Environment.SetEnvironmentVariable("OVH_APPLICATION_SECRET", "q3DUZlTvv0Eag01dML1LLo7KapYAj7rQ");
    //Environment.SetEnvironmentVariable("OVH_CONSUMER_KEY", "s1SL3ItS4noJTfWKo8gmRpb9wwVAi6VR");

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

        private readonly NotifyIcon _trayIcon;
        private static readonly Timer MainTimer = new Timer();

        private SysTrayApp()
        {
            _trayIcon = new NotifyIcon
            {
                Text = Resources.SysTrayApp_SysTrayApp_Legagladio_Ip_Changer,
                Icon = new Icon(SystemIcons.WinLogo, 32, 32),
                Visible = true
            };

            UpdateContextMenu();

            MainTimer.Tick += TimerEventProcessor;
            MainTimer.Interval = 300000;
            MainTimer.Start();
        }

        private void UpdateContextMenu()
        {
            var trayMenu = new ContextMenu();
            trayMenu.MenuItems.Add(new MenuItem() {Text = IpChangerLib.MachineName, Enabled = false});
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add(new MenuItem()
            {
                Text = IsEmptyOrNull(IpChangerLib.CurrentIp) ? "To be updated..." : IpChangerLib.CurrentIp,
                Enabled = false
            });
            trayMenu.MenuItems.Add("-");
            trayMenu.MenuItems.Add("Update Ip now", UpdateIp);
            trayMenu.MenuItems.Add("Exit", OnExit);
            _trayIcon.ContextMenu = trayMenu;
        }

        private void TimerEventProcessor(object sender, EventArgs e)
        {
            MainTimer.Stop();

            UpdateIp();

            MainTimer.Enabled = true;
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
                _trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }

        private async void UpdateIp(object sender = null, EventArgs e = null)
        {
            _trayIcon.Icon = new Icon(SystemIcons.Shield, 32, 32);
            _trayIcon.Text = Resources.SysTrayApp_UpdateIp_Processing___;
            await IpChangerLib.UpdateIp();
            UpdateContextMenu();
            _trayIcon.Icon = new Icon(SystemIcons.WinLogo, 32, 32);
            _trayIcon.Text = Resources.SysTrayApp_SysTrayApp_Legagladio_Ip_Changer;
        }


        private static bool IsEmptyOrNull(string val)
        {
            return val == null || val.Trim().Length == 0;
        }
    }
}