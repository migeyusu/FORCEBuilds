using System;
using FORCEBuild.Message.Base;

namespace FORCEBuild.Message.RPC
{

    public class CallProducePipe: MessagePipe<IMessage,IMessage>
    {
        public Actuator Actuator { get; set; }

        protected override IMessage InternalProcess(IMessage message)
        {
            var callRequest = message as CallRequest;
            if (callRequest != null)
            {
                try
                {
                    var execute = Actuator.Execute(callRequest);
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
            Actuator?.Dispose();
        }
    }
    
}