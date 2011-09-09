using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace WcfConsole.proxy
{
    public enum ProxyState
    {
        Opening,
        Opened,
        Closing,
        Closed,
        Faulted
    }
}
