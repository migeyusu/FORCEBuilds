using System;
using System.Security.Cryptography;
using System.Text;
using Castle.Windsor;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.RPC
{
    public static class RPCExtension
    {
        public static void AddServiceHandler(this IMessageProcessRoutine routine, IWindsorContainer container)
        {
            var callProducePipeline = new CallProducePipe(new ServiceHandler(container));
            if (routine.ProducePipe == null)
            {
                routine.ProducePipe = callProducePipeline;
            }
            else
            {
                routine.ProducePipe.Append(callProducePipeline);
            }
        }

        public static void AddPipe(this IMessageProcessRoutine routine, MessagePipe<IMessage, IMessage> pipe)
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

/*need for compute hash of type?*/
        /*static SHA1 _sha1 = SHA1.Create();

        public static int GetTypeHash(Type type)
        {
            Encoding.ASCII.
            _sha1.ComputeHash()
            type.FullName
            
        }*/
    }
}