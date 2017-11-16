namespace FORCEBuild.Message.Remote
{
    /// <summary>
    /// 消息路由规则
    /// </summary>
    public enum MailRoutedType
    {
        /// <summary>
        /// 直接匹配
        /// </summary>
        Direct,
        /// <summary>
        /// 正则匹配，订阅特定规则的消息
        /// </summary>
        Filter,
        /// <summary>
        /// 函数（只允许只用简单类型）
        /// </summary>
        Func,
    }
}