using System.Collections.Generic;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.MessageBus.DataTransferObject;

namespace FORCEBuild.Net.MessageBus.Buffer
{
    internal class EventMessageMail:IMessageMail<IMessage>
    {
        public MessageRouteStrategy MailStrategy { get; set; }

        public void Post(IMessage message)
        { }

        public bool IsMatch(string topic)
        {
            return false;
        }

        public IEnumerable<IMessage> Pull(IMessageTransferRequest request)
        {
            yield break;
        }
    }
}