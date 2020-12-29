using System.Net.NetworkInformation;
using AutoMapper;
using FORCEBuild.Configuration;

namespace FORCEBuild.Windows.Hardware
{
    public class InformationMapper:IMapProfile
    {
        public void Profile(IMapperConfigurationExpression expression)
        {
            expression.CreateMap<NetworkInterface, NetworkInfo>();
        }
    }
}