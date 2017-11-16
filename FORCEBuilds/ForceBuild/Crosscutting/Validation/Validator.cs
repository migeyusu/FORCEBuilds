using System;
using System.Collections.Generic;
using System.Reflection;
namespace FORCEBuild.AOP.Validator
{
    /// <summary>
    /// 对标记了检查标记的属性的进行验证
    /// </summary>
    public class Validator
    {
        private static Validator _instance;

        readonly Dictionary<Type, Dictionary<string,ValidatorItem>> ValidatorCache;

        public Validator()
        {
            ValidatorCache = new Dictionary<Type, Dictionary<string, ValidatorItem>>();
        }

        public static Validator Current
        {
            get
            {
                if (_instance == null)
                    _instance = new Validator();
                return _instance;
            }
        }

        public void Validate(object model,string name)
        {
            var type = model.GetType();
             Dictionary<string, ValidatorItem> validaties;
            if(!ValidatorCache.ContainsKey(type))
            {
                validaties = GetValidators(type);
                ValidatorCache.Add(type, validaties);
            }
            else
            {
                validaties = ValidatorCache[type]; 
            }
            if(validaties.ContainsKey(name))
            {
                var  vi= validaties[name];
                var oc = vi.Property.GetValue(model); 
                vi.Attribute.Validate(oc, vi.Property.Name);
            }
        }

        public void Validate(object model,Type type)
        {
            Dictionary<string, ValidatorItem> validaties;
            if (!ValidatorCache.ContainsKey(type))
            {
                validaties = GetValidators(type);
                ValidatorCache.Add(type, validaties);
            }
            else
            {
                validaties = ValidatorCache[type];
            }
           
            foreach(var vi in validaties)
            {
                var oc = vi.Value.Property.GetValue(model);
                vi.Value.Attribute.Validate(oc, vi.Value.Property.Name);
            }
        }

        public void Validate(object model)
        {
            var type = model.GetType();
            Dictionary<string, ValidatorItem> validaties;
            if (!ValidatorCache.ContainsKey(type))
            {
                validaties = GetValidators(type);
                ValidatorCache.Add(type, validaties);
            }
            else
            {
                validaties = ValidatorCache[type];
            }
            foreach (var vi in validaties)
            {
                var oc = vi.Value.Property.GetValue(model);
                vi.Value.Attribute.Validate(oc, vi.Value.Property.Name);
            }
        }

        public void Validate<T>(T model)
        {
            var type = typeof(T);
            Dictionary<string, ValidatorItem> validaties;
            if (!ValidatorCache.ContainsKey(type))
            {
                validaties = GetValidators(type);
                ValidatorCache.Add(type, validaties);
            }
            else
            {
                validaties = ValidatorCache[type];
            }
            foreach (var vi in validaties)
            {
                var oc = vi.Value.Property.GetValue(model);
                vi.Value.Attribute.Validate(oc, vi.Value.Property.Name);
            }
        }

        private Dictionary<string,ValidatorItem> GetValidators(Type type)
        {
            var dic = new Dictionary<string, ValidatorItem>();
            var properties = type.GetProperties();
            foreach (var property in properties)
            {
                var attributies = property.GetCustomAttributes();
                foreach (var at in attributies)
                {
                    if (typeof(IValidate).IsAssignableFrom(at.GetType()))
                        dic.Add(property.Name, new ValidatorItem() { Attribute = (IValidate)at, Property = property });
                }
            }
            return dic;
        }
    }
}
