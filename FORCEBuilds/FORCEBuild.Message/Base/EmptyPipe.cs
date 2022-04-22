namespace FORCEBuild.Net.Base
{
    public class EmptyPipe<TInput, TOutput> : MessagePipe<TInput, TOutput>
    {
        protected override TOutput InternalProcess(TInput input)
        {
            if (input is TOutput tOutput)
            {
                return tOutput;
            }

            return default(TOutput);
        }
    }
}