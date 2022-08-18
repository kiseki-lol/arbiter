using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Tadah.Arbiter
{
    public class TaipeiJob : Job
    {
        public TaipeiJob(string Id, int PlaceId, ClientVersion Version, int Port) : base(Id, PlaceId, Version, Port)
        {
            //
        }

        protected override string InternalClose(bool forceClose = false)
        {
            if (forceClose)
            {
                return "Crashed";
            }

            try
            {
                if (!this.IsRunning || this.Process.HasExited)
                {
                    if (!this.IsRunning)
                    {
                        this.Log("Process has shutdown");
                    }
                    else if (this.Process.HasExited)
                    {
                        this.Log("Process has exited");
                    }

                    if (!this.IsRunning)
                    {
                        this.Process.CloseMainWindow();
                    }

                    this.Process.CloseMainWindow();
                    this.Process.Close();

                    return "Closed";
                }
                else if (Status == JobStatus.Crashed || !this.Process.Responding)
                {
                    this.Process.Kill();
                    this.Process.Close();

                    return "Crashed";
                }
                else
                {
                    this.Process.CloseMainWindow();
                    this.Process.Close();

                    return "Crashed";
                }
            }
            catch (InvalidOperationException)
            {
                return "Crashed";
            }
        }

        protected override void InternalStart()
        {
            string GameserverScript = Http.GetGameserverScript(Id, PlaceId, Port);
            string[] CommandLine = JobManager.GetCommandLine(Version, GameserverScript, this.Id);

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                this.Process = new Process();
                this.Process.StartInfo.FileName = CommandLine[0];
                this.Process.StartInfo.Arguments = CommandLine[1];
                this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Process.Start();
                this.Process.WaitForInputIdle();
            }
            else
            {
                this.Process = new Process();
                this.Process.StartInfo.FileName = "wine";
                this.Process.StartInfo.Arguments = Directory.GetCurrentDirectory() + "/" + CommandLine[0].Replace(@"\", "/") + " " + CommandLine[1];
                this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
                this.Process.Start();
            }
        }

        public override void ExecuteScript(string script)
        {
            return;
        }
    }
}
