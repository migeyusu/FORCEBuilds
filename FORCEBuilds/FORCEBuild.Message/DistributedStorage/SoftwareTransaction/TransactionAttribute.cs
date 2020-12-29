using System;

namespace FORCEBuild.Net.DistributedStorage.SoftwareTransaction
{
    /// <summary>
    /// 标识方法为一个事务需要回滚
    /// </summary>
    [AttributeUsage(AttributeTargets.Method,AllowMultiple = false)]
    public class TransactionAttribute:Attribute
    {
        
    }
}