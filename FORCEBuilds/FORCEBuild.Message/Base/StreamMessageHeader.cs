using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Net.Base
{
    //为了pack对齐设置为int而不是byte

//todo:是否需要标准回复体，类似httpcode

    /// <summary>
    /// 基于流传输的消息头
    /// </summary>
    [StructLayout(LayoutKind.Sequential, Pack = 4, Size = 8)]
    public struct StreamMessageHeader
    {
        /// <summary>
        /// 表示无限长度
        /// </summary>
        public const int InfiniteLength = 0;

        /// <summary>
        /// 头标记
        /// </summary>
        public const string HeaderID = "RQHD";

        public const int HeaderSize = 8;

        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)]
        public byte[] Header;

        public int Length;

        public StreamMessageHeader(int length)
        {
            this.Length = length;
            this.Header = Encoding.ASCII.GetBytes(HeaderID);
        }

        public bool Verify()
        {
            var bytes = Encoding.ASCII.GetBytes(HeaderID);
            return bytes.SequenceEqual(Header);
        }
    }
}