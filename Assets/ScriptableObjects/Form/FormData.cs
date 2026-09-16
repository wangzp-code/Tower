using UnityEngine;

[CreateAssetMenu(fileName = "FormData", menuName = "ParasiteTower/Form")]
public class FormData : ScriptableObject
{
    [Header("Basic Info")]
    public string formId;
    public string formName;
    public string description;
    public Sprite icon;
    public GameObject prefab;

    [Header("Stats")]
    public int hpBonus;
    public int attackBonus;
    public int defenseBonus;

    [Header("Unlock Cost")]
    public int unlockPrice;
    public bool requiresMonsterDefeat;
    public string requiredMonsterId;

    [Header("Abilities")]
    public FormAbility[] abilities;
}

[System.Serializable]
public class FormAbility
{
    public string name;
    public string description;
    public int cooldown;
    public FormAbilityType type;
}

public enum FormAbilityType
{
    Passive,
    Active
}
