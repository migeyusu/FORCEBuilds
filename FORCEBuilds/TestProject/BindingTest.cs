using System;
using System.Diagnostics;
using System.Linq.Expressions;
using System.Reflection;
using Castle.Windsor;
using FORCEBuild.Data;
using FORCEBuild.Data.ManualBinding;
using Moq;
using Xunit;

namespace TestProject
{
    public class BindingTest
    {
        [Fact]
        public void TestBinding()
        {
            /* var mock = new Mock<Poco2>();
             var setupSetter = mock.SetupSet(new Action<Poco2>((poco2 => poco2.D)));
             var typeMappingBridge = TypeBinder<Poco>.Property(poco => poco.S)
                 .OneWayTo(() =>
                 {
                     return TypeBinder<Poco2>.Property(poco2 => poco2.S)
                         .SetTo(new DefaultInstanceFactory<Poco2>(new Poco2()));
                 });
                 typeMappingBridge.Consume(new Poco());
             */
            /*var poco3 = new Poco2(1d);
            var typeBinder = new TypeBinder<Poco>();
            var observableValueProvider = typeBinder.Property(poco => poco.S)
                .ToObservable();
            observableValueProvider
                .SubscribeBy((Poco2 poco2) => poco2.S)
                .OnInstance(poco3);
            observableValueProvider.Consume(new Poco());
            var binder = new TypeBinder<Poco2>();
            var propertyBridge = typeBinder.Property((poco => poco.S))
                .Bridge(binder,binder1 => binder1.Property(poco2 => poco2.S));
            propertyBridge.BridgeFrom();*/
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