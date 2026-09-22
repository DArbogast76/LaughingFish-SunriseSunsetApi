using System.Diagnostics;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LaughingFish.SunriseSunsetApi;

/// <summary>
/// Deployment smoke test. Does not calculate sunrise or sunset.
/// GET /api/health
/// </summary>
public sealed class Health
{
    private readonly ILogger<Health> _logger;

    public Health(ILogger<Health> logger)
    {
        _logger = logger;
    }

    [Function("Health")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "health")] HttpRequest req,
        FunctionContext context)
    {
        var started = Stopwatch.StartNew();
        var invocationId = context.InvocationId;

        _logger.LogInformation(
            "Health started. InvocationId={InvocationId} Method={Method} Path={Path}",
            invocationId,
            req.Method,
            req.Path.Value);

        try
        {
            var payload = new
            {
                status = "ok",
                utc = DateTime.UtcNow.ToString("o"),
                invocationId,
                worker = "dotnet-isolated",
                targetFramework = "net10.0",
                app = "LaughingFish.SunriseSunsetApi"
            };

            _logger.LogInformation(
                "Health succeeded. InvocationId={InvocationId} StatusCode={StatusCode} ElapsedMs={ElapsedMs}",
                invocationId,
                StatusCodes.Status200OK,
                started.ElapsedMilliseconds);

            return new OkObjectResult(payload);
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "Health failed. InvocationId={InvocationId} ElapsedMs={ElapsedMs}",
                invocationId,
                started.ElapsedMilliseconds);
            throw;
        }
    }
}
