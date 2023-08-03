namespace Kiseki.Arbiter.Helpers;

public static class UnixTime
{
    public static int Now()
    {
        return (int) DateTime.UtcNow.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
    }

    public static int From(DateTime date)
    {
        return (int) date.Subtract(new DateTime(1970, 1, 1, 0, 0, 0)).TotalSeconds;
    }
}