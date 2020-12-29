using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.Net.DistributedStorage.Cache
{
    /// <summary>
    /// 来自二级缓存的请求
    /// </summary>
    public struct L1Request
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 4)] public byte[] TagBytes;

        public bool IsCorect => Encoding.ASCII.GetString(TagBytes) == L1Cache.Tag1;
        
    }
}