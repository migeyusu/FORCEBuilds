using System;

namespace FORCEBuild.Concurrency
{
    public class ActionActor : Actor<Action>
    {
        protected override void Receive(Action message)
        {
            message?.Invoke();
        }
    }
}