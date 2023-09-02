namespace Kiseki.Arbiter;

public class Message
{
    public byte[] Data { get; private set; }
    public Proto.Signal Signal { get; private set; }
    public byte[] Signature { get; private set; }

    public Message(byte[] data, Proto.Signal signal, byte[] signature)
    {
        Data = data;
        Signal = signal;
        Signature = signature;
    }

    public static bool TryParse(byte[] buffer, out Message? message)
    {
        message = null;

        /*
         * 0x02      0x00 0x00 0x00 0x00 0x02      0x00 0x00 .. ..     0x02      0x00 0x00 .. ..
         * (STX)     (UINT16)  (UINT16)  (STX)     (UINT16)  (DATA)    (STX)     (UINT16)  (DATA)
         * (MSGREAD) (MSGSIZE) (CHKSUM)  (SIGREAD) (SIGSIZE) (SIGDATA) (BUFREAD) (BUFSIZE) (BUFDATA)
         *
         * Key notes:
         * - MSGSIZE is the size of everything AFTER it (including CHKSUM.)
         * - SIGSIZE is more of a sanity check than anything -- its always expected to be 256.
         * - CHKSUM is the sum of all bytes after it. This means its a checksum of the signature and buffer.
         * - The buffer always will contain the signal in byte form (encoded via ProtoBuf.)
         * - The first 3 bytes (i.e. MSGREAD, MSGSIZE) are mostly to assist the arbiter's TCP service.
         */

        ushort msgSize;
        ushort chkSum;
        ushort sigSize;
        byte[] sigData;
        ushort bufSize;
        byte[] bufData;

        // Extract MSGREAD, MSGSIZE, and CHKSUM
        if (buffer[0] != 0x02 || buffer.Length < 11)
            return false;

        msgSize = BitConverter.ToUInt16(buffer, 1);
        chkSum = BitConverter.ToUInt16(buffer, 3);

        if (buffer.Length != msgSize || msgSize == 0)
            return false;

        ushort sum = 0;
        for (int i = 5; i < msgSize; i++)
            sum = unchecked((ushort)(sum + buffer[i]));

        if (sum != chkSum)
            return false;

        // Extract SIGREAD, SIGSIZE, and SIGDATA
        if (buffer[5] != 0x02)
            return false;

        sigSize = BitConverter.ToUInt16(buffer, 6);

        if (buffer.Length < 8 + sigSize || sigSize == 0)
            return false;

        sigData = new byte[sigSize];
        Buffer.BlockCopy(buffer, 8, sigData, 0, sigSize - 1);

        // Extract BUFREAD, BUFSIZE, and BUFDATA
        if (buffer.Length < 8 + sigSize + 3 || buffer[8 + sigSize + 1] != 0x02)
            return false;

        bufSize = BitConverter.ToUInt16(buffer, 8 + sigSize + 1);

        if (buffer.Length < 8 + sigSize + 3 + bufSize || bufSize == 0)
            return false;

        bufData = new byte[bufSize];
        Buffer.BlockCopy(buffer, 8 + sigSize + 3, bufData, 0, bufSize - 1);

        // Check signature length (should be always 256 bytes because the signature is always SHA256)
        if (sigSize != 256)
            return false;

        // Parse signal
        Proto.Signal signal;
        
        try
        {
            signal = Proto.Signal.Parser.ParseFrom(bufData);
        }
        catch
        {
            return false;
        }

        // Return message
        message = new Message(bufData, signal, sigData);

        return true;
    }
}