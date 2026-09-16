using UnityEngine;

[CreateAssetMenu(fileName = "FragmentData", menuName = "ParasiteTower/Fragment")]
public class FragmentData : ScriptableObject
{
    public string fragmentId;
    public string fragmentName;
    public FragmentType type;
    public Sprite icon;
    public string description;

    [Header("Stats")]
    public int attackBonus;
    public int defenseBonus;
    public int hpBonus;
    public float critBonus;

    [Header("Skills")]
    public bool hasActiveSkill;
    public string activeSkillId;
    public bool hasPassiveSkill;
    public string passiveSkillId;

    [Header("Requirements")]
    public int unlockFloor;
    public int cost;
    public bool isDLC;

    public enum FragmentType
    {
        Attack,
        Defense,
        Support,
        Special
    }
}
