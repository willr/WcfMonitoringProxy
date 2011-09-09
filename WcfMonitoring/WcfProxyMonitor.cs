using System;
using System.ServiceModel;
using System.Diagnostics;

namespace WcfMonitoring
{
    public abstract class WcfProxyMonitor<T> : ClientBase<T> where T : class
    {
        private const int MAX_HANDLES = 150;
        private static int _instanceNum = 0;
        private int _myInstanceNum;
        private readonly static object LockObj = new object();

        private readonly StateHandler _stateHandler = StateHandler.CreateStateHandler(MAX_HANDLES);

        protected WcfProxyMonitor() : 
            base()
        {
            AddEvents();
        }

        protected WcfProxyMonitor(string endpointConfigurationName) : 
                base(endpointConfigurationName)
        {
            AddEvents();
        }

        protected WcfProxyMonitor(string endpointConfigurationName, string remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
            AddEvents();
        }

        protected WcfProxyMonitor(string endpointConfigurationName, EndpointAddress remoteAddress) : 
                base(endpointConfigurationName, remoteAddress)
        {
            AddEvents();
        }

        protected WcfProxyMonitor(System.ServiceModel.Channels.Binding binding, EndpointAddress remoteAddress) : 
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
            UpdatingProxyInfo(_myInstanceNum);
        }

        private void CfFaulted(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Faulted, _myInstanceNum);
            UpdatingProxyInfo(_myInstanceNum);
        }

        private void CfClosing(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Closing, _myInstanceNum);
            UpdatingProxyInfo(_myInstanceNum);
        }

        private void CfClosed(object sender, EventArgs e)
        {
            SaveProxyState(ProxyState.Closed, _myInstanceNum);
            UpdatingProxyInfo(_myInstanceNum);
        }

        private void SaveProxyState(ProxyState proxyState, int instanceNum)
        {
            ConnectionState connState = _stateHandler.SetConnectionState<T>(this.GetType(), instanceNum, proxyState, ChannelFactory);
            TraceOutput(connState, instanceNum);
        }

        private void UpdatingProxyInfo(int instanceNum)
        {
            Trace.WriteLine(string.Format("UpdatingProxyInfo: '{0}', instanceId: {1}", this.GetType(), instanceNum));
            _stateHandler.UpdateProxyInfo(this.GetType(), instanceNum, this.InnerChannel);
        }

        private void TraceOutput(ConnectionState state, int instanceNum)
        {
            Trace.WriteLine(string.Format("Connection #{0} in proxyState: {1} with connState: {2} @lastUpdate {3}", instanceNum, state.ProxyState.ToString(), state.CommState, state.LastUpdate.ToString("HH:mm:ss.ffff")));
        }
    }
}
