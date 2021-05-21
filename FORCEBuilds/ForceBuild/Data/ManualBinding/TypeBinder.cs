using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    public class DelegateConsumer
    {
        
    }
    /// <summary>
    /// 类型绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeBinder<T>
    {
        protected readonly IList<IInstanceConsumer<T>> Consumers = new List<IInstanceConsumer<T>>();

        public void Attach<TK>(Func<TypeBinder<T>,IInstanceConsumer<T>> configFunc)
        {
            var instanceConsumer = configFunc.Invoke(this);
            Consumers.Add(instanceConsumer);
        }

        public void Attach<K>()
        {
            
        }

        
        /*public static void ToProvider<TK>(Expression<Func<T, TK>> expression)
        {
            
        }*/
        
        public void Attach(IInstanceConsumer<T> consumer)
        {
            Consumers.Add(consumer);
        }

        public static PropertyAccessor<T, TK> Property<TK>(Expression<Func<T, TK>> expression)
        {
            return new PropertyAccessor<T, TK>(expression);
        }

        public static IPropertySet<T, TK> Setter<TK>(Expression<Func<T, TK>> expression)
        {
            return new PropertySetter<T, TK>(expression);
        }

        public static IPropertyGet<T, TK> Getter<TK>(Expression<Func<T, TK>> expression)
        {
            return new PropertyGetter<T, TK>(expression);
        }
    }


    public static class TypeBinderExtension
    {
        /*public static PropertyAccessor<T, TK> Property<TK, T>(this TypeBinder<T> typeBinder,
            Expression<Func<T, TK>> expression)
        {
            return new PropertyAccessor<T, TK>(expression);
        }

        public static IPropertySet<T, TK> Setter<TK, T>(this TypeBinder<T> typeBinder,
            Expression<Func<T, TK>> expression)
        {
            return new PropertySetter<T, TK>(expression);
        }

        public static IPropertyGet<T, TK> Getter<TK, T>(this TypeBinder<T> typeBinder,
            Expression<Func<T, TK>> expression)
        {
            return new PropertyGetter<T, TK>(expression);
        }*/
    }
}