using System;

namespace FORCEBuild.Core.Old
{
    public class MethodInvokingEventArgs:EventArgs
    {
        public  virtual  string MethodName { get; set; }

        public MethodInvokingEventArgs(string methodName)
        {
            MethodName = methodName;
        }
    }
}
