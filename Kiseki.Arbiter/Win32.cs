namespace Kiseki.Arbiter;

public static class Win32
{
    // REF: https://stackoverflow.com/a/74472605

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private class MEMORYSTATUSEX
    {
        public uint dwLength;
        public uint dwMemoryLoad;
        public ulong ullTotalPhys;
        public ulong ullAvailPhys;
        public ulong ullTotalPageFile;
        public ulong ullAvailPageFile;
        public ulong ullTotalVirtual;
        public ulong ullAvailVirtual;
        public ulong ullAvailExtendedVirtual;

        public MEMORYSTATUSEX()
        {
            dwLength = (uint)Marshal.SizeOf(typeof(MEMORYSTATUSEX));
        }
    }

    [return: MarshalAs(UnmanagedType.Bool)]
    [DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
    private static extern bool GlobalMemoryStatusEx([In] [Out] MEMORYSTATUSEX lpBuffer);

    private static readonly object MemoryLock = new();
    private static readonly MEMORYSTATUSEX Status = new();

    public static void GetRamTotals(out int available, out int total)
    {
        lock (MemoryLock)
        {
            GlobalMemoryStatusEx(Status);

            available = (int)(Status.ullAvailPhys / 1024L / 1024L);
            total = (int)(Status.ullTotalPhys / 1024L / 1024L);
        }
    }
}