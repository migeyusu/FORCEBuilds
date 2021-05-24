using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
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