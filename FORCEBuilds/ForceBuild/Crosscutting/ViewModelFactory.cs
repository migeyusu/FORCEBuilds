using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using FORCEBuild.Core;

namespace FORCEBuild.AOP
{
    public class ViewModelFactory
    {
        private static readonly ProxyGenerator _pg = new ProxyGenerator();
        public static T GetInstance<T>()
        {
            return (T) _pg.CreateClassProxy(typeof(T), new NotifyInterceptor());
        }
    }
}
