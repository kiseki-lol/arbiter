using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;

namespace Tadah.Arbiter
{
    public class TaipeiJob : Job
    {
        public TaipeiJob(string Id, uint PlaceId, Proto.ClientVersion Version, int Port) : base(Id, PlaceId, Version, Port)
        {
            //
        }

        protected override JobStatus InternalClose(bool forceClose = false)
        {
            if (forceClose)
            {
                return JobStatus.Crashed;
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

                    return JobStatus.Closed;
                }
                else if (Status == JobStatus.Crashed || !this.Process.Responding)
                {
                    this.Process.Kill();
                    this.Process.Close();

                    return JobStatus.Crashed;
                }
                else
                {
                    this.Process.CloseMainWindow();
                    this.Process.Close();

                    return JobStatus.Crashed;
                }
            }
            catch (InvalidOperationException)
            {
                return JobStatus.Crashed;
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
