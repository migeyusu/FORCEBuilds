using System;

namespace FORCEBuild.Core.Old
{
    public class MethodInvokedEventArgs:EventArgs
    {
        public virtual string MethodName { get; set; }

        public MethodInvokedEventArgs(string methodName)
        {
            MethodName = methodName;
        }
    }
}
