using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;

namespace WcfMonitoring
{
    public class ConnectionState
    {
        public ConnectionState(int instanceId, ProxyState proxyState, CommunicationState commState, string proxyType)
        {
            this.InstanceId = instanceId;
            this.ProxyState = proxyState;
            this.CommState = commState;
            this.ProxyType = proxyType;
        }

        public ProxyState ProxyState { get; set; }

        public CommunicationState CommState { get; set; }

        public int InstanceId { get; set; }

        public Uri ConnectionVia { get; set; }

        public EndpointAddress Endpoint { get; set; }

        public DateTime LastUpdate { get; set; }

        public string ProxyType { get; set; }

        public Guid Guid { get; set; }
    }
}
