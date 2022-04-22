using System;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.NamedPipe
{
    public abstract class NamedPipeMessageReadWriter : IDisposable
    {
        private readonly Stream _stream;

        private readonly bool _isDisposeInternal;

        public NamedPipeMessageReadWriter(Stream stream, bool isDisposeInternal = true)
        {
            this._stream = stream;
            this._isDisposeInternal = isDisposeInternal;
        }

        public async Task<byte[]> ReadAsync(CancellationToken token)
        {
            token.ThrowIfCancellationRequested();
            var headSize = StreamMessageHeader.HeaderSize;
            var headBytes = new byte[headSize];
            var read = await _stream.ReadAsync(headBytes, 0, headSize, token);
            if (read < headSize)
            {
                throw new EndOfStreamException();
            }

            var handle = GCHandle.Alloc(headBytes, GCHandleType.Pinned);
            StreamMessageHeader messageHead;
            try
            {
                messageHead = Marshal.PtrToStructure<StreamMessageHeader>(handle.AddrOfPinnedObject());
                if (!messageHead.Verify())
                {
                    throw new Exception($"Verify head failed");
                }
            }
            finally
            {
                handle.Free();
            }

            var contentLength = messageHead.Length;
            var contentBytes = new byte[contentLength];
            //由于单条消息不大，不做性能优化
            var readAsync = await _stream.ReadAsync(contentBytes, 0, contentLength, token);
            if (readAsync < contentLength)
            {
                throw new EndOfStreamException();
            }

            return contentBytes;
        }

        public async Task WriteAsync(byte[] content, CancellationToken token)
        {
            var headSize = StreamMessageHeader.HeaderSize;
            if (content == null || !content.Any())
            {
                throw new ArgumentNullException(nameof(content));
            }

            var contentLength = content.Length;
            var namedPipeMessageHead = new StreamMessageHeader(contentLength);
            var arrayBytes = new byte[headSize];
            var allocHGlobal = Marshal.AllocHGlobal(headSize);
            Marshal.StructureToPtr(namedPipeMessageHead, allocHGlobal, true);
            Marshal.Copy(allocHGlobal, arrayBytes, 0, headSize);
            Marshal.FreeHGlobal(allocHGlobal);
            await _stream.WriteAsync(arrayBytes,0,headSize,token);
            await _stream.WriteAsync(content,0,contentLength, token);
            await _stream.FlushAsync(token);
        }

        public void Dispose()
        {
            if (_isDisposeInternal)
            {
                _stream?.Dispose();
            }
        }
    }
}