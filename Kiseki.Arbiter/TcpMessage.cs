namespace Kiseki.Arbiter;

/**
 * TCP message format (from start to end):
 *
 * - Signal read (SOH, 0x01)
 * - Signal size (uint16, 2 bytes)
 * - Signal data (buffer)
 *
 * - Signature read (SOH, 0x01)
 * - Signature size (uint16, 2 bytes)
 * - Signature data (buffer)
 *
 * Notes:
 * - 6 total control bytes
 * - Minimum of 8 bytes for a message (6 control bytes + 2 buffers with a minimum of 1 byte)
 * - Annotated offsets are included for readability but are meaningless on their own -- see TcpMessage::TryParse
 * - An envelope contains header information (3 bytes, a SOH byte and a uint16 representing total size) and a message (TcpMessage). Envelopes are what the TcpServer always expects to receive
 */

using MessagePack;

public class TcpMessage
{    
    public Signal Signal { get; private set; }
    public byte[] Signature { get; private set; }
    public byte[] Raw { get; private set; }

    private const int MINIMUM_LENGTH         = 6 + 2;
    private const int SIGNAL_START_OFFSET    = 0;
    private const int SIGNAL_SIZE_OFFSET     = SIGNAL_START_OFFSET + 1;
    private const int SIGNAL_DATA_OFFSET     = SIGNAL_SIZE_OFFSET  + 2;
    private const int SIGNATURE_START_OFFSET = SIGNAL_DATA_OFFSET;            // SIGNAL_SIZE + this
    private const int SIGNATURE_SIZE_OFFSET  = SIGNATURE_START_OFFSET    + 1; // SIGNAL_SIZE + this
    private const int SIGNATURE_DATA_OFFSET  = SIGNATURE_SIZE_OFFSET     + 2; // SIGNAL_SIZE + this

    public TcpMessage(Signal signal, byte[] signature, byte[] raw)
    {
        Signal = signal;
        Signature = signature;
        Raw = raw;
    }

    public static bool TryParse(byte[] buffer, out TcpMessage? message)
    {
        const string LOG_IDENT = "TcpMessage::TryParse";

        message = null;

        ushort signalSize;
        byte[] signalData;
        Signal signal;

        ushort signatureSize;
        byte[] signatureData;

        try
        {
            if (buffer.Length < MINIMUM_LENGTH)
            {
                // Too tiny of a message
                Logger.Write(LOG_IDENT, $"Message is too small (expected at least {MINIMUM_LENGTH} byte(s), got {buffer.Length} byte(s)).", LogSeverity.Debug);

                return false;
            }

            if (buffer[SIGNAL_START_OFFSET] != 0x01)
            {
                // No signal start byte found
                Logger.Write(LOG_IDENT, $"No signal start byte found (expected 0x01, got 0x{buffer[SIGNAL_START_OFFSET]:X}).", LogSeverity.Debug);

                return false;
            }

            signalSize = BitConverter.ToUInt16(buffer, SIGNAL_SIZE_OFFSET);
            signalData = new byte[signalSize];
            Buffer.BlockCopy(buffer, SIGNAL_DATA_OFFSET, signalData, 0, signalSize);

            // Deserialize signal
            signal = MessagePackSerializer.Deserialize<Signal>(signalData)!;

            if (buffer[signalSize + SIGNATURE_START_OFFSET] != 0x01)
            {
                // No signature start byte found
                Logger.Write(LOG_IDENT, $"No signature start byte found (expected 0x01, got 0x{buffer[signalSize + SIGNATURE_START_OFFSET]:X}).", LogSeverity.Debug);

                return false;
            }

            signatureSize = BitConverter.ToUInt16(buffer, signalSize + SIGNATURE_SIZE_OFFSET);
            signatureData = new byte[signatureSize];
            Buffer.BlockCopy(buffer, signalSize + SIGNATURE_DATA_OFFSET, signatureData, 0, signatureSize);
        }
        catch
        {
            return false;
        }

        // Return message
        message = new TcpMessage(signal, signatureData, signalData);

        return true;
    }
}