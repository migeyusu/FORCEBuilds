using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Threading;

namespace FORCEBuild.Concurrency
{
        /// <summary>
        /// Concurrent collection that emits change notifications on a dispatcher thread
        /// </summary>
        /// <typeparam name="T">The type of objects in the collection</typeparam>
        [Serializable]
        [ComVisible(false)]
        [HostProtection(SecurityAction.LinkDemand, Synchronization = true, ExternalThreading = true)]
        public class ObservableConcurrentBag<T> : IProducerConsumerCollection<T>,
            IEnumerable<T>, ICollection, IEnumerable
        {
            /// <summary>
            /// The dispatcher on which event notifications will be raised
            /// </summary>
            private readonly Dispatcher dispatcher;

            /// <summary>
            /// The internal concurrent bag used for the 'heavy lifting' of the collection implementation
            /// </summary>
            private readonly ConcurrentBag<T> internalBag;

            /// <summary>
            /// Initializes a new instance of the ConcurrentBag<T> class that will raise <see cref="INotifyCollectionChanged"/> events
            /// on the specified dispatcher
            /// </summary>
            public ObservableConcurrentBag(Dispatcher dispatcher)
            {
                this.dispatcher = dispatcher;
                this.internalBag = new ConcurrentBag<T>();
            }

            /// <summary>
            /// Initializes a new instance of the ConcurrentBag<T> class that contains elements copied from the specified collection 
            /// that will raise <see cref="INotifyCollectionChanged"/> events on the specified dispatcher
            /// </summary>
            public ObservableConcurrentBag(Dispatcher dispatcher, IEnumerable<T> collection)
            {
                this.dispatcher = dispatcher;
                this.internalBag = new ConcurrentBag<T>(collection);
            }

            /// <summary>
            /// Occurs when the collection changes
            /// </summary>
            public event NotifyCollectionChangedEventHandler CollectionChanged;

            /// <summary>
            /// Raises the <see cref="CollectionChanged"/> event on the <see cref="dispatcher"/>
            /// </summary>
            private void RaiseCollectionChangedEventOnDispatcher(NotifyCollectionChangedEventArgs e)
            {
                this.dispatcher.BeginInvoke(new Action<NotifyCollectionChangedEventArgs>(this.RaiseCollectionChangedEvent), e);
            }

            /// <summary>
            /// Raises the <see cref="CollectionChanged"/> event
            /// </summary>
            /// <remarks>
            /// This method must only be raised on the dispatcher - use <see cref="RaiseCollectionChangedEventOnDispatcher" />
            /// to do this.
            /// </remarks>
            private void RaiseCollectionChangedEvent(NotifyCollectionChangedEventArgs e)
            {
                this.CollectionChanged(this, e);
            }

            #region Members that pass through to the internal concurrent bag but also raise change notifications

            bool IProducerConsumerCollection<T>.TryAdd(T item)
            {
                var result = ((IProducerConsumerCollection<T>)this.internalBag).TryAdd(item);
                if (result)
                {
                    this.RaiseCollectionChangedEventOnDispatcher(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
                }
                return result;
            }

            public void Add(T item)
            {
                this.internalBag.Add(item);
                this.RaiseCollectionChangedEventOnDispatcher(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Add, item));
            }

            public bool TryTake(out T item)
            {
                var result = this.TryTake(out item);
                if (result)
                {
                    this.RaiseCollectionChangedEventOnDispatcher(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Remove, item));
                }
                return result;
            }

            #endregion

            #region Members that pass through directly to the internal concurrent bag

            public int Count
            {
                get
                {
                    return this.internalBag.Count;
                }
            }

            public bool IsEmpty
            {
                get
                {
                    return this.internalBag.IsEmpty;
                }
            }

            bool ICollection.IsSynchronized
            {
                get
                {
                    return ((ICollection)this.internalBag).IsSynchronized;
                }
            }

            object ICollection.SyncRoot
            {
                get
                {
                    return ((ICollection)this.internalBag).SyncRoot;
                }
            }

            IEnumerator<T> IEnumerable<T>.GetEnumerator()
            {
                return ((IEnumerable<T>)this.internalBag).GetEnumerator();
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return ((IEnumerable)this.internalBag).GetEnumerator();
            }

            public T[] ToArray()
            {
                return this.internalBag.ToArray();
            }

            void IProducerConsumerCollection<T>.CopyTo(T[] array, int index)
            {
                ((IProducerConsumerCollection<T>)this.internalBag).CopyTo(array, index);
            }

            void ICollection.CopyTo(Array array, int index)
            {
                ((ICollection)this.internalBag).CopyTo(array, index);
            }

            #endregion
        }
}