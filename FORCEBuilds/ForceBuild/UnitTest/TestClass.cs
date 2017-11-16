using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization.Formatters.Binary;
using System.Windows.Documents;
using Castle.MicroKernel.Registration;
using FORCEBuild.Helper;
using Xunit;
using Xunit.Sdk;

namespace FORCEBuild.UnitTest
{
    public class TestClass
    {
        [Fact]
        public void FactMethodName()
        {
            //var list1=new List<Model>(){new Model()};
            //var list2=new List<Model>(){new Model()};
            //var list = list1.Except(list2);
            //Assert.Empty(list);
            //var type = typeof(Test).GetMethod("Add").ReturnType;
            //var gtype = typeof(IEnumerable<>).MakeGenericType(typeof(int));
            //var types = type== typeof(IEnumerable<>);
            //var dto = Add(1,2);
            //var baseCastMethod = typeof(Enumerable).GetMethod("ToArray");
            //var dtotype = typeof(IEnumerable<int>);
            //if (dtotype.FullName.StartsWith("System.Collections.Generic.IEnumerable`1"))
            //{
            //    var genericType = dto.GetType().GenericTypeArguments[0];
            //    var genericMethod = baseCastMethod.MakeGenericMethod(genericType);
            //    dto = (IEnumerable<int>)genericMethod.Invoke(null, new[] { dto });
            //}
            //Assert.Equal(dto,null);
            //   var types = typeof(IEnumerable<int>).FullName.StartsWith("System.Collections.Generic.IEnumerable`1");
            var s = "111.xd";
            Assert.Equal(s.Substring(0, s.LastIndexOf(".xd", StringComparison.Ordinal)), "111");
        }
    }


    public class Model
    {
        public Guid Guid { get; set; }

        public Model()
        {
            Guid = Guid.Empty;
        }

        public override bool Equals(object obj)
        {
            if (obj==null) {
                return false;
            }
            if (obj is Model) {
                return ((Model) obj).Guid == this.Guid;
            }
            return false;
        }

        protected bool Equals(Model other)
        {
            return Guid.Equals(other.Guid);
        }

        public override int GetHashCode()
        {
            return Guid.GetHashCode();
        }
    }
}