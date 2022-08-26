using System;
using System.Collections.Generic;
using Microsoft.Extensions.Configuration;

namespace Tadah.Arbiter
{
    public static class Configuration
    {
        public static Guid Uuid { get; set; }
        public static int ServicePort { get; set; }
        public static int BasePlaceJobPort { get; set; }
        public static int BaseTampaSoapPort { get; set; }
        public static int MaximumTampaProcesses { get; set; }
        public static int MaximumJobsPerTampaProcess { get; set; }
        public static int MaximumPlaceJobs { get; set; }
        public static int MaximumThumbnailJobs { get; set; }
        public static int MaximumThumbnailJobsPerThumbnailer { get; set; }
        public static int Thumbnailers { get; set; }

        public static readonly IConfiguration AppSettings = GetAppSettings();

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

            Uuid = Guid.Parse((string)identification["uuid"]);
            BasePlaceJobPort = (int)identification["base_place_job_port"];
            BaseTampaSoapPort = (int)identification["base_tampa_soap_port"];
            MaximumTampaProcesses = (int)identification["maximum_tampa_processes"];
            MaximumJobsPerTampaProcess = (int)identification["maximum_jobs_per_tampa_process"];
            MaximumPlaceJobs = (int)identification["maximum_place_jobs"];
            MaximumThumbnailJobs = (int)identification["maximum_thumbnail_jobs"];
            MaximumThumbnailJobsPerThumbnailer = (int)identification["maximum_thumbnail_jobs_per_thumbnailer"];
            Thumbnailers = (int)identification["thumbnailers"];
        }
    }
}
