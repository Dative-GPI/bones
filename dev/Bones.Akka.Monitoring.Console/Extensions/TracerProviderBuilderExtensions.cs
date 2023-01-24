using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;

namespace Bones.Akka.Monitoring.Console
{
    public static class TracerProviderBuilderExtensions
    {
        public static TracerProviderBuilder AddBonesAkkaMonitoringInstrumentation(this TracerProviderBuilder builder)
            => builder.AddSource("Bones.Akka.Monitoring");
        public static MeterProviderBuilder AddBonesAkkaMonitoringMeter(this MeterProviderBuilder builder)
            => builder.AddMeter("Bones.Akka.Meter");
    }
}