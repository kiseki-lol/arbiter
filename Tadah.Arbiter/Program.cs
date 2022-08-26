using System;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;

namespace Tadah.Arbiter
{
    public enum GameServerState
    {
        Online = 0,
        Offline = 1,
        Crashed = 2,
        Paused = 3
    };

    public class Program
    {
        internal static void Main(string[] args)
        {
#if DEBUG
            Log.Write($"Access Key read: {Configuration.AppSettings["AccessKey"]}", LogSeverity.Boot);
            Log.Write($"Current Access key: {Configuration.AppSettings["AccessKey"]}", LogSeverity.Boot);
#else
            Log.Write("Access Key read", LogSeverity.Information);
#endif
            Log.Write("Service starting...", LogSeverity.Boot);
            
            Configuration.Load();

            Log.Write($"Assigned GameserverId: {Configuration.Uuid}", LogSeverity.Boot);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                Task.Run(() => JobManager.MonitorCrashedJobs());
                Task.Run(() => JobManager.MonitorUnresponsiveJobs());
                Task.Run(() => Tampa.ProcessManager.MonitorUnresponsiveProcesses());
            }

            Task.Run(() => Http.StartResourceReporter());

            Http.UpdateState(GameServerState.Online);

            Log.Write("Initializing Tadah Arbiter Service", LogSeverity.Boot);
            int ServicePort = Service.Start();
            Log.Write($"Service Started on port {ServicePort}", LogSeverity.Boot);

            Console.CancelKeyPress += delegate
            {
                Log.Write("Service shutting down...", LogSeverity.Event);

                Service.Stop();
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
}
