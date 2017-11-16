using System;

namespace FORCEBuild.RPC1._0
{
    public class InstantiationRequest
    {
        public Type ClassType{ get; set; }
        public bool IsSingleton { get; set; }
        public Guid SyncGuid { get; set; }
    }

}
