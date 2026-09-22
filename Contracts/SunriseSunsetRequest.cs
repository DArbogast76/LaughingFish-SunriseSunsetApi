namespace LaughingFish.SunriseSunsetApi.Contracts;

public sealed class SunriseSunsetRequest
{
    public double Lat { get; init; }
    public double Lon { get; init; }
    public DateOnly Date { get; init; }
    public string? TimeZone { get; init; }
}
