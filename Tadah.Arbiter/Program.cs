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
            Log.Write($"Access Key read: {AppSettings.AccessKey}", LogSeverity.Boot);
            Log.Write($"Current Access key: {AppSettings.AccessKey}", LogSeverity.Boot);
#else
            Log.Write("Access Key read", LogSeverity.Information);
#endif
            Log.Write("Service starting...", LogSeverity.Boot);
            AppSettings.GameserverId = Http.GetGameserverId();
            Log.Write($"Assigned GameserverId: {AppSettings.GameserverId}", LogSeverity.Boot);

            Task.Run(() => JobManager.MonitorCrashedJobs());
            Task.Run(() => JobManager.MonitorUnresponsiveJobs());
            Task.Run(() => RccServiceProcessManager.MonitorUnresponsiveProcesses());
            Task.Run(() => Http.StartResourceReporter());

            Http.UpdateState(GameServerState.Online);
            new Mutex(true, "ROBLOX_singletonMutex");
            new Mutex(true, "COMET_singletonMutex");

            Log.Write("Initializing Tadah Arbiter Service", LogSeverity.Boot);
            int ServicePort = ArbiterService.Start();
            Log.Write($"Service Started on port {ServicePort}", LogSeverity.Boot);

            Console.CancelKeyPress += delegate
            {
                Console.WriteLine("");
                Log.Write("Service shutting down...", LogSeverity.Event);

                ArbiterService.Stop();
                JobManager.CloseAllJobs();

                Http.UpdateState(GameServerState.Offline);

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
