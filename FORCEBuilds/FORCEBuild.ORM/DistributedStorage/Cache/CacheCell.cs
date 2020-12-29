using System;
using System.IO;
using FORCEBuild.Concurrency;

namespace FORCEBuild.Persistence.DistributedStorage.Cache
{
    /// <summary>
    /// 缓存单元，操作不可分割的最小单元
    /// </summary>
    public class CacheCell:Actor<Operation>,IDistributedData
    {
        public MESIStatus PreStatus { get; set; }

        public int SyncKey => throw new NotImplementedException();

        public CacheCell()
        {
            
        }

        protected override void Receive(Operation message)
        {
           
        }

        public Stream Get()
        {
            throw new NotImplementedException();
        }

        public void Create(Stream stream)
        {
            throw new NotImplementedException();
        }
    }
}