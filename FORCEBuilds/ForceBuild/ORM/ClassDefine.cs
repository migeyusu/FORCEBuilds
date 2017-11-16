using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FORCEBuild.ORM
{
    [Serializable]
    public class ClassDefine
    {
        public string Name { get; set; }
        public string Table { get; set; }
        public Type ClassType { get; set; }
       // public IdElement IdElement { get; set; }
        public PropertyInfo IdPropertyInfo { get; set; }

        private Dictionary<string, BasePropertyElement> property;
        private Dictionary<string, ForeignOne> foreignOnes;
        private Dictionary<string, ManytoOne> manytoOnes;
        private Dictionary<string, OnetoOne> onetoOnes;
        private Dictionary<string, OnetoMany> ontoManys;
        private Dictionary<string, ManytoMany> manytoManys;

        public Dictionary<string, BasePropertyElement> Property
        {
            get
            {
                return property ?? (property = AllProperties.Where(allProperty => allProperty.Value.RelationType ==
                                                                                  RelationType.Value)
                           .ToDictionary(allProperty => allProperty.Key, allProperty => allProperty.Value));
            }
        }

        public Dictionary<string, ForeignOne> ForeignOne
        {
            get
            {
                return foreignOnes?? AllProperties.Where(allPropery => allPropery.Value.RelationType == RelationType.ForeignOne)
                    .ToDictionary(allProperty => allProperty.Key, allProperty => (ForeignOne) allProperty.Value);
            }
        }

        public Dictionary<string, ManytoOne> ManyToOne
        {
            get
            {
                return manytoOnes?? AllProperties.Where(allPropery => allPropery.Value.RelationType == RelationType.ManyToOne)
                    .ToDictionary(allProperty => allProperty.Key, allProperty => (ManytoOne) allProperty.Value);
            }
        }

        public Dictionary<string, OnetoOne> OneToOne
        {
            get
            {
                return onetoOnes?? AllProperties.Where(allPropery => allPropery.Value.RelationType == RelationType.OneToOne)
                    .ToDictionary(allProperty => allProperty.Key, allProperty => (OnetoOne) allProperty.Value);
            }
        }

        public Dictionary<string, OnetoMany> OneToMany
        {
            get
            {
                return ontoManys?? AllProperties.Where(allPropery => allPropery.Value.RelationType == RelationType.OneToMany)
                    .ToDictionary(allProperty => allProperty.Key, allProperty => (OnetoMany) allProperty.Value);
            }
        }

        public Dictionary<string, ManytoMany> ManyToMany
        {
            get
            {
                return manytoManys?? AllProperties.Where(allPropery => allPropery.Value.RelationType == RelationType.ManyToMany)
                    .ToDictionary(allProperty => allProperty.Key, allProperty => (ManytoMany) allProperty.Value);
            }
        }

        public Dictionary<string, BasePropertyElement> AllProperties { get; set; }

        public ClassDefine()
        {
            AllProperties = new Dictionary<string, BasePropertyElement>();
        }
    }

    public class BasePropertyElement
    {
        public bool IsEnum { get; set; }
        public bool IsNullable { get; set; }
        public Type NullableBaseType { get; set; }
        public string Column { get; set; }
        public string Type { get; set; }
        public RelationType RelationType { get; set; }
        public PropertyInfo PropertyInfo { get; set; }
    }

    //只适用于guid，第一次赋值后就不会变化，同时对UI不可见。默认int自增长
    //2017.3.8 默認值為IORMID,并取消該類定義
    //2017.6.19 允许使用自带id
    //public class IdElement : BasePropertyElement
    //{
    //    public string ProperyName { get; set; } //=> "ORMID";
    //}

    //column代表被引用类关联表的列名
    public class OnetoOne : BasePropertyElement 
    {
        /// <summary>
        /// 一对一外键列
        /// </summary>
        public new string Column { get; set; }

        public bool IsNeedUpdate { get; set; }
        /// <summary>
        /// 引用的类型定义
        /// </summary>
        public ClassDefine ReferClass { get; set; }

        public OnetoOne()
        {
            RelationType = RelationType.OneToOne;
        }
    }
    //多对多引用
    public class ManytoMany : BasePropertyElement 
    {
        public bool IsNeedUpdate { get; set; }
        public string Table { get; set; }
        public ClassDefine ReferClass { get; set; }
        /// <summary>
        /// 外键列
        /// </summary>
        public string ReferColumn { get; set; }

        public ManytoMany()
        {
            RelationType = RelationType.ManyToMany;
        }
    }
    //被多个对象引用，column无意义
    public class OnetoMany : BasePropertyElement 
    {
        public bool IsNeedUpdate { get; set; }

        public ClassDefine ReferClass { get; set; }
        /// <summary>
        /// 主键写入引用类对应表的外键列
        /// </summary>
        public string ReferColumn { get; set; }

        public OnetoMany()
        {
            RelationType = RelationType.OneToMany;
        }
    }
    //引用自其它对象
    public class ManytoOne : BasePropertyElement 
    {
        public ClassDefine ReferClass;

        public ManytoOne()
        {
            RelationType = RelationType.ManyToOne;
        }
    }

    public class ForeignOne : BasePropertyElement
    {
        public ClassDefine ReferClass;

        public ForeignOne()
        {
            RelationType = RelationType.ForeignOne;
        }
    }
}
