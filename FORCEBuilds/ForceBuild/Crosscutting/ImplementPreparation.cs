using System;
using FORCEBuild.Core;

namespace FORCEBuild.Crosscutting
{
    public class ImplementPreparation:IFactoryProxyPreparation
    {
        private readonly Action<PreProxyEventArgs> _agentPreparation;

        private ImplementPreparation(Action<PreProxyEventArgs> agentPreparation)
        {
            _agentPreparation = agentPreparation;
        }
        
        public void GeneratePreparation(PreProxyEventArgs args)
        {
            _agentPreparation.Invoke(args);
        }
            
        public static IFactoryProxyPreparation Create(Action<PreProxyEventArgs> action)
        {
            return new ImplementPreparation(action);
        }
    }
}
