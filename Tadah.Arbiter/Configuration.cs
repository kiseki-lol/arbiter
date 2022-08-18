using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Tadah.Arbiter
{
    public static class Configuration
    {
        public static Guid GameserverId;
        public static int MaximumPlaceJobs;
        public static int MaximumThumbnailJobs;

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

        public static void Load()
        {
            Dictionary<string, object> identification = Http.Identify();

            Guid.TryParse(identification["id"].ToString(), out GameserverId);
            int.TryParse(identification["maximum_place_jobs"].ToString(), out MaximumPlaceJobs);
            int.TryParse(identification["maximum_thumbnail_jobs"].ToString(), out MaximumThumbnailJobs);
        }
    }
}
