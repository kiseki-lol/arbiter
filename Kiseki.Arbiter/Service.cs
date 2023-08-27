namespace Kiseki.Arbiter;

using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Google.Protobuf;

public class Service
{
    private static readonly ManualResetEvent Finished = new(false);
    private static Socket? Listener;

    public static int Start()
    {
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
            catch (Exception e)
            {
                Log.Write($"Arbiter::Start failed on ListenForConnections: '{e}'", LogSeverity.Error);
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
        Client? client = null;
        Socket? handler = null;

        try
        {
            Finished.Set();

            Socket listener = (Socket)result.AsyncState!;
            handler = listener.EndAccept(result);
            client = new(handler);

            Log.Write($"[ArbiterService] '{client.IpAddress}' Connected on port {client.Port}", LogSeverity.Event);
        }
        finally
        {
            StateObject state = new()
            {
                WorkSocket = handler,
                Client = client
            };

            handler!.BeginReceive(state.Buffer, 0, 1024, 0, new(ReadCallback), state);
        }
    }

    private static void ReadCallback(IAsyncResult result)
    {
        StateObject state = (StateObject)result.AsyncState!;
        Socket handler = state.WorkSocket!;

        try
        {
            int read = handler.EndReceive(result);

            if (read > 0)
            {
                if (state.Buffer[0] != 0x02)
                {
                    Log.Write($"[ArbiterService::ReadCallback] '{state.Client!.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                    handler.Close();
                }

                if (read == 3)
                {
                    if (state.Size == 0)
                    {
                        ushort parsed = 0;

                        try
                        {
                            parsed = BitConverter.ToUInt16(state.Buffer, 1); // Offset by 1 (MSGREAD[STX])
                        }
                        catch
                        {
                            Log.Write($"[ArbiterService::ReadCallback] '{state.Client!.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                            handler.Close();
                        }
                        finally
                        {
                            if (parsed == 0)
                            {
                                Log.Write($"[ArbiterService::ReadCallback] '{state.Client!.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                                handler.Close();
                            }

                            state.Size = parsed;
                        }
                    }

                    handler.BeginReceive(state.Buffer, 0, state.Size - 3, 0, new AsyncCallback(ReadCallback), state); // state.Size - 3 for 3 bytes already read (MSGREAD[STX], MSGSIZE[UINT8])
                }

                // Parse given data
                if (!Message.TryParse(state.Buffer, out Message message))
                {
                    Log.Write($"[ArbiterService::ReadCallback] '{state.Client!.IpAddress}' - Not a TadahMessage", LogSeverity.Warning);
                    handler.Close();
                }

                // Verify signature
                if (!Verifier.Verify(message.Data!, message.Signature!))
                {
                    Log.Write($"[ArbiterService::ReadCallback] '{state.Client!.IpAddress}' - Bad or invalid signature", LogSeverity.Warning);
                    handler.Close();
                }

                // See if signal is older than ten seconds (so that people can't re-send signed destructive commands, such as "CloseAllJobs")
                if (DateTime.Now.ToUnixTime() + 10 > message.Signal!.Nonce.ToDateTime().ToUnixTime())
                {
                    Log.Write($"[ArbiterService::ReadCallback] '{state.Client!.IpAddress}' - Too old message received", LogSeverity.Warning);
                    handler.Close();
                }

                // Process data
                byte[] response = ProcessSignal(message.Signal, state.Client!);
                SendData(handler, response);
            }
        }
        catch (Exception exception)
        {
            Log.Write($"[ArbiterService::ReadCallback] - {exception}", LogSeverity.Error);
        }
    }

    private static void SendCallback(IAsyncResult result)
    {
        try
        {
            Socket handler = (Socket)result.AsyncState!;

            int bytesSent = handler.EndSend(result);
            handler.Shutdown(SocketShutdown.Both);
            handler.Close();
        }
        catch (Exception ex)
        {
            Log.Write($"[ArbiterService::SendCallback] {ex.Message}", LogSeverity.Error);
        }
    }

    private static void SendData(Socket handler, byte[] data)
    {
        try
        {
            handler.BeginSend(data, 0, data.Length, 0, new AsyncCallback(SendCallback), handler);
        }
        catch (Exception ex)
        {
            Log.Write($"[ArbiterService::SendData] {ex.Message}", LogSeverity.Error);
        }
    }

    private static byte[] SignalError(Proto.Signal signal, Client client, string reason)
    {
        byte[] stream = new byte[1024];

        try
        {
            Log.Write($"[ArbiterService::{client.IpAddress}] Tried '{Enum.GetName(signal.Operation)}' with '{signal.JobId}' - {reason}", LogSeverity.Warning);
        }
        finally
        {
            new Proto.Response
            {
                Operation = signal.Operation,
                Success = false,
                Message = reason
            }.WriteTo(stream);
        }

        return stream;
    }

    private static byte[] ProcessSignal(Proto.Signal signal, Client client)
    {
        byte[] stream = new byte[1024];
        bool success = false;
        object? responseData = null;

        if (success)
        {
            Proto.Response response = new()
            {
                Operation = signal.Operation,
                Success = true
            };

            if (responseData != null)
            {
                response.Data = (string)responseData;
            }

            response.WriteTo(stream);
        }
        else
        {
            // If it wasn't successful, the stream has already been written to
            // In the case of an exception or the "default" fallthrough in the switch statement.
        }

        return stream;
    }
}

internal class StateObject
{
    public byte[] Buffer = new byte[1024];
    public ushort Size = 0;
    public Socket? WorkSocket = null;
    public Client? Client;
}