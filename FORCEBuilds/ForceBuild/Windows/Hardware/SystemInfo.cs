using System;
using System.Runtime.InteropServices;

namespace FORCEBuild.Windows.Hardware
{
    [Serializable]
    public class SystemInfo:ISystemInfoGet
    {
        private static SystemInfo systemInfo;

        public int ProcessorCount { get; set; }

        public int TickCount { get; set; }

        public Version Version { get; set; }

        public OperatingSystem OSVersion { get; set; }

        //
        // 摘要:
        //     Determines whether the current process is a 64-bit process.
        //
        // 返回结果:
        //     true if the process is 64-bit; otherwise, false.
        public bool Is64BitProcess { get; set; }

        //
        // 摘要:
        //     Determines whether the current operating system is a 64-bit operating system.
        //
        // 返回结果:
        //     true if the operating system is 64-bit; otherwise, false.
        public bool Is64BitOperatingSystem { get; set; }

        //
        // 摘要:
        //     Gets the user name of the person who is currently logged on to the Windows operating
        //     system.
        //
        // 返回结果:
        //     The user name of the person who is logged on to Windows.
        public string UserName { get; set; }

        public string UserDomainName { get; set; }

        //
        // 摘要:
        //     Gets the NetBIOS name of this local computer.
        //
        // 返回结果:
        //     A string containing the name of this computer.
        //
        // 异常:
        //   T:System.InvalidOperationException:
        //     The name of this computer cannot be obtained.
        public string MachineName { get; set; }

        public static SystemInfo GetSystemInfo()
        {
            return systemInfo ?? (systemInfo = new SystemInfo {
                ProcessorCount = Environment.ProcessorCount,
                TickCount = Environment.TickCount,
                Version = Environment.Version,
                OSVersion = Environment.OSVersion,
                Is64BitOperatingSystem = Environment.Is64BitOperatingSystem,
                Is64BitProcess = Environment.Is64BitProcess,
                UserDomainName = Environment.UserDomainName,
                UserName = Environment.UserName,
                MachineName = Environment.MachineName
            });
        }

        [DllImport("kernel32")]
        public static extern uint GetTickCount();

        SystemInfo ISystemInfoGet.GetSystemInfo()
        {
            return GetSystemInfo();
        }
    }
}