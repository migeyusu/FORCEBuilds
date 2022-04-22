using System;

namespace FORCEBuild.Net.Base
{
    public class FuncPipe<TInput, TOutput> : MessagePipe<TInput, TOutput>
    {
        private readonly Func<TInput, TOutput> _stageFunc;

        public FuncPipe(Func<TInput, TOutput> stageFunc)
        {
            _stageFunc = stageFunc ?? throw new ArgumentNullException(nameof(stageFunc));
        }

        protected override TOutput InternalProcess(TInput input)
        {
            return _stageFunc.Invoke(input);
        }
    }
}