using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Formatters.Soap;
using System.IO;
using System.Collections;
using System.Runtime.Serialization.Formatters.Binary;
using Xunit;

namespace FORCEBuild.Persistence.Serialization
{
    /*
     * 11.18：
     * 匹配远程对象、方法使用的序列化
     * 新增对泛型集合的支持，只要继承IEnumerable
     * ——因soap不支持泛型，将泛型转换IEnumerable
     * 新增装饰方法，允许对已实例化对象赋值属性
     * 新增构造函数参数接口，被序列化类在构造阶段调用
     * 序列化、实例化过程属性，可接入ioc
     * 取消所有特性选项，序列器自动解析。
     */

    public class XSoapSerializer:IXFormatter
    {
        readonly Dictionary<Type, XSoapSerializeRoot> RootCache;

        public ISurrogateSelector SurrogateSelector { get; set; }
        /* 
         * 通用的binder，同时用于序列化器和activator
         */
        private SerializationBinder binder;

        public SerializationBinder Binder
        {
            get => binder;
            set
            {
                Serializer.Binder = value;
                binder = value;
            }
        }

        public StreamingContext Context { get; set; }
        /// <summary>
        /// 序列化
        /// </summary>
        public Func<Type, object> InstanceCreateFunc { get; set; }

        private readonly SoapFormatter Serializer;

        public XSoapSerializer()
        {
            RootCache = new Dictionary<Type, XSoapSerializeRoot>();
            InstanceCreateFunc = Activator.CreateInstance;
            Serializer = new SoapFormatter();
        }

        /// <summary>
        /// 自定义序列化类数组需要解析
        /// </summary>
        /// <param name="type"></param>
        private void RegisterRoot(Type type)
        {
            var sr = new XSoapSerializeRoot {ClassType = type};
            RootCache.Add(type, sr); //预先插入防止循环引用
            var childProperties = new List<XSoapSerializeItem>();
            var pis = type.GetProperties();
            foreach (var pi in pis)
            {
                var xmea = pi.GetCustomAttribute<XSoapElementAttribute>();
                if (xmea == null) continue;
                var ele = new XSoapSerializeItem
                {
                    Property = pi,
                    Type = pi.PropertyType,
                };
                //集合类的判断标准，要求所有序列化类都继承该接口
                ele.IsIEnumerable = typeof(IEnumerable).IsAssignableFrom(ele.Type);
                if (ele.IsIEnumerable)
                {
                    if (ele.Type.IsArray)
                    {
                        var name = ele.Type.FullName;
                        var itemname = name.Substring(0, name.LastIndexOf("["));
                        //优先直接加载，如果没有则加载所在的命名空间
                        var itemtype = Type.GetType(itemname);
                        if (itemtype==null)
                        {
                            var asm = Assembly.Load(itemname.Substring(0, itemname.IndexOf(".")));
                            ele.ItemType = asm.GetType(itemname);
                        }
                        else
                        {
                            ele.ItemType = itemtype;
                        }
                        ele.IsContainCustom = ele.ItemType.GetCustomAttribute<XSoapRootAttribute>() != null;
                    }
                    else
                    {
                        if (ele.Type.IsGenericType)
                        {
                            ele.ItemType = ele.Type.GenericTypeArguments[0];//默认单泛型
                            ele.IsContainCustom = ele.ItemType.GetCustomAttribute<XSoapRootAttribute>() != null;
                        }
                        //else
                        //{
                        //    //对于queue这种拆装箱类型,判断是否包含自建类即可，但是必须到实际数据体判断
                            
                        //}
                    }
                }
                else
                {
                    ele.IsContainCustom = ele.Type.GetCustomAttribute<XSoapRootAttribute>() != null;
                }
                childProperties.Add(ele);
            }
            sr.Elements = childProperties.ToArray();
        }

        public void Register(Type type)
        {
            if (type.GetCustomAttribute<XSoapRootAttribute>() != null && !RootCache.Keys.Contains(type))
                RegisterRoot(type);
        }

        public MemoryStream Serialize(object obj)
        {
            var memoryStream = new MemoryStream();
            var type = obj.GetType();
            if (type.GetCustomAttribute<XSoapRootAttribute>() == null)
            {
                Serializer.Serialize(memoryStream, obj);
                memoryStream.Seek(0, SeekOrigin.Begin);
                return memoryStream;
            }
            if (!RootCache.Keys.Contains(type))
                RegisterRoot(type);
            var sr = RootCache[type].Clone();
            FillRoot(sr, obj);
            Serializer.Serialize(memoryStream, sr);
            return memoryStream;
        }

        public void Serialize(Stream serializationStream, object graph)
        {
            var type = graph.GetType();
            if (type.GetCustomAttribute<XSoapRootAttribute>() == null)
            {
                Serializer.Serialize(serializationStream, graph);
                serializationStream.Seek(0, SeekOrigin.Begin);
                return;
            }
            if (!RootCache.Keys.Contains(type))
                RegisterRoot(type);
            var sr = RootCache[type].Clone();
            FillRoot(sr, graph);
            Serializer.Serialize(serializationStream, sr);
        }

        internal object GetSerializeRoot(object obj)
        {
            var type = obj.GetType();
            if (type.GetCustomAttribute<XSoapRootAttribute>() == null)
            {
                return obj;
            }
            if (!RootCache.Keys.Contains(type))
                RegisterRoot(type);
            var sr = RootCache[type].Clone();
            FillRoot(sr, obj);
            return sr;
        }

        private void FillRoot(XSoapSerializeRoot root, object model)
        {
            foreach (var ele in root.Elements)
            {
                if (ele.IsIEnumerable)
                {
                    var testobj = ele.Property.GetValue(model);
                    if (testobj == null)
                        continue;
                    var ie = (IEnumerable)testobj;
                    if (ele.Type.IsArray)
                    {
                        if (ele.IsContainCustom)
                        {
                            if (!RootCache.Keys.Contains(ele.ItemType))
                                RegisterRoot(ele.ItemType);
                            var templet = RootCache[ele.ItemType];
                            var al = new ArrayList();
                            foreach (var item in ie)
                            {
                                var copy = templet.Clone();
                                FillRoot(copy, item);
                                al.Add(copy);
                            }
                            ele.Data = al;
                        }
                        else
                        {
                            ele.Data = testobj;
                        }
                    }
                    else
                    {
                        if (ele.Type.IsGenericType)
                        {
                            if (ele.IsContainCustom)
                            {
                                if (!RootCache.Keys.Contains(ele.ItemType))
                                    RegisterRoot(ele.ItemType);
                                var templet = RootCache[ele.ItemType];
                                var al = new ArrayList();
                                foreach (var item in ie)
                                {
                                    var copy = templet.Clone();
                                    FillRoot(copy, item);
                                    al.Add(copy);
                                }
                                ele.Data = al;
                            }
                            else
                            {
                                var al = new ArrayList();
                                foreach (var item in ie)
                                    al.Add(item);
                                ele.Data = al;
                            }
                        }
                        else
                        {
                            //类似queue，stack,没有固定的itemtype,所以对itemtype的确定要在fillroot阶段
                            ele.IsContainCustom = ie.Cast<object>().Any(item => item.GetType().GetCustomAttribute<XSoapRootAttribute>() != null);
                            if (ele.IsContainCustom)
                            {
                                var al = new ArrayList();
                                foreach (var item in ie)
                                {
                                    var type = item.GetType();
                                    if (!RootCache.Keys.Contains(type))
                                        RegisterRoot(type);
                                    var template = RootCache[type].Clone();
                                    FillRoot(template, item);
                                    al.Add(template);
                                }
                                ele.Data = al;
                            }
                            else
                            {
                                ele.Data = testobj;
                            }
                        }
                    }
                }
                else
                {
                    if (!ele.IsContainCustom)
                        ele.Data = ele.Property.GetValue(model);
                    else
                    {
                        if (!RootCache.Keys.Contains(ele.Type))
                            RegisterRoot(ele.Type);
                        var val = ele.Property.GetValue(model);
                        if (val != null)
                        {
                            var child = RootCache[ele.Type].Clone();
                            FillRoot(child, val);
                            ele.Data = child;
                        }
                    }
                }
            }
        }

        public T Deserialize<T>(Stream stream)
        {
            var type = typeof(T);
            if (type.GetCustomAttribute<XSoapRootAttribute>() == null)
            {
                return (T) Serializer.Deserialize(stream);
            }
            var sr = (XSoapSerializeRoot) Serializer.Deserialize(stream);
            return (T) DeserializeRoot(sr);
        }

        public object Deserialize(Stream stream)
        {
            var obj = Serializer.Deserialize(stream);
            if (obj.GetType() == typeof(XSoapSerializeRoot))
            {
                return DeserializeRoot(obj as XSoapSerializeRoot);
            }
            return obj;
        }

        /// <summary>
        /// decorate
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="x"></param>
        /// <param name="st"></param>
        /// <returns></returns>
        public void Deserialize<T>(T x, Stream st)
        {
            var type = typeof(T);
            if (type.GetCustomAttribute<XSoapRootAttribute>() == null)
            {
                var copy = Serializer.Deserialize(st);
                foreach (var pi in copy.GetType().GetProperties())
                {
                    pi.SetValue(x, pi.GetValue(copy));
                }
                return;
            }
            var sr = (XSoapSerializeRoot) Serializer.Deserialize(st);
            DeserializeRoot(sr, x);
        }

        /// <summary>
        /// decorate
        /// </summary>
        /// <param name="root"></param>
        /// <param name="instance"></param>
        /// <param name="type"></param>
        private void DeserializeRoot(XSoapSerializeRoot root, object instance)
        {
            foreach (var element in root.Elements)
            {
                var data = element.Data;
                if (data == null)
                    continue;
                if (element.IsIEnumerable)
                {
                    #region 数组集合
                    if (element.Type.IsArray)
                    {
                        if (element.IsContainCustom)
                        {
                            var datas = (ArrayList)data;
                            var objs = new ArrayList();
                            foreach (var x in datas)
                            {
                                var oc = DeserializeRoot((XSoapSerializeRoot)x);
                                objs.Add(oc);
                            }
                            var array = objs.ToArray(element.ItemType);
                            element.Property.SetValue(instance, array);
                        }
                        else
                        {
                            element.Property.SetValue(instance,data);
                        }
                    }
                    #endregion
                    else
                    {
                        #region 泛型集合
                        if (element.Type.IsGenericType)
                        {
                            if (element.IsContainCustom)
                            {
                                var datas = (ArrayList) data;
                                var objs = new ArrayList();
                                foreach (var x in datas)
                                {
                                    var oc = DeserializeRoot((XSoapSerializeRoot) x);
                                    objs.Add(oc);
                                }
                                //由于queue、stack等未实现泛型icollection，故使用泛型IEnumerable实例化注入,
                                var listType = typeof(List<>);
                                listType = listType.MakeGenericType(element.ItemType);
                                var list = Convert.ChangeType(Activator.CreateInstance(listType), listType);
                                foreach (var x in objs)
                                    listType.InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                                        null, list, new[] {x});
                                var val = listType.InvokeMember("ToArray", BindingFlags.Default | BindingFlags.InvokeMethod,
                                    null, list, null);
                                element.Property.SetValue(instance, Activator.CreateInstance(element.Type, val));
                            }
                             else
                            {
                                //泛型集合使用构造函数注入
                                var type = typeof(List<>);
                                type = type.MakeGenericType(element.ItemType);
                                var list = Convert.ChangeType(Activator.CreateInstance(type), type);
                                foreach (var x in (ArrayList)data)
                                {
                                    type.InvokeMember("Add", BindingFlags.Default | BindingFlags.InvokeMethod,
                                        null, list, new[] { x });
                                }
                                var val = type.InvokeMember("ToArray", BindingFlags.Default | BindingFlags.InvokeMethod,
                                    null, list, null);
                                //下面判断运行为TRUE，代表toarray继承了泛型IEnumerable
                                //     global::System.Windows.Forms.MessageBox.Show(typeof(IEnumerable<int>).IsAssignableFrom(val.GetType()).ToString());
                                element.Property.SetValue(instance, Activator.CreateInstance(element.Type, val));
                            }
                        }
                        #endregion

                        #region 非泛型集合,装箱拆箱
                        else
                        {   //queue、stack等
                            if (element.IsContainCustom)
                            {
                                var datas = (ArrayList)data;
                                var objs = new ArrayList();
                                var tagType = typeof(XSoapSerializeRoot);
                                foreach (var x in datas)
                                {
                                    if (x.GetType() != tagType)
                                    {
                                        objs.Add(x);
                                    }
                                    else
                                    {
                                        var oc = DeserializeRoot((XSoapSerializeRoot)x);
                                        objs.Add(oc);
                                    }
                                }
                                var array = objs.ToArray(typeof(object));//装箱
                                                                         //构造函数注入
                                element.Property.SetValue(instance, Activator.CreateInstance(element.Type, array));
                            }
                            else
                            {
                                element.Property.SetValue(instance,data);
                            }
                        }
                        #endregion
                    }
                }
                else
                {
                    #region 非集合
                    if (element.IsContainCustom)
                    {
                        var oc = DeserializeRoot((XSoapSerializeRoot) data);
                        element.Property.SetValue(instance, oc);
                    }
                    else
                    {
                        element.Property.SetValue(instance, data);
                    }
                    #endregion
                }
            }
        }

        private object DeserializeRoot(XSoapSerializeRoot sr)
        {
            var oritype = sr.ClassType;
            var type = binder.BindToType(oritype.Assembly.ToString(), oritype.FullName); 
            var instance = InstanceCreateFunc(type);
            DeserializeRoot(sr, instance);
            return instance;
        }
    }
}
