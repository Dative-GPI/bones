using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using Microsoft.Extensions.Configuration;
using OpenTelemetry.Exporter;
using OpenTelemetry.Metrics;
using OpenTelemetry.Trace;
using Serilog;
using Serilog.Sinks.Grafana.Loki;

namespace Bones.Monitoring.Core
{
    public static class GrafanaExporterExtensions
    {
        public static void AddLokiExporter(this LoggerConfiguration loggerConfiguration,
            string connectionString,
            string environment,
            string serviceName,
            string hostname)
        {
            if (!String.IsNullOrWhiteSpace(connectionString))
            {
                var lokiRegex = new Regex(@"(https?:\/\/)(\w+):(.*)@(.*)");
                MatchCollection matches = lokiRegex.Matches(connectionString);
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
                            new LokiLabel() { Key = "service.namespace", Value = environment },
                            new LokiLabel() { Key = "service.name", Value = serviceName },
                            new LokiLabel() { Key = "service.instance.id", Value = hostname }
                        });

                Console.WriteLine($"Sending logs to loki endpoint : {lokiUrl}");
            };
        }

        public static void AddTempoExporter(this TracerProviderBuilder builder, string connectionString)
        {
            if (!String.IsNullOrWhiteSpace(connectionString))
            {
                var tempoRegex = new Regex(@"(https?:\/\/[^?]+)(?:\?token=(.*))?");
                var values = connectionString.Split(";");
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

        public static void AddPrometheusHttpListener(this MeterProviderBuilder builder, string scrapingEndpoint, int maxCardinality = 2000)
        {
            if (!String.IsNullOrWhiteSpace(scrapingEndpoint))
            {
                builder.SetMaxMetricPointsPerMetricStream(maxCardinality);
                builder.AddPrometheusHttpListener(options => options.UriPrefixes = new string[] { scrapingEndpoint });
                Console.WriteLine($"Exposing metrics at : {scrapingEndpoint}");
            }
        }

        public static void AddPrometheusExporter(this MeterProviderBuilder builder, string prometheusHost, string prometheusPath)
        {
            if (!String.IsNullOrWhiteSpace(prometheusHost))
            {
                var prometheusEndpoint = new Uri($"{prometheusHost}{prometheusPath}");
                builder.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = prometheusEndpoint;
                    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
                Console.WriteLine($"Sending metrics to : {prometheusEndpoint.ToString}");
            }
        }
    }
}