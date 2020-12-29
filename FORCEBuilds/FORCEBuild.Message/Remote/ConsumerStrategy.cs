using System;

namespace FORCEBuild.Net.Remote
{
    /// <summary>
    /// 消息消费策略
    /// </summary>
    [Serializable]
    public class ConsumerStrategy
    {
        /*2017.11版本：只支持片旬，不支持事务和持久化*/
        /// <summary>
        /// 是否支持持久化
        /// </summary>
        public bool IsPersistence { get; set; }
        /// <summary>
        /// 是否支持事务拉取
        /// </summary>
        public bool IsTransaction { get; set; }
        /// <summary>
        /// 是否使用事件推送，否表示片旬
        /// </summary>
        public bool IsEvent { get; set; }

        private static ConsumerStrategy _defaultStrategy;

        public static ConsumerStrategy Default => _defaultStrategy ?? (_defaultStrategy = new ConsumerStrategy());
   
    }
}