using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;

public class PerformanceManager : SingletonBase<PerformanceManager>
{

    [Header("Performance Settings")]
    [Tooltip("Target frame rate for the game")]
    public int targetFrameRate = 60;

    [Tooltip("Enable GPU Instancing for monsters")]
    public bool enableGPUInstancing = true;

    [Tooltip("Max number of monsters to render at once")]
    public int maxVisibleMonsters = 50;

    [Tooltip("Enable dynamic resolution")]
    public bool enableDynamicResolution = true;

    [Tooltip("Minimum resolution scale")]
    public float minResolutionScale = 0.7f;

    [Tooltip("Target frame time in milliseconds")]
    public float targetFrameTime = 16.67f;

    [Header("Quality Settings")]
    public bool enableSSAO = false;
    public bool enableMotionBlur = false;
    public bool enableDepthOfField = false;
    public int textureQuality = 2;
    public int antiAliasing = 2;

    private float frameTimeAccumulator = 0;
    private int frameCount = 0;
    private float currentResolutionScale = 1.0f;
    private float lastPerformanceUpdate = 0;

    private List<GameObject> activeMonsters = new List<GameObject>();
    private Dictionary<string, ObjectPool> objectPools = new Dictionary<string, ObjectPool>();

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        CleanupPools();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void Initialize()
    {
        Application.targetFrameRate = targetFrameRate;

        ApplyQualitySettings();

        if (enableDynamicResolution)
        {
            DynamicResolutionHandler.Init();
        }
    }

    private void ApplyQualitySettings()
    {
        if (Application.isMobilePlatform)
            SetMobileQualityFields();
        else
            SetDesktopQualityFields();

        ApplyUnityQualitySettings();
    }

    private void ApplyUnityQualitySettings()
    {
        ParasiteTowerArtOptimization.ApplyQualityForPlatform();
        QualitySettings.antiAliasing = antiAliasing;
    }

    private void SetMobileQualityFields()
    {
        enableSSAO = false;
        enableMotionBlur = false;
        enableDepthOfField = false;
        antiAliasing = 1;
        textureQuality = 1;
        maxVisibleMonsters = 30;
    }

    private void SetDesktopQualityFields()
    {
        enableSSAO = true;
        enableMotionBlur = true;
        enableDepthOfField = true;
        antiAliasing = 4;
        textureQuality = 3;
        maxVisibleMonsters = 100;
    }

    private void Update()
    {
        frameTimeAccumulator += Time.unscaledDeltaTime;
        frameCount++;

        if (Time.time - lastPerformanceUpdate >= 1.0f)
        {
            UpdatePerformanceMetrics();
            lastPerformanceUpdate = Time.time;
        }

        if (enableDynamicResolution)
        {
            UpdateDynamicResolution();
        }
    }

    private void UpdatePerformanceMetrics()
    {
        float avgFrameTime = frameTimeAccumulator / frameCount * 1000;
        int fps = Mathf.RoundToInt(1.0f / (frameTimeAccumulator / frameCount));

        frameTimeAccumulator = 0;
        frameCount = 0;

#if UNITY_EDITOR

#endif
    }

    private void UpdateDynamicResolution()
    {
        float currentFrameTime = Time.unscaledDeltaTime * 1000;

        if (currentFrameTime > targetFrameTime * 1.2f)
        {
            currentResolutionScale = Mathf.Max(minResolutionScale, currentResolutionScale - 0.05f);
        }
        else if (currentFrameTime < targetFrameTime * 0.8f)
        {
            currentResolutionScale = Mathf.Min(1.0f, currentResolutionScale + 0.02f);
        }

        DynamicResolutionHandler.SetScale(currentResolutionScale);
    }

    public void RegisterMonster(GameObject monster)
    {
        if (!activeMonsters.Contains(monster))
        {
            activeMonsters.Add(monster);
        }

        CullMonsters();
    }

    public void UnregisterMonster(GameObject monster)
    {
        activeMonsters.Remove(monster);
    }

    private void CullMonsters()
    {
        Camera mainCamera = Camera.main;
        if (mainCamera == null) return;

        Vector3 camPos = mainCamera.transform.position;

        activeMonsters.Sort((a, b) =>
        {
            float distA = Vector3.Distance(a.transform.position, camPos);
            float distB = Vector3.Distance(b.transform.position, camPos);
            return distA.CompareTo(distB);
        });

        for (int i = 0; i < activeMonsters.Count; i++)
        {
            // 在上限内的怪物恢复显示，超出上限的隐藏
            bool shouldBeActive = i < maxVisibleMonsters;
            if (activeMonsters[i].activeSelf != shouldBeActive)
            {
                activeMonsters[i].SetActive(shouldBeActive);
            }
        }
    }

    public ObjectPool GetOrCreatePool(string poolName, GameObject prefab, int initialSize = 10)
    {
        if (objectPools.TryGetValue(poolName, out ObjectPool pool))
        {
            return pool;
        }

        pool = new ObjectPool(prefab, initialSize);
        objectPools[poolName] = pool;
        return pool;
    }

    public void CleanupPools()
    {
        foreach (var pool in objectPools.Values)
        {
            pool.Clear();
        }
        objectPools.Clear();
    }

    public void LogMemoryUsage()
    {
        long totalMemory = Profiler.GetTotalAllocatedMemoryLong();
        long reservedMemory = Profiler.GetTotalReservedMemoryLong();
        long monoMemory = Profiler.GetMonoUsedSizeLong();


    }

    private string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }

    public void ApplyMobileOptimizations()
    {
        SetMobileQualityFields();
        ApplyUnityQualitySettings();
    }

    public void ApplyDesktopOptimizations()
    {
        SetDesktopQualityFields();
        ApplyUnityQualitySettings();
    }
}

public class ObjectPool
{
    private Queue<GameObject> pool = new Queue<GameObject>();
    private GameObject prefab;
    private Transform parent;

    public ObjectPool(GameObject prefab, int initialSize)
    {
        this.prefab = prefab;
        parent = new GameObject($"{prefab.name} Pool").transform;

        for (int i = 0; i < initialSize; i++)
        {
            CreateObject();
        }
    }

    private GameObject CreateObject()
    {
        GameObject obj = Object.Instantiate(prefab, parent);
        obj.SetActive(false);
        pool.Enqueue(obj);
        return obj;
    }

    public GameObject Get()
    {
        if (pool.Count == 0)
        {
            return CreateObject();
        }

        GameObject obj = pool.Dequeue();
        obj.SetActive(true);
        return obj;
    }

    public void Return(GameObject obj)
    {
        obj.SetActive(false);
        obj.transform.SetParent(parent);
        pool.Enqueue(obj);
    }

    public void Clear()
    {
        while (pool.Count > 0)
        {
            Object.Destroy(pool.Dequeue());
        }
        Object.Destroy(parent.gameObject);
    }
}

public static class DynamicResolutionHandler
{
    private static float currentScale = 1.0f;

    public static void Init()
    {
        // 连接 Unity ScalableBufferManager 实现真正的动态分辨率
        ScalableBufferManager.ResizeBuffers(currentScale, currentScale);
    }

    public static void SetScale(float scale)
    {
        currentScale = Mathf.Clamp01(scale);
        ScalableBufferManager.ResizeBuffers(currentScale, currentScale);
    }

    public static float GetScale()
    {
        return currentScale;
    }
}
