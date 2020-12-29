using System;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using AutoMapper;

namespace FORCEBuild.Windows.Hardware
{
    [Serializable]
    public class NetworkInfo : INetworkInfoGet
    {
        //
        // 摘要:
        //     获取网络接口的速度。
        //
        // 返回结果:
        //     System.Int64 值，指定速度（每位/秒为单位）。
        public long Speed { get; set; }

        //
        // 摘要:
        //     获取网络连接的当前操作状态。
        //
        // 返回结果:
        //     System.Net.NetworkInformation.OperationalStatus 值之一。
        public OperationalStatus OperationalStatus { get; set; }

        //
        // 摘要:
        //     获取接口的描述。
        //
        // 返回结果:
        //     System.String，用于描述此接口。
        public virtual string Description { get; set; }

        //
        // 摘要:
        //     获取网络适配器的名称。
        //
        // 返回结果:
        //     包含网络适配器名称的 System.String。
        public virtual string Name { get; set; }

        //
        // 摘要:
        //     获取网络适配器的标识符。
        //
        // 返回结果:
        //     包含标识符的 System.String。
        public virtual string Id { get; set; }

        //
        // 摘要:
        //     获取接口类型。
        //
        // 返回结果:
        //     System.Net.NetworkInformation.NetworkInterfaceType 值，指定网络接口类型。
        public virtual NetworkInterfaceType NetworkInterfaceType { get; set; }

        public long BytesReceived { get; set; }

        public long Bytes { get; set; }

        IEnumerable<NetworkInfo> INetworkInfoGet.GetNetworkInfo()
        {
            return GetNetworkInfo();
        }

        private static bool _initialized;
        private static IMapper mapper;

        public static IEnumerable<NetworkInfo> GetNetworkInfo()
        {
            if (!_initialized)
            {
                var config = new MapperConfiguration(cfg => { cfg.AddProfile<InformationMapper>(); });
                mapper = config.CreateMapper();
                _initialized = true;
            }

            var interfaces = NetworkInterface.GetAllNetworkInterfaces();
//            return interfaces.Select(Mapper.Map<NetworkInterface, NetworkInfo>);
            var infos = new List<NetworkInfo>();
            foreach (var @interface in interfaces)
            {
                var info = mapper.Map<NetworkInterface, NetworkInfo>(@interface);
                info.BytesReceived = @interface.GetIPv4Statistics().BytesReceived;
                info.Bytes = @interface.GetIPv4Statistics().BytesSent;
                infos.Add(info);
            }

            return infos;
        }
    }
}