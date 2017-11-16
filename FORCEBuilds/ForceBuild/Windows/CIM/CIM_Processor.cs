namespace FORCEBuild.Windows.CIM
{
    public class CIM_Processor: CIM_LogicalDevice
    {
        public ushort AddressWidth { get; set; }
        public uint CurrentClockSpeed { get; set; }
        public ushort DataWidth { get; set; }

        public ushort Family { get; set; }
        public ushort LoadPercentage { get; set; }
        public uint MaxClockSpeed { get; set; }
        public string OtherFamilyDescription { get; set; }

        public string Role { get; set; }

        public string Stepping { get; set; }
        public string UniqueId { get; set; }
        public ushort UpgradeMethod { get; set; }
    }
}