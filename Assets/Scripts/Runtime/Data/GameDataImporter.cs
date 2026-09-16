using UnityEngine;
using System.Linq;

public static class GameDataImporter
{
    private static (string id, string name, int hp, int atk, int def, int zone, bool boss, Color color, string[] traits, string[] axes)[] _monsterDefs;
    private static bool _loaded;

    public static void InitializeAllData()
    {
        _loaded = false;
        _monsterDefs = null;
    }

    public static (string id, string name, int hp, int atk, int def, int zone, bool boss, Color color, string[] traits, string[] axes)[] MonsterDefinitions
    {
        get
        {
            if (!_loaded) LoadFromConfig();
            return _monsterDefs;
        }
    }

    static void LoadFromConfig()
    {
        _loaded = true;
        var configs = DataConfigManager.Instance?.GetMonsterConfigs();
        if (configs != null && configs.Length > 0)
        {
            _monsterDefs = configs.Select(c => (
                c.id,
                c.name,
                c.hp,
                c.atk,
                c.def,
                c.zone,
                c.boss,
                new Color(c.colorR, c.colorG, c.colorB),
                c.traits ?? new string[0],
                c.axes ?? new string[0]
            )).ToArray();

        }
        else
        {
            _monsterDefs = FallbackDefinitions;

        }
    }

    public static readonly (string id, string name, Color color1, Color color2, Color color3, Color color4)[] ClassColorDefinitions = new[]
    {
        ("titan", "泰坦", new Color(0.27f,0.53f,0.8f), new Color(0.53f,0.8f,1f), new Color(0f,0.78f,1f), new Color(0.04f,0.1f,0.18f)),
        ("ghost", "幽灵", new Color(0.8f,0.27f,0.8f), new Color(1f,0.53f,1f), new Color(1f,0f,0.4f), new Color(0.1f,0.04f,0.1f)),
        ("swarm", "虫群", new Color(0.27f,0.8f,0.53f), new Color(0.53f,1f,0.8f), new Color(0f,1f,0.82f), new Color(0.04f,0.1f,0.08f)),
        ("blood", "血族", new Color(0.8f,0f,0.13f), new Color(1f,0.27f,0.4f), new Color(1f,0f,0.2f), new Color(0.1f,0.04f,0.04f)),
        ("mech", "机甲", new Color(1f,0.53f,0f), new Color(1f,0.78f,0.27f), new Color(1f,0.67f,0f), new Color(0.1f,0.08f,0.04f))
    };

    static readonly (string id, string name, int hp, int atk, int def, int zone, bool boss, Color color, string[] traits, string[] axes)[] FallbackDefinitions = new[]
    {
        ("human", "寄生体", 120, 8, 6, 0, false, new Color(0f, 1f, 0.82f), new string[]{}, new[]{"parasite"}),
        ("rat", "实验鼠", 45, 8, 1, 1, false, new Color(0.52f, 0.27f, 0.07f), new[]{"迅捷"}, new[]{"swift"}),
        ("roach", "辐射蟑螂", 65, 7, 3, 1, false, new Color(0.4f, 0.26f, 0.13f), new[]{"厚皮"}, new[]{"tank"}),
        ("slime", "失败体-α", 85, 11, 2, 1, false, new Color(0.2f, 0.8f, 0.2f), new[]{"再生"}, new[]{"tank","parasite"}),
        ("dog", "看门犬", 65, 10, 2, 1, false, new Color(0.4f, 0.4f, 0.4f), new[]{"忠诚"}, new[]{"sentinel"}),
        ("gecko", "壁虎变体", 55, 14, 1, 1, false, new Color(0.49f, 0.8f, 0.49f), new[]{"弹性"}, new[]{"swift"}),
        ("drone", "故障无人机", 50, 11, 4, 1, false, new Color(0.27f, 0.53f, 0.7f), new[]{"电击"}, new[]{"sentinel","swift"}),
        ("boss1", "实验主管", 250, 24, 7, 1, true, new Color(0.67f, 0f, 0f), new[]{"领袖","再生"}, new[]{"sentinel","tank"}),
        ("wolf", "培育狼", 140, 22, 7, 2, false, new Color(0.53f, 0f, 0f), new[]{"狂暴"}, new[]{"hunter"}),
        ("spider", "酸液蜘蛛", 120, 20, 9, 2, false, new Color(0.29f, 0f, 0f), new[]{"蛛网"}, new[]{"toxic","sentinel"}),
        ("bat", "铁爪猴", 100, 26, 4, 2, false, new Color(0.16f, 0.04f, 0.04f), new[]{"吸血"}, new[]{"hunter","parasite"}),
        ("wasp", "变异黄蜂", 125, 24, 5, 2, false, new Color(1f, 0.65f, 0f), new[]{"毒素"}, new[]{"toxic","swift"}),
        ("guard", "失控警卫", 180, 22, 11, 2, false, new Color(0.33f, 0.33f, 0.33f), new[]{"护甲"}, new[]{"sentinel","tank"}),
        ("vine", "绞缠藤蔓", 160, 20, 12, 2, false, new Color(0.18f, 0.55f, 0.34f), new[]{"再生","蛛网"}, new[]{"tank","toxic"}),
        ("boss2", "培育主管", 380, 34, 14, 2, true, new Color(0.72f, 0.53f, 0.04f), new[]{"反击","掠夺"}, new[]{"hunter","sentinel"}),
        ("larva", "实验体-Ω", 360, 45, 18, 3, false, new Color(0.6f, 0.86f, 0.2f), new[]{"再生"}, new[]{"tank","parasite"}),
        ("mantis", "镰刀螳螂", 290, 52, 14, 3, false, new Color(0.13f, 0.55f, 0.13f), new[]{"暴击"}, new[]{"hunter","swift"}),
        ("beetle", "装甲甲虫", 400, 44, 25, 3, false, new Color(0.18f, 0.31f, 0.31f), new[]{"厚皮","护甲"}, new[]{"tank","sentinel"}),
        ("worm", "寄生蠕虫", 330, 48, 16, 3, false, new Color(0.8f, 0.53f, 0.25f), new[]{"寄生强化"}, new[]{"parasite","toxic"}),
        ("moth", "毒粉飞蛾", 250, 46, 12, 3, false, new Color(0.87f, 0.63f, 0.87f), new[]{"毒素"}, new[]{"toxic","swift"}),
        ("scorpion", "守卫者", 310, 56, 22, 3, false, new Color(0.53f, 0.27f, 0.07f), new[]{"毒素","护甲"}, new[]{"toxic","sentinel"}),
        ("hydra", "多头蛇怪", 380, 50, 18, 3, false, new Color(0.33f, 0.42f, 0.18f), new[]{"再生+","撕裂"}, new[]{"tank","hunter"}),
        ("boss3", "污染核心", 650, 70, 26, 3, true, new Color(0.53f, 0f, 0.53f), new[]{"再生","污染光环","多重攻击"}, new[]{"toxic","parasite"}),
        ("shade", "暗影", 630, 78, 20, 4, false, new Color(0.1f, 0.1f, 0.47f), new[]{"相位"}, new[]{"swift","parasite"}),
        ("lurker", "虚空潜伏者", 690, 86, 22, 4, false, new Color(0f, 0f, 0.5f), new[]{"相位","伏击"}, new[]{"swift","hunter"}),
        ("wraith", "怨灵", 580, 82, 16, 4, false, new Color(0.28f, 0.24f, 0.55f), new[]{"不死","吸取"}, new[]{"parasite","sentinel"}),
        ("voidbeast", "虚空兽", 780, 85, 26, 4, false, new Color(0.12f, 0.12f, 0.25f), new[]{"相位","狂暴"}, new[]{"hunter","swift"}),
        ("nightmare", "梦魇", 710, 92, 20, 4, false, new Color(0.18f, 0.18f, 0.31f), new[]{"恐惧","吸取"}, new[]{"toxic","parasite"}),
        ("watcher", "深渊守望者", 830, 95, 30, 4, false, new Color(0.06f, 0.06f, 0.18f), new[]{"护甲","反击"}, new[]{"sentinel","tank"}),
        ("voiddragon", "虚空幼龙", 900, 100, 24, 4, false, new Color(0.1f, 0.1f, 0.23f), new[]{"相位","多重攻击"}, new[]{"hunter","swift"}),
        ("boss4", "深渊领主", 1500, 112, 36, 4, true, new Color(0f, 0f, 0f), new[]{"相位","不死","多重攻击","恐惧"}, new[]{"swift","sentinel"}),
        ("titan", "腐化泰坦", 1000, 112, 40, 5, false, new Color(0.18f, 0.31f, 0.18f), new[]{"护甲","再生"}, new[]{"tank","sentinel"}),
        ("chaos", "混沌之子", 900, 120, 30, 5, false, new Color(0.53f, 0f, 0f), new[]{"多重攻击","狂暴"}, new[]{"hunter","toxic"}),
        ("deathknight", "死亡骑士", 950, 128, 35, 5, false, new Color(0.11f, 0.11f, 0.11f), new[]{"不死","护甲","吸血"}, new[]{"sentinel","parasite"}),
        ("horror", "远古恐惧", 1100, 124, 32, 5, false, new Color(0.16f, 0.04f, 0.04f), new[]{"恐惧","污染光环","吸取"}, new[]{"toxic","parasite"}),
        ("colossus", "虚空巨像", 1300, 118, 45, 5, false, new Color(0.04f, 0.04f, 0.1f), new[]{"护甲","相位","反击"}, new[]{"tank","sentinel"}),
        ("plague", "瘟疫使者", 1000, 130, 30, 5, false, new Color(0.33f, 0.42f, 0.18f), new[]{"毒素","污染光环","爆炸"}, new[]{"toxic","hunter"}),
        ("origin", "原初寄生体", 1800, 135, 38, 5, false, new Color(0.29f, 0.04f, 0.29f), new[]{"再生","多重攻击","召唤","污染光环"}, new[]{"parasite","toxic"}),
        ("boss5", "真实形态", 2500, 140, 45, 5, true, new Color(0.04f, 0.04f, 0.04f), new[]{"不死","相位","多重攻击","再生","狂暴"}, new[]{"hunter","parasite"})
    };
}
