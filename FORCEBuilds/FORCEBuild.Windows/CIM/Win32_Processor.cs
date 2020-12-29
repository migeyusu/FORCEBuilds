using System;

namespace FORCEBuild.Windows.CIM
{
    public class Win32_Processor : CIM_Processor
    {
        public string AssetTag { get; set; }
        public uint Characteristics { get; set; }
        public ushort CpuStatus { get; set; }
        public ushort CurrentVoltage { get; set; }


        public uint ExtClock { get; set; }

        public uint L2CacheSize { get; set; }
        public uint L2CacheSpeed { get; set; }
        public uint L3CacheSize { get; set; }
        public uint L3CacheSpeed { get; set; }

        public ushort Level { get; set; }

        public string Manufacturer { get; set; }

        public uint NumberOfCores { get; set; }
        public uint NumberOfEnabledCore { get; set; }
        public uint NumberOfLogicalProcessors { get; set; }

        public string PartNumber { get; set; }

        public string ProcessorId { get; set; }
        public ushort ProcessorType { get; set; }
        public ushort Revision { get; set; }

        public bool SecondLevelAddressTranslationExtensions { get; set; }
        public string SerialNumber { get; set; }
        public string SocketDesignation { get; set; }

        public uint ThreadCount { get; set; }

        public string Version { get; set; }
        public bool VirtualizationFirmwareEnabled { get; set; }
        public bool VmMonitorModeExtensions { get; set; }
        public uint VoltageCaps { get; set; }
    }
}