using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using Bones.Akka.Monitoring.DI;
using Bones.Akka.DI;

using Demo.Akka.Monitoring.Console.Actors;
using Demo.Akka.Monitoring.Console.Abstractions;
using Bones.Flow;
using Demo.Akka.Monitoring.Console.Handlers;
using Demo.Akka.Monitoring.Console.Commands;
using Demo.Akka.Monitoring.Console.Middlewares;

using static Bones.Akka.Monitoring.Consts;
using static Bones.Flow.Core.Consts;
namespace Demo.Akka.Monitoring.Console
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
                .AddMeter(BONES_AKKA_MONITORING_METER)
                // .AddConsoleExporter((exporterOptions, metricsOptions) => metricsOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000)
                .Build();

            Sdk.CreateTracerProviderBuilder()
                // .AddSource(BONES_AKKA_MONITORING_INSTRUMENTATION)
                .AddSource(BONES_FLOW_INSTRUMENTATION)
                .AddConsoleExporter()
                .Build();

            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();

                    services.AddFlow();
                    services.AddScoped<ActorLicitMiddleware>();
                    services.AddScoped<LogRequestHandler<HelloWorldCommand>>();
                    services.AddScoped<LogExceptionHandler<HelloWorldCommand>>();
                    services.AddScoped<HelloWorldCommandHandler>();
                    
                    services.AddScoped<ICommandHandler<HelloWorldCommand>>(sp =>
                    {
                        var pipeline = sp.GetPipelineFactory<HelloWorldCommand>()
                            .Add<ActorLicitMiddleware>()
                            .OnSuccess<LogRequestHandler<HelloWorldCommand>>()
                            .OnFailure<LogExceptionHandler<HelloWorldCommand>>()
                            .Build();

                        return pipeline;
                    });


                    services.AddAkkaMonitoring();
                    services.AddAkka("BonesAkkaMonitoringConsole");

                    services.AddRootCreator<RootActor>();
                    services.AddCreator<IPingActor, PingActor>();
                    services.AddCreator<IPongActor, PongActor>();

                    // services.AddOpenTelemetry()
                    //     .WithTracing(builder =>
                    //     {
                    //         builder.AddBonesAkkaMonitoringInstrumentation();
                    //         builder.AddConsoleExporter();
                    //     })
                    //     .WithMetrics(builder =>
                    //     {
                    //         builder.AddBonesAkkaMonitoringMeter();
                    //         builder.AddMeter("MyCompany.MyProduct.MyLibrary");
                    //         builder.AddConsoleExporter();
                    //     })
                    //     .StartWithHost();
                })
                .ConfigureLogging(logging =>
                {
                    logging.SetMinimumLevel(LogLevel.Trace);
                    logging.AddConsole();
                })
                .Build();

            host.Run();

        }
    }
}