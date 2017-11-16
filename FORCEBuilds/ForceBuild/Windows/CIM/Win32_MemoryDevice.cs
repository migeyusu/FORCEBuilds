using System;

namespace FORCEBuild.Windows.CIM
{
    public class Win32_MemoryDevice
    {
        public ushort Access { get; set; }
        public byte AdditionalErrorData { get; set; }
        public ushort Availability { get; set; }
        public ulong BlockSize { get; set; }
        public string Caption { get; set; }
        public uint ConfigManagerErrorCode { get; set; }
        public bool ConfigManagerUserConfig { get; set; }
        public bool CorrectableError { get; set; }
        public string CreationClassName { get; set; }
        public string Description { get; set; }
        public string DeviceId { get; set; }
        public ulong EndingAddress { get; set; }
        public ushort ErrorAccess { get; set; }
        public ulong ErrorAddress { get; set; }
        public bool ErrorCleared { get; set; }
        public byte ErrorData { get; set; }
        public ushort ErrorDataOrder { get; set; }
        public string ErrorDescription { get; set; }
        public ushort ErrorGranularity { get; set; }
        public ushort ErrorInfo { get; set; }
        public string ErrorMethodology { get; set; }
        public ulong ErrorResolution { get; set; }
        public DateTime ErrorTime { get; set; }
        public uint ErrorTransferSize { get; set; }
        public DateTime InstallDate { get; set; }
        public uint LastErrorCode { get; set; }
        public string Name { get; set; }
        public ulong NumberOfBlocks { get; set; }
        public string OtherErrorDescription { get; set; }
        public string PnpDeviceId { get; set; }
        public ushort PowerManagementCapabilities { get; set; }
        public bool PowerManagementSupported { get; set; }
        public string Purpose { get; set; }
        public ulong StartingAddress { get; set; }
        public string Status { get; set; }
        public ushort StatusInfo { get; set; }
        public string SystemCreationClassName { get; set; }
        public bool SystemLevelAddress { get; set; }
        public string SystemName { get; set; }
    }
}