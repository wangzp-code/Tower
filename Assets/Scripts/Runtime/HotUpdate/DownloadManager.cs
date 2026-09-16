using UnityEngine;
using UnityEngine.Networking;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System;

namespace HotUpdate
{
    public class DownloadManager : MonoBehaviour
    {
        public static DownloadManager Instance { get; private set; }
        
        private HotUpdateConfig config;
        private List<DownloadTask> tasks = new List<DownloadTask>();
        private int maxThreads = 3;
        private int activeDownloads = 0;
        
        public event EventHandler<DownloadTask> OnTaskProgress;
        public event EventHandler<DownloadTask> OnTaskComplete;
        public event EventHandler<DownloadTask> OnTaskFailed;
        public event EventHandler<int> OnAllComplete;
        
        private bool isDownloading = false;
        private bool isPaused = false;
        
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void Initialize(HotUpdateConfig configuration)
        {
            config = configuration;
            maxThreads = configuration.maxDownloadThreads;
        }
        
        public void AddTask(DownloadTask task)
        {
            tasks.Add(task);
        }
        
        public void AddTasks(List<DownloadTask> newTasks)
        {
            tasks.AddRange(newTasks);
        }
        
        public IEnumerator StartDownload()
        {
            if (config == null || !config.enabled)
            {
                Debug.Log("[DownloadManager] Download disabled");
                yield break;
            }
            
            if (isDownloading)
            {
                Debug.LogWarning("[DownloadManager] Already downloading");
                yield break;
            }
            
            isDownloading = true;
            isPaused = false;
            
            var pendingTasks = new List<DownloadTask>(tasks.FindAll(t => !t.isComplete && !t.isFailed));
            
            Debug.Log($"[DownloadManager] Starting download: {pendingTasks.Count} tasks");
            
            int completed = 0;
            
            while (pendingTasks.Count > 0 && !isPaused)
            {
                if (activeDownloads < maxThreads)
                {
                    var task = pendingTasks[0];
                    pendingTasks.RemoveAt(0);
                    
                    StartCoroutine(DownloadFile(task));
                }
                
                yield return null;
                
                completed = tasks.FindAll(t => t.isComplete).Count;
                OnTaskProgress?.Invoke(this, new DownloadTask
                {
                    progress = tasks.Count > 0 ? (float)completed / tasks.Count : 0f,
                    completedBundles = completed,
                    totalBundles = tasks.Count
                });
            }
            
            while (activeDownloads > 0)
            {
                yield return null;
            }
            
            isDownloading = false;
            
            int successCount = tasks.FindAll(t => t.isComplete).Count;
            int failCount = tasks.FindAll(t => t.isFailed).Count;
            
            Debug.Log($"[DownloadManager] Download complete: {successCount} success, {failCount} failed");
            OnAllComplete?.Invoke(this, successCount);
            
            tasks.Clear();
        }
        
        private IEnumerator DownloadFile(DownloadTask task)
        {
            activeDownloads++;
            task.progress = 0;
            
            string tempPath = task.localPath + ".tmp";
            string directory = Path.GetDirectoryName(task.localPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string lastError = null;
            bool downloadSuccess = false;
            
            UnityWebRequest request = null;
            
            try
            {
                request = UnityWebRequest.Get(task.downloadUrl);
                var handler = new DownloadHandlerFile(tempPath);
                request.downloadHandler = handler;
            }
            catch (Exception e)
            {
                lastError = e.Message;
                activeDownloads--;
                task.isFailed = true;
                task.errorMessage = lastError;
                OnTaskFailed?.Invoke(this, task);
                Debug.LogWarning($"[DownloadManager] Failed to create request: {task.bundleName} - {lastError}");
                yield break;
            }
            
            var operation = request.SendWebRequest();
            float lastProgress = 0f;
            
            while (!operation.isDone)
            {
                if (isPaused)
                {
                    request.Abort();
                    lastError = "Download paused";
                    break;
                }
                
                task.progress = request.downloadProgress;
                
                if (task.progress - lastProgress >= 0.1f)
                {
                    lastProgress = task.progress;
                    OnTaskProgress?.Invoke(this, task);
                }
                
                yield return null;
            }
            
            if (!isPaused && request.result == UnityWebRequest.Result.Success)
            {
                downloadSuccess = true;
            }
            else if (!isPaused)
            {
                lastError = request.error;
            }
            
            request.Dispose();
            
            if (downloadSuccess && !isPaused)
            {
                if (File.Exists(tempPath))
                {
                    if (!string.IsNullOrEmpty(task.hash))
                    {
                        string fileHash = CalculateFileHash(tempPath);
                        if (fileHash != task.hash)
                        {
                            File.Delete(tempPath);
                            task.isFailed = true;
                            task.errorMessage = "Hash mismatch";
                            OnTaskFailed?.Invoke(this, task);
                            activeDownloads--;
                            yield break;
                        }
                    }
                    
                    if (File.Exists(task.localPath))
                    {
                        File.Delete(task.localPath);
                    }
                    File.Move(tempPath, task.localPath);
                    
                    task.progress = 1f;
                    task.isComplete = true;
                    OnTaskComplete?.Invoke(this, task);
                    Debug.Log($"[DownloadManager] Downloaded: {task.bundleName} ({task.size} bytes)");
                }
            }
            else if (!isPaused)
            {
                task.isFailed = true;
                task.errorMessage = lastError ?? "Unknown error";
                OnTaskFailed?.Invoke(this, task);
                Debug.LogWarning($"[DownloadManager] Failed: {task.bundleName} - {task.errorMessage}");
                
                if (File.Exists(tempPath))
                {
                    File.Delete(tempPath);
                }
            }
            
            activeDownloads--;
        }
        
        public void Pause()
        {
            isPaused = true;
            Debug.Log("[DownloadManager] Download paused");
        }
        
        public void Resume()
        {
            isPaused = false;
            Debug.Log("[DownloadManager] Download resumed");
        }
        
        public void CancelAll()
        {
            StopAllCoroutines();
            tasks.Clear();
            activeDownloads = 0;
            isDownloading = false;
            isPaused = false;
            Debug.Log("[DownloadManager] All downloads cancelled");
        }
        
        public bool IsDownloading()
        {
            return isDownloading;
        }
        
        public float GetOverallProgress()
        {
            if (tasks.Count == 0) return 0f;
            int completed = tasks.FindAll(t => t.isComplete).Count;
            return (float)completed / tasks.Count;
        }
        
        public int GetCompletedCount()
        {
            return tasks.FindAll(t => t.isComplete).Count;
        }
        
        public int GetTotalCount()
        {
            return tasks.Count;
        }
        
        private string CalculateFileHash(string filePath)
        {
            using (var md5 = MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                var builder = new StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}
