﻿using System.Collections.Generic;
using FORCEBuild.Persistence.DistributedStorage.SoftwareTransaction;

namespace FORCEBuild.Persistence.DistributedStorage.Cache
{
    public interface IStoreage
    {
        void Enqueue(List<TransactionMember> producers);

        void Enqueue(TransactionMember producer);
        /// <summary>
        /// 只针对更新方法
        /// </summary>
        /// <param name="producer"></param>
        void Enqueue(ITranscationProducer producer);

        T Get<T>(int ormid);

        dynamic Get(int ormid, string property);

    }
}