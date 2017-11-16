using System;
using System.IO;
using System.Net;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace FORCEBuild.RPC3._0
{
    [Serializable]
    public class ServiceUri
    {
        public ServiceUriType UriType { get; set; }

        public IPEndPoint EndPoint { get; set; }

        public string Url { get; set; }

    }
}