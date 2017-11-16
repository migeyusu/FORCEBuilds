using System;
using System.Collections.Generic;
using Castle.DynamicProxy;

namespace FORCEBuild.Core
{
    public class GenerateEventArgs:EventArgs
    {
        public Type ToProxyType { get; set; }

        public List<IInterceptor> Interceptors { get; set; }

        public List<Type> AdditionalInterfacesToProxy { get; set; }

        public ProxyGenerationOptions GenerationOptions { get; set; }
    }
}