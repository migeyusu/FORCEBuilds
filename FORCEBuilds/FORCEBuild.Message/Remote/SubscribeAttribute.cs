using System;

namespace FORCEBuild.Message.Remote
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