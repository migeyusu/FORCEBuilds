using AutoMapper;
using AutoMapper.Configuration;

namespace FORCEBuild.Configuration
{
    public interface IMapProfile
    {
        void Profile(IMapperConfigurationExpression expression);
    }
}