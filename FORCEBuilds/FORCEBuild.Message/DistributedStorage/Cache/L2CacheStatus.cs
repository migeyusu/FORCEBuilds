namespace FORCEBuild.Net.DistributedStorage.Cache
{
    public enum L2CacheStatus
    {
        /// <summary>
        /// 已被读取过，读取后为共享态
        /// </summary>
        Readed,
        /// <summary>
        /// 当前正在被修改，但未写回
        /// </summary>
        Modifing,
        /// <summary>
        /// 当前未被读过，一级缓存读取后直接为独占
        /// </summary>
        UnRead,
    }
}