
using System;
using System.Drawing;
using System.Net;
using System.Windows.Forms;
using OvhApi.Models.Domain.Zone;
using OVHApi;

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

        private NotifyIcon trayIcon;
        private ContextMenu trayMenu;
        private static Timer mainTimer = new Timer();
        private string currentIp;
        private static string machineName = Environment.MachineName;

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
            mainTimer.Interval = 30000;
            mainTimer.Start();
        }

        private void TimerEventProcessor(object sender, EventArgs e)
        {
            mainTimer.Stop();

            UpdateIp();

            mainTimer.Enabled = true;
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
                trayIcon.Dispose();
            }
            base.Dispose(isDisposing);
        }

        private void CheckCurrentIP()
        {
            try
            {
                currentIp = new WebClient().DownloadString("https://api.ipify.org/?format=string");
            }
            catch (Exception ex)
            {
                currentIp = null;
            }
        }

        private async void UpdateIp()
        {
            trayIcon.Icon = new Icon(SystemIcons.Shield, 32, 32);
            trayIcon.Text = "Processing...";

            // Call class to do the ip update logic
            CheckCurrentIP();
            if (currentIp != null)
            {
                // Check that legagladio is up
                if (!IsWebsiteUp())
                {
                    OvhApiClient client = new OvhApiClient("J1MXVC7BkTJzoTi4", "q3DUZlTvv0Eag01dML1LLo7KapYAj7rQ", OvhInfra.Europe, "s1SL3ItS4noJTfWKo8gmRpb9wwVAi6VR");
                    var record = await client.GetDomainZoneRecord("legagladio.it", 1445634879);
                    if (machineName == "legagladio1" || record.Target != currentIp)
                    {
                        var recordToUpdate = new Record();
                        recordToUpdate.Target = currentIp;
                        recordToUpdate.SubDomain = record.SubDomain;
                        recordToUpdate.Ttl = 60;
                        await client.UpdateDomainZoneRecord(recordToUpdate, "legagladio.it", 1445634879);
                    }
                }
            }

            trayIcon.Icon = new Icon(SystemIcons.WinLogo, 32, 32);
            trayIcon.Text = "Legagladio Ip Changer";
        }
    }
}