namespace FORCEBuild.Persistence.DistributedStorage.Cache
{
    public enum MESIStatus
    {
        /// <summary>
        /// 此cache行已被修改过（脏行），内容已不同于主存并且 为此cache专有；
        /// </summary>
        Modified,
        /// <summary>
        /// 此cache行内容同于主存，但不出现于其它cache中； 
        /// </summary>
        Exclusive,
        /// <summary>
        /// 此cache行内容同于主存，但也出现于其它cache中； 
        /// </summary>
        Shared,
        /// <summary>
        /// 此cache行内容无效（空行），被当做从来没有在cache出现过。 
        /// </summary>
        Invalid
    }

}