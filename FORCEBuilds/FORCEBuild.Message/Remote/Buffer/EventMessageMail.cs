using System.Collections.Generic;
using FORCEBuild.Message.Base;
using FORCEBuild.Message.Remote.DataTransferObject;

namespace FORCEBuild.Message.Remote.Buffer
{
    public class EventMessageMail:IMessageMail<IMessage>
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