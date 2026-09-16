using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System;

namespace HotUpdate
{
    public class ResourceManager : MonoBehaviour
    {
        public static ResourceManager Instance { get; private set; }
        
        private Dictionary<string, object> loadedBundles = new Dictionary<string, object>();
        private Dictionary<string, UnityEngine.Object> loadedAssets = new Dictionary<string, UnityEngine.Object>();
        private ManifestData currentManifest;
        private HotUpdateConfig config;
        
        public event EventHandler<string> OnBundleLoaded;
        public event EventHandler<string> OnBundleUnloaded;
        
        void Awake()
        {
            if (Instance != null && Instance != this) { Destroy(gameObject); return; }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        
        public void Initialize(HotUpdateConfig configuration)
        {
            config = configuration;
            LoadLocalManifest();
        }
        
        private void LoadLocalManifest()
        {
            string localManifestPath = GetLocalManifestPath();
            if (File.Exists(localManifestPath))
            {
                string json = File.ReadAllText(localManifestPath);
                currentManifest = JsonUtility.FromJson<ManifestData>(json);
                Debug.Log($"[ResourceManager] Loaded local manifest, version: {currentManifest?.version}");
            }
            else
            {
                Debug.Log("[ResourceManager] No local manifest found");
            }
        }
        
        public void UpdateManifest(ManifestData newManifest)
        {
            currentManifest = newManifest;
            SaveLocalManifest(newManifest);
        }
        
        private void SaveLocalManifest(ManifestData manifest)
        {
            string localPath = GetLocalManifestPath();
            string directory = Path.GetDirectoryName(localPath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }
            
            string json = JsonUtility.ToJson(manifest, true);
            File.WriteAllText(localPath, json);
            Debug.Log($"[ResourceManager] Manifest saved to: {localPath}");
        }
        
        private string GetLocalManifestPath()
        {
            return Path.Combine(Application.persistentDataPath, config?.localBundlePath ?? "Bundles", config?.manifestFileName ?? "manifest.json");
        }
        
        private string GetLocalBundlePath(string bundleName)
        {
            return Path.Combine(Application.persistentDataPath, config?.localBundlePath ?? "Bundles", bundleName);
        }
        
        private string GetStreamingBundlePath(string bundleName)
        {
            return Path.Combine(Application.streamingAssetsPath, config?.localBundlePath ?? "Bundles", bundleName);
        }
        
        public IEnumerator LoadBundleAsync(string bundleName)
        {
            if (loadedBundles.ContainsKey(bundleName))
            {
                yield break;
            }
            
            string localPath = GetLocalBundlePath(bundleName);
            string streamingPath = GetStreamingBundlePath(bundleName);
            
            string bundlePath = null;
            if (File.Exists(localPath))
            {
                bundlePath = localPath;
            }
            else if (File.Exists(streamingPath))
            {
                bundlePath = streamingPath;
            }
            
            if (string.IsNullOrEmpty(bundlePath))
            {
                Debug.LogError($"[ResourceManager] Bundle not found: {bundleName}");
                yield break;
            }
            
#if UNITY_ASSET_BUNDLE
            using (var request = AssetBundle.LoadFromFileAsync(bundlePath))
            {
                yield return request;
                
                if (request.assetBundle == null)
                {
                    Debug.LogError($"[ResourceManager] Failed to load bundle: {bundleName}");
                    yield break;
                }
                
                loadedBundles[bundleName] = request.assetBundle;
                OnBundleLoaded?.Invoke(this, bundleName);
                Debug.Log($"[ResourceManager] Bundle loaded: {bundleName}");
            }
#else
            Debug.LogWarning("[ResourceManager] AssetBundle support disabled, using Resources fallback");
            loadedBundles[bundleName] = true;
            OnBundleLoaded?.Invoke(this, bundleName);
#endif
        }
        
        public IEnumerator LoadAssetAsync<T>(string bundleName, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            yield return LoadBundleAsync(bundleName);
            
            if (!loadedBundles.ContainsKey(bundleName))
            {
                callback?.Invoke(null);
                yield break;
            }
            
            if (loadedAssets.ContainsKey($"{bundleName}/{assetName}"))
            {
                callback?.Invoke(loadedAssets[$"{bundleName}/{assetName}"] as T);
                yield break;
            }
            
#if UNITY_ASSET_BUNDLE
            var bundle = loadedBundles[bundleName] as AssetBundle;
            if (bundle != null)
            {
                using (var request = bundle.LoadAssetAsync<T>(assetName))
                {
                    yield return request;
                    
                    if (request.asset != null)
                    {
                        loadedAssets[$"{bundleName}/{assetName}"] = request.asset;
                        callback?.Invoke(request.asset as T);
                    }
                    else
                    {
                        Debug.LogError($"[ResourceManager] Failed to load asset: {assetName} from {bundleName}");
                        callback?.Invoke(null);
                    }
                }
            }
#else
            T asset = Resources.Load<T>(assetName);
            if (asset != null)
            {
                loadedAssets[$"{bundleName}/{assetName}"] = asset;
                callback?.Invoke(asset);
            }
            else
            {
                Debug.LogError($"[ResourceManager] Failed to load asset: {assetName} from {bundleName}");
                callback?.Invoke(null);
            }
#endif
        }
        
        public IEnumerator LoadAssetWithDependenciesAsync<T>(string bundleName, string assetName, Action<T> callback) where T : UnityEngine.Object
        {
            yield return LoadAssetAsync(bundleName, assetName, callback);
        }
        
        public void UnloadBundle(string bundleName, bool unloadAllLoadedAssets = true)
        {
            if (loadedBundles.ContainsKey(bundleName))
            {
#if UNITY_ASSET_BUNDLE
                var bundle = loadedBundles[bundleName] as AssetBundle;
                if (bundle != null)
                {
                    bundle.Unload(unloadAllLoadedAssets);
                }
#endif
                loadedBundles.Remove(bundleName);
                OnBundleUnloaded?.Invoke(this, bundleName);
                Debug.Log($"[ResourceManager] Bundle unloaded: {bundleName}");
            }
        }
        
        public void UnloadAllBundles()
        {
#if UNITY_ASSET_BUNDLE
            foreach (var bundle in loadedBundles.Values)
            {
                var ab = bundle as AssetBundle;
                if (ab != null)
                {
                    ab.Unload(true);
                }
            }
#endif
            loadedBundles.Clear();
            loadedAssets.Clear();
            Debug.Log("[ResourceManager] All bundles unloaded");
        }
        
        public bool IsBundleLoaded(string bundleName)
        {
            return loadedBundles.ContainsKey(bundleName);
        }
        
        public object GetLoadedBundle(string bundleName)
        {
            return loadedBundles.ContainsKey(bundleName) ? loadedBundles[bundleName] : null;
        }
        
        public ManifestData GetCurrentManifest()
        {
            return currentManifest;
        }
        
        public string GetCurrentVersion()
        {
            return currentManifest?.version ?? "0.0.0";
        }
        
        public void ClearCache()
        {
            string localPath = Path.Combine(Application.persistentDataPath, config?.localBundlePath ?? "Bundles");
            if (Directory.Exists(localPath))
            {
                Directory.Delete(localPath, true);
                Debug.Log("[ResourceManager] Cache cleared");
            }
            loadedAssets.Clear();
        }
    }
}
