using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using OvhApi.Models.Domain.Zone;
using OvhApi.Models.Sms;
using OVHApi;
using Exception = System.Exception;

namespace IpChanger
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
        private string _currentIp;
        private static readonly string MachineName = Environment.MachineName;

        private SysTrayApp()
        {
            _trayIcon = new NotifyIcon
            {
                Text = "Legagladio Ip Changer",
                Icon = new Icon(SystemIcons.WinLogo, 32, 32),
                Visible = true
            };

            UpdateContextMenu();

            MainTimer.Tick += TimerEventProcessor;
            MainTimer.Interval = 30000;
            MainTimer.Start();
        }

        private void UpdateContextMenu()
        {
            var trayMenu = new ContextMenu();
            var trayItem = new MenuItem()
            {
                Text = IsEmptyOrNull(_currentIp) ? "To be updated..." : _currentIp,
                Enabled = false
            };
            trayMenu.MenuItems.Add(trayItem);
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

        private bool IsWebsiteUp()
        {
            try
            {
                new WebClient().DownloadString("http://www.legagladio.it");
            }
            catch (Exception)
            {
                return false;
            }
            return false;
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

        private void CheckCurrentIp()
        {
            try
            {
                _currentIp = new WebClient().DownloadString("https://api.ipify.org/?format=string");
                UpdateContextMenu();
            }
            catch (Exception)
            {
                _currentIp = null;
            }
        }

        private async void UpdateIp(object sender = null, EventArgs e = null)
        {
            _trayIcon.Icon = new Icon(SystemIcons.Shield, 32, 32);
            _trayIcon.Text = "Processing...";

            // Call class to do the ip update logic
            CheckCurrentIp();
            if (_currentIp != null)
            {
                // Check that legagladio is up
                if (!IsWebsiteUp())
                {
                    var client = new OvhApiClient("J1MXVC7BkTJzoTi4", "q3DUZlTvv0Eag01dML1LLo7KapYAj7rQ",
                        OvhInfra.Europe, "s1SL3ItS4noJTfWKo8gmRpb9wwVAi6VR");
                    var record = await client.GetDomainZoneRecord("legagladio.it", 1445634879);
                    if (MachineName == "legagladio1" || record.Target != _currentIp)
                    {
                        var recordToUpdate = new Record
                        {
                            Target = _currentIp,
                            SubDomain = record.SubDomain,
                            Ttl = 60
                        };
                        await client.UpdateDomainZoneRecord(recordToUpdate, "legagladio.it", 1445634879);
                    }
                }
            }

            _trayIcon.Icon = new Icon(SystemIcons.WinLogo, 32, 32);
            _trayIcon.Text = "Legagladio Ip Changer";
        }

        private bool IsEmptyOrNull(string val)
        {
            return val == null || val.Trim().Length == 0;
        }
    }
}