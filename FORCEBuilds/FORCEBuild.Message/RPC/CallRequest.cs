using System;
using System.Reflection;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.RPC
{

    /// <summary>
    /// ��������
    /// </summary>
    [Serializable]
    public class CallRequest:IMessage
    {
        /// <summary>
        /// ������
        /// </summary>
        public MethodInfo Method { get; set; }
        /// <summary>
        /// ��������
        /// </summary>
        public object[] Parameters { get; set; }
        /// <summary>
        /// contract �ӿ�����
        /// </summary>
        public Type InterfaceType { get; set; }

    }
}
