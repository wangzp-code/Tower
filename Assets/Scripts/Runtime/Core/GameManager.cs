using UnityEngine;
using System;
using System.Collections.Generic;

public class GameManager : SingletonBase<GameManager>
{

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

    public GameState CurrentState { get; private set; } = GameState.MainMenu;
    public event Action<GameState> OnGameStateChanged;
    #endregion

    #region Player Data
    [Serializable]
    public class PlayerData
    {
        public int level = 1;
        public int hp = 100;
        public int maxHp = 100;
        public int attack = 10;
        public int defense = 5;
        public float pollution;
        public string selectedClass = "titan";
        public string currentFormId = "human";
        public List<string> ownedForms = new List<string>();
        public List<string> equippedFragments = new List<string>();
        public List<string> seenMonsterTypes = new List<string>();

        public float possessionBonus;
        public int evolutionPoints;
        public int gold;
        public int fragments;
        public int rebirthCount;
        public int formSlots = 3;
        public List<bool> deadForms = new List<bool>();
        public int collapseResistCharges;
        public int deathReviveCharges;
        public bool noDeathRun = true;

        public int baseMaxHp = 100;
        public int baseAttack = 10;
        public int baseDefense = 5;
        public int classFogRadius = 5;

        public float switchShieldTurns;
        public int switchCooldownTurns;
        public int defendCountThisCombat;
        public int switchCountThisCombat;
        public int possessionCountThisRun;
        public int totalKillsThisRun;
        public int turnsInCombat;
        public bool defendedLastTurn;
        public bool formBroken;

        // 遗产系统属性
        public float attackBonus;
        public float defenseBonus;
        public float regenPerTurn;
        public float poisonChance;
        public float lifesteal;
        public float critRate;

        // 死亡报告统计
        public int monstersKilled;
        public int totalDamageDealt;

        // Run statistics for iteration report
        public float runStartTime;
        public float maxPollutionReached;
        public string longestFormId = "";
        public float longestFormDuration;
        public float currentFormStartTime;
        public int runNumber;

        public Dictionary<string, int> formResonanceLevels = new Dictionary<string, int>();
        public Dictionary<string, int> formHpMap = new Dictionary<string, int>();
        public Dictionary<string, int> formMaxHpMap = new Dictionary<string, int>();

        // 新增字段
        public bool hasBloodMoon;
        public float tempRegenCombat;
        public float permRegen;
        public int extraRevive;
        public int deathBlast;
        public List<string> traits = new List<string>();
        public bool hasRevived;
        public int formBondCount;
        public Dictionary<string, int> formBondCounts = new Dictionary<string, int>();
        public Dictionary<string, int> formSlotLevels = new Dictionary<string, int>();
        public Dictionary<string, bool> storyFlags = new Dictionary<string, bool>();
        public Dictionary<string, int> evolution = new Dictionary<string, int>();
        public bool deathSaveAvailable;

        public string playerClass => selectedClass;
    }

    public PlayerData Player { get; private set; } = new PlayerData();
    #endregion

    #region Game Progress
    public int CurrentFloor { get; set; } = 1;
    public int MaxFloor { get; set; } = 50;
    public GameMode CurrentMode { get; set; } = GameMode.Classic;
    public bool IsNewGame { get; set; } = true;
    public int CurrentStage { get; set; } = 1;
    public int MaxStage { get; set; } = 10;
    public ExpeditionRunData ExpeditionData { get; set; }
    #endregion

    #region Managers
    public CombatManager Combat { get; private set; }
    public FormManager Forms { get; private set; }
    public PollutionSystem Pollution { get; private set; }
    public SaveSystem Save { get; private set; }
    public AudioManager Audio { get; private set; }
    #endregion

    protected override void Awake()
    {
        base.Awake();

        InitializeFXSystems();
        EnsureAudioListener();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (this == Instance)
        {
            OnGameStateChanged = null;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (this == Instance)
        {
            OnGameStateChanged = null;
        }
    }

    // 如果场景中没有 AudioListener，则尝试附加到主摄像机或创建一个全局的 AudioListener
    void EnsureAudioListener()
    {
        try
        {
            var existing = FindObjectOfType<AudioListener>();
            if (existing != null) return;

            // 尝试把 AudioListener 附到主摄像机
            if (Camera.main != null)
            {
                var camGo = Camera.main.gameObject;
                if (camGo.GetComponent<AudioListener>() == null)
                {
                    camGo.AddComponent<AudioListener>();
                }
            }
            else
            {
                // 若没有主摄像机，创建一个常驻的 AudioListener 对象
                var alGo = new GameObject("AudioListener");
                alGo.AddComponent<AudioListener>();
                DontDestroyOnLoad(alGo);
            }
        }
        catch (Exception)
        {
            // 忽略任何反射/编辑器上下文异常，保证不抛出
        }
    }

    private void InitializeFXSystems()
    {
        // 确保 ScreenEffectsManager 存在
        if (ScreenEffectsManager.Instance == null)
        {
            GameObject fxGo = new GameObject("ScreenEffectsManager");
            fxGo.AddComponent<ScreenEffectsManager>();
            DontDestroyOnLoad(fxGo);
        }

        // 确保 PollutionOverlay 存在
        if (PollutionOverlay.Instance == null)
        {
            GameObject pollGo = new GameObject("PollutionOverlay");
            pollGo.AddComponent<PollutionOverlay>();
            DontDestroyOnLoad(pollGo);
        }

        if (GuestAuthManager.Instance == null)
        {
            var authGo = new GameObject("GuestAuthManager");
            authGo.AddComponent<GuestAuthManager>();
            DontDestroyOnLoad(authGo);
        }
    }

    private void Start()
    {
        InitializeManagers();
    }

    private void InitializeManagers()
    {
        Combat = CombatManager.Instance;
        Forms = FormManager.Instance;
        Pollution = PollutionSystem.Instance;
        Save = SaveSystem.Instance;
        Audio = AudioManager.Instance;
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
        MaxFloor = GetModeFloorCount(CurrentMode);

        if (CurrentMode == GameMode.Expedition)
        {
            CurrentStage = 1;
            MaxStage = 10;
            ExpeditionData = new ExpeditionRunData();
        }
        else
        {
            CurrentStage = 1;
            ExpeditionData = null;
        }

        CreateNewPlayer();
        CurrentFloor = 1;
        IsNewGame = true;

        ChangeState(GameState.Playing);
        ShowGameScreen();

        bool isFirstRun = PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
        if (isFirstRun)
        {
            Analytics.Track("first_run_start", ("mode", gameMode), ("timestamp", DateTime.Now.Ticks));
        }
    }

    private int GetModeFloorCount(GameMode mode)
    {
        var config = ConfigManager.Instance != null ? ConfigManager.Instance.GetGameConfig() : null;
        if (config == null)
        {
            switch (mode)
            {
                case GameMode.Short: return 12;
                case GameMode.Expedition: return 20;
                case GameMode.Daily: return 20;
                case GameMode.Weekly: return 30;
                default: return 50;
            }
        }

        switch (mode)
        {
            case GameMode.Short:
                return config.shortModeFloorCount;
            case GameMode.Expedition:
                return config.expeditionModeFloorCount;
            case GameMode.Daily:
                return 20;
            case GameMode.Weekly:
                return 30;
            default:
                return config.classicModeFloorCount;
        }
    }

    private void ShowGameScreen()
    {
        CompleteGameSystem gameSystem = FindObjectOfType<CompleteGameSystem>();
        if (gameSystem == null)
        {
            GameObject systemObj = new GameObject("CompleteGameSystem");
            gameSystem = systemObj.AddComponent<CompleteGameSystem>();
            DontDestroyOnLoad(systemObj);
        }

        if (IsNewGame)
        {
            gameSystem.StartRun(CurrentMode);
        }
        else
        {
            gameSystem.ResumeRun(CurrentMode);
        }
    }

    public void StartGame(string gameMode)
    {
        StartNewGame(gameMode);
    }

    public void LoadGame(int slotIndex)
    {
        if (SaveSystem.Instance.Load(slotIndex))
        {
            IsNewGame = false;
            ChangeState(GameState.Playing);
            ShowGameScreen();
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
        EventBus.Emit(victory ? EventTypes.CombatVictory : EventTypes.CombatDefeat);
    }

    public void GameOver()
    {
        ChangeState(GameState.GameOver);

        bool isFirstRun = PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
        if (isFirstRun)
        {
            var player = Player;
            Analytics.Track("first_run_end", 
                ("floor", CurrentFloor),
                ("stage", CurrentStage),
                ("class", player?.selectedClass ?? "unknown"),
                ("possession_count", player?.possessionCountThisRun ?? 0),
                ("max_pollution", player?.pollution ?? 0),
                ("final_hp", player?.hp ?? 0),
                ("final_atk", player?.attack ?? 0),
                ("final_def", player?.defense ?? 0));
        }
        SaveSystem.Instance?.DeleteSave(0);
    }

    public void ReturnToMenu()
    {
        ChangeState(GameState.MainMenu);
    }

    public void AdvanceExpeditionStage()
    {
        if (ExpeditionData == null) return;
        CurrentStage++;
        ExpeditionData.currentStage = CurrentStage;
        CreateNewPlayer();
        CurrentFloor = 1;
    }
    #endregion

    #region Player Helpers
    public void SetClassBaseStats(string classId, int hp, int atk, int def, int fogRadius)
    {
        if (classId.StartsWith("t_")) classId = classId.Substring(2);
        Player.selectedClass = classId;
        Player.baseMaxHp = hp;
        Player.baseAttack = atk;
        Player.baseDefense = def;
        Player.classFogRadius = fogRadius;
        Player.currentFormId = "human";
        Player.maxHp = hp;
        Player.hp = hp;
        Player.attack = atk;
        Player.defense = def;
    }

    public void NotifyPlayerStatsChanged()
    {
        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    public void ResetCombatCounters()
    {
        Player.defendCountThisCombat = 0;
        Player.switchCountThisCombat = 0;
        Player.turnsInCombat = 0;
        Player.defendedLastTurn = false;
    }
    #endregion

    #region Helper Methods
    public void CreateNewPlayer()
    {
        Player = new PlayerData
        {
            level = 1,
            maxHp = 100,
            hp = 100,
            attack = 10,
            defense = 5,
            pollution = 0f,
            selectedClass = "titan",
            currentFormId = "human",
            ownedForms = new List<string> { "human" },
            equippedFragments = new List<string>(),
            seenMonsterTypes = new List<string>(),
            possessionBonus = 0f,
            evolutionPoints = 0,
            gold = 0,
            formSlots = 3,
            collapseResistCharges = 0,
            deathReviveCharges = 0,
            noDeathRun = true,
            baseMaxHp = 100,
            baseAttack = 10,
            baseDefense = 5,
            classFogRadius = 5,
            formBondCounts = new Dictionary<string, int>(),
            formSlotLevels = new Dictionary<string, int>(),
            formHpMap = new Dictionary<string, int>
            {
                { "human", 100 }
            }
        };

        if (CurrentMode == GameMode.Expedition && ExpeditionData != null && ExpeditionData.currentStage > 1)
        {
            Player.attack += ExpeditionData.permAtkBonus;
            Player.baseAttack += ExpeditionData.permAtkBonus;
            Player.defense += ExpeditionData.permDefBonus;
            Player.baseDefense += ExpeditionData.permDefBonus;
            Player.maxHp += ExpeditionData.permMaxHpBonus;
            Player.hp = Player.maxHp;
            Player.baseMaxHp += ExpeditionData.permMaxHpBonus;
            Player.evolutionPoints = ExpeditionData.startEpBonus;
            Player.possessionBonus = ExpeditionData.possessRateBonus;
            Player.permRegen = ExpeditionData.permRegenBonus;
        }

        // 残响圣坛永久加成
        int altarHp = PlayerPrefs.GetInt("pt_altar_hp", 0);
        int altarAtk = PlayerPrefs.GetInt("pt_altar_atk", 0);
        int altarFormSlot = PlayerPrefs.GetInt("pt_altar_form_slot", 0);
        int altarEvoRate = PlayerPrefs.GetInt("pt_altar_evo_rate", 0);
        Player.maxHp += altarHp * 5;
        Player.hp = Player.maxHp;
        Player.baseMaxHp += altarHp * 5;
        Player.attack += altarAtk;
        Player.baseAttack += altarAtk;
        Player.formSlots += altarFormSlot;
        Player.evolutionPoints += altarEvoRate * 10;
    }

    private GameMode ParseGameMode(string mode)
    {
        if (Enum.TryParse(mode, true, out GameMode result))
        {
            return result;
        }
        return GameMode.Classic;
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

[System.Serializable]
public class ExpeditionRunData
{
    public int currentStage = 1;
    public List<string> chosenBuffs = new List<string>();
    public int permAtkBonus;
    public int permDefBonus;
    public int permMaxHpBonus;
    public float permRegenBonus;
    public int startEpBonus;
    public float possessRateBonus;
}
