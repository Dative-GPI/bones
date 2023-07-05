using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace Bones.Monitoring.Core
{
    public static class GrafanaExporterExtensions
    {
        public static void AddLokiExporter(this LoggerConfiguration loggerConfiguration, IConfiguration configuration, string serviceName)
        {
            var lokiConnStr = configuration.GetConnectionString("Loki");
            if (!String.IsNullOrWhiteSpace(lokiConnStr))
            {
                var lokiRegex = new Regex(@"(https?:\/\/)(\w+):(.*)@(.*)");
                MatchCollection matches = lokiRegex.Matches(lokiConnStr);
                var match = matches.Single();
                var lokiUrl = match.Groups[1].Value + match.Groups[4].Value;
                var lokiUser = match.Groups[2].Value;
                var lokiPassword = match.Groups[3].Value;
                loggerConfiguration.WriteTo.GrafanaLoki(
                        lokiUrl,
                        credentials: new LokiCredentials()
                        {
                            Login = lokiUser,
                            Password = lokiPassword
                        },
                        labels: new List<LokiLabel> {
                            new LokiLabel() { Key = "Environment", Value = configuration.GetValue<string>("RELEASE_NAMESPACE", "default") },
                            new LokiLabel() { Key = "ServiceName", Value = serviceName },
                            new LokiLabel() { Key = "Instance", Value = configuration.GetValue<string>("HOSTNAME", Dns.GetHostName())}
                        });

                Console.WriteLine($"Sending logs to loki endpoint : {lokiUrl}");
            };
        }

        public static void AddTempoExporter(this TracerProviderBuilder builder, IConfiguration configuration)
        {
            var tempoConnStr = configuration.GetConnectionString("Tempo");
            if (!String.IsNullOrWhiteSpace(tempoConnStr))
            {
                var tempoRegex = new Regex(@"(https?:\/\/[^?]+)(?:\?token=(.*))?");
                var values = tempoConnStr.Split(";");
                foreach (string value in values)
                {
                    MatchCollection matches = tempoRegex.Matches(value);
                    var match = matches.Single();
                    var tempoUrl = match.Groups[1].Value;
                    var tempoAuthorization = match.Groups.Count > 1 ? "Authorization=Basic " + match.Groups[2].Value : "";
                    builder.AddOtlpExporter(opt =>
                    {
                        opt.Protocol = OpenTelemetry.Exporter.OtlpExportProtocol.Grpc;
                        opt.Endpoint = new System.Uri(tempoUrl);
                        opt.Headers = tempoAuthorization;
                    });
                    Console.WriteLine($"Sending traces to tempo endpoint : {tempoUrl}");
                }
            }
        }

        public static void AddPrometheusHttpListener(this MeterProviderBuilder builder, IConfiguration configuration, int maxCardinality = 2000)
        {
            var prometheusConnstr = configuration.GetConnectionString("Prometheus");
            if (!String.IsNullOrWhiteSpace(prometheusConnstr))
            {
                builder.SetMaxMetricPointsPerMetricStream(maxCardinality);
                builder.AddPrometheusHttpListener(options => options.UriPrefixes = new string[] { prometheusConnstr });
                Console.WriteLine($"Exposing metrics at : {prometheusConnstr}");
            }
        }
    }
}