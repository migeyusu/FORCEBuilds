using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.DistributedService
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RegistryInfo
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 6)] public byte[] TagBytes;
        /// <summary>
        /// 授时
        /// </summary>
        public long Time { get; set; }
        /// <summary>
        /// 订阅服务
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] RequestListenIpaBytes;

        public short RequestListenPort;

        public IPEndPoint RequestListenEndPoint => new IPEndPoint(new IPAddress(RequestListenIpaBytes), RequestListenPort);

        /// <summary>
        /// 发布服务
        /// </summary>
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] ServiceListenIpaBytes;

        public short ServiceListenPort;

        public IPEndPoint ServiceListenEndPoint => new IPEndPoint(new IPAddress(ServiceListenIpaBytes), ServiceListenPort);
        /// <summary>
        /// 订阅中心生命周期标识
        /// </summary>
        public Guid RegistryGuid;
        /// <summary>
        ///  过滤标识
        /// </summary>
        public Guid Filter;

        public bool IsCorrect => Encoding.Unicode.GetString(TagBytes) == ServiceFactory.RPC;

        public RegistryInfo(IPEndPoint requestEndPoint,IPEndPoint servicelistenPoint,Guid guid,long time,Guid filter)
        {
            TagBytes = Encoding.Unicode.GetBytes(ServiceFactory.RPC);
            RegistryGuid = guid;
            RequestListenIpaBytes = requestEndPoint.Address.GetAddressBytes();
            RequestListenPort = (short)requestEndPoint.Port;
            ServiceListenIpaBytes = servicelistenPoint.Address.GetAddressBytes();
            ServiceListenPort = (short)servicelistenPoint.Port;
            Time = time;
            Filter = filter;
        }
    }
}