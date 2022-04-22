using System;
using System.Linq.Expressions;
using System.Reflection;
using System.Security.Cryptography;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.RPC
{
    //todo:֧�ַ�������
    
    /// <summary>
    /// �������� ����ǰ��֧�ַ������أ�
    /// </summary>
    [Serializable]
    public class CallRequest : IMessage
    {
        /// <summary>
        /// ���ڱ�ʶ������
        /// </summary>
        public string MethodName { get; set; }

        [NonSerialized] private MethodInfo _methodInfo;

        /// <summary>
        /// ������
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
        /// ��������
        /// </summary>
        public object[] Parameters { get; set; }


        [NonSerialized] private Type _interfaceType;

        /// <summary>
        /// contract �ӿ�����
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