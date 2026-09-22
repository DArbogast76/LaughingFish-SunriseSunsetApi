using System.Diagnostics;
using LaughingFish.SunriseSunsetApi.Contracts;
using LaughingFish.SunriseSunsetApi.Services;
using LaughingFish.SunriseSunsetApi.Validation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LaughingFish.SunriseSunsetApi.Functions;

/// <summary>
/// Calculated sunrise and sunset. No external lookup.
/// GET|POST /api/v1/sunrise-sunset
/// </summary>
public sealed class GetSunriseSunsetFunction
{
    private readonly ILogger<GetSunriseSunsetFunction> _logger;
    private readonly ISunriseSunsetRequestParser _parser;
    private readonly ISunriseSunsetQuery _query;

    public GetSunriseSunsetFunction(
        ILogger<GetSunriseSunsetFunction> logger,
        ISunriseSunsetRequestParser parser,
        ISunriseSunsetQuery query)
    {
        _logger = logger;
        _parser = parser;
        _query = query;
    }

    [Function("GetSunriseSunset")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", "post", Route = "v1/sunrise-sunset")] HttpRequest req,
        FunctionContext context)
    {
        var started = Stopwatch.StartNew();
        var invocationId = context.InvocationId;

        _logger.LogInformation(
            "SunriseSunset started. InvocationId={InvocationId} Method={Method} Path={Path} QueryString={QueryString} ContentType={ContentType} ContentLength={ContentLength}",
            invocationId,
            req.Method,
            req.Path.Value,
            req.QueryString.Value,
            req.ContentType,
            req.ContentLength);

        try
        {
            if (!_parser.TryParse(req, invocationId, out var request, out var errorCode, out var errorMessage)
                || request is null)
            {
                _logger.LogInformation(
                    "SunriseSunset rejected. InvocationId={InvocationId} Error={Error} Message={Message} ElapsedMs={ElapsedMs}",
                    invocationId,
                    errorCode,
                    errorMessage,
                    started.ElapsedMilliseconds);
                return Error(StatusCodes.Status400BadRequest, errorCode, errorMessage, invocationId);
            }

            _logger.LogInformation(
                "SunriseSunset inputs accepted. InvocationId={InvocationId} Lat={Lat} Lon={Lon} Date={Date} TimeZone={TimeZone}",
                invocationId,
                request.Lat,
                request.Lon,
                request.Date,
                request.TimeZone);

            var payload = _query.Execute(invocationId, request);

            _logger.LogInformation(
                "SunriseSunset succeeded. InvocationId={InvocationId} StatusCode={StatusCode} Condition={Condition} Confidence={Confidence} Date={Date} Lat={Lat} Lon={Lon} ElapsedMs={ElapsedMs}",
                invocationId,
                StatusCodes.Status200OK,
                payload.Condition,
                payload.Confidence,
                payload.Date,
                payload.Location.Lat,
                payload.Location.Lon,
                started.ElapsedMilliseconds);

            return new OkObjectResult(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "SunriseSunset failed. InvocationId={InvocationId} ElapsedMs={ElapsedMs}",
                invocationId,
                started.ElapsedMilliseconds);
            return Error(
                StatusCodes.Status500InternalServerError,
                "sunrise_sunset_failed",
                "The sunrise/sunset request failed.",
                invocationId);
        }
    }

    private static ObjectResult Error(int statusCode, string code, string message, string invocationId)
    {
        return new ObjectResult(new ApiErrorResponse
        {
            Error = code,
            Message = message,
            InvocationId = invocationId
        })
        {
            StatusCode = statusCode
        };
    }
}
