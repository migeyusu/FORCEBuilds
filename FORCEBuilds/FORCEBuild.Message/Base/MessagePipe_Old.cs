using System;

namespace FORCEBuild.Message.Base
{
    public class MessagePipe
    {
        public static MessagePipe<TInput, TOutput> Create<TInput, TOutput>(Func<TInput, TOutput> func)
        {
            return new MessagePipe<TInput, TOutput>(func);
        }
    }

    /// <summary>
    /// 消息处理管线
    /// </summary>
    /// <typeparam name="TInput"></typeparam>
    /// <typeparam name="TOutput"></typeparam>
    public class MessagePipe<TInput, TOutput>:IDisposable
    {
        private readonly Func<TInput, TOutput> _processFunc;

        public MessagePipe(Func<TInput, TOutput> processFunc)
        {
            _processFunc = processFunc;
        }

        public MessagePipe() : this(null) { }

        public MessagePipe<TInput, TNextOutput> Append<TNextOutput>(Func<TOutput, TNextOutput> func)
        {
            return new MessagePipe<TInput, TNextOutput>(input => func(InternalProcess(input)));
        }

        public MessagePipe<TInput, TNextOutput> Append<TNextOutput>(MessagePipe<TOutput, TNextOutput> nextPipe)
        {
            return new MessagePipe<TInput, TNextOutput>(input => nextPipe.Process(this.InternalProcess(input)));
        }

        public TOutput Process(TInput input)
        {
            return InternalProcess(input);
        }

        /// <summary>
        /// 如果调用的是无参构造函数，需要重写
        /// </summary>
        /// <param name="input"></param>
        /// <returns></returns>
        protected virtual TOutput InternalProcess(TInput input)
        {
            return _processFunc(input);
        }

        public virtual void Dispose() { }
    }
}