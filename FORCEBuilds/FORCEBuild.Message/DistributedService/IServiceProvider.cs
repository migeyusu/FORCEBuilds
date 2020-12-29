using System;

namespace FORCEBuild.Net.DistributedService
{
    public interface IServiceProvider
    {
        Guid Filter { get; set; }

        bool Providing { get; set; }

        void Start();

        void End();
    }
}