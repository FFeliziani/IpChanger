using System;
using System.CodeDom;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.ServiceProcess;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using IpChangerLib = IPChangerLib.IPChangerLib;

namespace IPChangerService
{
    public partial class IPChangerService : ServiceBase
    {
        private static readonly EventLog Log = new EventLog();
        private static readonly Timer MainTimer = new Timer(300000);

        public IPChangerService()
        {
            InitializeComponent();
            if (!System.Diagnostics.EventLog.SourceExists("Legagladio"))
            {
                System.Diagnostics.EventLog.CreateEventSource(
                    "Legagladio", "IpChanger");
            }
            Log.Source = "Legagladio";
            Log.Log = "IpChanger";
        }

        protected override void OnStart(string[] args)
        {
            Log.WriteEntry("Started IpChanger Service");
            MainTimer.Enabled = true;
            MainTimer.Elapsed += TimerEventProcessor;
            MainTimer.Start();
        }

        private static async void TimerEventProcessor(object sender, ElapsedEventArgs e)
        {
            try
            {
                MainTimer.Stop();
                await IpChangerLib.UpdateIp();
                Log.WriteEntry($"Machine IP: {IpChangerLib.CurrentIp}");
                MainTimer.Start();
            }
            catch (Exception ex)
            {
                Log.WriteEntry($"Something wrong is happening! {ex.Message}");
            }
            
        }

        protected override void OnStop()
        {
            Log.WriteEntry("Stopped IpChanger Service");
            MainTimer.Stop();
        }
    }
}
