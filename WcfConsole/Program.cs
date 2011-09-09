using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.ServiceModel;
using System.Text;
using System.Threading;
using WcfConsole.proxy;
using WcfMonitoring;

namespace WcfConsole
{
    class Program
    {
        static void Main(string[] args)
        {
            Program p = new Program();
            p.RunWcf(5);
        }

        private void RunWcf(int numThreads)
        {
            for (int i = 0; i < numThreads; i++)
            {
                Thread thread = new Thread(ThreadStart);
                thread.Start();
            }

            DisplayProxyResults();
        }

        private void DisplayProxyResults()
        {
            while(true)
            {
                Dictionary<int, ConnectionState> states = StateHandler.GetConnectionStates();

                Console.WriteLine("Displaying proxy connection states: <instanceId>\t<proxy state>\t<comm state>");
                foreach (KeyValuePair<int, ConnectionState> pair in states)
                {
                    ConnectionState connState = pair.Value as ConnectionState;
                    Console.WriteLine("\t{0}\t{1}\t{2}\t{3}\t{4}", pair.Key, connState.ProxyState, connState.CommState, connState.ConnectionVia, connState.LastUpdate.ToString("HH:mm:ss.ffff"));
                }
                // buffer between runs
                Console.WriteLine();
                Thread.Sleep(new TimeSpan(0, 0, 0, 5));
            }
        }

        private static void ThreadStart()
        {
            TestClientService wcf = new TestClientService();

            try
            {
                try
                {
                    wcf.DoWork();
                }
                catch (CommunicationException ex)
                {
                    TraceLog(ex, wcf);
                    wcf.Abort();
                    TraceProxy(wcf);
                    
                    throw new ApplicationException("CommunicationsException on proxy", ex);
                }
                catch (Exception ex)
                {
                    TraceLog(ex, wcf);
                    wcf.Abort();
                    TraceProxy(wcf);
                    throw new ApplicationException("General Exception on proxy", ex);
                }
                finally
                {
                    try
                    {
                        if (wcf.State != CommunicationState.Closed && wcf.State != CommunicationState.Faulted)
                        {
                            wcf.Close(); // may throw exception while closing
                            TraceProxy(wcf);
                        }
                        else
                        {
                            wcf.Abort();
                            TraceProxy(wcf);
                        }
                    }
                    catch (Exception cex)
                    {
                        TraceLog(cex, wcf);
                        wcf.Abort();
                        TraceProxy(wcf);
                        throw new ApplicationException("Failure closing proxy", cex);
                    }
                }
            }
            catch (Exception)
            {
                // eat the exception
            }
        }

        private static void TraceProxy(TestClientService proxy)
        {
            string msg = string.Format("Proxy #{0}, current channel state: {1} @ time: {2}", proxy.ProxyInstanceNum, proxy.State, DateTime.Now.ToString("HH:mm:ss.ffff"));
            Trace.WriteLine(msg);
            Console.WriteLine(msg);
        }

        private static void TraceLog(Exception exType, TestClientService proxy)
        {
            string msg = string.Format("Proxy #{0}, with ExceptionType: {1}, ExceptionMessage: {2}", proxy.ProxyInstanceNum, exType.GetType().ToString(), exType.Message);
            Trace.WriteLine(msg);
            Console.WriteLine(msg);
        }
    }
}
