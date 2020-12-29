using System;

namespace FORCEBuild.Net.DistributedService
{

    [Serializable]
    public class ServiceNodeRealTimeInfo:IRealtimeInfo
    {
        public float CpuUsage { get; set; }

        public float RamUsage { get; set; }
    }
}