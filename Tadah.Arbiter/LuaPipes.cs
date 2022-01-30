using System;
using System.IO;
using System.IO.Pipes;
using System.Runtime.InteropServices;
using System.Text;

namespace Tadah.Arbiter
{
    public static class LuaPipes
    {
        [DllImport("kernel32.dll", SetLastError = true)]
        private static extern bool WaitNamedPipe(string lpNamedPipeName, int timeout);

        public static bool Exists(string pipeName)
        {
            try
            {
                if (!WaitNamedPipe(Path.GetFullPath(string.Format("\\\\.\\pipe\\{0}", pipeName)), 0))
                {
                    int lastWin32Error = Marshal.GetLastWin32Error();

                    if (lastWin32Error == 0 || lastWin32Error == 2)
                    {
                        return false;
                    }
                }
            }
            catch
            {
                return false;
            }

            return true;
        }

        private static byte[] ReadMessage(PipeStream pipe)
        {
            byte[] buffer = new byte[1024];
            using (var stream = new MemoryStream())
            {
                while (!pipe.IsMessageComplete)
                {
                    stream.Write(buffer, 0, pipe.Read(buffer, 0, buffer.Length));
                }

                return stream.ToArray();
            }
        }

        public static string Send(string pipe, string input)
        {
            if (Exists(pipe))
            {
                try
                {
                    using (NamedPipeClientStream stream = new NamedPipeClientStream(".", pipe, PipeDirection.InOut))
                    {
                        stream.Connect();
                        stream.ReadMode = PipeTransmissionMode.Message;

                        byte[] inputBytes = Encoding.Unicode.GetBytes(input);
                        stream.Write(inputBytes, 0, inputBytes.Length);

                        string result = Encoding.UTF8.GetString(ReadMessage(stream));
                        stream.Close();

                        return result;
                    }
                }
                catch
                {
                    return String.Empty;
                }
            }

            return String.Empty;
        }
    }
}
