using System;
using System.Net;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Net.ServiceGovernance
{
    /// <summary>
    /// 用于广播的服务信息结构体
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ServiceDescriptionDto
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ServiceBroadcaster.TAG_LENGTH)] public byte[] TagBytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] IpBytes;

        public short ServicePort;
        /// <summary>
        /// 服务uid，指代IServiceProvider的serviceuid
        /// </summary>
        public Guid ServiceUid;
        /// <summary>
        /// 过滤uid，只接受同名filter的服务，一般使用assembly guid
        /// </summary>
        public Guid Filter;

        public ServiceDescriptionDto(IPEndPoint ipEndPoint, Guid serviceGuid, Guid filterGuid)
        {
            IpBytes = ipEndPoint.Address.GetAddressBytes();
            ServicePort = (short)ipEndPoint.Port;
            TagBytes = Encoding.Unicode.GetBytes(ServiceBroadcaster.TAG);
            ServiceUid = serviceGuid;
            Filter = filterGuid;
        }

        public bool GetIsCorrect(Guid filterGuid)
        {
            return Encoding.Unicode.GetString(TagBytes) == ServiceBroadcaster.TAG
                   && filterGuid == Filter;
        }
    }
}