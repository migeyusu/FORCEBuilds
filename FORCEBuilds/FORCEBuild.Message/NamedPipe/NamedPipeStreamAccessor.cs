using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;
using FORCEBuild.Serialization;

namespace FORCEBuild.Net.NamedPipe
{
    public abstract class NamedPipeStreamAccessor
    {
        private readonly Stream _stream;

        protected NamedPipeStreamAccessor(Stream stream)
        {
            this._stream = stream;
        }

        public virtual async Task<byte[]> ReadAsync(CancellationToken token)
        {
            var headSize = StreamMessageHeader.HeaderSize;
            var headBytes = new byte[headSize];
            var read = await _stream.ReadAsync(headBytes, 0, headSize, token);
            if (read < headSize)
            {
                throw new EndOfStreamException();
            }

            var messageHeader = headBytes.ToStruct<StreamMessageHeader>();
            if (!messageHeader.Verify())
            {
                throw new Exception($"Verify head failed");
            }

            var contentLength = messageHeader.Length;
            var contentBytes = new byte[contentLength];
            //由于单条消息不大，不做性能优化
            var readAsync = await _stream.ReadAsync(contentBytes, 0, contentLength, token);
            if (readAsync < contentLength)
            {
                throw new EndOfStreamException();
            }

            return contentBytes;
        }

        public virtual byte[] Read()
        {
            var headSize = StreamMessageHeader.HeaderSize;
            var headBytes = new byte[headSize];
            var read = _stream.Read(headBytes, 0, headSize);
            if (read < headSize)
            {
                throw new EndOfStreamException();
            }

            var messageHeader = headBytes.ToStruct<StreamMessageHeader>();
            if (!messageHeader.Verify())
            {
                throw new Exception($"Verify head failed");
            }

            var contentLength = messageHeader.Length;
            var contentBytes = new byte[contentLength];
            //由于单条消息不大，不做性能优化
            var readAsync = _stream.Read(contentBytes, 0, contentLength);
            if (readAsync < contentLength)
            {
                throw new EndOfStreamException();
            }

            return contentBytes;
        }

        public virtual async Task WriteAsync(byte[] content, CancellationToken token)
        {
            var headSize = StreamMessageHeader.HeaderSize;
            if (content == null || !content.Any())
            {
                throw new ArgumentNullException(nameof(content));
            }

            var contentLength = content.Length;
            var namedPipeMessageHead = new StreamMessageHeader(contentLength);
            var arrayBytes = namedPipeMessageHead.GetBytes();
            await _stream.WriteAsync(arrayBytes, 0, headSize, token);
            await _stream.WriteAsync(content, 0, contentLength, token);
            await _stream.FlushAsync(token);
        }

        public virtual void Write(byte[] content)
        {
            var headSize = StreamMessageHeader.HeaderSize;
            if (content == null || !content.Any())
            {
                throw new ArgumentNullException(nameof(content));
            }

            var contentLength = content.Length;
            var namedPipeMessageHead = new StreamMessageHeader(contentLength);
            var arrayBytes = namedPipeMessageHead.GetBytes();
            _stream.Write(arrayBytes, 0, headSize);
            _stream.Write(content, 0, contentLength);
            _stream.Flush();
        }
    }
}