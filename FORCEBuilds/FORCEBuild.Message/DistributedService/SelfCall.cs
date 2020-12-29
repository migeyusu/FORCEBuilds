using System;
using System.Net;
using System.Reflection;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 调用指定对象的方法
    /// </summary>
    [Serializable]
    public class SelfCall
    {
        public MethodInfo Method { get; set; }

        public object[] Parameters { get; set; }

        [Fact]
        public void FactMethodName()
        {
            var ipendpoint = new IPEndPoint(IPAddress.Broadcast, 200);
            var ip2 = new IPEndPoint(IPAddress.Broadcast, 200);
            Assert.True(Equals(ipendpoint, ip2));
        }
    }
}