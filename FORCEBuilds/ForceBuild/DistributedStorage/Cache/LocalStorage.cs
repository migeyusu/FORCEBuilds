using System.Collections.Generic;
using FORCEBuild.DistributedStorage.Transaction;

namespace FORCEBuild.DistributedStorage.Cache
{
    public class LocalStorage:IStoreage
    {

        public void Enqueue(List<TransactionMember> producers)
        { }

        public void Enqueue(TransactionMember producer)
        { }

        public void Enqueue(ITranscationProducer producer)
        { }

        public T Get<T>(int ormid)
        {
            return default(T);
        }

        public dynamic Get(int ormid, string property)
        {
            return null;
        }
    }
}