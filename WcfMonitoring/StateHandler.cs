using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.ServiceModel;

namespace WcfMonitoring
{
    public class StateHandler
    {
        private readonly static Dictionary<string, ProxyTypeContainer> ProxyTypeContainers = new Dictionary<string, ProxyTypeContainer>();
        private readonly static object ProxyTypeLockObject = new object();
        
        private static int _maxHandles;
        private static StateHandler _stateHandler;

        private StateHandler()
        {
        }

        public static StateHandler CreateStateHandler(int maxHandles)
        {
            lock (ProxyTypeLockObject)
            {
                if(_stateHandler == null)
                {
                    _stateHandler = new StateHandler();
                }
                _maxHandles = maxHandles;
            }
            return _stateHandler;
        }

        public ConnectionState SetConnectionState<T>(Type proxy, int instanceId, ProxyState proxyState, ChannelFactory<T> channel)
        {
            ConnectionState connState = null;
            lock(ProxyTypeLockObject)
            {
                string proxyTypeKey = proxy.ToString();
                if(! ProxyTypeContainers.ContainsKey(proxyTypeKey))
                {
                    ProxyTypeContainer newContainer = new ProxyTypeContainer()
                                                       {
                                                           ConnectionInfo = new Dictionary<int, ConnectionState>(),
                                                           MostRecentlyUsed =
                                                               new SortedList(
                                                               new StateHandlerComparer()),
                                                           LockObj = new object()
                                                       };
                    ProxyTypeContainers.Add(proxyTypeKey, newContainer);
                }

                ProxyTypeContainer proxyContainer = ProxyTypeContainers[proxyTypeKey];
                object connLock = proxyContainer.LockObj;
                Dictionary<int, ConnectionState> connInfo = proxyContainer.ConnectionInfo;
                SortedList mru = proxyContainer.MostRecentlyUsed;

                lock (connLock)
                {
                    if (connInfo.ContainsKey(instanceId))
                    {
                        connState = connInfo[instanceId];
                        connState.ProxyState = proxyState;
                        connState.CommState = channel.State;
                        connState.LastUpdate = DateTime.Now;
                    }
                    else
                    {
                        SortedSet<CommunicationState> s = null;
                        
                        Trace.WriteLine(string.Format("Current mru.count: {0}, maxCount: {1}, instanceId: {2}", mru.Count, _maxHandles, instanceId));
                        if (mru.Count >= _maxHandles)
                        {
                            int lastIndexOfList = mru.Count - 1;
                            int indexToRemove = lastIndexOfList - 1;
                            int instanceId2Remove = (int)mru.GetByIndex(indexToRemove);
                            mru.RemoveAt(indexToRemove);
                            connInfo.Remove(instanceId2Remove);
                        }

                        connState = new ConnectionState(instanceId, proxyState, channel.State, proxyTypeKey)
                                        {
                                            LastUpdate = DateTime.Now,
                                            Guid = Guid.NewGuid()
                                        };

                        connInfo.Add(instanceId, connState);
                        mru.Add(connState, instanceId);
                    }
                }
                
            }
            return connState;
        }

        public static Dictionary<int, ConnectionState> GetConnectionStates()
        {
            Dictionary<int, ConnectionState> result = new Dictionary<int, ConnectionState>();
            lock (ProxyTypeLockObject)
            {
                foreach (KeyValuePair<string, ProxyTypeContainer> proxyPair in ProxyTypeContainers)
                {
                    ProxyTypeContainer proxyContainer = proxyPair.Value;
                    foreach (KeyValuePair<int, ConnectionState> pair in proxyContainer.ConnectionInfo)
                    {
                        result.Add(pair.Key, pair.Value);
                    }
                }
            }

            return result;
        }

        public void UpdateProxyInfo(Type proxyType, int instanceId, IClientChannel channel)
        {
            lock (ProxyTypeLockObject)
            {
                string proxyTypeKey = proxyType.ToString();
                if(! ProxyTypeContainers.ContainsKey(proxyTypeKey)) 
                { 
                    throw new ApplicationException(
                        string.Format("Failed to find ProxyTypeContaier for Type: {0}", proxyTypeKey));
                }

                ProxyTypeContainer proxyContainer = ProxyTypeContainers[proxyTypeKey];
                object connLock = proxyContainer.LockObj;
                Dictionary<int, ConnectionState> connInfo = proxyContainer.ConnectionInfo;
                lock (connLock)
                {
                    if (connInfo.ContainsKey(instanceId))
                    {
                        ConnectionState connState = connInfo[instanceId];
                        connState.ConnectionVia = channel.Via;
                        connState.Endpoint = channel.RemoteAddress;
                    }
                    else
                    {
                        throw new ApplicationException(
                            string.Format("Failed to find ConnectionState for InstanceId: {0} of ProxyType: {1}", instanceId, proxyTypeKey));
                    }
                }
            }
        }
    }
}
