using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reflection;


namespace TeachManagement.Helper
{
    /// <summary>
    /// 注入器，仅针对自建非泛型类，由于不同调用时构造同一个类的参数会不同
    /// 因此具体参数由属性注入，本类只方便继承关系的明确
    /// </summary>
    public class Injection
    {
        static Dictionary<Type, object> ObjectPool;
        static Injection _context;
        private Injection()
        {
            ObjectPool = new Dictionary<Type, object>();
        }

        public T Create<T>(bool singleton = true)
        {
            var type = typeof(T);
            if (singleton)
            {
                if (ObjectPool.ContainsKey(type))
                    return (T)ObjectPool[type];
                var obj = CreateInstance(typeof(T));
                ObjectPool.Add(type, obj);
                return (T)obj;
            }
            else
            {
                return (T)CreateInstance(typeof(T));
            }
        }

        public static Injection Current
        {
            get
            {
                if (_context == null)
                    _context = new Injection();
                return _context;
            }
        }

        private object CreateInstance(Type type)
        {
            var output = type.GetCustomAttribute<ExportAttribute>();
            var inject = type.GetCustomAttribute<ConstructAttribute>();
            if (output == null)//调用构造函数
            {
                if (inject == null)//无参构造
                {
                    return Activator.CreateInstance(type);
                }
                else
                {
                    return Activator.CreateInstance(type, ParamArray(inject));
                }
            }
            else//存在导出函数
            {
                if (inject == null)
                {
                    return type.GetMethod(output.Fuc).Invoke(null, null);
                }
                else
                {
                    return type.GetMethod(output.Fuc).Invoke(null, ParamArray(inject));
                }
            }
        }

        object[] ParamArray(ConstructAttribute inject)
        {
            var injects = new object[inject.Params.Count];
            for (var i = 0; i < inject.Params.Count; ++i)
            {
                injects[i] = CreateInstance(inject.Params[i]);
            }
            return injects;
        }
    }
    [AttributeUsage(AttributeTargets.Class)]
    public class ConstructAttribute : Attribute
    {
        public List<Type> Params { get; set; }
        public ConstructAttribute(params Type[] types)
        {
            Params = types.ToList();
        }
    }
    /// <summary>
    /// 导出函数,不存在默认调用构造函数
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class ExportAttribute : Attribute
    {
        /// <summary>
        /// 函数形式导出
        /// </summary>
        public string Fuc { get; set; }
        public ExportAttribute(string fuc)
        {
            Fuc = fuc;
        }
    }
}
