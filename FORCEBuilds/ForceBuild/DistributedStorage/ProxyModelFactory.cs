using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using Castle.DynamicProxy;
using FORCEBuild.Core.Interceptors;

namespace FORCEBuild.ORM
{
    public class ProxyModelFactory:ModelFactory
    {
        public Dictionary<Type, ClassDefine> DefinePairs { get; set; }

        public UpdateScheduler Scheduler { get; set; }
        //可供创建的类
        private readonly ProxyGenerator _pg = new ProxyGenerator();

        public object Create(ClassDefine define,bool cannotify=true)
        {
            var dic = define.AllProperties.ToDictionary(property => property.Key, property => new NotifyProperty {PropertyElement = property.Value});
             //防止初始化时notifyproperties为空，无法挂载事件
            var instance = (IOrmCell) _pg.CreateClassProxy(define.ClassType, new[] {typeof(IOrmCell)},
                new OrmInterceptor(cannotify,dic,Scheduler,define));
            //初始化propertydefine
            return instance;
        }

        public  T Get<T>()
        {
            if (!DefinePairs.ContainsKey(typeof(T)))
                throw new ArgumentException("未定义的类，无法创建实例");
            var type = typeof(T);
            return (T)Create(DefinePairs[type]);
        }
    }
}
