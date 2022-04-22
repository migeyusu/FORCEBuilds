using Castle.Windsor;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.RPC
{
    public static class RPCExtension
    {
        public static void AddServiceHandler(this IMessageProcessRoutine routine, IWindsorContainer container)
        {
            var callProducePipeline = new CallProducePipe
            {
                Handler = new ServiceHandler(container)
            };
            if (routine.ProducePipe == null)
            {
                routine.ProducePipe = callProducePipeline;
            }
            else
            {
                routine.ProducePipe.Append(callProducePipeline);
            }
        }
        
        public static void AddPipe(this IMessageProcessRoutine routine,MessagePipe<IMessage,IMessage> pipe)
        {
            if (routine.ProducePipe == null)
            {
                routine.ProducePipe = pipe;
            }
            else
            {
                routine.ProducePipe.Append(pipe);
            }
        }
    }
}