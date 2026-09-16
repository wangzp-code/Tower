using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class SpecialFloorSystem : SingletonBase<SpecialFloorSystem>
{
    public enum SpecialFloorType
    {
        None,
        Shop,
        Rest,
        Boss,
        Altar,
        Treasure,
        Trap,
        Mystery,
        BossRush,
        Event
    }

    public struct SpecialFloor
    {
        public int floor;
        public SpecialFloorType type;
        public string name;
        public string description;
        public int minFloor;
        public int maxFloor;
        public float weight;
        public System.Action onEnter;
    }

    private List<SpecialFloor> specialFloors = new List<SpecialFloor>();
    private Dictionary<int, SpecialFloorType> floorAssignments = new Dictionary<int, SpecialFloorType>();
    private float totalWeightCache = 0f;

    protected override void Awake()
    {
        base.Awake();
        InitializeSpecialFloors();
        InitializeBossRush();
    }

    void InitializeSpecialFloors()
    {
        specialFloors.Add(new SpecialFloor
        {
            type = SpecialFloorType.Shop,
            name = "商店",
            description = "商人在此等候",
            minFloor = 5,
            maxFloor = 50,
            weight = 0.15f,
            onEnter = () =>
            {
                CompleteGameSystem.Instance.AddCombatLog("† 你发现了一家神秘的商店");
                ShopManager.Instance.GenerateShopItems(GameManager.Instance.CurrentFloor);
                CanvasUIManager.Instance.ShowItemShopPanel();
            }
        });

        specialFloors.Add(new SpecialFloor
        {
            type = SpecialFloorType.Rest,
            name = "休息室",
            description = "可以在这里恢复",
            minFloor = 3,
            maxFloor = 50,
            weight = 0.1f,
            onEnter = () =>
            {
                var p = GameManager.Instance.Player;
                int healAmount = Mathf.FloorToInt(p.maxHp * 0.3f);
                p.hp = Mathf.Min(p.maxHp, p.hp + healAmount);
                CompleteGameSystem.Instance.AddCombatLog($"◇ 休息恢复了 {healAmount} HP");
            }
        });

        specialFloors.Add(new SpecialFloor
        {
            type = SpecialFloorType.Altar,
            name = "回声祭坛",
            description = "古老的祭坛散发着诡异的光芒",
            minFloor = 5,
            maxFloor = 50,
            weight = 0.08f,
            onEnter = () =>
            {
                CompleteGameSystem.Instance.AddCombatLog("⛫ 回声祭坛散发着神秘的光芒");
                CompleteGameSystem.Instance.TriggerAltarIfAvailable();
            }
        });

        specialFloors.Add(new SpecialFloor
        {
            type = SpecialFloorType.Treasure,
            name = "宝藏室",
            description = "闪闪发光的财宝",
            minFloor = 4,
            maxFloor = 50,
            weight = 0.07f,
            onEnter = () =>
            {
                var p = GameManager.Instance.Player;
                int floor = GameManager.Instance.CurrentFloor;
                int ep = 50 + floor * 10 + Random.Range(0, 51);
                p.evolutionPoints += ep;
                CompleteGameSystem.Instance.AddCombatLog($"◇ 发现宝藏！+{ep}EP");
            }
        });

        specialFloors.Add(new SpecialFloor
        {
            type = SpecialFloorType.Trap,
            name = "陷阱",
            description = "小心！",
            minFloor = 2,
            maxFloor = 50,
            weight = 0.1f,
            onEnter = () =>
            {
                var p = GameManager.Instance.Player;
                int damage = Mathf.FloorToInt(p.maxHp * 0.15f);
                p.hp = Mathf.Max(1, p.hp - damage);
                CompleteGameSystem.Instance.AddCombatLog($"⚠️ 触发陷阱！受到 {damage} 点伤害");
            }
        });

        specialFloors.Add(new SpecialFloor
        {
            type = SpecialFloorType.Mystery,
            name = "神秘房间",
            description = "充满未知...",
            minFloor = 6,
            maxFloor = 50,
            weight = 0.05f,
            onEnter = () =>
            {
                var p = GameManager.Instance.Player;
                int rand = Random.Range(0, 6);
                switch (rand)
                {
                    case 0:
                        int heal = Mathf.FloorToInt(p.maxHp * 0.25f);
                        p.hp = Mathf.Min(p.maxHp, p.hp + heal);
                        CompleteGameSystem.Instance.AddCombatLog($"✨ 神秘祝福！恢复 {heal} HP");
                        break;
                    case 1:
                        p.attack += 2;
                        p.defense += 2;
                        CompleteGameSystem.Instance.AddCombatLog($"⚡ 神秘力量！ATK+2 DEF+2");
                        break;
                    case 2:
                        p.evolutionPoints += 100;
                        CompleteGameSystem.Instance.AddCombatLog($"✦ 神秘馈赠！+100EP");
                        break;
                    case 3:
                        p.pollution = Mathf.Min(100f, p.pollution + 15f);
                        CompleteGameSystem.Instance.AddCombatLog($"☢️ 神秘腐蚀！污染+15%");
                        break;
                    case 4:
                        int dmg = Mathf.FloorToInt(p.maxHp * 0.2f);
                        p.hp = Mathf.Max(1, p.hp - dmg);
                        CompleteGameSystem.Instance.AddCombatLog($"◎ 神秘攻击！受到 {dmg} 点伤害");
                        break;
                    case 5:
                        int bonusEp = Random.Range(80, 160);
                        p.evolutionPoints += bonusEp;
                        CompleteGameSystem.Instance.AddCombatLog($"✦ 神秘宝箱！获得{bonusEp}EP");
                        break;
                }
            }
        });

        CacheTotalWeight();
    }

    void InitializeBossRush()
    {
        specialFloors.Add(new SpecialFloor
        {
            type = SpecialFloorType.BossRush,
            name = "波次挑战",
            description = "连续击败多波敌人",
            minFloor = 10,
            maxFloor = 50,
            weight = 0.04f,
            onEnter = () =>
            {
                CompleteGameSystem.Instance.AddCombatLog("⚔️ 波次挑战！连续战斗即将开始！");
                CompleteGameSystem.Instance.StartWaveDefense();
            }
        });
        CacheTotalWeight();
    }

    void CacheTotalWeight()
    {
        totalWeightCache = 0f;
        foreach (var sf in specialFloors)
        {
            totalWeightCache += sf.weight;
        }
    }

    public SpecialFloorType GetSpecialFloorType(int floor)
    {
        if (floorAssignments.ContainsKey(floor))
            return floorAssignments[floor];
        
        // BOSS floors
        if (floor % 10 == 0)
            return SpecialFloorType.Boss;
        
        // Event floors
        if (StorySystem.Instance != null && StorySystem.Instance.storyTriggers.ContainsKey(floor))
            return SpecialFloorType.Event;
        
        return SpecialFloorType.None;
    }

    public void AssignSpecialFloor(int floor)
    {
        if (floor % 10 == 0)
        {
            floorAssignments[floor] = SpecialFloorType.Boss;
            return;
        }

        if (StorySystem.Instance != null && StorySystem.Instance.storyTriggers.ContainsKey(floor))
        {
            floorAssignments[floor] = SpecialFloorType.Event;
            return;
        }

        // Random special floor
        float random = Random.value * totalWeightCache;
        float current = 0;

        foreach (var sf in specialFloors)
        {
            current += sf.weight;
            if (random <= current && floor >= sf.minFloor && floor <= sf.maxFloor)
            {
                floorAssignments[floor] = sf.type;
                return;
            }
        }

        floorAssignments[floor] = SpecialFloorType.None;
    }

    public void ForceSpecialFloor(int floor, SpecialFloorType type)
    {
        floorAssignments[floor] = type;
        var sf = specialFloors.Find(s => s.type == type);
        if (sf.onEnter != null)
        {
            sf.onEnter();
        }
    }

    public void OnEnterFloor(int floor)
    {
        SpecialFloorType type = GetSpecialFloorType(floor);

        if (type == SpecialFloorType.Boss)
        {
            CompleteGameSystem.Instance.AddCombatLog($"◎ BOSS层！准备战斗！");
        }
        else if (type == SpecialFloorType.Event)
        {
            StorySystem.Instance.CheckStoryTrigger(floor);
        }
    }

    public bool IsSpecialFloor(int floor)
    {
        return GetSpecialFloorType(floor) != SpecialFloorType.None;
    }

    public string GetSpecialFloorName(int floor)
    {
        var type = GetSpecialFloorType(floor);
        switch (type)
        {
            case SpecialFloorType.Shop: return "† 商店";
            case SpecialFloorType.Rest: return "◇ 休息室";
            case SpecialFloorType.Boss: return "◎ BOSS层";
            case SpecialFloorType.Altar: return "⛫ 回声祭坛";
            case SpecialFloorType.Treasure: return "◇ 宝藏室";
            case SpecialFloorType.Trap: return "⚠️ 陷阱";
            case SpecialFloorType.Mystery: return "❓ 神秘房间";
            case SpecialFloorType.BossRush: return "⚔️ 波次挑战";
            case SpecialFloorType.Event: return "◆ 剧情事件";
            default: return "";
        }
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        specialFloors.Clear();
        floorAssignments.Clear();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
