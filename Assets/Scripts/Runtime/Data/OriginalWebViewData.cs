using UnityEngine;
using System;
using System.Collections.Generic;

public static class OriginalShopData
{
    [Serializable]
    public class ShopItem
    {
        public string id, name, desc, type, cat;
        public int cost, maxBuy, minZone;
        public float priceScale;
    }

    public static readonly ShopItem[] Items = new ShopItem[]
    {
        new ShopItem{id="heal_potion",name="生命药水",cost=150,desc="恢复50%生命(每次涨价，限3次)",type="heal",cat="supply",priceScale=1.8f,maxBuy=3,minZone=0},
        new ShopItem{id="atk_potion",name="力量药剂",cost=300,desc="+2攻击(每次涨价)",type="atk",cat="supply",priceScale=1.4f,maxBuy=5,minZone=0},
        new ShopItem{id="def_potion",name="护甲强化",cost=300,desc="+2防御(每次涨价)",type="def",cat="supply",priceScale=1.4f,maxBuy=5,minZone=0},
        new ShopItem{id="hp_essence",name="生命精华",cost=350,desc="+8%最大生命(每次涨价)",type="maxhp",cat="supply",priceScale=1.5f,maxBuy=5,minZone=0},
        new ShopItem{id="purify_small",name="污染压制",cost=300,desc="污染-20",type="purify_small",cat="survival",priceScale=1f,maxBuy=99,minZone=0},
        new ShopItem{id="purify_full",name="污染净化",cost=1200,desc="污染清零",type="purify_full",cat="survival",priceScale=1f,maxBuy=99,minZone=0},
        new ShopItem{id="collapse_resist",name="崩溃抵抗",cost=2000,desc="下次污染100时自动清醒",type="collapse_resist",cat="survival",priceScale=1f,maxBuy=3,minZone=0},
        new ShopItem{id="death_revive",name="死亡复活",cost=2500,desc="下次死亡保留形态不回滚",type="death_revive",cat="survival",priceScale=1f,maxBuy=2,minZone=0},
        new ShopItem{id="form_slot",name="形态记忆槽",cost=2000,desc="可携带形态+1",type="form_slot",cat="growth",priceScale=1f,maxBuy=2,minZone=0},
        new ShopItem{id="form_lock",name="形态固化",cost=800,desc="当前形态死亡后仍保留1次",type="form_lock",cat="growth",priceScale=1f,maxBuy=3,minZone=0},
        new ShopItem{id="human_enhance",name="人类强化",cost=3000,desc="基础人类ATK+5/DEF+3",type="human_enhance",cat="growth",priceScale=1f,maxBuy=1,minZone=0},
        new ShopItem{id="map_scan",name="地图扫描",cost=300,desc="显示本层出口位置",type="map_scan",cat="info",priceScale=1f,maxBuy=99,minZone=0},
        new ShopItem{id="monster_scan",name="怪物解析",cost=150,desc="查看地图上所有怪物属性",type="monster_scan",cat="info",priceScale=1f,maxBuy=99,minZone=0},
        new ShopItem{id="regen_combat",name="生物共生体",cost=600,desc="战斗中每回合恢复3%HP",type="regen_combat",cat="supply",priceScale=1f,maxBuy=1,minZone=3},
        new ShopItem{id="full_heal",name="菌膜修复液",cost=1000,desc="恢复100%HP",type="full_heal",cat="supply",priceScale=1f,maxBuy=99,minZone=4},
        new ShopItem{id="perm_regen",name="寄生再生核",cost=2500,desc="永久:战斗中每回合回复5%HP",type="perm_regen",cat="growth",priceScale=1f,maxBuy=1,minZone=4}
    };

    public static void ApplyItem(string type, GameManager.PlayerData p)
    {
        switch (type)
        {
            case "heal": p.hp = Mathf.Min(p.maxHp, p.hp + Mathf.RoundToInt(p.maxHp * 0.5f)); break;
            case "full_heal": p.hp = p.maxHp; break;
            case "atk": p.attack += 2; p.baseAttack += 2; break;
            case "def": p.defense += 2; p.baseDefense += 2; break;
            case "maxhp": int add = Mathf.RoundToInt(p.maxHp * 0.08f); p.maxHp += add; p.baseMaxHp += add; p.hp += add; break;
            case "purify_small": p.pollution = Mathf.Max(0, p.pollution - 20); break;
            case "purify_full": p.pollution = 0; break;
            case "collapse_resist": p.collapseResistCharges++; break;
            case "death_revive": p.deathReviveCharges++; break;
            case "form_slot": p.formSlots++; break;
            case "human_enhance": p.baseAttack += 5; p.attack += 5; p.baseDefense += 3; p.defense += 3; break;
            case "map_scan": break;
            case "monster_scan": break;
        }
    }
}

public static class BossPhaseData
{
    [Serializable]
    public class Phase { public float hpThreshold; public float atkMult; public string[] addTraits; public string msg; }

    public static readonly Dictionary<string, Phase[]> Phases = new Dictionary<string, Phase[]>
    {
        ["boss1"] = new Phase[] {
            new Phase{hpThreshold=0.66f, atkMult=1.2f, addTraits=new[]{"狂暴"}, msg="实验主管的眼睛变红了!"},
            new Phase{hpThreshold=0.33f, atkMult=1.5f, addTraits=new[]{"吸血","再生"}, msg="实验主管开始疯狂自我修复!"}
        },
        ["boss2"] = new Phase[] {
            new Phase{hpThreshold=0.66f, atkMult=1.0f, addTraits=new[]{"护甲","再生"}, msg="培育主管激活了防御协议!"},
            new Phase{hpThreshold=0.33f, atkMult=1.8f, addTraits=new[]{"狂暴","反击"}, msg="培育主管暴怒了!"}
        },
        ["boss3"] = new Phase[] {
            new Phase{hpThreshold=0.66f, atkMult=1.3f, addTraits=new[]{"毒素","召唤"}, msg="污染核心开始扩散毒素!"},
            new Phase{hpThreshold=0.33f, atkMult=1.6f, addTraits=new[]{"爆炸","不死"}, msg="污染核心达到临界状态!"}
        },
        ["boss4"] = new Phase[] {
            new Phase{hpThreshold=0.66f, atkMult=1.3f, addTraits=new[]{"吸血","召唤"}, msg="深渊领主召唤暗影仆从!"},
            new Phase{hpThreshold=0.33f, atkMult=2.0f, addTraits=new[]{"狂暴","再生+"}, msg="深渊领主释放终极形态!"}
        },
        ["boss5"] = new Phase[] {
            new Phase{hpThreshold=0.75f, atkMult=1.2f, addTraits=new[]{"吸血"}, msg="真实形态开始吸取你的生命力!"},
            new Phase{hpThreshold=0.50f, atkMult=1.5f, addTraits=new[]{"召唤","反击"}, msg="真实形态与塔产生共鸣!"},
            new Phase{hpThreshold=0.25f, atkMult=2.0f, addTraits=new[]{"爆炸"}, msg="真实形态进入毁灭模式!"}
        }
    };
}

public static class PollutionTierData
{
    [Serializable]
    public class Tier { public int min, max; public float atkMult, defMult; public string label, icon; }

    public static readonly Tier[] Tiers = new Tier[]
    {
        new Tier{min=0,  max=29, atkMult=1.0f, defMult=1.10f, label="净化", icon="◇"},
        new Tier{min=30, max=59, atkMult=1.10f,defMult=1.0f,  label="觉醒", icon="◈"},
        new Tier{min=60, max=84, atkMult=1.20f,defMult=0.90f, label="侵蚀", icon="◆"},
        new Tier{min=85, max=99, atkMult=1.30f,defMult=0.80f, label="临界", icon="✦"},
        new Tier{min=100,max=100,atkMult=1.50f,defMult=0.70f, label="爆发", icon="☢"}
    };

    public static Tier GetTier(float pollution)
    {
        int p = Mathf.RoundToInt(pollution);
        for (int i = Tiers.Length - 1; i >= 0; i--)
            if (p >= Tiers[i].min) return Tiers[i];
        return Tiers[0];
    }

    [Serializable]
    public class PollutionSkill { public float threshold; public string name, desc; }
    public static readonly PollutionSkill[] Skills = new PollutionSkill[]
    {
        new PollutionSkill{threshold=30, name="污染爆发", desc="ATK×1.5伤害，污染+8"},
        new PollutionSkill{threshold=50, name="血祭", desc="HP-20%，技能次数恢复"},
        new PollutionSkill{threshold=70, name="异化吞噬", desc="击杀HP<30%目标，回复其HP，污染+15"}
    };

    [Serializable]
    public class PollutionPassive { public float threshold; public string name, desc; }
    public static readonly PollutionPassive[] Passives = new PollutionPassive[]
    {
        new PollutionPassive{threshold=50, name="污染共鸣", desc="15%概率攻击+污染值伤害"},
        new PollutionPassive{threshold=70, name="腐蚀之体", desc="10%概率免伤"},
        new PollutionPassive{threshold=90, name="死亡脉冲", desc="击杀回复10%HP"}
    };
}

public static class FloorSignatureData
{
    [Serializable]
    public class Signature
    {
        public string id, name, icon, desc;
        public float possessBonus, epMult, hpMult, atkMult, defMult;
        public bool randomDmg, monsterDoubleHit, peacefulMonsters;
        public float regenPerStep, healOnKill;
        public int fogRadius, minFloor;
        public float dmgMult;
        public float polPerStep;
        public float critBonus;
        public bool isDuel;
        public bool monsterHpHalf, monsterAtkReduce;
        public float monsterAtkMult;
    }

    public static readonly Signature[] Signatures = new Signature[]
    {
        // --- 正面/中性签名 ---
        new Signature{id="regenLand",name="再生之地",icon="♣",desc="每步回复2%HP",regenPerStep=0.02f},
        new Signature{id="goldenRain",name="黄金雨",icon="$",desc="EP奖励×3",epMult=3f},
        new Signature{id="peaceZone",name="和平区",icon="◇",desc="怪物不拦路 附身率+30%",peacefulMonsters=true,possessBonus=0.3f},
        new Signature{id="xray",name="透视",icon="◉",desc="全图可见",fogRadius=99},
        new Signature{id="parasiteParadise",name="寄生乐园",icon="★",desc="附身率+20% 但+20污染",possessBonus=0.2f},
        new Signature{id="evoAccel",name="进化加速",icon="★",desc="EP奖励×2",epMult=2f},
        new Signature{id="bountyHunt",name="猎杀令",icon="◎",desc="击杀悬赏目标+500EP",epMult=1f},
        new Signature{id="healSpring",name="治愈温泉",icon="♨",desc="击杀怪物回复15%HP",healOnKill=0.15f},
        new Signature{id="critZone",name="暴击场",icon="※",desc="所有攻击暴击率+30%",critBonus=0.3f},
        // --- 风险/回报签名 ---
        new Signature{id="hungerSwamp",name="饥饿沼泽",icon="◇",desc="停止不动流失HP 但移动回复1%HP",regenPerStep=0.01f},
        new Signature{id="toxicFog",name="毒雾弥漫",icon="☁",desc="每步+1污染 但怪物ATK-40%",polPerStep=1f,monsterAtkMult=0.6f},
        new Signature{id="fragileBarrier",name="脆弱结界",icon="♥",desc="所有伤害×2（双方均受影响）",dmgMult=2f},
        new Signature{id="gamblerHeaven",name="赌徒天堂",icon="◎",desc="伤害0.5x-2x随机",randomDmg=true},
        new Signature{id="giantify",name="巨人化",icon="◎",desc="怪物HP×2 ATK×1.5 EP×2",hpMult=2f,atkMult=1.5f,epMult=2f},
        new Signature{id="duel",name="单挑",icon="⚔",desc="仅1只精英 属性×3 奖励×5",epMult=5f,isDuel=true},
        new Signature{id="pollutionStorm",name="污染风暴",icon="☢",desc="每10步+5污染 怪物ATK-30% EP×1.5",epMult=1.5f,monsterAtkMult=0.7f},
        new Signature{id="judgment",name="审判",icon="⚖",desc="从弱到强击杀 每正确击杀+50EP"},
        // --- 挑战签名 ---
        new Signature{id="darkness",name="黑暗降临",icon="●",desc="视野缩小到3格 附身率+20%",fogRadius=3,possessBonus=0.2f,minFloor=8},
        new Signature{id="timeAccel",name="时间加速",icon="⏩",desc="怪物攻击2次 EP×2",monsterDoubleHit=true,epMult=2f,minFloor=15},
        new Signature{id="lilliput",name="小人国",icon="◎",desc="怪物HP÷2 数量翻倍",monsterHpHalf=true}
    };
}

public static class StoryData
{
    [Serializable]
    public class StoryEvent { public int floor; public string icon, title, text; public int epReward; }

    public static readonly StoryEvent[] Events = new StoryEvent[]
    {
        new StoryEvent{floor=5, icon="◆",title="墙上的刻字",text="你在墙上发现了奇怪的刻字...\n'第七次迭代...意识递归...不要信任镜子...'",epReward=20},
        new StoryEvent{floor=10,icon="◆",title="实验员笔记",text="一本破旧的笔记本:\n'实验体展现出意识迁移能力，我们称之为「寄生」。\n但谁才是真正的宿主？'",epReward=30},
        new StoryEvent{floor=15,icon="◆",title="血书",text="墙壁上用血写的字:\n'如果你能读到这些，说明你已经不是第一次了。\n每一次都是50层。每一次你都会「发现真相」。'",epReward=40},
        new StoryEvent{floor=20,icon="◆",title="研究日志",text="一份完整的研究日志:\n'项目名称：意识递归\n目的：创建自我维持的意识生态系统\n结果：失控。实验体获得了自我意识。'",epReward=50},
        new StoryEvent{floor=25,icon="⚠️",title="系统异常",text="你发现了一个没有出口的房间。\n墙壁上的字开始移动...\n'你不是第一个到达这里的。\n你也不会是最后一个。'",epReward=100},
        new StoryEvent{floor=30,icon="◆",title="被划掉的日记",text="大部分内容被划掉了，只留下:\n'...第三次迭代的受试者展现了前所未有的适应能力...\n...但它不知道自己是实验的一部分...'",epReward=100},
        new StoryEvent{floor=35,icon="◆",title="最后的警告",text="一个全息投影:\n'警告：意识递归深度超过安全阈值。\n建议终止实验。\n备注：建议被否决。实验继续。'",epReward=80},
        new StoryEvent{floor=40,icon="◎",title="记忆碎片恢复",text="突然，大量记忆涌入你的意识...\n你看到了自己——不，是另一个「你」——\n在同样的塔里，做着同样的选择。\n你是实验的宿主本人。你陷入了意识递归循环。",epReward=100},
        new StoryEvent{floor=45,icon="◎",title="第七次迭代记录",text="你发现了一份编号为7的迭代记录:\n'每次都是50层。每次受试者都会在第40层「恢复记忆」。\n每次他们都认为这是第一次发现真相。\n这是设计好的。这是实验的一部分。'",epReward=100},
        new StoryEvent{floor=50,icon="※",title="起源之地",text="你站在镜子前。\n镜中的你微笑着。\n'每一层都是你吞噬的记忆。\n每一个怪物都是曾经的「你」。\n污染不是腐蚀，是你在苏醒。\n附身不是夺取，是你在回收。'",epReward=200}
    };
}

public static class LocalizationData
{
    static readonly Dictionary<string, string> ZhToEn = new Dictionary<string, string>
    {
        // 通用
        ["继续游戏"]="Continue",["新游戏"]="New Game",["跳过"]="Skip",["进入高塔"]="Enter the Tower",
        ["开始游戏"]="Start Game",["随机"]="Random",["关闭"]="Close",["返回"]="Back",
        ["确定"]="OK",["取消"]="Cancel",["确认"]="Confirm",
        ["加载"]="Load",["删除"]="Delete",["退出游戏"]="Quit Game",
        ["保存游戏"]="Save Game",["游戏已保存"]="Game Saved",

        // 主页
        ["你 也 是 我"]="Y O U   A R E   M E",
        ["你每夺走一个身体，就离自己更远一步。"]="Every body you take brings you further from yourself.",
        ["继 续 寄 生"]="C O N T I N U E",
        ["新 一 轮 踏 塔"]="N E W   A S C E N T",
        ["终端功能"]="TERMINAL",
        ["成就回响"]="Achievements",["异种图鉴"]="Bestiary",["记忆档案"]="Archives",
        ["暗塔排行"]="Leaderboard",["终端设置"]="Settings",["回响商店"]="Shop",
        ["残响圣坛"]="Sanctuary",["每日登录"]="Daily Login",["记录管理"]="Records",

        // 战斗
        ["攻击"]="Attack",["防御"]="Defend",["附身"]="Possess",["逃跑"]="Flee",
        ["潜行"]="Stealth",["冲刺"]="Sprint",["终极"]="Ultimate",
        ["附身成功率"]="Possess Rate",["预计受伤"]="Est. Damage",
        ["战斗开始"]="Battle Start",["寄生体"]="Parasite",["目标"]="Target",

        // 游戏内菜单
        ["菜单"]="Menu",["进化"]="Evolution",["商店"]="Shop",["形态羁绊"]="Affinity",
        ["污染技能"]="P-Skills",["成就"]="Achievements",
        ["设置"]="Settings",["退出重开"]="Restart",
        ["锚点管理"]="Anchors",["固化记忆"]="Solidify",

        // 商店
        ["基础补给"]="Supplies",["净化保命"]="Purify & Survive",
        ["形态成长"]="Form Growth",["信息优势"]="Intel Advantage",
        ["已满"]="Sold Out",["已激活"]="Active",["EP不足"]="Low EP",

        // 职业
        ["泰坦"]="Titan",["幽灵"]="Ghost",["虫群"]="Swarm",["血族"]="Blood",["机甲"]="Mech",

        // 排行 / 统计
        ["排行榜"]="Leaderboard",["附身次数"]="Possessions",["击杀数"]="Kills",
        ["抵达层数"]="Floors",["存活时长"]="Survival",["最久宿主"]="Best Host",
        ["最高污染"]="Max Pollution",["最终形态"]="Final Form",
        ["本地游戏记录"]="Local Records",["当前游戏"]="Current Game",
        ["暂无游戏记录"]="No Records",

        // 结局
        ["宿主崩坏"]="Host Collapse",["时间耗尽"]="Time Expired",

        // 事件
        ["威压"]="Intimidate",["交易"]="Trade",["共鸣"]="Resonate",["加入"]="Join",
        ["搜索"]="Search",["休息"]="Rest",["探索"]="Explore",

        // 设置面板
        ["主音量"]="Master Vol",["背景音乐"]="BGM",["音效"]="SFX",
        ["语言"]="Language",["版本"]="Version",
        ["总游戏次数"]="Total Games",["完成度"]="Completion",

        // 记录管理
        ["存档管理"]="Save Management",["数据管理"]="Data Management",
        ["重置进度"]="Reset Progress",["空"]="Empty",

        // 图鉴
        ["已收集"]="Collected",["未发现"]="Unknown",

        // 记忆档案
        ["闯塔记录"]="Run History",["已达成结局"]="Endings Achieved",
        ["已解锁特性"]="Traits Unlocked",["职业等级"]="Class Levels",
        ["尚未开始寄生之旅"]="No parasitic journey yet",
        ["游戏中的记忆将存档于此"]="Memories from your runs will be archived here",

        // 污染技能
        ["主动技能"]="Active Skills",["被动效果"]="Passive Effects",

        // 统计
        ["总闯塔次数"]="Total Runs",["最深"]="Deepest",
        ["总击杀"]="Total Kills",["附身"]="Possessions",
        ["结局收集"]="Endings",["完美通关"]="Perfect Runs",
        ["闯塔次数"]="Runs",["击杀"]="Kills",

        // 进入/商店空
        ["进入游戏后可使用商店"]="Shop available during gameplay",
        ["使用进化点数(EP)购买增益"]="Use Evolution Points (EP) to buy upgrades",

        // 模式选择
        ["暗塔契约"]="Dark Tower Protocol",
        ["暗影"]="Shadow",["远征"]="Expedition",["宿命"]="Fate",
        ["12层速巡 · 约15分钟"]="12 floors sprint · ~15 min",
        ["10关 × 20层 · 约5小时"]="10 stages × 20 floors · ~5 hours",
        ["50层全程 · 约60分钟"]="50 floors full run · ~60 min",
        ["暗影穿行，宿主更迭。"]="Shadow crawl, host exchange.",
        ["穿越十重裂隙，征服暗塔。"]="Cross the rift, conquer the dark tower.",
        ["完整体验寄生者的宿命轮回。"]="Experience the parasite's fate cycle.",
        ["想快速开一局，测试构筑与附身路线的闯塔者。"]="Quick runs for tower climbers to test builds and possession routes.",
        ["喜欢长线挑战、享受跨层继承成长的闯塔者。"]="Long-distance challenge lovers enjoying cross-floor progression.",
        ["追求完整体验、挑战极限的闯塔者。"]="Tower climbers seeking the full experience and ultimate challenge.",
        ["开放排行榜"]="Leaderboard enabled",["支持每日挑战词条"]="Daily challenge mods",
        ["精英怪增加"]="More elites",["支持锚点系统"]="Anchor system enabled",
        ["Boss层完整"]="Full boss floors",["全进化路径解锁"]="All evolution paths unlocked",
        ["选择你的闯塔之路"]="Choose your path",
        ["今日挑战"]="Daily Challenge",["每日挑战"]="Daily",["本周挑战"]="Weekly",
        ["适合:"]="Suited for:",["结算特性:"]="Features:",
        ["自定义种子"]="Custom Seed",["开始"]="Start",
        ["开始每日挑战"]="Start Daily Challenge",["开始本次闯塔"]="Begin Ascent",
        ["开启新一轮闯塔"]="Start New Ascent",
        ["返回主页"]="Back to Menu",

        // 退出确认
        ["确定要退出游戏吗？"]="Are you sure you want to quit?",
        ["不保存"]="Don't Save",

        // 最近闯塔卡片
        ["最近一次闯塔"]="Last Run",["同步正常"]="Synced",
        ["模式"]="Mode",["楼层"]="Floor",["宿主"]="Host",
        ["形态"]="Form",["污染"]="Pollution",["记录"]="Record",
        ["新存档"]="New Save",["寄生体"]="Parasite",

        // 统计扩展
        ["总闯塔次数"]="Total Runs",["最深楼层"]="Deepest Floor",
        ["历史统计"]="Historical Stats",

        // 污染技能扩展
        ["当前污染值"]="Current Pollution",
        ["已解锁"]="Unlocked",["生效中"]="Active",
        ["解锁条件"]="Unlock Req.",["污染等级"]="Pollution Tier",

        // overlay标题
        ["异种图鉴"]="Bestiary",["暗塔排行"]="Leaderboard",
        ["回响商店"]="Echo Shop",["成就回响"]="Achievements",
        ["记忆档案"]="Archives",["进化树"]="Evolution",
        ["点击已解锁怪物查看详情"]="Tap unlocked monsters for details",

        // 记录/重置
        ["槽位"]="Slot",
        ["将清除所有历史统计数据，此操作不可撤销。"]="This will clear all historical stats. This cannot be undone.",

        // 赏金猎人挑战描述
        ["赏金猎人: 击杀奖励EP x 2  但怪物HP x 1.5"]="Bounty Hunter: Kill EP x2, but monster HP x1.5",
    };

    static bool _useEnglish = UnityEngine.PlayerPrefs.GetInt("PT_UseEnglish", 0) == 1;
    public static bool UseEnglish { get => _useEnglish; set => _useEnglish = value; }

    public static string T(string zh)
    {
        if (!_useEnglish) return zh;
        return ZhToEn.TryGetValue(zh, out var en) ? en : zh;
    }
}
