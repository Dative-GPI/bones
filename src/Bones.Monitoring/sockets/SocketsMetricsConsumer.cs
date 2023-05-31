using System.Diagnostics.Metrics;
using Yarp.Telemetry.Consumption;

using static Bones.Monitoring.Core.Consts;

namespace Bones.Monitoring.Core.Sockets
{
    public class SocketsMetricsConsumer : IMetricsConsumer<SocketsMetrics>
    {
        private Meter _socketsMeter = new Meter(SOCKETS_METER);
        private ObservableCounter<long> _outgoingSockets;
        private ObservableCounter<long> _incomingSockets;
        private ObservableCounter<long> _outgoingConnectAttempts;
        private ObservableCounter<long> _receivedBytes;
        private ObservableCounter<long> _sentBytes;
        private ObservableCounter<long> _datagramsReceived;
        private ObservableCounter<long> _datagramsSent;
        private SocketsMetrics _current;

        public SocketsMetricsConsumer()
        {
            _current = new SocketsMetrics();
            _outgoingSockets = _socketsMeter.CreateObservableCounter<long>("sockets_outgoing", () => new Measurement<long>(_current.OutgoingConnectionsEstablished));
            _incomingSockets = _socketsMeter.CreateObservableCounter<long>("sockets_incoming", () => new Measurement<long>(_current.IncomingConnectionsEstablished));
            _receivedBytes = _socketsMeter.CreateObservableCounter<long>("sockets_received_bytes", () => new Measurement<long>(_current.BytesReceived));
            _sentBytes = _socketsMeter.CreateObservableCounter<long>("sockets_sent_bytes", () => new Measurement<long>(_current.BytesSent));
            _datagramsReceived = _socketsMeter.CreateObservableCounter<long>("sockets_datagrams_received", () => new Measurement<long>(_current.DatagramsReceived));
            _datagramsSent = _socketsMeter.CreateObservableCounter<long>("sockets_datagrams_sent", () => new Measurement<long>(_current.DatagramsSent));
        }

        public void OnMetrics(SocketsMetrics previous, SocketsMetrics current)
        {
            _current = current;
        }
    }
}