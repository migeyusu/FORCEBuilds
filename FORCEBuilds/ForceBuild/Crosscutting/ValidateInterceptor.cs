using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Reflection;
using Castle.DynamicProxy;
using FORCEBuild.Crosscutting.Validation;

namespace FORCEBuild.Crosscutting {
    public class ValidateInterceptor : IInterceptor {

        /// <summary>
        /// 缓存了当前附加到的对象的属性集合
        /// </summary>
        private readonly ConcurrentDictionary<string, IEnumerable<XValidaterAttribute>> _validateDictionary;

        public ValidateInterceptor() {
            _validateDictionary = new ConcurrentDictionary<string, IEnumerable<XValidaterAttribute>>();
        }

        public void Intercept(IInvocation invocation) {
            if (invocation.Method.Name.StartsWith("set_")) {
                var methodName = invocation.Method.Name;
                if (_validateDictionary.TryGetValue(methodName,out IEnumerable<XValidaterAttribute> validaterAttributes)) {
                    foreach (var validate in validaterAttributes)
                        validate.Validate(invocation.Arguments[0], invocation.Method.Name);
                }
                else
                {
                    var attributes = invocation.TargetType.GetProperty(methodName.Substring(4))
                        .GetCustomAttributes<XValidaterAttribute>();
                    _validateDictionary.TryAdd(methodName,attributes);
                    foreach (var attribute in attributes) {
                        attribute.Validate(invocation.Arguments[0], invocation.Method.Name);
                    }
                }
            }
            invocation.Proceed();
        }
    }

}