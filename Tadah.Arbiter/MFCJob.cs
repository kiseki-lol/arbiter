using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Diagnostics;

namespace Tadah.Arbiter
{
    public class MFCJob : Job
    {
        public MFCJob(string Id, int PlaceId, int Version, int Port) : base(Id, PlaceId, Version, Port)
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
            string[] CommandLine = JobManager.GetCommandLine(Version, GameserverScript);

            this.Process = new Process();
            this.Process.StartInfo.FileName = CommandLine[0];
            this.Process.StartInfo.Arguments = CommandLine[1];
            // this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            this.Process.Start();
            this.Process.WaitForInputIdle();
        }

        public override void ExecuteScript(string script)
        {
            if (!LuaPipes.Exists(Id))
            {
                return;
            }

            // DLL will process the script
            LuaPipes.Send(Id, script);
        }
    }
}
