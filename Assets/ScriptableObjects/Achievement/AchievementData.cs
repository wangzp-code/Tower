using UnityEngine;

[CreateAssetMenu(fileName = "AchievementData", menuName = "ParasiteTower/AchievementData")]
public class AchievementData : ScriptableObject
{
    [Header("Basic Info")]
    public string achievementId;
    public string name;
    public string description;
    public string icon;

    [Header("Requirements")]
    public AchievementType type;
    public int target;

    [Header("Rewards")]
    public int goldReward;
    public AchievementBonus bonus;
}

[System.Serializable]
public class AchievementBonus
{
    public string stat;
    public float value;
    public string description;
}

public enum AchievementType
{
    Kill,
    Possession,
    Floor,
    PossessionCount,
    NoDeath,
    Ending,
    Defend,
    FormSwitch,
    PureRun
}