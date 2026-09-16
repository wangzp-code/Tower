using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class FloorEventManager : SingletonBase<FloorEventManager>
{
    [Serializable]
    public class ActiveFloorEvent
    {
        public string configId;
        public int floor;
        public bool completed;
        public int choiceIndex;
        public string outcome;
    }

    private FloorEventConfig[] _configs;
    private ActiveFloorEvent _currentEvent;
    private List<string> _triggeredThisRun = new List<string>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        _configs = DataConfigManager.Instance?.GetFloorEventConfigs() ?? new FloorEventConfig[0];

    }

    #region Event Triggering
    public void OnNewRun()
    {
        _triggeredThisRun.Clear();
        _currentEvent = null;
    }

    public FloorEventConfig TryTriggerEvent(int floor)
    {
        if (_configs == null || _configs.Length == 0) return null;
        if (_currentEvent != null && !_currentEvent.completed) return null;

        var candidates = GetCandidateEvents(floor);
        if (candidates.Count == 0) return null;

        foreach (var config in candidates)
        {
            if (UnityEngine.Random.value <= config.probability)
            {
                return ActivateEvent(config, floor);
            }
        }

        // 第2层保底触发一个事件
        if (floor == 2 && candidates.Count > 0)
        {
            return ActivateEvent(candidates[0], floor);
        }

        return null;
    }

    FloorEventConfig ActivateEvent(FloorEventConfig config, int floor)
    {
        _currentEvent = new ActiveFloorEvent
        {
            configId = config.id,
            floor = floor,
            completed = false,
            choiceIndex = -1
        };
        _triggeredThisRun.Add(config.id);

        EventBus.Emit(EventTypes.FloorEventStart, config.id);

        return config;
    }

    List<FloorEventConfig> GetCandidateEvents(int floor)
    {
        var result = new List<FloorEventConfig>();
        foreach (var config in _configs)
        {
            if (floor < config.floorMin || floor > config.floorMax) continue;
            if (_triggeredThisRun.Contains(config.id)) continue;
            result.Add(config);
        }

        for (int i = result.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var temp = result[i];
            result[i] = result[j];
            result[j] = temp;
        }

        return result;
    }
    #endregion

    #region Event Resolution
    public struct EventResult
    {
        public bool success;
        public string message;
        public string rewardType;
        public float rewardValue;
        public float pollutionChange;
    }

    public EventResult ResolveChoice(int choiceIndex)
    {
        var result = new EventResult();
        if (_currentEvent == null || _currentEvent.completed)
        {
            result.message = "没有进行中的事件";
            return result;
        }

        var config = GetConfig(_currentEvent.configId);
        if (config == null)
        {
            result.message = "事件配置丢失";
            return result;
        }

        _currentEvent.choiceIndex = choiceIndex;
        _currentEvent.completed = true;

        result = ApplyEventReward(config, choiceIndex);
        result.success = true;

        EventBus.Emit(EventTypes.FloorEventComplete, config.id);

        return result;
    }

    EventResult ApplyEventReward(FloorEventConfig config, int choiceIndex)
    {
        var result = new EventResult();
        var player = GameManager.Instance?.Player;
        if (player == null) return result;

        bool isPositiveChoice = (choiceIndex == 0);

        switch (config.eventType)
        {
            case "reward":
                ApplyReward(config, player, ref result);
                break;

            case "choice":
                if (isPositiveChoice)
                    ApplyReward(config, player, ref result);
                else
                    result.message = "你选择了离开";
                break;

            case "risk":
                if (isPositiveChoice)
                {
                    if (UnityEngine.Random.value > 0.4f)
                    {
                        ApplyReward(config, player, ref result);
                        result.pollutionChange = 5;
                        PollutionSystem.Instance?.GainPollution(5);
                    }
                    else
                    {
                        int dmg = Mathf.RoundToInt(player.maxHp * 0.15f);
                        player.hp = Mathf.Max(1, player.hp - dmg);
                        result.message = $"冒险失败！受到 {dmg} 伤害";
                        result.pollutionChange = 8;
                        PollutionSystem.Instance?.GainPollution(8);
                    }
                }
                else
                {
                    result.message = "你安全绕开了";
                }
                break;

            case "shop":
                result.message = "暗影商人出现了";
                ShopManager.Instance?.GenerateShopItems(GameManager.Instance.CurrentFloor);
                CanvasUIManager.Instance?.ShowItemShopPanel();
                break;

            case "challenge":
                if (isPositiveChoice)
                {
                    float pollution = player.pollution;
                    if (pollution >= 50)
                    {
                        ApplyReward(config, player, ref result);
                    }
                    else
                    {
                        result.message = "污染值不足，共鸣失败";
                    }
                }
                else
                {
                    result.message = "你离开了";
                }
                break;

            case "story":
                result.message = config.desc;
                if (config.rewardType == "pollution_reduce" && config.rewardValue > 0)
                {
                    PollutionSystem.Instance?.ReducePollution(config.rewardValue);
                    result.pollutionChange = -config.rewardValue;
                    result.message += $"\n污染降低 {config.rewardValue}";
                }
                break;
        }

        EventBus.Emit(EventTypes.PlayerStatsChanged);
        return result;
    }

    void ApplyReward(FloorEventConfig config, GameManager.PlayerData player, ref EventResult result)
    {
        result.rewardType = config.rewardType;
        result.rewardValue = config.rewardValue;

        switch (config.rewardType)
        {
            case "ep":
                player.evolutionPoints += Mathf.RoundToInt(config.rewardValue);
                result.message = $"获得 {Mathf.RoundToInt(config.rewardValue)} EP";
                break;

            case "heal":
                int heal = Mathf.RoundToInt(player.maxHp * config.rewardValue);
                player.hp = Mathf.Min(player.maxHp, player.hp + heal);
                result.message = $"恢复 {heal} HP";
                break;

            case "stat_buff":
                int buffVal = Mathf.RoundToInt(config.rewardValue);
                player.attack += buffVal;
                player.baseAttack += buffVal;
                result.message = $"攻击 +{buffVal}";
                break;

            case "form":
                var allMonsters = GameDataImporter.MonsterDefinitions;
                var unowned = new List<string>();
                foreach (var m in allMonsters)
                {
                    if (!m.boss && !player.ownedForms.Contains(m.id))
                        unowned.Add(m.id);
                }
                if (unowned.Count > 0)
                {
                    string newForm = unowned[UnityEngine.Random.Range(0, unowned.Count)];
                    FormManager.Instance?.TryAddForm(newForm);
                    result.message = $"获得新形态: {newForm}";
                }
                else
                {
                    int bonusEp = 80;
                    player.evolutionPoints += bonusEp;
                    result.message = $"无新形态可获取，改为+{bonusEp}EP";
                }
                break;

            case "item":
                result.message = "获得一件道具";
                break;

            case "legacy_progress":
                LegacyManager.Instance?.CheckUnlockConditions();
                result.message = "遗产进度推进";
                break;

            case "pollution_reduce":
                PollutionSystem.Instance?.ReducePollution(config.rewardValue);
                result.pollutionChange = -config.rewardValue;
                result.message = $"污染降低 {config.rewardValue}";
                break;

            default:
                result.message = config.desc;
                break;
        }
    }
    #endregion

    #region Query API
    public FloorEventConfig GetConfig(string id)
    {
        if (_configs == null) return null;
        return Array.Find(_configs, c => c.id == id);
    }

    public FloorEventConfig[] GetAllConfigs() => _configs ?? new FloorEventConfig[0];

    public ActiveFloorEvent GetCurrentEvent() => _currentEvent;

    public bool HasActiveEvent() => _currentEvent != null && !_currentEvent.completed;

    public List<string> GetTriggeredEventsThisRun() => new List<string>(_triggeredThisRun);
    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _currentEvent = null;
        _triggeredThisRun.Clear();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _currentEvent = null;
    }
}
