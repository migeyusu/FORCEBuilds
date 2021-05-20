using System;

namespace FORCEBuild.Data
{
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