using System;
using System.Collections.Generic;
using System.Diagnostics.Metrics;
using System.Net.Sockets;
using Yarp.Telemetry.Consumption;

using static Bones.Monitoring.Core.Consts;

namespace Bones.Monitoring.Core.Sockets
{

    public class SocketsEventsConsumer : ISocketsTelemetryConsumer
    {
        private Meter _socketsMeter = new Meter(SOCKETS_METER);
        private Counter<int> _openedSockets;
        private Counter<int> _closedSockets;
        private Counter<int> _failedSockets;

        public SocketsEventsConsumer()
        {
            _openedSockets = _socketsMeter.CreateCounter<int>("sockets_opened");
            _closedSockets = _socketsMeter.CreateCounter<int>("sockets_closed");
            _failedSockets = _socketsMeter.CreateCounter<int>("sockets_failed");
        }
        

        public void OnConnectStart(DateTime timestamp, string address)
        {
            _openedSockets.Add(1);
        }

        public void OnConnectStop(DateTime timestamp)
        {
            _closedSockets.Add(1);
        }

        public void OnConnectFailed(DateTime timestamp, SocketError error, string exceptionMessage)
        {
            _failedSockets.Add(1, new KeyValuePair<string, object>("error", error.ToString()));
        }
    }
}