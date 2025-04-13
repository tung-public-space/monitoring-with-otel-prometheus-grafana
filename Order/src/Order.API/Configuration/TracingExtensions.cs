using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;

namespace Order.API.Configuration;

public static class TracingExtensions
{
    public static void ConfigureOpenTelemetry(this IHostApplicationBuilder builder, string svcName, string appName)
    {
        builder.Services.AddOpenTelemetry()
            .WithMetrics(opt =>
            {
                var otelEndpoint = builder.Configuration["Otel:Endpoint"];

                opt
                    .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService(svcName))
                    .AddMeter(appName)
                    .AddAspNetCoreInstrumentation()
                    .AddRuntimeInstrumentation()
                    .AddProcessInstrumentation()
                    .AddOtlpExporter(opts =>
                    {
                        opts.Endpoint = new Uri(otelEndpoint ?? "");
                    });
            });
    }
}