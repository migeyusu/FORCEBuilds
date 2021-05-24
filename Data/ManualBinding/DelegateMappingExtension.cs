using System;
using System.Linq.Expressions;
using FORCEBuild.Data.ManualBinding.Abstraction;

namespace FORCEBuild.Data.ManualBinding
{
    public class Pro
    {
    }

    /// <summary>
    /// binding by delegate
    /// </summary>
    public static class DelegateMappingExtension
    {
        /*public static PropertyInstanceBinder<T, TK> Instance<T, TK>(this PropertyAccessor<T, TK> accessor, T t)
        {
            return new PropertyInstanceBinder<T, TK>(t, accessor);
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
        }*/

        /*public static IValueProvider<TK> ReadFrom<T, TK>(this IPropertyGet<T, TK> get, Func<T> provider)
        {
            return new ValueProvider<T, TK>(new DelegateInstanceFactory<T>(provider), get);
        }*/

        /*public static IValueProvider<TK> ReadFrom<T, TK>(this IPropertyGet<T, TK> get, IInstanceProvider<T> provider)
        {
            return new ValueProvider<T, TK>(provider, get);
        }*/

        public static IPropertyBridge<T,S> Bridge<T, S, TK>(this PropertyAccessor<T, TK> accessor,
            PropertyAccessor<S, TK> bindToAccessor)
        {
            return new PropertyBridge<T, S, TK>(accessor, bindToAccessor);
        }
        
        public static IPropertyBridge<T,S> Bridge<T, S, TK>(this PropertyAccessor<T, TK> accessor,
            TypeBinder<S> binder,Func<TypeBinder<S>,PropertyAccessor<S,TK>> bindToAccessorFunc)
        {
            var propertyAccessor = bindToAccessorFunc.Invoke(binder);
            return new PropertyBridge<T, S, TK>(accessor, propertyAccessor);
        }
        
        
        public static ObservablePropertyGetter<T, TK> ObservableGetter<T, TK>(this TypeBinder<T> typeBinder,
            Expression<Func<T, TK>> expression)
        {
            return typeBinder.Getter(expression).ToObservable();
        }

        public static ObservablePropertyGetter<T, TK> OnObservableProperty<T, TK>(this TypeBinder<T> typeBinder,
            Expression<Func<T, TK>> expression)
        {
            var observablePropertyGetter = typeBinder.ObservableGetter(expression);
            typeBinder.Attach(observablePropertyGetter);
            return observablePropertyGetter;
        }

        public static TypeBinder<T> Register<T, TK>(this TypeBinder<T> binder,
            Func<TypeBinder<T>, IInstanceConsumer<T>> func)
        {
            var instanceConsumer = func.Invoke(binder);
            binder.Attach(instanceConsumer);
            return binder;
        }

        public static ObservablePropertyGetter<T, TK> ToObservable<T, TK>(this IPropertyGet<T, TK> propertyGet)
        {
            return new ObservablePropertyGetter<T, TK>(propertyGet);
        }

        public static PropertySetterInstanceObserver<S, TK> SubscribeBy<S, TK>(
            this IObservable<TK> provider,
            Expression<Func<S, TK>> receiverExpression)
        {
            var propertyGetter = new PropertySetterInstanceObserver<S, TK>(receiverExpression, provider);
            return propertyGetter;
        }

        public static IObservable<TK> OnInstance<T, TK>(this PropertySetterInstanceObserver<T, TK> subscriber,
            Func<T> instanceFunc)
        {
            var delegateInstanceFactory = new DelegateInstanceFactory<T>(instanceFunc);
            return subscriber.OnInstance(delegateInstanceFactory);
        }

        public static IObservable<TK> OnInstance<T, TK>(this PropertySetterInstanceObserver<T, TK> subscriber,
            T instance)
        {
            var delegateInstanceFactory = new DefaultInstanceFactory<T>(instance);
            return subscriber.OnInstance(delegateInstanceFactory);
        }

        /*public static IValueReceiver<TK> OnInstance<T, TK>(this IPropertySet<T, TK> propertySet,
            IInstanceProvider<T> provider)
        {
            return new ValueDeceiver<T, TK>(provider, propertySet);
        }*/
    }
}