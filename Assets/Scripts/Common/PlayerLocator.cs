using System;
using UnityEngine;

public static class PlayerLocator
{
    static Transform _cached;
    public static event Action<Transform> OnAvailable;

    public static bool TryGet(out Transform t) { t = _cached; return t != null; }
    public static Transform Player => _cached;

    public static void Set(Transform t)
    {
        if (_cached == t) return;
        _cached = t;
        if (t) OnAvailable?.Invoke(t);
    }

    public static void Clear() { _cached = null; }
}
