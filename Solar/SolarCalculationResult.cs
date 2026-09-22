namespace LaughingFish.SunriseSunsetApi.Solar;

public sealed class SolarCalculationResult
{
    public required SolarCondition Condition { get; init; }
    public required string Confidence { get; init; }
    public string? ConfidenceNote { get; init; }
    public required double DeclinationDeg { get; init; }
    public required double EquationOfTimeMinutes { get; init; }
    public required double OfficialZenithDeg { get; init; }
    public required DateTimeOffset SolarNoonUtc { get; init; }
    public DateTimeOffset? SunriseUtc { get; init; }
    public DateTimeOffset? SunsetUtc { get; init; }
    public DateTimeOffset? CivilDawnUtc { get; init; }
    public DateTimeOffset? CivilDuskUtc { get; init; }
    public DateTimeOffset? NauticalDawnUtc { get; init; }
    public DateTimeOffset? NauticalDuskUtc { get; init; }
    public DateTimeOffset? AstronomicalDawnUtc { get; init; }
    public DateTimeOffset? AstronomicalDuskUtc { get; init; }
    public required int DayLengthSeconds { get; init; }
    public required TimeSpan UtcOffset { get; init; }
}
