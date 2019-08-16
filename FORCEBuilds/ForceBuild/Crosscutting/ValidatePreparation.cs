using FORCEBuild.Core;

namespace FORCEBuild.Crosscutting {
    public class ValidatePreparation:IFactoryProxyPreparation
    {
        public void GeneratePreparation(PreProxyEventArgs args)
        {
            args.Interceptors.Add(new ValidateInterceptor());
        }
    }
}