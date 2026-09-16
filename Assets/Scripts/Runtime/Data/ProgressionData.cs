using UnityEngine;
using System;
using System.Collections.Generic;

public static class EvolutionData
{
    [Serializable]
    public class EvolutionNode
    {
        public int level;
        public string name;
        public int epCost;
        public string description;
        public Action<GameManager.PlayerData> apply;
    }

    public static readonly Dictionary<string, List<EvolutionNode>> Trees = new Dictionary<string, List<EvolutionNode>>
    {
        ["titan"] = new List<EvolutionNode>
        {
            new EvolutionNode { level = 1, name = "铁壁", epCost = 200, description = "DEF+3",
                apply = p => { p.defense += 3; p.baseDefense += 3; } },
            new EvolutionNode { level = 2, name = "巨力", epCost = 500, description = "ATK+5, MaxHP+60",
                apply = p => { p.attack += 5; p.baseAttack += 5; p.maxHp += 60; p.baseMaxHp += 60; p.hp += 60; } },
            new EvolutionNode { level = 3, name = "再生", epCost = 1000, description = "每层恢复12%HP",
                apply = p => {} },
            new EvolutionNode { level = 4, name = "不屈", epCost = 1800, description = "MaxHP+100, DEF+5, 受伤-8%",
                apply = p => { p.maxHp += 100; p.baseMaxHp += 100; p.hp += 100; p.defense += 5; p.baseDefense += 5; } },
            new EvolutionNode { level = 5, name = "泰坦之躯", epCost = 3200, description = "MaxHP+150, ATK+10, DEF+8",
                apply = p => { p.maxHp += 150; p.baseMaxHp += 150; p.hp += 150; p.attack += 10; p.baseAttack += 10; p.defense += 8; p.baseDefense += 8; } }
        },
        ["ghost"] = new List<EvolutionNode>
        {
            new EvolutionNode { level = 1, name = "虚化", epCost = 200, description = "受伤-8%",
                apply = p => {} },
            new EvolutionNode { level = 2, name = "吸取", epCost = 500, description = "攻击回复15%伤害",
                apply = p => {} },
            new EvolutionNode { level = 3, name = "相位", epCost = 1000, description = "附身成功率+12%",
                apply = p => p.possessionBonus += 0.12f },
            new EvolutionNode { level = 4, name = "暗影", epCost = 1800, description = "ATK+10, 受伤-12%",
                apply = p => { p.attack += 10; p.baseAttack += 10; } },
            new EvolutionNode { level = 5, name = "幽灵形态", epCost = 3200, description = "ATK+15, 吸血25%, 附身+15%",
                apply = p => { p.attack += 15; p.baseAttack += 15; p.possessionBonus += 0.15f; } }
        },
        ["swarm"] = new List<EvolutionNode>
        {
            new EvolutionNode { level = 1, name = "毒刺", epCost = 200, description = "ATK+3, 额外伤害+8%",
                apply = p => { p.attack += 3; p.baseAttack += 3; } },
            new EvolutionNode { level = 2, name = "群体", epCost = 600, description = "击败怪物额外获得3EP",
                apply = p => {} },
            new EvolutionNode { level = 3, name = "寄生", epCost = 1200, description = "附身+10%, 污染+5变为+3",
                apply = p => p.possessionBonus += 0.10f },
            new EvolutionNode { level = 4, name = "虫群", epCost = 2000, description = "ATK+10, 额外伤害15%",
                apply = p => { p.attack += 10; p.baseAttack += 10; } },
            new EvolutionNode { level = 5, name = "虫群意志", epCost = 3500, description = "ATK+12, 额外伤害20%, 击败+5EP",
                apply = p => { p.attack += 12; p.baseAttack += 12; } }
        },
        ["blood"] = new List<EvolutionNode>
        {
            new EvolutionNode { level = 1, name = "鲜血渴望", epCost = 200, description = "攻击吸血10%",
                apply = p => {} },
            new EvolutionNode { level = 2, name = "血祭", epCost = 500, description = "ATK+5, HP<50%时ATK+30%",
                apply = p => { p.attack += 5; p.baseAttack += 5; } },
            new EvolutionNode { level = 3, name = "血池", epCost = 1000, description = "击杀恢复25%MaxHP",
                apply = p => {} },
            new EvolutionNode { level = 4, name = "血脉觉醒", epCost = 1800, description = "ATK+8, 吸血+15%",
                apply = p => { p.attack += 8; p.baseAttack += 8; } },
            new EvolutionNode { level = 5, name = "不死血王", epCost = 3200, description = "ATK+12, 吸血30%, HP<20%免死",
                apply = p => { p.attack += 12; p.baseAttack += 12; } }
        },
        ["mech"] = new List<EvolutionNode>
        {
            new EvolutionNode { level = 1, name = "纳米护盾", epCost = 200, description = "每层获得20护盾",
                apply = p => { p.defense += 2; p.baseDefense += 2; } },
            new EvolutionNode { level = 2, name = "过载充能", epCost = 500, description = "ATK+4, 护盾>50时ATK+20%",
                apply = p => { p.attack += 4; p.baseAttack += 4; } },
            new EvolutionNode { level = 3, name = "污染转化器", epCost = 1000, description = "污染+5变+3, 每10污染+1护盾",
                apply = p => {} },
            new EvolutionNode { level = 4, name = "装甲强化", epCost = 1800, description = "DEF+6, 护盾上限+80",
                apply = p => { p.defense += 6; p.baseDefense += 6; p.maxHp += 80; p.baseMaxHp += 80; p.hp += 80; } },
            new EvolutionNode { level = 5, name = "终极兵器", epCost = 3200, description = "ATK+10, DEF+5, 护盾满时双倍伤害",
                apply = p => { p.attack += 10; p.baseAttack += 10; p.defense += 5; p.baseDefense += 5; } }
        }
    };
}

public static class AltarData
{
    [Serializable]
    public class AltarPair
    {
        public int id;
        public string aggressiveName;
        public string aggressiveDesc;
        public Action<GameManager.PlayerData> applyAggressive;
        public string conservativeName;
        public string conservativeDesc;
        public Action<GameManager.PlayerData> applyConservative;
    }

    public static readonly AltarPair[] Pairs = new AltarPair[]
    {
        new AltarPair { id = 1, aggressiveName = "嗜血之力", aggressiveDesc = "ATK+50%, 每层-10HP",
            applyAggressive = p => { p.attack = Mathf.RoundToInt(p.attack * 1.5f); p.baseAttack = Mathf.RoundToInt(p.baseAttack * 1.5f); },
            conservativeName = "生命之泉", conservativeDesc = "每层+20HP, ATK-30%",
            applyConservative = p => { p.attack = Mathf.RoundToInt(p.attack * 0.7f); p.baseAttack = Mathf.RoundToInt(p.baseAttack * 0.7f); } },

        new AltarPair { id = 2, aggressiveName = "玻璃大炮", aggressiveDesc = "暴击+40%, DEF归零",
            applyAggressive = p => { p.defense = 0; p.baseDefense = 0; },
            conservativeName = "铁壁", conservativeDesc = "DEF×2, 无法暴击",
            applyConservative = p => { p.defense *= 2; p.baseDefense *= 2; } },

        new AltarPair { id = 3, aggressiveName = "加速代谢", aggressiveDesc = "每步+3HP, 污染+50%",
            applyAggressive = p => p.pollution = Mathf.Min(100f, p.pollution * 1.5f),
            conservativeName = "冬眠", conservativeDesc = "无污染增长, 速度-50%",
            applyConservative = p => p.pollution = Mathf.Max(0f, p.pollution - 20f) },

        new AltarPair { id = 4, aggressiveName = "贪婪", aggressiveDesc = "EP×2, 受伤+30%",
            applyAggressive = p => p.evolutionPoints *= 2,
            conservativeName = "节制", conservativeDesc = "受伤-30%, EP×0.5",
            applyConservative = p => p.evolutionPoints = Mathf.RoundToInt(p.evolutionPoints * 0.5f) },

        new AltarPair { id = 5, aggressiveName = "狂战士", aggressiveDesc = "ATK+80%, 无法逃跑",
            applyAggressive = p => { p.attack = Mathf.RoundToInt(p.attack * 1.8f); p.baseAttack = Mathf.RoundToInt(p.baseAttack * 1.8f); },
            conservativeName = "影行者", conservativeDesc = "可跳过战斗, ATK-40%",
            applyConservative = p => { p.attack = Mathf.RoundToInt(p.attack * 0.6f); p.baseAttack = Mathf.RoundToInt(p.baseAttack * 0.6f); } },

        new AltarPair { id = 6, aggressiveName = "寄生共鸣", aggressiveDesc = "附身+40%, 附身后-30%HP",
            applyAggressive = p => p.possessionBonus += 0.4f,
            conservativeName = "独立意志", conservativeDesc = "附身-20%, 每层DEF+5",
            applyConservative = p => { p.possessionBonus -= 0.2f; p.defense += 5; p.baseDefense += 5; } },

        new AltarPair { id = 7, aggressiveName = "脆弱之力", aggressiveDesc = "暴击伤害×3, 被暴击×3",
            applyAggressive = p => p.attack += 5,
            conservativeName = "坚韧", conservativeDesc = "免疫暴击, ATK-20%",
            applyConservative = p => { p.attack = Mathf.RoundToInt(p.attack * 0.8f); p.defense += 3; } },

        new AltarPair { id = 8, aggressiveName = "嗜血回复", aggressiveDesc = "击杀回20%HP, 每层+5污染",
            applyAggressive = p => {},
            conservativeName = "净化之路", conservativeDesc = "每层-5污染, 击杀无EP",
            applyConservative = p => p.pollution = Mathf.Max(0f, p.pollution - 15f) },

        new AltarPair { id = 9, aggressiveName = "赌徒", aggressiveDesc = "伤害0.3x~3x随机",
            applyAggressive = p => {},
            conservativeName = "稳定", conservativeDesc = "固定平均伤害",
            applyConservative = p => {} },

        new AltarPair { id = 10, aggressiveName = "速攻", aggressiveDesc = "先手攻击, DEF÷2",
            applyAggressive = p => { p.defense = Mathf.Max(0, p.defense / 2); p.baseDefense = Mathf.Max(0, p.baseDefense / 2); },
            conservativeName = "铁壁反击", conservativeDesc = "反弹50%受伤",
            applyConservative = p => p.defense += 4 },

        new AltarPair { id = 11, aggressiveName = "巨人化", aggressiveDesc = "HP×2, ATK×2, 每步-1HP",
            applyAggressive = p => { p.maxHp *= 2; p.baseMaxHp *= 2; p.hp *= 2; p.attack *= 2; p.baseAttack *= 2; },
            conservativeName = "小型化", conservativeDesc = "HP÷2, ATK÷2, 速度×2",
            applyConservative = p => { p.maxHp = Mathf.Max(1, p.maxHp / 2); p.baseMaxHp = Mathf.Max(1, p.baseMaxHp / 2); p.hp = Mathf.Min(p.hp, p.maxHp); p.attack = Mathf.Max(1, p.attack / 2); } },

        new AltarPair { id = 12, aggressiveName = "虹吸", aggressiveDesc = "击杀吸取30%HP",
            applyAggressive = p => {},
            conservativeName = "爆裂", conservativeDesc = "击杀爆炸30%AoE",
            applyConservative = p => {} },

        new AltarPair { id = 13, aggressiveName = "污染亲和", aggressiveDesc = "污染效果反转, 每层+10污染",
            applyAggressive = p => p.pollution = Mathf.Min(100f, p.pollution + 10f),
            conservativeName = "净化体质", conservativeDesc = "免疫污染, ATK-25%",
            applyConservative = p => { p.pollution = 0f; p.attack = Mathf.RoundToInt(p.attack * 0.75f); } },

        new AltarPair { id = 14, aggressiveName = "形态大师", aggressiveDesc = "切换无CD, ATK-15%",
            applyAggressive = p => { p.attack = Mathf.RoundToInt(p.attack * 0.85f); },
            conservativeName = "专注", conservativeDesc = "锁定形态, ATK+25%",
            applyConservative = p => { p.attack = Mathf.RoundToInt(p.attack * 1.25f); p.baseAttack = Mathf.RoundToInt(p.baseAttack * 1.25f); } },

        new AltarPair { id = 15, aggressiveName = "碎片磁铁", aggressiveDesc = "100%碎片掉率, 上限5",
            applyAggressive = p => {},
            conservativeName = "技能大师", conservativeDesc = "技能次数+2, 20%碎片率",
            applyConservative = p => {} }
    };
}

public static class PollutionPassiveData
{
    [Serializable]
    public class PollutionPassive
    {
        public float threshold;
        public string name;
        public string description;
    }

    public static readonly PollutionPassive[] Passives = new PollutionPassive[]
    {
        new PollutionPassive { threshold = 50f, name = "感官剥离", description = "无视地形伤害10秒" },
        new PollutionPassive { threshold = 70f, name = "现实扭曲", description = "重置本层敌人位置" },
        new PollutionPassive { threshold = 80f, name = "预知", description = "显示下3层地图轮廓" }
    };

    public static readonly string[] CollapseOptions = new string[]
    {
        "强制附身: 尝试附身最近怪物(60%)",
        "忍耐: 保持形态, 失去70%HP",
        "净化: 花费800EP清除污染",
        "加入: 获得形态, +20污染"
    };
}
