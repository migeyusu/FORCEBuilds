using System;
using System.Collections.Concurrent;
using System.Linq;
using System.Reflection;

namespace FORCEBuild.Helper
{
    /// <summary>
    /// 自定义指定属性更新检查（仅限值类型）
    /// </summary>
    public static class UpdateMapper
    {
        private static readonly ConcurrentDictionary<Type, UpdateRule> _rules
            =new ConcurrentDictionary<Type, UpdateRule>();
        
        public static void Register<T>()
        {
            var type = typeof(T);
            if (_rules.ContainsKey(type))
                return;
            _rules.TryAdd(type, new UpdateRule {
                PropertyInfos = type.GetProperties()
                    .Where(property => property.CanRead && property.CanWrite &&
                                       property.GetCustomAttribute<UpdateAttribute>() != null).ToList()
            });
        }

        public static void Register(params Type[] types)
        {
            foreach (var type in types)
            {
                if (_rules.ContainsKey(type))
                    continue;
                _rules.TryAdd(type, new UpdateRule {
                    PropertyInfos = type.GetProperties()
                        .Where(property => property.CanRead && property.CanWrite &&
                                           property.GetCustomAttribute<UpdateAttribute>() != null).ToList()
                });
            }
        }

        /// <summary>
        /// 比较两个对象并更新已被[UpdateAttribute]标记的同名可读写属性
        /// </summary>
        /// <param name="origin">原始对象，要被属性注入的目标</param>
        /// <param name="after">携带有注入属性的对象</param>
        public static void Update<T>(T origin,T after)
        {
            var type = typeof(T);
            if (!_rules.ContainsKey(type))
                Register<T>();
            foreach (var info in _rules[type].PropertyInfos)
            {
                var beforeval = info.GetValue(origin);
                var afterval = info.GetValue(after);
                if (beforeval!=afterval)
                {
                    info.SetValue(origin,afterval);
                }
            }
        }

        /// <summary>
        /// 以T为映射模板，任何值映射赋值都以被赋值对象为模板标准
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <typeparam name="TK"></typeparam>
        /// <param name="before">原始对象，要被属性注入的目标</param>
        /// <param name="after">携带有注入属性的对象</param>
        public static void Update<T,TK>(T before, TK after)
        {
            var beforeType = typeof(T);
            if (!_rules.ContainsKey(beforeType))
                Register(beforeType);
            var afterType = typeof(TK);
            if (!_rules.ContainsKey(afterType))
                Register(afterType);
            var afterRule = _rules[afterType];
            foreach (var beforePropertyInfo in _rules[beforeType].PropertyInfos)
            {
                var afterPropertyInfo =
                    afterRule.PropertyInfos.FirstOrDefault((info => info.Name == beforePropertyInfo.Name));//after.GetType().GetProperty(beforePropertyInfo.Name);
                if (afterPropertyInfo == null) continue;
                //这里不做错误处理，直接抛出
                var afterval = afterPropertyInfo.GetValue(after);
                if (beforePropertyInfo.GetValue(before) != afterval)
                    beforePropertyInfo.SetValue(before, afterval);
            }
        }

    }
}