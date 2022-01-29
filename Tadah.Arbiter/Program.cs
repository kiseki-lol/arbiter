using System;
using System.Threading;
using System.Threading.Tasks;

namespace Tadah.Arbiter
{
    public class Program
    {
        internal static void Main(string[] args)
        {
#if DEBUG
            ConsoleEx.WriteLine($"Access Key read: {AppSettings.AccessKey}");
            ConsoleEx.WriteLine($"Current Access key: {AppSettings.AccessKey}");
#else
            ConsoleEx.WriteLine("Access Key read");
#endif

            ConsoleEx.WriteLine("Service starting...");

            Task.Run(() => JobManager.MonitorCrashedJobs());
            Task.Run(() => JobManager.MonitorUnresponsiveJobs());
            Task.Run(() => WebManager.StartResourceReporter());

            WebManager.SetMarker(true);
            new Mutex(true, "ROBLOX_singletonMutex");

            ConsoleEx.WriteLine("Initializing Tadah Arbiter Service");
            int ServicePort = ArbiterService.Start();
            ConsoleEx.WriteLine($"Service Started on port {ServicePort}");

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("Service shutting down...");

                ArbiterService.Stop();
                WebManager.SetMarker(false);
                JobManager.CloseAllJobs();

                // wait for web requests to finish
                Thread.Sleep(10000);
            };

            while (true)
            {
                Thread.Sleep(1000);
            }
        }
    }

    public class Unix
    {
        public static int GetTimestamp()
        {
            return (Int32)(DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0))).TotalSeconds;
        }

        public static int From(DateTime time)
        {
            return (Int32)(time - new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
    }
}
