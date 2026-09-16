using UnityEngine;
using System.Collections.Generic;

public class BuildAxesSystem : MonoBehaviour
{
    public static BuildAxesSystem Instance { get; private set; }

    public enum BuildAxis
    {
        Offense,
        Defense,
        Speed,
        Pollution,
        Support,
        Utility
    }

    public struct AxisData
    {
        public BuildAxis axis;
        public string name;
        public string description;
        public Color color;
        public int points;
        public int maxPoints;
        public List<string> bonuses;
    }

    private Dictionary<BuildAxis, AxisData> axes = new Dictionary<BuildAxis, AxisData>();
    private int totalPoints = 0;
    private const int MAX_TOTAL_POINTS = 30;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        InitializeAxes();
    }

    void InitializeAxes()
    {
        axes[BuildAxis.Offense] = new AxisData
        {
            axis = BuildAxis.Offense,
            name = "进攻",
            description = "提升攻击力和伤害输出",
            color = new Color(1, 0.2f, 0.2f),
            points = 0,
            maxPoints = 10,
            bonuses = new List<string>
            {
                "ATK+5",
                "ATK+5",
                "暴击率+5%",
                "ATK+5",
                "暴击伤害+20%",
                "ATK+10",
                "击杀回复HP+5%",
                "ATK+10",
                "弱点攻击+30%",
                "终极技能伤害+50%"
            }
        };

        axes[BuildAxis.Defense] = new AxisData
        {
            axis = BuildAxis.Defense,
            name = "防御",
            description = "提升防御力和生存能力",
            color = new Color(0.2f, 0.6f, 1),
            points = 0,
            maxPoints = 10,
            bonuses = new List<string>
            {
                "DEF+3",
                "DEF+3",
                "MaxHP+20",
                "DEF+3",
                "伤害减免+10%",
                "DEF+5",
                "MaxHP+30",
                "DEF+5",
                "HP<30%时DEF×2",
                "受到致命伤害时有20%几率存活"
            }
        };

        axes[BuildAxis.Speed] = new AxisData
        {
            axis = BuildAxis.Speed,
            name = "速度",
            description = "提升行动速度和先手能力",
            color = new Color(0.2f, 1, 0.6f),
            points = 0,
            maxPoints = 8,
            bonuses = new List<string>
            {
                "闪避率+5%",
                "先手率+10%",
                "闪避率+5%",
                "行动速度+1",
                "闪避率+10%",
                "先手率+15%",
                "闪避率+10%",
                "攻击后有20%几率再次行动"
            }
        };

        axes[BuildAxis.Pollution] = new AxisData
        {
            axis = BuildAxis.Pollution,
            name = "污染",
            description = "驾驭污染获得强大力量",
            color = new Color(0.7f, 0.2f, 1),
            points = 0,
            maxPoints = 8,
            bonuses = new List<string>
            {
                "污染技能消耗-5%",
                "污染伤害+10%",
                "污染技能消耗-5%",
                "污染状态效果+20%",
                "污染技能消耗-10%",
                "污染抗性+10%",
                "污染技能消耗-10%",
                "污染100%时获得特殊能力"
            }
        };

        axes[BuildAxis.Support] = new AxisData
        {
            axis = BuildAxis.Support,
            name = "支援",
            description = "增强回复和增益效果",
            color = new Color(1, 0.8f, 0.2f),
            points = 0,
            maxPoints = 6,
            bonuses = new List<string>
            {
                "治疗效果+10%",
                "增益持续时间+1回合",
                "治疗效果+15%",
                "每回合回复2%HP",
                "治疗效果+20%",
                "复活时回复50%HP"
            }
        };

        axes[BuildAxis.Utility] = new AxisData
        {
            axis = BuildAxis.Utility,
            name = "实用",
            description = "增强探索和资源获取",
            color = new Color(0.8f, 0.8f, 0.8f),
            points = 0,
            maxPoints = 6,
            bonuses = new List<string>
            {
                "金币获取+10%",
                "EP获取+10%",
                "探索范围+1",
                "金币获取+15%",
                "EP获取+15%",
                "商店折扣+20%"
            }
        };
    }

    public bool AddPoint(BuildAxis axis)
    {
        if (totalPoints >= MAX_TOTAL_POINTS)
            return false;

        if (!axes.ContainsKey(axis))
            return false;

        if (axes[axis].points >= axes[axis].maxPoints)
            return false;

        AxisData current = axes[axis];
        current.points++;
        axes[axis] = current;
        totalPoints++;

        ApplyBonus(axis, axes[axis].points);
        return true;
    }

    void ApplyBonus(BuildAxis axis, int level)
    {
        var p = GameManager.Instance.Player;
        var data = axes[axis];

        if (level <= 0 || level > data.bonuses.Count)
            return;

        string bonus = data.bonuses[level - 1];
        CompleteGameSystem.Instance.AddCombatLog($"◆ {data.name}轴 Lv.{level}: {bonus}");

        switch (axis)
        {
            case BuildAxis.Offense:
                if (bonus.Contains("ATK")) p.attack += int.Parse(bonus.Split('+')[1]);
                break;
            case BuildAxis.Defense:
                if (bonus.Contains("DEF")) p.defense += int.Parse(bonus.Split('+')[1]);
                if (bonus.Contains("MaxHP"))
                {
                    int hpBonus = int.Parse(bonus.Split('+')[1]);
                    p.maxHp += hpBonus;
                    p.hp += hpBonus;
                }
                break;
            case BuildAxis.Support:
                if (level == 4) p.baseMaxHp += 10;
                break;
            case BuildAxis.Utility:
                break;
        }
    }

    public bool RemovePoint(BuildAxis axis)
    {
        if (!axes.ContainsKey(axis))
            return false;

        if (axes[axis].points <= 0)
            return false;

        AxisData current = axes[axis];
        current.points--;
        axes[axis] = current;
        totalPoints--;
        return true;
    }

    public int GetPoints(BuildAxis axis)
    {
        return axes.ContainsKey(axis) ? axes[axis].points : 0;
    }

    public int GetMaxPoints(BuildAxis axis)
    {
        return axes.ContainsKey(axis) ? axes[axis].maxPoints : 0;
    }

    public int GetTotalPoints()
    {
        return totalPoints;
    }

    public int GetMaxTotalPoints()
    {
        return MAX_TOTAL_POINTS;
    }

    public AxisData GetAxisData(BuildAxis axis)
    {
        return axes.ContainsKey(axis) ? axes[axis] : new AxisData();
    }

    public List<AxisData> GetAllAxes()
    {
        return new List<AxisData>(axes.Values);
    }

    public void ResetAll()
    {
        var axisKeys = new List<BuildAxis>(axes.Keys);
        foreach (var axis in axisKeys)
        {
            AxisData current = axes[axis];
            current.points = 0;
            axes[axis] = current;
        }
        totalPoints = 0;
    }

    public void OnFloorClear(int floor)
    {
        if (floor % 5 == 0)
        {
            CompleteGameSystem.Instance.AddCombatLog("✨ 获得构建轴点数！");
        }
    }
}
