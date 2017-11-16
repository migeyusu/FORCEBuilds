using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;
using FORCEBuild.Persistence.Serialization;
using Xunit;

namespace FORCEBuild.RPC3._0
{
    /* 失败，因为无法实现大数量的竞争
     */
    /// <summary>
    /// 兼具收发，协议透明化注册机制
    /// </summary>
    //public class ServiceRegistry
    //{
    //    private bool inspect, inspecting, struggle, struggling, publish, publishing;

    //    private Dictionary<string,Dictionary<Guid,ServiceNode>> serviceNodes;

    //    public IPEndPoint RegistryEndPoint { get; set; }
    //    /// <summary>
    //    /// 生命周期标识
    //    /// </summary>
    //    public Guid RegistryGuid { get; set; }

    //    public int StrugglePort { get; set; }

    //    public int PublishPort { get; set; }
    //    /// <summary>
    //    /// 竞争时间以最早为最优先，初始化时标识当前时间
    //    /// </summary>
    //    private long struggleTime;

    //    public ServiceRegistry(int struggleport = 9280, int publishport = 9290)
    //    {
    //        serviceNodes = new Dictionary<string, Dictionary<Guid, ServiceNode>>();
    //        struggleTime = DateTime.Now.ToBinary();
    //        StrugglePort = struggleport;
    //        PublishPort = publishport;
    //    }

    //    public void Start()
    //    {
    //        if (inspecting) {
    //            return;
    //        }
    //        inspect = true;
    //        RegistryGuid = Guid.NewGuid();
    //        Task.Run(new Action(RegistryInspect));
    //    }
    //    /// <summary>
    //    /// 发布线程
    //    /// </summary>
    //    private void RegistryPublish()
    //    {
    //       publishing = true;
    //        var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
    //        var target = new IPEndPoint(IPAddress.Broadcast, PublishPort);
    //        RegistryEndPoint = new IPEndPoint(NetHelper.InstanceIpv4, NetHelper.AviliblePort);

    //        var registryBytes = new RegistryInfo(RegistryEndPoint, RegistryGuid, struggleTime).ToBytes();
    //        while (publish)
    //        {
    //            client.Send(registryBytes, registryBytes.Length, target);
    //            Thread.Sleep(200);
    //        }
    //        client.Close();
    //        publishing = false;

    //    }

    //    /// <summary>
    //    /// 探查线程,起始线程
    //    /// </summary>
    //    private void RegistryInspect()
    //    {
    //        var client = new UdpClient(StrugglePort);
    //        inspecting = true;
    //        IPEndPoint endPoint = null;
    //        while (inspect)
    //        {
    //            if (client.Available==0) {
    //                Thread.Sleep(600);
    //                if (client.Available==0) {
    //                    //开始争夺
    //                    Task.Run(new Action(RegistryStruggle));
    //                }
    //            }
    //            else {
    //                var datas = client.Receive(ref endPoint);
    //                var head = datas.ToStruct<StruggleSign>();
    //                if (head.IsCorrect) {
    //                    if (head.Time<struggleTime)
    //                    {
    //                        //早于本机时间
    //                        if (struggling) {
    //                            struggle = false;
    //                        }
    //                    }
    //                }
    //            }
    //            Thread.Sleep(200);
    //        }
    //        if (publishing) {
    //            publish = false;
    //        }
    //        client.Close();
    //        inspecting = false;
    //    }

    //    [Fact]
    //    public void FactMethodName()
    //    {
    //        var time = DateTime.Now.ToBinary();
    //        Thread.Sleep(200);
    //        var time2 = DateTime.Now.ToBinary();
    //        Assert.True(time2-time>0);
    //    }

    //    /// <summary>
    //    /// 争夺线程
    //    /// </summary>
    //    private void RegistryStruggle()
    //    {
    //        struggling = true;
    //        var client = new UdpClient(new IPEndPoint(IPAddress.Any, 0));
    //        var target = new IPEndPoint(IPAddress.Broadcast, StrugglePort);
    //        var bytes=new StruggleSign(){Time = struggleTime}.ToBytes();
    //        while (struggle) {
    //            client.Send(bytes, bytes.Length, target);
    //            Thread.Sleep(200);
    //        }
    //        client.Close();
    //        struggling = false;

    //    }


    //    /// <summary>
    //    /// 监听线程
    //    /// </summary>
    //    private void RegistryListen()
    //    {
            
    //    }

    //    public void End()
    //    {
    //        if (inspecting) {
    //            inspect = false;
    //        }
    //    }

    //    public void Register()
    //    {
            
    //    }
    //}


    //服务调用注册
    public class ServiceRegistry
    {
        public ServiceRegistry()
        {
               
        }

        public void Start()
        {
            var client = new UdpClient();
            
        }


        public void Register()
        {
            
        }
    }
}