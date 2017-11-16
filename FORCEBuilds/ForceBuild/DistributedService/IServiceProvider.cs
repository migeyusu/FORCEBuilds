using System;

namespace FORCEBuild.DistributedService
{
    public interface IServiceProvider
    {
        Guid Filter { get; set; }

        bool Providing { get; set; }

        void Start();

        void End();
    }
}