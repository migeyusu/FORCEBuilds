using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace FORCEBuild.Serialization
{
    public static class SerializationExtension
    {
        private static readonly BinaryFormatter BinaryFormatter = new BinaryFormatter();
        
        public static byte[] BinarySerialize(this object graph)
        {
            using (var memoryStream = new MemoryStream())
            {
                BinaryFormatter.Serialize(memoryStream, graph);
                return memoryStream.ToArray();
            }
        }

        public static void BinarySerializeTo(this object graph,Stream stream )
        {
            BinaryFormatter.Serialize(stream, graph);
        }
        
        public static T BinaryDeserialize<T>(this Stream stream)
        {
            return (T) BinaryFormatter.Deserialize(stream);
        }

        public static T BinaryDeserialize<T>(this byte[] bytes)
        {
            using (var memoryStream = new MemoryStream(bytes))
            {
                return (T) BinaryFormatter.Deserialize(memoryStream);
            }
        }
    }
}