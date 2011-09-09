using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfConsole.proxy
{
    class StateHandler
    {
        private readonly static object LockObj = new object();
        private readonly static Dictionary<int, ProxyState> ConnectionInfo = new Dictionary<int, ProxyState>();

        public StateHandler()
        {
        }

        public void SetConnectionState(int instanceId, ProxyState state)
        {
            lock (LockObj)
            {
                if(ConnectionInfo.ContainsKey(instanceId))
                {
                    ConnectionInfo[instanceId] = state;
                }
                else
                {
                    ConnectionInfo.Add(instanceId, state);
                }
            }
        }

        public static Dictionary<int,ProxyState> GetConnectionStates()
        {
            Dictionary<int, ProxyState> result = new Dictionary<int, ProxyState>();
            lock (LockObj)
            {
                foreach (KeyValuePair<int, ProxyState> pair in ConnectionInfo)
                {
                    result.Add(pair.Key, pair.Value);
                }
            }

            return result;
        }
    }
}
