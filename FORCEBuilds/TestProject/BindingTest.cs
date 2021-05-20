
using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Windsor;
using FORCEBuild.Data;
using Moq;
using Xunit;

namespace TestProject
{
    public class BindingTest
    {
        [Fact]
        public void TestBinding()
        {
            ExpressionTest((poco2 => poco2.D));
            
           /* var mock = new Mock<Poco2>();
            var setupSetter = mock.SetupSet(new Action<Poco2>((poco2 => poco2.D)));
            var typeMappingBridge = TypeBinder<Poco>.Property(poco => poco.S)
                .OneWayTo(() =>
                {
                    return TypeBinder<Poco2>.Property(poco2 => poco2.S)
                        .SetTo(new DefaultInstanceFactory<Poco2>(new Poco2()));
                });
            typeMappingBridge.Consume(new Poco());
            TypeBinder<Poco>.Property(poco => poco.S)
                .ToObservable()
                .OnBind<Poco,string,Poco2>((poco2 => poco2.D));*/
           
        }


        public static void ExpressionTest(Expression<Func<Poco2,double>> expression)
        {
            if (!(expression.Body is MemberExpression memberExpression))
            {
                throw new Exception("Please use MemberExpression");
            }

            var memberExpressionMember = memberExpression.Member;
            if (!(memberExpressionMember is PropertyInfo propertyInfo))
            {
                throw new Exception($"Please define property expression such as x=>x.y in class {typeof(Poco2).Name}");
            }

            if (!propertyInfo.CanRead)
            {
                throw new Exception($"Property {propertyInfo.Name} should be readable!");
            }

            var propertyInfoGetMethod = propertyInfo.GetMethod;
            var @delegate = Delegate.CreateDelegate(typeof(Func<Poco2, double>), propertyInfoGetMethod) as Func<Poco2, double>;
            var invoke = @delegate.Invoke(new Poco2(2d));
            
            Debugger.Break();
        }

    }

    public class Poco
    {

        public string S { get; set; }
    }

    public class Poco2
    {
        public Poco2(double d)
        {
            D = d;
        }

        public string S { get; set; }
        
        public double D { get; }
    }
}