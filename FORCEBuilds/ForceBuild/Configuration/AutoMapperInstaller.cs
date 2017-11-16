using System;
using System.Collections.Generic;
using AutoMapper;
using AutoMapper.Configuration;

namespace FORCEBuild.Configuration
{
    public class AutoMapperInstaller
    {
      //  private List<IMapProfile> _profiles;

        private bool _isInitialzed;
        
        public AutoMapperInstaller()
        {
          //  _profiles = new List<IMapProfile>();
        }


        public void InitializeProfile(params IMapProfile[] profile)
        {
            if (_isInitialzed)
                throw new NotSupportedException("该方法只允许调用一次");
            var expression = new MapperConfigurationExpression();
            foreach (var mapProfile in profile)
            {
                mapProfile.Profile(expression);
            }
            Mapper.Initialize(expression);
            _isInitialzed = true;
          //  Mapper.AssertConfigurationIsValid();
        }

        //public void Instal()
        //{
        //    var expression = new MapperConfigurationExpression();
        //    foreach (var profile in profiles) {
        //        profile.Profile(expression);
        //    }
        //    Mapper.Initialize(expression);
        //}

    }
    //not support
    //public static class Extention
    //{
    //    public static void Add(this Mapper mapper, IMapProfile mapProfile)
    //    {
            
    //    }
    //}

}