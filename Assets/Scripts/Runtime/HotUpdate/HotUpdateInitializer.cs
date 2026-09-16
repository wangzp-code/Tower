using UnityEngine;
using System.Collections;

namespace HotUpdate
{
    public class HotUpdateInitializer : SingletonBase<HotUpdateInitializer>
    {
        public bool autoCheckOnStart = true;
        public string versionConfigPath = "Configs/HotUpdateConfig";
        
        public HotUpdateConfig config;
        
        protected override void Awake()
        {
            base.Awake();
            
            if (config == null)
            {
                config = LoadConfig();
            }
            
            if (autoCheckOnStart)
            {
                StartCoroutine(InitializeAndCheck());
            }
        }
        
        private HotUpdateConfig LoadConfig()
        {
            var config = new HotUpdateConfig();
            
            var configAsset = Resources.Load<TextAsset>(versionConfigPath);
            if (configAsset != null)
            {
                try
                {
                    config = JsonUtility.FromJson<HotUpdateConfig>(configAsset.text);
                    Debug.Log("[HotUpdateInitializer] Config loaded from Resources");
                }
                catch (System.Exception e)
                {
                    Debug.LogWarning($"[HotUpdateInitializer] Failed to parse config: {e.Message}, using defaults");
                }
            }
            else
            {
                Debug.Log("[HotUpdateInitializer] Using default config");
            }
            
            return config;
        }
        
        private IEnumerator InitializeAndCheck()
        {
            var hotUpdateManager = HotUpdateManager.Instance;
            if (hotUpdateManager == null)
            {
                var managerGo = new GameObject("HotUpdateManager");
                hotUpdateManager = managerGo.AddComponent<HotUpdateManager>();
            }
            
            yield return hotUpdateManager.InitializeAsync();
            
            if (config != null)
            {
                hotUpdateManager.SetConfig(config);
            }
            
            if (config != null && !config.enabled)
            {
                Debug.Log("[HotUpdateInitializer] HotUpdate is disabled, skipping check");
                yield break;
            }
            
            Debug.Log("[HotUpdateInitializer] Checking for updates...");
            yield return hotUpdateManager.CheckForUpdate();
        }
        
        public void ManualCheckForUpdate()
        {
            StartCoroutine(InitializeAndCheck());
        }
        
        public void SetConfig(HotUpdateConfig newConfig)
        {
            config = newConfig;
        }
        
        public void SetBaseUrl(string url)
        {
            if (config == null) config = new HotUpdateConfig();
            config.baseUrl = url;
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            StopAllCoroutines();
        }
    }
}
