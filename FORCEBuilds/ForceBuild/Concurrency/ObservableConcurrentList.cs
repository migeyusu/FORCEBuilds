using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Windows.Threading;
using FORCEBuild.Properties;

namespace FORCEBuild.Concurrency
{
    /* 权衡性能和功能后暂时使用读写锁实现,可以同时在wpf、winform或console中使用
     * 使用SynchronizationContext跨线程调用UI */

    public class ObservableConcurrentList<T> : IList<T>, INotifyCollectionChanged,INotifyPropertyChanged
    {
        private readonly IList<T> _collection;

        private const string CountString = "Count";

        private const string IndexerName = "Item[]";

        private readonly ReaderWriterLock _sync = new ReaderWriterLock();

        public ObservableConcurrentList() : this(new T[] { })
        {

        }

        public ObservableConcurrentList(IEnumerable<T> enumerable)
        {
            _collection = new List<T>(enumerable);
        }

        public void Add(T item)
        {
            _sync.AcquireWriterLock(Timeout.Infinite);
            var index = _collection.Count;
            InternalInsert(index,item);
            _sync.ReleaseWriterLock();
        }

        public void Clear()
        {
            InternalClear();
        }

        private void InternalClear()
        {
            _sync.AcquireWriterLock(Timeout.Infinite);
            _collection.Clear();
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionReset();
            _sync.ReleaseWriterLock();
        }

        public bool Contains(T item)
        {
            _sync.AcquireReaderLock(Timeout.Infinite);
            var result = _collection.Contains(item);
            _sync.ReleaseReaderLock();
            return result;
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _sync.AcquireWriterLock(Timeout.Infinite);
            _collection.CopyTo(array, arrayIndex);
            _sync.ReleaseWriterLock();
        }

        public int Count
        {
            get {
                _sync.AcquireReaderLock(Timeout.Infinite);
                var result = _collection.Count;
                _sync.ReleaseReaderLock();
                return result;
            }
        }

        public bool IsReadOnly => _collection.IsReadOnly;

        public bool Remove(T item)
        {
            _sync.AcquireWriterLock(Timeout.Infinite);
            int index;
            if ((index = _collection.IndexOf(item)) < 0) {
                _sync.ReleaseWriterLock();
                return false;
            }
            InternalRemoveAt(index);
            _sync.ReleaseWriterLock();
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
          //  _sync.AcquireReaderLock(Timeout.Infinite);
            return _collection.ToList().GetEnumerator();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _collection.ToList().GetEnumerator();
        }

        public int IndexOf(T item)
        {
            _sync.AcquireReaderLock(Timeout.Infinite);
            var result = _collection.IndexOf(item);
            _sync.ReleaseReaderLock();
            return result;
        }

        public void Insert(int index, T item)
        {
            _sync.AcquireWriterLock(Timeout.Infinite);
            InternalInsert(index, item);
            _sync.ReleaseWriterLock();
        }

        private void InternalInsert(int index, T item)
        {
            _collection.Insert(index, item);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Add,item,index);
        }

        public void RemoveAt(int index)
        {
            _sync.AcquireWriterLock(Timeout.Infinite);
            InternalRemoveAt(index);
            _sync.ReleaseWriterLock();
        }

        private void InternalRemoveAt(int index)
        {
            if (_collection.Count == 0 || _collection.Count <= index)
            {
                _sync.ReleaseWriterLock();
                return;
            }
            var removeditem = _collection[index];
            _collection.RemoveAt(index);
            OnPropertyChanged(CountString);
            OnPropertyChanged(IndexerName);
            OnCollectionChanged(NotifyCollectionChangedAction.Remove,removeditem,index);
           
        }

        public T this[int index]
        {
            get {
                _sync.AcquireReaderLock(Timeout.Infinite);
                var result = _collection[index];
                _sync.ReleaseReaderLock();
                return result;
            }
            set {
                _sync.AcquireWriterLock(Timeout.Infinite);
                if (_collection.Count == 0 || _collection.Count <= index)
                {
                    _sync.ReleaseWriterLock();
                    return;
                }
                var originalItem = _collection[index];
                _collection[index] = value;
                OnPropertyChanged(IndexerName);
                OnCollectionChanged(NotifyCollectionChangedAction.Replace,originalItem,value,index);
                _sync.ReleaseWriterLock();
            }
        }

        #region event fields

        public event NotifyCollectionChangedEventHandler CollectionChanged;

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index));
        }

        private void OnCollectionChanged(NotifyCollectionChangedAction action, object item, int index, int oldIndex)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, item, index, oldIndex));
        }


        private void OnCollectionChanged(NotifyCollectionChangedAction action, object oldItem, object newItem, int index)
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(action, newItem, oldItem, index));
        }

        private void OnCollectionReset()
        {
            OnCollectionChanged(new NotifyCollectionChangedEventArgs(NotifyCollectionChangedAction.Reset));
        }

        protected virtual void OnCollectionChanged(NotifyCollectionChangedEventArgs e)
        {
            SynchronizationHelper.InvokeAsync(o => {
                CollectionChanged?.Invoke(this, e);
            }, null);
        }

        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion



    }

}