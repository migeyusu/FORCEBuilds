using System;
using System.Net;

namespace FORCEBuild.Net.Service
{
    /// <summary>
    /// 用于servicelistener的服务集合，serviceinfo的类形式
    /// </summary>
    public class ServiceDefine
    {
        /// <summary>
        /// 服务生命周期guid
        /// </summary>
        public Guid Guid { get; set; }
        /// <summary>
        /// 服务提供者终结点
        /// </summary>
        public IPEndPoint ProviderIpEndPoint { get; set; }
        /// <summary>
        /// 上一次服务广播发出时间
        /// </summary>
        public DateTime LastTime { get; set; }

        protected bool Equals(ServiceDefine other)
        {
            return Guid.Equals(other.Guid) && Equals(ProviderIpEndPoint, other.ProviderIpEndPoint);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServiceDefine) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (Guid.GetHashCode() * 397) ^ (ProviderIpEndPoint != null ? ProviderIpEndPoint.GetHashCode() : 0);
            }
        }
    }
}