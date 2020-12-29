using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 通用回复头
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResponseHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ServiceFactory.TAG_LENGTH)] public byte[] TagBytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = ServiceFactory.TAG_LENGTH)] public byte[] LeaveLengthBytes;
        /// <summary>
        /// 如果否则返回exception
        /// </summary>
        public bool IsProcessSuccess;

        public bool IsCorrect => Encoding.Unicode.GetString(TagBytes) == ServiceFactory.TAG;

        public int LeaveLength => BitConverter.ToInt32(LeaveLengthBytes, 0);

        public ResponseHead(bool IsSuccess,int leavelen)
        {
            IsProcessSuccess = IsSuccess;
            LeaveLengthBytes = BitConverter.GetBytes(leavelen);
            TagBytes = Encoding.Unicode.GetBytes(ServiceFactory.TAG);
        }
        //    public static int SelfLength => Marshal.SizeOf(typeof(ResponseHead));
    }
}
