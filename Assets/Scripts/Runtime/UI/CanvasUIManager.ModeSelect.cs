using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    // ========== MODE SELECT (暗塔契约) ==========
    string _selectedModeId = "Short";
    string _selectedClassId = "titan";
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
    bool _classPanelBuiltWithAllModes;

    Image[] _classCardBgs = new Image[3];
    Outline[] _classCardOutlines = new Outline[3];
    Text[] _classCardNames = new Text[3];

    struct ClassCardInfo
    {
        public string id, name, desc, iconPath;
        public Color themeColor;
    }
    static readonly ClassCardInfo[] ClassCards = {
        new ClassCardInfo { id="titan", name="泰坦", desc="HP+高 防御+高\n坚如磐石", iconPath="Icons/Classes/c_titan",
            themeColor = new Color(0.4f, 0.7f, 1f) },
        new ClassCardInfo { id="swarm", name="虫群", desc="EP+高 速度+高\n数量即力量", iconPath="Icons/Classes/c_swarm",
            themeColor = new Color(0.2f, 0.9f, 0.5f) },
        new ClassCardInfo { id="ghost", name="幽灵", desc="攻击+高 视野+大\n无形无影", iconPath="Icons/Classes/c_ghost",
            themeColor = new Color(0.7f, 0.3f, 0.9f) },
    };

    struct ModeInfo
    {
        public string id, icon, label, descTitle, floors, flavor, fit, feat1, feat2;
        public Color themeColor;
        public Color bgColor;
        public string startBtnText;
    }
    static readonly ModeInfo[] ModeInfos = {
        new ModeInfo { id="Short", icon="▶", label="暗影", descTitle="▶ 暗影",
            floors="12层速巡 · 约15分钟", flavor="暗影穿行，宿主更迭。",
            fit="想快速开一局，测试构筑与附身路线的闯塔者。",
            feat1="★ 开放排行榜", feat2="◇ 支持每日挑战词条",
            themeColor = new Color(0.9f, 0.6f, 0.2f), bgColor = new Color(0.12f, 0.08f, 0.04f),
            startBtnText = "开始暗影" },
        new ModeInfo { id="Expedition", icon="◆", label="远征", descTitle="◆ 远征",
            floors="10关 × 20层 · 约5小时", flavor="穿越十重裂隙，征服暗塔。",
            fit="喜欢长线挑战、享受跨层继承成长的闯塔者。",
            feat1="★ 关间奖励继承", feat2="◇ 难度逐关递增",
            themeColor = new Color(0.0f, 0.94f, 0.94f), bgColor = new Color(0.04f, 0.1f, 0.12f),
            startBtnText = "开启远征" },
        new ModeInfo { id="Classic", icon="●", label="宿命", descTitle="● 宿命",
            floors="50层全程 · 约60分钟", flavor="完整体验寄生者的宿命轮回。",
            fit="追求完整体验、挑战极限的闯塔者。",
            feat1="★ Boss层完整", feat2="◇ 全进化路径解锁",
            themeColor = new Color(0.65f, 0.33f, 0.94f), bgColor = new Color(0.08f, 0.04f, 0.14f),
            startBtnText = "开启宿命" },
    };

    void BuildClass()
    {
        _classPanel = Panel("Class", new Color(0.06f, 0.04f, 0.10f));

        // Background art (same as menu for unified style)
        var heroTex = LoadTex("UI/menu_hero");
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
        var backBtn = new GameObject("BackBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        backBtn.transform.SetParent(backRow.transform, false);
        var backRT = backBtn.GetComponent<RectTransform>();
        backRT.anchorMin = new Vector2(0, 0.5f);
        backRT.anchorMax = new Vector2(0, 0.5f);
        backRT.pivot = new Vector2(0, 0.5f);
        backRT.anchoredPosition = new Vector2(0, 0);
        backRT.sizeDelta = new Vector2(130, 36);
        var backImg = backBtn.GetComponent<Image>();
        backImg.color = new Color(0.08f, 0.06f, 0.12f, 0.5f);
        var backOl = backBtn.GetComponent<Outline>();
        backOl.effectColor = new Color(0f, 1f, 0.8f, 0.35f);
        backOl.effectDistance = new Vector2(1, 1);
        var backShadow = backBtn.AddComponent<Shadow>();
        backShadow.effectColor = new Color(0, 0, 0, 0.24f);
        backShadow.effectDistance = new Vector2(0, -2);
        var backBtnComp = backBtn.GetComponent<Button>();
        backBtnComp.targetGraphic = backImg;
        backBtnComp.colors = new ColorBlock { normalColor = backImg.color, highlightedColor = new Color(0.1f, 0.08f, 0.14f, 1f), pressedColor = new Color(0.05f, 0.04f, 0.08f, 1f), colorMultiplier = 1f, fadeDuration = 0.1f, disabledColor = new Color(0.04f,0.04f,0.06f,0.6f) };
        backBtn.AddComponent<ButtonPressFeedback>();
        var backTxtGo = new GameObject("T", typeof(RectTransform), typeof(Text));
        backTxtGo.transform.SetParent(backBtn.transform, false);
        Stretch(backTxtGo);
        var backTxt = backTxtGo.GetComponent<Text>();
        backTxt.font = F(); backTxt.fontSize = 13; backTxt.color = new Color(0.7f, 0.65f, 0.8f);
        backTxt.text = "← 返回主页"; backTxt.alignment = TextAnchor.MiddleCenter;
        backTxt.raycastTarget = false;
        backBtnComp.onClick.AddListener(() => {
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
        var titleTxt = Txt(vl, "暗 塔 契 约", 34, Bright, 48);
        titleTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;
        Spacer(vl, 4);
        Txt(vl, "D A R K   T O W E R   P R O T O C O L", 10, new Color(0.5f, 0.45f, 0.6f), 16);
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
        Txt(vl, "选择你的闯塔之路", 16, new Color(0.7f, 0.65f, 0.8f), 26);

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

        int unlockedModeCount = 0;
        for (int i = 0; i < 3; i++)
        {
            var info = ModeInfos[i];
            bool isUnlocked = IsModeUnlocked(info.id);
            if (!isUnlocked) continue;

            int idx = i;
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

            var labelT = TxtGo(inner.transform, info.label, 15, active ? Bright : MutedPurp);
            labelT.alignment = TextAnchor.MiddleCenter;
            labelT.fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            labelT.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
            _modeTabLabels[i] = labelT;

            tab.GetComponent<Button>().targetGraphic = tabImg;
            tab.GetComponent<Button>().onClick.AddListener(() => SelectModeTab(idx));

            unlockedModeCount++;
        }

        if (unlockedModeCount > 0)
        {
            int firstUnlockedIdx = -1;
            for (int i = 0; i < 3; i++)
            {
                if (IsModeUnlocked(ModeInfos[i].id))
                {
                    firstUnlockedIdx = i;
                    break;
                }
            }
            if (firstUnlockedIdx >= 0 && !IsModeUnlocked(_selectedModeId))
            {
                SelectModeTab(firstUnlockedIdx);
            }
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

        var fitLabel = TxtGo(descInner.transform, "适合玩家:", 11, MutedPurp);
        fitLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
        _modeDescFit = TxtGo(descInner.transform, "", 14, Bright);
        _modeDescFit.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

        Spacer(divl, 8);

        var featLabel = TxtGo(descInner.transform, "结算特性:", 11, MutedPurp);
        featLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 18;
        _modeDescFeature1 = TxtGo(descInner.transform, "", 14, Bright);
        _modeDescFeature1.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;
        _modeDescFeature2 = TxtGo(descInner.transform, "", 14, Bright);
        _modeDescFeature2.gameObject.AddComponent<LayoutElement>().preferredHeight = 22;

        Spacer(vl, 30);

        // ── Hero class selection ──
        Txt(vl, "选择初始宿主", 16, new Color(0.7f, 0.65f, 0.8f), 26);
        Spacer(vl, 10);

        var classRow = new GameObject("ClassRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        classRow.transform.SetParent(contentGo.transform, false);
        classRow.AddComponent<LayoutElement>().preferredHeight = 120;
        var classHL = classRow.GetComponent<HorizontalLayoutGroup>();
        classHL.spacing = 10;
        classHL.childAlignment = TextAnchor.MiddleCenter;
        classHL.childForceExpandWidth = true;
        classHL.childForceExpandHeight = true;

        for (int ci = 0; ci < ClassCards.Length; ci++)
        {
            int cidx = ci;
            var cInfo = ClassCards[ci];
            bool active = cInfo.id == _selectedClassId;

            var card = new GameObject("Class_" + cInfo.id, typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            card.transform.SetParent(classRow.transform, false);

            var cardImg = card.GetComponent<Image>();
            cardImg.color = active ? new Color(0.08f, 0.06f, 0.14f, 0.7f) : new Color(0.06f, 0.04f, 0.10f, 0.4f);
            _classCardBgs[ci] = cardImg;

            var cardOL = card.GetComponent<Outline>();
            cardOL.effectColor = active
                ? new Color(cInfo.themeColor.r, cInfo.themeColor.g, cInfo.themeColor.b, 0.7f)
                : new Color(0.25f, 0.22f, 0.35f, 0.3f);
            cardOL.effectDistance = new Vector2(1, 1);
            _classCardOutlines[ci] = cardOL;

            var inner = new GameObject("Inner", typeof(RectTransform), typeof(VerticalLayoutGroup));
            inner.transform.SetParent(card.transform, false);
            Stretch(inner);
            var ivl = inner.GetComponent<VerticalLayoutGroup>();
            ivl.spacing = 2; ivl.padding = new RectOffset(4, 4, 6, 4);
            ivl.childAlignment = TextAnchor.MiddleCenter;
            ivl.childForceExpandWidth = false; ivl.childForceExpandHeight = false;

            var classTex = LoadTex(cInfo.iconPath);
            if (classTex != null)
            {
                var icoWrap = new GameObject("IconWrap", typeof(RectTransform), typeof(LayoutElement));
                icoWrap.transform.SetParent(inner.transform, false);
                var icoLE = icoWrap.GetComponent<LayoutElement>();
                icoLE.preferredHeight = 56;
                icoLE.preferredWidth = 56;
                icoLE.minHeight = 56;
                icoLE.minWidth = 56;

                BuildAspectIcon(icoWrap.transform, classTex, 3);
            }

            var nameT = TxtGo(inner.transform, cInfo.name, 14, active ? cInfo.themeColor : Dim);
            nameT.alignment = TextAnchor.MiddleCenter;
            nameT.fontStyle = FontStyle.Bold;
            nameT.horizontalOverflow = HorizontalWrapMode.Overflow;
            var nameLE = nameT.gameObject.AddComponent<LayoutElement>();
            nameLE.preferredHeight = 20;
            nameLE.preferredWidth = 80;
            nameLE.minHeight = 20;
            nameLE.minWidth = 80;
            nameLE.flexibleWidth = 0;
            _classCardNames[ci] = nameT;

            var descT = TxtGo(inner.transform, cInfo.desc, 10, active ? Bright : MutedPurp);
            descT.alignment = TextAnchor.MiddleCenter;
            descT.horizontalOverflow = HorizontalWrapMode.Overflow;
            var descLE = descT.gameObject.AddComponent<LayoutElement>();
            descLE.preferredHeight = 30;
            descLE.preferredWidth = 80;
            descLE.minHeight = 30;
            descLE.minWidth = 80;
            descLE.flexibleWidth = 0;

            card.GetComponent<Button>().targetGraphic = cardImg;
            card.GetComponent<Button>().onClick.AddListener(() => SelectClassCard(cidx));
        }

        Spacer(vl, 22);

        // Big start button
        var startBtn = BuildMenuMainButton(contentGo.transform, "开始本次闯塔", 22, Cyan,
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
        int initIdx = -1;
        for (int i = 0; i < 3; i++)
        {
            if (IsModeUnlocked(ModeInfos[i].id))
            {
                initIdx = i;
                break;
            }
        }
        if (initIdx >= 0)
            SelectModeTab(initIdx);

        _classPanelBuiltWithAllModes = ModuleUnlockSystem.Instance != null
            && ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ClassicMode)
            && ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ExpeditionMode);
    }

    void SelectModeTab(int idx)
    {
        var info = ModeInfos[idx];
        if (!IsModeUnlocked(info.id)) return;
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
                _modeTabLabels[i].color = active ? Bright : MutedPurp;
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

        bool allModesUnlocked = ModuleUnlockSystem.Instance != null
            && ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ClassicMode)
            && ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ExpeditionMode);
        bool needRebuild = _classPanel == null || (!_classPanelBuiltWithAllModes && allModesUnlocked);
        if (needRebuild)
        {
            if (_classPanel != null) { Destroy(_classPanel); _classPanel = null; }
            BuildClass();
        }
        SA(_classPanel, true);
    }

    bool IsModeUnlocked(string modeId)
    {
        switch (modeId)
        {
            case "Short": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ShortMode);
            case "Classic": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ClassicMode);
            case "Expedition": return ModuleUnlockSystem.Instance.IsModuleUnlocked(Module.ExpeditionMode);
            default: return true;
        }
    }

    void SelectClassCard(int idx)
    {
        _selectedClassId = ClassCards[idx].id;
        for (int i = 0; i < ClassCards.Length; i++)
        {
            bool active = i == idx;
            var ci = ClassCards[i];
            if (_classCardBgs[i] != null)
                _classCardBgs[i].color = active ? new Color(0.08f, 0.06f, 0.14f, 0.7f) : new Color(0.06f, 0.04f, 0.10f, 0.4f);
            if (_classCardOutlines[i] != null)
                _classCardOutlines[i].effectColor = active
                    ? new Color(ci.themeColor.r, ci.themeColor.g, ci.themeColor.b, 0.7f)
                    : new Color(0.25f, 0.22f, 0.35f, 0.3f);
            if (_classCardNames[i] != null)
            {
                _classCardNames[i].color = active ? ci.themeColor : Dim;
                _classCardNames[i].fontStyle = active ? FontStyle.Bold : FontStyle.Normal;
            }
        }
    }

    void StartSelectedMode()
    {
        if (!IsModeUnlocked(_selectedModeId)) return;

        var gm = GameManager.Instance;
        if (gm != null && (gm.CurrentMode == GameMode.Daily || gm.CurrentMode == GameMode.Weekly))
        {
            StartCoroutine(DelayedSelectClass());
            return;
        }
        gm?.StartNewGame(_selectedModeId);
        StartCoroutine(DelayedSelectClass());
    }

    IEnumerator DelayedSelectClass()
    {
        yield return null;
        CompleteGameSystem.Instance?.SelectClass(_selectedClassId);
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
        var content = BuildOverlayScaffold(ref _chalOvl, "ChalOvl", LocalizationData.T("挑战模式"), Gold, new Color(0.08f, 0.06f, 0.12f, 0.82f), "bg_challenge");

        // ===== 每日挑战 =====
        Spacer(content.GetComponent<VerticalLayoutGroup>(), 12);
        Txt(content, LocalizationData.T("今 日 挑 战"), 16, Gold, 28);
        Txt(content, "D A I L Y   A N O M A L Y", 9, Dim, 16);

        var dailyMod = CompleteGameSystem.GetTodayDailyModifier();
        Spacer(content.GetComponent<VerticalLayoutGroup>(), 6);
        var dailyModTxt = Txt(content, $"{dailyMod.icon}  {dailyMod.name}：{dailyMod.desc}", 13, new Color(1f, 0.7f, 0.2f), 40);
        dailyModTxt.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 8);
        var dailyBtn = new GameObject("DailyBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        dailyBtn.transform.SetParent(content.transform, false);
        dailyBtn.AddComponent<LayoutElement>().preferredHeight = 48;
        var dailyImg = dailyBtn.GetComponent<Image>();
        dailyImg.color = new Color(0.12f, 0.1f, 0.2f);
        var dailyOl = dailyBtn.GetComponent<Outline>();
        dailyOl.effectColor = new Color(Gold.r, Gold.g, Gold.b, 0.45f);
        dailyOl.effectDistance = new Vector2(1, 1);
        var dailySh = dailyBtn.AddComponent<Shadow>();
        dailySh.effectColor = new Color(0, 0, 0, 0.24f);
        dailySh.effectDistance = new Vector2(0, -2);
        dailyBtn.GetComponent<Button>().targetGraphic = dailyImg;
        dailyBtn.AddComponent<ButtonPressFeedback>();
        var dTxt = TxtGo(dailyBtn.transform, LocalizationData.T("开始每日挑战"), 14, Gold);
        dTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(dTxt.gameObject);
        dailyBtn.GetComponent<Button>().onClick.AddListener(() => {
            StartCoroutine(CloseOverlayAnimated(_chalOvl));
            CompleteGameSystem.Instance?.StartChallengeMode("daily");
        });

        // ===== 本周挑战 =====
        Spacer(content.GetComponent<VerticalLayoutGroup>(), 20);
        Txt(content, LocalizationData.T("本 周 挑 战"), 16, Purp, 28);
        Txt(content, "W E E K L Y   C H A L L E N G E", 9, Dim, 16);

        var weeklyMod = CompleteGameSystem.GetThisWeekModifier();
        Spacer(content.GetComponent<VerticalLayoutGroup>(), 6);
        var weeklyModTxt = Txt(content, $"{weeklyMod.icon}  {weeklyMod.name}：{weeklyMod.desc}", 13, new Color(0.8f, 0.5f, 1f), 40);
        weeklyModTxt.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 8);
        var weeklyBtn = new GameObject("WeeklyBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        weeklyBtn.transform.SetParent(content.transform, false);
        weeklyBtn.AddComponent<LayoutElement>().preferredHeight = 48;
        var weeklyImg = weeklyBtn.GetComponent<Image>();
        weeklyImg.color = new Color(0.12f, 0.1f, 0.2f);
        var weeklyOl = weeklyBtn.GetComponent<Outline>();
        weeklyOl.effectColor = new Color(Purp.r, Purp.g, Purp.b, 0.45f);
        weeklyOl.effectDistance = new Vector2(1, 1);
        var weeklySh = weeklyBtn.AddComponent<Shadow>();
        weeklySh.effectColor = new Color(0, 0, 0, 0.24f);
        weeklySh.effectDistance = new Vector2(0, -2);
        weeklyBtn.GetComponent<Button>().targetGraphic = weeklyImg;
        weeklyBtn.AddComponent<ButtonPressFeedback>();
        var wTxt = TxtGo(weeklyBtn.transform, LocalizationData.T("开始本周挑战"), 14, Purp);
        wTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(wTxt.gameObject);
        weeklyBtn.GetComponent<Button>().onClick.AddListener(() => {
            StartCoroutine(CloseOverlayAnimated(_chalOvl));
            CompleteGameSystem.Instance?.StartChallengeMode("weekly");
        });

        // ===== 自定义种子 =====
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

        var seedBtn = new GameObject("SeedStartBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        seedBtn.transform.SetParent(seedRow.transform, false);
        seedBtn.AddComponent<LayoutElement>().preferredWidth = 80;
        var seedImg = seedBtn.GetComponent<Image>();
        seedImg.color = new Color(0.12f, 0.1f, 0.2f);
        var seedOl = seedBtn.GetComponent<Outline>();
        seedOl.effectColor = new Color(Cyan.r, Cyan.g, Cyan.b, 0.45f);
        seedOl.effectDistance = new Vector2(1, 1);
        var seedSh = seedBtn.AddComponent<Shadow>();
        seedSh.effectColor = new Color(0, 0, 0, 0.24f);
        seedSh.effectDistance = new Vector2(0, -2);
        seedBtn.GetComponent<Button>().targetGraphic = seedImg;
        seedBtn.AddComponent<ButtonPressFeedback>();
        var sTxt = TxtGo(seedBtn.transform, LocalizationData.T("开始"), 13, Cyan);
        sTxt.alignment = TextAnchor.MiddleCenter;
        Stretch(sTxt.gameObject);
        seedBtn.GetComponent<Button>().onClick.AddListener(() => {
            string seed = _chalSeedInput != null ? _chalSeedInput.text : "";
            if (string.IsNullOrEmpty(seed)) seed = UnityEngine.Random.Range(100000, 999999).ToString();
            StartCoroutine(CloseOverlayAnimated(_chalOvl));
            CompleteGameSystem.Instance?.StartChallengeMode("custom", seed);
        });
    }


}
