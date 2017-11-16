using System;
using System.Reflection;

namespace FORCEBuild.RPC2._0.Interface
{
    /// <summary>
    /// 异常捕获处理，默认rethrow
    /// </summary>
    public interface IException
    {
        bool Catch(Exception exception, object source, MethodInfo method);
    }
}