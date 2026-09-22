using System.Diagnostics;
using LaughingFish.SunriseSunsetApi.Documentation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;

namespace LaughingFish.SunriseSunsetApi.Functions;

/// <summary>
/// Public documentation for Health and Sunrise/Sunset.
/// GET /api/v1/docs
/// </summary>
public sealed class GetApiDocsFunction
{
    private readonly ILogger<GetApiDocsFunction> _logger;

    public GetApiDocsFunction(ILogger<GetApiDocsFunction> logger)
    {
        _logger = logger;
    }

    [Function("GetApiDocs")]
    public IActionResult Run(
        [HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "v1/docs")] HttpRequest req,
        FunctionContext context)
    {
        var started = Stopwatch.StartNew();
        var invocationId = context.InvocationId;
        var baseUrl = $"{req.Scheme}://{req.Host.Value}";

        _logger.LogInformation(
            "GetApiDocs started. InvocationId={InvocationId} Method={Method} Path={Path} QueryString={QueryString} BaseUrl={BaseUrl} ContentType={ContentType}",
            invocationId,
            req.Method,
            req.Path.Value,
            req.QueryString.Value,
            baseUrl,
            req.ContentType);

        try
        {
            var html = ApiDocumentationHtml.Build(baseUrl);
            req.HttpContext.Response.Headers.CacheControl = "public, max-age=300";

            _logger.LogInformation(
                "GetApiDocs succeeded. InvocationId={InvocationId} StatusCode={StatusCode} Bytes={Bytes} ElapsedMs={ElapsedMs}",
                invocationId,
                StatusCodes.Status200OK,
                html.Length,
                started.ElapsedMilliseconds);

            return new ContentResult
            {
                Content = html,
                ContentType = "text/html; charset=utf-8",
                StatusCode = StatusCodes.Status200OK
            };
        }
        catch (Exception ex)
        {
            _logger.LogError(
                ex,
                "GetApiDocs failed. InvocationId={InvocationId} ExceptionType={ExceptionType} ExceptionMessage={ExceptionMessage} ElapsedMs={ElapsedMs}",
                invocationId,
                ex.GetType().Name,
                ex.Message,
                started.ElapsedMilliseconds);
            throw;
        }
    }
}
