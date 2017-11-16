using System.Threading;

namespace FORCEBuild.Concurrency
{
    internal class ActorDispatcher
    {
        private static ActorDispatcher _instance;

        public static ActorDispatcher Instance => _instance ?? (_instance = new ActorDispatcher());

        private ActorDispatcher() { }

        public void ReadyToExecute(IActor actor)
        {
            if (actor.Existed) return;

            var status = Interlocked.CompareExchange(
                ref actor.Context.m_status,
                ActorContext.EXECUTING,
                ActorContext.WAITING);

            if (status == ActorContext.WAITING)
            {
                ThreadPool.QueueUserWorkItem(this.Execute, actor);
            }
        }

        private void Execute(object o)
        {
            var actor = (IActor) o;
            if (actor.MessageCount>0)
            {
                actor.Execute();
            }
            
            if (actor.Existed) {
                Thread.VolatileWrite(
                    ref actor.Context.m_status,
                    ActorContext.EXITED);
            }
            else {
                Thread.VolatileWrite(
                    ref actor.Context.m_status,
                    ActorContext.WAITING);

                if (actor.MessageCount > 0) {
                    this.ReadyToExecute(actor);
                }
            }
        }
    }
}