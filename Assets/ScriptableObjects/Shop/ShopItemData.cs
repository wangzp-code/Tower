using UnityEngine;

[CreateAssetMenu(fileName = "ShopItemData", menuName = "ParasiteTower/ShopItem")]
public class ShopItemData : ScriptableObject
{
    [Header("Basic Info")]
    public string itemId;
    public string itemName;
    public string description;
    public Sprite icon;

    [Header("Price")]
    public int basePrice;
    public float priceScale = 1f;
    public int maxBuyTimes = -1;
    public int minFloor = 1;

    [Header("Category")]
    public ShopItemCategory category;
    public ShopItemType type;
}

public enum ShopItemCategory
{
    Supply,
    Survival,
    Growth,
    Info
}

public enum ShopItemType
{
    Heal,
    Attack,
    Defense,
    MaxHp,
    PurifySmall,
    PurifyFull,
    CollapseResist,
    DeathRevive,
    FormSlot,
    FormLock,
    HumanEnhance,
    MapScan,
    MonsterScan,
    RegenCombat,
    FullHeal,
    PermRegen
}
