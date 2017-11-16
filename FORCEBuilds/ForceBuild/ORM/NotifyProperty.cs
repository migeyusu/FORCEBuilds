using System.Collections.Generic;
using System.Collections.Specialized;

namespace FORCEBuild.ORM
{
    public class NotifyProperty
    {
        
        public bool IsChanged { get; set; }

        public BasePropertyElement PropertyElement { get; set; }

        public List<NotifyCollectionChangedEventArgs> OperatersList { get; set; }

        public NotifyProperty()
        {
            OperatersList = new List<NotifyCollectionChangedEventArgs>();
            IsChanged = false;
        }
    }
}
