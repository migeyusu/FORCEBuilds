namespace FORCEBuild.Crosscutting
{
    public interface IMethodBeforeAdvice
    {
        void Before(System.Reflection.MethodInfo method, object[] args, object target);
    }
}