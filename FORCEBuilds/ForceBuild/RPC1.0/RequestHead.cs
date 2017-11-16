using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.RPC1._0
{
    public struct RequestHead
    {
        [MarshalAs(UnmanagedType.ByValArray,SizeConst = LocalCallFactory.TAG_LENGTH)]
        public byte[] Tag;
        [MarshalAs(UnmanagedType.ByValArray,SizeConst = LocalCallFactory.TAG_LENGTH)]
        public byte[] NextLen;

        public CallType Calltype;

        public int Len => BitConverter.ToInt32(Tag, 0);

        public bool IsCorrect => Encoding.Unicode.GetString(Tag) == LocalCallFactory.TAG;

        public static int SelfLen => Marshal.SizeOf(typeof(RequestHead));
    }


}
