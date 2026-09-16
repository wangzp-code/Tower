using UnityEngine;
using System.Collections.Generic;

public class CurseManager : SingletonBase<CurseManager>
{
    private Dictionary<string, CurseData> curseDatabase = new Dictionary<string, CurseData>();
    private Dictionary<string, BlessingData> blessingDatabase = new Dictionary<string, BlessingData>();

    private void Awake()
    {
        base.Awake();
        InitializeCurses();
        InitializeBlessings();
    }

    private void InitializeCurses()
    {
        curseDatabase["脆弱"] = new CurseData
        {
            curseId = "fragile",
            name = "脆弱",
            description = "受到伤害+20%",
            icon = "♥",
            curseType = CurseType.Negative,
            effectValue = 0.2f,
            duration = -1
        };

        curseDatabase["虚弱"] = new CurseData
        {
            curseId = "weak",
            name = "虚弱",
            description = "攻击-20%",
            icon = "▼",
            curseType = CurseType.Negative,
            effectValue = -0.2f,
            duration = -1
        };

        curseDatabase["污染加速"] = new CurseData
        {
            curseId = "pollution_accel",
            name = "污染加速",
            description = "污染增长速度+50%",
            icon = "☠️",
            curseType = CurseType.Negative,
            effectValue = 0.5f,
            duration = -1
        };

        curseDatabase["盲眼"] = new CurseData
        {
            curseId = "blind",
            name = "盲眼",
            description = "无法看到怪物意图",
            icon = "◉",
            curseType = CurseType.Information,
            effectValue = 0f,
            duration = -1
        };

        curseDatabase["贪婪"] = new CurseData
        {
            curseId = "greedy",
            name = "贪婪",
            description = "商店价格+50%",
            icon = "$",
            curseType = CurseType.Negative,
            effectValue = 0.5f,
            duration = -1
        };

        curseDatabase["疫病"] = new CurseData
        {
            curseId = "plague",
            name = "疫病",
            description = "每回合损失5%最大HP",
            icon = "☢",
            curseType = CurseType.Dot,
            effectValue = 0.05f,
            duration = -1
        };

        curseDatabase["恐惧"] = new CurseData
        {
            curseId = "fear",
            name = "恐惧",
            description = "逃跑概率-30%",
            icon = "▲",
            curseType = CurseType.Negative,
            effectValue = -0.3f,
            duration = -1
        };

        curseDatabase["混乱"] = new CurseData
        {
            curseId = "confusion",
            name = "混乱",
            description = "每回合有10%概率攻击自己",
            icon = "※",
            curseType = CurseType.Chance,
            effectValue = 0.1f,
            duration = -1
        };

        curseDatabase["凋零"] = new CurseData
        {
            curseId = "wither",
            name = "凋零",
            description = "最大HP-20%",
            icon = "◇",
            curseType = CurseType.Negative,
            effectValue = -0.2f,
            duration = -1
        };

        curseDatabase["厄运"] = new CurseData
        {
            curseId = "bad_luck",
            name = "厄运",
            description = "暴击率-15%",
            icon = "♣",
            curseType = CurseType.Negative,
            effectValue = -0.15f,
            duration = -1
        };
    }

    private void InitializeBlessings()
    {
        blessingDatabase["强壮"] = new BlessingData
        {
            blessingId = "strong",
            name = "强壮",
            description = "攻击+20%",
            icon = "†",
            blessingType = BlessingType.Positive,
            effectValue = 0.2f,
            duration = -1
        };

        blessingDatabase["铁壁"] = new BlessingData
        {
            blessingId = "iron_wall",
            name = "铁壁",
            description = "防御+20%",
            icon = "◆",
            blessingType = BlessingType.Positive,
            effectValue = 0.2f,
            duration = -1
        };

        blessingDatabase["净化"] = new BlessingData
        {
            blessingId = "purify",
            name = "净化",
            description = "污染降低20%",
            icon = "✦",
            blessingType = BlessingType.Positive,
            effectValue = -20f,
            duration = 0
        };

        blessingDatabase["幸运"] = new BlessingData
        {
            blessingId = "lucky",
            name = "幸运",
            description = "暴击率+15%",
            icon = "♣",
            blessingType = BlessingType.Positive,
            effectValue = 0.15f,
            duration = -1
        };

        blessingDatabase["再生"] = new BlessingData
        {
            blessingId = "regen_blessing",
            name = "再生",
            description = "每回合恢复5%HP",
            icon = "♥",
            blessingType = BlessingType.Positive,
            effectValue = 0.05f,
            duration = -1
        };

        blessingDatabase["财富"] = new BlessingData
        {
            blessingId = "wealth",
            name = "财富",
            description = "金币获得+50%",
            icon = "$",
            blessingType = BlessingType.Positive,
            effectValue = 0.5f,
            duration = -1
        };

        blessingDatabase["吸血"] = new BlessingData
        {
            blessingId = "lifesteal_blessing",
            name = "吸血",
            description = "攻击回复10%伤害",
            icon = "♥",
            blessingType = BlessingType.Positive,
            effectValue = 0.1f,
            duration = -1
        };

        blessingDatabase["洞察"] = new BlessingData
        {
            blessingId = "insight",
            name = "洞察",
            description = "可以看到下一层地图",
            icon = "◉",
            blessingType = BlessingType.Information,
            effectValue = 0f,
            duration = -1
        };

        blessingDatabase["勇气"] = new BlessingData
        {
            blessingId = "courage",
            name = "勇气",
            description = "逃跑概率+30%",
            icon = "⚔",
            blessingType = BlessingType.Positive,
            effectValue = 0.3f,
            duration = -1
        };

        blessingDatabase["成长"] = new BlessingData
        {
            blessingId = "growth",
            name = "成长",
            description = "击杀获得额外20%进化点",
            icon = "↑",
            blessingType = BlessingType.Positive,
            effectValue = 0.2f,
            duration = -1
        };
    }

    public CurseData GetCurse(string curseName)
    {
        if (curseDatabase.TryGetValue(curseName, out CurseData curse))
        {
            return curse;
        }


        return null;
    }

    public BlessingData GetBlessing(string blessingName)
    {
        if (blessingDatabase.TryGetValue(blessingName, out BlessingData blessing))
        {
            return blessing;
        }


        return null;
    }

    public List<CurseData> GetAllCurses()
    {
        return new List<CurseData>(curseDatabase.Values);
    }

    public List<BlessingData> GetAllBlessings()
    {
        return new List<BlessingData>(blessingDatabase.Values);
    }

    public CurseData GetRandomCurse()
    {
        List<CurseData> curses = GetAllCurses();
        return curses[Random.Range(0, curses.Count)];
    }

    public BlessingData GetRandomBlessing()
    {
        List<BlessingData> blessings = GetAllBlessings();
        return blessings[Random.Range(0, blessings.Count)];
    }

    protected override void OnDestroy()
    {
        curseDatabase.Clear();
        blessingDatabase.Clear();
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}

[System.Serializable]
public class CurseData
{
    public string curseId;
    public string name;
    public string description;
    public string icon;
    public CurseType curseType;
    public float effectValue;
    public int duration;
}

[System.Serializable]
public class BlessingData
{
    public string blessingId;
    public string name;
    public string description;
    public string icon;
    public BlessingType blessingType;
    public float effectValue;
    public int duration;
}

public enum CurseType
{
    Negative,
    Information,
    Dot,
    Chance
}

public enum BlessingType
{
    Positive,
    Information,
    OneTime
}
