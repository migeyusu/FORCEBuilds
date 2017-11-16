using System.Collections.Generic;
using System.Linq;
using System.Management;
using FORCEBuild.Windows.CIM;
using Xunit;

namespace FORCEBuild.Windows.WMI
{
    public class ManagementClassCollector
    {
        public static T GetWin32<T>() where T:new ()
        {
            var type = typeof(T).GetType();
            var t = new T();
            var managementClass = new ManagementClass(type.Name);
            var collection = managementClass.GetInstances();
            //取得并判断所有实例（如果有多个硬盘，则会有多个实例）
            if (collection.Count == 0)
                return t;
            var list =  type.GetProperties();
            foreach (var mo in collection) {
                foreach (var info in list) {
                    info.SetValue(t, mo.Properties[info.Name].Value); 
                }
                return t;
            }
            return default(T);
        }

        public static IEnumerable<T> GetWin32Infos<T>() where T : new()
        {
            var type = typeof(T);
            var managementClass = new ManagementClass(type.Name);
            var collection = managementClass.GetInstances();
            if (collection.Count==0) {
                return new T[0];
            }
            var tList = new List<T>();
            var propertyInfos = type.GetProperties();
            foreach (var item in collection) {
                var t=new T();
                foreach (var propertyInfo in propertyInfos) {
                    propertyInfo.SetValue(t,item.Properties[propertyInfo.Name].Value);
                }
                tList.Add(t);
            }
            return tList;
        }
        
        public dynamic GetElementProperty<T>(string propertyName)
        {
            var type = typeof(T);
            var managementClass = new ManagementClass(type.Name);
            var collection = managementClass.GetInstances();
            return collection.Count == 0 ? null : (from ManagementBaseObject mo in collection select mo.Properties[propertyName].Value).FirstOrDefault();
        }


        [Fact]
        public void FactMethodName()
        {
            var list = GetWin32Infos<Win32_Processor>();
            Assert.Equal(list.Count(), 0);
        }
    }
}