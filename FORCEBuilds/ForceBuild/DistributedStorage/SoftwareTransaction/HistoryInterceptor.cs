using System.Collections.Generic;
using System.Collections.Specialized;
using Castle.DynamicProxy;
using FORCEBuild.Core.Interceptors;
using FORCEBuild.DistributedStorage.Cache;
using FORCEBuild.ORM;

namespace FORCEBuild.DistributedStorage.Transaction
{
    public class HistoryInterceptor : IInterceptor, ITranscationProducer
    {
        internal Dictionary<string, HistoryProperty> HistoryProperties { get; set; }

        public IStoreage Storeage { get; set; }

        public void Intercept(IInvocation invocation)
        {
            var method = invocation.Method.Name;
            if (method.StartsWith("get_")) {
                //延迟加载
                var property = HistoryProperties[method.Substring(4)];
                if (!property.IsInitialize) {
                   property.PropertyInfo.SetValue(invocation.Proxy, Storeage.Get(ORMID, method.Substring(4)));
                    property.IsInitialize = true;
                }
                invocation.Proceed();
            }
            else if (method.StartsWith("set_")) {
                var property = HistoryProperties[method.Substring(4)];
                var transaction = TransactionThreadContext.Current.Transaction;
                var propertyinfo = property.PropertyInfo;
                invocation.Proceed();
                //属性变化
                property.IsChanged = true; 
                var producer = invocation.Proxy as ITranscationProducer;
                if (property.RelationType == RelationType.OneToMany ||
                    property.RelationType == RelationType.ManyToMany) {
                    var val = propertyinfo.GetValue(invocation.Proxy);
                    if (val != null) {
                        ((INotifyCollectionChanged) val).CollectionChanged += (o, e) => {
                            property.EventArgses.Add(e);
                            if (transaction.IsInTransaction) {
                                transaction.Register(producer);
                            }
                            else {
                                Storeage.Enqueue(producer);
                                property.PreValue = propertyinfo.GetValue(invocation.Proxy);
                            }
                        };
                    }
                }
                if (transaction.IsInTransaction) {
                    transaction.Register(producer);
                }
                //每次变化后都获取当前数据，如果在进入会话后获取之前数据，不确定上一次是否不在会话中
                else
                {
                    Storeage.Enqueue(producer);
                    property.PreValue = propertyinfo.GetValue(invocation.Proxy);
                }
            }
            else {
                invocation.Proceed();
            }
        }

        //new TransactionMember()
        //{
        //    MemberType = TransactionMemberType.UPDATE,
        //    Producer = producer && HistoryProperties.ContainsKey(method.Substring(4))
        //}

        public void Rollback()
        {
            

        }

        public ClassDefine ClassDefine { get; set; }

        public bool IsInTask { get; set; }
        public OrmInterceptor OrmInterceptor { get; set; }

        public int ORMID { get; set; }
    }
}