using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class CanvasUIManager : MonoBehaviour
{
    public static CanvasUIManager Instance { get; private set; }

    static Font _font;
    static Font F()
    {
        if (_font != null) return _font;
        _font = Font.CreateDynamicFontFromOSFont("Arial", 16);
        if (_font == null) _font = Font.CreateDynamicFontFromOSFont("Roboto", 16);
        if (_font == null)
        {
            var names = Font.GetOSInstalledFontNames();
            if (names.Length > 0) _font = Font.CreateDynamicFontFromOSFont(names[0], 16);
        }
        return _font;
    }

    Canvas _canvas;
    RectTransform _root;
    bool _ready;

    // Panel roots
    GameObject _menuPanel, _classPanel, _explorePanel, _combatPanel, _overPanel, _endPanel;
    GameObject _avatarBtn;
    // Login panel
    GameObject _loginPanel;
    InputField _loginNickInput;
    // Profile panel
    GameObject _profileOvl;
    Text _profNick, _profId, _profGames, _profFloor, _profKills, _profPossess;
    // Menu top bar
    Text _menuPlayerName, _menuPlayerLevel;
    string _avatarEmoji;
    static readonly string[] HeroEmojis = { "泰", "虫", "灵" };
    static readonly string[] HeroNames = { "泰坦", "蛛灵", "幽灵" };
    Text _menuEpTxt, _menuFragTxt;
    RawImage _eFloorBg;
    int _eFloorBgIdx = -1;
    GameObject _altarOvl, _collapseOvl, _possessOvl, _routeOvl, _deathFormOvl, _storyOvl, _fragChoiceOvl;
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
    CompleteGameSystem.RunScreen _lastScreen = (CompleteGameSystem.RunScreen)(-1);
    CompleteGameSystem.RunScreen _prevScreen = (CompleteGameSystem.RunScreen)(-1);

    // Explore refs
    Text _eMsg, _eFloor, _eStats, _eHpTxt, _ePollTxt, _eProg, _eTimer, _eTutTxt, _eEvoTxt, _eFormName, _ePollValue, _eFragments, _eEvoLvl, _eRebirth;
    Text _eTutTitle, _eTutStep;
    GameObject _eTutSkipBtn;
    Image _eHpFill, _ePollFill;
    Image[,] _mapImg = new Image[13,13];
    Text[,] _mapTxt = new Text[13,13];
    Image[,] _mapIcon = new Image[13,13];
    GameObject _eTutGo;
    GameObject _softHintBubble;
    Text _softHintText;
    // Explore host bar
    GameObject _eHostBar;
    Text _eHostName, _eHostTraits;

    // Combat refs
    Text _cPName, _cPHpTxt, _cPStat, _cEName, _cEHpTxt, _cEStat, _cRate, _cMsg;
    Text _cBtmHud;
    Image _cPIcon, _cPHpFill, _cEIcon, _cEHpFill;
    Transform _cFormRow;
    int _lastPlayerHp = -1, _lastEnemyHp = -1;
    float _playerHpDisplay = 1f, _enemyHpDisplay = 1f;
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

    // 主色调 - 生物朋克配色 (来自 ParasiteTowerColorScheme)
    static readonly Color Cyan = ParasiteTowerColorScheme.HealthGreen;         // #00ffd0
    static readonly Color Mag = ParasiteTowerColorScheme.CriticalRed;          // #ff006e
    static readonly Color Purp = new Color(0.65f, 0.25f, 0.95f);              // 深渊紫
    static readonly Color Gold = new Color(1f, 0.75f, 0.3f);                  // 鎏金

    // 文本颜色
    static readonly Color Dim = ParasiteTowerColorScheme.MidGray;              // #8878aa 可读的二级文字
    static readonly Color Bright = ParasiteTowerColorScheme.White;             // #f0e8ff 暖白
    static readonly Color DarkText = ParasiteTowerColorScheme.LightGray;       // #c8b8e8

    // 背景颜色 - 提亮至原版 CSS 标准
    static readonly Color PanelBg = new Color(0.07f, 0.05f, 0.12f);                    // 面板背景
    static readonly Color CardBg = new Color(0.12f, 0.08f, 0.20f, 0.92f);             // 卡片背景
    static readonly Color CardBgHover = new Color(0.16f, 0.10f, 0.25f, 0.95f);        // 卡片悬停
    static readonly Color BtnBg = new Color(0.14f, 0.10f, 0.24f, 0.92f);              // 按钮背景
    static readonly Color BtnBgHover = new Color(0.18f, 0.16f, 0.30f, 0.95f);          // 按钮悬停

    // 边框和发光 - 青色主调
    static readonly Color BorderColor = new Color(0f, 1f, 0.816f, 0.25f);              // 青色边框
    static readonly Color GlowColor = ParasiteTowerColorScheme.BioGlowCyan;            // 青色发光
    static readonly Color HighlightColor = ParasiteTowerColorScheme.HealthGreen;        // 青色高亮

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
        DontDestroyOnLoad(gameObject);
        EventBus.Register(EventTypes.PlayerStatsChanged, OnPlayerStatsChanged);
    }

    void Update()
    {
        if (!_ready) { Init(); return; }
        UpdateAchievementToast();
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
    }

    void Init()
    {
        if (_ready) return;
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
        _root = go.GetComponent<RectTransform>();
        _ready = true;

        Build();
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
        BuildAltar();
        BuildCollapse();
        BuildPossessPanel();
        BuildRoutePanel();
        BuildDeathFormPanel();
        BuildDeathRollbackPanel();
        BuildFormReplacePanel();
        BuildStoryPanel();
        BuildFragChoicePanel();
        BuildSynthConfirmPanel();
        BuildMenuPanel();
        BuildTutorialPanel();
        BuildAchievementToast();
        BuildSoftHintBubble();
        if (_avatarBtn) _avatarBtn.transform.SetAsLastSibling();
        if (_eTutGo) _eTutGo.transform.SetAsLastSibling();
        if (_softHintBubble) _softHintBubble.transform.SetAsLastSibling();
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
            _playerHpDisplay = 1f;
            _enemyHpDisplay = 1f;
            _ultPulsing = false;
            if (_ultPulse != null) { StopCoroutine(_ultPulse); _ultPulse = null; }
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
            SA(_menuOvl, false);
            SA(_pollSkillOvl, false);
            SA(_bondOvl, false);
            SA(_anchorOvl, false);
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
            }
            if (active != null)
            {
                var rt = active.GetComponent<RectTransform>();
                if (s == CompleteGameSystem.RunScreen.Combat && _prevScreen == CompleteGameSystem.RunScreen.Exploration)
                {
                    ScreenEffectsManager.Instance?.FlashDamage();
                    StartCoroutine(UIAnimationSystem.FadeIn(rt, 0.15f));
                }
                else if (s == CompleteGameSystem.RunScreen.GameOver || s == CompleteGameSystem.RunScreen.Ending)
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

    void Sync(CompleteGameSystem gs)
    {
        Show(gs.CurrentScreen);
        var p = GameManager.Instance?.Player;
        if (p == null) return;

        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.MainMenu) SyncMainMenu();
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.Exploration) SyncExplore(gs, p);
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.Combat) SyncCombat(gs, p);
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.GameOver) SyncOver(gs, p);
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.Ending) SyncEnd(gs);

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
        SA(_storyOvl, gs.ShowingStoryEvent);
        if (gs.ShowingAltar && gs.PendingAltar != null) SyncAltar(gs);
        if (gs.ShowingRouteSelect) SyncRoute(gs);
        if (gs.ShowingDeathFormSelect) SyncDeathForm(gs);
        if (gs.ShowingStoryEvent) SyncStory(gs);

        SyncEvolution();
        SyncPollutionSkill();
        SyncFormBond();
        SyncAnchor();
        SyncRecord();
        SyncFragChoice(gs);
        if (!gs.ShowingFragmentChoice) _fragChoiceBuilt = false;
        SyncSynthConfirm(gs);
        SyncSkillBar(gs);
    }

    // ========== MENU ==========
    // Menu dynamic refs
    Text _menuLastRunMode, _menuLastRunFloor, _menuLastRunHost, _menuLastRunPoll;
    Text _menuRunStatus;
    GameObject _menuContinueBtn;
    GameObject _menuLastRunCard;
    GameObject _menuContinueSpacer;
    Text _menuHostLabel, _menuPollLabel;
    Image _menuPollBar;

    float _menuSyncTime;

    void SyncMainMenu()
    {
        if (Time.time - _menuSyncTime < 2f) return;
        _menuSyncTime = Time.time;

        var save = SaveSystem.Instance?.GetSaveInfo(0);
        bool hasSave = save != null;
        SA(_menuContinueSpacer, hasSave);

        if (hasSave)
        {
            ST(_menuLastRunMode, save.gameMode ?? "Classic");
            ST(_menuLastRunFloor, $"F{save.currentFloor}");
            if (save.player != null)
            {
                ST(_menuLastRunHost, CName(save.player.selectedClass));
                float poll = save.player.pollution;
                ST(_menuLastRunPoll, $"{poll:F0}%");
                if (_menuPollBar != null)
                {
                    float pct = Mathf.Clamp01(poll / 100f);
                    var pRT = _menuPollBar.GetComponent<RectTransform>();
                    pRT.anchorMax = new Vector2(pct, 1);
                    _menuPollBar.color = pct < 0.3f ? Cyan : pct < 0.6f ? Purp : Mag;
                }
            }
            string timeStr = save.saveTime.Year > 2000 ? save.saveTime.ToString("MM/dd HH:mm") : "已保存";
            ST(_menuRunStatus, $"进行中 · {timeStr}");
        }

        if (_menuHostLabel != null)
        {
            var p = GameManager.Instance?.Player;
            if (p != null)
            {
                ST(_menuHostLabel, CName(p.selectedClass));
                ST(_menuPollLabel, $"{p.pollution:F0}%");
            }
            else if (hasSave && save.player != null)
            {
                ST(_menuHostLabel, CName(save.player.selectedClass));
                ST(_menuPollLabel, $"{save.player.pollution:F0}%");
            }
            else
            {
                ST(_menuHostLabel, "未绑定");
                ST(_menuPollLabel, "0%");
            }
        }
    }

    void BuildLoginPanel()
    {
        _loginPanel = Panel("Login", new Color(0.06f, 0.04f, 0.10f));
        var rt = _loginPanel.GetComponent<RectTransform>();

        var heroTex = Resources.Load<Texture2D>("UI/menu_hero");
        if (heroTex != null)
        {
            var bgGo = new GameObject("BgHero", typeof(RectTransform), typeof(RawImage));
            bgGo.transform.SetParent(rt, false);
            bgGo.transform.SetAsFirstSibling();
            var bgRI = bgGo.GetComponent<RawImage>();
            bgRI.texture = heroTex;
            bgRI.color = new Color(1f, 1f, 1f, 0.8f);
            bgRI.raycastTarget = false;
            var bgRT = bgGo.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        }

        var vl = AddVL(_loginPanel, 30, 12);
        Spacer(vl, 160);

        var title = Txt(vl, "你 也 是 我", 42, Bright, 60).GetComponent<Text>();
        title.fontStyle = FontStyle.Bold;
        title.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 6);
        var sub = Txt(vl, "Y O U   A R E   A L S O   M E", 9, new Color(0.35f, 0.35f, 0.45f), 18).GetComponent<Text>();
        sub.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 20);
        var quote = Txt(vl, "「你每夺走一个身体，就离自己更远一步。」", 13, new Color(0.6f, 0.55f, 0.7f), 25).GetComponent<Text>();
        quote.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 50);

        // 游客登录标签
        var loginLabel = Txt(vl, "— 游客登录 —", 16, Cyan, 30).GetComponent<Text>();
        loginLabel.alignment = TextAnchor.MiddleCenter;
        loginLabel.fontStyle = FontStyle.Bold;

        Spacer(vl, 20);

        Txt(vl, "输入昵称（留空自动生成）", 11, Dim, 20).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        Spacer(vl, 6);

        var inputGo = new GameObject("NickInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputGo.transform.SetParent(vl.transform, false);
        inputGo.AddComponent<LayoutElement>().preferredHeight = 48;
        inputGo.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.14f, 0.9f);
        inputGo.AddComponent<Outline>().effectColor = new Color(0f, 1f, 0.816f, 0.3f);
        var phTxt = TxtGo(inputGo.transform, "请输入昵称...", 14, new Color(0.4f, 0.35f, 0.5f));
        phTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(phTxt.gameObject);
        var inputTxt = TxtGo(inputGo.transform, "", 14, Bright);
        inputTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(inputTxt.gameObject);
        _loginNickInput = inputGo.GetComponent<InputField>();
        _loginNickInput.textComponent = inputTxt;
        _loginNickInput.placeholder = phTxt;
        _loginNickInput.targetGraphic = inputGo.GetComponent<Image>();
        _loginNickInput.characterLimit = 12;

        Spacer(vl, 30);

        var startBtn = new GameObject("StartBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        startBtn.transform.SetParent(vl.transform, false);
        startBtn.AddComponent<LayoutElement>().preferredHeight = 56;
        startBtn.GetComponent<Image>().color = new Color(0.06f, 0.18f, 0.15f, 0.95f);
        var startOL = startBtn.GetComponent<Outline>();
        startOL.effectColor = new Color(0f, 1f, 0.816f, 0.6f);
        startOL.effectDistance = new Vector2(2, 2);
        var startTxt = TxtGo(startBtn.transform, "开 始 旅 程", 20, Cyan);
        startTxt.alignment = TextAnchor.MiddleCenter;
        startTxt.fontStyle = FontStyle.Bold;
        Stretch(startTxt.gameObject);
        startBtn.GetComponent<Button>().targetGraphic = startBtn.GetComponent<Image>();
        startBtn.GetComponent<Button>().onClick.AddListener(() => {
            if (GuestAuthManager.Instance == null) return;
            string nick = _loginNickInput != null ? _loginNickInput.text : "";
            GuestAuthManager.Instance.GuestLogin(string.IsNullOrEmpty(nick) ? null : nick);
            SA(_loginPanel, false);
            Show(CompleteGameSystem.RunScreen.MainMenu);
            SyncMenuPlayerBar();
        });

        Spacer(vl, 60);
        Txt(vl, $"v{Application.version}", 9, new Color(0.3f, 0.3f, 0.35f), 16).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
    }

    void BuildMenu()
    {
        _menuPanel = Panel("Menu", new Color(0.06f, 0.04f, 0.10f, 1f));
        EnsureEventSystem();

        int heroIdx = UnityEngine.Random.Range(0, HeroEmojis.Length);
        _avatarEmoji = HeroEmojis[heroIdx];

        // === Background art layers ===
        BuildMenuBackground(_menuPanel.transform);

        // Scrollable content (created first so TopBar/NavBar render on top)
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(_menuPanel.transform, false);
        var scrollRT = scrollGo.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = new Vector2(1, 1);
        scrollRT.offsetMin = new Vector2(0, 95);
        scrollRT.offsetMax = new Vector2(0, -80);
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 30f;

        var vpGo = new GameObject("VP", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(scrollGo.transform, false);
        Stretch(vpGo);
        vpGo.GetComponent<Image>().color = new Color(1,1,1,0.003f);
        vpGo.GetComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vpGo.GetComponent<RectTransform>();

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRT = contentGo.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);
        var contentVL = contentGo.GetComponent<VerticalLayoutGroup>();
        contentVL.padding = new RectOffset(24, 24, 0, 30);
        contentVL.spacing = 0;
        contentVL.childAlignment = TextAnchor.UpperCenter;
        contentVL.childForceExpandWidth = true;
        contentVL.childForceExpandHeight = false;
        var csf = contentGo.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRT;

        // ── Section 1: Title area (compact, upper zone) ──
        Spacer(contentVL, 60);

        var titleTxt = Txt(contentVL, LocalizationData.T("你 也 是 我"), 22, new Color(Cyan.r, Cyan.g, Cyan.b, 0.6f), 28);
        titleTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;
        titleTxt.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        Spacer(contentVL, 4);
        var subTxt = Txt(contentVL, "YOU ARE ALSO ME", 11, new Color(0.45f, 0.42f, 0.55f, 0.7f), 18);
        subTxt.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        // ── Section 2: Central hero breathing room ──
        Spacer(contentVL, 120);

        // ── Section 3: Host status strip (one-line under hero) ──
        var hostStrip = new GameObject("HostStrip", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        hostStrip.transform.SetParent(contentGo.transform, false);
        hostStrip.AddComponent<LayoutElement>().preferredHeight = 22;
        var hsHL = hostStrip.GetComponent<HorizontalLayoutGroup>();
        hsHL.spacing = 8; hsHL.childAlignment = TextAnchor.MiddleCenter;
        hsHL.childForceExpandWidth = false; hsHL.childForceExpandHeight = false;
        hsHL.padding = new RectOffset(40, 40, 0, 0);

        var hostLbl = TxtGo(hostStrip.transform, "当前宿主", 10, Dim);
        hostLbl.gameObject.AddComponent<LayoutElement>().preferredWidth = 56;
        _menuHostLabel = TxtGo(hostStrip.transform, "未绑定", 12, Bright);
        _menuHostLabel.fontStyle = FontStyle.Bold;
        _menuHostLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 50;

        var pollDot = TxtGo(hostStrip.transform, "·", 12, Dim);
        pollDot.gameObject.AddComponent<LayoutElement>().preferredWidth = 8;

        var pollLbl = TxtGo(hostStrip.transform, "污染", 10, Dim);
        pollLbl.gameObject.AddComponent<LayoutElement>().preferredWidth = 28;
        _menuPollLabel = TxtGo(hostStrip.transform, "0%", 12, Cyan);
        _menuPollLabel.fontStyle = FontStyle.Bold;
        _menuPollLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 36;

        Spacer(contentVL, 18);

        // ── Section 4: Iteration summary card + action buttons ──
        _menuContinueSpacer = new GameObject("ContinueGroup", typeof(RectTransform), typeof(VerticalLayoutGroup));
        _menuContinueSpacer.transform.SetParent(contentGo.transform, false);
        var cgVL = _menuContinueSpacer.GetComponent<VerticalLayoutGroup>();
        cgVL.spacing = 0; cgVL.childForceExpandWidth = true; cgVL.childForceExpandHeight = false;
        cgVL.childAlignment = TextAnchor.MiddleCenter;
        cgVL.padding = new RectOffset(20, 20, 0, 0);
        _menuContinueSpacer.AddComponent<LayoutElement>().flexibleWidth = 1;

        BuildMenuLastRunCard(_menuContinueSpacer.transform);

        _menuContinueBtn = BuildMenuMainButton(_menuContinueSpacer.transform, LocalizationData.T("▶  继续当前寄生"), 17, Bright,
            new Color(0f, 0.18f, 0.15f, 0.7f), new Color(0f, 0.25f, 0.2f, 0.8f), true);
        _menuContinueBtn.GetComponent<Button>().onClick.AddListener(() => {
            if (SaveSystem.Instance != null && SaveSystem.Instance.GetSaveInfo(0) != null)
                GameManager.Instance?.LoadGame(0);
            else
                GoToModeSelect();
        });

        Spacer(contentVL, 10);

        var newBtn = BuildMenuMainButton(contentGo.transform, LocalizationData.T("开启新一轮下潜"), 14, Dim,
            new Color(0.06f, 0.05f, 0.12f, 0.35f), new Color(0.08f, 0.06f, 0.14f, 0.5f), false);
        newBtn.GetComponent<Button>().onClick.AddListener(() => GoToModeSelect());

        Spacer(contentVL, 20);

        // ── Section 5: Side function entries ──
        // Left = Growth / Narrative
        BuildSideButton(_menuPanel.transform, "记忆", "icon_archive", 0.06f, 0.50f, Purp, "记忆档案");
        BuildSideButton(_menuPanel.transform, "圣坛", "icon_altar", 0.06f, 0.39f, Purp, "残响圣坛");
        BuildSideButton(_menuPanel.transform, "签到", "icon_daily", 0.06f, 0.28f, Purp, "每日登录");

        // Right = Combat / Achievement
        BuildSideButton(_menuPanel.transform, "挑战", "", 0.94f, 0.50f, Cyan, "挑战模式");
        BuildSideButton(_menuPanel.transform, "记录", "icon_records", 0.94f, 0.39f, Cyan, "记录管理");
        BuildSideButton(_menuPanel.transform, "排行", "icon_ranking", 0.94f, 0.28f, Cyan, "深渊排行");

        // === Bottom navigation bar (fixed, above scroll) ===
        BuildMenuNavBar(_menuPanel.transform);

        // === Top identity bar (rendered last, on top of everything) ===
        BuildMenuTopBar(_menuPanel.transform);

        SyncMenuPlayerBar();
    }

    void BuildMenuBackground(Transform parent)
    {
        var heroTex = Resources.Load<Texture2D>("UI/menu_hero");
        if (heroTex != null)
        {
            var heroGo = new GameObject("BgHero", typeof(RectTransform), typeof(RawImage));
            heroGo.transform.SetParent(parent, false);
            heroGo.transform.SetAsFirstSibling();
            var heroRI = heroGo.GetComponent<RawImage>();
            heroRI.texture = heroTex;
            heroRI.color = new Color(1f, 1f, 1f, 0.9f);
            heroRI.raycastTarget = false;
            var hRT = heroGo.GetComponent<RectTransform>();
            hRT.anchorMin = Vector2.zero;
            hRT.anchorMax = Vector2.one;
            hRT.offsetMin = Vector2.zero; hRT.offsetMax = Vector2.zero;
        }

        var monsterTex = Resources.Load<Texture2D>("UI/menu_monsters");
        if (monsterTex != null)
        {
            var monGo = new GameObject("BgMonsters", typeof(RectTransform), typeof(RawImage));
            monGo.transform.SetParent(parent, false);
            monGo.transform.SetSiblingIndex(1);
            var monRI = monGo.GetComponent<RawImage>();
            monRI.texture = monsterTex;
            monRI.color = new Color(1f, 1f, 1f, 0.4f);
            monRI.raycastTarget = false;
            var mRT = monGo.GetComponent<RectTransform>();
            mRT.anchorMin = Vector2.zero;
            mRT.anchorMax = new Vector2(1, 0.4f);
            mRT.offsetMin = Vector2.zero; mRT.offsetMax = Vector2.zero;
        }

        var gradTop = new GameObject("GradTop", typeof(RectTransform), typeof(RawImage));
        gradTop.transform.SetParent(parent, false);
        gradTop.transform.SetSiblingIndex(2);
        var gtRI = gradTop.GetComponent<RawImage>();
        gtRI.texture = MakeGradientTex(64, new Color(0.06f, 0.04f, 0.10f, 0.85f), new Color(0, 0, 0, 0));
        gtRI.raycastTarget = false;
        var gtRT = gradTop.GetComponent<RectTransform>();
        gtRT.anchorMin = new Vector2(0, 0.9f); gtRT.anchorMax = Vector2.one;
        gtRT.offsetMin = Vector2.zero; gtRT.offsetMax = Vector2.zero;
    }

    static Texture2D MakeGradientTex(int h, Color top, Color bottom)
    {
        var tex = new Texture2D(1, h, TextureFormat.ARGB32, false);
        tex.wrapMode = TextureWrapMode.Clamp;
        for (int y = 0; y < h; y++)
        {
            float t = (float)y / (h - 1);
            tex.SetPixel(0, y, Color.Lerp(bottom, top, t));
        }
        tex.Apply();
        return tex;
    }

    void BuildMenuNavBar(Transform parent)
    {
        var navBar = new GameObject("NavBar", typeof(RectTransform), typeof(Image));
        navBar.transform.SetParent(parent, false);
        navBar.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.10f, 0.8f);
        var navBarRT = navBar.GetComponent<RectTransform>();
        navBarRT.anchorMin = Vector2.zero;
        navBarRT.anchorMax = new Vector2(1, 0);
        navBarRT.offsetMin = Vector2.zero;
        navBarRT.offsetMax = new Vector2(0, 90);

        var navSep = new GameObject("NavSep", typeof(RectTransform), typeof(Image));
        navSep.transform.SetParent(navBar.transform, false);
        var navSepRT = navSep.GetComponent<RectTransform>();
        navSepRT.anchorMin = new Vector2(0.02f, 1); navSepRT.anchorMax = new Vector2(0.98f, 1);
        navSepRT.offsetMin = new Vector2(0, -1); navSepRT.offsetMax = Vector2.zero;
        navSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);

        var navHL = new GameObject("NavHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        navHL.transform.SetParent(navBar.transform, false);
        Stretch(navHL);
        var nhl = navHL.GetComponent<HorizontalLayoutGroup>();
        nhl.spacing = 6; nhl.childAlignment = TextAnchor.MiddleCenter;
        nhl.childForceExpandWidth = true; nhl.childForceExpandHeight = true;
        nhl.padding = new RectOffset(6, 6, 4, 4);

        string[] navNames = { "成就", "图鉴", "下潜", "设置", "商店" };
        string[] navIconTex = { "icon_achievement", "icon_bestiary", null, "icon_settings", "icon_shop" };
        for (int ni = 0; ni < navNames.Length; ni++)
        {
            string navName = navNames[ni];
            bool isCenter = ni == 2;

            var navCell = new GameObject("Nav_" + navName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            navCell.transform.SetParent(navHL.transform, false);
            var navCellImg = navCell.GetComponent<Image>();
            navCellImg.color = isCenter ? new Color(0.04f, 0.14f, 0.12f, 0.85f) : new Color(0.07f, 0.05f, 0.12f, 0.5f);
            var navOL = navCell.GetComponent<Outline>();
            navOL.effectColor = isCenter ? new Color(0f, 1f, 0.816f, 0.5f) : new Color(0f, 1f, 0.816f, 0.12f);
            navOL.effectDistance = new Vector2(1, 1);

            var navInner = new GameObject("Inner", typeof(RectTransform), typeof(VerticalLayoutGroup));
            navInner.transform.SetParent(navCell.transform, false);
            Stretch(navInner);
            var nivl = navInner.GetComponent<VerticalLayoutGroup>();
            nivl.spacing = 2; nivl.padding = new RectOffset(2, 2, 6, 4);
            nivl.childAlignment = TextAnchor.MiddleCenter;
            nivl.childForceExpandWidth = true; nivl.childForceExpandHeight = false;

            float iconH = isCenter ? 42 : 38;
            var navTex = navIconTex[ni] != null ? Resources.Load<Texture2D>("UI/" + navIconTex[ni]) : null;
            if (navTex != null)
            {
                var icoWrap = new GameObject("IconWrap", typeof(RectTransform), typeof(LayoutElement));
                icoWrap.transform.SetParent(navInner.transform, false);
                var wrapLE = icoWrap.GetComponent<LayoutElement>();
                wrapLE.preferredHeight = iconH; wrapLE.preferredWidth = iconH;
                var icoGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
                icoGo.transform.SetParent(icoWrap.transform, false);
                icoGo.GetComponent<RawImage>().texture = navTex;
                icoGo.GetComponent<RawImage>().raycastTarget = false;
                Stretch(icoGo);
                var arf = icoGo.AddComponent<AspectRatioFitter>();
                arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
                arf.aspectRatio = (float)navTex.width / navTex.height;
            }
            else
            {
                var niIcon = TxtGo(navInner.transform, "⚔", isCenter ? 28 : 22, isCenter ? Cyan : Dim);
                niIcon.alignment = TextAnchor.MiddleCenter;
                niIcon.gameObject.AddComponent<LayoutElement>().preferredHeight = iconH;
            }

            var niName = TxtGo(navInner.transform, navName, isCenter ? 13 : 11, isCenter ? Cyan : Dim);
            niName.alignment = TextAnchor.MiddleCenter;
            niName.fontStyle = FontStyle.Bold;
            niName.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;

            navCell.GetComponent<Button>().targetGraphic = navCellImg;
            navCell.GetComponent<Button>().onClick.AddListener(() => OnNavButtonClick(navName));
        }
    }

    void BuildMenuTopBar(Transform parent)
    {
        var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        topBar.transform.SetParent(parent, false);
        var topBarImg = topBar.GetComponent<Image>();
        topBarImg.color = new Color(0.06f, 0.04f, 0.10f, 0.7f);
        topBarImg.raycastTarget = false;
        var topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.offsetMin = new Vector2(0, -75);
        topBarRT.offsetMax = Vector2.zero;

        var avatarBtn = new GameObject("AvatarBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        _avatarBtn = avatarBtn;
        avatarBtn.transform.SetParent(_root, false);
        var avRT = avatarBtn.GetComponent<RectTransform>();
        avRT.anchorMin = new Vector2(0, 1); avRT.anchorMax = new Vector2(0, 1);
        avRT.pivot = new Vector2(0, 1);
        avRT.anchoredPosition = new Vector2(10, -9);
        avRT.sizeDelta = new Vector2(52, 52);
        avatarBtn.GetComponent<Image>().color = new Color(0.06f, 0.12f, 0.14f, 0.95f);
        var avOL = avatarBtn.GetComponent<Outline>();
        avOL.effectColor = new Color(0f, 1f, 0.816f, 0.5f);
        avOL.effectDistance = new Vector2(2, 2);
        var avIcon = TxtAnchored(avRT, _avatarEmoji, 26, Bright, Vector2.zero, Vector2.one, 0, 0);
        avIcon.alignment = TextAnchor.MiddleCenter;
        avatarBtn.GetComponent<Button>().targetGraphic = avatarBtn.GetComponent<Image>();
        avatarBtn.GetComponent<Button>().onClick.AddListener(() => ShowProfilePanel());

        _menuPlayerName = TxtAnchored(topBarRT, "寄生体", 14, Bright, new Vector2(0, 0.52f), new Vector2(0.55f, 1), 70, 0);
        _menuPlayerName.alignment = TextAnchor.MiddleLeft;
        _menuPlayerName.fontStyle = FontStyle.Bold;

        _menuPlayerLevel = TxtAnchored(topBarRT, "", 9, Dim, new Vector2(0, 0.05f), new Vector2(0.55f, 0.50f), 70, 0);
        _menuPlayerLevel.alignment = TextAnchor.MiddleLeft;

        // EP resource
        var epLabel = TxtAnchored(topBarRT, "演化能级", 8, Dim, new Vector2(0.58f, 0.55f), new Vector2(0.78f, 0.9f), 0, 0);
        epLabel.alignment = TextAnchor.MiddleRight;
        _menuEpTxt = TxtAnchored(topBarRT, "0", 14, Cyan, new Vector2(0.78f, 0.52f), new Vector2(0.98f, 0.95f), 4, 0);
        _menuEpTxt.alignment = TextAnchor.MiddleLeft;
        _menuEpTxt.fontStyle = FontStyle.Bold;

        // KO resource
        var koLabel = TxtAnchored(topBarRT, "回收样本", 8, Dim, new Vector2(0.58f, 0.1f), new Vector2(0.78f, 0.5f), 0, 0);
        koLabel.alignment = TextAnchor.MiddleRight;
        _menuFragTxt = TxtAnchored(topBarRT, "0", 14, Gold, new Vector2(0.78f, 0.08f), new Vector2(0.98f, 0.5f), 4, 0);
        _menuFragTxt.alignment = TextAnchor.MiddleLeft;
        _menuFragTxt.fontStyle = FontStyle.Bold;

        var topSep = new GameObject("TopSep", typeof(RectTransform), typeof(Image));
        topSep.transform.SetParent(parent, false);
        var topSepRT = topSep.GetComponent<RectTransform>();
        topSepRT.anchorMin = new Vector2(0.02f, 1);
        topSepRT.anchorMax = new Vector2(0.98f, 1);
        topSepRT.offsetMin = new Vector2(0, -77);
        topSepRT.offsetMax = new Vector2(0, -75);
        var topSepImg = topSep.GetComponent<Image>();
        topSepImg.color = new Color(0f, 1f, 0.816f, 0.12f);
        topSepImg.raycastTarget = false;
    }

    void BuildProfilePanel()
    {
        _profileOvl = Panel("ProfileOvl", new Color(0.06f, 0.04f, 0.10f, 0.92f));
        _profileOvl.SetActive(false);
        var rt = _profileOvl.GetComponent<RectTransform>();

        var container = new GameObject("Container", typeof(RectTransform), typeof(Image), typeof(Outline));
        container.transform.SetParent(rt, false);
        container.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.14f);
        container.GetComponent<Outline>().effectColor = new Color(0f, 1f, 0.816f, 0.5f);
        container.GetComponent<Outline>().effectDistance = new Vector2(2, 2);
        var cRT = container.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.08f, 0.25f);
        cRT.anchorMax = new Vector2(0.92f, 0.75f);
        cRT.offsetMin = Vector2.zero; cRT.offsetMax = Vector2.zero;

        var vl = AddVL(container, 20, 10);
        Spacer(vl, 15);

        var headerTxt = Txt(vl, "个人信息", 18, Cyan, 30).GetComponent<Text>();
        headerTxt.alignment = TextAnchor.MiddleCenter;
        headerTxt.fontStyle = FontStyle.Bold;

        Spacer(vl, 8);
        var sep1 = new GameObject("Sep", typeof(RectTransform), typeof(Image));
        sep1.transform.SetParent(vl.transform, false);
        sep1.AddComponent<LayoutElement>().preferredHeight = 1;
        sep1.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);

        Spacer(vl, 8);

        var avatarRow = new GameObject("AvatarRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        avatarRow.transform.SetParent(vl.transform, false);
        avatarRow.AddComponent<LayoutElement>().preferredHeight = 60;
        var arHL = avatarRow.GetComponent<HorizontalLayoutGroup>();
        arHL.spacing = 12; arHL.childAlignment = TextAnchor.MiddleCenter;
        arHL.childForceExpandWidth = false; arHL.childForceExpandHeight = false;

        var avBig = new GameObject("AvatarBig", typeof(RectTransform), typeof(Image), typeof(Outline));
        avBig.transform.SetParent(avatarRow.transform, false);
        var avLE = avBig.AddComponent<LayoutElement>();
        avLE.preferredWidth = 55; avLE.preferredHeight = 55;
        avBig.GetComponent<Image>().color = new Color(0.12f, 0.1f, 0.2f);
        avBig.GetComponent<Outline>().effectColor = new Color(0f, 1f, 0.816f, 0.4f);
        var avBigIcon = TxtGo(avBig.transform, _avatarEmoji ?? "泰", 28, Cyan);
        avBigIcon.alignment = TextAnchor.MiddleCenter;
        Stretch(avBigIcon.gameObject);

        var nickCol = new GameObject("NickCol", typeof(RectTransform), typeof(VerticalLayoutGroup));
        nickCol.transform.SetParent(avatarRow.transform, false);
        var ncLE = nickCol.AddComponent<LayoutElement>();
        ncLE.flexibleWidth = 1; ncLE.preferredHeight = 55;
        var ncVL = nickCol.GetComponent<VerticalLayoutGroup>();
        ncVL.spacing = 2; ncVL.childAlignment = TextAnchor.MiddleLeft;
        ncVL.childForceExpandWidth = true; ncVL.childForceExpandHeight = false;

        _profNick = Txt(ncVL, "", 16, Bright, 24).GetComponent<Text>();
        _profNick.fontStyle = FontStyle.Bold;
        _profNick.alignment = TextAnchor.MiddleLeft;

        _profId = Txt(ncVL, "", 11, Dim, 18).GetComponent<Text>();
        _profId.alignment = TextAnchor.MiddleLeft;

        Spacer(vl, 10);

        _profGames = Txt(vl, "", 13, Bright, 22).GetComponent<Text>();
        _profFloor = Txt(vl, "", 13, Bright, 22).GetComponent<Text>();
        _profKills = Txt(vl, "", 13, Bright, 22).GetComponent<Text>();
        _profPossess = Txt(vl, "", 13, Bright, 22).GetComponent<Text>();

        Spacer(vl, 15);

        var logoutBtn = BtnGo(vl.transform, "注 销", 16, new Color(1f, 0.3f, 0.3f), 50);
        logoutBtn.GetComponent<Image>().color = new Color(0.2f, 0.05f, 0.05f, 0.9f);
        logoutBtn.AddComponent<Outline>().effectColor = new Color(1f, 0.3f, 0.3f, 0.5f);
        logoutBtn.GetComponent<Button>().onClick.AddListener(() => {
            GuestAuthManager.Instance?.Logout();
            AchievementManager.Instance?.ResetAll();
            MetaProgressSystem.Instance?.ResetAll();
            _profileOvl.SetActive(false);
            SA(_menuPanel, false);
            SA(_avatarBtn, false);
            _loginPanel.transform.SetAsLastSibling();
            SA(_loginPanel, true);
        });

        Spacer(vl, 8);

        var closeBtn = BtnGo(vl.transform, "关闭", 14, Cyan, 44);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => _profileOvl.SetActive(false));

        Spacer(vl, 10);
    }

    void BuildMenuLastRunCard(Transform parent)
    {
        var card = new GameObject("LastRunCard", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        var cardLE = card.AddComponent<LayoutElement>();
        cardLE.preferredHeight = 110;
        var cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.06f, 0.05f, 0.12f, 0.6f);

        var outline = card.AddComponent<Outline>();
        outline.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.15f);
        outline.effectDistance = new Vector2(1, 1);

        var innerVL = new GameObject("Inner", typeof(RectTransform), typeof(VerticalLayoutGroup));
        innerVL.transform.SetParent(card.transform, false);
        Stretch(innerVL);
        var ivl = innerVL.GetComponent<VerticalLayoutGroup>();
        ivl.padding = new RectOffset(16, 16, 10, 10);
        ivl.spacing = 6;
        ivl.childAlignment = TextAnchor.UpperLeft;
        ivl.childForceExpandWidth = true;
        ivl.childForceExpandHeight = false;

        // Status line (one-line summary)
        _menuRunStatus = TxtGo(innerVL.transform, "进行中", 10, Dim);
        _menuRunStatus.gameObject.AddComponent<LayoutElement>().preferredHeight = 16;

        // Core data row: Mode · Floor · Host
        var dataRow = new GameObject("DataRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        dataRow.transform.SetParent(innerVL.transform, false);
        dataRow.AddComponent<LayoutElement>().preferredHeight = 28;
        var drHL = dataRow.GetComponent<HorizontalLayoutGroup>();
        drHL.spacing = 6; drHL.childAlignment = TextAnchor.MiddleLeft;
        drHL.childForceExpandWidth = false; drHL.childForceExpandHeight = false;

        _menuLastRunMode = TxtGo(dataRow.transform, "Classic", 14, Bright);
        _menuLastRunMode.fontStyle = FontStyle.Bold;
        _menuLastRunMode.gameObject.AddComponent<LayoutElement>().preferredWidth = 60;

        var dot1 = TxtGo(dataRow.transform, "·", 14, Dim);
        dot1.gameObject.AddComponent<LayoutElement>().preferredWidth = 10;

        _menuLastRunFloor = TxtGo(dataRow.transform, "F1", 14, Cyan);
        _menuLastRunFloor.fontStyle = FontStyle.Bold;
        _menuLastRunFloor.gameObject.AddComponent<LayoutElement>().preferredWidth = 30;

        var dot2 = TxtGo(dataRow.transform, "·", 14, Dim);
        dot2.gameObject.AddComponent<LayoutElement>().preferredWidth = 10;

        _menuLastRunHost = TxtGo(dataRow.transform, "泰坦", 14, Bright);
        _menuLastRunHost.fontStyle = FontStyle.Bold;
        _menuLastRunHost.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        // Pollution bar
        var pollRow = new GameObject("PollRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        pollRow.transform.SetParent(innerVL.transform, false);
        pollRow.AddComponent<LayoutElement>().preferredHeight = 18;
        var prHL = pollRow.GetComponent<HorizontalLayoutGroup>();
        prHL.spacing = 8; prHL.childAlignment = TextAnchor.MiddleLeft;
        prHL.childForceExpandWidth = false; prHL.childForceExpandHeight = false;

        var pollLbl = TxtGo(pollRow.transform, "污染", 9, Dim);
        pollLbl.gameObject.AddComponent<LayoutElement>().preferredWidth = 28;

        _menuLastRunPoll = TxtGo(pollRow.transform, "0%", 10, Cyan);
        _menuLastRunPoll.fontStyle = FontStyle.Bold;
        _menuLastRunPoll.gameObject.AddComponent<LayoutElement>().preferredWidth = 30;

        // Poll bar background
        var barBg = new GameObject("BarBg", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        barBg.transform.SetParent(pollRow.transform, false);
        barBg.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.16f, 0.8f);
        barBg.GetComponent<LayoutElement>().flexibleWidth = 1;
        barBg.GetComponent<LayoutElement>().preferredHeight = 6;

        var barFill = new GameObject("BarFill", typeof(RectTransform), typeof(Image));
        barFill.transform.SetParent(barBg.transform, false);
        _menuPollBar = barFill.GetComponent<Image>();
        _menuPollBar.color = Cyan;
        var bfRT = barFill.GetComponent<RectTransform>();
        bfRT.anchorMin = Vector2.zero;
        bfRT.anchorMax = new Vector2(0.09f, 1);
        bfRT.offsetMin = Vector2.zero; bfRT.offsetMax = Vector2.zero;

        var btn = card.AddComponent<Button>();
        btn.targetGraphic = cardImg;
        btn.onClick.AddListener(() => {
            if (SaveSystem.Instance != null && SaveSystem.Instance.GetSaveInfo(0) != null)
                GameManager.Instance?.LoadGame(0);
        });
    }

    GameObject BuildMenuMainButton(Transform parent, string label, int fontSize, Color textColor,
        Color bgColor, Color hoverColor, bool filled)
    {
        var btnGo = new GameObject("MainBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        btnGo.transform.SetParent(parent, false);
        btnGo.AddComponent<LayoutElement>().preferredHeight = filled ? 56 : 50;

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = bgColor;

        if (!filled)
        {
            var ol = btnGo.AddComponent<Outline>();
            ol.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.35f);
            ol.effectDistance = new Vector2(1, 1);
        }
        else
        {
            var ol = btnGo.AddComponent<Outline>();
            ol.effectColor = new Color(0.4f, 0.35f, 0.55f, 0.3f);
            ol.effectDistance = new Vector2(1, 1);
        }

        var shadow = btnGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.4f);
        shadow.effectDistance = new Vector2(0, -2);

        var txtGo = new GameObject("T", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(btnGo.transform, false);
        Stretch(txtGo);
        var txt = txtGo.GetComponent<Text>();
        txt.font = F();
        txt.fontSize = fontSize;
        txt.color = textColor;
        txt.fontStyle = FontStyle.Bold;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = label;
        txt.raycastTarget = false;

        var btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = bgColor * 0.7f;
        btn.colors = colors;

        return btnGo;
    }

    void BuildMenuNavRow(Transform parent, string[] names, string[] icons, Color[] colors)
    {
        var row = new GameObject("NavRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(parent, false);
        row.AddComponent<LayoutElement>().preferredHeight = 72;
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 8;
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.childForceExpandWidth = true;
        hl.childForceExpandHeight = true;

        for (int i = 0; i < names.Length; i++)
        {
            string name = names[i];
            bool isShop = name == "回响商店";
            Color col = colors[i];

            var cell = new GameObject("Nav_" + name, typeof(RectTransform), typeof(Image), typeof(Button));
            cell.transform.SetParent(row.transform, false);

            var cellImg = cell.GetComponent<Image>();
            cellImg.color = isShop ? new Color(0.10f, 0.07f, 0.16f) : new Color(0.08f, 0.06f, 0.12f);

            if (isShop)
            {
                var ol = cell.AddComponent<Outline>();
                ol.effectColor = new Color(Gold.r, Gold.g, Gold.b, 0.4f);
                ol.effectDistance = new Vector2(1, 1);
            }

            var inner = new GameObject("Inner", typeof(RectTransform), typeof(VerticalLayoutGroup));
            inner.transform.SetParent(cell.transform, false);
            Stretch(inner);
            var ivl = inner.GetComponent<VerticalLayoutGroup>();
            ivl.spacing = 4;
            ivl.padding = new RectOffset(4, 4, 10, 6);
            ivl.childAlignment = TextAnchor.MiddleCenter;
            ivl.childForceExpandWidth = true;
            ivl.childForceExpandHeight = false;

            var iconTxt = TxtGo(inner.transform, icons[i], 20, col);
            iconTxt.alignment = TextAnchor.MiddleCenter;
            iconTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;

            var nameTxt = TxtGo(inner.transform, LocalizationData.T(name), 10, isShop ? Gold : Bright);
            nameTxt.alignment = TextAnchor.MiddleCenter;
            nameTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 16;

            var btn = cell.GetComponent<Button>();
            btn.targetGraphic = cellImg;
            btn.onClick.AddListener(() => OnNavButtonClick(name));
        }
    }

    void BuildMenuFooter(Transform parent)
    {
        // Footer row 1: links (full width, evenly spaced)
        var row1 = new GameObject("FooterRow1", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row1.transform.SetParent(parent, false);
        row1.AddComponent<LayoutElement>().preferredHeight = 30;
        var hl1 = row1.GetComponent<HorizontalLayoutGroup>();
        hl1.spacing = 0;
        hl1.childAlignment = TextAnchor.MiddleCenter;
        hl1.childForceExpandWidth = true;
        hl1.childForceExpandHeight = true;

        var altarBtn = FooterLink(row1.transform, "残响圣坛", Dim, true);
        altarBtn.onClick.AddListener(() => ShowShopPanel());

        var dailyBtn = FooterLink(row1.transform, "每日登录", Dim, true);
        dailyBtn.onClick.AddListener(() => ShowAchievementPanel());

        var recordBtn = FooterLink(row1.transform, "记录管理", Dim, true);
        recordBtn.onClick.AddListener(() => ShowRecordPanel());

        Spacer(parent, 4);

        // Footer row 2: version
        var row2 = new GameObject("FooterRow2", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row2.transform.SetParent(parent, false);
        row2.AddComponent<LayoutElement>().preferredHeight = 18;
        var hl2 = row2.GetComponent<HorizontalLayoutGroup>();
        hl2.spacing = 8;
        hl2.childAlignment = TextAnchor.MiddleCenter;
        hl2.childForceExpandWidth = false;

        TxtGo(row2.transform, $"v{Application.version}", 9, Dim);
        TxtGo(row2.transform, "·", 9, Dim);

        int runNum = MetaProgressSystem.Instance?.metaData.currentRun ?? 1;
        TxtGo(row2.transform, $"第 {runNum} 次迭代", 9, Dim);
    }

    Button FooterLink(Transform parent, string label, Color color, bool expand = false)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Button));
        go.transform.SetParent(parent, false);
        var txt = TxtGo(go.transform, LocalizationData.T(label), 12, color);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        if (expand)
            go.AddComponent<LayoutElement>().flexibleWidth = 1;
        else
            go.AddComponent<LayoutElement>().preferredWidth = 60;
        var btn = go.GetComponent<Button>();
        return btn;
    }

    void EnsureEventSystem()
    {
        var eventSystem = FindObjectOfType<UnityEngine.EventSystems.EventSystem>();
        if (eventSystem == null)
        {
            var eventSystemObj = new GameObject("EventSystem");
            eventSystemObj.AddComponent<UnityEngine.EventSystems.EventSystem>();
            eventSystemObj.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }
    }

    void BuildSideButton(Transform parent, string label, string texName, float anchorX, float anchorY, Color col, string navTarget)
    {
        var tex = !string.IsNullOrEmpty(texName) ? Resources.Load<Texture2D>("UI/" + texName) : null;
        float btnSize = 48;

        var btn = new GameObject("Side_" + navTarget, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btn.transform.SetParent(parent, false);
        var btnRT = btn.GetComponent<RectTransform>();
        btnRT.anchorMin = new Vector2(anchorX, anchorY);
        btnRT.anchorMax = new Vector2(anchorX, anchorY);
        btnRT.pivot = new Vector2(0.5f, 0.5f);
        btnRT.sizeDelta = new Vector2(btnSize, btnSize);
        btn.GetComponent<Image>().color = new Color(0.07f, 0.05f, 0.12f, 0.55f);
        btn.GetComponent<Outline>().effectColor = new Color(col.r, col.g, col.b, 0.15f);

        if (tex != null)
        {
            var icoGo = new GameObject("Icon", typeof(RectTransform), typeof(RawImage));
            icoGo.transform.SetParent(btn.transform, false);
            icoGo.GetComponent<RawImage>().texture = tex;
            icoGo.GetComponent<RawImage>().raycastTarget = false;
            Stretch(icoGo);
            var arf = icoGo.AddComponent<AspectRatioFitter>();
            arf.aspectMode = AspectRatioFitter.AspectMode.FitInParent;
            arf.aspectRatio = (float)tex.width / tex.height;
        }
        else
        {
            var icoTxt = TxtGo(btn.transform, label.Length > 0 ? label.Substring(0, 1) : "?", 20, col);
            icoTxt.alignment = TextAnchor.MiddleCenter;
            Stretch(icoTxt.gameObject);
        }

        var lblTxt = TxtGo(btn.transform, label, 11, Bright);
        lblTxt.alignment = TextAnchor.UpperCenter;
        lblTxt.raycastTarget = false;
        lblTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var lblRT = lblTxt.GetComponent<RectTransform>();
        lblRT.anchorMin = new Vector2(-0.6f, -0.42f);
        lblRT.anchorMax = new Vector2(1.6f, -0.04f);
        lblRT.offsetMin = Vector2.zero;
        lblRT.offsetMax = Vector2.zero;

        btn.GetComponent<Button>().targetGraphic = btn.GetComponent<Image>();
        btn.GetComponent<Button>().onClick.AddListener(() => OnNavButtonClick(navTarget));
    }

    void SyncMenuPlayerBar()
    {
        var auth = GuestAuthManager.Instance;
        if (auth == null || !auth.IsLoggedIn) return;

        ST(_menuPlayerName, auth.Profile.nickname);
        int runs = MetaProgressSystem.Instance?.metaData.totalGamesPlayed ?? 0;
        string phase = runs == 0 ? "初始感染" : runs < 5 ? "稳定侵蚀" : runs < 15 ? "深度同化" : "完全融合";
        ST(_menuPlayerLevel, $"第{runs + 1}次迭代 · {phase}");

        var meta = MetaProgressSystem.Instance?.metaData;
        if (meta != null)
        {
            var p = GameManager.Instance?.Player;
            int ep = p != null ? p.evolutionPoints : meta.Value.totalEvoPointsEarned;
            ST(_menuEpTxt, ep.ToString());
            ST(_menuFragTxt, meta.Value.totalKills.ToString());
        }
    }

    void ShowProfilePanel()
    {
        if (_profileOvl == null) return;
        var auth = GuestAuthManager.Instance;
        var nick = auth != null && auth.IsLoggedIn ? auth.Profile.nickname : "寄生体";
        var gid = auth != null && auth.IsLoggedIn ? auth.Profile.guestId : "---";

        ST(_profNick, nick);
        ST(_profId, $"游客ID: {gid}");

        var meta = MetaProgressSystem.Instance?.metaData;
        ST(_profGames, $"总游戏次数: {meta?.totalGamesPlayed ?? 0}");
        ST(_profFloor, $"最高楼层: {meta?.maxFloorReached ?? 0}");
        ST(_profKills, $"总击杀数: {meta?.totalKills ?? 0}");
        ST(_profPossess, $"总附身数: {meta?.possessions ?? 0}");

        _profileOvl.transform.SetAsLastSibling();
        _profileOvl.SetActive(true);
    }

    void OnNavButtonClick(string name)
    {
        switch (name)
        {
            case "成就回响": case "成就": ShowAchievementPanel(); break;
            case "异种图鉴": case "图鉴": ShowBestiaryPanel(); break;
            case "记忆档案": ShowFragmentPanel(); break;
            case "深渊排行": ShowLeaderboardPanel(); break;
            case "终端设置": case "设置": ShowSettingsPanel(); break;
            case "回响商店": case "商店": ShowShopPanel(); break;
            case "挑战模式": ShowChallengePanel(); break;
            case "下潜": GoToModeSelect(); break;
            case "记录管理": ShowRecordPanel(); break;
            case "残响圣坛": ShowEchoAltarPanel(); break;
            case "每日登录": ShowDailyRewardPanel(); break;
        }
    }

    // ========== MODE SELECT (下降协议) ==========
    string _selectedModeId = "Short";
    Text _modeDescTitle, _modeDescFloors, _modeDescFlavor, _modeDescFit, _modeDescFeature1, _modeDescFeature2;
    Text _modeStartBtnTxt;
    Image[] _modeTabBgs = new Image[3];
    Text[] _modeTabLabels = new Text[3];
    Text[] _modeTabIcons = new Text[3];
    Outline[] _modeTabOutlines = new Outline[3];
    Outline _modeDescOutline;
    Image _modeDescBg;
    Image _modeStartBtnBg;
    Outline _modeStartBtnOutline;

    struct ModeInfo
    {
        public string id, icon, label, descTitle, floors, flavor, fit, feat1, feat2;
        public Color themeColor;
        public Color bgColor;
        public string startBtnText;
    }
    static readonly ModeInfo[] ModeInfos = {
        new ModeInfo { id="Short", icon="▶", label="一次迭代", descTitle="▶ 一次迭代",
            floors="12层坠落 · 约15分钟", flavor="快速燃烧，不留痕迹。",
            fit="想快速开一局，测试构筑与附身路线的玩家。",
            feat1="★ 开放排行榜", feat2="◇ 支持每日挑战词条",
            themeColor = new Color(0.9f, 0.6f, 0.2f), bgColor = new Color(0.12f, 0.08f, 0.04f),
            startBtnText = "快速迭代" },
        new ModeInfo { id="Expedition", icon="◆", label="远征", descTitle="◆ 远征",
            floors="20层深入 · 约30分钟", flavor="穿越裂隙，征服未知。",
            fit="喜欢中等长度、需要策略规划的玩家。",
            feat1="★ 精英怪增加", feat2="◇ 支持锚点系统",
            themeColor = new Color(0.0f, 0.94f, 0.94f), bgColor = new Color(0.04f, 0.1f, 0.12f),
            startBtnText = "开启远征" },
        new ModeInfo { id="Classic", icon="●", label="完整递归", descTitle="● 完整递归",
            floors="50层全程 · 约60分钟", flavor="完整体验一次完整的寄生之旅。",
            fit="追求完整体验、挑战极限的玩家。",
            feat1="★ Boss层完整", feat2="◇ 全进化路径解锁",
            themeColor = new Color(0.65f, 0.33f, 0.94f), bgColor = new Color(0.08f, 0.04f, 0.14f),
            startBtnText = "开始轮回" },
    };

    void BuildClass()
    {
        _classPanel = Panel("Class", new Color(0.06f, 0.04f, 0.10f));

        // Background art (same as menu for unified style)
        var heroTex = Resources.Load<Texture2D>("UI/menu_hero");
        if (heroTex != null)
        {
            var bgGo = new GameObject("BgHero", typeof(RectTransform), typeof(RawImage));
            bgGo.transform.SetParent(_classPanel.transform, false);
            var bgRI = bgGo.GetComponent<RawImage>();
            bgRI.texture = heroTex;
            bgRI.color = new Color(1f, 1f, 1f, 0.45f);
            bgRI.raycastTarget = false;
            Stretch(bgGo);
        }

        // Scrollable content
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(_classPanel.transform, false);
        Stretch(scrollGo);
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 30f;

        var vpGo = new GameObject("VP", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(scrollGo.transform, false);
        Stretch(vpGo);
        vpGo.GetComponent<Image>().color = new Color(1,1,1,0.003f);
        vpGo.GetComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vpGo.GetComponent<RectTransform>();

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRT = contentGo.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = new Vector2(0, 0);
        var contentVL = contentGo.GetComponent<VerticalLayoutGroup>();
        contentVL.padding = new RectOffset(16, 16, 0, 40);
        contentVL.spacing = 0;
        contentVL.childAlignment = TextAnchor.UpperCenter;
        contentVL.childForceExpandWidth = true;
        contentVL.childForceExpandHeight = false;
        var csf = contentGo.GetComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRT;

        var vl = contentVL;

        Spacer(vl, 24);

        // Back button
        var backRow = new GameObject("BackRow", typeof(RectTransform));
        backRow.transform.SetParent(contentGo.transform, false);
        backRow.AddComponent<LayoutElement>().preferredHeight = 36;
        var backBtn = new GameObject("BackBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        backBtn.transform.SetParent(backRow.transform, false);
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 0.5f);
        backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.pivot = new Vector2(0, 0.5f);
        backRT.anchoredPosition = new Vector2(0, 0);
        backRT.sizeDelta = new Vector2(130, 36);
        backBtn.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.12f, 0.5f);
        backBtn.AddComponent<Outline>().effectColor = new Color(0.3f, 0.25f, 0.4f, 0.3f);
        backBtn.GetComponent<Button>().targetGraphic = backBtn.GetComponent<Image>();
        var backTxtGo = new GameObject("T", typeof(RectTransform), typeof(Text));
        backTxtGo.transform.SetParent(backBtn.transform, false);
        Stretch(backTxtGo);
        var backTxt = backTxtGo.GetComponent<Text>();
        backTxt.font = F(); backTxt.fontSize = 13; backTxt.color = new Color(0.7f, 0.65f, 0.8f);
        backTxt.text = "← 返回主页"; backTxt.alignment = TextAnchor.MiddleCenter;
        backTxt.raycastTarget = false;
        backBtn.GetComponent<Button>().onClick.AddListener(() => {
            if (CompleteGameSystem.Instance != null)
                CompleteGameSystem.Instance.ReturnToMenu();
            else
            {
                SA(_classPanel, false);
                SA(_menuPanel, true);
            }
        });

        Spacer(vl, 30);

        // Title area
        var titleTxt = Txt(vl, "下 降 协 议", 34, Bright, 48);
        titleTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;
        Spacer(vl, 4);
        Txt(vl, "D E S C E N T   P R O T O C O L", 10, new Color(0.5f, 0.45f, 0.6f), 16);
        Spacer(vl, 10);

        // Decorative separator
        var sepRow = new GameObject("Sep", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        sepRow.transform.SetParent(contentGo.transform, false);
        sepRow.AddComponent<LayoutElement>().preferredHeight = 16;
        var sepHL = sepRow.GetComponent<HorizontalLayoutGroup>();
        sepHL.spacing = 12; sepHL.childAlignment = TextAnchor.MiddleCenter; sepHL.childForceExpandWidth = false;
        TxtGo(sepRow.transform, "━━━━━━", 9, new Color(0.3f, 0.25f, 0.4f)).alignment = TextAnchor.MiddleCenter;
        TxtGo(sepRow.transform, "◈", 12, new Color(0.5f, 0.4f, 0.65f)).alignment = TextAnchor.MiddleCenter;
        TxtGo(sepRow.transform, "━━━━━━", 9, new Color(0.3f, 0.25f, 0.4f)).alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 14);
        Txt(vl, "选择你的下降方式", 16, new Color(0.7f, 0.65f, 0.8f), 26);

        Spacer(vl, 24);

        // Mode tabs row
        var modeRow = new GameObject("ModeRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        modeRow.transform.SetParent(contentGo.transform, false);
        modeRow.AddComponent<LayoutElement>().preferredHeight = 72;
        var modeHL = modeRow.GetComponent<HorizontalLayoutGroup>();
        modeHL.spacing = 10;
        modeHL.childAlignment = TextAnchor.MiddleCenter;
        modeHL.childForceExpandWidth = true;
        modeHL.childForceExpandHeight = true;

        for (int i = 0; i < 3; i++)
        {
            int idx = i;
            var info = ModeInfos[i];
            bool active = info.id == _selectedModeId;

            var tab = new GameObject("Tab_" + info.id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            tab.transform.SetParent(modeRow.transform, false);

            var tabImg = tab.GetComponent<Image>();
            var activeBg = new Color(info.bgColor.r * 1.8f, info.bgColor.g * 1.8f, info.bgColor.b * 1.8f);
            tabImg.color = active ? new Color(activeBg.r, activeBg.g, activeBg.b, 0.6f) : new Color(0.08f, 0.06f, 0.12f, 0.4f);
            _modeTabBgs[i] = tabImg;

            var tabOL = tab.GetComponent<Outline>();
            tabOL.effectColor = active
                ? new Color(info.themeColor.r, info.themeColor.g, info.themeColor.b, 0.7f)
                : new Color(0.25f, 0.22f, 0.35f, 0.4f);
            tabOL.effectDistance = new Vector2(1, 1);
            _modeTabOutlines[i] = tabOL;

            var inner = new GameObject("Inner", typeof(RectTransform), typeof(VerticalLayoutGroup));
            inner.transform.SetParent(tab.transform, false);
            Stretch(inner);
            var ihl = inner.GetComponent<VerticalLayoutGroup>();
            ihl.spacing = 4;
            ihl.childAlignment = TextAnchor.MiddleCenter;
            ihl.childForceExpandWidth = true;
            ihl.childForceExpandHeight = false;
            ihl.padding = new RectOffset(4, 4, 8, 6);

            var iconT = TxtGo(inner.transform, info.icon, 22, active ? info.themeColor : new Color(0.5f, 0.45f, 0.6f));
            iconT.alignment = TextAnchor.MiddleCenter;
            iconT.fontStyle = FontStyle.Bold;
            iconT.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;
            _modeTabIcons[i] = iconT;

            var labelT = TxtGo(inner.transform, info.label, 15, active ? Bright : new Color(0.6f, 0.55f, 0.7f));
            labelT.alignment = TextAnchor.MiddleCenter;
            labelT.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            labelT.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
            _modeTabLabels[i] = labelT;

            tab.GetComponent<Button>().targetGraphic = tabImg;
            tab.GetComponent<Button>().onClick.AddListener(() => SelectModeTab(idx));
        }

        Spacer(vl, 22);

        // Description card
        var descCard = new GameObject("DescCard", typeof(RectTransform), typeof(Image), typeof(Outline));
        descCard.transform.SetParent(contentGo.transform, false);
        descCard.AddComponent<LayoutElement>().preferredHeight = 260;
        _modeDescBg = descCard.GetComponent<Image>();
        _modeDescBg.color = new Color(0.06f, 0.05f, 0.1f, 0.45f);
        _modeDescOutline = descCard.GetComponent<Outline>();
        _modeDescOutline.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.35f);
        _modeDescOutline.effectDistance = new Vector2(1.5f, 1.5f);

        var descInner = new GameObject("DescInner", typeof(RectTransform), typeof(VerticalLayoutGroup));
        descInner.transform.SetParent(descCard.transform, false);
        var diRT = descInner.GetComponent<RectTransform>();
        diRT.anchorMin = new Vector2(0.05f, 0.04f);
        diRT.anchorMax = new Vector2(0.95f, 0.96f);
        diRT.offsetMin = Vector2.zero;
        diRT.offsetMax = Vector2.zero;
        var divl = descInner.GetComponent<VerticalLayoutGroup>();
        divl.spacing = 4;
        divl.childAlignment = TextAnchor.UpperLeft;
        divl.childForceExpandWidth = true;
        divl.childForceExpandHeight = false;

        _modeDescTitle = TxtGo(descInner.transform, "", 13, Cyan);
        _modeDescTitle.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;

        Spacer(divl, 6);

        _modeDescFloors = TxtGo(descInner.transform, "", 20, Bright);
        _modeDescFloors.fontStyle = FontStyle.Bold;
        _modeDescFloors.gameObject.AddComponent<LayoutElement>().preferredHeight = 30;

        Spacer(divl, 4);

        _modeDescFlavor = TxtGo(descInner.transform, "", 14, new Color(0.65f, 0.6f, 0.75f));
        _modeDescFlavor.fontStyle = FontStyle.Italic;
        _modeDescFlavor.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

        Spacer(divl, 12);

        // Thin separator inside card
        var cardSep = new GameObject("CardSep", typeof(RectTransform), typeof(Image));
        cardSep.transform.SetParent(descInner.transform, false);
        cardSep.AddComponent<LayoutElement>().preferredHeight = 1;
        cardSep.GetComponent<Image>().color = new Color(0.3f, 0.25f, 0.4f, 0.4f);

        Spacer(divl, 10);

        var fitLabel = TxtGo(descInner.transform, "适合玩家:", 11, new Color(0.6f, 0.55f, 0.7f));
        fitLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
        _modeDescFit = TxtGo(descInner.transform, "", 14, Bright);
        _modeDescFit.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

        Spacer(divl, 8);

        var featLabel = TxtGo(descInner.transform, "结算特性:", 11, new Color(0.6f, 0.55f, 0.7f));
        featLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
        _modeDescFeature1 = TxtGo(descInner.transform, "", 14, Bright);
        _modeDescFeature1.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
        _modeDescFeature2 = TxtGo(descInner.transform, "", 14, Bright);
        _modeDescFeature2.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;

        Spacer(vl, 30);

        // Big start button
        var startBtn = BuildMenuMainButton(contentGo.transform, "开始本次迭代", 22, Cyan,
            new Color(0.04f, 0.08f, 0.06f, 0.45f), new Color(0.06f, 0.1f, 0.08f, 0.6f), false);
        startBtn.GetComponent<LayoutElement>().preferredHeight = 68;
        _modeStartBtnTxt = startBtn.GetComponentInChildren<Text>();
        _modeStartBtnBg = startBtn.GetComponent<Image>();
        _modeStartBtnOutline = startBtn.GetComponent<Outline>();
        _modeStartBtnOutline.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.5f);
        _modeStartBtnOutline.effectDistance = new Vector2(1.5f, 1.5f);
        startBtn.GetComponent<Button>().onClick.AddListener(() => StartSelectedMode());

        Spacer(vl, 35);

        // Init description
        SelectModeTab(0);
    }

    void SelectModeTab(int idx)
    {
        var info = ModeInfos[idx];
        _selectedModeId = info.id;

        for (int i = 0; i < 3; i++)
        {
            bool active = i == idx;
            var modeInfo = ModeInfos[i];
            if (_modeTabBgs[i] != null)
            {
                var activeBg = new Color(modeInfo.bgColor.r * 1.8f, modeInfo.bgColor.g * 1.8f, modeInfo.bgColor.b * 1.8f);
                _modeTabBgs[i].color = active ? new Color(activeBg.r, activeBg.g, activeBg.b, 0.6f) : new Color(0.08f, 0.06f, 0.12f, 0.4f);
            }
            if (_modeTabOutlines[i] != null)
                _modeTabOutlines[i].effectColor = active
                    ? new Color(modeInfo.themeColor.r, modeInfo.themeColor.g, modeInfo.themeColor.b, 0.7f)
                    : new Color(0.25f, 0.22f, 0.35f, 0.4f);
            if (_modeTabIcons[i] != null)
                _modeTabIcons[i].color = active ? modeInfo.themeColor : new Color(0.5f, 0.45f, 0.6f);
            if (_modeTabLabels[i] != null)
            {
                _modeTabLabels[i].color = active ? Bright : new Color(0.6f, 0.55f, 0.7f);
                _modeTabLabels[i].fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            }
        }

        if (_modeDescTitle != null)
        {
            _modeDescTitle.text = "当前已选: " + info.descTitle;
            _modeDescTitle.color = info.themeColor;
        }
        if (_modeDescFloors != null) _modeDescFloors.text = info.floors;
        if (_modeDescFlavor != null) _modeDescFlavor.text = info.flavor;
        if (_modeDescFit != null) _modeDescFit.text = info.fit;
        if (_modeDescFeature1 != null) _modeDescFeature1.text = info.feat1;
        if (_modeDescFeature2 != null) _modeDescFeature2.text = info.feat2;

        if (_modeDescOutline != null)
            _modeDescOutline.effectColor = new Color(info.themeColor.r, info.themeColor.g, info.themeColor.b, 0.35f);
        if (_modeDescBg != null)
            _modeDescBg.color = new Color(
                Mathf.Lerp(0.1f, info.bgColor.r * 1.5f, 0.3f),
                Mathf.Lerp(0.08f, info.bgColor.g * 1.5f, 0.3f),
                Mathf.Lerp(0.16f, info.bgColor.b * 1.5f, 0.3f));

        if (_modeStartBtnTxt != null)
        {
            _modeStartBtnTxt.text = info.startBtnText;
            _modeStartBtnTxt.color = info.themeColor;
        }
        if (_modeStartBtnOutline != null)
            _modeStartBtnOutline.effectColor = new Color(info.themeColor.r, info.themeColor.g, info.themeColor.b, 0.5f);
    }

    void BuildChallengeCard(Transform parent, string icon, string title, string desc, Color themeColor, Color bgColor, System.Action onClick)
    {
        var card = new GameObject("Chal_" + title, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        card.transform.SetParent(parent, false);
        var cardImg = card.GetComponent<Image>();
        cardImg.color = bgColor;
        var ol = card.GetComponent<Outline>();
        ol.effectColor = new Color(themeColor.r, themeColor.g, themeColor.b, 0.4f);
        ol.effectDistance = new Vector2(1, 1);

        var inner = new GameObject("Inner", typeof(RectTransform), typeof(VerticalLayoutGroup));
        inner.transform.SetParent(card.transform, false);
        Stretch(inner);
        var ivl = inner.GetComponent<VerticalLayoutGroup>();
        ivl.spacing = 3; ivl.padding = new RectOffset(8, 8, 10, 8);
        ivl.childAlignment = TextAnchor.MiddleCenter;
        ivl.childForceExpandWidth = true; ivl.childForceExpandHeight = false;

        var iconTxt = TxtGo(inner.transform, icon, 22, themeColor);
        iconTxt.alignment = TextAnchor.MiddleCenter;
        iconTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;

        var titleTxt = TxtGo(inner.transform, title, 13, Bright);
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 20;

        var descTxt = TxtGo(inner.transform, desc, 10, Dim);
        descTxt.alignment = TextAnchor.MiddleCenter;
        descTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 16;

        card.GetComponent<Button>().targetGraphic = cardImg;
        card.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
    }

    void GoToModeSelect()
    {
        if (CompleteGameSystem.Instance != null)
            CompleteGameSystem.Instance.ShowModeSelect();
        SA(_menuPanel, false);
        if (_classPanel == null) BuildClass();
        SA(_classPanel, true);
    }

    void StartSelectedMode()
    {
        GameManager.Instance?.StartNewGame(_selectedModeId);
        StartCoroutine(DelayedSelectClass());
    }

    IEnumerator DelayedSelectClass()
    {
        yield return null;
        CompleteGameSystem.Instance?.SelectClass("titan");
    }

    void BuildChallengeTab(Transform parent, string label, bool active)
    {
        var tab = new GameObject("ChalTab_" + label, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        tab.transform.SetParent(parent, false);
        tab.AddComponent<LayoutElement>().preferredWidth = 120;
        tab.GetComponent<Image>().color = active ? new Color(0.08f, 0.06f, 0.12f) : new Color(0.08f, 0.06f, 0.12f);
        tab.GetComponent<Outline>().effectColor = active ? new Color(Gold.r, Gold.g, Gold.b, 0.35f) : Color.clear;
        tab.GetComponent<Outline>().effectDistance = new Vector2(1, 1);

        var txt = new GameObject("T", typeof(RectTransform), typeof(Text));
        txt.transform.SetParent(tab.transform, false);
        Stretch(txt);
        var t = txt.GetComponent<Text>();
        t.font = F(); t.fontSize = 12; t.color = active ? Gold : Dim;
        t.alignment = TextAnchor.MiddleCenter; t.text = label;
        t.raycastTarget = false;

        tab.GetComponent<Button>().targetGraphic = tab.GetComponent<Image>();
    }

    void ShowChallengePanel()
    {
        if (_chalOvl == null) BuildChallengeOverlay();
        OpenOverlayAnimated(_chalOvl);
    }

    InputField _chalSeedInput;

    void BuildChallengeOverlay()
    {
        var content = BuildOverlayScaffold(ref _chalOvl, "ChalOvl", "🎯 " + LocalizationData.T("挑战模式"), Gold, new Color(0.08f, 0.06f, 0.12f, 0.82f), "bg_challenge");

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 12);
        Txt(content, LocalizationData.T("每日挑战"), 16, Gold, 28);
        Txt(content, LocalizationData.T("使用今天的随机种子进行挑战，排行榜自动记录成绩。"), 11, Dim, 36);

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 8);
        var dailyBtn = new GameObject("DailyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        dailyBtn.transform.SetParent(content.transform, false);
        dailyBtn.AddComponent<LayoutElement>().preferredHeight = 48;
        dailyBtn.GetComponent<Image>().color = new Color(0.12f, 0.1f, 0.2f);
        dailyBtn.AddComponent<Outline>().effectColor = new Color(Gold.r, Gold.g, Gold.b, 0.4f);
        var dTxt = TxtGo(dailyBtn.transform, LocalizationData.T("开始每日挑战"), 14, Gold);
        dTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(dTxt.gameObject);
        dailyBtn.GetComponent<Button>().targetGraphic = dailyBtn.GetComponent<Image>();
        dailyBtn.GetComponent<Button>().onClick.AddListener(() => {
            StartCoroutine(CloseOverlayAnimated(_chalOvl));
            CompleteGameSystem.Instance?.StartChallengeMode("daily");
        });

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 20);
        Txt(content, LocalizationData.T("本周挑战"), 16, Purp, 28);
        Txt(content, LocalizationData.T("极限模式：污染加速、精英怪率翻倍，每周一刷新。"), 11, Dim, 36);

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 8);
        var weeklyBtn = new GameObject("WeeklyBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        weeklyBtn.transform.SetParent(content.transform, false);
        weeklyBtn.AddComponent<LayoutElement>().preferredHeight = 48;
        weeklyBtn.GetComponent<Image>().color = new Color(0.12f, 0.1f, 0.2f);
        weeklyBtn.AddComponent<Outline>().effectColor = new Color(Purp.r, Purp.g, Purp.b, 0.4f);
        var wTxt = TxtGo(weeklyBtn.transform, LocalizationData.T("开始本周挑战"), 14, Purp);
        wTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(wTxt.gameObject);
        weeklyBtn.GetComponent<Button>().targetGraphic = weeklyBtn.GetComponent<Image>();
        weeklyBtn.GetComponent<Button>().onClick.AddListener(() => {
            StartCoroutine(CloseOverlayAnimated(_chalOvl));
            CompleteGameSystem.Instance?.StartChallengeMode("weekly");
        });

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 20);
        Txt(content, LocalizationData.T("自定义种子"), 16, Cyan, 28);
        Txt(content, LocalizationData.T("输入种子码与好友同台竞技，相同种子生成相同关卡。"), 11, Dim, 36);

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 8);
        var seedRow = new GameObject("SeedRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        seedRow.transform.SetParent(content.transform, false);
        seedRow.AddComponent<LayoutElement>().preferredHeight = 44;
        var shl = seedRow.GetComponent<HorizontalLayoutGroup>();
        shl.spacing = 8; shl.childForceExpandWidth = true; shl.childForceExpandHeight = true;

        var inputGo = new GameObject("SeedInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        inputGo.transform.SetParent(seedRow.transform, false);
        inputGo.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.1f);
        var placeholder = TxtGo(inputGo.transform, LocalizationData.T("输入种子码..."), 12, Dim);
        placeholder.alignment = TextAnchor.MiddleLeft;
        Stretch(placeholder.gameObject);
        var inputText = TxtGo(inputGo.transform, "", 12, Bright);
        inputText.alignment = TextAnchor.MiddleLeft;
        Stretch(inputText.gameObject);
        _chalSeedInput = inputGo.GetComponent<InputField>();
        _chalSeedInput.textComponent = inputText;
        _chalSeedInput.placeholder = placeholder;
        _chalSeedInput.targetGraphic = inputGo.GetComponent<Image>();

        var seedBtn = new GameObject("SeedStartBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        seedBtn.transform.SetParent(seedRow.transform, false);
        seedBtn.AddComponent<LayoutElement>().preferredWidth = 80;
        seedBtn.GetComponent<Image>().color = new Color(0.12f, 0.1f, 0.2f);
        seedBtn.AddComponent<Outline>().effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.4f);
        var sTxt = TxtGo(seedBtn.transform, LocalizationData.T("开始"), 13, Cyan);
        sTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(sTxt.gameObject);
        seedBtn.GetComponent<Button>().targetGraphic = seedBtn.GetComponent<Image>();
        seedBtn.GetComponent<Button>().onClick.AddListener(() => {
            string seed = _chalSeedInput != null ? _chalSeedInput.text : "";
            if (string.IsNullOrEmpty(seed)) seed = UnityEngine.Random.Range(100000, 999999).ToString();
            StartCoroutine(CloseOverlayAnimated(_chalOvl));
            CompleteGameSystem.Instance?.StartChallengeMode("custom", seed);
        });
    }

    // ========== EXPLORE ==========
    void BuildExplore()
    {
        _explorePanel = Panel("Explore", PanelBg);
        var rt = _explorePanel.GetComponent<RectTransform>();

        // 楼层背景图（最底层，被地图格子覆盖）
        var floorBgGo = new GameObject("FloorBg", typeof(RectTransform), typeof(RawImage));
        floorBgGo.transform.SetParent(rt, false);
        Stretch(floorBgGo);
        _eFloorBg = floorBgGo.GetComponent<RawImage>();
        _eFloorBg.color = new Color(1f, 1f, 1f, 0.3f);
        _eFloorBg.raycastTarget = false;
        _eFloorBgIdx = -1;

        // === HUD (top 10%) ===
        var hudGo = new GameObject("HUD", typeof(RectTransform), typeof(Image));
        hudGo.transform.SetParent(rt, false);
        var hudImg = hudGo.GetComponent<Image>();
        hudImg.color = CardBg;
        var hudRT = hudGo.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 0.9f);
        hudRT.anchorMax = new Vector2(1, 1);
        hudRT.offsetMin = Vector2.zero;
        hudRT.offsetMax = Vector2.zero;
        
        // 添加边框
        var hudOutline = hudGo.AddComponent<Outline>();
        hudOutline.effectColor = new Color(0.3f, 0.25f, 0.45f, 0.5f);
        hudOutline.effectDistance = new Vector2(0, -2);

        // 左上角：楼层和区域信息
        _eFloor = TxtAnchored(hudRT, "F1 入口通道", 14, Gold, new Vector2(0,0.55f), new Vector2(0.3f,1), 8, 0);
        _eFloor.alignment = TextAnchor.MiddleLeft;
        
        // 左侧：当前形态名称
        _eFormName = TxtAnchored(hudRT, "泰坦", 12, Cyan, new Vector2(0,0.05f), new Vector2(0.3f,0.55f), 8, 0);
        _eFormName.alignment = TextAnchor.MiddleLeft;
        
        // 中间：HP条和污染条
        _eHpFill = BarAnchored(hudRT, new Color(0,0.85f,0.45f), new Vector2(0.32f,0.55f), new Vector2(0.62f,0.95f), out _eHpTxt);
        _ePollFill = BarAnchored(hudRT, new Color(0.71f,0.33f,1), new Vector2(0.63f,0.55f), new Vector2(0.85f,0.95f), out _ePollTxt);
        
        // 右侧：时间和状态
        _eTimer = TxtAnchored(hudRT, "13:50", 14, Gold, new Vector2(0.85f,0.55f), new Vector2(0.94f,1), 0, 0);
        _eTimer.alignment = TextAnchor.MiddleCenter;
        
        // 最右侧：污染值和净化按钮
        _ePollValue = TxtAnchored(hudRT, "P 0%", 12, Dim, new Vector2(0.94f,0.55f), new Vector2(1,1), 0, 0);
        _ePollValue.alignment = TextAnchor.MiddleRight;
        
        // 右上角：当前形态名
        _eRebirth = TxtAnchored(hudRT, "", 11, Cyan, new Vector2(0.82f,0.05f), new Vector2(1,0.55f), 0, 0);
        _eRebirth.alignment = TextAnchor.MiddleRight;

        // === Host bar below HUD (explore) ===
        _eHostBar = new GameObject("ExHostBar", typeof(RectTransform), typeof(Image), typeof(Outline));
        _eHostBar.transform.SetParent(rt, false);
        _eHostBar.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.15f, 0.95f);
        var ehOL = _eHostBar.GetComponent<Outline>();
        ehOL.effectColor = new Color(0f, 1f, 0.816f, 0.3f);
        ehOL.effectDistance = new Vector2(1, 1);
        var ehRT = _eHostBar.GetComponent<RectTransform>();
        ehRT.anchorMin = new Vector2(0, 0.87f);
        ehRT.anchorMax = new Vector2(1, 0.898f);
        ehRT.offsetMin = new Vector2(4, 0); ehRT.offsetMax = new Vector2(-4, 0);

        _eHostName = TxtAnchored(ehRT, "", 12, Cyan, new Vector2(0, 0), new Vector2(0.36f, 1), 8, 0);
        _eHostName.alignment = TextAnchor.MiddleLeft;
        _eHostName.fontStyle = FontStyle.Bold;

        _eHostTraits = TxtAnchored(ehRT, "", 11, new Color(0.85f, 0.75f, 1f), new Vector2(0.36f, 0), new Vector2(0.84f, 1), 4, 0);
        _eHostTraits.alignment = TextAnchor.MiddleLeft;

        var eHostInfoBtn = new GameObject("EHostInfoBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        eHostInfoBtn.transform.SetParent(ehRT, false);
        var ehBtnRT = eHostInfoBtn.GetComponent<RectTransform>();
        ehBtnRT.anchorMin = new Vector2(0.85f, 0.1f);
        ehBtnRT.anchorMax = new Vector2(0.99f, 0.9f);
        ehBtnRT.offsetMin = Vector2.zero; ehBtnRT.offsetMax = Vector2.zero;
        eHostInfoBtn.GetComponent<Image>().color = new Color(0.2f, 0.12f, 0.3f, 0.9f);
        eHostInfoBtn.AddComponent<Outline>().effectColor = new Color(1f, 0.75f, 0.3f, 0.6f);
        var ehBtnTxt = TxtAnchored(ehBtnRT, "!", 14, Gold, Vector2.zero, Vector2.one, 0, 0);
        ehBtnTxt.alignment = TextAnchor.MiddleCenter;
        ehBtnTxt.fontStyle = FontStyle.Bold;
        eHostInfoBtn.GetComponent<Button>().targetGraphic = eHostInfoBtn.GetComponent<Image>();
        eHostInfoBtn.GetComponent<Button>().onClick.AddListener(() => ShowHostTraitInfo());

        _eHostBar.SetActive(false);

        // === Tutorial (thin bar below HUD) ===
        // Separator: HUD → Tutorial/Map
        var hudSep = new GameObject("HudSep", typeof(RectTransform), typeof(Image));
        hudSep.transform.SetParent(rt, false);
        var hudSepRT = hudSep.GetComponent<RectTransform>();
        hudSepRT.anchorMin = new Vector2(0.03f, 0.898f); hudSepRT.anchorMax = new Vector2(0.97f, 0.9f);
        hudSepRT.offsetMin = Vector2.zero; hudSepRT.offsetMax = Vector2.zero;
        hudSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);

        // === Tutorial notification bar (compact, top area) ===
        _eTutGo = new GameObject("Tut", typeof(RectTransform), typeof(Image), typeof(Outline));
        _eTutGo.transform.SetParent(_root, false);
        _eTutGo.GetComponent<Image>().color = new Color(0.133f, 0.125f, 0.251f, 0.95f);
        _eTutGo.GetComponent<Image>().raycastTarget = false;
        var tutOL = _eTutGo.GetComponent<Outline>();
        tutOL.effectColor = new Color(0f, 1f, 0.816f, 0.6f);
        tutOL.effectDistance = new Vector2(1, -1);
        var tutRT = _eTutGo.GetComponent<RectTransform>();
        // 固定高度 56px，紧贴 HUD 下方（HUD 占顶部 10%=96px）
        tutRT.anchorMin = new Vector2(0.04f, 1);
        tutRT.anchorMax = new Vector2(0.96f, 1);
        tutRT.offsetMin = new Vector2(0, -156);
        tutRT.offsetMax = new Vector2(0, -100);

        // 左侧：💡标题 (单行)
        _eTutTitle = TxtAnchored(tutRT, "", 14, new Color(0f, 1f, 0.816f), new Vector2(0, 0.52f), new Vector2(0.72f, 1), 10, 0);
        _eTutTitle.alignment = TextAnchor.MiddleLeft;
        _eTutTitle.fontStyle = FontStyle.Bold;
        _eTutTitle.raycastTarget = false;

        // 右上角：步骤计数
        _eTutStep = TxtAnchored(tutRT, "", 9, new Color(0.5f, 0.5f, 0.55f), new Vector2(0.72f, 0.55f), new Vector2(1, 1), 0, 6);
        _eTutStep.alignment = TextAnchor.MiddleRight;
        _eTutStep.raycastTarget = false;

        // 下半：描述 (单行或两行)
        _eTutTxt = TxtAnchored(tutRT, "", 11, new Color(0.82f, 0.82f, 0.82f), new Vector2(0, 0), new Vector2(0.78f, 0.52f), 10, 0);
        _eTutTxt.alignment = TextAnchor.MiddleLeft;
        _eTutTxt.raycastTarget = false;

        // 右下角：知道了 按钮
        _eTutSkipBtn = new GameObject("Skip", typeof(RectTransform), typeof(Image), typeof(Button));
        _eTutSkipBtn.transform.SetParent(tutRT, false);
        var skipRT = _eTutSkipBtn.GetComponent<RectTransform>();
        skipRT.anchorMin = new Vector2(0.78f, 0.08f);
        skipRT.anchorMax = new Vector2(0.98f, 0.48f);
        skipRT.offsetMin = Vector2.zero; skipRT.offsetMax = Vector2.zero;
        _eTutSkipBtn.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.1f);
        var skipTxt = new GameObject("T", typeof(RectTransform), typeof(Text));
        skipTxt.transform.SetParent(_eTutSkipBtn.transform, false); Stretch(skipTxt);
        var st = skipTxt.GetComponent<Text>();
        st.font = F(); st.text = "知道了"; st.fontSize = 10;
        st.color = new Color(0f, 1f, 0.816f, 0.7f);
        st.alignment = TextAnchor.MiddleCenter; st.raycastTarget = false;
        _eTutSkipBtn.GetComponent<Button>().targetGraphic = _eTutSkipBtn.GetComponent<Image>();
        _eTutSkipBtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.DismissTutorial());

        // === Map grid (middle 55%) ===
        var mapGo = new GameObject("Map", typeof(RectTransform));
        mapGo.transform.SetParent(rt, false);
        var mapRT = mapGo.GetComponent<RectTransform>();
        mapRT.anchorMin = new Vector2(0, 0.3f);
        mapRT.anchorMax = new Vector2(1, 0.87f);
        mapRT.offsetMin = new Vector2(4, 4);
        mapRT.offsetMax = new Vector2(-4, -4);

        var grid = mapGo.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 13;
        grid.spacing = new Vector2(1, 1);
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.cellSize = new Vector2(36, 36);

        for (int y = 12; y >= 0; y--)
        {
            for (int x = 0; x < 13; x++)
            {
                var cell = new GameObject($"C{x}_{y}", typeof(RectTransform), typeof(Image));
                cell.transform.SetParent(mapGo.transform, false);
                _mapImg[y,x] = cell.GetComponent<Image>();
                _mapImg[y,x].color = new Color(0.06f,0.04f,0.1f);

                var t = new GameObject("T", typeof(RectTransform), typeof(Text));
                t.transform.SetParent(cell.transform, false); Stretch(t);
                var tx = t.GetComponent<Text>();
                tx.font = F(); tx.fontSize = 14; tx.alignment = TextAnchor.MiddleCenter; tx.color = Color.white;
                _mapTxt[y,x] = tx;

                var ico = new GameObject("I", typeof(RectTransform), typeof(Image));
                ico.transform.SetParent(cell.transform, false); Stretch(ico);
                var icoImg = ico.GetComponent<Image>();
                icoImg.color = new Color(1,1,1,0);
                icoImg.preserveAspect = true;
                _mapIcon[y,x] = icoImg;
            }
        }

        // === Status bar (between map and actions) ===
        // 消息区域背景
        var msgBgGo = new GameObject("MsgBg", typeof(RectTransform), typeof(Image));
        msgBgGo.transform.SetParent(rt, false);
        var msgBgRT = msgBgGo.GetComponent<RectTransform>();
        msgBgRT.anchorMin = new Vector2(0, 0.275f); msgBgRT.anchorMax = new Vector2(1, 0.325f);
        msgBgRT.offsetMin = Vector2.zero; msgBgRT.offsetMax = Vector2.zero;
        msgBgGo.GetComponent<Image>().color = new Color(0.07f, 0.08f, 0.12f, 0.65f);
        msgBgGo.GetComponent<Image>().raycastTarget = false;

        // 左侧：消息提示
        _eMsg = TxtAnchored(rt, "", 12, new Color(0.6f,0.85f,0.6f), new Vector2(0,0.28f), new Vector2(0.7f,0.32f), 8, 0);
        _eMsg.alignment = TextAnchor.MiddleLeft;
        _eMsg.supportRichText = true;
        
        // 中间：属性信息
        _eProg = TxtAnchored(rt, "", 12, Bright, new Vector2(0,0.24f), new Vector2(0.5f,0.28f), 8, 0);
        _eProg.alignment = TextAnchor.MiddleLeft;
        
        // 右侧：记忆碎片和进化等级
        _eFragments = TxtAnchored(rt, "碎片×0", 12, Cyan, new Vector2(0.5f,0.24f), new Vector2(0.75f,0.28f), 0, 0);
        _eFragments.alignment = TextAnchor.MiddleCenter;
        
        _eEvoLvl = TxtAnchored(rt, "Evo Lv0", 12, Gold, new Vector2(0.75f,0.24f), new Vector2(1,0.28f), 0, 0);
        _eEvoLvl.alignment = TextAnchor.MiddleRight;

        // === Action area (bottom 22%) ===
        // Separator: Status → Actions
        var actSep = new GameObject("ActSep", typeof(RectTransform), typeof(Image));
        actSep.transform.SetParent(rt, false);
        var actSepRT = actSep.GetComponent<RectTransform>();
        actSepRT.anchorMin = new Vector2(0.03f, 0.22f); actSepRT.anchorMax = new Vector2(0.97f, 0.222f);
        actSepRT.offsetMin = Vector2.zero; actSepRT.offsetMax = Vector2.zero;
        actSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);

        var actGo = new GameObject("Actions", typeof(RectTransform), typeof(Image));
        actGo.transform.SetParent(rt, false);
        actGo.GetComponent<Image>().color = CardBg;
        var actRT = actGo.GetComponent<RectTransform>();
        actRT.anchorMin = new Vector2(0, 0);
        actRT.anchorMax = new Vector2(1, 0.22f);
        actRT.offsetMin = Vector2.zero;
        actRT.offsetMax = Vector2.zero;

        // D-pad wheel (left side)
        var dpadGo = new GameObject("DPad", typeof(RectTransform), typeof(Image), typeof(DPadWheel));
        dpadGo.transform.SetParent(actRT, false);
        var dpad = dpadGo.GetComponent<DPadWheel>();
        dpad.Setup(actRT, new Vector2(-120, 20), 140);
        dpad.onDirection = dir => CompleteGameSystem.Instance?.Move(dir);

        // Right side buttons
        ActBtn(actGo.transform, "菜单", Dim, new Vector2(140,25), new Vector2(120,70), () => ShowMenuPanel());

        // Mini-map (top right of map area)
        BuildMiniMap(rt);
    }

    // ========== COMBAT ==========
    // Additional combat refs
    Text _cEnemyTitle, _cEnemyDesc, _cPossLabel, _cBattleLog;
    Image _cPossFill;
    Text _cPTraits, _cETraits;
    Outline _cPCardOutline, _cECardOutline;
    // Host traits bar (combat)
    GameObject _cHostBar, _cPTraitBtn;
    Text _cHostName, _cHostTraits;
    // Combat action buttons
    Button _cAtkBtn, _cDefBtn, _cPossBtn, _cFleeBtn, _cUltBtn;
    Text _cUltLabel;
    // Trait info popup
    GameObject _traitInfoOvl;
    Text _traitInfoTitle, _traitInfoDesc;

    void ShowEnemyTraitInfo()
    {
        if (_traitInfoOvl == null || CompleteGameSystem.Instance?.CurrentEnemy == null) return;
        CompleteGameSystem.Instance.DismissSoftHint("trait");

        var enemy = CompleteGameSystem.Instance.CurrentEnemy;
        if (enemy.traits == null || enemy.traits.Length == 0) return;

        ShowTraitPopup(enemy.traits, "目标技能");
    }

    void ShowHostTraitInfo()
    {
        if (_traitInfoOvl == null) return;
        var p = GameManager.Instance?.Player;
        if (p == null || p.traits == null || p.traits.Count == 0) return;

        ShowTraitPopup(p.traits.ToArray(), "寄主技能");
    }

    void ShowTraitPopup(string[] traits, string header)
    {
        if (traits == null || traits.Length == 0) return;
        string allNames = string.Join(" / ", traits);
        string allDescs = "";
        foreach (var t in traits)
        {
            string desc = GetTraitDescription(t);
            allDescs += $"<color=#{ColorUtility.ToHtmlStringRGB(Gold)}>{t}</color>: {desc}\n";
        }

        ST(_traitInfoTitle, allNames);
        ST(_traitInfoDesc, allDescs.TrimEnd('\n'));
        _traitInfoOvl.SetActive(true);
    }

    string GetTraitDescription(string traitName)
    {
        switch(traitName)
        {
            case "电击": return "攻击时有30%几率使目标麻痹1回合，无法行动。";
            case "厚皮": return "受到的物理伤害降低30%。";
            case "剧毒": return "攻击时使目标中毒，每回合受到5点伤害，持续3回合。";
            case "再生": return "每回合恢复最大生命值的10%。";
            case "再生+": return "每回合恢复最大生命值的20%，强化版再生。";
            case "狂暴": return "生命值低于50%时，攻击力提升50%。";
            case "反射": return "受到攻击时有20%几率反弹30%伤害。";
            case "忠诚": return "被附身后保留原主人的部分防御力，防御+2。";
            case "迅捷": return "行动速度更快，有几率先手攻击。";
            case "弹性": return "受到致命攻击时有25%几率保留1点HP存活。";
            case "蛛网": return "攻击时有30%几率减速目标，降低其闪避率。";
            case "护甲": return "战斗前3回合防御力翻倍。";
            case "毒素": return "攻击附带毒素，目标每回合流失HP。";
            case "吸血": return "造成伤害时恢复等量30%的生命值。";
            case "反击": return "受到攻击后自动反击，造成自身攻击力50%的伤害。";
            case "掠夺": return "击败敌人时获得额外进化点数。";
            case "暴击": return "攻击时有25%几率造成1.5倍暴击伤害。";
            case "寄生强化": return "附身成功率+15%，附身后继承更多属性。";
            case "撕裂": return "攻击造成流血效果，每回合额外损失HP。";
            case "相位": return "有20%几率闪避攻击，伤害完全无效化。";
            case "伏击": return "战斗首回合造成双倍伤害。";
            case "不死": return "首次受到致命伤害时以1HP存活，每场战斗触发一次。";
            case "吸取": return "攻击时窃取目标少量属性值。";
            case "恐惧": return "战斗开始时有几率降低目标攻击力。";
            case "多重攻击": return "每回合攻击2次。";
            case "污染光环": return "每回合对目标施加额外污染值。";
            case "领袖": return "增强同区域其他怪物的属性。";
            case "爆炸": return "死亡时对攻击者造成自身最大HP30%的伤害。";
            case "召唤": return "战斗中有几率召唤增援怪物。";
            default: return "未知特性。";
        }
    }

    void AddCornerLight(Transform parent, Vector2 anchor, Color color, float intensity)
    {
        var lightGo = new GameObject("CornerLight", typeof(RectTransform), typeof(Image));
        lightGo.transform.SetParent(parent, false);
        var lightRT = lightGo.GetComponent<RectTransform>();
        lightRT.anchorMin = anchor;
        lightRT.anchorMax = anchor;
        lightRT.pivot = anchor;
        lightRT.sizeDelta = new Vector2(200, 200);
        lightRT.anchoredPosition = Vector2.zero;
        
        var lightImg = lightGo.GetComponent<Image>();
        lightImg.color = color * intensity;
        lightImg.type = Image.Type.Filled;
        lightImg.fillMethod = Image.FillMethod.Radial360;
        lightImg.fillAmount = 0.5f;
    }

    void BuildCombat()
    {
        _combatPanel = Panel("Combat", new Color(0.06f,0.04f,0.10f));
        var rt = _combatPanel.GetComponent<RectTransform>();

        // 添加背景网格图案
        var bgGrid = new GameObject("BgGrid", typeof(RectTransform), typeof(Image));
        bgGrid.transform.SetParent(rt, false);
        var bgGridRT = bgGrid.GetComponent<RectTransform>();
        bgGridRT.anchorMin = Vector2.zero; bgGridRT.anchorMax = Vector2.one;
        bgGridRT.offsetMin = Vector2.zero; bgGridRT.offsetMax = Vector2.zero;
        var bgGridImg = bgGrid.GetComponent<Image>();
        bgGridImg.color = new Color(0.15f, 0.35f, 0.3f, 0.12f);
        
        // 创建网格材质
        Shader gridShader = Shader.Find("Unlit/Transparent") ?? Shader.Find("UI/Default");
        var gridMat = new Material(gridShader);
        Texture2D gridTex = new Texture2D(64, 64);
        Color[] pixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                bool isLine = x % 16 == 0 || y % 16 == 0;
                pixels[y * 64 + x] = isLine ? new Color(0.2f, 0.6f, 0.5f, 0.3f) : new Color(0, 0, 0, 0);
            }
        }
        gridTex.SetPixels(pixels);
        gridTex.Apply();
        gridMat.mainTexture = gridTex;
        gridMat.color = new Color(1, 1, 1, 0.5f);
        bgGridImg.material = gridMat;

        // 添加渐变覆盖层
        var gradientGo = new GameObject("Gradient", typeof(RectTransform), typeof(Image));
        gradientGo.transform.SetParent(rt, false);
        var gradientRT = gradientGo.GetComponent<RectTransform>();
        gradientRT.anchorMin = Vector2.zero; gradientRT.anchorMax = Vector2.one;
        gradientRT.offsetMin = Vector2.zero; gradientRT.offsetMax = Vector2.zero;
        var gradientImg = gradientGo.GetComponent<Image>();
        gradientImg.color = new Color(0.07f, 0.04f, 0.12f, 0.7f);

        // 添加角落装饰光效 (柔和紫色调，不刺眼)
        AddCornerLight(rt, new Vector2(0, 1), new Color(0.15f, 0.25f, 0.4f), 0.08f);
        AddCornerLight(rt, new Vector2(1, 1), new Color(0.25f, 0.12f, 0.35f), 0.06f);

        // 污染渐晕叠层 — SyncCombat 中动态控制 alpha
        var pollVigGo = new GameObject("PollVignette", typeof(RectTransform), typeof(Image));
        pollVigGo.transform.SetParent(rt, false);
        var pollVigRT = pollVigGo.GetComponent<RectTransform>();
        pollVigRT.anchorMin = Vector2.zero; pollVigRT.anchorMax = Vector2.one;
        pollVigRT.offsetMin = Vector2.zero; pollVigRT.offsetMax = Vector2.zero;
        _cPollVignette = pollVigGo.GetComponent<Image>();
        _cPollVignette.color = new Color(0, 0, 0, 0);
        _cPollVignette.raycastTarget = false;

        // === Top: Enemy name + description (top 12%) ===
        var topGo = new GameObject("Top", typeof(RectTransform), typeof(Image));
        topGo.transform.SetParent(rt, false);
        topGo.GetComponent<Image>().color = new Color(0.10f,0.06f,0.16f,0.95f);
        var topRT = topGo.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0, 0.88f);
        topRT.anchorMax = new Vector2(1, 1);
        topRT.offsetMin = Vector2.zero; topRT.offsetMax = Vector2.zero;

        _cEnemyTitle = TxtAnchored(topRT, "", 28, Bright, new Vector2(0,0.4f), new Vector2(1,1), 10, 10);
        _cEnemyTitle.alignment = TextAnchor.MiddleCenter;
        _cEnemyTitle.fontStyle = FontStyle.Bold;
        _cEnemyDesc = TxtAnchored(topRT, "", 14, Dim, new Vector2(0,0), new Vector2(1,0.4f), 15, 15);
        _cEnemyDesc.alignment = TextAnchor.MiddleCenter;

        // === VS Cards area (middle 40%) ===
        var cardsGo = new GameObject("Cards", typeof(RectTransform), typeof(Image));
        cardsGo.transform.SetParent(rt, false);
        cardsGo.GetComponent<Image>().color = new Color(0.10f,0.08f,0.18f,0.9f);
        var cardsRT = cardsGo.GetComponent<RectTransform>();
        cardsRT.anchorMin = new Vector2(0, 0.48f);
        cardsRT.anchorMax = new Vector2(1, 0.88f);
        cardsRT.offsetMin = new Vector2(8, 4); cardsRT.offsetMax = new Vector2(-8, -4);

        // -- Player card (left 48%) --
        var pcGo = new GameObject("PCard", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        pcGo.transform.SetParent(cardsRT, false);
        pcGo.GetComponent<Image>().color = new Color(0.10f, 0.07f, 0.16f);
        
        // 卡片阴影
        var pcShadow = pcGo.GetComponent<Shadow>();
        pcShadow.effectColor = new Color(0, 0, 0, 0.5f);
        pcShadow.effectDistance = new Vector2(3, -3);
        
        // 卡片边框发光
        var pcOutline = pcGo.GetComponent<Outline>();
        pcOutline.effectColor = new Color(0.5f, 0.3f, 0.8f, 0.3f);
        pcOutline.effectDistance = new Vector2(2, 2);
        _cPCardOutline = pcOutline;
        
        var pcRT = pcGo.GetComponent<RectTransform>();
        pcRT.anchorMin = new Vector2(0, 0); pcRT.anchorMax = new Vector2(0.46f, 1);
        pcRT.offsetMin = new Vector2(6, 6); pcRT.offsetMax = new Vector2(-3, -6);
        
        // 内边框装饰
        var pcInner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
        pcInner.transform.SetParent(pcRT, false);
        var pcInnerRT = pcInner.GetComponent<RectTransform>();
        pcInnerRT.anchorMin = new Vector2(0, 0); pcInnerRT.anchorMax = new Vector2(1, 1);
        pcInnerRT.offsetMin = new Vector2(2, 2); pcInnerRT.offsetMax = new Vector2(-2, -2);
        pcInner.GetComponent<Image>().color = new Color(0.3f, 0.25f, 0.4f, 0.12f);

        TxtAnchored(pcRT, "寄生体", 14, Cyan, new Vector2(0,0.88f), new Vector2(1,1), 4, 0);
        _cPIcon = ImgAnchored(pcRT, new Vector2(0.15f,0.45f), new Vector2(0.85f,0.88f));
        _cPName = TxtAnchored(pcRT, "", 14, Bright, new Vector2(0,0.32f), new Vector2(1,0.45f), 4, 4);
        _cPHpFill = BarAnchored(pcRT, new Color(0,0.85f,0.45f), new Vector2(0.05f,0.22f), new Vector2(0.95f,0.32f), out _cPHpTxt);
        _cPStat = TxtAnchored(pcRT, "", 14, Bright, new Vector2(0,0.12f), new Vector2(1,0.22f), 4, 4);
        // 玩家技能按钮条（与敌人卡片相同样式）
        var pTraitBtnGo = new GameObject("PTraitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        pTraitBtnGo.transform.SetParent(pcRT, false);
        var pTraitBtnRT = pTraitBtnGo.GetComponent<RectTransform>();
        pTraitBtnRT.anchorMin = new Vector2(0, 0);
        pTraitBtnRT.anchorMax = new Vector2(1, 0.14f);
        pTraitBtnRT.offsetMin = new Vector2(3, 2);
        pTraitBtnRT.offsetMax = new Vector2(-3, -1);
        pTraitBtnGo.GetComponent<Image>().color = new Color(0.08f, 0.12f, 0.2f, 0.85f);
        var pTraitOL = pTraitBtnGo.GetComponent<Outline>();
        pTraitOL.effectColor = new Color(0f, 1f, 0.816f, 0.4f);
        pTraitOL.effectDistance = new Vector2(1, 1);
        pTraitBtnGo.GetComponent<Button>().targetGraphic = pTraitBtnGo.GetComponent<Image>();
        pTraitBtnGo.GetComponent<Button>().onClick.AddListener(() => ShowHostTraitInfo());

        _cPTraits = TxtAnchored(pTraitBtnRT, "", 12, Cyan, new Vector2(0,0), new Vector2(0.78f,1), 6, 2);
        _cPTraits.alignment = TextAnchor.MiddleLeft;
        _cPTraits.fontStyle = FontStyle.Bold;

        var pViewLabel = TxtAnchored(pTraitBtnRT, "[查看]", 11, Cyan, new Vector2(0.78f, 0), new Vector2(1, 1), 2, 2);
        pViewLabel.alignment = TextAnchor.MiddleCenter;
        _cPTraitBtn = pTraitBtnGo;

        // -- VS label --
        var vsGo = new GameObject("VS", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        vsGo.transform.SetParent(rt, false);
        var vsRT = vsGo.GetComponent<RectTransform>();
        vsRT.anchorMin = new Vector2(0.46f, 0.60f); vsRT.anchorMax = new Vector2(0.54f, 0.72f);
        vsRT.offsetMin = Vector2.zero; vsRT.offsetMax = Vector2.zero;
        
        // VS背景
        var vsImg = vsGo.GetComponent<Image>();
        vsImg.color = new Color(0.12f, 0.06f, 0.18f);
        
        // VS阴影
        var vsShadow = vsGo.GetComponent<Shadow>();
        vsShadow.effectColor = new Color(0, 0, 0, 0.3f);
        vsShadow.effectDistance = new Vector2(2, -2);
        
        // VS边框发光效果
        var vsOutline = vsGo.GetComponent<Outline>();
        vsOutline.effectColor = new Color(0.8f, 0.3f, 0.2f, 0.4f);
        vsOutline.effectDistance = new Vector2(2, 2);
        
        // VS文字
        var vsTxt = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Outline));
        vsTxt.transform.SetParent(vsGo.transform, false);
        var vsTxtRT = vsTxt.GetComponent<RectTransform>();
        vsTxtRT.anchorMin = Vector2.zero; vsTxtRT.anchorMax = Vector2.one;
        vsTxtRT.offsetMin = Vector2.zero; vsTxtRT.offsetMax = Vector2.zero;
        var vsT = vsTxt.GetComponent<Text>();
        vsT.font = F(); vsT.text = "VS"; vsT.fontSize = 26; vsT.color = new Color(0.9f, 0.5f, 0.4f);
        vsT.alignment = TextAnchor.MiddleCenter;
        vsT.fontStyle = FontStyle.Bold;
        vsT.raycastTarget = false;
        
        // VS文字发光 (更清晰的描边)
        var vsTxtOutline = vsTxt.GetComponent<Outline>();
        vsTxtOutline.effectColor = new Color(0.8f, 0.2f, 0.15f, 0.9f);
        vsTxtOutline.effectDistance = new Vector2(1.5f, 1.5f);
        
        // 添加Shadow增加立体感
        var vsTxtShadow = vsTxt.AddComponent<Shadow>();
        vsTxtShadow.effectColor = new Color(0, 0, 0, 0.8f);
        vsTxtShadow.effectDistance = new Vector2(1, -1);

        // -- Enemy card (right 48%) --
        var ecGo = new GameObject("ECard", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        ecGo.transform.SetParent(cardsRT, false);
        ecGo.GetComponent<Image>().color = new Color(0.10f, 0.07f, 0.12f);
        
        // 敌人卡片阴影
        var ecShadow = ecGo.GetComponent<Shadow>();
        ecShadow.effectColor = new Color(0, 0, 0, 0.5f);
        ecShadow.effectDistance = new Vector2(3, -3);
        
        var ecOutline = ecGo.GetComponent<Outline>();
        ecOutline.effectColor = new Color(0.8f, 0.15f, 0.4f, 0.3f);
        ecOutline.effectDistance = new Vector2(2, 2);
        _cECardOutline = ecOutline;
        var ecRT = ecGo.GetComponent<RectTransform>();
        ecRT.anchorMin = new Vector2(0.54f, 0); ecRT.anchorMax = new Vector2(1, 1);
        ecRT.offsetMin = new Vector2(3, 6); ecRT.offsetMax = new Vector2(-6, -6);
        
        // 内边框装饰
        var ecInner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
        ecInner.transform.SetParent(ecRT, false);
        var ecInnerRT = ecInner.GetComponent<RectTransform>();
        ecInnerRT.anchorMin = new Vector2(0, 0); ecInnerRT.anchorMax = new Vector2(1, 1);
        ecInnerRT.offsetMin = new Vector2(2, 2); ecInnerRT.offsetMax = new Vector2(-2, -2);
        ecInner.GetComponent<Image>().color = new Color(0.45f, 0.18f, 0.3f, 0.12f);

        TxtAnchored(ecRT, "目标", 14, Mag, new Vector2(0,0.88f), new Vector2(1,1), 4, 0);
        _cEIcon = ImgAnchored(ecRT, new Vector2(0.15f,0.45f), new Vector2(0.85f,0.88f));
        _cEName = TxtAnchored(ecRT, "", 14, Gold, new Vector2(0,0.32f), new Vector2(1,0.45f), 4, 4);
        _cEHpFill = BarAnchored(ecRT, new Color(0.9f,0.15f,0.15f), new Vector2(0.05f,0.22f), new Vector2(0.95f,0.32f), out _cEHpTxt);
        _cEStat = TxtAnchored(ecRT, "", 14, new Color(1,0.6f,0.6f), new Vector2(0,0.12f), new Vector2(1,0.22f), 4, 4);
        // 敌人特性按钮 (可点击查看描述)
        var traitBtnGo = new GameObject("TraitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        traitBtnGo.transform.SetParent(ecRT, false);
        var traitBtnRT = traitBtnGo.GetComponent<RectTransform>();
        traitBtnRT.anchorMin = new Vector2(0, 0);
        traitBtnRT.anchorMax = new Vector2(1, 0.14f);
        traitBtnRT.offsetMin = new Vector2(3, 2);
        traitBtnRT.offsetMax = new Vector2(-3, -1);
        traitBtnGo.GetComponent<Image>().color = new Color(0.2f, 0.12f, 0.3f, 0.85f);
        var traitOL = traitBtnGo.GetComponent<Outline>();
        traitOL.effectColor = new Color(0.6f, 0.4f, 0.9f, 0.5f);
        traitOL.effectDistance = new Vector2(1, 1);
        var traitBtn = traitBtnGo.GetComponent<Button>();

        _cETraits = TxtAnchored(traitBtnRT, "", 12, Mag, new Vector2(0,0), new Vector2(0.78f,1), 6, 2);
        _cETraits.alignment = TextAnchor.MiddleLeft;
        _cETraits.fontStyle = FontStyle.Bold;

        // "查看" label
        var viewLabel = TxtAnchored(traitBtnRT, "[查看]", 11, Cyan, new Vector2(0.78f, 0), new Vector2(1, 1), 2, 2);
        viewLabel.alignment = TextAnchor.MiddleCenter;

        var btnImage = traitBtnGo.GetComponent<Image>();
        Color normalColor = btnImage.color;
        Color hoverColor = new Color(0.3f, 0.18f, 0.45f, 0.95f);
        var eventTrigger = traitBtnGo.AddComponent<EventTrigger>();

        var enterEvent = new EventTrigger.Entry();
        enterEvent.eventID = EventTriggerType.PointerEnter;
        enterEvent.callback.AddListener((data) => { btnImage.color = hoverColor; });
        eventTrigger.triggers.Add(enterEvent);

        var exitEvent = new EventTrigger.Entry();
        exitEvent.eventID = EventTriggerType.PointerExit;
        exitEvent.callback.AddListener((data) => { btnImage.color = normalColor; });
        eventTrigger.triggers.Add(exitEvent);

        traitBtn.onClick.AddListener(() => ShowEnemyTraitInfo());

        // === Possess rate bar (8%) ===
        // Separator: Cards → Possess
        var cardsSep = new GameObject("CardsSep", typeof(RectTransform), typeof(Image));
        cardsSep.transform.SetParent(rt, false);
        var cardsSepRT = cardsSep.GetComponent<RectTransform>();
        cardsSepRT.anchorMin = new Vector2(0.03f, 0.479f); cardsSepRT.anchorMax = new Vector2(0.97f, 0.481f);
        cardsSepRT.offsetMin = Vector2.zero; cardsSepRT.offsetMax = Vector2.zero;
        cardsSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.12f);

        var possGo = new GameObject("PossBar", typeof(RectTransform), typeof(Image), typeof(Outline));
        possGo.transform.SetParent(rt, false);
        possGo.GetComponent<Image>().color = new Color(0.08f,0.06f,0.15f,0.95f);
        possGo.GetComponent<Outline>().effectColor = new Color(0.5f,0.25f,0.8f,0.15f);
        possGo.GetComponent<Outline>().effectDistance = new Vector2(1,1);
        var possRT = possGo.GetComponent<RectTransform>();
        possRT.anchorMin = new Vector2(0, 0.40f);
        possRT.anchorMax = new Vector2(1, 0.48f);
        possRT.offsetMin = new Vector2(10, 2); possRT.offsetMax = new Vector2(-10, -2);

        _cRate = TxtAnchored(possRT, "", 18, Purp, new Vector2(0,0), new Vector2(0.3f,1), 8, 0);
        _cRate.alignment = TextAnchor.MiddleLeft;
        _cPossLabel = TxtAnchored(possRT, "附身成功率", 14, Dim, new Vector2(0.3f,0.5f), new Vector2(1,1), 0, 8);
        _cPossLabel.alignment = TextAnchor.MiddleLeft;
        _cPossFill = BarAnchored(possRT, Purp, new Vector2(0.3f,0.1f), new Vector2(0.95f,0.45f), out var _);

        // === Battle log (12%) - multi-line display ===
        var logGo = new GameObject("Log", typeof(RectTransform), typeof(Image));
        logGo.transform.SetParent(rt, false);
        logGo.GetComponent<Image>().color = new Color(0.10f,0.08f,0.18f,0.9f);
        var logRT = logGo.GetComponent<RectTransform>();
        logRT.anchorMin = new Vector2(0, 0.28f);
        logRT.anchorMax = new Vector2(1, 0.40f);
        logRT.offsetMin = new Vector2(10, 2); logRT.offsetMax = new Vector2(-10, -2);

        var logTxtGo = new GameObject("LogTxt", typeof(RectTransform), typeof(Text), typeof(Outline));
        logTxtGo.transform.SetParent(logRT, false);
        var logTxtRT = logTxtGo.GetComponent<RectTransform>();
        logTxtRT.anchorMin = Vector2.zero; logTxtRT.anchorMax = Vector2.one;
        logTxtRT.offsetMin = new Vector2(8, 8); logTxtRT.offsetMax = new Vector2(-8, -8);
        _cMsg = logTxtGo.GetComponent<Text>();
        _cMsg.font = F(); _cMsg.fontSize = 12; _cMsg.color = new Color(0.9f, 0.82f, 0.5f);
        _cMsg.alignment = TextAnchor.UpperLeft;
        _cMsg.fontStyle = FontStyle.Normal;
        _cMsg.lineSpacing = 1.15f;
        _cMsg.raycastTarget = false;
        var logOutline = logTxtGo.GetComponent<Outline>();
        logOutline.effectColor = new Color(0, 0, 0, 0.8f);
        logOutline.effectDistance = new Vector2(1, 1);

        // === Bottom HUD bar (5%) ===
        var hudGo = new GameObject("BtmHud", typeof(RectTransform), typeof(Image));
        hudGo.transform.SetParent(rt, false);
        hudGo.GetComponent<Image>().color = new Color(0.08f,0.06f,0.15f);
        var hudRT = hudGo.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 0.27f);
        hudRT.anchorMax = new Vector2(1, 0.32f);
        hudRT.offsetMin = Vector2.zero; hudRT.offsetMax = Vector2.zero;

        _cBtmHud = TxtAnchored(hudRT, "", 11, Bright, new Vector2(0,0), new Vector2(1,1), 10, 10);
        _cBtmHud.alignment = TextAnchor.MiddleLeft;

        // === Action area (bottom 27%) ===
        // Separator: HUD → Actions
        var combatActSep = new GameObject("CombatActSep", typeof(RectTransform), typeof(Image));
        combatActSep.transform.SetParent(rt, false);
        var combatActSepRT = combatActSep.GetComponent<RectTransform>();
        combatActSepRT.anchorMin = new Vector2(0.03f, 0.269f); combatActSepRT.anchorMax = new Vector2(0.97f, 0.271f);
        combatActSepRT.offsetMin = Vector2.zero; combatActSepRT.offsetMax = Vector2.zero;
        combatActSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.12f);

        var actGo = new GameObject("Actions", typeof(RectTransform), typeof(Image));
        actGo.transform.SetParent(rt, false);
        actGo.GetComponent<Image>().color = new Color(0.08f,0.06f,0.15f);
        var actRT = actGo.GetComponent<RectTransform>();
        actRT.anchorMin = new Vector2(0, 0);
        actRT.anchorMax = new Vector2(1, 0.27f);
        actRT.offsetMin = Vector2.zero; actRT.offsetMax = Vector2.zero;

        // D-pad removed - not needed in combat

        // 敌人特性描述弹窗
        _traitInfoOvl = Panel("TraitInfo", new Color(0.06f, 0.04f, 0.10f, 0.92f));
        _traitInfoOvl.SetActive(false);
        var tiRT = _traitInfoOvl.GetComponent<RectTransform>();

        var tiContainer = new GameObject("Container", typeof(RectTransform), typeof(Image), typeof(Outline));
        tiContainer.transform.SetParent(tiRT, false);
        tiContainer.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.16f);
        var tiOl = tiContainer.GetComponent<Outline>();
        tiOl.effectColor = new Color(0.5f, 0.3f, 0.9f, 0.7f);
        tiOl.effectDistance = new Vector2(2, 2);
        var tiCRT = tiContainer.GetComponent<RectTransform>();
        tiCRT.anchorMin = new Vector2(0.12f, 0.32f);
        tiCRT.anchorMax = new Vector2(0.88f, 0.68f);
        tiCRT.offsetMin = Vector2.zero; tiCRT.offsetMax = Vector2.zero;

        var tiVL = AddVL(tiContainer, 14, 12);
        Spacer(tiVL, 10);
        var tiHeader = Txt(tiVL, "怪物技能", 12, new Color(0.6f, 0.55f, 0.7f), 16).GetComponent<Text>();
        Spacer(tiVL, 4);
        _traitInfoTitle = Txt(tiVL, "", 22, Gold, 32).GetComponent<Text>();
        _traitInfoTitle.fontStyle = FontStyle.Bold;
        Spacer(tiVL, 8);

        var tiSep = new GameObject("TiSep", typeof(RectTransform), typeof(Image));
        tiSep.transform.SetParent(tiVL.transform, false);
        tiSep.AddComponent<LayoutElement>().preferredHeight = 1;
        tiSep.GetComponent<Image>().color = new Color(0.3f, 0.25f, 0.45f, 0.5f);

        Spacer(tiVL, 8);
        _traitInfoDesc = Txt(tiVL, "", 15, Bright, 50).GetComponent<Text>();
        _traitInfoDesc.lineSpacing = 1.2f;
        Spacer(tiVL, 12);
        var closeBtn = BtnGo(tiVL.transform, "关闭", 16, Cyan, 48);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => {
            _traitInfoOvl.SetActive(false);
        });

        // Action buttons layout
        // Floor 1: Attack | Possess | Ultimate (single row)
        // Floor 2+: Attack | Possess (top row, justified), Defend | Flee (bottom row, justified), Ultimate (right, 2 rows height)
        
        float btnWidth = 110;
        float btnHeight = 50;
        float spacing = 12;
        float row1Y = 56;
        float row2Y = -2;
        
        // First row (top): Attack | Possess (left-aligned)
        _cAtkBtn = ActBtn(actGo.transform, "攻击", new Color(0.85f,0.45f,0.25f),
            new Vector2(-btnWidth - spacing/2, row1Y), new Vector2(btnWidth, btnHeight), null);
        var atkLabel = _cAtkBtn.GetComponentInChildren<Text>();
        var atkHold = _cAtkBtn.gameObject.AddComponent<HoldButton>();
        atkHold.onTrigger = () => CompleteGameSystem.Instance?.PlayerAttack();
        atkHold.firstDelay = 1f;
        atkHold.repeatInterval = 0.3f;
        atkHold.onStart = () => { atkLabel.text = "自动攻击"; };
        atkHold.onEnd = () => { atkLabel.text = "攻击"; };
        _cAtkBtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.PlayerAttack());

        _cPossBtn = ActBtn(actGo.transform, "附身", Purp,
            new Vector2(0, row1Y), new Vector2(btnWidth, btnHeight), () => ShowPossessPanel());

        // Ultimate - positioned on the right
        // In Floor 1: single row height
        // In Floor 2+: spans 2 rows
        _cUltBtn = ActBtn(actGo.transform, "终极技", new Color(0.9f,0.7f,0.25f),
            new Vector2(btnWidth + spacing/2, row1Y - btnHeight/2), new Vector2(btnWidth, btnHeight * 2 - 4), () => CompleteGameSystem.Instance?.UseUltimate());
        _cUltLabel = _cUltBtn.GetComponentInChildren<Text>();

        // Second row (bottom): Defend | Flee (justified)
        _cDefBtn = ActBtn(actGo.transform, "防御", Cyan,
            new Vector2(-btnWidth - spacing/2, row2Y), new Vector2(btnWidth, btnHeight), () => CompleteGameSystem.Instance?.PlayerDefend());

        _cFleeBtn = ActBtn(actGo.transform, "逃跑", Dim,
            new Vector2(0, row2Y), new Vector2(btnWidth, btnHeight), () => CompleteGameSystem.Instance?.PlayerFlee());

        // Active skill bar (below action buttons)
        var skillBar = new GameObject("SkillBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        skillBar.transform.SetParent(actGo.transform, false);
        var sbRT = skillBar.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0, 0.22f);
        sbRT.anchorMax = new Vector2(1, 0.42f);
        sbRT.offsetMin = new Vector2(4, 0);
        sbRT.offsetMax = new Vector2(-4, 0);
        var sbHL = skillBar.GetComponent<HorizontalLayoutGroup>();
        sbHL.spacing = 6; sbHL.childAlignment = TextAnchor.MiddleCenter;
        sbHL.childForceExpandWidth = true; sbHL.childForceExpandHeight = true;
        _skillBarRow = skillBar.transform;
        for (int si = 0; si < 3; si++)
        {
            int idx = si;
            var sb = BtnGo(skillBar.transform, "", 11, Cyan, 0);
            var sbBtnRT = sb.GetComponent<RectTransform>();
            sbBtnRT.sizeDelta = new Vector2(0, 0);
            sb.GetComponent<Image>().color = new Color(0.06f, 0.15f, 0.2f, 0.95f);
            sb.AddComponent<Outline>().effectColor = new Color(0f, 1f, 0.816f, 0.5f);
            _skillBarBtns[si] = sb.GetComponent<Button>();
            _skillBarBtns[si].onClick.AddListener(() => CompleteGameSystem.Instance?.UseActiveSkill(idx));
            var sbVL = AddVL(sb, 2, 1);
            _skillBarLabels[si] = Txt(sbVL, "", 12, Gold, 16).GetComponent<Text>();
            _skillBarLabels[si].alignment = TextAnchor.MiddleCenter;
            _skillBarLabels[si].fontStyle = FontStyle.Bold;
            _skillBarUses[si] = Txt(sbVL, "", 10, Cyan, 14).GetComponent<Text>();
            _skillBarUses[si].alignment = TextAnchor.MiddleCenter;
            sb.SetActive(false);
        }

        // Form switch row at very bottom
        var fr = new GameObject("Forms", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        fr.transform.SetParent(actGo.transform, false);
        var frRT = fr.GetComponent<RectTransform>();
        frRT.anchorMin = new Vector2(0, 0.02f);
        frRT.anchorMax = new Vector2(1, 0.20f);
        frRT.offsetMin = new Vector2(4, 0);
        frRT.offsetMax = new Vector2(-4, 0);
        fr.GetComponent<HorizontalLayoutGroup>().spacing = 4;
        fr.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        fr.GetComponent<HorizontalLayoutGroup>().childForceExpandWidth = true;
        fr.GetComponent<HorizontalLayoutGroup>().childForceExpandHeight = true;
        _cFormRow = fr.transform;

        // === Host traits bar (overlay on top of cards area, only shown when possessed) ===
        _cHostBar = new GameObject("HostBar", typeof(RectTransform), typeof(Image), typeof(Outline));
        _cHostBar.transform.SetParent(rt, false);
        _cHostBar.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.95f);
        var hostOL = _cHostBar.GetComponent<Outline>();
        hostOL.effectColor = new Color(0f, 1f, 0.816f, 0.35f);
        hostOL.effectDistance = new Vector2(1, 1);
        var hostRT = _cHostBar.GetComponent<RectTransform>();
        hostRT.anchorMin = new Vector2(0, 0.845f);
        hostRT.anchorMax = new Vector2(1, 0.885f);
        hostRT.offsetMin = new Vector2(8, 0); hostRT.offsetMax = new Vector2(-8, 0);

        _cHostName = TxtAnchored(hostRT, "", 12, Cyan, new Vector2(0, 0), new Vector2(0.38f, 1), 8, 0);
        _cHostName.alignment = TextAnchor.MiddleLeft;
        _cHostName.fontStyle = FontStyle.Bold;

        _cHostTraits = TxtAnchored(hostRT, "", 11, new Color(0.85f, 0.75f, 1f), new Vector2(0.38f, 0), new Vector2(0.84f, 1), 4, 0);
        _cHostTraits.alignment = TextAnchor.MiddleLeft;

        var hostInfoBtn = new GameObject("HostInfoBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        hostInfoBtn.transform.SetParent(hostRT, false);
        var hiBtnRT = hostInfoBtn.GetComponent<RectTransform>();
        hiBtnRT.anchorMin = new Vector2(0.85f, 0.1f);
        hiBtnRT.anchorMax = new Vector2(0.99f, 0.9f);
        hiBtnRT.offsetMin = Vector2.zero; hiBtnRT.offsetMax = Vector2.zero;
        hostInfoBtn.GetComponent<Image>().color = new Color(0.2f, 0.12f, 0.3f, 0.9f);
        hostInfoBtn.AddComponent<Outline>().effectColor = new Color(1f, 0.75f, 0.3f, 0.6f);
        var hiBtnTxt = TxtAnchored(hiBtnRT, "!", 15, Gold, Vector2.zero, Vector2.one, 0, 0);
        hiBtnTxt.alignment = TextAnchor.MiddleCenter;
        hiBtnTxt.fontStyle = FontStyle.Bold;
        hostInfoBtn.GetComponent<Button>().targetGraphic = hostInfoBtn.GetComponent<Image>();
        hostInfoBtn.GetComponent<Button>().onClick.AddListener(() => ShowHostTraitInfo());

        _cHostBar.SetActive(false);
    }

    // ========== GAME OVER ==========
    // Report refs
    Text _rptTitle, _rptSubtitle, _rptRank, _rptFloor, _rptPossess, _rptKills;
    Text _rptTime, _rptHost, _rptPollution, _rptForm, _rptScore, _rptEcho, _rptQuote;
    bool _rptBuilt;

    void BuildOver()
    {
        _overPanel = Panel("Over", new Color(0.05f, 0.03f, 0.10f));
        var rt = _overPanel.GetComponent<RectTransform>();

        // 背景图
        var bgTex = Resources.Load<Texture2D>("UI/death_report_bg");
        if (bgTex != null)
        {
            var bgGo = new GameObject("BgImg", typeof(RectTransform), typeof(RawImage));
            bgGo.transform.SetParent(rt, false);
            Stretch(bgGo);
            var bgRI = bgGo.GetComponent<RawImage>();
            bgRI.texture = bgTex;
            bgRI.color = new Color(1f, 1f, 1f, 0.7f);
            bgRI.raycastTarget = false;
        }

        // 顶部渐变遮罩
        var gradTop = new GameObject("GradTop", typeof(RectTransform), typeof(RawImage));
        gradTop.transform.SetParent(rt, false);
        var gtRI = gradTop.GetComponent<RawImage>();
        gtRI.texture = MakeGradientTex(64, new Color(0.05f, 0.03f, 0.10f, 0.9f), new Color(0, 0, 0, 0));
        gtRI.raycastTarget = false;
        var gtRT = gradTop.GetComponent<RectTransform>();
        gtRT.anchorMin = new Vector2(0, 0.75f); gtRT.anchorMax = Vector2.one;
        gtRT.offsetMin = Vector2.zero; gtRT.offsetMax = Vector2.zero;

        // 底部渐变遮罩
        var gradBot = new GameObject("GradBot", typeof(RectTransform), typeof(RawImage));
        gradBot.transform.SetParent(rt, false);
        var gbRI = gradBot.GetComponent<RawImage>();
        gbRI.texture = MakeGradientTex(64, new Color(0, 0, 0, 0), new Color(0.05f, 0.03f, 0.10f, 0.95f));
        gbRI.raycastTarget = false;
        var gbRT = gradBot.GetComponent<RectTransform>();
        gbRT.anchorMin = Vector2.zero; gbRT.anchorMax = new Vector2(1, 0.3f);
        gbRT.offsetMin = Vector2.zero; gbRT.offsetMax = Vector2.zero;

        // 内容滚动区
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(rt, false);
        Stretch(scrollGo);
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        var vpGo = new GameObject("VP", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(scrollGo.transform, false);
        Stretch(vpGo);
        vpGo.GetComponent<Image>().color = new Color(1,1,1,0.003f);
        vpGo.GetComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vpGo.GetComponent<RectTransform>();

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRT = contentGo.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1); contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.sizeDelta = Vector2.zero;
        var vlg = contentGo.GetComponent<VerticalLayoutGroup>();
        vlg.padding = new RectOffset(24, 24, 0, 30);
        vlg.spacing = 5;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRT;

        var vl = vlg;

        Spacer(vl, 50);

        // 英文副标题
        _rptSubtitle = Txt(vl, "", 11, new Color(0.5f, 0.8f, 0.8f), 18).GetComponent<Text>();
        _rptSubtitle.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 4);

        // 大标题
        _rptTitle = Txt(vl, "", 30, Mag, 44).GetComponent<Text>();
        _rptTitle.fontStyle = FontStyle.Bold;
        _rptTitle.alignment = TextAnchor.MiddleCenter;
        _rptTitle.GetComponent<Outline>().effectColor = new Color(0.5f, 0.1f, 0.3f, 0.6f);
        _rptTitle.GetComponent<Outline>().effectDistance = new Vector2(2f, 2f);

        // 评级
        _rptRank = Txt(vl, "", 40, Gold, 55).GetComponent<Text>();
        _rptRank.fontStyle = FontStyle.Bold;
        _rptRank.alignment = TextAnchor.MiddleCenter;
        _rptRank.GetComponent<Outline>().effectColor = new Color(0.8f, 0.6f, 0.1f, 0.6f);
        _rptRank.GetComponent<Outline>().effectDistance = new Vector2(2f, 2f);

        Spacer(vl, 8);

        // 分隔线
        var sep1 = new GameObject("Sep1", typeof(RectTransform), typeof(Image));
        sep1.transform.SetParent(vl.transform, false);
        sep1.AddComponent<LayoutElement>().preferredHeight = 1;
        sep1.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.25f);

        Spacer(vl, 8);

        // 数据行
        _rptFloor = MakeReportRow(vl, "到达层数");
        _rptPossess = MakeReportRow(vl, "附身次数");
        _rptKills = MakeReportRow(vl, "击杀数");
        _rptTime = MakeReportRow(vl, "存活时长");
        _rptHost = MakeReportRow(vl, "最久宿主");
        _rptPollution = MakeReportRow(vl, "最高污染");
        _rptForm = MakeReportRow(vl, "最终形态");

        Spacer(vl, 8);

        var sep2 = new GameObject("Sep2", typeof(RectTransform), typeof(Image));
        sep2.transform.SetParent(vl.transform, false);
        sep2.AddComponent<LayoutElement>().preferredHeight = 1;
        sep2.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);

        Spacer(vl, 10);

        // 分数
        _rptScore = Txt(vl, "", 22, Cyan, 32).GetComponent<Text>();
        _rptScore.fontStyle = FontStyle.Bold;
        _rptScore.alignment = TextAnchor.MiddleCenter;
        _rptScore.GetComponent<Outline>().effectColor = new Color(0f, 0.6f, 0.6f, 0.5f);
        _rptScore.GetComponent<Outline>().effectDistance = new Vector2(1f, 1f);

        _rptEcho = Txt(vl, "", 13, Purp, 20).GetComponent<Text>();
        _rptEcho.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 10);

        // 引言
        _rptQuote = Txt(vl, "", 12, new Color(0.5f, 0.5f, 0.6f), 30).GetComponent<Text>();
        _rptQuote.alignment = TextAnchor.MiddleCenter;
        _rptQuote.fontStyle = FontStyle.Italic;

        Spacer(vl, 20);

        // 按钮行
        var row = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(vl.transform, false);
        row.AddComponent<LayoutElement>().preferredHeight = 55;
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 12; hl.childAlignment = TextAnchor.MiddleCenter;
        hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;

        var retryBtn = BtnGo(row.transform, "再来一次", 16, Cyan, 50);
        retryBtn.GetComponent<Image>().color = new Color(0.04f, 0.12f, 0.1f, 0.9f);
        retryBtn.GetComponent<Outline>().effectColor = new Color(0f, 1f, 0.816f, 0.5f);
        retryBtn.AddComponent<ButtonPressFeedback>();
        retryBtn.GetComponent<Button>().onClick.AddListener(() => {
            _rptBuilt = false;
            string currentMode = GameManager.Instance?.CurrentMode.ToString() ?? "Classic";
            GameManager.Instance?.StartNewGame(currentMode);
        });

        var homeBtn = BtnGo(row.transform, "返回主页", 14, Dim, 50);
        homeBtn.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.1f, 0.85f);
        homeBtn.GetComponent<Outline>().effectColor = new Color(0.4f, 0.4f, 0.5f, 0.3f);
        homeBtn.AddComponent<ButtonPressFeedback>();
        homeBtn.GetComponent<Button>().onClick.AddListener(() => {
            _rptBuilt = false;
            CompleteGameSystem.Instance?.ReturnToMenu();
        });

        Spacer(vl, 30);
    }

    Text MakeReportRow(VerticalLayoutGroup vl, string label)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(vl.transform, false);
        row.AddComponent<LayoutElement>().preferredHeight = 32;
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.childAlignment = TextAnchor.MiddleCenter;
        hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;

        var lbl = new GameObject("L", typeof(RectTransform), typeof(Text));
        lbl.transform.SetParent(row.transform, false);
        var lt = lbl.GetComponent<Text>();
        lt.font = F(); lt.text = label; lt.fontSize = 13; lt.color = Bright;
        lt.alignment = TextAnchor.MiddleLeft;

        var val = new GameObject("V", typeof(RectTransform), typeof(Text));
        val.transform.SetParent(row.transform, false);
        var vt = val.GetComponent<Text>();
        vt.font = F(); vt.fontSize = 15; vt.color = Cyan;
        vt.alignment = TextAnchor.MiddleRight;

        return vt;
    }

    // ========== ENDING ==========
    void BuildEnd()
    {
        _endPanel = Panel("End", new Color(0.06f,0.04f,0.10f));
        var vl = AddVL(_endPanel, 25, 8);
        Spacer(vl, 60);
        _enTitle = Txt(vl, "", 30, Gold, 45).GetComponent<Text>();
        _enSub = Txt(vl, "", 14, Purp, 25).GetComponent<Text>();
        Spacer(vl, 15);
        _enBody = Txt(vl, "", 13, Bright, 500).GetComponent<Text>();
        _enBody.alignment = TextAnchor.UpperCenter;
        Spacer(vl, 20);
        var btn = BtnGo(vl.transform, "再来一次", 18, Cyan, 55);
        btn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ReturnToMenu());
    }

    // ========== ALTAR ==========
    void BuildAltar()
    {
        _altarOvl = Panel("Altar", new Color(0.06f, 0.04f, 0.10f, 0.88f));
        var vl = AddVL(_altarOvl, 20, 10);
        Spacer(vl, 200);
        Txt(vl, "— 祭坛抉择 —", 22, Gold, 35);
        Spacer(vl, 10);

        // Two cards side by side
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        row.transform.SetParent(vl.transform, false);
        row.GetComponent<RectTransform>().sizeDelta = new Vector2(0, 200);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 10; hl.childForceExpandWidth = true; hl.childForceExpandHeight = true;
        hl.padding = new RectOffset(5,5,0,0);
        row.AddComponent<LayoutElement>().preferredHeight = 200;

        var aggCard = CardGo(row.transform, new Color(0.2f,0.06f,0.06f,0.9f), 200);
        var avl = AddVL(aggCard, 8, 4);
        _alAggN = Txt(avl, "", 15, new Color(0.9f,0.12f,0.1f), 24).GetComponent<Text>();
        _alAggD = Txt(avl, "", 11, Bright, 80).GetComponent<Text>();
        _alAggD.alignment = TextAnchor.UpperCenter;
        var abtn = BtnGo(avl.transform, "选择激进", 13, new Color(0.9f,0.12f,0.1f), 38);
        abtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ChooseAltar(true));

        var conCard = CardGo(row.transform, new Color(0.06f,0.06f,0.2f,0.9f), 200);
        var cvl = AddVL(conCard, 8, 4);
        _alConN = Txt(cvl, "", 15, Cyan, 24).GetComponent<Text>();
        _alConD = Txt(cvl, "", 11, Bright, 80).GetComponent<Text>();
        _alConD.alignment = TextAnchor.UpperCenter;
        var cbtn = BtnGo(cvl.transform, "选择保守", 13, Cyan, 38);
        cbtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ChooseAltar(false));

        _altarOvl.SetActive(false);
    }

    void BuildCollapse()
    {
        _collapseOvl = Panel("Collapse", new Color(0.15f,0,0,0.8f));
        var vl = AddVL(_collapseOvl, 25, 10);
        Spacer(vl, 220);
        Txt(vl, "⚠ 污染崩溃 ⚠", 26, new Color(0.9f,0.12f,0.1f), 40);
        Txt(vl, "选择你的命运", 13, Dim, 22);
        Spacer(vl, 15);
        for (int i = 0; i < PollutionPassiveData.CollapseOptions.Length; i++)
        {
            int idx = i;
            var btn = BtnGo(vl.transform, PollutionPassiveData.CollapseOptions[i], 13, Purp, 52);
            btn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ResolveCollapse(idx));
        }
        _collapseOvl.SetActive(false);
    }

    // Possess refs
    Text _possTitle, _possRate, _possEnemyName;
    Text[] _possOptLabels = new Text[4];
    Text[] _possOptDescs = new Text[4];
    GameObject[] _possOptBtns = new GameObject[4];

    void BuildPossessPanel()
    {
        _possessOvl = Panel("PossessOvl", new Color(0.06f,0.04f,0.10f,0.88f));
        var rt = _possessOvl.GetComponent<RectTransform>();

        // Main card
        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(rt, false);
        card.GetComponent<Image>().color = new Color(0.08f,0.05f,0.18f,0.96f);
        card.GetComponent<Outline>().effectColor = new Color(0.71f,0.33f,1f,0.4f);
        card.GetComponent<Outline>().effectDistance = new Vector2(2,2);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0.04f, 0.15f);
        cardRT.anchorMax = new Vector2(0.96f, 0.85f);
        cardRT.offsetMin = Vector2.zero; cardRT.offsetMax = Vector2.zero;

        var cardVL = AddVL(card, 12, 6);

        // Title
        Txt(cardVL, "【意识交涉】", 28, Purp, 40);
        _possEnemyName = Txt(cardVL, "", 20, Gold, 30).GetComponent<Text>();
        _possRate = Txt(cardVL, "", 22, Purp, 32).GetComponent<Text>();

        Spacer(cardVL, 10);

        // 4 strategy cards
        string[] defaultLabels = {"威压","交易","共鸣","加入"};
        string[] defaultDescs = {
            "成功率-20%\n成功后满血继承",
            "成功率+20%\n污染+15",
            "成功率+10%\n进化点+2",
            "100%成功\n污染+20 (需60%+污染)"
        };
        Color[] optColors = { new Color(0.9f,0.4f,0.2f), Gold, Cyan, Mag };

        for (int i = 0; i < 4; i++)
        {
            var optGo = new GameObject($"Opt{i}", typeof(RectTransform), typeof(Image), typeof(Button));
            optGo.transform.SetParent(cardVL.transform, false);
            optGo.GetComponent<Image>().color = new Color(0.08f,0.05f,0.14f,0.95f);
            optGo.AddComponent<LayoutElement>().preferredHeight = 85;

            var optVL = AddVL(optGo, 10, 2);

            var lbl = Txt(optVL, defaultLabels[i], 24, optColors[i], 32);
            var lblText = lbl.GetComponent<Text>();
            lblText.fontStyle = FontStyle.Bold;
            _possOptLabels[i] = lblText;

            var desc = Txt(optVL, defaultDescs[i], 16, Dim, 40);
            var descText = desc.GetComponent<Text>();
            descText.alignment = TextAnchor.UpperCenter;
            descText.color = new Color(0.85f, 0.8f, 0.9f);
            _possOptDescs[i] = descText;

            var btn = optGo.GetComponent<Button>();
            btn.targetGraphic = optGo.GetComponent<Image>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.10f,0.08f,0.18f,0.9f);
            colors.highlightedColor = new Color(0.12f,0.08f,0.2f);
            colors.pressedColor = new Color(0.06f,0.04f,0.12f);
            btn.colors = colors;

            int idx = i;
            string[] optionIds = {"intimidate","trade","resonance","join"};
            btn.onClick.AddListener(() => {
                string oid = optionIds[idx];
                _possessOvl.SetActive(false);
                CompleteGameSystem.Instance?.TryPossess(oid);
            });

            _possOptBtns[i] = optGo;
        }

        Spacer(cardVL, 6);

        // Cancel button
        var cancelBtn = BtnGo(cardVL.transform, "✕ 取消", 15, Dim, 45);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => _possessOvl.SetActive(false));

        _possessOvl.SetActive(false);
    }

    void ShowPossessPanel()
    {
        var gs = CompleteGameSystem.Instance;
        if (gs == null || gs.CurrentEnemy == null) return;
        gs.DismissSoftHint("possess");

        // 形态破裂检查 - 无法寄生
        if (GameManager.Instance.Player.formBroken)
        {
            gs.AddCombatLog("💔 形态破裂，无法寄生");
            return;
        }

        // Boss check
        if (gs.CurrentEnemy.isBoss)
        {
            // Direct message, no panel
            gs.TryPossess("resonance");
            return;
        }

        // Update panel content
        var enemy = gs.CurrentEnemy;
        float baseRate = gs.CalculatePossessChance(enemy);

        // 教程看门犬100%
        bool tutDog = enemy.id == "dog" && GameManager.Instance.CurrentFloor == 1;
        if (tutDog) baseRate = 1f;

        ST(_possEnemyName, $"目标: {enemy.name}  HP:{enemy.hp}/{enemy.maxHp}");
        ST(_possRate, $"基础成功率: {Mathf.RoundToInt(baseRate * 100)}%");

        // Update each option with actual rates
        string[] ids = {"intimidate","trade","resonance","join"};
        float[] mods = {-0.2f, 0.2f, 0.1f, 0f};
        bool[] guaranteed = {false, false, false, true};
        string[] labels = {"威压","交易","共鸣","加入"};
        string[] descs = {
            $"成功率 {Mathf.RoundToInt(Mathf.Clamp01(baseRate-0.2f)*100)}%\n成功后满血继承\n失败: 目标ATK+2",
            $"成功率 {Mathf.RoundToInt(Mathf.Clamp01(baseRate+0.2f)*100)}%\n污染+15\n高效但有代价",
            $"成功率 {Mathf.RoundToInt(Mathf.Clamp01(baseRate+0.1f)*100)}%\n进化点+2\n失败也得EP",
            $"成功率 100%\n污染+20\n需60%+污染才出现"
        };

        for (int i = 0; i < 4; i++)
        {
            ST(_possOptLabels[i], labels[i]);
            ST(_possOptDescs[i], descs[i]);
            _possOptDescs[i].color = new Color(0.85f, 0.8f, 0.9f);
            _possOptDescs[i].fontStyle = FontStyle.Normal;
        }

        // 教程看门犬：只显示威压（满血继承，不会只有1点血）
        if (tutDog)
        {
            SA(_possOptBtns[0], true);   // 威压
            SA(_possOptBtns[1], false);  // 交易隐藏
            SA(_possOptBtns[2], false);  // 共鸣隐藏
            SA(_possOptBtns[3], false);  // 加入隐藏
            ST(_possOptDescs[0], "成功率 100%\n满血继承看门犬属性\n教程专属");
        }
        else
        {
            SA(_possOptBtns[0], true);
            SA(_possOptBtns[1], true);
            SA(_possOptBtns[2], true);
            // Hide "join" if pollution < 60
            SA(_possOptBtns[3], GameManager.Instance.Player.pollution >= 60f);
        }

        _possessOvl.SetActive(true);
    }

    // Route selection refs
    Text[] _routeLabels = new Text[3];
    Text[] _routeDescs = new Text[3];

    void BuildRoutePanel()
    {
        _routeOvl = Panel("RouteOvl", new Color(0.06f,0.04f,0.10f,0.88f));
        var vl = AddVL(_routeOvl, 20, 10);
        Spacer(vl, 180);
        Txt(vl, "选择前方路线", 24, Gold, 40);
        Txt(vl, "不同路线将影响本层规则", 12, Dim, 22);
        Spacer(vl, 10);

        for (int i = 0; i < 3; i++)
        {
            var card = CardGo(vl.transform, CardBg, 90);
            var cvl = AddVL(card, 10, 4);
            _routeLabels[i] = Txt(cvl, "", 18, Cyan, 26).GetComponent<Text>();
            _routeDescs[i] = Txt(cvl, "", 12, Bright, 40).GetComponent<Text>();
            _routeDescs[i].alignment = TextAnchor.UpperCenter;

            int idx = i;
            card.AddComponent<Button>().onClick.AddListener(() => {
                CompleteGameSystem.Instance?.SelectRoute(idx);
            });
            card.GetComponent<Button>().targetGraphic = card.GetComponent<Image>();
        }
        _routeOvl.SetActive(false);
    }

    void SyncRoute(CompleteGameSystem gs)
    {
        if (gs.PendingRoutes == null) return;
        Color[] routeColors = { Cyan, Purp, Gold };
        for (int i = 0; i < 3; i++)
        {
            if (i < gs.PendingRoutes.Length)
            {
                ST(_routeLabels[i], gs.PendingRoutes[i]);
                ST(_routeDescs[i], gs.PendingRouteDescs[i]);
                if (_routeLabels[i]) _routeLabels[i].color = routeColors[i % 3];
            }
        }
    }

    // Death form selection refs
    Transform _deathFormList;

    void BuildDeathFormPanel()
    {
        _deathFormOvl = Panel("DeathFormOvl", new Color(0.15f,0,0,0.85f));
        var vl = AddVL(_deathFormOvl, 20, 10);
        Spacer(vl, 200);
        Txt(vl, "⚠ 宿主死亡 ⚠", 26, new Color(0.9f,0.12f,0.1f), 40);
        Txt(vl, "选择一个备用形态继续", 14, Dim, 24);
        Spacer(vl, 10);

        var listGo = new GameObject("FormList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(vl.transform, false);
        listGo.AddComponent<LayoutElement>().preferredHeight = 350;
        var llg = listGo.GetComponent<VerticalLayoutGroup>();
        llg.spacing = 8; llg.padding = new RectOffset(10,10,5,5);
        llg.childAlignment = TextAnchor.UpperCenter;
        llg.childForceExpandWidth = true;
        llg.childForceExpandHeight = false;
        _deathFormList = listGo.transform;

        _deathFormOvl.SetActive(false);
    }

    Text _rollbackInfoTxt;
    Button _rollbackBtn;
    bool _rollbackBuilt;

    void BuildDeathRollbackPanel()
    {
        _deathRollbackOvl = Panel("DeathRollbackOvl", new Color(0.08f,0.02f,0.12f,0.92f));
        var vl = AddVL(_deathRollbackOvl, 20, 12);
        Spacer(vl, 180);
        Txt(vl, "☠ 意识崩坏", 28, new Color(0.9f,0.12f,0.1f), 40);
        Txt(vl, "检测到记忆锚点信号...", 14, Dim, 24);
        Spacer(vl, 20);

        var infoGo = Txt(vl, "", 15, Cyan, 80);
        _rollbackInfoTxt = infoGo.GetComponent<Text>();
        _rollbackInfoTxt.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 20);

        var rollBtn = BtnGo(vl.transform, "⛓ 回滚至锚点", 16, Cyan, 56);
        _rollbackBtn = rollBtn.GetComponent<Button>();
        _rollbackBtn.onClick.AddListener(() => {
            _rollbackBuilt = false;
            CompleteGameSystem.Instance?.ChooseDeathRollback(true);
        });

        Spacer(vl, 10);

        var giveUpBtn = BtnGo(vl.transform, "放弃挣扎...", 14, new Color(0.6f,0.3f,0.3f), 48);
        giveUpBtn.GetComponent<Button>().onClick.AddListener(() => {
            _rollbackBuilt = false;
            CompleteGameSystem.Instance?.ChooseDeathRollback(false);
        });

        _deathRollbackOvl.SetActive(false);
    }

    void SyncDeathRollback(CompleteGameSystem gs)
    {
        if (_rollbackBuilt) return;
        _rollbackBuilt = true;

        var player = GameManager.Instance?.Player;
        if (player == null) return;

        int ep = player.evolutionPoints;
        int cost = gs.DeathRollbackEpCost;
        int anchorFloor = gs.DeathRollbackAnchorFloor;
        bool hasAnchor = anchorFloor > 0;
        bool canAfford = hasAnchor && ep >= cost;

        string info;
        if (hasAnchor)
        {
            info = $"锚点位置: F{anchorFloor}\n";
            info += $"回滚消耗: {cost} EP\n";
            info += canAfford
                ? $"<color=#00ffcc>当前EP: {ep} (足够)</color>"
                : $"<color=#ff006e>当前EP: {ep} (不足)</color>";
        }
        else
        {
            info = "<color=#ff006e>无可用记忆锚点</color>\n";
            info += "在回声祭坛消耗EP可激活锚点";
        }

        _rollbackInfoTxt.text = info;
        _rollbackBtn.interactable = canAfford;
    }

    Transform _formReplaceList;
    bool _formReplaceBuilt;

    void BuildFormReplacePanel()
    {
        _formReplaceOvl = Panel("FormReplaceOvl", new Color(0.05f,0.02f,0.10f,0.90f));
        var vl = AddVL(_formReplaceOvl, 20, 10);
        Spacer(vl, 180);
        Txt(vl, "🔄 卡槽已满", 24, Gold, 36);
        Txt(vl, "选择一个形态替换", 14, Dim, 24);
        Spacer(vl, 10);

        var listGo = new GameObject("ReplaceList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(vl.transform, false);
        listGo.AddComponent<LayoutElement>().preferredHeight = 350;
        var llg = listGo.GetComponent<VerticalLayoutGroup>();
        llg.spacing = 8; llg.padding = new RectOffset(10,10,5,5);
        llg.childAlignment = TextAnchor.UpperCenter;
        llg.childForceExpandWidth = true;
        llg.childForceExpandHeight = false;
        _formReplaceList = listGo.transform;

        Spacer(vl, 10);
        var cancelBtn = BtnGo(vl.transform, "放弃附身", 14, new Color(0.6f,0.3f,0.3f), 48);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => {
            _formReplaceBuilt = false;
            CompleteGameSystem.Instance?.CancelFormReplace();
        });

        _formReplaceOvl.SetActive(false);
    }

    void SyncFormReplace(CompleteGameSystem gs)
    {
        if (_formReplaceBuilt) return;
        _formReplaceBuilt = true;

        for (int i = _formReplaceList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_formReplaceList.GetChild(i).gameObject);

        if (gs.FormReplaceOptions == null) return;

        foreach (var (idx, formId) in gs.FormReplaceOptions)
        {
            string displayName = formId;
            var info = System.Array.Find(GameDataImporter.MonsterDefinitions, m => m.id == formId);
            if (!string.IsNullOrEmpty(info.id))
                displayName = $"{info.name} (HP:{info.hp} ATK:{info.atk} DEF:{info.def})";

            int slotIdx = idx;
            var btn = BtnGo(_formReplaceList, $"替换: {displayName}", 14, Cyan, 56);
            btn.GetComponent<Button>().onClick.AddListener(() => {
                _formReplaceBuilt = false;
                CompleteGameSystem.Instance?.OnFormReplaceSelected(slotIdx);
            });
        }
    }

    bool _deathFormBuilt;

    void SyncDeathForm(CompleteGameSystem gs)
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return;

        if (_deathFormBuilt) return;
        _deathFormBuilt = true;

        for (int i = _deathFormList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_deathFormList.GetChild(i).gameObject);

        for (int i = 0; i < player.ownedForms.Count; i++)
        {
            string fid = player.ownedForms[i];
            if (fid == player.currentFormId) continue;
            bool isDead = i < player.deadForms.Count && player.deadForms[i];
            if (isDead) continue;

            string displayName = fid == "human" ? "基础形态(人类)" : fid;
            var info = Array.Find(GameDataImporter.MonsterDefinitions, m => m.id == fid);
            if (!string.IsNullOrEmpty(info.id))
                displayName = $"{info.name} (HP:{info.hp} ATK:{info.atk} DEF:{info.def})";

            int bondLv = gs.GetBondLevel(fid);
            string bondStr = bondLv > 0 ? $" [{gs.GetBondLevelName(fid)}]" : "";

            string formId = fid;
            var btn = BtnGo(_deathFormList, $"{displayName}{bondStr}", 14, Cyan, 60);
            btn.GetComponent<Button>().onClick.AddListener(() => {
                _deathFormBuilt = false;
                CompleteGameSystem.Instance?.SelectDeathFormByFormId(formId);
            });
        }

        if (_deathFormList.childCount == 0)
        {
            _deathFormBuilt = false;
            gs.SelectDeathForm(-1);
        }
    }

    // Story event refs
    Text _storyTitle, _storyBody;

    // Mini-map refs
    Image[,] _miniMapCells;

    void BuildMiniMap(RectTransform parent)
    {
        var mmGo = new GameObject("MiniMap", typeof(RectTransform), typeof(Image));
        mmGo.transform.SetParent(parent, false);
        mmGo.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.10f, 0.85f);
        var mmRT = mmGo.GetComponent<RectTransform>();
        mmRT.anchorMin = new Vector2(0.78f, 0.78f);
        mmRT.anchorMax = new Vector2(0.98f, 0.87f);
        mmRT.offsetMin = Vector2.zero;
        mmRT.offsetMax = Vector2.zero;

        var grid = mmGo.AddComponent<GridLayoutGroup>();
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 13;
        grid.spacing = Vector2.zero;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.cellSize = new Vector2(4, 4);

        _miniMapCells = new Image[13, 13];
        for (int y = 12; y >= 0; y--)
        {
            for (int x = 0; x < 13; x++)
            {
                var c = new GameObject("m", typeof(RectTransform), typeof(Image));
                c.transform.SetParent(mmGo.transform, false);
                _miniMapCells[y, x] = c.GetComponent<Image>();
                _miniMapCells[y, x].color = new Color(0.05f, 0.03f, 0.08f);
            }
        }
    }

    void SyncMiniMap(CompleteGameSystem gs)
    {
        if (_miniMapCells == null || gs.CurrentFloorState == null) return;
        for (int y = 0; y < 13; y++)
        {
            for (int x = 0; x < 13; x++)
            {
                if (!gs.CurrentFloorState.discovered[y, x])
                {
                    _miniMapCells[y, x].color = new Color(0.05f, 0.03f, 0.08f);
                    continue;
                }
                var pos = new Vector2Int(x, y);
                if (pos == gs.CurrentFloorState.playerPos)
                    _miniMapCells[y, x].color = Cyan;
                else if (pos == gs.CurrentFloorState.exitPos)
                    _miniMapCells[y, x].color = Gold;
                else if (!gs.CurrentFloorState.walkable[y, x])
                    _miniMapCells[y, x].color = new Color(0.12f, 0.08f, 0.18f);
                else if (gs.CurrentFloorState.actions.ContainsKey(pos) && !gs.CurrentFloorState.actions[pos].consumed)
                    _miniMapCells[y, x].color = new Color(0.8f, 0.2f, 0.2f);
                else
                    _miniMapCells[y, x].color = new Color(0.10f, 0.07f, 0.16f);
            }
        }
    }

    // Menu panel
    GameObject _menuOvl;

    Text _solidifyBtnTxt;
    Image _solidifyBtnImg;
    GameObject _evoRedDot;

    bool CanEvolve()
    {
        var gs = CompleteGameSystem.Instance;
        var player = GameManager.Instance?.Player;
        if (gs == null || player == null) return false;
        
        if (!EvolutionData.Trees.TryGetValue(player.selectedClass, out var tree)) return false;
        if (gs.EvolutionLevel >= tree.Count) return false;
        
        var nextNode = tree[gs.EvolutionLevel];
        return player.evolutionPoints >= nextNode.epCost;
    }

    IEnumerator EvoRedDotPulse()
    {
        while (true)
        {
            if (_evoRedDot != null && _evoRedDot.activeSelf)
            {
                var dot = _evoRedDot.transform.Find("Dot")?.GetComponent<Text>();
                var glow = _evoRedDot.transform.Find("Glow")?.GetComponent<Text>();
                
                if (dot == null)
                {
                    yield return new WaitForSeconds(0.1f);
                    continue;
                }
                
                // 渐亮
                for (float t = 0; t < 0.5f; t += Time.deltaTime)
                {
                    float alpha = Mathf.Lerp(0.6f, 1f, t / 0.5f);
                    dot.color = new Color(1f, 0.2f, 0.2f, alpha);
                    if (glow != null) glow.color = new Color(1f, 0.2f, 0.2f, alpha * 0.5f);
                    yield return null;
                }
                
                // 渐暗
                for (float t = 0; t < 0.5f; t += Time.deltaTime)
                {
                    float alpha = Mathf.Lerp(1f, 0.6f, t / 0.5f);
                    dot.color = new Color(1f, 0.2f, 0.2f, alpha);
                    if (glow != null) glow.color = new Color(1f, 0.2f, 0.2f, alpha * 0.5f);
                    yield return null;
                }
            }
            else
            {
                yield return new WaitForSeconds(0.5f);
            }
        }
    }

    

    void BuildMenuPanel()
    {
        _menuOvl = Panel("MenuOvl", new Color(0.10f, 0.07f, 0.18f, 0.95f));
        var root = _menuOvl.GetComponent<RectTransform>();

        var bgImg = _menuOvl.GetComponent<Image>();
        bgImg.color = new Color(0.10f, 0.07f, 0.18f, 0.95f);
        bgImg.material = null;

        var vl = new GameObject("MenuVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        vl.transform.SetParent(root, false);
        var vlRT = vl.GetComponent<RectTransform>();
        vlRT.anchorMin = new Vector2(0.1f, 0.06f);
        vlRT.anchorMax = new Vector2(0.9f, 0.92f);
        vlRT.offsetMin = Vector2.zero;
        vlRT.offsetMax = Vector2.zero;
        var vlg = vl.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 6;
        vlg.padding = new RectOffset(12, 12, 10, 10);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var titleGo = Txt(vl, "菜单", 20, Cyan, 28);
        titleGo.GetComponent<Text>().fontStyle = FontStyle.Bold;

        SepLine(vl);

        string[][] menuItems = {
            new string[] { "\U0001f9ec 进化",     "00ffd0" },
            new string[] { "\U0001f6d2 商店",     "00ffd0" },
            new string[] { "\U0001f517 形态羁绊", "00ffd0" },
            new string[] { "☢ 污染技能",         "00ffd0" },
            new string[] { "\U0001f4be 锚点管理", "00ffd0" },
            new string[] { "\U0001f4be 保存游戏", "00ffd0" },
        };

        Action[] menuActions = {
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowEvolutionPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowShopPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowFormBondPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowPollutionSkillPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowAnchorPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); DoSaveGame(); },
        };

        for (int i = 0; i < menuItems.Length; i++)
        {
            int idx = i;
            var item = menuItems[i];
            Color itemColor = ParseColor(item[1]);

            var btnGo = new GameObject($"MenuBtn_{i}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnGo.transform.SetParent(vl.transform, false);
            btnGo.AddComponent<LayoutElement>().preferredHeight = 54;

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = new Color(0.08f, 0.06f, 0.14f);
            btnImg.material = Resources.Load<Material>("Materials/UI/Default");
            
            var ol = btnGo.GetComponent<Outline>();
            ol.effectColor = new Color(0f, 1f, 0.82f, 0.3f);
            ol.effectDistance = new Vector2(1, 1);

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() => 
            { 
                StartCoroutine(MenuButtonClickAnim(btnGo));
                menuActions[idx](); 
            });

            // 添加悬停效果
            var eventTrigger = btnGo.AddComponent<EventTrigger>();
            
            var enterTrigger = new EventTrigger.Entry();
            enterTrigger.eventID = EventTriggerType.PointerEnter;
            enterTrigger.callback.AddListener((data) => MenuButtonHover(btnGo, true));
            eventTrigger.triggers.Add(enterTrigger);
            
            var exitTrigger = new EventTrigger.Entry();
            exitTrigger.eventID = EventTriggerType.PointerExit;
            exitTrigger.callback.AddListener((data) => MenuButtonHover(btnGo, false));
            eventTrigger.triggers.Add(exitTrigger);

            var txt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(btnGo.transform, false);
            Stretch(txt);
            var t = txt.GetComponent<Text>();
            t.font = F(); t.fontSize = 16; t.color = itemColor;
            t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
            t.text = item[0]; t.raycastTarget = false;

            // 进化按钮添加红点提示
            if (i == 0) // 进化按钮是第一个
            {
                _evoRedDot = new GameObject("EvoRedDot", typeof(RectTransform));
                _evoRedDot.transform.SetParent(btnGo.transform, false);
                var rdRT = _evoRedDot.GetComponent<RectTransform>();
                rdRT.anchorMin = new Vector2(1, 1);
                rdRT.anchorMax = new Vector2(1, 1);
                rdRT.pivot = new Vector2(0.5f, 0.5f);
                rdRT.anchoredPosition = new Vector2(-15, -15);
                rdRT.sizeDelta = new Vector2(16, 16);
                
                // 创建外圈光晕（先创建，放在底层）
                var glow = new GameObject("Glow", typeof(RectTransform), typeof(Text));
                glow.transform.SetParent(_evoRedDot.transform, false);
                var glowRT = glow.GetComponent<RectTransform>();
                glowRT.anchorMin = Vector2.zero;
                glowRT.anchorMax = Vector2.one;
                var glowTxt = glow.GetComponent<Text>();
                glowTxt.font = F();
                glowTxt.fontSize = 24;
                glowTxt.color = new Color(1f, 0.2f, 0.2f, 0.6f);
                glowTxt.alignment = TextAnchor.MiddleCenter;
                glowTxt.text = "●";
                
                // 创建红点（使用文字●）
                var dot = new GameObject("Dot", typeof(RectTransform), typeof(Text));
                dot.transform.SetParent(_evoRedDot.transform, false);
                var dotRT = dot.GetComponent<RectTransform>();
                dotRT.anchorMin = Vector2.zero;
                dotRT.anchorMax = Vector2.one;
                var dotTxt = dot.GetComponent<Text>();
                dotTxt.font = F();
                dotTxt.fontSize = 16;
                dotTxt.color = new Color(1f, 0.2f, 0.2f);
                dotTxt.alignment = TextAnchor.MiddleCenter;
                dotTxt.text = "●";
                
                // 先强制显示红点，验证是否能显示
                _evoRedDot.SetActive(true);
                StartCoroutine(EvoRedDotPulse());
                
                // 延迟一帧后再根据实际条件更新状态
                StartCoroutine(DelayedUpdateRedDot());
            }
        }

        // 固化记忆 (200EP) — special button with dynamic state
        {
            var btnGo = new GameObject("SolidifyBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnGo.transform.SetParent(vl.transform, false);
            btnGo.AddComponent<LayoutElement>().preferredHeight = 50;
            _solidifyBtnImg = btnGo.GetComponent<Image>();
            _solidifyBtnImg.color = new Color(0.08f, 0.06f, 0.14f);
            var ol = btnGo.GetComponent<Outline>();
            ol.effectColor = new Color(0f, 1f, 0.82f, 0.2f);
            ol.effectDistance = new Vector2(1, 1);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = _solidifyBtnImg;
            btn.onClick.AddListener(() => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); DoSolidifyMemory(); });
            var txt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(btnGo.transform, false);
            Stretch(txt);
            _solidifyBtnTxt = txt.GetComponent<Text>();
            _solidifyBtnTxt.font = F(); _solidifyBtnTxt.fontSize = 15; _solidifyBtnTxt.alignment = TextAnchor.MiddleCenter;
            _solidifyBtnTxt.fontStyle = FontStyle.Bold; _solidifyBtnTxt.raycastTarget = false;
        }

        // 退出游戏
        {
            var btnGo = new GameObject("ExitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnGo.transform.SetParent(vl.transform, false);
            btnGo.AddComponent<LayoutElement>().preferredHeight = 50;
            btnGo.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.14f);
            var ol = btnGo.GetComponent<Outline>();
            ol.effectColor = new Color(0.8f, 0.2f, 0.2f, 0.3f);
            ol.effectDistance = new Vector2(1, 1);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnGo.GetComponent<Image>();
            btn.onClick.AddListener(() => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ExitGame(); });
            var txt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(btnGo.transform, false);
            Stretch(txt);
            var t = txt.GetComponent<Text>();
            t.font = F(); t.fontSize = 15; t.color = new Color(1f, 0.4f, 0.4f);
            t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
            t.text = "\U0001f4e6 退出游戏"; t.raycastTarget = false;
        }

        Spacer(vl, 2);

        // 关闭
        var closeBtnGo = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnGo.transform.SetParent(vl.transform, false);
        closeBtnGo.AddComponent<LayoutElement>().preferredHeight = 44;
        closeBtnGo.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.10f);
        var closeBtn = closeBtnGo.GetComponent<Button>();
        closeBtn.targetGraphic = closeBtnGo.GetComponent<Image>();
        closeBtn.onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(_menuOvl)));
        var closeTxt = new GameObject("T", typeof(RectTransform), typeof(Text));
        closeTxt.transform.SetParent(closeBtnGo.transform, false);
        Stretch(closeTxt);
        var ct = closeTxt.GetComponent<Text>();
        ct.font = F(); ct.text = "✕ 关闭"; ct.fontSize = 15; ct.color = Dim;
        ct.alignment = TextAnchor.MiddleCenter; ct.raycastTarget = false;

        _menuOvl.SetActive(false);
    }

    Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        return new Color(r, g, b);
    }

    public void ShowLeaderboardPanel()
    {
        // 清理旧的UI
        if (_rankOvl != null)
        {
            DestroyImmediate(_rankOvl);
            _rankOvl = null;
        }
        _rankBody = null;
        _rankTabs = null;
        _rankSubTabs = null;
        
        BuildLeaderboardOverlay();
        OpenOverlayAnimated(_rankOvl);
    }

    bool _rankBuilt;
    Transform _rankBody;
    Transform _rankTabs;
    Transform _rankSubTabs;
    LeaderboardState _lbState = new LeaderboardState();

    class LeaderboardState
    {
        public string tab = "score";
        public string period = "all";
        public string cls = "swarm";
    }

    void BuildLeaderboardOverlay()
    {
        _rankOvl = Panel("RankOvl", new Color(0.06f, 0.04f, 0.10f, 0.82f));
        var panel = _rankOvl;
        var root = panel.GetComponent<RectTransform>();

        var rankBgTex = Resources.Load<Texture2D>("UI/bg_ranking");
        if (rankBgTex != null)
        {
            var bgGo = new GameObject("BgImg", typeof(RectTransform), typeof(RawImage), typeof(CanvasGroup));
            bgGo.transform.SetParent(root, false);
            Stretch(bgGo);
            bgGo.GetComponent<RawImage>().texture = rankBgTex;
            bgGo.GetComponent<RawImage>().color = new Color(1, 1, 1, 0.55f);
            bgGo.GetComponent<CanvasGroup>().blocksRaycasts = false;
        }

        // 创建主内容容器，使用垂直布局
        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(root, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.5f, 0.5f);
        contentRT.anchorMax = new Vector2(0.5f, 0.5f);
        contentRT.pivot = new Vector2(0.5f, 0.5f);
        contentRT.sizeDelta = new Vector2(400, 540);
        contentRT.anchoredPosition = Vector2.zero;
        
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 6;
        contentVL.padding = new RectOffset(8, 8, 8, 8);
        contentVL.childForceExpandWidth = true;
        contentVL.childForceExpandHeight = false;
        contentVL.childAlignment = TextAnchor.UpperLeft;

        // 标题栏
        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(content.transform, false);
        header.AddComponent<LayoutElement>().preferredHeight = 32;
        var headerHL = header.GetComponent<HorizontalLayoutGroup>();
        headerHL.childForceExpandWidth = false;
        headerHL.childForceExpandHeight = true;
        headerHL.childAlignment = TextAnchor.MiddleCenter;

        var titleLE = new GameObject("TitleWrap", typeof(RectTransform), typeof(LayoutElement), typeof(HorizontalLayoutGroup));
        titleLE.transform.SetParent(header.transform, false);
        titleLE.GetComponent<LayoutElement>().flexibleWidth = 1;
        var twHL = titleLE.GetComponent<HorizontalLayoutGroup>();
        twHL.spacing = 6; twHL.childAlignment = TextAnchor.MiddleLeft;
        twHL.childForceExpandWidth = false; twHL.childForceExpandHeight = true;

        var rankIconTex = Resources.Load<Texture2D>("UI/icon_ranking");
        if (rankIconTex != null)
        {
            var riGo = new GameObject("RankIcon", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
            riGo.transform.SetParent(titleLE.transform, false);
            riGo.GetComponent<RawImage>().texture = rankIconTex;
            riGo.GetComponent<RawImage>().raycastTarget = false;
            riGo.GetComponent<LayoutElement>().preferredWidth = 28;
            riGo.GetComponent<LayoutElement>().preferredHeight = 28;
        }

        var titleTxt = TxtGo(titleLE.transform, "深渊排行", 18, Cyan);
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var closeGo = new GameObject("Close", typeof(RectTransform), typeof(Image), typeof(Button), typeof(LayoutElement));
        closeGo.transform.SetParent(header.transform, false);
        var closeLE = closeGo.GetComponent<LayoutElement>();
        closeLE.preferredWidth = 32;
        closeLE.preferredHeight = 32;
        closeLE.flexibleWidth = 0;
        closeGo.GetComponent<Image>().color = new Color(0.15f, 0.08f, 0.12f, 0.6f);
        var closeTxt = TxtGo(closeGo.transform, "X", 16, new Color(0.9f, 0.3f, 0.3f));
        closeTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(closeTxt.gameObject);
        closeGo.GetComponent<Button>().targetGraphic = closeGo.GetComponent<Image>();
        closeGo.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(panel)));

        // 主标签行
        _rankTabs = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup)).transform;
        _rankTabs.SetParent(content.transform, false);
        _rankTabs.gameObject.AddComponent<LayoutElement>().preferredHeight = 32;
        var tabsHL = _rankTabs.GetComponent<HorizontalLayoutGroup>();
        tabsHL.spacing = 3;
        tabsHL.childForceExpandWidth = true;
        tabsHL.childForceExpandHeight = true;
        tabsHL.childAlignment = TextAnchor.MiddleCenter;

        // 子标签行
        _rankSubTabs = new GameObject("SubTabs", typeof(RectTransform), typeof(HorizontalLayoutGroup)).transform;
        _rankSubTabs.SetParent(content.transform, false);
        _rankSubTabs.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
        var subHL = _rankSubTabs.GetComponent<HorizontalLayoutGroup>();
        subHL.spacing = 5;
        subHL.childForceExpandWidth = false;
        subHL.childForceExpandHeight = true;
        subHL.childAlignment = TextAnchor.MiddleLeft;

        // 数据区域
        _rankBody = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
        _rankBody.SetParent(content.transform, false);
        var bodyLE = _rankBody.gameObject.AddComponent<LayoutElement>();
        bodyLE.preferredHeight = 400;
        bodyLE.flexibleHeight = 1;
        var bodyVL = _rankBody.GetComponent<VerticalLayoutGroup>();
        bodyVL.spacing = 2;
        bodyVL.childForceExpandWidth = true;
        bodyVL.childForceExpandHeight = false;

        BuildLeaderboardTabs();
    }

    void BuildLeaderboardTabs()
    {
        // 清理旧标签（倒序避免跳过）
        for (int i = _rankTabs.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankTabs.GetChild(i).gameObject);
        for (int i = _rankSubTabs.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankSubTabs.GetChild(i).gameObject);
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        string[][] tabs = {
            new[] { "SCORE", "score" },
            new[] { "SPEED", "speed" },
            new[] { "DAILY", "daily" },
            new[] { "CLASS", "class" },
            new[] { "ME", "me" }
        };

        foreach (var tab in tabs)
        {
            bool active = tab[1] == _lbState.tab;
            var btn = CreateLeaderboardTab(_rankTabs, tab[0], tab[1], active);
            btn.GetComponent<Button>().onClick.AddListener(() => { _lbState.tab = tab[1]; BuildLeaderboardTabs(); });
        }

        if (_lbState.tab == "score")
        {
            string[][] periods = {
                new[] { "ALL", "all" },
                new[] { "WEEK", "week" },
                new[] { "DAY", "day" }
            };
            foreach (var period in periods)
            {
                bool active = period[1] == _lbState.period;
                var btn = CreateLeaderboardSubTab(_rankSubTabs, period[0], period[1], active);
                btn.GetComponent<Button>().onClick.AddListener(() => { _lbState.period = period[1]; BuildLeaderboardTabs(); });
            }
        }
        else if (_lbState.tab == "class")
        {
            string[][] classes = {
                new[] { "SWARM", "swarm" },
                new[] { "TITAN", "titan" },
                new[] { "GHOST", "ghost" },
                new[] { "BLOOD", "blood" },
                new[] { "MECH", "mech" }
            };
            foreach (var cls in classes)
            {
                bool active = cls[1] == _lbState.cls;
                var btn = CreateLeaderboardSubTab(_rankSubTabs, cls[0], cls[1], active);
                btn.GetComponent<Button>().onClick.AddListener(() => { _lbState.cls = cls[1]; BuildLeaderboardTabs(); });
            }
        }

        ShowLoadingSkeleton();
        LoadLeaderboardData();
    }

    GameObject CreateLeaderboardTab(Transform parent, string label, string key, bool active)
    {
        var btn = new GameObject("Tab_" + key, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);

        var img = btn.GetComponent<Image>();
        img.color = active ? new Color(0.05f, 0.28f, 0.32f) : new Color(0.08f, 0.06f, 0.14f);

        var txt = TxtGo(btn.transform, label, 12, active ? Cyan : new Color(0.61f, 0.71f, 0.85f));
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);

        if (active)
        {
            var ol = btn.AddComponent<Outline>();
            ol.effectColor = new Color(0f, 1f, 0.82f, 0.45f);
            ol.effectDistance = new Vector2(1, 1);
        }

        var btnComp = btn.GetComponent<Button>();
        btnComp.targetGraphic = img;

        return btn;
    }

    GameObject CreateLeaderboardSubTab(Transform parent, string label, string key, bool active)
    {
        var btn = new GameObject("SubTab_" + key, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);

        var img = btn.GetComponent<Image>();
        img.color = active ? new Color(0.12f, 0.08f, 0.04f) : Color.clear;

        var txt = TxtGo(btn.transform, label, 11, active ? Gold : new Color(0.5f, 0.53f, 0.64f));
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);

        var le = btn.AddComponent<LayoutElement>();
        le.preferredWidth = 70;
        le.preferredHeight = 24;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        var btnComp = btn.GetComponent<Button>();
        btnComp.targetGraphic = img;

        return btn;
    }

    void ShowLoadingSkeleton()
    {
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        var skeleton = new GameObject("Skeleton", typeof(RectTransform));
        skeleton.transform.SetParent(_rankBody, false);
        var rt = skeleton.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var vl = skeleton.AddComponent<VerticalLayoutGroup>();
        vl.childAlignment = TextAnchor.MiddleCenter;

        var icon = TxtGo(skeleton.transform, "⌛", 22, new Color(0.5f, 0.53f, 0.64f));
        icon.alignment = TextAnchor.MiddleCenter;
        icon.fontStyle = FontStyle.Normal;

        Txt(skeleton.transform, "加载中…", 12, new Color(0.5f, 0.53f, 0.64f), 24);
    }

    async void LoadLeaderboardData()
    {
        try
        {
            if (!LeaderboardAPI.Instance.HasAPI())
            {
                ShowLocalFallback("本地记录（未连接服务器）");
                return;
            }

            var reqTab = _lbState.tab;
            var reqSub = _lbState.tab == "score" ? _lbState.period : (_lbState.tab == "class" ? _lbState.cls : "");

            object result = null;
            if (_lbState.tab == "score")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("score", _lbState.period);
            else if (_lbState.tab == "speed")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("speed");
            else if (_lbState.tab == "daily")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("daily");
            else if (_lbState.tab == "class")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("class", cls: _lbState.cls);
            else if (_lbState.tab == "me")
            {
                string uid = PlayerPrefs.GetString("PT_UID", "");
                if (string.IsNullOrEmpty(uid))
                {
                    ShowNoIdentityMessage();
                    return;
                }
                result = await LeaderboardAPI.Instance.FetchMyLeaderboard(uid);
            }

            if (_lbState.tab != reqTab) return;
            if (_lbState.tab == "score" && _lbState.period != reqSub) return;
            if (_lbState.tab == "class" && _lbState.cls != reqSub) return;

            for (int ci = _rankBody.childCount - 1; ci >= 0; ci--)
                DestroyImmediate(_rankBody.GetChild(ci).gameObject);

            if (_lbState.tab == "me")
                RenderMyLeaderboard(result as PlayerLeaderboardData);
            else
                RenderLeaderboardRows((result as LeaderboardResponse)?.rows);

        }
        catch (Exception e)
        {
            Debug.LogError($"Leaderboard load failed: {e.Message}");
            ShowLocalFallback("无法连接服务器，显示本地记录");
        }
    }

    void RenderLeaderboardRows(List<LeaderboardRow> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            Txt(_rankBody, "🏅", 28, new Color(0.3f, 0.3f, 0.3f), 36);
            Txt(_rankBody, "暂无记录", 13, new Color(0.61f, 0.71f, 0.85f), 24);
            Txt(_rankBody, "完成一次短局即可上榜", 11, new Color(0.36f, 0.38f, 0.48f), 18);
            return;
        }

        bool isSpeed = _lbState.tab == "speed";
        string myUid = PlayerPrefs.GetString("PT_UID", "");

        // 表头
        var hdr = CreateLBRow(_rankBody, 22);
        CreateLBCell(hdr, "#",  10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 24);
        if (!isSpeed) CreateLBCell(hdr, "评级", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "分数", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 48);
        CreateLBCell(hdr, "层",  10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "时间", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 50);
        CreateLBCell(hdr, "玩家", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleLeft, 0, 1);

        // 分隔线
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        line.transform.SetParent(_rankBody, false);
        line.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.25f);
        var lineLE = line.GetComponent<LayoutElement>();
        lineLE.preferredHeight = 1; lineLE.flexibleHeight = 0;

        // 数据行
        for (int i = 0; i < rows.Count; i++)
        {
            var data = rows[i];
            bool isMe = !string.IsNullOrEmpty(data.uid) && !string.IsNullOrEmpty(myUid) && data.uid == myUid;

            Color rankColor = i == 0 ? new Color(0.96f,0.77f,0.33f) :
                              i == 1 ? new Color(0.79f,0.82f,0.90f) :
                              i == 2 ? new Color(0.80f,0.50f,0.20f) : new Color(0.36f,0.38f,0.48f);
            Color rowTxt = isMe ? Cyan : new Color(0.80f, 0.85f, 0.93f);

            var row = CreateLBRow(_rankBody, 24);
            CreateLBCell(row, $"{(data.rank > 0 ? data.rank : i + 1)}", 11, rankColor, TextAnchor.MiddleCenter, 24);
            if (!isSpeed) CreateLBCell(row, data.rating ?? "-", 11, GetRatingColor(data.rating), TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, $"{data.score}", 11, rowTxt, TextAnchor.MiddleCenter, 48);
            CreateLBCell(row, $"F{data.floor}", 11, rowTxt, TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, FormatDuration(data.duration), 10, new Color(0.60f,0.71f,0.85f), TextAnchor.MiddleCenter, 50);

            string name = (data.nickname ?? "?").Replace("<","").Replace(">","");
            if (isMe) name += " (我)";
            CreateLBCell(row, name, 11, rowTxt, TextAnchor.MiddleLeft, 0, 1);
        }

        Txt(_rankBody, $"共 {rows.Count} 条记录", 10, new Color(0.36f, 0.38f, 0.48f), 20);
    }

    Transform CreateLBRow(Transform parent, int height)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 2;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.childAlignment = TextAnchor.MiddleCenter;
        var le = row.GetComponent<LayoutElement>();
        le.preferredHeight = height;
        le.flexibleHeight = 0;
        return row.transform;
    }

    void CreateLBCell(Transform parent, string text, int fontSize, Color color, TextAnchor align, float width, float flex = 0)
    {
        var cell = new GameObject("Cell", typeof(RectTransform), typeof(LayoutElement));
        cell.transform.SetParent(parent, false);
        var le = cell.GetComponent<LayoutElement>();
        if (width > 0) le.preferredWidth = width;
        le.flexibleWidth = flex;
        var txt = TxtGo(cell.transform, text, fontSize, color);
        txt.alignment = align;
        Stretch(txt.gameObject);
    }

    GameObject CreateLeaderboardRow(bool isSpeed, bool isHeader)
    {
        var row = new GameObject(isHeader ? "Header" : "Row", typeof(RectTransform));
        var rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = isHeader ? new Vector2(0, 28) : new Vector2(0, 34);

        var grid = row.AddComponent<GridLayoutGroup>();
        grid.spacing = new Vector2(5, 0);
        grid.childAlignment = TextAnchor.MiddleCenter;

        if (isSpeed)
            grid.constraintCount = 5;
        else
            grid.constraintCount = 6;

        grid.cellSize = new Vector2(0, isHeader ? 24 : 30);

        if (isHeader)
        {
            CreateHeaderCell(row.transform, "#", true);
            if (!isSpeed)
                CreateHeaderCell(row.transform, "评级", true);
            CreateHeaderCell(row.transform, "分数", true);
            CreateHeaderCell(row.transform, "层", true);
            CreateHeaderCell(row.transform, "时间", true);
            CreateHeaderCell(row.transform, "玩家", true);
        }

        return row;
    }

    void CreateHeaderCell(Transform parent, string text, bool isHeader)
    {
        var cell = new GameObject("Cell", typeof(RectTransform));
        cell.transform.SetParent(parent, false);
        var txt = TxtGo(cell.transform, text, isHeader ? 10 : 12, new Color(0.5f, 0.53f, 0.64f));
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontStyle = FontStyle.Normal;
    }

    void PopulateLeaderboardRow(GameObject row, LeaderboardRow data, int index, string myUid)
    {
        bool isSpeed = _lbState.tab == "speed";
        bool isMe = !string.IsNullOrEmpty(data.uid) && !string.IsNullOrEmpty(myUid) && data.uid == myUid;

        Color rankColor = index == 0 ? Gold : (index == 1 ? Bright : (index == 2 ? new Color(0.8f, 0.5f, 0.2f) : new Color(0.36f, 0.38f, 0.48f)));
        Color ratingColor = GetRatingColor(data.rating);
        string timeStr = FormatDuration(data.duration);

        var cells = row.GetComponent<GridLayoutGroup>();

        CreateLeaderboardCell(row.transform, $"{(data.rank > 0 ? data.rank : (index + 1))}", 12, rankColor, index < 3, TextAnchor.MiddleCenter);

        if (!isSpeed)
        {
            CreateLeaderboardCell(row.transform, data.rating ?? "-", 12, ratingColor, true, TextAnchor.MiddleCenter);
        }

        CreateLeaderboardCell(row.transform, $"{data.score}", 12, new Color(0.91f, 0.95f, 1f), true, TextAnchor.MiddleCenter);
        CreateLeaderboardCell(row.transform, $"{data.floor}", 12, new Color(0.61f, 0.71f, 0.85f), false, TextAnchor.MiddleCenter);
        CreateLeaderboardCell(row.transform, timeStr, 12, new Color(0.61f, 0.71f, 0.85f), false, TextAnchor.MiddleCenter);
        
        string name = (data.nickname ?? "?").Replace("<", "").Replace(">", "").Replace("\"", "").Replace("&", "");
        CreateLeaderboardCell(row.transform, name + (isMe ? " (我)" : ""), 11.5f, isMe ? Cyan : new Color(0.79f, 0.82f, 0.87f), false, TextAnchor.MiddleLeft);
    }

    void CreateLeaderboardCell(Transform parent, string text, float fontSize, Color color, bool bold, TextAnchor alignment)
    {
        var cell = new GameObject("Cell", typeof(RectTransform));
        cell.transform.SetParent(parent, false);
        var txt = TxtGo(cell.transform, text, (int)fontSize, color);
        txt.alignment = alignment;
        txt.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
    }

    void RenderMyLeaderboard(PlayerLeaderboardData data)
    {
        if (data == null)
        {
            ShowNoIdentityMessage();
            return;
        }

        string nick = (!string.IsNullOrEmpty(data.nickname) ? data.nickname : PlayerPrefs.GetString("PT_NICKNAME", "")).Replace("<", "").Replace(">", "").Replace("\"", "").Replace("&", "");
        if (string.IsNullOrEmpty(nick)) nick = "玩家";

        var header = Txt(_rankBody, $"玩家：{nick}", 12, new Color(0.61f, 0.71f, 0.85f), 24).GetComponent<Text>();
        header.alignment = TextAnchor.MiddleLeft;

        var grid = new GameObject("BoardsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(_rankBody, false);
        var glg = grid.GetComponent<GridLayoutGroup>();
        glg.constraintCount = 2;
        glg.spacing = new Vector2(8, 8);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        Dictionary<string, string> labels = new Dictionary<string, string> {
            { "score|all", "综合 · 全时段" }, { "score|week", "综合 · 本周" }, { "score|day", "综合 · 今日" },
            { "speed|", "速通 (F12 通关)" }, { "daily|", "今日 Modifier" },
            { "class|swarm", "职业 · 虫群" }, { "class|titan", "职业 · 巨像" }, { "class|ghost", "职业 · 幽影" },
            { "class|blood", "职业 · 血裔" }, { "class|mech", "职业 · 机械" }
        };

        if (data.boards != null)
        {
            foreach (var board in data.boards)
            {
                string key = $"{board.type}|{board.sub}";
                string label = labels.ContainsKey(key) ? labels[key] : $"{board.type}{(string.IsNullOrEmpty(board.sub) ? "" : $" · {board.sub}")}";

                var card = CardGo(grid.transform, new Color(0.08f, 0.06f, 0.14f), 80);
                var vl = AddVL(card, 8, 2);

                Txt(vl, label, 11, new Color(0.5f, 0.53f, 0.64f), 18);

                if (board.rank > 0)
                {
                    Txt(vl, $"<color={ColorToHex(Gold)}><b>#{board.rank}</b></color> <color={ColorToHex(new Color(0.36f, 0.38f, 0.48f))}> / {board.total}</color>", 14, Bright, 20);
                    string stat = $"超过 {board.percentile}% · ";
                    if (board.type == "speed")
                        stat += $"最快 {FormatDuration(board.bestDur)}";
                    else
                        stat += $"最高 {board.bestScore}";
                    Txt(vl, stat, 11, new Color(0.61f, 0.71f, 0.85f), 18);
                }
                else
                {
                    Txt(vl, "<color=" + ColorToHex(new Color(0.36f, 0.38f, 0.48f)) + ">未上榜</color>", 13, Bright, 18);
                    Txt(vl, "完成一次即可", 11, new Color(0.61f, 0.71f, 0.85f), 18);
                }
            }
        }
    }

    void ShowLocalFallback(string message)
    {
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        // 警告提示
        var warning = new GameObject("Warning", typeof(RectTransform), typeof(Image), typeof(Outline));
        warning.transform.SetParent(_rankBody, false);
        var rt = warning.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 32);

        // 添加 LayoutElement 确保高度固定
        var le = warning.AddComponent<LayoutElement>();
        le.preferredHeight = 32;
        le.flexibleHeight = 0;

        var img = warning.GetComponent<Image>();
        img.color = new Color(0.1f, 0.04f, 0.06f);

        var ol = warning.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.5f, 0.56f, 0.32f);
        ol.effectDistance = new Vector2(1, 1);

        var txt = TxtGo(warning.transform, message, 12, new Color(1f, 0.54f, 0.63f));
        txt.alignment = TextAnchor.MiddleCenter;

        // 渲染本地排行榜数据
        RenderLocalLeaderboard();
    }

    void ShowLocalGameRecords()
    {
        var list = new GameObject("LocalRecords", typeof(RectTransform), typeof(VerticalLayoutGroup));
        list.transform.SetParent(_rankBody, false);
        var lg = list.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6;
        lg.childForceExpandWidth = true;

        Txt(list.transform, "本地游戏记录", 14, Gold, 28);

        var save = SaveSystem.Instance;
        bool hasData = false;

        for (int slot = 0; slot < 3; slot++)
        {
            var data = save?.GetSaveInfo(slot);
            if (data == null) continue;
            if (data.currentFloor <= 0) continue;

            hasData = true;
            string mode = data.gameMode ?? "经典";
            string floor = $"F{data.currentFloor}";
            string cls = data.player?.selectedClass ?? "?";
            string time = data.saveTime.Year > 2000 ? data.saveTime.ToString("MM/dd HH:mm") : "已保存";

            Color rankC = slot == 0 ? Gold : slot == 1 ? Bright : Dim;
            var card = CardGo(list.transform, CardBg, 60);
            var cvl = AddVL(card, 6, 2);
            Txt(cvl, $"#{slot + 1}  {mode}  {floor}  [{cls}]", 12, rankC, 20);
            Txt(cvl, $"HP:{data.player?.hp ?? 0}/{data.player?.maxHp ?? 100}  ATK:{data.player?.attack ?? 0}  {time}", 10, Dim, 16);
        }

        var metaRank = MetaProgressSystem.Instance;
        if (metaRank != null && metaRank.metaData.totalGamesPlayed > 0)
        {
            var md = metaRank.metaData;
            var statsCard = CardGo(list.transform, CardBg, 80);
            var svl = AddVL(statsCard, 6, 3);
            Txt(svl, $"总迭代: {md.totalGamesPlayed}   最深: F{md.maxFloorReached}", 11, Bright, 16);
            Txt(svl, $"总击杀: {md.totalKills}   附身: {md.possessions}", 11, Bright, 16);
            Txt(svl, $"完成度: {metaRank.GetCompletionPercent()}%", 11, Cyan, 16);
        }

        if (!hasData)
            Txt(list.transform, "暂无游戏记录", 12, Dim, 24);
    }

    void RenderLocalLeaderboard()
    {
        // 表头
        var hdr = CreateLBRow(_rankBody, 22);
        CreateLBCell(hdr, "#",  10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 24);
        CreateLBCell(hdr, "评级", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "分数", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 48);
        CreateLBCell(hdr, "层",  10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "时间", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleCenter, 60);
        CreateLBCell(hdr, "职业", 10, new Color(0.5f,0.53f,0.64f), TextAnchor.MiddleLeft, 0, 1);

        // 分隔线
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        line.transform.SetParent(_rankBody, false);
        line.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.25f);
        var lineLE = line.GetComponent<LayoutElement>();
        lineLE.preferredHeight = 1; lineLE.flexibleHeight = 0;

        var save = SaveSystem.Instance;
        bool hasData = false;

        for (int slot = 0; slot < 3; slot++)
        {
            var data = save?.GetSaveInfo(slot);
            if (data == null) continue;
            if (data.currentFloor <= 0) continue;

            hasData = true;

            Color rankColor = slot == 0 ? new Color(0.96f,0.77f,0.33f) :
                              slot == 1 ? new Color(0.79f,0.82f,0.90f) :
                              new Color(0.80f,0.50f,0.20f);

            int score = (data.currentFloor * 100) + (data.player?.attack ?? 0) * 10;
            string rating = GetLocalRating(data.currentFloor);
            string time = data.saveTime.Year > 2000 ? data.saveTime.ToString("MM/dd HH:mm") : "--:--";
            string cls = data.player?.selectedClass ?? "?";

            var row = CreateLBRow(_rankBody, 24);
            CreateLBCell(row, $"{slot + 1}", 11, rankColor, TextAnchor.MiddleCenter, 24);
            CreateLBCell(row, rating, 11, GetRatingColor(rating), TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, $"{score}", 11, new Color(0.91f,0.95f,1f), TextAnchor.MiddleCenter, 48);
            CreateLBCell(row, $"F{data.currentFloor}", 11, new Color(0.60f,0.71f,0.85f), TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, time, 10, new Color(0.60f,0.71f,0.85f), TextAnchor.MiddleCenter, 60);
            CreateLBCell(row, cls, 11, new Color(0.80f,0.85f,0.93f), TextAnchor.MiddleLeft, 0, 1);
        }

        if (!hasData)
        {
            Txt(_rankBody, "暂无本地记录", 12, new Color(0.5f, 0.53f, 0.64f), 24);
        }
    }

    void CreateFixedWidthCell(Transform parent, string text, float fontSize, Color color, TextAnchor alignment, float width)
    {
        var cell = new GameObject("Cell", typeof(RectTransform), typeof(LayoutElement));
        cell.transform.SetParent(parent, false);
        
        var le = cell.GetComponent<LayoutElement>();
        le.preferredWidth = width;
        le.flexibleWidth = width >= 100 ? 1 : 0;
        
        var txt = TxtGo(cell.transform, text, (int)fontSize, color);
        txt.alignment = alignment;
        txt.fontStyle = FontStyle.Normal;
    }

    string GetLocalRating(int floor)
    {
        if (floor >= 12) return "SSS";
        if (floor >= 10) return "SS";
        if (floor >= 8) return "S";
        if (floor >= 6) return "A";
        if (floor >= 4) return "B";
        return "C";
    }

    void ShowNoIdentityMessage()
    {
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        var empty = new GameObject("Empty", typeof(RectTransform));
        empty.transform.SetParent(_rankBody, false);
        var rt = empty.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        var vl = empty.AddComponent<VerticalLayoutGroup>();
        vl.childAlignment = TextAnchor.MiddleCenter;
        vl.padding = new RectOffset(0, 0, 80, 0);

        Txt(empty.transform, "未识别到玩家身份", 13, new Color(0.61f, 0.71f, 0.85f), 24);
    }

    Color GetRatingColor(string rating)
    {
        switch (rating)
        {
            case "SSS": return new Color(1f, 0f, 0.43f);
            case "SS": return new Color(0.71f, 0.33f, 1f);
            case "S": return Gold;
            case "A": return Cyan;
            case "B": return new Color(1f, 0.55f, 0f);
            default: return new Color(0.5f, 0.53f, 0.64f);
        }
    }

    string FormatDuration(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes}:{secs:D2}";
    }

    string ColorToHex(Color c)
    {
        return $"#{Mathf.RoundToInt(c.r * 255):X2}{Mathf.RoundToInt(c.g * 255):X2}{Mathf.RoundToInt(c.b * 255):X2}";
    }

    void ShowMenuPanel()
    {
        UpdateSolidifyBtn();
        UpdateEvoRedDot();
        OpenOverlayAnimated(_menuOvl);
    }

    void UpdateEvoRedDot()
    {
        bool canEvolve = CanEvolve();
        
        if (_evoRedDot != null)
        {
            _evoRedDot.SetActive(canEvolve);
        }
        else if (canEvolve && _menuOvl != null)
        {
            // 如果红点不存在但需要显示，尝试找到进化按钮并创建红点
            var evoBtn = _menuOvl.transform.Find("MenuVL/MenuBtn_0");
            if (evoBtn != null)
            {
                CreateEvoRedDot(evoBtn);
            }
        }
    }

    void CreateEvoRedDot(Transform parent)
    {
        _evoRedDot = new GameObject("EvoRedDot", typeof(RectTransform));
        _evoRedDot.transform.SetParent(parent, false);
        var rdRT = _evoRedDot.GetComponent<RectTransform>();
        rdRT.anchorMin = new Vector2(1, 1);
        rdRT.anchorMax = new Vector2(1, 1);
        rdRT.pivot = new Vector2(0.5f, 0.5f);
        rdRT.anchoredPosition = new Vector2(-15, -15);
        rdRT.sizeDelta = new Vector2(16, 16);
        
        // 创建外圈光晕
        var glow = new GameObject("Glow", typeof(RectTransform), typeof(Text));
        glow.transform.SetParent(_evoRedDot.transform, false);
        var glowRT = glow.GetComponent<RectTransform>();
        glowRT.anchorMin = Vector2.zero;
        glowRT.anchorMax = Vector2.one;
        var glowTxt = glow.GetComponent<Text>();
        glowTxt.font = F();
        glowTxt.fontSize = 24;
        glowTxt.color = new Color(1f, 0.2f, 0.2f, 0.6f);
        glowTxt.alignment = TextAnchor.MiddleCenter;
        glowTxt.text = "●";
        
        // 创建红点
        var dot = new GameObject("Dot", typeof(RectTransform), typeof(Text));
        dot.transform.SetParent(_evoRedDot.transform, false);
        var dotRT = dot.GetComponent<RectTransform>();
        dotRT.anchorMin = Vector2.zero;
        dotRT.anchorMax = Vector2.one;
        var dotTxt = dot.GetComponent<Text>();
        dotTxt.font = F();
        dotTxt.fontSize = 16;
        dotTxt.color = new Color(1f, 0.2f, 0.2f);
        dotTxt.alignment = TextAnchor.MiddleCenter;
        dotTxt.text = "●";
        
        _evoRedDot.SetActive(true);
        StartCoroutine(EvoRedDotPulse());
    }

    void OnPlayerStatsChanged()
    {
        UpdateEvoRedDot();
    }

    IEnumerator DelayedUpdateRedDot()
    {
        yield return null; // 延迟一帧
        UpdateEvoRedDot();
    }

    void MenuButtonHover(GameObject btnGo, bool isEnter)
    {
        var btnImg = btnGo.GetComponent<Image>();
        var outline = btnGo.GetComponent<Outline>();
        var txt = btnGo.transform.Find("Txt")?.GetComponent<Text>();
        
        if (isEnter)
        {
            btnImg.color = new Color(0.12f, 0.10f, 0.20f);
            outline.effectColor = new Color(0f, 1f, 0.82f, 0.6f);
            outline.effectDistance = new Vector2(2, 2);
            if (txt != null)
            {
                txt.fontSize = 17;
                txt.color = new Color(0.3f, 1f, 0.9f);
            }
            btnGo.transform.localScale = Vector3.Lerp(btnGo.transform.localScale, new Vector3(1.02f, 1.02f, 1f), 0.2f);
        }
        else
        {
            btnImg.color = new Color(0.08f, 0.06f, 0.14f);
            outline.effectColor = new Color(0f, 1f, 0.82f, 0.3f);
            outline.effectDistance = new Vector2(1, 1);
            if (txt != null)
            {
                txt.fontSize = 16;
                txt.color = ParseColor("00ffd0");
            }
            btnGo.transform.localScale = Vector3.Lerp(btnGo.transform.localScale, Vector3.one, 0.2f);
        }
    }

    IEnumerator MenuButtonClickAnim(GameObject btnGo)
    {
        Vector3 originalScale = btnGo.transform.localScale;
        
        // 按下缩小
        btnGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        yield return new WaitForSeconds(0.08f);
        
        // 弹回
        btnGo.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        yield return new WaitForSeconds(0.05f);
        
        // 恢复原状
        btnGo.transform.localScale = originalScale;
    }

    void UpdateSolidifyBtn()
    {
        if (_solidifyBtnTxt == null) return;
        var p = GameManager.Instance?.Player;
        bool canAfford = p != null && p.evolutionPoints >= 200;
        _solidifyBtnTxt.text = canAfford ? "\U0001f9e0 固化记忆 (200EP)" : "\U0001f9e0 固化(需200EP)";
        _solidifyBtnTxt.color = canAfford ? Cyan : new Color(0.35f, 0.35f, 0.35f);
        if (_solidifyBtnImg != null)
        {
            var ol = _solidifyBtnImg.GetComponent<Outline>();
            if (ol) ol.effectColor = canAfford ? new Color(0f, 1f, 0.82f, 0.4f) : new Color(0.2f, 0.2f, 0.2f, 0.2f);
        }
    }

    void DoSaveGame()
    {
        try
        {
            SaveSystem.Instance?.Save(0);
            CompleteGameSystem.Instance?.AddCombatLog("\U0001f4be 游戏已保存");
        }
        catch (System.Exception ex)
        {
            Debug.LogError($"[SaveGame] {ex}");
        }
    }

    void DoSolidifyMemory()
    {
        var p = GameManager.Instance?.Player;
        if (p == null || p.evolutionPoints < 200)
        {
            CompleteGameSystem.Instance?.AddCombatLog("❌ 需要200EP来固化记忆");
            return;
        }
        AnchorSystem.Instance?.TryActivateAltar();
    }

    // Phase 3 panel refs
    GameObject _evoOvl, _shopOvl, _bestOvl, _achOvl, _setOvl, _rankOvl, _dailyOvl;
    Transform _evoList, _shopList, _bestList, _achList;
    bool _evoPanelBuilt, _shopPanelBuilt, _bestPanelBuilt, _achPanelBuilt;

    // Shared overlay builder: title bar (with close button) + scrollable content area
    Transform BuildOverlayScaffold(ref GameObject ovl, string panelName, string title, Color titleColor, Color bgColor, string bgTexture = null, string titleIcon = null)
    {
        ovl = Panel(panelName, bgColor);
        var panel = ovl;
        var root = panel.GetComponent<RectTransform>();

        if (!string.IsNullOrEmpty(bgTexture))
        {
            var tex = Resources.Load<Texture2D>("UI/" + bgTexture);
            if (tex != null)
            {
                var bgGo = new GameObject("BgImg", typeof(RectTransform), typeof(RawImage), typeof(CanvasGroup));
                bgGo.transform.SetParent(root, false);
                Stretch(bgGo);
                bgGo.GetComponent<RawImage>().texture = tex;
                bgGo.GetComponent<RawImage>().color = new Color(1, 1, 1, 0.55f);
                bgGo.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }

        // Header bar (fixed top 8%)
        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(root, false);
        var hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 0.92f); hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(15, 0); hRT.offsetMax = new Vector2(-15, -8);
        var hhl = header.GetComponent<HorizontalLayoutGroup>();
        hhl.childAlignment = TextAnchor.MiddleCenter;
        hhl.childForceExpandWidth = false; hhl.childForceExpandHeight = true;
        hhl.spacing = 6;

        if (!string.IsNullOrEmpty(titleIcon))
        {
            var iconTex = Resources.Load<Texture2D>("UI/" + titleIcon);
            if (iconTex != null)
            {
                var icoGo = new GameObject("TitleIcon", typeof(RectTransform), typeof(RawImage), typeof(LayoutElement));
                icoGo.transform.SetParent(header.transform, false);
                icoGo.GetComponent<RawImage>().texture = iconTex;
                icoGo.GetComponent<RawImage>().raycastTarget = false;
                icoGo.GetComponent<LayoutElement>().preferredWidth = 32;
                icoGo.GetComponent<LayoutElement>().preferredHeight = 32;
            }
        }

        var titleTxt = TxtGo(header.transform, title, 22, titleColor);
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var closeGo = new GameObject("X", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(header.transform, false);
        closeGo.AddComponent<LayoutElement>().preferredWidth = 40;
        closeGo.GetComponent<Image>().color = new Color(0.15f, 0.08f, 0.12f);
        var closeTxt = TxtGo(closeGo.transform, "✕", 20, new Color(0.9f, 0.3f, 0.3f));
        closeTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(closeTxt.gameObject);
        closeGo.GetComponent<Button>().targetGraphic = closeGo.GetComponent<Image>();
        closeGo.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(panel)));

        // Separator line under header
        var sepGo = new GameObject("HSep", typeof(RectTransform), typeof(Image));
        sepGo.transform.SetParent(root, false);
        var sepRT = sepGo.GetComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(0.05f, 0.915f); sepRT.anchorMax = new Vector2(0.95f, 0.917f);
        sepRT.offsetMin = Vector2.zero; sepRT.offsetMax = Vector2.zero;
        sepGo.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.2f);

        // Scrollable content area (92% of screen)
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(root, false);
        var sRT = scrollGo.GetComponent<RectTransform>();
        sRT.anchorMin = Vector2.zero; sRT.anchorMax = new Vector2(1, 0.91f);
        sRT.offsetMin = Vector2.zero; sRT.offsetMax = Vector2.zero;
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;
        scroll.scrollSensitivity = 30f;

        var vpGo = new GameObject("VP", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(scrollGo.transform, false);
        Stretch(vpGo);
        vpGo.GetComponent<Image>().color = new Color(1,1,1,0.003f);
        vpGo.GetComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vpGo.GetComponent<RectTransform>();

        var contentGo = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        contentGo.transform.SetParent(vpGo.transform, false);
        var cRT = contentGo.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot = new Vector2(0.5f, 1); cRT.sizeDelta = Vector2.zero;
        var cvl = contentGo.GetComponent<VerticalLayoutGroup>();
        cvl.padding = new RectOffset(15, 15, 10, 30);
        cvl.spacing = 8;
        cvl.childAlignment = TextAnchor.UpperCenter;
        cvl.childForceExpandWidth = true;
        cvl.childForceExpandHeight = false;
        contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = cRT;

        panel.SetActive(false);
        return contentGo.transform;
    }

    IEnumerator CloseOverlayAnimated(GameObject panel)
    {
        yield return UIAnimationSystem.ScaleOutSmooth(panel.GetComponent<RectTransform>(), 0.9f, 0.15f);
        panel.SetActive(false);
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1f;
        panel.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    void OpenOverlayAnimated(GameObject panel)
    {
        panel.SetActive(true);
        var rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        StartCoroutine(UIAnimationSystem.ScaleInSmooth(rt, 0.85f, 0.25f));
    }

    void ShowEvolutionPanel()
    {
        if (_evoOvl == null) BuildEvolutionOverlay();
        _evoPanelBuilt = false;
        OpenOverlayAnimated(_evoOvl);
    }

    void BuildEvolutionOverlay()
    {
        var content = BuildOverlayScaffold(ref _evoOvl, "EvoOvl", "⚡ 进化树", Gold, new Color(0.06f, 0.04f, 0.10f, 0.82f), "bg_evolution");
        var listGo = new GameObject("EvoList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6; lg.padding = new RectOffset(4,4,4,4);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _evoList = listGo.transform;
    }

    void SyncEvolution()
    {
        if (_evoOvl == null || !_evoOvl.activeSelf) return;
        if (_evoPanelBuilt) return;
        _evoPanelBuilt = true;

        for (int i = _evoList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_evoList.GetChild(i).gameObject);

        var gs = CompleteGameSystem.Instance;
        var player = GameManager.Instance?.Player;
        if (gs == null || player == null) return;

        if (!EvolutionData.Trees.TryGetValue(player.selectedClass, out var tree)) return;

        // 创建EP显示栏
        var epBar = new GameObject("EPBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        epBar.transform.SetParent(_evoList, false);
        var epHL = epBar.GetComponent<HorizontalLayoutGroup>();
        epHL.spacing = 8;
        epHL.childAlignment = TextAnchor.MiddleCenter;
        epHL.childForceExpandWidth = false;
        epBar.AddComponent<LayoutElement>().preferredHeight = 32;
        
        TxtGo(epBar.transform, "💰", 20, Gold);
        var epTxt = TxtGo(epBar.transform, player.evolutionPoints.ToString(), 18, Gold);
        epTxt.fontStyle = FontStyle.Bold;
        
        SepLine(_evoList);

        for (int i = 0; i < tree.Count; i++)
        {
            var node = tree[i];
            bool unlocked = i < gs.EvolutionLevel;
            bool canUnlock = i == gs.EvolutionLevel && player.evolutionPoints >= node.epCost;
            bool isNext = i == gs.EvolutionLevel;
            
            // 卡片背景颜色
            Color cardColor = unlocked ? new Color(0.05f, 0.15f, 0.1f) : 
                              canUnlock ? new Color(0.18f, 0.14f, 0.08f) : 
                              new Color(0.08f, 0.06f, 0.12f);
            
            // 文字颜色
            Color titleColor = unlocked ? Cyan : canUnlock ? Gold : Dim;
            Color descColor = unlocked ? Bright : canUnlock ? new Color(0.9f, 0.85f, 0.7f) : new Color(0.5f, 0.48f, 0.55f);

            var card = new GameObject($"EvoCard_{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(_evoList, false);
            var cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 75);
            card.GetComponent<LayoutElement>().preferredHeight = 75;
            
            var cardImg = card.GetComponent<Image>();
            cardImg.color = cardColor;
            
            // 可升级时添加边框发光效果
            if (canUnlock)
            {
                var outline = card.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.7f, 0.2f, 0.6f);
                outline.effectDistance = new Vector2(2, 2);
            }

            // 创建内部布局
            var cvl = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cvl.transform.SetParent(card.transform, false);
            var cvlRT = cvl.GetComponent<RectTransform>();
            cvlRT.anchorMin = Vector2.zero;
            cvlRT.anchorMax = Vector2.one;
            cvlRT.offsetMin = new Vector2(12, 8);
            cvlRT.offsetMax = new Vector2(-12, -8);
            var cvlHL = cvl.GetComponent<HorizontalLayoutGroup>();
            cvlHL.spacing = 12;
            cvlHL.childForceExpandWidth = true;
            cvlHL.childAlignment = TextAnchor.MiddleLeft;

            // 左侧：等级和名称
            var leftVL = new GameObject("Left", typeof(RectTransform), typeof(VerticalLayoutGroup));
            leftVL.transform.SetParent(cvl.transform, false);
            var leftVLg = leftVL.GetComponent<VerticalLayoutGroup>();
            leftVLg.spacing = 4;
            leftVLg.childForceExpandWidth = true;
            
            string status = unlocked ? "✓ 已解锁" : canUnlock ? $"花费 {node.epCost} EP" : $"🔒 需要 {node.epCost} EP";
            var title = TxtGo(leftVL.transform, $"Lv{node.level} {node.name}", 16, titleColor);
            title.fontStyle = FontStyle.Bold;
            var statusTxt = TxtGo(leftVL.transform, status, 12, canUnlock ? Gold : Dim);

            // 中间：描述
            var descVL = new GameObject("Desc", typeof(RectTransform), typeof(VerticalLayoutGroup));
            descVL.transform.SetParent(cvl.transform, false);
            descVL.AddComponent<LayoutElement>().flexibleWidth = 2;
            var descVLg = descVL.GetComponent<VerticalLayoutGroup>();
            descVLg.spacing = 2;
            
            TxtGo(descVL.transform, node.description, 11, descColor);

            // 右侧：解锁按钮或状态图标
            var rightVL = new GameObject("Right", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rightVL.transform.SetParent(cvl.transform, false);
            rightVL.AddComponent<LayoutElement>().flexibleWidth = 0;
            var rightVLg = rightVL.GetComponent<VerticalLayoutGroup>();
            rightVLg.spacing = 4;
            rightVLg.childAlignment = TextAnchor.MiddleRight;

            if (unlocked)
            {
                var check = TxtGo(rightVL.transform, "✓", 24, new Color(0.3f, 1f, 0.6f));
            }
            else if (canUnlock)
            {
                var btn = new GameObject("UnlockBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
                btn.transform.SetParent(rightVL.transform, false);
                var btnRT = btn.GetComponent<RectTransform>();
                btnRT.sizeDelta = new Vector2(80, 32);
                
                var btnImg = btn.GetComponent<Image>();
                btnImg.color = new Color(0.25f, 0.2f, 0.08f);
                
                var btnOutline = btn.GetComponent<Outline>();
                btnOutline.effectColor = new Color(1f, 0.8f, 0.3f, 0.8f);
                btnOutline.effectDistance = new Vector2(1, 1);
                
                var btnTxt = TxtGo(btn.transform, "解锁", 14, new Color(1f, 0.9f, 0.6f));
                btnTxt.alignment = TextAnchor.MiddleCenter;
                
                var button = btn.GetComponent<Button>();
                button.targetGraphic = btnImg;
                button.onClick.AddListener(() => {
                    gs.Evolve();
                    _evoPanelBuilt = false;
                    UpdateEvoRedDot();
                });
            }
            else
            {
                var lockIcon = TxtGo(rightVL.transform, "🔒", 20, Dim);
            }
        }

        // 底部提示
        if (gs.EvolutionLevel >= tree.Count)
        {
            Txt(_evoList, "✨ 所有进化已解锁！", 14, new Color(0.9f, 0.7f, 0.3f), 28);
        }
        else
        {
            var nextCost = tree[gs.EvolutionLevel].epCost;
            int needed = nextCost - player.evolutionPoints;
            if (needed > 0)
            {
                Txt(_evoList, $"还差 {needed} EP 可解锁下一级进化", 12, new Color(0.8f, 0.5f, 0.5f), 22);
            }
        }
    }

    public void ShowShopPanel()
    {
        if (_shopOvl != null) Destroy(_shopOvl);
        _shopOvl = Panel("ShopOvl", new Color(0.06f, 0.04f, 0.1f, 0.82f));
        var root = _shopOvl.GetComponent<RectTransform>();

        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(root, false);
        var hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 0.92f); hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(15, 0); hRT.offsetMax = new Vector2(-15, -8);
        var hhl = header.GetComponent<HorizontalLayoutGroup>();
        hhl.childAlignment = TextAnchor.MiddleCenter;
        hhl.childForceExpandWidth = false; hhl.childForceExpandHeight = true;
        hhl.spacing = 6;

        var titleTxt = TxtGo(header.transform, "🛒 回响商店", 22, Gold);
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var epTxt = TxtGo(header.transform, $"残响: {PlayerPrefs.GetInt("pt_meta_echoes", 0)}", 14, new Color(0.65f, 0.36f, 1f));

        var closeGo = new GameObject("X", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(header.transform, false);
        closeGo.AddComponent<LayoutElement>().preferredWidth = 40;
        closeGo.GetComponent<Image>().color = new Color(0.15f, 0.08f, 0.12f);
        var closeTxt = TxtGo(closeGo.transform, "✕", 20, new Color(0.9f, 0.3f, 0.3f));
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeGo.GetComponent<Button>().targetGraphic = closeGo.GetComponent<Image>();
        closeGo.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(_shopOvl)));

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(root, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero; contentRT.anchorMax = new Vector2(1, 0.91f);
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 8;
        contentVL.padding = new RectOffset(15, 15, 10, 30);
        contentVL.childAlignment = TextAnchor.UpperCenter;
        contentVL.childForceExpandWidth = true;

        var tabsHL = new GameObject("TabsHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabsHL.transform.SetParent(content.transform, false);
        var tabsHLG = tabsHL.GetComponent<HorizontalLayoutGroup>();
        tabsHLG.spacing = 4;
        tabsHLG.childForceExpandWidth = true;

        string[] tabs = { "职业", "皮肤", "功能", "合集" };
        foreach (var tab in tabs)
        {
            var tabBtn = BtnGo(tabsHL.transform, tab, 12, Dim, 38);
            tabBtn.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.08f);
        }

        string[][] items = {
            new string[] {"🩸", "Blood 血族", "300", "血液即武器。每一次受伤都让你更强，但失控的代价是彻底异化。"},
            new string[] {"🤖", "Mech 机械", "300", "钢铁与血肉的错误融合。用装置改写战场，直到装置开始改写你。"},
            new string[] {"⭐", "双职业包", "400", "两种极端，两种终局。完整体验寄生塔的进化深渊。"},
            new string[] {"💜", "霓虹污染", "150", "荧光在血管里流动。你的变异，值得被看见。5款形态专属配色。"},
            new string[] {"🔩", "锈蚀机械", "150", "废土朋克的终局美学。齿轮与骨刺，谁吞噬了谁？"},
            new string[] {"🌀", "深渊原生", "150", "克苏鲁不会敲门，它从内部生长。更深、更暗、更不可名状。"},
            new string[] {"⭐", "皮肤合集", "300", "全部15款 + 限定「镜面反射」皮肤。完整收藏你的异化形态。"},
            new string[] {"🎲", "种子工具包", "150", "输入指定种子，挑战同一局。掌控随机，挑战固定。"},
            new string[] {"⏪", "死亡回溯包", "150", "本局死亡？回溯3层，重新选择。寄生者的特权：错误可以重写。"},
            new string[] {"📖", "图鉴补全", "150", "未收集的形态也能预览名称与图标。看见所有可能，再决定成为谁。"},
            new string[] {"⭐", "功能合集", "300", "全部工具一次到位。完整掌控你的塔。"},
            new string[] {"👑", "终极合集", "800", "完整版《你也是我》。全部职业 + 全部皮肤 + 全部工具 + 限定镜面皮肤。"}
        };

        foreach (var item in items)
        {
            var card = CardGo(content.transform, CardBg, 80);
            var cvl = AddVL(card, 8, 2);
            
            Txt(cvl, $"{item[0]} {item[1]}", 14, Bright, 24);
            Txt(cvl, item[3], 11, Dim, 16);
            
            var buyBtn = BtnGo(cvl.gameObject.transform.parent, $"{item[2]} 残响", 11, new Color(0.65f, 0.36f, 1f), 60);
            buyBtn.GetComponent<Button>().onClick.AddListener(() => {
                ShowMessage("功能开发中");
            });
        }
        
        OpenOverlayAnimated(_shopOvl);
    }

    public void ShowBestiaryPanel()
    {
        if (_bestOvl != null) Destroy(_bestOvl);
        _bestOvl = Panel("BestOvl", new Color(0.06f, 0.04f, 0.1f, 0.82f));
        var root = _bestOvl.GetComponent<RectTransform>();

        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(root, false);
        var hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 0.92f); hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(15, 0); hRT.offsetMax = new Vector2(-15, -8);
        var hhl = header.GetComponent<HorizontalLayoutGroup>();
        hhl.childAlignment = TextAnchor.MiddleCenter;
        hhl.childForceExpandWidth = false; hhl.childForceExpandHeight = true;
        hhl.spacing = 6;

        var titleTxt = TxtGo(header.transform, "📕 异种图鉴", 22, new Color(1f, 0.75f, 0.3f));
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var closeGo = new GameObject("X", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(header.transform, false);
        closeGo.AddComponent<LayoutElement>().preferredWidth = 40;
        closeGo.GetComponent<Image>().color = new Color(0.15f, 0.08f, 0.12f);
        var closeTxt = TxtGo(closeGo.transform, "✕", 20, new Color(0.9f, 0.3f, 0.3f));
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeGo.GetComponent<Button>().targetGraphic = closeGo.GetComponent<Image>();
        closeGo.GetComponent<Button>().onClick.AddListener(() => { _bestPanelBuilt = false; StartCoroutine(CloseOverlayAnimated(_bestOvl)); });

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(root, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero; contentRT.anchorMax = new Vector2(1, 0.91f);
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 8;
        contentVL.padding = new RectOffset(15, 15, 10, 30);
        contentVL.childAlignment = TextAnchor.UpperCenter;
        contentVL.childForceExpandWidth = true;

        var seen = GameManager.Instance?.Player?.seenMonsterTypes;
        HashSet<string> seenSet;
        var metaProg = MetaProgressSystem.Instance;
        if (metaProg != null && metaProg.metaData.monstersEncountered?.Count > 0)
            seenSet = new HashSet<string>(metaProg.metaData.monstersEncountered);
        else
            seenSet = seen != null ? new HashSet<string>(seen) : new HashSet<string>();

        int discovered = 0;
        int total = 0;
        foreach (var m in GameDataImporter.MonsterDefinitions)
        {
            if (m.boss) continue;
            total++;
            if (seenSet.Contains(m.id)) discovered++;
        }
        
        var progressTxt = TxtGo(content.transform, $"已收集 <color=#00ffd0><b>{discovered}</b></color> / {total}", 12, Dim);
        progressTxt.alignment = TextAnchor.MiddleCenter;

        foreach (var m in GameDataImporter.MonsterDefinitions)
        {
            if (m.boss) continue;
            
            bool found = seenSet.Contains(m.id);
            var card = CardGo(content.transform, found ? CardBg : new Color(0.07f, 0.05f, 0.10f), 80);
            var cvl = AddVL(card, 8, 2);
            
            if (found)
            {
                string name = m.name;
                string stats = $"❤️HP:{m.hp} ⚔️ATK:{m.atk} 🛡️DEF:{m.def} 📍Z{m.zone}";
                string traits = m.traits != null ? string.Join(", ", m.traits) : "";
                
                Txt(cvl, name, 14, Bright, 24);
                Txt(cvl, stats, 11, Dim, 18);
                if (!string.IsNullOrEmpty(traits))
                {
                    Txt(cvl, $"✨特性: {traits}", 10, Purp, 16);
                }
                
                var monsterRef = m;
                card.AddComponent<Button>().onClick.AddListener(() => ShowMonsterDetail(monsterRef));
                card.GetComponent<Button>().targetGraphic = card.GetComponent<Image>();
            }
            else
            {
                Txt(cvl, "???", 13, new Color(0.3f, 0.3f, 0.4f), 22);
                Txt(cvl, LocalizationData.T("未发现"), 10, new Color(0.2f, 0.2f, 0.3f), 16);
            }
        }
        
        _bestPanelBuilt = true;
        OpenOverlayAnimated(_bestOvl);
    }

    void ShowMonsterDetail((string id, string name, int hp, int atk, int def, int zone, bool boss, Color color, string[] traits, string[] axes) m)
    {
        var detailOvl = new GameObject("MonsterDetailOvl", typeof(RectTransform), typeof(Image));
        detailOvl.transform.SetParent(_root, false);
        detailOvl.transform.SetAsLastSibling();
        var rt = detailOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        var img = detailOvl.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.92f);

        var content = new GameObject("DetailContent", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        content.transform.SetParent(detailOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.12f, 0.18f);
        contentRT.anchorMax = new Vector2(0.88f, 0.88f);

        var contentImg = content.GetComponent<Image>();
        contentImg.color = new Color(0.08f, 0.06f, 0.14f);

        var headerHL = new GameObject("HeaderHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerHL.transform.SetParent(content.transform, false);
        var headerHLG = headerHL.GetComponent<HorizontalLayoutGroup>();
        headerHLG.spacing = 8;
        headerHLG.childForceExpandWidth = false;
        headerHLG.childAlignment = TextAnchor.MiddleCenter;

        TxtGo(headerHL.transform, "📕", 20, new Color(1f, 0.75f, 0.3f));
        TxtGo(headerHL.transform, "异种图鉴", 16, new Color(1f, 0.75f, 0.3f));

        var closeBtn = BtnGo(headerHL.transform, "关闭", 12, Dim, 40);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => Destroy(detailOvl));

        var titleHL = new GameObject("TitleHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        titleHL.transform.SetParent(content.transform, false);
        var titleHLG = titleHL.GetComponent<HorizontalLayoutGroup>();
        titleHLG.spacing = 12;
        titleHLG.childAlignment = TextAnchor.MiddleCenter;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconGo.transform.SetParent(titleHL.transform, false);
        var iconRT = iconGo.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(64, 64);
        var iconImg = iconGo.GetComponent<Image>();
        iconImg.color = m.color;

        var titleVL = new GameObject("TitleVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        titleVL.transform.SetParent(titleHL.transform, false);
        var titleVLG = titleVL.GetComponent<VerticalLayoutGroup>();
        titleVLG.spacing = 2;

        TxtGo(titleVL.transform, m.name, 20, Bright);
        TxtGo(titleVL.transform, $"PARASITE.ARCHIVE / 区域{m.zone}", 11, Dim);

        var statsHL = new GameObject("StatsHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        statsHL.transform.SetParent(content.transform, false);
        var statsHLG = statsHL.GetComponent<HorizontalLayoutGroup>();
        statsHLG.spacing = 8;
        statsHLG.childForceExpandWidth = true;

        CreateStatCard(statsHL.transform, "❤️", "HP", m.hp.ToString(), new Color(0.8f, 0.2f, 0.2f));
        CreateStatCard(statsHL.transform, "⚔️", "ATK", m.atk.ToString(), new Color(0.2f, 0.6f, 0.8f));
        CreateStatCard(statsHL.transform, "🛡️", "DEF", m.def.ToString(), new Color(0.4f, 0.7f, 0.4f));

        if (m.traits != null && m.traits.Length > 0)
        {
            var traitsSection = new GameObject("TraitsSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
            traitsSection.transform.SetParent(content.transform, false);
            var traitsVL = traitsSection.GetComponent<VerticalLayoutGroup>();
            traitsVL.spacing = 4;

            TxtGo(traitsSection.transform, "⚡ 技能 / 特性", 13, Gold);

            foreach (string traitName in m.traits)
            {
                var traitCard = new GameObject("TraitCard", typeof(RectTransform), typeof(Image));
                traitCard.transform.SetParent(traitsSection.transform, false);
                var traitRT = traitCard.GetComponent<RectTransform>();
                traitRT.sizeDelta = new Vector2(0, 50);
                var traitImg = traitCard.GetComponent<Image>();
                traitImg.color = new Color(0.08f, 0.1f, 0.14f);

                var traitHL = new GameObject("TraitHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                traitHL.transform.SetParent(traitCard.transform, false);
                var traitHLG = traitHL.GetComponent<HorizontalLayoutGroup>();
                traitHLG.spacing = 8;
                traitHLG.padding = new RectOffset(8, 8, 4, 4);

                TxtGo(traitHL.transform, "▶", 12, Cyan);
                TxtGo(traitHL.transform, traitName, 12, Bright);
            }
        }

        var footerHL = new GameObject("FooterHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footerHL.transform.SetParent(content.transform, false);
        var footerHLG = footerHL.GetComponent<HorizontalLayoutGroup>();
        footerHLG.spacing = 12;
        footerHLG.childForceExpandWidth = true;

        var backBtn = BtnGo(footerHL.transform, "返回图鉴", 13, Dim, 100);
        backBtn.GetComponent<Button>().onClick.AddListener(() => { Destroy(detailOvl); ShowBestiaryPanel(); });

        var closeBtn2 = BtnGo(footerHL.transform, "关闭", 13, Dim, 100);
        closeBtn2.GetComponent<Button>().onClick.AddListener(() => Destroy(detailOvl));
    }

    void CreateStatCard(Transform parent, string icon, string label, string value, Color color)
    {
        var card = new GameObject($"StatCard_{label}", typeof(RectTransform), typeof(Image));
        card.transform.SetParent(parent, false);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.sizeDelta = new Vector2(0, 44);

        var cardImg = card.GetComponent<Image>();
        cardImg.color = color * 0.15f;

        var cardVL = new GameObject("CardVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        cardVL.transform.SetParent(card.transform, false);
        var cardVLG = cardVL.GetComponent<VerticalLayoutGroup>();
        cardVLG.spacing = 2;
        cardVLG.padding = new RectOffset(6, 6, 4, 4);
        cardVLG.childAlignment = TextAnchor.MiddleCenter;

        TxtGo(cardVL.transform, icon + " " + value, 16, color);
        TxtGo(cardVL.transform, label, 10, Dim);
    }

    string GetTraitTypeLabel(string type)
    {
        switch (type)
        {
            case "passive": return "被动";
            case "active": return "主动";
            case "reaction": return "反应";
            case "move": return "机动";
            default: return type;
        }
    }

    public void ShowAchievementPanel()
    {
        if (_achOvl != null) Destroy(_achOvl);
        _achOvl = Panel("AchOvl", new Color(0.06f, 0.04f, 0.1f, 0.82f));
        var root = _achOvl.GetComponent<RectTransform>();

        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(root, false);
        var hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 0.92f); hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(15, 0); hRT.offsetMax = new Vector2(-15, -8);
        var hhl = header.GetComponent<HorizontalLayoutGroup>();
        hhl.childAlignment = TextAnchor.MiddleCenter;
        hhl.childForceExpandWidth = false; hhl.childForceExpandHeight = true;
        hhl.spacing = 6;

        var titleTxt = TxtGo(header.transform, "🏆 成就回响", 22, Gold);
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var closeGo = new GameObject("X", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(header.transform, false);
        closeGo.AddComponent<LayoutElement>().preferredWidth = 40;
        closeGo.GetComponent<Image>().color = new Color(0.15f, 0.08f, 0.12f);
        var closeTxt = TxtGo(closeGo.transform, "✕", 20, new Color(0.9f, 0.3f, 0.3f));
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeGo.GetComponent<Button>().targetGraphic = closeGo.GetComponent<Image>();
        closeGo.GetComponent<Button>().onClick.AddListener(() => { _achPanelBuilt = false; StartCoroutine(CloseOverlayAnimated(_achOvl)); });

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(root, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero; contentRT.anchorMax = new Vector2(1, 0.91f);
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 8;
        contentVL.padding = new RectOffset(15, 15, 10, 30);
        contentVL.childAlignment = TextAnchor.UpperCenter;
        contentVL.childForceExpandWidth = true;

        var am = AchievementManager.Instance;
        int unlocked = 0;
        int total = 0;
        if (am != null)
        {
            var all = am.GetAllAchievements();
            total = all.Count;
            foreach (var ach in all)
                if (am.IsUnlocked(ach.achievementId)) unlocked++;
        }
        
        var progressTxt = TxtGo(content.transform, $"已解锁 <color=#ffd700><b>{unlocked}</b></color> / {total}", 12, Dim);
        progressTxt.alignment = TextAnchor.MiddleCenter;

        foreach (var ach in am.GetAllAchievements())
        {
            bool done = am.IsUnlocked(ach.achievementId);
            var card = CardGo(content.transform, done ? CardBg : new Color(0.07f, 0.05f, 0.10f), 80);
            var cvl = AddVL(card, 8, 2);
            
            string status = done ? "✓" : "○";
            Txt(cvl, $"{status} {ach.icon} {ach.name}", 14, done ? Gold : Dim, 24);
            Txt(cvl, ach.description, 11, done ? Bright : Dim, 18);
            
            if (!done && ach.target > 1)
            {
                int progress = am.GetProgress(ach.achievementId);
                Txt(cvl, $"进度: {progress}/{ach.target}", 10, Dim, 14);
            }
            
            if (ach.bonus != null && !string.IsNullOrEmpty(ach.bonus.description))
            {
                Txt(cvl, $"奖励: {ach.bonus.description}", 10, done ? Cyan : Dim, 14);
            }
        }
        
        _achPanelBuilt = true;
        OpenOverlayAnimated(_achOvl);
    }

    // Achievement Toast notification
    void BuildAchievementToast()
    {
        _achToast = new GameObject("AchToast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        _achToast.transform.SetParent(_root, false);
        var rt = _achToast.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.88f);
        rt.anchorMax = new Vector2(0.95f, 0.96f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        _achToast.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.15f, 0.95f);
        var outline = _achToast.AddComponent<Outline>();
        outline.effectColor = Gold;
        outline.effectDistance = new Vector2(2, -2);

        var hl = _achToast.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 8; hl.padding = new RectOffset(12, 12, 4, 4);
        hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;

        _achToastIcon = Txt(hl, "🏆", 28, Gold, 40).GetComponent<Text>();
        var le0 = _achToastIcon.gameObject.AddComponent<LayoutElement>();
        le0.preferredWidth = 40;

        var infoGo = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
        infoGo.transform.SetParent(_achToast.transform, false);
        var ivl = infoGo.GetComponent<VerticalLayoutGroup>();
        ivl.spacing = 2; ivl.childForceExpandWidth = true; ivl.childForceExpandHeight = false;
        ivl.childAlignment = TextAnchor.MiddleLeft;
        var le1 = infoGo.AddComponent<LayoutElement>();
        le1.flexibleWidth = 1;

        _achToastName = Txt(ivl, "", 16, Gold, 22).GetComponent<Text>();
        _achToastName.fontStyle = FontStyle.Bold;
        _achToastDesc = Txt(ivl, "", 11, Bright, 16).GetComponent<Text>();

        _achToast.GetComponent<CanvasGroup>().alpha = 0;
        _achToast.SetActive(false);
    }

    void BuildSoftHintBubble()
    {
        _softHintBubble = new GameObject("SoftHint", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        _softHintBubble.transform.SetParent(_root, false);
        _softHintBubble.GetComponent<Image>().color = new Color(0.02f, 0.06f, 0.04f, 0.95f);
        var ol = _softHintBubble.GetComponent<Outline>();
        ol.effectColor = new Color(0f, 1f, 0.816f, 0.9f);
        ol.effectDistance = new Vector2(2, -2);
        var shShadow = _softHintBubble.GetComponent<Shadow>();
        shShadow.effectColor = new Color(0f, 0f, 0f, 0.8f);
        shShadow.effectDistance = new Vector2(3, -3);
        var hrt = _softHintBubble.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.08f, 0.28f);
        hrt.anchorMax = new Vector2(0.92f, 0.35f);
        hrt.offsetMin = Vector2.zero; hrt.offsetMax = Vector2.zero;

        // Left icon
        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Text));
        iconGo.transform.SetParent(hrt, false);
        var iconRT = iconGo.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0, 0); iconRT.anchorMax = new Vector2(0.12f, 1);
        iconRT.offsetMin = new Vector2(6, 0); iconRT.offsetMax = Vector2.zero;
        var iconTxt = iconGo.GetComponent<Text>();
        iconTxt.font = F(); iconTxt.fontSize = 20; iconTxt.color = new Color(0f, 1f, 0.816f);
        iconTxt.text = "💡"; iconTxt.alignment = TextAnchor.MiddleCenter;
        iconTxt.raycastTarget = false;

        _softHintText = TxtAnchored(hrt, "", 15, new Color(0.7f, 1f, 0.9f), new Vector2(0.12f, 0), Vector2.one, 6, 8);
        _softHintText.alignment = TextAnchor.MiddleLeft;
        _softHintText.fontStyle = FontStyle.Bold;
        var textShadow = _softHintText.gameObject.AddComponent<Shadow>();
        textShadow.effectColor = new Color(0f, 0f, 0f, 0.9f);
        textShadow.effectDistance = new Vector2(1, -1);

        _softHintBubble.SetActive(false);
        _softHintBubble.transform.SetAsLastSibling();
    }

    void OnAchievementUnlocked(AchievementData ach)
    {
        _achToastQueue.Enqueue(ach);
    }

    Coroutine _achToastCo;
    void UpdateAchievementToast()
    {
        if (_achToast == null) return;
        if (_achToastCo != null) return;
        if (_achToastQueue.Count > 0)
        {
            var ach = _achToastQueue.Dequeue();
            ST(_achToastIcon, ach.icon ?? "🏆");
            ST(_achToastName, $"成就解锁: {ach.name}");
            string desc = ach.description;
            if (ach.bonus != null && !string.IsNullOrEmpty(ach.bonus.description))
                desc += $"  |  奖励: {ach.bonus.description}";
            ST(_achToastDesc, desc);
            _achToast.SetActive(true);
            _achToast.transform.SetAsLastSibling();
            _achToastCo = StartCoroutine(AchToastSequence());
        }
    }

    IEnumerator AchToastSequence()
    {
        var rt = _achToast.GetComponent<RectTransform>();
        yield return StartCoroutine(UIAnimationSystem.FadeIn(rt, 0.3f));
        yield return new WaitForSeconds(2.2f);
        yield return StartCoroutine(UIAnimationSystem.FadeOut(rt, 0.5f));
        _achToast.SetActive(false);
        var cg = _achToast.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 0;
        _achToastCo = null;
    }

    public void ShowFragmentPanel()
    {
        var p = GameManager.Instance?.Player;
        Dictionary<string, bool> flags = new Dictionary<string, bool>();
        
        if (p != null && p.storyFlags != null)
        {
            flags = p.storyFlags;
        }
        else
        {
            string saveData = PlayerPrefs.GetString("pt_save_classic", "");
            if (!string.IsNullOrEmpty(saveData))
            {
                try
                {
                    var pd = JsonUtility.FromJson<SaveSystem.SaveData>(saveData);
                    if (pd.player != null && pd.player.storyFlags != null)
                    {
                        foreach (var kvp in pd.player.storyFlags)
                        {
                            flags[kvp.key] = kvp.value;
                        }
                    }
                }
                catch { }
            }
        }

        var fragOvl = new GameObject("FragOvl", typeof(RectTransform), typeof(Image));
        fragOvl.transform.SetParent(_root, false);
        fragOvl.transform.SetAsLastSibling();
        var rt = fragOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        var img = fragOvl.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.92f);
        
        var bgBtn = fragOvl.AddComponent<Button>();
        bgBtn.targetGraphic = img;
        bgBtn.onClick.AddListener(() => Destroy(fragOvl));

        var content = new GameObject("FragContent", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        content.transform.SetParent(fragOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.08f, 0.12f);
        contentRT.anchorMax = new Vector2(0.92f, 0.92f);

        var contentImg = content.GetComponent<Image>();
        contentImg.color = new Color(0.06f, 0.04f, 0.1f);

        var headerSection = new GameObject("HeaderSection", typeof(RectTransform), typeof(Image));
        headerSection.transform.SetParent(content.transform, false);
        var headerRT = headerSection.GetComponent<RectTransform>();
        headerRT.sizeDelta = new Vector2(0, 100);
        var headerImg = headerSection.GetComponent<Image>();
        headerImg.color = new Color(0.08f, 0.04f, 0.14f);

        var headerInner = new GameObject("HeaderInner", typeof(RectTransform), typeof(VerticalLayoutGroup));
        headerInner.transform.SetParent(headerSection.transform, false);
        var headerInnerRT = headerInner.GetComponent<RectTransform>();
        headerInnerRT.anchorMin = Vector2.zero;
        headerInnerRT.anchorMax = Vector2.one;
        headerInnerRT.offsetMin = new Vector2(16, 12);
        headerInnerRT.offsetMax = new Vector2(-16, -12);
        var headerInnerVL = headerInner.GetComponent<VerticalLayoutGroup>();
        headerInnerVL.spacing = 4;
        headerInnerVL.childAlignment = TextAnchor.UpperLeft;

        var titleHL = new GameObject("TitleHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        titleHL.transform.SetParent(headerInner.transform, false);
        var titleHLG = titleHL.GetComponent<HorizontalLayoutGroup>();
        titleHLG.spacing = 8;
        titleHLG.childForceExpandWidth = false;

        TxtGo(titleHL.transform, "📁", 22, new Color(0.85f, 0.35f, 0.85f));
        var titleTxt = TxtGo(titleHL.transform, "记忆档案", 20, new Color(0.9f, 0.8f, 1f));
        titleTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;

        var closeBtn = BtnGo(titleHL.transform, "×", 18, new Color(0.6f, 0.5f, 0.7f), 36);
        closeBtn.GetComponent<LayoutElement>().preferredWidth = 36;
        closeBtn.GetComponent<Button>().onClick.AddListener(() => Destroy(fragOvl));
        closeBtn.GetComponent<Image>().color = new Color(0.1f, 0.06f, 0.16f);

        var subtitleTxt = TxtGo(headerInner.transform, "收集宿主残留记忆，解锁被遗忘的真相", 12, new Color(0.6f, 0.5f, 0.7f));

        int[] mainFloors = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
        string[] mainTitles = { "觉醒", "起源", "裂痕", "深渊", "真相", "终焉", "回归", "超越", "虚空", "重生" };
        string[] hiddenIds = { "echo_corridor", "mirror_whisper", "parasite_resonance", "death_memory" };
        string[] hiddenNames = { "记忆裂隙", "虚空低语", "寄生共鸣", "死亡记忆" };

        int mainCount = 0;
        foreach (int f in mainFloors)
        {
            if (flags.ContainsKey("floor_" + f)) mainCount++;
        }

        int hiddenCount = 0;
        foreach (string id in hiddenIds)
        {
            if (flags.ContainsKey("hidden_" + id)) hiddenCount++;
        }

        int wallGiftCount = flags.Keys.Count(k => k.StartsWith("hidden_wall_gift_"));
        int total = mainFloors.Length + hiddenIds.Length + 1;
        int found = mainCount + hiddenCount + (wallGiftCount > 0 ? 1 : 0);

        var progressRow = new GameObject("ProgressRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        progressRow.transform.SetParent(headerInner.transform, false);
        var progressHLG = progressRow.GetComponent<HorizontalLayoutGroup>();
        progressHLG.spacing = 12;
        progressHLG.childForceExpandWidth = true;

        var progressBarGo = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
        progressBarGo.transform.SetParent(progressRow.transform, false);
        var progressBarRT = progressBarGo.GetComponent<RectTransform>();
        progressBarRT.sizeDelta = new Vector2(0, 6);
        var progressBarImg = progressBarGo.GetComponent<Image>();
        progressBarImg.color = new Color(0.1f, 0.15f, 0.2f);

        var progressFill = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
        progressFill.transform.SetParent(progressBarGo.transform, false);
        var progressFillRT = progressFill.GetComponent<RectTransform>();
        progressFillRT.anchorMin = Vector2.zero;
        progressFillRT.anchorMax = new Vector2((float)found / total, 1);
        progressFillRT.offsetMin = Vector2.zero;
        progressFillRT.offsetMax = Vector2.zero;
        var progressFillImg = progressFill.GetComponent<Image>();
        progressFillImg.color = new Color(0.2f, 0.8f, 0.8f);

        var progressText = TxtGo(progressRow.transform, $"<color=#00ffff><b>{found}</b></color> / {total}", 13, Bright);
        progressText.GetComponent<RectTransform>().sizeDelta = new Vector2(60, 0);

        var mainSection = new GameObject("MainSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
        mainSection.transform.SetParent(content.transform, false);
        var mainVL = mainSection.GetComponent<VerticalLayoutGroup>();
        mainVL.spacing = 6;

        var mainHeader = new GameObject("MainHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        mainHeader.transform.SetParent(mainSection.transform, false);
        var mainHeaderHLG = mainHeader.GetComponent<HorizontalLayoutGroup>();
        mainHeaderHLG.spacing = 8;
        mainHeaderHLG.childForceExpandWidth = false;

        TxtGo(mainHeader.transform, "📜", 14, Cyan);
        var mainTitle = TxtGo(mainHeader.transform, "主线剧情", 14, Cyan);
        mainTitle.GetComponent<Text>().fontStyle = FontStyle.Bold;
        TxtGo(mainHeader.transform, $"({mainCount}/{mainFloors.Length})", 12, Dim);

        var mainList = new GameObject("MainList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        mainList.transform.SetParent(mainSection.transform, false);
        var mainListVL = mainList.GetComponent<VerticalLayoutGroup>();
        mainListVL.spacing = 4;

        for (int i = 0; i < mainFloors.Length; i++)
        {
            int floor = mainFloors[i];
            bool unlocked = flags.ContainsKey("floor_" + floor);
            var card = new GameObject($"MainCard_{floor}", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(mainList.transform, false);
            var cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 44);
            var cardImg = card.GetComponent<Image>();
            cardImg.color = unlocked ? new Color(0.08f, 0.1f, 0.14f) : new Color(0.06f, 0.04f, 0.08f);

            var cardHL = new GameObject("CardHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardHL.transform.SetParent(card.transform, false);
            var cardHLG = cardHL.GetComponent<HorizontalLayoutGroup>();
            cardHLG.spacing = 10;
            cardHLG.padding = new RectOffset(12, 12, 8, 8);
            cardHLG.childForceExpandWidth = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(cardHL.transform, false);
            var iconRT = iconGo.GetComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(28, 28);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = unlocked ? new Color(0.2f, 0.6f, 0.8f) : new Color(0.2f, 0.2f, 0.3f);

            var infoVL = new GameObject("InfoVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoVL.transform.SetParent(cardHL.transform, false);
            var infoVLg = infoVL.GetComponent<VerticalLayoutGroup>();
            infoVLg.spacing = 2;
            infoVLg.childForceExpandWidth = false;

            string idStr = "F" + (floor < 10 ? "0" + floor : floor.ToString());
            var idTxt = TxtGo(infoVL.transform, idStr, 11, unlocked ? Cyan : Dim);
            var nameTxt = TxtGo(infoVL.transform, unlocked ? mainTitles[i] : "???", 13, unlocked ? Bright : Dim);

            var statusGo = new GameObject("Status", typeof(RectTransform));
            statusGo.transform.SetParent(cardHL.transform, false);
            var statusRT = statusGo.GetComponent<RectTransform>();
            statusRT.sizeDelta = new Vector2(20, 0);

            TxtGo(statusGo.transform, unlocked ? "✓" : "🔒", 14, unlocked ? new Color(0.3f, 0.8f, 0.4f) : Dim);

            if (unlocked)
            {
                card.AddComponent<Button>().onClick.AddListener(() => 
                {
                    ShowMemoryDetail("主线", idStr, mainTitles[i], floor);
                });
                card.GetComponent<Button>().targetGraphic = cardImg;
                var btnColors = card.GetComponent<Button>().colors;
                btnColors.highlightedColor = new Color(0.12f, 0.14f, 0.2f);
                card.GetComponent<Button>().colors = btnColors;
            }
        }

        var hiddenSection = new GameObject("HiddenSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
        hiddenSection.transform.SetParent(content.transform, false);
        var hiddenVL = hiddenSection.GetComponent<VerticalLayoutGroup>();
        hiddenVL.spacing = 6;

        var hiddenHeader = new GameObject("HiddenHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        hiddenHeader.transform.SetParent(hiddenSection.transform, false);
        var hiddenHeaderHLG = hiddenHeader.GetComponent<HorizontalLayoutGroup>();
        hiddenHeaderHLG.spacing = 8;
        hiddenHeaderHLG.childForceExpandWidth = false;

        TxtGo(hiddenHeader.transform, "🔮", 14, new Color(0.85f, 0.35f, 0.85f));
        var hiddenTitle = TxtGo(hiddenHeader.transform, "隐藏事件", 14, new Color(0.85f, 0.6f, 0.9f));
        hiddenTitle.GetComponent<Text>().fontStyle = FontStyle.Bold;
        TxtGo(hiddenHeader.transform, $"({hiddenCount}/{hiddenIds.Length})", 12, Dim);

        var hiddenList = new GameObject("HiddenList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        hiddenList.transform.SetParent(hiddenSection.transform, false);
        var hiddenListVL = hiddenList.GetComponent<VerticalLayoutGroup>();
        hiddenListVL.spacing = 4;

        for (int i = 0; i < hiddenIds.Length; i++)
        {
            bool unlocked = flags.ContainsKey("hidden_" + hiddenIds[i]);
            var card = new GameObject($"HiddenCard_{hiddenIds[i]}", typeof(RectTransform), typeof(Image));
            card.transform.SetParent(hiddenList.transform, false);
            var cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 40);
            var cardImg = card.GetComponent<Image>();
            cardImg.color = unlocked ? new Color(0.1f, 0.06f, 0.16f) : new Color(0.06f, 0.04f, 0.08f);

            var cardHL = new GameObject("CardHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cardHL.transform.SetParent(card.transform, false);
            var cardHLG = cardHL.GetComponent<HorizontalLayoutGroup>();
            cardHLG.spacing = 10;
            cardHLG.padding = new RectOffset(12, 12, 6, 6);
            cardHLG.childForceExpandWidth = false;

            var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
            iconGo.transform.SetParent(cardHL.transform, false);
            var iconRT = iconGo.GetComponent<RectTransform>();
            iconRT.sizeDelta = new Vector2(24, 24);
            var iconImg = iconGo.GetComponent<Image>();
            iconImg.color = unlocked ? new Color(0.7f, 0.3f, 0.7f) : new Color(0.2f, 0.15f, 0.25f);

            var infoVL = new GameObject("InfoVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoVL.transform.SetParent(cardHL.transform, false);
            var infoVLg = infoVL.GetComponent<VerticalLayoutGroup>();
            infoVLg.spacing = 2;
            infoVLg.childForceExpandWidth = false;

            string idStr = "H" + (i + 1 < 10 ? "0" + (i + 1) : (i + 1).ToString());
            var idTxt = TxtGo(infoVL.transform, idStr, 11, unlocked ? new Color(0.85f, 0.4f, 0.85f) : Dim);
            var nameTxt = TxtGo(infoVL.transform, unlocked ? hiddenNames[i] : "???", 12, unlocked ? Bright : Dim);

            var statusGo = new GameObject("Status", typeof(RectTransform));
            statusGo.transform.SetParent(cardHL.transform, false);
            var statusRT = statusGo.GetComponent<RectTransform>();
            statusRT.sizeDelta = new Vector2(20, 0);

            TxtGo(statusGo.transform, unlocked ? "✓" : "🔒", 13, unlocked ? new Color(0.85f, 0.4f, 0.85f) : Dim);

            if (unlocked)
            {
                card.AddComponent<Button>().onClick.AddListener(() => 
                {
                    ShowMemoryDetail("隐藏", idStr, hiddenNames[i], i + 1);
                });
                card.GetComponent<Button>().targetGraphic = cardImg;
                var btnColors = card.GetComponent<Button>().colors;
                btnColors.highlightedColor = new Color(0.14f, 0.1f, 0.2f);
                card.GetComponent<Button>().colors = btnColors;
            }
        }

        var wallGiftSection = new GameObject("WallGiftSection", typeof(RectTransform), typeof(VerticalLayoutGroup));
        wallGiftSection.transform.SetParent(content.transform, false);
        var wallGiftVL = wallGiftSection.GetComponent<VerticalLayoutGroup>();
        wallGiftVL.spacing = 6;

        var wallGiftHeader = new GameObject("WallGiftHeader", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        wallGiftHeader.transform.SetParent(wallGiftSection.transform, false);
        var wallGiftHLG = wallGiftHeader.GetComponent<HorizontalLayoutGroup>();
        wallGiftHLG.spacing = 8;
        wallGiftHLG.childForceExpandWidth = false;

        TxtGo(wallGiftHeader.transform, "🎁", 14, new Color(1f, 0.7f, 0.3f));
        var wallGiftTitle = TxtGo(wallGiftHeader.transform, "馈赠", 14, new Color(1f, 0.7f, 0.3f));
        wallGiftTitle.GetComponent<Text>().fontStyle = FontStyle.Bold;

        bool wallGiftUnlocked = wallGiftCount > 0;
        var cardWG = new GameObject("WallGiftCard", typeof(RectTransform), typeof(Image));
        cardWG.transform.SetParent(wallGiftSection.transform, false);
        var cardWGRT = cardWG.GetComponent<RectTransform>();
        cardWGRT.sizeDelta = new Vector2(0, 40);
        var cardWGImg = cardWG.GetComponent<Image>();
        cardWGImg.color = wallGiftUnlocked ? new Color(0.12f, 0.08f, 0.1f) : new Color(0.06f, 0.04f, 0.08f);

        var cardWGHL = new GameObject("CardHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        cardWGHL.transform.SetParent(cardWG.transform, false);
        var cardWGHLG = cardWGHL.GetComponent<HorizontalLayoutGroup>();
        cardWGHLG.spacing = 10;
        cardWGHLG.padding = new RectOffset(12, 12, 6, 6);
        cardWGHLG.childForceExpandWidth = false;

        var iconWG = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        iconWG.transform.SetParent(cardWGHL.transform, false);
        var iconWGRT = iconWG.GetComponent<RectTransform>();
        iconWGRT.sizeDelta = new Vector2(24, 24);
        var iconWGImg = iconWG.GetComponent<Image>();
        iconWGImg.color = wallGiftUnlocked ? new Color(1f, 0.7f, 0.3f) : new Color(0.25f, 0.2f, 0.15f);

        var infoWG = new GameObject("Info", typeof(RectTransform));
        infoWG.transform.SetParent(cardWGHL.transform, false);
        TxtGo(infoWG.transform, wallGiftUnlocked ? "神秘馈赠" : "???", 12, wallGiftUnlocked ? Bright : Dim);

        var statusWG = new GameObject("Status", typeof(RectTransform));
        statusWG.transform.SetParent(cardWGHL.transform, false);
        var statusWGRT = statusWG.GetComponent<RectTransform>();
        statusWGRT.sizeDelta = new Vector2(20, 0);
        TxtGo(statusWG.transform, wallGiftUnlocked ? "✓" : "🔒", 13, wallGiftUnlocked ? new Color(1f, 0.7f, 0.3f) : Dim);

        if (wallGiftUnlocked)
        {
            cardWG.AddComponent<Button>().onClick.AddListener(() => 
            {
                ShowMemoryDetail("馈赠", "WG", "神秘馈赠", 0);
            });
            cardWG.GetComponent<Button>().targetGraphic = cardWGImg;
            var btnColors = cardWG.GetComponent<Button>().colors;
            btnColors.highlightedColor = new Color(0.16f, 0.12f, 0.14f);
            cardWG.GetComponent<Button>().colors = btnColors;
        }
    }

    void ShowMemoryDetail(string type, string id, string name, int index)
    {
        var detailOvl = new GameObject("MemoryDetailOvl", typeof(RectTransform), typeof(Image));
        detailOvl.transform.SetParent(_root, false);
        detailOvl.transform.SetAsLastSibling();
        var rt = detailOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        var img = detailOvl.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.95f);

        var content = new GameObject("DetailContent", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        content.transform.SetParent(detailOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.08f, 0.15f);
        contentRT.anchorMax = new Vector2(0.92f, 0.88f);

        var contentImg = content.GetComponent<Image>();
        contentImg.color = new Color(0.06f, 0.04f, 0.1f);
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 12;
        contentVL.padding = new RectOffset(16, 16, 16, 16);

        var headerHL = new GameObject("HeaderHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerHL.transform.SetParent(content.transform, false);
        var headerHLG = headerHL.GetComponent<HorizontalLayoutGroup>();
        headerHLG.spacing = 8;
        headerHLG.childForceExpandWidth = false;

        string icon = type == "主线" ? "📜" : type == "隐藏" ? "🔮" : "🎁";
        TxtGo(headerHL.transform, icon, 18, type == "主线" ? Cyan : type == "隐藏" ? new Color(0.85f, 0.35f, 0.85f) : new Color(1f, 0.7f, 0.3f));
        TxtGo(headerHL.transform, id, 14, Dim);

        var closeBtn = BtnGo(headerHL.transform, "×", 16, Dim, 32);
        closeBtn.GetComponent<LayoutElement>().preferredWidth = 32;
        closeBtn.GetComponent<Button>().onClick.AddListener(() => Destroy(detailOvl));

        TxtGo(content.transform, name, 18, Bright);

        var divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divider.transform.SetParent(content.transform, false);
        var dividerRT = divider.GetComponent<RectTransform>();
        dividerRT.sizeDelta = new Vector2(0, 1);
        divider.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.35f);

        string[] mainLores = {
            "觉醒：宿主意识开始觉醒，寄生体与宿主初次接触。",
            "起源：实验室的真相逐渐浮出水面，一切开始的地方。",
            "裂痕：现实与虚空的边界开始模糊，异常现象频发。",
            "深渊：深入地下设施，发现更古老的秘密。",
            "真相：揭开寄生体的真正目的与宿主的命运。",
            "终焉：最终的决战即将到来，命运的十字路口。",
            "回归：一切回归原点，但有些东西已经改变。",
            "超越：超越生死，超越寄生，超越一切。",
            "虚空：虚空的真相，万物的尽头。",
            "重生：新的开始，或者是另一个轮回的起点。"
        };

        string[] hiddenLores = {
            "记忆裂隙：在特定条件下出现的时空裂隙，连接着不同的记忆碎片。",
            "虚空低语：来自虚空深处的声音，包含着古老的知识。",
            "寄生共鸣：与寄生体产生共鸣，获得超越凡人的力量。",
            "死亡记忆：记录着无数死亡的记忆，每一个都是警示。"
        };

        string lore = "";
        if (type == "主线" && index >= 1 && index <= mainLores.Length)
        {
            lore = mainLores[index - 1];
        }
        else if (type == "隐藏" && index >= 1 && index <= hiddenLores.Length)
        {
            lore = hiddenLores[index - 1];
        }
        else if (type == "馈赠")
        {
            lore = "神秘的馈赠，来自虚空深处的礼物。它包含着未知的力量与秘密。";
        }

        if (string.IsNullOrEmpty(lore))
        {
            lore = "暂无更多档案记录。";
        }

        TxtGo(content.transform, lore, 13, new Color(0.75f, 0.7f, 0.8f));

        var footerHL = new GameObject("FooterHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footerHL.transform.SetParent(content.transform, false);
        var footerHLG = footerHL.GetComponent<HorizontalLayoutGroup>();
        footerHLG.spacing = 12;
        footerHLG.childForceExpandWidth = true;

        var backBtn = BtnGo(footerHL.transform, "返回", 13, Dim, 40);
        backBtn.GetComponent<Button>().onClick.AddListener(() => { Destroy(detailOvl); ShowFragmentPanel(); });

        var closeBtn2 = BtnGo(footerHL.transform, "关闭", 13, Dim, 40);
        closeBtn2.GetComponent<Button>().onClick.AddListener(() => Destroy(detailOvl));
    }

    // ========== 污染技能面板 ==========
    GameObject _pollSkillOvl;
    Transform _pollSkillList;
    bool _pollSkillBuilt;

    void ShowPollutionSkillPanel()
    {
        if (_pollSkillOvl == null) BuildPollutionSkillOverlay();
        _pollSkillBuilt = false;
        OpenOverlayAnimated(_pollSkillOvl);
    }

    void BuildPollutionSkillOverlay()
    {
        var content = BuildOverlayScaffold(ref _pollSkillOvl, "PollSkillOvl", "☢ 污染技能", new Color(0.9f, 0.5f, 0.1f), new Color(0.10f, 0.07f, 0.15f, 0.92f));
        var listGo = new GameObject("PollSkillList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6; lg.padding = new RectOffset(4, 4, 4, 4);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _pollSkillList = listGo.transform;
    }

    void SyncPollutionSkill()
    {
        if (_pollSkillOvl == null || !_pollSkillOvl.activeSelf) return;
        if (_pollSkillBuilt) return;
        _pollSkillBuilt = true;

        for (int i = _pollSkillList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_pollSkillList.GetChild(i).gameObject);

        var p = GameManager.Instance?.Player;
        float poll = p != null ? p.pollution : 0;

        Txt(_pollSkillList, $"当前污染值: {poll:F0}%", 15, ParasiteTowerColorScheme.GetPollutionColor(poll / 100f), 28);
        Spacer(_pollSkillList.GetComponent<VerticalLayoutGroup>(), 4);

        // 主动技能
        Txt(_pollSkillList, "⚡ 主动技能", 16, Gold, 26);
        foreach (var skill in PollutionTierData.Skills)
        {
            bool unlocked = poll >= skill.threshold;
            Color cardC = unlocked ? new Color(0.06f, 0.12f, 0.08f) : CardBg;
            Color textC = unlocked ? Cyan : Dim;

            var card = CardGo(_pollSkillList, cardC, 60);
            var cvl = AddVL(card, 8, 2);
            string status = unlocked ? "✓ 已解锁" : $"\U0001f512 需要{skill.threshold}%污染";
            Txt(cvl, $"☢ {skill.name}  [{status}]", 14, textC, 22);
            Txt(cvl, skill.desc, 11, unlocked ? Bright : Dim, 18);
            Txt(cvl, $"解锁条件: 污染≥{skill.threshold}%", 10, Dim, 14);
        }

        Spacer(_pollSkillList.GetComponent<VerticalLayoutGroup>(), 8);

        // 被动效果
        Txt(_pollSkillList, "\U0001f52e 被动效果", 16, Purp, 26);
        foreach (var passive in PollutionTierData.Passives)
        {
            bool active = poll >= passive.threshold;
            Color cardC = active ? new Color(0.12f, 0.06f, 0.18f) : CardBg;
            Color textC = active ? Purp : Dim;

            var card = CardGo(_pollSkillList, cardC, 50);
            var cvl = AddVL(card, 8, 2);
            string status = active ? "✓ 生效中" : $"\U0001f512 需要{passive.threshold}%";
            Txt(cvl, $"\U0001f52e {passive.name}  [{status}]", 13, textC, 20);
            Txt(cvl, passive.desc, 11, active ? Bright : Dim, 16);
        }

        Spacer(_pollSkillList.GetComponent<VerticalLayoutGroup>(), 8);

        // 污染等级
        var tier = PollutionPassiveSystem.GetPollutionTier(poll);
        Txt(_pollSkillList, $"污染等级: {tier.label} {tier.icon}", 14, ParasiteTowerColorScheme.GetPollutionColor(poll / 100f), 24);
        Txt(_pollSkillList, $"ATK倍率: x{PollutionPassiveSystem.GetPollutionAttackMultiplier(poll):F2}  DEF倍率: x{PollutionPassiveSystem.GetPollutionDefenseMultiplier(poll):F2}", 12, Dim, 18);
    }

    // ========== 形态羁绊面板 ==========
    GameObject _bondOvl;
    Transform _bondList;
    bool _bondBuilt;

    void ShowFormBondPanel()
    {
        if (_bondOvl == null) BuildFormBondOverlay();
        _bondBuilt = false;
        OpenOverlayAnimated(_bondOvl);
    }

    void BuildFormBondOverlay()
    {
        var content = BuildOverlayScaffold(ref _bondOvl, "BondOvl", "\U0001f517 形态羁绊", Cyan, new Color(0.08f, 0.06f, 0.12f, 0.92f));
        var listGo = new GameObject("BondList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6; lg.padding = new RectOffset(4, 4, 4, 4);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _bondList = listGo.transform;
    }

    void SyncFormBond()
    {
        if (_bondOvl == null || !_bondOvl.activeSelf) return;
        if (_bondBuilt) return;
        _bondBuilt = true;

        for (int i = _bondList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_bondList.GetChild(i).gameObject);

        var p = GameManager.Instance?.Player;
        if (p == null) return;

        int totalBonds = 0;
        if (p.formBondCounts != null)
            foreach (var kv in p.formBondCounts) totalBonds += kv.Value;

        Txt(_bondList, $"总羁绊次数: {totalBonds}", 14, Cyan, 24);
        Spacer(_bondList.GetComponent<VerticalLayoutGroup>(), 4);

        if (p.ownedForms == null || p.ownedForms.Count == 0)
        {
            Txt(_bondList, "暂无已拥有形态", 13, Dim, 24);
            return;
        }

        foreach (var formId in p.ownedForms)
        {
            int bondCount = 0;
            if (p.formBondCounts != null && p.formBondCounts.ContainsKey(formId))
                bondCount = p.formBondCounts[formId];

            int bondLevel = Mathf.Clamp(bondCount / 3, 0, 5);
            bool isCurrent = formId == p.currentFormId;
            string displayName = formId == "human" ? CName(p.selectedClass) : MName(formId);

            Color cardC = isCurrent ? new Color(0f, 0.12f, 0.1f) : CardBg;
            var card = CardGo(_bondList, cardC, 70);

            var hl = new GameObject("HL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            hl.transform.SetParent(card.transform, false);
            Stretch(hl);
            var hlg = hl.GetComponent<HorizontalLayoutGroup>();
            hlg.padding = new RectOffset(8, 8, 4, 4); hlg.spacing = 8;
            hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;

            // Icon
            var iconGo = new GameObject("I", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            iconGo.transform.SetParent(hl.transform, false);
            iconGo.GetComponent<LayoutElement>().preferredWidth = 50;
            iconGo.GetComponent<LayoutElement>().preferredHeight = 50;
            iconGo.GetComponent<Image>().preserveAspect = true;
            LoadIcon(iconGo.GetComponent<Image>(), formId == "human" ? "c_" + p.selectedClass : formId);

            // Info column
            var infoVl = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
            infoVl.transform.SetParent(hl.transform, false);
            infoVl.GetComponent<LayoutElement>().flexibleWidth = 1;
            var ivlg = infoVl.GetComponent<VerticalLayoutGroup>();
            ivlg.spacing = 2; ivlg.childForceExpandWidth = true; ivlg.childForceExpandHeight = false;

            Color nameC = isCurrent ? Cyan : GetResonanceColor(bondLevel);
            Txt(infoVl, $"{displayName}{(isCurrent ? " [当前]" : "")}", 14, nameC, 20);

            string stars = "";
            for (int s = 0; s < 5; s++) stars += s < bondLevel ? "★" : "☆";
            Txt(infoVl, $"羁绊 Lv.{bondLevel}  {stars}  ({bondCount}次)", 11, Dim, 16);

            string bonus = GetBondBonus(bondLevel);
            if (!string.IsNullOrEmpty(bonus))
                Txt(infoVl, $"加成: {bonus}", 10, Gold, 14);
        }
    }

    string GetBondBonus(int level)
    {
        switch (level)
        {
            case 1: return "ATK+5%";
            case 2: return "ATK+5%, DEF+5%";
            case 3: return "ATK+10%, DEF+5%, HP+5%";
            case 4: return "ATK+10%, DEF+10%, HP+10%";
            case 5: return "ATK+15%, DEF+10%, HP+10%, 附身率+5%";
            default: return "";
        }
    }

    // ========== 锚点管理面板 ==========
    GameObject _anchorOvl;
    Transform _anchorList;
    bool _anchorBuilt;

    void ShowAnchorPanel()
    {
        if (_anchorOvl == null) BuildAnchorOverlay();
        _anchorBuilt = false;
        OpenOverlayAnimated(_anchorOvl);
    }

    void BuildAnchorOverlay()
    {
        var content = BuildOverlayScaffold(ref _anchorOvl, "AnchorOvl", "\U0001f4be 锚点管理", Cyan, new Color(0.08f, 0.06f, 0.12f, 0.92f));
        var listGo = new GameObject("AnchorList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6; lg.padding = new RectOffset(4, 4, 4, 4);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _anchorList = listGo.transform;
    }

    void SyncAnchor()
    {
        if (_anchorOvl == null || !_anchorOvl.activeSelf) return;
        if (_anchorBuilt) return;
        _anchorBuilt = true;

        for (int i = _anchorList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_anchorList.GetChild(i).gameObject);

        var anchor = AnchorSystem.Instance;
        if (anchor == null)
        {
            Txt(_anchorList, "锚点系统未初始化", 13, Dim, 24);
            return;
        }

        int currentFloor = GameManager.Instance.CurrentFloor;
        Txt(_anchorList, $"当前楼层: F{currentFloor}", 14, Cyan, 24);
        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), 4);

        var allAnchors = anchor.GetAllAnchors();
        if (allAnchors.Count == 0)
        {
            Txt(_anchorList, "暂无活跃锚点", 13, Dim, 24);
            Txt(_anchorList, "消耗200EP可在当前楼层建立锚点", 11, Dim, 18);
        }
        else
        {
            Txt(_anchorList, $"锚点 ({allAnchors.Count}/3)", 15, Gold, 24);
            foreach (var a in allAnchors)
            {
                Color cardC = a.activated && !a.used ? new Color(0f, 0.12f, 0.1f) : CardBg;
                string status = a.used ? "已使用" : a.activated ? "可用" : "未激活";
                Color statusC = a.used ? Dim : a.activated ? Cyan : Dim;

                var card = CardGo(_anchorList, cardC, 55);
                var cvl = AddVL(card, 8, 2);
                Txt(cvl, $"⛓ F{a.floor} · {a.name}  [{status}]", 14, statusC, 22);

                if (a.activated && !a.used)
                {
                    int dist = currentFloor - a.floor;
                    Txt(cvl, $"距离当前: {dist}层  建立于: {a.activatedAt:MM/dd HH:mm}", 10, Dim, 16);

                    int floorIdx = a.floor;
                    card.AddComponent<Button>().onClick.AddListener(() => {
                        anchor.UseAnchor();
                        _anchorBuilt = false;
                        StartCoroutine(CloseOverlayAnimated(_anchorOvl));
                    });
                    card.GetComponent<Button>().targetGraphic = card.GetComponent<Image>();
                }
            }
        }

        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), 8);

        if (anchor.HasActiveAnchor())
        {
            Txt(_anchorList, $"⏪ 当前锚定: F{anchor.GetCurrentAnchorFloor()}", 13, Cyan, 20);
            Txt(_anchorList, "死亡时自动传送至最近锚点", 11, Dim, 16);
        }

        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), 4);
        Txt(_anchorList, "───── 记忆断层 ─────", 11, new Color(0.3f, 0.25f, 0.4f), 18);
        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), 4);
        Txt(_anchorList, "锚点之间的楼层记忆将丢失", 11, new Color(0.6f, 0.3f, 0.3f), 16);
    }

    public void ShowSettingsPanel()
    {
        if (_setOvl == null) BuildSettingsOverlay();
        OpenOverlayAnimated(_setOvl);
    }

    void BuildSettingsOverlay()
    {
        var content = BuildOverlayScaffold(ref _setOvl, "SetOvl", LocalizationData.T("终端设置"), Cyan, new Color(0.06f, 0.04f, 0.10f, 0.82f), "bg_settings", "icon_settings");
        var vl = content.GetComponent<VerticalLayoutGroup>();

        Spacer(vl, 8);

        // === Audio Section ===
        Txt(content, "-- 音量控制 --", 14, Bright, 24);
        Spacer(vl, 4);

        float[] volSteps = { 0f, 0.25f, 0.5f, 0.75f, 1f };

        var masterBtn = BtnGo(content, $"{LocalizationData.T("主音量")}: {Mathf.RoundToInt(AudioManager.Instance.GetMasterVolume() * 100)}%", 13, Cyan, 42);
        masterBtn.GetComponent<Button>().onClick.AddListener(() => {
            float cur = AudioManager.Instance.GetMasterVolume();
            float next = CycleVolume(cur, volSteps);
            AudioManager.Instance.SetMasterVolume(next);
            masterBtn.GetComponentInChildren<Text>().text = $"{LocalizationData.T("主音量")}: {Mathf.RoundToInt(next * 100)}%";
        });

        Spacer(vl, 4);
        var bgmBtn = BtnGo(content, $"{LocalizationData.T("背景音乐")}: {Mathf.RoundToInt(AudioManager.Instance.GetBGMVolume() * 100)}%", 13, Cyan, 42);
        bgmBtn.GetComponent<Button>().onClick.AddListener(() => {
            float cur = AudioManager.Instance.GetBGMVolume();
            float next = CycleVolume(cur, volSteps);
            AudioManager.Instance.SetBGMVolume(next);
            bgmBtn.GetComponentInChildren<Text>().text = $"{LocalizationData.T("背景音乐")}: {Mathf.RoundToInt(next * 100)}%";
        });

        Spacer(vl, 4);
        var sfxBtn = BtnGo(content, $"{LocalizationData.T("音效")}: {Mathf.RoundToInt(AudioManager.Instance.GetSFXVolume() * 100)}%", 13, Cyan, 42);
        sfxBtn.GetComponent<Button>().onClick.AddListener(() => {
            float cur = AudioManager.Instance.GetSFXVolume();
            float next = CycleVolume(cur, volSteps);
            AudioManager.Instance.SetSFXVolume(next);
            sfxBtn.GetComponentInChildren<Text>().text = $"{LocalizationData.T("音效")}: {Mathf.RoundToInt(next * 100)}%";
        });

        Spacer(vl, 12);

        // === Language ===
        string langLabel = LocalizationData.UseEnglish ? "Language: English  [切换中文]" : "语言: 中文  [Switch to English]";
        var langBtn = BtnGo(content, langLabel, 13, Cyan, 50);
        langBtn.GetComponent<Button>().onClick.AddListener(() => {
            LocalizationData.UseEnglish = !LocalizationData.UseEnglish;
            PlayerPrefs.SetInt("PT_UseEnglish", LocalizationData.UseEnglish ? 1 : 0);
            PlayerPrefs.Save();
            RebuildAllForLanguage();
        });

        Spacer(vl, 12);

        // === Privacy Policy ===
        var privacyBtn = BtnGo(content, "📜 隐私政策", 13, Dim, 36);
        privacyBtn.GetComponent<Button>().onClick.AddListener(() => {
            ShowPrivacyPolicy();
        });

        Spacer(vl, 8);

        // === Info Section (at bottom) ===
        Txt(content, "-- 信息 --", 14, Bright, 24);
        Spacer(vl, 4);
        var metaInfo = MetaProgressSystem.Instance;
        Txt(content, $"{LocalizationData.T("版本")}: v{Application.version}", 11, Dim, 18);
        if (metaInfo != null)
        {
            Txt(content, $"{LocalizationData.T("总游戏次数")}: {metaInfo.metaData.totalGamesPlayed}", 11, Dim, 18);
            Txt(content, $"{LocalizationData.T("完成度")}: {metaInfo.GetCompletionPercent()}%", 11, Dim, 18);
        }

        Spacer(vl, 10);
    }

    public void ShowEchoAltarPanel()
    {
        if (_altarOvl != null) Destroy(_altarOvl);
        _altarOvl = Panel("AltarOvl", new Color(0.06f, 0.04f, 0.1f, 0.82f));
        var root = _altarOvl.GetComponent<RectTransform>();

        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(root, false);
        var hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 0.92f); hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(15, 0); hRT.offsetMax = new Vector2(-15, -8);
        var hhl = header.GetComponent<HorizontalLayoutGroup>();
        hhl.childAlignment = TextAnchor.MiddleCenter;
        hhl.childForceExpandWidth = false; hhl.childForceExpandHeight = true;
        hhl.spacing = 6;

        var titleTxt = TxtGo(header.transform, "⛯ 残响圣坛", 22, new Color(0.85f, 0.4f, 0.95f));
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var epTxt = TxtGo(header.transform, $"残响: {PlayerPrefs.GetInt("pt_meta_echoes", 0)}", 14, new Color(0.85f, 0.4f, 0.95f));

        var closeGo = new GameObject("X", typeof(RectTransform), typeof(Image), typeof(Button));
        closeGo.transform.SetParent(header.transform, false);
        closeGo.AddComponent<LayoutElement>().preferredWidth = 40;
        closeGo.GetComponent<Image>().color = new Color(0.15f, 0.08f, 0.12f);
        var closeTxt = TxtGo(closeGo.transform, "✕", 20, new Color(0.9f, 0.3f, 0.3f));
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeGo.GetComponent<Button>().targetGraphic = closeGo.GetComponent<Image>();
        closeGo.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(_altarOvl)));

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(root, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero; contentRT.anchorMax = new Vector2(1, 0.91f);
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 8;
        contentVL.padding = new RectOffset(15, 15, 10, 30);
        contentVL.childAlignment = TextAnchor.UpperCenter;
        contentVL.childForceExpandWidth = true;

        var tabsHL = new GameObject("TabsHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabsHL.transform.SetParent(content.transform, false);
        var tabsHLG = tabsHL.GetComponent<HorizontalLayoutGroup>();
        tabsHLG.spacing = 4;
        tabsHLG.childForceExpandWidth = true;

        string[][] classes = {
            new string[] {"🦠", "虫群"},
            new string[] {"🪨", "泰坦"},
            new string[] {"👻", "幽灵"},
            new string[] {"🩸", "血族"},
            new string[] {"🤖", "机甲"}
        };

        for (int i = 0; i < classes.Length; i++)
        {
            var cls = classes[i];
            var tabBtn = BtnGo(tabsHL.transform, $"{cls[0]} {cls[1]}", 11, Dim, 36);
            tabBtn.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.08f);
        }

        string[][] nodes = {
            new string[] {"T1", "生命强化", "永久最大生命+5", "100"},
            new string[] {"T1", "攻击强化", "永久攻击+1", "100"},
            new string[] {"T2", "进化加速", "进化点获取+5%", "200"},
            new string[] {"T2", "词汇精通", "解锁更多记忆词汇", "200"},
            new string[] {"T3", "专属印记", "解锁职业专属被动", "400"},
            new string[] {"T3", "形态精通", "形态上限+1", "400"}
        };

        foreach (var node in nodes)
        {
            var card = CardGo(content.transform, CardBg, 80);
            var cvl = AddVL(card, 8, 2);
            
            Txt(cvl, $"{node[0]} {node[1]}", 14, Bright, 24);
            Txt(cvl, node[2], 11, Dim, 18);
            
            var buyBtn = BtnGo(cvl.gameObject.transform.parent, $"{node[3]} 残响", 11, new Color(0.85f, 0.4f, 0.95f), 60);
            buyBtn.GetComponent<Button>().onClick.AddListener(() => {
                ShowMessage("功能开发中");
            });
        }
        
        OpenOverlayAnimated(_altarOvl);
    }

    public void ShowDailyRewardPanel()
    {
        if (_dailyOvl != null) Destroy(_dailyOvl);
        
        var dailyOvl = new GameObject("DailyOvl", typeof(RectTransform), typeof(Image));
        dailyOvl.transform.SetParent(_root, false);
        dailyOvl.transform.SetAsLastSibling();
        _dailyOvl = dailyOvl;
        
        var rt = dailyOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        
        var img = dailyOvl.GetComponent<Image>();
        img.color = new Color(0.02f, 0.01f, 0.04f);
        
        var bgBtn = dailyOvl.AddComponent<Button>();
        bgBtn.targetGraphic = img;
        bgBtn.onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(dailyOvl)));

        var content = new GameObject("Content", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        content.transform.SetParent(dailyOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.04f, 0.06f);
        contentRT.anchorMax = new Vector2(0.96f, 0.94f);
        
        var contentImg = content.GetComponent<Image>();
        contentImg.color = new Color(0.04f, 0.03f, 0.08f);
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 12;
        contentVL.padding = new RectOffset(12, 12, 12, 12);

        var headerSection = new GameObject("HeaderSection", typeof(RectTransform), typeof(Image));
        headerSection.transform.SetParent(content.transform, false);
        var headerRT = headerSection.GetComponent<RectTransform>();
        headerRT.sizeDelta = new Vector2(0, 100);
        var headerImg = headerSection.GetComponent<Image>();
        headerImg.color = new Color(0.08f, 0.05f, 0.12f);
        
        var headerInner = new GameObject("HeaderInner", typeof(RectTransform), typeof(VerticalLayoutGroup));
        headerInner.transform.SetParent(headerSection.transform, false);
        var headerInnerRT = headerInner.GetComponent<RectTransform>();
        headerInnerRT.anchorMin = Vector2.zero;
        headerInnerRT.anchorMax = Vector2.one;
        headerInnerRT.offsetMin = new Vector2(16, 12);
        headerInnerRT.offsetMax = new Vector2(-16, -12);
        var headerInnerVL = headerInner.GetComponent<VerticalLayoutGroup>();
        headerInnerVL.spacing = 4;
        headerInnerVL.childAlignment = TextAnchor.UpperLeft;
        
        var titleHL = new GameObject("TitleHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        titleHL.transform.SetParent(headerInner.transform, false);
        var titleHLG = titleHL.GetComponent<HorizontalLayoutGroup>();
        titleHLG.spacing = 8;
        titleHLG.childForceExpandWidth = false;
        
        TxtGo(titleHL.transform, "🎁", 22, Gold);
        var titleTxt = TxtGo(titleHL.transform, "每日登录", 20, Gold);
        titleTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;
        
        var closeBtn = BtnGo(titleHL.transform, "✕", 18, new Color(0.9f, 0.3f, 0.3f), 40);
        closeBtn.GetComponent<LayoutElement>().preferredWidth = 40;
        closeBtn.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(dailyOvl)));
        closeBtn.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.1f);
        
        var subtitleTxt = TxtGo(headerInner.transform, "每日登录领取残响，累积奖励更丰厚", 12, new Color(0.6f, 0.5f, 0.5f));
        
        int currentDay = GetDailyLoginDays();
        var streakTxt = TxtGo(headerInner.transform, $"连续登录 <color=#ffd700><b>{currentDay}</b></color> 天", 13, Bright);

        var gridGO = new GameObject("RewardGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        gridGO.transform.SetParent(content.transform, false);
        var gridRT = gridGO.GetComponent<RectTransform>();
        gridRT.sizeDelta = new Vector2(0, 220);

        var grid = gridGO.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(76, 72);
        grid.spacing = new Vector2(6, 6);
        grid.padding = new RectOffset(0, 0, 0, 0);
        grid.startCorner = GridLayoutGroup.Corner.UpperLeft;
        grid.startAxis = GridLayoutGroup.Axis.Horizontal;
        grid.childAlignment = TextAnchor.MiddleCenter;
        grid.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        grid.constraintCount = 3;

        int[] rewards = { 20, 40, 60, 80, 100, 120, 140, 160, 180 };
        bool canClaim = CanClaimDailyReward();

        for (int i = 0; i < 9; i++)
        {
            bool claimed = i + 1 < currentDay;
            bool current = i + 1 == currentDay;
            
            var cell = new GameObject($"Day{i + 1}", typeof(RectTransform), typeof(Image));
            cell.transform.SetParent(gridGO.transform, false);
            var cellRT = cell.GetComponent<RectTransform>();
            cellRT.sizeDelta = new Vector2(76, 72);

            var cellImg = cell.GetComponent<Image>();
            
            if (current && canClaim)
            {
                cellImg.color = new Color(0.18f, 0.12f, 0.28f);
            }
            else if (claimed)
            {
                cellImg.color = new Color(0.08f, 0.1f, 0.12f);
            }
            else
            {
                cellImg.color = new Color(0.05f, 0.04f, 0.07f);
            }

            var cellVL = new GameObject("CellVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
            cellVL.transform.SetParent(cell.transform, false);
            var cellVLG = cellVL.GetComponent<VerticalLayoutGroup>();
            cellVLG.spacing = 4;
            cellVLG.childAlignment = TextAnchor.MiddleCenter;
            cellVLG.padding = new RectOffset(4, 4, 4, 4);

            if (claimed)
            {
                TxtGo(cellVL.transform, "✓", 20, new Color(0.3f, 0.8f, 0.4f));
            }
            else if (current && canClaim)
            {
                TxtGo(cellVL.transform, "!", 20, new Color(1f, 0.8f, 0.3f));
            }
            else
            {
                TxtGo(cellVL.transform, "○", 16, Dim);
            }

            var rewardTxt = TxtGo(cellVL.transform, $"{rewards[i]} ✦", claimed || current ? 12 : 10, claimed || current ? Gold : Dim);
            rewardTxt.alignment = TextAnchor.MiddleCenter;

            var dayTxt = TxtGo(cellVL.transform, $"Day {i + 1}", 10, Dim);
            dayTxt.alignment = TextAnchor.MiddleCenter;

            if (current && canClaim)
            {
                cell.AddComponent<Button>().onClick.AddListener(() => {
                    ClaimDailyReward();
                    StartCoroutine(CloseOverlayAnimated(dailyOvl));
                });
                cell.GetComponent<Button>().targetGraphic = cellImg;
            }
        }

        Spacer(content.transform, 8);

        int todayReward = rewards[Mathf.Min(currentDay - 1, 8)];
        var claimBtn = BtnGo(content.transform, $"\U0001f381 领取今日 {todayReward} 残响", 14, Gold, 56);
        var claimBtnImg = claimBtn.GetComponent<Image>();
        claimBtnImg.color = new Color(0.35f, 0.15f, 0.5f);
        claimBtn.GetComponent<Button>().onClick.AddListener(() => {
            ClaimDailyReward();
            StartCoroutine(CloseOverlayAnimated(dailyOvl));
        });
    }
