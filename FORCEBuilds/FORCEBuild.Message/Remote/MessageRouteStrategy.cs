﻿using System;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using Xunit;

namespace FORCEBuild.Message.Remote
{

    /// <summary>
    /// 邮箱路由规则
    /// </summary>
    [Serializable]
    public abstract class MessageRouteStrategy
    {
        /// <summary>
        /// 检验名称是否匹配话题
        /// </summary>
        /// <param name="topic">话题</param>
        /// <returns></returns>
        public abstract bool IsMatch(string topic);

        public static MessageRouteStrategy Create(MailRoutedType routedType,
            string filter = null, Func<string, bool> func = null)
        {
            switch (routedType) {
                case MailRoutedType.Direct:
                    return new ContentStrategy() {
                        Filter = filter,
                        IsRegex = false
                    };
                case MailRoutedType.Filter:
                    return new ContentStrategy() {
                        Filter = filter,
                        IsRegex = true
                    };
                case MailRoutedType.Func:
                    return new FunctionalStrategy() {
                        Func = func
                    };
                default:
                    throw new ArgumentOutOfRangeException(nameof(routedType), routedType, null);
            }
        }
    }


    [Serializable]
    public class ContentStrategy:MessageRouteStrategy
    {
        /// <summary>
        /// 路由字符串，用于MailRoutedType.Direct和Filter类型
        /// </summary>
        public string Filter {
            get { return _filter; }
            set {
                if (value.IsNullOrEmpty()) {
                    throw new Exception("禁止使用空字符串作为Filter");
                }
                _filter = value;
            }
        }

        public bool IsRegex { get; set; }
        
        private string _filter;

        public override bool IsMatch(string topic)
        {
            return IsRegex ? new Regex(Filter).IsMatch(topic) : topic == Filter;
        }

    }


    [Serializable]
    public class FunctionalStrategy:MessageRouteStrategy
    {
        /// <summary>
        /// 路由方法
        /// </summary>
        public Func<string, bool> Func { get; set; }
        
        public override bool IsMatch(string topic)
        {
            if (Func != null) {
                return Func(topic);
            }
            return false;
        }
    }
}