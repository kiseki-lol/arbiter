using Org.BouncyCastle.Asn1.Ocsp;

namespace Kiseki.Arbiter;

/**
 * TCP message format (from start to end):
 * 
 * - Message start (SOH, 0x01)
 * - Message size (uint16, 2 bytes)
 *
 * - Signature read (SOH, 0x01)
 * - Signature size (uint16, 2 bytes, expected to evaluate to 256)
 * - Signature data (buffer, expected to be 256 bytes)
 *
 * - Signal read (SOH, 0x01)
 * - Signal size (uint16, 2 bytes)
 * - Signal data (buffer)
 *
 * Notes:
 * - 9 total control bytes
 * - Minimum of 11 bytes for a message (9 control bytes + 2 buffers with a minimum of 1 byte)
 * - Annotated offsets are included for readability but are meaningless on their own. Please see TcpMessage::TryParse
 */

public class TcpMessage
{
    public Signal Signal { get; private set; }
    public byte[] Signature { get; private set; }
    public byte[] Raw { get; private set; }

    private const int MINIMUM_LENGTH = 11;
    private const int MESSAGE_START_OFFSET = 0;
    private const int MESSAGE_SIZE_OFFSET = 1;
    private const int SIGNATURE_START_OFFSET = 3;
    private const int SIGNATURE_SIZE_OFFSET = 4;
    private const int SIGNATURE_DATA_OFFSET = 6;
    private const int SIGNAL_START_OFFSET = 6;
    private const int SIGNAL_SIZE_OFFSET = 7;
    private const int SIGNAL_DATA_OFFSET = 9;

    public TcpMessage(Signal signal, byte[] signature, byte[] raw)
    {
        Signal = signal;
        Signature = signature;
        Raw = raw;
    }

    public static bool TryParse(byte[] buffer, out TcpMessage? message)
    {
        message = null;

        ushort messageSize;
        ushort signatureSize;
        ushort signalSize;
        byte[] signatureData;
        byte[] signalData;
        Signal signal;

        try
        {
            if (buffer[MESSAGE_START_OFFSET] != 0x01 || buffer.Length < MINIMUM_LENGTH)
            {
                // No message start byte found or otherwise malformed message
                return false;
            }

            messageSize = BitConverter.ToUInt16(buffer, MESSAGE_SIZE_OFFSET);
            
            if (buffer.Length != messageSize)
            {
                // Message size does not match buffer size (either malformed or corrupt message)
                return false;
            }

            if (buffer[SIGNATURE_START_OFFSET] != 0x01)
            {
                // No signature start byte found
                return false;
            }

            signatureSize = BitConverter.ToUInt16(buffer, SIGNATURE_SIZE_OFFSET);

            if (signatureSize != 256)
            {
                // Signature size does not match expected size for a SHA256 signature (either malformed or corrupt message)
                return false;
            }

            signatureData = new byte[signatureSize];
            Buffer.BlockCopy(buffer, SIGNATURE_DATA_OFFSET, signatureData, 0, signatureSize);

            if (buffer[signatureSize + SIGNAL_START_OFFSET] != 0x01)
            {
                // No signal start byte found
                return false;
            }

            signalSize = BitConverter.ToUInt16(buffer, signatureSize + SIGNAL_SIZE_OFFSET);
            signalData = new byte[signatureSize];
            Buffer.BlockCopy(buffer, signatureSize + SIGNAL_DATA_OFFSET, signalData, 0, signalSize);

            // Deserialize signal
            signal = JsonSerializer.Deserialize<Signal>(signalData)!;
        }
        catch
        {
            // Generally this means we tried to index out of bounds or failed deserialization
            return false;
        }

        // Return message
        message = new TcpMessage(signal, signatureData, signalData);

        return true;
    }
}