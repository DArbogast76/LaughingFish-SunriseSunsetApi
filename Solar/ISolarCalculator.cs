namespace LaughingFish.SunriseSunsetApi.Solar;

public interface ISolarCalculator
{
    SolarCalculationResult Calculate(
        DateOnly date,
        double latitudeDeg,
        double longitudeDeg,
        TimeSpan utcOffset);
}
