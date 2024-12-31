#if UNITY_ENGINE
using System;
using System.Threading;
using UnityEngine;

namespace DebugSystems
{
    public class UnityDebugLogger : IDebugLogger
    {
        public UnityDebugLogger()
        {
            _mainThreadContext = SynchronizationContext.Current;
        }

        SynchronizationContext _mainThreadContext;

        public void Log(string message)
        {
            _mainThreadContext.Post(_ => Debug.Log(message), null);
        }

        public void LogWarning(string message)
        {
            _mainThreadContext.Post(_ => Debug.LogWarning(message), null);
        }

        public void LogError(string message)
        {
            _mainThreadContext.Post(_ => Debug.LogError(message), null);
        }
    }
}
#endif