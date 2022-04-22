using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.RPC
{
    //todo:支持方法重载
    
    /// <summary>
    /// 调用请求 （当前不支持方法重载）
    /// </summary>
    [Serializable]
    public class CallRequest : IMessage
    {
        /// <summary>
        /// 用于标识方法名
        /// </summary>
        public string MethodName { get; set; }

        [NonSerialized] private MethodInfo _methodInfo;

        /// <summary>
        /// 方法体
        /// </summary>
        public MethodInfo Method
        {
            set { MethodName = value.Name; }
            get
            {
                if (this._methodInfo == null)
                {
                    this._methodInfo = this.InterfaceType.GetMethod(this.MethodName);
                }

                return _methodInfo;
            }
        }

        /// <summary>
        /// 参数数组
        /// </summary>
        public object[] Parameters { get; set; }


        [NonSerialized] private Type _interfaceType;

        /// <summary>
        /// contract 接口类型
        /// </summary>
        public Type InterfaceType
        {
            set { this.InterfaceTypeFullName = value.AssemblyQualifiedName; }
            get
            {
                if (_interfaceType == null)
                {
                    this._interfaceType = Type.GetType(this.InterfaceTypeFullName);
                }

                return _interfaceType;
            }
        }

        public string InterfaceTypeFullName { get; set; }
    }
}