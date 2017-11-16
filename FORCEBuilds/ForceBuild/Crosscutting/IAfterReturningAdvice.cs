using System.Reflection;

namespace FORCEBuild.Crosscutting
{
    public interface IAfterReturningAdvice
    {
        void AfterReturning(object returnValue, MethodInfo method, object[] args, object target);
    }
}