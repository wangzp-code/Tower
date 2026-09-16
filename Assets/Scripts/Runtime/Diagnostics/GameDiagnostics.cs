using UnityEngine;
using UnityEngine.Profiling;
using System.Collections.Generic;
using System.Diagnostics;

/// <summary>
/// 游戏诊断系统 - 收集运行时性能指标和诊断信息
/// 用于上线前的冒烟测试和性能基准测试
/// </summary>
public class GameDiagnostics : MonoBehaviour
{
    public static GameDiagnostics Instance { get; private set; }

    [Header("诊断设置")]
    public bool enableAutoDiagnostics = true;
    public float diagnosticsInterval = 5f; // 每5秒采集一次
    public bool logToConsole = true;

    private float _diagnosticsTimer = 0;
    
    // 性能指标
    private float _currentFPS;
    private float _avgFPS;
    private float _minFPS = float.MaxValue;
    private float _maxFPS = float.MinValue;
    private float _fpsAccumulator;
    private int _fpsFrameCount;
    
    // 内存指标
    private long _totalAllocatedMemory;
    private long _totalReservedMemory;
    private long _monoUsedMemory;
    private long _peakMemory;
    
    // 帧时间指标
    private float _currentFrameTimeMs;
    private float _avgFrameTimeMs;
    private float _maxFrameTimeMs;
    
    // 诊断历史
    private List<DiagnosticsSnapshot> _history = new List<DiagnosticsSnapshot>();
    private const int MAX_HISTORY = 100;
    
    // 警告阈值
    public float fpsWarningThreshold = 45f;
    public float fpsCriticalThreshold = 30f;
    public float frameTimeWarningMs = 22f;
    public long memoryWarningThreshold = 500 * 1024 * 1024; // 500MB
    public long memoryCriticalThreshold = 800 * 1024 * 1024; // 800MB

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        // 计算FPS
        _fpsAccumulator += Time.unscaledDeltaTime;
        _fpsFrameCount++;
        
        if (_fpsAccumulator >= 1f)
        {
            _currentFPS = _fpsFrameCount / _fpsAccumulator;
            _fpsFrameCount = 0;
            _fpsAccumulator = 0;
            
            UpdateFPSMetrics();
        }
        
        // 帧时间
        _currentFrameTimeMs = Time.unscaledDeltaTime * 1000f;
        if (_currentFrameTimeMs > _maxFrameTimeMs) _maxFrameTimeMs = _currentFrameTimeMs;
        
        // 定期诊断
        if (enableAutoDiagnostics)
        {
            _diagnosticsTimer += Time.unscaledDeltaTime;
            if (_diagnosticsTimer >= diagnosticsInterval)
            {
                _diagnosticsTimer = 0;
                CollectDiagnostics();
            }
        }
    }

    private void UpdateFPSMetrics()
    {
        // 更新平均值
        if (_avgFPS == 0) _avgFPS = _currentFPS;
        else _avgFPS = _avgFPS * 0.95f + _currentFPS * 0.05f; // 移动平均
        
        // 更新极值
        if (_currentFPS < _minFPS && _currentFPS > 0) _minFPS = _currentFPS;
        if (_currentFPS > _maxFPS) _maxFPS = _currentFPS;
        
        // 检测性能问题
        CheckPerformanceThresholds();
    }

    private void CheckPerformanceThresholds()
    {
        if (_currentFPS < fpsCriticalThreshold)
        {
            UnityEngine.Debug.LogError($"[Diagnostics] 严重性能警告: FPS={_currentFPS:F1} (阈值={fpsCriticalThreshold})");
        }
        else if (_currentFPS < fpsWarningThreshold)
        {
            UnityEngine.Debug.LogWarning($"[Diagnostics] 性能警告: FPS={_currentFPS:F1} (阈值={fpsWarningThreshold})");
        }
        
        if (_currentFrameTimeMs > frameTimeWarningMs)
        {
            UnityEngine.Debug.LogWarning($"[Diagnostics] 帧时间警告: {_currentFrameTimeMs:F1}ms (阈值={frameTimeWarningMs}ms)");
        }
    }

    public void CollectDiagnostics()
    {
        var snapshot = new DiagnosticsSnapshot
        {
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            fps = _currentFPS,
            avgFPS = _avgFPS,
            frameTimeMs = _currentFrameTimeMs,
            totalAllocatedMemory = Profiler.GetTotalAllocatedMemoryLong(),
            totalReservedMemory = Profiler.GetTotalReservedMemoryLong(),
            monoUsedMemory = Profiler.GetMonoUsedSizeLong(),
            sceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            gameObjectsCount = FindObjectsOfType<GameObject>().Length
        };
        
        // 更新峰值内存
        if (snapshot.totalReservedMemory > _peakMemory)
        {
            _peakMemory = snapshot.totalReservedMemory;
        }
        
        // 内存警告
        if (snapshot.totalReservedMemory > memoryCriticalThreshold)
        {
            UnityEngine.Debug.LogError($"[Diagnostics] 严重内存警告: {FormatBytes(snapshot.totalReservedMemory)} (阈值={FormatBytes(memoryCriticalThreshold)})");
        }
        else if (snapshot.totalReservedMemory > memoryWarningThreshold)
        {
            UnityEngine.Debug.LogWarning($"[Diagnostics] 内存警告: {FormatBytes(snapshot.totalReservedMemory)} (阈值={FormatBytes(memoryWarningThreshold)})");
        }
        
        // 添加到历史
        _history.Add(snapshot);
        if (_history.Count > MAX_HISTORY)
        {
            _history.RemoveAt(0);
        }
        
        if (logToConsole)
        {
            LogSnapshot(snapshot);
        }
    }

    private void LogSnapshot(DiagnosticsSnapshot snap)
    {
        UnityEngine.Debug.Log($"[Diagnostics] FPS:{snap.fps:F1} | 帧时间:{snap.frameTimeMs:F1}ms | 内存:{FormatBytes(snap.totalReservedMemory)} | 场景:{snap.sceneName} | 对象数:{snap.gameObjectsCount}");
    }

    public DiagnosticsReport GenerateReport()
    {
        return new DiagnosticsReport
        {
            reportTime = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            currentFPS = _currentFPS,
            avgFPS = _avgFPS,
            minFPS = _minFPS,
            maxFPS = _maxFPS,
            currentFrameTimeMs = _currentFrameTimeMs,
            avgFrameTimeMs = _avgFrameTimeMs,
            maxFrameTimeMs = _maxFrameTimeMs,
            peakMemoryBytes = _peakMemory,
            currentMemoryBytes = Profiler.GetTotalReservedMemoryLong(),
            currentMonoMemoryBytes = Profiler.GetMonoUsedSizeLong(),
            currentSceneName = UnityEngine.SceneManagement.SceneManager.GetActiveScene().name,
            history = new List<DiagnosticsSnapshot>(_history)
        };
    }

    public string GetQuickSummary()
    {
        return $"FPS: {_currentFPS:F1} (avg: {_avgFPS:F1})\n" +
               $"Frame: {_currentFrameTimeMs:F1}ms (max: {_maxFrameTimeMs:F1}ms)\n" +
               $"Memory: {FormatBytes(Profiler.GetTotalReservedMemoryLong())} (peak: {FormatBytes(_peakMemory)})\n" +
               $"Mono: {FormatBytes(Profiler.GetMonoUsedSizeLong())}\n" +
               $"Scene: {UnityEngine.SceneManagement.SceneManager.GetActiveScene().name}";
    }

    private string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }

    public void ResetMetrics()
    {
        _avgFPS = 0;
        _minFPS = float.MaxValue;
        _maxFPS = float.MinValue;
        _maxFrameTimeMs = 0;
        _peakMemory = 0;
        _history.Clear();
    }
}

/// <summary>
/// 诊断快照
/// </summary>
public class DiagnosticsSnapshot
{
    public long timestamp;
    public float fps;
    public float avgFPS;
    public float frameTimeMs;
    public long totalAllocatedMemory;
    public long totalReservedMemory;
    public long monoUsedMemory;
    public string sceneName;
    public int gameObjectsCount;
}

/// <summary>
/// 诊断报告
/// </summary>
public class DiagnosticsReport
{
    public long reportTime;
    public float currentFPS;
    public float avgFPS;
    public float minFPS;
    public float maxFPS;
    public float currentFrameTimeMs;
    public float avgFrameTimeMs;
    public float maxFrameTimeMs;
    public long peakMemoryBytes;
    public long currentMemoryBytes;
    public long currentMonoMemoryBytes;
    public string currentSceneName;
    public List<DiagnosticsSnapshot> history;
    
    public string ToSummaryString()
    {
        return $"=== 游戏诊断报告 ===\n" +
               $"时间: {System.DateTimeOffset.FromUnixTimeSeconds(reportTime).DateTime:yyyy-MM-dd HH:mm:ss}\n" +
               $"\n--- 性能指标 ---\n" +
               $"当前FPS: {currentFPS:F1}\n" +
               $"平均FPS: {avgFPS:F1}\n" +
               $"最低FPS: {minFPS:F1}\n" +
               $"最高FPS: {maxFPS:F1}\n" +
               $"当前帧时间: {currentFrameTimeMs:F1}ms\n" +
               $"最大帧时间: {maxFrameTimeMs:F1}ms\n" +
               $"\n--- 内存指标 ---\n" +
               $"当前内存: {FormatBytes(currentMemoryBytes)}\n" +
               $"峰值内存: {FormatBytes(peakMemoryBytes)}\n" +
               $"Mono内存: {FormatBytes(currentMonoMemoryBytes)}\n" +
               $"\n--- 场景信息 ---\n" +
               $"当前场景: {currentSceneName}\n" +
               $"历史快照数: {history?.Count ?? 0}";
    }
    
    private string FormatBytes(long bytes)
    {
        if (bytes < 1024) return $"{bytes} B";
        if (bytes < 1024 * 1024) return $"{bytes / 1024.0:F2} KB";
        if (bytes < 1024 * 1024 * 1024) return $"{bytes / (1024.0 * 1024):F2} MB";
        return $"{bytes / (1024.0 * 1024 * 1024):F2} GB";
    }
}
