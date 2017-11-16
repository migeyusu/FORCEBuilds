using System;
using FORCEBuild.Helper;

namespace FORCEBuild.Crosscutting.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class NotNullAttribute :XValidaterAttribute
    {
        public override void Validate(object val,string name)
        {
            if (val ==val.GetType().Default())
                throw new ArgumentException(name+"值不能为空！");
        }
    }
}
