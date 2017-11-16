using System;

namespace FORCEBuild.Windows.CIM
{
    public class CIM_LogicalDevice:CIM_LogicalElement
    {
        public ushort Availability { get; set; }
        public uint ConfigManagerErrorCode { get; set; }
        public bool ConfigManagerUserConfig { get; set; }
        public string CreationClassName { get; set; }
        public string DeviceId { get; set; }
        public uint PowerManagementCapabilities { get; set; }
        public bool ErrorCleared { get; set; }
        public string ErrorDescription { get; set; }
        public uint LastErrorCode { get; set; }
        public string PnpDeviceId { get; set; }
        public bool PowerManagementSupported { get; set; }
        public uint StatusInfo { get; set; }
        public string SystemCreationClassName { get; set; }
        public string SystemName { get; set; }
        
    }
}