using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public struct RareMonster
{
    public string id;
    public string name;
    public string originalMonsterId;
    public float spawnChance;
    public int minFloor;
    public int hpMultiplier;
    public int atkMultiplier;
    public string[] extraTraits;
}

public struct RareEvent
{
    public string id;
    public string name;
    public string description;
    public float triggerChance;
    public int minFloor;
    public List<EventOption> options;
}

public struct EventOption
{
    public string text;
    public string result;
    public Dictionary<string, object> effects;
}

public class RareContentSystem
{
    private CompleteGameSystem gameSystem;
    
    // 稀有怪物列表
    private List<RareMonster> rareMonsters = new List<RareMonster>();
    
    // 稀有事件列表
    private List<RareEvent> rareEvents = new List<RareEvent>();

    public RareContentSystem(CompleteGameSystem system)
    {
        gameSystem = system;
        InitializeRareContent();
    }

    // 初始化稀有内容
    private void InitializeRareContent()
    {
        // 稀有怪物
        rareMonsters.Add(new RareMonster {
            id = "rare_rat",
            name = "变异老鼠王",
            originalMonsterId = "rat",
            spawnChance = 0.02f,
            minFloor = 1,
            hpMultiplier = 3,
            atkMultiplier = 2,
            extraTraits = new[] { "暴击", "多重攻击" }
        });

        rareMonsters.Add(new RareMonster {
            id = "rare_dog",
            name = "幽灵犬",
            originalMonsterId = "dog",
            spawnChance = 0.015f,
            minFloor = 5,
            hpMultiplier = 4,
            atkMultiplier = 3,
            extraTraits = new[] { "不死", "恐惧" }
        });

        rareMonsters.Add(new RareMonster {
            id = "rare_spider",
            name = "剧毒蜘蛛女王",
            originalMonsterId = "spider",
            spawnChance = 0.02f,
            minFloor = 10,
            hpMultiplier = 3,
            atkMultiplier = 2,
            extraTraits = new[] { "毒素", "蛛网", "召唤" }
        });

        rareMonsters.Add(new RareMonster {
            id = "rare_golem",
            name = "远古石巨人",
            originalMonsterId = "golem",
            spawnChance = 0.01f,
            minFloor = 15,
            hpMultiplier = 5,
            atkMultiplier = 4,
            extraTraits = new[] { "护甲", "厚皮", "不死" }
        });

        // 稀有事件
        rareEvents.Add(new RareEvent {
            id = "mysterious_shrine",
            name = "神秘祭坛",
            description = "你发现了一座古老的祭坛，散发着诡异的光芒...",
            triggerChance = 0.05f,
            minFloor = 3,
            options = new List<EventOption> {
                new EventOption {
                    text = "祈祷",
                    result = "祭坛发出光芒，你获得了祝福！",
                    effects = new Dictionary<string, object> { { "hpHeal", 30 }, { "pollutionReduce", 10 } }
                },
                new EventOption {
                    text = "献祭",
                    result = "你献出了部分生命力，获得了强大的力量！",
                    effects = new Dictionary<string, object> { { "atkBoost", 20 }, { "hpDamage", 20 } }
                },
                new EventOption {
                    text = "离开",
                    result = "你谨慎地离开了祭坛...",
                    effects = new Dictionary<string, object>()
                }
            }
        });

        rareEvents.Add(new RareEvent {
            id = "treasure_chest",
            name = "神秘宝箱",
            description = "一个闪闪发光的宝箱出现在你面前！",
            triggerChance = 0.03f,
            minFloor = 5,
            options = new List<EventOption> {
                new EventOption {
                    text = "打开",
                    result = "宝箱中装满了进化点！",
                    effects = new Dictionary<string, object> { { "evoPoints", 200 } }
                },
                new EventOption {
                    text = "谨慎离开",
                    result = "你觉得这可能是陷阱...",
                    effects = new Dictionary<string, object>()
                }
            }
        });

        rareEvents.Add(new RareEvent {
            id = "traveling_merchant",
            name = "旅行商人",
            description = "一位神秘的商人出现在你面前，他的货物看起来很特别...",
            triggerChance = 0.04f,
            minFloor = 8,
            options = new List<EventOption> {
                new EventOption {
                    text = "购买净化药剂",
                    result = "你购买了一瓶净化药剂",
                    effects = new Dictionary<string, object> { { "pollutionReduce", 30 }, { "evoCost", 100 } }
                },
                new EventOption {
                    text = "购买力量卷轴",
                    result = "你购买了一张力量卷轴",
                    effects = new Dictionary<string, object> { { "atkBoost", 10 }, { "evoCost", 150 } }
                },
                new EventOption {
                    text = "离开",
                    result = "商人消失在迷雾中...",
                    effects = new Dictionary<string, object>()
                }
            }
        });
    }

    // 尝试生成稀有怪物
    public MonsterRuntime TrySpawnRareMonster(int floor)
    {
        foreach (var rare in rareMonsters)
        {
            if (floor >= rare.minFloor && Random.value < rare.spawnChance)
            {
                return CreateRareMonster(rare);
            }
        }
        return null;
    }

    // 创建稀有怪物
    private MonsterRuntime CreateRareMonster(RareMonster rare)
    {
        var def = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == rare.originalMonsterId);
        if (string.IsNullOrEmpty(def.id)) return null;

        var monster = new MonsterRuntime();
        monster.id = rare.id;
        monster.name = rare.name;
        monster.hp = def.hp * rare.hpMultiplier;
        monster.maxHp = monster.hp;
        monster.atk = def.atk * rare.atkMultiplier;
        monster.def = def.def;
        monster.zone = def.zone;
        monster.isBoss = false;
        monster.isElite = true;
        
        // 合并原特性和额外特性
        List<string> traits = new List<string>(def.traits ?? new string[]{});
        traits.AddRange(rare.extraTraits);
        monster.traits = traits.ToArray();
        
        monster.color = new Color(0.9f, 0.7f, 0.3f); // 金色表示稀有
        monster.possessBaseChance = 0.1f * 0.5f; // 稀有怪物更难附身

        gameSystem.AddCombatLog($"<color=#ffd700><b>⭐ 发现稀有怪物: {monster.name}！</b></color>");
        return monster;
    }

    // 尝试触发稀有事件
    public RareEvent TryTriggerRareEvent(int floor)
    {
        foreach (var rareEvent in rareEvents)
        {
            if (floor >= rareEvent.minFloor && Random.value < rareEvent.triggerChance)
            {
                return rareEvent;
            }
        }
        return default;
    }

    // 处理事件选择
    public string HandleEventChoice(RareEvent rareEvent, int optionIndex)
    {
        if (optionIndex < 0 || optionIndex >= rareEvent.options.Count)
            return "无效选项";

        var option = rareEvent.options[optionIndex];
        var player = GameManager.Instance.Player;

        // 应用效果
        foreach (var effect in option.effects)
        {
            switch (effect.Key)
            {
                case "hpHeal":
                    player.hp = Mathf.Min(player.maxHp, player.hp + (int)effect.Value);
                    break;
                case "hpDamage":
                    player.hp = Mathf.Max(1, player.hp - (int)effect.Value);
                    break;
                case "pollutionReduce":
                    player.pollution = Mathf.Max(0, player.pollution - (int)effect.Value);
                    break;
                case "atkBoost":
                    player.attack += (int)effect.Value;
                    break;
                case "evoPoints":
                    player.evolutionPoints += (int)effect.Value;
                    break;
                case "evoCost":
                    player.evolutionPoints -= (int)effect.Value;
                    break;
            }
        }

        GameManager.Instance.NotifyPlayerStatsChanged();
        return option.result;
    }

    // 获取所有稀有怪物ID
    public List<string> GetAllRareMonsterIds()
    {
        return rareMonsters.ConvertAll(m => m.id);
    }

    // 获取所有稀有事件ID
    public List<string> GetAllRareEventIds()
    {
        return rareEvents.ConvertAll(e => e.id);
    }
}