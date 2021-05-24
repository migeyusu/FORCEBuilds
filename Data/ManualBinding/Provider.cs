using System;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    /// <summary>
    /// 属性访问器的桥梁，一对一绑定
    /// </summary>
    /// <typeparam name="T"></typeparam>
    /// <typeparam name="S"></typeparam>
    public interface IPropertyBridge<T,S>
    {
        void BridgeTo(T t, S s);

        void BridgeFrom(S s, T t);
    }

    /*public class BridgeHead<T,S>
    {
        private readonly IInstanceProvider<T> _leftProvider;
        private readonly IInstanceProvider<S> _rightProvider;

        private readonly IPropertyBridge<T, S> _propertyBridge;
        
        public BridgeHead(IInstanceProvider<T> leftProvider, IInstanceProvider<S> rightProvider, IPropertyBridge<T, S> propertyBridge)
        {
            _leftProvider = leftProvider;
            _rightProvider = rightProvider;
            _propertyBridge = propertyBridge;
        }

        public void BridgeFrom()
        {
            var leftInstance = _leftProvider.Instance();
            var rightInstance = _rightProvider.Instance();
            _propertyBridge.BridgeFrom(rightInstance, leftInstance);
        }

        public void BridgeTo()
        {
            
        }


        
    }*/
    
    /// <summary>
    /// used for binding
    /// </summary>
    public class PropertyBridge<T, S, TK>: IPropertyBridge<T,S>
    {
        private readonly PropertyAccessor<T, TK> _leftAccessor;
        private readonly PropertyAccessor<S, TK> _rightAccessor;

        public PropertyBridge(PropertyAccessor<T, TK> leftAccessor,PropertyAccessor<S, TK> rightAccessor)
        {
            _rightAccessor = rightAccessor;
            _leftAccessor = leftAccessor;
        }


        public void BridgeTo(T t, S s)
        {
            var k = _leftAccessor.Get(t);
            _rightAccessor.Set(s,k);
        }

        public void BridgeFrom(S s, T t)
        {
            var k = _rightAccessor.Get(s);
            _leftAccessor.Set(t,k);
        }
    }

    public interface IInstanceProvider<T>
    {
        T Instance();
    }

    public class DelegateInstanceFactory<T> : IInstanceProvider<T>
    {
        private readonly Func<T> _func;

        public DelegateInstanceFactory(Func<T> func)
        {
            _func = func ?? throw new ArgumentNullException(nameof(func));
        }

        public T Instance()
        {
            return _func.Invoke();
        }
    }

    public class DefaultInstanceFactory<T> : IInstanceProvider<T>
    {
        private readonly T _t;

        public DefaultInstanceFactory(T t)
        {
            this._t = t;
        }

        public T Instance()
        {
            return _t;
        }
    }
}