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
using static Bones.Akka.Monitoring.Consts;

namespace Demo.Akka.Monitoring.Console
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
                .AddMeter(BONES_AKKA_MONITORING_METER)
                .AddConsoleExporter((exporterOptions, metricsOptions) => metricsOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000)
                .Build();

            Sdk.CreateTracerProviderBuilder()
                .AddSource(BONES_AKKA_MONITORING_INSTRUMENTATION)
                .AddConsoleExporter()
                .Build();
                
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddHostedService<Worker>();
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