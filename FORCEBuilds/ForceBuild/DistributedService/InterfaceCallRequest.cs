using System;

namespace FORCEBuild.DistributedService
{
    /* 2017.4.2
     * 改用soapformatter，不考虑泛型参数
     * 2017.5.10
     * 改用binaryformatter，考虑泛型情况
     */
    /// <summary>
    /// 调用接口的方法请求
    /// </summary>
    [Serializable]
    public class InterfaceCallRequest:SelfCall
    {
        //public MethodInfo Method { get; set; }
        //public object[] Parameters { get; set; }
        /// <summary>
        /// 调用的接口
        /// </summary>
        public string InterfaceType { get; set; }
    }
}
