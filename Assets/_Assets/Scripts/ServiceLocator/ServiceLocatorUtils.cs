using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
namespace FearProj.ServiceLocator
{
    public class ServiceLocatorUtils : MonoBehaviour
    {

        public static T GetSceneObject<T>() where T : IService 
        {
            for (var i = 0; i < UnityEngine.SceneManagement.SceneManager.sceneCount; i++)
            {
                var scene = UnityEngine.SceneManagement.SceneManager.GetSceneAt(i);
                if (scene.isLoaded)
                {
                    foreach (var gameObj in scene.GetRootGameObjects())
                    {
                        //check root
                        if(gameObj.TryGetComponent<T>(out var rootObj))
                        {
                            return rootObj;
                        }

                        //check children
                        var result = GetChildComponentsRecursively<T>(gameObj.transform);
                        if (result != null)
                        {
                            return result;
                        }
                    }
                }
            }
            return default(T);
        }

        private static T GetChildComponentsRecursively<T>(Transform root) where T : IService
        {
            T result = default(T);
            for (var i = 0; i < root.childCount; i++)
            {
                var child = root.transform.GetChild(i);
                if (child.transform.childCount > 0)
                    result = GetChildComponentsRecursively<T>(child.transform);

                if (result == null)
                    child.TryGetComponent<T>(out result);

                if (result != null)
                    break;
            }
            return result;
        }

        public static Type GetClassFromAssemblyByInterface(Type interfaceType)
        {
            foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
            {
                foreach (var type in asm.GetTypes())
                {
                    if (!type.IsClass || type.BaseType == typeof(MonoBehaviour)) continue;

                    foreach (var iInterface in type.GetInterfaces())
                    {
                        if (iInterface != interfaceType) continue;
                        return type;
                    }
                }
            }

            return null;
        }
    }
}