using System;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using Castle.DynamicProxy;

namespace FORCEBuild.Persistence
{
    [Serializable]
    public class OrmInterceptor : IInterceptor
    {
        internal ConcurrentDictionary<string, NotifyProperty> NotifyProperties { get; set; }
        /// <summary>
        /// 是否允许记录属性更改
        /// </summary>
        public bool IsRecordable { get; set; }

        /*全生命周期；在orm1.0阶段（2016中-2017上旬），该值类型为int，来自数据库自动生成，为了保持前向兼容，保留该属性*/

        public Guid ORMID { get; set; }

        //经过selector选择过滤
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            if (invocation.Method.Name.StartsWith("set_")) {
                var methodName = invocation.Method.Name.Substring(4);
                if (NotifyProperties.ContainsKey(methodName)) {
                    var property = NotifyProperties[methodName];
                    if (IsRecordable)
                        property.IsChanged = true;
                    if (property.PropertyElement.RelationType == RelationType.OneToMany ||
                        property.PropertyElement.RelationType == RelationType.ManyToMany) {
                        var val = property.PropertyElement.PropertyInfo.GetValue(invocation.Proxy);
                        if (val != null) {
                            ((INotifyCollectionChanged) val).CollectionChanged += (o, e) => {
                                if (IsRecordable) {
                                    if (e.Action == NotifyCollectionChangedAction.Move) return;
                                     property.OperatersList.Add(e);
                                }
                            };
                        }
                    }
                }
            }
        }

    }
}
