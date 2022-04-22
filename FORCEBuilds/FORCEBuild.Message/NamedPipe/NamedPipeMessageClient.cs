using System.IO;
using System.IO.Pipes;
using System.Runtime.Serialization;
using System.Threading;
using System.Threading.Tasks;
using FORCEBuild.Net.Base;

namespace FORCEBuild.Net.NamedPipe
{
    // not implement dispose
    public class NamedPipeMessageClient : INamedPipeMessageClient
    {
        public IFormatter Formatter { get; set; }

        public IMessage GetResponse(IMessage message)
        {
            Task<IMessage> task = Task.Run(( async () =>await this.GetResponseAsync(message,CancellationToken.None)));
            return task.Result;
        }

        public bool CanRequest { get; } = true;

        /// <summary>
        /// 
        /// </summary>
        /// <param name="pipeName"></param>
        public NamedPipeMessageClient(string pipeName)
        {
            this.PipeName = pipeName;
        }

        public string PipeName { get; set; }

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public async Task<IMessage> GetResponseAsync(IMessage message, CancellationToken token)
        {
            using (var clientStream = new NamedPipeClientStream(PipeName))
            {
                await clientStream.ConnectAsync(token);
                using (var readWriter = new NamedPipeMessageFormatterReadWriter(Formatter,clientStream))
                {
                    await readWriter.WriteMessageAsync(message,token);
                    var readMessageAsync = await readWriter.ReadMessageAsync(token);
                    return readMessageAsync;
                }
            }
        }
    }
}