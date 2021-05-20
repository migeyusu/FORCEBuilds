using System;
using System.Collections.Generic;

namespace FORCEBuild.Data
{
    public class ObservableReceiver<TK> : IValueReceiver<TK>
    {
        private readonly IList<IValueReceiver<TK>> _receivers = new List<IValueReceiver<TK>>();

        public void Add(IValueReceiver<TK> receiver)
        {
            _receivers.Add(receiver);
        }


        public void Receive(TK x)
        {
            foreach (var receiver in _receivers)
            {
                receiver.Receive(x);
            }
        }
    }

    public class ValueProvider<T, TK> : IValueProvider<TK>
    {
        private readonly IInstanceProvider<T> _instanceProvider;

        private readonly IPropertyGet<T, TK> _propertyGet;

        public ValueProvider(IInstanceProvider<T> instanceProvider, IPropertyGet<T, TK> propertyGet)
        {
            _instanceProvider = instanceProvider;
            _propertyGet = propertyGet;
        }

        public TK Provide()
        {
            var instance = _instanceProvider.Instance();
            return _propertyGet.Get(instance);
        }
    }

    public class ValueDeceiver<T, TK> : IValueReceiver<TK>
    {
        private readonly IInstanceProvider<T> _instanceProvider;

        private readonly IPropertySet<T, TK> _set;

        public ValueDeceiver(IInstanceProvider<T> instanceProvider, IPropertySet<T, TK> set)
        {
            _instanceProvider = instanceProvider;
            this._set = set;
        }

        public void Receive(TK x)
        {
            var instance = _instanceProvider.Instance();
            _set.Set(instance, x);
        }
    }

    /// <summary>
    /// encapsulate property accessor
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class TypeMappingBridge<T> : IInstanceConsumer<T>
    {
        private readonly Action<T> _valuePass;

        public TypeMappingBridge(Action<T> valuePass)
        {
            this._valuePass = valuePass;
        }

        public void Consume(T t)
        {
            _valuePass.Invoke(t);
        }
    }
    
}