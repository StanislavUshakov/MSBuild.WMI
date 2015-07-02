using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace MSBuild.WMI
{
    /// <summary>
    /// This static class contains some useful helpers
    /// </summary>
    public static class Helpers
    {
        /// <summary>
        /// Check if hostName is a local computer
        /// </summary>
        /// <param name="hostName">Host Name to be checked</param>
        /// <returns></returns>
        public static bool IsLocalHost(string hostName)
        {
            if (string.IsNullOrEmpty(hostName))
                return true;

            try
            {
                var hosIPs = Dns.GetHostAddresses(hostName);
                var localIPs = Dns.GetHostAddresses(Dns.GetHostName());
                return hosIPs.Any(hostIP => IPAddress.IsLoopback(hostIP) || localIPs.Contains(hostIP));
            }
            catch
            {
                return false;
            }
        }
    }
}
