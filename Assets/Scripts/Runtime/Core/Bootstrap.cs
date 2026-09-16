using UnityEngine;
using UnityEngine.UI;
using HotUpdate;

#if UNITY_EDITOR
using UnityEditor;
#endif

public class Bootstrap : SingletonBase<Bootstrap>
{

    [Header("Bootstrap Settings")]
    public bool autoStartGame;
    public string defaultGameMode = "Classic";

#if UNITY_EDITOR
    public bool enableDebugMode;

    [Header("Debug Mock Data (Editor Only)")]
    public bool mockPollutionOnStart;
    [Range(0f, 100f)] public float mockPollutionLevel = 95f;
    public bool mockFragmentOnStart;
#endif

    [Header("Hot Update Settings")]
    public bool enableHotUpdate = true;
    public bool autoCheckHotUpdate = true;
    public string hotUpdateBaseUrl = "http://127.0.0.1:8888/";

    protected override void Awake()
    {
        base.Awake();

        EnsureEssentialComponents();
        InitializeAllSystems();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (this == Instance)
        {
            StopAllCoroutines();
            CancelInvoke();
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (this == Instance)
        {
            StopAllCoroutines();
            CancelInvoke();
        }
    }

    private void EnsureEssentialComponents()
    {
        EnsureCamera();
        EnsureCanvas();
        EnsureEventSystem();
        OptimizeForMobile();
    }

    private void OptimizeForMobile()
    {
        Application.targetFrameRate = 60;
        QualitySettings.vSyncCount = 0;
        ParasiteTowerArtOptimization.ApplyQualityForPlatform();
        QualitySettings.shadowDistance = 0;
        QualitySettings.shadowCascades = 0;

        Screen.orientation = ScreenOrientation.Portrait;
        Screen.autorotateToPortrait = true;
        Screen.autorotateToPortraitUpsideDown = false;
        Screen.autorotateToLandscapeLeft = false;
        Screen.autorotateToLandscapeRight = false;
    }

    private void EnsureCamera()
    {
        // Destroy all existing cameras first to avoid conflicts
        var existingCameras = FindObjectsOfType<Camera>();
        foreach (var cam in existingCameras)
        {
            Destroy(cam.gameObject);
        }

        GameObject cameraObj = new GameObject("Main Camera");
        cameraObj.tag = "MainCamera";
        Camera camera = cameraObj.AddComponent<Camera>();
        camera.orthographic = true;
        camera.orthographicSize = 5;
        camera.clearFlags = CameraClearFlags.SolidColor;
        camera.backgroundColor = ParasiteTowerColorScheme.NearBlack;
        camera.cullingMask = 0; // Don't render any 3D - UI is overlay
        DontDestroyOnLoad(cameraObj);
    }

    private void EnsureCanvas()
    {
        // Destroy scene canvases to avoid overlap, but preserve CanvasUIManager's canvas
        var existingCanvases = FindObjectsOfType<Canvas>();
        foreach (var c in existingCanvases)
        {
            // Don't destroy CanvasUIManager's canvas
            if (c.gameObject.name == "GameUI") continue;
            Destroy(c.gameObject);
        }

        // Bootstrap canvas not needed - CanvasUIManager creates its own
    }

    private void EnsureEventSystem()
    {
        UnityEngine.EventSystems.EventSystem eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            GameObject eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
            DontDestroyOnLoad(eventSystemObj);
        }
    }

    private void InitializeAllSystems()
    {
        GameDataInitializer.EnsureAllDataExists();

        EnsureManager<GameManager>();
        EnsureManager<ConfigManager>();
        EnsureManager<DataConfigManager>();
        EnsureManager<SaveSystem>();
        EnsureManager<ModuleUnlockSystem>();
        EnsureManager<SceneLoader>();

        if (PlayerPrefs.GetInt("FirstRunComplete", 0) == 0)
        {
            var unlockSystem = ModuleUnlockSystem.Instance;
            if (unlockSystem != null && unlockSystem.IsAnyPeripheralModuleUnlocked())
            {
                unlockSystem.ResetAllUnlocks();
                PlayerPrefs.Save();
            }
        }

        EnsureManager<TraitManager>();
        EnsureManager<CurseManager>();
        EnsureManager<AchievementManager>();
        EnsureManager<TutorialManager>();
        EnsureManager<NegotiateManager>();
        EnsureManager<ShopManager>();
        EnsureManager<RewardManager>();
        EnsureManager<FloorManager>();
        EnsureManager<SpecialFloorSystem>();
        EnsureManager<PollutionSystem>();
        EnsureManager<FormManager>();
        EnsureManager<FragmentManager>();
        EnsureManager<ClassManager>();
        EnsureManager<CombatManager>();
        EnsureManager<StorySystem>();
        EnsureManager<AnchorSystem>();
        EnsureManager<MetaProgressSystem>();
        EnsureManager<BuildAxesSystem>();
        EnsureManager<LegacyManager>();
        EnsureManager<FormResonanceSystem>();
        EnsureManager<CorruptionSkillManager>();
        EnsureManager<FloorEventManager>();
        EnsureManager<BossAIManager>();

        EnsureManager<AudioManager>();
        EnsureManager<VFXManager>();

        EnsureManager<ParasiteTowerGame>();
        EnsureManager<CompleteGameSystem>();
        EnsureManager<CanvasUIManager>();
        EnsureManager<ShaderMaterialManager>();
        EnsureManager<ScreenEffectsManager>();
        EnsureManager<UIAnimationSystem>();
        EnsureManager<UIStyleSystem>();
        EnsureManager<PerformanceManager>();
        EnsureManager<DamageNumberPool>();
        EnsureManager<PossessionCinematic>();

        InitializeHotUpdate();

#if UNITY_EDITOR
        if (enableDebugMode)
        {
            Debug.Log("[Bootstrap] Debug mode enabled. Use Ctrl+Alt+P to toggle mock pollution.");
        }
#endif

        if (autoStartGame)
        {
            Invoke(nameof(StartGameDelayed), 0.5f);
        }
    }

    private void StartGameDelayed()
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame(defaultGameMode);

#if UNITY_EDITOR
            if (mockPollutionOnStart && GameManager.Instance.Player != null)
            {
                GameManager.Instance.Player.pollution = mockPollutionLevel;
                Debug.Log($"[Bootstrap] Mock pollution set to {mockPollutionLevel}%");
            }

            if (mockFragmentOnStart)
            {
                Invoke(nameof(ShowMockFragment), 1.5f);
            }
#endif
        }
    }

#if UNITY_EDITOR
    void ShowMockFragment()
    {
        var gs = CompleteGameSystem.Instance;
        if (gs != null)
        {
            gs.DebugShowFragChoice();
        }
    }
#endif

#if UNITY_EDITOR
    private void Update()
    {
        if (!enableDebugMode) return;

        if (Input.GetKey(KeyCode.LeftControl) && Input.GetKey(KeyCode.LeftAlt))
        {
            if (Input.GetKeyDown(KeyCode.Alpha1)) SetMockPollution(30f);
            else if (Input.GetKeyDown(KeyCode.Alpha2)) SetMockPollution(50f);
            else if (Input.GetKeyDown(KeyCode.Alpha3)) SetMockPollution(70f);
            else if (Input.GetKeyDown(KeyCode.Alpha4)) SetMockPollution(85f);
            else if (Input.GetKeyDown(KeyCode.Alpha5)) SetMockPollution(95f);
            else if (Input.GetKeyDown(KeyCode.Alpha0)) SetMockPollution(0f);
            else if (Input.GetKeyDown(KeyCode.Alpha6)) ShowMockFragment();
            else if (Input.GetKeyDown(KeyCode.Alpha7)) CheckHotUpdateManual();
        }
    }

    [ContextMenu("Set Pollution/Mock 95%")]
    void SetMockPollution95() => SetMockPollution(95f);

    [ContextMenu("Set Pollution/Mock 85%")]
    void SetMockPollution85() => SetMockPollution(85f);

    [ContextMenu("Set Pollution/Mock 70%")]
    void SetMockPollution70() => SetMockPollution(70f);

    [ContextMenu("Set Pollution/Clear (0%)")]
    void ClearMockPollution() => SetMockPollution(0f);

    [ContextMenu("UI/Show Fragment Choice (Mock)")]
    void ContextShowMockFragment() => ShowMockFragment();

    void SetMockPollution(float value)
    {
        if (GameManager.Instance == null || GameManager.Instance.Player == null)
        {
            Debug.LogWarning("[Bootstrap] Cannot set pollution - GameManager or Player not ready.");
            return;
        }
        GameManager.Instance.Player.pollution = value;
        Debug.Log($"[Bootstrap] Pollution set to {value}%");
    }

    [ContextMenu("HotUpdate/Check For Update")]
    void ContextCheckHotUpdate()
    {
        CheckHotUpdateManual();
    }

    [ContextMenu("HotUpdate/Set Base URL (Local Mock)")]
    void ContextSetMockUrl()
    {
        hotUpdateBaseUrl = "http://127.0.0.1:8888/";
        ApplyHotUpdateConfig();
        Debug.Log("[Bootstrap] HotUpdate base URL set to local mock server");
    }

    [ContextMenu("HotUpdate/Set Base URL (Production)")]
    void ContextSetProductionUrl()
    {
        hotUpdateBaseUrl = "https://your-server.com/bundles/";
        ApplyHotUpdateConfig();
        Debug.Log("[Bootstrap] HotUpdate base URL set to production");
    }

    [ContextMenu("HotUpdate/Simulate Hot Update (Local)")]
    void ContextSimulateHotUpdate()
    {
        StartCoroutine(SimulateHotUpdateCoroutine());
    }

    System.Collections.IEnumerator SimulateHotUpdateCoroutine()
    {
        Debug.Log("========================================");
        Debug.Log("  Simulating Hot Update (Local Mode)");
        Debug.Log("========================================");

        var manager = HotUpdate.HotUpdateManager.Instance;
        if (manager == null)
        {
            var go = new GameObject("HotUpdateManager");
            manager = go.AddComponent<HotUpdate.HotUpdateManager>();
        }

        var config = LoadHotUpdateConfig();
        config.baseUrl = hotUpdateBaseUrl;
        config.enabled = true;

        yield return manager.InitializeAsync();
        manager.SetConfig(config);

        // Simulate successful version check
        Debug.Log("[Simulate] Simulating version check...");
        manager.SetState(HotUpdate.HotUpdateState.Checking);
        
        var versionFile = new HotUpdate.VersionFile
        {
            version = "1.1.0",
            minVersion = "1.0.0",
            forceUpdate = false,
            releaseNotes = "v1.1.0: 模拟热更新测试",
            timestamp = 1725408000
        };

        // Use reflection to set internal state
        var versionFileProp = manager.GetType().GetProperty("LatestVersionFile", 
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        var latestVersionProp = manager.GetType().GetProperty("LatestVersion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

        if (versionFileProp != null) versionFileProp.SetValue(manager, versionFile);
        if (latestVersionProp != null) latestVersionProp.SetValue(manager, "1.1.0");

        Debug.Log("[Simulate] New version 1.1.0 detected!");
        manager.RaiseUpdateAvailable(true);
        manager.SetState(HotUpdate.HotUpdateState.Idle);

        // Simulate manifest download
        Debug.Log("[Simulate] Simulating manifest download...");
        var manifest = new HotUpdate.ManifestData
        {
            version = "1.1.0",
            timestamp = 1725408000,
            bundles = new System.Collections.Generic.List<HotUpdate.BundleInfo>
            {
                new HotUpdate.BundleInfo { name = "test_assets.bundle", size = 51, hash = "897b11172a7b8bad8859801117c7e9c7", version = "1.1.0" },
                new HotUpdate.BundleInfo { name = "game_configs.bundle", size = 52, hash = "413c2078e136dbb472c4908ab3e99f03", version = "1.1.0" }
            }
        };

        var manifestProp = manager.GetType().GetProperty("LatestManifest",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (manifestProp != null) manifestProp.SetValue(manager, manifest);

        manager.SetState(HotUpdate.HotUpdateState.Downloading);

        // Simulate download progress
        Debug.Log("[Simulate] Simulating bundle downloads...");
        for (int i = 0; i < manifest.bundles.Count; i++)
        {
            var bundle = manifest.bundles[i];
            Debug.Log($"[Simulate] Downloading: {bundle.name} ({bundle.size} bytes)...");
            
            // Write a dummy file to simulate download
            string localPath = System.IO.Path.Combine(
                UnityEngine.Application.persistentDataPath, 
                config.localBundlePath, 
                bundle.name);
            string dir = System.IO.Path.GetDirectoryName(localPath);
            if (!System.IO.Directory.Exists(dir)) System.IO.Directory.CreateDirectory(dir);
            System.IO.File.WriteAllText(localPath, $"Simulated bundle: {bundle.name} v{bundle.version}");
            
            yield return new WaitForSeconds(0.3f);
        }

        // Complete update
        Debug.Log("[Simulate] Completing update...");
        manager.UpdateManifest(manifest);
        
        // Use reflection to update current version
        var currentVersionField = manager.GetType().GetField("currentVersion",
            System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
        if (currentVersionField != null) currentVersionField.SetValue(manager, "1.1.0");
        
        UnityEngine.PlayerPrefs.SetString("CurrentVersion", "1.1.0");
        UnityEngine.PlayerPrefs.Save();

        manager.SetState(HotUpdate.HotUpdateState.Complete);
        manager.RaiseStateChanged(HotUpdate.HotUpdateEventArgs.UpdateComplete());
        manager.RaiseUpdateComplete();

        Debug.Log("========================================");
        Debug.Log("  Simulated Hot Update Complete!");
        Debug.Log("  Version: 1.0.0 → 1.1.0");
        Debug.Log("  Bundles: 2 downloaded");
        Debug.Log("========================================");
    }

    void InitializeHotUpdate()
    {
        if (!enableHotUpdate)
        {
            Debug.Log("[Bootstrap] Hot Update disabled");
            return;
        }

        if (autoCheckHotUpdate)
        {
            var initializer = FindObjectOfType<HotUpdateInitializer>();
            if (initializer == null)
            {
                GameObject go = new GameObject("HotUpdateInitializer");
                initializer = go.AddComponent<HotUpdateInitializer>();
                DontDestroyOnLoad(go);
            }
            
            initializer.autoCheckOnStart = false;
            
            var config = LoadHotUpdateConfig();
            if (config != null)
            {
                config.baseUrl = hotUpdateBaseUrl;
                initializer.SetConfig(config);
            }
            initializer.ManualCheckForUpdate();
        }

        Debug.Log($"[Bootstrap] Hot Update initialized (baseUrl: {hotUpdateBaseUrl})");
    }
    
    HotUpdateConfig LoadHotUpdateConfig()
    {
        var config = new HotUpdateConfig();
        
        var configAsset = Resources.Load<TextAsset>("Configs/HotUpdateConfig");
        if (configAsset != null)
        {
            try
            {
                config = JsonUtility.FromJson<HotUpdateConfig>(configAsset.text);
                Debug.Log("[Bootstrap] HotUpdate config loaded from Resources");
            }
            catch (System.Exception e)
            {
                Debug.LogWarning($"[Bootstrap] Failed to parse HotUpdate config: {e.Message}");
            }
        }
        else
        {
            Debug.Log("[Bootstrap] HotUpdate config not found, using defaults");
            config.enabled = enableHotUpdate;
        }
        
        return config;
    }

    void ApplyHotUpdateConfig()
    {
        var config = LoadHotUpdateConfig();
        config.baseUrl = hotUpdateBaseUrl;

        var initializer = FindObjectOfType<HotUpdateInitializer>();
        if (initializer != null)
        {
            initializer.SetConfig(config);
        }
    }

    void CheckHotUpdateManual()
    {
        ApplyHotUpdateConfig();
        var initializer = FindObjectOfType<HotUpdateInitializer>();
        if (initializer != null)
        {
            initializer.ManualCheckForUpdate();
            Debug.Log("[Bootstrap] Manual hot update check triggered");
        }
        else
        {
            Debug.LogWarning("[Bootstrap] HotUpdateInitializer not found. Initializing...");
            InitializeHotUpdate();
        }
    }
#endif

    private void EnsureManager<T>() where T : MonoBehaviour
    {
        var existing = GameSystemRegistry.Get<T>();
        if (existing == null)
        {
            existing = FindObjectOfType<T>();
            if (existing == null)
            {
                GameObject go = new GameObject(typeof(T).Name);
                existing = go.AddComponent<T>();
                DontDestroyOnLoad(go);
            }
            GameSystemRegistry.Register(existing);
        }
    }

    public void StartGame(string mode)
    {
        if (GameManager.Instance != null)
        {
            GameManager.Instance.StartNewGame(mode);
        }
    }
}
