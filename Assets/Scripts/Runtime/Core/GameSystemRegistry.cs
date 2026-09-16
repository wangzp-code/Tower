using UnityEngine;
using System;
using System.Collections.Generic;

public static class GameSystemRegistry
{
    private static readonly Dictionary<Type, MonoBehaviour> _systems = new Dictionary<Type, MonoBehaviour>();
    private static readonly Dictionary<Type, Lazy<MonoBehaviour>> _lazySystems = new Dictionary<Type, Lazy<MonoBehaviour>>();
    private static readonly object _lock = new object();

    public static void Register<T>(T system) where T : MonoBehaviour
    {
        if (system == null) return;
        lock (_lock)
        {
            _systems[typeof(T)] = system;
        }
    }

    public static void Unregister<T>(T system) where T : MonoBehaviour
    {
        lock (_lock)
        {
            if (_systems.TryGetValue(typeof(T), out var existing) && existing == system)
            {
                _systems.Remove(typeof(T));
            }
        }
    }

    public static T Get<T>() where T : MonoBehaviour
    {
        lock (_lock)
        {
            if (_systems.TryGetValue(typeof(T), out var system))
            {
                return system as T;
            }

            if (_lazySystems.TryGetValue(typeof(T), out var lazy))
            {
                if (lazy.Value is T typed)
                {
                    _systems[typeof(T)] = typed;
                    return typed;
                }
            }

            return null;
        }
    }

    public static bool TryGet<T>(out T system) where T : MonoBehaviour
    {
        system = Get<T>();
        return system != null;
    }

    public static T GetOrCreate<T>() where T : MonoBehaviour
    {
        var existing = Get<T>();
        if (existing != null) return existing;

        lock (_lock)
        {
            existing = Get<T>();
            if (existing != null) return existing;

            var go = new GameObject($"[{typeof(T).Name}]");
            existing = go.AddComponent<T>();
            _systems[typeof(T)] = existing;
            return existing;
        }
    }

    public static void ClearAll()
    {
        lock (_lock)
        {
            _systems.Clear();
            _lazySystems.Clear();
        }
    }

    public static void ForEach(Action<Type, MonoBehaviour> action)
    {
        lock (_lock)
        {
            foreach (var kvp in _systems)
            {
                action?.Invoke(kvp.Key, kvp.Value);
            }
        }
    }

    public static int Count => _systems.Count;
}