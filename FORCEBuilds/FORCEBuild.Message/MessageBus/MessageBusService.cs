using System.Collections.Concurrent;
using Castle.MicroKernel.Registration;
using Castle.Windsor;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.MessageBus.Buffer;
using FORCEBuild.Net.RPC;

namespace FORCEBuild.Net.MessageBus
{
    /* 消息总线服务端分为路由（Routed，类似RabbitMQ的Exchange）和消息策略两大部分
     * 路由机制是类似频道或正则字符串实现的发送频道规则
     * 消息策略是确定消息缓冲区类型和行为的规则
     * 
     * 服务端路由消息到具有具体处理机制的"邮箱"，邮箱根据消息处理规则决定是否持久化这些消息
     * 客户端向邮箱发送获取请求，邮箱根据消息处理规则决定是否弹出消息或使用指针跟踪不同的消息获取
     * 2017.10.21 整个过程以轻量化实现，不考虑持久化，不具有使用指针读取消息的能力
     * 对于邮箱的定义，从Provider-Consumer的角度看，相当于远程Consumer的代理或推送引擎
     * 消息服务类【MessageBusService】可以在.Net工程内部署，用于直接调用，也可以部署在不同的物理层使用Client访问。
     * 
     * */

    /// <summary>
    /// 消息总线远程服务
    /// </summary>
    public class MessageBusService : ITcpMessageCentralizedService
    {
        private readonly ConcurrentDictionary<string, IMessageMail<IMessage>> _concurrentDictionary 
            = new ConcurrentDictionary<string, IMessageMail<IMessage>>();

        /// <summary>
        /// 
        /// </summary>
        /// <param name="routine"></param>
        /// <param name="deployIndependently">是否独立部署。是：将在内部创建RPC
        /// 否：需要注册本服务接口【ITcpMessageCentralizedService】到外部RPC</param>
        public MessageBusService(IMessageProcessRoutine routine, bool deployIndependently = true)
        {
            MessagePipe<IMessage, IMessage> usePipe = new ConsumerProducePipe {
                MessageMails = this._concurrentDictionary
            };
            if (deployIndependently)
            {
                IWindsorContainer container = new WindsorContainer();
                container.Register(Component.For<ITcpMessageCentralizedService>()
                    .Instance(this));
                var serviceHandler = new ServiceHandler(container);
                var callProducePipe = new CallProducePipe {
                    Handler = serviceHandler
                };
                usePipe = usePipe.Append(callProducePipe);
            }
            routine.AddPipe(usePipe);
        }

        public void SendMessage<T>(T x, string topic) where T : IMessage
        {
            foreach (var mail in _concurrentDictionary.Values) {
                if (mail.IsMatch(topic)) {
                    mail.Post(x);
                }
            }
        }

        public void RegisterOrUpdateMail(MessageRouteStrategy strategy,
            ConsumerStrategy consumerStrategy, string name)
        {
            _concurrentDictionary.AddOrUpdate(name, s => new DefaultMessageMail {
                    Name = name,
                    MailStrategy = strategy
                },
                (s, mail) => {
                    mail.MailStrategy = strategy;
                    return mail;
                });
        }

        public bool IsMailName(string name)
        {
            return _concurrentDictionary.ContainsKey(name);
        }

        public void WrittenOffMail(string name)
        {
            _concurrentDictionary.TryRemove(name, out IMessageMail<IMessage> mail);
        }

    }
}