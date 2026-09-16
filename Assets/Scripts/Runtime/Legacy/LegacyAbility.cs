using UnityEngine;
using System;

[Serializable]
public class LegacyAbility
{
    public string id;
    public string name;
    public string description;
    public string icon;
    public string effectType;
    public float effectValue;
    public float baseEffectValue;
    public string sourceMonsterId;
    public string sourceMonsterName;
    public int rarity; // 0=Common, 1=Rare, 2=Epic
    public bool isMutated;
    public string originalEffectType;

    public LegacyAbility() { }

    public LegacyAbility(string id, string name, string desc, string icon,
        string effectType, float effectValue, string srcId, string srcName, int rarity)
    {
        this.id = id;
        this.name = name;
        this.description = desc;
        this.icon = icon;
        this.effectType = effectType;
        this.effectValue = effectValue;
        this.baseEffectValue = effectValue;
        this.sourceMonsterId = srcId;
        this.sourceMonsterName = srcName;
        this.rarity = rarity;
    }

    public string RarityLabel()
    {
        switch (rarity)
        {
            case 2: return "史诗";
            case 1: return "稀有";
            default: return "普通";
        }
    }

    public (string tag, Color color) BuildTag()
    {
        if (string.IsNullOrEmpty(effectType))
            return ("通用", Color.white);
            
        return effectType switch
        {
            "stat_buff" when icon == "◈" => ("防御", new Color(0.3f, 0.7f, 0.9f)),
            "stat_buff" when icon == "†" => ("攻击", new Color(1f, 0.4f, 0.2f)),
            "stat_buff" => ("强化", new Color(0.9f, 0.8f, 0.3f)),
            "heal_percent" => ("续航", new Color(0.3f, 0.9f, 0.3f)),
            "possess_bonus" => ("附身", new Color(0.2f, 0.9f, 0.8f)),
            "shield" => ("防御", new Color(0.3f, 0.7f, 0.9f)),
            "pollution_reduce" => ("净化", new Color(0.6f, 0.3f, 0.9f)),
            "skill_cost_reduce" => ("技能", new Color(0.9f, 0.5f, 0.1f)),
            "ep_bonus" => ("资源", new Color(0.8f, 0.6f, 0.2f)),
            "death_save" => ("保命", new Color(0.5f, 0.2f, 0.7f)),
            "remove_cooldown" => ("切换", new Color(0.4f, 0.6f, 0.9f)),
            "boss_damage_bonus" => ("Boss", new Color(1f, 0.3f, 0.6f)),
            "reveal_monsters" => ("探索", new Color(0.5f, 0.7f, 0.5f)),
            _ => ("通用", Color.white),
        };
    }
}
