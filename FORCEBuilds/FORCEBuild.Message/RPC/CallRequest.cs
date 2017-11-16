using System;
using System.Reflection;
using FORCEBuild.Message.Base;

namespace FORCEBuild.Message.RPC
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
