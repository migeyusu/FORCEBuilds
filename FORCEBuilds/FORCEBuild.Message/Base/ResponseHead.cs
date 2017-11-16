using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Message.Base
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct ResponseHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Extra.TagLength)] public byte[] TagBytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Extra.TagLength)] public byte[] LeaveLengthBytes;

        public bool IsCorrect => Encoding.Unicode.GetString(TagBytes) == Extra.Tag;

        public int LeaveLength => BitConverter.ToInt32(LeaveLengthBytes, 0);

        public ResponseHead(int leavelen)
        {
            LeaveLengthBytes = BitConverter.GetBytes(leavelen);
            TagBytes = Encoding.Unicode.GetBytes(Extra.Tag);

        }
    }
}
