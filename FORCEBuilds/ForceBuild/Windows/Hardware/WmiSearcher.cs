using System.Management;

namespace FORCEBuild.Windows.Hardware
{
    public class WmiSearcher
    {
        public static string[,] InfoSearch(string wmiClass, string[] param, string key, string value)
        {
            var str1 = "SELECT * FROM " + wmiClass + " WHERE " + key + "=" + "'" + value + "'";
            var len1 = param.Length;
            int i, j = 0;
            var mos = new ManagementObjectSearcher(str1);
            var moc = mos.Get();
            var rst = new string[moc.Count, len1];
            foreach (var mo in moc)
            {
                for (i = 0; i < len1; ++i)
                {
                    rst[j, i] = mo[param[i]].ToString();
                }
                ++j;
            }
            
            return rst;
        }

        public static string[,] InfoSearch(string wmiClass, string[] param, string key, int value)
        {
            var str1 = "SELECT * FROM " + wmiClass + " WHERE " + key + "=" + value.ToString();
            var len1 = param.Length;
            var j = 0;
            var mos = new ManagementObjectSearcher(str1);
            var moc = mos.Get();
            var rst = new string[moc.Count, len1];
            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;
                int i;
                for (i = 0; i < len1; ++i)
                {
                    rst[j, i] = mo[param[i]].ToString();
                }
                ++j;
            }
            return rst;
        }

        public static string[,] InfoSearch(string wmiClass, string[] param)
        {

            wmiClass = "SELECT * FROM " + wmiClass;
            var len1 = param.Length;
            var j = 0;
            var mos = new ManagementObjectSearcher(wmiClass);
            var moc = mos.Get();
            var rst = new string[moc.Count, len1];

            foreach (var o in moc)
            {
                var mo = (ManagementObject)o;
                int i;
                for (i = 0; i < len1; ++i)
                {
                    rst[j, i] = mo[param[i]].ToString();
                }
                ++j;
            }
            return rst;
        }

        public static string InfoSearch(string sqlSearcher, string param)
        {
            var mos = new ManagementObjectSearcher(sqlSearcher);
            var str1 = "";
            foreach (var o in mos.Get())
            {
                var mo = (ManagementObject)o;
                str1 = mo[param].ToString();
                break;
            }
            return str1;
        }
    }
}