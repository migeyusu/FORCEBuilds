namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// 邮箱缓冲区类型
    /// </summary>
    public enum MailBufferType:byte
    {
        /// <summary>
        /// 固定消息，可以重复读取
        /// </summary>
        Static=0,
        /// <summary>
        /// 先进后出，一个消息只能被取出一次
        /// </summary>
        Queue=1,
    }
}