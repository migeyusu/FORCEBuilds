using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Threading;
using System.Threading.Tasks;
using Castle.DynamicProxy;
using Castle.Windsor;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 不提供线程安全
    /// </summary>
    public class ServiceContainer : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        /// <summary>
        /// 生命周期为单次服务
        /// </summary>
        public Guid ServiceGuid
        {
            get { return _guid; }
            set
            {
                _guid = value;
                OnPropertyChanged();
            }
        }

        public IPEndPoint ServiceEndPoint
        {
            get { return _serviceEndPoint; }
            set
            {
                _serviceEndPoint = value;
                OnPropertyChanged();
            }
        }
        /// <summary>
        /// 是否在工作
        /// </summary>
        public bool Working
        {
            get { return _working; }
            set
            {
                _working = value;
                OnPropertyChanged();
            }
        }

        public List<Type> InterfaceList { get; set; }

        private readonly BinaryFormatter _serializer = new BinaryFormatter();
        
        /// <summary>
        /// interface|implement
        /// </summary>
        private readonly Dictionary<string, object> _servicesPool;

        private IPEndPoint _serviceEndPoint;
        private bool _working;
        private Guid _guid;
        private bool work;

        /// <summary>
        /// 服务类实例由容器提供
        /// </summary>
        private WindsorContainer ObjectContainer { get; set; }
        
        /// <summary>
        /// 注册需要的接口实现
        /// </summary>
        /// <typeparam name="T">接口</typeparam>
        /// <typeparam name="TK">实现类</typeparam>
        public void Register<T, TK>() where T : class where TK : T
        {
            ObjectContainer.Register(Castle.MicroKernel.Registration.
                Component.For<T>().ImplementedBy<TK>());
            InterfaceList.Add(typeof(T));
        }

        //public void RegisterWithInterceptor<T, TK>(IInterceptor interceptor) where T : class where TK : T
        //{
        //    ObjectContainer.Register(Castle.MicroKernel.Registration.
        //        Component.For<T>().ImplementedBy<TK>());
        //}

        public ServiceContainer()
        {
            _servicesPool = new Dictionary<string, object>();
            ServiceEndPoint = new IPEndPoint(NetHelper.InstanceIpv4,
                NetHelper.AvailablePort);

            ObjectContainer = new WindsorContainer();
            InterfaceList = new List<Type>();
        }

        //public ServiceContainer(IPEndPoint remoteEndPoint) : this()
        //{
        //    ServiceEndPoint = remoteEndPoint;
        //}

        public void Start(IPEndPoint ipEndPoint)
        {
            ServiceEndPoint = ipEndPoint;
            Start();
        }

        public void Start()
        {
            if (Working)
                return;
            work = true;
            Task.Run(() =>
            {
                ServiceGuid = Guid.NewGuid();
                TcpListener listener = null;
                try {
                    listener = new TcpListener(ServiceEndPoint);
                    listener.Start();
                    Working = true;
                    while (work) {
                        if (listener.Pending()) {
                            Task.Run(() => Produce(listener.AcceptSocket()));
                        }
                        else
                            Thread.Sleep(30);
                    }
                }
                catch (Exception e) {
                    throw new Exception("RPC服务将关闭", e);
                }
                finally {
                    listener?.Stop();
                    Working = false;
                }
            });
        }

        public void End()
        {
            if (Working)
            {
                work = false;
            }
        }

        private void Produce(Socket socket)
        {
            var requestHead = socket.ReadStruct<RequestHead>();//GetRequestHead(socket);
            if (!requestHead.IsCorrect)
            {
                SendResponse(false, new Exception("接收的数据发生损坏！"), socket);
                return;
            }
            try {
                switch (requestHead.Calltype)
                {
                    case CallType.Call:
                        var callrequest = _serializer.Deserialize(
                            socket.GetSpecificLenStream(requestHead.LeaveLength)) as InterfaceCallRequest;
                        object calledObj;
                        if (!_servicesPool.ContainsKey(callrequest.InterfaceType))
                        {
                            calledObj =
                                ObjectContainer.Resolve(Type.GetType(callrequest.InterfaceType));
                            _servicesPool.Add(callrequest.InterfaceType, calledObj);
                        }
                        else
                        {
                            calledObj = _servicesPool[callrequest.InterfaceType];
                        }
                        try
                        {

                            var result = callrequest.Method.Invoke(calledObj, callrequest.Parameters);
                            SendResponse(true, result, socket);
                        }
                        catch (Exception e)
                        {
                            SendResponse(false, e.InnerException, socket);
                        }
                        break;
                    case CallType.Info:
                        SendResponse(true, null, socket);
                        break;
                    case CallType.Test:
                        SendResponse(true, null, socket);
                        break;
                }
            }
            catch (Exception e)
            {
                SendResponse(false, e, socket);
            }
            finally
            {
                socket.Close();
            }
        }

        private void SendResponse(bool isExecuted, object dto, Socket socket)
        {
            if (dto != null) {
                var memoryStream = new MemoryStream();
                _serializer.Serialize(memoryStream, dto);
                var dataBytes = memoryStream.ToArray();
                socket.SendResponse(isExecuted, dataBytes);
            }
            else
                socket.SendResponse(isExecuted, new byte[0] { });
        }
        

        private void OnPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }

    public class WindsorAOP
    {
        public virtual int I { get; set; }
    }

    class WindosorInterceptor:IInterceptor
    {
        public void Intercept(IInvocation invocation)
        {
            invocation.Proceed();
            Debug.Print("dd");

        }
    }
}
