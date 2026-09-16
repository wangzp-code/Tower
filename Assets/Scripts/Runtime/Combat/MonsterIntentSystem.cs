using UnityEngine;
using System.Collections.Generic;

public enum IntentType
{
    Normal,
    Heavy,
    Buff,
    Heal,
    Special,
    Charge,
    Explode
}

public struct IntentData
{
    public IntentType type;
    public float mult;
    public int healAmount;
    public float buffAmount;
    public string effect;
}

public static class IntentTypes
{
    public static readonly Dictionary<IntentType, (string icon, string label, string color, string desc)> Data = new Dictionary<IntentType, (string, string, string, string)>
    {
        { IntentType.Normal, ("⚔", "攻击", "#ff6b35", "普通攻击") },
        { IntentType.Heavy, ("※", "蓄力重击", "#ff006e", "伤害×2，防御可完美格挡") },
        { IntentType.Buff, ("▲", "强化", "#ff8800", "提升自身攻击力") },
        { IntentType.Heal, ("♥", "回复", "#00ffd0", "恢复自身HP") },
        { IntentType.Special, ("⚡", "特殊", "#8844ff", "特殊能力") },
        { IntentType.Charge, ("★", "蓄力中", "#ff3344", "下回合将释放重击") },
        { IntentType.Explode, ("▼", "自爆预备", "#f80", "即将自爆！逃跑可完全回避") }
    };
}

public static class MonsterIntentSystem
{
    public static IntentData RollMonsterIntent(MonsterRuntime monster, int round)
    {
        if (monster == null)
            return new IntentData { type = IntentType.Normal, mult = 1f };

        // 蓄力怪：每3回合蓄力
        if (monster.HasTrait("蓄力") && (round + 1) % 3 == 0)
            return new IntentData { type = IntentType.Charge, mult = 1f };
        if (monster.HasTrait("蓄力") && round % 3 == 0)
            return new IntentData { type = IntentType.Heavy, mult = 2f };

        // 再生怪：低HP时倾向回复
        if ((monster.HasTrait("再生") || monster.HasTrait("再生+")) && 
            monster.hp < monster.maxHp * 0.4f && Random.value < 0.4f)
            return new IntentData { type = IntentType.Heal, mult = 0f, healAmount = Mathf.FloorToInt(monster.maxHp * 0.15f) };

        // 狂暴怪：低HP时重击
        if ((monster.ability == "berserk" || monster.HasTrait("狂暴")) && 
            monster.hp < monster.maxHp * 0.5f && Random.value < 0.35f)
            return new IntentData { type = IntentType.Heavy, mult = 1.8f };

        // 电击怪：有几率特殊攻击（眩晕）
        if (monster.HasTrait("电击") && Random.value < 0.2f)
            return new IntentData { type = IntentType.Special, mult = 1f, effect = "stun" };

        // 爆炸怪：低HP时预备自爆
        if (monster.HasTrait("爆炸") && monster.hp < monster.maxHp * 0.25f && !monster.hasExplodeWarned)
        {
            monster.hasExplodeWarned = true;
            return new IntentData { type = IntentType.Explode, mult = 0f };
        }

        // 召唤怪：偶尔强化
        if (monster.HasTrait("召唤") && Random.value < 0.15f)
            return new IntentData { type = IntentType.Buff, mult = 0f, buffAmount = 0.2f };

        // Boss有更多重击
        if (!string.IsNullOrEmpty(monster.id) && monster.id.StartsWith("boss") && Random.value < 0.25f)
            return new IntentData { type = IntentType.Heavy, mult = 1.8f };

        // 通用：20%几率重击
        if (round > 2 && Random.value < 0.15f)
            return new IntentData { type = IntentType.Heavy, mult = 1.5f };

        return new IntentData { type = IntentType.Normal, mult = 1f };
    }

    public static string GetIntentDisplay(IntentData intent)
    {
        if (!IntentTypes.Data.TryGetValue(intent.type, out var data))
            return "";
        
        return $"<color={data.color}><b>{data.icon} {data.label}</b></color>\n{data.desc}";
    }
}