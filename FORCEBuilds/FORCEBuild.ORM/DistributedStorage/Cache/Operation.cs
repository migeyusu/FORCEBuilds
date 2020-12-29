using System;

namespace FORCEBuild.Persistence.DistributedStorage.Cache
{
    /// <summary>
    /// 
    /// </summary>
    public struct Operation
    {
        public OperationType OperationType { get; set; }

        public int OperationKey { get; set; }

        public Action OperationAction { get; set; }
    }
}