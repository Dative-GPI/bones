using Bones.Akka.Monitoring.DI;
using Bones.Akka.DI;
using Microsoft.Extensions.Hosting;
using OpenTelemetry;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Bones.Akka.Monitoring.Console.Actors;
using Test.Abstractions;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;

namespace Bones.Akka.Monitoring.Console
{
    public class Program
    {

        public static void Main(string[] args)
        {
            Sdk.CreateMeterProviderBuilder()
                .SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("MyService"))
                .AddBonesAkkaMonitoringMeter()
                .AddConsoleExporter((exporterOptions, metricsOptions) => metricsOptions.PeriodicExportingMetricReaderOptions.ExportIntervalMilliseconds = 1000)
                .Build();

            Sdk.CreateTracerProviderBuilder()
                .AddBonesAkkaMonitoringInstrumentation()
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