using System;

namespace FORCEBuild.DistributedService
{

    [Serializable]
    public class ServiceNodeRealTimeInfo:IRealtimeInfo
    {
        public float CpuUsage { get; set; }

        public float RamUsage { get; set; }
    }
}