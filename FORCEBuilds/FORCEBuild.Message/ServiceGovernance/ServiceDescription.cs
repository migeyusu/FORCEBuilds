using System;
using System.Net;

namespace FORCEBuild.Net.ServiceGovernance
{
    /// <summary>
    /// 描述服务
    /// </summary>
    public class ServiceDescription
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

        protected bool Equals(ServiceDescription other)
        {
            return Guid.Equals(other.Guid) && Equals(ProviderIpEndPoint, other.ProviderIpEndPoint);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            if (ReferenceEquals(this, obj)) return true;
            if (obj.GetType() != this.GetType()) return false;
            return Equals((ServiceDescription) obj);
        }

        public override int GetHashCode()
        {
            unchecked {
                return (Guid.GetHashCode() * 397) ^ (ProviderIpEndPoint != null ? ProviderIpEndPoint.GetHashCode() : 0);
            }
        }
    }
}