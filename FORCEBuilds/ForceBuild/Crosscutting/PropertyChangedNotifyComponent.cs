using Castle.DynamicProxy;
using FORCEBuild.Core;

namespace FORCEBuild.Crosscutting
{
    public class PropertyChangedNotifyComponent:FactoryComponent
    {
        public override void GeneratePreparation(GenerateEventArgs args)
        {
            var notifyBase = new NotifyBase();
            args.Interceptors.Add(new PropertyNotifyInterceptor(notifyBase));
            args.GenerationOptions.AddMixinInstance(notifyBase);
        }
    }

    public class ValidateComponent:FactoryComponent
    {
        public override void GeneratePreparation(GenerateEventArgs args)
        {
            args.Interceptors.Add(new ValidateInterceptor());
        }
    }
}