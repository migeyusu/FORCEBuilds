using System;
using System.Net;

namespace FORCEBuild.Message.Base
{
    /// <summary>
    /// 消息管线的一级
    /// </summary>
    /// <typeparam name="TInput">传入类型</typeparam>
    /// <typeparam name="TOutput">传出类型</typeparam>
    public class PipelineStage<TInput, TOutput> :IDisposable 
        where TInput:IMessage
        where TOutput:IMessage
    {
        private readonly Func<TInput, TOutput> _stageFunc;

        public PipelineStage(Func<TInput,TOutput> func)
        {
            _stageFunc = func;
        }

        public PipelineStage<TInput,TNextOutput> Append<TNextOutput>(Func<TOutput,TNextOutput> func) where TNextOutput:IMessage
        {
            return new Pipeline<TNextOutput>(this, func);
        }
        
        public void Request(TInput x)
        {

        }

        protected virtual TOutput InternalProcess(TInput x)
        {
            return this._stageFunc(x);
        }

        public TOutput Process(TInput x)
        {
            return InternalProcess(x);
        }

        public virtual void Dispose() { }

        private sealed class Pipeline<TNextOutput> : PipelineStage<TInput, TNextOutput> where TNextOutput:IMessage
        {
            private readonly PipelineStage<TInput, TOutput> _beginningStage;

            private readonly Func<TOutput, TNextOutput> _lastStageFunc;

            public Pipeline(PipelineStage<TInput, TOutput> beginningStage, Func<TOutput, TNextOutput> func) : base(null)
            {
                _beginningStage = beginningStage;
                _lastStageFunc = func;
            }

            protected override TNextOutput InternalProcess(TInput x)
            {
                return _lastStageFunc.Invoke(_beginningStage.Process(x));
            }

        }
    }

}