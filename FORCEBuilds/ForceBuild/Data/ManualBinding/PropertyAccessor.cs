using System;
using System.Linq.Expressions;
using System.Reflection;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    /// <summary>
    /// 类型的属性绑定
    /// </summary>
    /// <typeparam name="T">期望的绑定class type</typeparam>
    /// <typeparam name="TK">期望绑定的属性 type</typeparam>
    public class PropertyAccessor<T, TK> : IPropertySet<T, TK>, IPropertyGet<T, TK>
    {
        public Expression<Func<T, TK>> Expression { get; }

        public MethodInfo GetMethodInfo { get; private set; }

        public Func<T, TK> GetMethodDelegate { get; private set; }

        public MethodInfo SetMethodInfo { get; private set; }

        public Action<T, TK> SetMethodDelegate { get; private set; }

        public PropertyAccessor(Expression<Func<T, TK>> expression)
        {
            this.Expression = expression;
            this.Build();
        }

        public void Build()
        {
            if (!(Expression.Body is MemberExpression memberExpression))
            {
                throw new Exception("Please use MemberExpression");
            }

            var memberExpressionMember = memberExpression.Member;
            if (!(memberExpressionMember is PropertyInfo propertyInfo))
            {
                throw new Exception($"Please define property expression such as x=>x.y in class {typeof(T).Name}");
            }

            if (!propertyInfo.CanRead)
            {
                throw new Exception($"Property {propertyInfo.Name} should be readable!");
            }

            if (!propertyInfo.CanWrite)
            {
                throw new Exception($"Property {propertyInfo.Name} should be writable!");
            }

            GetMethodInfo = propertyInfo.GetMethod;
            GetMethodDelegate = Delegate.CreateDelegate(typeof(Func<T, TK>), GetMethodInfo) as Func<T, TK>;
            SetMethodInfo = propertyInfo.SetMethod;
            SetMethodDelegate = Delegate.CreateDelegate(typeof(Action<T, TK>), SetMethodInfo) as Action<T, TK>;
        }


        public void Set(T x, TK y)
        {
            SetMethodDelegate.Invoke(x, y);
        }

        public TK Get(T t)
        {
            return GetMethodDelegate.Invoke(t);
        }
    }
}