using System;
using Microsoft.Extensions.Configuration;

namespace Tadah.Arbiter
{
    public static class Configuration
    {
        public static string GameserverId;
        public static IConfiguration AppSettings = GetAppSettings();

        public static IConfiguration GetAppSettings()
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(AppContext.BaseDirectory)
                .AddJsonFile("AppSettings.json",
                optional: true,
                reloadOnChange: true);

            return builder.Build();
        }
    }
}
