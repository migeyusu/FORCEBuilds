using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FORCEBuild.Data
{
    
    
    /// <summary>
    /// 类型绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeBinder<T>
    {
        private IList<IInstanceConsumer<T>> _typeBinder = new List<IInstanceConsumer<T>>();

        public void Attach(IInstanceConsumer<T> consumer)
        {
            _typeBinder.Add(consumer);
        }


        public static PropertyAccessor<T, TK> Property<TK>(Expression<Func<T, TK>> expression)
        {
            var propertyBinder = new PropertyAccessor<T, TK>(expression);
            return propertyBinder;
        }
    }
}