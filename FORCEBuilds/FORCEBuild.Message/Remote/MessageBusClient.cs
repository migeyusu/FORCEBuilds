using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Crosscutting.Log;
using FORCEBuild.Net.Base;
using FORCEBuild.Net.Remote.DataTransferObject;
using FORCEBuild.Net.RPC;

namespace FORCEBuild.Net.Remote
{
    /// <summary>
    /// a centralized local  message-bus mails
    /// </summary>
    public class MessageBusClient:IDisposable
    {
        public bool Working { get; set; }

        public ILog Log { get; set; }

        private readonly ServiceFactory _serviceFactory;

        private readonly IMessageRequester _messageRequester;

        private ITcpMessageCentralizedService _centralizedService;

        private ITcpMessageCentralizedService CentralizedService {
            get {
                if (!Working) {
                    throw new Exception("操作失败，本地服务未开启");
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

        public MessageBusClient(IMessageRequester messageRequester)
        {
            this._messageRequester = messageRequester ?? throw new NullReferenceException($"{messageRequester}不能为空");
            _serviceFactory = new ServiceFactory(messageRequester);
            _messageBusMails = new ConcurrentDictionary<string, LocalMessageMail>();
            Start();
        }

        public void Start()
        {
            if (Working) {
                return;
            }
            Working = true;
            _work = true;
            Task.Run(() => {
                try {
                    while (_work) {
                        if (_messageRequester.CanRequest) {
                            foreach (var mail in _messageBusMails.Values) {
                                var request = new MessageTransferRequest {
                                    MailName = mail.MailName,
                                };
                                var response = _messageRequester.GetResponse(request) as MessageTransferResponse;
                                if (response?.Messages != null) {
                                    foreach (var responseMessage in response.Messages)
                                        mail.OnReceived(responseMessage);
                                }
                            }
                        }
                        Thread.Sleep(100);
                    }
                }
                catch (Exception e) {
                    Log?.Write(e);
                }
                finally {
                    Working = false;
                    _messageBusMails.Clear();
                }
            });
        }

        /// <summary>
        /// 更新、创建或打开mail
        /// </summary>
        /// <param name="strategy"></param>
        /// <param name="mailname"></param>
        /// <param name="consumerStrategy"></param>
        /// <returns></returns>
        public void RegisterOrUpdateMail(MessageRouteStrategy strategy, string mailname
            , ConsumerStrategy consumerStrategy)
        {
            if (strategy == null || mailname == null) {
                throw new Exception("策略或邮箱名不能为空");
            }
            try {
                CentralizedService.RegisterOrUpdateMail(strategy, consumerStrategy, mailname);
            }
            catch (Exception) {
                CentralizedService.WrittenOffMail(mailname);
                throw;
            }
        }

        /// <summary>
        /// 订阅邮箱，服务总线不存在则抛出异常
        /// </summary>
        /// <param name="mailname"></param>
        /// <returns></returns>
        public LocalMessageMail OpenMail(string mailname)
        {
            if (CentralizedService.IsMailName(mailname)) {
                LocalMessageMail messageBusMail = null;
                _messageBusMails.AddOrUpdate(mailname, s => {
                        messageBusMail = new LocalMessageMail(this) {MailName = mailname};
                        return messageBusMail;
                    },
                    (s, mail) => {
                        messageBusMail = mail;
                        return mail;
                    });
                return messageBusMail;
            }
            throw new Exception("没有相应名称的远程邮箱");   
        }

        /// <summary>
        /// 停止在本地订阅
        /// </summary>
        /// <param name="mail"></param>
        public void CloseMail(LocalMessageMail mail)
        {
            _messageBusMails.TryRemove(mail.MailName, out LocalMessageMail value);
        }
        
        /// <summary>
        /// 停止在本地订阅
        /// </summary>
        /// <param name="mailName"></param>
        public void CloseMail(string mailName)
        {
            _messageBusMails.TryRemove(mailName, out LocalMessageMail value);
        }
        
        /// <summary>
        /// 在远程删除邮箱
        /// </summary>
        /// <param name="mail"></param>
        public void WrittenOff(LocalMessageMail mail)
        {
            try {
                CentralizedService.WrittenOffMail(mail.MailName);
            }
            finally {
                _messageBusMails.TryRemove(mail.MailName, out LocalMessageMail value);
            }
        }

        /// <summary>
        /// 发布带有指定话题的消息
        /// </summary>
        /// <param name="topic">消息话题</param>
        /// <param name="message">消息体</param>
        public void Publish(string topic,IMessage message)
        {
            CentralizedService.SendMessage(message,topic);
        }

        public void Dispose()
        {
            _work = false;
        }

        /// <summary>
        /// 本地消息邮箱,调用IDisposable后关闭本地消息接收
        /// </summary>
        public class LocalMessageMail:IDisposable
        {
            private MessageBusClient _messageBusClient;

            public event Action<IMessage> Received;
            
            /// <summary>
            /// 消息指针。-2:从请求开始取;-1:取得邮箱内所有消息;>=0:按指针取
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
            /// 停止订阅
            /// </summary>
            public void Dispose()
            {
                _messageBusClient.CloseMail(this);
            }
        }
    }
}