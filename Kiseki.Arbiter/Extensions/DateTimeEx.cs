namespace Kiseki.Arbiter;

public static class DateTimeEx
{
    public static int ToUnixTime(this DateTime time)
    {
        return (int)time.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
    }

    public static string ToFileName(this DateTime time)
    {
        return time.ToString("o").Replace(":", "_");
    }

    public static string ToUniversalIso8601(this DateTime time)
    {
        return time.ToUniversalTime().ToString("u").Replace(" ", "T");
    }
}