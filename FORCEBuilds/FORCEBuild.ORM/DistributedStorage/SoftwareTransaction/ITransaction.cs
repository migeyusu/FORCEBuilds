namespace FORCEBuild.Persistence.DistributedStorage.SoftwareTransaction
{
    /// <summary>
    /// 上下文事务
    /// </summary>
    public interface ITransaction
    {
        /// <summary>
        /// 进入事务标识
        /// </summary>
        bool IsInTransaction { get; set; }

        /// <summary>
        /// 注册更改
        /// </summary>
        /// <param name="model"></param>
        void Register(ITranscationProducer model, TransactionMemberType type=TransactionMemberType.UPDATE);
        /// <summary>
        /// 提交事务
        /// </summary>
        void Commit();

        /// <summary>
        /// 回滚事务
        /// </summary>
        void Rollback();

        /// <summary>
        /// 持久化接口
        /// </summary>
    //    IPersistence Persistence { get; set; }
    }
}