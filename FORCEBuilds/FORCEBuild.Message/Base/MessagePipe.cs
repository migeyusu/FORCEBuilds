using System;
using System.Reflection;

namespace FORCEBuild.Net.Base
{
    /// <summary>
    /// 消息处理管道，可以自定义配置基于消息的处理管道
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class MessagePipe<TInput,TOutput>:IDisposable
    {
        private readonly Func<TInput, TOutput> _stageFunc;

        public MessagePipe(Func<TInput, TOutput> stageFunc)
        {
            _stageFunc = stageFunc;
        }

        public MessagePipe():this(null)
        {   
            
        }

        public MessagePipe<TInput, TNextOutput> Append<TNextOutput>(Func<TOutput, TNextOutput> func)
        {
            return new Flange<TNextOutput>(this,new MessagePipe<TOutput, TNextOutput>(func));
        }

        public MessagePipe<TInput, TNextOutput> Append<TNextOutput>(MessagePipe<TOutput, TNextOutput> pipe)
        {
            return new Flange<TNextOutput>(this, pipe);
        }

        public TOutput Process(TInput input)
        {
            return InternalProcess(input);
        }

        protected virtual TOutput InternalProcess(TInput input)
        {
            if (_stageFunc==null) {
                throw new Exception($"需要重写方法{MethodBase.GetCurrentMethod().Name}或实现委托{_stageFunc}");
            }
            return this._stageFunc.Invoke(input);
        }


        public virtual void Dispose() { }

        private sealed class Flange<TNextOutput>:MessagePipe<TInput,TNextOutput>
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