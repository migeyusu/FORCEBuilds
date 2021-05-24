using System;
using System.Collections.Generic;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    /// <summary>
    /// 一对多
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TK"></typeparam>
    public class ObservablePropertyGetter<T, TK> : IObservable<TK>, IPropertyGet<T, TK>, IInstanceConsumer<T>
    {
        private readonly IPropertyGet<T, TK> _propertyGet;

        private readonly IList<IObserver<TK>> _observers = new List<IObserver<TK>>();

        public ObservablePropertyGetter(IPropertyGet<T, TK> propertyGet)
        {
            _propertyGet = propertyGet;
        }

        public IDisposable Subscribe(IObserver<TK> observer)
        {
            _observers.Add(observer);
            return null; //todo?:
        }

        public TK Get(T t)
        {
            return _propertyGet.Get(t);
        }

        public void Consume(T t)
        {
            var k = _propertyGet.Get(t);
            foreach (var observer in _observers)
            {
                observer.OnNext(k);
            }
        }
    }
}