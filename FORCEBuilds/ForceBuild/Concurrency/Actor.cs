using System.Collections.Generic;

namespace FORCEBuild.Concurrency
{
    public abstract class Actor<T>:IActor
    {
        private readonly Queue<T> _mMessageQueue = new Queue<T>();

        public void Post(T message)
        {
            if (this.m_exited) return;

            lock (this._mMessageQueue)
            {
                this._mMessageQueue.Enqueue(message);
            }

            ActorDispatcher.Instance.ReadyToExecute(this);
        }

        protected abstract void Receive(T message);

        protected void Exit()
        {
            this.m_exited = true;
        }

        protected Actor()
        {
            this._mContext = new ActorContext(this);
        }
        
        private readonly ActorContext _mContext;

        ActorContext IActor.Context => this._mContext;

        public bool Existed => this.m_exited;

        public int MessageCount => this._mMessageQueue.Count;

        public void Execute()
        {
            T message;
            lock (this._mMessageQueue)
            {
                message = this._mMessageQueue.Dequeue();
            }

            this.Receive(message);
        }

        private bool m_exited = false;

    }
}