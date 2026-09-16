using UnityEngine;
using System;

public enum Module
{
    MainMenu,
    ShortMode,
    ClassicMode,
    ExpeditionMode,
    Achievements,
    Bestiary,
    Archive,
    EchoAltar,
    Leaderboard,
    Settings,
    Shop,
    DailyReward,
    ChallengeMode
}

public class ModuleUnlockSystem : SingletonBase<ModuleUnlockSystem>
{

    private const string MigrationKey = "pt_migration_v1";

    protected override void Awake()
    {
        base.Awake();
        MigrateLegacyData();
        InitializeDefaultUnlocks();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private void MigrateLegacyData()
    {
        if (PlayerPrefs.GetInt(MigrationKey, 0) == 1)
            return;

        PlayerPrefs.SetInt(MigrationKey, 1);
        PlayerPrefs.Save();
    }

    private const string UnlockKeyPrefix = "pt_unlock_";

    public bool IsModuleUnlocked(Module module)
    {
        string key = UnlockKeyPrefix + module.ToString();
        return PlayerPrefs.GetInt(key, 0) == 1;
    }

    public void UnlockModule(Module module)
    {
        string key = UnlockKeyPrefix + module.ToString();
        PlayerPrefs.SetInt(key, 1);
        PlayerPrefs.Save();
        EventBus.Emit(EventTypes.ModuleUnlocked, module);
    }

    public void LockModule(Module module)
    {
        string key = UnlockKeyPrefix + module.ToString();
        PlayerPrefs.SetInt(key, 0);
        PlayerPrefs.Save();
    }

    public void UnlockModules(params Module[] modules)
    {
        foreach (var module in modules)
        {
            UnlockModule(module);
        }
    }

    public void ResetAllUnlocks()
    {
        foreach (Module module in Enum.GetValues(typeof(Module)))
        {
            LockModule(module);
        }
        InitializeDefaultUnlocks();
    }

    public void InitializeDefaultUnlocks()
    {
        UnlockModule(Module.MainMenu);
        UnlockModule(Module.ShortMode);
    }

    public bool IsAnyPeripheralModuleUnlocked()
    {
        return IsModuleUnlocked(Module.Achievements) ||
               IsModuleUnlocked(Module.Bestiary) ||
               IsModuleUnlocked(Module.Archive) ||
               IsModuleUnlocked(Module.EchoAltar) ||
               IsModuleUnlocked(Module.Leaderboard) ||
               IsModuleUnlocked(Module.Shop) ||
               IsModuleUnlocked(Module.DailyReward) ||
               IsModuleUnlocked(Module.ChallengeMode) ||
               IsModuleUnlocked(Module.ClassicMode);
    }

    [ContextMenu("Log Unlock Status")]
    public void LogUnlockStatus()
    {
        Debug.Log("=== Module Unlock Status ===");
        foreach (Module module in Enum.GetValues(typeof(Module)))
        {
            Debug.Log($"{module}: {IsModuleUnlocked(module)}");
        }
        Debug.Log($"IsAnyPeripheralModuleUnlocked: {IsAnyPeripheralModuleUnlocked()}");
    }

    [ContextMenu("Reset All Unlocks")]
    public void DebugResetAllUnlocks()
    {
        PlayerPrefs.DeleteAll();
        PlayerPrefs.Save();
        Debug.Log("All PlayerPrefs cleared!");
    }
}