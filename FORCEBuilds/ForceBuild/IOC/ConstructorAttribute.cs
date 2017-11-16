using System;

namespace FORCEBuild.IOC
{

    public class ConstructorAttribute:Attribute
    {
        public string PropertyName { get; set; }

        public string MethodName { get; set; }

        public Type[] Impletes { get; set; }

        public ConstructType ConstructType { get; set; }

        public ConstructorAttribute(ConstructType type,string name)
        {
            ConstructType = type;
            switch (type)
            {
                case ConstructType.StaticMethod:
                    MethodName = name;
                    break;
                case ConstructType.StaticProperty:
                    PropertyName = name;
                    break;
                default:
                    break;
            }
            
        }
        /// <summary>
        /// 需要指明构造函数所调用的接口、抽象类的具体实现类，
        /// 然后调用器找到这些类的构造函数递归构造
        /// </summary>
        /// <param name="impleteclassname"></param>
        public ConstructorAttribute(params string[] impleteclassname)
        {
            ConstructType = ConstructType.Parameters;
            Impletes = new Type[impleteclassname.Length];
            for (int i = 0; i < impleteclassname.Length; i++)
            {
                Impletes[i] = Type.GetType(impleteclassname[i]);
            }
        }

        /// <summary>
        /// 如果所有参数都非接口或抽象类
        /// </summary>
        public ConstructorAttribute()
        {
            ConstructType = ConstructType.Parameters;
        }


    }
}
