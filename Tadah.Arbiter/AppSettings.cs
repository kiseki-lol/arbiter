using System;
using System.Configuration;
using System.IO;

namespace Tadah.Arbiter
{
    public static class AppSettings
    {
        public static string GameserverID = ConfigurationManager.AppSettings.Get("GameserverID");
        public static string MachineAddress = ConfigurationManager.AppSettings.Get("MachineAddress");
        public static string BaseUrl = ConfigurationManager.AppSettings.Get("BaseUrl");
        public static string AccessKey = ConfigurationManager.AppSettings.Get("AccessKey");
        public static int BasePort = Convert.ToInt32(ConfigurationManager.AppSettings.Get("BasePort"));
        public static int ServicePort = Convert.ToInt32(ConfigurationManager.AppSettings.Get("ServicePort"));
        public static int MaximumJobs = Convert.ToInt32(ConfigurationManager.AppSettings.Get("MaximumJobs"));
        public static string PublicKeyPath = ConfigurationManager.AppSettings.Get("PublicKeyPath");
        public static string PublicKey = File.ReadAllText(PublicKeyPath);
    }
}
