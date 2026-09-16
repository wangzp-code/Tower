using UnityEngine;

public static class MonsterTraitSystem
{
    // 检查怪物是否拥有指定特性
    public static bool HasTrait(this MonsterRuntime monster, string trait)
    {
        if (monster.traits == null)
            return false;
        
        foreach (var t in monster.traits)
        {
            if (t.Equals(trait, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }

    // 应用怪物特性效果
    public static void ApplyTraitEffects(MonsterRuntime monster, ref float damage, ref float defense, IntentType currentIntent = IntentType.Normal, int combatRound = 0)
    {
        // 狂暴：低HP时攻击加成
        if ((monster.ability == "berserk" || monster.HasTrait("狂暴")) && monster.hp < monster.maxHp * 0.5f)
        {
            damage *= 1.5f;
        }

        // 护甲：前3回合防御翻倍
        if ((monster.ability == "armored" || monster.HasTrait("护甲")) && combatRound <= 3)
        {
            defense *= 2f;
        }

        // 多重攻击
        if (monster.HasTrait("多重攻击"))
        {
            damage *= 0.7f * 2f;
        }

        // 蓄力攻击：仅在重击回合生效
        if (monster.HasTrait("蓄力") && currentIntent == IntentType.Heavy)
        {
            damage *= 2f;
        }
    }

    // 处理怪物反击特性
    public static int HandleCounterAttack(MonsterRuntime monster, int damageTaken)
    {
        int counterDamage = 0;
        
        // 反击：受击时反弹50%伤害
        if (monster.HasTrait("反击") && damageTaken > 0)
        {
            counterDamage = Mathf.Max(1, Mathf.FloorToInt(damageTaken * 0.5f));
        }

        return counterDamage;
    }

    // 处理怪物吸血特性
    public static int HandleLifeSteal(MonsterRuntime monster, int damageDealt)
    {
        if (!monster.HasTrait("吸血") || damageDealt <= 0)
            return 0;

        return Mathf.Max(1, Mathf.FloorToInt(damageDealt * 0.3f));
    }

    // 处理怪物吸取特性
    public static int HandleDrain(MonsterRuntime monster, int damageDealt)
    {
        if (!monster.HasTrait("吸取") || damageDealt <= 0)
            return 0;

        return Mathf.Max(1, Mathf.FloorToInt(damageDealt * 0.1f));
    }

    // 处理怪物再生特性
    public static int HandleRegeneration(MonsterRuntime monster)
    {
        if (!monster.HasTrait("再生") && !monster.HasTrait("再生+"))
            return 0;

        float regenPercent = monster.HasTrait("再生+") ? 0.1f : 0.05f;
        return Mathf.Max(1, Mathf.FloorToInt(monster.maxHp * regenPercent));
    }

    // 处理怪物不死特性
    public static bool HandleUndead(MonsterRuntime monster)
    {
        if (!monster.HasTrait("不死") || monster.hasRevived)
            return false;

        monster.hasRevived = true;
        monster.hp = Mathf.Max(1, Mathf.FloorToInt(monster.maxHp * 0.3f));
        return true;
    }

    // 处理怪物爆炸特性
    public static int HandleExplosion(MonsterRuntime monster)
    {
        if (!monster.HasTrait("爆炸"))
            return 0;

        return Mathf.Max(1, Mathf.FloorToInt(monster.maxHp * 0.3f));
    }

    // 处理怪物毒素特性
    public static int HandlePoison(MonsterRuntime monster, int combatTotalDamage)
    {
        if (monster.ability != "poison")
            return 0;

        return Mathf.Max(1, Mathf.FloorToInt(combatTotalDamage * 0.1f));
    }

    // 处理怪物恐惧特性
    public static float HandleFear(float damage)
    {
        return Mathf.Max(1, Mathf.FloorToInt(damage * 0.9f));
    }

    // 处理怪物蛛网特性
    public static float HandleWebbed(float damage)
    {
        return Mathf.Max(1, Mathf.FloorToInt(damage * 0.7f));
    }

    // 获取特性描述
    public static string GetTraitDescription(string trait)
    {
        switch (trait)
        {
            case "狂暴": return "HP<50%时ATK+50%";
            case "再生": return "每回合恢复HP";
            case "再生+": return "每回合恢复更多HP";
            case "吸血": return "攻击回复30%伤害";
            case "吸取": return "攻击吸取生命";
            case "护甲": return "前3回合防御翻倍";
            case "反击": return "受击时反弹50%伤害";
            case "多重攻击": return "每回合攻击2次";
            case "不死": return "首次致死时复活";
            case "恐惧": return "降低玩家攻击力10%";
            case "电击": return "有几率眩晕玩家";
            case "撕裂": return "造成持续流血";
            case "召唤": return "可能呼叫援军";
            case "蛛网": return "减速并降低伤害";
            case "腐蚀": return "逐渐削弱防御";
            case "爆炸": return "死亡时自爆";
            case "蓄力": return "每3回合蓄力重击";
            case "厚皮": return "高防御";
            case "迅捷": return "快速行动";
            case "弹性": return "高闪避";
            case "毒素": return "击败后额外伤害";
            case "暴击": return "高暴击率";
            case "相位": return "闪避几率";
            case "伏击": return "首回合伤害×1.5";
            default: return trait;
        }
    }
}