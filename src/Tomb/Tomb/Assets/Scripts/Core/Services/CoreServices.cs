using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;

namespace Tomb.Core.Services
{
    public static class CoreServices
    {
        public static ServiceRegistry Registry { get; private set; }

        public static void Initialize(ServiceRegistry registry)
        {
            Registry = registry;
        }

        public static T Get<T>() where T : class
        {
            return Registry.Get<T>();
        }

        public static bool TryGet<T>(out T service) where T : class
        {
            return Registry.TryGet(out service);
        }
    }
}
