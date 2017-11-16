using System;
using System.ComponentModel;
using Castle.DynamicProxy;
using FORCEBuild.Core.Interceptors;

namespace FORCEBuild.UI.WPF
{
    public class ViewModelFactory
    {
        private static readonly ProxyGenerator proxyGenerator = new ProxyGenerator();

        public static T Get<T>(params object[] objects)
        {
            if (objects.Length == 0)
            {
                return (T) proxyGenerator.CreateClassProxy(typeof(T), new[] {typeof(INotifyPropertyChanged)},
                    new ViewModelInterceptor());
            }
            return (T) proxyGenerator.CreateClassProxy(typeof(T), new[] {typeof(INotifyPropertyChanged)},
                ProxyGenerationOptions.Default, objects, new ViewModelInterceptor());
        }

        public static object Get(Type type)
        {
            return proxyGenerator.CreateClassProxy(type, new[] {typeof(INotifyPropertyChanged)},
                new ViewModelInterceptor());
        }

        public static object Get(object oc,Type type)
        {
           var val=  proxyGenerator.CreateClassProxy(type, new[] { typeof(INotifyPropertyChanged) },
                new ViewModelInterceptor());
            PropertyInject(oc, val, type);
            return val;
        }

        public static void PropertyInject(object ori,object value,Type type)
        {
            foreach (var property in type.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                {
                    property.SetValue(value, property.GetValue(ori));
                }
            }
        }
        
    }
}
