namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// ���仺��������
    /// </summary>
    public enum MailBufferType:byte
    {
        /// <summary>
        /// �̶���Ϣ�������ظ���ȡ
        /// </summary>
        Static=0,
        /// <summary>
        /// �Ƚ������һ����Ϣֻ�ܱ�ȡ��һ��
        /// </summary>
        Queue=1,
    }
}