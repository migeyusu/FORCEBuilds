using System;
using System.Collections.Generic;
using System.Reflection;
using FORCEBuild.RPC2._0.Interface;
using Xunit;

namespace FORCEBuild.RPC2._0
{
    public class ThrowException:IException
    {
        public bool Catch(Exception exception, object source, MethodInfo method)
        {
            throw exception;
        }

    }
}