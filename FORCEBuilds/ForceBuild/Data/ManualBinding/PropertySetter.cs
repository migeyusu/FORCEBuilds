using System;
using System.Linq.Expressions;
using System.Reflection;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    public class PropertySetter<T, TK> : IPropertySet<T, TK>
    {
        public MethodInfo SetMethodInfo { get; private set; }

        public Action<T, TK> SetMethodDelegate { get; private set; }

        public PropertySetter(Expression<Func<T, TK>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
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

            SetMethodInfo = propertyInfo.SetMethod;
            SetMethodDelegate = Delegate.CreateDelegate(typeof(Action<T, TK>), SetMethodInfo) as Action<T, TK>;
        }

        public void Set(T x, TK y)
        {
            SetMethodDelegate.Invoke(x, y);
        }
    }
}