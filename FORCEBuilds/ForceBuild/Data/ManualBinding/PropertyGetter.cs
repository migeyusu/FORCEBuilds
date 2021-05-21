using System;
using System.Linq.Expressions;
using System.Reflection;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    public class PropertyGetter<T,TK>: IPropertyGet<T,TK>
    {

        public Expression<Func<T, TK>> Expression { get; }
        
        public MethodInfo GetMethodInfo { get; private set; }

        public Func<T, TK> GetMethodDelegate { get; private set; }

        public PropertyGetter(Expression<Func<T, TK>> expression)
        {
            Expression = expression;
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
            
            GetMethodInfo = propertyInfo.GetMethod;
            GetMethodDelegate = Delegate.CreateDelegate(typeof(Func<T, TK>), GetMethodInfo) as Func<T, TK>;
        }

        public TK Get(T t)
        {
            return GetMethodDelegate.Invoke(t);
        }
    }
}