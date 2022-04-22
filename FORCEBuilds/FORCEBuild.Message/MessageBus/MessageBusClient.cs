using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Net.Abstraction;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.MessageBus.DataTransferObject;
using FORCEBuild.Net.RPC;

namespace FORCEBuild.Net.MessageBus
{
    /// <summary>
    /// a centralized local message-bus mails
    /// </summary>
    public class MessageBusClient : IDisposable
    {
        public bool Working { get; set; }
        public ILog Log { get; set; }

        private readonly ProxyServiceFactory _serviceFactory;

        private readonly IMessageClient _messageRequester;

        private ITcpMessageCentralizedService _centralizedService;

        private ITcpMessageCentralizedService CentralizedService
        {
            get
            {
                if (!Working)
                {
                    throw new Exception("����ʧ�ܣ����ط���δ����");
                }

                return _centralizedService ?? (_centralizedService =
                    _serviceFactory.CreateService<ITcpMessageCentralizedService>());
            }
        }

        /// <summary>
        /// mailname:MessageBusMail
        /// </summary>
        private readonly ConcurrentDictionary<string, LocalMessageMail> _messageBusMails;

        private bool _work;

        public MessageBusClient(IMessageClient messageRequester)
        {
            this._messageRequester = messageRequester ?? throw new NullReferenceException($"{messageRequester}����Ϊ��");
            _serviceFactory = new ProxyServiceFactory(messageRequester);
            _messageBusMails = new ConcurrentDictionary<string, LocalMessageMail>();
            Start();
        }

        public void Start()
        {
            if (Working)
            {
                return;
            }

            Working = true;
            _work = true;
            Task.Run(() =>
            {
                try
                {
                    while (_work)
                    {
                        if (_messageRequester.CanRequest)
                        {
                            foreach (var mail in _messageBusMails.Values)
                            {
                                var request = new MessageTransferRequest
                                {
                                    MailName = mail.MailName,
                                };
                                var response = _messageRequester.GetResponse(request) as MessageTransferResponse;
                                if (response?.Messages != null)
                                {
                                    foreach (var responseMessage in response.Messages)
                                        mail.OnReceived(responseMessage);
                                }
                            }
                        }

                        Thread.Sleep(100);
                    }
                }
                catch (Exception e)
                {
                    Log?.Write(e);
                }
                finally
                {
                    Working = false;
                    _messageBusMails.Clear();
                }
            });
        }

        /// <summary>
        /// ���¡��������mail
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="mailname"></param>
        /// <param name="consumerStrategy"></param>
        /// <returns></returns>
        public void RegisterOrUpdateMail(MessageRouteStrategy strategy, string mailname
            , ConsumerStrategy consumerStrategy)
        {
            if (strategy == null || mailname == null)
            {
                throw new Exception("���Ի�����������Ϊ��");
            }

            try
            {
                CentralizedService.RegisterOrUpdateMail(strategy, consumerStrategy, mailname);
            }
            catch (Exception)
            {
                CentralizedService.WrittenOffMail(mailname);
                throw;
            }
        }

        /// <summary>
        /// �������䣬�������߲��������׳��쳣
        /// </summary>
        /// <param name="mailname"></param>
        /// <returns></returns>
        public LocalMessageMail OpenMail(string mailname)
        {
            if (CentralizedService.IsMailName(mailname))
            {
                LocalMessageMail messageBusMail = null;
                _messageBusMails.AddOrUpdate(mailname, s =>
                    {
                        messageBusMail = new LocalMessageMail(this) { MailName = mailname };
                        return messageBusMail;
                    },
                    (s, mail) =>
                    {
                        messageBusMail = mail;
                        return mail;
                    });
                return messageBusMail;
            }

            throw new Exception("û����Ӧ���Ƶ�Զ������");
        }

        /// <summary>
        /// ֹͣ�ڱ��ض���
        /// </summary>
        /// <param name="mail"></param>
        public void CloseMail(LocalMessageMail mail)
        {
            _messageBusMails.TryRemove(mail.MailName, out LocalMessageMail value);
        }

        /// <summary>
        /// ֹͣ�ڱ��ض���
        /// </summary>
        /// <param name="mailName"></param>
        public void CloseMail(string mailName)
        {
            _messageBusMails.TryRemove(mailName, out LocalMessageMail value);
        }

        /// <summary>
        /// ��Զ��ɾ������
        /// </summary>
        /// <param name="mail"></param>
        public void WrittenOff(LocalMessageMail mail)
        {
            try
            {
                CentralizedService.WrittenOffMail(mail.MailName);
            }
            finally
            {
                _messageBusMails.TryRemove(mail.MailName, out LocalMessageMail value);
            }
        }

        /// <summary>
        /// ��������ָ���������Ϣ
        /// </summary>
        /// <param name="topic">��Ϣ����</param>
        /// <param name="message">��Ϣ��</param>
        public void Publish(string topic, IMessage message)
        {
            CentralizedService.SendMessage(message, topic);
        }

        public void Dispose()
        {
            _work = false;
        }

        /// <summary>
        /// ������Ϣ����,����IDisposable��رձ�����Ϣ����
        /// </summary>
        public class LocalMessageMail : IDisposable
        {
            private MessageBusClient _messageBusClient;

            public event Action<IMessage> Received;

            /// <summary>
            /// ��Ϣָ�롣-2:������ʼȡ;-1:ȡ��������������Ϣ;>=0:��ָ��ȡ
            /// </summary>
            public string MailName { get; set; }

            internal LocalMessageMail(MessageBusClient messageBusClient)
            {
                this._messageBusClient = messageBusClient;
            }

            public virtual void OnReceived(IMessage obj)
            {
                if (Received != null)
                {
                    ThreadPool.QueueUserWorkItem(state => Received.Invoke(obj));
                }
            }

            /// <summary>
            /// ֹͣ����
            /// </summary>
            public void Dispose()
            {
                _messageBusClient.CloseMail(this);
            }
        }
    }
}