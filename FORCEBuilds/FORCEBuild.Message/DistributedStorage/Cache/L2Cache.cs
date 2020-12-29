using System.Collections.Concurrent;

namespace FORCEBuild.Net.DistributedStorage.Cache
{
    /* 分布式缓存采用包含式，二级缓存包含所有一级缓存的内容
     */
    public class L2Cache
    {
        public const string MESI = "MESIEndPoint";

        public ConcurrentDictionary<int,IDistributedData> AllDatas { get; set; }

        private bool work, working;

        public L2Cache()
        {
            AllDatas = new ConcurrentDictionary<int, IDistributedData>(8192, 8192);
        }



    }
}