﻿namespace FORCEBuild.Persistence.DistributedStorage.SoftwareTransaction
{
    public interface ITranscationProducer:IOrmModel
    {
        /// <summary>
        /// 回滚数据
        /// </summary>
        void Rollback();
    }
}