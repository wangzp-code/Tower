using UnityEngine;
using System.Collections.Generic;

public static class PollutionPassiveSystem
{
    // 获取当前污染等级下激活的被动技能
    public static List<PollutionPassiveData.PollutionPassive> GetActivePassives(float pollution)
    {
        List<PollutionPassiveData.PollutionPassive> active = new List<PollutionPassiveData.PollutionPassive>();
        
        foreach (var passive in PollutionPassiveData.Passives)
        {
            if (pollution >= passive.threshold)
                active.Add(passive);
        }
        
        return active;
    }

    // 检查指定被动是否激活
    public static bool IsPassiveActive(string passiveName, float pollution)
    {
        foreach (var passive in PollutionPassiveData.Passives)
        {
            if (passive.name == passiveName && pollution >= passive.threshold)
                return true;
        }
        return false;
    }

    // 应用污染共鸣效果
    public static int ApplyResonanceDamage(float pollution)
    {
        if (pollution < 50f)
            return 0;

        if (Random.value < 0.15f)
            return Mathf.FloorToInt(pollution);
        
        return 0;
    }

    // 应用腐蚀之体效果
    public static bool ApplyCorrodeBody(float pollution)
    {
        if (pollution < 70f)
            return false;

        return Random.value < 0.1f;
    }

    // 应用死亡脉冲效果
    public static int ApplyDeathPulse(float pollution, int maxHp)
    {
        if (pollution < 90f)
            return 0;

        return Mathf.FloorToInt(maxHp * 0.1f);
    }

    // 获取污染等级信息
    public static (string label, string icon, Color color, float atkMult, float defMult) GetPollutionTier(float pollution)
    {
        int p = Mathf.RoundToInt(pollution);

        if (p >= 100) return ("爆发", "☢", new Color(1f, 0.4f, 0.2f), 1.5f, 0.7f);
        if (p >= 85) return ("临界", "✦", new Color(0.8f, 0.3f, 1f), 1.3f, 0.8f);
        if (p >= 60) return ("侵蚀", "◆", new Color(1f, 0.7f, 0f), 1.2f, 0.9f);
        if (p >= 30) return ("觉醒", "◈", new Color(0f, 0.8f, 1f), 1.1f, 1.0f);

        return ("净化", "◇", new Color(0.6f, 0.8f, 0.6f), 1.0f, 1.15f);
    }

    // 获取污染攻击倍率
    public static float GetPollutionAttackMultiplier(float pollution)
    {
        int p = Mathf.RoundToInt(pollution);

        if (p >= 100) return 1.5f;
        if (p >= 85) return 1.3f;
        if (p >= 60) return 1.2f;
        if (p >= 30) return 1.1f;

        return 1.0f;
    }

    // 获取污染防御倍率
    public static float GetPollutionDefenseMultiplier(float pollution)
    {
        int p = Mathf.RoundToInt(pollution);

        if (p >= 100) return 0.7f;
        if (p >= 85) return 0.8f;
        if (p >= 60) return 0.9f;
        if (p >= 30) return 1.0f;

        return 1.15f;
    }

    // 低污染净化回血（每回合调用）
    public static int GetPurificationRegen(float pollution, int maxHp)
    {
        if (pollution < 30f)
            return Mathf.CeilToInt(maxHp * 0.02f);
        return 0;
    }

    // 附身时 trait 是否变异
    public static bool ShouldMutateTrait(float pollution)
    {
        if (pollution < 60f) return false;
        float chance = (pollution - 60f) / 200f;
        return Random.value < chance;
    }

    // 随机替换 trait
    public static string MutateTrait(string original)
    {
        string[] pool = { "迅捷", "厚皮", "狂暴", "吸血", "再生", "毒素", "反弹", "暴击", "闪避", "护甲" };
        string result;
        int safety = 20;
        do
        {
            result = pool[Random.Range(0, pool.Length)];
            safety--;
        } while (result == original && safety > 0);
        return result;
    }

    // 获取 tier 显示详情
    public static string GetTierDisplayInfo(float pollution)
    {
        var tier = GetPollutionTier(pollution);
        return $"{tier.icon} {tier.label} (ATK x{tier.atkMult:F1} DEF x{tier.defMult:F2})";
    }
}