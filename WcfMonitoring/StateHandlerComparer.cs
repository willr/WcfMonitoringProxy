using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace WcfMonitoring
{
    class StateHandlerComparer : Comparer<ConnectionState>
    {
        public override int Compare(ConnectionState x, ConnectionState y)
        {
            // Trace.WriteLine(string.Format("Comparing x: '{0}' y: '{1}", x.LastUpdate.ToString("HH:mm:ss.ffff"), y.LastUpdate.ToString("HH:mm:ss.ffff")));

            int compareResult = x.LastUpdate.CompareTo(y.LastUpdate);
            
            if(0 == compareResult)
            {
                compareResult = -1;
            }

            return compareResult;
        }
    }
}
