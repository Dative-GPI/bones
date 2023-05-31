# Bones.Monitoring
Bones.Monitoring is a tool library to monitor your .net app

First add the package with the following command :
```c#
dotnet add package Bones.Monitoring
```
Then you can inject tracing and metrics adding the following line to your Program.cs
```c#
services.AddMonitoring([options])
```
## Tracing

This library provides you a metric factory to easily create internals **traces**. 
The **BonesMonitoringOptions** allow you to add a spanEnricher to enrich every span that will be create by the factory.

*Example*
```c#
services.AddMonitoring(APP_NAME, options.SpanEnricher  = (trace, param) =>
{
	trace.SetTag("source.name", "demo");
});
```

An **activity filter processor** is also provider. It allow you to filter traces during collection as following :
```c#
//example using opentelemetry to collect traces
tracingBuilder
	.AddProcessor(new  ActivityFilterProcessor((activity) => 
	//If the filter return false, the trace isn't collected
	{
		//filtering span which have duration < 1ms 
		activity.Duration > TimeSpan.FromMilliseconds(1)
	})
...
```

## Metrics

The **MetricFactory** is the equivalent as TraceFactory for metrics. It allow you to easily create internal metrics for your apps.
The metric factory store your metrics for you to retrieve it in different instance in order to increment a same metrics with many instance. The Get/Creation process work as following :
#### For counter :
```c#
_myCounter = factory.GetCounter<int>(MY_COUNTER_NAME, meter);
```
#### For Histogram 
not implemented yet
#### For Up&Down counter 
not implemented yet
#### For Observable 
not implemented yet

As you see, the Meter instance is passed into the factory paramaters in order to follow best [Microsoft's best practices](https://learn.microsoft.com/en-us/dotnet/core/diagnostics/metrics-instrumentation) :
*"  Create the Meter once, store it in a static variable or DI container, and use that instance as long as needed. Each library or library subcomponent can (and often should) create its own  [Meter](https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.metrics.meter). Consider creating a new Meter rather than reusing an existing one if you anticipate app developers would appreciate being able to enable and disable the groups of metrics separately."*
⇒ Create a Meter in you app (static or injected by DI) so you will be able to enable/disable metrics only for this source and for others library which use the factory. You can also create many meter to manage your metrics collection with better granularity.

### Sockets Metrics
The library provides many automated metrics for sockets which are :
 - sockets_opened
 - sockets_closed
 - sockets_failed
 - sockets_outgoing
 - sockets_incoming
 - sockets_received_bytes
 - sockets_sent_bytes
 - sockets_datagrams_received
 - sockets_datagrams_sent

Thoses metrics are emitted by the *Sockets.Meter*. Use following code to collect those sockets metrics :
```c#
using static Bones.Monitoring.Core.Consts
...
meterBuilder
	.AddMeter(SOCKETS_METER)
...
```

## Logging
The **ActivityEnricher** adds following property to your logs :

 - *Environment* based on RELEASE_NAMESPACE environment variable.
 - *SpanId* based on the *Activity.Current.SpanId*
 - *TraceId* based on the *Activity.Current.TraceId*
 - *ParentId* based on the *Activity.Current.ParentSpanId*

*Usage example*
```c#
.Enrich.With(new  ActivityEnricher(configuration))
```
 
 The **ContextEnricher** adds the following property to your log :
  - *UserId* based on UserId Claim in httpContext.

*Usage example*
```c#
builder.Services.AddHttpContextAccessor();
builder.Services.AddSingleton<ContextEnricher>();
...
.Enrich.With(serviceProvider.GetRequiredService<ContextEnricher>())
```

## Grafana & Prometheus exporters
The GrafanaExporterExtensions provides method to easily export logs from Serilog to Grafana Loki & OpenTelemetry traces to Grafana Tempo.

### Export logs to tempo using Serilog :
Use the "Loki" connection string value with the regex *@"(https?:\/\/)(\w+):(.*)@(.*)"* to gets the credentials. The connection string should respect the following format : *http(s)://{username}:{password}@{url}*

*Example*
```c#
loggerConfiguration
	.AddLokiExporter(configuration, SERVICE_NAME);
```

### Export traces to tempo using OpenTelemetry :
Use the "Tempo" connection string value with the regex *@"(https?:\/\/[^?]+)(?:\?token=(.*))?")* to gets the credentials. The connection string should respect the following format : *http(s)://{url}?token={tempo token which is correspond to "username:password" base64 encoded}*

*Example*
```c#
builder.Services.AddOpenTelemetry()
	.WithTracing((tracingBuilder) => tracingBuilder
		...
		.AddTempoExporter(configuration))
```

### Expose metrics to an http endpoints

The **AddPrometheusHttpListener** method expose OpenTelemetry metrics to the *"http://localhost:9091/"* url for prometheus to scrape to.

*Example*
```c#
builder.Services.AddOpenTelemetry()
	.WithMetrics((meterBuilder) => meterBuilder
		...
		.AddPrometheusHttpListener(configuration, cardinality)) //cardinality is an int corresponding to the max metric point per metric stream
```