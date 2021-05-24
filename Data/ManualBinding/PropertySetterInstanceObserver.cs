using System;
using System.Linq.Expressions;

namespace FORCEBuild.Data.ManualBinding
{
    public class PropertySetterInstanceObserver<T, TK> : PropertySetter<T, TK>, IObserver<TK>
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
            if (_instanceProvider == null)
            {
                throw new NullReferenceException("Must call method OnValue first!");
            }

            var instance = _instanceProvider.Instance();
            this.Set(instance, value);
        }

        public void OnError(Exception error)
        {
            //do nothing
        }

        public void OnCompleted()
        {
            //do nothing
        }

        public PropertySetterInstanceObserver(Expression<Func<T, TK>> expression, IObservable<TK> tk) : base(expression)
        {
            this._tk = tk;
            tk.Subscribe(this);
        }
    }
}