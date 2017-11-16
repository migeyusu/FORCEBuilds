using System;
using System.Collections;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using Microsoft.SqlServer.Server;

namespace FORCEBuild.Persistence.Serialization
{

    /// <summary>
    /// 添加了有简单的标记功能
    /// </summary>
    public class XBinarySerializer:IXFormatter
    {
        public ISurrogateSelector SurrogateSelector { get; set; }

        public SerializationBinder Binder { get; set; }

        public StreamingContext Context { get; set; }

        public Func<Type, object> InstanceCreateFunc { get; set; }

        Dictionary<Type, XBinaryRootAttribute> cache;

        private BinaryFormatter Serializer;

        private XBinarySerializer()
        {
            cache = new Dictionary<Type, XBinaryRootAttribute>();
            InstanceCreateFunc = Activator.CreateInstance;
            Serializer=new BinaryFormatter();
        }

        public MemoryStream Serialize(object oc)
        {
            var ms = new MemoryStream();
            Serialize(ms, oc);
            return ms;
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            var type = graph.GetType();

            if (type.GetCustomAttribute<IBinaryClassAttribute>() == null)
            {
                Serializer.Serialize(ms, oc);
                serializationStream.Seek(0, SeekOrigin.Begin);
                return;
            }
            if (!cache.Keys.Contains(type))
            {
                Register(type);
            }
            SerializePairs sps = cache[type].Clone();
            FillPairs(sps, oc);
            bf.Serialize(ms, sps);
            ms.Seek(0, SeekOrigin.Begin);
        }

        public void Deserialize<T>(Stream s, T x)
        {
            var bf = new BinaryFormatter();
            var oc = bf.Deserialize(s);
            var pis = typeof(T).GetProperties();
            foreach (var item in pis)
            {
                item.SetValue(x, item.GetValue(oc));
            }

        }

        public object Deserialize(Stream serializationStream)
        {
            
        }
        /// <summary>
        /// 提取property
        /// </summary>
        private void Register(Type type)
        {
            var at = type.GetCustomAttribute<XBinaryRootAttribute>();
            if ( at== null)
                return;
            cache.Add(type,at);
            var childs = new List<XBinarySerializePair>();
            var pis = type.GetProperties();
            foreach (var pi in pis)
            {
                var cat = pi.GetCustomAttribute<XBinaryElementAttribute>();
                if (cat == null)
                    continue;
                var xe=new XBinarySerializePair()
                {
                    Property = pi,
                    Type = pi.PropertyType
                };
                xe.IsIEnumerable = typeof(IEnumerable).IsAssignableFrom(xe.Type);
                if (xe.IsIEnumerable)
                {
                    
                }
                else
                {
                    xe.IsCustom = xe.Type.GetCustomAttribute<XBinaryElementAttribute>() == null;
                }
            }
            SerializePairs sps = new SerializePairs();
            //预先插入防止循环引用
            cache.Add(type, sps);
            List<SerializePair> needserializes = new List<SerializePair>();
            foreach (var item in type.GetProperties())
            {
                IBinaryPropertyAttribute ibpa = item.GetCustomAttribute<IBinaryPropertyAttribute>();
                if (ibpa != null)
                {
                    SerializePair sp = new SerializePair() { IsCustom = ibpa.IsCustom };

                }

            }
        }

        private void FillPairs(SerializePairs sp, object oc)
        {

        }
    }



}
