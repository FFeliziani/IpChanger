using System;
using System.Net;
using System.Threading.Tasks;
using OVHApi;

namespace IPChangerLib
{
    public static class IPChangerLib
    {
        private const string Name = "legagladio.it";
        public static string CurrentIp { get; private set; }
        public static readonly string MachineName = Environment.MachineName;

        private static bool IsWebsiteUp()
        {
            try
            {
                new WebClient().DownloadString("http://www.legagladio.it");
            }
            catch (Exception)
            {
                return false;
            }
            return true;
        }

        private static void CheckCurrentIp()
        {
            try
            {
                CurrentIp = new WebClient().DownloadString("https://api.ipify.org/?format=string");
            }
            catch (Exception)
            {
                CurrentIp = null;
            }
        }

        public static async Task UpdateIp()
        {
            // Call class to do the ip update logic
            CheckCurrentIp();
            if (CurrentIp != null)
            {
                // Check that legagladio is up
                if (!IsWebsiteUp())
                {
                    var client = new OvhApiClient("CLIENT ID", "APP SECRET",
                        OvhInfra.Europe, "CONF CODE");
                    var WebClient = new WebClient();
                    var mainServerName =
                        WebClient.DownloadString("http://lnx.federicofeliziani.com/legagladio/currentMain");
                    var record = await client.GetDomainZoneRecord(Name, (long) WebsiteIds.MAIN);
                    var legagladio1Record = await client.GetDomainZoneRecord(Name, (long) WebsiteIds.SERVER1);
                    var legagladio2Record = await client.GetDomainZoneRecord(Name, (long) WebsiteIds.SERVER2);
                    if (MachineName.ToLower() == mainServerName)
                    {
                        if (record.Target != CurrentIp)
                        {
                            record.Target = CurrentIp;
                            await client.UpdateDomainZoneRecord(record, Name, 1445634879);
                        }
                        switch (MachineName.ToLower())
                        {
                            case "legagladio1":
                                legagladio1Record.Target = CurrentIp;
                                await client.UpdateDomainZoneRecord(legagladio1Record, Name, 1475221378);
                                break;
                            case "legagladio2":
                                legagladio2Record.Target = CurrentIp;
                                await client.UpdateDomainZoneRecord(legagladio2Record, Name, 1480973240);
                                break;
                            default:
                                break;
                        }
                        await client.CreateDomainZoneRefresh(Name);
                    }
                }
            }
        }
    }
}
