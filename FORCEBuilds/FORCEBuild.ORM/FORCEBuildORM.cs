using System;
using FORCEBuild.Core;
using FORCEBuild.Persistence.Configuration;

namespace FORCEBuild.Persistence
{
    /// <summary>
    /// 起始类
    /// </summary>
    public static class FORCEBuildORM
    {
        //todo:XML config
        //public static Field GeField(string config)
        //{
        //    var document = new XmlDocument();
        //    document.Load(config);
        //    var load = document.DocumentElement.FirstChild as XmlElement;
        //    var accessorName = load.GetElementsByTagName("DBType")[0].InnerText;
        //    #region 加载类
        //    var dryClasses = new ConcurrentDictionary<string, ClassDefine>();
        //    var relations = document.DocumentElement.LastChild as XmlElement;
        //    var loadAssembly = Assembly.Load(relations.GetAttribute("assembly"));
        //    var xnamespace = relations.GetAttribute("namespace");
        //    var classelements = relations.ChildNodes;

        //    #region 遍历基本属性

        //    foreach (XmlElement ele in classelements) //基本属性
        //    {
        //        var classdefine = new ClassDefine
        //        {
        //            Name = ele.GetAttribute("name"),
        //            Table = ele.GetAttribute("table")
        //        };
        //        classdefine.ClassType = loadAssembly.GetType(xnamespace + "." + classdefine.Name);
        //        var properties = ele.GetElementsByTagName("property");
        //        foreach (XmlElement property in properties)
        //        {
        //            var pe = new BasePropertyElement { Column = property.GetAttribute("column") };
        //            var propertyname = property.GetAttribute("name");
        //            classdefine.AllProperties.Add(propertyname, pe);
        //        }
        //        dryClasses.TryAdd(classdefine.Name, classdefine);
        //    }

        //    #endregion

        //    #region 建立联系

        //    foreach (XmlElement ele in classelements)
        //    {
        //        string propertyName;
        //        var define = dryClasses[ele.GetAttribute("name")];
        //        var otos = ele.GetElementsByTagName("one-to-one");
        //        foreach (XmlElement element in otos)
        //        {
        //            var oo = new OnetoOne
        //            {
        //                Column = element.GetAttribute("refercolumn"),
        //                IsNeedUpdate = !bool.Parse(element.GetAttribute("twoway"))
        //            };
        //            var property = element.GetAttribute("class");
        //            oo.ReferClass = dryClasses[property];
        //            propertyName = element.GetAttribute("name");
        //            define.AllProperties.Add(propertyName, oo);
        //        }
        //        var mtms = ele.GetElementsByTagName("many-to-many");
        //        foreach (XmlElement element in mtms)
        //        {
        //            var mm = new ManytoMany
        //            {
        //                Column = element.GetAttribute("key"),
        //                ReferColumn = element.GetAttribute("column"),
        //                Table = element.GetAttribute("table"),
        //                ReferClass = dryClasses[element.GetAttribute("class")],
        //                IsNeedUpdate = !bool.Parse(element.GetAttribute("twoway")) 
        //            };
        //            propertyName = element.GetAttribute("name");
        //            define.AllProperties.Add(propertyName, mm);
        //        }
        //        var otms = ele.GetElementsByTagName("one-to-many");
        //        foreach (XmlElement element in otms)
        //        {
        //            var om = new OnetoMany
        //            {
        //                ReferColumn = element.GetAttribute("column"),
        //                ReferClass = dryClasses[element.GetAttribute("class")],
        //                IsNeedUpdate = !bool.Parse(element.GetAttribute("twoway"))
        //            };
        //            propertyName = element.GetAttribute("name");
        //            define.AllProperties.Add(propertyName, om);
        //        }
        //        var mtos = ele.GetElementsByTagName("many-to-one");
        //        foreach (XmlElement element in mtos)
        //        {
        //            var mo = new ManytoOne
        //            {
        //                Column = element.GetAttribute("column"),
        //                ReferClass = dryClasses[element.GetAttribute("class")],
        //            };
        //            propertyName = element.GetAttribute("name");
        //            define.AllProperties.Add(propertyName, mo);
        //        }
        //        var fos = ele.GetElementsByTagName("foreign-one");
        //        foreach (XmlElement element in fos)
        //        {
        //            var fto = new ForeignOne
        //            {
        //                Column = element.GetAttribute("column"),
        //                ReferClass = dryClasses[element.GetAttribute("class")],
        //            };
        //            propertyName = element.GetAttribute("name");
        //            define.AllProperties.Add(propertyName, fto);
        //        }
        //    }

        //    #endregion

        //    foreach (var dryClassesValue in dryClasses.Values)
        //    {
        //        foreach (var basePropertyElement in dryClassesValue.AllProperties)
        //        {
        //            basePropertyElement.Value.PropertyInfo =
        //                dryClassesValue.ClassType.GetProperty(basePropertyElement.Key);
        //        }
        //    }
        //    #endregion
        //    //创建访问类
        //    var accessor = CreateAccessor(accessorName);
        //    accessor.ClassDefines = dryClasses.ToConcurrencyDictionary(kv => kv.Value.ClassType, kv => kv.Value);
        //    accessor.ConnectionString =
        //        $"Data Source={load.GetElementsByTagName("Instance")[0].InnerText};AttachDbFilename=" +
        //        $"{AppDomain.CurrentDomain.BaseDirectory + load.GetElementsByTagName("Path")[0].InnerText};" +
        //        $"Integrated Security=True;Connect Timeout=30;User Instance=True";
        //    //accessor.EmptyId = bool.Parse(load.GetElementsByTagName("EmptyID")[0].InnerText);
        //    //accessor.DefaultId = int.Parse(load.GetElementsByTagName("DefaultID")[0].InnerText);

        //    var dispatcher = new AccessorDispatcher { Accessor = accessor };
        //    accessor.Dispatcher = dispatcher;

        //    var field = new Field
        //    {
        //        Accessor = accessor,
        //        Dispatcher = dispatcher
        //    };

        //    return field;
        //}

        /// <summary>
        /// 通过配置文件获取访问域
        /// </summary>
        /// <param name="register"></param>
        public static Field OpenField(OrmConfig register)
        {
            if (register.ConnectionStringBuilder == null) {
                throw new NullReferenceException("Connectionstring is null!");
            }

            if (register.IsLinked && string.IsNullOrEmpty(register.LinkedIdName)) {
                throw new ArgumentNullException("LinkedIdName is null!");
            }

            if (register.AccessorType == default(AccessorType)) {
                throw new ArgumentNullException("AccessorType is null!");
            }

            if (register.Logger == null) {
                throw new NullReferenceException("Log is null!");
            }

            
            
             register.Function.HasFlag(DecoratorFunction.PropertyChangedNotify)
             {
                 
             }   


            var accessor = CreateAccessor(register.AccessorType);
            accessor.Config = register;
            if (accessor.IsLinked) {
                foreach (var define in register.ClassDefines.Values) {
                    define.IdPropertyInfo = define.ClassType.GetProperty(register.LinkedIdName);
                }
            }

            var field = new Field {
                Accessor = accessor,
            };

            return field;
        }

        private static Accessor CreateAccessor(AccessorType accessorType)
        {
            var space = typeof(Accessor).Namespace;
            return (Accessor) Activator.CreateInstance(
                Type.GetType(typeName: $"{space}.{accessorType.ToString()}Accessor"));
        }
    }
}