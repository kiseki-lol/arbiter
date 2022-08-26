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
             * TadahMessage format:
             * 
             * 0x02      0x00 0x00 0x02      0x00 0x00 .. ..     0x02      0x00 0x00 .. ..
             * (STX)     (UINT16)  (STX)     (UINT16)  (DATA)    (STX)     (UINT16)  (DATA)
             * (MSGREAD) (MSGSIZE) (SIGREAD) (SIGSIZE) (SIGDATA) (BUFREAD) (BUFSIZE) (BUFDATA)
             */

            // Check if byte 1 is MSGREAD
            if (buffer[0] != 0x02)
            {
                return false;
            }
            
            // Check if buffer is capable of supporting MSGREAD + MSGSIZE
            if (buffer.Length < 1 + 2)
            {
                return false;
            }

            // Parse for MSGSIZE
            ushort msgSize;

            try
            {
                msgSize = BitConverter.ToUInt16(buffer, 1); // offset for MSGREAD
            }
            catch
            {
                return false;
            }

            // Check if buffer is corrupt (msgSize == buffer.Length)
            if (buffer.Length != msgSize)
            {
                return false;
            }

            // Check if byte 4 is SIGREAD
            if (buffer[4] != 0x02)
            {
                return false;
            }

            // Check if buffer is capable of supporting MSGREAD + MSGSIZE + SIGREAD + SIGSIZE
            if (buffer.Length < 1 + 2 + 1 + 2)
            {
                return false;
            }

            ushort sigSize;

            try
            {
                sigSize = BitConverter.ToUInt16(buffer, 1 + 2 + 1 + 2); // offset for MSGREAD + MSGSIZE + SIGREAD + SIGSIZE
            }
            catch
            {
                return false;
            }

            // Check if buffer is capable of having a signature given the size (6 bytes for MSGREAD + MSGSIZE + SIGREAD + SIGSIZE)
            if (buffer.Length < 1 + 2 + 1 + 2)
            {
                return false;
            }

            // Extract SIGDATA
            byte[] sigData = new byte[sigSize];
            Buffer.BlockCopy(buffer, 6, sigData, 0, sigSize);

            // Check if buffer is capable of supporting MSGREAD + MSGSIZE + SIGREAD + SIGSIZE + SIGDATA + BUFREAD + BUFSIZE
            if (buffer.Length < 1 + 2 + 1 + 2 + sigSize + 1 + 2)
            {
                return false;
            }

            if (buffer[1 + 2 + 1 + 2 + sigSize + 1] != 0x02)
            {
                return false;
            }

            // Get BUFSIZE
            ushort bufSize;

            try
            {
                bufSize = BitConverter.ToUInt16(buffer, 1 + 2 + 1 + 2 + sigSize + 1); // offset for MSGREAD + MSGSIZE + SIGREAD + SIGSIZE + SIGDATA + BUFREAD
            }
            catch
            {
                return false;
            }

            // Can buffer support bufSize
            if (buffer.Length < 1 + 2 + 1 + 2 + sigSize + 1 + 2 + bufSize)
            {
                return false;
            }

            // Extract BUFDATA
            byte[] bufData = new byte[bufSize];
            Buffer.BlockCopy(buffer, 1 + 2 + 1 + 2 + sigSize + 1 + 2, bufData, 0, bufSize);

            // Convert signal
            Proto.Signal signal = null;

            try
            {
                signal = Proto.Signal.Parser.ParseFrom(bufData);
            }
            catch
            {
                return false;
            }

            // The signature should be 256 bytes for sha256
            if (sigSize != 256)
            {
                return false;
            }

            // And send
            message = new Message(bufData, signal, sigData);

            return true;
        }
    }
}
