using UnityEngine;

namespace TickBased.Logger
{
    public static class Logger
    {
        public static void Log(string debugText, string methodCall = "")
        {
            Debug.Log($"[{methodCall}] {debugText}");
        }

        public static void LogWarning(string debugText, string methodCall = "")
        {
            Debug.LogWarning($"[{methodCall}] {debugText}");
        }

        public static void LogError(string debugText, string methodCall = "")
        {
            Debug.LogError($"[{methodCall}] {debugText}");
        }
    }
}