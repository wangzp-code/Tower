using UnityEngine;
using System.Collections.Generic;

public struct ComboTier
{
    public int minCount;
    public string label;
    public float bonus;
    public int tier;
}

public static class ComboTiers
{
    public static readonly ComboTier[] Data = new ComboTier[]
    {
        new ComboTier { minCount = 1, label = "", bonus = 0f, tier = 1 },
        new ComboTier { minCount = 3, label = "连击!", bonus = 0.05f, tier = 2 },
        new ComboTier { minCount = 5, label = "猛攻!", bonus = 0.10f, tier = 2 },
        new ComboTier { minCount = 8, label = "狂暴连击!", bonus = 0.15f, tier = 3 },
        new ComboTier { minCount = 12, label = "毁灭风暴!", bonus = 0.25f, tier = 4 },
        new ComboTier { minCount = 20, label = "无尽杀戮!", bonus = 0.35f, tier = 5 }
    };
}

public class ComboSystem
{
    private int comboCount;
    private int lastComboTier;
    private CompleteGameSystem gameSystem;

    public int ComboCount => comboCount;
    public int LastComboTier => lastComboTier;

    public ComboSystem(CompleteGameSystem system)
    {
        gameSystem = system;
        comboCount = 0;
        lastComboTier = 0;
    }

    public ComboTier GetCurrentTier()
    {
        ComboTier tier = ComboTiers.Data[0];
        for (int i = ComboTiers.Data.Length - 1; i >= 0; i--)
        {
            if (comboCount >= ComboTiers.Data[i].minCount)
            {
                tier = ComboTiers.Data[i];
                break;
            }
        }
        return tier;
    }

    public float GetComboBonus()
    {
        float baseBonus = GetCurrentTier().bonus;
        if (baseBonus <= 0)
            return 0f;

        // 双核心耦合: 污染越高，形态共鸣越强
        float pollution = GameManager.Instance.Player.pollution / 100f;
        float pollutionMultiplier = pollution >= 0.7f ? 1.5f : pollution >= 0.5f ? 1.25f : 1f;

        return baseBonus * pollutionMultiplier;
    }

    public void AddCombo()
    {
        comboCount++;
        UpdateTier();
    }

    public void Reset(bool showEffect = false)
    {
        int previousCount = comboCount;
        
        if (previousCount >= 3 && showEffect)
        {
            // 连击中断视觉效果可以在这里添加
            gameSystem.AddCombatLog($"连击中断! ({previousCount}连击)");
        }

        comboCount = 0;
        lastComboTier = 0;
    }

    private void UpdateTier()
    {
        ComboTier currentTier = GetCurrentTier();
        
        if (lastComboTier != currentTier.tier && currentTier.tier > lastComboTier)
        {
            gameSystem.AddCombatLog($"<color={GetTierColor(currentTier.tier)}>{currentTier.label}!</color>");
            lastComboTier = currentTier.tier;
        }
    }

    public string GetTierColor(int tier)
    {
        switch (tier)
        {
            case 2: return "#ffff00";
            case 3: return "#ff8800";
            case 4: return "#b455ff";
            case 5: return "#00ffd0";
            default: return "#ffffff";
        }
    }
}