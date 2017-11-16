using System;
using System.Diagnostics;
using System.Threading;
using Xunit;

namespace FORCEBuild.Windows.Hardware
{
    public class Performance
    {
        /// <summary>
        /// 对于多个网卡的情况，要遍历得到
        /// </summary>
        

        [Fact]
        public void FactMethodName()
        {
            while (true)
            {
                var bytesSentPerformanceCounter = new PerformanceCounter {
                    CategoryName = ".NET CLR Networking",
                    CounterName = "Bytes Sent",
                    InstanceName = GetInstanceName(),
                    ReadOnly = true
                };

                var bytesReceivedPerformanceCounter = new PerformanceCounter {
                    CategoryName = ".NET CLR Networking",
                    CounterName = "Bytes Received",
                    InstanceName = GetInstanceName(),
                    ReadOnly = true
                };

                Console.WriteLine("Bytes sent: {0}", bytesSentPerformanceCounter.RawValue);
                Console.WriteLine("Bytes received: {0}", bytesReceivedPerformanceCounter.RawValue);
                Thread.Sleep(1000);
            }

        }

        private static string GetInstanceName()
        {
            var returnvalue = "not found";
            //Checks bandwidth usage for CUPC.exe..Change it with your application Name
            var Array = PerformanceCounterCategory.GetCategories();
            foreach (var t in Array) {
                if (!t.CategoryName.Contains(".NET CLR Networking")) continue;
                foreach (var item in t.GetInstanceNames())
                    if (item.ToLower().Contains("CUPC".ToLower()))
                        returnvalue = item;
            }
            return returnvalue;
        }
    }
}