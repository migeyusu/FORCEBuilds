using System.IO;

namespace FORCEBuild.Net.DistributedStorage
{
    /// <summary>
    /// 分布式数据的共同接口
    /// </summary>
    public interface IDistributedData
    {
        /// <summary>
        /// 该键为唯一键，支持从落地数据到分布式缓存的同步
        /// </summary>
        int SyncKey { get;}
        /// <summary>
        /// 生成用于传输的流
        /// </summary>
        /// <returns></returns>
        Stream Get();
        /// <summary>
        /// 根据流创建
        /// </summary>
        /// <param name="stream"></param>
        void Create(Stream stream);
    }
}