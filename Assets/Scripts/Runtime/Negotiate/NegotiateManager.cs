using UnityEngine;
using System.Collections.Generic;

public class NegotiateManager : SingletonBase<NegotiateManager>
{
    private void Awake()
    {
        base.Awake();
    }

    public NegotiationResult Negotiate(MonsterBase monster, NegotiationStrategy strategy)
    {
        NegotiationResult result = new NegotiationResult();
        result.success = false;
        result.monster = monster;
        result.strategy = strategy;

        float baseChance = CalculateBaseChance(monster);
        float strategyBonus = GetStrategyBonus(strategy);
        float playerBonus = GetPlayerBonus();
        float pollutionPenalty = GetPollutionPenalty();

        float finalChance = baseChance + strategyBonus + playerBonus + pollutionPenalty;

        if (Random.value < finalChance)
        {
            result.success = true;
            result.reward = GenerateReward(monster);
            result.message = GetSuccessMessage(monster, strategy);
        }
        else
        {
            result.message = GetFailureMessage(monster, strategy);
        }

        return result;
    }

    private float CalculateBaseChance(MonsterBase monster)
    {
        if (monster == null || monster.Data == null) return 0f;

        float baseChance = 0.3f;

        if (monster.Data.isBoss)
        {
            baseChance = 0.05f;
        }
        else if (monster.Data.isElite)
        {
            baseChance = 0.15f;
        }

        float hpPercent = monster.maxHp > 0 ? (float)monster.currentHp / (float)monster.maxHp : 1f;
        if (hpPercent < 0.3f)
        {
            baseChance += 0.2f;
        }
        else if (hpPercent < 0.5f)
        {
            baseChance += 0.1f;
        }

        return baseChance;
    }

    private float GetStrategyBonus(NegotiationStrategy strategy)
    {
        switch (strategy)
        {
            case NegotiationStrategy.Intimidate:
                return 0.15f;
            case NegotiationStrategy.Bribe:
                return 0.2f;
            case NegotiationStrategy.Promise:
                return 0.1f;
            case NegotiationStrategy.Threaten:
                return 0.05f;
            default:
                return 0f;
        }
    }

    private float GetPlayerBonus()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return 0f;

        float bonus = 0f;

        bonus += player.possessionBonus;

        return bonus;
    }

    private float GetPollutionPenalty()
    {
        var player = GameManager.Instance.Player;
        if (player == null) return 0f;

        return -player.pollution / 200f;
    }

    private NegotiationReward GenerateReward(MonsterBase monster)
    {
        NegotiationReward reward = new NegotiationReward();

        reward.gold = Random.Range(monster.Data.minGold, monster.Data.maxGold);

        if (Random.value < 0.3f)
        {
            reward.fragmentDrop = true;
        }

        if (Random.value < 0.15f)
        {
            reward.formUnlocked = true;
        }

        return reward;
    }

    private string GetSuccessMessage(MonsterBase monster, NegotiationStrategy strategy)
    {
        string[] messages = new string[]
        {
            $"{monster.Data.monsterName}被你说服了，它决定离开。",
            $"{monster.Data.monsterName}同意你的提议，转身离去。",
            $"你成功与{monster.Data.monsterName}达成协议。",
            $"{monster.Data.monsterName}选择相信你。"
        };

        return messages[Random.Range(0, messages.Length)];
    }

    private string GetFailureMessage(MonsterBase monster, NegotiationStrategy strategy)
    {
        string[] messages = new string[]
        {
            $"{monster.Data.monsterName}拒绝了你的提议！",
            $"谈判失败，{monster.Data.monsterName}变得更加愤怒！",
            $"{monster.Data.monsterName}不相信你的话。",
            $"你的话对{monster.Data.monsterName}没有效果。"
        };

        return messages[Random.Range(0, messages.Length)];
    }

    public List<NegotiationOption> GetAvailableOptions()
    {
        List<NegotiationOption> options = new List<NegotiationOption>();

        options.Add(new NegotiationOption
        {
            strategy = NegotiationStrategy.Intimidate,
            name = "威慑",
            description = "用力量震慑敌人，成功率+15%",
            icon = "▲",
            cost = null
        });

        options.Add(new NegotiationOption
        {
            strategy = NegotiationStrategy.Bribe,
            name = "贿赂",
            description = "用金币收买敌人，成功率+20%，消耗50金币",
            icon = "$",
            cost = new NegotiationCost { type = CostType.Gold, value = 50 }
        });

        options.Add(new NegotiationOption
        {
            strategy = NegotiationStrategy.Promise,
            name = "承诺",
            description = "承诺不伤害对方，成功率+10%",
            icon = "◈",
            cost = null
        });

        options.Add(new NegotiationOption
        {
            strategy = NegotiationStrategy.Threaten,
            name = "威胁",
            description = "威胁敌人，成功率+5%，失败会激怒对方",
            icon = "⚔️",
            cost = null
        });

        return options;
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

[System.Serializable]
public class NegotiationResult
{
    public bool success;
    public MonsterBase monster;
    public NegotiationStrategy strategy;
    public NegotiationReward reward;
    public string message;
}

[System.Serializable]
public class NegotiationReward
{
    public int gold;
    public bool fragmentDrop;
    public bool formUnlocked;
}

[System.Serializable]
public class NegotiationOption
{
    public NegotiationStrategy strategy;
    public string name;
    public string description;
    public string icon;
    public NegotiationCost cost;
}

[System.Serializable]
public class NegotiationCost
{
    public CostType type;
    public int value;
}

public enum NegotiationStrategy
{
    Intimidate,
    Bribe,
    Promise,
    Threaten
}

public enum CostType
{
    Gold,
    Hp,
    Pollution
}
