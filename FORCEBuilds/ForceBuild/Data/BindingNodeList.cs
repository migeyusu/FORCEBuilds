using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;

namespace FORCEBuild.Data
{
    public static class BridgeExtension
    {
        public static PropertyInstanceAccessor<T, TK> Instance<T, TK>(this PropertyAccessor<T, TK> accessor, T t)
        {
            return new PropertyInstanceAccessor<T, TK>(t, accessor);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="get">source</param>
        /// <param name="set">target</param>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <typeparam name="TS"></typeparam>
        /// <returns></returns>
        public static TwoWayMappingBridge<T, TS> To<T, TK, TS>(this IPropertyGet<T, TK> get, IPropertySet<TS, TK> set)
        {
            void BridgeAction(T t, TS ts)
            {
                var tk = get.Get(t);
                set.Set(ts, tk);
            }

            return new TwoWayMappingBridge<T, TS>(BridgeAction);
        }

        public static DelegateMappingBridge To<T>(this IValueProvider<T> provider, IValueReceiver<T> receiver)
        {
            void BridgeAction()
            {
                var provide = provider.Provide();
                receiver.Receive(provide);
            }

            return new DelegateMappingBridge(BridgeAction);
        }
    }

    public interface IMappingBridge
    {
        Direction Direction { get; }
    }

    public enum Direction
    {
        To,
        From,
    }
    
    
    /// <summary>
    /// 单向绑定，允许一对多
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OnewayDelegateMappingBridge<T>
    {
        private Action<T> _valuePass;

        public OnewayDelegateMappingBridge(Action<T> valuePass)
        {
            this._valuePass = valuePass;
        }
    }
    
    /// <summary>
    /// 委托绑定，可一对一，可一对多
    /// </summary>
    public class DelegateMappingBridge
    {
        private readonly Action _fromAction;

        private Action _toAction;

        public DelegateMappingBridge(Action fromAction, Action toAction)
        {
            _fromAction = fromAction;
            _toAction = toAction;
        }

        public void Invoke()
        {
            _fromAction.Invoke();
        }
    }
    
    /// <summary>
    /// 双向绑定，一对一
    /// </summary>
    public class TwoWayMappingBridge<T, TS>
    {
        private Action<T, TS> _settingAction;

        Action<TS,T>  
        public TwoWayMappingBridge(Action<T, TS> x)
        {
            this._settingAction = x;
        }
    }

    public class InstanceBinder
    {
        private readonly IList<DelegateMappingBridge> _bridges = new List<DelegateMappingBridge>();

        public InstanceBinder Register(DelegateMappingBridge bridge)
        {
            _bridges.Add(bridge);
            return this;
        }

        public void Notify()
        {
            foreach (var bridge in _bridges)
            {
                bridge.Invoke();
            }
        }

    }


    public class ClassInstaBinder<T>
    {
        public ClassBinder()
        {
        }

        private readonly IList<PropertyAccessor<T>> _classBinders = new List<IPropertySet<,>>();

        public PropertyAccessor<T, TK> Open<TK>(Expression<Func<T, TK>> expression)
        {
            var propertyBinder = new PropertyAccessor<T, TK>(expression);
            // _classBinders.Add(propertyBinder);
            return propertyBinder;
        }

        public void Apply(T t)
        {
            foreach (var binder in _classBinders)
            {
                binder.Set(t, TODO);
            }
        }
    }


    /// <summary>
    ///  期望的实例类型
    /// </summary>
    /// <typeparam name="K">class type</typeparam>
    /// <typeparam name="KS">property type</typeparam>
    public interface IPropertySet<K, KS>
    {
        /// <summary>
        /// 接收一个实例类型
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        void Set(K x, KS y);
    }

    /// <summary>
    /// 
    /// </summary>
    /// <typeparam name="T">class type</typeparam>
    /// <typeparam name="TK">property type</typeparam>
    public interface IPropertyGet<T, TK>
    {
        /// <summary>
        /// 广播一个类型的实例
        /// </summary>
        /// <param name="t"></param>
        TK Get(T t);
    }

    public interface IValueProvider<T>
    {
        T Provide();
    }

    public interface IValueReceiver<T>
    {
        void Receive(T x);
    }

    public class PropertyDelegateAccessor<T, TK> : PropertyAccessor<T, TK>, IValueProvider<TK>, IValueReceiver<TK>
    {
        private readonly Func<T> _factoryDelegate;

        public PropertyDelegateAccessor(Expression<Func<T, TK>> expression, Func<T> factoryDelegate) : base(expression)
        {
            this._factoryDelegate = factoryDelegate;
        }

        public TK Provide()
        {
            var invoke = _factoryDelegate.Invoke();
            return GetMethodDelegate.Invoke(invoke);
        }

        public void Receive(TK x)
        {
            var invoke = _factoryDelegate.Invoke();
            SetMethodDelegate.Invoke(invoke, x);
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

    /// <summary>
    /// 类型的属性绑定
    /// </summary>
    /// <typeparam name="T">期望的绑定class type</typeparam>
    /// <typeparam name="TK">期望绑定的属性 type</typeparam>
    public class PropertyAccessor<T, TK> : IPropertySet<T, TK>, IPropertyGet<T, TK>
    {
        public Expression<Func<T, TK>> Expression { get; }

        public MethodInfo GetMethodInfo { get; private set; }

        public Func<T, TK> GetMethodDelegate { get; private set; }

        public MethodInfo SetMethodInfo { get; private set; }

        public Action<T, TK> SetMethodDelegate { get; private set; }

        public PropertyAccessor(Expression<Func<T, TK>> expression)
        {
            this.Expression = expression;
            this.Compile();
        }

        public void Compile()
        {
            if (!(Expression.Body is MemberExpression memberExpression))
            {
                throw new Exception("Please use MemberExpression");
            }

            var memberExpressionMember = memberExpression.Member;
            if (!(memberExpressionMember is PropertyInfo propertyInfo))
            {
                throw new Exception($"Please define property expression such as x=>x.y in class {typeof(T).Name}");
            }

            if (!propertyInfo.CanRead)
            {
                throw new Exception($"Property {propertyInfo.Name} should be readable!");
            }

            GetMethodInfo = propertyInfo.GetMethod;
            GetMethodDelegate = Delegate.CreateDelegate(typeof(Func<T, TK>), GetMethodInfo) as Func<T, TK>;
            SetMethodInfo = propertyInfo.SetMethod;
            SetMethodDelegate = Delegate.CreateDelegate(typeof(Action<T, TK>), SetMethodInfo) as Action<T, TK>;
        }


        public void Set(T x, TK y)
        {
            SetMethodDelegate.Invoke(x, y);
        }

        public TK Get(T t)
        {
            return GetMethodDelegate.Invoke(t);
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