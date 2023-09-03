namespace Kiseki.Arbiter;

public class TcpClientState
{
    public byte[] Buffer = new byte[16384]; // 16KiB max
    public ushort Size = 0;
    public Socket? Socket = null;
    public TcpClient? TcpClient;
}