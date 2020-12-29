using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.Configuration;

namespace FORCEBuild.Configuration
{
    public static class AutoMapperInstaller
    {
        public static IMapper CreateMapper(params Profile[] profiles)
        {
            var config = new MapperConfiguration(cfg =>
            {
                foreach (var profile in profiles)
                {
                    cfg.AddProfile(profile);
                }
            });
            return config.CreateMapper();
        }
    }
}