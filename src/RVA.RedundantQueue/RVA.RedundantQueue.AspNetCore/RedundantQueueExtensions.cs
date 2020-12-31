using System;
using Microsoft.Extensions.DependencyInjection;
using RVA.RedundantQueue.Abstractions;
using RVA.RedundantQueue.Implementations;

// ReSharper disable UnusedType.Global
// ReSharper disable UnusedMember.Global
// ReSharper disable MemberCanBePrivate.Global

namespace RVA.RedundantQueue.AspNetCore
{
    public static class RedundantQueueExtensions
    {
        public static IServiceCollection AddRedundantQueues(IServiceCollection services,
            Action<IRedundantQueueOrchestrator> callback, out IRedundantQueueOrchestrator redundantQueueOrchestrator)
        {
            redundantQueueOrchestrator = new RedundantQueueOrchestrator();
            callback(redundantQueueOrchestrator);
            services.AddSingleton(redundantQueueOrchestrator);
            return services;
        }

        public static IServiceCollection AddRedundantQueues(IServiceCollection services,
            Action<IRedundantQueueOrchestrator> callback)
        {
            return AddRedundantQueues(services, callback, out _);
        }

        public static IRedundantQueueOrchestrator AddRedundantQueues(IServiceCollection services)
        {
            AddRedundantQueues(services, null, out var redundantQueueOrchestrator);
            return redundantQueueOrchestrator;
        }
    }
}