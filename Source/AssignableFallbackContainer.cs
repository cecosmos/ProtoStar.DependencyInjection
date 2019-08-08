// Copyright © 2018 ceCosmos, Brazil. All rights reserved.
// Project: ProtoStar
// Author: Johni Michels

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;

namespace ProtoStar.DependencyInjection
{
    /// <summary>
    /// This service container can fallback any service not declared to a
    /// declared service that implements the requested service.
    /// </summary>
    /// <example>
    /// <code>
    /// public interface INumberService
    /// {
    ///     IEnumerable<int> GetNumbers();
    /// }
    /// public interface IEvenService : INumberService
    /// {
    ///     IEnumerable<int> GetEvenNumbers();
    /// }
    /// public 
    /// public class EvenService : INumberService
    /// {
    ///     public IEnumerable<int> GetNumbers() => GetEvenNumbers();
    ///     public IEnumerable<int> GetEvenNumbers() => Enumerable.Range(0,int.MaxValue)
    /// }
    /// 
    /// </code>
    /// </example>
    public class AssignableFallbackContainer : ServiceContainer
    {
        private readonly Dictionary<Type, Func<object>> resolvers = new Dictionary<Type, Func<object>>();

        public AssignableFallbackContainer() { }

        public AssignableFallbackContainer(IServiceProvider parentProvider)
        {
            ParentProvider = parentProvider;
        }

        protected override Type[] DefaultServices => base.DefaultServices.Union(new[] { GetType() }).ToArray();

        private IServiceProvider ParentProvider { get; set; }

        private IServiceContainer ValidPromotion(bool promote) =>
            (promote && ParentProvider is IServiceContainer serviceContainer) ?
                serviceContainer :
                null;

        private IServiceContainer ValidPromotion(bool promote, out bool wasPromoted)
        {
            var result = ValidPromotion(promote);
            wasPromoted = result == null ? false : true;
            return result;
        }
        

        public override object GetService(Type serviceType)
        {
            serviceType.ThrowOnNull(nameof(serviceType));
            object service = null;

            if (DefaultServices.Any(t => serviceType.IsEquivalentTo(t))) return this;

            if (!resolvers.TryGetValue(serviceType, out var value))
                value = (resolvers.FirstOrDefault(keyValue=> serviceType.IsAssignableFrom(keyValue.Key)).Value) ??
                    (() => null);

            service = value();

            if (service is ServiceCreatorCallback serviceCreator)
            {
                service = serviceCreator(this, serviceType);
                if (service != null && !service.GetType().IsCOMObject && !serviceType.IsInstanceOfType(service)) service = null;
                resolvers[serviceType] = () => service;
            }

            if (service == null && ParentProvider != null) service = ParentProvider.GetService(serviceType);

            return service;
        }

        public override void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)
        {
            ValidPromotion(promote, out var wasPromoted)?.AddService(serviceType, callback, promote);
            if (wasPromoted) return;
            serviceType.ThrowOnNull(nameof(serviceType));
            callback.ThrowOnNull(nameof(callback));
            resolvers.Add(serviceType, () => callback);
            
        }

        public override void AddService(Type serviceType, object serviceInstance, bool promote)
        {

            ValidPromotion(promote, out var wasPromoted)?.AddService(serviceType, serviceInstance, promote);
            if (wasPromoted) return;

            if (serviceInstance is ServiceCreatorCallback serviceCreator)
            {
                AddService(serviceType, serviceCreator, promote);
                return;
            }

            if (!serviceInstance.ThrowOnNull(nameof(serviceInstance)).GetType().IsCOMObject &
                !serviceType.ThrowOnNull(nameof(serviceType)).IsInstanceOfType(serviceInstance))
                throw new ArgumentException();

            resolvers.Add(serviceType, () => serviceInstance);

        }

        public override void RemoveService(Type serviceType, bool promote)
        {
            ValidPromotion(promote, out var wasPromoted)?.RemoveService(serviceType, promote);
            if (wasPromoted) return;
            resolvers.Remove(serviceType);            
        }
    }
}