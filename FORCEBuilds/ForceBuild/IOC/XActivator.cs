using System;
using System.Reflection;
using System.Collections.ObjectModel;

namespace FORCEBuild.IOC
{
    /// <summary>
    /// 仅用于简单参数注入   
    /// </summary>
    public class XActivator
    {
        public static object GetInstance(Type type)
        {
            var at = type.GetCustomAttribute<ConstructorAttribute>();
            if ( at== null)
            {
                return Activator.CreateInstance(type);
            }
            else
            {
                var ctype = at.ConstructType;
                switch (ctype)
                {
                    case ConstructType.StaticMethod:
                        var method= type.GetMethod(at.MethodName);
                        return method.Invoke(null, null);
                    case ConstructType.StaticProperty:
                        var property = type.GetProperty(at.PropertyName);
                        return property.GetValue(null);
                    case ConstructType.Parameters:
                        var parameters = new object[at.Impletes.Length];
                        for (int i=0;i<at.Impletes.Length;++i)
                        {
                            parameters[i] = GetInstance(at.Impletes[i]);
                        }
                        return Activator.CreateInstance(type, parameters);
                    default:
                        throw new Exception("在预定义的范围之外");
                }
            }
            //Activator.CreateInstance()

        }
    }
}
