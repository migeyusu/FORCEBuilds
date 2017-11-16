using System;
using System.Text.RegularExpressions;

namespace FORCEBuild.Crosscutting.Validation
{
    public class FileNameCheckAttribute:XValidaterAttribute
    {
        public override void Validate(object val, string name)
        {
            var badChars = new string(System.IO.Path.GetInvalidPathChars());
            var badCharsRegex = new Regex("[" + Regex.Escape(badChars) + "]");
            if (badCharsRegex.IsMatch(val.ToString()))
            {
                throw new ArgumentException($"文件路径不能包含{badChars}等字符");
            }
        }
    }
}