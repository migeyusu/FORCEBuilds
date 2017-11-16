using System.Collections.Generic;

namespace FORCEBuild.Windows.Hardware
{
    public interface INetworkInfoGet
    {
        IEnumerable<NetworkInfo> GetNetworkInfo();

        
    }
}