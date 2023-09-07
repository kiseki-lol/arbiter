namespace Kiseki.Arbiter;

public static class SignalProcessor
{
    public static bool IsProcessingJobs { get; private set; } = true;

    public static byte[] Process(Signal signal, TcpClient client)
    {
        Response response = new()
        {
            Success = false
        };

        if (!IsProcessingJobs && (signal.Command == Command.CloseJob || signal.Command == Command.OpenJob))
        {
            Logger.Write($"Received command to either close/open a job from machine '{client.IpAddress}' while currently paused.", LogSeverity.Warning);
            response = new()
            {
                Success = false
            };

            return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
        }

        if (signal.Command == Command.Shutdown)
        {
            Logger.Write($"Received shutdown command from {client.IpAddress}!", LogSeverity.Information);

            IsProcessingJobs = false;
            Task.Run(() => {
                Thread.Sleep(1000);
                Program.Shutdown();
            });

            response = new()
            {
                Success = true
            };
        }

        if (signal.Command == Command.Pause)
        {
            // Ideally, we'd call Web::UpdateGameServerStatus but that causes the arbiter to hang indefinitely.
            // Hence, the website just dispatches a StateChange once we return success and the command is pause.
            // TODO: Fix this; and figure out why this happens.

            IsProcessingJobs = !IsProcessingJobs;

            if (!IsProcessingJobs)
            {
                Logger.Write($"Machine '{client.IpAddress}' has paused this gameserver. All future job operations will be ignored.", LogSeverity.Warning);
            }
            else
            {
                Logger.Write($"Machine '{client.IpAddress}' has unpaused this gameserver.", LogSeverity.Event);
            }

            response = new()
            {
                Success = true
            };
        }

        if (signal.Command == Command.Ping)
        {
            long elapsed = DateTimeOffset.Now.ToUnixTimeMilliseconds() - Convert.ToInt64(signal.Data!["timestamp"]);
            Logger.Write($"Received ping from {client.IpAddress} in {elapsed}ms!", LogSeverity.Information);

            response = new()
            {
                Success = true,
                Data = new Dictionary<string, object>()
                {
                    { "elapsed", elapsed }
                }
            };
        }

        return Encoding.UTF8.GetBytes(JsonSerializer.Serialize(response));
    }
}