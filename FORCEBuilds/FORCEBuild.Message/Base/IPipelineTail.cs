using System;

namespace FORCEBuild.Message.Base
{
    public interface IPipelineTail<TK>
    {
        TK GetResponse();
    }


    public class PipeStage<T,TK>
    {
        private Func<T, TK> _func;

        public PipeStage(Func<T,TK> func)
        {   
            _func = func;
        }

        public PipeStage<T,TNextK> Append<TNextK>(Func<TK,TNextK> nextProcess)
        {
            return new PipeStage<T,TNextK>(arg => nextProcess(this._func(arg)));
        }

        public TK Process(T x)
        {
            return _func.Invoke(x);
        }

    }


}