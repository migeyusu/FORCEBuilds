﻿using System;

namespace FORCEBuild.Net.DistributedService
{
    /// <summary>
    /// 标记该接口可被远程调用
    /// </summary>
    [AttributeUsage(AttributeTargets.Interface)]
    public class RemoteInterfaceAttribute:Attribute
    {
        public string Implement { get; set; }

        public RemoteInterfaceAttribute(string implementname="")
        {
            Implement = implementname;
        }
    }
}
