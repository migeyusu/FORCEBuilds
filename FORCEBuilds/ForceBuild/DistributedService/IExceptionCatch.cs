using System;
using System.Reflection;

namespace FORCEBuild.DistributedService
{
    public interface IExceptionCatch
    {
        bool Catch(Exception exception, object source, MethodInfo method);
    }
}