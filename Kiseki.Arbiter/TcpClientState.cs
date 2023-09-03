namespace Kiseki.Arbiter;

public class TcpClientState
{
    public byte[] Buffer = new byte[1024]; // Max 1 MiB of data
    public ushort Size = 0;
    public Socket? Socket = null;
    public TcpClient? TcpClient;
}