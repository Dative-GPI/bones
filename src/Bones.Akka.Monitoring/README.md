# What is Bones.Akka.Monitoring ?
Bones.Akka.Monitoring gets some logs and traces of your akka.net actors.

# How it works ?
Metrics : we created a set of essentials metrics for our actors using System.Diagnostics.Metrics;

Tracing : for the tracing, we use the tracing tools from Bones.Monitoring. For know we only trace messages independantly, we don't handle the flux of messages.


# Usage
The actors you want to monitor just have to inherit from [Bones.Akka.Monitoring.MonitoredReceiveActor](https://github.com/Dative-GPI/bones/blob/main/src/Bones.Akka.Monitoring/MonitoredReceiveActor.cs).

To automatically monitor your app without code changes, you can use our weaver as following : 

### Add following packages to your project :
```bash
dotnet add package Fody
dotnet add package Bones.Akka.Monitoring.Weaver.Fody
```

### Add the weaver to the configuration
Create a FodyWeavers.xml file at the root folder of your project containing :
```xml
<Weavers>
  <Bones.Akka.Monitoring.Weaver />
</Weavers>
```

### Enjoy
Once you build your project, all your actors which inherit from ReceiveActor will provide metrics and traces. 

Configure a meter collector with Bones.Akka.Monitoring.Consts.BONES_AKKA_MONITORING_METER meter.

Configure a new trace collector with Bones.Akka.Monitoring.Consts.BONES_AKKA_MONITORING_INSTRUMENTATION source.
