using System.Net.NetworkInformation;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 机器数据
    /// </summary>
    public class NetWorkInfo
    {
        //
        // 摘要:
        //     获取 IPv4 环回接口的索引。
        //
        // 返回结果:
        //     一个包含 IPv4 环回接口的索引的 System.Int32。
        //
        // 异常:
        //   T:System.Net.NetworkInformation.NetworkInformationException:
        //     此属性在仅运行 Ipv6 的计算机上无效。
        public static int LoopbackInterfaceIndex { get; }
        //
        // 摘要:
        //     获取 IPv6 环回接口的索引。
        //
        // 返回结果:
        //     返回 System.Int32。IPv6 环回接口的索引。
        public static int IPv6LoopbackInterfaceIndex { get; }
        //
        // 摘要:
        //     获取 System.Boolean 值，该值指示网络接口是否设置为仅接收数据包。
        //
        // 返回结果:
        //     如果接口仅接收网络通信，则为 true；否则为 false。
        //
        // 异常:
        //   T:System.PlatformNotSupportedException:
        //     此属性在运行早于 Windows XP 的操作系统的计算机上无效。
        public virtual bool IsReceiveOnly { get; }
        //
        // 摘要:
        //     获取网络接口的速度。
        //
        // 返回结果:
        //     System.Int64 值，指定速度（每位/秒为单位）。
        public virtual long Speed { get; }
        //
        // 摘要:
        //     获取网络连接的当前操作状态。
        //
        // 返回结果:
        //     System.Net.NetworkInformation.OperationalStatus 值之一。
        public virtual OperationalStatus OperationalStatus { get; }
        //
        // 摘要:
        //     获取接口的描述。
        //
        // 返回结果:
        //     System.String，用于描述此接口。
        public virtual string Description { get; }
        //
        // 摘要:
        //     获取网络适配器的名称。
        //
        // 返回结果:
        //     包含网络适配器名称的 System.String。
        public virtual string Name { get; }
        //
        // 摘要:
        //     获取网络适配器的标识符。
        //
        // 返回结果:
        //     包含标识符的 System.String。
        public virtual string Id { get; }
        //
        // 摘要:
        //     获取接口类型。
        //
        // 返回结果:
        //     System.Net.NetworkInformation.NetworkInterfaceType 值，指定网络接口类型。
        public virtual NetworkInterfaceType NetworkInterfaceType { get; }
        //
        // 摘要:
        //     获取 System.Boolean 值，该值指示是否启用网络接口以接收多路广播数据包。
        //
        // 返回结果:
        //     如果接口接收多路广播数据包，则为 true；否则为 false。
        //
        // 异常:
        //   T:System.PlatformNotSupportedException:
        //     此属性在运行早于 Windows XP 的操作系统的计算机上无效。
        public virtual bool SupportsMulticast { get; }
    }
}