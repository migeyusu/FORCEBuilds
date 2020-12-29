using System.Collections.Generic;
using FORCEBuild.Persistence.DistributedStorage.SoftwareTransaction;

namespace FORCEBuild.Persistence.DistributedStorage.Cache
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