namespace LaughingFish.SunriseSunsetApi.Solar;

/// <summary>
/// NOAA Solar Calculator (GML / Almanac for Computers style).
/// Official rise/set uses zenith 90.833° (refraction + solar radius).
/// No network lookup.
/// </summary>
public sealed class NoaaSolarCalculator : ISolarCalculator
{
    public const double OfficialZenithDeg = 90.833;
    public const double CivilZenithDeg = 96.0;
    public const double NauticalZenithDeg = 102.0;
    public const double AstronomicalZenithDeg = 108.0;

    private const double NearLimitEpsilon = 0.002;

    public SolarCalculationResult Calculate(
        DateOnly date,
        double latitudeDeg,
        double longitudeDeg,
        TimeSpan utcOffset)
    {
        var jd = JulianDay(date.Year, date.Month, date.Day);
        var t = JulianCentury(jd);
        var declination = SunDeclinationDeg(t);
        var equationOfTime = EquationOfTimeMinutes(t);

        var solarNoonUtcMinutes = 720.0 - (4.0 * longitudeDeg) - equationOfTime;
        var solarNoonUtc = DateFromUtcMinutes(date, solarNoonUtcMinutes);

        var official = HourAngle(latitudeDeg, declination, OfficialZenithDeg);
        var civil = HourAngle(latitudeDeg, declination, CivilZenithDeg);
        var nautical = HourAngle(latitudeDeg, declination, NauticalZenithDeg);
        var astronomical = HourAngle(latitudeDeg, declination, AstronomicalZenithDeg);

        var condition = official.Kind switch
        {
            HourAngleKind.NeverSets or HourAngleKind.NearNeverSets => SolarCondition.PolarDay,
            HourAngleKind.NeverRises or HourAngleKind.NearNeverRises => SolarCondition.PolarNight,
            _ => SolarCondition.RiseAndSet
        };

        var nearHorizon = official.Kind is HourAngleKind.NearRiseSet
            or HourAngleKind.NearNeverSets
            or HourAngleKind.NearNeverRises;
        var confidence = nearHorizon ? "low" : "standard";
        string? confidenceNote = nearHorizon
            ? "Sun path is within a few arcminutes of the official horizon. Refraction can flip rise/set on this date."
            : null;

        DateTimeOffset? sunrise = null;
        DateTimeOffset? sunset = null;
        if (official.Kind is HourAngleKind.Ok or HourAngleKind.NearRiseSet)
        {
            var haMinutes = 4.0 * official.HourAngleDeg;
            sunrise = RefineEvent(date, latitudeDeg, longitudeDeg, OfficialZenithDeg, rising: true, solarNoonUtcMinutes - haMinutes);
            sunset = RefineEvent(date, latitudeDeg, longitudeDeg, OfficialZenithDeg, rising: false, solarNoonUtcMinutes + haMinutes);
        }

        var dayLengthSeconds = condition switch
        {
            SolarCondition.PolarDay => 24 * 60 * 60,
            SolarCondition.PolarNight => 0,
            _ when sunrise is not null && sunset is not null =>
                Math.Max(0, (int)Math.Round((sunset.Value - sunrise.Value).TotalSeconds)),
            _ => 0
        };

        return new SolarCalculationResult
        {
            Condition = condition,
            Confidence = confidence,
            ConfidenceNote = confidenceNote,
            DeclinationDeg = Round4(declination),
            EquationOfTimeMinutes = Round4(equationOfTime),
            OfficialZenithDeg = OfficialZenithDeg,
            SolarNoonUtc = solarNoonUtc,
            SunriseUtc = sunrise,
            SunsetUtc = sunset,
            CivilDawnUtc = EventIfKnown(date, latitudeDeg, longitudeDeg, CivilZenithDeg, rising: true, civil, solarNoonUtcMinutes),
            CivilDuskUtc = EventIfKnown(date, latitudeDeg, longitudeDeg, CivilZenithDeg, rising: false, civil, solarNoonUtcMinutes),
            NauticalDawnUtc = EventIfKnown(date, latitudeDeg, longitudeDeg, NauticalZenithDeg, rising: true, nautical, solarNoonUtcMinutes),
            NauticalDuskUtc = EventIfKnown(date, latitudeDeg, longitudeDeg, NauticalZenithDeg, rising: false, nautical, solarNoonUtcMinutes),
            AstronomicalDawnUtc = EventIfKnown(date, latitudeDeg, longitudeDeg, AstronomicalZenithDeg, rising: true, astronomical, solarNoonUtcMinutes),
            AstronomicalDuskUtc = EventIfKnown(date, latitudeDeg, longitudeDeg, AstronomicalZenithDeg, rising: false, astronomical, solarNoonUtcMinutes),
            DayLengthSeconds = dayLengthSeconds,
            UtcOffset = utcOffset
        };
    }

    private static DateTimeOffset? EventIfKnown(
        DateOnly date,
        double latitudeDeg,
        double longitudeDeg,
        double zenithDeg,
        bool rising,
        HourAngleResult ha,
        double solarNoonUtcMinutes)
    {
        if (ha.Kind is not HourAngleKind.Ok and not HourAngleKind.NearRiseSet)
        {
            return null;
        }

        var offsetMinutes = 4.0 * ha.HourAngleDeg;
        var firstGuess = rising ? solarNoonUtcMinutes - offsetMinutes : solarNoonUtcMinutes + offsetMinutes;
        return RefineEvent(date, latitudeDeg, longitudeDeg, zenithDeg, rising, firstGuess);
    }

    private static DateTimeOffset RefineEvent(
        DateOnly date,
        double latitudeDeg,
        double longitudeDeg,
        double zenithDeg,
        bool rising,
        double firstGuessUtcMinutes)
    {
        var jd = JulianDay(date.Year, date.Month, date.Day) + (firstGuessUtcMinutes / 1440.0);
        var t = JulianCentury(jd);
        var declination = SunDeclinationDeg(t);
        var equationOfTime = EquationOfTimeMinutes(t);
        var ha = HourAngle(latitudeDeg, declination, zenithDeg);
        var solarNoon = 720.0 - (4.0 * longitudeDeg) - equationOfTime;

        if (ha.Kind is not HourAngleKind.Ok and not HourAngleKind.NearRiseSet)
        {
            return DateFromUtcMinutes(date, firstGuessUtcMinutes);
        }

        var refined = rising
            ? solarNoon - (4.0 * ha.HourAngleDeg)
            : solarNoon + (4.0 * ha.HourAngleDeg);

        return DateFromUtcMinutes(date, refined);
    }

    private static HourAngleResult HourAngle(double latitudeDeg, double declinationDeg, double zenithDeg)
    {
        var latRad = DegToRad(latitudeDeg);
        var decRad = DegToRad(declinationDeg);
        var cosLat = Math.Cos(latRad);
        var cosDec = Math.Cos(decRad);

        if (Math.Abs(cosLat * cosDec) < 1e-12)
        {
            var altitudeAtNoon = 90.0 - Math.Abs(latitudeDeg - declinationDeg);
            return altitudeAtNoon >= (90.0 - zenithDeg)
                ? new HourAngleResult(HourAngleKind.NeverSets, 180)
                : new HourAngleResult(HourAngleKind.NeverRises, 0);
        }

        var cosHa = (Math.Cos(DegToRad(zenithDeg)) / (cosLat * cosDec)) - (Math.Tan(latRad) * Math.Tan(decRad));

        if (cosHa < -1.0)
        {
            var kind = Math.Abs(cosHa + 1.0) < NearLimitEpsilon ? HourAngleKind.NearNeverSets : HourAngleKind.NeverSets;
            return new HourAngleResult(kind, 180);
        }

        if (cosHa > 1.0)
        {
            var kind = Math.Abs(cosHa - 1.0) < NearLimitEpsilon ? HourAngleKind.NearNeverRises : HourAngleKind.NeverRises;
            return new HourAngleResult(kind, 0);
        }

        var kindOk = Math.Abs(Math.Abs(cosHa) - 1.0) < NearLimitEpsilon
            ? HourAngleKind.NearRiseSet
            : HourAngleKind.Ok;

        return new HourAngleResult(kindOk, RadToDeg(Math.Acos(cosHa)));
    }

    private static DateTimeOffset DateFromUtcMinutes(DateOnly date, double utcMinutes)
    {
        var midnight = new DateTimeOffset(date.Year, date.Month, date.Day, 0, 0, 0, TimeSpan.Zero);
        var roundedMs = (long)Math.Round(utcMinutes * 60.0 * 1000.0, MidpointRounding.AwayFromZero);
        return midnight.AddMilliseconds(roundedMs);
    }

    private static double JulianDay(int year, int month, int day)
    {
        if (month <= 2)
        {
            year -= 1;
            month += 12;
        }

        var a = Math.Floor(year / 100.0);
        var b = 2 - a + Math.Floor(a / 4.0);
        return Math.Floor(365.25 * (year + 4716))
            + Math.Floor(30.6001 * (month + 1))
            + day
            + b
            - 1524.5;
    }

    private static double JulianCentury(double jd) => (jd - 2451545.0) / 36525.0;

    private static double GeomMeanLongSunDeg(double t)
    {
        var l0 = 280.46646 + (t * (36000.76983 + (0.0003032 * t)));
        l0 %= 360.0;
        if (l0 < 0)
        {
            l0 += 360.0;
        }

        return l0;
    }

    private static double GeomMeanAnomalySunDeg(double t) =>
        357.52911 + (t * (35999.05029 - (0.0001537 * t)));

    private static double EccentricityEarthOrbit(double t) =>
        0.016708634 - (t * (0.000042037 + (0.0000001267 * t)));

    private static double SunEqOfCenterDeg(double t)
    {
        var mRad = DegToRad(GeomMeanAnomalySunDeg(t));
        return (Math.Sin(mRad) * (1.914602 - (t * (0.004817 + (0.000014 * t)))))
            + (Math.Sin(2 * mRad) * (0.019993 - (0.000101 * t)))
            + (Math.Sin(3 * mRad) * 0.000289);
    }

    private static double SunTrueLongDeg(double t) => GeomMeanLongSunDeg(t) + SunEqOfCenterDeg(t);

    private static double SunApparentLongDeg(double t)
    {
        var omega = 125.04 - (1934.136 * t);
        return SunTrueLongDeg(t) - 0.00569 - (0.00478 * Math.Sin(DegToRad(omega)));
    }

    private static double MeanObliquityOfEclipticDeg(double t)
    {
        var seconds = 21.448 - (t * (46.8150 + (t * (0.00059 - (t * 0.000576)))));
        return 23.0 + ((26.0 + (seconds / 60.0)) / 60.0);
    }

    private static double ObliquityCorrectionDeg(double t)
    {
        var omega = 125.04 - (1934.136 * t);
        return MeanObliquityOfEclipticDeg(t) + (0.00256 * Math.Cos(DegToRad(omega)));
    }

    private static double SunDeclinationDeg(double t)
    {
        var sint = Math.Sin(DegToRad(ObliquityCorrectionDeg(t))) * Math.Sin(DegToRad(SunApparentLongDeg(t)));
        return RadToDeg(Math.Asin(sint));
    }

    private static double EquationOfTimeMinutes(double t)
    {
        var epsilonRad = DegToRad(ObliquityCorrectionDeg(t));
        var l0Rad = DegToRad(GeomMeanLongSunDeg(t));
        var e = EccentricityEarthOrbit(t);
        var mRad = DegToRad(GeomMeanAnomalySunDeg(t));
        var y = Math.Tan(epsilonRad / 2.0);
        y *= y;

        var eq = (y * Math.Sin(2.0 * l0Rad))
            - (2.0 * e * Math.Sin(mRad))
            + (4.0 * e * y * Math.Sin(mRad) * Math.Cos(2.0 * l0Rad))
            - (0.5 * y * y * Math.Sin(4.0 * l0Rad))
            - (1.25 * e * e * Math.Sin(2.0 * mRad));

        return RadToDeg(eq) * 4.0;
    }

    private static double DegToRad(double deg) => Math.PI / 180.0 * deg;

    private static double RadToDeg(double rad) => 180.0 / Math.PI * rad;

    private static double Round4(double value) => Math.Round(value, 4, MidpointRounding.AwayFromZero);

    private readonly record struct HourAngleResult(HourAngleKind Kind, double HourAngleDeg);

    private enum HourAngleKind
    {
        Ok,
        NearRiseSet,
        NearNeverRises,
        NearNeverSets,
        NeverRises,
        NeverSets
    }
}
