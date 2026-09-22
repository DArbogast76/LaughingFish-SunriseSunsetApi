namespace LaughingFish.SunriseSunsetApi.Services;

public interface ITimeZoneResolver
{
    ResolvedZone Resolve(DateOnly date, double longitudeDeg, string? timeZoneId);
}

public sealed class ResolvedZone
{
    public required string Id { get; init; }
    public required string Source { get; init; }
    public required TimeSpan Offset { get; init; }
    public TimeZoneInfo? TimeZone { get; init; }
}
