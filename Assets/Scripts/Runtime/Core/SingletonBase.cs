using UnityEngine;
using System;
using System.Collections.Generic;

public abstract class SingletonBase<T> : MonoBehaviour where T : MonoBehaviour
{
    private static T _instance;
    private static readonly object _lock = new object();
    private static bool _applicationIsQuitting;

    public static T Instance
    {
        get
        {
            if (_applicationIsQuitting)
            {
                Debug.LogWarning($"[SingletonBase] Instance of {typeof(T)} already destroyed on application quit. Won't create again.");
                return null;
            }

            if (_instance != null) return _instance;

            lock (_lock)
            {
                if (_instance != null) return _instance;

                _instance = GameSystemRegistry.Get<T>();
                if (_instance != null) return _instance;

                _instance = FindObjectOfType<T>();
                if (_instance != null)
                {
                    GameSystemRegistry.Register(_instance);
                    return _instance;
                }

                var go = new GameObject($"[{typeof(T).Name}]");
                _instance = go.AddComponent<T>();
                DontDestroyOnLoad(go);
                GameSystemRegistry.Register(_instance);

                return _instance;
            }
        }
    }

    public static bool HasInstance => _instance != null && !_applicationIsQuitting;

    public static T GetOrCreate()
    {
        return Instance;
    }

    protected virtual void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Debug.LogWarning($"[SingletonBase] Duplicate instance of {typeof(T)} found. Destroying the duplicate.");
            Destroy(gameObject);
            return;
        }

        _instance = this as T;
        DontDestroyOnLoad(gameObject);
        GameSystemRegistry.Register(_instance);
    }

    protected virtual void OnDestroy()
    {
        if (this == _instance)
        {
            _instance = null;
            GameSystemRegistry.Unregister(this as T);
        }
    }

    protected virtual void OnApplicationQuit()
    {
        _applicationIsQuitting = true;
    }

    protected virtual void OnDisable()
    {
        CleanupEvents();
    }

    protected virtual void CleanupEvents()
    {
    }

    protected void SafeSubscribe<TEvent>(ref Delegate eventField, Action<TEvent> handler)
    {
        if (handler == null) return;
        eventField = Delegate.Combine(eventField, handler);
    }

    protected void SafeUnsubscribe<TEvent>(ref Delegate eventField, Action<TEvent> handler)
    {
        if (handler == null || eventField == null) return;
        eventField = Delegate.Remove(eventField, handler);
    }
}

public interface IGameSystem
{
    void Initialize();
    void Shutdown();
    void Reset();
}

public interface IUpdateSystem
{
    void Tick(float deltaTime);
}

public interface IFixedUpdateSystem
{
    void FixedTick(float fixedDeltaTime);
}

public interface ILateUpdateSystem
{
    void LateTick();
}