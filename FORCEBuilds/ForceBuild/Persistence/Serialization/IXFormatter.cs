using System;
using System.Runtime.Serialization;
using System.Security.Cryptography.X509Certificates;

namespace FORCEBuild.Persistence.Serialization
{
    /// <summary>
    /// 支持构造函数
    /// </summary>
    public interface IXFormatter:IFormatter
    {
        /// <summary>
        /// 简单工厂
        /// </summary>
        Func<Type, object> InstanceCreateFunc { get; set; }
    }
}