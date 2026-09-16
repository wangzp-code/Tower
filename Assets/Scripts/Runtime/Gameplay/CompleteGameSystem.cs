using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public partial class CompleteGameSystem : SingletonBase<CompleteGameSystem>
{
    public enum RunScreen
    {
        MainMenu,
        CharacterSelect,
        Exploration,
        Combat,
        Victory,
        Defeat,
        RunEnd,
        GameOver,
        Ending,
        StageTransition
    }

    private RunScreen _currentScreen = RunScreen.MainMenu;
    public RunScreen CurrentScreen { get => _currentScreen; private set { 
        if (_currentScreen != value) { 
            if (_currentScreen == RunScreen.Combat && value != RunScreen.Combat)
                AudioManager.Instance?.RestoreBGM(0.3f);
            _currentScreen = value; 
            AudioManager.Instance?.SetScreenBGM(value); 
        } 
    } }
    public FloorRuntime CurrentFloorState { get; private set; }
    public void SetFloorState(FloorRuntime state)
    {
        if (state != null)
        {
            if (state.discovered == null) state.discovered = new bool[13, 13];
            if (state.walkable == null) state.walkable = new bool[13, 13];
            if (state.actions == null) state.actions = new Dictionary<Vector2Int, ExploreAction>();
        }
        CurrentFloorState = state;
    }

    public void LoadSavedFloorState()
    {
        var saveInfo = SaveSystem.Instance?.GetSaveInfo(0);
        if (saveInfo?.floorState == null) return;

        var fs = saveInfo.floorState;
        var floorState = new FloorRuntime
        {
            floor = fs.floor,
            zone = fs.zone,
            playerPos = new Vector2Int(fs.playerPosX, fs.playerPosY),
            exitPos = new Vector2Int(fs.exitPosX, fs.exitPosY),
            stepsTaken = fs.stepsTaken,
            discovered = new bool[13, 13],
            walkable = new bool[13, 13],
            actions = new Dictionary<Vector2Int, ExploreAction>()
        };

        foreach (var cell in fs.discoveredCells ?? new List<string>())
        {
            var parts = cell.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            {
                if (x >= 0 && x < 13 && y >= 0 && y < 13)
                    floorState.discovered[y, x] = true;
            }
        }

        foreach (var cell in fs.walkableCells ?? new List<string>())
        {
            var parts = cell.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            {
                if (x >= 0 && x < 13 && y >= 0 && y < 13)
                    floorState.walkable[y, x] = true;
            }
        }

        foreach (var actionSave in fs.actions ?? new List<ExploreActionSave>())
        {
            var pos = new Vector2Int(actionSave.posX, actionSave.posY);
            var action = new ExploreAction
            {
                type = (ExploreActionType)actionSave.type,
                title = actionSave.title,
                description = actionSave.description,
                consumed = actionSave.consumed
            };

            if (actionSave.monster != null)
            {
                var m = actionSave.monster;
                Color color = Color.white;
                if (!string.IsNullOrEmpty(m.colorHex))
                    ColorUtility.TryParseHtmlString("#" + m.colorHex, out color);

                action.monster = new MonsterRuntime
                {
                    id = m.id,
                    name = m.name,
                    hp = m.hp,
                    maxHp = m.maxHp,
                    atk = m.atk,
                    def = m.def,
                    zone = m.zone,
                    isBoss = m.isBoss,
                    traits = m.traits,
                    axes = m.axes,
                    possessBaseChance = m.possessBaseChance,
                    color = color,
                    stairGuard = m.stairGuard
                };
            }

            floorState.actions[pos] = action;
        }

        CurrentFloorState = floorState;
    }

    public MonsterRuntime CurrentEnemy { get; private set; }
    private Vector2Int? _peacefulMonsterPos;
    public string LastMessage { get; private set; } = "";
    public string LastCombatMessage { get; private set; } = "";
    public string LastEndingTitle { get; private set; } = "";
    public string LastEndingText { get; private set; } = "";
    public bool AutoCombatEnabled { get; private set; }
    public string PendingStoryTitle { get; private set; } = "";
    public string PendingStoryText { get; private set; } = "";
    public class Report
    {
        public bool isVictory;
        public string modeName;
        public string deathCause;
        public string rank;
        public int floorsReached;
        public int maxFloors;
        public int possessions;
        public int kills;
        public float survivalSeconds;
        public string longestHost;
        public float longestHostSeconds;
        public float maxPollution;
        public string finalForm;
        public int score;
        public int echoReward;
        public int legacyCount;
        public string legacySummary;
        public string pollutionTier;
        public int formCount;
        public int stagesCleared;
        public List<string> possessedForms;
        public List<string> legacyNames;
        public List<string> resonanceTriggers;
        public int maxDamage;
        public string maxDamageCombo;
        public string defeatAdvice;
    }

    public Report LastReport { get; private set; } = null;

    // Expedition stage transition
    public class ExpeditionRewardOption
    {
        public string id;
        public string name;
        public string desc;
        public string icon;
    }

    public static readonly ExpeditionRewardOption[] AllExpeditionRewards = new[]
    {
        new ExpeditionRewardOption { id = "atk", name = "寄生强化", desc = "永久ATK+3", icon = "⚔" },
        new ExpeditionRewardOption { id = "def", name = "宿主适应", desc = "永久DEF+2", icon = "◈" },
        new ExpeditionRewardOption { id = "hp",  name = "生命回路", desc = "永久MaxHP+15", icon = "♥" },
        new ExpeditionRewardOption { id = "pol", name = "腐蚀抵抗", desc = "永久回复+1/回合", icon = "☢" },
        new ExpeditionRewardOption { id = "ep",  name = "进化记忆", desc = "每关起始EP+20", icon = "★" },
        new ExpeditionRewardOption { id = "pos", name = "形态亲和", desc = "附身率+5%", icon = "◆" },
    };

    public ExpeditionRewardOption[] PendingStageRewards { get; private set; }
    public int StageKills { get; private set; }
    public float StageTime { get; private set; }

    // Combat log (multi-line)
    public List<string> CombatLog { get; private set; } = new List<string>();
    public void AddCombatLog(string msg) { CombatLog.Add(msg); if (CombatLog.Count > 30) CombatLog.RemoveAt(0); LastCombatMessage = msg; EventBus.Emit(EventTypes.CombatLogUpdated); }

    // Route selection
    public bool ShowingRouteSelect { get; private set; }
    public string[] PendingRoutes { get; private set; }
    public string[] PendingRouteDescs { get; private set; }

    // Death form selection
    public bool ShowingDeathFormSelect { get; private set; }

    // Death rollback selection
    public bool ShowingDeathRollback { get; private set; }
    public int DeathRollbackEpCost { get; private set; }
    public int DeathRollbackAnchorFloor { get; private set; }

    // Form replace selection (possession when slots full)
    public bool ShowingFormReplace { get; private set; }
    public List<(int index, string formId)> FormReplaceOptions { get; private set; }

    // Fragment pick-up choice
    public bool ShowingFragmentChoice { get; private set; }
    public string[] FragmentChoices { get; private set; }
    public string[] FragmentChoiceDescs { get; private set; }

    // Floor signature
    public string CurrentFloorSignature { get; private set; } = "";
    public float FloorSignatureAtkMult { get; private set; } = 1f;
    public float FloorSignatureDefMult { get; private set; } = 1f;
    public float FloorSignatureEpMult { get; private set; } = 1f;
    public FloorSignatureData.Signature ActiveSignature { get; private set; }
    private Dictionary<int, string> _floorSignatureMap = new Dictionary<int, string>();
    // 猎杀令目标
    public string BountyTargetId { get; private set; }
    public string BountyTargetName { get; private set; }
    // 审判系统
    private List<string> _judgmentKillOrder;
    private int _judgmentKillIdx;
    // 污染风暴步数计数
    private int _polStormSteps;

    // ======== 每日/每周挑战词条系统 ========
    public class ChallengeModifier
    {
        public string id;
        public string name;
        public string icon;
        public string desc;
        public float monsterHpMult;
        public float monsterAtkMult;
        public float epMult;
        public float eliteRateAdd;
        public float pollutionMult;
        public float playerDmgTakenMult;
        public float playerDmgDealtMult;
        public float healMult;
        public bool monsterRegen;
        public bool enrage;        // HP<50% 伤害×1.5
        public bool vampiric;      // 怪物吸血
        public bool thorns;        // 反伤
        public bool noFlee;        // 禁止逃跑
        public bool fog;           // 视野缩小
    }

    static readonly ChallengeModifier[] DailyModPool = new ChallengeModifier[]
    {
        new ChallengeModifier { id="enrage_night", name="狂暴之夜", icon="🔥",
            desc="所有怪物狂暴（HP<50%伤害×1.5）",
            monsterHpMult=1.2f, monsterAtkMult=1.0f, epMult=1.5f, enrage=true },
        new ChallengeModifier { id="iron_skin", name="铁皮浪潮", icon="🛡",
            desc="怪物HP×1.8，但EP奖励×2",
            monsterHpMult=1.8f, monsterAtkMult=1.0f, epMult=2.0f },
        new ChallengeModifier { id="glass_cannon", name="玻璃大炮", icon="💥",
            desc="怪物ATK×2，但HP×0.6",
            monsterHpMult=0.6f, monsterAtkMult=2.0f, epMult=1.3f },
        new ChallengeModifier { id="toxic_fog", name="毒雾弥漫", icon="☢",
            desc="污染增长×2，视野缩小",
            pollutionMult=2.0f, epMult=1.5f, fog=true },
        new ChallengeModifier { id="elite_surge", name="精英涌潮", icon="⚔",
            desc="精英怪率+30%，EP奖励×1.8",
            eliteRateAdd=0.30f, epMult=1.8f },
        new ChallengeModifier { id="vampiric", name="血族之夜", icon="🧛",
            desc="怪物吸血（造成伤害回复HP），ATK×1.3",
            monsterAtkMult=1.3f, epMult=1.5f, vampiric=true },
        new ChallengeModifier { id="thorns", name="荆棘迷宫", icon="🌿",
            desc="怪物反伤20%，HP×1.3",
            monsterHpMult=1.3f, epMult=1.5f, thorns=true },
        new ChallengeModifier { id="no_retreat", name="绝地求生", icon="🚫",
            desc="禁止逃跑，怪物ATK×1.2，EP×2",
            monsterAtkMult=1.2f, epMult=2.0f, noFlee=true },
        new ChallengeModifier { id="regen_horde", name="再生军团", icon="💚",
            desc="怪物每回合回复5%HP",
            monsterHpMult=1.0f, epMult=1.8f, monsterRegen=true },
        new ChallengeModifier { id="fragile", name="脆弱之体", icon="💔",
            desc="玩家受到伤害×1.5，但造成伤害×1.3",
            playerDmgTakenMult=1.5f, playerDmgDealtMult=1.3f, epMult=1.5f },
        new ChallengeModifier { id="bounty_hunt", name="赏金猎人", icon="🎯",
            desc="EP奖励×2.5，但怪物全面强化×1.3",
            monsterHpMult=1.3f, monsterAtkMult=1.3f, epMult=2.5f },
        new ChallengeModifier { id="famine", name="饥荒时代", icon="🍂",
            desc="治疗效果×0.5，怪物HP×0.8，EP×1.8",
            monsterHpMult=0.8f, epMult=1.8f, healMult=0.5f },
        new ChallengeModifier { id="berserk_all", name="全面战争", icon="⚡",
            desc="怪物ATK×1.5 HP×1.5，EP×2.5",
            monsterHpMult=1.5f, monsterAtkMult=1.5f, epMult=2.5f },
        new ChallengeModifier { id="pollution_surge", name="污染风暴", icon="☣",
            desc="污染增长×3，但每层获得额外EP",
            pollutionMult=3.0f, epMult=2.0f },
        new ChallengeModifier { id="mirror_world", name="镜像世界", icon="🪞",
            desc="玩家造成伤害×0.7，受到伤害×0.7，EP×2",
            playerDmgDealtMult=0.7f, playerDmgTakenMult=0.7f, epMult=2.0f },
        new ChallengeModifier { id="blood_moon", name="血月降临", icon="🌙",
            desc="怪物ATK×1.8，全部吸血，但HP×0.7",
            monsterHpMult=0.7f, monsterAtkMult=1.8f, vampiric=true, epMult=1.8f },
        new ChallengeModifier { id="steel_wall", name="铜墙铁壁", icon="🏛",
            desc="怪物HP×2.5，ATK×0.7，EP×2.5",
            monsterHpMult=2.5f, monsterAtkMult=0.7f, epMult=2.5f },
        new ChallengeModifier { id="speed_run", name="极速冲锋", icon="💨",
            desc="怪物HP×0.5，但禁止逃跑+反伤15%",
            monsterHpMult=0.5f, noFlee=true, thorns=true, epMult=1.5f },
        new ChallengeModifier { id="dark_maze", name="暗黑迷宫", icon="🌑",
            desc="视野极小，精英率+20%，EP×2",
            fog=true, eliteRateAdd=0.20f, epMult=2.0f },
        new ChallengeModifier { id="thorn_garden", name="荆棘花园", icon="🌹",
            desc="反伤30%+怪物再生，但ATK×0.8，EP×2",
            monsterAtkMult=0.8f, thorns=true, monsterRegen=true, epMult=2.0f },
        new ChallengeModifier { id="treasure_hunt", name="寻宝猎人", icon="💎",
            desc="EP×3，但怪物HP×2 ATK×1.5",
            monsterHpMult=2.0f, monsterAtkMult=1.5f, epMult=3.0f },
        new ChallengeModifier { id="infection", name="感染扩散", icon="🦠",
            desc="污染×2.5+治疗×0.6，精英率+15%，EP×2",
            pollutionMult=2.5f, healMult=0.6f, eliteRateAdd=0.15f, epMult=2.0f },
        new ChallengeModifier { id="last_stand", name="背水一战", icon="⛔",
            desc="禁逃+怪物狂暴+ATK×1.3，EP×2.5",
            monsterAtkMult=1.3f, noFlee=true, enrage=true, epMult=2.5f },
        new ChallengeModifier { id="titan_rush", name="巨人来袭", icon="🗿",
            desc="怪物HP×3 ATK×0.5，再生5%/回合，EP×2",
            monsterHpMult=3.0f, monsterAtkMult=0.5f, monsterRegen=true, epMult=2.0f },
    };

    static readonly ChallengeModifier[] WeeklyModPool = new ChallengeModifier[]
    {
        new ChallengeModifier { id="w_nightmare", name="噩梦协议", icon="💀",
            desc="怪物全属性×1.8，精英率+25%，EP×3",
            monsterHpMult=1.8f, monsterAtkMult=1.8f, eliteRateAdd=0.25f, epMult=3.0f },
        new ChallengeModifier { id="w_corruption", name="深度腐蚀", icon="☢",
            desc="污染增长×2.5，怪物吸血，视野缩小",
            pollutionMult=2.5f, vampiric=true, fog=true, epMult=2.0f },
        new ChallengeModifier { id="w_siege", name="围城之战", icon="🏰",
            desc="精英率+40%，怪物HP×2，禁止逃跑，EP×3",
            monsterHpMult=2.0f, eliteRateAdd=0.40f, noFlee=true, epMult=3.0f },
        new ChallengeModifier { id="w_glass_world", name="玻璃世界", icon="🔮",
            desc="双方伤害×2，治疗×0.5",
            playerDmgTakenMult=2.0f, playerDmgDealtMult=2.0f, monsterAtkMult=2.0f, healMult=0.5f, epMult=2.5f },
        new ChallengeModifier { id="w_death_march", name="死亡行军", icon="💀",
            desc="禁逃+怪物狂暴+吸血+HP×1.5，EP×3.5",
            monsterHpMult=1.5f, noFlee=true, enrage=true, vampiric=true, epMult=3.5f },
        new ChallengeModifier { id="w_hell_fog", name="地狱迷雾", icon="🌫",
            desc="视野极小+污染×3+精英+30%+反伤，EP×3",
            fog=true, pollutionMult=3.0f, eliteRateAdd=0.30f, thorns=true, epMult=3.0f },
        new ChallengeModifier { id="w_titan_siege", name="巨神攻城", icon="⚔",
            desc="怪物HP×3 ATK×2，再生+吸血，EP×4",
            monsterHpMult=3.0f, monsterAtkMult=2.0f, monsterRegen=true, vampiric=true, epMult=4.0f },
        new ChallengeModifier { id="w_chaos", name="混沌试炼", icon="🌀",
            desc="全词条叠加：狂暴+反伤+再生+迷雾，EP×3",
            enrage=true, thorns=true, monsterRegen=true, fog=true, monsterHpMult=1.3f, epMult=3.0f },
    };

    public ChallengeModifier ActiveChallengeModifier { get; private set; }

    public static ChallengeModifier GetTodayDailyModifier()
    {
        int daySeed = System.DateTime.Now.Year * 10000 + System.DateTime.Now.Month * 100 + System.DateTime.Now.Day;
        int idx = Mathf.Abs(daySeed * 2654435761.GetHashCode()) % DailyModPool.Length;
        return DailyModPool[Mathf.Abs(idx) % DailyModPool.Length];
    }

    public static ChallengeModifier GetThisWeekModifier()
    {
        var now = System.DateTime.Now;
        int weekSeed = now.Year * 100 + (now.DayOfYear / 7);
        int idx = Mathf.Abs(weekSeed * 2654435761.GetHashCode()) % WeeklyModPool.Length;
        return WeeklyModPool[Mathf.Abs(idx) % WeeklyModPool.Length];
    }

    public void RestoreChallengeModifier(string modId)
    {
        ActiveChallengeModifier = FindModifierById(modId);
    }

    public static ChallengeModifier FindModifierById(string modId)
    {
        if (string.IsNullOrEmpty(modId)) return null;
        foreach (var m in DailyModPool)
            if (m.id == modId) return m;
        foreach (var m in WeeklyModPool)
            if (m.id == modId) return m;
        return null;
    }

    public int EvolutionLevel { get; private set; }
    public AltarData.AltarPair PendingAltar { get; private set; }
    public bool ShowingAltar { get; private set; }
    public bool ShowingCollapse { get; private set; }
    public bool InWaveDefense { get; private set; }
    public int WaveNumber { get; private set; }
    public int WaveMonstersRemaining { get; private set; }

    // 原版5阶段新手引导系统
    public int TutorialStage { get; private set; }
    public string TutorialHint { get; private set; } = "";
    public string TutorialTitle { get; private set; } = "";
    public string TutorialTarget { get; private set; } = "";
    public string TutorialPos { get; private set; } = "";
    public bool IsInTutorial { get; private set; }
    public bool FirstRunDone { get; private set; }
    public bool TutorialRequired { get; private set; }
    public string TutorialStepLabel { get; private set; } = "";
    public bool ForceTutorial { get; private set; }
    public void SetForceTutorial(bool value) { ForceTutorial = value; }

    // 软提示气泡
    public string ActiveSoftHint { get; private set; } = "";
    public string ActiveSoftHintTarget { get; private set; } = "";
    float _softHintTime;
    HashSet<string> _shownSoftHints = new HashSet<string>();
    HashSet<string> _persistentShownSoftHints = new HashSet<string>();
    public HashSet<string> ShownSoftHintIds => _shownSoftHints;
    const string PersistentSoftHintKey = "ShownSoftHints_" + TutorialVersion;

    public enum TutorialType
    {
        Info,
        Action
    }

    public enum TutorialAction
    {
        None,
        Attack,
        Defend,
        Possess,
        Blessing
    }

    [System.Serializable]
    public class TutorialStep
    {
        public string id;
        public int stage;
        public string title;
        public string desc;
        public string trigger;
        public bool required;
        public string target;
        public string pos;
        public string icon;
        public TutorialType type = TutorialType.Info;
        public TutorialAction actionType = TutorialAction.None;
        public string nextButtonText;
    }

    static readonly TutorialStep[] AllTutorialSteps = new TutorialStep[]
    {
        // S0 觉醒
        new TutorialStep{id="move",     stage=0, title="移动",     desc="点击方向键或滑动", trigger="move", required=true, target="dpad-container", pos="top"},
        new TutorialStep{id="attack",   stage=0, title="攻击",     desc="点击攻击按钮进行战斗", trigger="combat", required=true, target="btn-attack-bottom", pos="top"},
        // S1 寄生
        new TutorialStep{id="possess",  stage=1, title="附身",     desc="虚弱目标更易附身\n攻击使怪物HP降至50%以下时触发意识裂隙", trigger="firstKill", required=true, target="btn-possess-bottom", pos="top"},
        // S2 战术
        new TutorialStep{id="new_body", stage=2, title="新身体",   desc="你现在变成了看门犬！用新身体战斗吧！", trigger="new_body", required=false, target="btn-attack-bottom", pos="top"},
        new TutorialStep{id="form",     stage=2, title="形态切换", desc="顶部形态栏可切换已获得形态", trigger="formUnlock", required=false, target="fslot-0", pos="bottom"},
        new TutorialStep{id="defend",   stage=2, title="防御",     desc="防御=减半伤害+回少量HP\n面对强敌时善用", trigger="defendHint", required=false, target="btn-defend-bottom", pos="top"},
        new TutorialStep{id="inspect",  stage=2, title="查看怪物", desc="长按地图怪物可查看属性", trigger="inspect", required=false, target=null, pos="center"},
        new TutorialStep{id="anchor",   stage=2, title="记忆锚定", desc="点击顶部锚点栏查看锚点\n祭坛可消耗200EP固化记忆\n死亡后回到锚定楼层", trigger="anchorHint", required=false, target="anchor-bar", pos="bottom"},
        // S3 成长
        new TutorialStep{id="evolution",stage=3, title="进化点",   desc="菜单→进化 解锁更强能力", trigger="evoHint", required=false, target="btn-menu-icon", pos="bottom"},
        new TutorialStep{id="ultimate", stage=3, title="终极技能", desc="右下技能球：危急时使用", trigger="ultReady", required=false, target="btn-ultimate", pos="top"},
        // S4 系统
        new TutorialStep{id="shop",     stage=4, title="残雪商店", desc="这里是残雪商店\n消耗进化点(EP)购买补给和强化", trigger="shop", required=false, target="btn-menu-icon", pos="bottom"},
        new TutorialStep{id="pollution",stage=4,title="污染",     desc=">80%异变 / 100%失控\n用净化道具压制", trigger="pollution", required=false, target="pol-badge", pos="bottom"},
        new TutorialStep{id="signature",stage=4,title="楼层签名", desc="每层 modifier 影响战斗规则", trigger="signature", required=false, target=null, pos="center"},
    };

    static readonly string[] StageSummary = {
        "",
        "",
        "觉醒完成 ✓ 下一步：尝试附身宿主",
        "附身完成 ✓ 下一步：形态切换 / 防御",
        "战术完成 ✓ 下一步：进化点 / 终极技能",
        "成长完成 ✓ 全系统已开放（商店/签名/污染）"
    };
    
    // 教程版本号 - bump 这个值会清空所有"已显示"标记
    const string TutorialVersion = "v6";
    
    // 持久化的已显示教程步骤（跨运行保存）
    HashSet<string> _persistentShownTutorials = new HashSet<string>();
    const string PersistentTutorialKey = "ShownTutorials_" + TutorialVersion;
    
    // 软提示系统（柔性教程）
    public class SoftHint {
        public string id;
        public int stage;
        public string target;
        public string targetId;
        public string text;
        public string trigger;
    }
    
    static readonly SoftHint[] SoftHints = new SoftHint[] {
        new SoftHint{id="hint_attack",stage=0,target=".act-btn.fight",targetId="attack",text="点击攻击开始战斗！",trigger="attack"},
        new SoftHint{id="hint_possess",stage=2,target=null,targetId="possess",text="尝试「附身」获取怪物能力！",trigger="possess"},
        new SoftHint{id="hint_defend",stage=2,target=null,targetId="defend",text="防御可减半伤害+恢复HP",trigger="defend"},
        new SoftHint{id="hint_flee",stage=2,target=null,targetId="flee",text="打不过？试试逃跑保全实力",trigger="flee"},
        new SoftHint{id="hint_form_switch",stage=2,target=null,targetId="form",text="点击顶部形态栏切换身体",trigger="form"},
        new SoftHint{id="hint_evolution",stage=3,target=null,targetId="evolution",text="点击进化按钮强化能力",trigger="evolution"},
        new SoftHint{id="hint_ultimate",stage=3,target=null,targetId="ultimate",text="终极技能就绪！危急时使用",trigger="ultimate"},
        new SoftHint{id="hint_anchor",stage=4,target=null,targetId="anchor",text="点击锚点栏查看记忆锚定",trigger="anchor"},
        new SoftHint{id="hint_shop",stage=4,target=null,targetId="shop",text="残雪商店：消耗EP购买补给",trigger="shop"},
        new SoftHint{id="hint_pollution",stage=4,target=null,targetId="pollution",text="污染>80%会异变，注意控制",trigger="pollution"}
    };

    bool _firstPossession = true;
    List<string> _resonanceTriggersThisRun = new List<string>();
    int _maxDamageThisRun = 0;
    string _maxDamageComboThisRun = "";

    HashSet<string> _shownTutorials = new HashSet<string>();
    float _tutLastShownTime;
    string _currentTutStepId;
    bool _tutorialDismissed;
    string _pendingTutTriggerId;
    public float ShortModeTimer { get; private set; }
    public bool ShortModeTimerActive { get; private set; }

    void LoadPersistentTutorials()
    {
        _persistentShownTutorials = new HashSet<string>();
        if (PlayerPrefs.HasKey(PersistentTutorialKey))
        {
            string data = PlayerPrefs.GetString(PersistentTutorialKey, "");
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var id in data.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                        _persistentShownTutorials.Add(id);
                }
            }
        }

        _persistentShownSoftHints = new HashSet<string>();
        if (PlayerPrefs.HasKey(PersistentSoftHintKey))
        {
            string data = PlayerPrefs.GetString(PersistentSoftHintKey, "");
            if (!string.IsNullOrEmpty(data))
            {
                foreach (var id in data.Split(','))
                {
                    if (!string.IsNullOrEmpty(id))
                        _persistentShownSoftHints.Add(id);
                }
            }
        }
    }

    void SavePersistentTutorial(string stepId)
    {
        if (string.IsNullOrEmpty(stepId)) return;
        if (_persistentShownTutorials.Contains(stepId)) return;
        _persistentShownTutorials.Add(stepId);
        PlayerPrefs.SetString(PersistentTutorialKey, string.Join(",", _persistentShownTutorials));
        PlayerPrefs.Save();
    }

    void SavePersistentSoftHint(string hintId)
    {
        if (string.IsNullOrEmpty(hintId)) return;
        if (_persistentShownSoftHints.Contains(hintId)) return;
        _persistentShownSoftHints.Add(hintId);
        PlayerPrefs.SetString(PersistentSoftHintKey, string.Join(",", _persistentShownSoftHints));
        PlayerPrefs.Save();
    }

    public bool IsTutorialStepShown(string stepId)
    {
        if (string.IsNullOrEmpty(stepId)) return false;
        return _persistentShownTutorials.Contains(stepId) || _shownTutorials.Contains(stepId);
    }

    public bool IsSoftHintShown(string hintId)
    {
        if (string.IsNullOrEmpty(hintId)) return false;
        return _persistentShownSoftHints.Contains(hintId) || _shownSoftHints.Contains(hintId);
    }

    void MarkSoftHintShown(string hintId)
    {
        _shownSoftHints.Add(hintId);
        SavePersistentSoftHint(hintId);
    }

    private float _autoCombatTimer;
    private const float AutoCombatInterval = 0.3f;
    private readonly HashSet<string> _usedNegotiationOptions = new HashSet<string>();
    private readonly HashSet<int> _usedAltarIds = new HashSet<int>();

    // 战斗系统字段
    private int _combatRound = 0;
    public int CombatRound => _combatRound;
    private int _attackRounds = 0;
    private bool _fleePollProtect = false;
    
    // 附身相关字段
    private string _pendingPossessFormId = null;
    private Action _pendingPossessCallback = null;
    private bool _firstPossessionSuccess = false;
    private int _comboCount = 0;
    private float _comboTimer = 0f;

    // 终极技能CD (原版: 每场战斗用1次, cooldown=战斗次数)
    private int _ultCooldown = 0;
    public int UltCooldown => _ultCooldown;
    public bool UltReady => _ultCooldown <= 0;
    
    // 教程触发检查（原版逻辑）
    bool _tutCheckPending;
    
    public void CheckTutorial(string triggerId)
    {
        if (_shownTutorials == null) _shownTutorials = new HashSet<string>();
        if (string.IsNullOrEmpty(triggerId)) return;
        
        Debug.Log($"[Tutorial] CheckTutorial: triggerId={triggerId}, stage={TutorialStage}, currentTutStepId={_currentTutStepId}, tutCheckPending={_tutCheckPending}");
        Debug.Log($"[Tutorial] persistentShown count={_persistentShownTutorials.Count}, shown count={_shownTutorials.Count}");
        
        if (!string.IsNullOrEmpty(_currentTutStepId))
        {
            Debug.Log($"[Tutorial] SKIP: _currentTutStepId is set: {_currentTutStepId}");
            return;
        }
        if (_tutCheckPending)
        {
            Debug.Log($"[Tutorial] SKIP: _tutCheckPending is true");
            return;
        }
        
        // 4秒冷却时间
        float now = Time.time;
        if (now - _tutLastShownTime < 4f) {
            Debug.Log($"[Tutorial] SKIP: cooldown {now - _tutLastShownTime:F1}s < 4s");
            ScheduleTutorialCheck(triggerId);
            return;
        }
        
        // 等待overlay关闭
        if (IsAnyOverlayOpen(triggerId == "combat" || triggerId == "defendHint" ? "combat-overlay" : null)) {
            Debug.Log($"[Tutorial] SKIP: overlay open");
            ScheduleTutorialCheck(triggerId);
            return;
        }

        int stage = TutorialStage;
        for (int i = 0; i < AllTutorialSteps.Length; i++)
        {
            var step = AllTutorialSteps[i];
            
            Debug.Log($"[Tutorial] Checking step: id={step.id}, trigger={step.trigger}, step.stage={step.stage}, condition: " +
                     $"persistent={_persistentShownTutorials.Contains(step.id)}, shown={_shownTutorials.Contains(step.id)}, " +
                     $"stageOk={step.stage <= stage}, triggerOk={step.trigger == triggerId}");
            
            if (_persistentShownTutorials.Contains(step.id)) continue;
            if (_shownTutorials.Contains(step.id)) continue;
            if (step.stage > stage) continue;
            if (step.trigger != triggerId) continue;
            
            // 攻击教程只在stage 0显示，stage >= 1时不再显示
            if (step.id == "attack" && stage > 0) continue;
            
            // combat触发需要检查目标存在
            if (step.trigger == "combat" && CurrentEnemy == null) continue;
            
            // possess触发需要检查目标可附身
            if (step.trigger == "firstKill" && CurrentEnemy == null) continue;
            
            // ultReady触发需要检查终极技能就绪
            if (step.trigger == "ultReady" && !UltReady) continue;
            
            // shop触发需要检查是否在商店区域
            if (step.trigger == "shop" && CurrentScreen != RunScreen.Exploration) continue;

            Debug.Log($"[Tutorial] FOUND step: id={step.id}, showing tutorial");
            ShowTutorialStep(step);
            return;
        }
        
        Debug.Log($"[Tutorial] No matching step found for triggerId={triggerId}");
    }
    
    void ScheduleTutorialCheck(string triggerId) {
        if (_tutCheckPending) return;
        _tutCheckPending = true;
        _pendingTutTriggerId = triggerId;
        Invoke(nameof(DelayedTutorialCheck), 3f);
    }

    // Delayed invoke target must be a class member for Unity's Invoke to find it
    void DelayedTutorialCheck()
    {
        _tutCheckPending = false;
        var trig = _pendingTutTriggerId;
        _pendingTutTriggerId = null;
        CheckTutorial(trig);
    }
    
    bool IsAnyOverlayOpen(string excludeId) {
        // 检查是否有overlay打开（与原项目保持一致）
        bool combatOpen = CurrentScreen == RunScreen.Combat;
        if (excludeId == "combat-overlay") combatOpen = false;
        
        return combatOpen ||
               ShowingAltar ||
               ShowingCollapse ||
               ShowingFragmentChoice ||
               ShowingDeathFormSelect ||
               ShowingDeathRollback ||
               ShowingFormReplace ||
               ShowingRouteSelect ||
               ShowingStoryEvent;
    }

    void ShowTutorialStep(TutorialStep step)
    {
        _currentTutStepId = step.id;
        _tutLastShownTime = Time.time;
        _shownTutorials.Add(step.id);
        SavePersistentTutorial(step.id);
        TutorialTitle = step.title;
        TutorialHint = step.desc;
        TutorialRequired = step.required;
        IsInTutorial = step.required;
        TutorialTarget = step.target ?? "";
        TutorialPos = step.pos ?? "center";
        int shown = _shownTutorials.Count + _persistentShownTutorials.Count;
        TutorialStepLabel = $"引导 {shown}/{AllTutorialSteps.Length}";
        AddCombatLog($"<color=#ffb340>◆ {step.title}: {step.desc.Replace("\n", " ")}</color>");
        Debug.Log($"[CompleteGameSystem] ShowTutorialStep: id={step.id} stage={step.stage} shown={shown}");

        // 通知TutorialManager并触发Toast软提示
        TutorialManager.Instance?.NotifyTutorialShown(step.stage, step.id);
        
        // 使用Toast显示教程提示
        TutorialManager.Instance?.ShowTutorialToast(step);
        
        // Toast模式: 显示后自动清除currentTutStepId，允许后续教程触发
        StartCoroutine(ClearTutorialStepAfterDelay(4f));
    }
    
    System.Collections.IEnumerator ClearTutorialStepAfterDelay(float delay)
    {
        yield return new WaitForSeconds(delay);
        _currentTutStepId = "";
        TutorialHint = "";
        TutorialTitle = "";
        TutorialTarget = "";
        TutorialPos = "";
        TutorialRequired = false;
        IsInTutorial = false;
        TutorialStepLabel = "";
        _tutCheckPending = false;
        _pendingTutTriggerId = null;
        TutorialManager.Instance?.NotifyTutorialDismissed();
    }

    public void DismissTutorial(string fromActionId = null, bool force = false)
    {
        if (!force && TutorialRequired && !string.IsNullOrEmpty(_currentTutStepId) && fromActionId != _currentTutStepId && fromActionId != "skip") return;
        _currentTutStepId = "";
        TutorialHint = "";
        TutorialTitle = "";
        TutorialTarget = "";
        TutorialPos = "";
        TutorialRequired = false;
        IsInTutorial = false;
        TutorialStepLabel = "";
        _tutCheckPending = false;
        _pendingTutTriggerId = null;
        CancelInvoke(nameof(DelayedTutorialCheck));

        TutorialManager.Instance?.NotifyTutorialDismissed();
    }

    public void AdvanceTutorialStage(int newStage)
    {
        if (newStage <= TutorialStage) return;
        int oldStage = TutorialStage;
        TutorialStage = newStage;
        Debug.Log($"[Tutorial] AdvanceTutorialStage: {oldStage} -> {newStage}");
        if (newStage >= 2) ForceTutorial = false;
        if (newStage < StageSummary.Length && !string.IsNullOrEmpty(StageSummary[newStage]))
        {
            LastMessage = StageSummary[newStage];
            AddCombatLog($"<color=#ffb340>{StageSummary[newStage]}</color>");
        }

        TutorialManager.Instance?.NotifyStageAdvanced(newStage);

        // 强制UI刷新 - 确保按钮状态立即更新
        if (CanvasUIManager.Instance != null)
        {
            CanvasUIManager.Instance.ForceRefresh();
        }

        if (newStage >= 4)
            TutorialManager.Instance?.NotifyTutorialCompleted();

        bool isFirstRun = PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
        if (isFirstRun)
        {
            string[] stageNames = { "start", "attack_done", "possess_done", "new_body_done", "floor_3", "floor_5" };
            string stageName = newStage < stageNames.Length ? stageNames[newStage] : $"stage_{newStage}";
            Analytics.Track("first_run_tutorial", ("stage", stageName), ("floor", GameManager.Instance.CurrentFloor));
        }
    }

    // 软提示气泡系统
    public void CheckSoftHint()
    {
        if (!string.IsNullOrEmpty(_currentTutStepId)) return;
        if (CurrentScreen != RunScreen.Combat || CurrentEnemy == null) return;

        int floor = GameManager.Instance.CurrentFloor;

        // 如果已有提示，不替换
        if (!string.IsNullOrEmpty(ActiveSoftHint)) return;

        // 根据教程阶段显示相应的软提示，每个提示只显示一次
        // 按优先级排序，高阶段提示优先显示
        string[][] defs;
        if (TutorialStage >= 1)
        {
            defs = new string[][]
            {
                new string[]{"hint_possess", "1", "0", "possess", "尝试「附身」获取怪物能力！"},
                new string[]{"hint_trait", "1", "0", "trait", "点击怪物技能栏 [查看] 了解敌人能力"},
                new string[]{"hint_defend", "2", "2", "defend", "「防御」可减半本回合受到的伤害"},
                new string[]{"hint_flee", "2", "2", "flee", "打不过？试试「逃跑」保全实力"},
                new string[]{"hint_attack", "0", "0", "attack", "点击「攻击」开始战斗！"},
            };
        }
        else
        {
            defs = new string[][]
            {
                new string[]{"hint_attack", "0", "0", "attack", "点击「攻击」开始战斗！"},
                new string[]{"hint_possess", "1", "0", "possess", "尝试「附身」获取怪物能力！"},
                new string[]{"hint_trait", "1", "0", "trait", "点击怪物技能栏 [查看] 了解敌人能力"},
                new string[]{"hint_defend", "2", "2", "defend", "「防御」可减半本回合受到的伤害"},
                new string[]{"hint_flee", "2", "2", "flee", "打不过？试试「逃跑」保全实力"},
            };
        }

        foreach (var h in defs)
        {
            string hintId = h[0];
            int requiredStage = int.Parse(h[1]);
            int requiredFloor = int.Parse(h[2]);
            string target = h[3];
            string message = h[4];

            // 检查条件：未显示过、达到教程阶段、达到楼层要求
            if (_persistentShownSoftHints.Contains(hintId)) continue;
            if (_shownSoftHints.Contains(hintId)) continue;
            if (TutorialStage < requiredStage) continue;
            if (floor < requiredFloor) continue;

            // 攻击提示只在新手阶段（stage 0）显示
            if (hintId == "hint_attack" && TutorialStage != 0) continue;

            ActiveSoftHint = message;
            ActiveSoftHintTarget = target;
            _softHintTime = Time.time;
            _shownSoftHints.Add(hintId);
            SavePersistentSoftHint(hintId);
            return;
        }
    }

    public void DismissSoftHint(string action = null)
    {
        if (string.IsNullOrEmpty(ActiveSoftHint)) return;
        if (action != null && ActiveSoftHintTarget != action) return;
        ActiveSoftHint = "";
        ActiveSoftHintTarget = "";
    }

    void UpdateSoftHint()
    {
        if (!string.IsNullOrEmpty(ActiveSoftHint) && Time.time - _softHintTime > 8f)
        {
            ActiveSoftHint = "";
            ActiveSoftHintTarget = "";
        }
    }

    protected override void Awake()
    {
        base.Awake();
        LoadTutorialProgress();
        LoadPersistentTutorials();
    }

    private void LoadTutorialProgress()
    {
        FirstRunDone = SaveSystem.Instance.LoadTutorialCompleted();
    }

    private void Start()
    {
        AudioManager.Instance?.StartBGM();
    }

    private void Update()
    {
        if (CurrentScreen == RunScreen.Exploration)
        {
            HandleExplorationInput();
            UpdateAutoCombat();
        }
        HandleKeyboardInput();
        UpdateSoftHint();

        // 短局模式计时器
        if (ShortModeTimerActive && ShortModeTimer > 0)
        {
            ShortModeTimer -= Time.deltaTime;
            if (ShortModeTimer <= 0)
            {
                ShortModeTimer = 0;
                ShortModeTimerActive = false;
                OnShortModeTimeUp();
            }
        }
    }

    private void HandleExplorationInput()
    {
        if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
        {
            if (ShowingAltar) { ChooseAltar(false); return; }
            if (ShowingCollapse) { ChooseCollapse(false); return; }
            if (ShowingRouteSelect) { SelectRoute(0); return; }
            if (ShowingStoryEvent)
            {
                if (IsIntroPlaying())
                {
                    AdvanceIntroDialogue();
                }
                else
                {
                    DismissStoryEvent();
                }
                return;
            }
        }
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (ShowingAltar) { ChooseAltar(false); return; }
            if (ShowingCollapse) { ChooseCollapse(false); return; }
            if (ShowingRouteSelect) { SelectRoute(0); return; }
            if (ShowingStoryEvent)
            {
                if (IsIntroPlaying())
                {
                    AdvanceIntroDialogue();
                }
                else
                {
                    DismissStoryEvent();
                }
                return;
            }
        }
    }

    private void UpdateAutoCombat()
    {
        if (!AutoCombatEnabled || CurrentScreen != RunScreen.Combat || CurrentEnemy == null
            || ShowingDeathFormSelect || ShowingDeathRollback || ShowingFormReplace || ShowingCollapse)
        {
            return;
        }

        _autoCombatTimer += Time.deltaTime;
        if (_autoCombatTimer >= AutoCombatInterval)
        {
            _autoCombatTimer = 0f;
            PlayerAttack();
        }
    }

    private void HandleKeyboardInput()
    {
        if (CurrentScreen == RunScreen.Exploration && !ShowingAltar && !ShowingCollapse && !ShowingRouteSelect && !ShowingStoryEvent)
        {
            if (Input.GetKeyDown(KeyCode.W) || Input.GetKeyDown(KeyCode.UpArrow))    Move(Vector2Int.up);
            if (Input.GetKeyDown(KeyCode.S) || Input.GetKeyDown(KeyCode.DownArrow))  Move(Vector2Int.down);
            if (Input.GetKeyDown(KeyCode.A) || Input.GetKeyDown(KeyCode.LeftArrow))  Move(Vector2Int.left);
            if (Input.GetKeyDown(KeyCode.D) || Input.GetKeyDown(KeyCode.RightArrow)) Move(Vector2Int.right);
            if (Input.GetKeyDown(KeyCode.E)) ExploreCurrentTile();
            if (Input.GetKeyDown(KeyCode.R)) Rest();
            if (Input.GetKeyDown(KeyCode.V) && CanEvolve()) Evolve();
        }
        else if (CurrentScreen == RunScreen.Combat && CurrentEnemy != null && !AutoCombatEnabled && !ShowingDeathFormSelect)
        {
            if (Input.GetKeyDown(KeyCode.J) || Input.GetKeyDown(KeyCode.Alpha1)) PlayerAttack();
            if (Input.GetKeyDown(KeyCode.K) || Input.GetKeyDown(KeyCode.Alpha2)) PlayerDefend();
            if (Input.GetKeyDown(KeyCode.L) || Input.GetKeyDown(KeyCode.Alpha3)) PlayerFlee();
            if (Input.GetKeyDown(KeyCode.Space)) ToggleAutoCombat();
            if (Input.GetKeyDown(KeyCode.P)) TryPossess("resonance");
        }
    }

    public void StartRun(GameMode mode)
    {
        ActiveChallengeModifier = null;
        CurrentScreen = RunScreen.CharacterSelect;
        AutoCombatEnabled = false;
        _autoCombatTimer = 0f;
        LastMessage = GetModeIntro(mode);
        CombatLog.Clear(); LastCombatMessage = "";
        CurrentEnemy = null;
        CurrentFloorState = null;

        PlayerPrefs.DeleteKey("IntroShown_" + mode.ToString());
        PlayerPrefs.Save();

        // 重置所有局内状态
        EvolutionLevel = 0;
        _polStormSteps = 0;
        _consecutiveDeaths = 0;
        StageKills = 0;
        StageTime = 0f;
        BountyTargetId = null;
        BountyTargetName = null;
        _judgmentKillOrder = null;
        _judgmentKillIdx = 0;

        // 楼层签名
        _floorSignatureMap.Clear();
        CurrentFloorSignature = "";
        FloorSignatureAtkMult = 1f;
        FloorSignatureDefMult = 1f;
        FloorSignatureEpMult = 1f;
        ActiveSignature = null;

        // UI交互状态
        ShowingRouteSelect = false;
        ShowingDeathFormSelect = false;
        ShowingDeathRollback = false;
        ShowingFormReplace = false;
        ShowingFragmentChoice = false;
        ShowingAltar = false;
        ShowingCollapse = false;
        ShowingStoryEvent = false;
        InWaveDefense = false;
        PendingStoryTitle = "";
        PendingStoryText = "";
        LastEndingTitle = "";
        LastEndingText = "";
        PendingRoutes = null;
        PendingRouteDescs = null;
        FormReplaceOptions = null;
        FragmentChoices = null;
        FragmentChoiceDescs = null;
        PendingStageRewards = null;
        _usedAltarIds.Clear();

        // 碎片/技能系统
        SkillFragments.Clear();
        ActiveSkills.Clear();
        PassiveEffects.Clear();
        ShowingSynthConfirm = false;
        PendingSynthTrait = null;
        CurrentFragCandidates = null;
        CurrentFragTraits = null;
        _fragDropCount = 0;
        _nextAtkMult = 1f;
        _nextCrit = false;
        _shieldTurns = 0;
        _dodgeTurns = 0;
        _enemyStunTurns = 0;
        _enemyPoisonTurns = 0;
        _enemyPoisonRate = 0f;
        _enemyBleedRate = 0f;
        _perfectCounterNext = false;
        _decoyActive = false;

        // 构筑轴系统
        BuildAxesSystem.Instance?.ResetAll();

        // 污染系统
        PollutionSystem.Instance?.ResetState();

        // 初始化玩家形态属性
        var player = GameManager.Instance.Player;
        if (player != null && !string.IsNullOrEmpty(player.currentFormId))
        {
            LoadForm(player.currentFormId);
            player.formHpMap[player.currentFormId] = player.maxHp;
            player.formMaxHpMap[player.currentFormId] = player.maxHp;
        }
    }

    public void StartChallengeMode(string type, string seed = "")
    {


        GameMode mode;
        ChallengeModifier mod;
        if (type == "weekly")
        {
            mode = GameMode.Weekly;
            mod = GetThisWeekModifier();
        }
        else if (type == "custom")
        {
            mode = GameMode.Daily;
            int seedHash = string.IsNullOrEmpty(seed) ? UnityEngine.Random.Range(0, 99999) : seed.GetHashCode();
            int idx = Mathf.Abs(seedHash) % DailyModPool.Length;
            mod = DailyModPool[idx];
        }
        else
        {
            mode = GameMode.Daily;
            mod = GetTodayDailyModifier();
        }

        ActiveChallengeModifier = mod;

        GameManager.Instance.CurrentMode = mode;
        GameManager.Instance.MaxFloor = mode == GameMode.Weekly ? 30 : 20;
        GameManager.Instance.CurrentStage = 1;
        GameManager.Instance.ExpeditionData = null;
        GameManager.Instance.CreateNewPlayer();
        GameManager.Instance.CurrentFloor = 1;
        GameManager.Instance.IsNewGame = true;
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);

        string modeLabel = type == "weekly" ? "本周挑战" : "每日挑战";
        StartRun(mode);
        ActiveChallengeModifier = mod;
        string weeklyBonus = type == "weekly" ? "\n🏆 通关额外+100残响" : "";
        LastMessage = $"{modeLabel}开始！{mod.icon} {mod.name}：{mod.desc}{weeklyBonus}";
    }

    public void ResumeRun(GameMode mode)
    {
        AutoCombatEnabled = false;
        _autoCombatTimer = 0f;
        CombatLog.Clear(); LastCombatMessage = "";
        CurrentEnemy = null;

        AssignFloorSignatures();
        
        if (CurrentFloorState == null)
        {
            LoadSavedFloorState();
            if (CurrentFloorState == null)
            {
                GenerateFloor(GameManager.Instance.CurrentFloor);
            }
        }
        CurrentScreen = RunScreen.Exploration;

        if (mode == GameMode.Short)
        {
            ShortModeTimer = 900f;
            ShortModeTimerActive = true;
        }
        else
        {
            ShortModeTimerActive = false;
        }

        // 恢复存档时也初始化新系统
        LegacyManager.Instance?.Initialize();
        LegacyManager.Instance?.SubscribeHooks();
        FormResonanceSystem.Instance?.Initialize();
        CorruptionSkillManager.Instance?.Initialize();
        FloorEventManager.Instance?.Initialize();
        BossAIManager.Instance?.Initialize();

        // 恢复引导阶段
        var saveInfo = SaveSystem.Instance?.GetSaveInfo(0);
        if (saveInfo?.player != null)
        {
            AdvanceTutorialStage(saveInfo.player.tutorialStage);
            SetForceTutorial(saveInfo.player.forceTutorial);

            if (saveInfo.player.tutorialStage < 4)
                TutorialManager.Instance?.NotifyTutorialShown(saveInfo.player.tutorialStage, "restore");
        }

        LastMessage = $"存档已恢复。第 {GameManager.Instance.CurrentFloor} 层。";
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public void SelectClass(string classId)
    {
        var classStats = Array.Find(OriginalGameData.ClassBaseStats, s => s.id == classId);
        if (classStats == null)
        {
            classStats = OriginalGameData.ClassBaseStats[0];
        }

        GameManager.Instance.SetClassBaseStats(classStats.id, classStats.maxHp, classStats.atk, classStats.def, classStats.fogRadius);
        ApplyClassIdentity(classStats.id);
        AssignFloorSignatures();
        GenerateFloor(GameManager.Instance.CurrentFloor);
        CurrentScreen = RunScreen.Exploration;

        // Init run statistics
        var p = GameManager.Instance.Player;
        p.runStartTime = Time.time;
        p.maxPollutionReached = 0f;
        p.longestFormId = "human";
        p.longestFormDuration = 0f;
        p.currentFormStartTime = Time.time;
        p.runNumber++;

        TutorialStage = 0;
        ForceTutorial = true;
        _shownTutorials.Clear();
        _shownSoftHints.Clear();
        _persistentShownTutorials.Clear();
        _persistentShownSoftHints.Clear();
        PlayerPrefs.DeleteKey(PersistentTutorialKey);
        PlayerPrefs.DeleteKey(PersistentSoftHintKey);
        _currentTutStepId = "";
        _consecutiveDeaths = 0;

        TutorialManager.Instance?.ResetTutorialProgress();

        // 先执行第一个教程的检查
        CheckTutorial("move");

        if (GameManager.Instance.CurrentMode == GameMode.Short)
        {
            ShortModeTimer = 900f;
            ShortModeTimerActive = true;
        }
        else
        {
            ShortModeTimerActive = false;
        }

        // 初始化新系统
        LegacyManager.Instance?.Initialize();
        LegacyManager.Instance?.CheckUnlockConditions();
        LegacyManager.Instance?.SubscribeHooks();
        FormResonanceSystem.Instance?.Initialize();
        CorruptionSkillManager.Instance?.Initialize();
        FloorEventManager.Instance?.Initialize();
        FloorEventManager.Instance?.OnNewRun();
        BossAIManager.Instance?.Initialize();
        EventBus.Emit(EventTypes.RunStart);

        LastMessage = $"{GetClassName(classStats.id)} 已接入。第 {GameManager.Instance.CurrentFloor} 层开始。";
        GameManager.Instance.NotifyPlayerStatsChanged();

        bool introShown = PlayerPrefs.GetInt("IntroShown_" + GameManager.Instance.CurrentMode, 0) == 1;
        if (!introShown)
        {
            Debug.Log($"[Intro] Triggering intro dialogue for mode: {GameManager.Instance.CurrentMode}");
            StartIntroDialogue();
        }
        else
        {
            Debug.Log($"[Intro] Already shown for mode: {GameManager.Instance.CurrentMode}, skipping.");
        }
    }

    public void Move(Vector2Int dir)
    {
        if (CurrentFloorState == null) return;
        var target = CurrentFloorState.playerPos + dir;

        if (target.x < 0 || target.x >= 13 || target.y < 0 || target.y >= 13) return;
        if (!CurrentFloorState.walkable[target.y, target.x])
        {
            StorySystem.Instance?.CheckHiddenStory("wall_bump");
            return;
        }

        // 在更新位置之前，先检查目标位置是否有需要处理的事件
        // 这样可以防止玩家穿过怪物
        if (CurrentFloorState.actions.TryGetValue(target, out var action) && !action.consumed)
        {
            // 新手引导：检查是否按顺序进行
            if (ForceTutorial && GameManager.Instance.CurrentFloor == 1 && action.type == ExploreActionType.Monster)
            {
                if (TutorialStage == 0 && action.monster.id != "rat" && GameManager.Instance.Player.totalKillsThisRun == 0)
                {
                    LastMessage = "先去攻击那只虚弱实验鼠！了解战斗基础操作";
                    AddCombatLog("<color=#ff8c00>⚠ 请先攻击前方的虚弱实验鼠，了解战斗基础操作</color>");
                    return;
                }
                if (TutorialStage == 0 && action.monster.id != "dog" && GameManager.Instance.Player.totalKillsThisRun > 0)
                {
                    LastMessage = "去附身那只看门犬！学习核心机制";
                    AddCombatLog("<color=#ff8c00>⚠ 请去附身看门犬，学习核心附身机制</color>");
                    return;
                }
            }
        }

        CurrentFloorState.playerPos = target;
        CurrentFloorState.stepsTaken++;
        AudioManager.Instance?.PlaySFX("move");
        CheckTutorial("move");

        // 再生之地/饥饿沼泽：每步回复 HP
        if (ActiveSignature != null && ActiveSignature.regenPerStep > 0)
        {
            var rp = GameManager.Instance.Player;
            int regen = Mathf.Max(1, Mathf.RoundToInt(rp.maxHp * ActiveSignature.regenPerStep));
            rp.hp = Mathf.Min(rp.maxHp, rp.hp + regen);
        }

        // 毒雾弥漫：每步+污染
        if (ActiveSignature != null && ActiveSignature.polPerStep > 0)
        {
            PollutionSystem.Instance?.GainPollution(ActiveSignature.polPerStep);
        }

        // 污染风暴：每10步+5污染
        if (ActiveSignature != null && ActiveSignature.id == "pollutionStorm")
        {
            _polStormSteps++;
            if (_polStormSteps >= 10)
            {
                _polStormSteps = 0;
                PollutionSystem.Instance?.GainPollution(5);
                AddCombatLog("☢ 污染风暴 — 污染+5");
            }
        }

        // 黑暗降临：视野缩小
        int effectiveRadius = GameManager.Instance.Player.classFogRadius;
        if (ActiveSignature != null && ActiveSignature.fogRadius > 0)
            effectiveRadius = ActiveSignature.fogRadius;
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.fog)
            effectiveRadius = Mathf.Max(1, effectiveRadius - 1);
        RevealAround(target, effectiveRadius);

        if (CurrentFloorState.actions.TryGetValue(target, out var targetAction) && !targetAction.consumed)
        {
            // 和平区：怪物不拦路，但记录位置以便玩家主动攻击
            if (ActiveSignature != null && ActiveSignature.peacefulMonsters
                && targetAction.type == ExploreActionType.Monster && targetAction.monster != null)
            {
                LastMessage = $"◇ {targetAction.monster.name} 不具敌意。";
                _peacefulMonsterPos = target;
            }
            else
            {
                ResolveExploreAction(targetAction, target);
                _peacefulMonsterPos = null;
            }
        }
        else if (target == CurrentFloorState.exitPos)
        {
            if (ForceTutorial && TutorialStage < 2)
            {
                LastMessage = "先附身一只怪物再下楼 — 这是核心机制";
                AddCombatLog("<color=#ff8c00>➤ 先附身一只怪物再下楼</color>");
                return;
            }
            bool allGuardsDefeated = CheckAllFloorGuardsDefeated();
            if (!allGuardsDefeated)
            {
                LastMessage = "楼梯守卫仍在坚守！清除所有守卫才能前进。";
                AddCombatLog("<color=#ff8c00>⚠ 楼梯守卫仍在坚守！</color>");
                return;
            }
            AdvanceFloor();
        }

        DismissTutorial("move");
        TickMonsterAI();
        GameManager.Instance.NotifyPlayerStatsChanged();
        StorySystem.Instance?.CheckHiddenStory("move");
    }

    private bool CheckAllFloorGuardsDefeated()
    {
        // 检查所有标记为楼梯守卫的怪物是否都已被击败（原版：检查所有 _stairGuard 怪物）
        foreach (var action in CurrentFloorState.actions.Values)
        {
            if (action.type == ExploreActionType.Monster && 
                !action.consumed &&
                action.monster != null &&
                action.monster.stairGuard) // 只检查标记为楼梯守卫的怪物
            {
                return false; // 还有守卫存活
            }
        }
        
        return true; // 所有守卫都被击败
    }

    public void ExploreCurrentTile()
    {
        if (CurrentFloorState == null) return;
        var pos = CurrentFloorState.playerPos;

        if (CurrentFloorState.actions.TryGetValue(pos, out var action) && !action.consumed)
        {
            ResolveExploreAction(action, pos);
        }
    }

    private void RevealAround(Vector2Int center, int radius)
    {
        if (CurrentFloorState == null) return;
        for (int dy = -radius; dy <= radius; dy++)
        {
            for (int dx = -radius; dx <= radius; dx++)
            {
                int nx = center.x + dx;
                int ny = center.y + dy;
                if (nx >= 0 && nx < 13 && ny >= 0 && ny < 13)
                {
                    if (Mathf.Abs(dx) + Mathf.Abs(dy) <= radius)
                        CurrentFloorState.discovered[ny, nx] = true;
                }
            }
        }
    }

    private void ResolveExploreAction(ExploreAction action, Vector2Int pos)
    {
        switch (action.type)
        {
            case ExploreActionType.Monster:
                bool combatStarted = StartCombat(action.monster);
                if (combatStarted)
                {
                    action.consumed = true;
                    LastMessage = $"遭遇 {action.monster.name}。";
                }
                break;
            case ExploreActionType.Event:
                action.consumed = true;
                if (action.title != null && action.title.Contains("波次挑战"))
                    StartWaveDefense();
                else
                {
                    CanvasUIManager.Instance?.ShowPickupAnimation("📦", pos);
                    StartCoroutine(DelayedTriggerEvent(action));
                }
                break;
            case ExploreActionType.Fragment:
                action.consumed = true;
                CanvasUIManager.Instance?.ShowPickupAnimation("💎", pos);
                AudioManager.Instance?.PlaySFX("pickup");
                StartCoroutine(DelayedTriggerFragment());
                break;
        }
    }
    
    System.Collections.IEnumerator DelayedTriggerEvent(ExploreAction action)
    {
        yield return new WaitForSeconds(0.6f);
        TriggerSpecialFloorOrEvent(action);
    }
    
    System.Collections.IEnumerator DelayedTriggerFragment()
    {
        yield return new WaitForSeconds(0.6f);
        TriggerFragmentChoice();
    }

    private void TriggerSpecialFloorOrEvent(ExploreAction action)
    {
        var sfType = SpecialFloorSystem.Instance?.GetSpecialFloorType(GameManager.Instance.CurrentFloor)
                     ?? SpecialFloorSystem.SpecialFloorType.None;

        if (action.title != null && sfType != SpecialFloorSystem.SpecialFloorType.None)
        {
            var p = GameManager.Instance.Player;
            switch (sfType)
            {
                case SpecialFloorSystem.SpecialFloorType.Rest:
                    int healAmount = Mathf.FloorToInt(p.maxHp * 0.3f);
                    p.hp = Mathf.Min(p.maxHp, p.hp + healAmount);
                    LastMessage = $"◇ 休息恢复了 {healAmount} HP";
                    return;
                case SpecialFloorSystem.SpecialFloorType.Treasure:
                    int floor = GameManager.Instance.CurrentFloor;
                    int ep = 50 + floor * 10 + UnityEngine.Random.Range(0, 51);
                    p.evolutionPoints += ep;
                    LastMessage = $"◇ 发现宝藏！+{ep}EP";
                    return;
                case SpecialFloorSystem.SpecialFloorType.Trap:
                    int damage = Mathf.FloorToInt(p.maxHp * 0.15f);
                    p.hp = Mathf.Max(1, p.hp - damage);
                    LastMessage = $"⚠️ 触发陷阱！受到 {damage} 点伤害";
                    ScreenEffectsManager.Instance?.FlashDamage();
                    if (p.hp <= 0) { OnPlayerDefeated(); return; }
                    return;
                case SpecialFloorSystem.SpecialFloorType.Mystery:
                    TriggerMysteryRoom();
                    return;
            }
        }

        var floorEventMgr = FloorEventManager.Instance;
        if (floorEventMgr != null && !floorEventMgr.HasActiveEvent())
        {
            var floorEvent = floorEventMgr.TryTriggerEvent(GameManager.Instance.CurrentFloor);
            if (floorEvent != null)
            {
                AddCombatLog($"<color=#ffd700>📦 发现物品：{floorEvent.name}</color>");
                CanvasUIManager.Instance?.ShowFloorEventPanel();
                return;
            }
        }
        else if (floorEventMgr != null && floorEventMgr.HasActiveEvent())
        {
            CanvasUIManager.Instance?.ShowFloorEventPanel();
            return;
        }

        TriggerRandomEvent();
    }

    private void TriggerMysteryRoom()
    {
        var p = GameManager.Instance.Player;
        int rand = UnityEngine.Random.Range(0, 6);
        switch (rand)
        {
            case 0:
                int heal = Mathf.FloorToInt(p.maxHp * 0.25f);
                p.hp = Mathf.Min(p.maxHp, p.hp + heal);
                LastMessage = $"✨ 神秘祝福！恢复 {heal} HP";
                break;
            case 1:
                p.attack += 2; p.defense += 2;
                LastMessage = "⚡ 神秘力量！ATK+2 DEF+2";
                break;
            case 2:
                p.evolutionPoints += 100;
                LastMessage = "✦ 神秘馈赠！+100EP";
                break;
            case 3:
                p.pollution = Mathf.Min(100f, p.pollution + 15f);
                LastMessage = "☢️ 神秘腐蚀！污染+15%";
                break;
            case 4:
                int dmg = Mathf.FloorToInt(p.maxHp * 0.2f);
                p.hp = Mathf.Max(1, p.hp - dmg);
                LastMessage = $"◎ 神秘攻击！受到 {dmg} 点伤害";
                break;
            case 5:
                int bonusEp = UnityEngine.Random.Range(80, 160);
                p.evolutionPoints += bonusEp;
                LastMessage = $"✦ 神秘宝箱！获得 {bonusEp} EP";
                break;
        }
    }

    private void TriggerRandomEvent()
    {
        float roll = UnityEngine.Random.value;
        float trapChance = 0.2f;

        if (roll < trapChance)
        {
            int damage = Mathf.RoundToInt(GameManager.Instance.Player.maxHp * 0.1f);
            GameManager.Instance.Player.hp -= damage;
            GameManager.Instance.Player.hp = Mathf.Max(0, GameManager.Instance.Player.hp);
            LastMessage = $"踩到陷阱！受到 {damage} 点伤害。";
            ScreenEffectsManager.Instance?.FlashDamage();
            if (GameManager.Instance.Player.hp <= 0)
            {
                OnPlayerDefeated();
                return;
            }
        }
        else if (roll < trapChance + 0.15f)
        {
            int bonusEp = 10 + GameManager.Instance.CurrentFloor * 3;
            GameManager.Instance.Player.evolutionPoints += bonusEp;
            LastMessage = $"发现进化能量！获得 {bonusEp} EP。";
        }
        else if (roll < trapChance + 0.25f)
        {
            int heal = Mathf.RoundToInt(GameManager.Instance.Player.maxHp * 0.15f);
            GameManager.Instance.Player.hp = Mathf.Min(GameManager.Instance.Player.maxHp, GameManager.Instance.Player.hp + heal);
            LastMessage = $"找到治疗者！恢复 {heal} HP。";
        }
        else
        {
            LastMessage = "这一带什么都没有...";
        }

        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public bool StartCombat(MonsterRuntime monster)
    {
        if (monster == null) return false;

        // 新手引导：关卡1必须按顺序进行
        if (ForceTutorial && GameManager.Instance.CurrentFloor == 1)
        {
            if (TutorialStage == 0 && monster.id != "rat" && GameManager.Instance.Player.totalKillsThisRun == 0)
            {
                // 还没攻击过虚弱鼠，先提醒玩家
                LastMessage = "先去攻击那只虚弱实验鼠！了解战斗基础操作";
                AddCombatLog("<color=#ff8c00>⚠ 请先攻击前方的虚弱实验鼠，了解战斗基础操作</color>");
                return false;
            }
            if (TutorialStage == 0 && monster.id != "dog" && GameManager.Instance.Player.totalKillsThisRun > 0)
            {
                // 已攻击过，现在应该去附身看门犬
                LastMessage = "去附身那只看门犬！学习核心机制";
                AddCombatLog("<color=#ff8c00>⚠ 请去附身看门犬，学习核心附身机制</color>");
                return false;
            }
        }

        CombatLog.Clear();
        LastCombatMessage = "";
        CurrentEnemy = monster;
        AudioManager.Instance?.PlaySFX(monster.isBoss ? "boss" : "alert");
        MetaProgressSystem.Instance?.OnMonsterEncounter(monster.id);
        CurrentScreen = RunScreen.Combat;
        AudioManager.Instance?.DuckBGM(0.3f);
        GameManager.Instance.ResetCombatCounters();
        GameManager.Instance.ChangeState(GameManager.GameState.Combat);

        EventBus.Emit(EventTypes.CombatStarted);
        CorruptionSkillManager.Instance?.OnCombatStart();

        // 进入战斗时强制关闭 move 教程
        if (_currentTutStepId == "move") DismissTutorial("move");
        
        // 看门犬战斗时不显示攻击教程，直接显示附身引导
        if (!(ForceTutorial && GameManager.Instance.CurrentFloor == 1 && 
              TutorialStage == 0 && GameManager.Instance.Player.totalKillsThisRun > 0 && monster.id == "dog"))
        {
            CheckTutorial("combat");
        }
        
        // 重置战斗计数
        _combatRound = 0;
        _attackRounds = 0;
        _comboCount = 0;
        _comboTimer = 0f;

        // 终极技能CD递减
        if (_ultCooldown > 0) _ultCooldown--;

        AddCombatLog($"⚔️ 遭遇 {monster.name}！(HP:{monster.hp} ATK:{monster.atk} DEF:{monster.def})");
        if (UltReady) CheckTutorial("ultReady");
        
        // 伏击：怪物先手攻击
        if (monster._ambush)
        {
            AddCombatLog($"<color=#ff006e>【伏击！】敌人突袭 — 第1回合你无法反击！</color>");
            _combatRound++;
            // 怪物先手攻击
            EnemyTurn();
        }
        
        // 攻击提示只在第一次战斗时显示（玩家还没有击杀记录时）
        if (GameManager.Instance.Player.totalKillsThisRun > 0)
        {
            MarkSoftHintShown("hint_attack"); // 标记攻击提示已显示，避免重复
        }
        
        // 看门犬战斗时强制显示附身引导
        if (ForceTutorial && GameManager.Instance.CurrentFloor == 1 && 
            TutorialStage == 0 && GameManager.Instance.Player.totalKillsThisRun > 0 && monster.id == "dog")
        {
            ActiveSoftHint = "尝试「附身」获取怪物能力！";
            ActiveSoftHintTarget = "possess";
            _softHintTime = Time.time;
            MarkSoftHintShown("hint_possess");
        }
        else
        {
            CheckSoftHint();
        }
        return true;
    }

    public void PlayerAttack()
    {
        // 如果不在战斗中但附近有和平区怪物，可以主动攻击
        if (CurrentEnemy == null && CurrentScreen != RunScreen.Combat && _peacefulMonsterPos.HasValue)
        {
            if (CurrentFloorState.actions.TryGetValue(_peacefulMonsterPos.Value, out var action) 
                && action.type == ExploreActionType.Monster && action.monster != null)
            {
                StartCombat(action.monster);
                _peacefulMonsterPos = null;
                return;
            }
        }
        
        if (CurrentEnemy == null || CurrentScreen != RunScreen.Combat) return;

        // 检查是否是看门犬且在教程阶段（只有当玩家还不是看门犬形态时才触发）
        // 只有当看门犬已被攻击过（HP <= 50%）时才提示去附身
        bool isTutorialDog = CurrentEnemy != null && CurrentEnemy.id == "dog" && GameManager.Instance.CurrentFloor == 1 && 
                             GameManager.Instance.Player.currentFormId != "dog" && CurrentEnemy.hp <= CurrentEnemy.maxHp * 0.5f;
        
        if (isTutorialDog)
        {
            AddCombatLog("<color=#ff0>看门犬已被削弱！尝试附身它！</color>");
            return; // 不执行攻击，提示去附身
        }

        _combatRound++;
        _attackRounds++;
        _comboTimer = 2f;
        _comboCount++;

        if (_comboCount == 3 || _comboCount == 5 || _comboCount == 8 || _comboCount == 12 || _comboCount == 20)
        {
            AudioManager.Instance?.PlaySFX("comboUp");
        }

        TutorialManager.Instance?.RecordAction(TutorialAction.Attack);
        TutorialManager.Instance?.RecordCombo(_comboCount);
        DismissSoftHint("attack");

        EventBus.Emit(EventTypes.TurnStart);
        EventBus.Emit(EventTypes.PlayerAttack);

        var player = GameManager.Instance.Player;
        var dmgTags = new System.Collections.Generic.List<string>();

        // 原项目公式：calcPlayerDamage逻辑
        float baseAtk = player.attack + player.attackBonus;

        // 血族被动：血怒（HP<30%时+50%攻击）
        if (player.playerClass == "blood" && player.hp < player.maxHp * 0.3f)
        {
            baseAtk = Mathf.RoundToInt(baseAtk * 1.5f);
            dmgTags.Add("<color=#cc4444>♦血怒</color>");
        }
        
        float finalAtk = baseAtk * FloorSignatureAtkMult;
        // JS original: 护甲特性前3回合 def×2
        int effectiveDef = CurrentEnemy.def;
        if (HasTrait(CurrentEnemy, "护甲") && _combatRound <= 3)
            effectiveDef *= 2;
        int rawDamage = Mathf.Max(1, Mathf.RoundToInt(finalAtk - effectiveDef));

        // 连击加成
        float comboBonus = GetComboBonus();
        int damage = comboBonus > 0 ? Mathf.RoundToInt(rawDamage * (1 + comboBonus)) : rawDamage;
        if (comboBonus > 0) dmgTags.Add($"<color=#ffcc00>连击+{Mathf.RoundToInt(comboBonus*100)}%</color>");

        // 赌徒天堂：伤害 0.5x~2x 随机
        if (ActiveSignature != null && ActiveSignature.randomDmg)
        {
            float rMult = 0.5f + UnityEngine.Random.value * 1.5f;
            damage = Mathf.Max(1, Mathf.RoundToInt(damage * rMult));
        }

        // 脆弱结界：所有伤害×2
        if (ActiveSignature != null && ActiveSignature.dmgMult > 0)
            damage = Mathf.RoundToInt(damage * ActiveSignature.dmgMult);

        // 暴击场：30% 暴击率，暴击伤害×2
        if (ActiveSignature != null && ActiveSignature.critBonus > 0 && UnityEngine.Random.value < ActiveSignature.critBonus)
        {
            damage *= 2;
            AddCombatLog("※ 暴击！");
            AudioManager.Instance?.PlaySFX("crit", 1.2f);
        }

        // 持久战加成：攻击回合超过6回合后，每回合增加怪物最大HP 8%的额外伤害
        if (_attackRounds > 6)
        {
            int overRounds = _attackRounds - 6;
            int bonusDmg = Mathf.RoundToInt(CurrentEnemy.maxHp * 0.08f * overRounds);
            damage += bonusDmg;
            if (bonusDmg > 0) AddCombatLog($"持久战加成 +{bonusDmg}");
        }

        // 铁壁：10% 完全格挡
        bool ironBlock = false;
        if (damage > 0 && HasTrait(CurrentEnemy, "铁壁") && UnityEngine.Random.value < 0.1f)
        {
            ironBlock = true;
            AddCombatLog($"◆ {CurrentEnemy.name} 铁壁！完全格挡 -{damage}");
            damage = 0;
        }

        // 职业被动加成
        ClassAbilityData.ApplyPassiveOnAttack(player, ref damage, false);
        // 特质钩子加成
        damage = TraitEffects.ApplyOnPlayerAttack(player.traits?.ToArray(), damage);
        // 构筑轴线协同
        float synergyBonus = BuildAxesData.GetSynergyBonus(CurrentEnemy.axes, player.selectedClass);
        if (synergyBonus > 0)
        {
            damage = Mathf.RoundToInt(damage * (1f + synergyBonus));
            dmgTags.Add($"<color=#88ddff>轴线+{Mathf.RoundToInt(synergyBonus*100)}%</color>");
        }

        // 遗产Boss伤害加成
        if (CurrentEnemy.isBoss)
        {
            float bossBonusLegacy = LegacyManager.Instance?.GetActiveLegacyEffectValue("boss_damage_bonus") ?? 0;
            if (bossBonusLegacy > 0)
            {
                damage = Mathf.RoundToInt(damage * (1f + bossBonusLegacy));
                dmgTags.Add($"<color=#ffcc00>★Boss+{Mathf.RoundToInt(bossBonusLegacy*100)}%</color>");
            }
        }

        // 共鸣效果：攻击加成
        float resonanceAtkBoost = FormResonanceSystem.Instance?.GetActiveEffectValue("atk_boost") ?? 0;
        if (resonanceAtkBoost > 0)
        {
            damage = Mathf.RoundToInt(damage * (1f + resonanceAtkBoost));
            dmgTags.Add($"<color=#00ccff>◈共鸣+{Mathf.RoundToInt(resonanceAtkBoost*100)}%</color>");
        }
        // 共鸣效果：无视防御
        float resonanceIgnoreDef = FormResonanceSystem.Instance?.GetActiveEffectValue("ignore_def") ?? 0;
        if (resonanceIgnoreDef > 0)
        {
            int defIgnored = Mathf.RoundToInt(CurrentEnemy.def * resonanceIgnoreDef);
            damage += defIgnored;
        }
        // 共鸣效果：暴击加成
        float resonanceCritBoost = FormResonanceSystem.Instance?.GetActiveEffectValue("crit_boost") ?? 0;
        if (resonanceCritBoost > 0 && UnityEngine.Random.value < resonanceCritBoost)
        {
            damage *= 2;
            AddCombatLog("<color=#ffcc00>※ 共鸣暴击！</color>");
        }

        // 遗产暴击
        if (player.critRate > 0 && UnityEngine.Random.value < player.critRate)
        {
            damage *= 2;
            dmgTags.Add("<color=#ffcc00>★暴击</color>");
        }

        // 共鸣效果：二连击
        float resonanceDoubleAtk = FormResonanceSystem.Instance?.GetActiveEffectValue("double_attack") ?? 0;
        if (resonanceDoubleAtk > 0)
        {
            int bonusDmg = Mathf.RoundToInt(damage * resonanceDoubleAtk);
            damage += bonusDmg;
            AddCombatLog($"<color=#ffaa44>◈ 二连击 +{bonusDmg}</color>");
        }

        // Soft cap：总倍率超过2.0时超出部分×0.6
        if (rawDamage > 0 && damage > rawDamage * 2)
        {
            damage = Mathf.RoundToInt(rawDamage * 2f + (damage - rawDamage * 2f) * 0.6f);
        }

        // 挑战词条：玩家伤害倍率
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.playerDmgDealtMult > 0f)
            damage = Mathf.RoundToInt(damage * ActiveChallengeModifier.playerDmgDealtMult);

        CurrentEnemy.hp -= damage;
        float hitVol = Mathf.Clamp01((float)damage / Mathf.Max(1, CurrentEnemy.maxHp)) * 0.5f + 0.5f;
        AudioManager.Instance?.PlaySFX("hit", hitVol);
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.thorns && damage > 0)
        {
            int thornsDmg = Mathf.Max(1, Mathf.RoundToInt(damage * 0.2f));
            player.hp = Mathf.Max(1, player.hp - thornsDmg);
            AddCombatLog($"<color=#88aa44>🌿 荆棘反伤 -{thornsDmg}HP</color>");
        }

        // 共鸣效果：毒DOT
        float resonancePoisonDot = FormResonanceSystem.Instance?.GetActiveEffectValue("poison_dot") ?? 0;
        if (resonancePoisonDot > 0 && CurrentEnemy.hp > 0)
        {
            int poisonDmg = Mathf.CeilToInt(CurrentEnemy.maxHp * resonancePoisonDot);
            CurrentEnemy.hp -= poisonDmg;
            AddCombatLog($"<color=#aa44ff>◈ 毒蚀 -{poisonDmg}HP</color>");
        }

        // 遗产中毒
        if (player.poisonChance > 0 && CurrentEnemy.hp > 0 && UnityEngine.Random.value < player.poisonChance)
        {
            int poisonDmg = Mathf.Max(1, Mathf.CeilToInt(CurrentEnemy.maxHp * 0.05f));
            CurrentEnemy.hp -= poisonDmg;
            AddCombatLog($"<color=#aa44ff>● 遗产毒素 -{poisonDmg}HP</color>");
        }

        // 特效：伤害飘字 + 暴击闪烁 + 屏幕震动
        if (damage > 0)
        {
            DamageNumberPool.Instance?.SpawnDamage(damage, false);
            ScreenEffectsManager.Instance?.Shake(0.05f, 0.08f);
        }

        // 统计伤害
        player.totalDamageDealt += damage;

        // 吸血回复
        if (damage > 0 && player.lifesteal > 0)
        {
            float lsMult = player.lifesteal;
            // Blood Lv3+: 高污染时吸血×1.5
            if (player.selectedClass == "blood" && EvolutionLevel >= 3 && player.pollution >= 60f)
                lsMult *= 1.5f;
            // 共鸣效果：满吸血
            float resonanceLS = FormResonanceSystem.Instance?.GetActiveEffectValue("lifesteal_full") ?? 0;
            if (resonanceLS > 0) lsMult += resonanceLS;

            int lsHeal = Mathf.Max(1, Mathf.CeilToInt(damage * lsMult));
            if (ActiveChallengeModifier != null && ActiveChallengeModifier.healMult > 0f)
                lsHeal = Mathf.Max(1, Mathf.RoundToInt(lsHeal * ActiveChallengeModifier.healMult));
            player.hp = Mathf.Min(player.hp + lsHeal, player.maxHp);
            AddCombatLog($"<color=#88ff88>♥ 吸血 +{lsHeal}HP</color>");
        }
        else if (damage > 0)
        {
            // 无吸血属性但有共鸣全吸血
            float resonanceLS = FormResonanceSystem.Instance?.GetActiveEffectValue("lifesteal_full") ?? 0;
            if (resonanceLS > 0)
            {
                int lsHeal = Mathf.CeilToInt(damage * resonanceLS);
                player.hp = Mathf.Min(player.hp + lsHeal, player.maxHp);
                AddCombatLog($"<color=#88ff88>◈ 寄生吸取 +{lsHeal}HP</color>");
            }
        }

        // 构建回合日志
        string roundLog = $"R{_combatRound} 你→<color=#4a4>{damage}</color>";
        if (comboBonus > 0) roundLog += $" <color=#ff0>(连击+{Mathf.RoundToInt(comboBonus * 100)}%)</color>";
        if (!ironBlock && damage > 0) roundLog += $" 对 {CurrentEnemy.name} 造成 {damage} 点伤害";
        if (dmgTags.Count > 0) roundLog += " " + string.Join(" ", dmgTags);

        AddCombatLog(roundLog);

        // 附身窗口 — 破防（HP首次低于50%）
        if (CurrentEnemy.hp > 0 && !CurrentEnemy._possessWindowUsed && !CurrentEnemy._breakWindowDone && CurrentEnemy.hp <= CurrentEnemy.maxHp * 0.5f)
        {
            CurrentEnemy._breakWindowDone = true;
            AddCombatLog($"◈ <color=#c6f>{CurrentEnemy.name} 意识裂隙 — 附身窗口开启!</color>");
            
            Debug.Log($"[Tutorial] Break window: TutorialStage={TutorialStage}, ForceTutorial={ForceTutorial}, Enemy={CurrentEnemy.id}");
            
            // 教程阶段：怪物破防时显示提示，但不推进阶段（F1只显示攻击按钮）
            if (TutorialStage == 0 && ForceTutorial)
            {
                Debug.Log("[Tutorial] Break window on F1 - keeping stage 0, attack-only mode");
            }
        }

        // 教程阶段：看门犬不能被杀死，必须附身
        if (ForceTutorial && GameManager.Instance.CurrentFloor == 1 && TutorialStage == 0 && GameManager.Instance.Player.totalKillsThisRun > 0 && CurrentEnemy.id == "dog" && CurrentEnemy.hp <= 0)
        {
            CurrentEnemy.hp = 1;
            AddCombatLog("<color=#ff8c00>⚠ 看门犬必须被附身，不能杀死！使用附身按钮</color>");
            LastMessage = "看门犬必须被附身！使用附身按钮";
            return;
        }

        // Boss阶段转换检查
        if (CurrentEnemy.hp > 0 && BossAIManager.Instance != null && BossAIManager.Instance.IsActiveBoss())
        {
            BossAIManager.Instance.CheckPhaseTransition(CurrentEnemy);
        }

        if (CurrentEnemy.hp <= 0)
        {
            CurrentEnemy.hp = 0;
            player.monstersKilled++;
            player.totalKillsThisRun++;
            OnEnemyDefeated(false);
            return;
        }

        EnemyTurn();
    }

    public void ApplyDamageToEnemy(int damage)
    {
        if (CurrentEnemy == null || CurrentScreen != RunScreen.Combat) return;
        CurrentEnemy.hp -= damage;
        AddCombatLog($"☢ 造成 {damage} 点污染伤害");

        if (damage > _maxDamageThisRun)
        {
            _maxDamageThisRun = damage;
            _maxDamageComboThisRun = "污染共鸣";
        }

        // Boss阶段转换
        if (CurrentEnemy.hp > 0 && BossAIManager.Instance != null && BossAIManager.Instance.IsActiveBoss())
        {
            BossAIManager.Instance.CheckPhaseTransition(CurrentEnemy);
        }

        if (CurrentEnemy.hp <= 0)
        {
            CurrentEnemy.hp = 0;
            var player = GameManager.Instance?.Player;
            if (player != null)
            {
                player.monstersKilled++;
                player.totalKillsThisRun++;
            }
            OnEnemyDefeated(false);
        }
    }

    private float GetComboBonus()
    {
        // JS original: 6-tier combo {1:0, 3:0.05, 5:0.10, 8:0.15, 12:0.25, 20:0.35}
        float baseBonus;
        if (_comboCount >= 20) baseBonus = 0.35f;
        else if (_comboCount >= 12) baseBonus = 0.25f;
        else if (_comboCount >= 8) baseBonus = 0.15f;
        else if (_comboCount >= 5) baseBonus = 0.10f;
        else if (_comboCount >= 3) baseBonus = 0.05f;
        else return 0f;
        // JS original: pollution multiplier (pol>=70: ×1.5, pol>=50: ×1.25)
        float pol = GameManager.Instance.Player.pollution;
        float polMult = pol >= 70 ? 1.5f : pol >= 50 ? 1.25f : 1f;
        return baseBonus * polMult;
    }

    private bool HasTrait(MonsterRuntime monster, string traitName)
    {
        return monster.traits != null && System.Array.Exists(monster.traits, t => t == traitName);
    }

    public void PlayerDefend()
    {
        if (CurrentScreen != RunScreen.Combat || CurrentEnemy == null) return;
        AudioManager.Instance?.PlaySFX("defend");
        var player = GameManager.Instance.Player;
        _combatRound++;
        player.defendCountThisCombat++;
        player.defendedLastTurn = true;

        EventBus.Emit(EventTypes.TurnStart);
        EventBus.Emit(EventTypes.PlayerDefend);

        AchievementManager.Instance?.UpdateProgress("defend10", player.defendCountThisCombat);

        // 原版防御回复：前3次8%，第4次5%，第5次3%，第6次+1%
        float healRate = 0.08f;
        int dc = player.defendCountThisCombat;
        if (dc > 3) healRate = Mathf.Max(0.01f, 0.08f - (dc - 3) * 0.025f);
        int heal = Mathf.FloorToInt(player.maxHp * healRate);
        player.hp = Mathf.Min(player.maxHp, player.hp + heal);

        AddCombatLog($"◆ 防御！回复{heal}HP ({Mathf.RoundToInt(healRate * 100)}%)");
        DamageNumberPool.Instance?.SpawnHeal(heal);
        TutorialManager.Instance?.RecordAction(TutorialAction.Defend);
        CheckTutorial("defendHint");
        DismissTutorial("defend");
        DismissSoftHint("defend");

        // 重置连击
        if (_comboCount > 0)
        {
            AudioManager.Instance?.PlaySFX("comboBreak");
        }
        _comboCount = 0;
        _attackRounds = 0;

        EnemyTurn(true);
    }

    public void PlayerFlee()
    {
        if (CurrentScreen != RunScreen.Combat || CurrentEnemy == null) return;
        DismissSoftHint("flee");
        if (CurrentEnemy.isBoss)
        {
            AddCombatLog("Boss战无法逃跑！");
            AudioManager.Instance?.PlaySFX("error");
            return;
        }
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.noFlee)
        {
            AddCombatLog("<color=#ff4444>🚫 挑战词条：禁止逃跑！</color>");
            AudioManager.Instance?.PlaySFX("error");
            return;
        }
        int fleeCost = Mathf.Max(5, Mathf.FloorToInt(GameManager.Instance.Player.maxHp * 0.05f));
        CanvasUIManager.Instance?.ShowFleeConfirm(fleeCost);
    }

    public void ExecuteFlee()
    {
        if (CurrentScreen != RunScreen.Combat || CurrentEnemy == null) return;
        AudioManager.Instance?.PlaySFX("evade");

        int fleeCost = Mathf.Max(5, Mathf.FloorToInt(GameManager.Instance.Player.maxHp * 0.05f));
        var player = GameManager.Instance.Player;

        // 恢复怪物到地图上（逃跑不消灭怪物）
        Vector2Int monsterPos = Vector2Int.zero;
        bool restoredMonster = false;
        if (CurrentFloorState != null)
        {
            monsterPos = CurrentFloorState.playerPos;
            if (CurrentFloorState.actions.TryGetValue(monsterPos, out var action)
                && action.type == ExploreActionType.Monster
                && action.monster == CurrentEnemy)
            {
                action.consumed = false;
                restoredMonster = true;
            }
        }

        // 逃跑保护 — 免除本次战斗的污染后果
        _fleePollProtect = true;

        // 血量不够 → 当前形态死亡
        if (player.hp <= fleeCost)
        {
            player.hp = 0;
            AddCombatLog($"逃跑失败 — 体力耗尽，当前形态崩溃！");
            MarkCurrentFormAsDead(player);
            EndCombat(false);
            // 移到安全位置
            if (restoredMonster) MovePlayerAwayFrom(monsterPos, 2);
            HandleFormDeathAfterFlee(player);
            return;
        }

        player.hp -= fleeCost;

        // 逃跑补偿 — 基于战斗回合给予少量EP
        int roundsEP = Mathf.FloorToInt(_combatRound * 3);
        string fleeBonus = "";
        if (roundsEP > 0)
        {
            player.evolutionPoints += roundsEP;
            fleeBonus = $" 回合补偿+{roundsEP}EP";
        }

        AddCombatLog($"逃跑成功 HP-{fleeCost}{fleeBonus}");
        EndCombat(false);

        // 逃跑后远离怪物2格
        if (restoredMonster) MovePlayerAwayFrom(monsterPos, 2);
    }

    private void HandleFormDeathAfterFlee(GameManager.PlayerData player)
    {
        bool hasAliveBackup = false;
        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            if (player.ownedForms[i] == player.currentFormId) continue;
            if (player.ownedForms[i] == "human") continue;
            bool isDead = i < player.deadForms.Count && player.deadForms[i];
            if (!isDead) { hasAliveBackup = true; break; }
        }

        if (hasAliveBackup)
        {
            ShowingDeathFormSelect = true;
        }
        else if (player.currentFormId != "human")
        {
            // 回退到本命英雄
            LoadForm("human");
            player.currentFormId = "human";
            // 恢复本命英雄HP
            if (player.formHpMap.TryGetValue("human", out int humanHp))
                player.hp = Mathf.Clamp(humanHp, 1, player.maxHp);
            else
                player.hp = player.maxHp;
            AddCombatLog("意识回归本命英雄...");
            GameManager.Instance.NotifyPlayerStatsChanged();
        }
        else
        {
            // 本命也死了 → GameOver
            CurrentScreen = RunScreen.GameOver;
            GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
            TriggerDeathReport();
        }
    }

    private void MovePlayerAwayFrom(Vector2Int dangerPos, int dist)
    {
        if (CurrentFloorState == null) return;
        Vector2Int best = CurrentFloorState.playerPos;
        int bestDist = 0;
        for (int dy = -dist; dy <= dist; dy++)
        {
            for (int dx = -dist; dx <= dist; dx++)
            {
                int nx = dangerPos.x + dx, ny = dangerPos.y + dy;
                if (nx < 1 || nx > 11 || ny < 1 || ny > 11) continue;
                if (!CurrentFloorState.walkable[ny, nx]) continue;
                var candidate = new Vector2Int(nx, ny);
                if (CurrentFloorState.actions.TryGetValue(candidate, out var act) && !act.consumed) continue;
                int d = Mathf.Abs(dx) + Mathf.Abs(dy);
                if (d >= 2 && d > bestDist)
                {
                    bestDist = d;
                    best = candidate;
                }
            }
        }
        CurrentFloorState.playerPos = best;
    }

    public void CancelFlee()
    {
        // 取消逃跑，继续战斗
        CanvasUIManager.Instance?.HideFleeConfirm();
    }

    public void PlayerSwitchForm(string formId)
    {
        if (CurrentScreen != RunScreen.Combat && CurrentScreen != RunScreen.Exploration) return;
        var player = GameManager.Instance.Player;

        if (formId == player.currentFormId) return;
        if (!player.ownedForms.Contains(formId)) return;

        int idx = player.ownedForms.IndexOf(formId);
        if (idx >= 0 && idx < player.deadForms.Count && player.deadForms[idx]) return;

        // 保存当前形态HP和maxHp到formHpMap
        player.formHpMap[player.currentFormId] = player.hp;
        player.formMaxHpMap[player.currentFormId] = player.maxHp;

        // 形态共鸣
        string oldFormForResonance = player.currentFormId;

        // 使用LoadForm计算属性
        bool loadSuccess = LoadForm(formId);
        if (!loadSuccess)
        {
            AddCombatLog($"<color=#ff4444>无法加载形态 {formId}！</color>");
            return;
        }

        player.currentFormStartTime = Time.time;
        player.switchCountThisCombat++;
        AchievementManager.Instance?.UpdateProgress("switch3", player.switchCountThisCombat);

        FormResonanceSystem.Instance?.OnFormSwitch(oldFormForResonance, formId);
        player.switchShieldTurns = FormResonanceSystem.Instance?.HasActiveEffect("shield") == true ? 1 : 0;

        var activeEffects = FormResonanceSystem.Instance?.GetAllActiveEffects();
        if (activeEffects != null && activeEffects.Count > 0 && !IsSoftHintShown("hint_resonance"))
        {
            MarkSoftHintShown("hint_resonance");
            AddCombatLog("<color=#00ccff>◈ 共鸣 = 不同形态组合的被动效果。多切换形态提升熟练度！</color>");
        }

        // 熟练度加成（按比例增加HP）
        float prof = FormResonanceSystem.Instance?.GetProficiencyBonus(formId) ?? 0;
        if (prof > 0)
        {
            player.attack = Mathf.CeilToInt(player.attack * (1f + prof));
            player.defense = Mathf.CeilToInt(player.defense * (1f + prof));
            player.maxHp = Mathf.CeilToInt(player.maxHp * (1f + prof));
            player.hp = Mathf.RoundToInt(player.hp * (1f + prof));
        }

        // 共鸣加成（按比例增加HP）
        ApplyResonanceBonuses(formId);

        // 恢复目标形态的独立HP（在所有加成应用之后）
        if (player.formHpMap.TryGetValue(formId, out int savedHp))
        {
            // 按比例恢复HP，保持血量百分比不变
            if (player.formMaxHpMap.TryGetValue(formId, out int savedMaxHp) && savedMaxHp > 0)
            {
                float hpRatio = (float)savedHp / savedMaxHp;
                int restoredHp = Mathf.RoundToInt(player.maxHp * hpRatio);
                player.hp = Mathf.Clamp(restoredHp, 1, player.maxHp);
            }
            else
            {
                player.hp = Mathf.Clamp(savedHp, 1, player.maxHp);
            }
        }
        else
        {
            player.hp = player.maxHp;
        }

        AddCombatLog($"切换→{GetDisplayName(formId)} HP:{player.hp}/{player.maxHp} ATK:{player.attack} DEF:{player.defense}");
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    private void ApplyResonanceBonuses(string formId)
    {
        var player = GameManager.Instance.Player;
        int level = GetFormResonanceLevel(formId);

        // Bond level bonuses applied on top of LoadForm stats
        if (level >= 1) // 熟悉: HP+10%
        {
            player.maxHp = Mathf.RoundToInt(player.maxHp * 1.1f);
            player.hp = Mathf.RoundToInt(player.hp * 1.1f);
        }
        if (level >= 2) // 挚友: class passive
        {
            switch (player.selectedClass)
            {
                case "titan": player.defense += 3; break;
                case "ghost": player.attack += 3; break;
                case "swarm": player.maxHp += 20; player.hp += 20; break;
                case "blood": player.attack += 2; break;
                case "mech": player.defense += 2; break;
            }
        }
        if (level >= 3) // 共生: all +5%
        {
            player.maxHp = Mathf.RoundToInt(player.maxHp * 1.05f);
            player.hp = Mathf.RoundToInt(player.hp * 1.05f);
            player.attack = Mathf.RoundToInt(player.attack * 1.05f);
            player.defense = Mathf.RoundToInt(player.defense * 1.05f);
        }
        player.hp = Mathf.Clamp(player.hp, 1, player.maxHp);
    }
    
    private void SwitchToPossessedForm(string formId)
    {
        var player = GameManager.Instance.Player;
        string oldFormId = player.currentFormId;

        // 保存旧形态HP和maxHp
        player.formHpMap[oldFormId] = player.hp;
        player.formMaxHpMap[oldFormId] = player.maxHp;

        if (!player.ownedForms.Contains(oldFormId))
            player.ownedForms.Add(oldFormId);

        if (!player.ownedForms.Contains(formId))
        {
            // 计算实际可用卡槽数（总卡槽 - 已死卡槽）
            int deadCount = 0;
            for (int i = 0; i < player.deadForms.Count && i < player.ownedForms.Count; i++)
                if (player.deadForms[i]) deadCount++;
            int aliveSlots = player.ownedForms.Count - deadCount;

            if (aliveSlots < player.formSlots - deadCount)
            {
                // 还有空闲的活卡槽位，直接添加
                player.ownedForms.Add(formId);
                while (player.deadForms.Count < player.ownedForms.Count)
                    player.deadForms.Add(false);
                CheckTutorial("formUnlock");
            }
            else
            {
                // 所有活卡槽都满了，替换当前形态所在卡槽
                int currentIdx = player.ownedForms.IndexOf(oldFormId);
                if (currentIdx >= 0)
                {
                    player.ownedForms[currentIdx] = formId;
                }
                else
                {
                    // fallback：替换第一个活着的非human非当前
                    for (int i = 0; i < player.ownedForms.Count; i++)
                    {
                        bool isDead = i < player.deadForms.Count && player.deadForms[i];
                        if (isDead) continue;
                        if (player.ownedForms[i] == "human") continue;
                        if (player.ownedForms[i] == oldFormId) continue;
                        player.ownedForms[i] = formId;
                        break;
                    }
                }
            }
        }

        // Track longest form
        float dur = Time.time - player.currentFormStartTime;
        if (dur > player.longestFormDuration)
        {
            player.longestFormDuration = dur;
            player.longestFormId = oldFormId;
        }
        player.currentFormStartTime = Time.time;

        // Apply new form stats (base + weighted)
        LoadForm(formId);

        // 熟练度加成（按比例增加HP）
        float prof = FormResonanceSystem.Instance?.GetProficiencyBonus(formId) ?? 0;
        if (prof > 0)
        {
            player.attack = Mathf.CeilToInt(player.attack * (1f + prof));
            player.defense = Mathf.CeilToInt(player.defense * (1f + prof));
            player.maxHp = Mathf.CeilToInt(player.maxHp * (1f + prof));
            player.hp = Mathf.RoundToInt(player.hp * (1f + prof));
        }

        // 共鸣加成（按比例增加HP）
        ApplyResonanceBonuses(formId);

        // 附身新形态时设置为满血
        player.hp = player.maxHp;

        // Cinematic
        PossessionCinematic.Instance?.Play(oldFormId, formId, GetDisplayName(formId), player.maxHp, player.attack, player.defense);
        ScreenEffectsManager.Instance?.FlashPossess();


        AddCombatLog($"附身→{GetDisplayName(formId)} HP:{player.hp}/{player.maxHp} ATK:{player.attack} DEF:{player.defense}");
        GameManager.Instance.NotifyPlayerStatsChanged();
    }
    
    private void ApplyPossessionPollution(string type)
    {
        // 原版公式：base=10 + 策略修正，再乘楼层曲线倍率
        float basePollution = 10f;
        float extraPollution = 0f;

        switch (type)
        {
            case "intimidate": extraPollution = 0f; break;   // 威压: 10+0=10
            case "trade": extraPollution = 10f; break;       // 交易: 10+10=20
            case "resonance": extraPollution = -5f; break;   // 共鸣: 10-5=5
            case "join": extraPollution = 15f; break;        // 加入: 10+15=25
        }

        float polAdd = basePollution + extraPollution;

        // 楼层曲线倍率
        var curveParams = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, GameManager.Instance.CurrentFloor, GameManager.Instance.CurrentStage);
        polAdd = Mathf.Round(polAdd * curveParams.possessPollMult);

        // 进化减免（虫群Lv3: -2）
        var player = GameManager.Instance.Player;
        if (player.selectedClass == "swarm" && EvolutionLevel >= 3)
            polAdd = Mathf.Max(0, polAdd - 2);

        PollutionSystem.Instance.GainPollution(polAdd);
        AddCombatLog($"☢ 污染 +{polAdd:F0} ({type})");
    }

    private bool LoadForm(string formId)
    {
        var player = GameManager.Instance.Player;
        player.currentFormId = formId;

        if (formId == "human")
        {
            var cls = System.Array.Find(OriginalGameData.ClassBaseStats, s => s.id == player.selectedClass);
            if (cls != null)
            {
                player.baseMaxHp = cls.maxHp;
                player.baseAttack = cls.atk;
                player.baseDefense = cls.def;
            }
            player.maxHp = player.baseMaxHp;
            player.attack = player.baseAttack;
            player.defense = player.baseDefense;
            player.hp = player.maxHp;
            player.traits.Clear();
            return true;
        }

        var monsterDef = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == formId);
        if (!string.IsNullOrEmpty(monsterDef.id))
        {
            player.baseMaxHp = monsterDef.hp;
            player.baseAttack = monsterDef.atk;
            player.baseDefense = monsterDef.def;
            player.maxHp = player.baseMaxHp + Mathf.RoundToInt(monsterDef.hp * 0.85f);
            player.attack = player.baseAttack + Mathf.RoundToInt(monsterDef.atk * 0.5f);
            player.defense = player.baseDefense + Mathf.RoundToInt(monsterDef.def * 0.65f);
            player.hp = player.maxHp;
            player.traits.Clear();
            if (monsterDef.traits != null)
                player.traits.AddRange(monsterDef.traits);
            if (player.traits.Count > 0)
                AudioManager.Instance?.PlaySFX("trait");
            return true;
        }

        return false;
    }

    public void ToggleAutoCombat()
    {
        AutoCombatEnabled = !AutoCombatEnabled;
        AddCombatLog(AutoCombatEnabled ? "⚙️ 自动战斗开启" : "⚙️ 自动战斗关闭");
    }

    private void EnemyTurn(bool playerDefending = false)
    {
        if (CurrentEnemy == null) return;

        var player = GameManager.Instance.Player;

        // 滚动本回合敌人意图
        if (CurrentEnemy._intent == null)
        {
            CurrentEnemy._intent = RollMonsterIntent(CurrentEnemy);
        }
        var currentIntent = CurrentEnemy._intent;

        bool intentSkipAtk = false;
        string intentLog = "";

        // 处理怪物意图
        if (currentIntent != null)
        {
            switch (currentIntent.type)
            {
                case MonsterIntentType.buff:
                    float buffAmt = currentIntent.buffAmt > 0 ? currentIntent.buffAmt : 0.2f;
                    CurrentEnemy._intentAtkBuff += buffAmt;
                    intentLog = $"▲ <color=#ff8800>{CurrentEnemy.name} 强化了自身!(ATK+{Mathf.RoundToInt(buffAmt * 100)}%)</color>";
                    intentSkipAtk = true;
                    break;
                case MonsterIntentType.heal:
                    int healAmt = currentIntent.healAmt > 0 ? Mathf.RoundToInt(currentIntent.healAmt) : Mathf.RoundToInt(CurrentEnemy.maxHp * 0.1f);
                    CurrentEnemy.hp = Mathf.Min(CurrentEnemy.maxHp, CurrentEnemy.hp + healAmt);
                    intentLog = $"♥ <color=#00ffd0>{CurrentEnemy.name} 恢复了 {healAmt} HP!</color>";
                    intentSkipAtk = true;
                    break;
                case MonsterIntentType.charge:
                    intentLog = $"★ <color=#ff3344>{CurrentEnemy.name} 正在蓄力...</color>";
                    intentSkipAtk = true;
                    break;
                case MonsterIntentType.explode:
                    intentLog = $"▼ <color=#f80>{CurrentEnemy.name} 散发出危险的气息！</color>";
                    intentSkipAtk = true;
                    break;
            }
        }

        if (!string.IsNullOrEmpty(intentLog))
        {
            AddCombatLog(intentLog);
        }

        // 非攻击意图，跳过反击
        if (intentSkipAtk)
        {
            // 滚动下一回合意图
            CurrentEnemy._intent = RollMonsterIntent(CurrentEnemy);
            return;
        }

        // 挑战词条：怪物每回合回血5%
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.monsterRegen && CurrentEnemy.hp < CurrentEnemy.maxHp)
        {
            int regen = Mathf.Max(1, Mathf.RoundToInt(CurrentEnemy.maxHp * 0.05f));
            CurrentEnemy.hp = Mathf.Min(CurrentEnemy.maxHp, CurrentEnemy.hp + regen);
            AddCombatLog($"<color=#44cc44>💚 再生 +{regen}HP</color>");
        }

        // Boss回合特性效果（再生等）
        if (BossAIManager.Instance != null && BossAIManager.Instance.IsActiveBoss())
        {
            BossAIManager.Instance.ApplyTraitEffects(CurrentEnemy);

            // Boss技能：50%概率使用技能代替普通攻击
            if (UnityEngine.Random.value < 0.5f)
            {
                string skill = BossAIManager.Instance.GetBossSkillAction(CurrentEnemy);
                if (skill != "attack")
                {
                    int skillDmg = BossAIManager.Instance.ExecuteBossSkill(skill, CurrentEnemy, player);
                    if (skillDmg >= 0)
                    {
                        if (player.hp <= 0)
                        {
                            player.hp = 0;
                            OnPlayerDefeated();
                        }
                        CurrentEnemy._intent = RollMonsterIntent(CurrentEnemy);
                        EventBus.Emit(EventTypes.TurnEnd);
                        CorruptionSkillManager.Instance?.OnTurnEnd();
                        GameManager.Instance.NotifyPlayerStatsChanged();
                        return;
                    }
                }
            }
        }

        // 检查眩晕状态
        if (CurrentEnemy._netStunTurns > 0)
        {
            CurrentEnemy._netStunTurns--;
            if (CurrentEnemy._netStunTurns <= 0) CurrentEnemy._stunned = false;
            AddCombatLog($"« {CurrentEnemy.name} 在网中无法行动");
            CurrentEnemy._intent = RollMonsterIntent(CurrentEnemy);
            return;
        }

        // 原项目公式：calcMonsterDamage逻辑
        float baseAtk = CurrentEnemy.atk;
        
        // Z3污染核心：第3区首回合怪物ATK×1.2
        int zone = Mathf.Min(5, Mathf.CeilToInt(GameManager.Instance.CurrentFloor / 10f));
        if (zone == 3 && _combatRound == 1)
        {
            baseAtk = Mathf.RoundToInt(baseAtk * 1.2f);
        }

        // 狂暴：HP<50%时ATK+50%
        if ((CurrentEnemy.axes != null && System.Array.Exists(CurrentEnemy.axes, a => a == "berserk") || HasTrait(CurrentEnemy, "狂暴")) && CurrentEnemy.hp < CurrentEnemy.maxHp * 0.5f)
        {
            baseAtk = Mathf.RoundToInt(baseAtk * 1.5f);
        }

        float finalDef = (player.defense + player.defenseBonus) * FloorSignatureDefMult;
        int damage = Mathf.Max(1, Mathf.RoundToInt(baseAtk - finalDef));

        // 赌徒天堂：伤害 0.5x~2x 随机
        if (ActiveSignature != null && ActiveSignature.randomDmg)
        {
            float rMult = 0.5f + UnityEngine.Random.value * 1.5f;
            damage = Mathf.Max(1, Mathf.RoundToInt(damage * rMult));
        }

        // 脆弱结界：所有伤害×2
        if (ActiveSignature != null && ActiveSignature.dmgMult > 0)
            damage = Mathf.RoundToInt(damage * ActiveSignature.dmgMult);

        // 时间加速：怪物伤害×2
        if (ActiveSignature != null && ActiveSignature.monsterDoubleHit)
            damage = Mathf.FloorToInt(damage * 2);

        // heavy意图伤害翻倍
        if (currentIntent != null && currentIntent.type == MonsterIntentType.heavy)
        {
            float mult = currentIntent.mult > 0 ? currentIntent.mult : 1.5f;
            damage = Mathf.RoundToInt(damage * mult);
            AddCombatLog($"※ <color=#ff006e>重击!</color>");
        }

        // 永久ATK buff叠加
        if (CurrentEnemy._intentAtkBuff > 0)
        {
            damage = Mathf.RoundToInt(damage * (1 + CurrentEnemy._intentAtkBuff));
        }

        // 伏击：第1回合怪物伤害×1.5
        bool isAmbushRound = CurrentEnemy._ambush && _combatRound == 1;
        if (isAmbushRound)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            AddCombatLog($"<color=#ff006e>[突袭]</color>");
        }

        // 吸血：恢复造成伤害的30%
        if ((CurrentEnemy.axes != null && System.Array.Exists(CurrentEnemy.axes, a => a == "vampiric") || HasTrait(CurrentEnemy, "吸血")) && damage > 0)
        {
            int vHeal = Mathf.RoundToInt(damage * 0.3f);
            CurrentEnemy.hp = Mathf.Min(CurrentEnemy.maxHp, CurrentEnemy.hp + vHeal);
            AddCombatLog($"<color=#a4a>吸血+{vHeal}</color>");
        }

        if (playerDefending)
        {
            // 原版防御减伤：前3次50%，第4次65%，第5次80%，第6次+不再减伤
            int dc = player.defendCountThisCombat;
            float defReduce = 0.5f;
            if (dc > 3) defReduce = Mathf.Min(1.0f, 0.5f + (dc - 3) * 0.15f);
            if (defReduce < 1.0f)
            {
                damage = Mathf.RoundToInt(damage * defReduce);
                AddCombatLog($"◆ 防御减伤{Mathf.RoundToInt((1f - defReduce) * 100)}%");
            }
            else
            {
                AddCombatLog("⚠ 连续防御过多，减伤失效！");
            }
        }

        // 职业被动减伤
        damage = ClassAbilityData.ApplyPassiveOnDefend(player, damage);
        // 敌人特质加成（狂暴/多重攻击等）
        float eHpRatio = CurrentEnemy.maxHp > 0 ? (float)CurrentEnemy.hp / CurrentEnemy.maxHp : 1f;
        damage = TraitEffects.ApplyOnEnemyAttack(CurrentEnemy.traits, damage, eHpRatio);

        // 挑战词条：怪物狂暴（HP<50%伤害×1.5）
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.enrage
            && CurrentEnemy.hp < CurrentEnemy.maxHp * 0.5f)
        {
            damage = Mathf.RoundToInt(damage * 1.5f);
            AddCombatLog("<color=#ff4444>🔥 狂暴！</color>");
        }
        // 挑战词条：玩家受伤倍率
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.playerDmgTakenMult > 0f)
            damage = Mathf.RoundToInt(damage * ActiveChallengeModifier.playerDmgTakenMult);
        // 挑战词条：怪物吸血
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.vampiric && damage > 0)
        {
            int vHeal = Mathf.RoundToInt(damage * 0.25f);
            CurrentEnemy.hp = Mathf.Min(CurrentEnemy.maxHp, CurrentEnemy.hp + vHeal);
            AddCombatLog($"<color=#cc44aa>🧛 词条吸血 +{vHeal}HP</color>");
        }

        player.hp -= damage;

        // 特效：受伤红闪 + 伤害飘字
        if (damage > 0)
        {
            ScreenEffectsManager.Instance?.FlashDamage();
            DamageNumberPool.Instance?.SpawnDamage(damage, false);
        }

        AddCombatLog($"{CurrentEnemy.name}→<color=#ff006e>{damage}</color> 对你造成 {damage} 点伤害");

        EventBus.Emit(EventTypes.PlayerDamaged, damage);

        if (player.hp <= 0)
        {
            player.hp = 0;
            OnPlayerDefeated();
        }

        // 滚动下一回合意图
        CurrentEnemy._intent = RollMonsterIntent(CurrentEnemy);

        EventBus.Emit(EventTypes.TurnEnd);
        CorruptionSkillManager.Instance?.OnTurnEnd();
        FormResonanceSystem.Instance?.OnTurnEnd();

        // 净化回血（低污染奖励）
        int pureRegen = PollutionPassiveSystem.GetPurificationRegen(player.pollution, player.maxHp);
        if (pureRegen > 0 && player.hp > 0 && player.hp < player.maxHp)
        {
            player.hp = Mathf.Min(player.hp + pureRegen, player.maxHp);
            AddCombatLog($"<color=#88cc88>◇ 净化回复 +{pureRegen}HP</color>");
        }

        // 遗产回血
        if (player.regenPerTurn > 0 && player.hp > 0 && player.hp < player.maxHp)
        {
            int legacyRegen = Mathf.Max(1, Mathf.RoundToInt(player.maxHp * player.regenPerTurn));
            player.hp = Mathf.Min(player.hp + legacyRegen, player.maxHp);
            AddCombatLog($"<color=#88ff88>◎ 遗产回复 +{legacyRegen}HP</color>");
        }

        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    private MonsterIntent RollMonsterIntent(MonsterRuntime monster)
    {
        float roll = UnityEngine.Random.value;
        
        // 意图权重
        if (roll < 0.5f)
        {
            return new MonsterIntent { type = MonsterIntentType.attack };
        }
        else if (roll < 0.7f)
        {
            return new MonsterIntent { type = MonsterIntentType.heavy, mult = 1.5f };
        }
        else if (roll < 0.8f)
        {
            return new MonsterIntent { type = MonsterIntentType.buff, buffAmt = 0.2f };
        }
        else if (roll < 0.9f)
        {
            return new MonsterIntent { type = MonsterIntentType.heal, healAmt = Mathf.RoundToInt(monster.maxHp * 0.1f) };
        }
        else if (roll < 0.95f)
        {
            return new MonsterIntent { type = MonsterIntentType.charge };
        }
        else
        {
            return new MonsterIntent { type = MonsterIntentType.explode };
        }
    }

    private void OnEnemyDefeated(bool fromPossess)
    {
        var player = GameManager.Instance.Player;
        var enemy = CurrentEnemy;
        if (enemy == null) { EndCombat(true); return; }

        // Boss击败通知
        if (enemy.isBoss && BossAIManager.Instance != null && BossAIManager.Instance.IsActiveBoss())
        {
            BossAIManager.Instance.OnBossDefeated();
        }

        EventBus.Emit(EventTypes.MonsterKill);
        MetaProgressSystem.Instance?.OnKill();
        bool isElite = enemy.name.Contains("精英") || enemy._elite;
        if (isElite) EventBus.Emit(EventTypes.EliteKill);

        // JS原版: 首次击杀 → F1保持stage 0（仅攻击），F2自动推进到stage 2
        if (_currentTutStepId == "attack") DismissTutorial("attack");
        
        if (!fromPossess)
        {
            if (GameManager.Instance.CurrentFloor >= 2 && TutorialStage < 2) AdvanceTutorialStage(2);
            CheckTutorial("firstKill");
        }
        else
        {
            DismissTutorial("possess");
            DismissSoftHint("possess");
            if (TutorialStage < 2) AdvanceTutorialStage(2);
            CheckTutorial("new_body");
        }

        // EP & Gold rewards — JS original: 10 + mZone*15 + random(0,8)
        int baseEpReward = 15;
        int epPerZone = 20;
        int epRandomRange = 10;
        float eliteEpMultiplier = 1.8f;
        int swarmEpBonusL2 = 5;
        int swarmEpBonusL5 = 10;
        
        if (DataConfigManager.Instance != null)
        {
            var combatConfig = DataConfigManager.Instance.GetCombatConfig();
            if (combatConfig != null)
            {
                baseEpReward = combatConfig.baseEpReward;
                epPerZone = combatConfig.epPerZone;
                epRandomRange = combatConfig.epRandomRange;
                eliteEpMultiplier = combatConfig.eliteEpMultiplier;
                swarmEpBonusL2 = combatConfig.swarmEpBonusL2;
                swarmEpBonusL5 = combatConfig.swarmEpBonusL5;
            }
        }
        
        int mZone = enemy.zone > 0 ? enemy.zone : 1;
        bool couldEvolveBefore = CanEvolve();
        int epReward = baseEpReward + mZone * epPerZone + UnityEngine.Random.Range(0, epRandomRange);
        if (isElite) epReward = Mathf.FloorToInt(epReward * eliteEpMultiplier);
        // Floor curve epMult
        var floorCurve = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, GameManager.Instance.CurrentFloor, GameManager.Instance.CurrentStage);
        if (floorCurve.epMult != 1f) epReward = Mathf.FloorToInt(epReward * floorCurve.epMult);
        // Signature epMult
        if (FloorSignatureEpMult != 1f) epReward = Mathf.FloorToInt(epReward * FloorSignatureEpMult);
        // 遗产EP加成
        float epBonusLegacy = LegacyManager.Instance?.GetActiveLegacyEffectValue("ep_bonus") ?? 0;
        if (epBonusLegacy > 0) epReward = Mathf.RoundToInt(epReward * (1f + epBonusLegacy));
        player.evolutionPoints += epReward;

        TutorialManager.Instance?.RecordKill();
        TutorialManager.Instance?.OnEvolutionPointsGained(player.evolutionPoints);

        AddCombatLog($"{enemy.name} 被击败！");
        AddCombatLog($"<color=#4af>获得 EP +{epReward}</color>");

        if (!fromPossess)
        {
            AddCombatLog("<color=#ffd>你可以选择继续战斗或寻找新宿主</color>");
        }
        else
        {
            AddCombatLog("<color=#0f8>你已获得新形态！继续向上爬塔吧！</color>");
        }

        // Swarm evolution: bonus EP
        if (player.selectedClass == "swarm" && EvolutionLevel >= 2)
            player.evolutionPoints += EvolutionLevel >= 5 ? swarmEpBonusL5 : swarmEpBonusL2;

        // Blood evolution: kill heal
        if (player.selectedClass == "blood" && EvolutionLevel >= 3)
        {
            int killHeal = Mathf.RoundToInt(player.maxHp * 0.25f);
            player.hp = Mathf.Min(player.maxHp, player.hp + killHeal);
        }

        // 治愈温泉：击杀回复 HP
        if (ActiveSignature != null && ActiveSignature.healOnKill > 0)
        {
            int sigHeal = Mathf.RoundToInt(player.maxHp * ActiveSignature.healOnKill);
            player.hp = Mathf.Min(player.maxHp, player.hp + sigHeal);
            AddCombatLog($"♨ {CurrentFloorSignature} 回复 {sigHeal} HP");
        }

        // 猎杀令：击杀悬赏目标 +500EP
        if (ActiveSignature != null && ActiveSignature.id == "bountyHunt"
            && !string.IsNullOrEmpty(BountyTargetId) && enemy.id == BountyTargetId)
        {
            player.evolutionPoints += 500;
            AddCombatLog($"◎ 猎杀令完成！+500EP");
            BountyTargetId = null;
        }

        // 审判：按顺序击杀 +50EP
        if (ActiveSignature != null && ActiveSignature.id == "judgment" && _judgmentKillOrder != null)
        {
            if (_judgmentKillIdx < _judgmentKillOrder.Count && enemy.id == _judgmentKillOrder[_judgmentKillIdx])
            {
                player.evolutionPoints += 50;
                _judgmentKillIdx++;
                AddCombatLog($"⚖ 审判正确！+50EP ({_judgmentKillIdx}/{_judgmentKillOrder.Count})");
            }
        }

        if (!player.seenMonsterTypes.Contains(CurrentEnemy.id))
        {
            player.seenMonsterTypes.Add(CurrentEnemy.id);
        }
        MetaProgressSystem.Instance?.OnMonsterEncounter(CurrentEnemy.id);

        // Achievement triggers: kill
        AchievementManager.Instance?.UpdateProgress("first_kill", player.totalKillsThisRun);

        // Fragment drop — limited by floor cap
        bool fragDrop = false;
        var mode = GameManager.Instance.CurrentMode;
        var floor = GameManager.Instance.CurrentFloor;
        var cp = ModeFloorCurves.GetParams(mode, floor, GameManager.Instance.CurrentStage);
        
        int maxDropsPerFloor = mode == GameMode.Short ? (floor <= 5 ? 1 : 2) : (floor <= 10 ? 1 : 2);
        
        float fragChance = Mathf.Min(0.5f, cp.fragRate * 0.6f);
        
        if (!enemy.isBoss && !isElite && UnityEngine.Random.value < fragChance && player.fragments < 12 && _fragDropCount < maxDropsPerFloor && CurrentFloorState != null)
        {
            fragDrop = true;
            _fragDropCount++;
            Vector2Int fragPos = FindNearbyOpenCell(CurrentFloorState.playerPos);
            CurrentFloorState.actions[fragPos] = new ExploreAction
            {
                type = ExploreActionType.Fragment,
                title = "◇ 碎片",
                description = "走过去拾取",
                consumed = false
            };
            AddCombatLog("◇ 碎片掉落在附近！走过去拾取");
        }

        string rewardText = $"+{epReward}EP" + (fragDrop ? " +◇" : "");
        AddCombatLog(rewardText);

        if (enemy.isBoss || isElite) TryDropFragment(enemy);

        // JS original: boss defeat → full HP restore
        if (enemy.isBoss)
        {
            player.hp = player.maxHp;
            AddCombatLog("★ Boss击败！HP完全恢复！");
        }

        StorySystem.Instance?.CheckHiddenStory("combat_win");

        if (!couldEvolveBefore && CanEvolve())
        {
            AddCombatLog("<color=#ffcc00>★ 进化点已足够！打开菜单→进化 解锁新能力</color>");
            CanvasUIManager.Instance?.ShowFlashBanner("★ 可以进化了！", new Color(1f, 0.8f, 0f), 1.5f);
        }

        EndCombat(true);
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    private int _consecutiveDeaths = 0;

    private void OnPlayerDefeated()
    {
        AudioManager.Instance?.PlaySFX("death");
        var player = GameManager.Instance.Player;

        // death_save 遗产效果：拦截致死伤害，恢复10%HP
        if (player.deathSaveAvailable)
        {
            player.deathSaveAvailable = false;
            player.hp = Mathf.Max(1, Mathf.RoundToInt(player.maxHp * 0.1f));
            AddCombatLog("<color=#a040f0>★ 遗产发动：死亡拯救！恢复10%HP</color>");
            EventBus.Emit(EventTypes.PlayerStatsChanged);
            return;
        }

        AddCombatLog("<color=#ff006e><b>// 宿主死亡 //</b></color>");
        MetaProgressSystem.Instance?.OnDeath();
        EventBus.Emit(EventTypes.HostDeath);

        _consecutiveDeaths++;
        player.deathReviveCharges = Mathf.Max(0, player.deathReviveCharges);

        // 标记当前形态所在的卡槽为死亡
        MarkCurrentFormAsDead(player);

        // 检查是否还有存活的备用卡槽（非当前、非死亡）
        bool hasAliveBackup = false;
        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            string formId = player.ownedForms[i];
            bool isDead = i < player.deadForms.Count && player.deadForms[i];

            if (i == player.ownedForms.IndexOf(player.currentFormId)) continue;
            if (isDead) continue;

            hasAliveBackup = true;
            break;
        }

        if (hasAliveBackup)
        {
            // 还有存活形态（包括本命），弹出切换选择
            ShowingDeathFormSelect = true;
        }
        else
        {
            // 所有卡槽全部死亡 → 弹回滚页面
            int epPenalty = Mathf.Min(2000, 500 + GameManager.Instance.CurrentFloor * 50);
            bool hasAnchor = AnchorSystem.Instance != null && AnchorSystem.Instance.HasActiveAnchor();

            DeathRollbackEpCost = epPenalty;
            DeathRollbackAnchorFloor = hasAnchor ? AnchorSystem.Instance.GetCurrentAnchorFloor() : 0;
            ShowingDeathRollback = true;
        }
    }

    void MarkCurrentFormAsDead(GameManager.PlayerData player)
    {
        int currentIndex = player.ownedForms.IndexOf(player.currentFormId);
        if (currentIndex >= 0)
        {
            while (player.deadForms.Count <= currentIndex)
            {
                player.deadForms.Add(false);
            }
            player.deadForms[currentIndex] = true;
            AddCombatLog($"<color=#ff4444>✖ {GetDisplayName(player.currentFormId)} 形态已死亡，无法再切换</color>");
            if (player.currentFormId != "human")
            {
                AddCombatLog("<color=#aaaaaa>   击败新宿主可获得替代形态</color>");
            }
        }
    }

    private void TriggerDeathReport()
    {
        GameManager.Instance?.GameOver();
        GenerateRunReport(false, "宿主崩坏");
    }

    private void OnShortModeTimeUp()
    {
        AddCombatLog("<color=#ffaa00><b>// 闯塔时间耗尽 //</b></color>");
        CurrentScreen = RunScreen.GameOver;
        GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
        GenerateRunReport(false, "时间耗尽");
    }

    public void ChooseDeathRollback(bool doRollback)
    {
        ShowingDeathRollback = false;
        var player = GameManager.Instance.Player;

        if (doRollback)
        {
            if (player.evolutionPoints >= DeathRollbackEpCost)
            {
                player.evolutionPoints -= DeathRollbackEpCost;
                AddCombatLog($"⛓ 记忆锚点激活 -{DeathRollbackEpCost}EP");
                AnchorSystem.Instance.UseAnchor(_consecutiveDeaths);
                return;
            }
            else
            {
                AddCombatLog($"<color=#ff006e>EP不足（{player.evolutionPoints}/{DeathRollbackEpCost}），回滚失败</color>");
                AudioManager.Instance?.PlaySFX("error");
            }
        }

        AddCombatLog("意识消散...");
        CurrentScreen = RunScreen.GameOver;
        TriggerDeathReport();
    }

    public void GenerateRunReport(bool victory, string cause)
    {
        var player = GameManager.Instance.Player;
        float elapsed = Time.time - player.runStartTime;
        var mode = GameManager.Instance.CurrentMode;

        if (mode == GameMode.Short)
        {
            PlayerPrefs.SetInt("FirstRunComplete", 1);
            PlayerPrefs.Save();
            
            var unlockSystem = ModuleUnlockSystem.Instance;
            if (unlockSystem != null)
            {
                unlockSystem.UnlockModules(
                    Module.Achievements,
                    Module.Bestiary,
                    Module.Archive,
                    Module.EchoAltar,
                    Module.Leaderboard,
                    Module.Shop,
                    Module.DailyReward,
                    Module.ChallengeMode,
                    Module.ClassicMode,
                    Module.ExpeditionMode,
                    Module.Settings
                );
            }
        }

        float currentDur = Time.time - player.currentFormStartTime;
        if (currentDur > player.longestFormDuration)
        {
            player.longestFormDuration = currentDur;
            player.longestFormId = player.currentFormId;
        }

        string longestName = player.longestFormId == "human" ? "基础形态" : GetDisplayName(player.longestFormId);

        // 原版模式标题
        string modeTitle, modeSubtitle;
        switch (mode)
        {
            case GameMode.Short:
                modeTitle = victory ? "暗影通关报告" : "暗影终止报告";
                modeSubtitle = "YOU · ARE · ME";
                break;
            case GameMode.Expedition:
            {
                int stage = GameManager.Instance.CurrentStage;
                int maxStage = GameManager.Instance.MaxStage;
                if (victory && stage >= maxStage)
                {
                    modeTitle = "远征全通关报告";
                    modeSubtitle = $"EXPEDITION · ALL {maxStage} STAGES CLEARED";
                }
                else
                {
                    modeTitle = victory ? "远征报告" : "远征终止报告";
                    modeSubtitle = victory ? "EXPEDITION · COMPLETE" : $"EXPEDITION · TERMINATED (STAGE {stage})";
                }
                break;
            }
            default:
                modeTitle = victory ? "宿命通关报告" : "宿命终止报告";
                modeSubtitle = victory ? "PARASITE TOWER · CLEARED" : "PARASITE TOWER · TERMINATED";
                break;
        }

        // 原版死因标签
        string causeLabel, causeIcon;
        if (victory)
        {
            if (mode == GameMode.Expedition)
                causeLabel = GameManager.Instance.CurrentStage >= GameManager.Instance.MaxStage
                    ? $"远征全通关({GameManager.Instance.MaxStage}关)" : "远征完成";
            else
                causeLabel = mode == GameMode.Short ? "暗影完成" : "通关";
            causeIcon = "⚔";
        }
        else
        {
            switch (cause)
            {
                case "宿主崩坏": causeLabel = "宿主崩坏"; causeIcon = "☠"; break;
                case "污染失控": causeLabel = "污染失控"; causeIcon = "☢"; break;
                case "时间耗尽": causeLabel = "时间耗尽"; causeIcon = "⏱"; break;
                default: causeLabel = cause; causeIcon = "☠"; break;
            }
        }

        // 评分公式 - 调整为更合理的分布
        int baseScore = GameManager.Instance.CurrentFloor * 20
            + player.totalKillsThisRun * 5
            + player.possessionCountThisRun * 20
            + player.evolutionPoints / 8;

        // 胜利奖励
        int victoryBonus = victory ? 300 : 0;

        // 短局模式：12层通关基准分约1000
        // 经典模式：50层通关基准分约1500
        // 远征模式：20层通关基准分约1200
        int modeMultiplier = 1;
        if (mode == GameMode.Classic) modeMultiplier = 2;
        if (mode == GameMode.Expedition) modeMultiplier = 3;

        int score = baseScore * modeMultiplier + victoryBonus;

        // 短局模式速通加分
        if (mode == GameMode.Short && victory && elapsed < 300f)
            score += Mathf.RoundToInt((300f - elapsed) * 1f);

        // 评级 - 根据分数分布调整阈值
        string rank;
        Color rankColor;
        if (score >= 1000) { rank = "SSS"; rankColor = new Color(1f, 0f, 0.43f); }
        else if (score >= 750) { rank = "SS"; rankColor = new Color(0f, 1f, 0.82f); }
        else if (score >= 500) { rank = "S"; rankColor = new Color(1f, 0.8f, 0f); }
        else if (score >= 350) { rank = "A"; rankColor = new Color(0.4f, 0.8f, 1f); }
        else if (score >= 150) { rank = "B"; rankColor = new Color(0.7f, 0.7f, 0.7f); }
        else { rank = "C"; rankColor = new Color(0.5f, 0.5f, 0.5f); }

        // 残响奖励
        var reportConfig = DataConfigManager.Instance?.GetRunReportConfig();
        int echo;
        if (reportConfig != null)
            echo = score / reportConfig.echoDivider + (victory ? reportConfig.victoryBonus : reportConfig.defeatBonus);
        else
            echo = score / 3 + (victory ? 50 : 10);

        // 每周挑战额外奖励：通关 +100 残响
        if (mode == GameMode.Weekly && victory)
            echo += 100;

        // 章节进度（远征模式）
        string chapters = "";
        if (mode == GameMode.Expedition)
        {
            int floor = GameManager.Instance.CurrentFloor;
            chapters = (floor >= 1 ? "前厅" : "") + (floor >= 6 ? " · 裂变" : "") + (floor >= 11 ? " · 深层" : "") + (floor >= 16 ? " · 终域" : "");
        }

        var possessedForms = player.ownedForms.Where(f => f != "human").Select(f => GetDisplayName(f)).ToList();
        var legacyNames = LegacyManager.Instance?.GetActiveLegacies()?.Select(l => l.name).ToList() ?? new List<string>();
        var resonanceTriggers = _resonanceTriggersThisRun;
        string defeatAdvice = victory ? "" : GenerateDefeatAdvice(player);

        LastReport = new Report
        {
            isVictory = victory,
            modeName = modeTitle,
            deathCause = $"{causeIcon} {causeLabel}",
            rank = rank,
            floorsReached = GameManager.Instance.CurrentFloor,
            maxFloors = GameManager.Instance.MaxFloor,
            possessions = player.possessionCountThisRun,
            kills = player.totalKillsThisRun,
            survivalSeconds = elapsed,
            longestHost = longestName,
            longestHostSeconds = player.longestFormDuration,
            maxPollution = player.maxPollutionReached,
            finalForm = player.currentFormId == "human" ? "基础形态" : GetDisplayName(player.currentFormId),
            score = score,
            echoReward = echo,
            legacyCount = LegacyManager.Instance?.GetLegacyCount() ?? 0,
            legacySummary = BuildLegacySummary(),
            pollutionTier = PollutionPassiveSystem.GetPollutionTier(player.pollution).label,
            formCount = player.ownedForms.Count,
            stagesCleared = mode == GameMode.Expedition ? GameManager.Instance.CurrentStage : 0,
            possessedForms = possessedForms,
            legacyNames = legacyNames,
            resonanceTriggers = resonanceTriggers,
            maxDamage = _maxDamageThisRun,
            maxDamageCombo = _maxDamageComboThisRun,
            defeatAdvice = defeatAdvice
        };

        // 残响奖励写入持久存储
        int currentEchoes = PlayerPrefs.GetInt("pt_meta_echoes", 0);
        PlayerPrefs.SetInt("pt_meta_echoes", currentEchoes + echo);
        PlayerPrefs.Save();

        LastMessage = $"{modeSubtitle}\n{modeTitle}";

        // Achievement triggers on run end
        if (victory)
        {
            // JS: 结局成就只在经典50层通关后解锁（triggerEnding 只在 floorCap=50 时触发）
            if (GameManager.Instance.CurrentMode == GameMode.Classic && GameManager.Instance.CurrentFloor >= 50)
            {
                string classId = player.selectedClass;
                AchievementManager.Instance?.UnlockAchievement(classId + "_end");
            }
            if (player.noDeathRun && GameManager.Instance.CurrentFloor >= 25)
                AchievementManager.Instance?.UnlockAchievement("no_death");
            if (player.pollution <= 0f && GameManager.Instance.CurrentFloor >= 25)
                AchievementManager.Instance?.UnlockAchievement("pollution0");

            if (GameManager.Instance.CurrentMode == GameMode.Expedition
                && GameManager.Instance.CurrentStage >= GameManager.Instance.MaxStage)
                MetaProgressSystem.Instance?.OnExpeditionFullClear();
        }
    }

    public void OnResonanceTriggered(string resonanceName)
    {
        if (!_resonanceTriggersThisRun.Contains(resonanceName))
            _resonanceTriggersThisRun.Add(resonanceName);
    }

    string GenerateDefeatAdvice(GameManager.PlayerData player)
    {
        var legacyManager = LegacyManager.Instance;
        var legacies = legacyManager?.GetActiveLegacies() ?? new List<LegacyAbility>();
        float pollution = player.pollution;
        int ownedForms = player.ownedForms.Count;

        if (pollution >= 80f && !legacies.Any(l => l.effectType == "regen" || l.effectType == "lifesteal"))
            return "你在高污染状态下缺少生存遗产，试试携带吸血或恢复类遗产";

        if (pollution >= 60f && !legacies.Any(l => l.effectType == "DefenseBonus" || l.effectType == "DamageReduction"))
            return "高污染会降低防御，建议携带防御类遗产抵消负面效果";

        if (_resonanceTriggersThisRun.Count == 0 && ownedForms >= 3)
            return "你拥有多个形态但未触发共鸣，战斗中尝试切换形态获得增益";

        if (player.possessionCountThisRun == 0 && GameManager.Instance.CurrentFloor >= 5)
            return "尽快附身敌人获取更强形态，不要一直使用本命战斗";

        if (legacies.Count == 0 && GameManager.Instance.CurrentFloor >= 5)
            return "获取遗产能大幅提升战力，记得在附身后选择遗产";

        if (pollution < 30f && ownedForms >= 2)
            return "适度提升污染可增强攻击力，不必完全保持低污染";

        var offensiveLegacies = legacies.Count(l => l.effectType == "AttackBonus" || l.effectType == "crit_rate");
        var defensiveLegacies = legacies.Count(l => l.effectType == "DefenseBonus" || l.effectType == "DamageReduction" || l.effectType == "Evasion");
        
        if (offensiveLegacies >= 2 && defensiveLegacies == 0)
            return "你的构筑偏重攻击缺少生存，建议搭配防御类遗产";

        if (defensiveLegacies >= 2 && offensiveLegacies == 0)
            return "你的构筑偏重防御缺少输出，建议搭配攻击类遗产";

        return "";
    }

    string GetFormBaseType(string formId)
    {
        // 去掉形态ID的编号后缀（如 dog_1234 → dog）
        if (string.IsNullOrEmpty(formId)) return formId;
        int underscoreIndex = formId.LastIndexOf('_');
        if (underscoreIndex > 0)
        {
            string suffix = formId.Substring(underscoreIndex + 1);
            if (int.TryParse(suffix, out _))
            {
                return formId.Substring(0, underscoreIndex);
            }
        }
        return formId;
    }

    string BuildLegacySummary()
    {
        var legs = LegacyManager.Instance?.GetEquippedLegacies();
        if (legs == null || legs.Count == 0) return "无";
        return string.Join(" ", legs.Select(l => $"{l.icon}{l.name}"));
    }

    public bool IsOnlyBaseFormAlive()
    {
        var player = GameManager.Instance.Player;

        if (player.ownedForms.Count <= 1)
        {
            return false;
        }

        bool hasAliveNonBase = false;
        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            string formId = player.ownedForms[i];
            if (formId == "human") continue;

            bool isDead = i < player.deadForms.Count && player.deadForms[i];
            if (!isDead)
            {
                hasAliveNonBase = true;
                break;
            }
        }

        if (hasAliveNonBase)
        {
            return false;
        }

        return true;
    }

    public bool CanAcceptNewForm()
    {
        var player = GameManager.Instance.Player;

        // 只剩本命英雄存活时不能附身
        if (IsOnlyBaseFormAlive()) return false;

        // 有空位可以直接添加
        int deadCount = 0;
        for (int i = 0; i < player.ownedForms.Count && i < player.deadForms.Count; i++)
            if (player.deadForms[i]) deadCount++;
        int aliveCount = player.ownedForms.Count - deadCount;
        if (aliveCount < player.formSlots - deadCount)
            return true;

        // 卡槽满了，但有非human的活形态可替换
        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            bool isDead = i < player.deadForms.Count && player.deadForms[i];
            if (isDead) continue;
            if (player.ownedForms[i] == "human") continue;
            return true;
        }

        return false;
    }

    public void OnFormReplaceSelected(int originalIndex)
    {
        ShowingFormReplace = false;
        var player = GameManager.Instance.Player;

        if (originalIndex >= 0 && originalIndex < player.ownedForms.Count && _pendingPossessFormId != null)
        {
            string replacedOldFormId = player.ownedForms[originalIndex];
            player.ownedForms[originalIndex] = _pendingPossessFormId;
            if (!string.IsNullOrEmpty(replacedOldFormId) && replacedOldFormId != "human")
                EventBus.Emit(EventTypes.FormReplaced, replacedOldFormId);
        }

        if (_pendingPossessCallback != null)
        {
            _pendingPossessCallback.Invoke();
            _pendingPossessCallback = null;
            _pendingPossessFormId = null;
        }
    }

    public void CancelFormReplace()
    {
        ShowingFormReplace = false;
        _pendingPossessCallback = null;
        _pendingPossessFormId = null;
        AddCombatLog("放弃附身");
        EndCombat(true);
    }

    void ContinuePossessAfterSelection(string newFormId, MonsterRuntime target, GameManager.PlayerData player, string oldFormId, bool fullHpInherit, int extraPollution)
    {
        // 使用LoadForm计算属性（与PlayerSwitchForm保持一致）
        LoadForm(newFormId);
        ApplyResonanceBonuses(newFormId);

        // 附身血量继承
        if (fullHpInherit)
            player.hp = player.maxHp;
        else
        {
            float hpRatio = target.maxHp > 0 ? target.hp / (float)target.maxHp : 1f;
            player.hp = Mathf.Max(1, Mathf.RoundToInt(player.maxHp * Mathf.Clamp01(hpRatio)));
        }

        // 保存到formHpMap
        player.formHpMap[newFormId] = player.hp;
        player.formMaxHpMap[newFormId] = player.maxHp;

        // 继承怪物技能
        player.traits.Clear();
        if (target.traits != null)
            player.traits.AddRange(target.traits);
        if (player.traits.Count > 0)
            AudioManager.Instance?.PlaySFX("trait");

        // 污染变异检查
        if (player.traits.Count > 0 && PollutionPassiveSystem.ShouldMutateTrait(player.pollution))
        {
            int mutIdx = UnityEngine.Random.Range(0, player.traits.Count);
            string oldTrait = player.traits[mutIdx];
            string newTrait = PollutionPassiveSystem.MutateTrait(oldTrait);
            player.traits[mutIdx] = newTrait;
            AddCombatLog($"<color=#ff6600>※ 污染变异: {oldTrait} → {newTrait}</color>");
        }

        player.baseMaxHp = player.maxHp;
        player.baseAttack = player.attack;
        player.baseDefense = player.defense;

        float polAdd = 10f + extraPollution;
        PollutionSystem.Instance.GainPollution(polAdd);

        player.possessionCountThisRun++;

        AddCombatLog($"附身→{GetDisplayName(newFormId)} HP:{player.hp}/{player.maxHp} ATK:{player.attack} DEF:{player.defense}");


        GameManager.Instance.NotifyPlayerStatsChanged();
        CanvasUIManager.Instance?.SyncCombat(this, player);

        EndCombat(true);
    }

    public void TryPossess(string type)
    {
        if (CurrentEnemy == null || CurrentScreen != RunScreen.Combat) return;

        var player = GameManager.Instance.Player;
        var target = CurrentEnemy;

        // 检查是否只剩本命英雄（无法附身）
        if (IsOnlyBaseFormAlive())
        {
            AddCombatLog("只剩本命英雄，无法附身");
            AudioManager.Instance?.PlaySFX("error");
            return;
        }

        // 检查是否有可用卡槽容纳新形态（死亡卡槽不可覆盖）
        if (!CanAcceptNewForm())
        {
            AddCombatLog("没有可用卡槽，无法附身！");
            AudioManager.Instance?.PlaySFX("error");
            return;
        }

        // 加入：100%成功，跳过判定
        if (type == "join")
        {
            ExecutePossessSuccess(false, 15);
            return;
        }

        // 基础成功率（原版公式）
        float hpRatio = target.maxHp > 0 ? (float)target.hp / target.maxHp : 0.5f;
        float hpFactor = Mathf.Max(0f, 1f - hpRatio * 0.6f);
        float defResist = Mathf.Min(0.15f, target.def * 0.005f);
        float baseRate = 0.4f * hpFactor - defResist + player.possessionBonus;

        // 策略修正
        float finalRate = baseRate;
        bool fullHpInherit = false;
        bool affinityBonus = false;
        int extraPollution = 0;

        switch (type)
        {
            case "intimidate":
                finalRate = baseRate - 0.2f;
                fullHpInherit = true;
                extraPollution = 0;
                break;
            case "trade":
                finalRate = baseRate + 0.2f;
                extraPollution = 10;
                break;
            case "resonance":
                finalRate = baseRate + 0.1f;
                affinityBonus = true;
                extraPollution = -5;
                break;
        }

        // 怪物心理倾向匹配 +15%
        string bias = GetMonsterBias(target.id);
        if (bias == type)
        {
            finalRate += 0.15f;
            AddCombatLog("✨ 意识松动！倾向匹配+15%");
        }

        // 教程看门犬100%
        bool isTutorialDog = target.id == "dog" && GameManager.Instance.CurrentFloor == 1;
        
        // 首局第一次附身必成
        bool isFirstPossession = _firstPossession && PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
        if (isTutorialDog || isFirstPossession) finalRate = 1f;

        finalRate = Mathf.Clamp(finalRate, 0.01f, 0.95f);
        if (isTutorialDog || isFirstPossession) finalRate = 1f;

        int ratePercent = Mathf.RoundToInt(finalRate * 100);

        if (UnityEngine.Random.value < finalRate)
        {
            // === 成功 ===
            if (affinityBonus)
            {
                int bond = player.formBondCounts.ContainsKey(target.id) ? player.formBondCounts[target.id] : 0;
                player.formBondCounts[target.id] = bond + 1;
                AddCombatLog($"✦ 共鸣羁绊+1 (当前{bond + 1}次)");
            }
            if (type == "intimidate")
            {
                player.evolutionPoints += 50;
                AddCombatLog("† 威压成功！额外+50EP");
            }

            AddCombatLog($"◎ [{type}] 附身成功！({ratePercent}%)");
            AudioManager.Instance?.PlaySFX("possess");

            // 跨局羁绊统计 + 可视化通知
            MetaProgressSystem.Instance?.OnFormBondIncrease(target.id, 1);
            int bondNow = player.formBondCounts.ContainsKey(target.id) ? player.formBondCounts[target.id] : 0;
            int bondLv = Mathf.Clamp(bondNow / 3, 0, 5);
            if (bondNow > 0 && bondNow % 3 == 0)
                CanvasUIManager.Instance?.ShowFlashBanner($"✦ {target.name} 羁绊 Lv.{bondLv}！ATK/DEF +{bondLv * 5}%", new Color(0.65f, 0.25f, 0.95f), 2f);

            // 寄生乐园：附身成功 +20 污染
            if (ActiveSignature != null && ActiveSignature.id == "parasiteParadise")
            {
                PollutionSystem.Instance.GainPollution(20);
                AddCombatLog("★ 寄生乐园 — 污染+20");
            }

            ExecutePossessSuccess(fullHpInherit, extraPollution);

            if (GameManager.Instance.CurrentFloor >= 2 && TutorialStage < 2)
            {
                AdvanceTutorialStage(2);
            }
        }
        else
        {
            // === 失败 — 按策略分层后果 ===
            AudioManager.Instance?.PlaySFX("possessFail");
            ScreenEffectsManager.Instance?.FlashPossessFail();
            int failPol = 5;
            if (type == "intimidate")
            {
                int bkDmg = Mathf.FloorToInt(player.maxHp * 0.3f);
                player.hp = Mathf.Max(1, player.hp - bkDmg);
                target.atk = Mathf.FloorToInt(target.atk * 1.5f);
                failPol = 20;
                AddCombatLog($"<color=#ff006e>◆◆◆ 反噬！</color> <color=#ff6b35>HP-{bkDmg}</color> <color=#ffcc00>{target.name}狂暴ATK×1.5</color> <color=#ff006e>污染+{failPol}</color>");
            }
            else if (type == "resonance")
            {
                int defLoss = Mathf.Max(1, Mathf.FloorToInt(player.defense * 0.3f));
                player.defense = Mathf.Max(0, player.defense - defLoss);
                int mHeal = Mathf.FloorToInt(target.maxHp * 0.3f);
                target.hp = Mathf.Min(target.maxHp, target.hp + mHeal);
                failPol = 15;
                AddCombatLog($"<color=#ff006e>◆◆◆ 意识割裂！</color> <color=#00c8ff>DEF-{defLoss}</color> <color=#00ffd0>{target.name}回复{mHeal}HP</color> <color=#ff006e>污染+{failPol}</color>");
            }
            else
            {
                target.atk = Mathf.FloorToInt(target.atk * 1.5f);
                failPol = 5;
                AddCombatLog($"<color=#ff006e>◆◆◆ 交易被识破！</color> <color=#ffcc00>{target.name}狂暴ATK×1.5</color> <color=#ff006e>污染+{failPol}</color>");
            }

            PollutionSystem.Instance.GainPollution(failPol);
            GameManager.Instance.NotifyPlayerStatsChanged();

            // 附身失败后，当前形态破裂，本战斗内无法再次寄生
            player.formBroken = true;

            // 失败后不结束战斗，敌人回合
            EnemyTurn(false);
        }
    }

    void ExecutePossessSuccess(bool fullHpInherit, int extraPollution)
    {
        var player = GameManager.Instance.Player;
        var target = CurrentEnemy;
        if (target == null) return;

        TutorialManager.Instance?.RecordAction(TutorialAction.Possess);

        // 保存旧形态HP和maxHp
        player.formHpMap[player.currentFormId] = player.hp;
        player.formMaxHpMap[player.currentFormId] = player.maxHp;

        EventBus.Emit(EventTypes.PossessStart);

        string oldFormId = player.currentFormId;

        // 保存旧形态到槽
        if (!player.ownedForms.Contains(oldFormId))
            player.ownedForms.Add(oldFormId);

        // 添加新形态到槽
        string newFormId = target.id;
        
        // 检查是否已有同种形态（直接覆盖）
        bool hasSameType = false;
        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            // 同种形态直接覆盖（去掉编号后缀比较）
            string existingForm = player.ownedForms[i];
            if (GetFormBaseType(existingForm) == GetFormBaseType(newFormId))
            {
                player.ownedForms[i] = newFormId;
                hasSameType = true;
                EventBus.Emit(EventTypes.FormReplaced, existingForm);
                break;
            }
        }
        
        // 如果没有同种形态，需要检查槽位
        if (!hasSameType)
        {
            // 计算活卡槽数和死亡卡槽数
            int deadCount = 0;
            for (int i = 0; i < player.ownedForms.Count && i < player.deadForms.Count; i++)
                if (player.deadForms[i]) deadCount++;
            int aliveCount = player.ownedForms.Count - deadCount;
            int availableSlots = player.formSlots - deadCount;

            if (aliveCount < availableSlots)
            {
                player.ownedForms.Add(newFormId);
                while (player.deadForms.Count < player.ownedForms.Count)
                    player.deadForms.Add(false);
            }
            else
            {
                // 卡槽满了，弹出选择面板让玩家选替换哪个
                var options = new List<(int index, string formId)>();
                for (int i = 0; i < player.ownedForms.Count; i++)
                {
                    bool isDead = i < player.deadForms.Count && player.deadForms[i];
                    if (isDead) continue;
                    if (player.ownedForms[i] == "human") continue;
                    options.Add((i, player.ownedForms[i]));
                }

                if (options.Count > 1)
                {
                    FormReplaceOptions = options;
                    _pendingPossessFormId = newFormId;
                    _pendingPossessCallback = () => ContinuePossessAfterSelection(newFormId, target, player, oldFormId, fullHpInherit, extraPollution);
                    ShowingFormReplace = true;
                    return;
                }

                // 只有1个可替换，直接替换
                if (options.Count == 1)
                {
                    string replacedFormId = options[0].formId;
                    player.ownedForms[options[0].index] = newFormId;
                    EventBus.Emit(EventTypes.FormReplaced, replacedFormId);
                }
                else
                {
                    int currentIdx = player.ownedForms.IndexOf(oldFormId);
                    if (currentIdx >= 0 && oldFormId != "human")
                    {
                        player.ownedForms[currentIdx] = newFormId;
                        EventBus.Emit(EventTypes.FormReplaced, oldFormId);
                    }
                }
            }
        }

        // 使用LoadForm计算属性（与PlayerSwitchForm保持一致）
        LoadForm(newFormId);
        ApplyResonanceBonuses(newFormId);

        // 附身血量继承
        if (fullHpInherit)
        {
            // 威压: 满血接管
            player.hp = player.maxHp;
        }
        else
        {
            // trade / resonance: 继承怪物当前残血比例
            float hpRatio = target.maxHp > 0 ? target.hp / (float)target.maxHp : 1f;
            hpRatio = Mathf.Clamp01(hpRatio);
            player.hp = Mathf.Max(1, Mathf.RoundToInt(player.maxHp * hpRatio));
            AddCombatLog($"∿ 意识融合: 继承目标残血 {player.hp}/{player.maxHp} ({hpRatio * 100:F0}%)");
        }

        // 保存到formHpMap
        player.formHpMap[newFormId] = player.hp;
        player.formMaxHpMap[newFormId] = player.maxHp;

        // 保存玩家原始特性（用于污染减免计算，必须在特性替换之前）
        var originalTraits = new List<string>();
        if (player.traits != null)
            originalTraits.AddRange(player.traits);

        // 继承怪物技能
        player.traits.Clear();
        if (target.traits != null)
            player.traits.AddRange(target.traits);
        if (player.traits.Count > 0)
            AudioManager.Instance?.PlaySFX("trait");

        // 污染变异检查
        if (player.traits.Count > 0 && PollutionPassiveSystem.ShouldMutateTrait(player.pollution))
        {
            int mutIdx = UnityEngine.Random.Range(0, player.traits.Count);
            string oldTrait = player.traits[mutIdx];
            string newTrait = PollutionPassiveSystem.MutateTrait(oldTrait);
            player.traits[mutIdx] = newTrait;
            AddCombatLog($"<color=#ff6600>※ 污染变异: {oldTrait} → {newTrait}</color>");
        }

        // 更新基础值（防止形态切换时丢失）
        player.baseMaxHp = player.maxHp;
        player.baseAttack = player.attack;
        player.baseDefense = player.defense;

        // 污染计算（原版逻辑：基础10 + 额外 + 楼层乘数 + 特性减免）
        // 使用玩家原始特性计算减免（不是怪物的特性）
        float polAdd = 10f + extraPollution;
        var curveP = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, GameManager.Instance.CurrentFloor, GameManager.Instance.CurrentStage);
        polAdd = Mathf.Round(polAdd * curveP.possessPollMult);
        
        // 特性减免（使用原始特性）
        float polResist = 0f;
        foreach (var trait in originalTraits)
        {
            if (trait == "净化") polResist += 0.5f;
            else if (trait == "寄生强化") polResist += 0.15f;
        }
        if (polResist > 0)
        {
            polAdd = Mathf.FloorToInt(polAdd * (1 - polResist));
            AddCombatLog($"<color=#00ffd0>◇ 污染抵抗: 减少{Mathf.RoundToInt(polResist * 100)}%</color>");
        }
        
        // Ghost Lv5: 附身不增加污染
        if (player.selectedClass == "ghost" && EvolutionLevel >= 5)
        {
            polAdd = 0;
            AddCombatLog("<color=#aaddff>◇ 幽灵精通: 附身免污染</color>");
        }
        PollutionSystem.Instance.GainPollution(Mathf.Max(0, polAdd));

        player.possessionCountThisRun++;
        MetaProgressSystem.Instance?.OnPossession();
        AchievementManager.Instance?.UpdateProgress("first_possess", 1);
        AchievementManager.Instance?.UpdateProgress("possess5", player.seenMonsterTypes.Count);
        AchievementManager.Instance?.UpdateProgress("possess10", player.seenMonsterTypes.Count);
        EventBus.Emit(EventTypes.PossessEnd);

        int oldHp = player.formHpMap.ContainsKey(oldFormId) ? player.formHpMap[oldFormId] : player.maxHp;
        int oldAtk = player.baseAttack;
        int oldDef = player.baseDefense;

        if (_firstPossession)
        {
            AddCombatLog($"<color=#ffd700>【首次附身】</color>");
            AddCombatLog($"<color=#00ff88>你成功夺取了 {GetDisplayName(newFormId)} 的身体！</color>");
            AddCombatLog($"<color=#00ff88>HP: {oldHp} → {player.hp}</color>");
            AddCombatLog($"<color=#ff6666>ATK: {oldAtk} → {player.attack}</color>");
            AddCombatLog($"<color=#6666ff>DEF: {oldDef} → {player.defense}</color>");
            _firstPossession = false;
            _firstPossessionSuccess = true;

            Analytics.Track("first_run_first_possession", 
                ("form_id", newFormId), 
                ("form_name", GetDisplayName(newFormId)),
                ("hp_gain", player.hp - oldHp),
                ("atk_gain", player.attack - oldAtk),
                ("def_gain", player.defense - oldDef),
                ("floor", GameManager.Instance.CurrentFloor));

            AdvanceTutorialStage(2);
        }
        else
        {
            AddCombatLog($"附身→{GetDisplayName(newFormId)} HP:{player.hp}/{player.maxHp} ATK:{player.attack} DEF:{player.defense} 污染+{polAdd:F0}");
        }

        StorySystem.Instance?.CheckHiddenStory("possess_success");

        string[] newTraits = target.traits?.ToArray() ?? new string[0];

        PossessionCinematic.Instance?.Play(oldFormId, newFormId, GetDisplayName(newFormId), player.maxHp, player.attack, player.defense, oldHp, oldAtk, oldDef, newTraits, () => {
            OnEnemyDefeated(true);
            GameManager.Instance.NotifyPlayerStatsChanged();
            
            if (_firstPossessionSuccess)
            {
                CanvasUIManager.Instance?.ShowFlashBanner("这就是附身的力量！", new Color(0.3f, 0.8f, 1f), 3f);
            }
            else
            {
                CanvasUIManager.Instance?.ShowFlashBanner("你已经变成了更强的怪物！", new Color(0.3f, 0.8f, 1f), 2f);
            }
        });
    }

    string GetMonsterBias(string monsterId)
    {
        // 原版怪物心理倾向
        switch (monsterId)
        {
            case "rat": case "slime": case "dog": case "bat": case "vine": case "worm": case "moth":
            case "shade": case "lurker": case "wraith": case "nightmare": case "horror": case "origin":
            case "boss4": case "boss5":
                return "resonance";
            case "roach": case "wolf": case "wasp": case "larva": case "mantis": case "scorpion": case "hydra":
            case "voidbeast": case "titan": case "chaos": case "voiddragon":
            case "boss1": case "boss2":
                return "intimidate";
            case "gecko": case "drone": case "spider": case "guard": case "beetle": case "watcher":
            case "deathknight": case "colossus": case "plague":
            case "boss3":
                return "trade";
            default: return "";
        }
    }

    public void EndCombat(bool victory)
    {
        if (victory && CurrentEnemy != null)
        {
            GameManager.Instance.Player.totalKillsThisRun++;
        }

        EventBus.Emit(EventTypes.CombatEnd);
        if (victory) EventBus.Emit(EventTypes.CombatVictory);
        CorruptionSkillManager.Instance?.OnCombatEnd();

        PossessionCinematic.Instance?.ForceComplete();
        if (PossessionCinematic.Instance != null)
        {
            var go = PossessionCinematic.Instance.gameObject;
            Canvas canvas = go.GetComponentInChildren<Canvas>();
            if (canvas != null) canvas.gameObject.SetActive(false);
        }

        if (victory && InWaveDefense)
        {
            OnWaveMonsterDefeated();
            return;
        }

        GameManager.Instance.Player.formBroken = false;
        
        CurrentScreen = RunScreen.Exploration;
        AudioManager.Instance?.RestoreBGM(0.3f);
        CurrentEnemy = null;
        DismissSoftHint();
        DismissTutorial(null, true);
        GameManager.Instance.ChangeState(GameManager.GameState.Playing);
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public void Rest()
    {
        if (CurrentScreen != RunScreen.Exploration) return;
        AudioManager.Instance?.PlaySFX("heal");
        var player = GameManager.Instance.Player;
        int heal = Mathf.RoundToInt(player.maxHp * 0.2f);
        if (ActiveChallengeModifier != null && ActiveChallengeModifier.healMult > 0f)
            heal = Mathf.RoundToInt(heal * ActiveChallengeModifier.healMult);
        player.hp = Mathf.Min(player.maxHp, player.hp + heal);
        if (!IsSoftHintShown("hint_rest"))
        {
            MarkSoftHintShown("hint_rest");
            AddCombatLog("<color=#88ff88>◇ 休息可恢复HP，合理利用休息站管理血量</color>");
        }
        LastMessage = $"休息恢复 {heal} HP。";
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public bool CanEvolve()
    {
        var player = GameManager.Instance.Player;
        if (!EvolutionData.Trees.TryGetValue(player.selectedClass, out var tree)) return false;
        if (EvolutionLevel >= tree.Count) return false;
        return player.evolutionPoints >= tree[EvolutionLevel].epCost;
    }

    public void Evolve()
    {
        if (!CanEvolve()) return;
        var player = GameManager.Instance.Player;
        if (!EvolutionData.Trees.TryGetValue(player.selectedClass, out var tree)) return;
        var node = tree[EvolutionLevel];
        player.evolutionPoints -= node.epCost;
        node.apply?.Invoke(player);
        EvolutionLevel++;
        AudioManager.Instance?.PlaySFX("evolve");
        AudioManager.Instance?.PlaySFX("levelUp");
        LastMessage = $"进化 Lv{EvolutionLevel}: {node.name} — {node.description}";
        AddCombatLog($"进化解锁: {node.name}");

        // 进化联动提示
        int maxLeg = LegacyManager.Instance?.GetMaxLegacies() ?? 2;
        AddCombatLog($"<color=#aaddff>遗产槽位: {maxLeg}</color>");

        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    private int GetEvolutionCost()
    {
        var player = GameManager.Instance.Player;
        if (!EvolutionData.Trees.TryGetValue(player.selectedClass, out var tree)) return 9999;
        if (EvolutionLevel >= tree.Count) return 9999;
        return tree[EvolutionLevel].epCost;
    }

    private void AdvanceFloor()
    {
        AudioManager.Instance?.PlaySFX("floorDown");
        GameManager.Instance.CurrentFloor++;
        int floor = GameManager.Instance.CurrentFloor;
        int maxFloor = GameManager.Instance.MaxFloor;
        MetaProgressSystem.Instance?.OnFloorClear(floor);
        TutorialManager.Instance?.OnFloorEnter(floor);

        // 到达最大层 → 通关
        if (floor > maxFloor)
        {
            GameManager.Instance.CurrentFloor = maxFloor;

            if (GameManager.Instance.CurrentMode == GameMode.Expedition
                && GameManager.Instance.CurrentStage < GameManager.Instance.MaxStage)
            {
                GenerateStageTransition();
                CurrentScreen = RunScreen.StageTransition;
                return;
            }

            GenerateRunReport(true, "通关");
            CurrentScreen = RunScreen.GameOver;
            GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
            return;
        }

        PollutionSystem.Instance.ReducePollution(GetFloorAdvancePollutionDecay());

        var player = GameManager.Instance.Player;

        // 职业每层被动
        ClassAbilityData.ApplyPassivePerFloor(player, EvolutionLevel);

        GenerateFloor(floor);

        var curveParams = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, GameManager.Instance.CurrentFloor, GameManager.Instance.CurrentStage);
        if (curveParams.pollutionPerFloor > 0f)
        {
            PollutionSystem.Instance.GainPollution(curveParams.pollutionPerFloor);
        }

        string waveText = ModeFloorCurves.GetWaveTransitionText(GameManager.Instance.CurrentMode, GameManager.Instance.CurrentFloor);
        LastMessage = waveText != null
            ? $"第 {GameManager.Instance.CurrentFloor} 层 — {waveText}"
            : $"你进入第 {GameManager.Instance.CurrentFloor} 层。";

        if (!string.IsNullOrEmpty(CurrentFloorSignature))
            LastMessage += $" [{CurrentFloorSignature}]";

        AchievementManager.Instance?.UpdateProgress("floor10", GameManager.Instance.CurrentFloor);
        AchievementManager.Instance?.UpdateProgress("floor25", GameManager.Instance.CurrentFloor);
        AchievementManager.Instance?.UpdateProgress("floor50", GameManager.Instance.CurrentFloor);

        // 策略提示检查
        string hint = StrategyHints.Check(player, floor, player.pollution);
        if (!string.IsNullOrEmpty(hint)) AddCombatLog(hint);

        // 自动存档
        SaveSystem.Instance?.AutoSave();

        // 教程触发
        if (player.pollution >= 30f)
        {
            CheckTutorial("pollution");
            if (!IsSoftHintShown("hint_pollution_30"))
            {
                MarkSoftHintShown("hint_pollution_30");
                AddCombatLog("<color=#ff6600>☢ 污染觉醒(30%+)：ATK×1.1 / DEF×1.0。污染越高攻击越强、防御越脆！</color>");
            }
        }

        // JS原版阶段推进: F2→S2, F3→S3(evoHint+ultReady), F5→S4(shop+pollution+signature)
        if (floor >= 2 && TutorialStage < 2) AdvanceTutorialStage(2);
        if (floor >= 2 && TutorialStage >= 2)
        {
            CheckTutorial("defendHint");
            CheckTutorial("inspect");
            CheckTutorial("anchorHint");
        }
        if (floor >= 3 && TutorialStage < 3) { AdvanceTutorialStage(3); CheckTutorial("evoHint"); CheckTutorial("ultReady"); }
        if (floor >= 5 && TutorialStage < 4) { AdvanceTutorialStage(4); CheckTutorial("shop"); CheckTutorial("pollution"); CheckTutorial("signature"); }

        if (GameManager.Instance.CurrentFloor == 1 && TutorialStage == 0)
        {
            CheckTutorial("move");
        }

        if (IsBossFloor(GameManager.Instance.CurrentFloor))
        {
            LastMessage = $"⚠️ BOSS层！";
        }

        // 远征固定抉择事件
        if (GameManager.Instance.CurrentMode == GameMode.Expedition)
        {
            if (floor == 8)
            {
                PendingAltar = new AltarData.AltarPair
                {
                    id = 100, aggressiveName = "☢ 污染注入", aggressiveDesc = "+25%污染，获得一个稀有腐蚀遗产",
                    applyAggressive = p => {
                        PollutionSystem.Instance?.GainPollution(25f);
                        var candidates = LegacyManager.Instance?.ExtractLegaciesFromForm(p.currentFormId);
                        if (candidates != null && candidates.Count > 0)
                        {
                            var leg = candidates[0];
                            leg.rarity = 2;
                            leg.isMutated = true;
                            LegacyManager.Instance.AddLegacy(leg);
                        }
                    },
                    conservativeName = "✦ 净化仪式", conservativeDesc = "-15%污染，所有遗产效果+30%",
                    applyConservative = p => {
                        PollutionSystem.Instance?.ReducePollution(15f);
                    }
                };
                ShowingAltar = true;
                if (!IsSoftHintShown("hint_altar")) { MarkSoftHintShown("hint_altar"); AddCombatLog("<color=#ffcc00>◆ 祭坛抉择：两个选项各有利弊，选择影响本局走向</color>"); }
                LastMessage = "⚡ 远征转折点 — 污染分叉抉择！";
                AddCombatLog("<color=#ffcc00>⚡ 第8层：你面临一个关键抉择...</color>");
            }
            else if (floor == 15)
            {
                PendingAltar = new AltarData.AltarPair
                {
                    id = 101, aggressiveName = "⚔ 全力一搏", aggressiveDesc = "污染→85%，ATK翻倍至Boss战",
                    applyAggressive = p => {
                        p.pollution = 85f;
                        p.attack *= 2;
                        p.baseAttack *= 2;
                    },
                    conservativeName = "🛡 稳健通关", conservativeDesc = "回复50%HP，污染→30%，DEF+50%",
                    applyConservative = p => {
                        p.hp = Mathf.Min(p.maxHp, p.hp + p.maxHp / 2);
                        p.pollution = 30f;
                        p.defense = Mathf.RoundToInt(p.defense * 1.5f);
                        p.baseDefense = Mathf.RoundToInt(p.baseDefense * 1.5f);
                    }
                };
                ShowingAltar = true;
                if (!IsSoftHintShown("hint_altar")) { MarkSoftHintShown("hint_altar"); AddCombatLog("<color=#ffcc00>◆ 祭坛抉择：两个选项各有利弊，选择影响本局走向</color>"); }
                LastMessage = "⚡ 最终抉择 — 决定你的通关策略！";
                AddCombatLog("<color=#ff006e>⚡ 第15层：Boss前的最终抉择...</color>");
            }
        }



        // 短局F3：无遗产时赠送免费遗产
        if (GameManager.Instance.CurrentMode == GameMode.Short && floor == 3
            && LegacyManager.Instance != null && LegacyManager.Instance.GetLegacyCount() == 0)
        {
            var candidates = LegacyManager.Instance.ExtractLegaciesFromForm(player.currentFormId);
            if (candidates != null && candidates.Count > 0)
            {
                AddCombatLog("<color=#ffcc00>⚡ 寄生记忆觉醒！从当前形态中感知到遗产...</color>");
                AddCombatLog("<color=#aaaaff>   遗产是旧形态留下的被动能力，选一个装备</color>");
                PollutionSystem.Instance?.GainPollution(5f);
                EventBus.Emit(EventTypes.LegacySelection, candidates, player.currentFormId);
            }
        }

        // 短局F4：有≥2形态但从未触发共鸣时强制触发保底共鸣
        if (GameManager.Instance.CurrentMode == GameMode.Short && floor == 4
            && player.ownedForms.Count >= 2
            && FormResonanceSystem.Instance != null
            && FormResonanceSystem.Instance.GetAllActiveEffects().Count == 0)
        {
            AddCombatLog("<color=#00ccff>◈ 形态共鸣觉醒！切换形态时触发了共鸣效果！</color>");
            ForceResonanceTrigger();
        }

        EventBus.Emit<int>(EventTypes.FloorEntered, floor);

        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    void ForceResonanceTrigger()
    {
        var player = GameManager.Instance.Player;
        if (player.ownedForms.Count < 2) return;

        string currentForm = player.currentFormId;
        string otherForm = player.ownedForms.FirstOrDefault(f => f != currentForm && f != "human");

        if (!string.IsNullOrEmpty(otherForm))
        {
            AddCombatLog($"<color=#00ccff>◈ 强制共鸣测试：从 {GetDisplayName(currentForm)} 切换到 {GetDisplayName(otherForm)}</color>");
            FormResonanceSystem.Instance?.OnFormSwitch(currentForm, otherForm);
            CanvasUIManager.Instance?.ShowFlashBanner("形态共鸣！切换形态获得增益！", new Color(0f, 0.8f, 1f), 2f);
        }
    }

    private void GenerateStageTransition()
    {
        var player = GameManager.Instance.Player;
        int stage = GameManager.Instance.CurrentStage;
        StageKills = player.totalKillsThisRun;
        StageTime = Time.time - player.runStartTime;

        MetaProgressSystem.Instance?.OnExpeditionStageCleared(stage);

        var pool = new List<ExpeditionRewardOption>(AllExpeditionRewards);
        for (int i = pool.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = pool[i]; pool[i] = pool[j]; pool[j] = tmp;
        }
        PendingStageRewards = new[] { pool[0], pool[1] };

        LastMessage = $"第 {stage} 关 通关！选择跨关奖励";
        AddCombatLog($"<color=#00ffcc>★ 第 {stage} 关通关！选择一项永久奖励进入下一关</color>");
    }

    public void ApplyExpeditionReward(string rewardId)
    {
        var data = GameManager.Instance.ExpeditionData;
        if (data == null) return;

        data.chosenBuffs.Add(rewardId);
        switch (rewardId)
        {
            case "atk": data.permAtkBonus += 3; break;
            case "def": data.permDefBonus += 2; break;
            case "hp":  data.permMaxHpBonus += 15; break;
            case "pol": data.permRegenBonus += 1f; break;
            case "ep":  data.startEpBonus += 20; break;
            case "pos": data.possessRateBonus += 0.05f; break;
        }

        GameManager.Instance.AdvanceExpeditionStage();

        LegacyManager.Instance?.Initialize();
        LegacyManager.Instance?.SubscribeHooks();

        EvolutionLevel = 0;
        _shownSoftHints.Clear();
        CombatLog.Clear();
        LastCombatMessage = "";
        CurrentEnemy = null;
        CurrentFloorState = null;

        int newStage = GameManager.Instance.CurrentStage;
        AddCombatLog($"<color=#00ffcc>══ 第 {newStage} 关开始 ══</color>");
        LastMessage = $"第 {newStage} 关 — 深入裂隙";

        GenerateFloor(1);
        CurrentScreen = RunScreen.Exploration;
        PendingStageRewards = null;

        SaveSystem.Instance?.AutoSave();
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public bool IsBossFloor(int floor)
    {
        int maxFloor = GameManager.Instance.MaxFloor;
        if (floor == maxFloor) return true;
        return floor % 10 == 0;
    }

    public void RevealMonsters()
    {
        if (CurrentFloorState == null) return;
        int revealed = 0;
        foreach (var kvp in CurrentFloorState.actions)
        {
            if (kvp.Value.type == ExploreActionType.Monster && !kvp.Value.consumed)
            {
                var pos = kvp.Key;
                if (pos.y >= 0 && pos.y < CurrentFloorState.discovered.GetLength(0)
                    && pos.x >= 0 && pos.x < CurrentFloorState.discovered.GetLength(1))
                {
                    CurrentFloorState.discovered[pos.y, pos.x] = true;
                    revealed++;
                }
            }
        }
        if (revealed > 0)
            AddCombatLog($"<color=#a040f0>★ 遗产发动：感知到 {revealed} 个敌人的位置</color>");
    }

    private void GenerateFloor(int floor)
    {
        AudioManager.Instance?.PlaySFX("floorUp");
        int zone = GetZoneForFloor(floor, GameManager.Instance.CurrentMode);
        CurrentFloorState = new FloorRuntime
        {
            floor = floor,
            zone = zone,
            discovered = new bool[13, 13],
            walkable = new bool[13, 13],
            playerPos = new Vector2Int(2, 6),
            exitPos = new Vector2Int(10, 6),
            actions = new Dictionary<Vector2Int, ExploreAction>(),
            decorations = new Dictionary<Vector2Int, MapDecoration>(),
            stepsTaken = 0
        };

        _fragDropCount = 0;

        GenerateRoomsAndCorridors(floor);

        CurrentFloorState.walkable[1, 6] = true;
        CurrentFloorState.walkable[11, 6] = true;

        RevealAround(CurrentFloorState.playerPos, 3);
        ApplyFloorSignature();
        SeedFloorActions(zone, floor);
        GenerateDecorations(zone, floor);
        EventBus.Emit(EventTypes.FloorChanged, floor);
        GameManager.Instance.NotifyPlayerStatsChanged();

        // Call special floor system
        SpecialFloorSystem.Instance?.AssignSpecialFloor(floor);
        SpecialFloorSystem.Instance?.OnEnterFloor(floor);

        // 短模式楼层类型逻辑
        if (GameManager.Instance.CurrentMode == GameMode.Short)
        {
            var floorType = ModeFloorCurves.GetShortModeFloorType(floor);
            if (floorType == ModeFloorCurves.FloorType.Rest)
            {
                SpecialFloorSystem.Instance?.ForceSpecialFloor(floor, SpecialFloorSystem.SpecialFloorType.Rest);
            }
        }

        // Check for story events
        StorySystem.Instance?.CheckStoryTrigger(floor);
        StorySystem.Instance?.CheckHiddenStory("floor_enter");

        // Check for anchor system
        AnchorSystem.Instance?.OnFloorEnter(floor);
    }

    private void GenerateRoomsAndCorridors(int floor)
    {
        // Initialize all tiles: border = wall, inner = floor
        for (int y = 0; y < 13; y++)
        {
            for (int x = 0; x < 13; x++)
            {
                if (x == 0 || y == 0 || x == 12 || y == 12)
                    CurrentFloorState.walkable[y, x] = false;
                else
                    CurrentFloorState.walkable[y, x] = true;
            }
        }

        // Random inner walls (3-6 walls)
        int wallCount = 3 + UnityEngine.Random.Range(0, 4);
        
        // Protected zones: spawn point, stairs, and their adjacent cells
        HashSet<string> protectedCells = new HashSet<string>();
        int[][] protectedPositions = new int[][] { new int[] { 2, 6 }, new int[] { 10, 6 } };
        foreach (var pos in protectedPositions)
        {
            int px = pos[0], py = pos[1];
            for (int ddx = -1; ddx <= 1; ddx++)
            {
                for (int ddy = -1; ddy <= 1; ddy++)
                {
                    if (Mathf.Abs(ddx) + Mathf.Abs(ddy) <= 1)
                    {
                        protectedCells.Add((px + ddx) + "," + (py + ddy));
                    }
                }
            }
        }

        for (int w = 0; w < wallCount; w++)
        {
            int wx = 2 + UnityEngine.Random.Range(0, 9);
            int wy = 2 + UnityEngine.Random.Range(0, 9);
            if (protectedCells.Contains(wx + "," + wy)) continue;
            CurrentFloorState.walkable[wy, wx] = false;
        }

        // Ensure connectivity
        EnsureConnectivity();
    }

    private void EnsureConnectivity()
    {
        HashSet<string> reachable = FloodFill(6, 6);

        // Ensure stairs are reachable
        int[][] targets = new int[][] { new int[] { 10, 6 }, new int[] { 2, 6 } };
        foreach (var target in targets)
        {
            int tx = target[0], ty = target[1];
            if (reachable.Contains(tx + "," + ty)) continue;
            
            // Punch through corridor along y=6
            int x0 = Mathf.Min(6, tx);
            int x1 = Mathf.Max(6, tx);
            for (int cx = x0; cx <= x1; cx++)
            {
                if (!CurrentFloorState.walkable[6, cx])
                {
                    CurrentFloorState.walkable[6, cx] = true;
                    reachable.Add(cx + ",6");
                }
            }
        }

        // Convert unreachable isolated cells to walls
        for (int yy = 1; yy <= 11; yy++)
        {
            for (int xx = 1; xx <= 11; xx++)
            {
                if (CurrentFloorState.walkable[yy, xx] && !reachable.Contains(xx + "," + yy))
                {
                    CurrentFloorState.walkable[yy, xx] = false;
                }
            }
        }
    }

    private HashSet<string> FloodFill(int sx, int sy)
    {
        HashSet<string> visited = new HashSet<string>();
        Queue<int[]> queue = new Queue<int[]>();
        queue.Enqueue(new int[] { sx, sy });
        visited.Add(sx + "," + sy);

        int[][] directions = new int[][] { new int[] { 0, 1 }, new int[] { 0, -1 }, new int[] { 1, 0 }, new int[] { -1, 0 } };

        while (queue.Count > 0)
        {
            int[] current = queue.Dequeue();
            int cx = current[0], cy = current[1];

            foreach (var dir in directions)
            {
                int nx = cx + dir[0], ny = cy + dir[1];
                string key = nx + "," + ny;

                if (nx >= 1 && ny >= 1 && nx <= 11 && ny <= 11 && 
                    !visited.Contains(key) && CurrentFloorState.walkable[ny, nx])
                {
                    visited.Add(key);
                    queue.Enqueue(new int[] { nx, ny });
                }
            }
        }

        return visited;
    }

    private Vector2Int PickOpenCell(HashSet<Vector2Int> used)
    {
        for (int attempts = 0; attempts < 100; attempts++)
        {
            int x = UnityEngine.Random.Range(2, 11);
            int y = UnityEngine.Random.Range(2, 11);
            var pos = new Vector2Int(x, y);
            if (CurrentFloorState.walkable[y, x] && !used.Contains(pos))
                return pos;
        }
        return new Vector2Int(5, 5);
    }

    private void GenerateDecorations(int zone, int floor)
    {
        var floorState = CurrentFloorState;
        if (floorState == null || floorState.decorations == null) return;

        int decorCount = 6 + UnityEngine.Random.Range(0, 5);
        
        for (int i = 0; i < decorCount; i++)
        {
            int x, y;
            if (i < 3)
            {
                x = floorState.playerPos.x + UnityEngine.Random.Range(-4, 5);
                y = floorState.playerPos.y + UnityEngine.Random.Range(-4, 5);
            }
            else
            {
                x = UnityEngine.Random.Range(1, 12);
                y = UnityEngine.Random.Range(1, 12);
            }
            
            x = Mathf.Clamp(x, 1, 11);
            y = Mathf.Clamp(y, 1, 11);
            
            var pos = new Vector2Int(x, y);
            if (!floorState.walkable[y, x]) continue;
            if (floorState.actions.ContainsKey(pos)) continue;
            if (floorState.decorations.ContainsKey(pos)) continue;
            if (floorState.exitPos == pos) continue;
            if (floorState.playerPos == pos) continue;
            
            bool tooClose = false;
            foreach (var existingPos in floorState.decorations.Keys)
            {
                if (Mathf.Abs(existingPos.x - pos.x) + Mathf.Abs(existingPos.y - pos.y) < 3)
                {
                    tooClose = true;
                    break;
                }
            }
            if (tooClose) continue;
            
            MapDecoration decor = GenerateRandomDecoration(zone, floor);
            floorState.decorations[pos] = decor;
        }
        
        GenerateRiver(zone);
    }
    


    private MapDecoration GenerateRandomDecoration(int zone, int floor)
    {
        float rand = UnityEngine.Random.value;
        DecorationType type;
        string icon;
        Color color;
        bool isAnimated;
        float animSpeed;

        float pollution = GameManager.Instance.Player?.pollution ?? 0f;
        float pollFactor = pollution / 100f;

        switch (zone)
        {
            case 1:
                if (rand < 0.2f)
                {
                    type = DecorationType.Mountain;
                    icon = "▲";
                    color = new Color(0.7f, 0.6f, 0.5f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else if (rand < 0.35f)
                {
                    type = DecorationType.River;
                    icon = "~";
                    color = new Color(0.3f, 0.5f, 0.9f);
                    isAnimated = true;
                    animSpeed = 2f;
                }
                else if (rand < 0.55f)
                {
                    type = DecorationType.BrokenMachine;
                    icon = "@";
                    color = new Color(0.8f, 0.8f, 0.9f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else if (rand < 0.75f)
                {
                    type = DecorationType.Growth;
                    icon = "+";
                    color = new Color(0.3f, 0.9f, 0.5f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else
                {
                    type = DecorationType.Hologram;
                    icon = "*";
                    color = new Color(0.4f, 0.85f, 1f);
                    isAnimated = true;
                    animSpeed = 2.5f;
                }
                break;

            case 2:
                if (rand < 0.2f)
                {
                    type = DecorationType.Mountain;
                    icon = "▲";
                    color = new Color(0.5f, 0.5f, 0.4f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else if (rand < 0.35f)
                {
                    type = DecorationType.River;
                    icon = "~";
                    color = new Color(0.25f, 0.45f, 0.85f);
                    isAnimated = true;
                    animSpeed = 2f;
                }
                else if (rand < 0.5f)
                {
                    type = DecorationType.Fungus;
                    icon = "%";
                    color = new Color(0.9f, 0.4f, 0.95f);
                    isAnimated = true;
                    animSpeed = 1.8f;
                }
                else if (rand < 0.65f)
                {
                    type = DecorationType.Bioluminescence;
                    icon = "^";
                    color = new Color(0.3f, 1f, 0.75f);
                    isAnimated = true;
                    animSpeed = 3.5f;
                }
                else if (rand < 0.85f)
                {
                    type = DecorationType.Growth;
                    icon = "&";
                    color = new Color(0.25f, 0.95f, 0.5f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else
                {
                    type = DecorationType.ToxicPuddle;
                    icon = "$";
                    color = new Color(0.4f, 0.9f, 0.35f);
                    isAnimated = true;
                    animSpeed = 1.5f;
                }
                break;

            case 3:
                if (rand < 0.2f)
                {
                    type = DecorationType.Mountain;
                    icon = "▲";
                    color = new Color(0.6f, 0.5f, 0.45f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else if (rand < 0.35f)
                {
                    type = DecorationType.River;
                    icon = "~";
                    color = new Color(0.2f, 0.4f, 0.8f);
                    isAnimated = true;
                    animSpeed = 2f;
                }
                else if (rand < 0.5f)
                {
                    type = DecorationType.Crystal;
                    icon = "^";
                    color = new Color(1f, 0.5f, 0.95f);
                    isAnimated = true;
                    animSpeed = 3f;
                }
                else if (rand < 0.65f)
                {
                    type = DecorationType.ToxicPuddle;
                    icon = "$";
                    color = new Color(0.5f, 0.95f, 0.3f);
                    isAnimated = true;
                    animSpeed = 1.2f;
                }
                else if (rand < 0.8f)
                {
                    type = DecorationType.Fungus;
                    icon = "%";
                    color = new Color(1f, 0.3f, 0.7f);
                    isAnimated = true;
                    animSpeed = 2f;
                }
                else
                {
                    type = DecorationType.Bioluminescence;
                    icon = "^";
                    color = new Color(0.5f, 0.85f, 1f);
                    isAnimated = true;
                    animSpeed = 4f;
                }
                break;

            case 4:
                if (rand < 0.2f)
                {
                    type = DecorationType.Mountain;
                    icon = "▲";
                    color = new Color(0.55f, 0.5f, 0.4f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else if (rand < 0.35f)
                {
                    type = DecorationType.River;
                    icon = "~";
                    color = new Color(0.35f, 0.55f, 0.95f);
                    isAnimated = true;
                    animSpeed = 2f;
                }
                else if (rand < 0.55f)
                {
                    type = DecorationType.Hologram;
                    icon = "*";
                    color = new Color(0.95f, 0.6f, 1f);
                    isAnimated = true;
                    animSpeed = 3f;
                }
                else if (rand < 0.75f)
                {
                    type = DecorationType.BrokenMachine;
                    icon = "@";
                    color = new Color(0.65f, 0.7f, 0.9f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else
                {
                    type = DecorationType.Crystal;
                    icon = "^";
                    color = new Color(0.5f, 0.8f, 1f);
                    isAnimated = true;
                    animSpeed = 2.5f;
                }
                break;

            case 5:
            default:
                if (rand < 0.2f)
                {
                    type = DecorationType.Mountain;
                    icon = "▲";
                    color = new Color(0.7f, 0.6f, 0.55f);
                    isAnimated = false;
                    animSpeed = 0;
                }
                else if (rand < 0.35f)
                {
                    type = DecorationType.River;
                    icon = "~";
                    color = new Color(0.4f, 0.6f, 1f);
                    isAnimated = true;
                    animSpeed = 2f;
                }
                else if (rand < 0.55f)
                {
                    type = DecorationType.Crystal;
                    icon = "^";
                    color = new Color(1f, 0.6f, 0.9f);
                    isAnimated = true;
                    animSpeed = 3.5f;
                }
                else if (rand < 0.75f)
                {
                    type = DecorationType.Hologram;
                    icon = "*";
                    color = new Color(0.9f, 0.8f, 1f);
                    isAnimated = true;
                    animSpeed = 3.2f;
                }
                else
                {
                    type = DecorationType.Bioluminescence;
                    icon = "^";
                    color = new Color(0.6f, 0.95f, 1f);
                    isAnimated = true;
                    animSpeed = 4.5f;
                }
                break;
        }

        if (pollFactor > 0.5f)
        {
            color.r += (1 - color.r) * pollFactor * 0.3f;
            color.g -= color.g * pollFactor * 0.2f;
        }

        return new MapDecoration
        {
            type = type,
            icon = icon,
            color = color,
            isAnimated = isAnimated,
            animationSpeed = animSpeed
        };
    }

    private void GenerateRiver(int zone)
    {
        var floorState = CurrentFloorState;
        if (floorState == null) return;

        if (UnityEngine.Random.value > 0.15f) return;

        bool horizontal = UnityEngine.Random.value > 0.5f;
        int fixedCoord = UnityEngine.Random.Range(4, 9);
        int start = UnityEngine.Random.Range(3, 7);
        int end = UnityEngine.Random.Range(start + 2, start + 5);

        for (int i = start; i <= end; i++)
        {
            int x = horizontal ? i : fixedCoord;
            int y = horizontal ? fixedCoord : i;

            var pos = new Vector2Int(x, y);
            if (!floorState.walkable[y, x]) continue;
            if (floorState.actions.ContainsKey(pos)) continue;
            if (floorState.decorations.ContainsKey(pos)) continue;
            if (floorState.exitPos == pos) continue;
            if (floorState.playerPos == pos) continue;

            floorState.decorations[pos] = new MapDecoration
            {
                type = DecorationType.River,
                icon = "~",
                color = new Color(0.3f, 0.6f, 1f),
                isAnimated = true,
                animationSpeed = 2f
            };
        }
    }

    private Vector2Int FindNearbyOpenCell(Vector2Int center)
    {
        // 在玩家周围1-2格找空位放碎片
        int[] dx = {1, -1, 0, 0, 1, -1, 1, -1, 2, -2, 0, 0};
        int[] dy = {0, 0, 1, -1, 1, -1, -1, 1, 0, 0, 2, -2};
        for (int i = 0; i < dx.Length; i++)
        {
            int nx = center.x + dx[i];
            int ny = center.y + dy[i];
            if (nx < 1 || nx > 11 || ny < 1 || ny > 11) continue;
            if (!CurrentFloorState.walkable[ny, nx]) continue;
            var pos = new Vector2Int(nx, ny);
            if (!CurrentFloorState.actions.ContainsKey(pos) || CurrentFloorState.actions[pos].consumed)
                return pos;
        }
        return center;
    }

    private void SeedFloorActions(int zone, int floor)
    {
        var used = new HashSet<Vector2Int> { CurrentFloorState.playerPos, CurrentFloorState.exitPos };

        if (floor != 1)
        {
            int preExitX = CurrentFloorState.exitPos.x;
            int preExitY = CurrentFloorState.exitPos.y;
            var preGuardPositions = new Vector2Int[]
            {
                new Vector2Int(preExitX - 1, preExitY),
                new Vector2Int(preExitX, preExitY - 1),
                new Vector2Int(preExitX, preExitY + 1)
            };
            foreach (var gp in preGuardPositions)
            {
                if (gp.x >= 1 && gp.x <= 11 && gp.y >= 1 && gp.y <= 11)
                    used.Add(gp);
            }
        }

        if (floor == 1)
        {
            IsInTutorial = true;
            SeedTutorialFloor(used);
            return;
        }

        IsInTutorial = false;

        int exitX = CurrentFloorState.exitPos.x;
        int exitY = CurrentFloorState.exitPos.y;

        // 1. 怪物（原版：5+floor(floor/5)，上限15）
        int monsterCount = GetMonsterCountForModeFloor(floor, GameManager.Instance.CurrentMode);
        bool bossFloor = ModeFloorCurves.IsBossFloor(GameManager.Instance.CurrentMode, floor, GameManager.Instance.MaxFloor);

        for (int i = 0; i < monsterCount; i++)
        {
            Vector2Int pos = PickOpenCell(used);
            bool isBoss = bossFloor && i == monsterCount - 1;
            var monster = CreateMonsterForFloor(zone, floor, isBoss);
            InitMonsterAI(monster, pos);
            CurrentFloorState.actions[pos] = new ExploreAction
            {
                type = ExploreActionType.Monster,
                title = monster.name,
                description = string.Join(" / ", monster.traits ?? Array.Empty<string>()),
                monster = monster
            };
            used.Add(pos);
        }

        // 短局F2保底：插入一个zone2强敌作为附身目标
        if (GameManager.Instance.CurrentMode == GameMode.Short && floor == 2)
        {
            var zone2Pool = GameDataImporter.MonsterDefinitions
                .Where(m => m.zone == 2 && !m.boss).ToList();
            if (zone2Pool.Count > 0)
            {
                var pick = zone2Pool[UnityEngine.Random.Range(0, zone2Pool.Count)];
                var curve = ModeFloorCurves.GetParams(GameMode.Short, floor);
                float rnd = 0.8f + UnityEngine.Random.value * 0.4f;
                Vector2Int pos = PickOpenCell(used);
                var elite = new MonsterRuntime
                {
                    id = pick.id, name = $"★{pick.name}",
                    hp = Mathf.Max(20, Mathf.RoundToInt(pick.hp * curve.hpMult * rnd)),
                    maxHp = Mathf.Max(20, Mathf.RoundToInt(pick.hp * curve.hpMult * rnd)),
                    atk = Mathf.Max(3, Mathf.RoundToInt(pick.atk * curve.atkMult * rnd)),
                    def = Mathf.Max(1, pick.def),
                    zone = pick.zone, isBoss = false, traits = pick.traits, axes = pick.axes,
                    color = new Color(1f, 0.85f, 0.2f),
                    possessBaseChance = 0.75f
                };
                InitMonsterAI(elite, pos);
                CurrentFloorState.actions[pos] = new ExploreAction
                {
                    type = ExploreActionType.Monster, title = elite.name,
                    description = "强力目标 — 值得附身", monster = elite
                };
                used.Add(pos);
                
                // 首局F2：附身后安排一场爽战——用较弱敌人让玩家体验新身体的强大
                bool isFirstRun = PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
                if (isFirstRun)
                {
                    var easyPool = GameDataImporter.MonsterDefinitions
                        .Where(m => m.zone == 1 && !m.boss && m.id != "dog").ToList();
                    if (easyPool.Count > 0)
                    {
                        var easyPick = easyPool[UnityEngine.Random.Range(0, easyPool.Count)];
                        Vector2Int easyPos = PickOpenCell(used);
                        var easyMonster = new MonsterRuntime
                        {
                            id = easyPick.id, name = $"◇{easyPick.name}",
                            hp = Mathf.Max(15, Mathf.RoundToInt(easyPick.hp * 0.5f)),
                            maxHp = Mathf.Max(15, Mathf.RoundToInt(easyPick.hp * 0.5f)),
                            atk = Mathf.Max(3, Mathf.RoundToInt(easyPick.atk * 0.6f)),
                            def = Mathf.Max(1, Mathf.RoundToInt(easyPick.def * 0.5f)),
                            zone = easyPick.zone, isBoss = false, traits = easyPick.traits, 
                            axes = easyPick.axes,
                            color = new Color(0.6f, 0.8f, 1f),
                            possessBaseChance = 0.4f
                        };
                        InitMonsterAI(easyMonster, easyPos);
                        CurrentFloorState.actions[easyPos] = new ExploreAction
                        {
                            type = ExploreActionType.Monster, title = easyMonster.name,
                            description = "弱小敌人 — 测试新身体", monster = easyMonster
                        };
                        used.Add(easyPos);
                    }
                }
            }
        }

        // 2. 楼梯守卫（原版逻辑：从已生成的怪物中选择离楼梯最近的作为守卫）
        // 守卫数量：F1:3个, F2-F5:1个, F6-F10:2个, F11+:3个（上限）
        int wantGuards = floor == 1 ? 3 : Mathf.Min(3, 1 + floor / 5);
        var guardSpots = new Vector2Int[]
        {
            new Vector2Int(exitX - 1, exitY),      // (9,6)
            new Vector2Int(exitX, exitY - 1),      // (10,5)
            new Vector2Int(exitX, exitY + 1),      // (10,7)
            new Vector2Int(exitX - 1, exitY - 1),  // (9,5)
            new Vector2Int(exitX - 1, exitY + 1),  // (9,7)
            new Vector2Int(exitX - 2, exitY),      // (8,6)
            new Vector2Int(exitX - 2, exitY - 1),  // (8,5)
            new Vector2Int(exitX - 2, exitY + 1)   // (8,7)
        };
        
        // 收集所有非Boss、非守卫的怪物，按离楼梯距离排序
        var guardCandidates = new List<Tuple<Vector2Int, MonsterRuntime>>();
        foreach (var kvp in CurrentFloorState.actions)
        {
            if (kvp.Value.type == ExploreActionType.Monster && 
                kvp.Value.monster != null && 
                !kvp.Value.monster.isBoss && 
                !kvp.Value.monster.stairGuard)
            {
                guardCandidates.Add(Tuple.Create(kvp.Key, kvp.Value.monster));
            }
        }
        guardCandidates.Sort((a, b) => 
        {
            int distA = Mathf.Abs(a.Item1.x - exitX) + Mathf.Abs(a.Item1.y - exitY);
            int distB = Mathf.Abs(b.Item1.x - exitX) + Mathf.Abs(b.Item1.y - exitY);
            return distA.CompareTo(distB);
        });

        int guardsAssigned = 0;
        int spotUsed = 0;
        foreach (var candidate in guardCandidates)
        {
            if (guardsAssigned >= wantGuards) break;

            Vector2Int monsterPos = candidate.Item1;
            MonsterRuntime monster = candidate.Item2;

            // 为守卫找一个楼梯附近的空位
            Vector2Int? assignedSpot = null;
            for (int si = spotUsed; si < guardSpots.Length; si++)
            {
                Vector2Int spot = guardSpots[si];
                if (spot.x < 1 || spot.x > 11 || spot.y < 1 || spot.y > 11) continue;
                if (!CurrentFloorState.walkable[spot.y, spot.x]) continue;
                
                bool occupied = false;
                foreach (var kvp in CurrentFloorState.actions)
                {
                    if (kvp.Key == spot) { occupied = true; break; }
                }
                if (!occupied)
                {
                    assignedSpot = spot;
                    spotUsed = si + 1;
                    break;
                }
            }

            if (assignedSpot.HasValue)
                {
                    // 从原位置移除怪物
                    CurrentFloorState.actions.Remove(monsterPos);
                    used.Remove(monsterPos);

                    // 将怪物移动到守卫位置
                    monster.stairGuard = true;
                    monster.name = $"🛡{monster.name}";
                    
                    InitMonsterAI(monster, assignedSpot.Value);
                    
                    // 在 InitMonsterAI 之后升级属性（原版逻辑：主动攻击、警戒等级提升、检测范围扩大）
                    // 必须在 InitMonsterAI 之后设置，因为 InitMonsterAI 会覆盖这些值
                    monster.aiType = MonsterAIType.Aggressive;
                    monster.alertLevel = 2;
                    monster.detectRange = Mathf.Max(monster.detectRange, 6);
                    
                    CurrentFloorState.actions[assignedSpot.Value] = new ExploreAction
                    {
                        type = ExploreActionType.Monster,
                        title = monster.name,
                        description = "楼梯守卫",
                        monster = monster
                    };
                    used.Add(assignedSpot.Value);
                    guardsAssigned++;
                }
        }

        // 如果没有足够的候选怪物，直接生成守卫（原版回退逻辑）
        if (guardsAssigned < wantGuards)
        {
            var guardTypes = GameDataImporter.MonsterDefinitions
                .Where(m => m.zone == zone && !m.boss)
                .Select(m => m.id).ToList();
            if (guardTypes.Count == 0)
                guardTypes = GameDataImporter.MonsterDefinitions.Where(m => !m.boss).Select(m => m.id).ToList();

            for (int i = guardsAssigned; i < wantGuards; i++)
            {
                Vector2Int? assignedSpot = null;
                for (int si = spotUsed; si < guardSpots.Length; si++)
                {
                    Vector2Int spot = guardSpots[si];
                    if (spot.x < 1 || spot.x > 11 || spot.y < 1 || spot.y > 11) continue;
                    if (!CurrentFloorState.walkable[spot.y, spot.x]) CurrentFloorState.walkable[spot.y, spot.x] = true;
                    
                    bool occupied = false;
                    foreach (var kvp in CurrentFloorState.actions)
                    {
                        if (kvp.Key == spot) { occupied = true; break; }
                    }
                    if (!occupied)
                    {
                        assignedSpot = spot;
                        spotUsed = si + 1;
                        break;
                    }
                }

                if (assignedSpot.HasValue)
                {
                    string gid = guardTypes[i % guardTypes.Count];
                    var gdef = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == gid);
                    if (string.IsNullOrEmpty(gdef.id)) continue;

                    var curveP = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, floor, GameManager.Instance.CurrentStage);
                    var guard = new MonsterRuntime
                        {
                            id = gid,
                            name = $"🛡{gdef.name}",
                            hp = Mathf.RoundToInt(gdef.hp * curveP.hpMult * 0.7f),
                            maxHp = Mathf.RoundToInt(gdef.hp * curveP.hpMult * 0.7f),
                            atk = Mathf.RoundToInt(gdef.atk * curveP.atkMult * 0.8f),
                            def = gdef.def,
                            zone = gdef.zone,
                            isBoss = false,
                            traits = gdef.traits,
                            color = gdef.color,
                            possessBaseChance = 0.6f,
                            stairGuard = true
                        };
                    InitMonsterAI(guard, assignedSpot.Value);
                    
                    // 在 InitMonsterAI 之后升级属性（原版逻辑：主动攻击、警戒等级提升、检测范围扩大）
                    // 必须在 InitMonsterAI 之后设置，因为 InitMonsterAI 会覆盖这些值
                    guard.aiType = MonsterAIType.Aggressive;
                    guard.alertLevel = 2;
                    guard.detectRange = Mathf.Max(guard.detectRange, 6);
                    CurrentFloorState.actions[assignedSpot.Value] = new ExploreAction
                    {
                        type = ExploreActionType.Monster,
                        title = guard.name,
                        description = "楼梯守卫",
                        monster = guard
                    };
                    used.Add(assignedSpot.Value);
                    guardsAssigned++;
                }
            }
        }

        if (guardsAssigned > 0 && !IsSoftHintShown("hint_guard"))
        {
            MarkSoftHintShown("hint_guard");
            AddCombatLog("<color=#ffcc00>◆ 击败楼梯守卫才能进入下一层</color>");
        }

        // 3. 祭坛（原版：每5层，非Boss层，40%概率位置随机）
        if (floor % 5 == 0 && !bossFloor)
        {
            Vector2Int altarPos = PickOpenCell(used);
            CurrentFloorState.actions[altarPos] = new ExploreAction
            {
                type = ExploreActionType.Event,
                title = "⛯ 祭坛",
                description = "净化/诅咒/祝福"
            };
            used.Add(altarPos);
        }

        // 4. 事件房（原版：40%概率）
        if (UnityEngine.Random.value < 0.4f && !bossFloor)
        {
            Vector2Int eventPos = PickOpenCell(used);
            CurrentFloorState.actions[eventPos] = new ExploreAction
            {
                type = ExploreActionType.Event,
                title = "异质事件",
                description = "菌膜、残响或污染结晶"
            };
            used.Add(eventPos);
        }

        // 5. 资源房（原版：50%概率）
        if (UnityEngine.Random.value < 0.5f && !bossFloor)
        {
            Vector2Int resPos = PickOpenCell(used);
            CurrentFloorState.actions[resPos] = new ExploreAction
            {
                type = ExploreActionType.Event,
                title = "资源室",
                description = "EP或金币"
            };
            used.Add(resPos);
        }

        // 7. 特殊楼层图块 (由 SpecialFloorSystem 分配的 Rest/Treasure/Trap/Mystery/BossRush)
        var sfType = SpecialFloorSystem.Instance?.GetSpecialFloorType(floor) ?? SpecialFloorSystem.SpecialFloorType.None;
        if (sfType == SpecialFloorSystem.SpecialFloorType.Rest
            || sfType == SpecialFloorSystem.SpecialFloorType.Treasure
            || sfType == SpecialFloorSystem.SpecialFloorType.Trap
            || sfType == SpecialFloorSystem.SpecialFloorType.Mystery)
        {
            Vector2Int sfPos = PickOpenCell(used);
            CurrentFloorState.actions[sfPos] = new ExploreAction
            {
                type = ExploreActionType.Event,
                title = SpecialFloorSystem.Instance.GetSpecialFloorName(floor),
                description = ""
            };
            used.Add(sfPos);
        }
        else if (sfType == SpecialFloorSystem.SpecialFloorType.BossRush)
        {
            Vector2Int brPos = PickOpenCell(used);
            CurrentFloorState.actions[brPos] = new ExploreAction
            {
                type = ExploreActionType.Event,
                title = "⚔️ 波次挑战",
                description = "连续击败多波敌人"
            };
            used.Add(brPos);
        }

        // 8. 签名对怪物的即时效果（原版 onEnter 直接改怪物属性）
        if (ActiveSignature != null)
        {
            // 单挑：选ATK最高的怪物，其他全移除，属性×3
            if (ActiveSignature.isDuel)
            {
                MonsterRuntime best = null;
                Vector2Int bestPos = Vector2Int.zero;
                var toRemove = new List<Vector2Int>();
                foreach (var kvp in CurrentFloorState.actions)
                {
                    if (kvp.Value.type == ExploreActionType.Monster && kvp.Value.monster != null)
                    {
                        if (best == null || kvp.Value.monster.atk > best.atk)
                        {
                            if (best != null) toRemove.Add(bestPos);
                            best = kvp.Value.monster;
                            bestPos = kvp.Key;
                        }
                        else
                        {
                            toRemove.Add(kvp.Key);
                        }
                    }
                }
                foreach (var pos in toRemove)
                {
                    CurrentFloorState.actions[pos] = new ExploreAction { consumed = true };
                }
                if (best != null)
                {
                    best.maxHp *= 3; best.hp = best.maxHp; best.atk *= 3; best.def *= 3;
                    best.name = "精英 " + best.name; best._elite = true;
                    best.color = new Color(1f, 0.3f, 0.1f);
                }
            }

            // 巨人化：原版 onEnter 直接改活怪物的 HP×2 和 ATK×1.5
            if (ActiveSignature.hpMult > 0 || ActiveSignature.atkMult > 0)
            {
                float hm = ActiveSignature.hpMult > 0 ? ActiveSignature.hpMult : 1f;
                float am = ActiveSignature.atkMult > 0 ? ActiveSignature.atkMult : 1f;
                if (hm != 1f || am != 1f)
                {
                    foreach (var kvp in CurrentFloorState.actions)
                    {
                        if (kvp.Value.type == ExploreActionType.Monster && kvp.Value.monster != null)
                        {
                            var m = kvp.Value.monster;
                            m.maxHp = Mathf.RoundToInt(m.maxHp * hm);
                            m.hp = Mathf.RoundToInt(m.hp * hm);
                            m.atk = Mathf.FloorToInt(m.atk * am);
                        }
                    }
                }
            }

            // 毒雾/污染风暴：怪物 ATK 乘以系数
            if (ActiveSignature.monsterAtkMult > 0 && ActiveSignature.monsterAtkMult < 1f)
            {
                foreach (var kvp in CurrentFloorState.actions)
                {
                    if (kvp.Value.type == ExploreActionType.Monster && kvp.Value.monster != null)
                        kvp.Value.monster.atk = Mathf.Max(1, Mathf.FloorToInt(kvp.Value.monster.atk * ActiveSignature.monsterAtkMult));
                }
            }

            // 小人国：怪物 HP÷2，数量翻倍
            if (ActiveSignature.monsterHpHalf)
            {
                // 先 HP÷2
                foreach (var kvp in CurrentFloorState.actions)
                {
                    if (kvp.Value.type == ExploreActionType.Monster && kvp.Value.monster != null)
                    {
                        kvp.Value.monster.maxHp = Mathf.Max(1, kvp.Value.monster.maxHp / 2);
                        kvp.Value.monster.hp = Mathf.Min(kvp.Value.monster.hp, kvp.Value.monster.maxHp);
                    }
                }
                // 克隆怪物
                var clones = new List<KeyValuePair<Vector2Int, ExploreAction>>();
                foreach (var kvp in CurrentFloorState.actions)
                {
                    if (kvp.Value.type == ExploreActionType.Monster && kvp.Value.monster != null && !kvp.Value.monster.isBoss)
                    {
                        Vector2Int clonePos = PickOpenCell(used);
                        var orig = kvp.Value.monster;
                        var clone = new MonsterRuntime
                        {
                            id = orig.id + "_clone", name = orig.name, hp = orig.hp, maxHp = orig.maxHp,
                            atk = orig.atk, def = orig.def, zone = orig.zone, isBoss = false,
                            traits = orig.traits, axes = orig.axes, color = orig.color,
                            possessBaseChance = orig.possessBaseChance, _elite = orig._elite,
                            _ambush = false, _possessWindowUsed = false, _breakWindowDone = false
                        };
                        clones.Add(new KeyValuePair<Vector2Int, ExploreAction>(clonePos, new ExploreAction
                        {
                            type = ExploreActionType.Monster, title = clone.name,
                            description = string.Join(" / ", clone.traits ?? Array.Empty<string>()), monster = clone
                        }));
                        used.Add(clonePos);
                    }
                }
                foreach (var c in clones) CurrentFloorState.actions[c.Key] = c.Value;
            }

            // 猎杀令：选定悬赏目标
            if (ActiveSignature.id == "bountyHunt")
            {
                var alive = new List<MonsterRuntime>();
                foreach (var kvp in CurrentFloorState.actions)
                    if (kvp.Value.type == ExploreActionType.Monster && kvp.Value.monster != null && !kvp.Value.monster.isBoss)
                        alive.Add(kvp.Value.monster);
                if (alive.Count > 0)
                {
                    var target = alive[UnityEngine.Random.Range(0, alive.Count)];
                    BountyTargetId = target.id;
                    BountyTargetName = target.name;
                    AddCombatLog($"◎ 猎杀令: 击杀 {target.name} 奖励500EP!");
                }
            }

            // 审判：按ATK从弱到强排序
            if (ActiveSignature.id == "judgment")
            {
                var alive = new List<MonsterRuntime>();
                foreach (var kvp in CurrentFloorState.actions)
                    if (kvp.Value.type == ExploreActionType.Monster && kvp.Value.monster != null)
                        alive.Add(kvp.Value.monster);
                alive.Sort((a, b) => a.atk.CompareTo(b.atk));
                _judgmentKillOrder = new List<string>();
                foreach (var m in alive) _judgmentKillOrder.Add(m.id);
                _judgmentKillIdx = 0;
                if (alive.Count > 0) AddCombatLog("⚖ 审判: 从弱到强击杀可获得额外EP!");
            }
        }
    }

    private void SeedTutorialFloor(HashSet<Vector2Int> used)
    {
        // 教程走廊：玩家(6,6) -> 楼梯(10,6)
        int playerY = CurrentFloorState.playerPos.y;
        int exitX = CurrentFloorState.exitPos.x;
        int playerX = CurrentFloorState.playerPos.x;

        // 确保走廊是通的（沿y=6的水平通道）
        for (int x = playerX + 1; x <= exitX; x++)
        {
            CurrentFloorState.walkable[playerY, x] = true;
        }

        // 扫描玩家东侧可走的路径
        List<int> path = new List<int>();
        for (int cx = playerX + 1; cx < 13; cx++)
        {
            if (CurrentFloorState.walkable[playerY, cx])
                path.Add(cx);
        }

        // 1. 虚弱实验鼠 - 路径首格（紧邻玩家，先打一只学操作）
        int ratX = path.Count > 0 ? path[0] : playerX + 1;
        Vector2Int ratPos = new Vector2Int(ratX, playerY);
        
        // 强制鼠位置为地板
        CurrentFloorState.walkable[ratPos.y, ratPos.x] = true;
        
        var rat = CreateTutorialMonster("rat", 15, 15, -5, -1, "#8b4513");
        CurrentFloorState.actions[ratPos] = new ExploreAction
        {
            type = ExploreActionType.Monster,
            title = rat.name,
            description = string.Join(" / ", rat.traits ?? Array.Empty<string>()),
            monster = rat
        };
        used.Add(ratPos);

        // 2. 看门犬 - 路径中段，附身后玩家在此位置，需向东打通楼梯口
        // 确保狗在鼠的后面，且距离至少2格，同时不超过exitX-2（给守卫留位置）
        int minDogX = ratX + 2;
        int maxDogX = exitX - 2;
        int dogX = Math.Clamp(minDogX + 1, minDogX, maxDogX);
        
        Vector2Int dogPos = new Vector2Int(dogX, playerY);
        
        // 强制狗位置为地板
        CurrentFloorState.walkable[dogPos.y, dogPos.x] = true;
        
        int dogHp = 1;
        var dog = CreateTutorialMonster("dog", dogHp, 80, 0, 0, null);
        CurrentFloorState.actions[dogPos] = new ExploreAction
        {
            type = ExploreActionType.Monster,
            title = dog.name,
            description = string.Join(" / ", dog.traits ?? Array.Empty<string>()),
            monster = dog
        };
        used.Add(dogPos);

        // 3. 楼梯守卫 - 守住楼梯口，3个（原版：(9,6)直接挡路，(10,5)(10,7)侧翼）
        var guardPositions = new List<Vector2Int>
        {
            new Vector2Int(exitX - 1, playerY),      // (9,6) 直接挡路
            new Vector2Int(exitX, playerY - 1),      // (10,5) 侧翼
            new Vector2Int(exitX, playerY + 1)       // (10,7) 侧翼
        };

        // 强制守卫位置为地板（教学保证可放置）
        foreach (var pos in guardPositions)
        {
            if (pos.y >= 0 && pos.y < 13 && pos.x >= 0 && pos.x < 13)
            {
                CurrentFloorState.walkable[pos.y, pos.x] = true;
            }
        }

        // 获取zone-1的怪物类型（排除rat和dog）
        var guardTypes = GameDataImporter.MonsterDefinitions
            .Where(m => m.zone == 1 && !m.boss && m.id != "rat" && m.id != "dog")
            .Select(m => m.id)
            .ToList();

        int gPlaced = 0;
        foreach (var guardPos in guardPositions)
        {
            // 跳过已被占用的位置
            if (guardPos.x == ratPos.x && guardPos.y == ratPos.y) continue;
            if (guardPos.x == dogPos.x && guardPos.y == dogPos.y) continue;
            if (used.Any(u => u.x == guardPos.x && u.y == guardPos.y)) continue;
            if (guardPos.x < 0 || guardPos.x >= 13 || guardPos.y < 0 || guardPos.y >= 13) continue;

            string guardType = guardTypes[gPlaced % guardTypes.Count];
            var guardDef = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == guardType);
            if (guardDef.Equals(default)) continue;

            // 守卫属性削弱：HP 60% / ATK 70%（适配看门犬玩家）
            int guardHp = Mathf.Max(8, Mathf.RoundToInt(guardDef.hp * 0.6f));
            int guardAtk = Mathf.Max(2, Mathf.RoundToInt(guardDef.atk * 0.7f));

            var guard = new MonsterRuntime
            {
                id = guardType,
                name = $"守卫·{guardDef.name}",
                hp = guardHp,
                maxHp = guardHp,
                atk = guardAtk,
                def = guardDef.def,
                zone = guardDef.zone,
                isBoss = false,
                traits = guardDef.traits,
                axes = guardDef.axes,
                color = guardDef.color,
                possessBaseChance = 0.6f,
                phaseIndex = 0,
                stairGuard = true
            };

            CurrentFloorState.actions[guardPos] = new ExploreAction
            {
                type = ExploreActionType.Monster,
                title = guard.name,
                description = string.Join(" / ", guard.traits ?? Array.Empty<string>()),
                monster = guard
            };
            used.Add(guardPos);
            gPlaced++;
        }
    }

    private bool IsValidPosition(Vector2Int pos, HashSet<Vector2Int> used)
    {
        if (pos.x < 1 || pos.x > 11 || pos.y < 1 || pos.y > 11) return false;
        if (!CurrentFloorState.walkable[pos.y, pos.x]) return false;
        if (used.Contains(pos)) return false;
        return true;
    }

    private MonsterRuntime CreateTutorialMonster(string id, int hp, int maxHp, int atkMod, int defMod, string colorHex)
    {
        var def = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == id);
        if (def.Equals(default))
        {
            return new MonsterRuntime { id = id, name = id, hp = hp, maxHp = maxHp > 0 ? maxHp : hp };
        }

        int finalHp = hp > 0 ? hp : def.hp;
        int finalMaxHp = maxHp > 0 ? maxHp : def.hp;

        Color finalColor = def.color;
        if (!string.IsNullOrEmpty(colorHex))
        {
            ColorUtility.TryParseHtmlString(colorHex, out finalColor);
        }

        int finalDef = def.def + defMod;
        
        return new MonsterRuntime
        {
            id = def.id,
            name = def.name,
            hp = finalHp,
            maxHp = finalMaxHp,
            atk = def.atk + atkMod,
            def = finalDef,
            zone = def.zone,
            isBoss = def.boss,
            traits = def.traits,
            axes = def.axes,
            color = finalColor,
            possessBaseChance = 0.6f,
            phaseIndex = 0,
            _intent = null,
            _intentAtkBuff = 0f,
            _stunned = false,
            _netStunTurns = 0,
            _ambush = false, // 教程怪物不伏击
            _elite = false,
            _possessWindowUsed = false,
            _breakWindowDone = false
        };
    }

    private MonsterRuntime CreateMonsterForFloor(int zone, int floor, bool forceBoss)
    {
        // 最终层Boss：零号容器
        if (forceBoss && floor == GameManager.Instance.MaxFloor && BossAIManager.Instance != null)
        {
            var zeroBoss = BossAIManager.Instance.CreateZeroContainerBoss(floor);
            if (zeroBoss != null) return zeroBoss;
        }

        var pool = GameDataImporter.MonsterDefinitions
            .Where(m => forceBoss ? m.boss && m.zone == zone : !m.boss && m.zone == zone)
            .ToList();
        if (pool.Count == 0)
        {
            pool = GameDataImporter.MonsterDefinitions.Where(m => forceBoss ? m.boss : !m.boss).ToList();
        }

        var baseDef = pool[UnityEngine.Random.Range(0, pool.Count)];
        var curve = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, floor, GameManager.Instance.CurrentStage);

        // Elite check from curve params
        bool isElite = !forceBoss && floor >= 5 && UnityEngine.Random.value < curve.eliteRate;

        float eliteMult = isElite ? 1.6f : 1f;
        float eliteAtkMult = isElite ? 1.4f : 1f;
        string name = baseDef.name;
        if (isElite) name = "精英 " + name;

        // 原项目公式: mHp = Math.max(20, Math.floor(pHp * _curve.hpScale * (0.8 + Math.random() * 0.4)))
        // mAtk = Math.max(3, Math.floor(pAtk * _curve.atkScale * (0.8 + Math.random() * 0.4)))
        float randomMult = 0.8f + UnityEngine.Random.value * 0.4f;
        int monsterHp = Mathf.Max(20, Mathf.RoundToInt(baseDef.hp * curve.hpMult * randomMult * eliteMult));
        int monsterAtk = Mathf.Max(3, Mathf.RoundToInt(baseDef.atk * curve.atkMult * randomMult * eliteAtkMult));
        int monsterMaxHp = monsterHp;
        int monsterDef = Mathf.Max(1, Mathf.RoundToInt(baseDef.def));
        if (isElite) monsterDef = Mathf.RoundToInt(baseDef.def * 1.3f);

        // Tutorial: 看门犬在教程阶段 HP=1，maxHP固定为80，显示 1/80
        if (IsInTutorial && floor == 1 && baseDef.id == "dog")
        {
            monsterHp = 1;
            monsterMaxHp = 80;
        }

        return new MonsterRuntime
        {
            id = baseDef.id,
            name = name,
            hp = monsterHp,
            maxHp = monsterMaxHp,
            atk = monsterAtk,
            def = monsterDef,
            zone = baseDef.zone,
            isBoss = baseDef.boss,
            traits = baseDef.traits,
            axes = baseDef.axes,
            color = isElite ? new Color(1f, 0.85f, 0.2f) : baseDef.color,
            possessBaseChance = 0.6f,
            phaseIndex = 0,
            _intent = null,
            _intentAtkBuff = 0f,
            _stunned = false,
            _netStunTurns = 0,
            _ambush = UnityEngine.Random.value < 0.1f,
            _elite = isElite,
            _possessWindowUsed = false,
            _breakWindowDone = false
        };
    }

    private void InitMonsterAI(MonsterRuntime m, Vector2Int pos)
    {
        m.x = pos.x;
        m.y = pos.y;
        m.prevX = pos.x;
        m.prevY = pos.y;
        m.homeX = pos.x;
        m.homeY = pos.y;
        m.hasInitializedAI = true;

        var template = new MonsterTemplate
        {
            id = m.id, name = m.name, hp = m.hp, atk = m.atk, def = m.def,
            zone = m.zone, boss = m.isBoss, color = m.color,
            traits = m.traits != null ? new System.Collections.Generic.List<string>(m.traits) : new System.Collections.Generic.List<string>(),
            axes = m.axes != null ? new System.Collections.Generic.List<string>(m.axes) : new System.Collections.Generic.List<string>()
        };
        m.aiType = MonsterAISystem.GetMonsterAI(template);
        m.detectRange = MonsterAISystem.GetMonsterDetectRange(template);
    }

    private void TickMonsterAI()
    {
        if (CurrentFloorState == null || CurrentScreen != RunScreen.Exploration) return;
        var playerPos = CurrentFloorState.playerPos;
        var toMove = new System.Collections.Generic.List<(Vector2Int oldPos, ExploreAction action)>();

        foreach (var kvp in CurrentFloorState.actions)
        {
            if (kvp.Value.type != ExploreActionType.Monster || kvp.Value.consumed) continue;
            var m = kvp.Value.monster;
            if (m == null || !m.hasInitializedAI) continue;
            if (m.isTutorialPinned || m.stairGuard) continue;

            int dist = Mathf.Abs(playerPos.x - m.x) + Mathf.Abs(playerPos.y - m.y);

            switch (m.aiType)
            {
                case MonsterAIType.Idle:
                    break;
                case MonsterAIType.Patrol:
                    if (dist <= m.detectRange)
                        toMove.Add((kvp.Key, kvp.Value));
                    break;
                case MonsterAIType.Aggressive:
                    if (dist <= m.detectRange + 2)
                        toMove.Add((kvp.Key, kvp.Value));
                    break;
                case MonsterAIType.Ambush:
                    if (dist <= 2)
                        toMove.Add((kvp.Key, kvp.Value));
                    break;
            }
        }

        foreach (var (oldPos, action) in toMove)
        {
            var m = action.monster;
            int oldX = m.x, oldY = m.y;
            MonsterAISystem.MoveMonsterToward(m, playerPos.x, playerPos.y);

            if (m.x != oldX || m.y != oldY)
            {
                var newPos = new Vector2Int(m.x, m.y);
                if (!CurrentFloorState.walkable[m.y, m.x] ||
                    CurrentFloorState.actions.ContainsKey(newPos))
                {
                    m.x = oldX; m.y = oldY;
                    m.prevX = oldX; m.prevY = oldY;
                    continue;
                }
                CurrentFloorState.actions.Remove(oldPos);
                CurrentFloorState.actions[newPos] = action;
            }
        }
    }

    private float GetEnemyScaleForFloor(int floor, GameMode mode)
    {
        return ModeFloorCurves.GetParams(mode, floor, GameManager.Instance.CurrentStage).hpMult;
    }

    private int GetZoneForFloor(int floor, GameMode mode)
    {
        return ModeFloorCurves.GetZone(mode, floor);
    }

    private int GetMonsterCountForModeFloor(int floor, GameMode mode)
    {
        return ModeFloorCurves.GetParams(mode, floor, GameManager.Instance.CurrentStage).monsterCount;
    }

    #region Altar System
    public void ReturnToAnchor(int floor, int consecutiveDeaths = 0)
    {
        GameManager.Instance.CurrentFloor = floor;
        GenerateFloor(floor);
        CurrentScreen = RunScreen.Exploration;
        var p = GameManager.Instance.Player;
        // JS: 回滚后最低30%HP
        p.hp = Mathf.Max(1, Mathf.FloorToInt(p.maxHp * 0.3f));
        // JS: 连续死亡>=3次时额外强化
        if (consecutiveDeaths >= 3)
        {
            p.hp = p.maxHp;
            p.attack += Mathf.FloorToInt(p.attack * 0.2f);
            p.defense += Mathf.FloorToInt(p.defense * 0.2f);
            AddCombatLog("记忆共鸣强化: HP回满, ATK/DEF+20%");
        }
        AddCombatLog($"⏪ 返回至锚点 F{floor}");
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public void TriggerAltarIfAvailable()
    {
        if (ShowingAltar || ShowingCollapse) return;
        var available = new List<AltarData.AltarPair>();
        foreach (var pair in AltarData.Pairs)
        {
            if (!_usedAltarIds.Contains(pair.id))
                available.Add(pair);
        }
        if (available.Count == 0) return;

        PendingAltar = available[UnityEngine.Random.Range(0, available.Count)];
        ShowingAltar = true;
        if (!_shownSoftHints.Contains("hint_altar")) { _shownSoftHints.Add("hint_altar"); AddCombatLog("<color=#ffcc00>◆ 祭坛抉择：两个选项各有利弊，选择影响本局走向</color>"); }
        LastMessage = $"祭坛出现: {PendingAltar.aggressiveName} 或 {PendingAltar.conservativeName}";
    }

    public void ChooseAltar(bool aggressive)
    {
        if (PendingAltar == null) return;
        _usedAltarIds.Add(PendingAltar.id);

        if (aggressive)
        {
            PendingAltar.applyAggressive?.Invoke(GameManager.Instance.Player);
            LastMessage = $"你选择了: {PendingAltar.aggressiveName}";
        }
        else
        {
            PendingAltar.applyConservative?.Invoke(GameManager.Instance.Player);
            LastMessage = $"你选择了: {PendingAltar.conservativeName}";
        }

        ShowingAltar = false;
        PendingAltar = null;
        GameManager.Instance.NotifyPlayerStatsChanged();
    }
    #endregion

    #region Route System
    public void SelectRoute(int index)
    {
        ShowingRouteSelect = false;
        LastMessage = $"选择了路线: {PendingRoutes[index]}";
        GameManager.Instance.NotifyPlayerStatsChanged();
    }
    #endregion

    #region Wave Defense
    public void StartWaveDefense()
    {
        InWaveDefense = true;
        WaveNumber = 1;
        int zone = CurrentFloorState != null ? CurrentFloorState.zone : 1;
        WaveMonstersRemaining = 3 + zone / 2;
        SpawnWaveMonster();
        LastMessage = $"波次防御开始！第 {WaveNumber}/3 波，{WaveMonstersRemaining} 只敌人。";
    }

    private void SpawnWaveMonster()
    {
        int zone = CurrentFloorState != null ? CurrentFloorState.zone : 1;
        int floor = GameManager.Instance.CurrentFloor;
        CurrentEnemy = CreateMonsterForFloor(zone, floor, false);
        CurrentScreen = RunScreen.Combat;
        AudioManager.Instance?.DuckBGM(0.3f);
        GameManager.Instance.ResetCombatCounters();
        GameManager.Instance.ChangeState(GameManager.GameState.Combat);
        AddCombatLog($"波次 {WaveNumber}: {CurrentEnemy.name} 出现。剩余 {WaveMonstersRemaining}。");
    }

    public void OnWaveMonsterDefeated()
    {
        WaveMonstersRemaining--;
        if (WaveMonstersRemaining <= 0)
        {
            WaveNumber++;
            if (WaveNumber > 3)
            {
                InWaveDefense = false;
                CurrentScreen = RunScreen.Exploration;
                GameManager.Instance.EndCombat(true);
                LastMessage = "所有波次已清除！楼梯解锁。";
                return;
            }
            int zone = CurrentFloorState != null ? CurrentFloorState.zone : 1;
            WaveMonstersRemaining = 3 + zone / 2;
            LastMessage = $"第 {WaveNumber}/3 波开始。";
        }
        SpawnWaveMonster();
    }
    #endregion

    #region Floor Signatures
    // 原版逻辑：游戏开始时一次性预分配签名到楼层（Short模式不出签名）
    private void AssignFloorSignatures()
    {
        _floorSignatureMap.Clear();

        // Short 模式不出签名
        if (GameManager.Instance.CurrentMode == GameMode.Short) return;

        int maxFloor = GameManager.Instance.MaxFloor;

        // 签名池洗牌
        var sigIds = new List<string>();
        foreach (var s in FloorSignatureData.Signatures)
            sigIds.Add(s.id);
        for (int i = sigIds.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = sigIds[i]; sigIds[i] = sigIds[j]; sigIds[j] = tmp;
        }

        // 可用楼层池：F2~maxFloor-1，排除10的倍数(Boss层)
        var floors = new List<int>();
        for (int f = 2; f <= maxFloor - 1; f++)
        {
            if (f % 10 != 0) floors.Add(f);
        }
        for (int i = floors.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            int tmp = floors[i]; floors[i] = floors[j]; floors[j] = tmp;
        }

        // 每个签名分配到一个楼层（1:1），需满足 minFloor
        foreach (var sigId in sigIds)
        {
            var sig = System.Array.Find(FloorSignatureData.Signatures, s => s.id == sigId);
            if (sig == null) continue;

            bool assigned = false;
            for (int j = 0; j < floors.Count; j++)
            {
                if (sig.minFloor <= 0 || floors[j] >= sig.minFloor)
                {
                    _floorSignatureMap[floors[j]] = sigId;
                    floors.RemoveAt(j);
                    assigned = true;
                    break;
                }
            }
        }
    }

    private void ApplyFloorSignature()
    {
        ClearFloorSignature();

        int floor = GameManager.Instance.CurrentFloor;
        if (!_floorSignatureMap.TryGetValue(floor, out string sigId)) return;

        var sig = System.Array.Find(FloorSignatureData.Signatures, s => s.id == sigId);
        if (sig == null) return;

        ActiveSignature = sig;
        CurrentFloorSignature = sig.name;
        FloorSignatureAtkMult = 1f;
        FloorSignatureDefMult = 1f;
        FloorSignatureEpMult = sig.epMult > 0 ? sig.epMult : 1f;

        AddCombatLog($"{sig.icon} 楼层签名: {sig.name} — {sig.desc}");
    }

    private void ClearFloorSignature()
    {
        ActiveSignature = null;
        CurrentFloorSignature = "";
        FloorSignatureAtkMult = 1f;
        FloorSignatureDefMult = 1f;
        FloorSignatureEpMult = 1f;
        BountyTargetId = null;
        BountyTargetName = null;
        _judgmentKillOrder = null;
        _judgmentKillIdx = 0;
        _polStormSteps = 0;
    }
    #endregion

    #region Tutorial
    private void OnTutorialStageUp(int stage)
    {
        AdvanceTutorialStage(stage);
    }
    #endregion

    #region Story Events
    public bool ShowingStoryEvent { get; private set; }
    public string CurrentStoryEventTitle { get; private set; }
    public string CurrentStoryEventText { get; private set; }

    public void ShowStoryEvent(string title, string text)
    {
        ShowingStoryEvent = true;
        CurrentStoryEventTitle = title;
        CurrentStoryEventText = text;
    }

    public void DismissStoryEvent()
    {
        ShowingStoryEvent = false;
    }

    public void StartIntroDialogue()
    {
        var intro = StorySystem.Instance?.GetIntroScript();
        if (intro == null || intro.Count == 0)
        {
            Debug.LogError("[Intro] GetIntroScript returned null or empty!");
            return;
        }

        Debug.Log($"[Intro] Starting intro dialogue, {intro.Count} lines total.");
        StorySystem.Instance.introCurrentIndex = 0;
        StorySystem.Instance.introPlaying = true;
        ShowingStoryEvent = true;
        ShowIntroLine(0);
    }

    public void AdvanceIntroDialogue()
    {
        var ss = StorySystem.Instance;
        if (ss == null || !ss.introPlaying) return;

        ss.introCurrentIndex++;
        var intro = ss.introLines;

        Debug.Log($"[Intro] Advancing to line {ss.introCurrentIndex}/{intro.Count}");

        if (ss.introCurrentIndex >= intro.Count)
        {
            ss.introPlaying = false;
            ShowingStoryEvent = false;
            PlayerPrefs.SetInt("IntroShown_" + GameManager.Instance.CurrentMode, 1);
            PlayerPrefs.Save();
            Debug.Log("[Intro] Intro complete. PlayerPrefs saved.");
            return;
        }

        ShowIntroLine(ss.introCurrentIndex);
    }

    void ShowIntroLine(int index)
    {
        var ss = StorySystem.Instance;
        if (ss == null || index < 0 || index >= ss.introLines.Count) return;

        var line = ss.introLines[index];
        CurrentStoryEventTitle = line.speaker;
        CurrentStoryEventText = line.text;
        ShowingStoryEvent = true;
        Debug.Log($"[Intro] Line {index}: [{line.speaker}] {line.text}");
    }

    public bool IsIntroPlaying() => StorySystem.Instance?.introPlaying ?? false;

    #region Fragment Choice System
    // 碎片系统完整实现在 FragmentSystem.cs (partial class)
    #endregion
    #endregion

    #region Collapse System
    public void TriggerCollapseIfAvailable()
    {
        if (ShowingAltar || ShowingCollapse) return;
        ShowingCollapse = true;
        LastMessage = "警告：崩溃区域！";
    }

    public void ChooseCollapse(bool escape)
    {
        ShowingCollapse = false;
        if (escape)
        {
            LastMessage = "逃离崩溃区域！";
        }
        else
        {
            int damage = Mathf.RoundToInt(GameManager.Instance.Player.maxHp * 0.3f);
            GameManager.Instance.Player.hp -= damage;
            LastMessage = $"被崩溃吞噬！受到 {damage} 点伤害。";
        }
        GameManager.Instance.NotifyPlayerStatsChanged();
    }
    #endregion

    #region Helpers
    private string GetClassName(string classId)
    {
        switch (classId)
        {
            case "parasite": return "寄生体";
            case "swarm": return "虫群";
            case "hunter": return "猎手";
            case "blood": return "血裔";
            default: return classId;
        }
    }

    private string GetModeIntro(GameMode mode)
    {
        int maxF = GameManager.Instance.MaxFloor;
        switch (mode)
        {
            case GameMode.Classic: return $"宿命轮回开始。{maxF}层挑战。";
            case GameMode.Short: return $"暗影穿行开始。{maxF}层挑战。";
            case GameMode.Expedition: return $"远征模式开始。{maxF}层挑战。";
            default: return "游戏开始。";
        }
    }

    private float GetFloorAdvancePollutionDecay()
    {
        if (GameManager.Instance.CurrentMode == GameMode.Expedition)
        {
            return 5f;
        }
        return 0f;
    }

    private void ApplyClassIdentity(string classId)
    {
        var player = GameManager.Instance.Player;
        // 原版：初始形态是人类(human)，基础属性来自classBaseStats
        player.currentFormId = "human";
        if (!player.ownedForms.Contains("human"))
            player.ownedForms.Insert(0, "human");
        player.pollution = 0f;
        player.gold = 0;
        player.evolutionPoints = 0;
        player.fragments = 0;
        player.formSlots = 3;
        player.collapseResistCharges = 0;
        player.deathReviveCharges = 0;
        player.noDeathRun = true;
        player.possessionCountThisRun = 0;
        player.totalKillsThisRun = 0;
        player.monstersKilled = 0;
        player.totalDamageDealt = 0;
        player.maxPollutionReached = 0f;
        player.formBondCounts.Clear();
        player.seenMonsterTypes.Clear();
        player.equippedFragments.Clear();

        _firstPossession = true;
        _resonanceTriggersThisRun.Clear();
        _maxDamageThisRun = 0;
        _maxDamageComboThisRun = "";
    }

    public bool HasEvolutionPassive(string classId, int level)
    {
        if (EvolutionLevel < level) return false;
        var player = GameManager.Instance.Player;
        return player.selectedClass == classId;
    }

    public void UseUltimate()
    {

        if (CurrentScreen != RunScreen.Combat || CurrentEnemy == null)
        {

            return;
        }
        if (_ultCooldown > 0)
        {
            AddCombatLog($"终极技能冷却中（还需 {_ultCooldown} 场战斗）");
            return;
        }
        var player = GameManager.Instance.Player;

        switch (player.selectedClass)
        {
            case "titan":
                player.maxHp = Mathf.RoundToInt(player.maxHp * 1.5f);
                player.hp = player.maxHp;
                player.attack += 15;
                player.defense += 15;
                AddCombatLog("泰坦之怒！HP×1.5 ATK+15 DEF+15");

                break;
            case "ghost":
                int burstDmg = player.attack * 5;
                CurrentEnemy.hp = Mathf.Max(0, CurrentEnemy.hp - burstDmg);
                AddCombatLog($"虚空行者！暴击×5 造成{burstDmg}伤害");
                DamageNumberPool.Instance?.SpawnDamage(burstDmg, true);
                break;
            case "swarm":
                int clones = Mathf.Max(1, Mathf.FloorToInt(player.pollution / 10f));
                int cloneDmg = Mathf.RoundToInt(player.attack * 0.8f) * clones;
                CurrentEnemy.hp = Mathf.Max(0, CurrentEnemy.hp - cloneDmg);
                AddCombatLog($"虫群之心！{clones}分身 造成{cloneDmg}伤害");
                DamageNumberPool.Instance?.SpawnDamage(cloneDmg, true);
                break;
            case "blood":
                int heal = Mathf.RoundToInt(player.maxHp * 0.5f);
                player.hp = Mathf.Min(player.maxHp, player.hp + heal);
                player.attack = Mathf.RoundToInt(player.attack * 1.5f);
                AddCombatLog($"血月狂宴！ATK×1.5 回复{heal}HP");
                break;
            case "mech":
                int shieldDmg = Mathf.RoundToInt(player.pollution * 2f);
                CurrentEnemy.hp = Mathf.Max(0, CurrentEnemy.hp - shieldDmg);
                player.defense += 10;
                player.pollution = 0f;
                AddCombatLog($"过载核心！AOE{shieldDmg}伤害 DEF+10 污染清零");
                DamageNumberPool.Instance?.SpawnDamage(shieldDmg, true);
                break;
        }

        ScreenEffectsManager.Instance?.FlashCrit();
        GameManager.Instance.NotifyPlayerStatsChanged();

        // 设置冷却 (原版: titan/ghost/swarm/blood=3, mech=4)
        _ultCooldown = player.selectedClass == "mech" ? 4 : 3;

        if (CurrentEnemy != null && CurrentEnemy.hp <= 0)
        {
            OnEnemyDefeated(false);
        }
    }

    public void ReturnToMenu()
    {
        CurrentScreen = RunScreen.MainMenu;
        CurrentFloorState = null;
        CurrentEnemy = null;
        AutoCombatEnabled = false;
        ShowingAltar = false;
        ShowingCollapse = false;
        ShowingRouteSelect = false;
        ShowingDeathFormSelect = false;
        ShowingDeathRollback = false;
        ShowingFormReplace = false;
        ShowingStoryEvent = false;
        InWaveDefense = false;
        EvolutionLevel = 0;
        _usedAltarIds.Clear();
        CombatLog.Clear();
        LastCombatMessage = "";
        LastMessage = "";
        GameManager.Instance.ChangeState(GameManager.GameState.MainMenu);
    }

    public void ShowModeSelect()
    {
        CurrentScreen = RunScreen.CharacterSelect;
    }

    public void ResolveCollapse(int index)
    {
        ChooseCollapse(index == 0);
    }

    public void SelectDeathFormByFormId(string formId)
    {
        ShowingDeathFormSelect = false;

        var player = GameManager.Instance.Player;

        // 检查目标形态是否为死亡状态
        int idx = player.ownedForms.IndexOf(formId);
        if (idx < 0 || idx >= player.deadForms.Count || player.deadForms[idx])
        {
            AddCombatLog($"<color=#ff4444>形态 {GetDisplayName(formId)} 已死亡，无法切换！</color>");
            return;
        }

        // 保存旧形态ID（必须在LoadForm之前，因为LoadForm会修改currentFormId）
        string oldFormId = player.currentFormId;

        // 保存当前（濒死）形态HP到formHpMap，避免死亡丢失血量数据
        player.formHpMap[oldFormId] = player.hp;
        player.formMaxHpMap[oldFormId] = player.maxHp;

        // 使用LoadForm计算属性（与PlayerSwitchForm保持一致）
        bool loadSuccess = LoadForm(formId);
        if (!loadSuccess)
        {
            AddCombatLog($"<color=#ff4444>无法加载形态 {formId}！</color>");
            return;
        }

        player.currentFormStartTime = Time.time;

        // 熟练度加成（按比例增加HP）
        float prof = FormResonanceSystem.Instance?.GetProficiencyBonus(formId) ?? 0;
        if (prof > 0)
        {
            player.attack = Mathf.CeilToInt(player.attack * (1f + prof));
            player.defense = Mathf.CeilToInt(player.defense * (1f + prof));
            player.maxHp = Mathf.CeilToInt(player.maxHp * (1f + prof));
            player.hp = Mathf.RoundToInt(player.hp * (1f + prof));
        }

        // 共鸣加成（按比例增加HP）
        ApplyResonanceBonuses(formId);

        // 恢复目标形态的独立HP（在所有加成应用之后）
        if (player.formHpMap.TryGetValue(formId, out int savedHp))
        {
            if (player.formMaxHpMap.TryGetValue(formId, out int savedMaxHp) && savedMaxHp > 0)
            {
                float hpRatio = (float)savedHp / savedMaxHp;
                int restoredHp = Mathf.RoundToInt(player.maxHp * hpRatio);
                player.hp = Mathf.Clamp(restoredHp, 1, player.maxHp);
            }
            else
            {
                player.hp = Mathf.Clamp(savedHp, 1, player.maxHp);
            }
        }
        else
        {
            player.hp = player.maxHp;
        }

        // 共鸣效果处理
        FormResonanceSystem.Instance?.OnFormSwitch(oldFormId, formId);
        player.switchShieldTurns = FormResonanceSystem.Instance?.HasActiveEffect("shield") == true ? 1 : 0;

        // Back to combat
        CurrentScreen = RunScreen.Combat;
        AudioManager.Instance?.DuckBGM(0.3f);

        AddCombatLog($"濒死转移！→{GetDisplayName(formId)} HP:{player.hp}/{player.maxHp} ATK:{player.attack} DEF:{player.defense}");
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public void SelectDeathForm(int index)
    {
        ShowingDeathFormSelect = false;
        
        var player = GameManager.Instance.Player;
        
        // index = -1 表示没有可用形态，游戏结束
        if (index < 0 || index >= player.ownedForms.Count)
        {
            GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
            TriggerDeathReport();
            return;
        }
        
        // 获取选中的形态ID（跳过当前形态）
        string selectedFormId = "";
        int count = 0;
        foreach (var formId in player.ownedForms)
        {
            if (formId == player.currentFormId) continue;
            if (count == index)
            {
                selectedFormId = formId;
                break;
            }
            count++;
        }
        
        if (!string.IsNullOrEmpty(selectedFormId))
        {
            SelectDeathFormByFormId(selectedFormId);
        }
        else
        {
            GameManager.Instance.ChangeState(GameManager.GameState.GameOver);
            TriggerDeathReport();
        }
    }

    public string GetResonanceLevelName(int level)
    {
        switch (level)
        {
            case 1: return "陌生";
            case 2: return "熟悉";
            case 3: return "共鸣";
            case 4: return "同步";
            case 5: return "融合";
            default: return "未知";
        }
    }
    
    public int GetFormResonanceLevel(string formId)
    {
        var player = GameManager.Instance.Player;
        if (player.formResonanceLevels.TryGetValue(formId, out int level))
        {
            return level;
        }
        return 1;
    }
    
    public void AddFormResonance(string formId)
    {
        var player = GameManager.Instance.Player;
        if (!player.formResonanceLevels.TryGetValue(formId, out int level))
        {
            level = 1;
        }
        level = Mathf.Min(level + 1, 5);
        player.formResonanceLevels[formId] = level;
        AddCombatLog($"✨ {formId} 共鸣等级提升至 {GetResonanceLevelName(level)}");
    }
    
    public float GetResonanceAttackBonus(string formId)
    {
        int level = GetFormResonanceLevel(formId);
        switch (level)
        {
            case 2: return 1.05f;
            case 3: return 1.10f;
            case 4: return 1.15f;
            case 5: return 1.25f;
            default: return 1.0f;
        }
    }
    
    public float GetResonanceDefenseBonus(string formId)
    {
        int level = GetFormResonanceLevel(formId);
        switch (level)
        {
            case 2: return 1.05f;
            case 3: return 1.10f;
            case 4: return 1.15f;
            case 5: return 1.25f;
            default: return 1.0f;
        }
    }
    
    public float GetResonancePossessBonus(string formId)
    {
        int level = GetFormResonanceLevel(formId);
        switch (level)
        {
            case 3: return 0.05f;
            case 4: return 0.10f;
            case 5: return 0.15f;
            default: return 0f;
        }
    }

    public float CalculatePossessChance(MonsterRuntime enemy)
    {
        if (enemy == null) return 0f;
        // 原版公式: 0.4 * hpFactor - defResist + possessionBonus
        float hpRatio = enemy.maxHp > 0 ? (float)enemy.hp / enemy.maxHp : 0.5f;
        float hpFactor = Mathf.Max(0f, 1f - hpRatio * 0.6f);
        float defResist = Mathf.Min(0.15f, enemy.def * 0.005f);
        float sigPossBonus = ActiveSignature != null ? ActiveSignature.possessBonus : 0f;
        float rate = 0.4f * hpFactor - defResist + GameManager.Instance.Player.possessionBonus + sigPossBonus;
        return Mathf.Clamp(rate, 0.01f, 0.95f);
    }

    public int GetFormBond(string id)
    {
        return GetFormResonanceLevel(id);
    }

    public int GetBondLevel(string id)
    {
        return GetFormResonanceLevel(id);
    }

    public float GetExploreProgress()
    {
        if (CurrentFloorState == null) return 0f;
        return 0.5f;
    }

    public EvolutionData.EvolutionNode GetNextEvolutionNode()
    {
        return new EvolutionData.EvolutionNode { level = EvolutionLevel + 1, name = "进化", epCost = GetEvolutionCost(), description = "提升进化等级" };
    }

    public void TriggerGameOver()
    {
        GameManager.Instance.GameOver();
    }

    public void NotifyDeathChoice(List<string> availableForms)
    {
        ShowingDeathFormSelect = true;
    }

    public string GetDisplayName(string formId)
    {
        if (string.IsNullOrEmpty(formId) || formId == "human") return "基础形态";
        // Check standard definitions
        var def = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == formId);
        if (!string.IsNullOrEmpty(def.id)) return def.name;
        // Check if it's a tutorial/custom monster - look up in current floor actions
        if (CurrentFloorState != null)
        {
            foreach (var kvp in CurrentFloorState.actions)
            {
                if (kvp.Value.monster != null && kvp.Value.monster.id == formId)
                    return kvp.Value.monster.name;
            }
        }
        // Check current enemy
        if (CurrentEnemy != null && CurrentEnemy.id == formId)
            return CurrentEnemy.name;
        return formId;
    }

    public string GetBondLevelName(string formId)
    {
        int level = GetFormResonanceLevel(formId);
        return GetResonanceLevelName(level);
    }
    #endregion

    public string GetExploreCellGlyph(int x, int y)
    {
        if (CurrentFloorState == null) return " ";
        if (CurrentFloorState.discovered == null || CurrentFloorState.walkable == null) return " ";
        if (x < 0 || x >= 13 || y < 0 || y >= 13) return " ";
        var pos = new Vector2Int(x, y);
        if (!CurrentFloorState.discovered[y, x]) return "·";
        if (CurrentFloorState.playerPos == pos) return "你";
        if (CurrentFloorState.exitPos == pos) return "⇧";
        if (!CurrentFloorState.walkable[y, x]) return "█";
        if (CurrentFloorState.actions != null && CurrentFloorState.actions.TryGetValue(pos, out var action) && !action.consumed)
        {
            switch (action.type)
            {
                case ExploreActionType.Monster: return action.monster != null && action.monster.isBoss ? "王" : "敌";
                case ExploreActionType.Event: return "事";
                case ExploreActionType.Fragment: return "◆";
                default: return "□";
            }
        }
        return "□";
    }
}

[System.Serializable]
public class FloorRuntime
{
    public int floor;
    public int zone;
    public bool[,] discovered;
    public bool[,] walkable;
    public Vector2Int playerPos;
    public Vector2Int exitPos;
    public Dictionary<Vector2Int, ExploreAction> actions;
    public Dictionary<Vector2Int, MapDecoration> decorations;
    public int stepsTaken;
}

public enum DecorationType
{
    River,
    Mountain,
    Crystal,
    Fungus,
    Bioluminescence,
    BrokenMachine,
    ToxicPuddle,
    Growth,
    Hologram
}

public class MapDecoration
{
    public DecorationType type;
    public string icon;
    public Color color;
    public bool isAnimated;
    public float animationSpeed;
}

[System.Serializable]
public class ExploreAction
{
    public ExploreActionType type;
    public string title;
    public string description;
    public MonsterRuntime monster;
    public bool consumed;
}

public enum ExploreActionType
{
    Monster,
    Event,
    Shop,
    Fragment
}

public enum MonsterIntentType
{
    attack,
    heavy,
    buff,
    heal,
    charge,
    explode
}

[System.Serializable]
public class MonsterIntent
{
    public MonsterIntentType type;
    public float buffAmt;
    public float healAmt;
    public float mult;
}

[System.Serializable]
public class MonsterRuntime
{
    public string id;
    public string name;
    public int hp;
    public int maxHp;
    public int atk;
    public int def;
    public int zone;
    public bool isBoss;
    public string[] traits;
    public string[] axes;
    public Color color;
    public float possessBaseChance;
    public int phaseIndex;
    public string ability;
    
    // 意图系统相关字段
    public MonsterIntent _intent;
    public float _intentAtkBuff;
    public bool _stunned;
    public int _netStunTurns;
    public bool _ambush;
    public bool _elite;
    public bool _possessWindowUsed;
    public bool _breakWindowDone;
    
    // 特性状态字段
    public bool hasExplodeWarned;
    public bool hasRevived;
    public bool hasBleedApplied;
    public bool isElite;
    
    // 状态效果
    public float _atkDebuff;
    public int _atkDebuffTurns;
    public int _corrodeApplied;
    
    // AI相关字段
    public MonsterAIType aiType;
    public int homeX;
    public int homeY;
    public int x;
    public int y;
    public int prevX;
    public int prevY;
    public int alertLevel;
    public int alertDecay;
    public int lastSeenX;
    public int lastSeenY;
    public int detectRange;
    public int fleeImmunity;
    public int[] lastDir;
    public bool hasInitializedAI;
    public bool isPossessed;
    public bool isAmbush;
    public bool isTutorialPinned;
    public bool stairGuard;
    
    // 检查是否拥有指定特性
    public bool HasTrait(string trait)
    {
        if (traits == null) return false;
        foreach (var t in traits)
        {
            if (t.Equals(trait, System.StringComparison.OrdinalIgnoreCase))
                return true;
        }
        return false;
    }
}
