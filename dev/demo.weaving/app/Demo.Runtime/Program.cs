using Bones.Akka.Monitoring.DI;
using Microsoft.Extensions.Hosting;
using OpenTelemetry.Trace;
using OpenTelemetry.Metrics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OpenTelemetry.Resources;
using Demo.Core.DI;
using Bones.Flow;

using static Bones.Akka.Monitoring.Consts;
using static Bones.Flow.Core.Consts;

namespace Demo.Runtime
{
    public class Program
    {

        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices(services =>
                {
                    services.AddOpenTelemetry()
                        .WithTracing(builder =>
                        {
                            builder.SetResourceBuilder(ResourceBuilder.CreateDefault()
                                .AddService("demo"));
                            builder.AddSource(BONES_AKKA_MONITORING_INSTRUMENTATION);
                            builder.AddSource(BONES_FLOW_INSTRUMENTATION);
                            builder.AddConsoleExporter();
                        })
                        .WithMetrics(builder =>
                        {
                            builder.SetResourceBuilder(ResourceBuilder.CreateDefault().AddService("demo"));
                            builder.AddMeter(BONES_AKKA_MONITORING_METER);
                            builder.AddConsoleExporter();
                        });

                    services.AddCore();
                    services.AddHostedService<Worker>();
                    services.AddAkkaMonitoring(options =>
                        options.SpanEnricher = (trace, param) =>
                        {
                            trace.SetTag("Source.name", "Bones.Akka.Monitoring");
                        }
                    );
                    services.AddFlow(options =>
                        options.SpanEnricher = (trace, param) =>
                        {
                            trace.SetTag("Source.name", "Bones.Flow");
                        }
                    );
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