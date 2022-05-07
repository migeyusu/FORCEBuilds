using System.IO;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.NamedPipe
{
    public class NamedPipeMessageFormatterAccessor : NamedPipeStreamAccessor
    {
        private readonly IFormatter _formatter;

        public NamedPipeMessageFormatterAccessor(IFormatter formatter, Stream stream, bool isDisposeInternal = true) :
            base(stream, isDisposeInternal)
        {
            _formatter = formatter;
        }

        public async Task<IMessage> ReadMessageAsync(CancellationToken token)
        {
            var readAsync = await ReadAsync(token);
            using (var memoryStream = new MemoryStream(readAsync))
            {
                return _formatter.Deserialize(memoryStream) as IMessage;
            }
        }

        public IMessage ReadMessage()
        {
            var readAsync = Read();
            using (var memoryStream = new MemoryStream(readAsync))
            {
                return _formatter.Deserialize(memoryStream) as IMessage;
            }
        }

        public async Task WriteMessageAsync(IMessage message, CancellationToken token)
        {
            using (var memoryStream = new MemoryStream())
            {
                _formatter.Serialize(memoryStream, message);
                await WriteAsync(memoryStream.ToArray(), token);
            }
        }

        public void WriteMessage(IMessage message)
        {
            using (var memoryStream = new MemoryStream())
            {
                _formatter.Serialize(memoryStream, message);
                Write(memoryStream.ToArray());
            }
        }
    }
}