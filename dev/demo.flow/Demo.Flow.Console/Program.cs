using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;

using Bones.Flow;
using Demo.Flow.Console.Handlers;
using Demo.Flow.Console.Commands;
using Demo.Flow.Console.Middlewares;

using System.Diagnostics;
using static Bones.Flow.Core.Consts;

namespace Demo.Flow.Console
{
    public class Program
    {
        public const string SOURCE_NAME = "bones.flow.demo";
        public static readonly ActivitySource LOCAL_SOURCE = new ActivitySource("bones.flow.demo");

        public static void Main(string[] args)
        {
            Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
                .AddMeter(BONES_FLOW_METER)
                // .AddConsoleExporter((exporterOptions, metricsOptions) => metricsOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000)
                .Build();

            Sdk.CreateTracerProviderBuilder()
                // .AddSource(BONES_AKKA_MONITORING_INSTRUMENTATION)
                .AddSource(BONES_FLOW_INSTRUMENTATION)
                // .AddSource(SOURCE_NAME)
                // .AddConsoleExporter()
                .Build();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();

                    services.AddFlow();
                    services.AddScoped<WriteLineLicitMiddleware>();
                    services.AddScoped<LogRequestHandler<HelloWorldCommand>>();
                    services.AddScoped<LogExceptionHandler<HelloWorldCommand>>();
                    services.AddScoped<HelloWorldCommandHandler>();
                    
                    services.AddScoped<ICommandHandler<HelloWorldCommand>>(sp =>
                    {
                        var pipeline = sp.GetPipelineFactory<HelloWorldCommand>()
                            .Add<WriteLineLicitMiddleware>()
                            .Add<HelloWorldCommandHandler>()
                            .OnSuccess<LogRequestHandler<HelloWorldCommand>>()
                            .OnFailure<LogExceptionHandler<HelloWorldCommand>>()
                            .Build();

                        return pipeline;
                    });

                    services.AddScoped<BoolLicitMiddleware>();
                    services.AddScoped<LogRequestHandler<BoolQuery>>();
                    services.AddScoped<LogExceptionHandler<BoolQuery>>();
                    services.AddScoped<BoolQueryHandler>();

                    services.AddScoped<IQueryHandler<BoolQuery, bool>>(sp =>
                    {
                        var pipeline = sp.GetPipelineFactory<BoolQuery, bool>()
                            .Add<BoolLicitMiddleware>()
                            .Add<BoolQueryHandler>()
                            .OnSuccess<LogRequestHandler<BoolQuery>>()
                            .OnFailure<LogExceptionHandler<BoolQuery>>()
                            .Build();

                        return pipeline;
                    });
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Information);
                    logging.AddConsole();
                })
                .Build();

            host.Run();

        }
    }
}