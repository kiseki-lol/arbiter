namespace Kiseki.Arbiter;

using Google.Protobuf;

internal class SocketState
{
    public byte[] Buffer = new byte[1024];
    public ushort Size = 0;
    public Socket? Socket = null;
    public Client? Client;
}

public class Service
{
    private static readonly ManualResetEvent Finished = new(false);
    private static Socket? Listener;

    public static int Start()
    {
        const string LOG_IDENT = "Service::Start";

        IPEndPoint localEndPoint = new(IPAddress.Any, Settings.GetServicePort());

        Listener = new(IPAddress.Any.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

        try
        {
            Listener.Bind(localEndPoint);
            Listener.Listen(100);
        }
        finally
        {
            try
            {
                Task.Run(ListenForConnections);
            }
            catch (Exception ex)
            {
                Logger.Write(LOG_IDENT, $"Failed to start service: {ex}", LogSeverity.Error);
            }
        }

        return Settings.GetServicePort();
    }

    public static void Stop()
    {
        Listener!.Close();
    }

    private static void ListenForConnections()
    {
        while (true)
        {
            Finished.Reset();
            Listener!.BeginAccept(new(AcceptCallback), Listener);
            Finished.WaitOne();
        }
    }

    private static void AcceptCallback(IAsyncResult result)
    {
        const string LOG_IDENT = "Service::AcceptCallback";

        Socket? handler = null;
        Client? client = null;

        try
        {
            Finished.Set();

            Socket listener = (Socket)result.AsyncState!;
            
            handler = listener.EndAccept(result);
            client = new(handler);

            Logger.Write(LOG_IDENT, $"Machine '{client.IpAddress}' connected on port {client.Port}.", LogSeverity.Event);
        }
        finally
        {
            SocketState state = new()
            {
                Socket = handler,
                Client = client
            };

            handler!.BeginReceive(state.Buffer, 0, 1024, 0, new(ReadCallback), state);
        }
    }

    private static void ReadCallback(IAsyncResult result)
    {
        const string LOG_IDENT = "Service::ReadCallback";

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
                Logger.Write(LOG_IDENT, $"Machine '{state.Client!.IpAddress}' did not send a valid message (no MSGREAD byte).", LogSeverity.Warning);
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
                    Logger.Write(LOG_IDENT, $"Machine '{state.Client!.IpAddress}' did not send a valid message (no MSGSIZE bytes).", LogSeverity.Warning);
                    handler.Close();

                    return;
                }

                if (size < ushort.MinValue || size > ushort.MaxValue)
                {
                    Logger.Write(LOG_IDENT, $"Machine '{state.Client!.IpAddress}' did not send a valid message (MSGSIZE was out of range).", LogSeverity.Warning);
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
                Logger.Write(LOG_IDENT, $"Machine '{state.Client!.IpAddress}' did not send a valid message.", LogSeverity.Warning);
                handler.Close();
                
                return;
            }

            // Verify signature
            if (!Verifier.Verify(message!.Data!, message.Signature!))
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.Client!.IpAddress}' sent a message with a bad or malformed signature.", LogSeverity.Warning);
                handler.Close();

                return;
            }

            // See if signal is older than ten seconds (so that people can't re-send signed destructive commands, such as "CloseAllJobs")
            int elapsed = DateTime.Now.ToUnixTime() - message.Signal!.Nonce.ToDateTime().ToUnixTime();
            if (elapsed > 10)
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.Client!.IpAddress}' sent an expired message (was {elapsed - 10} seconds past expiry).", LogSeverity.Warning);
                handler.Close();
            }

            // Process and send response data
            SendData(handler, ProcessSignal(message.Signal, state.Client!));
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed to read data: {ex}", LogSeverity.Error);
        }
    }

    private static void SendCallback(IAsyncResult result)
    {
        const string LOG_IDENT = "Service::SendCallback";

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
        const string LOG_IDENT = "Service::SendData";

        try
        {
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed to send data: {ex}", LogSeverity.Error);
        }
    }

    private static byte[] ProcessSignal(Proto.Signal signal, Client client)
    {
        return new byte[1024];
    }
}