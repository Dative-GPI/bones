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
        public static LoggerConfiguration AddLokiExporter(this LoggerConfiguration loggerConfiguration,
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
                            new LokiLabel() { Key = "service_namespace", Value = environment },
                            new LokiLabel() { Key = "service_name", Value = serviceName },
                            new LokiLabel() { Key = "service_instance_id", Value = hostname }
                        });

                Console.WriteLine($"Sending logs to loki endpoint : {lokiUrl}");
            }
            else
            {
                Console.WriteLine($"No loki endpoint configured");
            }

            return loggerConfiguration;
        }

        public static TracerProviderBuilder AddTempoExporter(this TracerProviderBuilder builder, string connectionString)
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
            else
            {
                Console.WriteLine($"No tempo endpoint configured");
            }

            return builder;
        }

        public static MeterProviderBuilder AddPrometheusHttpListener(this MeterProviderBuilder builder, string scrapingEndpoint, int maxCardinality = 2000)
        {
            if (!String.IsNullOrWhiteSpace(scrapingEndpoint))
            {
                builder.SetMaxMetricPointsPerMetricStream(maxCardinality);
                builder.AddPrometheusHttpListener(options => options.UriPrefixes = new string[] { scrapingEndpoint });
                Console.WriteLine($"Exposing metrics at : {scrapingEndpoint}");
            }
            else
            {
                Console.WriteLine($"No prometheus scraping endpoint configured");
            }
            return builder;
        }

        public static MeterProviderBuilder AddPrometheusExporter(this MeterProviderBuilder builder, string prometheusHost, string prometheusPath)
        {
            if (!String.IsNullOrWhiteSpace(prometheusHost))
            {
                var endpoint = new Uri($"{prometheusHost}{prometheusPath}");
                builder.AddOtlpExporter(opt =>
                {
                    opt.Endpoint = endpoint;
                    opt.Protocol = OtlpExportProtocol.HttpProtobuf;
                });
                Console.WriteLine($"Sending metrics to : {endpoint}");
            }
            else
            {
                Console.WriteLine($"No prometheus host configured");
            }
            return builder;
        }
    }
}