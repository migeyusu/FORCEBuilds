using System;
using System.Net;

namespace FORCEBuild.DistributedService
{
    public class ClientDefine
    {
        public Guid Guid { get; set; }

        public IPEndPoint ContainerEndPoint { get; set; }

    }
} 