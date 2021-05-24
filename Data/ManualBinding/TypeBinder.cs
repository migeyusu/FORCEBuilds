using System.Collections.Generic;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    /// <summary>
    /// 类型绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeBinder<T>: IInstanceConsumer<T>
    {
        protected readonly IList<IInstanceConsumer<T>> Consumers = new List<IInstanceConsumer<T>>();
        
        public void Attach(IInstanceConsumer<T> consumer)
        {
            Consumers.Add(consumer);
        }

        public virtual void Consume(T t)
        {
            foreach (var consumer in Consumers)
            {
                consumer.Consume(t);
            }
        }
    }
}