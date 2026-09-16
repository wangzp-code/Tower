using UnityEngine;
using System.Collections.Generic;

public class TraitManager : SingletonBase<TraitManager>
{
    private Dictionary<string, TraitData> traitDatabase = new Dictionary<string, TraitData>();

    private void Awake()
    {
        base.Awake();
        InitializeTraits();
    }

    private void InitializeTraits()
    {
        // 原项目的特质定义
        traitDatabase["迅捷"] = new TraitData
        {
            traitId = "fast",
            name = "迅捷",
            description = "速度提升，先手概率+20%",
            icon = "⚡",
            effectType = TraitEffectType.Passive,
            effectValue = 0.2f,
            effectTarget = TraitEffectTarget.Initiative
        };

        traitDatabase["厚皮"] = new TraitData
        {
            traitId = "thick_skin",
            name = "厚皮",
            description = "受到伤害-10%",
            icon = "◆",
            effectType = TraitEffectType.Passive,
            effectValue = 0.1f,
            effectTarget = TraitEffectTarget.DamageReduction
        };

        traitDatabase["再生"] = new TraitData
        {
            traitId = "regen",
            name = "再生",
            description = "每回合恢复3%最大生命",
            icon = "♥",
            effectType = TraitEffectType.Passive,
            effectValue = 0.03f,
            effectTarget = TraitEffectTarget.HpRegen
        };

        traitDatabase["忠诚"] = new TraitData
        {
            traitId = "loyal",
            name = "忠诚",
            description = "保护友方单位",
            icon = "❤️",
            effectType = TraitEffectType.Passive,
            effectValue = 0f,
            effectTarget = TraitEffectTarget.ProtectAlly
        };

        traitDatabase["弹性"] = new TraitData
        {
            traitId = "elastic",
            name = "弹性",
            description = "受到攻击有20%概率减免50%伤害",
            icon = "●",
            effectType = TraitEffectType.Chance,
            effectValue = 0.2f,
            effectTarget = TraitEffectTarget.DamageReductionChance,
            secondaryValue = 0.5f
        };

        traitDatabase["电击"] = new TraitData
        {
            traitId = "shock",
            name = "电击",
            description = "攻击有30%概率麻痹敌人一回合",
            icon = "⚡",
            effectType = TraitEffectType.Chance,
            effectValue = 0.3f,
            effectTarget = TraitEffectTarget.StunChance
        };

        traitDatabase["领袖"] = new TraitData
        {
            traitId = "leader",
            name = "领袖",
            description = "周围友方攻击+10%",
            icon = "♛",
            effectType = TraitEffectType.Aura,
            effectValue = 0.1f,
            effectTarget = TraitEffectTarget.AllyAttackBonus
        };

        traitDatabase["狂暴"] = new TraitData
        {
            traitId = "fury",
            name = "狂暴",
            description = "HP低于30%时攻击+50%",
            icon = "★",
            effectType = TraitEffectType.Conditional,
            effectValue = 0.3f,
            effectTarget = TraitEffectTarget.LowHpAttackBonus,
            secondaryValue = 0.5f
        };

        traitDatabase["蛛网"] = new TraitData
        {
            traitId = "web",
            name = "蛛网",
            description = "攻击有25%概率减速敌人",
            icon = "«",
            effectType = TraitEffectType.Chance,
            effectValue = 0.25f,
            effectTarget = TraitEffectTarget.SlowChance
        };

        traitDatabase["吸血"] = new TraitData
        {
            traitId = "vampiric",
            name = "吸血",
            description = "攻击回复伤害15%的生命",
            icon = "♥",
            effectType = TraitEffectType.Passive,
            effectValue = 0.15f,
            effectTarget = TraitEffectTarget.Lifesteal
        };

        traitDatabase["毒素"] = new TraitData
        {
            traitId = "toxin",
            name = "毒素",
            description = "攻击附加毒素，每回合造成5%最大HP伤害，持续3回合",
            icon = "☠️",
            effectType = TraitEffectType.Dot,
            effectValue = 0.05f,
            effectTarget = TraitEffectTarget.PoisonDot,
            secondaryValue = 3f
        };

        traitDatabase["护甲"] = new TraitData
        {
            traitId = "armored",
            name = "护甲",
            description = "额外护甲+15",
            icon = "◆",
            effectType = TraitEffectType.Passive,
            effectValue = 15f,
            effectTarget = TraitEffectTarget.ArmorBonus
        };

        traitDatabase["再生+"] = new TraitData
        {
            traitId = "regen_plus",
            name = "再生+",
            description = "每回合恢复8%最大生命",
            icon = "♥",
            effectType = TraitEffectType.Passive,
            effectValue = 0.08f,
            effectTarget = TraitEffectTarget.HpRegen
        };

        traitDatabase["撕裂"] = new TraitData
        {
            traitId = "tear",
            name = "撕裂",
            description = "攻击附加撕裂效果，每回合造成10%攻击伤害",
            icon = "♥",
            effectType = TraitEffectType.Dot,
            effectValue = 0.1f,
            effectTarget = TraitEffectTarget.TearDot
        };

        traitDatabase["相位"] = new TraitData
        {
            traitId = "phase",
            name = "相位",
            description = "闪避概率+30%",
            icon = "◇",
            effectType = TraitEffectType.Passive,
            effectValue = 0.3f,
            effectTarget = TraitEffectTarget.Evasion
        };

        traitDatabase["伏击"] = new TraitData
        {
            traitId = "ambush",
            name = "伏击",
            description = "先手攻击伤害+50%",
            icon = "◎",
            effectType = TraitEffectType.Conditional,
            effectValue = 0.5f,
            effectTarget = TraitEffectTarget.FirstAttackBonus
        };

        traitDatabase["不死"] = new TraitData
        {
            traitId = "undead",
            name = "不死",
            description = "死亡后有30%概率复活",
            icon = "☠",
            effectType = TraitEffectType.Chance,
            effectValue = 0.3f,
            effectTarget = TraitEffectTarget.ResurrectChance
        };

        traitDatabase["吸取"] = new TraitData
        {
            traitId = "drain",
            name = "吸取",
            description = "攻击吸取敌人10%防御",
            icon = "↩",
            effectType = TraitEffectType.Passive,
            effectValue = 0.1f,
            effectTarget = TraitEffectTarget.DefenseDrain
        };

        traitDatabase["多重攻击"] = new TraitData
        {
            traitId = "multi_attack",
            name = "多重攻击",
            description = "每回合攻击2次",
            icon = "⚔️",
            effectType = TraitEffectType.Passive,
            effectValue = 2f,
            effectTarget = TraitEffectTarget.AttackCount
        };

        traitDatabase["恐惧"] = new TraitData
        {
            traitId = "fear",
            name = "恐惧",
            description = "攻击有20%概率使敌人逃跑",
            icon = "▲",
            effectType = TraitEffectType.Chance,
            effectValue = 0.2f,
            effectTarget = TraitEffectTarget.FearChance
        };

        traitDatabase["反击"] = new TraitData
        {
            traitId = "counter",
            name = "反击",
            description = "受到攻击时反弹20%伤害",
            icon = "←",
            effectType = TraitEffectType.Passive,
            effectValue = 0.2f,
            effectTarget = TraitEffectTarget.DamageReflect
        };

        traitDatabase["掠夺"] = new TraitData
        {
            traitId = "plunder",
            name = "掠夺",
            description = "击杀后获得额外50%金币",
            icon = "$",
            effectType = TraitEffectType.Passive,
            effectValue = 0.5f,
            effectTarget = TraitEffectTarget.GoldBonus
        };

        traitDatabase["污染光环"] = new TraitData
        {
            traitId = "pollution_aura",
            name = "污染光环",
            description = "每回合对敌人造成5%污染",
            icon = "☠️",
            effectType = TraitEffectType.Aura,
            effectValue = 0.05f,
            effectTarget = TraitEffectTarget.PollutionAura
        };

        traitDatabase["召唤"] = new TraitData
        {
            traitId = "summon",
            name = "召唤",
            description = "每3回合召唤一只分身",
            icon = "◈",
            effectType = TraitEffectType.Periodic,
            effectValue = 3f,
            effectTarget = TraitEffectTarget.SummonMinion
        };

        traitDatabase["爆炸"] = new TraitData
        {
            traitId = "explode",
            name = "爆炸",
            description = "死亡时对周围造成100%攻击伤害",
            icon = "※",
            effectType = TraitEffectType.OnDeath,
            effectValue = 1f,
            effectTarget = TraitEffectTarget.DeathExplosion
        };

        traitDatabase["寄生强化"] = new TraitData
        {
            traitId = "parasite_buff",
            name = "寄生强化",
            description = "被附身时属性+20%",
            icon = "★",
            effectType = TraitEffectType.Conditional,
            effectValue = 0.2f,
            effectTarget = TraitEffectTarget.PossessionBonus
        };

        traitDatabase["暴击"] = new TraitData
        {
            traitId = "crit",
            name = "暴击",
            description = "暴击概率+15%",
            icon = "※",
            effectType = TraitEffectType.Passive,
            effectValue = 0.15f,
            effectTarget = TraitEffectTarget.CritChance
        };
    }

    public TraitData GetTrait(string traitName)
    {
        if (traitDatabase.TryGetValue(traitName, out TraitData trait))
        {
            return trait;
        }


        return null;
    }

    public List<TraitData> GetTraits(List<string> traitNames)
    {
        List<TraitData> traits = new List<TraitData>();
        foreach (string name in traitNames)
        {
            TraitData trait = GetTrait(name);
            if (trait != null)
            {
                traits.Add(trait);
            }
        }
        return traits;
    }

    public void ApplyTraitEffects(MonsterBase monster)
    {
        if (monster == null || monster.Data == null) return;

        foreach (string traitName in monster.Data.traits)
        {
            TraitData trait = GetTrait(traitName);
            if (trait != null)
            {
                ApplyTraitEffect(monster, trait);
            }
        }
    }

    private void ApplyTraitEffect(MonsterBase monster, TraitData trait)
    {
        switch (trait.effectTarget)
        {
            case TraitEffectTarget.DamageReduction:
                monster.damageReduction += trait.effectValue;
                break;
            case TraitEffectTarget.HpRegen:
                monster.regenRate += trait.effectValue;
                break;
            case TraitEffectTarget.Lifesteal:
                monster.lifesteal += trait.effectValue;
                break;
            case TraitEffectTarget.Evasion:
                monster.evasion += trait.effectValue;
                break;
            case TraitEffectTarget.ArmorBonus:
                monster.armor += (int)trait.effectValue;
                break;
            case TraitEffectTarget.AttackCount:
                monster.attackCount = (int)trait.effectValue;
                break;
            case TraitEffectTarget.LowHpAttackBonus:
                monster.lowHpThreshold = trait.effectValue;
                monster.lowHpAttackMultiplier = trait.secondaryValue;
                break;
            case TraitEffectTarget.CritChance:
                monster.critChance += trait.effectValue;
                break;
            case TraitEffectTarget.DamageReflect:
                monster.damageReflect += trait.effectValue;
                break;
            case TraitEffectTarget.GoldBonus:
                monster.goldBonus += trait.effectValue;
                break;
        }
    }

    protected override void OnDestroy()
    {
        traitDatabase.Clear();
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}

[System.Serializable]
public class TraitData
{
    public string traitId;
    public string name;
    public string description;
    public string icon;
    public TraitEffectType effectType;
    public TraitEffectTarget effectTarget;
    public float effectValue;
    public float secondaryValue;
}

public enum TraitEffectType
{
    Passive,
    Chance,
    Conditional,
    Aura,
    Dot,
    Periodic,
    OnDeath
}

public enum TraitEffectTarget
{
    Initiative,
    DamageReduction,
    HpRegen,
    ProtectAlly,
    DamageReductionChance,
    StunChance,
    AllyAttackBonus,
    LowHpAttackBonus,
    SlowChance,
    Lifesteal,
    PoisonDot,
    ArmorBonus,
    TearDot,
    Evasion,
    FirstAttackBonus,
    ResurrectChance,
    DefenseDrain,
    AttackCount,
    FearChance,
    DamageReflect,
    GoldBonus,
    PollutionAura,
    SummonMinion,
    DeathExplosion,
    PossessionBonus,
    CritChance
}
