using System.Collections.Generic;
using System.Collections.Specialized;
using System.Reflection;
using Castle.Components.DictionaryAdapter;

namespace FORCEBuild.Persistence.DistributedStorage.SoftwareTransaction
{
    public class HistoryProperty
    {
        public bool IsChanged { get; set; }

        public dynamic PreValue { get; set; }

        public bool IsInitialize { get; set; }

        public RelationType RelationType { get; set; }

        public PropertyInfo PropertyInfo { get; set; }

        public List<NotifyCollectionChangedEventArgs> EventArgses { get; set; }

        public HistoryProperty()
        {
            EventArgses = new EditableList<NotifyCollectionChangedEventArgs>();
            IsChanged = false;
        }
    }
}