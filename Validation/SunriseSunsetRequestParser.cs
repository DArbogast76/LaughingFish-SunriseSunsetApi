using System.Globalization;
using System.Text.Json;
using LaughingFish.SunriseSunsetApi.Contracts;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using TimeZoneConverter;

namespace LaughingFish.SunriseSunsetApi.Validation;

public interface ISunriseSunsetRequestParser
{
    bool TryParse(
        HttpRequest req,
        string invocationId,
        out SunriseSunsetRequest? request,
        out string errorCode,
        out string errorMessage);
}

public sealed class SunriseSunsetRequestParser : ISunriseSunsetRequestParser
{
    private readonly ILogger<SunriseSunsetRequestParser> _logger;

    public SunriseSunsetRequestParser(ILogger<SunriseSunsetRequestParser> logger)
    {
        _logger = logger;
    }

    public bool TryParse(
        HttpRequest req,
        string invocationId,
        out SunriseSunsetRequest? request,
        out string errorCode,
        out string errorMessage)
    {
        request = null;
        errorCode = "invalid_request";
        errorMessage = "lat, lon, and date are required.";

        string? latRaw;
        string? lonRaw;
        string? dateRaw;
        string? timeZoneRaw;

        if (HasQueryCoordinates(req))
        {
            latRaw = req.Query["lat"].ToString();
            lonRaw = req.Query["lon"].ToString();
            dateRaw = req.Query["date"].ToString();
            timeZoneRaw = req.Query["timeZone"].ToString();

            _logger.LogInformation(
                "Request parsed from query. InvocationId={InvocationId} LatRaw={LatRaw} LonRaw={LonRaw} DateRaw={DateRaw} TimeZoneRaw={TimeZoneRaw}",
                invocationId,
                latRaw,
                lonRaw,
                dateRaw,
                timeZoneRaw);
        }
        else if (HttpMethods.IsPost(req.Method) && req.ContentLength is > 0)
        {
            using var reader = new StreamReader(req.Body);
            var body = reader.ReadToEnd();

            _logger.LogInformation(
                "Request parsed from body. InvocationId={InvocationId} BodyLength={BodyLength}",
                invocationId,
                body.Length);

            try
            {
                using var document = JsonDocument.Parse(string.IsNullOrWhiteSpace(body) ? "{}" : body);
                var root = document.RootElement;
                latRaw = ReadNode(root, "lat");
                lonRaw = ReadNode(root, "lon");
                dateRaw = ReadNode(root, "date");
                timeZoneRaw = ReadNode(root, "timeZone");
            }
            catch (JsonException ex)
            {
                _logger.LogInformation(ex, "Request body is not JSON. InvocationId={InvocationId}", invocationId);
                errorCode = "invalid_json";
                errorMessage = "Request body must be JSON with lat, lon, and date.";
                return false;
            }
        }
        else
        {
            return false;
        }

        if (!TryParseLatitude(latRaw, out var lat, out errorMessage))
        {
            errorCode = "invalid_coordinates";
            return false;
        }

        if (!TryParseLongitude(lonRaw, out var lon, out errorMessage))
        {
            errorCode = "invalid_coordinates";
            return false;
        }

        if (!TryParseDate(dateRaw, out var date, out errorMessage))
        {
            errorCode = "invalid_date";
            return false;
        }

        if (!TryParseTimeZone(timeZoneRaw, out var timeZone, out errorMessage))
        {
            errorCode = "invalid_time_zone";
            return false;
        }

        request = new SunriseSunsetRequest
        {
            Lat = lat,
            Lon = lon,
            Date = date,
            TimeZone = timeZone
        };

        return true;
    }

    private static bool HasQueryCoordinates(HttpRequest req) =>
        req.Query.ContainsKey("lat") || req.Query.ContainsKey("lon") || req.Query.ContainsKey("date");

    private static string? ReadNode(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var node) || node.ValueKind is JsonValueKind.Null or JsonValueKind.Undefined)
        {
            return null;
        }

        return node.ValueKind == JsonValueKind.String ? node.GetString() : node.ToString();
    }

    private static bool TryParseLatitude(string? raw, out double lat, out string error)
    {
        lat = 0;
        if (!TryParseFinite(raw, out lat))
        {
            error = "lat must be a finite number.";
            return false;
        }

        if (lat is < -90 or > 90)
        {
            error = "lat must be between -90 and 90.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryParseLongitude(string? raw, out double lon, out string error)
    {
        lon = 0;
        if (!TryParseFinite(raw, out lon))
        {
            error = "lon must be a finite number.";
            return false;
        }

        if (lon is < -180 or > 180)
        {
            error = "lon must be between -180 and 180.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryParseDate(string? raw, out DateOnly date, out string error)
    {
        date = default;
        if (string.IsNullOrWhiteSpace(raw))
        {
            error = "date is required and must be yyyy-MM-dd.";
            return false;
        }

        if (!DateOnly.TryParseExact(raw.Trim(), "yyyy-MM-dd", CultureInfo.InvariantCulture, DateTimeStyles.None, out date))
        {
            error = "date must be yyyy-MM-dd.";
            return false;
        }

        if (date.Year is < 1900 or > 2100)
        {
            error = "date year must be between 1900 and 2100.";
            return false;
        }

        error = string.Empty;
        return true;
    }

    private static bool TryParseTimeZone(string? raw, out string? timeZone, out string error)
    {
        timeZone = null;
        error = string.Empty;

        if (string.IsNullOrWhiteSpace(raw))
        {
            return true;
        }

        var trimmed = raw.Trim();
        if (TZConvert.TryGetTimeZoneInfo(trimmed, out _))
        {
            timeZone = trimmed;
            return true;
        }

        error = "timeZone must be a valid IANA or Windows time zone id.";
        return false;
    }

    private static bool TryParseFinite(string? raw, out double value)
    {
        value = 0;
        return !string.IsNullOrWhiteSpace(raw)
            && double.TryParse(raw, NumberStyles.Float, CultureInfo.InvariantCulture, out value)
            && !double.IsNaN(value)
            && !double.IsInfinity(value);
    }
}
