# Unity项目结构模板

## 寄生魔塔 (Parasite Tower) - Unity版本

### 项目基本信息
- **Unity版本**: 2022.3.62f2c1 (LTS)
- **渲染管线**: URP (Universal Render Pipeline)
- **物理引擎**: Unity 2D Physics
- **UI系统**: UGUI
- **目标平台**: Android / iOS / PC

---

## 一、推荐项目设置

### 1.1 Player Settings

```csharp
// Company Name: YourCompany
// Product Name: Parasite Tower
// Version: 1.0.0

// Android
// - Package Name: com.parasitetower.game
// - Minimum API Level: 24 (Android 7.0)
// - Target API Level: 34

// iOS
// - Bundle Identifier: com.parasitetower.game
// - Target Device: iPhone Only
// - Target Resolution: Portrait (9:16)
```

### 1.2 Quality Settings

```
Ultra Quality:
- Texture Quality: Full Res
- Anisotropic Textures: Per Texture
- Anti Aliasing: 4x
- Soft Particles: On
- Shadows: All

Medium Quality (Mobile):
- Texture Quality: Half Res
- Anisotropic Textures: Disabled
- Anti Aliasing: 2x
- Soft Particles: Off
- Shadows: Hard Shadows Only
```

---

## 二、推荐文件夹结构

```
Assets/
├── 🎯 Scripts/                      # 所有脚本
│   ├── 📦 Runtime/
│   │   ├── 🎮 Core/                 # 核心系统
│   │   │   ├── GameManager.cs      # 游戏总管理器
│   │   │   ├── EventBus.cs         # 事件总线
│   │   │   ├── SaveSystem.cs       # 存档系统
│   │   │   ├── ConfigManager.cs    # 配置管理
│   │   │   ├── PoolManager.cs      # 对象池
│   │   │   ├── AudioManager.cs     # 音频管理
│   │   │   ├── SceneLoader.cs      # 场景加载
│   │   │   ├── TutorialManager.cs  # 新手引导
│   │   │   ├── DataManager.cs      # 数据管理
│   │   │   ├── InputManager.cs     # 输入管理
│   │   │   └── TimeScaleManager.cs # 时间管理
│   │   │
│   │   ├── 👤 Player/              # 玩家系统
│   │   │   ├── PlayerController.cs # 玩家控制器
│   │   │   ├── PlayerMovement.cs   # 移动系统
│   │   │   ├── PlayerStats.cs      # 玩家属性
│   │   │   └── PlayerAnimator.cs   # 动画控制
│   │   │
│   │   ├── 🗺️ Map/                 # 地图系统
│   │   │   ├── TileMapGenerator.cs # 地图生成
│   │   │   ├── TileMapRenderer.cs  # 地图渲染
│   │   │   ├── FogOfWar.cs         # 战争迷雾
│   │   │   ├── FloorSignature.cs   # 楼层签名
│   │   │   ├── SpecialFloor.cs     # 特殊楼层
│   │   │   └── NavigationPath.cs  # 导航路径
│   │   │
│   │   ├── ⚔️ Combat/              # 战斗系统
│   │   │   ├── CombatManager.cs    # 战斗管理
│   │   │   ├── CombatState.cs      # 战斗状态
│   │   │   ├── ComboSystem.cs      # 连击系统
│   │   │   ├── EnemyIntent.cs     # 敌人意图
│   │   │   ├── DamageCalculator.cs # 伤害计算
│   │   │   ├── DefendSystem.cs     # 防御系统
│   │   │   ├── FleeSystem.cs       # 逃跑系统
│   │   │   └── CombatUI.cs         # 战斗UI
│   │   │
│   │   ├── 🔮 Forms/               # 形态系统
│   │   │   ├── FormManager.cs      # 形态管理
│   │   │   ├── PossessionSystem.cs # 附身系统
│   │   │   ├── EvolutionTree.cs    # 进化树
│   │   │   ├── FormBond.cs         # 羁绊系统
│   │   │   └── FormUI.cs           # 形态UI
│   │   │
│   │   ├── ☣️ Pollution/           # 污染系统
│   │   │   ├── PollutionSystem.cs  # 污染积累
│   │   │   ├── PollutionSkill.cs   # 污染技能
│   │   │   ├── BurstMode.cs        # 爆发态
│   │   │   ├── CollapsePenalty.cs  # 崩溃惩罚
│   │   │   └── PollutionVisual.cs  # 污染特效
│   │   │
│   │   ├── 🛡️ Class/               # 职业系统
│   │   │   ├── ClassManager.cs     # 职业管理
│   │   │   ├── TitanAbility.cs     # 泰坦技能
│   │   │   ├── GhostAbility.cs     # 幽灵技能
│   │   │   ├── SwarmAbility.cs     # 虫群技能
│   │   │   └── ClassUI.cs          # 职业UI
│   │   │
│   │   ├── 💎 Fragment/            # 碎片系统
│   │   │   ├── FragmentManager.cs  # 碎片管理
│   │   │   ├── FragmentSkill.cs    # 碎片技能
│   │   │   ├── FragmentData.cs     # 碎片数据
│   │   │   └── FragmentUI.cs       # 碎片UI
│   │   │
│   │   ├── 👹 Monster/             # 怪物系统
│   │   │   ├── MonsterBase.cs      # 怪物基类
│   │   │   ├── MonsterSpawner.cs   # 怪物生成
│   │   │   ├── MonsterAI.cs        # 怪物AI
│   │   │   ├── BossManager.cs      # BOSS管理
│   │   │   ├── EliteMonster.cs     # 精英怪
│   │   │   └── MonsterData.cs      # 怪物数据
│   │   │
│   │   ├── 🔊 Audio/              # 音频系统
│   │   │   ├── BGAudioManager.cs   # BGM管理
│   │   │   ├── SFXManager.cs       # 音效管理
│   │   │   ├── AreaReverb.cs       # 区域混响
│   │   │   └── PollutionAudio.cs  # 污染音效
│   │   │
│   │   ├── 🎨 UI/                 # UI系统
│   │   │   ├── UIManager.cs       # UI总管理
│   │   │   ├── PanelManager.cs    # 面板管理
│   │   │   ├── HUD.cs             # 游戏HUD
│   │   │   ├── MainMenuUI.cs      # 主菜单
│   │   │   ├── ShopUI.cs          # 商店UI
│   │   │   ├── InventoryUI.cs      # 背包UI
│   │   │   ├── SettingsUI.cs      # 设置UI
│   │   │   └── ModeSelectUI.cs    # 模式选择
│   │   │
│   │   ├── 📖 Story/              # 剧情系统
│   │   │   ├── StoryManager.cs    # 剧情管理
│   │   │   ├── DialogueSystem.cs  # 对话系统
│   │   │   └── StoryUI.cs         # 剧情UI
│   │   │
│   │   ├── 🏆 Social/             # 社交系统
│   │   │   ├── AchievementSystem.cs # 成就系统
│   │   │   ├── Leaderboard.cs     # 排行榜
│   │   │   ├── ShareManager.cs    # 分享系统
│   │   │   └── MetaProgress.cs    # 元进度
│   │   │
│   │   ├── ✨ Trait/             # 特质系统
│   │   │   ├── TraitSystem.cs     # 特质管理
│   │   │   ├── TraitHook.cs      # 特质钩子
│   │   │   └── TraitUI.cs        # 特质UI
│   │   │
│   │   ├── 🎮 GameMode/          # 游戏模式
│   │   │   ├── GameModeManager.cs # 模式管理
│   │   │   ├── ClassicMode.cs    # 经典模式
│   │   │   ├── ShortMode.cs      # 短局模式
│   │   │   ├── ExpeditionMode.cs # 远征模式
│   │   │   ├── DailyChallenge.cs # 每日挑战
│   │   │   └── WeeklyChallenge.cs # 每周挑战
│   │   │
│   │   └── 🛠️ Utils/             # 工具类
│   │       ├── Extensions.cs     # 扩展方法
│   │       ├── Constants.cs      # 常量定义
│   │       ├── CoroutineHelper.cs # 协程助手
│   │       └── Debugger.cs       # 调试工具
│   │
│   └── 📦 Editor/                  # 编辑器脚本
│       ├── LevelGeneratorEditor.cs
│       └── DataTableEditor.cs
│
├── 📦 ScriptableObjects/            # ScriptableObject数据
│   ├── 🧙 Monster/                 # 怪物数据
│   │   ├── MonsterData.cs        # 怪物数据模板
│   │   ├── BossData.cs           # BOSS数据
│   │   └── EliteData.cs          # 精英数据
│   │
│   ├── 🔮 Forms/                   # 形态数据
│   │   ├── FormData.cs           # 形态数据模板
│   │   ├── EvolutionData.cs      # 进化数据
│   │   └── BondData.cs           # 羁绊数据
│   │
│   ├── 💎 Fragment/               # 碎片数据
│   │   ├── FragmentData.cs       # 碎片数据模板
│   │   ├── ActiveSkillData.cs    # 主动技能
│   │   └── PassiveSkillData.cs   # 被动技能
│   │
│   ├── 🗺️ Map/                    # 地图数据
│   │   ├── FloorData.cs          # 楼层数据
│   │   ├── SignatureData.cs     # 签名数据
│   │   └── AreaTheme.cs         # 区域主题
│   │
│   ├── ⚙️ Config/                 # 游戏配置
│   │   ├── GameConfig.cs         # 游戏配置
│   │   ├── CombatConfig.cs       # 战斗配置
│   │   ├── BalanceConfig.cs      # 平衡配置
│   │   └── AudioConfig.cs        # 音频配置
│   │
│   ├── 📖 Story/                  # 剧情数据
│   │   ├── StoryData.cs          # 剧情数据
│   │   └── DialogueData.cs       # 对话数据
│   │
│   └── 🏆 Achievement/             # 成就数据
│       ├── AchievementData.cs    # 成就数据
│       └── RewardData.cs         # 奖励数据
│
├── 🎨 Art/                         # 美术资源
│   ├── 🧑 Characters/            # 角色资源
│   │   ├── Player/              # 玩家角色
│   │   ├── Forms/               # 形态立绘
│   │   └── Classes/             # 职业立绘
│   │
│   ├── 👹 Monsters/              # 怪物资源
│   │   ├── Normal/              # 普通怪
│   │   ├── Elite/               # 精英怪
│   │   ├── Boss/                # BOSS
│   │   └── Animations/          # 怪物动画
│   │
│   ├── 🏞️ Environment/          # 环境资源
│   │   ├── Tiles/              # 瓦片资源
│   │   ├── Backgrounds/         # 背景图
│   │   └── Effects/            # 环境特效
│   │
│   ├── 🎮 UI/                   # UI资源
│   │   ├── Icons/              # 图标
│   │   ├── Buttons/            # 按钮
│   │   ├── Panels/             # 面板背景
│   │   └── Fonts/             # 字体
│   │
│   └── ✨ Effects/              # 特效资源
│       ├── Particles/          # 粒子特效
│       ├── Shaders/            # Shader效果
│       └── Animations/        # UI动画
│
├── 🎵 Audio/                      # 音频资源
│   ├── 🎶 BGM/                   # 背景音乐
│   │   ├── Menu/
│   │   ├── Area1/
│   │   ├── Area2/
│   │   ├── Area3/
│   │   ├── Area4/
│   │   ├── Area5/
│   │   ├── Combat/
│   │   └── Boss/
│   │
│   ├── 🔊 SFX/                   # 音效
│   │   ├── Combat/
│   │   ├── UI/
│   │   ├── Player/
│   │   └── Environment/
│   │
│   └── 🌿 Ambient/                # 环境音
│
├── 🏠 Prefabs/                     # 预制体
│   ├── 👤 Player/                 # 玩家预制体
│   │   ├── Player.prefab
│   │   ├── Titan.prefab
│   │   ├── Ghost.prefab
│   │   └── Swarm.prefab
│   │
│   ├── 👹 Monsters/              # 怪物预制体
│   │   ├── Normal/
│   │   ├── Elite/
│   │   └── Boss/
│   │
│   ├── 🗺️ Map/                    # 地图预制体
│   │   ├── Tiles/
│   │   ├── FloorObjects/
│   │   └── SpecialObjects/
│   │
│   ├── 🎮 UI/                     # UI预制体
│   │   ├── Panels/
│   │   ├── Buttons/
│   │   └── Elements/
│   │
│   └── ✨ Effects/                # 特效预制体
│
├── 📄 Scenes/                      # 场景
│   ├── 🏠 MainMenu/
│   │   └── MainMenu.unity
│   │
│   ├── 🎮 Gameplay/
│   │   ├── FloorExplore.unity   # 探索场景
│   │   ├── Combat.unity         # 战斗场景
│   │   └── BossBattle.unity     # BOSS场景
│   │
│   ├── 📖 Story/
│   │   └── Prologue.unity       # 序章
│   │
│   └── 🏆 UI/
│       ├── Settings.unity
│       ├── Shop.unity
│       └── Leaderboard.unity
│
├── 📦 Resources/                    # 动态加载资源
│   ├── 📊 Data/                   # 数据表
│   │   ├── MonsterTable.json
│   │   ├── FormTable.json
│   │   ├── FragmentTable.json
│   │   └── FloorTable.json
│   │
│   └── 🗣️ Localization/           # 本地化
│       ├── zh.txt
│       └── en.txt
│
├── ⚙️ Settings/                    # 设置
│   ├── InputSettings.asset
│   ├── GraphicsSettings.asset
│   └── AudioSettings.asset
│
└── 📚 Documentation/                # 文档
    ├── API/
    ├── Architecture/
    └── ArtStyleGuide/
```

---

## 三、核心脚本示例

### 3.1 GameManager.cs (单例模式)

```csharp
using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : MonoBehaviour
{
    #region Singleton
    private static GameManager _instance;
    public static GameManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<GameManager>();
                if (_instance == null)
                {
                    var go = new GameObject("GameManager");
                    _instance = go.AddComponent<GameManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }
    #endregion

    #region Game State
    public enum GameState
    {
        MainMenu,
        Playing,
        Paused,
        Combat,
        Cutscene,
        GameOver
    }

    public GameState CurrentState { get; private set; }
    public event Action<GameState> OnGameStateChanged;
    #endregion

    #region Player Data
    [Serializable]
    public class PlayerData
    {
        public int level = 1;
        public int hp, maxHp;
        public int attack, defense;
        public float pollution;
        public string selectedClass;
        public List<string> ownedForms = new List<string>();
        public List<string> equippedFragments = new List<string>();
        // ... 更多玩家数据
    }

    public PlayerData Player { get; private set; } = new PlayerData();
    #endregion

    #region Game Progress
    public int CurrentFloor { get; set; } = 1;
    public int MaxFloor { get; set; } = 50;
    public GameMode CurrentMode { get; set; } = GameMode.Classic;
    public bool IsNewGame { get; set; } = true;
    #endregion

    #region Managers
    public CombatManager Combat { get; private set; }
    public FormManager Forms { get; private set; }
    public PollutionSystem Pollution { get; private set; }
    public SaveSystem Save { get; private set; }
    public AudioManager Audio { get; private set; }
    #endregion

    private void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }

        _instance = this;
        DontDestroyOnLoad(gameObject);

        InitializeManagers();
    }

    private void InitializeManagers()
    {
        Combat = GetComponent<CombatManager>() ?? gameObject.AddComponent<CombatManager>();
        Forms = GetComponent<FormManager>() ?? gameObject.AddComponent<FormManager>();
        Pollution = GetComponent<PollutionSystem>() ?? gameObject.AddComponent<PollutionSystem>();
        Save = GetComponent<SaveSystem>() ?? gameObject.AddComponent<SaveSystem>();
        Audio = GetComponent<AudioManager>() ?? gameObject.AddComponent<AudioManager>();
    }

    #region State Management
    public void ChangeState(GameState newState)
    {
        if (CurrentState == newState) return;

        var oldState = CurrentState;
        CurrentState = newState;
        OnGameStateChanged?.Invoke(newState);

        EventBus.Emit(EventTypes.GameStateChanged, oldState, newState);
    }

    public bool IsPlaying() => CurrentState == GameState.Playing;
    public bool IsInCombat() => CurrentState == GameState.Combat;
    public bool IsPaused() => CurrentState == GameState.Paused;
    #endregion

    #region Game Flow
    public void StartNewGame(string gameMode)
    {
        CurrentMode = ParseGameMode(gameMode);
        InitializePlayerData();
        CurrentFloor = 1;
        IsNewGame = true;

        ChangeState(GameState.Playing);
        SceneLoader.Instance.LoadScene("FloorExplore");
    }

    public void LoadGame(int slotIndex)
    {
        if (Save.Load(slotIndex))
        {
            IsNewGame = false;
            ChangeState(GameState.Playing);
            SceneLoader.Instance.LoadScene("FloorExplore");
        }
    }

    public void StartCombat(MonsterBase monster)
    {
        ChangeState(GameState.Combat);
        Combat.StartCombat(monster);
    }

    public void EndCombat(bool victory)
    {
        ChangeState(GameState.Playing);
        if (victory)
        {
            EventBus.Emit(EventTypes.CombatVictory);
        }
        else
        {
            EventBus.Emit(EventTypes.CombatDefeat);
        }
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);
        Save.AutoSave();
        Audio.PlaySFX("GameOver");
    }

    public void ReturnToMenu()
    {
        ChangeState(GameState.MainMenu);
        SceneLoader.Instance.LoadScene("MainMenu");
    }
    #endregion

    #region Helper Methods
    private void InitializePlayerData()
    {
        Player = new PlayerData
        {
            level = 1,
            maxHp = 100,
            hp = 100,
            attack = 10,
            defense = 5,
            pollution = 0f,
            selectedClass = "Titan",
            ownedForms = new List<string> { "Human" },
            equippedFragments = new List<string>()
        };
    }

    private GameMode ParseGameMode(string mode)
    {
        switch (mode.ToLower())
        {
            case "classic": return GameMode.Classic;
            case "short": return GameMode.Short;
            case "expedition": return GameMode.Expedition;
            default: return GameMode.Classic;
        }
    }
    #endregion

    #region Pause/Resume
    public void Pause()
    {
        if (CurrentState == GameState.Playing)
        {
            ChangeState(GameState.Paused);
            Time.timeScale = 0f;
        }
    }

    public void Resume()
    {
        if (CurrentState == GameState.Paused)
        {
            Time.timeScale = 1f;
            ChangeState(GameState.Playing);
        }
    }
    #endregion
}

public enum GameMode
{
    Classic,
    Short,
    Expedition,
    Daily,
    Weekly
}
```

### 3.2 EventBus.cs (事件系统)

```csharp
using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections.Generic;

public static class EventBus
{
    private static readonly Dictionary<string, UnityEventBase> events = new Dictionary<string, UnityEventBase>();
    private static readonly Dictionary<string, int> eventCounts = new Dictionary<string, int>();

    #region Event Registration
    public static void Register<T>(string eventName, Action<T> callback)
    {
        if (!events.ContainsKey(eventName))
        {
            events[eventName] = new UnityEvent<T>();
        }

        var evt = events[eventName] as UnityEvent<T>;
        evt.AddListener(callback);
        TrackEvent(eventName);
    }

    public static void Unregister<T>(string eventName, Action<T> callback)
    {
        if (events.ContainsKey(eventName))
        {
            var evt = events[eventName] as UnityEvent<T>;
            evt.RemoveListener(callback);
        }
    }

    public static void Register(string eventName, UnityAction callback)
    {
        if (!events.ContainsKey(eventName))
        {
            events[eventName] = new UnityEvent();
        }

        var evt = events[eventName] as UnityEvent;
        evt.AddListener(callback);
        TrackEvent(eventName);
    }

    public static void Unregister(string eventName, UnityAction callback)
    {
        if (events.ContainsKey(eventName))
        {
            var evt = events[eventName] as UnityEvent;
            evt.RemoveListener(callback);
        }
    }
    #endregion

    #region Event Emission
    public static void Emit(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            var evt = events[eventName] as UnityEvent;
            evt?.Invoke();
        }
    }

    public static void Emit<T>(string eventName, T param)
    {
        if (events.ContainsKey(eventName))
        {
            var evt = events[eventName] as UnityEvent<T>;
            evt?.Invoke(param);
        }
    }

    public static void Emit<T1, T2>(string eventName, T1 param1, T2 param2)
    {
        if (events.ContainsKey(eventName))
        {
            var evt = events[eventName] as UnityEvent<T1, T2>;
            evt?.Invoke(param1, param2);
        }
    }
    #endregion

    #region Event Management
    private static void TrackEvent(string eventName)
    {
        if (!eventCounts.ContainsKey(eventName))
        {
            eventCounts[eventName] = 0;
        }
        eventCounts[eventName]++;
    }

    public static void ClearAll()
    {
        events.Clear();
        eventCounts.Clear();
    }

    public static void ClearEvent(string eventName)
    {
        if (events.ContainsKey(eventName))
        {
            events.Remove(eventName);
            eventCounts.Remove(eventName);
        }
    }
    #endregion
}

public static class EventTypes
{
    public const string GameStateChanged = "GameStateChanged";
    public const string CombatVictory = "CombatVictory";
    public const string CombatDefeat = "CombatDefeat";
    public const string FloorChanged = "FloorChanged";
    public const string PollutionChanged = "PollutionChanged";
    public const string FormSwitched = "FormSwitched";
    public const string FragmentEquipped = "FragmentEquipped";
    public const string AchievementUnlocked = "AchievementUnlocked";
    // ... 更多事件类型
}

public delegate void UnityAction();
public delegate void UnityAction<T>(T arg);
public delegate void UnityAction<T1, T2>(T1 arg1, T2 arg2);
```

### 3.3 SaveSystem.cs (存档系统)

```csharp
using UnityEngine;
using System;
using System.IO;

public class SaveSystem : MonoBehaviour
{
    private const string SAVE_FOLDER = "Saves";
    private const int MAX_SAVE_SLOTS = 3;

    [Serializable]
    public class SaveData
    {
        public int version = 1;
        public DateTime saveTime;
        public string gameMode;
        public int currentFloor;
        public PlayerSaveData player;
        public InventorySaveData inventory;
        public ProgressSaveData progress;
        public AchievementSaveData achievements;
    }

    [Serializable]
    public class PlayerSaveData
    {
        public int hp, maxHp;
        public int attack, defense;
        public float pollution;
        public string selectedClass;
        public string currentForm;
    }

    [Serializable]
    public class InventorySaveData
    {
        public string[] ownedForms;
        public string[] equippedFragments;
        public int gold;
        public int fragments;
    }

    [Serializable]
    public class ProgressSaveData
    {
        public int highestFloor;
        public int totalWins;
        public int totalPlays;
        public string[] unlockedForms;
        public string[] unlockedSkills;
    }

    [Serializable]
    public class AchievementSaveData
    {
        public string[] unlockedAchievements;
        public int[] achievementProgress;
    }

    #region Public Methods
    public void AutoSave()
    {
        Save(0);
    }

    public bool Save(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {
            Debug.LogError($"Invalid save slot: {slotIndex}");
            return false;
        }

        try
        {
            var saveData = CreateSaveData();
            var json = JsonUtility.ToJson(saveData, true);
            var path = GetSavePath(slotIndex);

            EnsureSaveDirectory();
            File.WriteAllText(path, json);

            Debug.Log($"Game saved to slot {slotIndex}");
            EventBus.Emit(EventTypes.GameSaved, slotIndex);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Save failed: {e.Message}");
            return false;
        }
    }

    public bool Load(int slotIndex)
    {
        var path = GetSavePath(slotIndex);
        if (!File.Exists(path))
        {
            Debug.LogWarning($"No save found in slot {slotIndex}");
            return false;
        }

        try
        {
            var json = File.ReadAllText(path);
            var saveData = JsonUtility.FromJson<SaveData>(json);
            ApplySaveData(saveData);

            Debug.Log($"Game loaded from slot {slotIndex}");
            EventBus.Emit(EventTypes.GameLoaded, slotIndex);
            return true;
        }
        catch (Exception e)
        {
            Debug.LogError($"Load failed: {e.Message}");
            return false;
        }
    }

    public SaveData GetSaveInfo(int slotIndex)
    {
        var path = GetSavePath(slotIndex);
        if (!File.Exists(path))
        {
            return null;
        }

        try
        {
            var json = File.ReadAllText(path);
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteSave(int slotIndex)
    {
        var path = GetSavePath(slotIndex);
        if (File.Exists(path))
        {
            File.Delete(path);
            return true;
        }
        return false;
    }
    #endregion

    #region Private Methods
    private SaveData CreateSaveData()
    {
        var gm = GameManager.Instance;

        return new SaveData
        {
            version = 1,
            saveTime = DateTime.Now,
            gameMode = gm.CurrentMode.ToString(),
            currentFloor = gm.CurrentFloor,
            player = new PlayerSaveData
            {
                hp = gm.Player.hp,
                maxHp = gm.Player.maxHp,
                attack = gm.Player.attack,
                defense = gm.Player.defense,
                pollution = gm.Player.pollution,
                selectedClass = gm.Player.selectedClass,
                currentForm = gm.Player.ownedForms.Count > 0 ? gm.Player.ownedForms[0] : "Human"
            },
            inventory = new InventorySaveData
            {
                ownedForms = gm.Player.ownedForms.ToArray(),
                equippedFragments = gm.Player.equippedFragments.ToArray(),
                gold = 0,
                fragments = 0
            },
            progress = new ProgressSaveData
            {
                highestFloor = gm.MaxFloor,
                totalWins = 0,
                totalPlays = 1,
                unlockedForms = new string[0],
                unlockedSkills = new string[0]
            },
            achievements = new AchievementSaveData
            {
                unlockedAchievements = new string[0],
                achievementProgress = new int[0]
            }
        };
    }

    private void ApplySaveData(SaveData data)
    {
        var gm = GameManager.Instance;

        gm.CurrentMode = ParseGameMode(data.gameMode);
        gm.CurrentFloor = data.currentFloor;

        gm.Player.hp = data.player.hp;
        gm.Player.maxHp = data.player.maxHp;
        gm.Player.attack = data.player.attack;
        gm.Player.defense = data.player.defense;
        gm.Player.pollution = data.player.pollution;
        gm.Player.selectedClass = data.player.selectedClass;
        gm.Player.ownedForms = new System.Collections.Generic.List<string>(data.inventory.ownedForms);
        gm.Player.equippedFragments = new System.Collections.Generic.List<string>(data.inventory.equippedFragments);
    }

    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, $"save_{slotIndex}.json");
    }

    private void EnsureSaveDirectory()
    {
        var dir = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private GameMode ParseGameMode(string mode)
    {
        if (Enum.TryParse<GameMode>(mode, out var result))
        {
            return result;
        }
        return GameMode.Classic;
    }
    #endregion

    #region Auto Save
    private float autoSaveInterval = 30f;
    private float lastAutoSaveTime;

    private void Update()
    {
        if (GameManager.Instance.IsPlaying() && Time.time - lastAutoSaveTime > autoSaveInterval)
        {
            AutoSave();
            lastAutoSaveTime = Time.time;
        }
    }
    #endregion
}
```

### 3.4 TileMapGenerator.cs (地图生成)

```csharp
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using Random = UnityEngine.Random;

public class TileMapGenerator : MonoBehaviour
{
    [Header("Map Settings")]
    public int mapWidth = 13;
    public int mapHeight = 13;
    public int tileSize = 40;

    [Header("Tile Rules")]
    public float wallRatio = 0.3f;
    public int roomCount = 5;
    public int minRoomSize = 3;
    public int maxRoomSize = 5;

    [Header("Prefabs")]
    public TileBase floorTile;
    public TileBase wallTile;
    public GameObject exitPrefab;
    public GameObject[] monsterPrefabs;
    public GameObject[] itemPrefabs;

    [Header("References")]
    public Tilemap floorTilemap;
    public Tilemap wallTilemap;

    private int currentFloor;
    private List<Vector3Int> floorTiles = new List<Vector3Int>();
    private List<Vector3Int> wallTiles = new List<Vector3Int>();
    private List<Room> rooms = new List<Room>();

    [System.Serializable]
    private class Room
    {
        public Vector3Int center;
        public int width, height;
        public List<Vector3Int> tiles;
    }

    public void GenerateMap(int floor)
    {
        currentFloor = floor;
        ClearMap();

        int seed = GetFloorSeed(floor);
        Random.InitState(seed);

        GenerateRooms();
        ConnectRooms();
        PlaceWalls();
        AddDecorations();
        SpawnEntities();
        CreateExit();
    }

    private int GetFloorSeed(int floor)
    {
        return floor * 1337 + (int)System.DateTime.Now.Ticks % 1000;
    }

    private void GenerateRooms()
    {
        rooms.Clear();

        for (int i = 0; i < roomCount; i++)
        {
            int roomWidth = Random.Range(minRoomSize, maxRoomSize + 1);
            int roomHeight = Random.Range(minRoomSize, maxRoomSize + 1);
            int x = Random.Range(1, mapWidth - roomWidth - 1);
            int y = Random.Range(1, mapHeight - roomHeight - 1);

            Room room = new Room
            {
                center = new Vector3Int(x + roomWidth / 2, y + roomHeight / 2, 0),
                width = roomWidth,
                height = roomHeight,
                tiles = new List<Vector3Int>()
            };

            for (int rx = 0; rx < roomWidth; rx++)
            {
                for (int ry = 0; ry < roomHeight; ry++)
                {
                    var tile = new Vector3Int(x + rx, y + ry, 0);
                    room.tiles.Add(tile);
                    floorTiles.Add(tile);
                    floorTilemap.SetTile(tile, floorTile);
                }
            }

            rooms.Add(room);
        }
    }

    private void ConnectRooms()
    {
        for (int i = 0; i < rooms.Count - 1; i++)
        {
            var start = rooms[i].center;
            var end = rooms[i + 1].center;

            CreateCorridor(start, end);
        }
    }

    private void CreateCorridor(Vector3Int start, Vector3Int end)
    {
        int x = start.x;
        int y = start.y;

        while (x != end.x)
        {
            AddFloorTile(x, y);
            x += x < end.x ? 1 : -1;
        }

        while (y != end.y)
        {
            AddFloorTile(x, y);
            y += y < end.y ? 1 : -1;
        }
    }

    private void AddFloorTile(int x, int y)
    {
        var tile = new Vector3Int(x, y, 0);
        if (!floorTiles.Contains(tile))
        {
            floorTiles.Add(tile);
            floorTilemap.SetTile(tile, floorTile);
        }
    }

    private void PlaceWalls()
    {
        wallTiles.Clear();
        HashSet<Vector3Int> checkedTiles = new HashSet<Vector3Int>();

        foreach (var floor in floorTiles)
        {
            for (int dx = -1; dx <= 1; dx++)
            {
                for (int dy = -1; dy <= 1; dy++)
                {
                    var check = new Vector3Int(floor.x + dx, floor.y + dy, 0);
                    if (checkedTiles.Contains(check)) continue;
                    checkedTiles.Add(check);

                    if (!floorTiles.Contains(check) && IsInBounds(check))
                    {
                        wallTiles.Add(check);
                        wallTilemap.SetTile(check, wallTile);
                    }
                }
            }
        }
    }

    private bool IsInBounds(Vector3Int pos)
    {
        return pos.x >= 0 && pos.x < mapWidth && pos.y >= 0 && pos.y < mapHeight;
    }

    private void AddDecorations()
    {
        int decorationCount = Random.Range(3, 8);
        for (int i = 0; i < decorationCount; i++)
        {
            if (rooms.Count > 0)
            {
                var room = rooms[Random.Range(0, rooms.Count)];
                var pos = room.tiles[Random.Range(0, room.tiles.Count)];
                // 可以放置装饰物
            }
        }
    }

    private void SpawnEntities()
    {
        int monsterCount = GetMonsterCountForFloor(currentFloor);

        for (int i = 0; i < monsterCount; i++)
        {
            SpawnMonster();
        }
    }

    private void SpawnMonster()
    {
        if (floorTiles.Count == 0 || monsterPrefabs.Length == 0) return;

        var spawnTile = floorTiles[Random.Range(0, floorTiles.Count)];
        var prefab = monsterPrefabs[Random.Range(0, monsterPrefabs.Length)];

        Vector3 worldPos = floorTilemap.CellToWorld(spawnTile) + new Vector3(tileSize / 2f, tileSize / 2f, 0);
        Instantiate(prefab, worldPos, Quaternion.identity);
    }

    private int GetMonsterCountForFloor(int floor)
    {
        if (floor <= 10) return Random.Range(2, 4);
        if (floor <= 20) return Random.Range(3, 5);
        if (floor <= 30) return Random.Range(4, 6);
        if (floor <= 40) return Random.Range(5, 7);
        return Random.Range(6, 8);
    }

    private void CreateExit()
    {
        if (rooms.Count > 0)
        {
            var lastRoom = rooms[rooms.Count - 1];
            Vector3 worldPos = floorTilemap.CellToWorld(lastRoom.center) + new Vector3(tileSize / 2f, tileSize / 2f, 0);
            Instantiate(exitPrefab, worldPos, Quaternion.identity);
        }
    }

    private void ClearMap()
    {
        floorTilemap.ClearAllTiles();
        wallTilemap.ClearAllTiles();
        floorTiles.Clear();
        wallTiles.Clear();
        rooms.Clear();
    }

    public bool IsWalkable(Vector3 worldPos)
    {
        var cell = floorTilemap.WorldToCell(worldPos);
        return floorTiles.Contains(cell);
    }

    public List<Vector3Int> GetFloorTiles() => floorTiles;
    public List<Room> GetRooms() => rooms;
}
```

### 3.5 CombatManager.cs (战斗系统)

```csharp
using UnityEngine;
using System;
using System.Collections.Generic;

public class CombatManager : MonoBehaviour
{
    [Header("Combat Settings")]
    public float turnDelay = 0.5f;
    public int baseDamage = 10;
    public float critMultiplier = 1.5f;
    public float critChance = 0.15f;

    private MonsterBase currentEnemy;
    private bool isPlayerTurn = true;
    private int comboCount = 0;
    private List<string> combatLog = new List<string>();

    public enum CombatState
    {
        Idle,
        PlayerTurn,
        EnemyTurn,
        Animating,
        Victory,
        Defeat
    }

    public CombatState State { get; private set; } = CombatState.Idle;
    public event Action<CombatState> OnStateChanged;

    #region Combat Flow
    public void StartCombat(MonsterBase enemy)
    {
        currentEnemy = enemy;
        comboCount = 0;
        combatLog.Clear();

        State = CombatState.PlayerTurn;
        NotifyStateChanged();

        EventBus.Emit(EventTypes.CombatStarted, enemy);
        UpdateEnemyIntent();
    }

    public void EndCombat(bool victory)
    {
        State = victory ? CombatState.Victory : CombatState.Defeat;
        NotifyStateChanged();

        EventBus.Emit(victory ? EventTypes.CombatVictory : EventTypes.CombatDefeat);
        currentEnemy = null;
    }

    private void ChangeState(CombatState newState)
    {
        if (State == newState) return;
        State = newState;
        NotifyStateChanged();
    }
    #endregion

    #region Player Actions
    public void Attack()
    {
        if (State != CombatState.PlayerTurn) return;

        ChangeState(CombatState.Animating);

        float damage = CalculatePlayerDamage();
        bool isCrit = CheckCrit();
        int finalDamage = Mathf.RoundToInt(damage * (isCrit ? critMultiplier : 1f));

        currentEnemy.TakeDamage(finalDamage);
        AddCombatLog($"Attack dealt {finalDamage} damage{(isCrit ? " (CRITICAL!)" : "")}");

        comboCount++;
        ApplyComboBonus();

        StartCoroutine(AnimationDelay(() =>
        {
            if (currentEnemy.IsDead)
            {
                EndCombat(true);
            }
            else
            {
                ChangeState(CombatState.EnemyTurn);
                StartCoroutine(EnemyTurnDelay());
            }
        }));
    }

    public void Defend()
    {
        if (State != CombatState.PlayerTurn) return;

        ChangeState(CombatState.Animating);

        var player = GameManager.Instance.Player;
        player.defense *= 2;

        AddCombatLog($"Player defends! Defense doubled for this turn.");

        StartCoroutine(AnimationDelay(() =>
        {
            ChangeState(CombatState.EnemyTurn);
            StartCoroutine(EnemyTurnDelay());
        }));
    }

    public void UseSkill(string skillId)
    {
        if (State != CombatState.PlayerTurn) return;

        ChangeState(CombatState.Animating);

        var skill = DataManager.Instance.GetFragmentSkill(skillId);
        if (skill != null)
        {
            skill.Execute();
            AddCombatLog($"Used skill: {skill.Name}");
        }

        StartCoroutine(AnimationDelay(() =>
        {
            if (currentEnemy.IsDead)
            {
                EndCombat(true);
            }
            else
            {
                ChangeState(CombatState.EnemyTurn);
                StartCoroutine(EnemyTurnDelay());
            }
        }));
    }

    public void Flee()
    {
        if (State != CombatState.PlayerTurn) return;

        float fleeChance = CalculateFleeChance();
        bool success = Random.value < fleeChance;

        if (success)
        {
            AddCombatLog("Successfully fled!");
            EndCombat(false);
        }
        else
        {
            AddCombatLog("Failed to flee!");
            ChangeState(CombatState.EnemyTurn);
            StartCoroutine(EnemyTurnDelay());
        }
    }

    private float CalculateFleeChance()
    {
        float baseChance = 0.5f;
        int floor = GameManager.Instance.CurrentFloor;

        if (currentEnemy.Data.isBoss)
        {
            baseChance = 0.1f;
        }
        else if (floor > 30)
        {
            baseChance = 0.3f;
        }

        return baseChance;
    }
    #endregion

    #region Enemy Turn
    private System.Collections.IEnumerator EnemyTurnDelay()
    {
        yield return new WaitForSeconds(turnDelay);

        if (currentEnemy == null || currentEnemy.IsDead)
        {
            yield break;
        }

        EnemyIntent.IntentType intent = currentEnemy.GetCurrentIntent();
        ExecuteEnemyIntent(intent);
    }

    private void ExecuteEnemyIntent(EnemyIntent.IntentType intent)
    {
        ChangeState(CombatState.Animating);

        switch (intent)
        {
            case EnemyIntent.IntentType.Normal:
                ExecuteNormalAttack();
                break;
            case EnemyIntent.IntentType.Heavy:
                ExecuteHeavyAttack();
                break;
            case EnemyIntent.IntentType.Buff:
                currentEnemy.ApplyBuff();
                AddCombatLog($"{currentEnemy.Data.name} is powering up!");
                break;
            case EnemyIntent.IntentType.Heal:
                currentEnemy.Heal(Random.Range(5, 15));
                AddCombatLog($"{currentEnemy.Data.name} recovered health!");
                break;
        }

        StartCoroutine(AnimationDelay(() =>
        {
            ResetPlayerDefense();
            ChangeState(CombatState.PlayerTurn);
            UpdateEnemyIntent();
        }));
    }

    private void ExecuteNormalAttack()
    {
        int damage = CalculateEnemyDamage();
        GameManager.Instance.Player.hp -= damage;
        AddCombatLog($"Enemy attacked for {damage} damage!");

        EventBus.Emit(EventTypes.PlayerDamaged, damage);

        if (GameManager.Instance.Player.hp <= 0)
        {
            GameManager.Instance.Player.hp = 0;
            EndCombat(false);
        }
    }

    private void ExecuteHeavyAttack()
    {
        int damage = CalculateEnemyDamage() * 2;
        GameManager.Instance.Player.hp -= damage;
        AddCombatLog($"Enemy heavy attack for {damage} damage!");

        EventBus.Emit(EventTypes.PlayerDamaged, damage);

        if (GameManager.Instance.Player.hp <= 0)
        {
            GameManager.Instance.Player.hp = 0;
            EndCombat(false);
        }
    }
    #endregion

    #region Damage Calculation
    private float CalculatePlayerDamage()
    {
        var player = GameManager.Instance.Player;
        float damage = baseDamage + player.attack;

        float pollutionBonus = 1f + (player.pollution * 0.01f);
        damage *= pollutionBonus;

        return damage;
    }

    private int CalculateEnemyDamage()
    {
        if (currentEnemy == null) return 0;

        int baseDmg = currentEnemy.Data.baseAttack;
        int floor = GameManager.Instance.CurrentFloor;
        float scaling = 1f + (floor * 0.05f);

        int damage = Mathf.RoundToInt(baseDmg * scaling);
        damage -= GameManager.Instance.Player.defense;

        return Mathf.Max(1, damage);
    }

    private bool CheckCrit()
    {
        return Random.value < critChance;
    }

    private void ApplyComboBonus()
    {
        float bonus = GetComboBonus();
        if (bonus > 0)
        {
            AddCombatLog($"Combo {comboCount}: +{bonus * 100}% damage!");
        }
    }

    private float GetComboBonus()
    {
        if (comboCount >= 20) return 0.35f;
        if (comboCount >= 12) return 0.25f;
        if (comboCount >= 8) return 0.15f;
        if (comboCount >= 5) return 0.10f;
        if (comboCount >= 3) return 0.05f;
        return 0f;
    }
    #endregion

    #region Combat UI
    private void UpdateEnemyIntent()
    {
        if (currentEnemy != null)
        {
            currentEnemy.UpdateIntent();
        }
    }

    private void AddCombatLog(string message)
    {
        combatLog.Add(message);
        EventBus.Emit(EventTypes.CombatLogUpdated, message);
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(State);
    }

    private System.Collections.IEnumerator AnimationDelay(Action callback)
    {
        yield return new WaitForSeconds(0.3f);
        callback?.Invoke();
    }

    private void ResetPlayerDefense()
    {
        var player = GameManager.Instance.Player;
        player.defense /= 2;
    }
    #endregion

    #region Public Accessors
    public MonsterBase GetCurrentEnemy() => currentEnemy;
    public int GetComboCount() => comboCount;
    public List<string> GetCombatLog() => combatLog;
    #endregion
}
```

---

## 四、数据配置示例

### 4.1 MonsterData.cs (ScriptableObject)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "MonsterData", menuName = "ParasiteTower/Monster")]
public class MonsterData : ScriptableObject
{
    [Header("Basic Info")]
    public string monsterId;
    public string monsterName;
    public string description;
    public Sprite icon;
    public GameObject prefab;

    [Header("Stats")]
    public int baseHp = 50;
    public int baseAttack = 10;
    public int baseDefense = 5;
    public int baseSpeed = 10;

    [Header("Behavior")]
    public bool isBoss = false;
    public bool isElite = false;
    public float spawnWeight = 1f;

    [Header("Drops")]
    public int minGold = 10;
    public int maxGold = 20;
    public float fragmentDropChance = 0.2f;
    public string[] possibleFragmentDrops;

    [Header("AI")]
    public EnemyIntent.IntentType[] possibleIntents;
    public float[] intentWeights;

    [Header("Area Restriction")]
    public int minFloor = 1;
    public int maxFloor = 50;
    public string areaId;
}

[CreateAssetMenu(fileName = "BossData", menuName = "ParasiteTower/Boss")]
public class BossData : MonsterData
{
    [Header("Boss Specific")]
    public int phases = 2;
    public float[] phaseThreshold;
    public EnemyIntent.IntentType[] phaseIntents;
    public string[] phaseAttacks;
}
```

### 4.2 FragmentData.cs (碎片数据)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "FragmentData", menuName = "ParasiteTower/Fragment")]
public class FragmentData : ScriptableObject
{
    public string fragmentId;
    public string fragmentName;
    public FragmentType type;
    public Sprite icon;
    public string description;

    [Header("Stats")]
    public int attackBonus;
    public int defenseBonus;
    public int hpBonus;
    public float critBonus;

    [Header("Skills")]
    public bool hasActiveSkill;
    public string activeSkillId;
    public bool hasPassiveSkill;
    public string passiveSkillId;

    [Header("Requirements")]
    public int unlockFloor;
    public int cost;
    public bool isDLC;

    public enum FragmentType
    {
        Attack,
        Defense,
        Support,
        Special
    }
}
```

### 4.3 GameConfig.cs (游戏配置)

```csharp
using UnityEngine;

[CreateAssetMenu(fileName = "GameConfig", menuName = "ParasiteTower/Config/GameConfig")]
public class GameConfig : ScriptableObject
{
    [Header("Player Settings")]
    public int startingHp = 100;
    public int startingAttack = 10;
    public int startingDefense = 5;
    public int maxFragmentSlots = 4;

    [Header("Combat Settings")]
    public float baseCritChance = 0.15f;
    public float critMultiplier = 1.5f;
    public float defendMultiplier = 2f;
    public float fleeBaseChance = 0.5f;

    [Header("Pollution Settings")]
    public float pollutionGainPerTurn = 2f;
    public float pollutionDecayPerFloor = 5f;
    public float pollutionThreshold = 50f;
    public float burstModeMultiplier = 1.5f;

    [Header("Combo Settings")]
    public float combo3Bonus = 0.05f;
    public float combo5Bonus = 0.10f;
    public float combo8Bonus = 0.15f;
    public float combo12Bonus = 0.25f;
    public float combo20Bonus = 0.35f;

    [Header("Floor Settings")]
    public int classicModeFloorCount = 50;
    public int shortModeFloorCount = 12;
    public int expeditionModeFloorCount = 20;

    [Header("Difficulty Scaling")]
    public float enemyHpScaling = 0.05f;
    public float enemyAttackScaling = 0.05f;
    public float goldScaling = 0.03f;
}
```

---

## 五、常用工具类

### 5.1 Constants.cs

```csharp
public static class GameConstants
{
    #region Tags & Layers
    public const string PLAYER_TAG = "Player";
    public const string MONSTER_TAG = "Monster";
    public const string TILE_MAP_LAYER = "TileMap";
    #endregion

    #region Paths
    public const string SAVE_PATH = "Saves/";
    public const string DATA_PATH = "Data/";
    public const string PREFAB_PATH = "Prefabs/";
    #endregion

    #region Events
    public const string ON_PLAYER_MOVE = "OnPlayerMove";
    public const string ON_MONSTER_SPAWN = "OnMonsterSpawn";
    public const string ON_FLOOR_CHANGE = "OnFloorChange";
    public const string ON_COMBAT_START = "OnCombatStart";
    public const string ON_COMBAT_END = "OnCombatEnd";
    #endregion

    #region Animation
    public const float MOVE_DURATION = 0.2f;
    public const float ATTACK_DURATION = 0.3f;
    public const float DAMAGE_DURATION = 0.15f;
    #endregion

    #region Combat
    public const int MAX_COMBO = 99;
    public const float DEFAULT_CRIT = 0.15f;
    public const float DEFAULT_CRIT_MULT = 1.5f;
    #endregion
}
```

### 5.2 Extensions.cs

```csharp
using UnityEngine;

public static class Extensions
{
    #region Vector Extensions
    public static Vector3 GetGridPosition(this Vector3 worldPos, int tileSize)
    {
        return new Vector3(
            Mathf.FloorToInt(worldPos.x / tileSize) * tileSize,
            Mathf.FloorToInt(worldPos.y / tileSize) * tileSize,
            0
        );
    }

    public static Vector3 SnapToGrid(this Vector3 pos, int gridSize)
    {
        pos.x = Mathf.RoundToInt(pos.x / gridSize) * gridSize;
        pos.y = Mathf.RoundToInt(pos.y / gridSize) * gridSize;
        return pos;
    }
    #endregion

    #region Color Extensions
    public static Color WithAlpha(this Color color, float alpha)
    {
        color.a = alpha;
        return color;
    }

    public static string ToHex(this Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGB(color)}";
    }
    #endregion

    #region Array Extensions
    public static T RandomElement<T>(this T[] array)
    {
        if (array.Length == 0) return default(T);
        return array[Random.Range(0, array.Length)];
    }

    public static T RandomElement<T>(this System.Collections.Generic.List<T> list)
    {
        if (list.Count == 0) return default(T);
        return list[Random.Range(0, list.Count)];
    }
    #endregion

    #region Float Extensions
    public static bool Approximately(this float a, float b, float threshold = 0.001f)
    {
        return Mathf.Abs(a - b) < threshold;
    }
    #endregion
}
```

---

## 六、性能优化建议

### 6.1 对象池使用

```csharp
public class PoolManager : MonoBehaviour
{
    private Dictionary<string, Queue<GameObject>> pools = new Dictionary<string, Queue<GameObject>>();
    private Dictionary<string, GameObject> prefabs = new Dictionary<string, GameObject>();

    public void Preload(GameObject prefab, string poolId, int count)
    {
        if (!prefabs.ContainsKey(poolId))
        {
            prefabs[poolId] = prefab;
            pools[poolId] = new Queue<GameObject>();
        }

        for (int i = 0; i < count; i++)
        {
            var obj = Instantiate(prefab);
            obj.SetActive(false);
            pools[poolId].Enqueue(obj);
        }
    }

    public GameObject Spawn(string poolId, Vector3 position, Quaternion rotation)
    {
        if (!pools.ContainsKey(poolId))
        {
            Debug.LogError($"Pool {poolId} not found!");
            return null;
        }

        GameObject obj;
        if (pools[poolId].Count > 0)
        {
            obj = pools[poolId].Dequeue();
        }
        else
        {
            obj = Instantiate(prefabs[poolId]);
        }

        obj.transform.position = position;
        obj.transform.rotation = rotation;
        obj.SetActive(true);
        return obj;
    }

    public void Despawn(string poolId, GameObject obj)
    {
        obj.SetActive(false);
        pools[poolId].Enqueue(obj);
    }
}
```

### 6.2 协程工具

```csharp
public class CoroutineHelper : MonoBehaviour
{
    private static CoroutineHelper _instance;
    private static CoroutineHelper Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = new GameObject("CoroutineHelper").AddComponent<CoroutineHelper>();
                DontDestroyOnLoad(_instance);
            }
            return _instance;
        }
    }

    public new static Coroutine Start(IEnumerator routine)
    {
        return Instance.StartCoroutine(routine);
    }

    public new static void Stop(Coroutine routine)
    {
        Instance.StopCoroutine(routine);
    }

    public static Coroutine DelayedCall(float delay, Action callback)
    {
        return Start(DelayedCallRoutine(delay, callback));
    }

    private static IEnumerator DelayedCallRoutine(float delay, Action callback)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }
}
```

---

## 七、项目启动检查清单

### 开发环境
- [ ] Unity 2022.3.62f2c1 安装
- [ ] URP 包安装
- [ ] 版本控制设置 (Git)
- [ ] 项目规范文档

### 核心框架
- [ ] GameManager 单例
- [ ] EventBus 事件系统
- [ ] SaveSystem 存档系统
- [ ] PoolManager 对象池
- [ ] SceneLoader 场景加载

### 美术规范
- [ ] 角色立绘规格 (1920x1080)
- [ ] 瓦片规格 (64x64)
- [ ] UI 规格 (1080x1920)
- [ ] 动画帧率 (12 FPS)
- [ ] 导出格式规范

### 测试环境
- [ ] 真机测试设备
- [ ] 性能分析工具
- [ ] 崩溃日志系统
- [ ] 自动化测试框架

---

**创建日期**: 2026-05-29
**版本**: v1.0
