using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfMonitoring
{
    public class ProxyTypeContainer
    {
        public string ProxyType { get; set; }
        public object LockObj { get; set; }
        public Dictionary<int, ConnectionState> ConnectionInfo { get; set; }
        public SortedList MostRecentlyUsed { get; set; }
    }
}
