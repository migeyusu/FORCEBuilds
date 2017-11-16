using System;

namespace FORCEBuild.Windows.CIM
{
    public class CIM_ManagedSystemElement:IElement
    {
        public string Caption { get; set; }

        public string Description { get; set; }
        public DateTime InstallDate { get; set; }
        public string Name { get; set; }
        public string Status { get; set; }

    }
}