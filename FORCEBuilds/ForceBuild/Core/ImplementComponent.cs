using System;

namespace FORCEBuild.Core
{
    public class ImplementComponent:FactoryComponent
    {
        private Action<GenerateEventArgs> _agentPreparation;

        private ImplementComponent(Action<GenerateEventArgs> agentPreparation)
        {
            _agentPreparation = agentPreparation;
        }
        
        public override void GeneratePreparation(GenerateEventArgs args)
        {
            _agentPreparation.Invoke(args);
        }
            
        public static FactoryComponent Create(Action<GenerateEventArgs> action)
        {
            return new ImplementComponent(action);
            
        }
    }
}
