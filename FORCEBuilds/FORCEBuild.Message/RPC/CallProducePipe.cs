using System;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.RPC
{
    public class CallProducePipe : MessagePipe<IMessage, IMessage>
    {
        public CallProducePipe(ServiceHandler handler)
        {
            Handler = handler;
        }

        public ServiceHandler Handler { get; }

        protected override IMessage InternalProcess(IMessage message)
        {
            var callRequest = message as CallRequest;
            if (callRequest != null)
            {
                try
                {
                    var execute = Handler.Handle(callRequest);
                    return new CallResponse
                    {
                        IsProcessSucceed = true,
                        Transfer = execute
                    };
                }
                catch (Exception exception)
                {
                    return new CallResponse
                    {
                        IsProcessSucceed = false,
                        Transfer = exception.InnerException ?? exception
                    };
                }
            }

            return message;
        }

        public override void Dispose()
        {
            base.Dispose();
        }
    }
}