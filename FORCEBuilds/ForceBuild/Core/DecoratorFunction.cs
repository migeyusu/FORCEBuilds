using System;

namespace FORCEBuild.Core
{
    /// <summary>
    /// 包含了预定义的功能，可以attach添加功能
    /// </summary>
    [Flags]
    public enum DecoratorFunction
    {
        /// <summary>
        /// 赋值检查
        /// </summary>
        Validate = 1,
        /// <summary>
        /// 对象映射信息元
        /// </summary>
        //   MapInfo = 2,
        /// <summary>
        /// 属性变化通知
        /// </summary>
        PropertyChangedNotify = 4,
        /// <summary>
        /// 远程调用代理
        /// </summary>
        //    RemoteProxy = 8,
        /// <summary>
        /// 前置拦截
        /// </summary>
        BeforeMethod = 16,
        /// <summary>
        /// 执行后记录
        /// </summary>
        AfterMethod=32,
        /// <summary>
        /// 错误拦截
        /// </summary>
        Exception=64,
    }
}