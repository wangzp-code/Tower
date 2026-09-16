using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "ParasiteTower/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterId;
    public string monsterName;
    public string description;
    public Sprite icon;
    public GameObject prefab;
    public Color primaryColor;
    public string silhouettePath;

    [Header("Stats")]
    public int baseHp = 50;
    public int baseAttack = 10;
    public int baseDefense = 5;
    public int baseSpeed = 10;

    [Header("Traits")]
    public string[] traits;

    [Header("Behavior")]
    public bool isBoss = false;
    public bool isElite = false;
    public float spawnWeight = 1f;

    [Header("Drops")]
    public int minGold = 10;
    public int maxGold = 20;
    public float fragmentDropChance = 0.2f;
    public string[] possibleFragmentDrops;
    public string dropFormId;

    [Header("AI")]
    public IntentType[] possibleIntents;
    public float[] intentWeights;

    [Header("Area Restriction")]
    public int minFloor = 1;
    public int maxFloor = 50;
    public string areaId;

    [Header("Build Axes")]
    public string[] buildAxes;
}

public enum BuildAxis
{
    Tank,
    Hunter,
    Parasite,
    Toxic,
    Swift,
    Sentinel
}

public enum TraitType
{
    Fast,
    ThickSkin,
    Regen,
    Loyal,
    Elastic,
    Shock,
    Leader,
    Fury,
    Web,
    Vampiric,
    Toxin,
    Armored,
    RegenPlus,
    Tear,
    Phase,
    Ambush,
    Undead,
    Drain,
    MultiAttack,
    Fear,
    Counter,
    Plunder,
    Aura,
    Summon,
    Explode
}
