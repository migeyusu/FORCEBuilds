using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Net.Base
{
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    public struct RequestHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Extra.TagLength)] public byte[] TagBytes;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = Extra.TagLength)] public byte[] LeaveLengthBytes;
        
        public int LeaveLength => BitConverter.ToInt32(LeaveLengthBytes, 0);

        public bool IsCorrect => Encoding.Unicode.GetString(TagBytes) == Extra.Tag;

        public RequestHead(int leaveLength)
        {
            TagBytes = Encoding.Unicode.GetBytes(Extra.Tag);
            LeaveLengthBytes = BitConverter.GetBytes(leaveLength);
        }
    }


}
