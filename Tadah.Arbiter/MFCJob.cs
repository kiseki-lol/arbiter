﻿using System;
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

        protected override string InternalClose()
        {
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
            string GameserverScript = WebManager.GetGameserverScript(Id, PlaceId, Port);
            string[] CommandLine = JobManager.GetCommandLine(Version, GameserverScript);

            this.Process = new Process();
            this.Process.StartInfo.FileName = CommandLine[0];
            this.Process.StartInfo.Arguments = CommandLine[1];
            // this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
            this.Process.StartInfo.WindowStyle = ProcessWindowStyle.Minimized;
            this.Process.Start();
            this.Process.WaitForInputIdle();
        }

        public override object ExecuteScript(string script)
        {
            if (!NamedPipes.Exists(Id))
            {
                return "";
            }

            return NamedPipes.Send(Id, script);
        }
    }
}
