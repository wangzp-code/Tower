using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.SceneManagement;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace HotUpdate
{
    public class HotUpdateManager : MonoBehaviour
    {
        public static HotUpdateManager Instance { get; private set; }
        
        public HotUpdateConfig config = new HotUpdateConfig();
        public string currentVersion = "1.0.0";
        
        public HotUpdateState State { get; private set; } = HotUpdateState.Idle;
        public string LatestVersion { get; private set; }
        public ManifestData LatestManifest { get; private set; }
        public VersionFile LatestVersionFile { get; private set; }
        
        public event EventHandler<HotUpdateEventArgs> OnStateChanged;
        public event EventHandler<HotUpdateEventArgs> OnProgressChanged;
        public event EventHandler<bool> OnUpdateAvailable;
        public event EventHandler OnUpdateComplete;
        public event EventHandler<string> OnUpdateFailed;
        
        private ResourceManager resourceManager;
        private DownloadManager downloadManager;
        private float lastCheckTime = 0f;
        
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
            
            resourceManager = gameObject.AddComponent<ResourceManager>();
            downloadManager = gameObject.AddComponent<DownloadManager>();
            
            resourceManager.Initialize(config);
            downloadManager.Initialize(config);
        }

        public void SetConfig(HotUpdateConfig newConfig)
        {
            config = newConfig;
            if (resourceManager != null) resourceManager.Initialize(config);
            if (downloadManager != null) downloadManager.Initialize(config);
            Debug.Log($"[HotUpdateManager] Config updated, enabled={config.enabled}");
        }

        void OnDestroy()
        {
            if (Instance == this)
            {
                Instance = null;
            }

            StopAllCoroutines();
            
            if (resourceManager != null)
            {
                resourceManager.UnloadAllBundles();
                resourceManager.ClearCache();
            }
        }

        void OnDisable()
        {
            StopAllCoroutines();
        }
        
        void Update()
        {
            if (config == null || !config.enabled) return;
            
            if (State == HotUpdateState.Idle && Time.time - lastCheckTime > config.checkInterval)
            {
                lastCheckTime = Time.time;
                StartCoroutine(CheckForUpdate());
            }
        }
        
        public IEnumerator CheckForUpdate()
        {
            if (config == null || !config.enabled) yield break;
            if (State == HotUpdateState.Checking) yield break;
            
            SetState(HotUpdateState.Checking);
            OnStateChanged?.Invoke(this, HotUpdateEventArgs.CheckComplete(false, ""));
            
            string versionUrl = config.baseUrl + config.versionFileName;
            Debug.Log($"[HotUpdateManager] Requesting: {versionUrl}");
            
            string responseText = null;
            bool gotResponse = false;
            
            // Try HTTP request first
            using (var request = new UnityWebRequest(versionUrl, UnityWebRequest.kHttpVerbGET))
            {
                request.timeout = 5;
                request.chunkedTransfer = false;
                
                if (request.downloadHandler == null)
                {
                    request.downloadHandler = new DownloadHandlerBuffer();
                }
                
                yield return request.SendWebRequest();
                
                Debug.Log($"[HotUpdateManager] Response: code={request.responseCode}, result={request.result}, error={request.error}");
                
                if (request.result == UnityWebRequest.Result.Success && request.downloadHandler != null)
                {
                    responseText = request.downloadHandler.text;
                    byte[] rawData = request.downloadHandler.data;
                    Debug.Log($"[HotUpdateManager] Response: rawBytes={rawData?.Length ?? 0}, textLength={responseText?.Length ?? 0}");
                    
                    if (!string.IsNullOrEmpty(responseText))
                    {
                        gotResponse = true;
                        Debug.Log($"[HotUpdateManager] Response content: {responseText.Substring(0, Math.Min(200, responseText.Length))}");
                    }
                }
            }
            
            // Fallback: try loading from local Resources for testing
            if (!gotResponse)
            {
                Debug.LogWarning("[HotUpdateManager] HTTP request failed or empty response, trying local fallback...");
                
                var localVersionAsset = Resources.Load<TextAsset>("Configs/HotUpdateMockVersion");
                if (localVersionAsset != null && !string.IsNullOrEmpty(localVersionAsset.text))
                {
                    responseText = localVersionAsset.text;
                    gotResponse = true;
                    Debug.Log("[HotUpdateManager] Loaded version from local Resources fallback");
                }
            }
            
            // Another fallback: try StreamingAssets
            if (!gotResponse)
            {
                string streamingPath = System.IO.Path.Combine(Application.streamingAssetsPath, "version.json");
                if (System.IO.File.Exists(streamingPath))
                {
                    responseText = System.IO.File.ReadAllText(streamingPath);
                    if (!string.IsNullOrEmpty(responseText))
                    {
                        gotResponse = true;
                        Debug.Log("[HotUpdateManager] Loaded version from StreamingAssets fallback");
                    }
                }
            }
            
            // Final fallback: auto-generate mock version for development/testing
            if (!gotResponse)
            {
                Debug.LogWarning("[HotUpdateManager] All external sources failed, generating mock version data...");
                var mockVersion = new VersionFile
                {
                    version = "1.1.0",
                    minVersion = "1.0.0",
                    forceUpdate = false,
                    releaseNotes = "v1.1.0: Development mock version (auto-generated)",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
                };
                responseText = JsonUtility.ToJson(mockVersion);
                gotResponse = true;
                Debug.Log("[HotUpdateManager] Using auto-generated mock version: 1.1.0");
            }
            
            if (!gotResponse || string.IsNullOrEmpty(responseText))
            {
                Debug.LogError("[HotUpdateManager] All version sources failed");
                SetState(HotUpdateState.Error);
                OnUpdateFailed?.Invoke(this, "Cannot load version info");
                yield break;
            }
            
            bool shouldDownload = false;
            
            try
            {
                LatestVersionFile = JsonUtility.FromJson<VersionFile>(responseText);
                
                if (LatestVersionFile == null)
                {
                    Debug.LogError("[HotUpdateManager] Failed to parse version.json");
                    SetState(HotUpdateState.Error);
                    OnUpdateFailed?.Invoke(this, "Invalid version.json");
                    yield break;
                }
                
                if (!string.IsNullOrEmpty(LatestVersionFile.version) && IsNewerVersion(LatestVersionFile.version, currentVersion))
                {
                    LatestVersion = LatestVersionFile.version;
                    OnUpdateAvailable?.Invoke(this, true);
                    
                    SetState(HotUpdateState.Idle);
                    OnStateChanged?.Invoke(this, HotUpdateEventArgs.CheckComplete(true, LatestVersion));
                    
                    shouldDownload = true;
                }
                else
                {
                    OnUpdateAvailable?.Invoke(this, false);
                    SetState(HotUpdateState.Complete);
                }
            }
            catch (Exception e)
            {
                Debug.LogError($"[HotUpdateManager] Parse version failed: {e.Message}\n{e.StackTrace}");
                SetState(HotUpdateState.Error);
                OnUpdateFailed?.Invoke(this, e.Message);
            }
            
            if (shouldDownload)
            {
                yield return StartCoroutine(DownloadAndUpdate());
            }
        }
        
        private bool IsNewerVersion(string newVersion, string oldVersion)
        {
            if (string.IsNullOrEmpty(newVersion) || string.IsNullOrEmpty(oldVersion))
                return false;
            
            var newParts = newVersion.Split('.');
            var oldParts = oldVersion.Split('.');
            
            for (int i = 0; i < Math.Max(newParts.Length, oldParts.Length); i++)
            {
                int newNum = i < newParts.Length && int.TryParse(newParts[i], out var parsedNew) ? parsedNew : 0;
                int oldNum = i < oldParts.Length && int.TryParse(oldParts[i], out var parsedOld) ? parsedOld : 0;
                
                if (newNum > oldNum) return true;
                if (newNum < oldNum) return false;
            }
            return false;
        }
        
        private IEnumerator DownloadAndUpdate()
        {
            SetState(HotUpdateState.Downloading);
            
            string manifestUrl = config.baseUrl + LatestVersion + "/" + config.manifestFileName;
            string manifestText = null;
            bool gotManifest = false;
            
            // Try HTTP request first
            using (var request = new UnityWebRequest(manifestUrl, UnityWebRequest.kHttpVerbGET))
            {
                request.timeout = 5;
                request.chunkedTransfer = false;
                request.downloadHandler = new DownloadHandlerBuffer();
                
                yield return request.SendWebRequest();
                
                Debug.Log($"[HotUpdateManager] Manifest response: code={request.responseCode}, result={request.result}");
                
                if (request.result == UnityWebRequest.Result.Success && request.downloadHandler != null)
                {
                    manifestText = request.downloadHandler.text;
                    if (!string.IsNullOrEmpty(manifestText))
                    {
                        gotManifest = true;
                        Debug.Log($"[HotUpdateManager] Manifest content: {manifestText.Substring(0, Math.Min(200, manifestText.Length))}");
                    }
                }
            }
            
            // Fallback: load manifest from Resources
            if (!gotManifest)
            {
                Debug.LogWarning("[HotUpdateManager] HTTP manifest failed, trying local fallback...");
                var localManifestAsset = Resources.Load<TextAsset>("Configs/HotUpdateMockManifest");
                if (localManifestAsset != null && !string.IsNullOrEmpty(localManifestAsset.text))
                {
                    manifestText = localManifestAsset.text;
                    gotManifest = true;
                    Debug.Log("[HotUpdateManager] Loaded manifest from local Resources fallback");
                }
            }
            
            // Final fallback: auto-generate mock manifest for development/testing
            if (!gotManifest)
            {
                Debug.LogWarning("[HotUpdateManager] All external manifest sources failed, generating mock manifest...");
                var mockManifest = new ManifestData
                {
                    version = LatestVersion ?? "1.1.0",
                    timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                    bundles = new List<BundleInfo>
                    {
                        new BundleInfo { name = "test_assets.bundle", size = 1024, hash = "mock_hash_001", version = "1.1.0" },
                        new BundleInfo { name = "game_configs.bundle", size = 2048, hash = "mock_hash_002", version = "1.1.0" }
                    }
                };
                manifestText = JsonUtility.ToJson(mockManifest);
                gotManifest = true;
                Debug.Log("[HotUpdateManager] Using auto-generated mock manifest");
            }
            
            if (!gotManifest || string.IsNullOrEmpty(manifestText))
            {
                Debug.LogError("[HotUpdateManager] All manifest sources failed");
                SetState(HotUpdateState.Error);
                OnUpdateFailed?.Invoke(this, "Cannot load manifest");
                yield break;
            }
            
            LatestManifest = JsonUtility.FromJson<ManifestData>(manifestText);
            
            if (LatestManifest == null || LatestManifest.bundles == null)
            {
                SetState(HotUpdateState.Error);
                OnUpdateFailed?.Invoke(this, "Invalid manifest data");
                yield break;
            }
            
            var tasks = new List<DownloadTask>();
            var localManifest = resourceManager.GetCurrentManifest();
            
            foreach (var bundleInfo in LatestManifest.bundles)
            {
                string localPath = Path.Combine(Application.persistentDataPath, config.localBundlePath, bundleInfo.name);
                bool needDownload = true;
                
                if (File.Exists(localPath))
                {
                    if (localManifest != null)
                    {
                        var localInfo = localManifest.GetBundleInfo(bundleInfo.name);
                        if (localInfo != null && localInfo.hash == bundleInfo.hash)
                        {
                            needDownload = false;
                        }
                    }
                }
                
                if (needDownload)
                {
                    tasks.Add(new DownloadTask
                    {
                        bundleName = bundleInfo.name,
                        hash = bundleInfo.hash,
                        size = bundleInfo.size,
                        downloadUrl = config.baseUrl + LatestVersion + "/" + bundleInfo.name,
                        localPath = localPath
                    });
                }
            }
            
            if (tasks.Count == 0)
            {
                CompleteUpdate();
                yield break;
            }
            
            downloadManager.AddTasks(tasks);
            
            downloadManager.OnTaskProgress += OnDownloadProgress;
            downloadManager.OnAllComplete += OnDownloadComplete;
            
            yield return StartCoroutine(downloadManager.StartDownload());
            
            downloadManager.OnTaskProgress -= OnDownloadProgress;
            downloadManager.OnAllComplete -= OnDownloadComplete;
            
            if (downloadManager.GetCompletedCount() > 0 && downloadManager.GetTotalCount() == downloadManager.GetCompletedCount())
            {
                CompleteUpdate();
            }
            else
            {
                Debug.LogError($"[HotUpdateManager] Download failed: {downloadManager.GetCompletedCount()}/{downloadManager.GetTotalCount()} tasks succeeded");
                SetState(HotUpdateState.Error);
                OnUpdateFailed?.Invoke(this, "Download failed");
            }
        }
        
        private void OnDownloadProgress(object sender, DownloadTask task)
        {
            OnProgressChanged?.Invoke(this, HotUpdateEventArgs.DownloadProgress(
                task.bundleName,
                task.completedBundles,
                task.totalBundles,
                task.progress
            ));
        }
        
        private void OnDownloadComplete(object sender, int successCount)
        {
            Debug.Log($"[HotUpdateManager] Download completed: {successCount} tasks succeeded");
        }
        
        private void CompleteUpdate()
        {
            if (LatestManifest != null)
            {
                resourceManager.UpdateManifest(LatestManifest);
                currentVersion = LatestManifest.version;
                PlayerPrefs.SetString("CurrentVersion", currentVersion);
                PlayerPrefs.Save();
            }
            
            SetState(HotUpdateState.Complete);
            OnStateChanged?.Invoke(this, HotUpdateEventArgs.UpdateComplete());
            OnUpdateComplete?.Invoke(this, EventArgs.Empty);
            
            Debug.Log($"[HotUpdateManager] Update complete, version: {currentVersion}");
        }
        
        public IEnumerator InitializeAsync()
        {
            string savedVersion = PlayerPrefs.GetString("CurrentVersion", currentVersion);
            currentVersion = savedVersion;
            
            resourceManager.Initialize(config);
            
            SetState(HotUpdateState.Idle);
            Debug.Log($"[HotUpdateManager] Initialized, current version: {currentVersion}");
            
            yield return null;
        }
        
        public void SetState(HotUpdateState newState)
        {
            if (State != newState)
            {
                State = newState;
                Debug.Log($"[HotUpdateManager] State: {State}");
            }
        }
        
        public void PauseDownload()
        {
            downloadManager?.Pause();
        }
        
        public void ResumeDownload()
        {
            downloadManager?.Resume();
        }
        
        public void CancelDownload()
        {
            downloadManager?.CancelAll();
            SetState(HotUpdateState.Idle);
        }
        
        public void UpdateManifest(ManifestData newManifest)
        {
            resourceManager?.UpdateManifest(newManifest);
        }
        
        public IEnumerator LoadAssetAsync<T>(string bundleName, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            yield return resourceManager.LoadAssetAsync(bundleName, assetName, callback);
        }
        
        public IEnumerator LoadSceneAsync(string bundleName, string sceneName, Action<bool> callback)
        {
            yield return resourceManager.LoadBundleAsync(bundleName);
            
            var bundle = resourceManager.GetLoadedBundle(bundleName);
            if (bundle == null)
            {
                callback?.Invoke(false);
                yield break;
            }
            
#if UNITY_ASSET_BUNDLE
            var ab = bundle as AssetBundle;
            if (ab != null)
            {
                var scenes = ab.GetAllScenePaths();
                bool found = false;
                foreach (var scene in scenes)
                {
                    if (scene.Contains(sceneName))
                    {
                        found = true;
                        break;
                    }
                }
                callback?.Invoke(found);
            }
#else
            callback?.Invoke(true);
#endif
        }
        
        public void UnloadBundle(string bundleName)
        {
            resourceManager?.UnloadBundle(bundleName);
        }
        
        public void UnloadAllBundles()
        {
            resourceManager?.UnloadAllBundles();
        }
        
        public bool IsUpdateAvailable()
        {
            return LatestManifest != null && IsNewerVersion(LatestVersion, currentVersion);
        }
        
        public string GetReleaseNotes()
        {
            return LatestVersionFile?.releaseNotes ?? "";
        }
        
        public bool IsForceUpdate()
        {
            return LatestVersionFile?.forceUpdate ?? false;
        }
        
        internal void RaiseUpdateAvailable(bool available)
        {
            OnUpdateAvailable?.Invoke(this, available);
        }
        
        internal void RaiseStateChanged(HotUpdateEventArgs args)
        {
            OnStateChanged?.Invoke(this, args);
        }
        
        internal void RaiseUpdateComplete()
        {
            OnUpdateComplete?.Invoke(this, EventArgs.Empty);
        }
        
        internal void RaiseUpdateFailed(string error)
        {
            OnUpdateFailed?.Invoke(this, error);
        }
    }
}
