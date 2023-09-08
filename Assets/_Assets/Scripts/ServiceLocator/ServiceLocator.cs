using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace FearProj.ServiceLocator
{
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> _registry = null;

        public static void ClearRegistry()
        {
            _registry.Clear();
        }

        public static void Register<T>() where T : IService
        {
            _registry ??= new Dictionary<Type, object>();

            //does service exist?
            if (_registry.ContainsKey(typeof(T))) return;

            var potentialService = ServiceLocatorUtils.GetClassFromAssemblyByInterface(typeof(T));

            if (potentialService == null)
            {
                var potentialObject = ServiceLocatorUtils.GetSceneObject<T>();

                //monobehaviour
                if (potentialObject != null)
                {
                    Logger.Log($"Registered (Monobehaviour)<color=green>{typeof(T)}</color> success!", "ServiceLocator");
                    _registry[typeof(T)] = potentialObject;
                }
                return;
            }
            Logger.Log($"Registered (Pure C#)<color=green>{typeof(T)}</color> success!", "ServiceLocator");
            var instance = Activator.CreateInstance(potentialService);
            _registry[typeof(T)] = instance;
            //Logger.Log($"Service: Registered (Pure C# Class){typeof(T)} success!");
        }

        public static T Get<T>() where T : IService
        {
            Register<T>();
            return (T)_registry[typeof(T)];
        }
    }
}