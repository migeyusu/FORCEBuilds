using System.Runtime.InteropServices;
using System.Text;

namespace FORCEBuild.DistributedStorage.Cache
{
    /// <summary>
    /// 操作广播，从主存读或本地写都会引发广播（实际可能由tcp实现）
    /// </summary>
    public struct OperationBroadcast
    {
        [MarshalAs(UnmanagedType.ByValArray, SizeConst =4)] public byte[] TagBytes;
        public OperationType Operation;
        /// <summary>
        /// 操作的key
        /// </summary>
        public int OperationKey;

        public bool IsCorect => Encoding.ASCII.GetString(TagBytes) == L1Cache.Tag;
    }
}