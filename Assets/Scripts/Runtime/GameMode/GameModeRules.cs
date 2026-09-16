using UnityEngine;
using System.Collections.Generic;

public struct ModeRuleSet
{
    public GameMode modeType;
    public string name;
    public string description;
    public bool hasTimer;
    public int defaultTimer;
    public bool pollutionEnabled;
    public bool collapseEnabled;
    public bool floorRewards;
    public bool comboSystem;
    public float evoMultiplier;
    public float pollutionMultiplier;
}

public static class GameModeRules
{
    public static readonly Dictionary<GameMode, ModeRuleSet> Rules = new Dictionary<GameMode, ModeRuleSet>
    {
        {
            GameMode.Short,
            new ModeRuleSet
            {
                modeType = GameMode.Short,
                name = "短局模式",
                description = "限时挑战模式，追求高分快速通关",
                hasTimer = true,
                defaultTimer = 900,
                pollutionEnabled = true,
                collapseEnabled = true,
                floorRewards = true,
                comboSystem = true,
                evoMultiplier = 1.5f,
                pollutionMultiplier = 1.2f
            }
        },
        {
            GameMode.Classic,
            new ModeRuleSet
            {
                modeType = GameMode.Classic,
                name = "经典模式",
                description = "传统无尽模式，挑战更高楼层",
                hasTimer = false,
                defaultTimer = 0,
                pollutionEnabled = true,
                collapseEnabled = true,
                floorRewards = true,
                comboSystem = true,
                evoMultiplier = 1.0f,
                pollutionMultiplier = 1.0f
            }
        },
        {
            GameMode.Expedition,
            new ModeRuleSet
            {
                modeType = GameMode.Expedition,
                name = "远征模式",
                description = "阶段性挑战模式，解锁特殊奖励",
                hasTimer = true,
                defaultTimer = 1800,
                pollutionEnabled = true,
                collapseEnabled = false,
                floorRewards = true,
                comboSystem = true,
                evoMultiplier = 1.2f,
                pollutionMultiplier = 0.8f
            }
        }
    };

    public static ModeRuleSet GetCurrentModeRules()
    {
        GameMode currentMode = GameManager.Instance.CurrentMode;
        if (Rules.TryGetValue(currentMode, out ModeRuleSet rules))
        {
            return rules;
        }
        return Rules[GameMode.Classic];
    }

    public static bool IsTimerEnabled()
    {
        return GetCurrentModeRules().hasTimer;
    }

    public static int GetDefaultTimer()
    {
        return GetCurrentModeRules().defaultTimer;
    }

    public static bool IsPollutionEnabled()
    {
        return GetCurrentModeRules().pollutionEnabled;
    }

    public static bool IsCollapseEnabled()
    {
        return GetCurrentModeRules().collapseEnabled;
    }

    public static float GetEvolutionMultiplier()
    {
        return GetCurrentModeRules().evoMultiplier;
    }

    public static float GetPollutionMultiplier()
    {
        return GetCurrentModeRules().pollutionMultiplier;
    }

    public static int CalculateFloorReward(int floor)
    {
        ModeRuleSet rules = GetCurrentModeRules();
        if (!rules.floorRewards) return 0;

        int baseReward = 10;
        int floorBonus = floor * 5;
        int randomBonus = Random.Range(0, 10);

        return Mathf.FloorToInt((baseReward + floorBonus + randomBonus) * rules.evoMultiplier);
    }

    public static bool CanSkipFloors()
    {
        return GameManager.Instance.CurrentMode == GameMode.Expedition;
    }

    public static bool CheckModeEndCondition(int floor, int pollution, int timeRemaining)
    {
        ModeRuleSet rules = GetCurrentModeRules();

        if (rules.modeType == GameMode.Short)
        {
            if (timeRemaining <= 0) return true;
            if (pollution >= 100) return true;
        }

        if (rules.modeType == GameMode.Classic)
        {
            if (pollution >= 100) return true;
        }

        if (rules.modeType == GameMode.Expedition)
        {
            if (timeRemaining <= 0) return true;
            if (floor >= 50) return true;
        }

        return false;
    }

    public static Dictionary<string, object> GetModeStatistics(int floor, int timeElapsed, int evoPoints, int kills)
    {
        ModeRuleSet rules = GetCurrentModeRules();

        return new Dictionary<string, object>
        {
            { "mode", rules.name },
            { "floor", floor },
            { "timeElapsed", timeElapsed },
            { "evoPoints", evoPoints },
            { "kills", kills },
            { "efficiency", Mathf.RoundToInt((float)evoPoints / Mathf.Max(1, timeElapsed / 60)) }
        };
    }
}