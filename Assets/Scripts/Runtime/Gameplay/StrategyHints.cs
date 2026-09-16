using UnityEngine;
using System.Collections.Generic;

public class StrategyHint
{
    public string id;
    public string title;
    public string description;
    public string icon;
    public int priority;
    public bool hasShown;
}

public class StrategyHintsSystem
{
    private CompleteGameSystem gameSystem;
    private List<StrategyHint> hints = new List<StrategyHint>();
    private float lastHintTime;
    private float hintInterval = 30f;

    public StrategyHintsSystem(CompleteGameSystem system)
    {
        gameSystem = system;
        InitializeHints();
    }

    private void InitializeHints()
    {
        hints.Add(new StrategyHint {
            id = "low_hp",
            title = "⚠ 低血量警告",
            description = "你的血量很低！尝试防御回复或寻找回复道具",
            icon = "❤️",
            priority = 10
        });

        hints.Add(new StrategyHint {
            id = "high_pollution",
            title = "☢ 污染过高",
            description = "污染值过高！考虑净化或使用污染被动技能",
            icon = "☢️",
            priority = 9
        });

        hints.Add(new StrategyHint {
            id = "possess_window",
            title = "◈ 附身窗口",
            description = "敌人进入可附身状态！抓住机会尝试附身",
            icon = "★",
            priority = 8
        });

        hints.Add(new StrategyHint {
            id = "elite_monster",
            title = "⭐ 精英怪物",
            description = "发现精英怪物！击败它可获得丰厚奖励",
            icon = "⭐",
            priority = 7
        });

        hints.Add(new StrategyHint {
            id = "boss_warning",
            title = "◎ Boss警告",
            description = "Boss出现！做好战斗准备，注意它的特性",
            icon = "◎",
            priority = 11
        });

        hints.Add(new StrategyHint {
            id = "combo_bonus",
            title = "★ 连击加成",
            description = "保持连击可获得伤害加成！",
            icon = "★",
            priority = 6
        });

        hints.Add(new StrategyHint {
            id = "form_switch",
            title = "↩ 形态切换",
            description = "尝试切换形态来应对不同敌人",
            icon = "↩",
            priority = 5
        });

        hints.Add(new StrategyHint {
            id = "skill_ready",
            title = "✨ 技能就绪",
            description = "你的技能已经准备好了！",
            icon = "✨",
            priority = 7
        });

        hints.Add(new StrategyHint {
            id = "evolution_point",
            title = "◆ 进化点",
            description = "你有足够的进化点！考虑强化能力",
            icon = "◆",
            priority = 4
        });

        hints.Add(new StrategyHint {
            id = "rare_item",
            title = "◇ 稀有发现",
            description = "发现稀有物品！记得收集",
            icon = "◇",
            priority = 8
        });
    }

    public void Update()
    {
        if (Time.time - lastHintTime < hintInterval) return;

        List<StrategyHint> availableHints = GetAvailableHints();
        
        if (availableHints.Count > 0)
        {
            availableHints.Sort((a, b) => b.priority.CompareTo(a.priority));
            ShowHint(availableHints[0]);
            lastHintTime = Time.time;
        }
    }

    private List<StrategyHint> GetAvailableHints()
    {
        List<StrategyHint> available = new List<StrategyHint>();
        var player = GameManager.Instance.Player;

        if (player.hp < player.maxHp * 0.2f)
        {
            available.Add(hints.Find(h => h.id == "low_hp"));
        }

        if (player.pollution > 70f)
        {
            available.Add(hints.Find(h => h.id == "high_pollution"));
        }

        if (gameSystem.CurrentEnemy != null && gameSystem.CurrentEnemy.isElite)
        {
            available.Add(hints.Find(h => h.id == "elite_monster"));
        }

        if (gameSystem.CurrentEnemy != null && gameSystem.CurrentEnemy.isBoss)
        {
            available.Add(hints.Find(h => h.id == "boss_warning"));
        }

        // 连击加成提示 (需要在CompleteGameSystem中实现ComboCount属性后启用)
        // if (gameSystem.ComboCount >= 5)
        // {
        //     available.Add(hints.Find(h => h.id == "combo_bonus"));
        // }

        if (player.evolutionPoints >= 500)
        {
            available.Add(hints.Find(h => h.id == "evolution_point"));
        }

        return available;
    }

    public void ShowHint(StrategyHint hint)
    {
        gameSystem.AddCombatLog($"<color=#ffff00><b>{hint.icon} {hint.title}</b></color>\n{hint.description}");
    }

    public void ShowSpecificHint(string hintId)
    {
        StrategyHint hint = hints.Find(h => h.id == hintId);
        if (hint != null)
        {
            ShowHint(hint);
        }
    }

    public void ResetHintStates()
    {
        foreach (var hint in hints)
        {
            hint.hasShown = false;
        }
    }

    public void SetHintInterval(float interval)
    {
        hintInterval = interval;
    }
}