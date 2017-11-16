using System;

namespace FORCEBuild.Crosscutting.Validation
{
    [AttributeUsage(AttributeTargets.Property)]
    public class LengthAdmitAttribute : XValidaterAttribute
    {
        private readonly int _maxLen;

        public LengthAdmitAttribute(int maxlen)
        {
            _maxLen = maxlen;
        }
        public override void Validate(object val, string name)
        {
            if (val == null)
                throw new ArgumentOutOfRangeException($"属性{name}不能为空！");
            if (val.ToString().Length > _maxLen)
                throw new ArgumentOutOfRangeException($"值{name}的范围不能超过{_maxLen}");
        }
    }


}
