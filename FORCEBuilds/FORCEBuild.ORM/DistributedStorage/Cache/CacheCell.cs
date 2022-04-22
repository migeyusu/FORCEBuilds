using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Concurrency;

namespace FORCEBuild.Persistence.DistributedStorage.Cache
{
    /// <summary>
    /// 缓存单元，操作不可分割的最小单元
    /// </summary>
    public class CacheCell:TaskBasedActor<Operation>,IDistributedData
    {
        public MESIStatus PreStatus { get; set; }

        public int SyncKey => throw new NotImplementedException();



        public Stream Get()
        {
            throw new NotImplementedException();
        }

        public void Create(Stream stream)
        {
            throw new NotImplementedException();
        }

        protected override Task ReceiveMessage(Operation message, CancellationToken token)
        {
            throw new NotImplementedException();
        }

        public CacheCell(TaskScheduler scheduler) : base(scheduler)
        {
        }
    }
}