using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;
using System;

public class MainMenuUI : MonoBehaviour
{
    Canvas _canvas;
    RectTransform _root;
    Font _font;

    // Challenge panel
    GameObject _challengePanel;
    InputField _customSeedInput;

    Font F()
    {
        if (_font != null) return _font;
        // 统一走 ThemeUIFonts,保证中文字体覆盖一致
        _font = ThemeUIFonts.Get(16);
        return _font;
    }

    static readonly Color Cyan = new Color(0.15f, 0.95f, 0.85f);
    static readonly Color Gold = new Color(1f, 0.75f, 0.3f);
    static readonly Color Purp = new Color(0.65f, 0.25f, 0.95f);
    static readonly Color Dim = new Color(0.4f, 0.4f, 0.55f);
    static readonly Color Bright = new Color(0.98f, 0.98f, 0.99f);
    static readonly Color PanelBg = new Color(0.05f, 0.03f, 0.1f);
    static readonly Color CardBg = new Color(0.08f, 0.06f, 0.14f);

    void Awake()
    {
        Init();
    }

    void Init()
    {
        var go = new GameObject("MainMenuCanvas");
        go.transform.SetParent(transform);
        _canvas = go.AddComponent<Canvas>();
        _canvas.renderMode = RenderMode.ScreenSpaceOverlay;
        // 与 CanvasUIManager 统一,避免两套 Canvas 叠加冲突
        _canvas.sortingOrder = 200;
        var sc = go.AddComponent<CanvasScaler>();
        sc.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        sc.referenceResolution = new Vector2(540, 960);
        sc.matchWidthOrHeight = 0.5f;
        go.AddComponent<GraphicRaycaster>();
        _root = go.GetComponent<RectTransform>();

        BuildMainMenu();
        BuildChallengePanel();
    }

    void BuildMainMenu()
    {
        var bgGo = new GameObject("Background", typeof(RectTransform), typeof(Image));
        bgGo.transform.SetParent(_root, false);
        var bgRT = bgGo.GetComponent<RectTransform>();
        bgRT.anchorMin = Vector2.zero;
        bgRT.anchorMax = Vector2.one;
        bgRT.offsetMin = Vector2.zero;
        bgRT.offsetMax = Vector2.zero;
        var bgImg = bgGo.GetComponent<Image>();
        bgImg.color = new Color(0.03f, 0.02f, 0.05f);

        var vl = new GameObject("MainVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        vl.transform.SetParent(_root, false);
        var vlRT = vl.GetComponent<RectTransform>();
        vlRT.anchorMin = new Vector2(0.05f, 0.05f);
        vlRT.anchorMax = new Vector2(0.95f, 0.95f);
        vlRT.offsetMin = Vector2.zero;
        vlRT.offsetMax = Vector2.zero;
        var vlg = vl.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 0;
        vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        Spacer(vl, 100);

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(vl.transform, false);
        var titleRT = titleGo.GetComponent<RectTransform>();
        titleRT.sizeDelta = new Vector2(0, 60);
        var titleTxt = titleGo.GetComponent<Text>();
        titleTxt.font = F();
        titleTxt.fontSize = 42;
        titleTxt.color = Bright;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.text = "你也是我";

        Spacer(vl, 15);

        var subtitleGo = new GameObject("Subtitle", typeof(RectTransform), typeof(Text));
        subtitleGo.transform.SetParent(vl.transform, false);
        var subtitleRT = subtitleGo.GetComponent<RectTransform>();
        subtitleRT.sizeDelta = new Vector2(0, 25);
        var subtitleTxt = subtitleGo.GetComponent<Text>();
        subtitleTxt.font = F();
        subtitleTxt.fontSize = 11;
        subtitleTxt.color = Dim;
        subtitleTxt.alignment = TextAnchor.MiddleCenter;
        subtitleTxt.text = "Y O U   A R E   A L S O   M E";

        Spacer(vl, 35);

        var loreTxt = new GameObject("LoreText", typeof(RectTransform), typeof(Text));
        loreTxt.transform.SetParent(vl.transform, false);
        var loreRT = loreTxt.GetComponent<RectTransform>();
        loreRT.sizeDelta = new Vector2(0, 25);
        var t = loreTxt.GetComponent<Text>();
        t.font = F();
        t.fontSize = 13;
        t.color = new Color(0.7f, 0.65f, 0.8f);
        t.alignment = TextAnchor.MiddleCenter;
        t.text = "你每夺走一个身体，就离自己更远一步。";

        Spacer(vl, 35);

        var saveDataInfo = SaveSystem.Instance?.LoadSaveData(0);
        bool hasValidSave = saveDataInfo != null && !string.IsNullOrEmpty(saveDataInfo.gameMode);

        var lastCard = new GameObject("LastRunCard", typeof(RectTransform), typeof(Image));
        lastCard.transform.SetParent(vl.transform, false);
        var lastCardRT = lastCard.GetComponent<RectTransform>();
        lastCardRT.sizeDelta = new Vector2(0, 160);
        lastCard.GetComponent<Image>().color = hasValidSave
            ? new Color(0.06f, 0.05f, 0.1f)
            : new Color(0.04f, 0.04f, 0.06f, 0.6f);
        lastCard.GetComponent<Image>().material = null;

        var cardBorder = new GameObject("CardBorder", typeof(RectTransform), typeof(Image));
        cardBorder.transform.SetParent(lastCard.transform, false);
        var cardBorderRT = cardBorder.GetComponent<RectTransform>();
        cardBorderRT.anchorMin = new Vector2(-1, -1);
        cardBorderRT.anchorMax = new Vector2(1, 1);
        cardBorderRT.offsetMin = new Vector2(-2, -2);
        cardBorderRT.offsetMax = new Vector2(2, 2);
        cardBorder.GetComponent<Image>().color = hasValidSave
            ? new Color(0.15f, 0.95f, 0.85f, 0.3f)
            : new Color(0.3f, 0.3f, 0.35f, 0.2f);

        var cardVL = new GameObject("CardVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        cardVL.transform.SetParent(lastCard.transform, false);
        var cardVLRT = cardVL.GetComponent<RectTransform>();
        cardVLRT.anchorMin = new Vector2(0.06f, 0.06f);
        cardVLRT.anchorMax = new Vector2(0.94f, 0.94f);
        cardVLRT.offsetMin = Vector2.zero;
        cardVLRT.offsetMax = Vector2.zero;
        var cardVLG = cardVL.GetComponent<VerticalLayoutGroup>();
        cardVLG.spacing = 10;
        cardVLG.childAlignment = TextAnchor.MiddleLeft;
        cardVLG.childForceExpandWidth = true;

        var headerRow = new GameObject("HeaderRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerRow.transform.SetParent(cardVL.transform, false);
        var headerHLG = headerRow.GetComponent<HorizontalLayoutGroup>();
        headerHLG.spacing = 8;
        headerHLG.childAlignment = TextAnchor.MiddleCenter;
        headerHLG.childForceExpandWidth = true;

        var recentTxt = new GameObject("RecentTxt", typeof(RectTransform), typeof(Text));
        recentTxt.transform.SetParent(headerRow.transform, false);
        var rt = recentTxt.GetComponent<Text>();
        rt.font = F();
        rt.fontSize = 11;
        rt.color = hasValidSave ? Cyan : Dim;
        rt.alignment = TextAnchor.MiddleLeft;
        rt.fontStyle = FontStyle.Bold;
        rt.text = hasValidSave ? "最近一次闯塔" : "暂无进行中的闯塔";

        var syncTxt = new GameObject("SyncTxt", typeof(RectTransform), typeof(Text));
        syncTxt.transform.SetParent(headerRow.transform, false);
        var syncTxtComp = syncTxt.GetComponent<Text>();
        syncTxtComp.font = F();
        syncTxtComp.fontSize = 10;
        syncTxtComp.color = Dim;
        syncTxtComp.alignment = TextAnchor.MiddleRight;
        syncTxtComp.text = hasValidSave ? $"F{saveDataInfo.currentFloor} · {saveDataInfo.saveTime:HH:mm}" : "等待开始";

        string[][] displayData;
        if (hasValidSave && saveDataInfo.player != null)
        {
            displayData = new[]
            {
                new[] {"模式", GetGameModeLabel(saveDataInfo.gameMode)},
                new[] {"楼层", $"F{saveDataInfo.currentFloor}"},
                new[] {"宿主", saveDataInfo.player.selectedClass ?? "-"},
                new[] {"形态", saveDataInfo.player.currentForm ?? "-"},
                new[] {"污染", $"{saveDataInfo.player.pollution:F1}%"},
                new[] {"记录", FormatTimeAgo(saveDataInfo.saveTime)}
            };
        }
        else
        {
            displayData = new[]
            {
                new[] {"模式", "-"},
                new[] {"楼层", "-"},
                new[] {"宿主", "-"},
                new[] {"形态", "-"},
                new[] {"污染", "-"},
                new[] {"记录", "-"}
            };
        }

        foreach (var row in displayData)
        {
            var dataRow = new GameObject("DataRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            dataRow.transform.SetParent(cardVL.transform, false);
            var drHLG = dataRow.GetComponent<HorizontalLayoutGroup>();
            drHLG.spacing = 16;
            drHLG.childAlignment = TextAnchor.MiddleLeft;

            var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
            labelGo.transform.SetParent(dataRow.transform, false);
            var labelRT = labelGo.GetComponent<RectTransform>();
            labelRT.sizeDelta = new Vector2(50, 0);
            var label = labelGo.GetComponent<Text>();
            label.font = F();
            label.fontSize = 10;
            label.color = Dim;
            label.alignment = TextAnchor.MiddleLeft;
            label.text = row[0];

            var valueGo = new GameObject("Value", typeof(RectTransform), typeof(Text));
            valueGo.transform.SetParent(dataRow.transform, false);
            var value = valueGo.GetComponent<Text>();
            value.font = F();
            value.fontSize = 12;
            value.color = Bright;
            value.alignment = TextAnchor.MiddleLeft;
            value.fontStyle = FontStyle.Bold;
            value.text = row[1];
        }

        var cardBtn = lastCard.AddComponent<Button>();
        cardBtn.interactable = hasValidSave;
        cardBtn.onClick.AddListener(() => ContinueGame());

        Spacer(vl, 30);

        var continueBtn = CreateContinueButton(vl.transform, hasValidSave);
        continueBtn.onClick.AddListener(ContinueGame);

        Spacer(vl, 12);

        var newBtn = CreateNewButton(vl.transform);
        newBtn.onClick.AddListener(StartNewGame);

        Spacer(vl, 40);

        var navTitle = new GameObject("NavTitle", typeof(RectTransform), typeof(Text));
        navTitle.transform.SetParent(vl.transform, false);
        var navTitleRT = navTitle.GetComponent<RectTransform>();
        navTitleRT.sizeDelta = new Vector2(0, 25);
        var navTitleTxt = navTitle.GetComponent<Text>();
        navTitleTxt.font = F();
        navTitleTxt.fontSize = 11;
        navTitleTxt.color = Dim;
        navTitleTxt.alignment = TextAnchor.MiddleCenter;
        navTitleTxt.text = "终端功能";

        Spacer(vl, 12);

        var navHL = new GameObject("NavHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        navHL.transform.SetParent(vl.transform, false);
        var navHLRT = navHL.GetComponent<RectTransform>();
        navHLRT.sizeDelta = new Vector2(0, 160);
        var navHLG = navHL.GetComponent<HorizontalLayoutGroup>();
        navHLG.spacing = 16;
        navHLG.childAlignment = TextAnchor.MiddleCenter;
        navHLG.childForceExpandWidth = false;

        var leftNav = new GameObject("LeftNav", typeof(RectTransform), typeof(VerticalLayoutGroup));
        leftNav.transform.SetParent(navHL.transform, false);
        var leftVL = leftNav.GetComponent<VerticalLayoutGroup>();
        leftVL.spacing = 8;
        leftVL.childAlignment = TextAnchor.MiddleCenter;

        string[][] leftItems = {
            new string[] {"★", "成就回响", "#00ffd0"},
            new string[] {"◆", "异种图鉴", "#ffd700"},
            new string[] {"◎", "记忆档案", "#ff006e"},
            new string[] {"◇", "残响圣坛", "#ffd700"}
        };

        foreach (var item in leftItems)
        {
            if (IsNavItemUnlocked(item[1]))
                CreateNavButton(leftNav.transform, item[0], item[1], item[2], false, false);
        }

        var centerSpacer = new GameObject("CenterSpacer");
        centerSpacer.transform.SetParent(navHL.transform, false);
        var centerLE = centerSpacer.AddComponent<LayoutElement>();
        centerLE.flexibleWidth = 1;

        var rightNav = new GameObject("RightNav", typeof(RectTransform), typeof(VerticalLayoutGroup));
        rightNav.transform.SetParent(navHL.transform, false);
        var rightVL = rightNav.GetComponent<VerticalLayoutGroup>();
        rightVL.spacing = 8;
        rightVL.childAlignment = TextAnchor.MiddleCenter;

        string[][] rightItems = {
            new string[] {"※", "暗塔排行", "#8844ff"},
            new string[] {"◇", "终端设置", "#999999"},
            new string[] {"†", "残响商店", "#ff9933"},
            new string[] {"◎", "每日登录", "#00ffd0"}
        };

        foreach (var item in rightItems)
        {
            if (IsNavItemUnlocked(item[1]))
                CreateNavButton(rightNav.transform, item[0], item[1], item[2], false, false);
        }

        Spacer(vl, 30);

        var footerTop = new GameObject("FooterTop", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footerTop.transform.SetParent(vl.transform, false);
        var footerTopRT = footerTop.GetComponent<RectTransform>();
        footerTopRT.sizeDelta = new Vector2(0, 25);
        var footerTopHLG = footerTop.GetComponent<HorizontalLayoutGroup>();
        footerTopHLG.spacing = 16;
        footerTopHLG.childAlignment = TextAnchor.MiddleCenter;
        footerTopHLG.childForceExpandWidth = false;

        CreateFooterItem(footerTop.transform, "残响圣坛", "245", true, null);
        CreateFooterItem(footerTop.transform, "每日登录", "未开放", false, null);
        CreateFooterItem(footerTop.transform, "记录管理", "", false, () => { CanvasUIManager.Instance?.ShowRecordPanel(); });

        Spacer(vl, 10);

        var footerBottom = new GameObject("FooterBottom", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footerBottom.transform.SetParent(vl.transform, false);
        var footerBottomRT = footerBottom.GetComponent<RectTransform>();
        footerBottomRT.sizeDelta = new Vector2(0, 20);
        var footerBottomHLG = footerBottom.GetComponent<HorizontalLayoutGroup>();
        footerBottomHLG.spacing = 8;
        footerBottomHLG.childAlignment = TextAnchor.MiddleCenter;
        footerBottomHLG.childForceExpandWidth = false;

        var verTxt = new GameObject("VerTxt", typeof(RectTransform), typeof(Text));
        verTxt.transform.SetParent(footerBottom.transform, false);
        var vt = verTxt.GetComponent<Text>();
        vt.font = F();
        vt.fontSize = 10;
        vt.color = Dim;
        vt.text = "v1.3.2";

        var sepTxt = new GameObject("SepTxt", typeof(RectTransform), typeof(Text));
        sepTxt.transform.SetParent(footerBottom.transform, false);
        var st = sepTxt.GetComponent<Text>();
        st.font = F();
        st.fontSize = 10;
        st.color = Dim;
        st.text = "·";

        var statusDot = new GameObject("StatusDot", typeof(RectTransform), typeof(Image));
        statusDot.transform.SetParent(footerBottom.transform, false);
        var sdRT = statusDot.GetComponent<RectTransform>();
        sdRT.sizeDelta = new Vector2(5, 5);
        var sdImg = statusDot.GetComponent<Image>();
        sdImg.color = Cyan;
        sdImg.material = null;

        var statusTxt = new GameObject("StatusTxt", typeof(RectTransform), typeof(Text));
        statusTxt.transform.SetParent(footerBottom.transform, false);
        var stt = statusTxt.GetComponent<Text>();
        stt.font = F();
        stt.fontSize = 10;
        stt.color = Dim;
        stt.text = "记录同步正常";
    }

    Button CreateContinueButton(Transform parent, bool hasSave)
    {
        var btnGo = new GameObject("Btn_Continue", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(parent, false);
        var btnRT = btnGo.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(320, 52);

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = hasSave ? new Color(0.1f, 0.08f, 0.14f) : new Color(0.06f, 0.05f, 0.08f);
        btnImg.material = null;

        var outline = btnGo.GetComponent<Outline>();
        outline.effectColor = hasSave ? new Color(0f, 1f, 0.8f, 0.35f) : new Color(0.3f, 0.3f, 0.35f, 0.2f);
        outline.effectDistance = new Vector2(1, 1);

        var shadow = btnGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.25f);
        shadow.effectDistance = new Vector2(0, -2);

        var txtGo = new GameObject("Txt", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(btnGo.transform, false);
        Stretch(txtGo);
        var txt = txtGo.GetComponent<Text>();
        txt.font = F();
        txt.fontSize = 17;
        txt.color = hasSave ? Bright : Dim;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontStyle = FontStyle.Bold;
        txt.text = hasSave ? "继续寄生" : "暂无存档";

        var btn = btnGo.GetComponent<Button>();
        btn.interactable = hasSave;
        btn.targetGraphic = btnImg;
        btn.colors = new ColorBlock { normalColor = btnImg.color, highlightedColor = new Color(0.15f,0.12f,0.18f,1f), pressedColor = new Color(0.07f,0.05f,0.09f,1f), disabledColor = new Color(0.05f,0.04f,0.06f,0.65f), colorMultiplier = 1f, fadeDuration = 0.1f };
        if (hasSave) btnGo.AddComponent<ButtonPressFeedback>();

        return btn;
    }

    Button CreateNewButton(Transform parent)
    {
        var btnGo = new GameObject("Btn_New", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(parent, false);
        var btnRT = btnGo.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(320, 48);

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = new Color(0.06f, 0.05f, 0.08f);
        btnImg.material = null;

        var outline = btnGo.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 1f, 0.8f, 0.35f);
        outline.effectDistance = new Vector2(1, 1);

        var shadow = btnGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.24f);
        shadow.effectDistance = new Vector2(0, -2);

        var txtGo = new GameObject("Txt", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(btnGo.transform, false);
        Stretch(txtGo);
        var txt = txtGo.GetComponent<Text>();
        txt.font = F();
        txt.fontSize = 15;
        txt.color = Cyan;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontStyle = FontStyle.Bold;
        txt.text = "新一轮闯塔";

        var btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        btn.colors = new ColorBlock { normalColor = btnImg.color, highlightedColor = new Color(0.12f,0.09f,0.14f,1f), pressedColor = new Color(0.08f,0.06f,0.1f,1f), disabledColor = new Color(0.04f,0.04f,0.06f,0.65f), colorMultiplier = 1f, fadeDuration = 0.1f };
        btnGo.AddComponent<ButtonPressFeedback>();

        return btn;
    }

    void CreateNavButton(Transform parent, string icon, string name, string colorHex, bool highlight, bool isChallenge = false)
    {
        Color color = ParseColor(colorHex);

        var btnGo = new GameObject("NavBtn_" + name, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(parent, false);
        var btnRT = btnGo.GetComponent<RectTransform>();
        btnRT.sizeDelta = new Vector2(100, 68);

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = highlight ? new Color(0.12f, 0.08f, 0.18f) : new Color(0.06f, 0.05f, 0.09f);
        var mat = Resources.Load<Material>("Materials/UI/Default");
        if (mat != null) btnImg.material = mat;

        var outline = btnGo.GetComponent<Outline>();
        outline.effectColor = highlight ? new Color(1f, 0.8f, 0.3f, 0.5f) : new Color(0.3f, 0.4f, 0.6f, 0.3f);
        outline.effectDistance = new Vector2(1, 1);

        if (highlight)
        {
            var borderGo = new GameObject("Border", typeof(RectTransform), typeof(Image));
            borderGo.transform.SetParent(btnGo.transform, false);
            var borderRT = borderGo.GetComponent<RectTransform>();
            borderRT.anchorMin = new Vector2(-1, -1);
            borderRT.anchorMax = new Vector2(1, 1);
            borderRT.offsetMin = new Vector2(-2, -2);
            borderRT.offsetMax = new Vector2(2, 2);
            var borderImg = borderGo.GetComponent<Image>();
            borderImg.color = Gold;
            var borderMat = Resources.Load<Material>("Materials/UI/Default");
            if (borderMat != null) borderImg.material = borderMat;
        }

        var innerVL = new GameObject("InnerVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        innerVL.transform.SetParent(btnGo.transform, false);
        var innerRT = innerVL.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = Vector2.zero;
        innerRT.offsetMax = Vector2.zero;
        var innerVLG = innerVL.GetComponent<VerticalLayoutGroup>();
        innerVLG.spacing = 4;
        innerVLG.padding = new RectOffset(4, 4, 8, 4);
        innerVLG.childAlignment = TextAnchor.MiddleCenter;

        var iconGo = new GameObject("Icon", typeof(RectTransform), typeof(Text));
        iconGo.transform.SetParent(innerVL.transform, false);
        var iconRT = iconGo.GetComponent<RectTransform>();
        iconRT.sizeDelta = new Vector2(0, 30);
        var iconTxt = iconGo.GetComponent<Text>();
        iconTxt.font = F();
        iconTxt.fontSize = 22;
        iconTxt.color = color;
        iconTxt.alignment = TextAnchor.MiddleCenter;
        iconTxt.text = icon;

        var nameGo = new GameObject("Name", typeof(RectTransform), typeof(Text));
        nameGo.transform.SetParent(innerVL.transform, false);
        var nameRT = nameGo.GetComponent<RectTransform>();
        nameRT.sizeDelta = new Vector2(0, 18);
        var nameTxt = nameGo.GetComponent<Text>();
        nameTxt.font = F();
        nameTxt.fontSize = 10;
        nameTxt.color = highlight ? Gold : Bright;
        nameTxt.alignment = TextAnchor.MiddleCenter;
        nameTxt.text = name;

        var btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = btnImg;
        
        // 添加悬停效果
        var eventTrigger = btnGo.AddComponent<EventTrigger>();
        
        var enterTrigger = new EventTrigger.Entry();
        enterTrigger.eventID = EventTriggerType.PointerEnter;
        enterTrigger.callback.AddListener((data) => NavButtonHover(btnGo, color, highlight, true));
        eventTrigger.triggers.Add(enterTrigger);
        
        var exitTrigger = new EventTrigger.Entry();
        exitTrigger.eventID = EventTriggerType.PointerExit;
        exitTrigger.callback.AddListener((data) => NavButtonHover(btnGo, color, highlight, false));
        eventTrigger.triggers.Add(exitTrigger);
        
        // 添加点击动画
        if (isChallenge)
        {
            btn.onClick.AddListener(() => { 
                StartCoroutine(NavButtonClickAnim(btnGo));
                OpenChallengePanel(); 
            });
        }
        else
        {
            btn.onClick.AddListener(() => { 
                StartCoroutine(NavButtonClickAnim(btnGo));
                OnNavButtonClick(name); 
            });
        }
    }

    void NavButtonHover(GameObject btnGo, Color iconColor, bool isHighlight, bool isEnter)
    {
        var btnImg = btnGo.GetComponent<Image>();
        var outline = btnGo.GetComponent<Outline>();
        var iconTxt = btnGo.transform.Find("InnerVL/Icon")?.GetComponent<Text>();
        var nameTxt = btnGo.transform.Find("InnerVL/Name")?.GetComponent<Text>();
        
        if (isEnter)
        {
            btnImg.color = new Color(0.15f, 0.12f, 0.22f);
            outline.effectColor = new Color(0.3f, 1f, 0.9f, 0.7f);
            outline.effectDistance = new Vector2(2, 2);
            if (iconTxt != null)
            {
                iconTxt.fontSize = 24;
                iconTxt.color = new Color(0.3f, 1f, 0.9f);
            }
            if (nameTxt != null)
            {
                nameTxt.fontSize = 11;
                nameTxt.color = new Color(0.9f, 0.95f, 1f);
            }
            btnGo.transform.localScale = Vector3.Lerp(btnGo.transform.localScale, new Vector3(1.05f, 1.05f, 1f), 0.2f);
        }
        else
        {
            btnImg.color = isHighlight ? new Color(0.12f, 0.08f, 0.18f) : new Color(0.06f, 0.05f, 0.09f);
            outline.effectColor = isHighlight ? new Color(1f, 0.8f, 0.3f, 0.5f) : new Color(0.3f, 0.4f, 0.6f, 0.3f);
            outline.effectDistance = new Vector2(1, 1);
            if (iconTxt != null)
            {
                iconTxt.fontSize = 22;
                iconTxt.color = iconColor;
            }
            if (nameTxt != null)
            {
                nameTxt.fontSize = 10;
                nameTxt.color = isHighlight ? Gold : Bright;
            }
            btnGo.transform.localScale = Vector3.Lerp(btnGo.transform.localScale, Vector3.one, 0.2f);
        }
    }

    IEnumerator NavButtonClickAnim(GameObject btnGo)
    {
        Vector3 originalScale = btnGo.transform.localScale;
        
        // 按下缩小
        btnGo.transform.localScale = new Vector3(0.92f, 0.92f, 1f);
        yield return new WaitForSeconds(0.06f);
        
        // 弹回
        btnGo.transform.localScale = new Vector3(1.08f, 1.08f, 1f);
        yield return new WaitForSeconds(0.05f);
        
        // 恢复原状
        btnGo.transform.localScale = originalScale;
    }

    void CreateFooterItem(Transform parent, string labelText, string value, bool hasValue, Action onClick)
    {
        var itemGo = new GameObject("FooterItem", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Button));
        itemGo.transform.SetParent(parent, false);
        var itemHLG = itemGo.GetComponent<HorizontalLayoutGroup>();
        itemHLG.spacing = 4;
        itemHLG.childAlignment = TextAnchor.MiddleCenter;

        var labelGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        labelGo.transform.SetParent(itemGo.transform, false);
        var labelComp = labelGo.GetComponent<Text>();
        labelComp.font = F();
        labelComp.fontSize = 10;
        labelComp.color = onClick != null ? Cyan : Dim;
        labelComp.text = labelText;

        if (hasValue)
        {
            var valueGo = new GameObject("Value", typeof(RectTransform), typeof(Text));
            valueGo.transform.SetParent(itemGo.transform, false);
            var valueTxt = valueGo.GetComponent<Text>();
            valueTxt.font = F();
            valueTxt.fontSize = 10;
            valueTxt.color = Cyan;
            valueTxt.fontStyle = FontStyle.Bold;
            valueTxt.text = "[" + value + "]";
        }
        else
        {
            var dotGo = new GameObject("Dot", typeof(RectTransform), typeof(Image));
            dotGo.transform.SetParent(itemGo.transform, false);
            var dotRT = dotGo.GetComponent<RectTransform>();
            dotRT.sizeDelta = new Vector2(4, 4);
            var dotImg = dotGo.GetComponent<Image>();
            dotImg.color = new Color(1f, 0.3f, 0.5f);
            dotImg.material = null;
        }

        if (onClick != null)
        {
            var btn = itemGo.GetComponent<Button>();
            btn.onClick.AddListener(() => { StartCoroutine(NavButtonClickAnim(itemGo)); onClick(); });
        }
    }

    Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        return new Color(r, g, b);
    }

    void Spacer(GameObject parent, int height)
    {
        var go = new GameObject("Spacer", typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, height);
    }

    void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    void ContinueGame()
    {
        var saveData = SaveSystem.Instance?.LoadSaveData(0);
        if (saveData == null || string.IsNullOrEmpty(saveData.gameMode))
        {
            ShowNotification("没有可继续的存档");
            return;
        }

        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.LoadGame(0);
    }

    void StartNewGame()
    {
        var gm = GameManager.Instance;
        if (gm == null) return;
        gm.StartNewGame("Classic");
    }

    string GetGameModeLabel(string mode)
    {
        switch (mode)
        {
            case "Classic": return "经典模式";
            case "Expedition": return "远征模式";
            case "Short": return "暗影模式";
            default: return mode ?? "-";
        }
    }

    string FormatTimeAgo(System.DateTime dt)
    {
        var diff = System.DateTime.Now - dt;
        if (diff.TotalMinutes < 1) return "刚刚";
        if (diff.TotalMinutes < 60) return $"{(int)diff.TotalMinutes}分钟前";
        if (diff.TotalHours < 24) return $"{(int)diff.TotalHours}小时前";
        if (diff.TotalDays < 30) return $"{(int)diff.TotalDays}天前";
        return dt.ToString("yyyy-MM-dd");
    }

    void ShowNotification(string msg)
    {
        if (CanvasUIManager.Instance != null)
        {
            CanvasUIManager.Instance.ShowFlashBanner(msg, Color.white, 1.5f);
        }
    }

    void OnNavButtonClick(string name)
    {
        switch (name)
        {
            case "成就回响": CanvasUIManager.Instance?.ShowAchievementPanel(); break;
            case "异种图鉴": CanvasUIManager.Instance?.ShowBestiaryPanel(); break;
            case "记忆档案": CanvasUIManager.Instance?.ShowFragmentPanel(); break;
            case "暗塔排行": CanvasUIManager.Instance?.ShowLeaderboardPanel(); break;
            case "终端设置": CanvasUIManager.Instance?.ShowSettingsPanel(); break;
            case "挑战模式": OpenChallengePanel(); break;
            case "残响商店": CanvasUIManager.Instance?.ShowShopPanel(); break;
            case "残响圣坛": CanvasUIManager.Instance?.ShowEchoAltarPanel(); break;
            case "每日登录": CanvasUIManager.Instance?.ShowDailyRewardPanel(); break;
        }
    }

    void OpenExternalShop()
    {
        Application.OpenURL("https://example.com/shop");
    }

    void BuildChallengePanel()
    {
        _challengePanel = new GameObject("ChallengePanel", typeof(RectTransform), typeof(Image));
        _challengePanel.transform.SetParent(_root, false);
        var panelRT = _challengePanel.GetComponent<RectTransform>();
        panelRT.anchorMin = Vector2.zero;
        panelRT.anchorMax = Vector2.one;
        panelRT.offsetMin = Vector2.zero;
        panelRT.offsetMax = Vector2.zero;
        
        var panelImg = _challengePanel.GetComponent<Image>();
        panelImg.color = new Color(0.03f, 0.02f, 0.05f, 0.95f);

        var mainVL = new GameObject("MainVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        mainVL.transform.SetParent(_challengePanel.transform, false);
        var mainVLRT = mainVL.GetComponent<RectTransform>();
        mainVLRT.anchorMin = new Vector2(0.05f, 0.05f);
        mainVLRT.anchorMax = new Vector2(0.95f, 0.95f);
        mainVLRT.offsetMin = Vector2.zero;
        mainVLRT.offsetMax = Vector2.zero;
        var mainVLG = mainVL.GetComponent<VerticalLayoutGroup>();
        mainVLG.spacing = 16;
        mainVLG.padding = new RectOffset(16, 16, 16, 16);
        mainVLG.childAlignment = TextAnchor.MiddleCenter;

        Spacer(mainVL.transform, 60);

        var titleVL = new GameObject("TitleVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        titleVL.transform.SetParent(mainVL.transform, false);
        var titleVLG = titleVL.GetComponent<VerticalLayoutGroup>();
        titleVLG.spacing = 4;
        titleVLG.childAlignment = TextAnchor.MiddleCenter;

        var titleGo = new GameObject("Title", typeof(RectTransform), typeof(Text));
        titleGo.transform.SetParent(titleVL.transform, false);
        var titleTxt = titleGo.GetComponent<Text>();
        titleTxt.font = F();
        titleTxt.fontSize = 24;
        titleTxt.color = Gold;
        titleTxt.alignment = TextAnchor.MiddleCenter;
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.text = "今日挑战";

        var subtitleGo = new GameObject("Subtitle", typeof(RectTransform), typeof(Text));
        subtitleGo.transform.SetParent(titleVL.transform, false);
        var subtitleTxt = subtitleGo.GetComponent<Text>();
        subtitleTxt.font = F();
        subtitleTxt.fontSize = 10;
        subtitleTxt.color = Dim;
        subtitleTxt.alignment = TextAnchor.MiddleCenter;
        subtitleTxt.text = "DAILY ANOMALY";

        Spacer(mainVL.transform, 20);

        var tabHL = new GameObject("TabHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabHL.transform.SetParent(mainVL.transform, false);
        var tabHLRT = tabHL.GetComponent<RectTransform>();
        tabHLRT.sizeDelta = new Vector2(0, 44);
        var tabHLG = tabHL.GetComponent<HorizontalLayoutGroup>();
        tabHLG.spacing = 8;
        tabHLG.childAlignment = TextAnchor.MiddleCenter;
        tabHLG.childForceExpandWidth = true;

        var dailyBtn = CreateTabButton(tabHL.transform, "每日挑战", true);
        var weeklyBtn = CreateTabButton(tabHL.transform, "本周挑战", false);

        Spacer(mainVL.transform, 10);

        var descCard = new GameObject("DescCard", typeof(RectTransform), typeof(Image));
        descCard.transform.SetParent(mainVL.transform, false);
        var descCardRT = descCard.GetComponent<RectTransform>();
        descCardRT.sizeDelta = new Vector2(0, 80);
        descCard.GetComponent<Image>().color = new Color(0.12f, 0.08f, 0.20f);

        var descBorder = new GameObject("DescBorder", typeof(RectTransform), typeof(Image));
        descBorder.transform.SetParent(descCard.transform, false);
        var descBorderRT = descBorder.GetComponent<RectTransform>();
        descBorderRT.anchorMin = new Vector2(-1, -1);
        descBorderRT.anchorMax = new Vector2(1, 1);
        descBorderRT.offsetMin = new Vector2(-2, -2);
        descBorderRT.offsetMax = new Vector2(2, 2);
        descBorder.GetComponent<Image>().color = new Color(0, 1f, 0.816f, 0.3f);

        var descVL = new GameObject("DescVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        descVL.transform.SetParent(descCard.transform, false);
        var descVLRT = descVL.GetComponent<RectTransform>();
        descVLRT.anchorMin = new Vector2(0.05f, 0.05f);
        descVLRT.anchorMax = new Vector2(0.95f, 0.95f);
        var descVLG = descVL.GetComponent<VerticalLayoutGroup>();
        descVLG.spacing = 4;

        var descTxtGo = new GameObject("DescTxt", typeof(RectTransform), typeof(Text));
        descTxtGo.transform.SetParent(descVL.transform, false);
        var descTxt = descTxtGo.GetComponent<Text>();
        descTxt.font = F();
        descTxt.fontSize = 13;
        descTxt.color = new Color(0, 1f, 0.816f);
        descTxt.alignment = TextAnchor.MiddleCenter;
        descTxt.text = "赏金猎人: 击杀奖励EP x 2 但怪物HP x 1.5";

        Spacer(mainVL.transform, 10);

        var startBtn = new GameObject("StartBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        startBtn.transform.SetParent(mainVL.transform, false);
        var startBtnRT = startBtn.GetComponent<RectTransform>();
        startBtnRT.sizeDelta = new Vector2(0, 50);

        var startBtnImg = startBtn.GetComponent<Image>();
        startBtnImg.color = new Color(0.15f, 0.08f, 0.08f);

        var startBorder = new GameObject("StartBorder", typeof(RectTransform), typeof(Image));
        startBorder.transform.SetParent(startBtn.transform, false);
        var startBorderRT = startBorder.GetComponent<RectTransform>();
        startBorderRT.anchorMin = new Vector2(-1, -1);
        startBorderRT.anchorMax = new Vector2(1, 1);
        startBorderRT.offsetMin = new Vector2(-2, -2);
        startBorderRT.offsetMax = new Vector2(2, 2);
        startBorder.GetComponent<Image>().color = new Color(1f, 0.3f, 0.3f, 0.5f);

        var startBtnComp = startBtn.GetComponent<Button>();
        startBtnComp.targetGraphic = startBtnImg;
        startBtnComp.onClick.AddListener(StartDailyChallenge);

        var startTxtGo = new GameObject("StartTxt", typeof(RectTransform), typeof(Text));
        startTxtGo.transform.SetParent(startBtn.transform, false);
        var startTxt = startTxtGo.GetComponent<Text>();
        startTxt.font = F();
        startTxt.fontSize = 16;
        startTxt.color = Gold;
        startTxt.alignment = TextAnchor.MiddleCenter;
        startTxt.fontStyle = FontStyle.Bold;
        startTxt.text = "开始每日挑战";

        Spacer(mainVL.transform, 20);

        Separator(mainVL.transform);

        Spacer(mainVL.transform, 10);

        var customTitleGo = new GameObject("CustomTitle", typeof(RectTransform), typeof(Text));
        customTitleGo.transform.SetParent(mainVL.transform, false);
        var customTitleTxt = customTitleGo.GetComponent<Text>();
        customTitleTxt.font = F();
        customTitleTxt.fontSize = 14;
        customTitleTxt.color = new Color(0.8f, 0.6f, 1f);
        customTitleTxt.alignment = TextAnchor.MiddleCenter;
        customTitleTxt.fontStyle = FontStyle.Bold;
        customTitleTxt.text = "自定义种子";

        Spacer(mainVL.transform, 8);

        var seedHL = new GameObject("SeedHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        seedHL.transform.SetParent(mainVL.transform, false);
        var seedHLRT = seedHL.GetComponent<RectTransform>();
        seedHLRT.sizeDelta = new Vector2(0, 44);
        var seedHLG = seedHL.GetComponent<HorizontalLayoutGroup>();
        seedHLG.spacing = 8;
        seedHLG.childForceExpandWidth = true;

        var seedInputGo = new GameObject("SeedInput", typeof(RectTransform), typeof(Image), typeof(InputField));
        seedInputGo.transform.SetParent(seedHL.transform, false);
        var seedInputRT = seedInputGo.GetComponent<RectTransform>();
        seedInputRT.sizeDelta = new Vector2(200, 0);

        var seedInputImg = seedInputGo.GetComponent<Image>();
        seedInputImg.color = new Color(0.12f, 0.08f, 0.18f);

        _customSeedInput = seedInputGo.GetComponent<InputField>();
        var placeholderGo = new GameObject("Placeholder", typeof(RectTransform), typeof(Text));
        placeholderGo.transform.SetParent(seedInputGo.transform, false);
        var placeholderTxt = placeholderGo.GetComponent<Text>();
        placeholderTxt.font = F();
        placeholderTxt.fontSize = 14;
        placeholderTxt.color = Dim;
        placeholderTxt.alignment = TextAnchor.MiddleCenter;
        placeholderTxt.text = "SEED";
        _customSeedInput.placeholder = placeholderTxt;

        var textComponentGo = new GameObject("TextComponent", typeof(RectTransform), typeof(Text));
        textComponentGo.transform.SetParent(seedInputGo.transform, false);
        var textComponentTxt = textComponentGo.GetComponent<Text>();
        textComponentTxt.font = F();
        textComponentTxt.fontSize = 14;
        textComponentTxt.color = Bright;
        textComponentTxt.alignment = TextAnchor.MiddleCenter;
        _customSeedInput.textComponent = textComponentTxt;

        var seedStartBtn = new GameObject("SeedStartBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        seedStartBtn.transform.SetParent(seedHL.transform, false);
        var seedStartBtnRT = seedStartBtn.GetComponent<RectTransform>();
        seedStartBtnRT.sizeDelta = new Vector2(100, 0);

        var seedStartImg = seedStartBtn.GetComponent<Image>();
        seedStartImg.color = new Color(0.12f, 0.08f, 0.20f);

        var seedStartOutline = seedStartBtn.GetComponent<Outline>();
        seedStartOutline.effectColor = new Color(0f, 1f, 0.8f, 0.35f);
        seedStartOutline.effectDistance = new Vector2(1, 1);

        var seedStartShadow = seedStartBtn.AddComponent<Shadow>();
        seedStartShadow.effectColor = new Color(0, 0, 0, 0.22f);
        seedStartShadow.effectDistance = new Vector2(0, -2);

        var seedStartBtnComp = seedStartBtn.GetComponent<Button>();
        seedStartBtnComp.targetGraphic = seedStartImg;
        seedStartBtnComp.colors = new ColorBlock { normalColor = seedStartImg.color, highlightedColor = new Color(0.15f,0.10f,0.22f,1f), pressedColor = new Color(0.08f,0.06f,0.12f,1f), disabledColor = new Color(0.05f,0.04f,0.06f,0.65f), colorMultiplier = 1f, fadeDuration = 0.1f };
        seedStartBtnComp.onClick.AddListener(StartCustomChallenge);
        seedStartBtn.AddComponent<ButtonPressFeedback>();

        var seedStartTxtGo = new GameObject("SeedStartTxt", typeof(RectTransform), typeof(Text));
        seedStartTxtGo.transform.SetParent(seedStartBtn.transform, false);
        var seedStartTxt = seedStartTxtGo.GetComponent<Text>();
        seedStartTxt.font = F();
        seedStartTxt.fontSize = 14;
        seedStartTxt.color = new Color(0.8f, 0.6f, 1f);
        seedStartTxt.alignment = TextAnchor.MiddleCenter;
        seedStartTxt.text = "开始";

        Spacer(mainVL.transform, 20);

        var closeBtn = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        closeBtn.transform.SetParent(mainVL.transform, false);
        var closeBtnRT = closeBtn.GetComponent<RectTransform>();
        closeBtnRT.sizeDelta = new Vector2(0, 40);

        var closeBtnImg = closeBtn.GetComponent<Image>();
        closeBtnImg.color = new Color(0.06f, 0.04f, 0.10f);

        var closeOutline = closeBtn.GetComponent<Outline>();
        closeOutline.effectColor = new Color(0f, 1f, 0.8f, 0.35f);
        closeOutline.effectDistance = new Vector2(1, 1);

        var closeShadow = closeBtn.AddComponent<Shadow>();
        closeShadow.effectColor = new Color(0, 0, 0, 0.24f);
        closeShadow.effectDistance = new Vector2(0, -2);

        var closeBtnComp = closeBtn.GetComponent<Button>();
        closeBtnComp.targetGraphic = closeBtnImg;
        closeBtnComp.colors = new ColorBlock { normalColor = closeBtnImg.color, highlightedColor = new Color(0.08f,0.06f,0.12f,1f), pressedColor = new Color(0.05f,0.04f,0.08f,1f), disabledColor = new Color(0.03f,0.03f,0.05f,0.65f), colorMultiplier = 1f, fadeDuration = 0.1f };
        closeBtnComp.onClick.AddListener(CloseChallengePanel);
        closeBtn.AddComponent<ButtonPressFeedback>();

        var closeTxtGo = new GameObject("CloseTxt", typeof(RectTransform), typeof(Text));
        closeTxtGo.transform.SetParent(closeBtn.transform, false);
        var closeTxt = closeTxtGo.GetComponent<Text>();
        closeTxt.font = F();
        closeTxt.fontSize = 14;
        closeTxt.color = Dim;
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeTxt.text = "✕ 关闭";

        _challengePanel.SetActive(false);
    }

    Button CreateTabButton(Transform parent, string label, bool isActive)
    {
        var btnGo = new GameObject("TabBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        btnGo.transform.SetParent(parent, false);

        var btnImg = btnGo.GetComponent<Image>();
        btnImg.color = isActive ? new Color(0.18f, 0.12f, 0.08f) : new Color(0.12f, 0.08f, 0.15f);

        var outline = btnGo.GetComponent<Outline>();
        outline.effectColor = isActive ? new Color(1f, 0.8f, 0.3f, 0.45f) : new Color(0.3f, 0.4f, 0.6f, 0.35f);
        outline.effectDistance = new Vector2(1, 1);

        var shadow = btnGo.AddComponent<Shadow>();
        shadow.effectColor = new Color(0, 0, 0, 0.22f);
        shadow.effectDistance = new Vector2(0, -2);

        var border = new GameObject("Border", typeof(RectTransform), typeof(Image));
        border.transform.SetParent(btnGo.transform, false);
        var borderRT = border.GetComponent<RectTransform>();
        borderRT.anchorMin = new Vector2(-1, -1);
        borderRT.anchorMax = new Vector2(1, 1);
        borderRT.offsetMin = new Vector2(-1, -1);
        borderRT.offsetMax = new Vector2(1, 1);
        border.GetComponent<Image>().color = isActive ? Gold : new Color(0.5f, 0.5f, 0.7f);

        var btn = btnGo.GetComponent<Button>();
        btn.targetGraphic = btnImg;

        var txtGo = new GameObject("Label", typeof(RectTransform), typeof(Text));
        txtGo.transform.SetParent(btnGo.transform, false);
        var txt = txtGo.GetComponent<Text>();
        txt.font = F();
        txt.fontSize = 14;
        txt.color = isActive ? Gold : Dim;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontStyle = FontStyle.Bold;
        txt.text = label;

        return btn;
    }

    void Spacer(Transform parent, float height)
    {
        var spacer = new GameObject("Spacer", typeof(RectTransform));
        spacer.transform.SetParent(parent, false);
        var spacerRT = spacer.GetComponent<RectTransform>();
        spacerRT.sizeDelta = new Vector2(0, height);
    }

    bool IsNavItemUnlocked(string itemName)
    {
        switch (itemName)
        {
            case "成就回响": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Achievements);
            case "异种图鉴": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Bestiary);
            case "记忆档案": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Archive);
            case "残响圣坛": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.EchoAltar);
            case "暗塔排行": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Leaderboard);
            case "终端设置": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Settings);
            case "残响商店": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.Shop);
            case "每日登录": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.DailyReward);
            default: return true;
        }
    }

    void Separator(Transform parent)
    {
        var sep = new GameObject("Separator", typeof(RectTransform), typeof(Image));
        sep.transform.SetParent(parent, false);
        var sepRT = sep.GetComponent<RectTransform>();
        sepRT.sizeDelta = new Vector2(0, 1);
        sep.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.3f);
    }

    void OpenChallengePanel()
    {
        _challengePanel.SetActive(true);
    }

    void CloseChallengePanel()
    {
        _challengePanel.SetActive(false);
    }

    void StartDailyChallenge()
    {
        CompleteGameSystem.Instance?.StartChallengeMode("daily");
    }

    void StartCustomChallenge()
    {
        string seed = _customSeedInput.text;
        if (string.IsNullOrEmpty(seed)) seed = UnityEngine.Random.Range(100000, 999999).ToString();
        CompleteGameSystem.Instance?.StartChallengeMode("custom", seed);
    }
}