namespace Kiseki.Arbiter;

public class TcpClientState
{
    public byte[] Buffer = new byte[ushort.MaxValue]; // 64KiB max
    public ushort Size = 0;
    public Socket? Socket = null;
    public TcpClient? TcpClient;
}