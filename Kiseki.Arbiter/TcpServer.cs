namespace Kiseki.Arbiter;

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

            return -1;
        }

        try
        {
            IsListening = true;
            Task.Run(ListenForConnections);
        }
        catch (Exception ex)
        {
            Logger.Write(LOG_IDENT, $"Failed: {ex}", LogSeverity.Error);

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
        TcpClient? client = null;

        try
        {
            Finished.Set();

            Socket listener = (Socket)result.AsyncState!;
            
            handler = listener.EndAccept(result);
            client = new(handler);

            Logger.Write(LOG_IDENT, $"Machine '{client.IpAddress}' connected on port {client.Port}.", LogSeverity.Event);
        }
        catch (Exception ex)
        {
            if (IsListening)
            {
                Logger.Write(LOG_IDENT, $"Failed to accept connection: {ex}", LogSeverity.Error);
            }

            return;
        }

        TcpClientState state = new()
        {
            Socket = handler,
            TcpClient = client
        };

        handler!.BeginReceive(state.Buffer, 0, 1024, 0, new(ReadCallback), state);
    }

    private static void ReadCallback(IAsyncResult result)
    {
        const string LOG_IDENT = "TcpServer::ReadCallback";

        TcpClientState state = (TcpClientState)result.AsyncState!;
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
                Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' did not send a valid message (no message read byte found).", LogSeverity.Warning);
                handler.Close();
            }

            if (state.Size == 0)
            {
                // We're on the message read byte, so let's read the next 2 bytes which should give us the message size bytes.
                // Once we're able to read the message size, we'll read the entire message :-)

                state.Size = 3;
                handler.BeginReceive(state.Buffer, 0, 2, 0, new AsyncCallback(ReadCallback), state);

                return;
            }
            else if (state.Size == 3)
            {
                // Now we're on the two message size bytes.
                // Let's read the rest of the message according to what it tells us to read to.

                ushort size = 0;

                try
                {
                    size = BitConverter.ToUInt16(state.Buffer, 1); // Offset by 1 (message read byte)
                }
                catch
                {
                    Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' did not send a valid message (no explicit message size given).", LogSeverity.Warning);
                    handler.Close();

                    return;
                }

                state.Size = size;

                // We'll read up to state.Size - 3 bytes here since 3 bytes have already been read (MSGREAD and MSGSIZE)
                handler.BeginReceive(state.Buffer, 0, state.Size - 3, 0, new AsyncCallback(ReadCallback), state);

                return;
            }

            // If we've reached this far, we should now be able to parse this into a workable message.
            if (!TcpMessage.TryParse(state.Buffer, out TcpMessage? message))
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' did not send a valid message (failed to parse).", LogSeverity.Warning);
                handler.Close();
                
                return;
            }

            // Verify signature
            if (!Verifier.Verify(message!.Raw!, message.Signature!))
            {
                Logger.Write(LOG_IDENT, $"Machine '{state.TcpClient!.IpAddress}' sent a message with a bad or malformed signature.", LogSeverity.Warning);
                handler.Close();

                return;
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

    private static byte[] ProcessSignal(Signal signal, TcpClient TcpClient)
    {
        const string LOG_IDENT = "TcpServer::ProcessSignal";

        Logger.Write(LOG_IDENT, $"Received command '{signal.Command.ToString()}' from machine '{TcpClient.IpAddress}'!", LogSeverity.Debug);

        return new byte[] { 0xfa, 0xca, 0xde };
    }
}