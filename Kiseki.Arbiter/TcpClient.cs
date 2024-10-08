namespace Kiseki.Arbiter;

public class TcpClient
{
    public string? IpAddress { get; private set; }
    public int? Port { get; private set; }
    public Socket Socket { get; private set; }

    public TcpClient(Socket socket)
    {
        Socket = socket;

        IPEndPoint? remote = socket.RemoteEndPoint as IPEndPoint;
        IPEndPoint? local = socket.LocalEndPoint as IPEndPoint;

        if (remote != null)
        {
            IpAddress = remote.Address.ToString();
            Port = remote.Port;
        }

        if (local != null)
        {
            IpAddress = local.Address.ToString();
            Port = local.Port;
        }

        if (local == null && remote == null)
        {
            throw new("Failed to resolve information from socket");
        }
    }
}