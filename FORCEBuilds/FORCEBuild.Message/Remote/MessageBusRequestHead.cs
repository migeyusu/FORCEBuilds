using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Message.Remote
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct MessageBusRequestHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = MessageBusService.RequestTagLength)] public byte[] TagBytes;
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] IpBytes;
        //public short Port;

        public OperationType Request;

        public int LeaveLength;

        //public IPEndPoint EndPoint {
        //    get {
        //        return new IPEndPoint(new IPAddress(IpBytes), Port);
        //    }
        //    set {
        //        this.IpBytes = value.Address.GetAddressBytes();
        //        this.Port = (short)value.Port;
        //    }
        //}

        public bool IsCorrect => Encoding.ASCII.GetString(TagBytes) == MessageBusService.ServerTag;

    }
}