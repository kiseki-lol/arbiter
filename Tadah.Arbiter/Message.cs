using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tadah
{
    public class Message
    {
        public byte[] Data { get; set; }
        public Proto.Signal Signal { get; set; }
        public byte[] Signature { get; set; }

        public Message(byte[] data, Proto.Signal signal, byte[] signature)
        {
            this.Data = data;
            this.Signal = signal;
            this.Signature = signature;
        }

        public static bool TryParse(byte[] buffer, out Message message)
        {
            message = new Message(null, null, null);

            /*
             * 0x02      0x00 0x00 0x00 0x00 0x02      0x00 0x00 .. ..     0x02      0x00 0x00 .. ..
             * (STX)     (UINT16)  (UINT16)  (STX)     (UINT16)  (DATA)    (STX)     (UINT16)  (DATA)
             * (MSGREAD) (MSGSIZE) (CHKSUM)  (SIGREAD) (SIGSIZE) (SIGDATA) (BUFREAD) (BUFSIZE) (BUFDATA)
             */

            ushort msgSize;
            ushort chkSum;
            ushort sigSize;
            byte[] sigData;
            ushort bufSize;
            byte[] bufData;

            // Extract MSG* and CHKSUM
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

            // Extract SIG*
            if (buffer[5] != 0x02)
                return false;

            sigSize = BitConverter.ToUInt16(buffer, 6);

            if (buffer.Length < 8 + sigSize || sigSize == 0)
                return false;

            sigData = new byte[sigSize];
            Buffer.BlockCopy(buffer, 8, sigData, 0, sigSize - 1);

            // Extract BUF*
            if (buffer.Length < 8 + sigSize + 3 || buffer[8 + sigSize + 1] != 0x02)
                return false;

            bufSize = BitConverter.ToUInt16(buffer, 8 + sigSize + 1);

            if (buffer.Length < 8 + sigSize + 3 + bufSize || bufSize == 0)
                return false;

            bufData = new byte[bufSize];
            Buffer.BlockCopy(buffer, 8 + sigSize + 3, bufData, 0, bufSize - 1);

            // Check signature length (should be 256 bytes; is SHA256)
            if (sigSize != 256)
                return false;

            // Convert signal
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
}
