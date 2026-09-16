using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    // ========== EXPLORE ==========
    void BuildExplore()
    {
        _explorePanel = Panel("Explore", PanelBg);
        var rt = _explorePanel.GetComponent<RectTransform>();
        
        // ParasiteBackground Shader 动态 procedural 主背景 (生物膜质感 + 粒子流动 + 视差)
        // 每层独立 seed, 每层都有独特场景感, 完全替代静态 floor_bg 图
        var shaderBgGo = new GameObject("BgShaderMain", typeof(RectTransform), typeof(Image));
        shaderBgGo.transform.SetParent(rt, false);
        Stretch(shaderBgGo);
        _eBgShaderOverlay = shaderBgGo.GetComponent<Image>();
        _eBgShaderOverlay.raycastTarget = false;
        _eBgShaderOverlay.sprite = WhiteSprite;
        _eBgShaderOverlay.color = new Color(1, 1, 1, 1f);  // 主背景完全不透明
        // Material 在首次楼层更新时由 ShaderMaterialManager 按楼层 seed 创建

        // floor_bg 原图 (次底层, 用于加载纹理占位, 被 Shader 主背景盖住后可见 floor_bg 的 meta 作用)
        var floorBgGo = new GameObject("FloorBg", typeof(RectTransform), typeof(RawImage));
        floorBgGo.transform.SetParent(rt, false);
        Stretch(floorBgGo);
        _eFloorBg = floorBgGo.GetComponent<RawImage>();
        _eFloorBg.color = new Color(1f, 1f, 1f, 0.0f);  // 全透明, 不参与最终渲染
        _eFloorBg.raycastTarget = false;
        _eFloorBgIdx = -1;

        // === HUD (top ~8%) ===
        var hudGo = new GameObject("HUD", typeof(RectTransform), typeof(Image));
        hudGo.transform.SetParent(rt, false);
        var hudImg = hudGo.GetComponent<Image>();
        hudImg.color = new Color(0.04f, 0.03f, 0.08f, 0.95f);
        
        var hudRT = hudGo.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 0.91f);
        hudRT.anchorMax = new Vector2(1, 1);
        hudRT.offsetMin = Vector2.zero;
        hudRT.offsetMax = Vector2.zero;

        // Row 1 (上半): 楼层 | HP | 时间
        _eFloor = TxtAnchored(hudRT, "F1 入口通道", 14, Gold, new Vector2(0, 0.5f), new Vector2(0.38f, 1), 10, 0);
        _eFloor.alignment = TextAnchor.MiddleLeft;
        _eFloor.fontStyle = FontStyle.Bold;

        _eHpTxt = TxtAnchored(hudRT, "♥ 100/100", 14, Color.white, new Vector2(0.38f, 0.5f), new Vector2(0.72f, 1), 0, 0);
        _eHpTxt.alignment = TextAnchor.MiddleCenter;
        _eHpTxt.fontStyle = FontStyle.Bold;

        _eTimer = TxtAnchored(hudRT, "", 13, new Color(0.8f, 0.75f, 0.95f), new Vector2(0.72f, 0.5f), new Vector2(1, 1), 0, 6);
        _eTimer.alignment = TextAnchor.MiddleRight;
        _eTimer.fontStyle = FontStyle.Bold;

        // Row 2 (下半): 形态名 | 污染
        _eFormName = TxtAnchored(hudRT, "泰坦", 12, new Color(0.9f, 0.88f, 0.95f), new Vector2(0, 0.02f), new Vector2(0.5f, 0.5f), 10, 0);
        _eFormName.alignment = TextAnchor.MiddleLeft;
        _eFormName.fontStyle = FontStyle.Bold;

        _ePollTxt = TxtAnchored(hudRT, "☢ 0%", 12, Color.white, new Vector2(0.5f, 0.02f), new Vector2(1, 0.5f), 0, 6);
        _ePollTxt.alignment = TextAnchor.MiddleRight;
        _ePollTxt.fontStyle = FontStyle.Bold;

        // === Host bar below HUD (explore) ===
        _eHostBar = new GameObject("ExHostBar", typeof(RectTransform), typeof(Image), typeof(Outline));
        _eHostBar.transform.SetParent(rt, false);
        var ehBg = _eHostBar.GetComponent<Image>();
        ehBg.color = new Color(0.03f, 0.03f, 0.07f, 0.97f);
        var ehOutline = _eHostBar.GetComponent<Outline>();
        ehOutline.effectColor = new Color(0f, 1f, 0.816f, 0.22f);
        ehOutline.effectDistance = new Vector2(0.5f, 0.5f);
        
        var ehRT = _eHostBar.GetComponent<RectTransform>();
        ehRT.anchorMin = new Vector2(0, 0.88f);
        ehRT.anchorMax = new Vector2(1, 0.91f);
        ehRT.offsetMin = new Vector2(4, 0); ehRT.offsetMax = new Vector2(-4, 0);

        var possessBadgeGo = new GameObject("PossessBadge", typeof(RectTransform), typeof(Image), typeof(Outline));
        possessBadgeGo.transform.SetParent(ehRT, false);
        var possessBadgeRT = possessBadgeGo.GetComponent<RectTransform>();
        possessBadgeRT.anchorMin = new Vector2(0, 0);
        possessBadgeRT.anchorMax = new Vector2(0.06f, 1);
        possessBadgeRT.offsetMin = new Vector2(4, 4);
        possessBadgeRT.offsetMax = new Vector2(-4, -4);
        var possessBadgeImg = possessBadgeGo.GetComponent<Image>();
        possessBadgeImg.color = new Color(0f, 1f, 0.816f, 0.60f);
        possessBadgeImg.sprite = CreateCircleSprite(24);
        var possessBadgeOutline = possessBadgeGo.GetComponent<Outline>();
        possessBadgeOutline.effectColor = new Color(0f, 1f, 0.816f, 0.35f);
        possessBadgeOutline.effectDistance = new Vector2(0.5f, 0.5f);
        _ePossessBadge = possessBadgeGo;
        
        var possessBadgeTxt = TxtAnchored(possessBadgeRT, "🧬", 10, Color.black, new Vector2(0,0), new Vector2(1,1), 0, 0);
        possessBadgeTxt.alignment = TextAnchor.MiddleCenter;
        possessBadgeTxt.fontStyle = FontStyle.Bold;

        _eHostName = TxtAnchored(ehRT, "", 13, Cyan, new Vector2(0.06f, 0), new Vector2(0.36f, 1), 6, 0);
        _eHostName.alignment = TextAnchor.MiddleLeft;
        _eHostName.fontStyle = FontStyle.Bold;

        _eHostTraits = TxtAnchored(ehRT, "", 10, new Color(0.82f, 0.75f, 0.95f), new Vector2(0.36f, 0), new Vector2(0.82f, 1), 6, 0);
        _eHostTraits.alignment = TextAnchor.MiddleLeft;

        var eHostInfoBtn = new GameObject("EHostInfoBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        eHostInfoBtn.transform.SetParent(ehRT, false);
        var ehBtnRT = eHostInfoBtn.GetComponent<RectTransform>();
        ehBtnRT.anchorMin = new Vector2(0.84f, 0.12f);
        ehBtnRT.anchorMax = new Vector2(0.98f, 0.88f);
        ehBtnRT.offsetMin = Vector2.zero; ehBtnRT.offsetMax = Vector2.zero;
        eHostInfoBtn.GetComponent<Image>().color = new Color(0.08f, 0.06f, 0.14f, 0.95f);
        
        var ehBtnTxt = TxtAnchored(ehBtnRT, "i", 13, Gold, Vector2.zero, Vector2.one, 0, 0);
        ehBtnTxt.alignment = TextAnchor.MiddleCenter;
        ehBtnTxt.fontStyle = FontStyle.Bold;
        eHostInfoBtn.GetComponent<Button>().targetGraphic = eHostInfoBtn.GetComponent<Image>();
        eHostInfoBtn.GetComponent<Button>().onClick.AddListener(() => ShowHostTraitInfo());

        _eHostBar.SetActive(false);

        // === Tutorial (thin bar below HUD) ===
        var hudSep = new GameObject("HudSep", typeof(RectTransform), typeof(Image));
        hudSep.transform.SetParent(rt, false);
        var hudSepRT = hudSep.GetComponent<RectTransform>();
        hudSepRT.anchorMin = new Vector2(0.03f, 0.91f); hudSepRT.anchorMax = new Vector2(0.97f, 0.912f);
        hudSepRT.offsetMin = Vector2.zero; hudSepRT.offsetMax = Vector2.zero;
        hudSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.06f);

        // === Tutorial notification bar (compact, top area) ===
        _eTutGo = new GameObject("Tut", typeof(RectTransform), typeof(Image), typeof(Outline));
        _eTutGo.transform.SetParent(_root, false);
        _eTutGo.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.12f, 0.93f);
        _eTutGo.GetComponent<Image>().raycastTarget = false;
        var tutOL = _eTutGo.GetComponent<Outline>();
        tutOL.effectColor = new Color(0f, 1f, 0.816f, 0.22f);
        tutOL.effectDistance = new Vector2(0.5f, -0.5f);
        var tutRT = _eTutGo.GetComponent<RectTransform>();
        tutRT.anchorMin = new Vector2(0.04f, 0.92f);
        tutRT.anchorMax = new Vector2(0.96f, 0.98f);

        _eTutTitle = TxtAnchored(tutRT, "", 14, new Color(0f, 1f, 0.816f), new Vector2(0, 0.52f), new Vector2(0.72f, 1), 10, 0);
        _eTutTitle.alignment = TextAnchor.MiddleLeft;
        _eTutTitle.fontStyle = FontStyle.Bold;
        _eTutTitle.raycastTarget = false;

        _eTutStep = TxtAnchored(tutRT, "", 9, new Color(0.5f, 0.5f, 0.55f), new Vector2(0.72f, 0.55f), new Vector2(1, 1), 0, 6);
        _eTutStep.alignment = TextAnchor.MiddleRight;
        _eTutStep.raycastTarget = false;

        _eTutTxt = TxtAnchored(tutRT, "", 11, new Color(0.82f, 0.82f, 0.82f), new Vector2(0, 0), new Vector2(0.78f, 0.52f), 10, 0);
        _eTutTxt.alignment = TextAnchor.MiddleLeft;
        _eTutTxt.raycastTarget = false;

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

        // === Map grid (middle 60%) ===
        var mapGo = new GameObject("Map", typeof(RectTransform), typeof(Image));
        // Map container: transparent Image to act as layout root
        var mapImgComp = mapGo.GetComponent<Image>();
        mapImgComp.color = new Color(1, 1, 1, 0);
        mapImgComp.raycastTarget = false;
        mapGo.transform.SetParent(rt, false);
        var mapRT = mapGo.GetComponent<RectTransform>();
        mapRT.anchorMin = new Vector2(0, 0.27f);
        mapRT.anchorMax = new Vector2(1, 0.88f);
        mapRT.offsetMin = new Vector2(0, 0);
        mapRT.offsetMax = new Vector2(0, 0);
        
        int gridCount = 13;
        for (int yy = 0; yy < 13; yy++)
        {
            for (int xx = 0; xx < 13; xx++)
            {
                int dataY = 12 - yy;
                int dataX = xx;
                
                var cell = new GameObject($"C{dataX}_{dataY}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
                cell.transform.SetParent(mapGo.transform, false);
                var cellRT = cell.GetComponent<RectTransform>();

                float xMin = (float)xx / gridCount;
                float xMax = (float)(xx + 1) / gridCount;
                float yMin = (float)(12 - yy) / gridCount;
                float yMax = (float)(13 - yy) / gridCount;
                cellRT.anchorMin = new Vector2(xMin, yMin);
                cellRT.anchorMax = new Vector2(xMax, yMax);
                cellRT.offsetMin = Vector2.zero;
                cellRT.offsetMax = Vector2.zero;
                cellRT.pivot = new Vector2(0.5f, 0.5f);

                var cellImg = cell.GetComponent<Image>();
                cellImg.sprite = WhiteSprite;
                _mapImg[dataY, dataX] = cellImg;
                _mapImg[dataY, dataX].color = new Color(1, 1, 1, 0);
                _mapImg[dataY, dataX].raycastTarget = false;
                _mapCellOutline[dataY, dataX] = cell.GetComponent<Outline>();
                _mapCellOutline[dataY, dataX].effectColor = new Color(0, 0, 0, 0);
                _mapCellOutline[dataY, dataX].effectDistance = new Vector2(0, 0);
                _mapCellShadow[dataY, dataX] = cell.GetComponent<Shadow>();
                _mapCellShadow[dataY, dataX].enabled = false;
                _mapCellShadow[dataY, dataX].effectDistance = new Vector2(1, -1);
                _mapCellShadow[dataY, dataX].effectColor = new Color(0.03f, 0.02f, 0.10f, 0.7f);

                // Cell Highlight Outline: 墙壁专用右上高光 (立体)
                var hlOutline = cell.AddComponent<Outline>();
                hlOutline.effectColor = new Color(1f, 1f, 1f, 0);
                hlOutline.effectDistance = new Vector2(-0.5f, 0.5f);
                hlOutline.enabled = false;
                _mapCellHighlight[dataY, dataX] = hlOutline;

                // Pattern: 地板/墙壁 生物膜纹理层 (Perlin noise, alpha 0.18)
                var patGo = new GameObject("Pat", typeof(RectTransform), typeof(Image));
                patGo.transform.SetParent(cell.transform, false);
                var patRT = patGo.GetComponent<RectTransform>();
                patRT.anchorMin = Vector2.zero;
                patRT.anchorMax = Vector2.one;
                patRT.offsetMin = Vector2.zero;
                patRT.offsetMax = Vector2.zero;
                var patImg = patGo.GetComponent<Image>();
                patImg.color = new Color(1, 1, 1, 0);
                patImg.raycastTarget = false;
                _mapPattern[dataY, dataX] = patImg;

                // Icon Glow Ring: scale 1.18x, alpha 0.22 的同色图标 → 悬浮光晕
                // 紧凑尺寸: anchor -0.08 → 1.08, 避免溢出到邻格造成十字交叉
                var glowGo = new GameObject("Glow", typeof(RectTransform), typeof(Image));
                glowGo.transform.SetParent(cell.transform, false);
                var glowRT = glowGo.GetComponent<RectTransform>();
                glowRT.anchorMin = new Vector2(-0.08f, -0.08f);
                glowRT.anchorMax = new Vector2(1.08f, 1.08f);
                glowRT.offsetMin = Vector2.zero;
                glowRT.offsetMax = Vector2.zero;
                glowRT.pivot = new Vector2(0.5f, 0.5f);
                glowGo.SetActive(false);
                var glowImg = glowGo.GetComponent<Image>();
                glowImg.color = new Color(1, 1, 1, 0);
                glowImg.preserveAspect = true;
                glowImg.raycastTarget = false;
                _mapGlow[dataY, dataX] = glowImg;
                
                // Icon: 填满格子 + Outline发光边框 (唯一区分手段)
                var ico = new GameObject("I", typeof(RectTransform), typeof(Image), typeof(Outline));
                ico.transform.SetParent(cell.transform, false);
                var icoRT = ico.GetComponent<RectTransform>();
                icoRT.anchorMin = Vector2.zero;
                icoRT.anchorMax = Vector2.one;
                icoRT.offsetMin = Vector2.zero;
                icoRT.offsetMax = Vector2.zero;
                icoRT.pivot = new Vector2(0.5f, 0.5f);
                var icoImg = ico.GetComponent<Image>();
                icoImg.color = new Color(1, 1, 1, 0);
                icoImg.preserveAspect = true;
                icoImg.raycastTarget = false;
                _mapIcon[dataY, dataX] = icoImg;
                var icoOutline = ico.GetComponent<Outline>();
                icoOutline.effectColor = new Color(0f, 1f, 0.85f, 0f);
                icoOutline.effectDistance = new Vector2(1, 1);
                _mapOutline[dataY, dataX] = icoOutline;
                
                // Badge: 右上角标记
                var badgeGo = new GameObject("Badge", typeof(RectTransform), typeof(Image));
                badgeGo.transform.SetParent(cell.transform, false);
                var badgeRT = badgeGo.GetComponent<RectTransform>();
                badgeRT.anchorMin = new Vector2(0.55f, 0.55f);
                badgeRT.anchorMax = new Vector2(1f, 1f);
                badgeRT.offsetMin = Vector2.zero;
                badgeRT.offsetMax = Vector2.zero;
                badgeRT.pivot = new Vector2(1f, 1f);
                badgeGo.SetActive(false);
                var badgeImg = badgeGo.GetComponent<Image>();
                badgeImg.preserveAspect = true;
                badgeImg.color = Color.white;
                badgeImg.raycastTarget = false;
                _mapBadge[dataY, dataX] = badgeImg;

                // Fog overlay: darkens undiscovered cells
                var fogGo = new GameObject("Fog", typeof(RectTransform), typeof(Image));
                fogGo.transform.SetParent(cell.transform, false);
                var fogRT = fogGo.GetComponent<RectTransform>();
                fogRT.anchorMin = Vector2.zero;
                fogRT.anchorMax = Vector2.one;
                fogRT.offsetMin = Vector2.zero;
                fogRT.offsetMax = Vector2.zero;
                var fogImg = fogGo.GetComponent<Image>();
                fogImg.color = new Color(0.02f, 0.01f, 0.04f, 0.94f);
                fogImg.raycastTarget = false;
                _mapFog[dataY, dataX] = fogImg;
            }
        }

        // === Message area (5%) ===
        var msgBgGo = new GameObject("MsgBg", typeof(RectTransform), typeof(Image));
        msgBgGo.transform.SetParent(rt, false);
        var msgBgRT = msgBgGo.GetComponent<RectTransform>();
        msgBgRT.anchorMin = new Vector2(0, 0.22f); msgBgRT.anchorMax = new Vector2(1, 0.27f);
        msgBgRT.offsetMin = Vector2.zero; msgBgRT.offsetMax = Vector2.zero;
        msgBgGo.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.10f, 0.7f);
        msgBgGo.GetComponent<Image>().raycastTarget = false;

        _eMsg = TxtAnchored(rt, "", 12, new Color(0.6f,0.85f,0.6f), new Vector2(0,0.22f), new Vector2(0.7f,0.27f), 8, 0);
        _eMsg.alignment = TextAnchor.MiddleLeft;
        _eMsg.supportRichText = true;

        // === Status bar (4%) ===
        _eProg = TxtAnchored(rt, "", 12, Bright, new Vector2(0,0.18f), new Vector2(0.5f,0.22f), 8, 0);
        _eProg.alignment = TextAnchor.MiddleLeft;

        _eFragments = TxtAnchored(rt, "碎片×0", 12, Cyan, new Vector2(0.5f,0.18f), new Vector2(0.75f,0.22f), 0, 0);
        _eFragments.alignment = TextAnchor.MiddleCenter;

        _eEvoLvl = TxtAnchored(rt, "Evo Lv0", 12, Gold, new Vector2(0.75f,0.18f), new Vector2(1,0.22f), 0, 0);
        _eEvoLvl.alignment = TextAnchor.MiddleRight;

        // === Action area (15%) ===
        var actSep = new GameObject("ActSep", typeof(RectTransform), typeof(Image));
        actSep.transform.SetParent(rt, false);
        var actSepRT = actSep.GetComponent<RectTransform>();
        actSepRT.anchorMin = new Vector2(0.03f, 0.058f); actSepRT.anchorMax = new Vector2(0.97f, 0.06f);
        actSepRT.offsetMin = Vector2.zero; actSepRT.offsetMax = Vector2.zero;
        actSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.06f);

        var actGo = new GameObject("Actions", typeof(RectTransform), typeof(Image));
        actGo.transform.SetParent(rt, false);
        actGo.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.10f, 0.92f);
        var actRT = actGo.GetComponent<RectTransform>();
        actRT.anchorMin = new Vector2(0, 0.06f);
        actRT.anchorMax = new Vector2(1, 0.21f);
        actRT.offsetMin = Vector2.zero;
        actRT.offsetMax = Vector2.zero;

        // === Form slots bar (4%) - at very bottom ===
        var eFormRow = new GameObject("EForms", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        eFormRow.transform.SetParent(rt, false);
        var eFormRT = eFormRow.GetComponent<RectTransform>();
        eFormRT.anchorMin = new Vector2(0, 0);
        eFormRT.anchorMax = new Vector2(1, 0.04f);
        eFormRT.offsetMin = new Vector2(8, 0);
        eFormRT.offsetMax = new Vector2(-8, 0);
        var eFormHL = eFormRow.GetComponent<HorizontalLayoutGroup>();
        eFormHL.spacing = 4;
        eFormHL.childAlignment = TextAnchor.MiddleCenter;
        eFormHL.childForceExpandWidth = true;
        eFormHL.childForceExpandHeight = true;
        _eFormRow = eFormRow.transform;

        // D-pad wheel (left side) — 基于锚点的相对位置,适配不同分辨率
        var dpadGo = new GameObject("DPad", typeof(RectTransform), typeof(Image), typeof(DPadWheel));
        dpadGo.transform.SetParent(actRT, false);
        var dpadRT = dpadGo.GetComponent<RectTransform>();
        dpadRT.anchorMin = new Vector2(0f, 0.5f);
        dpadRT.anchorMax = new Vector2(0f, 0.5f);
        dpadRT.pivot = new Vector2(0.5f, 0.5f);
        dpadRT.anchoredPosition = new Vector2(80f, 0f);
        var dpad = dpadGo.GetComponent<DPadWheel>();
        dpad.Setup(actRT, dpadRT.anchoredPosition, 110);
        dpad.onDirection = dir => CompleteGameSystem.Instance?.Move(dir);

        // Right side: 4 quick-access icon buttons (icon + label)
        float qbSize = 56f;
        float qbSpacing = 6f;
        float qbStartX = 20f;
        float qbY = 35f;

        Button MakeIconBtn(Transform par, string ico, string lbl, Color acc, Vector2 pos, Action act)
        {
            var go = new GameObject($"QB_{lbl}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            go.transform.SetParent(par, false);
            var goRT = go.GetComponent<RectTransform>();
            goRT.anchoredPosition = pos;
            goRT.sizeDelta = new Vector2(qbSize, qbSize);
            Color bg = Color.Lerp(new Color(0.08f, 0.06f, 0.14f), acc, 0.12f);
            bg.a = 0.92f;
            go.GetComponent<Image>().color = bg;
            var ol = go.GetComponent<Outline>();
            ol.effectColor = new Color(acc.r, acc.g, acc.b, 0.3f);
            ol.effectDistance = new Vector2(1, 1);
            var it = TxtAnchored(goRT, ico, 26, acc, new Vector2(0, 0.28f), new Vector2(1, 1), 0, 0);
            it.alignment = TextAnchor.MiddleCenter;
            it.fontStyle = FontStyle.Bold;
            it.raycastTarget = false;
            var lt = TxtAnchored(goRT, lbl, 10, new Color(acc.r, acc.g, acc.b, 0.6f),
                new Vector2(0, 0), new Vector2(1, 0.30f), 0, 0);
            lt.alignment = TextAnchor.MiddleCenter;
            lt.raycastTarget = false;
            var btn = go.GetComponent<Button>();
            btn.targetGraphic = go.GetComponent<Image>();
            var cols = btn.colors;
            cols.normalColor = bg;
            cols.highlightedColor = Color.Lerp(bg, Color.white, 0.15f);
            cols.pressedColor = Color.Lerp(bg, Color.black, 0.25f);
            btn.colors = cols;
            btn.onClick.AddListener(() => act?.Invoke());
            go.AddComponent<ButtonPressFeedback>();
            return btn;
        }

        _eEvoBtn = MakeIconBtn(actGo.transform, "★", "进化", Cyan, new Vector2(qbStartX, qbY), () => ShowEvolutionPanel()).gameObject;
        _hudMenuRedDot = new GameObject("EvoRedDot", typeof(RectTransform));
        _hudMenuRedDot.transform.SetParent(_eEvoBtn.transform, false);
        var mrdRT = _hudMenuRedDot.GetComponent<RectTransform>();
        mrdRT.anchorMin = new Vector2(1, 1); mrdRT.anchorMax = new Vector2(1, 1);
        mrdRT.pivot = new Vector2(0.5f, 0.5f);
        mrdRT.anchoredPosition = new Vector2(-6, -6);
        mrdRT.sizeDelta = new Vector2(12, 12);
        var mrdDot = TxtGo(_hudMenuRedDot.transform, "●", 12, LightRed);
        mrdDot.alignment = TextAnchor.MiddleCenter;
        Stretch(mrdDot.gameObject);
        _hudMenuRedDot.SetActive(false);

        _eSaveBtn = MakeIconBtn(actGo.transform, "◆", "存档", new Color(0.5f, 0.75f, 0.55f),
            new Vector2(qbStartX + (qbSize + qbSpacing), qbY), () => DoSaveGame()).gameObject;
        _eSkillBtn = MakeIconBtn(actGo.transform, "☢", "技能", new Color(0.75f, 0.5f, 0.3f),
            new Vector2(qbStartX + (qbSize + qbSpacing) * 2, qbY), () => ShowPollutionSkillPanel()).gameObject;
        _eMenuBtn = MakeIconBtn(actGo.transform, "≡", "菜单", Dim,
            new Vector2(qbStartX + (qbSize + qbSpacing) * 3, qbY), () => ShowMenuPanel()).gameObject;

        // Mini-map (top right of map area)
        BuildMiniMap(rt);

        // 启动探索界面光效脉冲协程 (常驻, 动态开关)
        _eFxPulseCoroutine = StartCoroutine(PulseExploreMapFX());
    }


}
