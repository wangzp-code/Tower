using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    void BuildFragChoicePanel()
    {
        _fragChoiceOvl = Panel("FragChoiceOvl", new Color(0.06f, 0.05f, 0.12f, 1f));
        var rt = _fragChoiceOvl.GetComponent<RectTransform>();

        // === 六边形花纹纹理 ===
        var hexPattern = new GameObject("HexPattern", typeof(RectTransform), typeof(Image));
        hexPattern.transform.SetParent(rt, false);
        var hexRT = hexPattern.GetComponent<RectTransform>();
        hexRT.anchorMin = Vector2.zero; hexRT.anchorMax = Vector2.one;
        hexRT.offsetMin = Vector2.zero; hexRT.offsetMax = Vector2.zero;
        var hexImg = hexPattern.GetComponent<Image>();
        Texture2D hexTex = new Texture2D(64, 64);
        Color[] hexPixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                int cellX = x / 16; int cellY = y / 16;
                int lx = x % 16; int ly = y % 16;
                bool isOffset = cellY % 2 == 1;
                int ox = isOffset ? (lx + 8) % 16 : lx;
                int cx = 8, cy = 8;
                float dist = Mathf.Sqrt((ox - cx) * (ox - cx) + (ly - cy) * (ly - cy));
                bool isRing = dist > 5.5f && dist < 7f;
                bool isDot = dist < 1.5f;
                float a = isRing ? 0.12f : isDot ? 0.08f : 0f;
                hexPixels[y * 64 + x] = new Color(0.3f, 0.9f, 0.8f, a);
            }
        }
        hexTex.SetPixels(hexPixels);
        hexTex.filterMode = FilterMode.Bilinear;
        hexTex.wrapMode = TextureWrapMode.Repeat;
        hexTex.Apply();
        hexImg.sprite = Sprite.Create(hexTex, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
        hexImg.type = Image.Type.Tiled;
        hexImg.raycastTarget = false;

        // === 菱形格纹叠加 ===
        var diamondPattern = new GameObject("DiamondPattern", typeof(RectTransform), typeof(Image));
        diamondPattern.transform.SetParent(rt, false);
        var diaRT = diamondPattern.GetComponent<RectTransform>();
        diaRT.anchorMin = Vector2.zero; diaRT.anchorMax = Vector2.one;
        diaRT.offsetMin = Vector2.zero; diaRT.offsetMax = Vector2.zero;
        var diaImg = diamondPattern.GetComponent<Image>();
        Texture2D diaTex = new Texture2D(32, 32);
        Color[] diaPixels = new Color[32 * 32];
        for (int y = 0; y < 32; y++)
        {
            for (int x = 0; x < 32; x++)
            {
                int dx = Mathf.Abs(x - 16) + Mathf.Abs(y - 16);
                bool isEdge = dx == 12 || dx == 13;
                diaPixels[y * 32 + x] = isEdge ? new Color(0.5f, 0.3f, 0.8f, 0.06f) : Color.clear;
            }
        }
        diaTex.SetPixels(diaPixels);
        diaTex.filterMode = FilterMode.Bilinear;
        diaTex.wrapMode = TextureWrapMode.Repeat;
        diaTex.Apply();
        diaImg.sprite = Sprite.Create(diaTex, new Rect(0, 0, 32, 32), Vector2.one * 0.5f);
        diaImg.type = Image.Type.Tiled;
        diaImg.raycastTarget = false;

        // === 中央大光晕 — 主发光特效 ===
        var centerGlow = new GameObject("CenterGlow", typeof(RectTransform), typeof(Image));
        centerGlow.transform.SetParent(rt, false);
        var cgRT = centerGlow.GetComponent<RectTransform>();
        cgRT.anchorMin = new Vector2(0.5f, 0.55f); cgRT.anchorMax = new Vector2(0.5f, 0.55f);
        cgRT.pivot = new Vector2(0.5f, 0.5f);
        cgRT.sizeDelta = new Vector2(400, 400);
        centerGlow.GetComponent<Image>().color = new Color(0.15f, 0.5f, 0.45f, 0.2f);
        centerGlow.GetComponent<Image>().raycastTarget = false;

        // === 顶部横向光带 ===
        var topGlow = new GameObject("TopGlow", typeof(RectTransform), typeof(Image));
        topGlow.transform.SetParent(rt, false);
        var tgRT = topGlow.GetComponent<RectTransform>();
        tgRT.anchorMin = new Vector2(0, 0.85f); tgRT.anchorMax = new Vector2(1, 1);
        tgRT.offsetMin = Vector2.zero; tgRT.offsetMax = Vector2.zero;
        topGlow.GetComponent<Image>().color = new Color(0.2f, 0.4f, 0.7f, 0.15f);
        topGlow.GetComponent<Image>().raycastTarget = false;

        // === 底部渐变光带 ===
        var botGlow = new GameObject("BotGlow", typeof(RectTransform), typeof(Image));
        botGlow.transform.SetParent(rt, false);
        var bgRT2 = botGlow.GetComponent<RectTransform>();
        bgRT2.anchorMin = new Vector2(0, 0); bgRT2.anchorMax = new Vector2(1, 0.15f);
        bgRT2.offsetMin = Vector2.zero; bgRT2.offsetMax = Vector2.zero;
        botGlow.GetComponent<Image>().color = new Color(0.1f, 0.3f, 0.25f, 0.2f);
        botGlow.GetComponent<Image>().raycastTarget = false;

        // === 角落发光 — 更亮更大 ===
        AddCornerLight(rt, new Vector2(0, 1), new Color(0.3f, 0.6f, 1f), 0.3f);
        AddCornerLight(rt, new Vector2(1, 1), new Color(0.6f, 0.3f, 0.9f), 0.25f);
        AddCornerLight(rt, new Vector2(0, 0), new Color(0.1f, 0.6f, 0.5f), 0.2f);
        AddCornerLight(rt, new Vector2(1, 0), new Color(0.4f, 0.2f, 0.6f), 0.15f);

        // === 散落光点装饰 ===
        float[] dotX = {0.15f, 0.82f, 0.35f, 0.68f, 0.50f, 0.22f, 0.75f};
        float[] dotY = {0.78f, 0.25f, 0.45f, 0.72f, 0.18f, 0.55f, 0.88f};
        float[] dotSize = {6, 4, 5, 3, 7, 4, 5};
        float[] dotAlpha = {0.4f, 0.3f, 0.35f, 0.25f, 0.45f, 0.3f, 0.35f};
        for (int i = 0; i < dotX.Length; i++)
        {
            var dot = new GameObject($"Dot{i}", typeof(RectTransform), typeof(Image));
            dot.transform.SetParent(rt, false);
            var dotRT = dot.GetComponent<RectTransform>();
            dotRT.anchorMin = dotRT.anchorMax = new Vector2(dotX[i], dotY[i]);
            dotRT.pivot = new Vector2(0.5f, 0.5f);
            dotRT.sizeDelta = new Vector2(dotSize[i], dotSize[i]);
            dot.GetComponent<Image>().color = new Color(0.5f, 0.95f, 0.9f, dotAlpha[i]);
            dot.GetComponent<Image>().raycastTarget = false;
        }

        // === 内容容器 — 半透明，带双层边框 ===
        var container = new GameObject("Container", typeof(RectTransform), typeof(Image), typeof(Outline));
        container.transform.SetParent(rt, false);
        container.GetComponent<Image>().color = new Color(0.08f, 0.07f, 0.14f, 0.88f);
        var cOutline = container.GetComponent<Outline>();
        cOutline.effectColor = new Color(0.25f, 0.85f, 0.75f, 0.6f);
        cOutline.effectDistance = new Vector2(2, 2);
        var cRT = container.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0.04f, 0.05f);
        cRT.anchorMax = new Vector2(0.96f, 0.97f);
        cRT.offsetMin = Vector2.zero; cRT.offsetMax = Vector2.zero;

        // 容器内顶部高光条
        var innerTopLight = new GameObject("InnerTopLight", typeof(RectTransform), typeof(Image));
        innerTopLight.transform.SetParent(cRT, false);
        var itlRT = innerTopLight.GetComponent<RectTransform>();
        itlRT.anchorMin = new Vector2(0.1f, 0.92f); itlRT.anchorMax = new Vector2(0.9f, 0.98f);
        itlRT.offsetMin = Vector2.zero; itlRT.offsetMax = Vector2.zero;
        innerTopLight.GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.65f, 0.12f);
        innerTopLight.GetComponent<Image>().raycastTarget = false;

        // 容器内部底部渐变
        var innerGrad = new GameObject("InnerGrad", typeof(RectTransform), typeof(Image));
        innerGrad.transform.SetParent(cRT, false);
        var igRT = innerGrad.GetComponent<RectTransform>();
        igRT.anchorMin = Vector2.zero; igRT.anchorMax = new Vector2(1, 0.25f);
        igRT.offsetMin = Vector2.zero; igRT.offsetMax = Vector2.zero;
        innerGrad.GetComponent<Image>().color = new Color(0.1f, 0.2f, 0.18f, 0.35f);
        innerGrad.GetComponent<Image>().raycastTarget = false;

        var vl = AddVL(container, 10, 6);

        // 标题区域
        Spacer(vl, 6);
        var titleRow = new GameObject("TitleRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        titleRow.transform.SetParent(vl.transform, false);
        titleRow.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        titleRow.GetComponent<HorizontalLayoutGroup>().spacing = 10;

        var leftDeco = Txt(titleRow.transform, "◆━━", 18, new Color(0.4f, 0.95f, 0.9f), 22).GetComponent<Text>();
        var title = Txt(titleRow.transform, "选择碎片奖励", 22, new Color(1f, 0.95f, 0.75f), 34).GetComponent<Text>();
        title.fontStyle = FontStyle.Bold;
        title.GetComponent<Outline>().effectColor = new Color(0.4f, 0.8f, 0.7f, 0.9f);
        title.GetComponent<Outline>().effectDistance = new Vector2(1.5f, 1.5f);
        var rightDeco = Txt(titleRow.transform, "━━◆", 18, new Color(0.4f, 0.95f, 0.9f), 22).GetComponent<Text>();

        // 副标题
        Spacer(vl, 2);
        var subTitle = Txt(vl, "击败守护者，选择战利品", 13, new Color(0.5f, 0.7f, 0.7f), 20).GetComponent<Text>();
        subTitle.alignment = TextAnchor.MiddleCenter;

        // 分隔线 — 发光效果
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image), typeof(Outline));
        line.transform.SetParent(vl.transform, false);
        line.GetComponent<Image>().color = new Color(0.3f, 0.8f, 0.75f, 0.6f);
        line.GetComponent<Outline>().effectColor = new Color(0.2f, 0.7f, 0.65f, 0.3f);
        line.GetComponent<Outline>().effectDistance = new Vector2(0, 1);
        line.AddComponent<LayoutElement>().preferredHeight = 2;

        Spacer(vl, 6);

        // 网格布局 (2x2)
        var grid = new GameObject("FragGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(vl.transform, false);
        var gridGL = grid.GetComponent<GridLayoutGroup>();
        gridGL.cellSize = new Vector2(155, 225);
        gridGL.spacing = new Vector2(10, 12);
        gridGL.startCorner = GridLayoutGroup.Corner.UpperLeft;
        gridGL.startAxis = GridLayoutGroup.Axis.Horizontal;
        gridGL.childAlignment = TextAnchor.MiddleCenter;
        gridGL.constraint = GridLayoutGroup.Constraint.FixedColumnCount;
        gridGL.constraintCount = 2;
        var gridLE = grid.AddComponent<LayoutElement>();
        gridLE.preferredWidth = 320;
        gridLE.preferredHeight = 462;
        gridLE.minHeight = 462;

        _fragChoiceCards[0] = BuildFragCardNew(grid.transform, 0);
        _fragChoiceCards[1] = BuildFragCardNew(grid.transform, 1);
        _fragChoiceCards[2] = BuildFragCardNew(grid.transform, 2);

        Spacer(vl, 6);

        // 底部分割线
        var bottomLine = new GameObject("BottomLine", typeof(RectTransform), typeof(Image));
        bottomLine.transform.SetParent(vl.transform, false);
        bottomLine.GetComponent<Image>().color = new Color(0.2f, 0.45f, 0.4f, 0.4f);
        bottomLine.AddComponent<LayoutElement>().preferredHeight = 1;

        Spacer(vl, 4);

        // 放弃选项区域 — 带背景面板
        var skipArea = new GameObject("SkipArea", typeof(RectTransform), typeof(Image));
        skipArea.transform.SetParent(vl.transform, false);
        skipArea.GetComponent<Image>().color = new Color(0.09f, 0.13f, 0.16f, 0.7f);
        skipArea.AddComponent<LayoutElement>().preferredHeight = 62;
        var skipAreaVL = skipArea.AddComponent<VerticalLayoutGroup>();
        skipAreaVL.spacing = 3;
        skipAreaVL.padding = new RectOffset(14, 14, 8, 8);
        skipAreaVL.childAlignment = TextAnchor.MiddleCenter;
        skipAreaVL.childForceExpandWidth = true;
        skipAreaVL.childForceExpandHeight = false;

        // 顶部细线装饰
        var skipTopLine = new GameObject("TopLine", typeof(RectTransform), typeof(Image));
        skipTopLine.transform.SetParent(skipArea.transform, false);
        var stlRT = skipTopLine.GetComponent<RectTransform>();
        stlRT.anchorMin = new Vector2(0.05f, 1); stlRT.anchorMax = new Vector2(0.95f, 1);
        stlRT.offsetMin = new Vector2(0, -1); stlRT.offsetMax = Vector2.zero;
        skipTopLine.GetComponent<Image>().color = new Color(0.3f, 0.7f, 0.65f, 0.4f);
        skipTopLine.GetComponent<Image>().raycastTarget = false;

        var skipContainer = new GameObject("SkipRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        skipContainer.transform.SetParent(skipAreaVL.transform, false);
        skipContainer.GetComponent<HorizontalLayoutGroup>().childAlignment = TextAnchor.MiddleCenter;
        skipContainer.GetComponent<HorizontalLayoutGroup>().spacing = 14;

        Txt(skipContainer.transform, "不需要这些碎片？", 12, new Color(0.5f, 0.6f, 0.6f), 18);
        var skipBtn = BtnGo(skipContainer.transform, "转化为 50 进化点", 13, new Color(0.6f, 0.8f, 0.75f), 34);
        skipBtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.SkipAllFragments());
        var skipImg = skipBtn.GetComponent<Image>();
        skipImg.color = new Color(0.12f, 0.22f, 0.20f, 0.9f);
        var skipOutline = skipBtn.AddComponent<Outline>();
        skipOutline.effectColor = new Color(0.25f, 0.7f, 0.6f, 0.5f);
        skipOutline.effectDistance = new Vector2(1, 1);

        // EP说明
        var epDesc = Txt(skipAreaVL, "进化点不会随局数重置，用于永久升级", 10, new Color(0.4f, 0.5f, 0.5f), 14).GetComponent<Text>();
        epDesc.alignment = TextAnchor.MiddleCenter;

        _fragChoiceOvl.SetActive(false);
    }

    GameObject BuildFragCardNew(Transform parent, int index)
    {
        Color activeFrame = new Color(0.0f, 0.94f, 0.94f);
        Color passiveFrame = new Color(0.65f, 0.33f, 0.94f);

        var card = new GameObject($"FragCard{index}", typeof(RectTransform), typeof(Image), typeof(Button));
        card.transform.SetParent(parent, false);
        var cardImg = card.GetComponent<Image>();
        cardImg.color = activeFrame;
        cardImg.type = Image.Type.Sliced;
        card.AddComponent<LayoutElement>().preferredWidth = 155;
        card.AddComponent<LayoutElement>().preferredHeight = 225;

        float frameThick = 4f;

        // === 内部面板 — 比卡片小frameThick，形成边框 ===
        var inner = new GameObject("Inner", typeof(RectTransform), typeof(Image));
        inner.transform.SetParent(card.transform, false);
        var innerRT = inner.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(frameThick, frameThick);
        innerRT.offsetMax = new Vector2(-frameThick, -frameThick);
        var innerImg = inner.GetComponent<Image>();
        innerImg.color = new Color(0.07f, 0.09f, 0.16f, 0.96f);
        innerImg.raycastTarget = false;

        // === 内边框装饰线 — 1px高亮线 ===
        var innerLine = new GameObject("InnerLine", typeof(RectTransform), typeof(Image));
        innerLine.transform.SetParent(inner.transform, false);
        var ilRT = innerLine.GetComponent<RectTransform>();
        ilRT.anchorMin = Vector2.zero; ilRT.anchorMax = Vector2.one;
        ilRT.offsetMin = new Vector2(1, 1); ilRT.offsetMax = new Vector2(-1, -1);
        innerLine.GetComponent<Image>().color = new Color(0.3f, 0.9f, 0.85f, 0.25f);
        innerLine.GetComponent<Image>().raycastTarget = false;
        _fragInnerLine[index] = innerLine.GetComponent<Image>();

        // === 外发光层 ===
        var outerGlow = new GameObject("OuterGlow", typeof(RectTransform), typeof(Image));
        outerGlow.transform.SetParent(card.transform, false);
        var ogRT = outerGlow.GetComponent<RectTransform>();
        ogRT.anchorMin = new Vector2(-0.1f, -0.08f); ogRT.anchorMax = new Vector2(1.1f, 1.08f);
        ogRT.offsetMin = Vector2.zero; ogRT.offsetMax = Vector2.zero;
        var outerGlowImg = outerGlow.GetComponent<Image>();
        Texture2D glowTex = new Texture2D(64, 64);
        Color[] glowPixels = new Color[64 * 64];
        for (int y = 0; y < 64; y++)
        {
            for (int x = 0; x < 64; x++)
            {
                float cx = 32f, cy = 32f;
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float t = Mathf.Clamp01((dist - 18f) / 14f);
                float alpha = Mathf.Exp(-t * 2.8f);
                glowPixels[y * 64 + x] = new Color(0.15f, 0.75f, 0.7f, alpha * 0.4f);
            }
        }
        glowTex.SetPixels(glowPixels);
        glowTex.filterMode = FilterMode.Bilinear;
        glowTex.Apply();
        outerGlowImg.sprite = Sprite.Create(glowTex, new Rect(0, 0, 64, 64), Vector2.one * 0.5f);
        outerGlowImg.type = Image.Type.Sliced;
        outerGlowImg.raycastTarget = false;
        outerGlow.transform.SetAsFirstSibling();
        _fragBorderGlow[index] = outerGlowImg;

        // === 角标装饰 — 明显的L形 ===
        float cornerSize = 10f;
        _fragCornerTL[index] = CreateCornerMark(inner.transform, "CornerTL",
            new Vector2(0, 1), new Vector2(0, 1), cornerSize, cornerSize, activeFrame);
        _fragCornerTR[index] = CreateCornerMark(inner.transform, "CornerTR",
            new Vector2(1, 1), new Vector2(1, 1), -cornerSize, cornerSize, activeFrame);
        _fragCornerBL[index] = CreateCornerMark(inner.transform, "CornerBL",
            new Vector2(0, 0), new Vector2(0, 0), cornerSize, -cornerSize, activeFrame);
        _fragCornerBR[index] = CreateCornerMark(inner.transform, "CornerBR",
            new Vector2(1, 0), new Vector2(1, 0), -cornerSize, -cornerSize, activeFrame);

        // === 内部渐变高光 ===
        var innerHighlight = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
        innerHighlight.transform.SetParent(inner.transform, false);
        var ihRT = innerHighlight.GetComponent<RectTransform>();
        ihRT.anchorMin = new Vector2(0, 0.5f); ihRT.anchorMax = Vector2.one;
        ihRT.offsetMin = Vector2.zero; ihRT.offsetMax = Vector2.zero;
        innerHighlight.GetComponent<Image>().color = new Color(0.15f, 0.45f, 0.4f, 0.25f);
        innerHighlight.GetComponent<Image>().raycastTarget = false;

        var innerBotGrad = new GameObject("BotGrad", typeof(RectTransform), typeof(Image));
        innerBotGrad.transform.SetParent(inner.transform, false);
        var ibgRT = innerBotGrad.GetComponent<RectTransform>();
        ibgRT.anchorMin = Vector2.zero; ibgRT.anchorMax = new Vector2(1, 0.3f);
        ibgRT.offsetMin = Vector2.zero; ibgRT.offsetMax = Vector2.zero;
        innerBotGrad.GetComponent<Image>().color = new Color(0.08f, 0.2f, 0.18f, 0.3f);
        innerBotGrad.GetComponent<Image>().raycastTarget = false;

        // === 竖条纹背景 ===
        var stripes = new GameObject("Stripes", typeof(RectTransform), typeof(Image));
        stripes.transform.SetParent(inner.transform, false);
        var sRT = stripes.GetComponent<RectTransform>();
        sRT.anchorMin = Vector2.zero; sRT.anchorMax = Vector2.one;
        sRT.offsetMin = Vector2.zero; sRT.offsetMax = Vector2.zero;
        var sImg = stripes.GetComponent<Image>();
        Texture2D stripeTex = new Texture2D(20, 4);
        Color[] stripePixels = new Color[20 * 4];
        for (int y2 = 0; y2 < 4; y2++)
            for (int x2 = 0; x2 < 20; x2++)
                stripePixels[y2 * 20 + x2] = x2 % 5 == 0 ? new Color(0.35f, 0.75f, 0.7f, 0.06f) : Color.clear;
        stripeTex.SetPixels(stripePixels);
        stripeTex.filterMode = FilterMode.Point;
        stripeTex.wrapMode = TextureWrapMode.Repeat;
        stripeTex.Apply();
        sImg.sprite = Sprite.Create(stripeTex, new Rect(0, 0, 20, 4), Vector2.one * 0.5f);
        sImg.type = Image.Type.Tiled;
        sImg.raycastTarget = false;

        // === 内容容器 ===
        var cvl = inner.AddComponent<VerticalLayoutGroup>();
        cvl.spacing = 2;
        cvl.padding = new RectOffset(10, 10, 8, 6);
        cvl.childAlignment = TextAnchor.UpperCenter;
        cvl.childForceExpandWidth = true;
        cvl.childForceExpandHeight = false;

        // 顶部色条（主动/被动区分）
        var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        topBar.transform.SetParent(cvl.transform, false);
        topBar.AddComponent<LayoutElement>().preferredHeight = 4;
        _fragChoiceTagBg[index] = topBar.GetComponent<Image>();
        _fragChoiceTagBg[index].color = activeFrame * 0.7f;

        Spacer(cvl, 2);

        // 图标
        _fragChoiceIcons[index] = Txt(cvl, "◆", 30, new Color(0.95f, 0.98f, 0.9f), 42).GetComponent<Text>();
        _fragChoiceIcons[index].alignment = TextAnchor.MiddleCenter;
        var iconShadow = _fragChoiceIcons[index].gameObject.AddComponent<Shadow>();
        iconShadow.effectColor = Mask80;
        iconShadow.effectDistance = new Vector2(2, -2);

        Spacer(cvl, 1);

        // 名称
        _fragChoiceLabels[index] = Txt(cvl, "", 14, new Color(0.98f, 0.98f, 0.95f), 20).GetComponent<Text>();
        _fragChoiceLabels[index].fontStyle = FontStyle.Bold;
        _fragChoiceLabels[index].alignment = TextAnchor.MiddleCenter;
        var labelShadow = _fragChoiceLabels[index].gameObject.AddComponent<Shadow>();
        labelShadow.effectColor = new Color(0, 0, 0, 0.9f);
        labelShadow.effectDistance = new Vector2(1, -1);

        // 星级
        _fragChoiceStars[index] = Txt(cvl, "★★★☆☆", 14, new Color(1f, 0.9f, 0.35f), 20).GetComponent<Text>();
        _fragChoiceStars[index].fontStyle = FontStyle.Bold;
        _fragChoiceStars[index].alignment = TextAnchor.MiddleCenter;
        var starShadow = _fragChoiceStars[index].gameObject.AddComponent<Shadow>();
        starShadow.effectColor = Mask80;
        starShadow.effectDistance = new Vector2(1, -1);

        Spacer(cvl, 1);

        // 核心效果区域
        var effectArea = new GameObject("EffectArea", typeof(RectTransform), typeof(Image));
        effectArea.transform.SetParent(cvl.transform, false);
        effectArea.GetComponent<Image>().color = new Color(0.04f, 0.08f, 0.1f, 0.92f);
        var effectLE = effectArea.AddComponent<LayoutElement>();
        effectLE.preferredHeight = 40;
        
        var effectVL = effectArea.AddComponent<VerticalLayoutGroup>();
        effectVL.spacing = 1;
        effectVL.childAlignment = TextAnchor.MiddleCenter;
        effectVL.padding = new RectOffset(6, 6, 3, 3);

        _fragChoiceDescs[index] = Txt(effectVL, "", 13, new Color(0.95f, 0.98f, 0.9f), 18).GetComponent<Text>();
        _fragChoiceDescs[index].alignment = TextAnchor.MiddleCenter;
        _fragChoiceDescs[index].fontStyle = FontStyle.Bold;
        var descShadow = _fragChoiceDescs[index].gameObject.AddComponent<Shadow>();
        descShadow.effectColor = new Color(0, 0, 0, 0.85f);
        descShadow.effectDistance = new Vector2(1, -1);

        _fragChoiceTags[index] = Txt(effectVL, "▶ 主动", 10, new Color(0.0f, 0.98f, 0.98f), 14).GetComponent<Text>();
        _fragChoiceTags[index].alignment = TextAnchor.MiddleCenter;
        var tagShadow = _fragChoiceTags[index].gameObject.AddComponent<Shadow>();
        tagShadow.effectColor = Mask70;
        tagShadow.effectDistance = new Vector2(1, -1);

        Spacer(cvl, 2);

        // 持有进度条区域
        var progressArea = new GameObject("ProgressArea", typeof(RectTransform));
        progressArea.transform.SetParent(cvl.transform, false);
        var paVL = progressArea.AddComponent<VerticalLayoutGroup>();
        paVL.spacing = 3;
        paVL.childAlignment = TextAnchor.MiddleCenter;
        paVL.childForceExpandWidth = true;
        paVL.childForceExpandHeight = false;
        var paLE = progressArea.AddComponent<LayoutElement>();
        paLE.preferredHeight = 32;

        // 进度条本体
        var progressBar = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image));
        progressBar.transform.SetParent(paVL.transform, false);
        progressBar.GetComponent<Image>().color = new Color(0.06f, 0.10f, 0.10f, 0.8f);
        var pbLE = progressBar.AddComponent<LayoutElement>();
        pbLE.preferredHeight = 12;
        pbLE.preferredWidth = 110;

        var progressFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        progressFill.transform.SetParent(progressBar.transform, false);
        var pfRT = progressFill.GetComponent<RectTransform>();
        pfRT.anchorMin = new Vector2(0, 0);
        pfRT.anchorMax = new Vector2(0, 1);
        pfRT.offsetMin = new Vector2(2, 2);
        pfRT.offsetMax = new Vector2(0, -2);
        _fragChoiceProgressFill[index] = pfRT;
        _fragChoiceProgressFillImg[index] = progressFill.GetComponent<Image>();
        _fragChoiceProgressFillImg[index].color = new Color(0.2f, 0.8f, 0.75f);

        // 进度文字 — 单独一行在进度条下方
        _fragChoiceProgress[index] = Txt(paVL, "持有 0/3", 11, new Color(0.85f, 0.92f, 0.88f), 14).GetComponent<Text>();
        _fragChoiceProgress[index].alignment = TextAnchor.MiddleCenter;
        _fragChoiceProgress[index].raycastTarget = false;
        var progShadow = _fragChoiceProgress[index].gameObject.AddComponent<Shadow>();
        progShadow.effectColor = Mask70;
        progShadow.effectDistance = new Vector2(1, -1);

        // 按钮事件
        int idx = index;
        card.GetComponent<Button>().targetGraphic = card.GetComponent<Image>();
        card.GetComponent<Button>().onClick.AddListener(() => {
            CompleteGameSystem.Instance?.SelectFragment(idx);
        });

        var btnColors = card.GetComponent<Button>().colors;
        btnColors.normalColor = Color.white;
        btnColors.highlightedColor = new Color(1.3f, 1.3f, 1.3f);
        btnColors.pressedColor = new Color(0.85f, 0.85f, 0.85f);
        card.GetComponent<Button>().colors = btnColors;
        card.AddComponent<ButtonPressFeedback>();

        _fragBorderTop[index] = cardImg;
        _fragBorderBottom[index] = cardImg;
        _fragBorderLeft[index] = cardImg;
        _fragBorderRight[index] = cardImg;

        return card;
    }

    void SyncFragChoice(CompleteGameSystem gs)
    {
        if (_fragChoiceOvl == null || !gs.ShowingFragmentChoice) return;

        var candidates = gs.CurrentFragCandidates;
        var traits = gs.CurrentFragTraits;
        if (candidates == null || traits == null) return;

        for (int i = 0; i < 3 && i < candidates.Length; i++)
        {
            var frag = candidates[i];
            if (frag == null) continue;

            ST(_fragChoiceIcons[i], frag.fragIcon);
            ST(_fragChoiceLabels[i], frag.fragName);
            ST(_fragChoiceDescs[i], frag.skillDesc);
            string tag = frag.passive ? "被动" : "主动";
            ST(_fragChoiceTags[i], $"[ {tag} ]");

            if (_fragChoiceTags[i] != null)
                _fragChoiceTags[i].color = frag.passive ? Purp : Cyan;
            if (_fragChoiceTagBg[i] != null)
                _fragChoiceTagBg[i].color = frag.passive
                    ? new Color(0.25f, 0.1f, 0.35f, 0.5f)
                    : new Color(0.1f, 0.3f, 0.4f, 0.5f);

            SetFragBorderTypeColor(i, frag.passive);

            int starCount = frag.passive ? 2 : (frag.maxUses >= 2 ? 1 : 3);
            string stars = "";
            for (int s = 0; s < 3; s++) stars += s < starCount ? "★" : "☆";
            ST(_fragChoiceStars[i], stars);

            int owned = 0;
            foreach (var sf in gs.SkillFragments)
                if (sf.type == traits[i]) owned++;
            ST(_fragChoiceProgress[i], $"持有 {owned}/3");

            // 更新进度条填充
            if (_fragChoiceProgressFill[i] != null)
            {
                float fillRatio = Mathf.Clamp01(owned / 3f);
                _fragChoiceProgressFill[i].anchorMax = new Vector2(fillRatio, 1);
            }
        }
    }

    void BuildSynthConfirmPanel()
    {
        _synthConfirmOvl = Panel("SynthConfirmOvl", OvlBgDense);
        var rt = _synthConfirmOvl.GetComponent<RectTransform>();
        var vl = AddVL(_synthConfirmOvl, 20, 8);
        Spacer(vl, 200);

        var (box, bvl) = InfoCard(vl.transform, new Color(0.08f, 0.05f, 0.15f, 0.95f), 280);
        Txt(bvl, "⚡ 碎片合成 ⚡", 20, Gold, 34).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        Spacer(bvl, SpaceSmall);
        _synthTitle = Txt(bvl, "", 16, Bright, 28).GetComponent<Text>();
        _synthTitle.alignment = TextAnchor.MiddleCenter;
        _synthDesc = Txt(bvl, "", 13, Dim, 40).GetComponent<Text>();
        _synthDesc.alignment = TextAnchor.MiddleCenter;
        Spacer(bvl, SpaceSmall);
        _synthPreview = Txt(bvl, "", 14, Cyan, 50).GetComponent<Text>();
        _synthPreview.alignment = TextAnchor.MiddleCenter;
        Spacer(bvl, SpaceMed);

        var btnRow = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(bvl.transform, false);
        var brHL = btnRow.GetComponent<HorizontalLayoutGroup>();
        brHL.spacing = 20; brHL.childAlignment = TextAnchor.MiddleCenter;
        brHL.childForceExpandWidth = true; brHL.childForceExpandHeight = false;
        var brLE = btnRow.AddComponent<LayoutElement>();
        brLE.preferredHeight = 50;

        var confirmBtn = PrimaryBtn(btnRow.transform, "合成!", 16, 45);
        confirmBtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ConfirmSynthesize());
        var cancelBtn = SecondaryBtn(btnRow.transform, "取消", Dim, 14, 45);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.CancelSynthesize());

        _synthConfirmOvl.SetActive(false);
    }

    void SyncSynthConfirm(CompleteGameSystem gs)
    {
        SA(_synthConfirmOvl, gs.ShowingSynthConfirm);
        if (!gs.ShowingSynthConfirm || string.IsNullOrEmpty(gs.PendingSynthTrait)) return;

        var entry = gs.GetFragSkillEntry(gs.PendingSynthTrait);
        if (entry == null) return;
        string tag = entry.passive ? "被动" : "主动";
        ST(_synthTitle, $"{entry.fragIcon} {entry.fragName} x3 → 合成");
        ST(_synthDesc, $"将消耗3个 {entry.fragName}");
        string preview = entry.passive
            ? $"◈ [{tag}] {entry.skillName}: {entry.skillDesc}"
            : $"⚡ [{tag}] {entry.skillName}: {entry.skillDesc} ({entry.maxUses}次)";
        ST(_synthPreview, preview);
    }

    void SyncSkillBar(CompleteGameSystem gs)
    {
        if (_skillBarRow == null) return;
        bool inCombat = gs.CurrentScreen == CompleteGameSystem.RunScreen.Combat;
        for (int i = 0; i < 3; i++)
        {
            if (i < gs.ActiveSkills.Count && inCombat)
            {
                var skill = gs.ActiveSkills[i];
                SA(_skillBarBtns[i].gameObject, true);
                ST(_skillBarLabels[i], skill.icon + " " + skill.name);
                ST(_skillBarUses[i], skill.uses + "/" + skill.maxUses);
                _skillBarBtns[i].interactable = skill.uses > 0;
                var img = _skillBarBtns[i].GetComponent<Image>();
                var ol = _skillBarBtns[i].GetComponent<Outline>();
                if (skill.uses > 0)
                {
                    if (img) img.color = new Color(0.06f, 0.18f, 0.25f, 0.95f);
                    if (ol) ol.effectColor = new Color(0f, 1f, 0.816f, 0.6f);
                }
                else
                {
                    if (img) img.color = new Color(0.08f, 0.08f, 0.1f, 0.6f);
                    if (ol) ol.effectColor = new Color(0.3f, 0.3f, 0.3f, 0.3f);
                }
            }
            else
            {
                SA(_skillBarBtns[i].gameObject, false);
            }
        }
    }

    Text _storyContinueBtnText;
    CanvasGroup _storyCanvasGroup;
    CanvasGroup _storyPanelCanvasGroup;
    string _storyLastTitle;
    string _storyLastBody;
    LayoutElement _storyProgressDots;
    Text _storyProgressText;

    void BuildStoryPanel()
    {
        Debug.Log("[Intro] BuildStoryPanel called - creating story panel UI.");

        _storyOvl = new GameObject("StoryOvl", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        _storyOvl.transform.SetParent(_root, false);
        var blockerRT = _storyOvl.GetComponent<RectTransform>();
        blockerRT.anchorMin = Vector2.zero;
        blockerRT.anchorMax = Vector2.one;
        blockerRT.offsetMin = Vector2.zero;
        blockerRT.offsetMax = Vector2.zero;
        var blockerImg = _storyOvl.GetComponent<Image>();
        blockerImg.color = new Color(0, 0, 0, 0.55f);
        blockerImg.raycastTarget = true;

        _storyCanvasGroup = _storyOvl.GetComponent<CanvasGroup>();
        _storyCanvasGroup.alpha = 0f;
        _storyCanvasGroup.blocksRaycasts = true;

        _storyBlocker = _storyOvl;

        // Panel container - anchored to bottom, ~45% height
        _storyPanel = new GameObject("StoryPanel", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        _storyPanel.transform.SetParent(_root, false);
        var panelRT = _storyPanel.GetComponent<RectTransform>();
        panelRT.anchorMin = new Vector2(0.06f, 0);
        panelRT.anchorMax = new Vector2(0.94f, 0.45f);
        panelRT.offsetMin = new Vector2(0, 20);
        panelRT.offsetMax = new Vector2(0, 20);
        
        var panelImg = _storyPanel.GetComponent<Image>();
        panelImg.color = new Color(0.04f, 0.03f, 0.08f, 0.97f);
        panelImg.raycastTarget = false;
        
        // Outer glow border
        var panelOutline = _storyPanel.GetComponent<Outline>();
        panelOutline.effectColor = new Color(0f, 1f, 0.816f, 0.35f);
        panelOutline.effectDistance = new Vector2(2, 3);
        
        // Shadow for depth
        var panelShadow = _storyPanel.GetComponent<Shadow>();
        panelShadow.effectColor = new Color(0, 0, 0, 0.5f);
        panelShadow.effectDistance = new Vector2(3, -3);

        // Inner border - second outline
        var innerBorderGo = new GameObject("InnerBorder", typeof(RectTransform), typeof(Image), typeof(Outline));
        innerBorderGo.transform.SetParent(_storyPanel.transform, false);
        var innerRT = innerBorderGo.GetComponent<RectTransform>();
        innerRT.anchorMin = Vector2.zero;
        innerRT.anchorMax = Vector2.one;
        innerRT.offsetMin = new Vector2(2, 2);
        innerRT.offsetMax = new Vector2(-2, -2);
        innerBorderGo.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        innerBorderGo.GetComponent<Image>().raycastTarget = false;
        var innerOl = innerBorderGo.GetComponent<Outline>();
        innerOl.effectColor = new Color(0f, 1f, 0.816f, 0.12f);
        innerOl.effectDistance = new Vector2(1, 1);

        var panelCg = _storyPanel.AddComponent<CanvasGroup>();
        panelCg.alpha = 0f;
        panelCg.blocksRaycasts = false;
        _storyPanelCanvasGroup = panelCg;

        // Content container with padding
        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(_storyPanel.transform, false);
        var contentRT = contentGo.GetComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero;
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = new Vector2(0, 0);
        contentRT.offsetMax = new Vector2(0, 0);
        
        var vl = AddVL(contentGo, 28, 8);

        // Top accent bar
        var topBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        topBar.transform.SetParent(vl.transform, false);
        var topBarImg = topBar.GetComponent<Image>();
        topBarImg.color = new Color(0f, 1f, 0.816f, 0.6f);
        topBarImg.raycastTarget = false;
        var topBarRT = topBar.GetComponent<RectTransform>();
        topBarRT.sizeDelta = new Vector2(0, 3);
        topBar.AddComponent<LayoutElement>().preferredHeight = 3;

        Spacer(vl, 18);

        // Speaker name / Title
        _storyTitle = Txt(vl, "", 22, Gold, 36).GetComponent<Text>();
        _storyTitle.alignment = TextAnchor.MiddleCenter;
        _storyTitle.fontStyle = FontStyle.Bold;

        Spacer(vl, 8);

        // Divider line
        var divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divider.transform.SetParent(vl.transform, false);
        divider.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);
        divider.GetComponent<Image>().raycastTarget = false;
        divider.AddComponent<LayoutElement>().preferredHeight = 1;

        Spacer(vl, 14);

        // Body text
        _storyBody = Txt(vl, "", 17, Bright, 160).GetComponent<Text>();
        _storyBody.alignment = TextAnchor.UpperCenter;
        _storyBody.horizontalOverflow = HorizontalWrapMode.Wrap;
        _storyBody.lineSpacing = 1.3f;

        Spacer(vl, 12);

        // Progress dots (for intro)
        var progressGo = new GameObject("Progress", typeof(RectTransform));
        progressGo.transform.SetParent(vl.transform, false);
        var progressVL = progressGo.AddComponent<VerticalLayoutGroup>();
        progressVL.spacing = 4;
        progressVL.childAlignment = TextAnchor.MiddleCenter;
        _storyProgressDots = progressGo.AddComponent<LayoutElement>();
        _storyProgressDots.preferredHeight = 14;
        
        _storyProgressText = Txt(progressGo.transform, "", 12, Dim, 14).GetComponent<Text>();
        _storyProgressText.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 8);

        // Button row
        var btnRow = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(vl.transform, false);
        var btnRowHL = btnRow.GetComponent<HorizontalLayoutGroup>();
        btnRowHL.childAlignment = TextAnchor.MiddleCenter;
        btnRowHL.spacing = 12;
        btnRowHL.padding = new RectOffset(40, 40, 0, 0);

        var btn = PrimaryBtn(btnRow.transform, "继续", 16, 48);
        _storyContinueBtnText = btn.GetComponentInChildren<Text>();
        btn.GetComponent<Button>().onClick.AddListener(() => {
            AdvanceStoryOrIntro();
        });
        
        var clickHint = Txt(btnRow.transform, "点击任意处推进", 12, Dim, 20);
        clickHint.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        var clickArea = _storyOvl.AddComponent<UnityEngine.UI.Button>();
        clickArea.targetGraphic = blockerImg;
        clickArea.onClick.AddListener(() => {
            AdvanceStoryOrIntro();
        });

        _storyOvl.SetActive(false);
        _storyPanel.SetActive(false);
    }

    void AdvanceStoryOrIntro()
    {
        var gs = CompleteGameSystem.Instance;
        if (gs == null) return;

        if (gs.IsIntroPlaying())
        {
            if (_storyTypewriterActive)
            {
                CompleteTypewriterNow();
                return;
            }
            gs.AdvanceIntroDialogue();
        }
        else
        {
            gs.DismissStoryEvent();
        }
    }

    void CompleteTypewriterNow()
    {
        if (!_storyTypewriterActive) return;
        if (_storyTypewriterText != null)
            _storyTypewriterText.text = _storyTypewriterFull;
        _storyTypewriterChar = _storyTypewriterFull.Length;
        _storyTypewriterActive = false;
    }

    void UpdateStoryEffects()
    {
        if (_storyCanvasGroup == null || _storyOvl == null || !_storyOvl.activeSelf)
        {
            _storyTypewriterActive = false;
            return;
        }

        if (_storyFadeAnimating)
        {
            _storyFadeTime += Time.deltaTime;
            float t = Mathf.Clamp01(_storyFadeTime / 0.35f);
            if (_storyCanvasGroup != null) _storyCanvasGroup.alpha = t;
            if (_storyPanelCanvasGroup != null) _storyPanelCanvasGroup.alpha = t;
            if (t >= 1f)
            {
                _storyFadeAnimating = false;
                _storyFadeTime = 0f;
                Debug.Log("[Intro] Story panel fade-in complete.");
            }
        }
        else if (_storyCanvasGroup != null && _storyCanvasGroup.alpha < 1f)
        {
            _storyCanvasGroup.alpha = 1f;
            if (_storyPanelCanvasGroup != null) _storyPanelCanvasGroup.alpha = 1f;
        }

        if (_storyTypewriterActive && _storyTypewriterText != null)
        {
            _storyTypewriterDelay -= Time.deltaTime;
            if (_storyTypewriterDelay <= 0f)
            {
                _storyTypewriterChar++;
                _storyTypewriterText.text = _storyTypewriterFull.Substring(0, _storyTypewriterChar);
                _storyTypewriterDelay = 0.02f;
                if (_storyTypewriterChar >= _storyTypewriterFull.Length)
                {
                    _storyTypewriterActive = false;
                    Debug.Log("[Intro] Typewriter complete.");
                }
            }
        }
    }

    void SyncStory(CompleteGameSystem gs)
    {
        if (gs.CurrentScreen == CompleteGameSystem.RunScreen.Exploration)
        {
            bool isIntro = gs.IsIntroPlaying();
            if (isIntro || gs.ShowingStoryEvent)
            {
                if (_storyOvl == null) BuildStoryPanel();
                _storyOvl.SetActive(true);
                _storyPanel.SetActive(true);

                string newTitle = gs.CurrentStoryEventTitle;
                string newBody = gs.CurrentStoryEventText;

                if (_storyLastTitle != newTitle || _storyLastBody != newBody)
                {
                    _storyLastTitle = newTitle;
                    _storyLastBody = newBody;
                    _storyTitle.text = isIntro && !string.IsNullOrEmpty(newTitle) ? $"【{newTitle}】" : newTitle;
                    _storyTypewriterFull = newBody ?? "";
                    _storyTypewriterChar = 0;
                    _storyTypewriterDelay = 0f;
                    _storyTypewriterText = _storyBody;
                    _storyBody.text = "";
                    _storyTypewriterActive = _storyTypewriterFull.Length > 0;
                    _storyFadeAnimating = true;
                    _storyFadeTime = 0f;
                    if (_storyCanvasGroup != null) _storyCanvasGroup.alpha = 0f;
                    if (_storyPanelCanvasGroup != null) _storyPanelCanvasGroup.alpha = 0f;
                    Debug.Log($"[Intro] SyncStory update: title='{newTitle}', bodyLen={_storyTypewriterFull.Length}");
                }

                Color titleColor = Gold;
                if (isIntro)
                {
                    var ss = StorySystem.Instance;
                    if (ss != null && ss.introCurrentIndex >= 0 && ss.introCurrentIndex < ss.introLines.Count)
                    {
                        var line = ss.introLines[ss.introCurrentIndex];
                        if (!string.IsNullOrEmpty(line.colorHex))
                        {
                            if (ColorUtility.TryParseHtmlString(line.colorHex, out var parsed))
                                titleColor = parsed;
                        }

                        if (_storyContinueBtnText != null)
                            _storyContinueBtnText.text = (ss.introCurrentIndex >= ss.introLines.Count - 1) ? "开始探索" : "继续";

                        if (_storyProgressText != null)
                        {
                            _storyProgressText.text = $"{ss.introCurrentIndex + 1} / {ss.introLines.Count}";
                            _storyProgressText.gameObject.SetActive(true);
                        }
                    }
                }
                else
                {
                    if (_storyContinueBtnText != null)
                        _storyContinueBtnText.text = "继续";
                    
                    if (_storyProgressText != null)
                        _storyProgressText.gameObject.SetActive(false);
                }
                _storyTitle.color = titleColor;
            }
            else if (_storyOvl != null && _storyOvl.activeSelf)
            {
                _storyOvl.SetActive(false);
                _storyPanel.SetActive(false);
                _storyTypewriterActive = false;
                _storyLastTitle = null;
                _storyLastBody = null;
                if (_storyProgressText != null) _storyProgressText.gameObject.SetActive(false);
            }
        }
    }

    public void ShowStoryEvent(StorySystem.StoryTrigger trigger)
    {
        if (_storyOvl == null) BuildStoryPanel();
        
        _storyOvl.SetActive(true);
        _storyPanel.SetActive(true);

        Debug.Log($"[Story] ShowStoryEvent: type={trigger.type}, title='{trigger.title}', textLen={trigger.text?.Length ?? 0}");

        Color titleColor = Gold;
        if (trigger.type == "note") titleColor = new Color(1, 0.8f, 0);
        else if (trigger.type == "revelation") titleColor = new Color(1, 0, 0.43f);
        else if (trigger.type == "fragment") titleColor = new Color(0.71f, 0.33f, 1);
        else if (trigger.type == "finale") titleColor = new Color(1, 1, 0);

        CompleteGameSystem.Instance.ShowStoryEvent(trigger.title, trigger.text);

        _storyLastTitle = trigger.title;
        _storyLastBody = trigger.text;
        _storyTitle.text = trigger.title;
        _storyTitle.color = titleColor;
        _storyTypewriterFull = trigger.text ?? "";
        _storyTypewriterChar = 0;
        _storyTypewriterDelay = 0f;
        _storyTypewriterText = _storyBody;
        _storyBody.text = "";
        _storyTypewriterActive = _storyTypewriterFull.Length > 0;
        _storyFadeAnimating = true;
        _storyFadeTime = 0f;
        if (_storyCanvasGroup != null) _storyCanvasGroup.alpha = 0f;
        if (_storyPanelCanvasGroup != null) _storyPanelCanvasGroup.alpha = 0f;
        
        if (_storyProgressText != null) _storyProgressText.gameObject.SetActive(false);
        
        if (trigger.type == "note")
        {
            CompleteGameSystem.Instance.AddCombatLog($"◆ 发现笔记：{trigger.title}");
        }
        else if (trigger.type == "revelation")
        {
            GameManager.Instance.Player.pollution = Mathf.Min(100f, GameManager.Instance.Player.pollution + 10f);
            CompleteGameSystem.Instance.AddCombatLog("【认知冲击】污染+10%");
        }
        else if (trigger.type == "fragment")
        {
            GameManager.Instance.Player.pollution = Mathf.Min(100f, GameManager.Instance.Player.pollution + 5f);
            CompleteGameSystem.Instance.AddCombatLog("【记忆恢复】意识碎片涌入...");
        }
        else if (trigger.type == "finale")
        {
            CompleteGameSystem.Instance.AddCombatLog("【最终真相】一切都清晰了...");
        }
        
        if (!string.IsNullOrEmpty(trigger.storyReward))
        {
            StorySystem.Instance.ApplyStoryReward(trigger.storyReward);
        }
    }

    public void ShowHiddenStoryEvent(StorySystem.HiddenStoryEvent ev)
    {
        if (_storyOvl == null) BuildStoryPanel();
        
        _storyOvl.SetActive(true);
        _storyPanel.SetActive(true);

        string titleTxt = $"<color=#b455ff>◈ 隐藏发现</color> {ev.name}";
        CompleteGameSystem.Instance.ShowStoryEvent(titleTxt, ev.text);

        _storyLastTitle = titleTxt;
        _storyLastBody = ev.text;
        _storyTitle.text = titleTxt;
        _storyTitle.color = new Color(0.71f, 0.33f, 1);
        _storyTypewriterFull = ev.text ?? "";
        _storyTypewriterChar = 0;
        _storyTypewriterDelay = 0f;
        _storyTypewriterText = _storyBody;
        _storyBody.text = "";
        _storyTypewriterActive = _storyTypewriterFull.Length > 0;
        _storyFadeAnimating = true;
        _storyFadeTime = 0f;
        if (_storyCanvasGroup != null) _storyCanvasGroup.alpha = 0f;
        if (_storyPanelCanvasGroup != null) _storyPanelCanvasGroup.alpha = 0f;
        
        if (_storyProgressText != null) _storyProgressText.gameObject.SetActive(false);
        
        ev.reward?.Invoke();
    }

    // ========== 碎片卡片边框辅助方法 ==========

    Image CreateCornerMark(Transform parent, string name, Vector2 anchorMin, Vector2 anchorMax, float offX, float offY, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = anchorMin;
        rt.anchorMax = anchorMax;
        rt.offsetMin = new Vector2(offX, offY);
        rt.offsetMax = new Vector2(offX + (offX >= 0 ? 6 : -6), offY + (offY >= 0 ? 6 : -6));
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    void UpdateFragBorderAnimation()
    {
        if (_fragChoiceOvl == null || !_fragChoiceOvl.activeSelf) return;

        _fragBorderAnimTime += Time.deltaTime;
        float pulse = 0.7f + 0.3f * Mathf.Sin(_fragBorderAnimTime * 2.5f);

        for (int i = 0; i < 3; i++)
        {
            if (_fragBorderTop[i] == null) continue;

            Color frameC = _fragBorderTop[i].color;
            float baseAlpha = Mathf.Clamp01(frameC.a);
            frameC.a = baseAlpha * (0.85f + pulse * 0.15f);
            _fragBorderTop[i].color = frameC;

            if (_fragCornerTL[i] != null)
            {
                Color cc = _fragCornerTL[i].color;
                cc.a = Mathf.Clamp01(cc.a) * (0.85f + pulse * 0.15f);
                _fragCornerTL[i].color = cc;
                _fragCornerTR[i].color = cc;
                _fragCornerBL[i].color = cc;
                _fragCornerBR[i].color = cc;
            }

            if (_fragBorderGlow[i] != null)
            {
                Color glowC = _fragBorderGlow[i].color;
                glowC.a = 0.25f + pulse * 0.15f;
                _fragBorderGlow[i].color = glowC;
            }

            if (_fragInnerLine[i] != null)
            {
                Color ilC = _fragInnerLine[i].color;
                ilC.a = 0.2f + pulse * 0.12f;
                _fragInnerLine[i].color = ilC;
            }

            if (_fragChoiceProgressFillImg[i] != null)
            {
                Color pfC = _fragChoiceProgressFillImg[i].color;
                pfC.a = 0.7f + pulse * 0.3f;
                _fragChoiceProgressFillImg[i].color = pfC;
            }
        }
    }

    void SetFragBorderTypeColor(int index, bool passive)
    {
        if (_fragBorderTop[index] == null) return;

        Color activeFrame = new Color(0.0f, 0.94f, 0.94f, 1f);
        Color passiveFrame = new Color(0.65f, 0.33f, 0.94f, 1f);
        Color glowColor = passive
            ? new Color(0.55f, 0.25f, 0.85f, 0.35f)
            : new Color(0.0f, 0.8f, 0.75f, 0.4f);
        Color innerLineColor = passive
            ? new Color(0.6f, 0.4f, 0.9f, 0.3f)
            : new Color(0.3f, 0.9f, 0.85f, 0.3f);

        Color fc = passive ? passiveFrame : activeFrame;
        _fragBorderTop[index].color = fc;
        _fragCornerTL[index].color = fc;
        _fragCornerTR[index].color = fc;
        _fragCornerBL[index].color = fc;
        _fragCornerBR[index].color = fc;

        if (_fragBorderGlow[index] != null)
            _fragBorderGlow[index].color = glowColor;
        if (_fragInnerLine[index] != null)
            _fragInnerLine[index].color = innerLineColor;
    }

}
