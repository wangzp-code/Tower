using UnityEngine;

[CreateAssetMenu(fileName = "ClassData", menuName = "ParasiteTower/Class")]
public class ClassData : ScriptableObject
{
    [Header("Basic Info")]
    public string classId;
    public string className;
    public string quote;
    public Sprite icon;
    public Color primaryColor;
    public Color highlightColor;
    public Color glowColor;
    public Color bgColor;
    public string iconEmoji;

    [Header("Difficulty")]
    public string difficulty;
    public string style;
    public string[] tags;

    [Header("Base Stats")]
    public int baseHp;
    public int baseAttack;
    public int baseDefense;
    public int fogRadius;

    [Header("Ultimate Skill")]
    public string ultimateName;
    public string ultimateDesc;
    public int ultimateCooldown;
    public int ultimateDuration;

    [Header("Evolution Tree")]
    public EvolutionNode[] evolutionNodes;

    [Header("Mechanics")]
    public ClassMechanic[] mechanics;
}

[System.Serializable]
public class EvolutionNode
{
    public string name;
    public int cost;
    public string description;
    public EvolutionEffect effect;
}

[System.Serializable]
public class EvolutionEffect
{
    public int defense;
    public int attack;
    public int maxHp;
    public float damageReduce;
    public float lifeSteal;
    public float possessBonus;
    public float extraDmg;
    public int bonusEvo;
    public float regen;
    public int pollutionReduce;
    public float lowHpBonus;
    public float killHeal;
    public bool deathImmune;
    public int shieldPerFloor;
    public float shieldAtkBonus;
    public bool pollToShield;
    public int shieldCap;
    public bool shieldDoubleDmg;
}

[System.Serializable]
public class ClassMechanic
{
    public string name;
    public string description;
}

[System.Serializable]
public class ClassEnding
{
    public string title;
    public string subtitle;
    public string achievementId;
    public string text;
    public string quote;
}
