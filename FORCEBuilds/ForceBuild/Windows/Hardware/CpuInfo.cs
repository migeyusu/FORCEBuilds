using System.Linq;
using System.Management;

namespace FORCEBuild.Windows.Hardware
{
    public class CpuInfo
    {
                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                                        

        private enum WmiProcesser
        {
            CurrentClockSpeed = 1,
            NumberOfCores = 2,
            Name = 3,
            NumberOfLogicalProcessors = 4
        }

        public static string InfoSearch(string searcher)
        {
            return WmiSearcher.InfoSearch("SELECT * FROM Win32_Processor", searcher);
        }

        public static string[] ProcessInfo()
        {
            string[] searcher = {WmiProcesser.Name.ToString(), WmiProcesser.NumberOfCores.ToString(),
                WmiProcesser.NumberOfLogicalProcessors.ToString()};
            return InfoSearch(searcher);
        }

        public static string[] InfoSearch(string[] searcher)
        {
            var rut = WmiSearcher.InfoSearch("Win32_Processor", searcher);
            var rst = new string[searcher.Length];
            for (var i = 0; i < searcher.Length; ++i)
            {
                rst[i] = rut[0, i];
            }
            return rst;
        }

        public static int ClockSpeed()
        {
            var mos = new ManagementObjectSearcher("SELECT * FROM Win32_Processor");
            return (from ManagementObject mo in mos.Get() select int.Parse(mo["CurrentClockSpeed"].ToString())).FirstOrDefault();
        }


    }
}