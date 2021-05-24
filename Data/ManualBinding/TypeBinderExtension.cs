using System;
using System.Linq.Expressions;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    public static class TypeBinderExtension
    {
        public static PropertyAccessor<T, TK> Property<TK, T>(this TypeBinder<T> typeBinder,
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
        }
    }
}