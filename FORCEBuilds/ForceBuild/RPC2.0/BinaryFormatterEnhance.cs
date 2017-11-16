using System.IO;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Binary;
using FORCEBuild.Persistence.Serialization;

namespace FORCEBuild.RPC2._0
{
    /* 由于binaryformatter对IEnumerable<>类型不能序列化，考虑两种方法：
     * 1.代为实现序列化过程
     * 2.转制array
     */

    /// <summary>
    /// 增强的序列化能力
    /// </summary>
    //public class BinaryFormatterEnhance:IFormatter
    //{
    //    private static BinaryFormatter _binaryFormatter = new BinaryFormatter();

    //    public object Deserialize(Stream serializationStream)
    //    {
    //        return 
    //    }

    //    public void Serialize(Stream serializationStream, object graph)
    //    {
            
    //    }

    //    public ISurrogateSelector SurrogateSelector { get; set; }
    //    public SerializationBinder Binder { get; set; }
    //    public StreamingContext Context { get; set; }
    //}
}