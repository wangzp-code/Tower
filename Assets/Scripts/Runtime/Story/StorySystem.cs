using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public class StorySystem : SingletonBase<StorySystem>
{
    public enum StoryEventType { Note, Revelation, Fragment, Finale }

    public struct IntroLine
    {
        public string speaker;
        public string text;
        public string colorHex;
    }

    public List<IntroLine> introLines = new List<IntroLine>();
    public int introCurrentIndex = -1;
    public bool introPlaying = false;

    public List<IntroLine> GetIntroScript()
    {
        if (introLines.Count == 0) InitializeIntro();
        return introLines;
    }

    void InitializeIntro()
    {
        introLines = new List<IntroLine>
        {
            new IntroLine { speaker = "神秘声音", text = "你醒了。记不起自己是谁，只清楚一件事——你在塔里。", colorHex = "#ffcc44" },
            new IntroLine { speaker = "神秘声音", text = "这座塔没有出口。只有向上走，或者成为塔的一部分。", colorHex = "#ff8866" },
            new IntroLine { speaker = "系统", text = "第 1 层 — 入口通道已解锁。警告：污染值已初始化。", colorHex = "#88ccff" },
            new IntroLine { speaker = "神秘声音", text = "杀死敌人，占据他们的身体。这是你唯一的活路。", colorHex = "#aa66ff" }
        };
    }

    public struct StoryTrigger
    {
        public int floor;
        public string type;
        public string title;
        public string text;
        public string rewardName;
        public int reward;
        public string rewardDesc;
        public string storyReward;
    }

    public struct HiddenStoryEvent
    {
        public string id;
        public string name;
        public string text;
        public string triggerOn;
        public bool repeatable;
        public System.Func<bool> check;
        public System.Action reward;
    }

    public Dictionary<int, StoryTrigger> storyTriggers = new Dictionary<int, StoryTrigger>();
    public List<HiddenStoryEvent> hiddenStoryEvents = new List<HiddenStoryEvent>();

    private void Awake()
    {
        base.Awake();
        InitializeStoryTriggers();
        InitializeHiddenEvents();
    }

    protected override void OnDestroy()
    {
        storyTriggers.Clear();
        hiddenStoryEvents.Clear();
        base.OnDestroy();
    }

    void InitializeStoryTriggers()
    {
        storyTriggers.Add(5, new StoryTrigger
        {
            floor = 5,
            type = "note",
            title = "◆ 古老的笔记",
            text = "一张泛黄的羊皮纸躺在地上。上面写着：\n\n\"这座塔不是监狱，而是孵化器。我们都是实验体，等待着觉醒的那一天。\"\n\n角落里画着一个诡异的符号——像是某种寄生生物的图腾。",
            rewardName = "+150EP",
            reward = 150,
            storyReward = "parasite_instinct"
        });

        storyTriggers.Add(10, new StoryTrigger
        {
            floor = 10,
            type = "revelation",
            title = "◇ 认知冲击",
            text = "当你踏入这一层，一阵剧痛撕裂你的脑海。\n\n无数记忆碎片涌入——实验室的白光、冰冷的仪器、以及一个反复出现的声音：\n\n\"实验体...觉醒...成为...\"\n\n记忆突然中断，但你感觉自己变得更加强大了。",
            rewardName = "...继续前进",
            storyReward = "echo_heal"
        });

        storyTriggers.Add(15, new StoryTrigger
        {
            floor = 15,
            type = "fragment",
            title = "◈ 记忆碎片",
            text = "墙壁上浮现出模糊的影像。你看到了另一个自己——或者说是，另一个宿主。\n\n\"每一次死亡都是蜕变，每一次寄生都是进化。\"\n\n声音在你耳边低语，像是来自深渊的呼唤。",
            rewardName = "接受记忆",
            storyReward = "dead_relic"
        });

        storyTriggers.Add(20, new StoryTrigger
        {
            floor = 20,
            type = "revelation",
            title = "○ 真相之门",
            text = "一扇巨大的金属门挡住了去路。门上刻着古老的符文：\n\n\"只有接受真相的人才能通过。\"\n\n你触摸符文的瞬间，一股能量涌入体内。",
            rewardName = "推开大门",
            storyReward = "anchor_shield"
        });

        storyTriggers.Add(25, new StoryTrigger
        {
            floor = 25,
            type = "fragment",
            title = "◎ 深渊回响",
            text = "深渊在召唤。\n\n你听到了无数声音——那些曾经在这里死去的实验体的哀嚎。但其中一个声音格外清晰：\n\n\"加入我们...成为一体...\"\n\n你感到污染正在侵蚀你的意识，但同时也带来了力量。",
            rewardName = "倾听深渊",
            storyReward = "cognitive_split"
        });

        storyTriggers.Add(30, new StoryTrigger
        {
            floor = 30,
            type = "revelation",
            title = "⚡ 残响之力",
            text = "一道耀眼的光芒笼罩了你。\n\n\"你的进化之路已经开启。选择一条道路，成为真正的存在。\"\n\n你感到力量在体内沸腾，等待着释放。",
            rewardName = "接受力量",
            storyReward = "echo_power"
        });

        storyTriggers.Add(35, new StoryTrigger
        {
            floor = 35,
            type = "fragment",
            title = "◉ 预知残像",
            text = "时间似乎变慢了。\n\n你看到了无数可能的未来——每一条都通向不同的结局。但有一条路格外清晰：\n\n\"超越...进化...成为...\"\n\n当你回过神来，战斗的技巧已经刻入骨髓。",
            rewardName = "预见未来",
            storyReward = "foresight"
        });

        storyTriggers.Add(40, new StoryTrigger
        {
            floor = 40,
            type = "revelation",
            title = "★ 记忆融合",
            text = "所有的记忆开始融合。\n\n你看到了自己的过去、现在和可能的未来。每一次死亡、每一次寄生、每一次进化——都在这一刻汇聚。\n\n\"你已经准备好了。\"\n\n一个声音宣告。",
            rewardName = "融合记忆",
            storyReward = "memory_fusion"
        });

        storyTriggers.Add(45, new StoryTrigger
        {
            floor = 45,
            type = "fragment",
            title = "◎ 递归觉醒",
            text = "你站在了塔的边缘。\n\n下方是无尽的深渊，上方是刺眼的光芒。你终于理解了这座塔的真相——它不是一个地方，而是一个循环。\n\n\"突破循环，成为新的存在。\"\n\n这是最后的考验。",
            rewardName = "突破循环",
            storyReward = "recursive_awaken"
        });

        storyTriggers.Add(50, new StoryTrigger
        {
            floor = 50,
            type = "finale",
            title = "★ 起源之地",
            text = "你终于到达了塔顶。\n\n这里什么都没有——只有一面镜子。镜子里的你看着你，眼神中充满了期待。\n\n\"你已经走了很远的路。现在，做出你的选择吧。\"\n\n镜中的你伸出了手。",
            rewardName = "选择命运"
        });
    }

    void InitializeHiddenEvents()
    {
        hiddenStoryEvents.Add(new HiddenStoryEvent
        {
            id = "echo_corridor",
            name = "回声走廊",
            triggerOn = "move",
            repeatable = false,
            check = () =>
            {
                var game = CompleteGameSystem.Instance;
                return game != null && game.CurrentFloorState != null && game.CurrentFloorState.stepsTaken >= 15 && 
                       !GameManager.Instance.Player.storyFlags.ContainsKey("hidden_echo_corridor");
            },
            text = "你的脚步声在走廊中产生了奇怪的共鸣...\n墙壁震动，露出一个暗格！",
            reward = () =>
            {
                var p = GameManager.Instance.Player;
                p.attack += 2;
                p.defense += 2;
                p.maxHp += 15;
                p.hp = Mathf.Min(p.maxHp, p.hp + 15);
                CompleteGameSystem.Instance.AddCombatLog("◈ 回声走廊：ATK+2 DEF+2 MaxHP+15");
            }
        });

        hiddenStoryEvents.Add(new HiddenStoryEvent
        {
            id = "mirror_whisper",
            name = "镜中低语",
            triggerOn = "shop",
            repeatable = false,
            check = () =>
            {
                var p = GameManager.Instance.Player;
                return p.ownedForms.Count >= 3 && 
                       !p.storyFlags.ContainsKey("hidden_mirror_whisper");
            },
            text = "商人盯着你看了很久：\n\"你身上有太多灵魂的气息...\"\n他从柜台下取出一个发光的碎片。",
            reward = () =>
            {
                var p = GameManager.Instance.Player;
                p.attack += 3;
                p.defense += 3;
                p.maxHp += 20;
                p.hp = Mathf.Min(p.maxHp, p.hp + 20);
                CompleteGameSystem.Instance.AddCombatLog("◈ 镜中低语：ATK+3 DEF+3 MaxHP+20");
            }
        });

        hiddenStoryEvents.Add(new HiddenStoryEvent
        {
            id = "parasite_resonance",
            name = "寄生共鸣",
            triggerOn = "possess_success",
            repeatable = false,
            check = () =>
            {
                var p = GameManager.Instance.Player;
                return p.pollution >= 60f && 
                       !p.storyFlags.ContainsKey("hidden_parasite_resonance");
            },
            text = "\"污染不是毒药...它在帮助你\"\n一股温暖的能量从污染核心流出。",
            reward = () =>
            {
                var p = GameManager.Instance.Player;
                p.maxHp += 25;
                p.hp = Mathf.Min(p.maxHp, p.hp + 10);
                CompleteGameSystem.Instance.AddCombatLog("☢️ 寄生共鸣：MaxHP+25 HP+10");
            }
        });

        hiddenStoryEvents.Add(new HiddenStoryEvent
        {
            id = "death_memory",
            name = "亡者记忆",
            triggerOn = "combat_win",
            repeatable = false,
            check = () =>
            {
                var p = GameManager.Instance.Player;
                return p.deathReviveCharges > 0 && 
                       !p.storyFlags.ContainsKey("hidden_death_memory");
            },
            text = "\"每次死亡都让你更强...\"\n亡者的记忆融入了你的意识。",
            reward = () =>
            {
                var p = GameManager.Instance.Player;
                p.defense += 3;
                CompleteGameSystem.Instance.AddCombatLog("☠ 亡者记忆：DEF+3");
            }
        });

        hiddenStoryEvents.Add(new HiddenStoryEvent
        {
            id = "lone_explorer",
            name = "孤独探索者",
            triggerOn = "floor_enter",
            repeatable = false,
            check = () =>
            {
                var p = GameManager.Instance.Player;
                return p.ownedForms.Count == 1 && 
                       !p.storyFlags.ContainsKey("hidden_lone_explorer");
            },
            text = "\"独行的意志赋予了你力量\"\n你感受到一股孤独而强大的能量。",
            reward = () =>
            {
                var p = GameManager.Instance.Player;
                p.attack += 4;
                CompleteGameSystem.Instance.AddCombatLog("◎ 孤独探索者：ATK+4");
            }
        });

        hiddenStoryEvents.Add(new HiddenStoryEvent
        {
            id = "wall_gift",
            name = "墙壁暗格",
            triggerOn = "wall_bump",
            repeatable = true,
            check = () =>
            {
                var p = GameManager.Instance.Player;
                return Random.value < 0.03f && 
                       !p.storyFlags.ContainsKey("hidden_wall_gift_" + GameManager.Instance.CurrentFloor);
            },
            text = "你撞到墙壁时，发现了一个隐藏暗格！\n里面有前人留下的物资。",
            reward = () =>
            {
                var p = GameManager.Instance.Player;
                var floor = GameManager.Instance.CurrentFloor;
                p.storyFlags["hidden_wall_gift_" + floor] = true;
                float r = Random.value;
                if (r < 0.3f)
                {
                    p.hp = Mathf.Min(p.maxHp, p.hp + Mathf.FloorToInt(p.maxHp * 0.2f));
                    CompleteGameSystem.Instance.AddCombatLog("★ 墙壁暗格：回复20%HP");
                }
                else if (r < 0.6f)
                {
                    p.evolutionPoints += 50;
                    CompleteGameSystem.Instance.AddCombatLog("★ 墙壁暗格：+50EP");
                }
                else if (r < 0.85f)
                {
                    p.attack += 1;
                    p.defense += 1;
                    CompleteGameSystem.Instance.AddCombatLog("★ 墙壁暗格：ATK+1 DEF+1");
                }
                else
                {
                    int heal = Mathf.FloorToInt(p.maxHp * 0.3f);
                    p.hp = Mathf.Min(p.maxHp, p.hp + heal);
                    p.pollution = Mathf.Max(0, p.pollution - 5);
                    CompleteGameSystem.Instance.AddCombatLog("★ 墙壁暗格：回复30%HP 污染-5%");
                }
            }
        });
    }

    public void CheckStoryTrigger(int floor)
    {
        if (!storyTriggers.ContainsKey(floor)) return;

        var trigger = storyTriggers[floor];
        if (trigger.type == "finale") return;

        var p = GameManager.Instance.Player;
        if (p.storyFlags.ContainsKey("floor_" + floor)) return;

        p.storyFlags["floor_" + floor] = true;

        // JS original: each story event awards EP from StoryData
        var storyEvent = System.Array.Find(StoryData.Events, e => e.floor == floor);
        if (storyEvent != null && storyEvent.epReward > 0)
        {
            p.evolutionPoints += storyEvent.epReward;
            CompleteGameSystem.Instance.AddCombatLog($"◆ +{storyEvent.epReward}EP");
        }

        CanvasUIManager.Instance.ShowStoryEvent(trigger);
    }

    public void CheckHiddenStory(string trigger)
    {
        var p = GameManager.Instance.Player;
        if (p == null || p.storyFlags == null) return;

        foreach (var ev in hiddenStoryEvents)
        {
            if (ev.triggerOn != null && ev.triggerOn != trigger) continue;
            if (ev.triggerOn == null && trigger != "move") continue;

            try
            {
                if (!ev.check()) continue;
            }
            catch { continue; }

            if (!ev.repeatable)
                p.storyFlags["hidden_" + ev.id] = true;

            CanvasUIManager.Instance.ShowHiddenStoryEvent(ev);
            break;
        }
    }

    public void ApplyStoryReward(string key)
    {
        if (string.IsNullOrEmpty(key)) return;

        var p = GameManager.Instance.Player;
        switch (key)
        {
            case "parasite_instinct":
                p.attack += 3;
                CompleteGameSystem.Instance.AddCombatLog("★ 寄生本能觉醒：ATK+3");
                break;

            case "echo_heal":
                p.maxHp += 20;
                p.hp = Mathf.Min(p.maxHp, p.hp + 10);
                CompleteGameSystem.Instance.AddCombatLog("♥ 记忆回声：MaxHP+20 HP+10");
                break;

            case "dead_relic":
                if (p.formSlots < 4)
                {
                    p.formSlots++;
                    CompleteGameSystem.Instance.AddCombatLog("□ 死者遗物：形态槽+1");
                }
                else
                {
                    p.evolutionPoints += 200;
                    CompleteGameSystem.Instance.AddCombatLog("□ 形态槽已满，获得200EP");
                }
                break;

            case "anchor_shield":
                p.collapseResistCharges++;
                CompleteGameSystem.Instance.AddCombatLog("◆ 意识锚定强化：获得1次崩溃抵抗");
                break;

            case "cognitive_split":
                p.defense += 2;
                CompleteGameSystem.Instance.AddCombatLog("☢️ 认知裂变：DEF+2");
                break;

            case "echo_power":
                p.evolutionPoints += 300;
                CompleteGameSystem.Instance.AddCombatLog("⚡ 残响之力：获得300EP");
                break;

            case "foresight":
                p.attack += 2;
                p.defense += 2;
                CompleteGameSystem.Instance.AddCombatLog("◉ 预知残像：ATK+2 DEF+2");
                break;

            case "memory_fusion":
                p.attack += 8;
                p.defense += 5;
                CompleteGameSystem.Instance.AddCombatLog("★ 记忆融合：ATK+8 DEF+5");
                break;

            case "recursive_awaken":
                p.maxHp += 50;
                p.hp = p.maxHp;
                CompleteGameSystem.Instance.AddCombatLog("◎ 递归觉醒：MaxHP+50，HP回满");
                break;
        }

        SaveSystem.Instance.AutoSave();
    }
}
