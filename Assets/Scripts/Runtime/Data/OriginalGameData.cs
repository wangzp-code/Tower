using UnityEngine;
using System.Collections.Generic;

public static class OriginalGameData
{
    // Shop Items
    public class ShopItem
    {
        public string id;
        public string name;
        public int cost;
        public string desc;
        public string type;
        public string cat;
        public float priceScale;
        public int maxBuy;
        public int minZone;
    }

    public static readonly ShopItem[] ShopItems = new ShopItem[]
    {
        new ShopItem{id="heal_potion",name="生命药水",cost=150,desc="恢复50%生命(每次涨价，限3次)",type="heal",cat="supply",priceScale=1.8f,maxBuy=3,minZone=0},
        new ShopItem{id="atk_potion",name="力量药剂",cost=300,desc="+2攻击(每次涨价)",type="atk",cat="supply",priceScale=1.4f,maxBuy=5,minZone=0},
        new ShopItem{id="def_potion",name="护甲强化",cost=300,desc="+2防御(每次涨价)",type="def",cat="supply",priceScale=1.4f,maxBuy=5,minZone=0},
        new ShopItem{id="hp_essence",name="生命精华",cost=350,desc="+8%最大生命(每次涨价)",type="maxhp",cat="supply",priceScale=1.5f,maxBuy=5,minZone=0},
        new ShopItem{id="purify_small",name="污染压制",cost=300,desc="污染-20",type="purify_small",cat="survival",minZone=0},
        new ShopItem{id="purify_full",name="污染净化",cost=1200,desc="污染清零",type="purify_full",cat="survival",minZone=0},
        new ShopItem{id="collapse_resist",name="崩溃抵抗",cost=2000,desc="下次污染100时自动清醒",type="collapse_resist",cat="survival",minZone=0},
        new ShopItem{id="death_revive",name="死亡复活",cost=2500,desc="下次死亡保留形态不回滚",type="death_revive",cat="survival",minZone=0},
        new ShopItem{id="form_slot",name="形态记忆槽",cost=2000,desc="可携带形态+1",type="form_slot",cat="growth",minZone=0},
        new ShopItem{id="form_lock",name="形态固化",cost=800,desc="当前形态死亡后仍保留1次",type="form_lock",cat="growth",minZone=0},
        new ShopItem{id="human_enhance",name="人类强化",cost=3000,desc="基础人类ATK+5/DEF+3",type="human_enhance",cat="growth",minZone=0},
        new ShopItem{id="map_scan",name="地图扫描",cost=300,desc="显示本层出口位置",type="map_scan",cat="info",minZone=0},
        new ShopItem{id="monster_scan",name="怪物解析",cost=150,desc="查看地图上所有怪物属性",type="monster_scan",cat="info",minZone=0},
        new ShopItem{id="regen_combat",name="生物共生体",cost=600,desc="战斗中每回合恢复3%HP",type="regen_combat",cat="supply",minZone=3},
        new ShopItem{id="full_heal",name="菌膜修复液",cost=1000,desc="恢复100%HP",type="full_heal",cat="supply",minZone=4},
        new ShopItem{id="perm_regen",name="寄生再生核",cost=2500,desc="永久: 战斗中每回合回复5%HP",type="perm_regen",cat="growth",minZone=4}
    };

    // Class Base Stats
    public class ClassStats
    {
        public string id;
        public int hp;
        public int maxHp;
        public int atk;
        public int def;
        public int fogRadius;
    }

    public static readonly ClassStats[] ClassBaseStats = new ClassStats[]
    {
        new ClassStats{id="titan",hp=45,maxHp=45,atk=3,def=3,fogRadius=5},
        new ClassStats{id="ghost",hp=28,maxHp=28,atk=5,def=1,fogRadius=7},
        new ClassStats{id="swarm",hp=35,maxHp=35,atk=4,def=1,fogRadius=6},
        new ClassStats{id="blood",hp=32,maxHp=32,atk=6,def=1,fogRadius=6},
        new ClassStats{id="mech",hp=40,maxHp=40,atk=4,def=2,fogRadius=4}
    };

    // Achievements
    public class Achievement
    {
        public string id;
        public string name;
        public string desc;
        public string icon;
    }

    public static readonly Achievement[] AchievementDefs = new Achievement[]
    {
        new Achievement{id="first_kill",name="初猎",desc="击杀第一只怪物",icon="⚔"},
        new Achievement{id="first_possess",name="寄生觉醒",desc="首次成功附身",icon="★"},
        new Achievement{id="floor10",name="深入",desc="到达第10层",icon="◄"},
        new Achievement{id="floor25",name="中途觉醒",desc="到达第25层",icon="⚡"},
        new Achievement{id="floor50",name="登顶",desc="到达第50层",icon="♛"},
        new Achievement{id="possess5",name="收集者",desc="附身5种不同生物",icon="♦"},
        new Achievement{id="possess10",name="百变怪",desc="附身10种不同生物",icon="↔"},
        new Achievement{id="no_death",name="不死传说",desc="不死亡通关25层",icon="☠"},
        new Achievement{id="titan_end",name="不可移动的永恒",desc="达成泰坦结局",icon="■"},
        new Achievement{id="ghost_end",name="不存在的自由",desc="达成幽灵结局",icon="◎"},
        new Achievement{id="swarm_end",name="增殖的混沌",desc="达成虫群结局",icon="†"},
        new Achievement{id="blood_end",name="永恒的饥渴",desc="达成血族结局",icon="♥"},
        new Achievement{id="mech_end",name="超越肉体",desc="达成机甲结局",icon="⚙"},
        new Achievement{id="hidden_end",name="递归的观察者",desc="达成隐藏结局",icon="◈"},
        new Achievement{id="defend10",name="铁壁",desc="单场战斗防御10次",icon="◆"},
        new Achievement{id="switch3",name="形态大师",desc="单场战斗切换形态3次",icon="↩"},
        new Achievement{id="pollution0",name="纯净",desc="通关25层时污染为0",icon="✨"}
    };

    // Build Axes
    public class BuildAxis
    {
        public string id;
        public string name;
        public string icon;
        public string color;
        public string desc;
        public Dictionary<string, float> bonus;
    }

    public static readonly BuildAxis[] BuildAxes = new BuildAxis[]
    {
        new BuildAxis{id="tank",name="坦克",icon="◆",color="#4488cc",desc="高生命/高防御",bonus=new Dictionary<string, float>{{"def",2f},{"maxHp",15f}}},
        new BuildAxis{id="hunter",name="猎手",icon="⚔",color="#ff4444",desc="高攻击/暴击",bonus=new Dictionary<string, float>{{"atk",3f}}},
        new BuildAxis{id="parasite",name="寄生",icon="★",color="#b455ff",desc="附身/吸血/寄生",bonus=new Dictionary<string, float>{{"possessBonus",0.05f}}},
        new BuildAxis{id="toxic",name="腐蚀",icon="☠",color="#88cc00",desc="毒素/污染/腐蚀",bonus=new Dictionary<string, float>{{"extraDmg",0.06f}}},
        new BuildAxis{id="swift",name="迅影",icon="⚡",color="#ffaa00",desc="速度/闪避/相位",bonus=new Dictionary<string, float>{{"dmgReduce",0.04f}}},
        new BuildAxis{id="sentinel",name="守卫",icon="◆",color="#888888",desc="护甲/反击/不死",bonus=new Dictionary<string, float>{{"def",1f},{"atk",1f}}}
    };

    // Monster Silhouettes Data
    public class MonsterSilhouette
    {
        public string id;
        public string bodyPath;
        public string[][] extras;
        public float[][] eyes;
        public string glowColor;
    }

    // Endings
    public class Ending
    {
        public string id;
        public string classId;
        public string title;
        public string subtitle;
        public string achievementId;
        public string text;
        public string quote;
    }

    public static readonly Ending[] Endings = new Ending[]
    {
        new Ending
        {
            id="titan_end",classId="titan",title="成为永恒",subtitle="泰坦结局",achievementId="titan_end",
            text="你不再试图逃离。\n你成为了塔本身。\n\n每一面墙壁都是你的骨骼，\n每一层楼都是你的记忆，\n每一个进入的灵魂\n都将成为你永恒的一部分。\n\n你终于理解了——\n保护，就是囚禁。\n囚禁，就是保护。\n而你，选择了两者。",
            quote="\"有些存在太过沉重，\n连崩塌都是奢侈。\""
        },
        new Ending
        {
            id="ghost_end",classId="ghost",title="穿透虚实",subtitle="幽灵结局",achievementId="ghost_end",
            text="你找到了真正的出口。\n不是向上，而是\"之间\"。\n\n你消散在虚实的边界——\n不再是塔，不再是囚徒，\n不再是任何可以被定义的东西。\n\n你成为了永恒的观察者，\n看着一个又一个\"你\"\n在同一座塔里寻找出口。",
            quote="\"他们仍在轮回，\n而我终于自由——\n以不存在的形式。\""
        },
        new Ending
        {
            id="swarm_end",classId="swarm",title="拥抱混沌",subtitle="虫群结局",achievementId="swarm_end",
            text="你不再是一个意识。\n你是无数碎片，遍布每一层。\n\n每一次死亡都是繁殖，\n每一次附身都是扩张。\n塔不再困住你——\n因为你已经是塔的每一个角落。\n\n不是逃出牢笼，\n而是成为牢笼本身的\n每一根栏杆。",
            quote="\"我即是我们，\n我们即是塔，\n每一次死亡都是繁殖。\""
        },
        new Ending
        {
            id="blood_end",classId="blood",title="血之永恒",subtitle="血族结局",achievementId="blood_end",
            text="你已经不需要宿主了。\n你的血管延伸至整座塔的每一层。\n\n每一个生物的心跳\n都是你的养分来源。\n你已分不清自己是寄生体\n还是这座塔的血液循环本身。\n\n饥渴从未消退，\n但你不再需要进食——\n因为一切活物都是你的一部分。",
            quote="\"每一滴血都是永恒的承诺，\n每一次心跳都是我的脉搏。\""
        },
        new Ending
        {
            id="mech_end",classId="mech",title="钢铁意志",subtitle="机甲结局",achievementId="mech_end",
            text="有机组织已被完全替换。\n你的躯壳是钢铁与菌丝的融合体。\n\n污染不再是威胁——\n它是你的燃料。\n每一次过载都让你\n距离「纯粹」更近一步。\n\n当最后一丝血肉消散时，\n你听到了齿轮永恒转动的声音。\n那是自由的声音。",
            quote="\"当肉体不再是限制，\n你便不再是囚徒。\""
        }
    };

    // Hidden Ending
    public static readonly Ending HiddenEnding = new Ending
    {
        id="hidden_end",classId="",title="开发者模式",subtitle="隐藏结局",achievementId="hidden_end",
        text="你发现了一个未完成的房间。\n墙上有潦草的笔记：\n\n\"如果你看到这个，\n说明有人在测试。\n这个「游戏」本身也是\n某个更大系统的一个实验...\n\n也许我们都是被观察的。\"\n\n屏幕闪烁了一下。\n你看到了代码。\n你看到了数字。\n你看到了...自己在玩游戏。",
        quote="\"感谢游玩。\n你的选择数据将用于改进下一个迭代。\""
    };
}
