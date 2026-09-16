using UnityEngine;
using System;
using System.Collections.Generic;

/// <summary>
/// 职业被动+终极技+锚点+策略提示+怪物AI意图+构筑轴线+特质钩子
/// 所有核心玩法和系统完善模块集中定义
/// </summary>
public static class ClassAbilityData
{
    [Serializable]
    public class UltimateInfo
    {
        public string name, icon, desc;
        public int cooldown, duration;
    }

    public static readonly Dictionary<string, UltimateInfo> Ultimates = new Dictionary<string, UltimateInfo>
    {
        ["titan"] = new UltimateInfo { name = "泰坦之怒", icon = "※", desc = "HP×1.5 ATK+15 DEF+15，持续10回合", cooldown = 3, duration = 10 },
        ["ghost"] = new UltimateInfo { name = "虚空行者", icon = "◎", desc = "无敌5回合(不能攻击)，结束后暴击×5", cooldown = 3, duration = 5 },
        ["swarm"] = new UltimateInfo { name = "虫群之心", icon = "◎", desc = "释放污染/10只分身，继承80%属性", cooldown = 3, duration = 10 },
        ["blood"] = new UltimateInfo { name = "血月狂宴", icon = "♥", desc = "全攻击100%吸血，每回合-5HP，持续8回合", cooldown = 3, duration = 8 },
        ["mech"]  = new UltimateInfo { name = "过载核心", icon = "⚙️", desc = "污染×3转护盾+污染×2 AOE伤害，清零污染", cooldown = 4, duration = 1 }
    };

    // 职业被动效果（每回合/每次攻击/受伤时检查）
    public static void ApplyPassiveOnAttack(GameManager.PlayerData p, ref int damage, bool isCrit)
    {
        switch (p.selectedClass)
        {
            case "blood":
                if (p.hp < p.maxHp * 0.3f) damage = Mathf.RoundToInt(damage * 1.5f);
                if (p.hasBloodMoon) { p.hp = Mathf.Min(p.maxHp, p.hp + damage); }
                break;
            case "ghost":
                if (p.turnsInCombat == 1) damage = Mathf.RoundToInt(damage * 2f);
                break;
            case "mech":
                if (p.pollution >= 50f) damage += 2;
                break;
        }
    }

    public static int ApplyPassiveOnDefend(GameManager.PlayerData p, int incomingDamage)
    {
        switch (p.selectedClass)
        {
            case "titan":
                return Mathf.RoundToInt(incomingDamage * 0.9f);
            case "mech":
                if (p.defense > 10) return Mathf.RoundToInt(incomingDamage * 0.85f);
                break;
        }
        return incomingDamage;
    }

    public static void ApplyPassiveOnKill(GameManager.PlayerData p)
    {
        switch (p.selectedClass)
        {
            case "blood":
                int heal = Mathf.RoundToInt(p.maxHp * 0.1f);
                p.hp = Mathf.Min(p.maxHp, p.hp + heal);
                break;
            case "swarm":
                p.evolutionPoints += 2;
                break;
        }
    }

    public static void ApplyPassivePerFloor(GameManager.PlayerData p, int evolutionLevel)
    {
        switch (p.selectedClass)
        {
            case "titan":
                if (evolutionLevel >= 3)
                {
                    int regen = Mathf.Max(1, Mathf.RoundToInt(p.maxHp * 0.12f));
                    p.hp = Mathf.Min(p.maxHp, p.hp + regen);
                }
                break;
            case "mech":
                if (evolutionLevel >= 1) p.defense += 2;
                break;
        }
    }
}

public static class AnchorHelper
{
    public static int AnchorFloor { get; set; } = 0;
    public static bool HasAnchor => AnchorFloor > 0;

    public static bool SetAnchor(int floor, GameManager.PlayerData p)
    {
        if (p.evolutionPoints < 200) return false;
        p.evolutionPoints -= 200;
        AnchorFloor = floor;
        return true;
    }

    public static void Reset()
    {
        AnchorFloor = 0;
    }
}

public static class StrategyHints
{
    [Serializable]
    public class Hint
    {
        public string id, text;
        public Func<GameManager.PlayerData, int, bool> condition;
    }

    static HashSet<string> _shown = new HashSet<string>();
    static float _lastShownTime;

    public static string Check(GameManager.PlayerData p, int floor, float pollution)
    {
        if (Time.time - _lastShownTime < 90f) return null;

        var hints = new Hint[]
        {
            new Hint { id = "low_hp", text = "► HP过低，考虑使用休息或净化道具", condition = (pl, f) => pl.hp < pl.maxHp * 0.25f },
            new Hint { id = "high_poll", text = "⚠ 污染过高！使用净化道具或避免附身", condition = (pl, f) => pl.pollution >= 70f },
            new Hint { id = "poll_crit", text = "☢ 污染临界！立即净化否则崩溃", condition = (pl, f) => pl.pollution >= 90f },
            new Hint { id = "no_possess", text = "► 已经3层没附身了，尝试附身获取更强形态", condition = (pl, f) => f >= 4 && pl.possessionCountThisRun == 0 },
            new Hint { id = "can_evolve", text = "► EP足够进化！菜单→进化解锁更强能力", condition = (pl, f) => pl.evolutionPoints >= 200 },
            new Hint { id = "elite_warn", text = "⚠ 精英怪出没！做好准备再前进", condition = (pl, f) => f >= 5 },
        };

        foreach (var h in hints)
        {
            if (_shown.Contains(h.id)) continue;
            if (h.condition(p, floor))
            {
                _shown.Add(h.id);
                _lastShownTime = Time.time;
                return h.text;
            }
        }
        return null;
    }

    public static void Reset() { _shown.Clear(); _lastShownTime = 0; }
}

public static class BuildAxesData
{
    [Serializable]
    public class Axis
    {
        public string id, name, icon, color;
        public float atkBonus, defBonus, hpBonus, possessBonus;
    }

    public static readonly Axis[] Axes = new Axis[]
    {
        new Axis { id = "tank", name = "坦克", icon = "◆", color = "#4488cc", defBonus = 2f, hpBonus = 15f },
        new Axis { id = "hunter", name = "猎手", icon = "⚔", color = "#ff4444", atkBonus = 3f },
        new Axis { id = "parasite", name = "寄生", icon = "★", color = "#b455ff", possessBonus = 0.05f },
        new Axis { id = "toxic", name = "腐蚀", icon = "☠", color = "#88cc00", atkBonus = 1.5f },
        new Axis { id = "swift", name = "迅影", icon = "⚡", color = "#ffaa00", defBonus = 1f },
        new Axis { id = "sentinel", name = "守卫", icon = "◆", color = "#888888", atkBonus = 1f, defBonus = 1f }
    };

    public static float GetSynergyBonus(string[] monsterAxes, string playerClass)
    {
        if (monsterAxes == null || monsterAxes.Length == 0) return 0f;
        string classAxis = GetClassAxis(playerClass);
        foreach (var a in monsterAxes)
        {
            if (a == classAxis) return 0.1f;
        }
        return 0f;
    }

    static string GetClassAxis(string cls)
    {
        switch (cls)
        {
            case "titan": return "tank";
            case "ghost": return "swift";
            case "swarm": return "parasite";
            case "blood": return "hunter";
            case "mech": return "sentinel";
            default: return "";
        }
    }
}

public static class TraitEffects
{
    public static int ApplyOnPlayerAttack(string[] traits, int baseDamage)
    {
        if (traits == null) return baseDamage;
        float mult = 1f;
        foreach (var t in traits)
        {
            switch (t)
            {
                case "暴击": if (UnityEngine.Random.value < 0.15f) mult *= 1.5f; break;
                case "撕裂": mult *= 1.08f; break;
                case "毒素": break;
                case "伏击": break;
            }
        }
        return Mathf.RoundToInt(baseDamage * mult);
    }

    public static int ApplyOnEnemyAttack(string[] traits, int baseDamage, float enemyHpRatio)
    {
        if (traits == null) return baseDamage;
        float mult = 1f;
        foreach (var t in traits)
        {
            switch (t)
            {
                case "狂暴": if (enemyHpRatio < 0.5f) mult *= 1.5f; break;
                case "多重攻击": mult *= 0.7f * 2f; break;
                case "护甲": break;
            }
        }
        return Mathf.RoundToInt(baseDamage * mult);
    }

    public static void ApplyOnEnemyTurnEnd(string[] traits, MonsterRuntime enemy)
    {
        if (traits == null || enemy == null) return;
        foreach (var t in traits)
        {
            switch (t)
            {
                case "再生":
                    enemy.hp = Mathf.Min(enemy.maxHp, enemy.hp + Mathf.Max(1, Mathf.RoundToInt(enemy.maxHp * 0.03f)));
                    break;
                case "再生+":
                    enemy.hp = Mathf.Min(enemy.maxHp, enemy.hp + Mathf.Max(1, Mathf.RoundToInt(enemy.maxHp * 0.08f)));
                    break;
            }
        }
    }

    public static float GetPossessBonus(string[] traits)
    {
        if (traits == null) return 0f;
        float bonus = 0f;
        foreach (var t in traits)
        {
            if (t == "寄生强化") bonus += 0.1f;
        }
        return bonus;
    }

    public static void ApplyPollutionTraits(string[] traits, GameManager.PlayerData p)
    {
        if (traits == null) return;
        foreach (var t in traits)
        {
            switch (t)
            {
                case "毒素": PollutionSystem.Instance?.GainPollution(2f); break;
                case "污染光环": PollutionSystem.Instance?.GainPollution(3f); break;
            }
        }
    }
}
