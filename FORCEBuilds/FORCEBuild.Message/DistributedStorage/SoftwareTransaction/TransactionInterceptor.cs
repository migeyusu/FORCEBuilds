using System;
using System.Reflection;
using Castle.DynamicProxy;

namespace FORCEBuild.Net.DistributedStorage.SoftwareTransaction
{
    public class TransactionInterceptor:IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            var attribute = invocation.Method.GetCustomAttribute<TransactionAttribute>();
            if (attribute!=null) {
                var context = TransactionThreadContext.Current;
                context.Transaction.IsInTransaction = true;
                try {
                    invocation.Proceed();
                    /* 提交阶段，此时分布式缓存会检查离线并发锁
                     * 提交成功则写入缓存，失败则回滚
                     */
                    context.Transaction.Commit();
                }
                catch (Exception) {
                    context.Transaction.Rollback();
                    throw;
                }
                finally {
                    context.Transaction.IsInTransaction = false;
                }
            }
            else {
                invocation.Proceed();
            }
            
        }
    }
}