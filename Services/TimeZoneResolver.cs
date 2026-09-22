using TimeZoneConverter;

namespace LaughingFish.SunriseSunsetApi.Services;

public sealed class TimeZoneResolver : ITimeZoneResolver
{
    public ResolvedZone Resolve(DateOnly date, double longitudeDeg, string? timeZoneId)
    {
        if (!string.IsNullOrWhiteSpace(timeZoneId) && TZConvert.TryGetTimeZoneInfo(timeZoneId.Trim(), out var zone))
        {
            var localNoon = new DateTime(date.Year, date.Month, date.Day, 12, 0, 0, DateTimeKind.Unspecified);
            var offset = zone.GetUtcOffset(localNoon);
            return new ResolvedZone
            {
                Id = timeZoneId.Trim(),
                Source = "request",
                Offset = offset,
                TimeZone = zone
            };
        }

        var hours = (int)Math.Round(longitudeDeg / 15.0, MidpointRounding.AwayFromZero);
        if (hours < -12)
        {
            hours = -12;
        }
        else if (hours > 14)
        {
            hours = 14;
        }

        return new ResolvedZone
        {
            Id = $"UTC{FormatOffset(TimeSpan.FromHours(hours))}",
            Source = "longitude",
            Offset = TimeSpan.FromHours(hours),
            TimeZone = null
        };
    }

    public static string FormatOffset(TimeSpan offset)
    {
        var sign = offset < TimeSpan.Zero ? "-" : "+";
        var abs = offset.Duration();
        return $"{sign}{abs.Hours:00}:{abs.Minutes:00}";
    }
}
