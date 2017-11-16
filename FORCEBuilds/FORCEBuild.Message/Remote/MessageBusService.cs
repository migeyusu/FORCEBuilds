using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Castle.MicroKernel.Registration;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Message.Base;
using FORCEBuild.Message.Remote.Buffer;
using FORCEBuild.Message.RPC;
using FORCEBuild.Net;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.Service;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.Message.Remote
{
    /* 消息总线服务端分为路由（Routed，类似RabbitMQ的Exchange）和消息策略两大部分
     * 路由机制是类似频道或正则字符串实现的发送频道规则
     * 消息策略是确定消息缓冲区类型和行为的规则
     * 
     * 服务端路由消息到具有具体处理机制的"邮箱"，邮箱根据消息处理规则决定是否持久化这些消息
     * 客户端向邮箱发送获取请求，邮箱根据消息处理规则决定是否弹出消息或使用指针跟踪不同的消息获取
     * 2017.10.21 整个过程以轻量化实现，不考虑持久化，不具有使用指针读取消息的能力
     * 对于邮箱的定义，从Provider-Consumer的角度看，相当于远程Consumer的代理或推送引擎
     */
     
    /// <summary>
    /// 消息总线远程服务
    /// </summary>
    public class MessageBusService : ITcpMessageCentralizedService,ITcpServiceProvider
    {
        public ILog Log {
            get { return _log; }
            set {
                _log = value;
                _responseListener.Log = value;
            }
        }

        public Guid ServiceGuid { get; set; }

        public bool Working => _responseListener.Working;

        private readonly ConcurrentDictionary<string, IMessageMail<IMessage>> _concurrentDictionary 
            = new ConcurrentDictionary<string, IMessageMail<IMessage>>();

        /// <summary>
        /// 服务查询终结点
        /// </summary>
        public IPEndPoint ServiceEndPoint {
            get => _responseListener.EndPoint;
            set => _responseListener.EndPoint = value;
        }

        private readonly TcpMessageReplier _responseListener = new TcpMessageReplier();
        private ILog _log;

        public MessageBusService()
        {
            var actuator = new Actuator();
            actuator.Container.Register(Component.For<ITcpMessageCentralizedService>()
                .Instance(this));
            var usePipe = new CallProducePipe {
                Actuator = actuator
            }.Append(new ConsumerProducePipe {
                MessageMails = this._concurrentDictionary
            });
            _responseListener.ProducePipe = usePipe;
        }

        public void Start()
        {
            var instanceEndPoint = ServiceEndPoint ?? NetHelper.InstanceEndPoint;
            Start(instanceEndPoint);
        }

        public void Start(IPEndPoint serviceEndPoint)
        {
            if (Working)
                return;
            ServiceEndPoint = serviceEndPoint;
            ServiceGuid = Guid.NewGuid();
            _responseListener.Start();
        }

        public void End()
        {
            _responseListener.End();
        }

        public void SendMessage<T>(T x, string name) where T : IMessage
        {
            foreach (var mail in _concurrentDictionary.Values) {
                if (mail.IsMatch(name)) {
                    mail.Post(x);
                }
            }
        }

        public void RegisterOrUpdateMail(MessageRouteStrategy strategy,
            ConsumerStrategy consumerStrategy, string name)
        {
            _concurrentDictionary.AddOrUpdate(name, s => new DefaultMessageMail() {
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

        public void Dispose()
        {
            this.End();
            _responseListener.Dispose();
        }

    }
}