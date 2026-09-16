using UnityEngine;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Linq;

public class AnalyticsSystem : SingletonBase<AnalyticsSystem>
{

    private string _logFilePath;
    private Queue<LogEntry> _logQueue = new Queue<LogEntry>();
    private bool _isProcessing = false;

    [Serializable]
    public class LogEntry
    {
        public string timestamp;
        public LogLevel level;
        public string category;
        public string message;
        public string stackTrace;
        public string userInfo;
    }

    public enum LogLevel
    {
        Debug,
        Info,
        Warning,
        Error,
        Fatal
    }

    protected override void Awake()
    {
        base.Awake();
        Initialize();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (this == Instance)
        {
            Application.logMessageReceived -= OnLogMessageReceived;
            ProcessLogQueue();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (this == Instance)
        {
            Application.logMessageReceived -= OnLogMessageReceived;
        }
    }

    private void Initialize()
    {
        _logFilePath = Path.Combine(Application.persistentDataPath, "game_logs.txt");
        
        Application.logMessageReceived += OnLogMessageReceived;
        
        WriteLog(LogLevel.Info, "System", "AnalyticsSystem initialized", null);
    }

    private void OnLogMessageReceived(string condition, string stackTrace, LogType type)
    {
        LogLevel level = LogLevel.Debug;
        switch (type)
        {
            case LogType.Error: level = LogLevel.Error; break;
            case LogType.Warning: level = LogLevel.Warning; break;
            case LogType.Log: level = LogLevel.Info; break;
            case LogType.Assert: level = LogLevel.Error; break;
            case LogType.Exception: level = LogLevel.Fatal; break;
        }
        
        WriteLog(level, "Unity", condition, stackTrace);
    }

    public static void TrackEvent(string eventName, Dictionary<string, object> parameters = null)
    {
        if (Instance == null) return;
        
        StringBuilder sb = new StringBuilder();
        sb.Append(eventName);
        
        if (parameters != null && parameters.Count > 0)
        {
            sb.Append(" {");
            foreach (var kvp in parameters)
            {
                sb.Append($"{kvp.Key}={kvp.Value}, ");
            }
            sb.Remove(sb.Length - 2, 2);
            sb.Append("}");
        }
        
        Instance.WriteLog(LogLevel.Info, "Event", sb.ToString(), null);
    }

    public static void TrackError(string errorMessage, string stackTrace = null)
    {
        if (Instance == null) return;
        Instance.WriteLog(LogLevel.Error, "Error", errorMessage, stackTrace);
    }

    public static void TrackException(Exception ex)
    {
        if (Instance == null || ex == null) return;
        
        string message = $"{ex.GetType().Name}: {ex.Message}";
        Instance.WriteLog(LogLevel.Fatal, "Exception", message, ex.StackTrace);
    }

    public static void TrackGameState(string state, Dictionary<string, object> data = null)
    {
        if (Instance == null) return;
        
        StringBuilder sb = new StringBuilder();
        sb.Append($"State: {state}");
        
        if (data != null && data.Count > 0)
        {
            sb.Append(" | ");
            foreach (var kvp in data)
            {
                sb.Append($"{kvp.Key}={kvp.Value}, ");
            }
            sb.Remove(sb.Length - 2, 2);
        }
        
        Instance.WriteLog(LogLevel.Info, "GameState", sb.ToString(), null);
    }

    private void WriteLog(LogLevel level, string category, string message, string stackTrace)
    {
        var entry = new LogEntry
        {
            timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            level = level,
            category = category,
            message = message,
            stackTrace = stackTrace,
            userInfo = GetUserInfo()
        };
        
        _logQueue.Enqueue(entry);
        
        if (!_isProcessing)
        {
            ProcessLogQueue();
        }
    }

    private string GetUserInfo()
    {
        try
        {
            var player = GameManager.Instance?.Player;
            if (player != null)
            {
                return $"Class={player.selectedClass}, Floor={GameManager.Instance.CurrentFloor}, HP={player.hp}/{player.maxHp}";
            }
        }
        catch { }
        
        return "Unknown";
    }

    private void ProcessLogQueue()
    {
        _isProcessing = true;
        
        try
        {
            using (var writer = new StreamWriter(_logFilePath, true, Encoding.UTF8))
            {
                while (_logQueue.Count > 0)
                {
                    var entry = _logQueue.Dequeue();
                    string line = $"[{entry.timestamp}] [{entry.level}] [{entry.category}] {entry.message} | User:{entry.userInfo}";
                    writer.WriteLine(line);
                    
                    if (!string.IsNullOrEmpty(entry.stackTrace))
                    {
                        writer.WriteLine($"Stack Trace: {entry.stackTrace}");
                    }
                }
            }
        }
        catch (Exception e)
        {
            Debug.LogError($"Failed to write log: {e.Message}");
        }
        
        _isProcessing = false;
        
        TrimLogFile();
    }

    private void TrimLogFile()
    {
        try
        {
            FileInfo fileInfo = new FileInfo(_logFilePath);
            if (fileInfo.Exists && fileInfo.Length > 10 * 1024 * 1024)
            {
                string backupPath = _logFilePath + ".bak";
                if (File.Exists(backupPath))
                {
                    File.Delete(backupPath);
                }
                File.Move(_logFilePath, backupPath);
            }
        }
        catch { }
    }

    public static string GetLogFilePath()
    {
        return Instance?._logFilePath ?? string.Empty;
    }

    public static List<LogEntry> GetRecentLogs(int count = 100)
    {
        if (Instance == null) return new List<LogEntry>();
        
        List<LogEntry> logs = new List<LogEntry>();
        
        try
        {
            if (File.Exists(Instance._logFilePath))
            {
                var lines = File.ReadAllLines(Instance._logFilePath, Encoding.UTF8);
                for (int i = Math.Max(0, lines.Length - count * 2); i < lines.Length; i += 2)
                {
                    if (i < lines.Length)
                    {
                        var line = lines[i];
                        if (!string.IsNullOrEmpty(line))
                        {
                            logs.Add(ParseLogLine(line));
                        }
                    }
                }
            }
        }
        catch { }
        
        return logs;
    }

    private static LogEntry ParseLogLine(string line)
    {
        try
        {
            int idx1 = line.IndexOf('[');
            int idx2 = line.IndexOf(']', idx1);
            string timestamp = line.Substring(idx1 + 1, idx2 - idx1 - 1);
            
            idx1 = line.IndexOf('[', idx2 + 1);
            idx2 = line.IndexOf(']', idx1);
            string levelStr = line.Substring(idx1 + 1, idx2 - idx1 - 1);
            
            idx1 = line.IndexOf('[', idx2 + 1);
            idx2 = line.IndexOf(']', idx1);
            string category = line.Substring(idx1 + 1, idx2 - idx1 - 1);
            
            string message = line.Substring(idx2 + 2);
            
            return new LogEntry
            {
                timestamp = timestamp,
                level = (LogLevel)Enum.Parse(typeof(LogLevel), levelStr),
                category = category,
                message = message
            };
        }
        catch
        {
            return new LogEntry { message = line };
        }
    }

    protected override void OnApplicationQuit()
    {
        base.OnApplicationQuit();
        ProcessLogQueue();
        WriteLog(LogLevel.Info, "System", "Application quit", null);
    }
}

public static class Analytics
{
    public static void Track(string eventName, params (string, object)[] parameters)
    {
        var dict = parameters.ToDictionary(p => p.Item1, p => p.Item2);
        AnalyticsSystem.TrackEvent(eventName, dict);
    }

    public static void Error(string message, string stackTrace = null)
    {
        AnalyticsSystem.TrackError(message, stackTrace);
    }

    public static void Exception(Exception ex)
    {
        AnalyticsSystem.TrackException(ex);
    }

    public static void GameState(string state, params (string, object)[] data)
    {
        var dict = data.ToDictionary(p => p.Item1, p => p.Item2);
        AnalyticsSystem.TrackGameState(state, dict);
    }
}