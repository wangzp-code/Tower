using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

public partial class CanvasUIManager : MonoBehaviour
{
    public static CanvasUIManager Instance { get; private set; }

    // Simple main-thread action queue for background tasks to schedule Unity API work.
    static readonly Queue<Action> _mainThreadActions = new Queue<Action>();
    public static void RunOnMainThread(Action a)
    {
        if (a == null) return;
        lock (_mainThreadActions) _mainThreadActions.Enqueue(a);
    }

    static Font _font;
    static Font F()
    {
        if (_font != null) return _font;
        _font = ThemeUIFonts.Get(16);
        return _font;
    }

    Canvas _canvas;
    RectTransform _root;
    bool _ready;

    // Panel roots
    GameObject _menuPanel, _classPanel, _explorePanel, _combatPanel, _overPanel, _endPanel, _stageTransPanel;
    GameObject _avatarBtn;
    // Login panel
    GameObject _loginPanel;
    InputField _loginNickInput;
    // Profile panel
    GameObject _profileOvl;
    Text _profNick, _profId, _profGames, _profFloor, _profKills, _profPossess;
    // Menu top bar
    Text _menuPlayerName, _menuPlayerLevel;
    Texture2D _avatarTex;
    static readonly string[] HeroIconPaths = { "Icons/Classes/c_titan", "Icons/Classes/c_swarm", "Icons/Classes/c_ghost" };
    static readonly string[] HeroNames = { "泰坦", "虫群", "幽灵" };
    Text _menuEpTxt, _menuFragTxt;
    RawImage _eFloorBg;
    Image _eBgShaderOverlay;  // ParasiteBackground 动态叠加层
    int _eFloorBgIdx = -1;
    GameObject _floorBanner;
    CanvasGroup _floorBannerCG;
    Image _ePlayerGlow;   // 玩家光标光晕 CircleSprite
    Image _eExitGlow1, _eExitGlow2;   // 出口脉冲圈 (2 层)
    Vector2Int _lastPlayerGlowPos = new Vector2Int(-1, -1);
    Vector2Int _lastExitGlowPos = new Vector2Int(-1, -1);
    Coroutine _eFxPulseCoroutine;
    GameObject _altarOvl, _echoAltarOvl, _collapseOvl, _possessOvl, _routeOvl, _deathFormOvl, _storyOvl, _fragChoiceOvl;
    GameObject _storyBlocker, _storyPanel;
    GameObject _deathRollbackOvl;
    GameObject _formReplaceOvl;
    GameObject _tutorialOvl;
    GameObject _chalOvl;
    GameObject _synthConfirmOvl;
    Text _synthTitle, _synthDesc, _synthPreview;
    Transform _skillBarRow;
    Button[] _skillBarBtns = new Button[3];
    Text[] _skillBarLabels = new Text[3];
    Text[] _skillBarUses = new Text[3];
    Image _cPollVignette;
    Image _cPollPulse;
    Image _cBgShaderOverlay;   // 战斗界面 ParasiteBackground 动态背景
    int _cBgShaderFloor = -1;
    Image _cPollScanlines;
    float _lastPollShakeTime = -999f;
    Coroutine _evoRedDotCoroutine;
    CompleteGameSystem.RunScreen _lastScreen = (CompleteGameSystem.RunScreen)(-1);
    CompleteGameSystem.RunScreen _prevScreen = (CompleteGameSystem.RunScreen)(-1);

    // Explore refs
    Text _eMsg, _eFloor, _eStats, _eHpTxt, _ePollTxt, _eProg, _eTimer, _eTutTxt, _eEvoTxt, _eFormName, _eFragments, _eEvoLvl;
    Text _eTutTitle, _eTutStep;
    GameObject _eTutSkipBtn;
    Image[,] _mapImg = new Image[13,13];
    Outline[,] _mapCellOutline = new Outline[13,13];
    Shadow[,] _mapCellShadow = new Shadow[13,13];
    Text[,] _mapTxt = new Text[13,13];
    Image[,] _mapIcon = new Image[13,13];
    Outline[,] _mapOutline = new Outline[13,13];
    Image[,] _mapBadge = new Image[13,13];
    Image[,] _mapFog = new Image[13,13];
    string[,] _mapLastGlyph = new string[13,13];
    bool _mapLogged;
    bool _iconLogged;
    HashSet<string> _iconFailed = new HashSet<string>();
    bool _mapIconSized;
    int _lastExploreFloor = -1;
    GameObject _eTutGo;
    GameObject _softHintBubble;
    Text _softHintText;
    RectTransform _softHintArrow;
    // Explore host bar
    GameObject _eHostBar, _ePossessBadge;
    Transform _eFormRow;
    Text _eHostName, _eHostTraits;
    // Explore action buttons
    GameObject _eEvoBtn, _eSaveBtn, _eSkillBtn, _eMenuBtn;
    // Tutorial guide system
    GameObject _guideBubble;
    Text _guideBubbleText;
    Image _guideBubbleArrow;
    RectTransform _guideBubbleRT;
    float _guideShowTime;
    string _guideTargetId;
    Coroutine _guideCoroutine;
    int _guideFindRetries;
    const int MaxGuideFindRetries = 10;

    // Guide target refs (removed, using dynamic lookup instead)

    // Combat refs
    Text _cPName, _cPHpTxt, _cPStat, _cEName, _cEHpTxt, _cEStat, _cRate, _cMsg;
    ScrollRect _cLogScroll;
    RectTransform _cLogContentRT;
    Text _cBtmHud;
    Image _cPIcon, _cPHpFill, _cEIcon, _cEHpFill;
    Transform _cFormRow;
    int _lastPlayerHp = -1, _lastEnemyHp = -1;
    float _playerHpDisplay = 1f, _enemyHpDisplay = 1f;
    int _lastPlayerAtk = -1, _lastPlayerDef = -1;
    bool _ultPulsing;
    Coroutine _ultPulse;

    // Over/End
    Text _oStats, _oMsg, _enTitle, _enSub, _enBody;

    // Altar
    Text _alAggN, _alAggD, _alConN, _alConD;

    // Achievement toast
    GameObject _achToast;
    Text _achToastIcon, _achToastName, _achToastDesc;
    Queue<AchievementData> _achToastQueue = new Queue<AchievementData>();

    // Tutorial Toast & Progress
    GameObject _toastContainer;
    RectTransform _toastContainerRT;
    GameObject _tutorialProgressBar;
    Image _tutorialProgressFill;
    Text _tutorialProgressText;
    Text _tutorialStageText;
    Vector2 _tutorialProgressBarPos;

    // 主色调 - 生物朋克配色 (来自 ParasiteTowerColorScheme)
    static readonly Color Cyan = ParasiteTowerColorScheme.HealthGreen;
    static readonly Color Mag = ParasiteTowerColorScheme.CriticalRed;
    static readonly Color Purp = ParasiteTowerColorScheme.AbyssPurple;
    static readonly Color Gold = ParasiteTowerColorScheme.AccentGold;

    // 文本颜色
    static readonly Color Dim = ParasiteTowerColorScheme.MidGray;
    static readonly Color Bright = ParasiteTowerColorScheme.White;
    static readonly Color DarkText = ParasiteTowerColorScheme.LightGray;

    // 背景颜色
    static readonly Color PanelBg = ParasiteTowerColorScheme.UiPanelBg;
    static readonly Color CardBg = ParasiteTowerColorScheme.UiCardBg;
    static readonly Color CardBgHover = ParasiteTowerColorScheme.UiCardBgHover;
    static readonly Color BtnBg = ParasiteTowerColorScheme.UiBtnBg;
    static readonly Color BtnBgHover = ParasiteTowerColorScheme.UiBtnBgHover;

    // 边框和发光
    static readonly Color BorderColor = ParasiteTowerColorScheme.UiBorder;
    static readonly Color GlowColor = ParasiteTowerColorScheme.BioGlowCyan;
    static readonly Color HighlightColor = ParasiteTowerColorScheme.HealthGreen;

    // 高频复用颜色
    static readonly Color SlateText = ParasiteTowerColorScheme.UiSlateText;
    static readonly Color SteelBlue = ParasiteTowerColorScheme.UiSteelBlue;
    static readonly Color DarkItemBg = ParasiteTowerColorScheme.UiPanelBg;
    static readonly Color OvlBg = ParasiteTowerColorScheme.UiOverlayBg;
    static readonly Color OvlBgDense = ParasiteTowerColorScheme.UiOverlayBgDense;
    static readonly Color Mask70 = new Color(0, 0, 0, 0.7f);
    static readonly Color Mask80 = new Color(0, 0, 0, 0.8f);
    static readonly Color SoftRed = ParasiteTowerColorScheme.UiSoftRed;
    static readonly Color BrightRed = ParasiteTowerColorScheme.UiBrightRed;
    static readonly Color LightRed = ParasiteTowerColorScheme.CriticalRed;
    static readonly Color MutedPurp = ParasiteTowerColorScheme.UiMutedPurple;
    static readonly Color DarkStatText = ParasiteTowerColorScheme.UiDarkStat;

    // 状态颜色
    static readonly Color SuccessGreen = ParasiteTowerColorScheme.UiSuccessGreen;
    static readonly Color UnlockedCardBg = new Color(0.05f, 0.15f, 0.1f);
    static readonly Color CanUnlockCardBg = new Color(0.18f, 0.14f, 0.08f);
    static readonly Color LockedCardBg = ParasiteTowerColorScheme.UiPanelBg;
    static readonly Color CyanBorder15 = new Color(ParasiteTowerColorScheme.HealthGreen.r, ParasiteTowerColorScheme.HealthGreen.g, ParasiteTowerColorScheme.HealthGreen.b, 0.15f);
    static readonly Color CyanBorder25 = new Color(ParasiteTowerColorScheme.HealthGreen.r, ParasiteTowerColorScheme.HealthGreen.g, ParasiteTowerColorScheme.HealthGreen.b, 0.25f);

    // 间距令牌
    const float SpaceTiny = 4f;
    const float SpaceSmall = 8f;
    const float SpaceMed = 16f;
    const float SpaceLarge = 24f;

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EventBus.Register(EventTypes.PlayerStatsChanged, OnPlayerStatsChanged);
        EventBus.Register(EventTypes.RunStart, OnRunStart);
        EventBus.Register<string>(EventTypes.ShowHint, OnShowHint);
        EventBus.Register<string, string>(EventTypes.ShowButtonGuide, OnShowButtonGuide);
    }

    void OnDestroy()
    {
        EventBus.Unregister(EventTypes.PlayerStatsChanged, OnPlayerStatsChanged);
        EventBus.Unregister(EventTypes.RunStart, OnRunStart);
        EventBus.Unregister<string>(EventTypes.ShowHint, OnShowHint);
        EventBus.Unregister<string, string>(EventTypes.ShowButtonGuide, OnShowButtonGuide);
        EventBus.Unregister(EventTypes.PollutionTierUp, OnFlashPollutionTierUp);
        EventBus.Unregister(EventTypes.PollutionTierDown, OnFlashPollutionTierDown);
        EventBus.Unregister<LegacyAbility>(EventTypes.LegacyAdded, OnFlashLegacyAdded);
        EventBus.Unregister<int, LegacyAbility>(EventTypes.LegacyReplaced, OnFlashLegacyReplaced);
        EventBus.Unregister<string>(EventTypes.FormSlotFull, OnFormSlotFull);
        EventBus.Unregister<List<LegacyAbility>, string>(EventTypes.LegacySelection, OnLegacySelection);
        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked -= OnAchievementUnlocked;
        if (_evoRedDotCoroutine != null)
            StopCoroutine(_evoRedDotCoroutine);
    }

    void OnRunStart()
    {
        for (int y = 0; y < 13; y++)
        {
            for (int x = 0; x < 13; x++)
            {
                _mapLastGlyph[y, x] = null;
            }
        }
        _lastExploreFloor = -1;
    }

    void Update()
    {
        if (!_ready) { Init(); return; }
        UpdateAchievementToast();
        UpdateFragBorderAnimation();
        UpdateTutorialProgress();
        UpdateGuideAnimation();

        // 校正布局完成后的图标尺寸（确保在布局系统运行后获取正确的父容器rect）
        FlushAspectIcons();
        var gs = CompleteGameSystem.Instance;
        if (gs != null)
        {
            Sync(gs);
        }
        else
        {
            SyncMainMenu();
            SyncEvolution();
        }

        // Drain main-thread actions queued from background tasks.
        if (_mainThreadActions.Count > 0)
        {
            Action a = null;
            lock (_mainThreadActions)
            {
                if (_mainThreadActions.Count > 0) a = _mainThreadActions.Dequeue();
            }
            while (a != null)
            {
                try { a(); } catch (Exception ex) { Debug.LogException(ex); }
                lock (_mainThreadActions)
                {
                    if (_mainThreadActions.Count > 0) a = _mainThreadActions.Dequeue(); else a = null;
                }
            }
        }
    }

    void Init()
    {
        if (_ready) return;
        _spriteCache.Clear();
        _texCache.Clear();
        var go = new GameObject("GameUI");
        go.transform.SetParent(transform);
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _canvas.sortingOrder = 200;
        var sc = go.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(540, 960);
        sc.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();

        var safeGo = new GameObject("SafeArea", typeof(RectTransform));
        safeGo.transform.SetParent(go.transform, false);
        var safeRT = safeGo.GetComponent<RectTransform>();
        safeRT.anchorMin = Vector2.zero;
        safeRT.anchorMax = Vector2.one;
        safeRT.offsetMin = Vector2.zero;
        safeRT.offsetMax = Vector2.zero;
        safeGo.AddComponent<SafeAreaFitter>();

        _root = safeRT;
        _ready = true;

        Build();
        BuildGlobalEffects();
        EnsurePosterShareSystem();
        if (GuestAuthManager.Instance != null && GuestAuthManager.Instance.IsLoggedIn)
        {
            SA(_loginPanel, false);
            Show(CompleteGameSystem.RunScreen.MainMenu);
        }
        else
        {
            SA(_loginPanel, true);
            SA(_menuPanel, false);
        }

        if (AchievementManager.Instance != null)
            AchievementManager.Instance.OnAchievementUnlocked += OnAchievementUnlocked;
    }

    void EnsurePosterShareSystem()
    {
        if (PosterShareSystem.Instance != null) return;

        var posterGo = new GameObject("PosterShareSystem");
        posterGo.transform.SetParent(transform, false);
        posterGo.AddComponent<PosterShareSystem>();
    }

    void Build()
    {
        BuildLoginPanel();
        BuildMenu();
        BuildProfilePanel();
        BuildClass();
        BuildExplore();
        BuildCombat();
        BuildOver();
        BuildEnd();
        BuildStageTransition();
        BuildAltar();
        BuildCollapse();
        BuildPossessPanel();
        BuildRoutePanel();
        BuildDeathFormPanel();
        BuildDeathRollbackPanel();
        BuildFormReplacePanel();
        BuildFormReplacementPanel();
        BuildLegacySelectionPanel();
        BuildStoryPanel();
        BuildFragChoicePanel();
        BuildSynthConfirmPanel();
        BuildMenuPanel();
        BuildTutorialPanel();
        BuildAchievementToast();
        BuildFlashBanner();
        RegisterFlashEvents();
        BuildSoftHintBubble();
        if (_avatarBtn) _avatarBtn.transform.SetAsLastSibling();
        if (_eTutGo) _eTutGo.transform.SetAsLastSibling();
        if (_softHintBubble) _softHintBubble.transform.SetAsLastSibling();
        if (_fragChoiceOvl) _fragChoiceOvl.transform.SetAsLastSibling();
        if (_possessOvl) _possessOvl.transform.SetAsLastSibling();
    }

    void Show(CompleteGameSystem.RunScreen s)
    {
        SA(_menuPanel, s == CompleteGameSystem.RunScreen.MainMenu);
        SA(_avatarBtn, s == CompleteGameSystem.RunScreen.MainMenu);
        SA(_classPanel, s == CompleteGameSystem.RunScreen.CharacterSelect);
        SA(_explorePanel, s == CompleteGameSystem.RunScreen.Exploration);
        SA(_combatPanel, s == CompleteGameSystem.RunScreen.Combat);
        SA(_overPanel, s == CompleteGameSystem.RunScreen.GameOver);
        SA(_endPanel, s == CompleteGameSystem.RunScreen.Ending);
        SA(_stageTransPanel, s == CompleteGameSystem.RunScreen.StageTransition);

        bool inGame = s == CompleteGameSystem.RunScreen.Exploration || s == CompleteGameSystem.RunScreen.Combat;
        if (!inGame)
        {
            SA(_eTutGo, false);
            SA(_softHintBubble, false);
        }

        bool screenChanged = s != _lastScreen;
        _lastScreen = s;

        if (screenChanged && s == CompleteGameSystem.RunScreen.MainMenu)
            _menuSyncTime = 0;

        if (screenChanged && s == CompleteGameSystem.RunScreen.Combat)
        {
            _lastPlayerHp = -1;
            _lastEnemyHp = -1;
            _lastPlayerAtk = -1;
            _lastPlayerDef = -1;
            _playerHpDisplay = 1f;
            _enemyHpDisplay = 1f;
            _ultPulsing = false;
            _lastCombatLogCount = -1;
            if (_ultPulse != null) { StopCoroutine(_ultPulse); _ultPulse = null; }
            if (_combatPanel != null && _combatPanel.activeSelf)
                StartCoroutine(AnimateCombatEntrance());
        }

        // Hide possess overlay when not in combat
        if (s != CompleteGameSystem.RunScreen.Combat)
        {
            SA(_possessOvl, false);
        }

        // Hide menu overlay panels only when screen actually changes
        if (screenChanged)
        {
            SA(_evoOvl, false);
            SA(_shopOvl, false);
            SA(_bestOvl, false);
            SA(_achOvl, false);
            SA(_setOvl, false);
            SA(_rankOvl, false);
            SA(_fragOvl, false);
            SA(_echoAltarOvl, false);
            SA(_dailyOvl, false);
            SA(_menuOvl, false);
            SA(_pollSkillOvl, false);
            SA(_bondOvl, false);
            SA(_anchorOvl, false);
            SA(_buildOvl, false);
            SA(_recOvl, false);

            // 屏幕切换动画 — 根据场景类型差异化
            GameObject active = null;
            switch(s) {
                case CompleteGameSystem.RunScreen.MainMenu: active = _menuPanel; break;
                case CompleteGameSystem.RunScreen.CharacterSelect: active = _classPanel; break;
                case CompleteGameSystem.RunScreen.Exploration: active = _explorePanel; break;
                case CompleteGameSystem.RunScreen.Combat: active = _combatPanel; break;
                case CompleteGameSystem.RunScreen.GameOver: active = _overPanel; break;
                case CompleteGameSystem.RunScreen.Ending: active = _endPanel; break;
                case CompleteGameSystem.RunScreen.StageTransition: active = _stageTransPanel; break;
            }
            if (active != null)
            {
                var rt = active.GetComponent<RectTransform>();
                if (s == CompleteGameSystem.RunScreen.Combat && _prevScreen == CompleteGameSystem.RunScreen.Exploration)
                {
                    ScreenEffectsManager.Instance?.FlashDamage();
                    StartCoroutine(UIAnimationSystem.FadeIn(rt, 0.15f));
                }
                else if (s == CompleteGameSystem.RunScreen.GameOver || s == CompleteGameSystem.RunScreen.Ending
                    || s == CompleteGameSystem.RunScreen.StageTransition)
                {
                    StartCoroutine(UIAnimationSystem.SlideInFromBottom(rt, 0.4f));
                }
                else if (s == CompleteGameSystem.RunScreen.Exploration && _prevScreen == CompleteGameSystem.RunScreen.Combat)
                {
                    StartCoroutine(UIAnimationSystem.FadeIn(rt, 0.3f));
                }
                else
                {
                    StartCoroutine(UIAnimationSystem.FadeIn(rt, 0.25f));
                }
            }
            _prevScreen = s;
        }
    }

    // 强制UI刷新 - 在教程阶段推进时调用
    public void ForceRefresh()
    {
        var gs = CompleteGameSystem.Instance;
        if (gs != null)
        {
            Sync(gs);
        }
    }

    void Sync(CompleteGameSystem gs)
    {
        Show(gs.CurrentScreen);
        var p = GameManager.Instance?.Player;
        if (p == null) return;

        // 屏幕切换时清理引导气泡
        if (gs.CurrentScreen != CompleteGameSystem.RunScreen.Exploration &&
            gs.CurrentScreen != CompleteGameSystem.RunScreen.Combat)
        {
            HideGuideBubble();
        }

        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.MainMenu) SyncMainMenu();
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.Exploration) SyncExplore(gs, p);
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.Combat) SyncCombat(gs, p);
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.GameOver) SyncOver(gs, p);
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.Ending) SyncEnd(gs);
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.StageTransition) SyncStageTransition(gs);

        SA(_altarOvl, gs.ShowingAltar && gs.PendingAltar != null);
        SA(_collapseOvl, gs.ShowingCollapse);
        SA(_routeOvl, gs.ShowingRouteSelect);
        SA(_deathFormOvl, gs.ShowingDeathFormSelect);
        if (!gs.ShowingDeathFormSelect) _deathFormBuilt = false;
        SA(_deathRollbackOvl, gs.ShowingDeathRollback);
        if (gs.ShowingDeathRollback) SyncDeathRollback(gs);
        if (!gs.ShowingDeathRollback) _rollbackBuilt = false;
        SA(_formReplaceOvl, gs.ShowingFormReplace);
        if (gs.ShowingFormReplace) SyncFormReplace(gs);
        if (!gs.ShowingFormReplace) _formReplaceBuilt = false;
        SA(_fragChoiceOvl, gs.ShowingFragmentChoice);
        if (_cBtmHudGo != null) _cBtmHudGo.SetActive(!gs.ShowingFragmentChoice);
        SA(_storyOvl, gs.ShowingStoryEvent);
        SA(_storyPanel, gs.ShowingStoryEvent);
        if (gs.ShowingAltar && gs.PendingAltar != null) SyncAltar(gs);
        if (gs.ShowingRouteSelect) SyncRoute(gs);
        if (gs.ShowingDeathFormSelect) SyncDeathForm(gs);
        if (gs.ShowingStoryEvent) SyncStory(gs);
        UpdateStoryEffects();

        if (gs.IsIntroPlaying())
        {
            Debug.Log($"[Intro] Active: index={StorySystem.Instance?.introCurrentIndex}/{StorySystem.Instance?.introLines.Count}, playing={gs.IsIntroPlaying()}, showing={gs.ShowingStoryEvent}");
        }

        SyncEvolution();
        SyncPollutionSkill();
        SyncFormBond();
        SyncAnchor();
        SyncRecord();
        SyncLegacy();
        SyncFragChoice(gs);
        if (!gs.ShowingFragmentChoice) _fragChoiceBuilt = false;
        SyncSynthConfirm(gs);
        SyncSkillBar(gs);
        if (_hudMenuRedDot != null) _hudMenuRedDot.SetActive(CanEvolve());
    }

    // ========== SYNC ==========
    void SyncExplore(CompleteGameSystem gs, GameManager.PlayerData p)
    {
        int currentFloor = GameManager.Instance != null ? GameManager.Instance.CurrentFloor : 1;
        if (_lastExploreFloor != currentFloor)
        {
            _lastExploreFloor = currentFloor;
            _mapLogged = false;
            for (int y = 0; y < 13; y++)
                for (int x = 0; x < 13; x++)
                {
                    _mapLastGlyph[y, x] = "";
                }
            // 楼层氛围横幅
            ShowFloorBanner(currentFloor);
        }
        
        int floor = GameManager.Instance != null ? GameManager.Instance.CurrentFloor : 1;

        // Shader 主背景: 每层独特 procedural pattern, 固定 alpha 1.0
        if (_eBgShaderOverlay != null)
        {
            var bgMat = ShaderMaterialManager.Instance != null
                ? ShaderMaterialManager.Instance.GetFloorBackgroundMat(floor)
                : null;
            if (bgMat != null)
            {
                _eBgShaderOverlay.material = bgMat;
                _eBgShaderOverlay.color = new Color(1, 1, 1, 1f);
            }
        }

        // floor_bg 纹理 (占位, 全透明, 不参与渲染)
        int bgIdx = floor <= 4 ? 1 : floor <= 8 ? 2 : 3;
        if (_eFloorBg != null && bgIdx != _eFloorBgIdx)
        {
            _eFloorBgIdx = bgIdx;
            string variant = (floor % 2 == 0) ? "a" : "b";
            var tex = LoadTex($"UI/floor_bg_{bgIdx}{variant}");
            if (tex == null) tex = LoadTex($"UI/floor_bg_{bgIdx}a");
            _eFloorBg.texture = tex;
            _eFloorBg.color = new Color(1f, 1f, 1f, 0f);  // 全透明, shader 主背景透出
        }
        // 楼层和区域信息
        int zone = gs.CurrentFloorState?.zone ?? 1;
        string zoneName = GetZoneName(zone);
        string specialLabel = SpecialFloorSystem.Instance?.GetSpecialFloorName(GameManager.Instance.CurrentFloor) ?? "";
        string sigLabel = "";
        var sig = CompleteGameSystem.Instance?.ActiveSignature;
        if (sig != null)
            sigLabel = $" {sig.icon}{sig.name}";
        string stagePrefix = GameManager.Instance.CurrentMode == GameMode.Expedition
            ? $"第{GameManager.Instance.CurrentStage}关 " : "";
        if (!string.IsNullOrEmpty(specialLabel))
            ST(_eFloor, $"{stagePrefix}F{GameManager.Instance.CurrentFloor} {zoneName} {specialLabel}{sigLabel}");
        else
            ST(_eFloor, $"{stagePrefix}F{GameManager.Instance.CurrentFloor} {zoneName}{sigLabel}");

        // 当前形态名称 + 寄主技能条
        bool isPossessed = p.currentFormId != "human";
        bool hasTraits = p.traits != null && p.traits.Count > 0;
        string formName = p.currentFormId == "human" ? CName(p.selectedClass) : MName(p.currentFormId);

        // 附身后隐藏 _eFormName（_eHostBar 代替），未附身时正常显示
        if (_eFormName)
        {
            _eFormName.gameObject.SetActive(!isPossessed);
            ST(_eFormName, formName);
        }

        // 探索界面寄主技能条
        if (_eHostBar != null)
        {
            _eHostBar.SetActive(isPossessed);
            if (isPossessed)
            {
                ST(_eHostName, formName);
                ST(_eHostTraits, hasTraits ? string.Join(" · ", p.traits) : "无特性");
                
                var ehBg = _eHostBar.GetComponent<Image>();
                if (ehBg != null)
                {
                    float pulseBrightness = Mathf.Sin(Time.time * 2f) * 0.02f;
                    ehBg.color = new Color(0.02f + pulseBrightness, 0.05f + pulseBrightness, 0.08f + pulseBrightness, 0.97f);
                }
                
                var ehOutline = _eHostBar.GetComponent<Outline>();
                if (ehOutline != null)
                {
                    float pulseAlpha = Mathf.Sin(Time.time * 3f) * 0.15f + 0.4f;
                    ehOutline.effectColor = new Color(0f, 1f, 0.816f, pulseAlpha);
                }
            }
        }

        // 更新探索界面形态卡槽
        UpdateExploreFormSlots(gs, p);

        float hr = (float)p.hp / Mathf.Max(1, p.maxHp);
        // HP 文字统一白色+黑色描边(在 TxtAnchored 中已设置),HP 条颜色仍可动态变化以传达状态
        if (_eHpTxt) { _eHpTxt.text = $"♥ {p.hp}/{p.maxHp}"; _eHpTxt.color = Color.white; }

        if (_ePollTxt) { _ePollTxt.text = $"☢ {p.pollution:F0}%"; _ePollTxt.color = Color.white; }

        // 底部状态栏 — 语义化配色
        ST(_eMsg, gs.LastMessage);
        string atkC = ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.DangerOrange);
        string defC = ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.InfoBlue);
        string epC = ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.HealthGreen);
        if (_eProg) { _eProg.supportRichText = true; int atkTotal = p.attack + Mathf.CeilToInt(p.attackBonus); int defTotal = p.defense + Mathf.CeilToInt(p.defenseBonus); string atkStr = p.attackBonus > 0 ? $"{atkTotal}(+{Mathf.CeilToInt(p.attackBonus)})" : $"{p.attack}"; string defStr = p.defenseBonus > 0 ? $"{defTotal}(+{Mathf.CeilToInt(p.defenseBonus)})" : $"{p.defense}"; _eProg.text = $"<color=#{atkC}>ATK {atkStr}</color> · <color=#{defC}>DEF {defStr}</color> · <color=#{epC}>EP {p.evolutionPoints}</color>"; }

        // 记忆碎片和进化等级
        ST(_eFragments, $"碎片 ×{gs.SkillFragments.Count}");
        ST(_eEvoLvl, "");

        if (gs.ShortModeTimerActive)
        {
            int m=Mathf.FloorToInt(gs.ShortModeTimer/60f), s=Mathf.FloorToInt(gs.ShortModeTimer%60f);
            ST(_eTimer, $"{m:D2}:{s:D2}");
            if (_eTimer) _eTimer.color = gs.ShortModeTimer<60?BrightRed:Gold;
        }
        else ST(_eTimer, "");

        SyncTutorialPanel(gs);
        SyncSoftHint(gs);

        SyncExploreActionButtons(gs);

        // 同步 13×13 地图单元格
        if (gs.CurrentFloorState != null)
        {
            var floorState = gs.CurrentFloorState;

            if (floorState.discovered == null)
            {
                floorState.discovered = new bool[13, 13];
            }
            if (floorState.walkable == null)
            {
                floorState.walkable = new bool[13, 13];
            }

            // 图标尺寸初始化: 根据格子实际尺寸放大图标,让玩家/怪物更显眼
            if (!_mapIconSized)
            {
                bool anyValid = false;
                for (int y = 0; y < 13 && !anyValid; y++)
                    for (int x = 0; x < 13 && !anyValid; x++)
                        if (_mapImg[y, x] != null && _mapImg[y, x].rectTransform.rect.width > 1f)
                            anyValid = true;
                if (anyValid)
                {
                    _mapIconSized = true;
                    for (int y = 0; y < 13; y++)
                    {
                        for (int x = 0; x < 13; x++)
                        {
                            var cellImg = _mapImg[y, x];
                            if (cellImg == null) continue;
                            var cellRT = cellImg.rectTransform;
                            float cellW = cellRT.rect.width;
                            float cellH = cellRT.rect.height;
                            if (cellW < 1f || cellH < 1f) continue;
                            // 图标尺寸 = 格子最小边的 1.0 倍,完全在格子内,不溢出到相邻格子
                            float iconSide = Mathf.Min(cellW, cellH) * 1.0f;
                            var iconImg = _mapIcon[y, x];
                            if (iconImg != null)
                            {
                                var iconRT = iconImg.rectTransform;
                                iconRT.anchorMin = new Vector2(0.5f, 0.5f);
                                iconRT.anchorMax = new Vector2(0.5f, 0.5f);
                                iconRT.pivot = new Vector2(0.5f, 0.5f);
                                iconRT.sizeDelta = new Vector2(iconSide, iconSide);
                            }
                        }
                    }
                }
            }
            
            // 1. 清空所有格子 + 应用迷雾
            // 调试: 检查迷雾系统状态
            int discoveredCount = 0;
            for (int y = 0; y < 13; y++)
                for (int x = 0; x < 13; x++)
                    if (floorState.discovered[y, x]) discoveredCount++;

            // 1. 渲染 floor/wall 格子底色 + 边框 (一次性区分地板与墙壁)
            // 主题色提前到循环外 — 整个 SyncExplore 调用内 currentFloor 不变, 主题常量
            var floorTheme = ShaderMaterialManager.GetFloorTheme(currentFloor);
            Color floorColor = floorTheme.FloorTint;
            Color wallColor = floorTheme.WallTint;
            Color accentColor = floorTheme.Accent;

            for (int y = 0; y < 13; y++)
            {
                for (int x = 0; x < 13; x++)
                {
                    // 重置所有层
                    if (_mapIcon[y, x] != null)
                    {
                        _mapIcon[y, x].sprite = null;
                        _mapIcon[y, x].color = new Color(1, 1, 1, 0);
                    }
                    if (_mapOutline[y, x] != null)
                        _mapOutline[y, x].effectColor = new Color(0, 0, 0, 0);
                    if (_mapBadge[y, x] != null)
                        _mapBadge[y, x].gameObject.SetActive(false);

                    bool discovered = floorState.discovered[y, x];
                    bool walkable = floorState.walkable[y, x];

                    if (_mapFog[y, x] != null)
                    {
                        _mapFog[y, x].color = discovered
                            ? new Color(0, 0, 0, 0)
                            : new Color(0.02f, 0.01f, 0.04f, 0.94f);
                        _mapFog[y, x].raycastTarget = false;
                    }

                    if (!discovered)
                    {
                        if (_mapImg[y, x] != null)
                        {
                            _mapImg[y, x].color = new Color(1, 1, 1, 0);
                            _mapImg[y, x].sprite = WhiteSprite;
                        }
                        if (_mapCellOutline[y, x] != null)
                            _mapCellOutline[y, x].effectDistance = new Vector2(0, 0);
                        if (_mapCellShadow[y, x] != null)
                            _mapCellShadow[y, x].enabled = false;
                    }
                    else if (walkable)
                    {
                        // 地板: WhiteSprite + FloorTint 纯色染色, Shader 主背景生物膜纹理完整透出
                        if (_mapImg[y, x] != null)
                        {
                            _mapImg[y, x].sprite = WhiteSprite;
                            _mapImg[y, x].color = floorColor;
                        }
                        // 地板格子描边: 主题 Accent 色 — 清晰但不抢眼
                        if (_mapCellOutline[y, x] != null)
                        {
                            _mapCellOutline[y, x].effectColor = accentColor;
                            _mapCellOutline[y, x].effectDistance = new Vector2(1.2f, 1.2f);
                        }
                        if (_mapCellShadow[y, x] != null)
                            _mapCellShadow[y, x].enabled = false;
                    }
                    else
                    {
                        // 墙壁: 主题 WallTint + Shadow 凹墙立体感
                        if (_mapImg[y, x] != null)
                        {
                            _mapImg[y, x].sprite = WhiteSprite;
                            _mapImg[y, x].color = wallColor;
                        }
                        // 墙壁 Outline 关闭 (连续墙不产生十字线)
                        if (_mapCellOutline[y, x] != null)
                            _mapCellOutline[y, x].effectDistance = new Vector2(0, 0);
                        // 墙壁 Shadow: 主题 Accent 偏移做凹墙质感 — 明显但不扎眼
                        if (_mapCellShadow[y, x] != null)
                        {
                            _mapCellShadow[y, x].enabled = true;
                            _mapCellShadow[y, x].effectColor = new Color(
                                accentColor.r * 0.3f,
                                accentColor.g * 0.3f,
                                accentColor.b * 0.3f,
                                0.55f);
                        }
                    }
                    // 重置 cell Outline — 不再需要单块描边 (会造成相邻墙双线)
                    // 注: 上面已按类型分别设置 distance, 这行旧重置注释掉
                }
            }
            
            // 辅助: 设置图标 Outline 发光边框 + 呼吸脉冲
            // 统一呼吸风格: 慢 (1.4~2.0 rad/s) + 细 (baseThickness 1~2) + 弱 (pulseStrength 0.35~0.5)
            void SetOutline(int gx, int gy, Color baseColor, float baseThickness, float pulseSpeed, float pulseStrength)
            {
                var outline = _mapOutline[gy, gx];
                if (outline == null) return;
                float pulse = 0.5f + 0.5f * Mathf.Sin(Time.time * pulseSpeed);
                float alpha = Mathf.Lerp(baseColor.a * (1f - pulseStrength), baseColor.a, pulse);
                outline.effectColor = new Color(baseColor.r, baseColor.g, baseColor.b, alpha);
                float thick = Mathf.Lerp(baseThickness - pulseStrength, baseThickness + 0.5f, pulse);
                outline.effectDistance = new Vector2(Mathf.Max(0.5f, thick), Mathf.Max(0.5f, thick));
            }

            // === 颜色派生: 主题 Accent + 品类偏移, 让每个楼层都有独特视觉 ===
            // 地板 L ≈ 0.06~0.09, 墙 L ≈ 0.02~0.04
            // 底托必须 L ≥ 0.15 才能跳出来 (亮度差 ≥ 0.06)
            Func<Color, float, Color> DimColor = (c, dim) => new Color(c.r * dim, c.g * dim, c.b * dim, c.a);

            // 统一底托生成 — (主题偏移色, 品类强化色) → 混合 + 提高 alpha 保证对比
            Func<Color, Color, Color> MakeTint = (themeAccent, category) => {
                Color mixed = Color.Lerp(themeAccent, category, 0.55f);
                // 强制 alpha ≥ 0.78, 保证跟地板拉开对比
                mixed.a = Mathf.Max(mixed.a, 0.78f);
                // 强制 RGB 最低 0.12 — 避免过暗的楼层底托被地板吞
                mixed.r = Mathf.Max(mixed.r, 0.12f);
                mixed.g = Mathf.Max(mixed.g, 0.12f);
                mixed.b = Mathf.Max(mixed.b, 0.12f);
                return mixed;
            };

            // 2. 渲染出口 — (发现后) 金色底托 + 呼吸边框
            Vector2Int exitPos = floorState.exitPos;
            if (exitPos.x >= 0 && exitPos.x < 13 && exitPos.y >= 0 && exitPos.y < 13)
            {
                bool exitDiscovered = floorState.discovered[exitPos.y, exitPos.x];
                if (exitDiscovered)
                {
                    // 出口格子底托: 金色 (统一提示色)
                    var eCellImg = _mapImg[exitPos.y, exitPos.x];
                    if (eCellImg != null)
                        eCellImg.color = MakeTint(accentColor, new Color(1f, 0.9f, 0.3f, 0.82f));
                    // 图标
                    if (_mapIcon[exitPos.y, exitPos.x] != null && _mapGlyphToType.TryGetValue("⇧", out var eType)
                        && _mapElementTypeColors.TryGetValue(eType, out var eColor))
                    {
                        _mapIcon[exitPos.y, exitPos.x].sprite = CreateMapElementSprite(eType, eColor);
                        _mapIcon[exitPos.y, exitPos.x].color = Color.white;
                    }
                    // Outline: 金绿色 + 克制呼吸
                    Color exitOutline = MakeTint(accentColor, new Color(0.5f, 1f, 0.6f, 0.88f));
                    SetOutline(exitPos.x, exitPos.y, exitOutline, 2f, 1.6f, 0.4f);
                }
            }

            // 3. 渲染所有 action (怪物/事件/商店/碎片) — 统一有格子底托
            if (floorState.actions != null)
            {
                foreach (var kvp in floorState.actions)
                {
                    var pos = kvp.Key;
                    var action = kvp.Value;
                    if (action == null || pos.x < 0 || pos.x >= 13 || pos.y < 0 || pos.y >= 13) continue;
                    if (!floorState.discovered[pos.y, pos.x]) continue;

                    var cellImg = _mapImg[pos.y, pos.x];     // 格子底托 (覆盖者)
                    var cellIcon = _mapIcon[pos.y, pos.x];   // 图标 (不变)
                    var cellBadge = _mapBadge[pos.y, pos.x];

                    switch (action.type)
                    {
                        case ExploreActionType.Monster:
                            if (action.monster == null || action.consumed) break;
                            {
                                bool isBoss = action.monster.isBoss;
                                bool isElite = action.monster.isElite || action.monster._elite;
                                bool isStairGuard = action.monster.stairGuard;

                                SetCellIcon(cellIcon, action.monster.id);

                                // 怪物格子底托: 主题 Accent + 品类偏移 (不再固定红!)
                                // 保证每层的怪物底托颜色随楼层变化 → 视觉融合
                                Color tint;
                                if (isBoss)
                                    tint = MakeTint(accentColor, new Color(0.95f, 0.10f, 0.28f, 0.88f));
                                else if (isStairGuard)
                                    tint = MakeTint(accentColor, new Color(0.60f, 0.50f, 0.08f, 0.84f));
                                else if (isElite)
                                    tint = MakeTint(accentColor, new Color(0.55f, 0.28f, 0.08f, 0.84f));
                                else
                                    // 普通怪物: 跟主题 Accent 更接近 (40% 品类偏移) — 降低视觉噪声
                                    tint = Color.Lerp(accentColor, new Color(0.70f, 0.12f, 0.20f, 0.84f), 0.40f);
                                if (cellImg != null) cellImg.color = tint;

                                // Outline: 跟主题 Accent 联动 + 品类偏移
                                Color ol; float thickness, speed, strength;
                                if (isBoss)
                                {
                                    ol = MakeTint(accentColor, new Color(1f, 0.18f, 0.35f, 0.92f));
                                    thickness = 3f; speed = 2.0f; strength = 0.5f;
                                }
                                else if (isElite)
                                {
                                    ol = MakeTint(accentColor, new Color(1f, 0.60f, 0.18f, 0.82f));
                                    thickness = 2f; speed = 1.8f; strength = 0.4f;
                                }
                                else if (isStairGuard)
                                {
                                    ol = MakeTint(accentColor, new Color(1f, 0.82f, 0.12f, 0.88f));
                                    thickness = 2f; speed = 1.6f; strength = 0.4f;
                                }
                                else
                                {
                                    ol = MakeTint(accentColor, new Color(0.90f, 0.30f, 0.38f, 0.78f));
                                    thickness = 2f; speed = 1.4f; strength = 0.35f;
                                }
                                SetOutline(pos.x, pos.y, ol, thickness, speed, strength);

                                // Badge
                                if (cellBadge != null && (isBoss || isElite || isStairGuard))
                                {
                                    cellBadge.gameObject.SetActive(true);
                                    if (isBoss)
                                        cellBadge.sprite = CreateBadgeSprite(BadgeType.BossStar, MakeTint(accentColor, new Color(1f, 0.35f, 0.6f, 0.9f)));
                                    else if (isElite)
                                        cellBadge.sprite = CreateBadgeSprite(BadgeType.Star, MakeTint(accentColor, new Color(1f, 0.65f, 0.3f, 0.9f)));
                                    else
                                        cellBadge.sprite = CreateBadgeSprite(BadgeType.StairGate, MakeTint(accentColor, new Color(1f, 0.85f, 0.15f, 0.9f)));
                                }
                            }
                            break;

                        case ExploreActionType.Event:
                            if (action.consumed || cellIcon == null) break;
                            {
                                // 底托 + Outline: 紫蓝色 (神秘事件)
                                if (cellImg != null)
                                    cellImg.color = MakeTint(accentColor, new Color(0.55f, 0.28f, 1f, 0.78f));
                                if (_mapElementTypeColors.TryGetValue(MapElementType.Event, out var eColor))
                                    cellIcon.sprite = CreateMapElementSprite(MapElementType.Event, eColor);
                                cellIcon.color = Color.white;
                                Color ol = MakeTint(accentColor, new Color(0.60f, 0.32f, 1f, 0.75f));
                                SetOutline(pos.x, pos.y, ol, 1.5f, 1.2f, 0.35f);
                            }
                            break;

                        case ExploreActionType.Shop:
                            if (action.consumed || cellIcon == null) break;
                            {
                                // 底托 + Outline: 金橙色 (交易)
                                if (cellImg != null)
                                    cellImg.color = MakeTint(accentColor, new Color(1f, 0.78f, 0.30f, 0.80f));
                                if (_mapElementTypeColors.TryGetValue(MapElementType.Shop, out var sColor))
                                    cellIcon.sprite = CreateMapElementSprite(MapElementType.Shop, sColor);
                                cellIcon.color = Color.white;
                                Color ol = MakeTint(accentColor, new Color(1f, 0.85f, 0.35f, 0.85f));
                                SetOutline(pos.x, pos.y, ol, 2f, 1.0f, 0.35f);
                            }
                            break;

                        case ExploreActionType.Fragment:
                            if (action.consumed || cellIcon == null) break;
                            {
                                // 底托 + Outline: 青蓝色 (记忆碎片)
                                if (cellImg != null)
                                    cellImg.color = MakeTint(accentColor, new Color(0.28f, 0.85f, 0.95f, 0.78f));
                                if (_mapElementTypeColors.TryGetValue(MapElementType.Fragment, out var fColor))
                                    cellIcon.sprite = CreateMapElementSprite(MapElementType.Fragment, fColor);
                                cellIcon.color = Color.white;
                                Color ol = MakeTint(accentColor, new Color(0.32f, 0.90f, 1f, 0.82f));
                                SetOutline(pos.x, pos.y, ol, 1.5f, 1.5f, 0.4f);
                            }
                            break;
                    }
                }
            }

            // 4. 渲染玩家 — 跟 Accent 联动的青绿色 + 克制呼吸 (唯一"亮"的元素, 不抢但必现)
            Vector2Int playerPos = floorState.playerPos;
            if (playerPos.x >= 0 && playerPos.x < 13 && playerPos.y >= 0 && playerPos.y < 13)
            {
                // 玩家格子底托: 主题偏移 + 青绿 (保证在任何楼层都可辨)
                var pCellImg = _mapImg[playerPos.y, playerPos.x];
                if (pCellImg != null)
                    pCellImg.color = MakeTint(accentColor, new Color(0.15f, 1f, 0.85f, 0.82f));

                Color pOutline = MakeTint(accentColor, new Color(0.0f, 1f, 0.85f, 0.92f));
                SetOutline(playerPos.x, playerPos.y, pOutline, 3f, 2.2f, 0.45f);

                if (_mapIcon[playerPos.y, playerPos.x] != null)
                {
                    string playerIconId = p.currentFormId == "human" ? "c_" + p.selectedClass : p.currentFormId;
                    SetCellIcon(_mapIcon[playerPos.y, playerPos.x], playerIconId);
                    _mapIcon[playerPos.y, playerPos.x].color = new Color(1f, 1f, 0.88f, 1f);
                    _mapIcon[playerPos.y, playerPos.x].rectTransform.SetAsLastSibling();
                }
            }

            // 调试: 打印迷雾系统状态
            if (Time.frameCount % 300 == 0)
            {
                Debug.Log($"[Map Fog] Discovered: {discoveredCount}/169, Player: ({playerPos.x},{playerPos.y})");
            }
        }

        // 清理旧教程面板和箭头
        SyncTutorialPanel(gs);
    }
    
    public void SyncCombat(CompleteGameSystem gs, GameManager.PlayerData p)
    {
        // === ParasiteBackground Shader 动态楼层主题 (仅楼层变化时更新 Material) ===
        if (_cBgShaderOverlay != null)
        {
            int floor = GameManager.Instance != null ? GameManager.Instance.CurrentFloor : 1;
            if (floor != _cBgShaderFloor)
            {
                _cBgShaderFloor = floor;
                var bgMat = ShaderMaterialManager.Instance != null
                    ? ShaderMaterialManager.Instance.GetFloorBackgroundMat(floor)
                    : null;
                if (bgMat != null) _cBgShaderOverlay.material = bgMat;
            }
        }

        // Player card - name + HP + ATK/DEF + form
        string pName = p.currentFormId=="human" ? CName(p.selectedClass) : MName(p.currentFormId);
        ST(_cPName, pName);
        
        bool isPossessed = p.currentFormId != "human";
        
        if (_cPName) 
            _cPName.color = isPossessed ? new Color(0f, 1f, 0.816f) : ParasiteTowerColorScheme.GetClassPrimaryColor(p.selectedClass);
        
        if (_cPCardOutline)
        {
            if (isPossessed)
            {
                float pulseAlpha = Mathf.Sin(Time.time * 4f) * 0.2f + 0.6f;
                _cPCardOutline.effectColor = new Color(0f, 1f, 0.816f, pulseAlpha);
                _cPCardOutline.effectDistance = new Vector2(2, 2);
            }
            else
            {
                var cc = ParasiteTowerColorScheme.GetClassPrimaryColor(p.selectedClass);
                // 常规状态也做轻微呼吸脉动 (alpha 0.25~0.45)
                float pulseAlpha = Mathf.Sin(Time.time * 2.2f) * 0.1f + 0.35f;
                _cPCardOutline.effectColor = new Color(cc.r, cc.g, cc.b, pulseAlpha);
                _cPCardOutline.effectDistance = new Vector2(1, 1);
            }
        }
        
        if (_cPCardBg)
        {
            if (isPossessed)
            {
                float pulseBrightness = Mathf.Sin(Time.time * 2f) * 0.03f;
                _cPCardBg.color = new Color(0.05f + pulseBrightness, 0.08f + pulseBrightness, 0.12f + pulseBrightness, 0.85f);
            }
            else
            {
                _cPCardBg.color = new Color(0.08f, 0.06f, 0.14f, 0.75f);
            }
        }
        float hr=(float)p.hp/Mathf.Max(1,p.maxHp);
        if(_cPHpFill)
        {
            _playerHpDisplay = Mathf.Lerp(_playerHpDisplay, hr, Time.deltaTime * 8f);
            if (Mathf.Abs(_playerHpDisplay - hr) < 0.005f) _playerHpDisplay = hr;
            _cPHpFill.fillAmount=_playerHpDisplay;
            if (_cPHpFill.material != null && _cPHpFill.material.HasProperty("_FillAmount"))
                ShaderMaterialManager.Instance?.UpdateHpBarFill(_cPHpFill, _playerHpDisplay);
            else
                _cPHpFill.color=hr>0.6f?ParasiteTowerColorScheme.HealthGreen:hr>0.3f?ParasiteTowerColorScheme.WarningYellow:ParasiteTowerColorScheme.CriticalRed;
            if (_lastPlayerHp > 0 && p.hp < _lastPlayerHp)
            {
                int dmgDelta = _lastPlayerHp - p.hp;
                StartCoroutine(UIAnimationSystem.Flash(_cPHpFill, Color.white, 0.15f));
                DamageNumberPool.Instance?.SpawnDamage(dmgDelta, false);
                ScreenEffectsManager.Instance?.Shake(0.06f, 0.08f);
                if (_cPCardOutline) StartCoroutine(UIAnimationSystem.DamageShake(_cPCardOutline.transform.parent.GetComponent<RectTransform>(), 0.08f));
            }
            else if (_lastPlayerHp > 0 && p.hp > _lastPlayerHp)
            {
                int healDelta = p.hp - _lastPlayerHp;
                DamageNumberPool.Instance?.SpawnHeal(healDelta);
                if (_cPHpFill) StartCoroutine(UIAnimationSystem.Flash(_cPHpFill, ParasiteTowerColorScheme.HealthGreen, 0.2f));
            }
        }
        _lastPlayerHp = p.hp;
        ST(_cPHpTxt,$"HP {p.hp}/{p.maxHp}");
        if (_cPStat)
        {
            _cPStat.supportRichText = true;
            int ca = p.attack + Mathf.CeilToInt(p.attackBonus);
            int cd = p.defense + Mathf.CeilToInt(p.defenseBonus);
            string cas = p.attackBonus > 0 ? $"{ca}(+{Mathf.CeilToInt(p.attackBonus)})" : $"{p.attack}";
            string cds = p.defenseBonus > 0 ? $"{cd}(+{Mathf.CeilToInt(p.defenseBonus)})" : $"{p.defense}";
            _cPStat.text = $"<color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.DangerOrange)}>ATK {cas}</color>  <color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.InfoBlue)}>DEF {cds}</color>";
            // 属性提升动画: 检测 ATK/DEF 变化 → 闪绿 + 缩放脉冲
            if (_lastPlayerAtk >= 0 && (_lastPlayerAtk != ca || _lastPlayerDef != cd))
            {
                StartCoroutine(StatChangeFlash(_cPStat, ca > _lastPlayerAtk || cd > _lastPlayerDef));
            }
            _lastPlayerAtk = ca;
            _lastPlayerDef = cd;
        }
        if(_cPTraits != null)
        {
            bool hasTraits = p.traits != null && p.traits.Count > 0 && p.currentFormId != "human";
            if (hasTraits)
                _cPTraits.text = string.Join(" ", p.traits);
            else
                _cPTraits.text = "";
            if (_cPTraitBtn != null) _cPTraitBtn.SetActive(hasTraits);
        }
        if (_cPTypeTags != null)
        {
            string[] pAxes = GetFormAxes(p.currentFormId);
            _cPTypeTags.text = FormatAxesTags(pAxes);
        }
        LoadIcon(_cPIcon, p.currentFormId=="human"?"c_"+p.selectedClass:p.currentFormId);
        
        if (_cPIcon != null)
        {
            if (isPossessed)
            {
                float pulseBrightness = Mathf.Sin(Time.time * 2f) * 0.1f + 0.9f;
                _cPIcon.color = new Color(0.2f, pulseBrightness, 0.9f, 1f);
            }
            else
            {
                _cPIcon.color = new Color(1f, 1f, 1f, 1f);
            }
        }
        
        if (_cPossessBadge != null)
        {
            _cPossessBadge.SetActive(true);
        }
        
        if (_cPossessRing != null)
        {
            _cPossessRing.SetActive(isPossessed);
            if (isPossessed)
            {
                var ringImg = _cPossessRing.GetComponent<Image>();
                if (ringImg != null)
                {
                    float pulseAlpha = Mathf.Sin(Time.time * 3f) * 0.2f + 0.5f;
                    ringImg.color = new Color(0f, 1f, 0.816f, pulseAlpha);
                }
            }
        }

        var e=gs.CurrentEnemy;
        if(e!=null)
        {
            // Top bar: combat status (enemy name already on card)
            string topLabel = e.isBoss ? $"⚔ BOSS 战" : e.stairGuard ? $"⚔ 楼梯守卫" : $"⚔ F{GameManager.Instance.CurrentFloor} 战斗";
            string descLabel = e.isBoss ? e.name : e.stairGuard ? $"楼梯守卫 · {e.name}" : $"区域{e.zone} · {e.name}";

            // Boss阶段指示器
            if (e.isBoss && BossAIManager.Instance != null && BossAIManager.Instance.IsActiveBoss())
            {
                int phase = BossAIManager.Instance.CurrentPhase;
                int total = BossAIManager.Instance.TotalPhases;
                topLabel = $"⚔ BOSS 战 [{phase}/{total}]";
                var phaseConfig = BossAIManager.Instance.GetCurrentPhaseConfig();
                if (phaseConfig != null)
                    descLabel = $"{e.name} · 阶段{phase}";
            }

            ST(_cEnemyTitle, topLabel);
            ST(_cEnemyDesc, descLabel);

            ST(_cEName, e.name);
            if(_cEName) _cEName.color = e.isBoss?ParasiteTowerColorScheme.CriticalRed:e._elite?ParasiteTowerColorScheme.DangerOrange:Gold;
            if (_cECardOutline) {
                Color ec2 = e.isBoss?ParasiteTowerColorScheme.CriticalRed:e._elite?ParasiteTowerColorScheme.DangerOrange:ParasiteTowerColorScheme.GlowRedIntense;
                float pulseSpeed = e.isBoss ? 3.0f : e._elite ? 2.5f : 1.8f;
                float pulseBase = e.isBoss ? 0.55f : e._elite ? 0.42f : 0.32f;
                float pulseRange = e.isBoss ? 0.25f : e._elite ? 0.15f : 0.1f;
                float pulseAlpha = Mathf.Sin(Time.time * pulseSpeed) * pulseRange + pulseBase;
                _cECardOutline.effectColor = new Color(ec2.r, ec2.g, ec2.b, pulseAlpha);
                _cECardOutline.effectDistance = new Vector2(e.isBoss ? 3 : e._elite ? 2 : 1, e.isBoss ? 3 : e._elite ? 2 : 1);
            }
            float er=(float)e.hp/Mathf.Max(1,e.maxHp);
            if(_cEHpFill)
            {
                _enemyHpDisplay = Mathf.Lerp(_enemyHpDisplay, er, Time.deltaTime * 8f);
                if (Mathf.Abs(_enemyHpDisplay - er) < 0.005f) _enemyHpDisplay = er;
                _cEHpFill.fillAmount=_enemyHpDisplay;
                if (_cEHpFill.material != null && _cEHpFill.material.HasProperty("_FillAmount"))
                    ShaderMaterialManager.Instance?.UpdateHpBarFill(_cEHpFill, _enemyHpDisplay);
                else
                    _cEHpFill.color=er>0.6f?new Color(0.9f,0.2f,0.2f):er>0.3f?ParasiteTowerColorScheme.DangerOrange:ParasiteTowerColorScheme.CriticalRed;
                if (_lastEnemyHp > 0 && e.hp < _lastEnemyHp)
                {
                    int eDmgDelta = _lastEnemyHp - e.hp;
                    StartCoroutine(UIAnimationSystem.Flash(_cEHpFill, Color.white, 0.15f));
                    var ecRT = _cEHpFill.transform.parent.GetComponent<RectTransform>();
                    if (ecRT) StartCoroutine(UIAnimationSystem.DamageShake(ecRT, 0.1f));
                    DamageNumberPool.Instance?.SpawnDamage(eDmgDelta, eDmgDelta > 20);
                    ScreenEffectsManager.Instance?.Shake(0.1f, 0.1f);
                }
            }
            _lastEnemyHp = e.hp;
            ST(_cEHpTxt,$"{e.hp}/{e.maxHp} ({Mathf.RoundToInt(er*100)}%)");
            if (_cEStat) { _cEStat.supportRichText = true; _cEStat.text = $"<color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.DangerOrange)}>ATK {e.atk}</color>  <color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.InfoBlue)}>DEF {e.def}</color>"; }
            ST(_cETraits, e.traits!=null?string.Join(" ", e.traits):"");
            if (_cETypeTags != null)
            {
                _cETypeTags.text = FormatAxesTags(e.axes);
            }
            var iconTier = e.isBoss ? ThemedIconDisplay.IconTier.Boss
                : e._elite ? ThemedIconDisplay.IconTier.Elite
                : ThemedIconDisplay.IconTier.Normal;
            LoadIcon(_cEIcon, e.id, iconTier);

            // Possess rate — 颜色分级
            float possRate = gs.CalculatePossessChance(e);
            ST(_cRate, $"{Mathf.RoundToInt(possRate*100)}%");
            if (_cRate) _cRate.color = possRate>=0.6f?ParasiteTowerColorScheme.HealthGreen:possRate>=0.3f?ParasiteTowerColorScheme.WarningYellow:ParasiteTowerColorScheme.CriticalRed;
            if(_cPossFill)
            {
                _cPossFill.fillAmount = Mathf.Clamp01(possRate);
                _cPossFill.color = possRate>=0.6f?new Color(0.2f, 1f, 0.5f):possRate>=0.3f?new Color(1f, 0.8f, 0.2f):new Color(1f, 0.3f, 0.3f);
            }
        }
        // Show last 18 combat log lines (scrollable viewport)
        var log = gs.CombatLog;
        int logCount = log.Count;
        string logStr = "";
        int logStart = Mathf.Max(0, logCount - 18);
        for (int i = logStart; i < logCount; i++)
        {
            string entry = log[i];
            string prefix = "◆ ";
            
            if (entry.Contains("寄生") || entry.Contains("附身"))
            {
                if (entry.Contains("成功") || entry.Contains("夺取"))
                    prefix = "<color=#00ff88>◆ </color>";
                else if (entry.Contains("失败") || entry.Contains("识破"))
                    prefix = "<color=#ff006e>◆ </color>";
                else
                    prefix = "<color=#aa66ff>◆ </color>";
            }
            else if (entry.Contains("攻击") || entry.Contains("伤害") || entry.Contains("暴击"))
            {
                if (entry.Contains("暴击"))
                    prefix = "<color=#ffaa00>◆ </color>";
                else if (entry.Contains("受到"))
                    prefix = "<color=#ff4444>◆ </color>";
                else
                    prefix = "<color=#ff6644>◆ </color>";
            }
            else if (entry.Contains("防御") || entry.Contains("抵挡"))
            {
                prefix = "<color=#44aaff>◆ </color>";
            }
            else if (entry.Contains("回复") || entry.Contains("恢复") || entry.Contains("治愈"))
            {
                prefix = "<color=#88ff88>◆ </color>";
            }
            else if (entry.Contains("进化") || entry.Contains("获得") || entry.Contains("解锁"))
            {
                prefix = "<color=#ffcc00>◆ </color>";
            }
            else if (entry.Contains("污染"))
            {
                prefix = "<color=#cc66ff>◆ </color>";
            }
            else if (entry.Contains("技能") || entry.Contains("终极"))
            {
                prefix = "<color=#00ffff>◆ </color>";
            }
            
            if (i == logCount - 1)
            {
                logStr += $"{prefix}<color=#ffffff><b>{entry}</b></color>\n";
            }
            else
            {
                logStr += $"{prefix}<color=#9999aa>{entry}</color>\n";
            }
        }
        ST(_cMsg, logStr.TrimEnd());
        if (_cMsg) _cMsg.supportRichText = true;
        if (logCount > _lastCombatLogCount && _lastCombatLogCount >= 0)
            PlayCombatLogPulse();
        _lastCombatLogCount = logCount;
        if (_cLogScroll != null && _cLogScroll.content != null)
        {
            var contentRT = _cLogScroll.content;
            var msgRT = _cMsg.rectTransform;
            float width = _cLogScroll.viewport != null ? _cLogScroll.viewport.rect.width : 420f;
            if (width <= 1f)
            {
                width = _cLogScroll.GetComponent<RectTransform>()?.rect.width ?? 420f;
            }
            if (width <= 1f) width = 420f;
            msgRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, width);
            Canvas.ForceUpdateCanvases();
            float prefHeight = Mathf.Max(1f, _cMsg.preferredHeight);
            msgRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, prefHeight);
            contentRT.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, prefHeight);
            _cLogScroll.verticalNormalizedPosition = 0f;
            _cLogScroll.velocity = Vector2.zero;
            if (gs.CombatLog.Count > 0 && string.IsNullOrEmpty(_cMsg.text.Trim()))
            {
                Debug.LogWarning($"Combat UI log text is empty despite {gs.CombatLog.Count} entries; viewport width={width}");
            }
        }

        // Update form slots
        UpdateCombatFormSlots(gs, p);

        // Bottom HUD bar — 语义化配色
        string resonanceStr = "";
        if (FormResonanceSystem.Instance != null)
        {
            var effects = FormResonanceSystem.Instance.GetAllActiveEffects();
            if (effects.Count > 0)
            {
                var effectLabels = new List<string>();
                foreach (var effect in effects)
                {
                    string valueStr = effect.value >= 1 ? $"{effect.value:F0}" : $"{effect.value * 100:F0}%";
                    effectLabels.Add($"{effect.comboName}({valueStr})");
                }
                resonanceStr = $" · <color=#{ColorUtility.ToHtmlStringRGB(new Color(0f, 0.8f, 1f))}>◈{string.Join(" ", effectLabels)}</color>";
            }
        }
        string pollutionTierInfo = "";
        var pollTier = PollutionPassiveSystem.GetPollutionTier(p.pollution);
        if (p.pollution >= 30f && (pollTier.atkMult != 1f || pollTier.defMult != 1f))
        {
            string atkStr = pollTier.atkMult > 1f ? $"+{Mathf.RoundToInt((pollTier.atkMult - 1f) * 100)}%ATK" : pollTier.atkMult < 1f ? $"{Mathf.RoundToInt((pollTier.atkMult - 1f) * 100)}%ATK" : "";
            string defStr = pollTier.defMult < 1f ? $"{Mathf.RoundToInt((pollTier.defMult - 1f) * 100)}%DEF" : pollTier.defMult > 1f ? $"+{Mathf.RoundToInt((pollTier.defMult - 1f) * 100)}%DEF" : "";
            string bonusStr = "";
            if (!string.IsNullOrEmpty(atkStr)) bonusStr += atkStr;
            if (!string.IsNullOrEmpty(defStr)) bonusStr += (bonusStr.Length > 0 ? " " : "") + defStr;
            if (bonusStr.Length > 0)
            {
                pollutionTierInfo = $" [{bonusStr}]";
            }
        }
        if (_cBtmHud) { _cBtmHud.supportRichText = true; int ba = p.attack + Mathf.CeilToInt(p.attackBonus); int bd = p.defense + Mathf.CeilToInt(p.defenseBonus); string bas = p.attackBonus > 0 ? $"{ba}(+{Mathf.CeilToInt(p.attackBonus)})" : $"{p.attack}"; string bds = p.defenseBonus > 0 ? $"{bd}(+{Mathf.CeilToInt(p.defenseBonus)})" : $"{p.defense}"; _cBtmHud.text = $"<color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.DangerOrange)}>⚔{bas}</color> <color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.InfoBlue)}>◆{bds}</color> <color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.HealthGreen)}>EP:{p.evolutionPoints}</color> <color=#{ColorUtility.ToHtmlStringRGB(Purp)}>◇{gs.SkillFragments.Count}</color> <color=#{ColorUtility.ToHtmlStringRGB(ParasiteTowerColorScheme.GetPollutionColor(p.pollution/100f))}>☢{p.pollution:F0}%{pollutionTierInfo}</color>{resonanceStr}"; }

        // 战斗面板污染特效 — 与 ScreenEffectsManager 全局特效形成互补
        // 全局特效处理全屏叠加色、暗角、RGB分离、扫描线、Glitch
        // 本地特效处理：面板内径向暗角、脉冲扩散、细密扫描线、面板UI变色
        float pp = p.pollution / 100f;
        bool highPoll = pp >= 0.7f;
        bool extremePoll = pp >= 0.9f;
        float pollPulse = 0.5f + 0.5f * Mathf.Sin(Time.time * (1f + pp * 4f));

        // === 面板径向暗角 — 从面板边缘向中心的渐变黑化 ===
        if (_cPollVignette)
        {
            if (pp > 0.4f)
            {
                if (pp <= 0.7f)
                {
                    float vigAlpha = Mathf.Lerp(0.05f, 0.15f, (pp - 0.4f) / 0.3f);
                    _cPollVignette.color = new Color(0.3f, 0.08f, 0.4f, vigAlpha);
                }
                else if (pp <= 0.9f)
                {
                    float vigAlpha = Mathf.Lerp(0.18f, 0.38f, (pp - 0.7f) / 0.2f);
                    float vigPulse = pollPulse * 0.3f + 0.7f;
                    _cPollVignette.color = new Color(0.55f, 0.08f, 0.12f, vigAlpha * vigPulse);
                }
                else
                {
                    float vigAlpha = Mathf.Lerp(0.4f, 0.6f, (pp - 0.9f) / 0.1f);
                    float vigPulse = pollPulse * 0.4f + 0.6f;
                    float flicker = Mathf.PerlinNoise(Time.time * 15f, 0f) * 0.2f + 0.8f;
                    _cPollVignette.color = new Color(0.65f, 0.05f, 0.08f, vigAlpha * vigPulse * flicker);
                }
            }
            else
            {
                _cPollVignette.color = Color.clear;
            }
        }

        // === 面板脉冲扩散 — 中心向四周的红色脉冲 ===
        if (_cPollPulse)
        {
            if (pp > 0.5f)
            {
                if (pp <= 0.8f)
                {
                    float pulseAlpha = Mathf.Lerp(0.04f, 0.12f, (pp - 0.5f) / 0.3f);
                    float pulsePhase = Mathf.Sin(Time.time * (1.5f + pp * 3f)) * 0.3f + 0.7f;
                    _cPollPulse.color = new Color(0.4f, 0.12f, 0.5f, pulseAlpha * pulsePhase);
                }
                else if (pp <= 0.95f)
                {
                    float pulseAlpha = Mathf.Lerp(0.15f, 0.35f, (pp - 0.8f) / 0.15f);
                    float pulsePhase = Mathf.Sin(Time.time * (4f + pp * 6f)) * 0.4f + 0.6f;
                    _cPollPulse.color = new Color(0.75f, 0.08f, 0.15f, pulseAlpha * pulsePhase);
                }
                else
                {
                    float pulseAlpha = Mathf.Lerp(0.35f, 0.55f, (pp - 0.95f) / 0.05f);
                    float pulsePhase = Mathf.Sin(Time.time * (8f + pp * 10f)) * 0.5f + 0.5f;
                    float flicker = Mathf.PerlinNoise(Time.time * 20f, 0f) * 0.3f + 0.7f;
                    _cPollPulse.color = new Color(1f, 0.15f * flicker, 0.2f * flicker, pulseAlpha * pulsePhase * flicker);
                }
            }
            else
            {
                _cPollPulse.color = Color.clear;
            }
        }

        // === 面板细密扫描线 — 90%+ 才出现的高级故障效果 ===
        if (_cPollScanlines)
        {
            if (pp >= 0.85f)
            {
                float scanAlpha;
                float scrollY = 0f;
                if (pp < 0.95f)
                {
                    scanAlpha = Mathf.Lerp(0.04f, 0.15f, (pp - 0.85f) / 0.1f);
                    float glitchPhase = Mathf.Sin(Time.time * 10f) * 0.4f + 0.6f;
                    _cPollScanlines.color = new Color(0.9f, 0.25f, 0.3f, scanAlpha * glitchPhase);
                }
                else
                {
                    scanAlpha = Mathf.Lerp(0.15f, 0.35f, (pp - 0.95f) / 0.05f);
                    float glitchPhase = Mathf.Sin(Time.time * (12f + pp * 8f)) * 0.4f + 0.6f;
                    float noise = Mathf.PerlinNoise(Time.time * 25f, 0f) * 0.3f + 0.7f;
                    _cPollScanlines.color = new Color(1f, 0.15f + 0.2f * noise, 0.2f + 0.2f * noise, scanAlpha * glitchPhase * noise);
                    float scrollSpeed = (pp - 0.95f) / 0.05f * 60f + 30f;
                    scrollY = Mathf.Repeat(Time.time * scrollSpeed, 80f) - 40f;
                }
                var scanRT = _cPollScanlines.GetComponent<RectTransform>();
                scanRT.offsetMin = new Vector2(0, scrollY);
                scanRT.offsetMax = new Vector2(0, scrollY);
            }
            else
            {
                _cPollScanlines.color = Color.clear;
                var scanRT = _cPollScanlines.GetComponent<RectTransform>();
                scanRT.offsetMin = Vector2.zero;
                scanRT.offsetMax = Vector2.zero;
            }
        }

        // === 面板UI元素变色 — 高污染时边框和标题颜色变化 ===
        if (highPoll)
        {
            if (_cPCardOutline != null)
            {
                float outlineAlpha = extremePoll ? 0.6f + pollPulse * 0.3f : 0.35f + pollPulse * 0.2f;
                _cPCardOutline.effectColor = new Color(1f, 0.1f * (1f - pp), 0.15f * (1f - pp), outlineAlpha);
            }
            if (_cECardOutline != null)
            {
                float outlineAlpha = extremePoll ? 0.7f + pollPulse * 0.25f : 0.4f + pollPulse * 0.2f;
                _cECardOutline.effectColor = new Color(1f, 0.05f * (1f - pp), 0.1f * (1f - pp), outlineAlpha);
            }
            if (_cPName != null)
            {
                _cPName.color = extremePoll
                    ? new Color(1f, 0.3f * pollPulse, 0.3f * pollPulse, 1f)
                    : new Color(1f, 0.6f * (1f - pp), 0.6f * (1f - pp), 1f);
            }
            if (_cEName != null && e != null)
            {
                _cEName.color = extremePoll
                    ? new Color(1f, 0.2f * pollPulse, 0.25f * pollPulse, 1f)
                    : new Color(1f, 0.4f * (1f - pp), 0.45f * (1f - pp), 1f);
            }
        }

        // === 污染脉动下的微抖屏 — 85%+ ===
        if (pp > 0.85f && Time.time - _lastPollShakeTime > (pp > 0.95f ? 0.6f : 1.2f))
        {
            float shakeIntensity;
            float shakeDuration;
            if (pp > 0.95f)
            {
                shakeIntensity = Mathf.Lerp(0.15f, 0.3f, (pp - 0.95f) / 0.05f);
                shakeDuration = 0.35f;
            }
            else
            {
                shakeIntensity = Mathf.Lerp(0.08f, 0.15f, (pp - 0.85f) / 0.1f);
                shakeDuration = 0.2f;
            }
            ScreenEffectsManager.Instance?.Shake(shakeIntensity, shakeDuration);
            _lastPollShakeTime = Time.time;
        }
        
        // Button visibility based on tutorial stage
        int tutStage = gs.TutorialStage;
        bool isTutorialStage0 = tutStage == 0;
        
        // On F1 stage 0: only attack button for rat; attack+possess for dog (after rat killed)
        bool isDogFight = gs.CurrentEnemy != null && gs.CurrentEnemy.id == "dog";
        bool showPossessOnF1 = isTutorialStage0 && GameManager.Instance.CurrentFloor == 1 && p.totalKillsThisRun > 0 && isDogFight;
        
        if (isTutorialStage0 && GameManager.Instance.CurrentFloor == 1 && !showPossessOnF1)
        {
            // 1F新手引导阶段（实验鼠）：只显示攻击按钮
            SA(_cAtkBtn?.gameObject, true);
            SA(_cPossBtn?.gameObject, false);
            SA(_cUltBtn?.gameObject, false);
            SA(_cDefBtn?.gameObject, false);
            SA(_cFleeBtn?.gameObject, false);
        }
        else if (showPossessOnF1)
        {
            // 1F看门犬战斗：显示攻击+附身
            SA(_cAtkBtn?.gameObject, true);
            SA(_cPossBtn?.gameObject, true);
            SA(_cUltBtn?.gameObject, false);
            SA(_cDefBtn?.gameObject, false);
            SA(_cFleeBtn?.gameObject, false);
            
            if (_cPossBtn != null)
            {
                _cPossBtn.interactable = true;
                var possTxt = _cPossBtn.GetComponentInChildren<Text>();
                if (possTxt != null) possTxt.text = "附身";
                var possImg = _cPossBtn.GetComponent<Image>();
                if (possImg != null) possImg.color = Purp;
            }
        }
        else
        {
            // 正常战斗阶段：所有按钮正常显示和逻辑处理
            SA(_cAtkBtn?.gameObject, true);
            
            // 附身按钮
            if (_cPossBtn != null)
            {
                SA(_cPossBtn.gameObject, true);
                
                bool isBroken = p.formBroken;
                bool cantPossess = CompleteGameSystem.Instance != null && !CompleteGameSystem.Instance.CanAcceptNewForm();
                bool disabled = isBroken || cantPossess;
                
                _cPossBtn.interactable = !disabled;
                var possTxt = _cPossBtn.GetComponentInChildren<Text>();
                if (possTxt != null)
                {
                    possTxt.text = isBroken ? "破裂" : cantPossess ? "无法" : "附身";
                }
                var possImg = _cPossBtn.GetComponent<Image>();
                if (possImg != null)
                {
                    possImg.color = disabled ? new Color(0.4f, 0.35f, 0.5f) : Purp;
                }
            }
            
            if (_cDefBtn != null) SA(_cDefBtn.gameObject, GameManager.Instance.CurrentFloor >= 2);
            if (_cFleeBtn != null) SA(_cFleeBtn.gameObject, GameManager.Instance.CurrentFloor >= 2);

            // 终极技能按钮
            if (_cUltBtn != null)
            {
                SA(_cUltBtn.gameObject, true);
                var gs2 = CompleteGameSystem.Instance;
                bool ready = gs2 != null && gs2.UltReady;
                _cUltBtn.interactable = ready;
                if (_cUltLabel != null)
                {
                    if (ready)
                        _cUltLabel.text = "终极技";
                    else
                        _cUltLabel.text = $"终极技\nCD:{gs2.UltCooldown}";
                }
                var img = _cUltBtn.GetComponent<Image>();
                if (img != null) img.color = ready ? new Color(0.85f,0.65f,0.2f) : new Color(0.3f, 0.3f, 0.35f);

                var ultRT = _cUltBtn.GetComponent<RectTransform>();
                if (ready && !_ultPulsing && ultRT)
                {
                    _ultPulsing = true;
                    _ultPulse = StartCoroutine(UIAnimationSystem.Pulse(ultRT, 1.08f, 1.2f));
                }
                else if (!ready && _ultPulsing)
                {
                    _ultPulsing = false;
                    if (_ultPulse != null) { StopCoroutine(_ultPulse); _ultPulse = null; }
                    if (ultRT) ultRT.localScale = Vector3.one;
                }
            }
        }

        // 战斗中教程面板同步（面板在_root上，需手动同步）
        SyncTutorialPanel(gs);

        // 软提示气泡 - 在所有按钮颜色设置之后调用，确保高亮生效
        SyncSoftHint(gs);
    }

    int _lastFormCount = -1;
    int _lastDeadCount = -1;
    string _lastCurrentForm = "";
    float _tutShowTime;

    void SyncTutorialPanel(CompleteGameSystem gs)
    {
        // 使用Toast软提示模式，隐藏旧的教程面板
        if (_eTutGo != null && _eTutGo.activeSelf)
        {
            SA(_eTutGo, false);
            _tutShowTime = 0;
        }
        // 始终隐藏旧教程箭头
        HideTutorialArrow();
        // 隐藏教程进度条
        if (_tutorialProgressBar != null && _tutorialProgressBar.activeSelf)
        {
            SA(_tutorialProgressBar, false);
        }
    }
    
    // 属性变化动画: 统计文字闪绿/闪红 + 缩放脉冲
    IEnumerator StatChangeFlash(Text statText, bool isGain)
    {
        if (statText == null) yield break;
        var rt = statText.rectTransform;
        Color origColor = statText.color;
        Color flashColor = isGain ? new Color(0.2f, 1f, 0.6f, 1f) : new Color(1f, 0.3f, 0.3f, 1f);

        // Phase 1: 快速放大 + 闪色 (0.2s)
        float t = 0;
        while (t < 0.2f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.2f;
            float s = Mathf.Lerp(1f, 1.25f, Mathf.Sin(p * Mathf.PI));
            rt.localScale = Vector3.one * s;
            statText.color = Color.Lerp(origColor, flashColor, Mathf.Sin(p * Mathf.PI));
            yield return null;
        }
        rt.localScale = Vector3.one;
        statText.color = origColor;

        // Phase 2: 外发光 Outline 脉冲 (0.4s)
        var outline = statText.GetComponentInParent<Outline>();
        if (outline != null)
        {
            Color origOl = outline.effectColor;
            t = 0;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                float p = t / 0.4f;
                float pulse = Mathf.Sin(p * Mathf.PI);
                outline.effectColor = Color.Lerp(origOl, flashColor, pulse * 0.7f);
                yield return null;
            }
            outline.effectColor = origOl;
        }
    }
    
    void SyncExploreActionButtons(CompleteGameSystem gs)
    {
        if (gs == null) return;
        
        int stage = gs.TutorialStage;
        bool forceTut = gs.ForceTutorial;
        
        if (!forceTut)
        {
            SA(_eEvoBtn, true);
            SA(_eSaveBtn, true);
            SA(_eSkillBtn, true);
            SA(_eMenuBtn, true);
            return;
        }
        
        SA(_eEvoBtn, stage >= 1);
        SA(_eSaveBtn, stage >= 2);
        SA(_eSkillBtn, stage >= 3);
        SA(_eMenuBtn, stage >= 4);
    }

    // ============ 探索界面光效脉冲: 玩家光标光晕 + 出口脉冲圈 ============
    System.Collections.IEnumerator PulseExploreMapFX()
    {
        while (true)
        {
            yield return null;

            // 非探索界面: 隐藏所有光晕
            bool inExplore = CompleteGameSystem.Instance != null
                && CompleteGameSystem.Instance.CurrentScreen == CompleteGameSystem.RunScreen.Exploration;

            Vector2Int playerPos = new Vector2Int(-1, -1);
            Vector2Int exitPos = new Vector2Int(-1, -1);

            if (inExplore)
            {
                var fs = CompleteGameSystem.Instance?.CurrentFloorState;
                playerPos = fs?.playerPos ?? new Vector2Int(-1, -1);
                exitPos = fs?.exitPos ?? new Vector2Int(-1, -1);
            }

            // --- 玩家光标光晕 ---
            if (playerPos.x >= 0 && playerPos.y >= 0 && _mapImg[playerPos.y, playerPos.x] != null)
            {
                if (_ePlayerGlow == null)
                {
                    var go = new GameObject("PlayerGlow", typeof(RectTransform), typeof(Image));
                    go.transform.SetParent(_mapImg[playerPos.y, playerPos.x].transform, false);
                    var rt = go.GetComponent<RectTransform>();
                    rt.anchorMin = new Vector2(0.05f, 0.05f);
                    rt.anchorMax = new Vector2(0.95f, 0.95f);
                    rt.offsetMin = Vector2.zero;
                    rt.offsetMax = Vector2.zero;
                    var img = go.GetComponent<Image>();
                    img.sprite = CreateCircleSprite(128);
                    img.color = new Color(0.15f, 0.65f, 0.55f, 0.22f);
                    img.raycastTarget = false;
                    go.transform.SetAsLastSibling();
                    _ePlayerGlow = img;
                }
                else
                {
                    // 位置变化时重设 parent
                    if (playerPos != _lastPlayerGlowPos && _mapImg[playerPos.y, playerPos.x] != null)
                    {
                        _ePlayerGlow.transform.SetParent(_mapImg[playerPos.y, playerPos.x].transform, false);
                        _ePlayerGlow.transform.SetAsLastSibling();
                        _lastPlayerGlowPos = playerPos;
                    }
                    float pulse = Mathf.Sin(Time.time * 2.2f) * 0.5f + 0.5f;
                    // 玩家光晕: 极克制的呼吸感 — alpha 0.08~0.18, 颜色去饱和
                    _ePlayerGlow.color = new Color(0.15f, 0.65f, 0.55f, 0.08f + pulse * 0.10f);
                    var pRT = _ePlayerGlow.rectTransform;
                    pRT.localScale = Vector3.one * (0.98f + pulse * 0.06f);
                }
                _ePlayerGlow.gameObject.SetActive(true);
            }
            else if (_ePlayerGlow != null)
            {
                _ePlayerGlow.gameObject.SetActive(false);
            }

            // --- 出口脉冲圈 (2 层) ---
            if (exitPos.x >= 0 && exitPos.y >= 0 && _mapImg[exitPos.y, exitPos.x] != null)
            {
                // 第一次: 创建 2 层 CircleSprite
                if (_eExitGlow1 == null || _eExitGlow2 == null)
                {
                    if (_eExitGlow1 == null)
                    {
                        _eExitGlow1 = CreateExitPulseRing();
                    }
                    if (_eExitGlow2 == null)
                    {
                        _eExitGlow2 = CreateExitPulseRing();
                        _eExitGlow2.color = new Color(0.75f, 0.70f, 0.25f, 0.18f);
                    }
                }

                // 位置变化时重设 parent
                if (exitPos != _lastExitGlowPos)
                {
                    if (_mapImg[exitPos.y, exitPos.x] != null)
                    {
                        _eExitGlow1.transform.SetParent(_mapImg[exitPos.y, exitPos.x].transform, false);
                        _eExitGlow1.transform.SetAsLastSibling();
                        _eExitGlow2.transform.SetParent(_mapImg[exitPos.y, exitPos.x].transform, false);
                        _eExitGlow2.transform.SetAsLastSibling();
                        _lastExitGlowPos = exitPos;
                    }
                }

                // 不同相位呼吸 — 更慢更克制
                float t1 = Time.time * 1.8f;
                float t2 = Time.time * 2.6f + 1.5f;
                float p1 = Mathf.Sin(t1) * 0.5f + 0.5f;
                float p2 = Mathf.Sin(t2) * 0.5f + 0.5f;

                // 出口脉冲: 极弱的引导, 不抢眼 — alpha 0.05~0.15
                _eExitGlow1.color = new Color(0.30f, 0.70f, 0.40f, 0.05f + p1 * 0.10f);
                _eExitGlow1.rectTransform.localScale = Vector3.one * (0.92f + p1 * 0.12f);
                _eExitGlow2.color = new Color(0.75f, 0.70f, 0.25f, 0.04f + p2 * 0.08f);
                _eExitGlow2.rectTransform.localScale = Vector3.one * (0.88f + p2 * 0.15f);

                _eExitGlow1.gameObject.SetActive(true);
                _eExitGlow2.gameObject.SetActive(true);
            }
            else
            {
                if (_eExitGlow1 != null) _eExitGlow1.gameObject.SetActive(false);
                if (_eExitGlow2 != null) _eExitGlow2.gameObject.SetActive(false);
            }
        }
    }

    void ShowFloorBanner(int floorId)
    {
        StartCoroutine(AnimateFloorBanner(floorId));
    }

    System.Collections.IEnumerator AnimateFloorBanner(int floorId)
    {
        var theme = ShaderMaterialManager.GetFloorTheme(floorId);

        // 创建横幅 (如果还没)
        if (_floorBanner == null)
        {
            _floorBanner = new GameObject("FloorBanner", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(CanvasGroup));
            _floorBanner.transform.SetParent(_root.transform, false);
            var rt = _floorBanner.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0.08f, 0.78f);
            rt.anchorMax = new Vector2(0.92f, 0.88f);
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
            rt.pivot = new Vector2(0.5f, 0.5f);

            var img = _floorBanner.GetComponent<Image>();
            img.color = new Color(0.04f, 0.02f, 0.06f, 0.92f);

            var outline = _floorBanner.GetComponent<Outline>();
            outline.effectDistance = new Vector2(2, 2);

            _floorBannerCG = _floorBanner.GetComponent<CanvasGroup>();
            _floorBannerCG.alpha = 0f;

            // 顶部光带 (更亮的 accent 色)
            var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
            topBar.transform.SetParent(_floorBanner.transform, false);
            var barRT = topBar.GetComponent<RectTransform>();
            barRT.anchorMin = new Vector2(0, 0.9f);
            barRT.anchorMax = new Vector2(1, 1f);
            barRT.offsetMin = Vector2.zero;
            barRT.offsetMax = Vector2.zero;
            topBar.GetComponent<Image>().raycastTarget = false;

            // 横幅文字
            var titleText = new GameObject("Title", typeof(RectTransform), typeof(Text), typeof(Outline));
            titleText.transform.SetParent(_floorBanner.transform, false);
            var tRT = titleText.GetComponent<RectTransform>();
            tRT.anchorMin = new Vector2(0, 0.05f);
            tRT.anchorMax = new Vector2(1, 1f);
            tRT.offsetMin = Vector2.zero;
            tRT.offsetMax = Vector2.zero;
            tRT.pivot = new Vector2(0.5f, 0.5f);

            var txt = titleText.GetComponent<Text>();
            txt.font = F();
            txt.alignment = TextAnchor.MiddleCenter;
            txt.horizontalOverflow = HorizontalWrapMode.Wrap;
            txt.verticalOverflow = VerticalWrapMode.Overflow;

            _floorBanner.transform.SetAsLastSibling();
        }

        // 设置主题色和文字
        var imgBg = _floorBanner.GetComponent<Image>();
        var imgTopBar = _floorBanner.transform.Find("TopBar").GetComponent<Image>();
        var titleTxt = _floorBanner.transform.Find("Title").GetComponent<Text>();
        var titleOutline = _floorBanner.transform.Find("Title").GetComponent<Outline>();
        var bannerOutline = _floorBanner.GetComponent<Outline>();

        Color accent = theme.Accent;
        Color sec = theme.Sec;

        // 边框用 accent — 极弱化, 仅若隐若现
        bannerOutline.effectColor = new Color(accent.r, accent.g, accent.b, 0.25f);
        bannerOutline.effectDistance = new Vector2(0.5f, 0.5f);
        // 顶部光带 — 几乎只是个暗示
        imgTopBar.color = new Color(accent.r, accent.g, accent.b, 0.18f);
        // 文字
        titleTxt.text = $"【  F{floorId}  ·  {theme.Name}  】";
        titleTxt.color = new Color(sec.r * 1.1f + 0.05f, sec.g * 1.1f + 0.05f, sec.b * 1.1f + 0.05f, 1f);
        titleTxt.fontSize = 24;
        titleTxt.fontStyle = FontStyle.Bold;
        titleOutline.effectColor = new Color(accent.r, accent.g, accent.b, 0.25f);
        titleOutline.effectDistance = new Vector2(0.5f, 0.5f);

        // 渐入: 0 -> 1, 0.4s
        _floorBanner.SetActive(true);
        float duration = 0.4f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            _floorBannerCG.alpha = t / duration;
            yield return null;
        }
        _floorBannerCG.alpha = 1f;

        // 保持显示 2.2s
        yield return new WaitForSeconds(2.2f);

        // 渐出: 1 -> 0, 0.5s
        duration = 0.5f;
        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            _floorBannerCG.alpha = 1f - t / duration;
            yield return null;
        }
        _floorBannerCG.alpha = 0f;
        _floorBanner.SetActive(false);
    }

    Image CreateExitPulseRing()
    {
        var go = new GameObject("ExitPulse", typeof(RectTransform), typeof(Image));
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0f, 0f);
        rt.anchorMax = new Vector2(1f, 1f);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.sprite = CreateCircleSprite(128);
        img.raycastTarget = false;
        img.color = new Color(0.4f, 1f, 0.55f, 0.35f);
        return img;
    }

    void AdjustTutorialPosition(string targetId, string pos)
    {
        HideTutorialArrow();
        
        var tutRT = _eTutGo.GetComponent<RectTransform>();
        float canvasWidth = Screen.width;
        float canvasHeight = Screen.height;
        float tutWidth = canvasWidth * 0.8f;
        float tutHeight = 80f;
        
        if (string.IsNullOrEmpty(targetId))
        {
            tutRT.anchorMin = new Vector2(0.5f, 0);
            tutRT.anchorMax = new Vector2(0.5f, 0);
            tutRT.offsetMin = new Vector2(-tutWidth / 2, canvasHeight * 0.25f);
            tutRT.offsetMax = new Vector2(tutWidth / 2, canvasHeight * 0.25f + tutHeight);
            return;
        }
        
        GameObject targetObj = GameObject.Find(targetId);
        if (targetObj == null)
        {
            switch(targetId)
            {
                case "btn-possess-bottom":
                    targetObj = _cPossBtn?.gameObject;
                    break;
                case "btn-defend-bottom":
                    targetObj = _cDefBtn?.gameObject;
                    break;
                case "btn-flee-bottom":
                    targetObj = _cFleeBtn?.gameObject;
                    break;
                case "btn-attack-bottom":
                    targetObj = _cAtkBtn?.gameObject;
                    break;
                case "btn-ultimate":
                    targetObj = _cUltBtn?.gameObject;
                    break;
                case "btn-menu-icon":
                    targetObj = _eMenuBtn?.gameObject;
                    break;
                case "pol-badge":
                    targetObj = GetGuideTarget("pol-badge");
                    break;
                case "anchor-bar":
                    targetObj = GetGuideTarget("anchor-bar");
                    break;
                case "fslot-0":
                    targetObj = GetGuideTarget("fslot-0");
                    break;
                case "dpad-container":
                case "dpad":
                    targetObj = GetGuideTarget("dpad");
                    break;
            }
        }
        
        if (targetObj != null)
        {
            var targetRT = targetObj.GetComponent<RectTransform>();
            if (targetRT != null)
            {
                Vector3 targetWorldPos = targetRT.position;
                Vector2 targetScreenPos = RectTransformUtility.WorldToScreenPoint(Camera.main, targetWorldPos);
                
                tutRT.anchorMin = new Vector2(0.5f, 0);
                tutRT.anchorMax = new Vector2(0.5f, 0);
                
                if (pos == "top")
                {
                    float targetTop = targetWorldPos.y + targetRT.rect.height / 2;
                    float yPos = Mathf.Max(canvasHeight * 0.35f, targetTop + 30);
                    tutRT.offsetMin = new Vector2(-tutWidth / 2, yPos);
                    tutRT.offsetMax = new Vector2(tutWidth / 2, yPos + tutHeight);
                    
                    ShowTutorialArrow(new Vector2(canvasWidth / 2, yPos), new Vector2(targetScreenPos.x, targetTop), false);
                }
                else
                {
                    float targetBottom = targetWorldPos.y - targetRT.rect.height / 2;
                    float yPos = Mathf.Min(canvasHeight * 0.6f, targetBottom - tutHeight - 30);
                    float minYPos = canvasHeight * 0.38f;
                    if (yPos < minYPos) yPos = minYPos;
                    tutRT.offsetMin = new Vector2(-tutWidth / 2, yPos);
                    tutRT.offsetMax = new Vector2(tutWidth / 2, yPos + tutHeight);
                    
                    ShowTutorialArrow(new Vector2(canvasWidth / 2, yPos + tutHeight), new Vector2(targetScreenPos.x, targetBottom), true);
                }
            }
        }
        else
        {
            tutRT.anchorMin = new Vector2(0.5f, 0);
            tutRT.anchorMax = new Vector2(0.5f, 0);
            float defaultY = pos == "top" ? canvasHeight * 0.7f : canvasHeight * 0.25f;
            tutRT.offsetMin = new Vector2(-tutWidth / 2, defaultY);
            tutRT.offsetMax = new Vector2(tutWidth / 2, defaultY + tutHeight);
        }
    }
    
    GameObject _tutArrow;
    
    void ShowTutorialArrow(Vector2 from, Vector2 to, bool arrowDown)
    {
        if (_tutArrow == null)
        {
            _tutArrow = new GameObject("TutorialArrow", typeof(RectTransform), typeof(Image));
            _tutArrow.transform.SetParent(_canvas.transform, false);
            var rt = _tutArrow.GetComponent<RectTransform>();
            rt.anchorMin = new Vector2(0, 0);
            rt.anchorMax = new Vector2(0, 0);
            rt.sizeDelta = new Vector2(32, 32);
            
            var img = _tutArrow.GetComponent<Image>();
            img.color = new Color(0f, 1f, 0.816f);
            
            // 创建箭头精灵
            Texture2D arrowTex = new Texture2D(32, 32);
            Color[] pixels = new Color[32 * 32];
            for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
            
            // 绘制向下箭头
            int centerX = 16, centerY = 16;
            for (int i = 0; i <= 8; i++)
            {
                int x = centerX - i;
                int y = arrowDown ? centerY + i : centerY - i;
                if (x >= 0 && x < 32 && y >= 0 && y < 32) pixels[y * 32 + x] = Color.white;
                
                x = centerX + i;
                if (x >= 0 && x < 32 && y >= 0 && y < 32) pixels[y * 32 + x] = Color.white;
            }
            
            arrowTex.SetPixels(pixels);
            arrowTex.Apply();
            
            Sprite arrowSprite = Sprite.Create(arrowTex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
            img.sprite = arrowSprite;
            
            if (!arrowDown)
            {
                _tutArrow.transform.rotation = Quaternion.Euler(0, 0, 180);
            }
        }
        
        _tutArrow.SetActive(true);
        var rt2 = _tutArrow.GetComponent<RectTransform>();
        rt2.position = from;
    }
    
    void HideTutorialArrow()
    {
        if (_tutArrow != null)
        {
            _tutArrow.SetActive(false);
        }
    }

    void SyncSoftHint(CompleteGameSystem gs)
    {
        if (_softHintBubble == null) return;
        
        if (gs.CurrentScreen != CompleteGameSystem.RunScreen.Combat)
        {
            SA(_softHintBubble, false);
            ResetButtonHighlights();
            return;
        }
        
        if (!string.IsNullOrEmpty(gs.ActiveSoftHint))
        {
            SA(_softHintBubble, true);
            ST(_softHintText, gs.ActiveSoftHint);
            
            var hrt = _softHintBubble.GetComponent<RectTransform>();
            _softHintBubble.transform.SetParent(_root, false);
            
            RectTransform targetBtnRT = null;
            string targetId = gs.ActiveSoftHintTarget;
            
            switch(targetId)
            {
                case "attack": targetBtnRT = _cAtkBtn?.GetComponent<RectTransform>(); break;
                case "possess": targetBtnRT = _cPossBtn?.GetComponent<RectTransform>(); break;
                case "ultimate": targetBtnRT = _cUltBtn?.GetComponent<RectTransform>(); break;
                case "defend": targetBtnRT = _cDefBtn?.GetComponent<RectTransform>(); break;
                case "flee": targetBtnRT = _cFleeBtn?.GetComponent<RectTransform>(); break;
            }
            
            Vector2 btnCenter;

            if (targetBtnRT != null && targetBtnRT.gameObject.activeInHierarchy)
            {
                btnCenter = GetAnchorCenterInRoot(targetBtnRT);
            }
            else
            {
                btnCenter = GetFallbackButtonCenter(targetId);
            }

            // bubble 宽度根据消息长度自适应: 最少 0.22 最多 0.40
            float msgLen = gs.ActiveSoftHint != null ? gs.ActiveSoftHint.Length : 8f;
            float bubbleWidth = Mathf.Clamp(0.14f + msgLen * 0.018f, 0.22f, 0.40f);
            float bubbleHeight = 0.08f;
            float offsetAboveButton = 0.025f;
            
            float bubbleBottomY = btnCenter.y + bubbleHeight/2 + offsetAboveButton;
            float bubbleTopY = bubbleBottomY + bubbleHeight;
            bool flipArrow = false;
            
            if (bubbleTopY > 0.95f)
            {
                bubbleTopY = btnCenter.y - bubbleHeight/2 - offsetAboveButton;
                bubbleBottomY = bubbleTopY - bubbleHeight;
                flipArrow = true;
            }
            
            hrt.anchorMin = new Vector2(btnCenter.x - bubbleWidth, bubbleBottomY);
            hrt.anchorMax = new Vector2(btnCenter.x + bubbleWidth, bubbleTopY);
            hrt.pivot = new Vector2(0.5f, 0);
            hrt.offsetMin = Vector2.zero;
            hrt.offsetMax = Vector2.zero;

            // 边界钳制 — 防止气泡超出屏幕被截断
            float aMinX = hrt.anchorMin.x, aMaxX = hrt.anchorMax.x;
            float aMinY = hrt.anchorMin.y, aMaxY = hrt.anchorMax.y;
            if (aMinX < 0) { aMinX = 0; aMaxX = bubbleWidth * 2; }
            if (aMaxX > 1) { aMaxX = 1; aMinX = 1 - bubbleWidth * 2; }
            if (aMinY < 0) aMinY = 0;
            if (aMaxY > 1) aMaxY = 1;
            hrt.anchorMin = new Vector2(aMinX, aMinY);
            hrt.anchorMax = new Vector2(aMaxX, aMaxY);
            
            // Flip arrow direction if bubble is below button
            if (_softHintArrow != null)
            {
                _softHintArrow.localScale = flipArrow ? new Vector3(1, -1, 1) : Vector3.one;
                if (flipArrow)
                {
                    _softHintArrow.anchorMin = new Vector2(0.5f, 1);
                    _softHintArrow.anchorMax = new Vector2(0.5f, 1);
                    _softHintArrow.pivot = new Vector2(0.5f, 0);
                    _softHintArrow.offsetMin = new Vector2(-18, -2);
                    _softHintArrow.offsetMax = new Vector2(18, 2);
                }
                else
                {
                    _softHintArrow.anchorMin = new Vector2(0.5f, 0);
                    _softHintArrow.anchorMax = new Vector2(0.5f, 0);
                    _softHintArrow.pivot = new Vector2(0.5f, 1);
                    _softHintArrow.offsetMin = new Vector2(-18, -32);
                    _softHintArrow.offsetMax = new Vector2(18, -2);
                }
            }
            
            _softHintBubble.transform.SetAsLastSibling();
            
            ApplyButtonHighlights(gs.ActiveSoftHintTarget);
        }
        else
        {
            SA(_softHintBubble, false);
            ResetButtonHighlights();
        }
    }

    Vector2 GetAnchorCenterInRoot(RectTransform rt)
    {
        float x = (rt.anchorMin.x + rt.anchorMax.x) / 2f;
        float y = (rt.anchorMin.y + rt.anchorMax.y) / 2f;
        
        Transform parent = rt.parent;
        while (parent != null && parent != _root)
        {
            var parentRT = parent as RectTransform;
            if (parentRT == null) break;
            
            float sizeX = parentRT.anchorMax.x - parentRT.anchorMin.x;
            float sizeY = parentRT.anchorMax.y - parentRT.anchorMin.y;
            
            x = parentRT.anchorMin.x + x * sizeX;
            y = parentRT.anchorMin.y + y * sizeY;
            
            parent = parentRT.parent;
        }
        
        return new Vector2(x, y);
    }

    Vector2 GetFallbackButtonCenter(string targetId)
    {
        // Computed from actual button anchors:
        // attack:   anchorMin=(0.03,0.52), anchorMax=(0.33,0.92) → center in Actions
        // possess:  anchorMin=(0.35,0.52), anchorMax=(0.62,0.92) → center in Actions
        // ultimate: anchorMin=(0.68,0.52), anchorMax=(0.97,0.92) → center in Actions
        // defend:   anchorMin=(0.03,0.08), anchorMax=(0.33,0.48) → center in Actions
        // flee:     anchorMin=(0.35,0.08), anchorMax=(0.62,0.48) → center in Actions
        // Actions:  anchorMin=(0,0.06),    anchorMax=(1,0.28)    → in CombatPanel
        // CombatPanel: anchorMin=(0,0),    anchorMax=(1,1)       → in Root
        
        switch(targetId)
        {
            case "attack":   return new Vector2(0.18f, 0.218f);
            case "possess":  return new Vector2(0.485f, 0.218f);
            case "ultimate": return new Vector2(0.825f, 0.218f);
            case "defend":   return new Vector2(0.18f, 0.122f);
            case "flee":     return new Vector2(0.485f, 0.122f);
            default:         return new Vector2(0.5f, 0.218f);
        }
    }

    void ResetButtonHighlights()
    {
        if (_cAtkBtn != null)
        {
            var img = _cAtkBtn.GetComponent<Image>();
            if (img != null) img.color = new Color(0.9f, 0.55f, 0.3f);
        }
        if (_cPossBtn != null)
        {
            var img = _cPossBtn.GetComponent<Image>();
            if (img != null) img.color = new Color(0.55f, 0.3f, 0.8f);
        }
        if (_cDefBtn != null)
        {
            var img = _cDefBtn.GetComponent<Image>();
            if (img != null) img.color = new Color(0.3f, 0.7f, 0.6f);
        }
        if (_cFleeBtn != null)
        {
            var img = _cFleeBtn.GetComponent<Image>();
            if (img != null) img.color = new Color(0.5f, 0.5f, 0.55f);
        }
        if (_cUltBtn != null)
        {
            var img = _cUltBtn.GetComponent<Image>();
            if (img != null) img.color = new Color(0.85f, 0.65f, 0.2f);
        }
    }

    void ApplyButtonHighlights(string targetId)
    {
        ResetButtonHighlights();
        
        Color dimColor = new Color(0.3f, 0.25f, 0.35f);
        
        if (targetId == "attack")
        {
            if (_cAtkBtn != null)
            {
                var img = _cAtkBtn.GetComponent<Image>();
                if (img != null) img.color = new Color(1f, 0.7f, 0.3f);
            }
        }
        else if (targetId == "possess")
        {
            if (_cPossBtn != null)
            {
                var img = _cPossBtn.GetComponent<Image>();
                if (img != null) img.color = new Color(0.7f, 0.4f, 1f);
            }
        }
        else if (targetId == "defend")
        {
            if (_cDefBtn != null)
            {
                var img = _cDefBtn.GetComponent<Image>();
                if (img != null) img.color = new Color(0.4f, 0.9f, 0.8f);
            }
        }
        else if (targetId == "flee")
        {
            if (_cFleeBtn != null)
            {
                var img = _cFleeBtn.GetComponent<Image>();
                if (img != null) img.color = new Color(0.7f, 0.7f, 0.8f);
            }
        }
        
        if (targetId == "attack" || targetId == "possess" || targetId == "defend" || targetId == "flee")
        {
            if (_cAtkBtn != null && targetId != "attack")
            {
                var img = _cAtkBtn.GetComponent<Image>();
                if (img != null) img.color = dimColor;
            }
            if (_cPossBtn != null && targetId != "possess")
            {
                var img = _cPossBtn.GetComponent<Image>();
                if (img != null) img.color = dimColor;
            }
            if (_cDefBtn != null && targetId != "defend")
            {
                var img = _cDefBtn.GetComponent<Image>();
                if (img != null) img.color = dimColor;
            }
            if (_cFleeBtn != null && targetId != "flee")
            {
                var img = _cFleeBtn.GetComponent<Image>();
                if (img != null) img.color = dimColor;
            }
            if (_cUltBtn != null)
            {
                var img = _cUltBtn.GetComponent<Image>();
                if (img != null) img.color = dimColor;
            }
        }
    }

    void ApplySoftHintButtonHighlights(CompleteGameSystem gs)
    {
        if (!string.IsNullOrEmpty(gs.ActiveSoftHint))
        {
            ApplyButtonHighlights(gs.ActiveSoftHintTarget);
        }
        else
        {
            ResetButtonHighlights();
        }
    }

    void AdjustSoftHintPosition(string targetId)
    {
        var hrt = _softHintBubble.GetComponent<RectTransform>();
        
        _softHintBubble.transform.SetParent(_root, false);
        
        float targetX = 0.5f;
        float targetY = 0.34f;
        
        switch(targetId)
        {
            case "attack": targetX = 0.16f; break;
            case "possess": targetX = 0.46f; break;
            case "ultimate": targetX = 0.80f; break;
            case "defend": targetX = 0.16f; break;
            case "flee": targetX = 0.46f; break;
        }
        
        hrt.anchorMin = Vector2.zero;
        hrt.anchorMax = Vector2.one;
        hrt.pivot = new Vector2(0.5f, 0);
        hrt.offsetMin = new Vector2(targetX * Screen.width - 140, targetY * Screen.height);
        hrt.offsetMax = new Vector2(targetX * Screen.width + 140, targetY * Screen.height + 40);
    }

    void UpdateCombatFormSlots(CompleteGameSystem gs, GameManager.PlayerData p)
    {
        if (gs == null || p == null || _cFormRow == null)
            return;

        if (p.ownedForms == null)
            return;

        int deadCount = 0;
        if (p.deadForms != null) foreach (var d in p.deadForms) if (d) deadCount++;
        if (p.ownedForms.Count == _lastFormCount && p.currentFormId == _lastCurrentForm && deadCount == _lastDeadCount)
            return;
        _lastFormCount = p.ownedForms.Count;
        _lastDeadCount = deadCount;
        _lastCurrentForm = p.currentFormId;

        for (int i = _cFormRow.childCount - 1; i >= 0; i--)
            DestroyImmediate(_cFormRow.GetChild(i).gameObject);

        foreach (var formId in p.ownedForms)
        {
            bool isCurrent = formId == p.currentFormId;
            int slotIdx = p.ownedForms.IndexOf(formId);
            bool isDead = slotIdx >= 0 && slotIdx < p.deadForms.Count && p.deadForms[slotIdx];
            int resonanceLevel = gs.GetFormResonanceLevel(formId);
            Color resColor = GetResonanceColor(resonanceLevel);

            Color baseColor = isDead
                ? new Color(0.10f, 0.04f, 0.04f, 0.65f)
                : isCurrent
                    ? new Color(0.04f, 0.15f, 0.14f, 0.95f)
                    : new Color(0.06f, 0.05f, 0.10f, 0.88f);

            var slotGo = new GameObject($"F_{formId}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Shadow));
            slotGo.transform.SetParent(_cFormRow, false);
            slotGo.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 32);
            slotGo.GetComponent<Image>().color = baseColor;
            
            if (isCurrent)
            {
                var outline = slotGo.GetComponent<Outline>();
                outline.effectColor = new Color(0f, 1f, 0.816f, 0.8f);
                outline.effectDistance = new Vector2(2, 2);
                
                var shadow = slotGo.GetComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0.5f, 0.4f, 0.5f);
                shadow.effectDistance = new Vector2(0, 0);
            }
            else
            {
                var outline = slotGo.GetComponent<Outline>();
                outline.effectColor = Color.clear;
                
                var shadow = slotGo.GetComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.4f);
                shadow.effectDistance = new Vector2(1, -1);
            }

            var iconGo = new GameObject("I", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(slotGo.transform, false);
            var iconRT = iconGo.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.04f, 0.06f);
            iconRT.anchorMax = new Vector2(0.50f, 0.94f);
            iconRT.offsetMin = Vector2.zero; iconRT.offsetMax = Vector2.zero;
            iconGo.GetComponent<Image>().preserveAspect = true;
            iconGo.GetComponent<Image>().color = isDead ? new Color(0.3f, 0.15f, 0.15f, 0.5f) : new Color(1,1,1,0.95f);
            LoadIcon(iconGo.GetComponent<Image>(), formId == "human" ? "c_" + p.selectedClass : formId);

            var resGo = new GameObject("Resonance", typeof(RectTransform), typeof(Image));
            resGo.transform.SetParent(slotGo.transform, false);
            var resRT = resGo.GetComponent<RectTransform>();
            resRT.anchorMin = new Vector2(0.74f, 0.56f);
            resRT.anchorMax = new Vector2(0.98f, 0.94f);
            resRT.offsetMin = Vector2.zero; resRT.offsetMax = Vector2.zero;
            resGo.GetComponent<Image>().color = GetResonanceBgColor(resonanceLevel);
            
            var resTxtGo = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            resTxtGo.transform.SetParent(resGo.transform, false);
            Stretch(resTxtGo);
            var resTxt = resTxtGo.GetComponent<Text>();
            resTxt.font = F(); resTxt.fontSize = 8; resTxt.color = resColor; resTxt.alignment = TextAnchor.MiddleCenter;
            resTxt.text = $"★{resonanceLevel}"; resTxt.fontStyle = FontStyle.Bold; resTxt.raycastTarget = false;

            var txtGo = new GameObject("T", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(slotGo.transform, false);
            var txtRT = txtGo.GetComponent<RectTransform>();
            txtRT.anchorMin = new Vector2(0.50f, 0.04f);
            txtRT.anchorMax = new Vector2(0.98f, 0.54f);
            txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
            var t = txtGo.GetComponent<Text>();
            t.font = F();
            t.fontSize = 9;
            t.color = isDead ? new Color(0.35f, 0.12f, 0.12f)
                : isCurrent ? new Color(0.98f, 0.98f, 0.99f) : resColor;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
            string shortName = formId == "human" ? CName(p.selectedClass) : MName(formId);
            if (shortName.Length > 2) shortName = shortName.Substring(0, 2);
            t.text = isDead ? $"†{shortName}" : isCurrent ? $"<color=#00ffcc>{shortName}</color>" : $"{shortName}";
            t.raycastTarget = false;

            var btn = slotGo.GetComponent<Button>();
            btn.targetGraphic = slotGo.GetComponent<Image>();
            
            if (!isDead)
            {
                var cols = btn.colors;
                cols.normalColor = baseColor;
                cols.highlightedColor = isCurrent ? Color.Lerp(baseColor, new Color(0, 1f, 0.8f), 0.2f)
                    : Color.Lerp(baseColor, resColor, 0.2f);
                cols.pressedColor = Color.Lerp(baseColor, Color.black, 0.2f);
                btn.colors = cols;
            }

            // Click handler - switch form
            if (!isCurrent && !isDead)
            {
                string targetId = formId;
                btn.onClick.AddListener(() => {
                    gs.PlayerSwitchForm(targetId);
                });
            }
            
            if (isDead) 
            {
                btn.interactable = false;
                btn.enabled = false;
            }
        }
    }

    void UpdateExploreFormSlots(CompleteGameSystem gs, GameManager.PlayerData p)
    {
        if (gs == null || p == null || _eFormRow == null)
            return;

        if (p.ownedForms == null)
            return;

        for (int i = _eFormRow.childCount - 1; i >= 0; i--)
            DestroyImmediate(_eFormRow.GetChild(i).gameObject);

        foreach (var formId in p.ownedForms)
        {
            bool isCurrent = formId == p.currentFormId;
            int slotIdx = p.ownedForms.IndexOf(formId);
            bool isDead = slotIdx >= 0 && slotIdx < p.deadForms.Count && p.deadForms[slotIdx];
            int resonanceLevel = gs.GetFormResonanceLevel(formId);
            Color resColor = GetResonanceColor(resonanceLevel);

            Color baseColor = isDead
                ? new Color(0.10f, 0.04f, 0.04f, 0.65f)
                : isCurrent
                    ? new Color(0.04f, 0.15f, 0.14f, 0.95f)
                    : new Color(0.06f, 0.05f, 0.10f, 0.88f);

            var slotGo = new GameObject($"EF_{formId}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Shadow));
            slotGo.transform.SetParent(_eFormRow, false);
            slotGo.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 32);
            slotGo.GetComponent<Image>().color = baseColor;
            
            if (isCurrent)
            {
                var outline = slotGo.GetComponent<Outline>();
                outline.effectColor = new Color(0f, 1f, 0.816f, 0.8f);
                outline.effectDistance = new Vector2(2, 2);
                
                var shadow = slotGo.GetComponent<Shadow>();
                shadow.effectColor = new Color(0f, 0.5f, 0.4f, 0.5f);
                shadow.effectDistance = new Vector2(0, 0);
            }
            else
            {
                var outline = slotGo.GetComponent<Outline>();
                outline.effectColor = Color.clear;
                
                var shadow = slotGo.GetComponent<Shadow>();
                shadow.effectColor = new Color(0, 0, 0, 0.4f);
                shadow.effectDistance = new Vector2(1, -1);
            }

            var iconGo = new GameObject("I", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(slotGo.transform, false);
            var iconRT = iconGo.GetComponent<RectTransform>();
            iconRT.anchorMin = new Vector2(0.06f, 0.08f);
            iconRT.anchorMax = new Vector2(0.60f, 0.92f);
            iconRT.offsetMin = Vector2.zero; iconRT.offsetMax = Vector2.zero;
            iconGo.GetComponent<Image>().preserveAspect = true;
            iconGo.GetComponent<Image>().color = isDead ? new Color(0.3f, 0.15f, 0.15f, 0.5f) : new Color(1,1,1,0.95f);
            LoadIcon(iconGo.GetComponent<Image>(), formId == "human" ? "c_" + p.selectedClass : formId);

            var resGo = new GameObject("Resonance", typeof(RectTransform), typeof(Image));
            resGo.transform.SetParent(slotGo.transform, false);
            var resRT = resGo.GetComponent<RectTransform>();
            resRT.anchorMin = new Vector2(0.70f, 0.56f);
            resRT.anchorMax = new Vector2(0.98f, 0.94f);
            resRT.offsetMin = Vector2.zero; resRT.offsetMax = Vector2.zero;
            resGo.GetComponent<Image>().color = GetResonanceBgColor(resonanceLevel);
            
            var resTxtGo = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            resTxtGo.transform.SetParent(resGo.transform, false);
            Stretch(resTxtGo);
            var resTxt = resTxtGo.GetComponent<Text>();
            resTxt.font = F(); resTxt.fontSize = 7; resTxt.color = resColor; resTxt.alignment = TextAnchor.MiddleCenter;
            resTxt.text = $"★{resonanceLevel}"; resTxt.fontStyle = FontStyle.Bold; resTxt.raycastTarget = false;

            var txtGo = new GameObject("T", typeof(RectTransform), typeof(Text));
            txtGo.transform.SetParent(slotGo.transform, false);
            var txtRT = txtGo.GetComponent<RectTransform>();
            txtRT.anchorMin = new Vector2(0.60f, 0.04f);
            txtRT.anchorMax = new Vector2(0.98f, 0.54f);
            txtRT.offsetMin = Vector2.zero; txtRT.offsetMax = Vector2.zero;
            var t = txtGo.GetComponent<Text>();
            t.font = F();
            t.fontSize = 8;
            t.color = isDead ? new Color(0.35f, 0.12f, 0.12f)
                : isCurrent ? new Color(0.98f, 0.98f, 0.99f) : resColor;
            t.alignment = TextAnchor.MiddleCenter;
            t.fontStyle = FontStyle.Bold;
            string shortName = formId == "human" ? CName(p.selectedClass) : MName(formId);
            if (shortName.Length > 2) shortName = shortName.Substring(0, 2);
            t.text = isDead ? $"†{shortName}" : isCurrent ? $"<color=#00ffcc>{shortName}</color>" : $"{shortName}";
            t.raycastTarget = false;

            var btn = slotGo.GetComponent<Button>();
            btn.targetGraphic = slotGo.GetComponent<Image>();
            
            if (!isDead)
            {
                var cols = btn.colors;
                cols.normalColor = baseColor;
                cols.highlightedColor = isCurrent ? Color.Lerp(baseColor, new Color(0, 1f, 0.8f), 0.2f)
                    : Color.Lerp(baseColor, resColor, 0.2f);
                cols.pressedColor = Color.Lerp(baseColor, Color.black, 0.2f);
                btn.colors = cols;
            }

            if (!isCurrent && !isDead)
            {
                string targetId = formId;
                btn.onClick.AddListener(() => {
                    gs.PlayerSwitchForm(targetId);
                });
            }
            
            if (isDead) 
            {
                btn.interactable = false;
                btn.enabled = false;
            }
        }
    }
    
    Color GetResonanceColor(int level)
    {
        switch (level)
        {
            case 2: return new Color(0.5f, 0.9f, 0.5f);
            case 3: return new Color(0.6f, 0.8f, 1f);
            case 4: return new Color(0.9f, 0.6f, 1f);
            case 5: return new Color(1f, 0.7f, 0.4f);
            default: return new Color(0.6f, 0.6f, 0.6f);
        }
    }
    
    Color GetResonanceBgColor(int level)
    {
        switch (level)
        {
            case 2: return new Color(0.08f, 0.3f, 0.08f, 0.85f);
            case 3: return new Color(0.08f, 0.2f, 0.35f, 0.85f);
            case 4: return new Color(0.25f, 0.12f, 0.35f, 0.85f);
            case 5: return new Color(0.35f, 0.15f, 0.12f, 0.85f);
            default: return new Color(0.15f, 0.12f, 0.2f, 0.8f);
        }
    }

    void SyncOver(CompleteGameSystem gs, GameManager.PlayerData p)
    {
        if (_rptBuilt) return;
        _rptBuilt = true;

        var rpt = gs.LastReport;
        if (rpt == null) return;

        // Title based on mode
        string modeLabel;
        Color titleColor;
        if (rpt.isVictory)
        {
            modeLabel = rpt.modeName;
            titleColor = Cyan;
            ST(_rptSubtitle, $"{rpt.modeName.ToUpper()} · COMPLETE");
        }
        else
        {
            modeLabel = "闯塔终止报告";
            titleColor = Mag;
            ST(_rptSubtitle, $"ASCENT TERMINATED · {rpt.deathCause}");
        }
        ST(_rptTitle, modeLabel);
        if (_rptTitle) _rptTitle.color = titleColor;

        ST(_rptRank, rpt.rank);
        if (_rptRank) _rptRank.color = rpt.rank == "SSS" ? Mag : rpt.rank.StartsWith("S") ? Cyan : Gold;

        ST(_rptFloor, rpt.stagesCleared > 0
            ? $"第{rpt.stagesCleared}关 F{rpt.floorsReached}/{rpt.maxFloors}"
            : $"{rpt.floorsReached}/{rpt.maxFloors}" + (rpt.floorsReached >= rpt.maxFloors ? " ★" : ""));
        ST(_rptPossess, $"{rpt.possessions}");
        ST(_rptKills, $"{rpt.kills}");

        int mins = Mathf.FloorToInt(rpt.survivalSeconds / 60f);
        int secs = Mathf.FloorToInt(rpt.survivalSeconds % 60f);
        ST(_rptTime, $"{mins}分{secs}秒" + (mins < 3 && rpt.isVictory ? " ★" : ""));

        int hostMins = Mathf.FloorToInt(rpt.longestHostSeconds / 60f);
        int hostSecs = Mathf.FloorToInt(rpt.longestHostSeconds % 60f);
        ST(_rptHost, $"{rpt.longestHost}（{hostMins*60+hostSecs}秒）");

        ST(_rptPollution, $"{rpt.maxPollution:F0}%");
        if (_rptPollution) _rptPollution.color = rpt.maxPollution >= 80 ? Mag : rpt.maxPollution >= 50 ? Gold : Cyan;

        ST(_rptForm, rpt.finalForm);

        ST(_rptScore, $"SCORE  {rpt.score}");
        ST(_rptEcho, $"ECHO · 残响  +{rpt.echoReward}");

        string[] quotes = {
            "你征服了暗塔，但塔已占据你的灵魂。",
            "每一次闯塔都是一次蜕变。",
            "寄生不是夺取，是你在延续。",
            "暗塔之轮从未停歇。"
        };
        ST(_rptQuote, $"\"{quotes[UnityEngine.Random.Range(0, quotes.Length)]}\"");

        // 构筑回顾
        ST(_rptBuildTier, rpt.pollutionTier);
        ST(_rptBuildForms, $"{rpt.formCount}");
        ST(_rptBuildLegacy, rpt.legacySummary);

        if (_rptShareBtn != null)
        {
            var btn = _rptShareBtn.GetComponent<Button>();
            var img = _rptShareBtn.GetComponent<Image>();
            bool canShare = PosterShareSystem.Instance != null && rpt != null;
            if (btn != null) btn.interactable = canShare;
            if (img != null) img.color = canShare ? new Color(0.66f, 0.33f, 0.97f) : new Color(0.3f, 0.2f, 0.35f);
        }
    }

    void SyncEnd(CompleteGameSystem gs)
    {
        ST(_enTitle, gs.LastEndingTitle);
        ST(_enSub, gs.LastMessage);
        ST(_enBody, gs.LastEndingText);
    }

    void SyncAltar(CompleteGameSystem gs)
    {
        var a = gs.PendingAltar; if(a==null) return;
        ST(_alAggN, a.aggressiveName); ST(_alAggD, a.aggressiveDesc);
        ST(_alConN, a.conservativeName); ST(_alConD, a.conservativeDesc);
    }

    // ========== UI HELPERS ==========
    static void SA(GameObject g, bool v) { if(g) g.SetActive(v); }
    static void ST(Text t, string v) { if(t) t.text = v ?? ""; }

    GameObject Panel(string n, Color bg)
    {
        var go = new GameObject(n, typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        go.transform.SetParent(_root, false);
        Stretch(go);
        var image = go.GetComponent<Image>();
        var defaultBg = ParasiteTowerArtOptimization.UIPolish.PanelStyle.BackgroundColor;
        image.color = Color.Lerp(defaultBg, bg, 0.65f);
        image.raycastTarget = true;

        var outline = go.AddComponent<Outline>();
        outline.effectColor = ParasiteTowerArtOptimization.UIPolish.PanelStyle.BorderColor;
        outline.effectDistance = new Vector2(1, 1);

        var shadow = go.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.35f);
        shadow.effectDistance = new Vector2(2, -2);
        return go;
    }

    static void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    // 创建保持原始宽高比的图标
    // mode: 0=fit(完整显示,留白透明), 1=cover(填满容器,裁剪超出)
    // 返回的RawImage会在布局完成后自动校正尺寸
    static RawImage BuildAspectIcon(Transform parent, Texture2D tex, int padding = 0, int mode = 1)
    {
        var icoGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
        icoGo.transform.SetParent(parent, false);
        var ri = icoGo.GetComponent<RawImage>();
        ri.texture = tex;
        ri.raycastTarget = false;

        // 使用中心锚点，基于父容器尺寸计算
        var rt = icoGo.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;

        // 获取父容器尺寸：rect最准确（布局完成后），其次LayoutElement.preferred，最后sizeDelta
        float pw = 0f, ph = 0f;
        var parentRT = parent as RectTransform;
        if (parentRT != null)
        {
            // 1) 优先用rect（布局系统运行后的最终尺寸）
            if (parentRT.rect.width > 0 && parentRT.rect.width < 50000f) pw = parentRT.rect.width;
            if (parentRT.rect.height > 0 && parentRT.rect.height < 50000f) ph = parentRT.rect.height;

            // 2) 若rect为0，用LayoutElement.preferredSize（布局前就已设置）
            if (pw <= 0 || ph <= 0)
            {
                var ple = parentRT.GetComponent<LayoutElement>();
                if (ple != null)
                {
                    if (ple.preferredWidth > 0 && pw <= 0) pw = ple.preferredWidth;
                    if (ple.preferredHeight > 0 && ph <= 0) ph = ple.preferredHeight;
                }
            }

            // 3) 若仍为0，用sizeDelta（可能是手动设置的固定尺寸）
            if (pw <= 0 && parentRT.sizeDelta.x > 0 && parentRT.sizeDelta.x < 50000f) pw = parentRT.sizeDelta.x;
            if (ph <= 0 && parentRT.sizeDelta.y > 0 && parentRT.sizeDelta.y < 50000f) ph = parentRT.sizeDelta.y;
        }

        // 如果初始无法获取尺寸，使用默认值并标记为需要延迟校正
        bool needsPostLayoutFix = false;
        if (pw <= 0 || ph <= 0)
        {
            pw = 100f;
            ph = 100f;
            needsPostLayoutFix = true;
        }

        float usableW = Mathf.Max(1, pw - padding * 2);
        float usableH = Mathf.Max(1, ph - padding * 2);

        float texAspect = (float)tex.width / tex.height;
        float containerAspect = usableW / usableH;

        float showW, showH;
        float uvW, uvH, uvX, uvY;

        if (mode == 0) // fit: 完整显示
        {
            if (texAspect >= containerAspect)
            {
                showW = usableW;
                showH = usableW / texAspect;
            }
            else
            {
                showH = usableH;
                showW = usableH * texAspect;
            }
            uvX = 0f; uvY = 0f; uvW = 1f; uvH = 1f;
        }
        else // cover: 填满容器（默认）
        {
            if (texAspect >= containerAspect)
            {
                showW = usableW;
                showH = usableW / texAspect;
                // 裁剪上下
                uvH = containerAspect / texAspect;
                uvX = 0f; uvW = 1f;
                uvY = (1f - uvH) * 0.5f;
            }
            else
            {
                showH = usableH;
                showW = usableH * texAspect;
                // 裁剪左右
                uvW = texAspect / containerAspect;
                uvX = (1f - uvW) * 0.5f;
                uvY = 0f; uvH = 1f;
            }
        }

        rt.sizeDelta = new Vector2(showW, showH);
        ri.uvRect = new Rect(uvX, uvY, uvW, uvH);

        // 如果初始使用了默认值或父容器rect为0，标记为需要延迟校正
        if (needsPostLayoutFix || (parentRT != null && (parentRT.rect.width <= 0 || parentRT.rect.height <= 0)))
        {
            _pendingAspectIcons.Add(new AspectIconTask { iconRT = rt, tex = tex, padding = padding, mode = mode, parentRT = parentRT });
        }

        return ri;
    }

    // 延迟校正的图标任务列表（静态，所有CanvasUIManager实例共享）
    static readonly List<AspectIconTask> _pendingAspectIcons = new List<AspectIconTask>();
    struct AspectIconTask
    {
        public RectTransform iconRT;
        public Texture2D tex;
        public int padding;
        public int mode;
        public RectTransform parentRT;
    }

    // 统一校正所有待处理的图标尺寸
    public static void FlushAspectIcons()
    {
        for (int i = _pendingAspectIcons.Count - 1; i >= 0; i--)
        {
            var task = _pendingAspectIcons[i];
            if (task.iconRT == null || task.parentRT == null)
            {
                _pendingAspectIcons.RemoveAt(i);
                continue;
            }

            float pw = task.parentRT.rect.width;
            float ph = task.parentRT.rect.height;
            if (pw <= 1f || ph <= 1f) continue; // 布局还未完成

            float usableW = Mathf.Max(1, pw - task.padding * 2);
            float usableH = Mathf.Max(1, ph - task.padding * 2);

            float texAspect = (float)task.tex.width / task.tex.height;
            float containerAspect = usableW / usableH;

            float showW, showH;
            float uvW, uvH, uvX, uvY;

            if (task.mode == 0) // fit
            {
                if (texAspect >= containerAspect)
                {
                    showW = usableW;
                    showH = usableW / texAspect;
                }
                else
                {
                    showH = usableH;
                    showW = usableH * texAspect;
                }
                uvX = 0f; uvY = 0f; uvW = 1f; uvH = 1f;
            }
            else // cover
            {
                if (texAspect >= containerAspect)
                {
                    showW = usableW;
                    showH = usableW / texAspect;
                    uvH = containerAspect / texAspect;
                    uvX = 0f; uvW = 1f;
                    uvY = (1f - uvH) * 0.5f;
                }
                else
                {
                    showH = usableH;
                    showW = usableH * texAspect;
                    uvW = texAspect / containerAspect;
                    uvX = (1f - uvW) * 0.5f;
                    uvY = 0f; uvH = 1f;
                }
            }

            task.iconRT.sizeDelta = new Vector2(showW, showH);
            var ri = task.iconRT.GetComponent<RawImage>();
            if (ri != null) ri.uvRect = new Rect(uvX, uvY, uvW, uvH);

            _pendingAspectIcons.RemoveAt(i);
        }
    }

    VerticalLayoutGroup AddVL(GameObject go, int pad, int space)
    {
        var vl = go.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(pad,pad,pad,pad);
        vl.spacing = space;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        return vl;
    }

    GameObject Txt(object parent, string text, int size, Color col, float h)
    {
        var go = new GameObject("T", typeof(RectTransform), typeof(Text), typeof(Outline), typeof(Shadow));
        go.transform.SetParent(GetTransform(parent), false);
        var t = go.GetComponent<Text>();
        t.font = F(); t.text = text; t.fontSize = size; t.color = col;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Wrap;
        t.verticalOverflow = VerticalWrapMode.Overflow;
        t.fontStyle = FontStyle.Normal;
        t.raycastTarget = false;

        var outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(0.05f, 0.02f, 0.10f, 0.40f);
        outline.effectDistance = new Vector2(1, 1);

        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0.05f, 0.02f, 0.10f, 0.20f);
        shadow.effectDistance = new Vector2(1, -1);

        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = h;
        return go;
    }

    void Spacer(object parent, float h)
    {
        var go = new GameObject("S", typeof(RectTransform));
        go.transform.SetParent(GetTransform(parent), false);
        go.AddComponent<LayoutElement>().preferredHeight = h;
    }

    void SepLine(object parent)
    {
        var go = new GameObject("Sep", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(GetTransform(parent), false);
        go.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);
        var le = go.AddComponent<LayoutElement>();
        le.preferredHeight = 1;
        le.flexibleWidth = 1;
    }

    GameObject BtnGo(Transform parent, string label, int fontSize, Color accent, float h)
    {
        var go = new GameObject("Btn", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().preferredHeight = h;

        var buttonBg = go.GetComponent<Image>();
        buttonBg.color = BtnBg;
        buttonBg.raycastTarget = true;

        var txt = new GameObject("T", typeof(RectTransform), typeof(Text));
        txt.transform.SetParent(go.transform, false);
        Stretch(txt);
        var t = txt.GetComponent<Text>();
        t.font = F(); t.text = label; t.fontSize = fontSize; t.color = accent;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        t.fontStyle = FontStyle.Bold;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = buttonBg;
        var colors = btn.colors;
        colors.normalColor = BtnBg;
        colors.highlightedColor = Color.Lerp(BtnBg, Color.white, 0.12f);
        colors.pressedColor = Color.Lerp(BtnBg, Color.black, 0.15f);
        colors.disabledColor = new Color(BtnBg.r * 0.5f, BtnBg.g * 0.5f, BtnBg.b * 0.5f, 0.65f);
        btn.colors = colors;

        btn.onClick.AddListener(() => AudioManager.Instance?.PlaySFX("click"));

        go.AddComponent<ButtonPressFeedback>();
        return go;
    }

    GameObject CardGo(Transform parent, Color bg, float h)
    {
        var go = new GameObject("Card", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        bg.a = Mathf.Max(bg.a, 0.88f);
        go.GetComponent<Image>().color = bg;
        go.GetComponent<Image>().raycastTarget = false;
        go.AddComponent<LayoutElement>().preferredHeight = h;
        return go;
    }

    Text TxtGo(Transform parent, string text, int size, Color col)
    {
        var go = new GameObject("T", typeof(RectTransform), typeof(Text), typeof(Shadow));
        go.transform.SetParent(parent, false);
        var t = go.GetComponent<Text>();
        t.font = F();
        t.text = text;
        t.fontSize = size;
        t.color = col;
        t.alignment = TextAnchor.MiddleLeft;
        t.raycastTarget = false;

        // 添加微妙的阴影提高文字清晰度
        var shadow = go.GetComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.35f);
        shadow.effectDistance = new Vector2(1, 1);

        if (col.a > 0.8f)
        {
            var outline = go.AddComponent<Outline>();
            outline.effectColor = new Color(0, 0, 0, 0.4f);
            outline.effectDistance = new Vector2(1, 1);
        }

        return t;
    }

    GameObject Rect(RectTransform parent, Color bg, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("R", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().anchoredPosition = pos;
        go.GetComponent<RectTransform>().sizeDelta = size;
        go.GetComponent<Image>().color = bg;
        return go;
    }

    // Anchor helpers for non-layout panels
    Vector4 AnchorTop(float offsetX, float height, float topOffset = 0)
    { return new Vector4(offsetX, height, 0, topOffset); }
    Vector4 AnchorBot(float offsetX, float maxY)
    { return new Vector4(offsetX, 0, 0, maxY); }
    Vector4 AnchorStretch(float left, float top, float right, float bottom)
    { return new Vector4(left, top, right, bottom); }

    GameObject Rect(RectTransform parent, Color bg, Vector4 anchor)
    {
        var go = new GameObject("R", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<Image>().color = bg;
        // Simplified: just set sizeDelta
        go.GetComponent<RectTransform>().anchorMin = Vector2.zero;
        go.GetComponent<RectTransform>().anchorMax = Vector2.one;
        go.GetComponent<RectTransform>().offsetMin = Vector2.zero;
        go.GetComponent<RectTransform>().offsetMax = Vector2.zero;
        return go;
    }

    Image Bar(Transform parent, Color fill, Vector2 pos, Vector2 size, out Text valTxt)
    {
        var bg = new GameObject("Bar", typeof(RectTransform), typeof(Image));
        bg.transform.SetParent(parent, false);
        bg.GetComponent<RectTransform>().anchoredPosition = pos;
        bg.GetComponent<RectTransform>().sizeDelta = size;
        bg.GetComponent<Image>().color = new Color(0.18f,0.14f,0.25f);

        var fg = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fg.transform.SetParent(bg.transform, false);
        Stretch(fg);
        var img = fg.GetComponent<Image>();
        img.color = fill; img.type = Image.Type.Filled; img.fillMethod = Image.FillMethod.Horizontal;

        var t = new GameObject("V", typeof(RectTransform), typeof(Text));
        t.transform.SetParent(bg.transform, false);
        Stretch(t);
        valTxt = t.GetComponent<Text>();
        valTxt.font = F(); valTxt.fontSize = 10; valTxt.color = Color.white; valTxt.alignment = TextAnchor.MiddleCenter;
        return img;
    }

    // Overload for anchor-based bars
    Image Bar(Transform parent, Color fill, Vector4 anch, out Text valTxt)
    {
        return Bar(parent, fill, Vector2.zero, new Vector2(200, 16), out valTxt);
    }

    Image ImgChild(Transform parent, Vector2 pos, Vector2 size)
    {
        var go = new GameObject("Img", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().anchoredPosition = pos;
        go.GetComponent<RectTransform>().sizeDelta = size;
        return go.GetComponent<Image>();
    }

    Image ImgAnchored(RectTransform parent, Vector2 ancMin, Vector2 ancMax)
    {
        var go = new GameObject("Img", typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = new Vector2(4, 2); r.offsetMax = new Vector2(-4, -2);
        var img = go.GetComponent<Image>();
        img.preserveAspect = true;
        img.color = new Color(1,1,1,0.9f);
        return img;
    }

    Button ActBtn(Transform parent, string label, Color accent, Vector2 pos, Vector2 size, Action act)
    {
        var go = new GameObject("AB", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().anchoredPosition = pos;
        go.GetComponent<RectTransform>().sizeDelta = size;
        go.GetComponent<Image>().color = BtnBg;
        var t = new GameObject("T", typeof(RectTransform), typeof(Text));
        t.transform.SetParent(go.transform, false); Stretch(t);
        var tx = t.GetComponent<Text>();
        tx.font = F(); tx.text = label; tx.fontSize = 16; tx.color = accent; tx.alignment = TextAnchor.MiddleCenter;
        tx.fontStyle = FontStyle.Bold;
        tx.raycastTarget = false;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var cols = btn.colors;
        cols.normalColor = BtnBg;
        cols.highlightedColor = Color.Lerp(BtnBg, accent, 0.25f);
        cols.pressedColor = Color.Lerp(BtnBg, Color.black, 0.3f);
        cols.disabledColor = new Color(BtnBg.r * 0.5f, BtnBg.g * 0.5f, BtnBg.b * 0.5f, 0.6f);
        btn.colors = cols;
        btn.onClick.AddListener(() => act?.Invoke());
        go.AddComponent<ButtonPressFeedback>();
        return btn;
    }

    Button CreateCombatButton(Transform parent, string label, Color accent, float anchorMinX, float anchorMinY, float anchorMaxX, float anchorMaxY, Action act = null)
    {
        var go = new GameObject("CB", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(anchorMinX, anchorMinY);
        rt.anchorMax = new Vector2(anchorMaxX, anchorMaxY);
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        go.GetComponent<Image>().color = BtnBg;
        var t = new GameObject("T", typeof(RectTransform), typeof(Text));
        t.transform.SetParent(go.transform, false); Stretch(t);
        var tx = t.GetComponent<Text>();
        tx.font = F(); tx.text = label; tx.fontSize = 16; tx.color = accent; tx.alignment = TextAnchor.MiddleCenter;
        tx.fontStyle = FontStyle.Bold;
        tx.raycastTarget = false;
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = go.GetComponent<Image>();
        var cols = btn.colors;
        cols.normalColor = BtnBg;
        cols.highlightedColor = Color.Lerp(BtnBg, accent, 0.25f);
        cols.pressedColor = Color.Lerp(BtnBg, Color.black, 0.3f);
        cols.disabledColor = new Color(BtnBg.r * 0.5f, BtnBg.g * 0.5f, BtnBg.b * 0.5f, 0.6f);
        btn.colors = cols;
        btn.onClick.AddListener(() => act?.Invoke());
        go.AddComponent<ButtonPressFeedback>();
        return btn;
    }

    void HoldBtn(Transform parent, string label, Vector2 pos, float size, Action act)
    {
        var go = new GameObject("HB", typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        go.GetComponent<RectTransform>().anchoredPosition = pos;
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(size, size);
        go.GetComponent<Image>().color = BtnBg;
        var t = new GameObject("T", typeof(RectTransform), typeof(Text));
        t.transform.SetParent(go.transform, false); Stretch(t);
        var tx = t.GetComponent<Text>();
        tx.font = F(); tx.text = label; tx.fontSize = 24; tx.color = Cyan; tx.alignment = TextAnchor.MiddleCenter;
        tx.raycastTarget = false;
        var hold = go.AddComponent<HoldButton>();
        hold.onTrigger = act;
        hold.firstDelay = 0.25f;
        hold.repeatInterval = 0.1f;
    }

    void SetAnchPos(Text t, Vector2 pos)
    {
        if (t) t.GetComponent<RectTransform>().anchoredPosition = pos;
    }

    Text TxtAnchored(RectTransform parent, string text, int fontSize, Color col, Vector2 ancMin, Vector2 ancMax, float padL, float padR)
    {
        var go = new GameObject("T", typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(parent, false);
        var r = go.GetComponent<RectTransform>();
        r.anchorMin = ancMin; r.anchorMax = ancMax;
        r.offsetMin = new Vector2(padL, 0);
        r.offsetMax = new Vector2(-padR, 0);
        var t = go.GetComponent<Text>();
        t.font = F(); t.text = text; t.fontSize = fontSize; t.color = col;
        t.alignment = TextAnchor.MiddleCenter;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        t.raycastTarget = false;
        var outline = go.GetComponent<Outline>();
        outline.effectColor = new Color(0.05f, 0.02f, 0.10f, 0.55f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);

        return t;
    }

    Image BarAnchored(RectTransform parent, Color fill, Vector2 ancMin, Vector2 ancMax, out Text valTxt, bool enemy = false)
    {
        var bgGo = new GameObject("BarBg", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(parent, false);
        var bgR = bgGo.GetComponent<RectTransform>();
        bgR.anchorMin = ancMin; bgR.anchorMax = ancMax;
        bgR.offsetMin = Vector2.zero; bgR.offsetMax = Vector2.zero;
        bgGo.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.12f);

        var fgGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fgGo.transform.SetParent(bgGo.transform, false);
        var fgR = fgGo.GetComponent<RectTransform>();
        fgR.anchorMin = new Vector2(0, 0);
        fgR.anchorMax = new Vector2(1, 1);
        fgR.offsetMin = new Vector2(3, 3);
        fgR.offsetMax = new Vector2(-3, -3);
        var img = fgGo.GetComponent<Image>();
        img.color = fill;
        img.type = Image.Type.Filled;
        img.fillMethod = Image.FillMethod.Horizontal;
        img.fillOrigin = (int)Image.OriginHorizontal.Left;

        if (ShaderMaterialManager.Instance != null)
        {
            ShaderMaterialManager.Instance.ApplyHpBar(img, 1f, enemy);
            if (img.material != null)
            {
                img.type = Image.Type.Simple;
                img.color = Color.white;
                img.material.SetFloat("_FillAmount", 1f);
            }
            else
            {
                img.type = Image.Type.Filled;
            }
        }
        else
        {
            img.type = Image.Type.Filled;
        }

        var tGo = new GameObject("V", typeof(RectTransform), typeof(Text), typeof(Outline));
        tGo.transform.SetParent(bgGo.transform, false);
        var tR = tGo.GetComponent<RectTransform>();
        tR.anchorMin = new Vector2(0, 0);
        tR.anchorMax = new Vector2(1, 1);
        tR.offsetMin = Vector2.zero;
        tR.offsetMax = Vector2.zero;
        valTxt = tGo.GetComponent<Text>();
        valTxt.font = F(); 
        valTxt.fontSize = 14; 
        valTxt.color = new Color(1f, 1f, 1f); 
        valTxt.alignment = TextAnchor.MiddleCenter;
        valTxt.fontStyle = FontStyle.Bold;
        valTxt.raycastTarget = false;
        valTxt.supportRichText = true;
        
        var outline = tGo.GetComponent<Outline>();
        outline.effectColor = new Color(0.05f, 0.02f, 0.10f, 0.55f);
        outline.effectDistance = new Vector2(1.5f, -1.5f);
        
        tGo.transform.SetAsLastSibling();
        
        return img;
    }

    // ═══════════════════════════════════════════════════════
    // 标准化 UI Builder 方法
    // ═══════════════════════════════════════════════════════

    // 分区标题：16pt bold + 可选右侧计数徽章
    Text SectionHeader(VerticalLayoutGroup parent, string text, Color color, string badge = null)
    {
        var row = new GameObject("SH", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent.transform, false);
        row.GetComponent<LayoutElement>().preferredHeight = 26;
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 6; hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;

        var title = TxtGo(row.transform, text, 16, color);
        title.fontStyle = FontStyle.Bold;
        title.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        if (!string.IsNullOrEmpty(badge))
        {
            var b = TxtGo(row.transform, badge, 12, Dim);
            b.gameObject.AddComponent<LayoutElement>().preferredWidth = 50;
            b.alignment = TextAnchor.MiddleRight;
        }
        return title;
    }

    // 小标签：11pt，默认 MutedPurp
    Text Label(VerticalLayoutGroup parent, string text, Color? color = null, int fontSize = 11, int height = 16)
    {
        return Txt(parent, text, fontSize, color ?? MutedPurp, height).GetComponent<Text>();
    }

    // 分区分割线：SepLine + 居中文字 + SepLine
    void SectionDivider(VerticalLayoutGroup parent, string text, Color color)
    {
        Spacer(parent, SpaceSmall);
        var row = new GameObject("SD", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent.transform, false);
        row.GetComponent<LayoutElement>().preferredHeight = 20;
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 8; hl.childAlignment = TextAnchor.MiddleCenter;
        hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;

        var lineL = new GameObject("L", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        lineL.transform.SetParent(row.transform, false);
        lineL.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.25f);
        lineL.GetComponent<LayoutElement>().flexibleWidth = 1;
        lineL.GetComponent<LayoutElement>().preferredHeight = 1;

        var t = TxtGo(row.transform, text, 13, color);
        t.alignment = TextAnchor.MiddleCenter;

        var lineR = new GameObject("R", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        lineR.transform.SetParent(row.transform, false);
        lineR.GetComponent<Image>().color = new Color(color.r, color.g, color.b, 0.25f);
        lineR.GetComponent<LayoutElement>().flexibleWidth = 1;
        lineR.GetComponent<LayoutElement>().preferredHeight = 1;
    }

    // 主按钮：深青背景 + Cyan 文字 + outline 发光
    GameObject PrimaryBtn(Transform parent, string label, int fontSize = 16, int height = 56)
    {
        var go = new GameObject("PriBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<LayoutElement>().preferredHeight = height;
        var img = go.GetComponent<Image>();
        img.color = new Color(0.06f, 0.18f, 0.15f, 0.95f);
        img.raycastTarget = true;
        var ol = go.GetComponent<Outline>();
        ol.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.NormalBorder;
        ol.effectDistance = new Vector2(1, 1);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = img.color;
        cb.highlightedColor = new Color(0.08f, 0.24f, 0.20f, 1f);
        cb.pressedColor = new Color(0.04f, 0.12f, 0.10f, 1f);
        cb.disabledColor = new Color(0.06f, 0.06f, 0.08f, 0.6f);
        btn.colors = cb;
        go.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.3f);
        go.AddComponent<ButtonPressFeedback>();
        var txt = TxtGo(go.transform, label, fontSize, Cyan);
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);
        return go;
    }

    // 次按钮：透明背景 + accent 色 outline
    GameObject SecondaryBtn(Transform parent, string label, Color accent, int fontSize = 14, int height = 44)
    {
        var go = new GameObject("SecBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<LayoutElement>().preferredHeight = height;
        var img = go.GetComponent<Image>();
        img.color = new Color(0.08f, 0.06f, 0.12f, 0.35f);
        img.raycastTarget = true;
        var ol = go.GetComponent<Outline>();
        ol.effectColor = new Color(accent.r, accent.g, accent.b, 0.45f);
        ol.effectDistance = new Vector2(1, 1);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = img.color;
        cb.highlightedColor = new Color(accent.r * 0.18f, accent.g * 0.18f, accent.b * 0.18f, 0.65f);
        cb.pressedColor = new Color(0.04f, 0.04f, 0.06f, 0.95f);
        btn.colors = cb;
        go.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.22f);
        go.AddComponent<ButtonPressFeedback>();
        var txt = TxtGo(go.transform, label, fontSize, accent);
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);
        return go;
    }

    // 危险按钮：暗红背景 + SoftRed 文字
    GameObject DangerBtn(Transform parent, string label, int fontSize = 14, int height = 44)
    {
        var go = new GameObject("DanBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        go.GetComponent<LayoutElement>().preferredHeight = height;
        var img = go.GetComponent<Image>();
        img.color = new Color(0.15f, 0.05f, 0.05f, 0.9f);
        img.raycastTarget = true;
        var ol = go.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.3f, 0.3f, 0.35f);
        ol.effectDistance = new Vector2(1, 1);
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        var cb = btn.colors;
        cb.normalColor = img.color;
        cb.highlightedColor = new Color(0.24f, 0.09f, 0.09f, 1f);
        cb.pressedColor = new Color(0.10f, 0.03f, 0.03f, 1f);
        btn.colors = cb;
        go.AddComponent<Shadow>().effectColor = new Color(0, 0, 0, 0.26f);
        go.AddComponent<ButtonPressFeedback>();
        var txt = TxtGo(go.transform, label, fontSize, SoftRed);
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);
        return go;
    }

    // 信息卡片：CardGo + AddVL
    (GameObject card, VerticalLayoutGroup vl) InfoCard(Transform parent, Color bg, int height = 70)
    {
        var card = CardGo(parent, bg, height);
        var vl = AddVL(card, 10, 4);
        return (card, vl);
    }

    // 可点击卡片：附加 Button + targetGraphic
    (GameObject card, VerticalLayoutGroup vl) ClickCard(Transform parent, Color bg, int height = 70)
    {
        var card = CardGo(parent, bg, height);
        var vl = AddVL(card, 10, 4);
        var btn = card.AddComponent<Button>();
        btn.targetGraphic = card.GetComponent<Image>();
        return (card, vl);
    }

    // 列表行：HorizontalLayoutGroup
    GameObject ListRow(Transform parent, int height = 24)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        row.GetComponent<LayoutElement>().preferredHeight = height;
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 4; hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;
        return row;
    }

    // Tab 按钮
    GameObject TabBtn(Transform parent, string label, bool active, Color activeColor, Color? activeBg = null, float width = 0, int fontSize = 12)
    {
        var go = new GameObject("Tab", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(LayoutElement));
        go.transform.SetParent(parent, false);
        var le = go.GetComponent<LayoutElement>();
        if (width > 0) le.preferredWidth = width;
        else le.flexibleWidth = 1;
        le.preferredHeight = 32;
        var img = go.GetComponent<Image>();
        var ol = go.GetComponent<Outline>();
        var btn = go.GetComponent<Button>();
        btn.targetGraphic = img;
        if (active)
        {
            img.color = activeBg ?? new Color(activeColor.r * 0.15f, activeColor.g * 0.15f, activeColor.b * 0.15f, 0.9f);
            ol.effectColor = new Color(activeColor.r, activeColor.g, activeColor.b, 0.5f);
            ol.effectDistance = new Vector2(1, 1);
        }
        else
        {
            img.color = DarkItemBg;
            ol.effectColor = new Color(0.3f, 0.3f, 0.4f, 0.2f);
            ol.effectDistance = new Vector2(1, 1);
        }
        var txt = TxtGo(go.transform, label, fontSize, active ? activeColor : SlateText);
        if (active) txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);
        return go;
    }

    string GetZoneName(int zone)
    {
        switch (zone)
        {
            case 1: return "入口通道";
            case 2: return "生物实验室";
            case 3: return "污染核心";
            case 4: return "数据中心";
            case 5: return "中枢控制";
            default: return $"区域{zone}";
        }
    }

    void LoadIcon(Image img, string id, ThemedIconDisplay.IconTier tier = ThemedIconDisplay.IconTier.Normal)
    {
        if (!img) return;
        string cleanId = id;
        if (cleanId.Contains(" ")) cleanId = cleanId.Substring(cleanId.LastIndexOf(' ') + 1);

        if (_spriteCache.TryGetValue(cleanId, out var spr) && spr != null)
        {
            img.sprite = spr;
            img.color = Color.white;
            var themed = img.GetComponent<ThemedIconDisplay>() ?? img.gameObject.AddComponent<ThemedIconDisplay>();
            if (cleanId.StartsWith("c_"))
                tier = ThemedIconDisplay.IconTier.Class;
            themed.Configure(tier);
            return;
        }

        var tex = LoadTex("Icons/Monsters/" + cleanId);
        if (!tex) tex = LoadTex("Icons/Classes/" + cleanId);
        if (tex)
        {
            spr = Sprite.Create(tex, new UnityEngine.Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            _spriteCache[cleanId] = spr;
        }
        else
        {
            var fallbackKey = "fallback_" + cleanId;
            if (_spriteCache.TryGetValue(fallbackKey, out spr) && spr != null)
            {
                _spriteCache[cleanId] = spr;
                img.sprite = spr;
                img.color = Color.white;
                var themed = img.GetComponent<ThemedIconDisplay>() ?? img.gameObject.AddComponent<ThemedIconDisplay>();
                if (cleanId.StartsWith("c_"))
                    tier = ThemedIconDisplay.IconTier.Class;
                themed.Configure(tier);
                return;
            }
            spr = CreateFallbackIcon(cleanId);
            _spriteCache[fallbackKey] = spr;
            _spriteCache[cleanId] = spr;
        }

        if (spr != null)
        {
            img.sprite = spr;
            img.color = Color.white;
            var themed = img.GetComponent<ThemedIconDisplay>() ?? img.gameObject.AddComponent<ThemedIconDisplay>();
            if (cleanId.StartsWith("c_"))
                tier = ThemedIconDisplay.IconTier.Class;
            themed.Configure(tier);
        }
    }

    static Dictionary<string, Sprite> _spriteCache = new Dictionary<string, Sprite>();
    static Dictionary<string, Texture2D> _texCache = new Dictionary<string, Texture2D>();
    static Dictionary<string, Sprite> _decorSpriteCache = new Dictionary<string, Sprite>();
    static Dictionary<string, Sprite> _badgeSpriteCache = new Dictionary<string, Sprite>();
    static Dictionary<string, Sprite> _mapElementSpriteCache = new Dictionary<string, Sprite>();
    
    [ContextMenu("Clear All Sprite Caches")]
    public static void ClearAllSpriteCaches()
    {
        foreach (var sprite in _spriteCache.Values) if (sprite != null) Destroy(sprite.texture);
        _spriteCache.Clear();
        
        foreach (var tex in _texCache.Values) if (tex != null) Destroy(tex);
        _texCache.Clear();
        
        foreach (var sprite in _decorSpriteCache.Values) if (sprite != null) Destroy(sprite.texture);
        _decorSpriteCache.Clear();
        
        foreach (var sprite in _badgeSpriteCache.Values) if (sprite != null) Destroy(sprite.texture);
        _badgeSpriteCache.Clear();
        
        foreach (var sprite in _mapElementSpriteCache.Values) if (sprite != null) Destroy(sprite.texture);
        _mapElementSpriteCache.Clear();
    }
    
    static Sprite CreateDecorSprite(DecorationType type, Color color)
    {
        int res = 64;
        string key = type.ToString() + "_" + ColorUtility.ToHtmlStringRGB(color) + "_" + res;
        if (_decorSpriteCache.TryGetValue(key, out var spr)) return spr;
        
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[res * res];
        
        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;
        
        float cx = res / 2f;
        float cy = res / 2f;
        
        switch (type)
        {
            case DecorationType.BrokenMachine:
                DrawMachine(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.Growth:
                DrawPlant(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.Hologram:
                DrawHologram(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.Fungus:
                DrawGlow(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.Bioluminescence:
                DrawGlow(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.ToxicPuddle:
                DrawPuddle(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.Crystal:
                DrawCrystal(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.River:
                DrawRiver(tex, pixels, cx, cy, res, color);
                break;
            case DecorationType.Mountain:
                DrawMountain(tex, pixels, cx, cy, res, color);
                break;
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        spr = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        _decorSpriteCache[key] = spr;
        return spr;
    }
    
    static void DrawMountain(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float snowLine = cy - res * 0.15f;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                
                float slope1 = 1.5f;
                float slope2 = 2.5f;
                float peakWidth = res * 0.08f;
                
                bool inMountain = false;
                float height = 0;
                
                if (Mathf.Abs(dx) < peakWidth)
                {
                    height = res * 0.45f * (1 - Mathf.Abs(dx) / peakWidth);
                    inMountain = dy < -height;
                }
                else if (Mathf.Abs(dx) < res * 0.3f)
                {
                    height = res * 0.35f * (1 - (Mathf.Abs(dx) - peakWidth) / (res * 0.3f - peakWidth));
                    inMountain = dy < -height;
                }
                else if (Mathf.Abs(dx) < res * 0.45f)
                {
                    height = res * 0.2f * (1 - (Mathf.Abs(dx) - res * 0.3f) / (res * 0.45f - res * 0.3f));
                    inMountain = dy < -height;
                }
                
                if (inMountain)
                {
                    float distFromBase = -dy / (res * 0.45f);
                    float alpha = Mathf.SmoothStep(0.2f, 1f, distFromBase);
                    
                    if (y < snowLine)
                    {
                        float snowMix = Mathf.SmoothStep(0, 1, (snowLine - y) / (res * 0.15f));
                        pixels[y * res + x] = new Color(
                            color.r * (1 - snowMix) + 0.95f * snowMix,
                            color.g * (1 - snowMix) + 0.95f * snowMix,
                            color.b * (1 - snowMix) + 1f * snowMix,
                            alpha
                        );
                    }
                    else
                    {
                        pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                    }
                }
            }
        }
    }
    
    static void DrawRiver(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float riverWidth = res * 0.12f;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                
                float curve = Mathf.Sin(dx * 0.15f) * res * 0.08f;
                float distance = Mathf.Abs(dy - curve);
                
                if (distance < riverWidth)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - distance / riverWidth);
                    
                    float wavePattern = Mathf.Sin(dx * 0.2f + dy * 0.3f) * 0.1f + 0.9f;
                    pixels[y * res + x] = new Color(
                        color.r * wavePattern,
                        color.g * wavePattern,
                        color.b * wavePattern,
                        alpha
                    );
                }
                else if (distance < riverWidth + res * 0.05f)
                {
                    float bankAlpha = Mathf.SmoothStep(0, 0.3f, 1 - (distance - riverWidth) / (res * 0.05f));
                    pixels[y * res + x] = new Color(0.3f, 0.25f, 0.2f, bankAlpha);
                }
            }
        }
    }
    
    static void DrawCrystal(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                
                bool inCrystal = false;
                float shapeValue = 0;
                
                float dist1 = Mathf.Abs(dx) + Mathf.Abs(dy);
                float dist2 = Mathf.Abs(dx) - Mathf.Abs(dy);
                
                if (dist1 < res * 0.35f && dy < 0)
                {
                    shapeValue = 1 - dist1 / (res * 0.35f);
                    inCrystal = true;
                }
                else if (dist1 < res * 0.25f && dy > 0)
                {
                    shapeValue = 0.7f * (1 - dist1 / (res * 0.25f));
                    inCrystal = true;
                }
                
                if (inCrystal)
                {
                    float highlight = Mathf.Max(0, 1 - Mathf.Abs(dx) / (res * 0.1f));
                    float alpha = Mathf.SmoothStep(0.3f, 1f, shapeValue);
                    
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r + highlight * 0.5f),
                        Mathf.Min(1, color.g + highlight * 0.5f),
                        Mathf.Min(1, color.b + highlight * 0.5f),
                        alpha
                    );
                }
            }
        }
    }
    
    static void DrawMachine(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float size = res * 0.35f;
        float border = res * 0.03f;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);
                
                bool inOuter = dx < size && dy < size;
                bool inInner = dx < size - border * 2 && dy < size - border * 2;
                
                if (inOuter && !inInner)
                {
                    float alpha = 0.9f;
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                }
                
                float centerSize = size * 0.25f;
                if (dx < centerSize && dy < centerSize)
                {
                    float alpha = 0.4f;
                    pixels[y * res + x] = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, alpha);
                }
                
                float lightPos = (x - cx) / size;
                float highlight = Mathf.SmoothStep(0, 0.3f, lightPos);
                if (inOuter && !inInner && x > cx)
                {
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r + highlight),
                        Mathf.Min(1, color.g + highlight),
                        Mathf.Min(1, color.b + highlight),
                        0.9f
                    );
                }
            }
        }
    }
    
    static void DrawPlant(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                
                for (int branch = 0; branch < 3; branch++)
                {
                    float angle = (branch - 1) * 0.8f;
                    float length = res * 0.25f;
                    float width = res * 0.06f;
                    
                    float perpX = Mathf.Cos(angle);
                    float perpY = Mathf.Sin(angle);
                    
                    float proj = dx * perpX + dy * perpY;
                    float along = dx * Mathf.Sin(angle) - dy * Mathf.Cos(angle);
                    
                    if (Mathf.Abs(proj) < width && along > -res * 0.05f && along < length)
                    {
                        float alpha = Mathf.SmoothStep(0, 1, along / length);
                        pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                    }
                }
                
                float stemWidth = res * 0.04f;
                if (Mathf.Abs(dx) < stemWidth && dy < res * 0.1f)
                {
                    float alpha = Mathf.SmoothStep(0, 0.6f, 1 - Mathf.Abs(dy) / (res * 0.1f));
                    pixels[y * res + x] = new Color(color.r * 0.7f, color.g * 0.6f, color.b * 0.4f, alpha);
                }
            }
        }
    }
    
    static void DrawHologram(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                float radius1 = res * 0.15f;
                float radius2 = res * 0.25f;
                float radius3 = res * 0.35f;
                
                if (dist < radius1)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - dist / radius1);
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                }
                else if (dist > radius2 - res * 0.03f && dist < radius2 + res * 0.03f)
                {
                    float ringAlpha = Mathf.SmoothStep(0, 1, 1 - Mathf.Abs(dist - radius2) / (res * 0.03f));
                    pixels[y * res + x] = new Color(color.r * 0.8f, color.g * 0.8f, color.b, ringAlpha);
                }
                else if (dist > radius3 - res * 0.03f && dist < radius3 + res * 0.03f)
                {
                    float ringAlpha = Mathf.SmoothStep(0, 0.5f, 1 - Mathf.Abs(dist - radius3) / (res * 0.03f));
                    pixels[y * res + x] = new Color(color.r * 0.6f, color.g * 0.6f, color.b, ringAlpha);
                }
            }
        }
    }
    
    static void DrawGlow(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                float radius = res * 0.3f;
                
                if (dist < radius)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - dist / radius);
                    float brightness = Mathf.SmoothStep(0.5f, 1f, 1 - dist / radius);
                    
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r + brightness * 0.4f),
                        Mathf.Min(1, color.g + brightness * 0.4f),
                        Mathf.Min(1, color.b + brightness * 0.4f),
                        alpha
                    );
                }
                
                if (dist < radius * 1.5f)
                {
                    float outerAlpha = Mathf.SmoothStep(0, 0.2f, 1 - dist / (radius * 1.5f));
                    pixels[y * res + x] = new Color(color.r * 0.5f, color.g * 0.5f, color.b * 0.5f, outerAlpha);
                }
            }
        }
    }
    
    static void DrawPuddle(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                
                float shape = (dx * dx) / (res * 0.35f * res * 0.35f) + (dy * dy) / (res * 0.25f * res * 0.25f);
                
                if (shape < 1.2f && dy > -res * 0.1f)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - shape / 1.2f);
                    float highlight = Mathf.Max(0, 1 - Mathf.Abs(dy) / (res * 0.2f));
                    
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r + highlight * 0.3f),
                        Mathf.Min(1, color.g + highlight * 0.3f),
                        Mathf.Min(1, color.b + highlight * 0.3f),
                        alpha
                    );
                }
            }
        }
    }

    static Texture2D LoadTex(string path)
    {
        if (_texCache.TryGetValue(path, out var cached))
        {
            if (cached != null && cached)
                return cached;
            _texCache.Remove(path);
        }
        var tex = Resources.Load<Texture2D>(path);
        if (tex != null)
            _texCache[path] = tex;
        return tex;
    }

    void SetCellIcon(Image img, string id)
    {
        if (!img) return;
        
        var spriteKey = id;
        if (_spriteCache.TryGetValue(spriteKey, out var spr) && spr != null)
        {
            img.sprite = spr;
            img.color = Color.white;
            return;
        }
        
        // Try loading as Texture2D directly
        var tex = Resources.Load<Texture2D>("Icons/Monsters/"+id);
        if (!tex) tex = Resources.Load<Texture2D>("Icons/Classes/"+id);
        if (!tex) tex = Resources.Load<Texture2D>("UI/icon_"+id);
        
        // Log if failed (not cached)
        if (tex == null && !_iconFailed.Contains(id))
        {
            _iconFailed.Add(id);
            UnityEngine.Debug.LogWarning($"[SetCellIcon] FAILED to load icon: '{id}' (tried Monsters and Classes)");
        }
        else if (tex != null && !_iconLogged)
        {
            _iconLogged = true;
            UnityEngine.Debug.Log($"[SetCellIcon] Loaded '{id}': {tex.width}x{tex.height}");
        }
        
        if (tex != null)
        {
            spr = Sprite.Create(tex, new UnityEngine.Rect(0,0,tex.width,tex.height), new Vector2(0.5f,0.5f));
            _spriteCache[id] = spr;
            img.sprite = spr;
            img.color = Color.white;
        }
        else
        {
            // Fallback
            var fallbackKey = "fallback_" + id;
            if (_spriteCache.TryGetValue(fallbackKey, out spr) && spr != null)
            {
                _spriteCache[id] = spr;
                img.sprite = spr;
                img.color = Color.white;
                return;
            }
            spr = CreateFallbackIcon(id);
            _spriteCache[fallbackKey] = spr;
            _spriteCache[id] = spr;
            img.sprite = spr;
            img.color = Color.white;
        }
    }

    static Sprite _whiteSprite;
    static Sprite WhiteSprite
    {
        get
        {
            if (_whiteSprite == null)
                _whiteSprite = Sprite.Create(Texture2D.whiteTexture, new Rect(0, 0, 1, 1), new Vector2(0.5f, 0.5f));
            return _whiteSprite;
        }
    }

    static Sprite CreateFallbackIcon(string id)
    {
        int res = 64;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[res * res];

        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        float cx = res / 2f;
        float cy = res / 2f;

        Color ringColor;
        Color centerColor;

        if (id.StartsWith("c_"))
        {
            ringColor = new Color(0f, 1f, 0.816f);
            centerColor = new Color(0.2f, 0.9f, 0.75f);
        }
        else
        {
            float hash = 0;
            foreach (char c in id) hash = (hash * 31 + c) % 1000f;
            float hue = (hash / 1000f);
            ringColor = Color.HSVToRGB(hue, 0.6f, 0.9f);
            centerColor = Color.HSVToRGB(hue, 0.8f, 0.7f);
        }

        float outerR = res * 0.38f;
        float innerR = res * 0.22f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > innerR && dist < outerR)
                {
                    float ringWidth = outerR - innerR;
                    float ringPos = (dist - innerR) / ringWidth;
                    float alpha = Mathf.SmoothStep(0, 1, ringPos) * Mathf.SmoothStep(0, 1, 1 - ringPos);
                    pixels[y * res + x] = new Color(ringColor.r, ringColor.g, ringColor.b, alpha);
                }
                else if (dist <= innerR)
                {
                    float normalizedDist = dist / innerR;
                    float gradient = 1f - normalizedDist * 0.4f;
                    pixels[y * res + x] = new Color(centerColor.r * gradient, centerColor.g * gradient, centerColor.b * gradient, 0.85f);
                }
            }
        }

        float glowR = res * 0.45f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > outerR && dist < glowR)
                {
                    float glow = 1f - (dist - outerR) / (glowR - outerR);
                    float alpha = glow * glow * 0.3f;
                    pixels[y * res + x] = new Color(ringColor.r, ringColor.g, ringColor.b, alpha);
                }
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    static Transform GetTransform(object o)
    {
        if (o is VerticalLayoutGroup vl) return vl.transform;
        if (o is Transform t) return t;
        if (o is GameObject go) return go.transform;
        if (o is MonoBehaviour mb) return mb.transform;
        return null;
    }

    static Color TileC(string g, float pr)
    {
        switch(g)
        {
            case "█": return new Color(0, 0, 0, 0);
            case "□": return new Color(0, 0, 0, 0);
            case " ": return new Color(0, 0, 0, 0);
            case "·": return new Color(0, 0, 0, 0);
            case "你": return new Color(0, 0, 0, 0);
            case "⇧": return new Color(0, 0, 0, 0);
            case "王": return new Color(0, 0, 0, 0);
            case "敌": return new Color(0, 0, 0, 0);
            case "事": return new Color(0, 0, 0, 0);
            case "商": return new Color(0, 0, 0, 0);
            case "◆": return new Color(0, 0, 0, 0);
            case "◇": return new Color(0, 0, 0, 0);
            default: return new Color(0, 0, 0, 0);
        }
    }

    static Color GlyphC(string g)
    {
        switch(g){
            case "你": return new Color(0, 1f, 0.9f);
            case "⇧": return new Color(1f, 0.8f, 0.5f);
            case "王": return new Color(1f, 0.4f, 0.7f);
            case "敌": return new Color(1f, 0.5f, 0.55f);
            case "事": return new Color(1f, 0.7f, 0.5f);
            case "商": return new Color(1f, 0.85f, 0.95f);
            case "◇": return new Color(0.6f, 0.85f, 1f);
            default: return new Color(0.95f, 0.92f, 0.98f);
        }
    }

    static string CName(string id)
    {
        if (string.IsNullOrEmpty(id)) return "泰坦";
        
        var x = Array.Find(GameDataImporter.ClassColorDefinitions, c => c.id == id);
        if (!string.IsNullOrEmpty(x.id)) return x.name;
        
        return "泰坦";
    }

    static string MName(string id)
    {
        // First check MonsterDefinitions
        var x = Array.Find(GameDataImporter.MonsterDefinitions, m=>m.id==id);
        if (!string.IsNullOrEmpty(x.id)) return x.name;
        // Fallback: check current enemy or use GetDisplayName
        var gs = CompleteGameSystem.Instance;
        if (gs != null) return gs.GetDisplayName(id);
        return id;
    }

    void BuildTutorialPanel()
    {
    }

    // ========== TOAST & HINT SYSTEM ==========
    void OnShowHint(string message)
    {
        Debug.Log($"[Toast] OnShowHint received: {message}");
        if (string.IsNullOrEmpty(message)) return;
        ShowToast(message, 2f);
    }
    
    void OnShowButtonGuide(string buttonId, string message)
    {
        Debug.Log($"[Guide] OnShowButtonGuide: button={buttonId}, msg={message}");
        if (string.IsNullOrEmpty(buttonId)) return;
        ShowGuideBubble(buttonId, message, 3f);
    }

    public void ShowToast(string message, float duration = 2f)
    {
        if (string.IsNullOrEmpty(message)) return;
        Debug.Log($"[Toast] ShowToast: {message}");
        EnsureToastContainer();
        if (_toastContainer == null)
        {
            Debug.LogError("[Toast] _toastContainer is null!");
            return;
        }

        var toastGo = new GameObject("Toast", typeof(RectTransform), typeof(Image));
        toastGo.transform.SetParent(_toastContainer.transform, false);
        toastGo.layer = _toastContainer.layer;

        var toastRT = toastGo.GetComponent<RectTransform>();
        toastRT.sizeDelta = new Vector2(400f, 32f);

        var toastImg = toastGo.GetComponent<Image>();
        toastImg.color = new Color(0.04f, 0.03f, 0.08f, 0.92f);
        toastImg.raycastTarget = false;

        var toastOutline = toastGo.AddComponent<Outline>();
        toastOutline.effectColor = new Color(0f, 1f, 0.816f, 0.5f);
        toastOutline.effectDistance = new Vector2(1, -1);

        var txtGo = new GameObject("T", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(toastGo.transform, false);
        var txtRT = txtGo.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(16, 0);
        txtRT.offsetMax = new Vector2(-16, 0);
        var txt = txtGo.GetComponent<Text>();
        txt.font = F();
        txt.text = message;
        txt.fontSize = 14;
        txt.color = new Color(0f, 1f, 0.816f);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.horizontalOverflow = HorizontalWrapMode.Wrap;
        txt.verticalOverflow = VerticalWrapMode.Truncate;
        txt.raycastTarget = false;

        StartCoroutine(AnimateToastRoutine(toastGo, toastImg, txt, duration));
    }

    void EnsureToastContainer()
    {
        if (_toastContainer != null) return;
        _toastContainer = new GameObject("ToastContainer", typeof(RectTransform));
        _toastContainer.transform.SetParent(_root, false);
        _toastContainer.layer = _root.gameObject.layer;
        _toastContainerRT = _toastContainer.GetComponent<RectTransform>();
        // 居中于屏幕底部，向上移动避免与按钮重叠
        _toastContainerRT.anchorMin = new Vector2(0.5f, 0.20f);
        _toastContainerRT.anchorMax = new Vector2(0.5f, 0.20f);
        _toastContainerRT.pivot = new Vector2(0.5f, 0.5f);
        _toastContainerRT.sizeDelta = new Vector2(0, 0);

        var vlg = _toastContainer.AddComponent<VerticalLayoutGroup>();
        vlg.childControlWidth = false;
        vlg.childControlHeight = false;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.spacing = 8;
        vlg.padding = new RectOffset(0, 0, 5, 5);

        var cg = _toastContainer.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
    }

    IEnumerator AnimateToastRoutine(GameObject toastGo, Image img, Text txt, float duration)
    {
        if (toastGo == null) yield break;
        float halfDur = Mathf.Max(0.5f, duration * 0.2f);

        float t = 0;
        while (t < halfDur && toastGo != null)
        {
            t += Time.deltaTime;
            float a = Mathf.Lerp(0, 1f, Mathf.Clamp01(t / halfDur));
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.92f * a);
            if (txt != null) txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, a);
            yield return null;
        }

        float holdTime = Mathf.Max(0.3f, duration - halfDur * 2f);
        yield return new WaitForSeconds(holdTime);

        if (toastGo == null) yield break;
        t = 0;
        while (t < halfDur && toastGo != null)
        {
            t += Time.deltaTime;
            float a = 1f - Mathf.Clamp01(t / halfDur);
            img.color = new Color(img.color.r, img.color.g, img.color.b, 0.92f * a);
            if (txt != null) txt.color = new Color(txt.color.r, txt.color.g, txt.color.b, a);
            yield return null;
        }

        if (toastGo != null) Destroy(toastGo);
    }

    // ========== BUTTON GUIDE SYSTEM ==========
    void ShowGuideBubble(string targetId, string message, float duration = 3f)
    {
        HideGuideBubble();
        
        _guideTargetId = targetId;
        _guideShowTime = Time.time;
        
        EnsureGuideBubble();
        if (_guideBubble == null) return;
        
        _guideBubble.SetActive(true);
        _guideBubbleText.text = message;
        _guideFindRetries = 0;
        
        // 初始缩放0，淡入动画
        _guideBubbleRT.localScale = Vector3.zero;
        var cg = _guideBubble.GetComponent<CanvasGroup>();
        if (cg == null) cg = _guideBubble.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        
        // 延迟一帧再计算位置，确保目标对象布局已完成
        if (_guideCoroutine != null) StopCoroutine(_guideCoroutine);
        _guideCoroutine = StartCoroutine(GuideDelayedShowRoutine(duration));
        
        Debug.Log($"[Guide] ShowGuideBubble: {targetId} - {message}");
    }
    
    System.Collections.IEnumerator GuideDelayedShowRoutine(float duration)
    {
        // 等待一帧让布局更新完成
        yield return null;
        // 计算位置
        UpdateGuidePosition();
        // 启动动画
        yield return StartCoroutine(GuideAnimationRoutine(duration));
    }
    
    public void HideGuideBubble()
    {
        if (_guideCoroutine != null)
        {
            StopCoroutine(_guideCoroutine);
            _guideCoroutine = null;
        }
        if (_guideBubble != null && _guideBubble.activeSelf)
        {
            _guideBubble.SetActive(false);
        }
        if (_guideBubbleArrow != null)
        {
            _guideBubbleArrow.gameObject.SetActive(false);
        }
        _guideTargetId = null;
    }
    
    System.Collections.IEnumerator GuideAnimationRoutine(float duration)
    {
        // 淡入缩放
        float t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / 0.3f);
            _guideBubbleRT.localScale = Vector3.Lerp(Vector3.zero, Vector3.one, k);
            var cg = _guideBubble.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = k;
            UpdateGuidePosition();
            yield return null;
        }
        
        // 保持显示
        float remainTime = duration;
        while (remainTime > 0)
        {
            remainTime -= Time.deltaTime;
            UpdateGuidePosition();
            // 脉冲呼吸效果
            float pulse = 1f + 0.05f * Mathf.Sin(Time.time * 4f);
            _guideBubbleRT.localScale = Vector3.one * pulse;
            yield return null;
        }
        
        // 淡出
        t = 0;
        while (t < 0.3f)
        {
            t += Time.deltaTime;
            float k = Mathf.Clamp01(t / 0.3f);
            var cg = _guideBubble.GetComponent<CanvasGroup>();
            if (cg != null) cg.alpha = 1f - k;
            _guideBubbleRT.localScale = Vector3.one * (1f - 0.1f * k);
            yield return null;
        }
        
        HideGuideBubble();
    }
    
    void UpdateGuidePosition()
    {
        if (_guideBubble == null || string.IsNullOrEmpty(_guideTargetId)) return;
        
        GameObject targetObj = GetGuideTarget(_guideTargetId);
        if (targetObj == null)
        {
            // 目标暂时不存在，重试几次后再隐藏
            _guideFindRetries++;
            if (_guideFindRetries > MaxGuideFindRetries)
            {
                if (_guideBubble.activeSelf) _guideBubble.SetActive(false);
                if (_guideBubbleArrow != null) _guideBubbleArrow.gameObject.SetActive(false);
            }
            else if (!_guideBubble.activeSelf)
            {
                // 保持引导气泡可见，等待目标出现
                _guideBubble.SetActive(true);
            }
            return;
        }
        
        // 找到目标，重置重试计数
        _guideFindRetries = 0;
        
        var targetRT = targetObj.GetComponent<RectTransform>();
        if (targetRT == null) return;
        
        // 使用世界坐标转换，确保跨层级正确定位
        Vector3[] corners = new Vector3[4];
        targetRT.GetWorldCorners(corners);
        Vector3 worldCenter = (corners[0] + corners[2]) / 2f;
        
        // 转换到_root（Canvas SafeArea）的局部坐标
        Vector2 screenPoint = RectTransformUtility.WorldToScreenPoint(null, worldCenter);
        Vector2 localPos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _root as RectTransform,
            screenPoint,
            null, out localPos);
        
        // 气泡在目标上方，增加偏移距离
        float offsetY = 80f;
        _guideBubbleRT.localPosition = new Vector3(localPos.x, localPos.y + offsetY, 0);
        
        // 箭头指向按钮，并显示箭头
        if (_guideBubbleArrow != null)
        {
            _guideBubbleArrow.rectTransform.localPosition = new Vector3(localPos.x, localPos.y + 30f, 0);
            _guideBubbleArrow.gameObject.SetActive(true);
        }
    }
    
    GameObject GetGuideTarget(string targetId)
    {
        switch (targetId)
        {
            case "attack":
            case "btn-attack-bottom":
                return _cAtkBtn?.gameObject;
            case "possess":
            case "btn-possess-bottom":
                return _cPossBtn?.gameObject;
            case "defend":
            case "btn-defend-bottom":
                return _cDefBtn?.gameObject;
            case "flee":
            case "btn-flee-bottom":
                return _cFleeBtn?.gameObject;
            case "save":
            case "btn-save-bottom":
                return _eSaveBtn;
            case "evolution":
            case "btn-evolution":
                return _eEvoBtn;
            case "skill":
            case "btn-skill":
                return _eSkillBtn;
            case "menu":
            case "btn-menu":
                return _eMenuBtn;
            case "dpad":
            case "dpad-container":
                // Use Transform.Find instead of GameObject.Find to find inactive objects
                if (_explorePanel != null)
                {
                    var found = _explorePanel.transform.Find("DPad");
                    if (found != null) return found.gameObject;
                }
                // Fallback: search all panels (including inactive ones)
                var allPanels = new[] { _explorePanel, _combatPanel };
                foreach (var panel in allPanels)
                {
                    if (panel != null)
                    {
                        var found = panel.transform.Find("DPad");
                        if (found != null) return found.gameObject;
                    }
                }
                return null;
            case "fslot-0":
            case "form_slot_0":
                // 从形态行容器中查找第一个形态槽
                if (_eFormRow != null && _eFormRow.childCount > 0)
                    return _eFormRow.GetChild(0).gameObject;
                if (_cFormRow != null && _cFormRow.childCount > 0)
                    return _cFormRow.GetChild(0).gameObject;
                return null;
            case "anchor-bar":
                // 锚点管理是菜单项，指向菜单按钮
                return _eMenuBtn;
            case "pol-badge":
                // 污染徽章指向菜单中的污染技能按钮
                if (_explorePanel != null)
                {
                    var found = _explorePanel.transform.Find("PollutionBadge");
                    if (found != null) return found.gameObject;
                }
                return _eMenuBtn; // fallback到菜单按钮
            case "btn-ultimate":
                return _cUltBtn?.gameObject;
            default:
                return null;
        }
    }
    
    void EnsureGuideBubble()
    {
        if (_guideBubble != null) return;
        if (_root == null) return;
        
        _guideBubble = new GameObject("GuideBubble", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(CanvasGroup), typeof(Button));
        _guideBubble.transform.SetParent(_root, false);
        _guideBubble.layer = _root.gameObject.layer;
        
        _guideBubbleRT = _guideBubble.GetComponent<RectTransform>();
        _guideBubbleRT.sizeDelta = new Vector2(220f, 36f);
        _guideBubbleRT.anchorMin = new Vector2(0.5f, 0.5f);
        _guideBubbleRT.anchorMax = new Vector2(0.5f, 0.5f);
        _guideBubbleRT.pivot = new Vector2(0.5f, 0);
        _guideBubbleRT.position = Vector2.zero;
        
        var bgImg = _guideBubble.GetComponent<Image>();
        bgImg.color = new Color(0.04f, 0.03f, 0.08f, 0.95f);
        bgImg.raycastTarget = true;
        
        var outline = _guideBubble.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 1f, 0.816f, 0.8f);
        outline.effectDistance = new Vector2(2, -2);
        
        var cg = _guideBubble.GetComponent<CanvasGroup>();
        cg.alpha = 0f;
        cg.blocksRaycasts = true;
        
        var btn = _guideBubble.GetComponent<Button>();
        btn.targetGraphic = bgImg;
        btn.onClick.AddListener(() => {
            HideGuideBubble();
            CompleteGameSystem.Instance?.DismissSoftHint();
        });
        
        // 文字内容
        var txtGo = new GameObject("Text", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(_guideBubble.transform, false);
        var txtRT = txtGo.GetComponent<RectTransform>();
        txtRT.anchorMin = Vector2.zero;
        txtRT.anchorMax = Vector2.one;
        txtRT.offsetMin = new Vector2(12, 0);
        txtRT.offsetMax = new Vector2(-12, 0);
        _guideBubbleText = txtGo.GetComponent<Text>();
        _guideBubbleText.font = F();
        _guideBubbleText.fontSize = 13;
        _guideBubbleText.color = new Color(0f, 1f, 0.816f);
        _guideBubbleText.alignment = TextAnchor.MiddleCenter;
        _guideBubbleText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _guideBubbleText.raycastTarget = false;
        
        // 箭头
        var arrowGo = new GameObject("Arrow", typeof(RectTransform), typeof(Image));
        arrowGo.transform.SetParent(_root, false);
        _guideBubbleArrow = arrowGo.GetComponent<Image>();
        _guideBubbleArrow.color = new Color(0f, 1f, 0.816f, 0.9f);
        _guideBubbleArrow.raycastTarget = false;
        var arrowRT = arrowGo.GetComponent<RectTransform>();
        arrowRT.sizeDelta = new Vector2(12f, 12f);
        _guideBubbleArrow.sprite = CreateTriangleSprite();
        arrowGo.SetActive(false); // 初始隐藏
        
        Debug.Log("[Guide] EnsureGuideBubble created");
    }
    
    Sprite CreateTriangleSprite()
    {
        const int size = 32;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[size * size];
        
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                // 三角形指向下
                bool inTriangle = (x >= size/2 - (y - size/2)) && (x <= size/2 + (y - size/2)) && (y >= size/2);
                pixels[y * size + x] = inTriangle ? Color.white : Color.clear;
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0f));
    }
    
    void UpdateGuideAnimation()
    {
        if (_guideBubble == null || !_guideBubble.activeSelf) return;
        UpdateGuidePosition();
    }

    // ========== TUTORIAL PROGRESS BAR ==========
    void UpdateTutorialProgress()
    {
        var gs = CompleteGameSystem.Instance;
        if (gs == null) return;

        // 使用Toast软提示模式，不再显示进度条
        if (_tutorialProgressBar != null && _tutorialProgressBar.activeSelf)
            SA(_tutorialProgressBar, false);
    }

    void EnsureTutorialProgressBar()
    {
        if (_tutorialProgressBar != null) return;
        if (_root == null) return;

        _tutorialProgressBar = new GameObject("TutorialProgress", typeof(RectTransform), typeof(Image));
        _tutorialProgressBar.transform.SetParent(_root, false);
        var barRT = _tutorialProgressBar.GetComponent<RectTransform>();
        barRT.anchorMin = new Vector2(0.06f, 0.14f);
        barRT.anchorMax = new Vector2(0.94f, 0.16f);
        barRT.offsetMin = Vector2.zero;
        barRT.offsetMax = Vector2.zero;

        var barImg = _tutorialProgressBar.GetComponent<Image>();
        barImg.color = new Color(0.04f, 0.03f, 0.08f, 0.85f);
        barImg.raycastTarget = false;

        var fillGo = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        fillGo.transform.SetParent(_tutorialProgressBar.transform, false);
        var fillRT = fillGo.GetComponent<RectTransform>();
        fillRT.anchorMin = new Vector2(0, 0);
        fillRT.anchorMax = new Vector2(1, 1);
        fillRT.offsetMin = new Vector2(1, 1);
        fillRT.offsetMax = new Vector2(-1, -1);
        _tutorialProgressFill = fillGo.GetComponent<Image>();
        _tutorialProgressFill.color = new Color(0f, 1f, 0.816f, 0.7f);
        _tutorialProgressFill.raycastTarget = false;

        _tutorialStageText = TxtAnchored(barRT, "", 9, new Color(0f, 1f, 0.816f),
            new Vector2(0, 0), new Vector2(0.45f, 1), 6, 0);
        _tutorialStageText.alignment = TextAnchor.MiddleLeft;
        _tutorialStageText.raycastTarget = false;

        _tutorialProgressText = TxtAnchored(barRT, "", 9, new Color(0.7f, 0.7f, 0.75f),
            new Vector2(0.85f, 0), new Vector2(1, 1), 0, 6);
        _tutorialProgressText.alignment = TextAnchor.MiddleRight;
        _tutorialProgressText.raycastTarget = false;

        var cg = _tutorialProgressBar.AddComponent<CanvasGroup>();
        cg.alpha = 1f;
    }

    void BuildGlobalEffects()
    {
    }

    enum BadgeType { Star, Plus, Circle, BossStar, X, Shield, StairGate }
    static Sprite CreateBadgeSprite(BadgeType type, Color color)
    {
        int res = 32;
        string key = "Badge_" + type.ToString() + "_" + ColorUtility.ToHtmlStringRGB(color) + "_" + res;
        if (_badgeSpriteCache.TryGetValue(key, out var spr)) return spr;

        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[res * res];

        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        float cx = res / 2f;
        float cy = res / 2f;

        switch (type)
        {
            case BadgeType.Star:
                DrawStarBadge(tex, pixels, cx, cy, res, color);
                break;
            case BadgeType.Plus:
                DrawPlusBadge(tex, pixels, cx, cy, res, color);
                break;
            case BadgeType.Circle:
                DrawCircleBadge(tex, pixels, cx, cy, res, color);
                break;
            case BadgeType.BossStar:
                DrawBossStarBadge(tex, pixels, cx, cy, res, color);
                break;
            case BadgeType.X:
                DrawXBadge(tex, pixels, cx, cy, res, color);
                break;
            case BadgeType.Shield:
                DrawShieldBadge(tex, pixels, cx, cy, res, color);
                break;
            case BadgeType.StairGate:
                DrawStairGateBadge(tex, pixels, cx, cy, res, color);
                break;
        }

        tex.SetPixels(pixels);
        tex.Apply();

        spr = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        _badgeSpriteCache[key] = spr;
        return spr;
    }

    static void DrawStarBadge(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float outerR = res * 0.4f;
        float innerR = res * 0.15f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float starR = innerR + (outerR - innerR) * (1f + Mathf.Cos(angle * 5f)) / 2f;
                if (dist < starR)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - dist / outerR);
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }
    }

    static void DrawPlusBadge(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float crossW = res * 0.15f;
        float crossH = res * 0.4f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);
                bool inHorizontal = dx < crossH && dy < crossW;
                bool inVertical = dx < crossW && dy < crossH;
                if (inHorizontal || inVertical)
                {
                    float dist = Mathf.Max(dx, dy);
                    float alpha = Mathf.SmoothStep(0, 1, 1 - dist / crossH);
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }
    }

    static void DrawCircleBadge(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float outerR = res * 0.4f;
        float innerR = res * 0.18f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist > innerR && dist < outerR)
                {
                    float ringWidth = outerR - innerR;
                    float ringPos = (dist - innerR) / ringWidth;
                    float alpha = Mathf.SmoothStep(0, 1, ringPos) * Mathf.SmoothStep(0, 1, 1 - ringPos);
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }
    }

    static void DrawBossStarBadge(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float outerR = res * 0.4f;
        float innerR = res * 0.12f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                float starR = innerR + (outerR - innerR) * (1f + Mathf.Cos(angle * 5f)) / 2f;
                if (dist < starR)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - dist / outerR);
                    float brightness = Mathf.SmoothStep(0.7f, 1f, 1 - dist / outerR);
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r * brightness + 0.2f),
                        Mathf.Min(1, color.g * brightness + 0.2f),
                        Mathf.Min(1, color.b * brightness + 0.2f),
                        alpha
                    );
                }
            }
        }
    }

    static void DrawXBadge(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float lineW = res * 0.12f;
        float lineL = res * 0.35f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist1 = Mathf.Abs(dx - dy);
                float dist2 = Mathf.Abs(dx + dy);
                float maxDist = Mathf.Max(Mathf.Abs(dx), Mathf.Abs(dy));
                bool inDiagonal1 = dist1 < lineW && maxDist < lineL;
                bool inDiagonal2 = dist2 < lineW && maxDist < lineL;
                if (inDiagonal1 || inDiagonal2)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - maxDist / lineL);
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }
    }

    static void DrawShieldBadge(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float shieldW = res * 0.3f;
        float shieldH = res * 0.35f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = Mathf.Abs(x - cx);
                float dy = y - cy;
                float shape = dx / shieldW + dy / shieldH;
                if (shape < 1f)
                {
                    float edge = 1f;
                    bool isBorder = dx > shieldW - edge || dy > shieldH - edge;
                    float alpha = isBorder ? 0.9f : 0.6f;
                    float brightness = isBorder ? 1f : 0.8f;
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r * brightness),
                        Mathf.Min(1, color.g * brightness),
                        Mathf.Min(1, color.b * brightness),
                        alpha
                    );
                }
            }
        }
    }

    static void DrawStairGateBadge(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float gateW = res * 0.28f;
        float gateH = res * 0.38f;
        float pillarW = res * 0.06f;
        float topBarH = res * 0.06f;
        float topBarW = gateW * 2f;
        float topY = cy + gateH * 0.5f;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = Mathf.Abs(x - cx);
                float dy = y - cy;
                
                bool inLeftPillar = dx < pillarW && dy > -gateH * 0.5f && dy < gateH * 0.5f;
                bool inRightPillar = dx > gateW - pillarW && dx < gateW && dy > -gateH * 0.5f && dy < gateH * 0.5f;
                bool inTopBar = Mathf.Abs(x - cx) < topBarW * 0.5f && Mathf.Abs(y - topY) < topBarH * 0.5f;
                bool inGate = (inLeftPillar || inRightPillar || inTopBar);
                
                if (inGate)
                {
                    float edgeDist = Mathf.Min(dx, Mathf.Abs(gateW - dx));
                    float alpha = Mathf.SmoothStep(0, 1, 1f - edgeDist / (pillarW * 2f));
                    float brightness = 0.9f + 0.1f * Mathf.Sin(dx * 0.8f + dy * 0.8f);
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r * brightness),
                        Mathf.Min(1, color.g * brightness),
                        Mathf.Min(1, color.b * brightness),
                        Mathf.Clamp01(alpha)
                    );
                }
            }
        }
    }

    enum MapElementType { Stairs, Event, Fragment, Shop }
    static Dictionary<string, MapElementType> _mapGlyphToType = new Dictionary<string, MapElementType>
    {
        { "⇧", MapElementType.Stairs },
        { "事", MapElementType.Event },
        { "◆", MapElementType.Fragment },
        { "商", MapElementType.Shop },
    };
    static Dictionary<MapElementType, Color> _mapElementTypeColors = new Dictionary<MapElementType, Color>
    {
        { MapElementType.Stairs, new Color(1f, 0.85f, 0.45f) },
        { MapElementType.Event, new Color(1f, 0.85f, 0.35f) },
        { MapElementType.Fragment, new Color(0.5f, 0.8f, 1f) },
        { MapElementType.Shop, new Color(0.95f, 0.85f, 1f) },
    };

    static Sprite CreateMapElementSprite(MapElementType type, Color color)
    {
        int res = 64;
        string key = "MapElement_" + type.ToString() + "_" + ColorUtility.ToHtmlStringRGB(color) + "_" + res;
        if (_mapElementSpriteCache.TryGetValue(key, out var spr)) return spr;

        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[res * res];

        for (int i = 0; i < pixels.Length; i++) pixels[i] = Color.clear;

        float cx = res / 2f;
        float cy = res / 2f;

        switch (type)
        {
            case MapElementType.Stairs:
                DrawStairsIcon(tex, pixels, cx, cy, res, color);
                break;
            case MapElementType.Event:
                DrawEventIcon(tex, pixels, cx, cy, res, color);
                break;
            case MapElementType.Fragment:
                DrawFragmentIcon(tex, pixels, cx, cy, res, color);
                break;
            case MapElementType.Shop:
                DrawShopIcon(tex, pixels, cx, cy, res, color);
                break;
        }

        tex.SetPixels(pixels);
        tex.Apply();

        spr = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        _mapElementSpriteCache[key] = spr;
        return spr;
    }

    static void DrawStairsIcon(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float w = res * 0.35f;
        float h = res * 0.12f;
        float gap = res * 0.04f;

        for (int step = 0; step < 3; step++)
        {
            float stepY = cy - step * (h + gap);
            float stepW = w * (1 + step * 0.2f);

            for (int x = 0; x < res; x++)
            {
                for (int y = 0; y < res; y++)
                {
                    float dx = Mathf.Abs(x - cx);
                    float dy = Mathf.Abs(y - stepY);

                    if (dx < stepW && dy < h)
                    {
                        float edge = 2f;
                        bool isBorder = dx > stepW - edge || dy > h - edge || dy < edge;
                        float alpha = isBorder ? 1f : 0.7f;
                        float brightness = isBorder ? 1f : 0.8f;

                        pixels[y * res + x] = new Color(
                            Mathf.Min(1, color.r * brightness),
                            Mathf.Min(1, color.g * brightness),
                            Mathf.Min(1, color.b * brightness),
                            alpha
                        );
                    }
                }
            }
        }

        float arrowW = res * 0.1f;
        float arrowH = res * 0.15f;
        float arrowY = cy - 3 * (h + gap) - arrowH * 0.3f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = arrowY - y;

                if (dy > 0 && Mathf.Abs(dx) < arrowW * (1 - dy / arrowH))
                {
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, 1f);
                }
            }
        }
    }

    static void DrawEventIcon(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float outerR = res * 0.35f;
        float innerR = res * 0.15f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist > innerR && dist < outerR)
                {
                    float ringWidth = outerR - innerR;
                    float ringPos = (dist - innerR) / ringWidth;
                    float alpha = Mathf.SmoothStep(0, 1, ringPos) * Mathf.SmoothStep(0, 1, 1 - ringPos);
                    alpha *= 0.9f;

                    float brightness = Mathf.SmoothStep(0.7f, 1f, 1 - ringPos);
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r * brightness),
                        Mathf.Min(1, color.g * brightness),
                        Mathf.Min(1, color.b * brightness),
                        alpha
                    );
                }
            }
        }

        float starOuterR = res * 0.22f;
        float starInnerR = res * 0.08f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;

                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float starR = starInnerR + (starOuterR - starInnerR) * (1f + Mathf.Cos(angle * 5f)) / 2f;

                if (dist < starR)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1 - dist / starOuterR);
                    pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha);
                }
            }
        }
    }

    static void DrawFragmentIcon(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float outerR = res * 0.35f;
        float innerR = res * 0.12f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float hexRadius = outerR * (0.9f + 0.1f * Mathf.Cos(angle * 6f));

                if (dist <= hexRadius)
                {
                    float alpha = 0f;
                    float r = color.r, g = color.g, b = color.b;

                    float ringPos = dist / hexRadius;
                    if (ringPos > 0.75f)
                    {
                        alpha = Mathf.SmoothStep(0, 1, (ringPos - 0.75f) / 0.25f);
                        float brightness = 1f + (1f - ringPos) * 2f;
                        r = Mathf.Min(1, color.r * brightness);
                        g = Mathf.Min(1, color.g * brightness);
                        b = Mathf.Min(1, color.b * brightness);
                    }
                    else if (ringPos > 0.35f)
                    {
                        float midAlpha = Mathf.SmoothStep(0.3f, 0.6f, 1f - ringPos);
                        alpha = midAlpha;
                        float brightness = 0.7f + (1f - ringPos) * 0.3f;
                        r = Mathf.Min(1, color.r * brightness);
                        g = Mathf.Min(1, color.g * brightness);
                        b = Mathf.Min(1, color.b * brightness);
                    }

                    if (alpha > 0)
                    {
                        pixels[y * res + x] = new Color(r, g, b, alpha);
                    }
                }
            }
        }

        float innerHexR = res * 0.2f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float angle = Mathf.Atan2(dy, dx);
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                float hexR = innerHexR * (0.95f + 0.05f * Mathf.Cos(angle * 6f));

                if (dist < hexR && dist > innerHexR * 0.4f)
                {
                    float radialPos = dist / hexR;
                    float edgeAlpha = Mathf.SmoothStep(0, 0.5f, 1f - radialPos);
                    float centerAlpha = Mathf.SmoothStep(0, 0.4f, radialPos - 0.4f);
                    float alpha = Mathf.Max(edgeAlpha, centerAlpha);

                    float brightness = 0.8f + Mathf.Sin(angle * 3f + Mathf.PI) * 0.2f;
                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r * brightness + 0.3f),
                        Mathf.Min(1, color.g * brightness + 0.3f),
                        Mathf.Min(1, color.b * brightness + 0.3f),
                        alpha * 0.7f
                    );
                }
            }
        }

        float coreSize = res * 0.06f;
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = y - cy;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);

                if (dist < coreSize)
                {
                    float alpha = Mathf.SmoothStep(0, 1, 1f - dist / coreSize);
                    pixels[y * res + x] = new Color(1f, 1f, 1f, alpha);
                }
            }
        }

        float lineWidth = res * 0.03f;
        for (int i = 0; i < 3; i++)
        {
            float lineAngle = (Mathf.PI / 3f) * i;
            for (int x = 0; x < res; x++)
            {
                for (int y = 0; y < res; y++)
                {
                    float dx = x - cx;
                    float dy = y - cy;

                    float rotatedX = dx * Mathf.Cos(lineAngle) + dy * Mathf.Sin(lineAngle);
                    float rotatedY = -dx * Mathf.Sin(lineAngle) + dy * Mathf.Cos(lineAngle);

                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist < innerHexR * 0.85f && Mathf.Abs(rotatedX) < lineWidth)
                    {
                        float alpha = Mathf.SmoothStep(0, 0.4f, 1f - dist / (innerHexR * 0.85f));
                        pixels[y * res + x] = new Color(color.r, color.g, color.b, alpha * 0.6f);
                    }
                }
            }
        }
    }

    static void DrawShopIcon(Texture2D tex, Color[] pixels, float cx, float cy, int res, Color color)
    {
        float boxW = res * 0.35f;
        float boxH = res * 0.25f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);

                if (dx < boxW && dy < boxH)
                {
                    float edge = 2f;
                    bool isBorder = dx > boxW - edge || dy > boxH - edge || dy < edge;
                    float alpha = isBorder ? 1f : 0.7f;
                    float brightness = isBorder ? 1f : 0.85f;

                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r * brightness),
                        Mathf.Min(1, color.g * brightness),
                        Mathf.Min(1, color.b * brightness),
                        alpha
                    );
                }
            }
        }

        float roofW = res * 0.42f;
        float roofH = res * 0.12f;
        float roofY = cy - boxH - roofH * 0.3f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - cx;
                float dy = roofY - y;

                if (dy > 0 && Mathf.Abs(dx) < roofW * (1 - dy / roofH))
                {
                    float edge = 2f;
                    float distToEdge = Mathf.Min(dy, roofW * (1 - dy / roofH) - Mathf.Abs(dx));
                    bool isBorder = distToEdge < edge;
                    float alpha = isBorder ? 1f : 0.8f;
                    float brightness = isBorder ? 1f : 0.9f;

                    pixels[y * res + x] = new Color(
                        Mathf.Min(1, color.r * brightness),
                        Mathf.Min(1, color.g * brightness),
                        Mathf.Min(1, color.b * brightness),
                        alpha
                    );
                }
            }
        }
    }

}
