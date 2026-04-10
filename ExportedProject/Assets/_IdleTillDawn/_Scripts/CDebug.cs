using System.Diagnostics;

/// <summary>
/// 에디터 전용 디버그 래퍼.
/// [Conditional("UNITY_EDITOR")] 에 의해 빌드 시 호출 코드 자체가 제거됩니다.
/// </summary>
public static class CDebug
{
    [Conditional("UNITY_EDITOR")]
    public static void Log(object message) => UnityEngine.Debug.Log(message);

    [Conditional("UNITY_EDITOR")]
    public static void Log(object message, UnityEngine.Object context) => UnityEngine.Debug.Log(message, context);

    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(object message) => UnityEngine.Debug.LogWarning(message);

    [Conditional("UNITY_EDITOR")]
    public static void LogWarning(object message, UnityEngine.Object context) => UnityEngine.Debug.LogWarning(message, context);

    [Conditional("UNITY_EDITOR")]
    public static void LogError(object message) => UnityEngine.Debug.LogError(message);

    [Conditional("UNITY_EDITOR")]
    public static void LogError(object message, UnityEngine.Object context) => UnityEngine.Debug.LogError(message, context);

    [Conditional("UNITY_EDITOR")]
    public static void LogFormat(string format, params object[] args) => UnityEngine.Debug.LogFormat(format, args);
}
