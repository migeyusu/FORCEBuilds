using System;

namespace FORCEBuild.Crosscutting.Log
{
    /*日志接口的功能非常简单，复杂场景下应使用log4net，将来此工程也可能实现接近log4net的功能*/

    /// <summary>
    /// 提供单一异常和字符串输出接口
    /// </summary>
    public interface ILog
    {
        void Write(string sentence);
        void Write(Exception ex);
    }
}
