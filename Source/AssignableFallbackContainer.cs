// Copyright © 2018 ceCosmos, Brazil. All rights reserved.
// Project: ProtoStar
// Author: Johni Michels

using System;
using System.Collections.Generic;
using System.Linq;
using System.ComponentModel.Design;
using Microsoft.Extensions.DependencyInjection;
using ProtoStar.Core;
using ProtoStar.Collections;

namespace ProtoStar.DependencyInjection
{
    public class AssignableFallbackContainer :
        IServiceContainer, ISupportRequiredService
    {
        private readonly Dictionary<Type, Func<object>> resolvers = new Dictionary<Type, Func<object>>();

        public void AddService(Type serviceType, ServiceCreatorCallback callback)=>
            resolvers[serviceType] = ()=>callback(this,serviceType);
        

        public void AddService(Type serviceType, ServiceCreatorCallback callback, bool promote)=>
            AddService(serviceType,callback);

        public void AddService(Type serviceType, object serviceInstance)
        {
            serviceType.EnsureInheritance(serviceInstance.GetType());
            resolvers[serviceType] = ()=>serviceInstance;
        }

        public void AddService(Type serviceType, object serviceInstance, bool promote)=>
            AddService(serviceType,serviceInstance);

        public void RemoveService(Type serviceType)=>
            resolvers.Remove(serviceType);
        

        public void RemoveService(Type serviceType, bool promote)=>
            RemoveService(serviceType);

        public object GetService(Type serviceType)
        {
            if (!resolvers.TryGetValue(serviceType, out var result))
            {
                result = resolvers.TryFind(
                    keyValue=> serviceType.IsAssignableFrom(keyValue.Key),
                    out var service)?
                    service.Value:
                    ()=>null;
            }
            return result();
        }

        public object GetRequiredService(Type serviceType)=>
            ((Func<object>)GetService(serviceType).ThrowOnNull).
            CatchingException<object,ArgumentNullException>(()=>throw new InvalidOperationException());

    }
}