using System;
using System.Runtime.Serialization;

namespace FORCEBuild.Serialization
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