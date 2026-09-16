using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public static class GameDataInitializer
{
    private static bool _initialized;

    public static void EnsureAllDataExists()
    {
        if (_initialized) return;
        _initialized = true;

        try
        {
            EnsureGameConfigExists();
            EnsureMonstersExist();
            EnsureClassesExist();
            EnsureFormsExist();
            EnsureFragmentsExist();
            EnsureShopItemsExist();
            EnsureAchievementsExist();

            Debug.Log("[GameDataInitializer] All data assets verified");
        }
        catch (Exception e)
        {
            Debug.LogError($"[GameDataInitializer] Data initialization failed: {e.Message}");
        }
    }

    public static GameConfig GetOrCreateGameConfig()
    {
        var config = Resources.Load<GameConfig>("Data/GameConfig");
        if (config == null)
        {
            Debug.LogWarning("[GameDataInitializer] GameConfig not found, creating default");
            config = ScriptableObject.CreateInstance<GameConfig>();
            SetDefaultGameConfig(config);
        }
        return config;
    }

    private static void EnsureGameConfigExists()
    {
        GetOrCreateGameConfig();
    }

    private static void SetDefaultGameConfig(GameConfig config)
    {
        config.startingHp = 100;
        config.startingAttack = 10;
        config.startingDefense = 5;
        config.maxFragmentSlots = 4;
        config.baseCritChance = 0.15f;
        config.critMultiplier = 1.5f;
        config.defendMultiplier = 2f;
        config.fleeBaseChance = 0.5f;
        config.pollutionGainPerTurn = 2f;
        config.pollutionDecayPerFloor = 5f;
        config.pollutionThreshold = 50f;
        config.burstModeMultiplier = 1.5f;
        config.classicModeFloorCount = 50;
        config.shortModeFloorCount = 12;
        config.expeditionModeFloorCount = 20;
        config.enemyHpScaling = 0.05f;
        config.enemyAttackScaling = 0.05f;
        config.goldScaling = 0.03f;
    }

    private static void EnsureMonstersExist()
    {
        var firstMonster = Resources.Load<MonsterData>("Data/Monsters/Slime");
        if (firstMonster == null)
        {
            Debug.LogWarning("[GameDataInitializer] No monster data found - runtime monster data must be provided");
        }
    }

    private static void EnsureClassesExist()
    {
        var firstClass = Resources.Load<ClassData>("Data/Classes/Warrior");
        if (firstClass == null)
        {
            Debug.LogWarning("[GameDataInitializer] No class data found - runtime class data must be provided");
        }
    }

    private static void EnsureFormsExist()
    {
        var firstForm = Resources.Load<FormData>("Data/Forms/Normal Form");
        if (firstForm == null)
        {
            Debug.LogWarning("[GameDataInitializer] No form data found - runtime form data must be provided");
        }
    }

    private static void EnsureFragmentsExist()
    {
        var firstFragment = Resources.Load<FragmentData>("Data/Fragments/Strength Shard");
        if (firstFragment == null)
        {
            Debug.LogWarning("[GameDataInitializer] No fragment data found - runtime fragment data must be provided");
        }
    }

    private static void EnsureShopItemsExist()
    {
        var firstItem = Resources.Load<ShopItemData>("Data/ShopItems/Health Potion");
        if (firstItem == null)
        {
            Debug.LogWarning("[GameDataInitializer] No shop item data found - runtime shop data must be provided");
        }
    }

    private static void EnsureAchievementsExist()
    {
        var firstAchievement = Resources.Load<AchievementData>("Data/Achievements/First Blood");
        if (firstAchievement == null)
        {
            Debug.LogWarning("[GameDataInitializer] No achievement data found - runtime achievement data must be provided");
        }
    }

    public static void ResetInitialization()
    {
        _initialized = false;
    }
}