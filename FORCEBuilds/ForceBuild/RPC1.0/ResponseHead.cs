using System;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.RPC1._0
{
    public struct ResponseHead
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LocalCallFactory.TAG_LENGTH)]
        public byte[] Tag;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = LocalCallFactory.TAG_LENGTH)]
        public byte[] NextLen;
        /// <summary>
        /// 如果否则返回exception
        /// </summary>
        public bool IsProcessSuccess;

        public bool IsCorrect => Encoding.Unicode.GetString(Tag) == LocalCallFactory.TAG;

        public int Len => BitConverter.ToInt32(NextLen, 0);

        public static int SelfLen => Marshal.SizeOf(typeof(ResponseHead));
    }
}
