using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ParasiteTower/Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Player Settings")]
    public int startingHp = 100;
    public int startingAttack = 10;
    public int startingDefense = 5;
    public int maxFragmentSlots = 4;

    [Header("Combat Settings")]
    public float baseCritChance = 0.15f;
    public float critMultiplier = 1.5f;
    public float defendMultiplier = 2f;
    public float fleeBaseChance = 0.5f;

    [Header("Pollution Settings")]
    public float pollutionGainPerTurn = 2f;
    public float pollutionDecayPerFloor = 5f;
    public float pollutionThreshold = 50f;
    public float burstModeMultiplier = 1.5f;

    [Header("Combo Settings")]
    public float combo3Bonus = 0.05f;
    public float combo5Bonus = 0.10f;
    public float combo8Bonus = 0.15f;
    public float combo12Bonus = 0.25f;
    public float combo20Bonus = 0.35f;

    [Header("Floor Settings")]
    public int classicModeFloorCount = 50;
    public int shortModeFloorCount = 12;
    public int expeditionModeFloorCount = 20;

    [Header("Difficulty Scaling")]
    public float enemyHpScaling = 0.05f;
    public float enemyAttackScaling = 0.05f;
    public float goldScaling = 0.03f;
}
