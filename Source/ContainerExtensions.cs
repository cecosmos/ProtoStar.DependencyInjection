// Copyright © 2018 ceCosmos, Brazil. All rights reserved.
// Project: ProtoStar
// Author: Johni Michels

using System.Linq;
using System;
using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.Generic;

namespace ProtoStar.DependencyInjection
{
    public static class ContainerExtensions
    {
        public static void SolveDependencies(this IServiceProvider container, object dependent)
        {
            var genericIDependent = typeof(IDependent<>);
            var dependencies = dependent.GetType().
                GetGenericArgumentsForBaseType(genericIDependent).
                Select(t => t.First());
            foreach(var t in dependencies)
            {
                var dependencyTypeResolver = genericIDependent.MakeGenericType(t);
                var targetInjectionProperty = dependencyTypeResolver.GetProperties().First();
                var containerResolved = container.GetService(t);
                targetInjectionProperty.SetValue(dependent, containerResolved);
            }
        }

        public static void AddService<T>(this IServiceContainer provider, T service)=>
            provider.AddService(typeof(T), service);

    }
}
