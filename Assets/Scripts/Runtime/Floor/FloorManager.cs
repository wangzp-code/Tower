using UnityEngine;
using System.Collections.Generic;

public class FloorManager : SingletonBase<FloorManager>
{
    [Header("Floor Settings")]
    public int bossFloorInterval = 10;
    public int shopFloorInterval = 5;
    public int eventFloorInterval = 3;
    public int maxRoomsPerFloor = 5;

    [Header("Event Settings")]
    public float treasureEventChance = 0.2f;
    public float trapEventChance = 0.15f;
    public float restEventChance = 0.1f;
    public float mysteriousEventChance = 0.1f;

    public enum FloorType
    {
        Normal,
        Boss,
        Shop,
        Event
    }

    public enum EventType
    {
        Treasure,
        Trap,
        Rest,
        Mysterious,
        Empty
    }

    protected override void Awake()
    {
        base.Awake();
    }

    public FloorType GetFloorType(int floor)
    {
        if (IsBossFloor(floor)) return FloorType.Boss;
        if (IsShopFloor(floor)) return FloorType.Shop;
        return FloorType.Normal;
    }

    public bool IsBossFloor(int floor)
    {
        switch (GameManager.Instance.CurrentMode)
        {
            case GameMode.Short:
                return floor >= GameManager.Instance.MaxFloor;
            case GameMode.Expedition:
                return floor % 5 == 0;
            default:
                return floor % bossFloorInterval == 0;
        }
    }

    public bool IsShopFloor(int floor)
    {
        if (GameManager.Instance.CurrentMode == GameMode.Expedition)
        {
            return floor == 8 || floor == 18;
        }
        return floor % shopFloorInterval == 0 && !IsBossFloor(floor);
    }

    public EventType GenerateRandomEvent()
    {
        float rand = Random.value;
        if (rand < treasureEventChance) return EventType.Treasure;
        rand -= treasureEventChance;
        if (rand < trapEventChance) return EventType.Trap;
        rand -= trapEventChance;
        if (rand < restEventChance) return EventType.Rest;
        rand -= restEventChance;
        if (rand < mysteriousEventChance) return EventType.Mysterious;
        return EventType.Empty;
    }

    public int GenerateRoomsCount(int floor)
    {
        if (IsBossFloor(floor)) return 1;
        return Random.Range(3, maxRoomsPerFloor + 1);
    }

    public int GetMonsterCount(int floor)
    {
        switch (GameManager.Instance.CurrentMode)
        {
            case GameMode.Short:
                return Mathf.Clamp(4 + floor / 2, 4, 12);
            case GameMode.Expedition:
                return Mathf.Clamp(4 + floor / 3, 4, 8);
            default:
                return Mathf.Min(5 + floor / 5, 15);
        }
    }

    public void OnEnterFloor(int floor)
    {
        FloorType type = GetFloorType(floor);
        EventBus.Emit(EventTypes.FloorEntered, floor, type);
        EventBus.Emit(EventTypes.FloorChanged, floor);

        if (IsShopFloor(floor))
        {
            ShopManager.Instance.GenerateShopItems(floor);
        }
    }

    public void OnExitFloor(int floor)
    {
        EventBus.Emit(EventTypes.FloorExited, floor);
    }

    public void HandleEvent(EventType eventType)
    {
        switch (eventType)
        {
            case EventType.Treasure:
                HandleTreasureEvent();
                break;
            case EventType.Trap:
                HandleTrapEvent();
                break;
            case EventType.Rest:
                HandleRestEvent();
                break;
            case EventType.Mysterious:
                HandleMysteriousEvent();
                break;
        }
    }

    private void HandleTreasureEvent()
    {
        var player = GameManager.Instance.Player;
        int ep = 50 + GameManager.Instance.CurrentFloor * 10 + UnityEngine.Random.Range(0, 51);
        player.evolutionPoints += ep;
        EventBus.Emit(EventTypes.EventTriggered, $"发现了宝藏！获得 {ep} EP！");
        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    private void HandleTrapEvent()
    {
        var player = GameManager.Instance.Player;
        int damage = Mathf.RoundToInt(player.maxHp * 0.1f);
        player.hp = Mathf.Max(1, player.hp - damage);
        player.pollution = Mathf.Min(100f, player.pollution + 5f);
        EventBus.Emit(EventTypes.EventTriggered, $"触发了陷阱！受到 {damage} 点伤害！");
        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    private void HandleRestEvent()
    {
        var player = GameManager.Instance.Player;
        int healAmount = Mathf.RoundToInt(player.maxHp * 0.2f);
        player.hp = Mathf.Min(player.hp + healAmount, player.maxHp);
        EventBus.Emit(EventTypes.EventTriggered, $"休息恢复了 {healAmount} 点生命值！");
        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    private void HandleMysteriousEvent()
    {
        int rand = Random.Range(0, 3);
        var player = GameManager.Instance.Player;

        switch (rand)
        {
            case 0:
                int healAmount = Mathf.RoundToInt(player.maxHp * 0.2f);
                player.hp = Mathf.Min(player.hp + healAmount, player.maxHp);
                EventBus.Emit(EventTypes.EventTriggered, $"神秘力量治愈了你！恢复了 {healAmount} 点生命值！");
                break;
            case 1:
                player.pollution = Mathf.Min(100f, player.pollution + 8f);
                player.evolutionPoints += 15;
                EventBus.Emit(EventTypes.EventTriggered, "神秘力量污染了你！污染增加并获得 EP。 ");
                break;
            case 2:
                RewardManager.Instance.GenerateRandomReward();
                EventBus.Emit(EventTypes.EventTriggered, "神秘宝箱开启！获得了奖励！");
                break;
        }

        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    public void AdvanceFloor()
    {
        GameManager.Instance.CurrentFloor++;
        var config = ConfigManager.Instance.GetGameConfig();
        PollutionSystem.Instance.ReducePollution(config.pollutionDecayPerFloor);
        OnEnterFloor(GameManager.Instance.CurrentFloor);

    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
