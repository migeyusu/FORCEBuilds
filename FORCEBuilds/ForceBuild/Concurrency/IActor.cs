namespace FORCEBuild.Concurrency
{
    internal interface IActor
    {
        void Execute();

        bool Existed { get; }

        int MessageCount { get; }

        ActorContext Context { get; }
    }
}