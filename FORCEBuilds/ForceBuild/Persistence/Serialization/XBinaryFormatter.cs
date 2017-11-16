using System;
using System.IO;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace FORCEBuild.Persistence.Serialization
{
    //暂时不使用，计划改造成支持构造函数+内部实现

    /// <summary>
    /// 仅添加属性注入功能   
    /// </summary>
    public class XBinaryFormatter : IXFormatter
    {
        public ISurrogateSelector SurrogateSelector { get; set; }

        public SerializationBinder Binder { get; set; }

        public StreamingContext Context { get; set; }

        public Func<Type, object> InstanceCreateFunc { get; set; }

        private readonly BinaryFormatter _formatter = new BinaryFormatter();

        public XBinaryFormatter()
        {
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            _formatter.Serialize(serializationStream, graph);
        }



        public object Deserialize(Stream serializationStream)
        {
            var obj = _formatter.Deserialize(serializationStream);
            var type = obj.GetType();
            var oc = InstanceCreateFunc(type);
            foreach (var property in type.GetProperties())
            {
                if (property.CanRead && property.CanWrite)
                {
                    property.SetValue(oc, property.GetValue(obj));
                }
            }
            return oc;
        }



        public object Deserialize(Stream serialization, Type type)
        {
            var re = InstanceCreateFunc(type);
            Deserialize(serialization, re, type);
            return re;
        }

        public void Deserialize(Stream serialization, object re, Type type)
        {
            var oc = _formatter.Deserialize(serialization);
            var pis = type.GetProperties(BindingFlags.Public);
            foreach (var pi in pis)
            {
                if (pi.CanRead && pi.CanWrite)
                {
                    pi.SetValue(re, pi.GetValue(oc));
                }
            }

        }
    }
}