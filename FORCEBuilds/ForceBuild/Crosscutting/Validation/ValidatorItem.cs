using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace FORCEBuild.AOP.Validator
{
    public class ValidatorItem
    {
        public IValidate Attribute { get; set; }
        public PropertyInfo Property { get; set; }
    }

    public class ValidatorItemCollection
    {
        public Dictionary<string, ValidatorItem> collectiion;
        public ValidatorItemCollection()
        {
            collectiion = new Dictionary<string, ValidatorItem>();
        }
        public ValidatorItem this[string name] => collectiion.Keys.Contains(name) ? collectiion[name] : null;
    }

}
