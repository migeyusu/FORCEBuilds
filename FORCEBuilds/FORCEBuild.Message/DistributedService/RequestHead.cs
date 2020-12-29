using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 通用请求头
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RequestHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ServiceFactory.TAG_LENGTH)] public byte[] TagBytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ServiceFactory.TAG_LENGTH)] public byte[] LeaveLengthBytes;

        public CallType Calltype;

        public int LeaveLength => BitConverter.ToInt32(LeaveLengthBytes, 0);

        public bool IsCorrect => Encoding.Unicode.GetString(TagBytes) == ServiceFactory.TAG;

        public RequestHead(CallType callType,int leaveLength)
        {
            Calltype = callType;
            TagBytes = Encoding.Unicode.GetBytes(ServiceFactory.TAG);
            LeaveLengthBytes = BitConverter.GetBytes(leaveLength);
        }

        //   public static int SelfLength => Marshal.SizeOf(typeof(RequestHead));
    }


}
