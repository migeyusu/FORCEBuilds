using Castle.DynamicProxy;
using FORCEBuild.Core;

namespace FORCEBuild.Crosscutting
{
    public class PropertyChangedNotifyPreparation:IFactoryProxyPreparation
    {
        public void GeneratePreparation(PreProxyEventArgs args)
        {
            var notifyBase = new NotifyBase();
            args.Interceptors.Add(new PropertyNotifyInterceptor(notifyBase));
            args.GenerationOptions.AddMixinInstance(notifyBase);
        }
    }
}