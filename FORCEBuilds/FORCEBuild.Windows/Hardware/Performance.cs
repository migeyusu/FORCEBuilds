using System;
using System.Diagnostics;
using System.Threading;


namespace FORCEBuild.Windows.Hardware
{
    public class Performance
    {
        /// <summary>
        /// 对于多个网卡的情况，要遍历得到
        /// </summary>
        private static string GetInstanceName()
        {
            //Checks bandwidth usage for CUPC.exe..Change it with your application Name
            var array = PerformanceCounterCategory.GetCategories();
            foreach (var t in array) {
                if (!t.CategoryName.Contains(".NET CLR Networking")) continue;
                foreach (var item in t.GetInstanceNames())
                    if (item.ToLower().Contains("CUPC".ToLower()))
                        return item;
            }
            return string.Empty;
        }
    }
}