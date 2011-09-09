using System;
using System.Collections.Generic;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Diagnostics;

namespace WcfConsole.proxy
{
    public partial class WcfProxyMonitor<T> : ClientBase<T> where T : class
    {
        private static int _instanceNum = 0;
        private int _myInstanceNum;
        private readonly static object LockObj = new object();

        private readonly StateHandler _stateHandler = new StateHandler();

        public WcfProxyMonitor() : 
            base()
        {
            AddEvents();
        }
        
        public WcfProxyMonitor(string endpointConfigurationName) : 
                base(endpointConfigurationName)
        {
            AddEvents();
        }
        
        public WcfProxyMonitor(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
            AddEvents();
        }
        
        public WcfProxyMonitor(string endpointConfigurationName, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
            AddEvents();
        }

        public WcfProxyMonitor(System.ServiceModel.Channels.Binding binding, System.ServiceModel.EndpointAddress remoteAddress) : 
                base(binding, remoteAddress)
        {
            AddEvents();
        }

        private void AddEvents()
        {
            ChannelFactory<T> cf = base.ChannelFactory;

            cf.Closed += new EventHandler(CfClosed);
            cf.Closing += new EventHandler(CfClosing);
            cf.Faulted += new EventHandler(CfFaulted);
            cf.Opened += new EventHandler(CfOpened);
            cf.Opening += new EventHandler(CfOpening);

            lock (LockObj)
            {
                _instanceNum += 1;
                _myInstanceNum = _instanceNum;
            }
            
            Trace.WriteLine(string.Format("Adding event handling for proxyId: {0}", _myInstanceNum));
        }

        public int ProxyInstanceNum
        {
            get { return _myInstanceNum; }
        }

        private void CfOpening(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Opening, _myInstanceNum);
        }

        private void CfOpened(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Opened, _myInstanceNum);
        }

        private void CfFaulted(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Faulted, _myInstanceNum);
        }

        private void CfClosing(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Closing, _myInstanceNum);
        }

        private void CfClosed(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Closed, _myInstanceNum);
        }

        private void SaveProxyState(ProxyState state, int instanceNum)
        {
            _stateHandler.SetConnectionState(instanceNum, state);
            TraceOutput(state, instanceNum);
        }

        private void TraceOutput(ProxyState state, int instanceNum)
        {
            Trace.WriteLine(string.Format("Connection #{0} in state: {1}", instanceNum, state.ToString()));
        }
    }
}
