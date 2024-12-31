using System.Runtime.CompilerServices;

namespace DebugSystems
{
    public static class DebugLogger
    {
#if UNITY_ENGINE
        static IDebugLogger s_debugLogger = new UnityDebugLogger();
#else
        static IDebugLogger s_debugLogger = new ConsoleDebugLogger();
#endif

        public static void Log(
            string message,
            [CallerMemberName] string callerMemeberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callLineNumber = 0)
        {
            s_debugLogger.Log($"[{callerMemeberName} @{callerFilePath}'\'{callLineNumber}] {message}");
        }

        public static void LogWarning(
            string message,
            [CallerMemberName] string callerMemeberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callLineNumber = 0)
        {
            s_debugLogger.LogWarning($"[{callerMemeberName} @{callerFilePath}'\'{callLineNumber}] {message}");
        }

        public static void LogError(
            string message,
            [CallerMemberName] string callerMemeberName = "",
            [CallerFilePath] string callerFilePath = "",
            [CallerLineNumber] int callLineNumber = 0)
        {
            s_debugLogger.LogError($"[{callerMemeberName} @{callerFilePath}'\'{callLineNumber}] {message}");
        }
    }
}