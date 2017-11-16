namespace FORCEBuild.Concurrency
{
    internal class ActorContext
    {
        public const int WAITING = 0;
        public const int EXECUTING = 1;
        public const int EXITED = 2;

        public int m_status;

        public ActorContext(IActor actor)
        {
            this.Actor = actor;
        }

        public IActor Actor { get; private set; }
    }
}