using System.Net.NetworkInformation;
using AutoMapper;
using FORCEBuild.Configuration;

namespace FORCEBuild.Windows.Hardware
{
    public class InformationMapper: Profile
    {
        public InformationMapper()
        {
            CreateMap<NetworkInterface, NetworkInfo>();
        }
    }
}