using System;

namespace Kiseki.Arbiter
{
    public class Helpers
    {
        public static int UnixTime()
        {
            return (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }

        public static int UnixTime(DateTime date)
        {
            return (int) date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
        }
    }
}