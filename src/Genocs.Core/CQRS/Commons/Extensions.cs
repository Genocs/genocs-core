﻿namespace Genocs.Core.CQRS.Commons
{
    using Genocs.Core.CQRS.Commands;
    using Genocs.Core.CQRS.Commands.Dispatchers;
    using Genocs.Core.CQRS.Events;
    using Genocs.Core.CQRS.Events.Dispatchers;
    using Genocs.Core.CQRS.Queries;
    using Genocs.Core.CQRS.Queries.Dispatchers;
    using Genocs.Core.Types;
    using Microsoft.Extensions.DependencyInjection;
    using System;
    using System.Linq;

    /// <summary>
    /// Extension helper to handle the whole set of Dispatcher
    /// </summary>
    public static class Extensions
    {
        /// <summary>
        /// AddHandlers implementation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="project"></param>
        /// <returns></returns>
        public static IServiceCollection AddHandlers(this IServiceCollection services, string project)
        {
            var assemblies = AppDomain.CurrentDomain.GetAssemblies()
                .Where(x => x.FullName  != null && x.FullName.Contains(project))
                .ToArray();

            services.Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(ICommandHandler<>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IEventHandler<>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            services.Scan(s => s.FromAssemblies(assemblies)
                .AddClasses(c => c.AssignableTo(typeof(IQueryHandler<,>))
                    .WithoutAttribute<DecoratorAttribute>())
                .AsImplementedInterfaces()
                .WithScopedLifetime());

            return services;
        }

        /// <summary>
        /// AddDispatchers Implementation  
        /// </summary>
        /// <param name="services"></param>
        /// <returns></returns>
        public static IServiceCollection AddDispatchers(this IServiceCollection services)
            => services
                .AddSingleton<IDispatcher, InMemoryDispatcher>()
                .AddSingleton<ICommandDispatcher, CommandDispatcher>()
                .AddSingleton<IEventDispatcher, EventDispatcher>()
                .AddSingleton<IQueryDispatcher, QueryDispatcher>();
    }
}