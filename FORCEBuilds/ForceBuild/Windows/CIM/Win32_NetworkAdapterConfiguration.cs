using System;

namespace FORCEBuild.Windows.CIM
{
    public class Win32_NetworkAdapterConfiguration
    {
        public string Caption { get; set; }
        public string Description { get; set; }
        public string SettingId { get; set; }
        public bool ArpAlwaysSourceRoute { get; set; }
        public bool ArpUseEtherSnap { get; set; }
        public string DatabasePath { get; set; }
        public bool DeadGwDetectEnabled { get; set; }
        public string[] DefaultIpGateway { get; set; }
        public byte DefaultTOS { get; set; }
        public byte DefaultTtl { get; set; }
        public bool DhcpEnabled { get; set; }
        public string DhcpLeaseExpires { get; set; }
        public string DhcpLeaseObtained { get; set; }
        public string DhcpServer { get; set; }
        public string DnsDomain { get; set; }
        public string[] DnsDomainSuffixSearchOrder { get; set; }
        public bool DnsEnabledForWinsResolution { get; set; }
        public string DnsHostName { get; set; }
        public string[] DnsServerSearchOrder { get; set; }
        public bool DomainDnsRegistrationEnabled { get; set; }
        public uint ForwardBufferMemory { get; set; }
        public bool FullDnsRegistrationEnabled { get; set; }
        public ushort[] GatewayCostMetric { get; set; }
        public byte IgmpLevel { get; set; }
        public uint Index { get; set; }
        public uint InterfaceIndex { get; set; }
        public string[] IpAddress { get; set; }
        public uint IpConnectionMetric { get; set; }
        public bool IPEnabled { get; set; }
        public bool IpFilterSecurityEnabled { get; set; }
        public bool IpPortSecurityEnabled { get; set; }
        public string[] IpSecPermitIpProtocols { get; set; }
        public string[] IpSecPermitTcpPorts { get; set; }
        public string[] IpSecPermitUdpPorts { get; set; }
        public string[] IpSubnet { get; set; }
        public bool IpUseZeroBroadcast { get; set; }
        public string IpxAddress { get; set; }
        public bool IpxEnabled { get; set; }
        public uint IpxFrameType { get; set; }
        public uint IpxMediaType { get; set; }
        public string IpxNetworkNumber { get; set; }
        public string IpxVirtualNetNumber { get; set; }
        public uint KeepAliveInterval { get; set; }
        public uint KeepAliveTime { get; set; }
        public string MACAddress { get; set; }
        public uint Mtu { get; set; }
        public uint NumForwardPackets { get; set; }
        public bool PmtubhDetectEnabled { get; set; }
        public bool PmtuDiscoveryEnabled { get; set; }
        public string ServiceName { get; set; }
        public uint TcpipNetbiosOptions { get; set; }
        public uint TcpMaxConnectRetransmissions { get; set; }
        public uint TcpMaxDataRetransmissions { get; set; }
        public uint TcpNumConnections { get; set; }
        public bool TcpUseRfc1122UrgentPointer { get; set; }
        public ushort TcpWindowSize { get; set; }
        public bool WinsEnableLmHostsLookup { get; set; }
        public string WinsHostLookupFile { get; set; }
        public string WinsPrimaryServer { get; set; }
        public string WinsScopeId { get; set; }
        public string WinsSecondaryServer { get; set; }
    }
}