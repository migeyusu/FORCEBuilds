using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace FORCEBuild.Data
{
    interface IMapping<T, K>
    {
        void Map(T t, K k);
    }

    public class MyClass
    {
    }

    public static class BridgeExtension
    {
        public static PropertyInstanceAccessor<T, TK> Instance<T, TK>(this PropertyAccessor<T, TK> accessor, T t)
        {
            return new PropertyInstanceAccessor<T, TK>(t, accessor);
        }


        public static IInstanceConsumer<T> OneWayTo<T, TK>(this IPropertyGet<T, TK> get, Action<TK> action)
        {
            var receiver = new ActionValueReceiver<TK>(action);
            return new InstanceConsumer<T, TK>(get, receiver);
        }


        public static TypeMappingBridge<T> OneWayTo<T, TK>(this IPropertyGet<T, TK> get, Func<IValueReceiver<TK>> func)
        {
            void Action(T obj)
            {
                var k = get.Get(obj);
                var receiver = func.Invoke();
                receiver.Receive(k);
            }

            return new TypeMappingBridge<T>(Action);
        }

        public static TypeMappingBridge<T> OneWayTo<T, TK>(this IPropertyGet<T, TK> get, IValueReceiver<TK> set)
        {
            void Action(T obj)
            {
                var k = get.Get(obj);
                set.Receive(k);
            }

            return new TypeMappingBridge<T>(Action);
        }

        /*public static IValueProvider<TK> ReadFrom<T, TK>(this IPropertyGet<T, TK> get, Func<T> provider)
        {
            return new ValueProvider<T, TK>(new DelegateInstanceFactory<T>(provider), get);
        }*/

        public static IValueProvider<TK> ReadFrom<T, TK>(this IPropertyGet<T, TK> get, IInstanceProvider<T> provider)
        {
            return new ValueProvider<T, TK>(provider, get);
        }

        public static ObservableValueProvider<T, TK> ToObservable<T, TK>(this IPropertyGet<T, TK> propertyGet)
        {
            return new ObservableValueProvider<T, TK>(propertyGet);
        }

        /*public static PropertyGetterDelegateSubscriber<S, TK> OnBind<T, TK, S>(
            this ObservableValueProvider<T, TK> provider,
            Action<S> receiverExpression)
        {
            
            var propertyGetter = new PropertyGetterDelegateSubscriber<S, TK>(new Expression<Action<S>>(receiverExpression), provider);
            return propertyGetter;
        }*/


        public static IValueReceiver<TK> SetTo<T, TK>(this IPropertySet<T, TK> propertySet,
            IInstanceProvider<T> provider)
        {
            return new ValueDeceiver<T, TK>(provider, propertySet);
        }
    }

    public class PropertyGetterDelegateSubscriber<T, TK> : PropertyGetter<T, TK>, IObserver<TK>
    {
        private readonly IObservable<TK> _tk;

        private IInstanceProvider<T> _instanceProvider;

        public IObservable<TK> OnInstance(IInstanceProvider<T> instanceProvider)
        {
            this._instanceProvider = instanceProvider;
            return _tk;
        }

        public void OnNext(TK value)
        {
            if (_instanceProvider==null)
            {
                throw new NullReferenceException("Must call method OnValue first!");
            }
        }

        public void OnError(Exception error)
        {
            //do nothing
        }

        public void OnCompleted()
        {
            //do nothing
        }

        public PropertyGetterDelegateSubscriber(Expression<Action<T>> expression, IObservable<TK> tk) : base(expression)
        {
            this._tk = tk;
            tk.Subscribe(this);
        }
    }

    /// <summary>
    /// 一对多
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="TK"></typeparam>
    public class ObservableValueProvider<T, TK> : IObservable<TK>, IPropertyGet<T, TK>
    {
        private readonly IPropertyGet<T, TK> _propertyGet;

        private IList<IValueReceiver<TK>> _receivers = new List<IValueReceiver<TK>>();

        public ObservableValueProvider(IPropertyGet<T, TK> propertyGet)
        {
            _propertyGet = propertyGet;
        }

        public IDisposable Subscribe(IObserver<TK> observer)
        {
            return null;
        }

        public TK Get(T t)
        {
            return _propertyGet.Get(t);
        }
    }


    public interface IInstanceConsumer<T>
    {
        void Consume(T t);
    }

    public class InstanceConsumer<T, TK> : IInstanceConsumer<T>
    {
        private readonly IPropertyGet<T, TK> _get;

        private readonly IValueReceiver<TK> _receiver;

        public InstanceConsumer(IPropertyGet<T, TK> get, IValueReceiver<TK> receiver)
        {
            _receiver = receiver;
            _get = get;
        }

        public void Consume(T t)
        {
            var k = _get.Get(t);
            _receiver.Receive(k);
        }
    }

    /// <summary>
    /// provider connection
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ValueBridge<T>
    {
        private readonly IValueProvider<T> _provider;
        private readonly IList<IValueReceiver<T>> _receivers;

        public ValueBridge(IValueProvider<T> provider, IValueReceiver<T> receiver)
        {
            _receivers = new List<IValueReceiver<T>>() {receiver};
            _provider = provider;
        }

        public void Add(IValueReceiver<T> receiver)
        {
            this._receivers.Add(receiver);
        }

        public ValueBridge<T> OnValue(IValueReceiver<T> receiver)
        {
            this._receivers.Add(receiver);
            return this;
        }

        public void Push()
        {
            var provide = _provider.Provide();
            foreach (var receiver in _receivers)
            {
                receiver.Receive(provide);
            }
        }
    }

    public interface IValueProvider<T>
    {
        T Provide();
    }

    public interface IValueReceiver<T>
    {
        void Receive(T x);
    }

    public class ActionValueReceiver<T> : IValueReceiver<T>
    {
        private readonly Action<T> _action;

        public ActionValueReceiver(Action<T> action)
        {
            _action = action;
        }

        public void Receive(T x)
        {
            _action.Invoke(x);
        }
    }

    public class PropertyInstanceAccessor<T, TK> : PropertyAccessor<T, TK>, IValueProvider<TK>, IValueReceiver<TK>
    {
        private readonly T _t;

        public PropertyInstanceAccessor(T t, PropertyAccessor<T, TK> accessor) : base(accessor.Expression)
        {
            this._t = t;
        }

        public TK Provide()
        {
            return GetMethodDelegate.Invoke(_t);
        }

        public void Receive(TK x)
        {
            SetMethodDelegate.Invoke(_t, x);
        }
    }


    public class BindingNodeList<T>
    {
        public BindingNodeList()
        {
        }

        protected virtual void Invoke(T t)
        {
        }

        public BindingNode<TResult> Bind<TResult>(Expression<Func<T, TResult>> valueExpression,
            Expression<Action<TResult>> consumeExpression)
        {
            return new BindingNode<TResult>(this, valueExpression, consumeExpression);
        }

        public class BindingNode<TK> : BindingNodeList<T>
        {
            private readonly BindingNodeList<T> _previousNode;

            private readonly Expression<Func<T, TK>> _valueExpression;

            private readonly Expression<Action<TK>> _consumeExpression;

            public BindingNode(BindingNodeList<T> previousNode, Expression<Func<T, TK>> resultExpression,
                Expression<Action<TK>> consumeExpression)
            {
                this._valueExpression = resultExpression;
                this._consumeExpression = consumeExpression;
                _previousNode = previousNode;
            }

            protected override void Invoke(T t)
            {
                _previousNode.Invoke(t);
                var invoke = _valueExpression.Compile().Invoke(t);
                _consumeExpression.Compile().Invoke(invoke);
            }
        }
    }
}