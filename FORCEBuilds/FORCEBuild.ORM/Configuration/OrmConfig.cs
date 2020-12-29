using System;
using System.Collections.Concurrent;
using System.Data.Common;
using System.Reflection;
using FORCEBuild.Core;
using FORCEBuild.Crosscutting.Log;

namespace FORCEBuild.Persistence.Configuration
{
    /// <summary>
    /// 注册器
    /// </summary>
    public abstract class OrmConfig
    {
        internal ConcurrentDictionary<Type, ClassDefine> ClassDefines { get; set; }

        public DbConnectionStringBuilder ConnectionStringBuilder { get; set; }

        public DecoratorFunction Function { get; set; }

        /// <summary>
        /// 是否绑定 super layer class的id
        /// </summary>
        public bool IsLinked { get; set; }

        /// <summary>
        /// super-layer class propertyname
        /// </summary>
        public string LinkedIdName { get; set; }

        public ILog Logger { get; set; }

        public AccessorType AccessorType { get; set; }

        protected OrmConfig()
        {
            ClassDefines = new ConcurrentDictionary<Type, ClassDefine>();
            InstallAll();
        }

        private void Register(Type type)
        {
            var ormAttribute = type.GetCustomAttribute<OrmAttribute>();
            if (ormAttribute == null)
            {
                throw new ArgumentException("未标记可被映射");
            }
            if (ClassDefines.ContainsKey(type))
                return;
            var mainClassDefine = new ClassDefine {
                Name = type.Name,
                Table = ormAttribute.Table,
                ClassType = type
            };
            //预先插入防止双向引用
            ClassDefines.TryAdd(type, mainClassDefine);
            var propertyInfos = type.GetProperties();
            var enumType = typeof(Enum);
            var nullableType = typeof(Nullable<>);
            foreach (var propertyInfo in propertyInfos)
            {
                var propertyAttribute = propertyInfo.GetCustomAttribute<PropertyAttribute>();
                if (propertyAttribute != null)
                {
                    var basePropertyElement = new BasePropertyElement {
                        Column = propertyAttribute.Column,
                        PropertyInfo = propertyInfo,
                    };
                    if (enumType.IsAssignableFrom(propertyInfo.PropertyType)) {
                        basePropertyElement.IsEnum = true;
                    }
                    else if (propertyInfo.PropertyType
                        .IsGenericType&&propertyInfo.PropertyType.GetGenericTypeDefinition()==nullableType) {
                        basePropertyElement.IsNullable = true;
                        basePropertyElement.NullableBaseType = propertyInfo.PropertyType.GenericTypeArguments[0];
                    }
                    mainClassDefine.AllProperties.Add(propertyInfo.Name, basePropertyElement);
                    continue;
                }
                var oneToOneAttribute = propertyInfo.GetCustomAttribute<OneToOneAttribute>();
                if (oneToOneAttribute != null)
                {
                    var onetoOne = new OnetoOne {
                        Column = oneToOneAttribute.ForeignKey,
                        IsNeedUpdate = oneToOneAttribute.Update,
                        PropertyInfo = propertyInfo
                    };
                    if (!ClassDefines.ContainsKey(propertyInfo.PropertyType))
                    {
                        Register(propertyInfo.PropertyType);
                    }
                    var classDefine = ClassDefines[propertyInfo.PropertyType];
                    onetoOne.ReferClass = classDefine;
                    mainClassDefine.AllProperties.Add(propertyInfo.Name, onetoOne);
                    continue;
                }
                var manyToManyAttribute = propertyInfo.GetCustomAttribute<ManyToManyAttribute>();
                if (manyToManyAttribute != null)
                {
                    var manytoMany = new ManytoMany {
                        Column = manyToManyAttribute.PrimaryKay,
                        ReferColumn = manyToManyAttribute.ForeignKey,
                        Table = manyToManyAttribute.Table,
                        IsNeedUpdate = manyToManyAttribute.Update,
                        PropertyInfo = propertyInfo
                    };
                    if (!ClassDefines.ContainsKey(propertyInfo.PropertyType.GenericTypeArguments[0]))
                    {
                        Register(propertyInfo.PropertyType.GenericTypeArguments[0]);
                    }
                    var define = ClassDefines[propertyInfo.PropertyType.GenericTypeArguments[0]];
                    manytoMany.ReferClass = define;
                    mainClassDefine.AllProperties.Add(propertyInfo.Name, manytoMany);
                    continue;
                }
                var manyToOneAttribute = propertyInfo.GetCustomAttribute<ManyToOneAttribute>();
                if (manyToOneAttribute != null)
                {
                    var manytoOne = new ManytoOne {
                        Column = manyToOneAttribute.ForeignKey,
                        PropertyInfo = propertyInfo
                    };
                    if (!ClassDefines.ContainsKey(propertyInfo.PropertyType))
                    {
                        Register(propertyInfo.PropertyType);
                    }
                    var define = ClassDefines[propertyInfo.PropertyType];
                    manytoOne.ReferClass = define;
                    mainClassDefine.AllProperties.Add(propertyInfo.Name, manytoOne);
                    continue;
                }
                var oneToManyAttribute = propertyInfo.GetCustomAttribute<OneToManyAttribute>();
                if (oneToManyAttribute != null)
                {
                    var onetoMany = new OnetoMany {
                        ReferColumn = oneToManyAttribute.ForeignKey,
                        IsNeedUpdate = oneToManyAttribute.Update,
                        PropertyInfo = propertyInfo
                    };
                    if (!ClassDefines.ContainsKey(propertyInfo.PropertyType.GenericTypeArguments[0]))
                    {
                        Register(propertyInfo.PropertyType.GenericTypeArguments[0]);
                    }
                    var define = ClassDefines[propertyInfo.PropertyType.GenericTypeArguments[0]];
                    onetoMany.ReferClass = define;
                    mainClassDefine.AllProperties.Add(propertyInfo.Name, onetoMany);
                    continue;
                }
                var foreignOneAttribute = propertyInfo.GetCustomAttribute<ForeignOneAttribute>();
                if (foreignOneAttribute != null)
                {
                    var foreignOne = new ForeignOne {
                        Column = foreignOneAttribute.ForeignKey,
                        PropertyInfo = propertyInfo

                    };
                    if (!ClassDefines.ContainsKey(propertyInfo.PropertyType))
                    {
                        Register(propertyInfo.PropertyType);
                    }
                    var define = ClassDefines[propertyInfo.PropertyType];
                    foreignOne.ReferClass = define;
                    mainClassDefine.AllProperties.Add(propertyInfo.Name, foreignOne);
                }

            }
        }

        protected void Register(params Type[] types)
        {
            foreach (var type in types)
            {
                Register(type);
            }
        }

        protected abstract void InstallAll();

    }
}