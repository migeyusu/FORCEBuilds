﻿using System.Collections.Generic;

namespace FORCEBuild.Persistence.DistributedStorage
{
    public interface IOrmCell
    {
        /// <summary>
        /// 標記同步属性
        /// </summary>
        Dictionary<string, NotifyProperty> NotifyProperites { get; set; }
    }
}
