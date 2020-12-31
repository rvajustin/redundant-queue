using System;
using Microsoft.Extensions.DependencyInjection;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Implementations;

namespace RVA.RedundantQueue.AspNetCore
{
    public static class RedundantQueueExtensions
    {
        public static IServiceCollection AddRedundantQueues(IServiceCollection services,
            Action<IRedundantQueueOrchestrator> callback)
        {
            var orchestrator = new RedundantQueueOrchestrator();
            callback(orchestrator);
            services.AddSingleton<IRedundantQueueOrchestrator>(orchestrator);
            return services;
        }
    }
}