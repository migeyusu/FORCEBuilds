using System;

namespace FORCEBuild.DistributedService
{
    /* 2017.4.2
     * ����soapformatter�������Ƿ��Ͳ���
     * 2017.5.10
     * ����binaryformatter�����Ƿ������
     */
    /// <summary>
    /// ���ýӿڵķ�������
    /// </summary>
    [Serializable]
    public class InterfaceCallRequest:SelfCall
    {
        //public MethodInfo Method { get; set; }
        //public object[] Parameters { get; set; }
        /// <summary>
        /// ���õĽӿ�
        /// </summary>
        public string InterfaceType { get; set; }
    }
}
