using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace FORCEBuild.UI.WPF
{
    internal class ProduceItem
    {
        public Action Action { get; set; }

        public Func<bool> Func { get; set; }

    }

    public static class IntervalProducing
    {
        private static readonly List<ProduceItem> _produceItems = new List<ProduceItem>(10);

        private static readonly object Locker = new object();

        private static bool _isstart;
        /// <summary>
        /// 以固定的频率刷新，满足条件后停止
        /// </summary>
        /// <param name="action"></param>
        /// <param name="validateFunc"></param>
        /// <param name="interval">间隔，周期</param>
        public static void Start(Action action, Func<bool> validateFunc,int interval=1000)
        {
            var item = new ProduceItem {
                Action = action,
                Func = validateFunc,
            };
            lock (Locker) {
                 _produceItems.Add(item);
            }
        }
    }
}