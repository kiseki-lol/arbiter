namespace Kiseki.Arbiter;

using Google.Protobuf;

internal class SocketState
{
    public byte[] Buffer = new byte[1024];
    public ushort Size = 0;
    public Socket? Socket = null;
    public TcpClient? TcpClient;
}

public class TcpServer
{
    private static readonly ManualResetEvent Finished = new(false);
    private static Socket? Listener;
    private static bool IsListening = false;

    public static int Start()
    {
        const string LOG_IDENT = "TcpServer::Start";

        IPEndPoint localEndPoint = new(IPAddress.Any, Settings.GetServicePort());

        Listener = new(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            Listener.Bind(localEndPoint);
            Listener.Listen(100);
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed to bind to port {Settings.GetServicePort()}: {ex}", LogSeverity.Error);

            if (ex is SocketException)
            {
                Logger.Write(LOG_IDENT, $"Is another instance of {Constants.PROJECT_NAME}.Arbiter running?", LogSeverity.Error);
            }

            return -1;
        }

        try
        {
            IsListening = true;
            Task.Run(ListenForConnections);
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed to start TcpServer: {ex}", LogSeverity.Error);

            return -1;
        }

        return Settings.GetServicePort();
    }

    public static void Stop()
    {
        IsListening = false;
        Listener!.Close();
    }

    private static void ListenForConnections()
    {
        while (IsListening)
        {
            Finished.Reset();
            Listener!.BeginAccept(new(AcceptCallback), Listener);
            Finished.WaitOne();
        }
    }

    private static void AcceptCallback(IAsyncResult result)
    {
        const string LOG_IDENT = "TcpServer::AcceptCallback";

        Socket? handler = null;
        TcpClient? TcpClient = null;

        try
        {
            Finished.Set();

            Socket listener = (Socket)result.AsyncState!;
            
            handler = listener.EndAccept(result);
            TcpClient = new(handler);

            Logger.Write(LOG_IDENT, $"Machine '{TcpClient.IpAddress}' connected on port {TcpClient.Port}.", LogSeverity.Event);
        }
        catch (Exception ex)
        {
            if (IsListening)
            {
                Logger.Write(LOG_IDENT, $"Failed to accept connection: {ex}", LogSeverity.Error);
            }

            return;
        }

        SocketState state = new()
        {
            Socket = handler,
            TcpClient = TcpClient
        };

        handler!.BeginReceive(state.Buffer, 0, 1024, 0, new(ReadCallback), state);
    }

    private static void ReadCallback(IAsyncResult result)
    {
        const string LOG_IDENT = "TcpServer::ReadCallback";

        SocketState state = (SocketState)result.AsyncState!;
        Socket handler = state.Socket!;

        try
        {
            int bytes = handler.EndReceive(result);

            if (bytes <= 0)
            {
                return;
            }

            if (state.Buffer[0] != 0x02)
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' did not send a valid message (no MSGREAD byte).", LogSeverity.Warning);
                handler.Close();
            }

            if (state.Size == 0)
            {
                // We're on the MSGREAD byte, so let's read the next 2 bytes which should give us MSGSIZE.
                // Once we're able to read MSGSIZE, we'll read all the way to MSGSIZE - 3 (since 3 bytes have already been read)

                state.Size = 3;
                handler.BeginReceive(state.Buffer, 0, 2, 0, new AsyncCallback(ReadCallback), state);

                return;
            }
            else if (state.Size == 3)
            {
                // Now we're on the two MSGSIZE bytes.
                // Let's read the rest of the message according to what MSGSIZE tells us to read to.

                ushort size = 0;

                try
                {
                    size = BitConverter.ToUInt16(state.Buffer, 1); // Offset by 1 (MSGREAD[STX])
                }
                catch
                {
                    Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' did not send a valid message (no MSGSIZE bytes).", LogSeverity.Warning);
                    handler.Close();

                    return;
                }

                if (size < ushort.MinValue || size > ushort.MaxValue)
                {
                    Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' did not send a valid message (MSGSIZE was out of range).", LogSeverity.Warning);
                    handler.Close();
                }

                state.Size = size;

                // We'll read up to state.Size - 3 bytes here since 3 bytes have already been read (MSGREAD and MSGSIZE)
                handler.BeginReceive(state.Buffer, 0, state.Size - 3, 0, new AsyncCallback(ReadCallback), state);

                return;
            }

            // If we've reached this far, we should now be able to parse this into a workable message.
            if (!Message.TryParse(state.Buffer, out Message? message))
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' did not send a valid message.", LogSeverity.Warning);
                handler.Close();
                
                return;
            }

            // Verify signature
            if (!Verifier.Verify(message!.Data!, message.Signature!))
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' sent a message with a bad or malformed signature.", LogSeverity.Warning);
                handler.Close();

                return;
            }

            // See if signal is older than ten seconds (so that people can't re-send signed destructive commands, such as "CloseAllJobs")
            int elapsed = DateTime.Now.ToUnixTime() - message.Signal!.Nonce.ToDateTime().ToUnixTime();
            if (elapsed > 10)
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' sent an expired message (was {elapsed - 10} seconds past expiry).", LogSeverity.Warning);
                handler.Close();
            }

            // Process and send response data
            SendData(handler, ProcessSignal(message.Signal, state.TcpClient!));
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed to read data: {ex}", LogSeverity.Error);
        }
    }

    private static void SendCallback(IAsyncResult result)
    {
        const string LOG_IDENT = "TcpServer::SendCallback";

        try
        {
            Socket handler = (Socket)result.AsyncState!;

            handler.EndSend(result);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed to finish sending data: {ex}", LogSeverity.Error);
        }
    }

    private static void SendData(Socket handler, byte[] data)
    {
        const string LOG_IDENT = "TcpServer::SendData";

        try
        {
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed to send data: {ex}", LogSeverity.Error);
        }
    }

    private static byte[] ProcessSignal(Proto.Signal signal, TcpClient TcpClient)
    {
        return new byte[1024];
    }
}