using LaughingFish.SunriseSunsetApi.Contracts;
using LaughingFish.SunriseSunsetApi.Solar;
using Microsoft.Extensions.Logging;

namespace LaughingFish.SunriseSunsetApi.Services;

public sealed class SunriseSunsetQuery : ISunriseSunsetQuery
{
    private readonly ILogger<SunriseSunsetQuery> _logger;
    private readonly ISolarCalculator _calculator;
    private readonly ITimeZoneResolver _timeZones;

    public SunriseSunsetQuery(
        ILogger<SunriseSunsetQuery> logger,
        ISolarCalculator calculator,
        ITimeZoneResolver timeZones)
    {
        _logger = logger;
        _calculator = calculator;
        _timeZones = timeZones;
    }

    public SunriseSunsetResponse Execute(string invocationId, SunriseSunsetRequest request)
    {
        var zone = _timeZones.Resolve(request.Date, request.Lon, request.TimeZone);

        _logger.LogInformation(
            "SunriseSunset timezone resolved. InvocationId={InvocationId} TimeZoneId={TimeZoneId} Source={Source} Offset={Offset}",
            invocationId,
            zone.Id,
            zone.Source,
            TimeZoneResolver.FormatOffset(zone.Offset));

        var calculated = _calculator.Calculate(request.Date, request.Lat, request.Lon, zone.Offset);

        _logger.LogInformation(
            "SunriseSunset calculated. InvocationId={InvocationId} Condition={Condition} Confidence={Confidence} SunriseUtc={SunriseUtc} SunsetUtc={SunsetUtc} SolarNoonUtc={SolarNoonUtc} DayLengthSeconds={DayLengthSeconds} DeclinationDeg={DeclinationDeg}",
            invocationId,
            calculated.Condition,
            calculated.Confidence,
            calculated.SunriseUtc,
            calculated.SunsetUtc,
            calculated.SolarNoonUtc,
            calculated.DayLengthSeconds,
            calculated.DeclinationDeg);

        return new SunriseSunsetResponse
        {
            Location = new SunriseSunsetLocation
            {
                Lat = request.Lat,
                Lon = request.Lon
            },
            Date = request.Date.ToString("yyyy-MM-dd"),
            TimeZone = new ResolvedTimeZone
            {
                Id = zone.Id,
                Offset = TimeZoneResolver.FormatOffset(zone.Offset),
                Source = zone.Source
            },
            Condition = ToConditionName(calculated.Condition),
            Confidence = calculated.Confidence,
            ConfidenceNote = calculated.ConfidenceNote,
            Events = new SolarEvents
            {
                AstronomicalDawn = Instant(calculated.AstronomicalDawnUtc, zone),
                NauticalDawn = Instant(calculated.NauticalDawnUtc, zone),
                CivilDawn = Instant(calculated.CivilDawnUtc, zone),
                Sunrise = Instant(calculated.SunriseUtc, zone),
                SolarNoon = Instant(calculated.SolarNoonUtc, zone),
                Sunset = Instant(calculated.SunsetUtc, zone),
                CivilDusk = Instant(calculated.CivilDuskUtc, zone),
                NauticalDusk = Instant(calculated.NauticalDuskUtc, zone),
                AstronomicalDusk = Instant(calculated.AstronomicalDuskUtc, zone)
            },
            DayLength = new DayLength
            {
                Seconds = calculated.DayLengthSeconds,
                Hours = Math.Round(calculated.DayLengthSeconds / 3600.0, 4, MidpointRounding.AwayFromZero)
            },
            Sun = new SunGeometry
            {
                DeclinationDeg = calculated.DeclinationDeg,
                EquationOfTimeMinutes = calculated.EquationOfTimeMinutes,
                OfficialZenithDeg = calculated.OfficialZenithDeg
            }
        };
    }

    private static string ToConditionName(SolarCondition condition) =>
        condition switch
        {
            SolarCondition.PolarDay => "polarDay",
            SolarCondition.PolarNight => "polarNight",
            _ => "riseAndSet"
        };

    private static SolarInstant? Instant(DateTimeOffset? utc, ResolvedZone zone)
    {
        if (utc is null)
        {
            return null;
        }

        var local = zone.TimeZone is null
            ? utc.Value.ToOffset(zone.Offset)
            : TimeZoneInfo.ConvertTime(utc.Value, zone.TimeZone);

        return new SolarInstant
        {
            Utc = utc.Value.ToUniversalTime().ToString("o"),
            Local = local.ToString("yyyy-MM-ddTHH:mm:sszzz")
        };
    }
}
