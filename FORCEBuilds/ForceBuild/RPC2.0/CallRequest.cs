using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using FORCEBuild.Message.Base;
using FORCEBuild.Persistence.Serialization;
using Xunit;

namespace FORCEBuild.RPC2._0
{

    /// <summary>
    /// 调用请求
    /// </summary>
    [Serializable]
    public class CallRequest:IMessage
    {
        /// <summary>
        /// 方法体
        /// </summary>
        public MethodInfo Method { get; set; }
        /// <summary>
        /// 参数数组
        /// </summary>
        public object[] Parameters { get; set; }
        /// <summary>
        /// contract 接口类型
        /// </summary>
        public Type InterfaceType { get; set; }

    }
}
