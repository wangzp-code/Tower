using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    // ========== MENU ==========
    // Menu dynamic refs
    Text _menuLastRunMode, _menuLastRunFloor, _menuLastRunHost, _menuLastRunPoll;
    Text _menuRunStatus;
    GameObject _menuContinueBtn;
    GameObject _menuLastRunCard;
    GameObject _menuContinueSpacer;
    Image _menuPollBar;

    float _menuSyncTime;
    bool _menuBuiltWithPeripherals;

    void SyncMainMenu()
    {
        if (Time.time - _menuSyncTime < 2f) return;
        _menuSyncTime = Time.time;

        bool hasPeripherals = ModuleUnlockSystem.Instance != null && ModuleUnlockSystem.Instance.IsAnyPeripheralModuleUnlocked();
        if (!_menuBuiltWithPeripherals && hasPeripherals && _menuPanel != null)
        {
            Destroy(_menuPanel);
            _menuPanel = null;
            if (_avatarBtn != null) { Destroy(_avatarBtn); _avatarBtn = null; }
            BuildMenu();
            if (_avatarBtn) _avatarBtn.transform.SetAsLastSibling();
        }

        var save = SaveSystem.Instance?.GetSaveInfo(0);
        bool hasSave = save != null;
        SA(_menuContinueSpacer, hasSave);

        if (hasSave)
        {
            string modeName = GetModeDisplayName(save.gameMode);
            string modeLabel = modeName;
            if (!string.IsNullOrEmpty(save.challengeModId))
            {
                var mod = FindChallengeModById(save.challengeModId);
                if (mod != null)
                    modeLabel = $"{modeName} {mod.icon}{mod.name}";
            }
            ST(_menuLastRunMode, modeLabel);
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

        if (_menuPollBar != null)
        {
            var p = GameManager.Instance?.Player;
            float poll = 0f;
            if (p != null)
                poll = p.pollution;
            else if (hasSave && save.player != null)
                poll = save.player.pollution;
            float pct = Mathf.Clamp01(poll / 100f);
            var pRT = _menuPollBar.GetComponent<RectTransform>();
            pRT.anchorMax = new Vector2(pct, 1);
            _menuPollBar.color = pct < 0.3f ? Cyan : pct < 0.6f ? Purp : Mag;
        }
    }

    void BuildLoginPanel()
    {
        _loginPanel = Panel("Login", ParasiteTowerColorScheme.BioBgDeep);
        var loginBg = _loginPanel.GetComponent<Image>();
        var rt = _loginPanel.GetComponent<RectTransform>();

        var heroTex = LoadTex("UI/menu_hero");
        if (heroTex != null)
        {
            var bgGo = new GameObject("BgHero", typeof(RectTransform), typeof(RawImage));
            bgGo.transform.SetParent(rt, false);
            bgGo.transform.SetAsFirstSibling();
            var bgRI = bgGo.GetComponent<RawImage>();
            bgRI.texture = heroTex;
            ThemeUIHelper.ApplyArtBackgroundTint(bgRI, 0.8f);
            bgRI.raycastTarget = false;
            var bgRT = bgGo.GetComponent<RectTransform>();
            bgRT.anchorMin = Vector2.zero; bgRT.anchorMax = Vector2.one;
            bgRT.offsetMin = Vector2.zero; bgRT.offsetMax = Vector2.zero;
        }
        else
        {
            var fallbackBg = new GameObject("FallbackBg", typeof(RectTransform), typeof(RawImage));
            fallbackBg.transform.SetParent(rt, false);
            fallbackBg.transform.SetAsFirstSibling();
            var fbRI = fallbackBg.GetComponent<RawImage>();
            fbRI.texture = MakeGradientTex(256, new Color(0.03f, 0.02f, 0.08f), new Color(0.12f, 0.08f, 0.22f));
            fbRI.color = Color.white;
            fbRI.raycastTarget = false;
            var fbRT = fallbackBg.GetComponent<RectTransform>();
            fbRT.anchorMin = Vector2.zero; fbRT.anchorMax = Vector2.one;
            fbRT.offsetMin = Vector2.zero; fbRT.offsetMax = Vector2.zero;
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
        var quote = Txt(vl, "「你每夺走一个身体，就离自己更远一步。」", 13, MutedPurp, 25).GetComponent<Text>();
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
        var startImg = startBtn.GetComponent<Image>();
        startImg.color = new Color(0.06f, 0.18f, 0.15f, 0.95f);
        var startOL = startBtn.GetComponent<Outline>();
        startOL.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.NormalBorder;
        startOL.effectDistance = new Vector2(1, 1);
        var startShadow = startBtn.AddComponent<Shadow>();
        startShadow.effectColor = new Color(0, 0, 0, 0.28f);
        startShadow.effectDistance = new Vector2(0, -2);
        var startTxt = TxtGo(startBtn.transform, "开 始 旅 程", 20, Cyan);
        startTxt.alignment = TextAnchor.MiddleCenter;
        startTxt.fontStyle = FontStyle.Bold;
        Stretch(startTxt.gameObject);
        var startButton = startBtn.GetComponent<Button>();
        startButton.targetGraphic = startImg;
        var startColors = startButton.colors;
        startColors.highlightedColor = new Color(0.08f, 0.24f, 0.20f, 1f);
        startColors.pressedColor = new Color(0.04f, 0.12f, 0.10f, 1f);
        startButton.colors = startColors;
        startBtn.AddComponent<ButtonPressFeedback>();
        startButton.onClick.AddListener(() => {
            if (GuestAuthManager.Instance == null) return;
            string nick = _loginNickInput != null ? _loginNickInput.text : "";
            GuestAuthManager.Instance.GuestLogin(string.IsNullOrEmpty(nick) ? null : nick);
            SA(_loginPanel, false);
            
            bool isFirstRun = !ModuleUnlockSystem.Instance.IsAnyPeripheralModuleUnlocked();
            if (isFirstRun)
            {
                GameManager.Instance?.StartNewGame("Short");
            }
            else
            {
                Show(CompleteGameSystem.RunScreen.MainMenu);
                SyncMenuPlayerBar();
            }
        });

        Spacer(vl, 60);
        Txt(vl, $"v{Application.version}", 9, new Color(0.3f, 0.3f, 0.35f), 16).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
    }

    void BuildMenu()
    {
        _menuPanel = Panel("Menu", new Color(0.06f, 0.04f, 0.10f, 1f));
        EnsureEventSystem();

        int heroIdx = UnityEngine.Random.Range(0, HeroIconPaths.Length);
        _avatarTex = LoadTex(HeroIconPaths[heroIdx]);

        // === Background art layers ===
        BuildMenuBackground(_menuPanel.transform);

        // Scrollable content (created first so TopBar/NavBar render on top)
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(_menuPanel.transform, false);
        var scrollRT = scrollGo.GetComponent<RectTransform>();
        scrollRT.anchorMin = Vector2.zero;
        scrollRT.anchorMax = new Vector2(1, 1);
        // 滚动区域底部留出导航栏空间 - 使用设计令牌
        scrollRT.offsetMin = new Vector2(0, UIDesignTokens.Layout.ScrollBottomOffset);
        scrollRT.offsetMax = new Vector2(0, -UIDesignTokens.Component.TopBarHeight - UIDesignTokens.Space.M);
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

        Spacer(contentVL, 18);

        // ── Section 4: Iteration summary card + action buttons ──
        _menuContinueSpacer = new GameObject("ContinueGroup", typeof(RectTransform), typeof(VerticalLayoutGroup));
        _menuContinueSpacer.transform.SetParent(contentGo.transform, false);
        _menuContinueSpacer.SetActive(false);
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

        var newBtn = BuildMenuMainButton(contentGo.transform, LocalizationData.T("开启新一轮闯塔"), 14, Dim,
            new Color(0.06f, 0.05f, 0.12f, 0.35f), new Color(0.08f, 0.06f, 0.14f, 0.5f), false);
        newBtn.GetComponent<Button>().onClick.AddListener(() => GoToModeSelect());

        Spacer(contentVL, 20);

        // ── Section 5: Side function entries ──
        // 首局隐藏次要入口，专注体验核心玩法
        if (ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Archive))
            BuildSideButton(_menuPanel.transform, "记忆", "icon_archive", 0.06f, 0.50f, Purp, "记忆档案");
        if (ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.EchoAltar))
            BuildSideButton(_menuPanel.transform, "圣坛", "icon_altar", 0.06f, 0.39f, Purp, "残响圣坛");
        if (ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.DailyReward))
            BuildSideButton(_menuPanel.transform, "签到", "icon_daily", 0.06f, 0.28f, Purp, "每日登录");
        if (ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ChallengeMode))
            BuildSideButton(_menuPanel.transform, "挑战", "挑战", 0.94f, 0.50f, Cyan, "挑战模式");
        if (ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Achievements))
            BuildSideButton(_menuPanel.transform, "记录", "icon_records", 0.94f, 0.39f, Cyan, "记录管理");
        if (ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Leaderboard))
            BuildSideButton(_menuPanel.transform, "排行", "icon_ranking", 0.94f, 0.28f, Cyan, "暗塔排行");

        // === Bottom navigation bar (fixed, above scroll) ===
        BuildMenuNavBar(_menuPanel.transform);

        // === Top identity bar (rendered last, on top of everything) ===
        BuildMenuTopBar(_menuPanel.transform);

        SyncMenuPlayerBar();

        _menuBuiltWithPeripherals = ModuleUnlockSystem.Instance != null && ModuleUnlockSystem.Instance.IsAnyPeripheralModuleUnlocked();

        // === Menu entrance animation - staggered fade-in ===
        StartCoroutine(AnimateMenuEntrance());
    }

    /// <summary>
    /// 菜单入场动画 - 分阶段渐入各元素，提供层次感和仪式感
    /// </summary>
    IEnumerator AnimateMenuEntrance()
    {
        if (_menuPanel == null) yield break;

        float stagger = UIDesignTokens.Motion.StaggerDelay;
        float fadeDur = UIDesignTokens.Motion.StateChange;
        float slideDur = UIDesignTokens.Motion.PanelSlide;

        // 1. 面板整体淡入
        var panelRT = _menuPanel.GetComponent<RectTransform>();
        var panelCG = _menuPanel.GetComponent<CanvasGroup>() ?? _menuPanel.AddComponent<CanvasGroup>();
        panelCG.alpha = 0f;
        float elapsed = 0f;
        while (elapsed < fadeDur)
        {
            elapsed += Time.deltaTime;
            panelCG.alpha = Mathf.Clamp01(elapsed / fadeDur);
            yield return null;
        }
        panelCG.alpha = 1f;

        // 2. 导航栏从底部滑入
        var navBar = _menuPanel.transform.Find("NavBar");
        if (navBar != null)
        {
            var navRT = navBar.GetComponent<RectTransform>();
            Vector2 origPos = navRT.anchoredPosition;
            navRT.anchoredPosition = origPos + new Vector2(0, -20f);
            var navCG = navBar.GetComponent<CanvasGroup>() ?? navBar.gameObject.AddComponent<CanvasGroup>();
            navCG.alpha = 0f;

            elapsed = 0f;
            while (elapsed < slideDur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideDur;
                float ease = 1f - Mathf.Pow(1f - t, 3f); // easeOutCubic
                navRT.anchoredPosition = origPos + new Vector2(0, -20f * (1f - ease));
                navCG.alpha = ease;
                yield return null;
            }
            navRT.anchoredPosition = origPos;
            navCG.alpha = 1f;
        }

        // 3. 顶部栏从顶部滑入
        var topBar = _menuPanel.transform.Find("TopBar");
        if (topBar != null)
        {
            var topRT = topBar.GetComponent<RectTransform>();
            Vector2 origPos = topRT.anchoredPosition;
            topRT.anchoredPosition = origPos + new Vector2(0, 15f);
            var topCG = topBar.GetComponent<CanvasGroup>() ?? topBar.gameObject.AddComponent<CanvasGroup>();
            topCG.alpha = 0f;

            elapsed = 0f;
            while (elapsed < slideDur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / slideDur;
                float ease = 1f - Mathf.Pow(1f - t, 3f);
                topRT.anchoredPosition = origPos + new Vector2(0, 15f * (1f - ease));
                topCG.alpha = ease;
                yield return null;
            }
            topRT.anchoredPosition = origPos;
            topCG.alpha = 1f;
        }

        // 4. 侧边按钮错落淡入
        string[] sideBtnNames = { "Side_记忆档案", "Side_残响圣坛", "Side_每日登录", "Side_挑战模式", "Side_记录管理", "Side_暗塔排行" };
        for (int i = 0; i < sideBtnNames.Length; i++)
        {
            var sideBtn = _menuPanel.transform.Find(sideBtnNames[i]);
            if (sideBtn == null) continue;

            var btnRT = sideBtn.GetComponent<RectTransform>();
            var btnCG = sideBtn.GetComponent<CanvasGroup>() ?? sideBtn.gameObject.AddComponent<CanvasGroup>();
            btnCG.alpha = 0f;
            btnRT.localScale = Vector3.one * 0.85f;

            // 错落延迟
            yield return new WaitForSeconds(stagger);

            elapsed = 0f;
            float localDur = UIDesignTokens.Motion.Hover;
            while (elapsed < localDur)
            {
                elapsed += Time.deltaTime;
                float t = elapsed / localDur;
                btnCG.alpha = Mathf.Clamp01(t);
                btnRT.localScale = Vector3.Lerp(Vector3.one * 0.85f, Vector3.one, t);
                yield return null;
            }
            btnCG.alpha = 1f;
            btnRT.localScale = Vector3.one;
        }
    }

    void BuildMenuBackground(Transform parent)
    {
        var heroTex = LoadTex("UI/menu_hero");
        if (heroTex != null)
        {
            var heroGo = new GameObject("BgHero", typeof(RectTransform), typeof(RawImage));
            heroGo.transform.SetParent(parent, false);
            heroGo.transform.SetAsFirstSibling();
            var heroRI = heroGo.GetComponent<RawImage>();
            heroRI.texture = heroTex;
            ThemeUIHelper.ApplyArtBackgroundTint(heroRI, 0.9f);
            heroRI.raycastTarget = false;
            var hRT = heroGo.GetComponent<RectTransform>();
            hRT.anchorMin = Vector2.zero;
            hRT.anchorMax = Vector2.one;
            hRT.offsetMin = Vector2.zero; hRT.offsetMax = Vector2.zero;
        }
        else
        {
            var fallbackBg = new GameObject("FallbackBg", typeof(RectTransform), typeof(RawImage));
            fallbackBg.transform.SetParent(parent, false);
            fallbackBg.transform.SetAsFirstSibling();
            var fbRI = fallbackBg.GetComponent<RawImage>();
            fbRI.texture = MakeGradientTex(256, new Color(0.03f, 0.02f, 0.08f), new Color(0.12f, 0.08f, 0.22f));
            fbRI.color = Color.white;
            fbRI.raycastTarget = false;
            var fbRT = fallbackBg.GetComponent<RectTransform>();
            fbRT.anchorMin = Vector2.zero; fbRT.anchorMax = Vector2.one;
            fbRT.offsetMin = Vector2.zero; fbRT.offsetMax = Vector2.zero;
        }

        var monsterTex = LoadTex("UI/menu_monsters");
        if (monsterTex != null)
        {
            var monGo = new GameObject("BgMonsters", typeof(RectTransform), typeof(RawImage));
            monGo.transform.SetParent(parent, false);
            monGo.transform.SetSiblingIndex(1);
            var monRI = monGo.GetComponent<RawImage>();
            monRI.texture = monsterTex;
            ThemeUIHelper.ApplyArtBackgroundTint(monRI, 0.45f);
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
        gtRI.texture = MakeGradientTex(64, new Color(0.06f, 0.04f, 0.10f, 0.95f), Color.clear);
        gtRI.color = ParasiteTowerArtOptimization.UIPolish.PanelStyle.HighlightColor;
        gtRI.raycastTarget = false;
        var gtRT = gradTop.GetComponent<RectTransform>();
        gtRT.anchorMin = new Vector2(0, 0.9f); gtRT.anchorMax = Vector2.one;
        gtRT.offsetMin = Vector2.zero; gtRT.offsetMax = Vector2.zero;

        var vignette = new GameObject("Vignette", typeof(RectTransform), typeof(RawImage));
        vignette.transform.SetParent(parent, false);
        vignette.transform.SetSiblingIndex(3);
        var vigRI = vignette.GetComponent<RawImage>();
        vigRI.texture = MakeGradientTex(128, new Color(0, 0, 0, 0.8f), Color.clear);
        vigRI.color = new Color(0, 0, 0, 0.65f);
        vigRI.raycastTarget = false;
        var vigRT = vignette.GetComponent<RectTransform>();
        vigRT.anchorMin = Vector2.zero; vigRT.anchorMax = Vector2.one;
        vigRT.offsetMin = Vector2.zero; vigRT.offsetMax = Vector2.zero;
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
        // 使用设计令牌 - 基于参考分辨率 (540x960)
        float navBarHeight = UIDesignTokens.Component.NavBarHeight;
        float navLabelFont = UIDesignTokens.Component.NavLabelFont;
        float navLabelH = UIDesignTokens.Component.NavLabelH;
        
        var navBar = new GameObject("NavBar", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        navBar.transform.SetParent(parent, false);
        navBar.GetComponent<Image>().color = UIDesignTokens.Colors.NavBarBg;
        navBar.GetComponent<CanvasGroup>().alpha = 1f;
        navBar.GetComponent<CanvasGroup>().interactable = true;
        var navBarRT = navBar.GetComponent<RectTransform>();
        navBarRT.anchorMin = Vector2.zero;
        navBarRT.anchorMax = new Vector2(1, 0);
        navBarRT.offsetMin = Vector2.zero;
        navBarRT.offsetMax = new Vector2(0, navBarHeight);

        // 顶部生物青色发光分隔线 - 带渐变淡出
        var navSep = new GameObject("NavSep", typeof(RectTransform), typeof(Image));
        navSep.transform.SetParent(navBar.transform, false);
        var navSepRT = navSep.GetComponent<RectTransform>();
        navSepRT.anchorMin = new Vector2(0.04f, 1); navSepRT.anchorMax = new Vector2(0.96f, 1);
        navSepRT.offsetMin = new Vector2(0, -1); navSepRT.offsetMax = Vector2.zero;
        navSep.GetComponent<Image>().color = UIDesignTokens.Colors.PrimaryBorder;

        // 底部微弱分隔线 - 增加层次感
        var navSepBot = new GameObject("NavSepBot", typeof(RectTransform), typeof(Image));
        navSepBot.transform.SetParent(navBar.transform, false);
        var navSepBotRT = navSepBot.GetComponent<RectTransform>();
        navSepBotRT.anchorMin = new Vector2(0.02f, 0); navSepBotRT.anchorMax = new Vector2(0.98f, 0);
        navSepBotRT.offsetMin = Vector2.zero; navSepBotRT.offsetMax = new Vector2(0, 1);
        navSepBot.GetComponent<Image>().color = UIDesignTokens.Colors.PrimaryDim;

        // 导航栏水平布局
        var navHL = new GameObject("NavHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        navHL.transform.SetParent(navBar.transform, false);
        Stretch(navHL);
        var nhl = navHL.GetComponent<HorizontalLayoutGroup>();
        nhl.spacing = UIDesignTokens.Space.XS;
        nhl.childAlignment = TextAnchor.MiddleCenter;
        nhl.childForceExpandWidth = true;
        nhl.childForceExpandHeight = true;
        nhl.padding = new RectOffset((int)UIDesignTokens.Space.M, (int)UIDesignTokens.Space.M, (int)UIDesignTokens.Space.S, (int)UIDesignTokens.Space.S);

        string[] navNames = { "成就", "图鉴", "闯塔", "设置", "商店" };
        string[] navIconTex = { "icon_achievement", "icon_bestiary", null, "icon_settings", "icon_shop" };
        string[] navFallbackIcons = { null, null, "塔", null, null };

        // 图标尺寸 - 使用设计令牌
        float iconSizeNormal = 13f;  // 原22 ×0.6
        float iconSizeCenter = 17f;  // 原28 ×0.6

        for (int ni = 0; ni < navNames.Length; ni++)
        {
            string navName = navNames[ni];
            bool isCenter = ni == 2;

            if (!IsNavModuleUnlocked(navName)) continue;

            var navCell = new GameObject("Nav_" + navName, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            navCell.transform.SetParent(navHL.transform, false);
            var navCellImg = navCell.GetComponent<Image>();
            navCellImg.color = isCenter ? UIDesignTokens.Colors.BtnCenterBg : new Color(0.07f, 0.05f, 0.12f, 0.5f);
            var navOL = navCell.GetComponent<Outline>();
            navOL.effectColor = isCenter ? UIDesignTokens.Colors.BtnCenterBorder : new Color(0f, 1f, 0.816f, 0.15f);
            navOL.effectDistance = new Vector2(UIDesignTokens.Effect.OutlineNormal, UIDesignTokens.Effect.OutlineNormal);

            // 中心按钮额外添加发光边框
            if (isCenter)
            {
                var navGlowOL = navCell.AddComponent<Outline>();
                navGlowOL.effectColor = new Color(0f, 1f, 0.816f, 0.08f);
                navGlowOL.effectDistance = new Vector2(UIDesignTokens.Effect.OutlineLarge, UIDesignTokens.Effect.OutlineLarge);
            }

            var navInner = new GameObject("Inner", typeof(RectTransform), typeof(VerticalLayoutGroup));
            navInner.transform.SetParent(navCell.transform, false);
            Stretch(navInner);
            var nivl = navInner.GetComponent<VerticalLayoutGroup>();
            nivl.spacing = UIDesignTokens.Space.XS;
            nivl.padding = new RectOffset((int)UIDesignTokens.Space.XS, (int)UIDesignTokens.Space.XS, (int)UIDesignTokens.Space.XS, (int)UIDesignTokens.Space.XS);
            nivl.childAlignment = TextAnchor.MiddleCenter;
            nivl.childForceExpandWidth = true;
            nivl.childForceExpandHeight = false;
            nivl.childControlWidth = true;
            nivl.childControlHeight = true;

            float cellIconSize = isCenter ? iconSizeCenter : iconSizeNormal;
            float cellLabelFont = isCenter ? navLabelFont + 1 : navLabelFont;

            string iconTexName = navIconTex[ni];
            var navTex = !string.IsNullOrEmpty(iconTexName) ? LoadTex("UI/" + iconTexName) : null;
            if (navTex != null)
            {
                var iconWrap = new GameObject("IconWrap", typeof(RectTransform), typeof(LayoutElement));
                iconWrap.transform.SetParent(navInner.transform, false);
                var wrapLE = iconWrap.GetComponent<LayoutElement>();
                wrapLE.preferredHeight = cellIconSize;
                wrapLE.preferredWidth = cellIconSize;
                wrapLE.minHeight = cellIconSize;
                wrapLE.minWidth = cellIconSize;

                BuildAspectIcon(iconWrap.transform, navTex, (int)UIDesignTokens.Space.XS);
            }
            else
            {
                string fallbackIcon = navFallbackIcons[ni];
                var niIcon = TxtGo(navInner.transform, !string.IsNullOrEmpty(fallbackIcon) ? fallbackIcon : "⚔", (int)(cellLabelFont + 8), isCenter ? UIDesignTokens.Colors.Primary : UIDesignTokens.Colors.TextSecondary);
                niIcon.alignment = TextAnchor.MiddleCenter;
                niIcon.fontStyle = FontStyle.Bold;
                var iconLE = niIcon.gameObject.AddComponent<LayoutElement>();
                iconLE.preferredHeight = cellIconSize;
                iconLE.preferredWidth = cellIconSize;
                iconLE.minHeight = cellIconSize;
                iconLE.minWidth = cellIconSize;
                iconLE.flexibleWidth = 0;
                var iconRT = niIcon.GetComponent<RectTransform>();
                iconRT.sizeDelta = new Vector2(cellIconSize, cellIconSize);
            }

            var niName = TxtGo(navInner.transform, navName, (int)cellLabelFont, isCenter ? UIDesignTokens.Colors.Primary : UIDesignTokens.Colors.TextSecondary);
            niName.alignment = TextAnchor.MiddleCenter;
            niName.fontStyle = FontStyle.Bold;
            niName.horizontalOverflow = HorizontalWrapMode.Overflow;
            var nameLE = niName.gameObject.AddComponent<LayoutElement>();
            nameLE.preferredHeight = navLabelH;
            nameLE.minHeight = navLabelH;
            nameLE.flexibleWidth = 1;
            nameLE.flexibleHeight = 0;

            navCell.GetComponent<Button>().targetGraphic = navCellImg;
            var navBtnColors = navCell.GetComponent<Button>().colors;
            navBtnColors.normalColor = navCellImg.color;
            navBtnColors.highlightedColor = Color.Lerp(navCellImg.color, Color.white, 0.12f);
            navBtnColors.pressedColor = Color.Lerp(navCellImg.color, Color.black, 0.15f);
            navCell.GetComponent<Button>().colors = navBtnColors;

            // 添加微交互 - 使用强调色实现hover/press状态
            var feedback = navCell.AddComponent<ButtonPressFeedback>();
            if (isCenter)
                feedback.SetAccentColor(UIDesignTokens.Colors.Primary);

            navCell.GetComponent<Button>().onClick.AddListener(() => OnNavButtonClick(navName));
        }

        // 强制立即刷新布局, 确保导航栏图标尺寸生效
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(navHL.GetComponent<RectTransform>());
    }

    void BuildMenuTopBar(Transform parent)
    {
        // 使用设计令牌 - 统一管理顶部栏尺寸
        float topBarHeight = UIDesignTokens.Component.TopBarHeight;
        float topBarFont = UIDesignTokens.Component.TopBarFont;
        float topBarSmallFont = UIDesignTokens.Component.TopBarSmallFont;
        float avatarSize = UIDesignTokens.Component.AvatarSize;
        float avatarFontSize = UIDesignTokens.Component.AvatarFont;

        var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        topBar.transform.SetParent(parent, false);
        var topBarImg = topBar.GetComponent<Image>();
        topBarImg.color = UIDesignTokens.Colors.TopBarBg;
        topBarImg.raycastTarget = false;
        topBar.GetComponent<CanvasGroup>().alpha = 1f;
        var topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.anchorMin = new Vector2(0, 1);
        topBarRT.anchorMax = new Vector2(1, 1);
        topBarRT.offsetMin = new Vector2(0, -topBarHeight);
        topBarRT.offsetMax = Vector2.zero;

        // 头像按钮
        var avatarBtn = new GameObject("AvatarBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        _avatarBtn = avatarBtn;
        avatarBtn.transform.SetParent(_root, false);
        var avRT = avatarBtn.GetComponent<RectTransform>();
        avRT.anchorMin = new Vector2(0, 1); avRT.anchorMax = new Vector2(0, 1);
        avRT.pivot = new Vector2(0, 1);
        avRT.anchoredPosition = new Vector2(UIDesignTokens.Space.M + 2, -(UIDesignTokens.Space.M + 1));
        avRT.sizeDelta = new Vector2(avatarSize, avatarSize);
        avatarBtn.GetComponent<Image>().color = new Color(0.06f, 0.12f, 0.14f, 0.95f);
        var avOL = avatarBtn.GetComponent<Outline>();
        avOL.effectColor = UIDesignTokens.Colors.PrimaryBorder;
        avOL.effectDistance = new Vector2(UIDesignTokens.Effect.OutlineNormal * 2, UIDesignTokens.Effect.OutlineNormal * 2);
        if (_avatarTex != null)
        {
            BuildAspectIcon(avatarBtn.transform, _avatarTex, (int)UIDesignTokens.Space.S);
        }
        else
        {
            var avIcon = TxtAnchored(avRT, "?", (int)avatarFontSize, Bright, Vector2.zero, Vector2.one, 0, 0);
            avIcon.alignment = TextAnchor.MiddleCenter;
        }
        avatarBtn.GetComponent<Button>().targetGraphic = avatarBtn.GetComponent<Image>();
        avatarBtn.GetComponent<Button>().onClick.AddListener(() => ShowProfilePanel());

        // 玩家名字
        _menuPlayerName = TxtAnchored(topBarRT, "寄生体", (int)topBarFont, UIDesignTokens.Colors.TextBright, new Vector2(0, 0.52f), new Vector2(0.55f, 1), 70, 0);
        _menuPlayerName.alignment = TextAnchor.MiddleLeft;
        _menuPlayerName.fontStyle = FontStyle.Bold;

        _menuPlayerLevel = TxtAnchored(topBarRT, "", (int)topBarSmallFont, UIDesignTokens.Colors.TextSecondary, new Vector2(0, 0.05f), new Vector2(0.55f, 0.50f), 70, 0);
        _menuPlayerLevel.alignment = TextAnchor.MiddleLeft;

        // EP resource
        var epLabel = TxtAnchored(topBarRT, "演化能级", (int)topBarSmallFont, UIDesignTokens.Colors.TextSecondary, new Vector2(0.58f, 0.55f), new Vector2(0.78f, 0.9f), 0, 0);
        epLabel.alignment = TextAnchor.MiddleRight;
        _menuEpTxt = TxtAnchored(topBarRT, "0", (int)topBarFont, UIDesignTokens.Colors.Primary, new Vector2(0.78f, 0.52f), new Vector2(0.98f, 0.95f), 4, 0);
        _menuEpTxt.alignment = TextAnchor.MiddleLeft;
        _menuEpTxt.fontStyle = FontStyle.Bold;

        // KO resource
        var koLabel = TxtAnchored(topBarRT, "回收样本", (int)topBarSmallFont, UIDesignTokens.Colors.TextSecondary, new Vector2(0.58f, 0.1f), new Vector2(0.78f, 0.5f), 0, 0);
        koLabel.alignment = TextAnchor.MiddleRight;
        _menuFragTxt = TxtAnchored(topBarRT, "0", (int)topBarFont, UIDesignTokens.Colors.Secondary, new Vector2(0.78f, 0.08f), new Vector2(0.98f, 0.5f), 4, 0);
        _menuFragTxt.alignment = TextAnchor.MiddleLeft;
        _menuFragTxt.fontStyle = FontStyle.Bold;

        // 分隔线
        var topSep = new GameObject("TopSep", typeof(RectTransform), typeof(Image));
        topSep.transform.SetParent(parent, false);
        var topSepRT = topSep.GetComponent<RectTransform>();
        topSepRT.anchorMin = new Vector2(0.04f, 1);
        topSepRT.anchorMax = new Vector2(0.96f, 1);
        topSepRT.offsetMin = new Vector2(0, -(topBarHeight - 2));
        topSepRT.offsetMax = new Vector2(0, -(topBarHeight - 4));
        var topSepImg = topSep.GetComponent<Image>();
        topSepImg.color = UIDesignTokens.Colors.PrimaryBorder;
        topSepImg.raycastTarget = false;
    }

    void BuildProfilePanel()
    {
        _profileOvl = Panel("ProfileOvl", OvlBgDense);
        _profileOvl.SetActive(false);
        var rt = _profileOvl.GetComponent<RectTransform>();

        var container = new GameObject("Container", typeof(RectTransform), typeof(Image), typeof(Outline));
        container.transform.SetParent(rt, false);
        container.GetComponent<Image>().color = DarkItemBg;
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
        if (_avatarTex != null)
        {
            BuildAspectIcon(avBig.transform, _avatarTex, 4);
        }
        else
        {
            var avBigIcon = TxtGo(avBig.transform, "?", 28, Cyan);
            avBigIcon.alignment = TextAnchor.MiddleCenter;
            Stretch(avBigIcon.gameObject);
        }

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

        Spacer(vl, SpaceMed);

        var logoutBtn = DangerBtn(vl.transform, "注 销", 16, 50);
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

        Spacer(vl, SpaceSmall);

        var closeBtn = PrimaryBtn(vl.transform, "关闭", 14, 44);
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

        _menuLastRunMode = TxtGo(dataRow.transform, "经典", 13, Bright);
        _menuLastRunMode.fontStyle = FontStyle.Bold;
        _menuLastRunMode.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

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
        btnImg.raycastTarget = true;

        var ol = btnGo.AddComponent<Outline>();
        ol.effectColor = filled ? new Color(0.4f, 0.35f, 0.55f, 0.35f) : ParasiteTowerArtOptimization.UIPolish.ButtonStyle.NormalBorder;
        ol.effectDistance = new Vector2(1, 1);

        var shadow = btnGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.36f);
        shadow.effectDistance = new Vector2(0, -2);

        var txtGo = new GameObject("T", typeof(RectTransform), typeof(Text), typeof(Outline));
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

        var textOl = txtGo.GetComponent<Outline>();
        textOl.effectColor = new Color(0, 0, 0, 0.45f);
        textOl.effectDistance = new Vector2(1, 1);

        var btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        var colors = btn.colors;
        colors.normalColor = bgColor;
        colors.highlightedColor = hoverColor;
        colors.pressedColor = Color.Lerp(bgColor, Color.black, 0.2f);
        colors.disabledColor = new Color(bgColor.r * 0.55f, bgColor.g * 0.55f, bgColor.b * 0.55f, 0.65f);
        btn.colors = colors;
        btnGo.AddComponent<ButtonPressFeedback>();

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
            bool isShop = name == "残响商店";
            Color col = colors[i];

            var cell = new GameObject("Nav_" + name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            cell.transform.SetParent(row.transform, false);

            var cellImg = cell.GetComponent<Image>();
            cellImg.color = isShop ? new Color(0.10f, 0.07f, 0.16f) : new Color(0.08f, 0.06f, 0.12f);

            var ol = cell.GetComponent<Outline>();
            ol.effectColor = isShop ? new Color(Gold.r, Gold.g, Gold.b, 0.4f) : new Color(0f, 1f, 0.8f, 0.35f);
            ol.effectDistance = new Vector2(1, 1);

            var sh = cell.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.24f);
            sh.effectDistance = new Vector2(0, -2);
            cell.AddComponent<ButtonPressFeedback>();

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
        TxtGo(row2.transform, $"第 {runNum} 次闯塔", 9, Dim);
    }

    Button FooterLink(Transform parent, string label, Color color, bool expand = false)
    {
        var go = new GameObject(label, typeof(RectTransform), typeof(Image), typeof(Button));
        go.transform.SetParent(parent, false);
        var bg = go.GetComponent<Image>();
        bg.color = new Color(0.08f, 0.06f, 0.12f, 0.35f);
        bg.raycastTarget = true;
        var ol = go.AddComponent<Outline>();
        ol.effectColor = new Color(color.r, color.g, color.b, 0.35f);
        ol.effectDistance = new Vector2(1, 1);

        var txt = TxtGo(go.transform, LocalizationData.T(label), 12, color);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.raycastTarget = false;
        Stretch(txt.gameObject);

        if (expand)
            go.AddComponent<LayoutElement>().flexibleWidth = 1;
        else
            go.AddComponent<LayoutElement>().preferredWidth = 60;

        var btn = go.GetComponent<Button>();
        btn.targetGraphic = bg;
        var colors = btn.colors;
        colors.normalColor = bg.color;
        colors.highlightedColor = new Color(color.r * 0.18f, color.g * 0.18f, color.b * 0.18f, 0.75f);
        colors.pressedColor = new Color(0.05f, 0.04f, 0.08f, 0.9f);
        btn.colors = colors;
        go.AddComponent<ButtonPressFeedback>();
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
        var tex = !string.IsNullOrEmpty(texName) ? LoadTex("UI/" + texName) : null;

        // 使用设计令牌 - 基于参考分辨率 (540x960)
        float btnSize = 26f;  // 原44 ×0.6
        float labelH = UIDesignTokens.Component.SideLabelH;
        float labelFontSize = UIDesignTokens.Component.SideLabelFont;
        float iconSize = 22f;  // 原36 ×0.6
        float padXS = UIDesignTokens.Space.XS;
        float padS = UIDesignTokens.Space.S;

        // 创建按钮容器（包含图标和标签）
        var container = new GameObject("Side_" + navTarget, typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(CanvasGroup));
        container.GetComponent<CanvasGroup>().alpha = 1f;
        container.transform.SetParent(parent, false);
        var cRT = container.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(anchorX, anchorY);
        cRT.anchorMax = new Vector2(anchorX, anchorY);
        cRT.pivot = new Vector2(0.5f, 0.5f);
        float containerW = btnSize + padS * 2;
        float containerH = btnSize + labelH + padS * 2 + padXS;
        cRT.sizeDelta = new Vector2(containerW, containerH);

        var vlg = container.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = padXS;
        vlg.padding = new RectOffset((int)padXS, (int)padXS, (int)padXS, (int)padXS);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = false;
        vlg.childForceExpandHeight = false;
        vlg.childControlWidth = true;
        vlg.childControlHeight = true;

        // 图标按钮容器
        var btn = new GameObject("IconBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btn.transform.SetParent(container.transform, false);
        btn.GetComponent<Image>().color = new Color(0.07f, 0.05f, 0.12f, 0.55f);
        var btnOutline = btn.GetComponent<Outline>();
        btnOutline.effectColor = new Color(col.r, col.g, col.b, 0.25f);
        btnOutline.effectDistance = new Vector2(UIDesignTokens.Effect.OutlineNormal, UIDesignTokens.Effect.OutlineNormal);

        // 添加内层发光边框
        var btnGlowOutline = btn.AddComponent<Outline>();
        btnGlowOutline.effectColor = new Color(col.r, col.g, col.b, 0.08f);
        btnGlowOutline.effectDistance = new Vector2(UIDesignTokens.Effect.OutlineLarge * 2, UIDesignTokens.Effect.OutlineLarge * 2);

        var btnLE = btn.AddComponent<LayoutElement>();
        btnLE.preferredWidth = btnSize;
        btnLE.preferredHeight = btnSize;
        btnLE.minWidth = btnSize;
        btnLE.minHeight = btnSize;

        // 强制立即刷新布局, 确保 sizeDelta 生效
        UnityEngine.UI.LayoutRebuilder.ForceRebuildLayoutImmediate(cRT);

        if (tex != null)
        {
            var iconWrap = new GameObject("IconWrap", typeof(RectTransform), typeof(LayoutElement));
            iconWrap.transform.SetParent(btn.transform, false);
            var wrapLE = iconWrap.GetComponent<LayoutElement>();
            wrapLE.preferredHeight = iconSize;
            wrapLE.preferredWidth = iconSize;
            wrapLE.minHeight = iconSize;
            wrapLE.minWidth = iconSize;
            BuildAspectIcon(iconWrap.transform, tex, (int)padXS);
        }
        else
        {
            var icoTxt = TxtGo(btn.transform, label.Length > 0 ? label.Substring(0, 1) : "?", (int)(labelFontSize * 1.6f), col);
            icoTxt.alignment = TextAnchor.MiddleCenter;
            icoTxt.fontStyle = FontStyle.Bold;
            var icoLE = icoTxt.gameObject.AddComponent<LayoutElement>();
            icoLE.preferredHeight = iconSize;
            icoLE.preferredWidth = iconSize;
            icoLE.minHeight = iconSize;
            icoLE.minWidth = iconSize;
            Stretch(icoTxt.gameObject);
        }

        // 标签
        var lblTxt = TxtGo(container.transform, label, (int)labelFontSize, UIDesignTokens.Colors.TextSecondary);
        lblTxt.alignment = TextAnchor.MiddleCenter;
        lblTxt.raycastTarget = false;
        lblTxt.fontStyle = FontStyle.Bold;
        lblTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var lblLE = lblTxt.gameObject.AddComponent<LayoutElement>();
        lblLE.preferredHeight = labelH;
        lblLE.preferredWidth = btnSize + padS * 2;
        lblLE.minHeight = labelH;
        lblLE.minWidth = btnSize + padS * 2;
        lblLE.flexibleWidth = 0;

        btn.GetComponent<Button>().targetGraphic = btn.GetComponent<Image>();
        var sideBtn = btn.GetComponent<Button>();
        var sideBtnImg = btn.GetComponent<Image>();
        var sideColors = sideBtn.colors;
        sideColors.normalColor = sideBtnImg.color;
        sideColors.highlightedColor = Color.Lerp(sideBtnImg.color, new Color(col.r * 0.3f, col.g * 0.3f, col.b * 0.3f, 0.8f), 1f);
        sideColors.pressedColor = Color.Lerp(sideBtnImg.color, new Color(col.r * 0.15f, col.g * 0.15f, col.b * 0.15f, 0.9f), 1f);
        sideBtn.colors = sideColors;

        // 添加微交互反馈 - 使用自定义强调色实现hover/press状态变化
        var feedback = btn.AddComponent<ButtonPressFeedback>();
        feedback.SetAccentColor(col);

        btn.GetComponent<Button>().onClick.AddListener(() => OnNavButtonClick(navTarget));
    }

    void SyncMenuPlayerBar()
    {
        var auth = GuestAuthManager.Instance;
        if (auth == null || !auth.IsLoggedIn) return;

        ST(_menuPlayerName, auth.Profile.nickname);
        int runs = MetaProgressSystem.Instance?.metaData.totalGamesPlayed ?? 0;
        string phase = runs == 0 ? "初始感染" : runs < 5 ? "稳定侵蚀" : runs < 15 ? "深度同化" : "完全融合";
        ST(_menuPlayerLevel, $"第{runs + 1}次闯塔 · {phase}");

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
            case "暗塔排行": ShowLeaderboardPanel(); break;
            case "终端设置": case "设置": ShowSettingsPanel(); break;
            case "残响商店": case "商店": ShowShopPanel(); break;
            case "挑战模式": ShowChallengePanel(); break;
            case "闯塔": GoToModeSelect(); break;
            case "记录管理": ShowRecordPanel(); break;
            case "残响圣坛": ShowEchoAltarPanel(); break;
            case "每日登录": ShowDailyRewardPanel(); break;
        }
    }

    static string GetModeDisplayName(string gameMode)
    {
        if (string.IsNullOrEmpty(gameMode)) return "经典";
        switch (gameMode)
        {
            case "Classic": return "经典";
            case "Short": return "暗影";
            case "Expedition": return "远征";
            case "Daily": return "每日挑战";
            case "Weekly": return "每周挑战";
            default: return gameMode;
        }
    }

    bool IsNavModuleUnlocked(string navName)
    {
        switch (navName)
        {
            case "成就": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Achievements);
            case "图鉴": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Bestiary);
            case "闯塔": return true;
            case "设置": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Settings);
            case "商店": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Shop);
            default: return true;
        }
    }

    static CompleteGameSystem.ChallengeModifier FindChallengeModById(string id)
    {
        if (string.IsNullOrEmpty(id)) return null;
        return CompleteGameSystem.FindModifierById(id);
    }


}
