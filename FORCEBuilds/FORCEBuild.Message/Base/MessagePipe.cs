using System;
using System.Reflection;

namespace FORCEBuild.Net.Base
{
    /// <summary>
    /// 消息处理管道，可以自定义配置基于消息的处理管道
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public abstract class MessagePipe<TInput, TOutput> : IDisposable
    {
        public MessagePipe<TInput, TNextOutput> Append<TNextOutput>(Func<TOutput, TNextOutput> func)
        {
            return new Flange<TNextOutput>(this, new FuncPipe<TOutput, TNextOutput>(func));
        }

        public MessagePipe<TInput, TNextOutput> Append<TNextOutput>(MessagePipe<TOutput, TNextOutput> pipe)
        {
            return new Flange<TNextOutput>(this, pipe);
        }

        public TOutput Process(TInput input)
        {
            return InternalProcess(input);
        }

        protected abstract TOutput InternalProcess(TInput input);

        public virtual void Dispose()
        {
        }

        private sealed class Flange<TNextOutput> : MessagePipe<TInput, TNextOutput>
        {
            private readonly MessagePipe<TInput, TOutput> _preMessagePipe;

            private readonly MessagePipe<TOutput, TNextOutput> _lastMessagePipe;

            public Flange(MessagePipe<TInput, TOutput> preMessagePipe, MessagePipe<TOutput, TNextOutput> thisStageFunc)
            {
                this._preMessagePipe = preMessagePipe;
                _lastMessagePipe = thisStageFunc;
            }

            protected override TNextOutput InternalProcess(TInput input)
            {
                return _lastMessagePipe.InternalProcess(_preMessagePipe.InternalProcess(input));
            }

            public override void Dispose()
            {
                base.Dispose();
                _preMessagePipe.Dispose();
                _lastMessagePipe.Dispose();
            }
        }
    }
}