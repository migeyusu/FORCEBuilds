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
