using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// 消息总线响应
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MessageBusResponseHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageBusService.ResponseTagLength)] public byte[] TagBytes;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] IpBytes;
        //public short Port;

        public int LeaveLength;

        //public IPEndPoint EndPoint {
        //    get => new IPEndPoint(new IPAddress(IpBytes), Port);
        //    set {
        //        this.IpBytes = value.Address.GetAddressBytes();
        //        this.Port = (short)value.Port;
        //    }
        //}

        public bool IsSuccess;

        public bool IsCorrect => Encoding.ASCII.GetString(TagBytes) == MessageBusService.ServerTag;

    }
}