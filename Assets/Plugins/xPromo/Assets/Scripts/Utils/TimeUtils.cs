using System;

public static class TimeUtils {
    public static readonly DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
    public static long DateToUnixTime(DateTime date) {
        return (long)(date.Subtract(epoch)).TotalSeconds;
    }
    public static DateTime UnixTimeToDate(long unixTime) {
        return epoch.AddSeconds(unixTime);
    }
}