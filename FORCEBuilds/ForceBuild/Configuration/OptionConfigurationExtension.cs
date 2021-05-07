using System;
using System.Configuration;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;

namespace FORCEBuild.Configuration
{
    /// <summary>
    /// suite for web.config
    /// </summary>
    public static class XmlConfigurationExtension
    {
        public static T GetSection<T>(this System.Configuration.Configuration configuration, string key)where T:ConfigurationSection
        {
            return configuration.GetSection(key) as T;
        }

        public static T GetOption<T>(this System.Configuration.Configuration configuration, string sectionName)
        {
            var type = typeof(T);
            if (type.GetConstructor(Type.EmptyTypes) == null)
            {
                throw new Exception($"No public and param-less constructor in class {type.Name}.");
            }

            var _propertyInfos = type.GetProperties(BindingFlags.Instance | BindingFlags.Public);
            if (!_propertyInfos.Any())
            {
                throw new Exception($"No public property in class {type.Name}.");
            }

            if (!_propertyInfos.All((info => info.CanWrite)))
            {
                throw new Exception($"Property of class {type.Name} must all be writable.");
            }

            var openCustomConfig = configuration.GetSection<DefaultSection>(sectionName);
            var elementXml = openCustomConfig.SectionInformation.GetRawXml();
            var xElement = XElement.Parse(elementXml);
            var keyValuePairs = xElement.Attributes()
                .ToDictionary((attribute => attribute.Name.LocalName), (attribute => attribute.Value));

            var instance = Activator.CreateInstance<T>();
            foreach (var propertyInfo in _propertyInfos)
            {
                if (keyValuePairs.TryGetValue(propertyInfo.Name, out var value))
                {
                    propertyInfo.SetValue(instance, value);
                }
            }

            return instance;
        }
    }
}