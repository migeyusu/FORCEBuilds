using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
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

    public class FactoryBinder<T>: TypeBinder<T>
    {
        private IInstanceProvider<T> _provider;

        public FactoryBinder(IInstanceProvider<T> provider)
        {
            _provider = provider;
        }
    }
    
    public class InstanceBinder<T>: TypeBinder<T>
    {
        private T _t;

        public InstanceBinder(T t)
        {
            this._t = t;
        }
    }
    
    public class PropertyInstanceBinder<T, TK> : PropertyAccessor<T, TK>, IValueProvider<TK>, IValueReceiver<TK>
    {
        private readonly T _t;

        public PropertyInstanceBinder(T t, PropertyAccessor<T, TK> accessor) : base(accessor.Expression)
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