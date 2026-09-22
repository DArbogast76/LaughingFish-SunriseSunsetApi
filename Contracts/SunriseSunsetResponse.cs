namespace LaughingFish.SunriseSunsetApi.Contracts;

public sealed class SunriseSunsetResponse
{
    public string Schema { get; init; } = "laughingfish.sunriseSunset.v1";
    public required SunriseSunsetLocation Location { get; init; }
    public required string Date { get; init; }
    public required ResolvedTimeZone TimeZone { get; init; }
    public required string Condition { get; init; }
    public required string Confidence { get; init; }
    public string? ConfidenceNote { get; init; }
    public required SolarEvents Events { get; init; }
    public required DayLength DayLength { get; init; }
    public required SunGeometry Sun { get; init; }
}

public sealed class SunriseSunsetLocation
{
    public required double Lat { get; init; }
    public required double Lon { get; init; }
}

public sealed class ResolvedTimeZone
{
    public required string Id { get; init; }
    public required string Offset { get; init; }
    public required string Source { get; init; }
}

public sealed class SolarEvents
{
    public SolarInstant? AstronomicalDawn { get; init; }
    public SolarInstant? NauticalDawn { get; init; }
    public SolarInstant? CivilDawn { get; init; }
    public SolarInstant? Sunrise { get; init; }
    public SolarInstant? SolarNoon { get; init; }
    public SolarInstant? Sunset { get; init; }
    public SolarInstant? CivilDusk { get; init; }
    public SolarInstant? NauticalDusk { get; init; }
    public SolarInstant? AstronomicalDusk { get; init; }
}

public sealed class SolarInstant
{
    public required string Utc { get; init; }
    public required string Local { get; init; }
}

public sealed class DayLength
{
    public required int Seconds { get; init; }
    public required double Hours { get; init; }
}

public sealed class SunGeometry
{
    public required double DeclinationDeg { get; init; }
    public required double EquationOfTimeMinutes { get; init; }
    public required double OfficialZenithDeg { get; init; }
}
