using System;
using System.Collections.Concurrent;

namespace FORCEBuild    
{
    public class ObjectPool<T>
    {
        protected readonly ConcurrentBag<T> _objects;
        protected readonly Func<T> _objectGenerator;
        
        public ObjectPool(Func<T> objectGenerator)
        {
            _objects = new ConcurrentBag<T>();
            _objectGenerator = objectGenerator ?? throw new ArgumentNullException("objectGenerator");
        }

        public virtual T GetObject()
        {
            return _objects.TryTake(out T item) ? item : _objectGenerator();
        }

        public virtual void PutObject(T item)
        {
            _objects.Add(item);
        }
    }
}
