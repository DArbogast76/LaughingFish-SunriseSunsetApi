using Azure.Monitor.OpenTelemetry.Exporter;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Azure.Functions.Worker.OpenTelemetry;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

var telemetry = builder.Services.AddOpenTelemetry()
    .UseFunctionsWorkerDefaults();

var appInsightsConnection =
    builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"]
    ?? builder.Configuration["AzureMonitor:ConnectionString"];

if (!string.IsNullOrWhiteSpace(appInsightsConnection))
{
    telemetry.UseAzureMonitorExporter();
}

builder.Build().Run();
