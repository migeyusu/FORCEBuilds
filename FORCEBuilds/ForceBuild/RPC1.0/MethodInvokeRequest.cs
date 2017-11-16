using System;
using System.Reflection;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.RPC1._0
{
    /// <summary>
    /// 带了参数和方法名称
    /// </summary>
    [XSoapRoot]
    public class MethodInvokeRequest
    {
        [XSoapElement]
        public MethodInfo Method { get; set; }
        [XSoapElement]
        public Guid SyncGuid { get; set; }
        [XSoapElement]
        public object[] Parameters { get; set; }
        [XSoapElement]
        public Type TargetType { get; set; }
    }
}
