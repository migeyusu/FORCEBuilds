using FORCEBuild.ORM;

namespace FORCEBuild.DistributedStorage.Transaction
{
    public interface ITranscationProducer:IOrmModel
    {
        /// <summary>
        /// 回滚数据
        /// </summary>
        void Rollback();
    }
}