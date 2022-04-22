using System;

namespace FORCEBuild.Net.MessageBus
{
    public class SubscribeAttribute:Attribute
    {
        public string RoutedFilter { get; set; }

        public SubscribeAttribute(string routedfilter)
        {
            this.RoutedFilter = routedfilter;
        }
    }
}