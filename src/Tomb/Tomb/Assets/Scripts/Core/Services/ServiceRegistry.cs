using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Services
{
    public sealed class ServiceRegistry
    {
        private readonly Dictionary<Type, object> services = new();

        public void Register<T>(T service) where T : class
        {
            Type type = typeof(T);

            if (services.ContainsKey(type))
            {
                services[type] = service;
                return;
            }

            services.Add(type, service);
        }

        public T Get<T>() where T : class
        {
            Type type = typeof(T);

            if (services.TryGetValue(type, out object service))
            {
                return service as T;
            }

            throw new InvalidOperationException($"Service not found: {type.Name}");
        }

        public bool TryGet<T>(out T service) where T : class
        {
            Type type = typeof(T);

            if (services.TryGetValue(type, out object foundService))
            {
                service = foundService as T;
                return service != null;
            }

            service = null;
            return false;
        }

        public void Clear()
        {
            services.Clear();
        }
    }
}
