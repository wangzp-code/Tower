using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    // ========== COMBAT ==========
    // Additional combat refs
    Text _cEnemyTitle, _cEnemyDesc, _cPossLabel;
    Image _cPossFill, _cLogPanel;
    RectTransform _cVsRT;
    int _lastCombatLogCount = -1;
    Text _cPTraits, _cETraits;
    Text _cETypeTags, _cPTypeTags;
    Outline _cPCardOutline, _cECardOutline;
    GameObject _cPossessBadge, _cPossessRing;
    Image _cPCardBg;
    // Host traits bar (combat)
    GameObject _cHostBar, _cPTraitBtn;
    Text _cHostName, _cHostTraits;
    // Combat action buttons
    Button _cAtkBtn, _cDefBtn, _cPossBtn, _cFleeBtn, _cUltBtn;
    Text _cUltLabel;
    // Trait info popup
    GameObject _traitInfoOvl;
    Text _traitInfoTitle, _traitInfoHeader;
    CanvasGroup _traitInfoCG, _traitInfoPanelCG;
    GameObject _traitCardContainer;
    // Bottom HUD bar
    GameObject _cBtmHudGo;

    static readonly Dictionary<string, (string label, Color color)> AxesTypeMap = new Dictionary<string, (string, Color)>
    {
        { "hunter", ("猎手", new Color(1f, 0.4f, 0.2f)) },
        { "tank", ("坦克", new Color(0.3f, 0.7f, 0.9f)) },
        { "swift", ("迅捷", new Color(0.3f, 0.9f, 0.3f)) },
        { "toxic", ("毒系", new Color(0.6f, 0.3f, 0.9f)) },
        { "sentinel", ("哨兵", new Color(0.9f, 0.8f, 0.3f)) },
        { "parasite", ("寄生", new Color(0.2f, 0.9f, 0.8f)) },
    };

    static string FormatAxesTags(string[] axes)
    {
        if (axes == null || axes.Length == 0) return "";
        var tags = new List<string>();
        foreach (var axis in axes)
        {
            if (AxesTypeMap.TryGetValue(axis, out var info))
            {
                tags.Add($"<color=#{ColorUtility.ToHtmlStringRGB(info.color)}>{info.label}</color>");
            }
        }
        return string.Join(" · ", tags);
    }

    string[] GetFormAxes(string formId)
    {
        if (formId == "human") return new[] { "parasite" };
        if (GameDataImporter.MonsterDefinitions == null) return null;
        var monster = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == formId);
        if (string.IsNullOrEmpty(monster.id)) return null;
        return monster.axes;
    }

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

        ST(_traitInfoHeader, header);
        ST(_traitInfoTitle, $"{header} ({traits.Length})");

        // Clear existing cards
        if (_traitCardContainer != null)
        {
            for (int i = _traitCardContainer.transform.childCount - 1; i >= 0; i--)
            {
                Destroy(_traitCardContainer.transform.GetChild(i).gameObject);
            }
        }

        // Build one card per trait
        var cardVL = _traitCardContainer.GetComponent<VerticalLayoutGroup>();
        if (cardVL == null) cardVL = _traitCardContainer.AddComponent<VerticalLayoutGroup>();
        cardVL.padding = new RectOffset(0, 0, 0, 0);
        cardVL.spacing = 12;
        cardVL.childAlignment = TextAnchor.UpperCenter;
        cardVL.childForceExpandWidth = true;
        cardVL.childForceExpandHeight = false;

        for (int i = 0; i < traits.Length; i++)
        {
            string trait = traits[i];
            string desc = GetTraitDescription(trait);
            AddTraitCard(_traitCardContainer.transform, trait, desc, i);
        }

        _traitInfoOvl.SetActive(true);
        if (_traitInfoCG != null)
        {
            _traitInfoCG.alpha = 0f;
            StartCoroutine(FadeInTraitInfo());
        }
    }

    System.Collections.IEnumerator FadeInTraitInfo()
    {
        float t = 0f;
        while (t < 0.25f)
        {
            t += Time.deltaTime;
            if (_traitInfoCG != null) _traitInfoCG.alpha = Mathf.Clamp01(t / 0.25f);
            if (_traitInfoPanelCG != null) _traitInfoPanelCG.alpha = Mathf.Clamp01(t / 0.25f);
            yield return null;
        }
        if (_traitInfoCG != null) _traitInfoCG.alpha = 1f;
        if (_traitInfoPanelCG != null) _traitInfoPanelCG.alpha = 1f;
    }

    void AddTraitCard(Transform parent, string traitName, string desc, int index)
    {
        var card = new GameObject($"TraitCard_{index}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        var cardRT = card.GetComponent<RectTransform>();
        cardRT.anchorMin = new Vector2(0, 1);
        cardRT.anchorMax = new Vector2(1, 1);
        cardRT.pivot = new Vector2(0.5f, 1);
        cardRT.offsetMin = new Vector2(0, 0);
        cardRT.offsetMax = new Vector2(0, 0);

        float hueShift = (index * 47) % 360f / 360f;
        Color accentCol = Color.HSVToRGB(hueShift, 0.65f, 0.95f);

        var cardImg = card.GetComponent<Image>();
        cardImg.color = new Color(0.04f, 0.03f, 0.08f, 0.95f);
        cardImg.raycastTarget = false;

        var cardOl = card.GetComponent<Outline>();
        cardOl.effectColor = new Color(accentCol.r, accentCol.g, accentCol.b, 0.4f);
        cardOl.effectDistance = new Vector2(1, 2);

        var cardShadow = card.GetComponent<Shadow>();
        cardShadow.effectColor = new Color(0, 0, 0, 0.5f);
        cardShadow.effectDistance = new Vector2(2, -3);

        var cardLE = card.GetComponent<LayoutElement>();
        cardLE.minHeight = 0;
        cardLE.preferredHeight = 80;
        cardLE.flexibleWidth = 1;

        // Accent left bar (full height, glowing)
        var accentBar = new GameObject("AccentBar", typeof(RectTransform), typeof(Image));
        accentBar.transform.SetParent(card.transform, false);
        var accentRT = accentBar.GetComponent<RectTransform>();
        accentRT.anchorMin = new Vector2(0, 0);
        accentRT.anchorMax = new Vector2(0, 1);
        accentRT.pivot = new Vector2(0, 0.5f);
        accentRT.offsetMin = new Vector2(0, 8);
        accentRT.offsetMax = new Vector2(3, -8);
        var accentImg = accentBar.GetComponent<Image>();
        accentImg.color = accentCol;
        accentImg.raycastTarget = false;

        // Icon badge on left (small circle with glow)
        var badgeBg = new GameObject("BadgeBg", typeof(RectTransform), typeof(Image), typeof(Outline));
        badgeBg.transform.SetParent(card.transform, false);
        var badgeRT = badgeBg.GetComponent<RectTransform>();
        badgeRT.anchorMin = new Vector2(0, 0.5f);
        badgeRT.anchorMax = new Vector2(0, 0.5f);
        badgeRT.pivot = new Vector2(0, 0.5f);
        badgeRT.offsetMin = new Vector2(12, -14);
        badgeRT.offsetMax = new Vector2(36, 14);
        var badgeImg = badgeBg.GetComponent<Image>();
        badgeImg.sprite = CreateCircleSprite(32);
        badgeImg.color = new Color(accentCol.r * 0.4f, accentCol.g * 0.4f, accentCol.b * 0.4f, 0.9f);
        badgeImg.raycastTarget = false;
        var badgeOl = badgeBg.GetComponent<Outline>();
        badgeOl.effectColor = new Color(accentCol.r, accentCol.g, accentCol.b, 0.7f);
        badgeOl.effectDistance = new Vector2(1, 1);

        // Badge text (first letter or symbol)
        string badgeSym = traitName.Length > 0 ? traitName[0].ToString() : "?";
        var badgeTxtGo = new GameObject("BadgeTxt", typeof(RectTransform), typeof(Text));
        badgeTxtGo.transform.SetParent(badgeBg.transform, false);
        var badgeTxtRT = badgeTxtGo.GetComponent<RectTransform>();
        badgeTxtRT.anchorMin = Vector2.zero;
        badgeTxtRT.anchorMax = Vector2.one;
        badgeTxtRT.offsetMin = Vector2.zero;
        badgeTxtRT.offsetMax = Vector2.zero;
        var badgeTxt = badgeTxtGo.GetComponent<Text>();
        badgeTxt.font = F();
        badgeTxt.text = badgeSym;
        badgeTxt.fontSize = 14;
        badgeTxt.fontStyle = FontStyle.Bold;
        badgeTxt.color = accentCol;
        badgeTxt.alignment = TextAnchor.MiddleCenter;
        badgeTxt.raycastTarget = false;

        // Content area to the right of badge
        var content = new GameObject("Content", typeof(RectTransform));
        content.transform.SetParent(card.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = Vector2.zero;
        contentRT.anchorMax = Vector2.one;
        contentRT.offsetMin = new Vector2(44, 6);
        contentRT.offsetMax = new Vector2(-14, -6);

        var vl = content.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(4, 4, 4, 4);
        vl.spacing = 3;
        vl.childAlignment = TextAnchor.UpperLeft;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;

        // Trait name
        var nameGo = new GameObject("TraitName", typeof(RectTransform), typeof(Text), typeof(Outline));
        nameGo.transform.SetParent(vl.transform, false);
        var nameText = nameGo.GetComponent<Text>();
        nameText.font = F();
        nameText.text = traitName;
        nameText.fontSize = 17;
        nameText.fontStyle = FontStyle.Bold;
        nameText.color = accentCol;
        nameText.alignment = TextAnchor.MiddleLeft;
        nameText.horizontalOverflow = HorizontalWrapMode.Wrap;
        nameText.raycastTarget = false;
        var nameOL = nameGo.GetComponent<Outline>();
        nameOL.effectColor = new Color(0, 0, 0, 0.6f);
        nameOL.effectDistance = new Vector2(1, 1);
        var nameLE = nameGo.AddComponent<LayoutElement>();
        nameLE.preferredHeight = 22;

        // Description
        var descGo = new GameObject("TraitDesc", typeof(RectTransform), typeof(Text));
        descGo.transform.SetParent(vl.transform, false);
        var descText = descGo.GetComponent<Text>();
        descText.font = F();
        descText.text = desc;
        descText.fontSize = 13;
        descText.color = new Color(0.88f, 0.9f, 0.95f);
        descText.alignment = TextAnchor.UpperLeft;
        descText.horizontalOverflow = HorizontalWrapMode.Wrap;
        descText.verticalOverflow = VerticalWrapMode.Overflow;
        descText.lineSpacing = 1.15f;
        descText.raycastTarget = false;
        var descLE = descGo.AddComponent<LayoutElement>();
        descLE.minHeight = 0;
        descLE.preferredHeight = 40;
        descLE.flexibleHeight = 1;
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

        // ParasiteBackground Shader 动态生物膜背景 (完整替换静态网格)
        var bgShaderGo = new GameObject("BgShaderOverlay", typeof(RectTransform), typeof(Image));
        bgShaderGo.transform.SetParent(rt, false);
        var bgShaderRT = bgShaderGo.GetComponent<RectTransform>();
        bgShaderRT.anchorMin = Vector2.zero; bgShaderRT.anchorMax = Vector2.one;
        bgShaderRT.offsetMin = Vector2.zero; bgShaderRT.offsetMax = Vector2.zero;
        _cBgShaderOverlay = bgShaderGo.GetComponent<Image>();
        _cBgShaderOverlay.sprite = WhiteSprite;
        _cBgShaderOverlay.color = new Color(1, 1, 1, 1f);  // Shader 输出完全不透明
        _cBgShaderOverlay.raycastTarget = false;
        // Material 延迟到 SyncCombat 里按楼层主题创建

        // 渐变覆盖层 (调低 alpha, 让 Shader 动态背景透出)
        var gradientGo = new GameObject("Gradient", typeof(RectTransform), typeof(Image));
        gradientGo.transform.SetParent(rt, false);
        var gradientRT = gradientGo.GetComponent<RectTransform>();
        gradientRT.anchorMin = Vector2.zero; gradientRT.anchorMax = Vector2.one;
        gradientRT.offsetMin = Vector2.zero; gradientRT.offsetMax = Vector2.zero;
        var gradientImg = gradientGo.GetComponent<Image>();
        gradientImg.color = new Color(0.05f, 0.03f, 0.10f, 0.35f);  // 更淡, 不遮 Shader 细节

        // 添加角落装饰光效 (增强氛围)
        AddCornerLight(rt, new Vector2(0, 1), new Color(0.12f, 0.22f, 0.35f), 0.1f);
        AddCornerLight(rt, new Vector2(1, 1), new Color(0.22f, 0.10f, 0.32f), 0.08f);

        // 污染渐晕叠层 — 径向渐变（边缘紫色、中心透明）
        var pollVigGo = new GameObject("PollVignette", typeof(RectTransform), typeof(Image));
        pollVigGo.transform.SetParent(rt, false);
        var pollVigRT = pollVigGo.GetComponent<RectTransform>();
        pollVigRT.anchorMin = Vector2.zero; pollVigRT.anchorMax = Vector2.one;
        pollVigRT.offsetMin = Vector2.zero; pollVigRT.offsetMax = Vector2.zero;
        _cPollVignette = pollVigGo.GetComponent<Image>();
        _cPollVignette.sprite = CreateRadialGradientSprite(256, 0.35f);
        _cPollVignette.color = Color.clear;
        _cPollVignette.raycastTarget = false;

        // 污染脉冲层 — 径向渐变边缘脉冲
        var pollPulseGo = new GameObject("PollPulse", typeof(RectTransform), typeof(Image));
        pollPulseGo.transform.SetParent(rt, false);
        var pollPulseRT = pollPulseGo.GetComponent<RectTransform>();
        pollPulseRT.anchorMin = Vector2.zero; pollPulseRT.anchorMax = Vector2.one;
        pollPulseRT.offsetMin = Vector2.zero; pollPulseRT.offsetMax = Vector2.zero;
        _cPollPulse = pollPulseGo.GetComponent<Image>();
        _cPollPulse.sprite = CreateRadialGradientSprite(256, 0.3f);
        _cPollPulse.color = Color.clear;
        _cPollPulse.raycastTarget = false;

        // 污染干扰线层 — 高污染时的崩坏效果
        var pollScanGo = new GameObject("PollScanlines", typeof(RectTransform), typeof(Image));
        pollScanGo.transform.SetParent(rt, false);
        var pollScanRT = pollScanGo.GetComponent<RectTransform>();
        pollScanRT.anchorMin = Vector2.zero; pollScanRT.anchorMax = Vector2.one;
        pollScanRT.offsetMin = Vector2.zero; pollScanRT.offsetMax = Vector2.zero;
        _cPollScanlines = pollScanGo.GetComponent<Image>();
        _cPollScanlines.sprite = CreateScanlineSprite();
        _cPollScanlines.color = Color.clear;
        _cPollScanlines.raycastTarget = false;

        // === Top: Enemy name + description (top 6%) ===
        var topGo = new GameObject("Top", typeof(RectTransform), typeof(Image));
        topGo.transform.SetParent(rt, false);
        topGo.GetComponent<Image>().color = new Color(0.10f,0.06f,0.16f,0.95f);
        var topRT = topGo.GetComponent<RectTransform>();
        topRT.anchorMin = new Vector2(0, 0.90f);
        topRT.anchorMax = new Vector2(1, 1);
        topRT.offsetMin = Vector2.zero; topRT.offsetMax = Vector2.zero;

        var topSep = new GameObject("TopSep", typeof(RectTransform), typeof(Image));
        topSep.transform.SetParent(topRT, false);
        var topSepRT = topSep.GetComponent<RectTransform>();
        topSepRT.anchorMin = new Vector2(0.05f, 0); topSepRT.anchorMax = new Vector2(0.95f, 0);
        topSepRT.offsetMin = Vector2.zero; topSepRT.offsetMax = new Vector2(0, 2);
        topSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.18f);

        _cEnemyTitle = TxtAnchored(topRT, "", 26, Bright, new Vector2(0,0.42f), new Vector2(1,1), 12, 12);
        _cEnemyTitle.alignment = TextAnchor.MiddleCenter;
        _cEnemyTitle.fontStyle = FontStyle.Bold;
        _cEnemyDesc = TxtAnchored(topRT, "", 14, Dim, new Vector2(0,0), new Vector2(1,0.4f), 15, 15);
        _cEnemyDesc.alignment = TextAnchor.MiddleCenter;

        // === VS Cards area (middle 34%) ===
        var cardsGo = new GameObject("Cards", typeof(RectTransform), typeof(Image));
        cardsGo.transform.SetParent(rt, false);
        cardsGo.GetComponent<Image>().color = new Color(0.07f,0.06f,0.12f,0.6f);
        var cardsRT = cardsGo.GetComponent<RectTransform>();
        cardsRT.anchorMin = new Vector2(0, 0.54f);
        cardsRT.anchorMax = new Vector2(1, 0.90f);
        cardsRT.offsetMin = new Vector2(8, 4); cardsRT.offsetMax = new Vector2(-8, -4);

        // -- Player card (left 48%) --
        var pcGo = new GameObject("PCard", typeof(RectTransform), typeof(Image), typeof(Outline));
        pcGo.transform.SetParent(cardsRT, false);
        _cPCardBg = pcGo.GetComponent<Image>();
        _cPCardBg.color = new Color(0.06f, 0.16f, 0.18f, 0.95f);
        
        var pcOutline = pcGo.GetComponent<Outline>();
        pcOutline.effectColor = new Color(0f, 0.9f, 0.75f, 0.7f);
        pcOutline.effectDistance = new Vector2(2, 2);
        _cPCardOutline = pcOutline;
        
        var pcRT = pcGo.GetComponent<RectTransform>();
        pcRT.anchorMin = new Vector2(0, 0); pcRT.anchorMax = new Vector2(0.46f, 1);
        pcRT.offsetMin = new Vector2(8, 8); pcRT.offsetMax = new Vector2(-4, -8);
        
        var possessBarGo = new GameObject("PossessBar", typeof(RectTransform), typeof(Image));
        possessBarGo.transform.SetParent(pcRT, false);
        var possessBarRT = possessBarGo.GetComponent<RectTransform>();
        possessBarRT.anchorMin = new Vector2(0, 0.88f);
        possessBarRT.anchorMax = new Vector2(1, 1);
        possessBarRT.offsetMin = Vector2.zero;
        possessBarRT.offsetMax = Vector2.zero;
        var possessBarImg = possessBarGo.GetComponent<Image>();
        possessBarImg.color = new Color(0f, 0.4f, 0.35f, 0.9f);
        possessBarImg.raycastTarget = false;
        _cPossessBadge = possessBarGo;
        
        var possessBarTxt = TxtAnchored(possessBarRT, "🧬 寄主", 11, new Color(0f, 1f, 0.816f), new Vector2(0,0), new Vector2(1,1), 6, 0);
        possessBarTxt.alignment = TextAnchor.MiddleLeft;
        possessBarTxt.fontStyle = FontStyle.Bold;
        possessBarTxt.raycastTarget = false;
        
        _cPIcon = ImgAnchored(pcRT, new Vector2(0.15f,0.5f), new Vector2(0.85f,0.88f));
        
        _cPossessRing = new GameObject("PossessRing", typeof(RectTransform), typeof(Image));
        _cPossessRing.transform.SetParent(pcRT, false);
        var possessRingRT = _cPossessRing.GetComponent<RectTransform>();
        possessRingRT.anchorMin = new Vector2(0.1f, 0.48f);
        possessRingRT.anchorMax = new Vector2(0.9f, 0.9f);
        possessRingRT.offsetMin = Vector2.zero;
        possessRingRT.offsetMax = Vector2.zero;
        var possessRingImg = _cPossessRing.GetComponent<Image>();
        possessRingImg.color = new Color(0f, 1f, 0.816f, 0.4f);
        possessRingImg.sprite = CreateRadialGradientSprite(128, 0.75f);
        possessRingImg.raycastTarget = false;
        _cPossessRing.SetActive(false);
        
        _cPName = TxtAnchored(pcRT, "", 15, Bright, new Vector2(0,0.38f), new Vector2(1,0.5f), 4, 4);
        _cPName.alignment = TextAnchor.MiddleCenter;
        _cPHpFill = BarAnchored(pcRT, new Color(0,0.85f,0.45f), new Vector2(0.08f,0.28f), new Vector2(0.92f,0.38f), out _cPHpTxt);
        _cPStat = TxtAnchored(pcRT, "", 12, Bright, new Vector2(0,0.18f), new Vector2(1,0.28f), 4, 4);
        _cPStat.alignment = TextAnchor.MiddleCenter;
        _cPTypeTags = TxtAnchored(pcRT, "", 10, Bright, new Vector2(0,0.12f), new Vector2(1,0.18f), 4, 4);
        _cPTypeTags.alignment = TextAnchor.MiddleCenter;
        _cPTypeTags.supportRichText = true;
        
        var pTraitBtnGo = new GameObject("PTraitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        pTraitBtnGo.transform.SetParent(pcRT, false);
        var pTraitBtnRT = pTraitBtnGo.GetComponent<RectTransform>();
        pTraitBtnRT.anchorMin = new Vector2(0, 0);
        pTraitBtnRT.anchorMax = new Vector2(1, 0.12f);
        pTraitBtnRT.offsetMin = new Vector2(4, 2);
        pTraitBtnRT.offsetMax = new Vector2(-4, -2);
        pTraitBtnGo.GetComponent<Image>().color = new Color(0.1f, 0.2f, 0.22f, 0.85f);
        var pTraitOL = pTraitBtnGo.GetComponent<Outline>();
        pTraitOL.effectColor = new Color(0f, 1f, 0.816f, 0.35f);
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
        var vsGo = new GameObject("VS", typeof(RectTransform), typeof(Image), typeof(Outline));
        vsGo.transform.SetParent(cardsRT, false);
        var vsRT = vsGo.GetComponent<RectTransform>();
        _cVsRT = vsRT;
        vsRT.anchorMin = new Vector2(0.47f, 0.45f); vsRT.anchorMax = new Vector2(0.53f, 0.55f);
        vsRT.offsetMin = Vector2.zero; vsRT.offsetMax = Vector2.zero;
        
        // VS背景
        var vsImg = vsGo.GetComponent<Image>();
        vsImg.color = new Color(0.12f, 0.06f, 0.18f, 0.75f);
        
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
        vsTxtShadow.effectColor = Mask80;
        vsTxtShadow.effectDistance = new Vector2(1, -1);

        // -- Enemy card (right 48%) --
        var ecGo = new GameObject("ECard", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        ecGo.transform.SetParent(cardsRT, false);
        ecGo.GetComponent<Image>().color = new Color(0.14f, 0.05f, 0.10f, 0.95f);
        
        var ecOutline = ecGo.GetComponent<Outline>();
        ecOutline.effectColor = new Color(1f, 0.3f, 0.5f, 0.8f);
        ecOutline.effectDistance = new Vector2(2, 2);
        _cECardOutline = ecOutline;
        
        var ecRT = ecGo.GetComponent<RectTransform>();
        ecRT.anchorMin = new Vector2(0.54f, 0); ecRT.anchorMax = new Vector2(1, 1);
        ecRT.offsetMin = new Vector2(4, 8); ecRT.offsetMax = new Vector2(-8, -8);

        TxtAnchored(ecRT, "👹 怪物", 11, new Color(0.95f, 0.25f, 0.45f), new Vector2(0,0.88f), new Vector2(1,1), 6, 0);
        _cEIcon = ImgAnchored(ecRT, new Vector2(0.15f,0.5f), new Vector2(0.85f,0.88f));
        _cEName = TxtAnchored(ecRT, "", 15, Gold, new Vector2(0,0.38f), new Vector2(1,0.5f), 4, 4);
        _cEName.alignment = TextAnchor.MiddleCenter;
        _cEHpFill = BarAnchored(ecRT, new Color(0.9f,0.15f,0.15f), new Vector2(0.08f,0.28f), new Vector2(0.92f,0.38f), out _cEHpTxt, enemy: true);
        _cEStat = TxtAnchored(ecRT, "", 12, new Color(1,0.6f,0.6f), new Vector2(0,0.18f), new Vector2(1,0.28f), 4, 4);
        _cEStat.alignment = TextAnchor.MiddleCenter;
        _cETypeTags = TxtAnchored(ecRT, "", 10, Bright, new Vector2(0,0.12f), new Vector2(1,0.18f), 4, 4);
        _cETypeTags.alignment = TextAnchor.MiddleCenter;
        _cETypeTags.supportRichText = true;
        
        var traitBtnGo = new GameObject("TraitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
        traitBtnGo.transform.SetParent(ecRT, false);
        var traitBtnRT = traitBtnGo.GetComponent<RectTransform>();
        traitBtnRT.anchorMin = new Vector2(0, 0);
        traitBtnRT.anchorMax = new Vector2(1, 0.12f);
        traitBtnRT.offsetMin = new Vector2(4, 2);
        traitBtnRT.offsetMax = new Vector2(-4, -2);
        traitBtnGo.GetComponent<Image>().color = new Color(0.25f, 0.15f, 0.32f, 0.85f);
        var traitOL = traitBtnGo.GetComponent<Outline>();
        traitOL.effectColor = new Color(0.95f, 0.3f, 0.6f, 0.35f);
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

        // === Possess rate bar (6%) ===
        // Separator: Cards → Possess
        var cardsSep = new GameObject("CardsSep", typeof(RectTransform), typeof(Image));
        cardsSep.transform.SetParent(rt, false);
        var cardsSepRT = cardsSep.GetComponent<RectTransform>();
        cardsSepRT.anchorMin = new Vector2(0.03f, 0.489f); cardsSepRT.anchorMax = new Vector2(0.97f, 0.491f);
        cardsSepRT.offsetMin = Vector2.zero; cardsSepRT.offsetMax = Vector2.zero;
        cardsSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.12f);

        var possGo = new GameObject("PossBar", typeof(RectTransform), typeof(Image), typeof(Outline));
        possGo.transform.SetParent(rt, false);
        possGo.GetComponent<Image>().color = new Color(0.08f,0.06f,0.15f,0.92f);
        possGo.GetComponent<Outline>().effectColor = new Color(0.5f,0.25f,0.8f,0.2f);
        possGo.GetComponent<Outline>().effectDistance = new Vector2(1,1);
        var possRT = possGo.GetComponent<RectTransform>();
        possRT.anchorMin = new Vector2(0, 0.485f);
        possRT.anchorMax = new Vector2(1, 0.545f);
        possRT.offsetMin = new Vector2(12, 3); possRT.offsetMax = new Vector2(-12, -3);

        _cRate = TxtAnchored(possRT, "", 22, Purp, new Vector2(0,0), new Vector2(0.14f,1), 4, 0);
        _cRate.alignment = TextAnchor.MiddleCenter;
        _cPossLabel = TxtAnchored(possRT, "附身成功率", 14, Dim, new Vector2(0.14f,0), new Vector2(0.32f,1), 0, 4);
        _cPossLabel.alignment = TextAnchor.MiddleLeft;
        
        var possBgGo = new GameObject("PossBarBg", typeof(RectTransform), typeof(Image));
        possBgGo.transform.SetParent(possRT, false);
        var possBgRT = possBgGo.GetComponent<RectTransform>();
        possBgRT.anchorMin = new Vector2(0.38f, 0.2f);
        possBgRT.anchorMax = new Vector2(0.92f, 0.65f);
        possBgRT.offsetMin = Vector2.zero; possBgRT.offsetMax = Vector2.zero;
        possBgGo.GetComponent<Image>().color = ParasiteTowerColorScheme.DarkGray;
        
        var possFillGo = new GameObject("PossFill", typeof(RectTransform), typeof(Image));
        possFillGo.transform.SetParent(possBgRT, false);
        var possFillRT = possFillGo.GetComponent<RectTransform>();
        possFillRT.anchorMin = new Vector2(0, 0);
        possFillRT.anchorMax = new Vector2(1, 1);
        possFillRT.offsetMin = new Vector2(2, 2);
        possFillRT.offsetMax = new Vector2(-2, -2);
        _cPossFill = possFillGo.GetComponent<Image>();
        _cPossFill.color = new Color(0.7f, 0.3f, 1f);
        _cPossFill.type = Image.Type.Filled;
        _cPossFill.fillMethod = Image.FillMethod.Horizontal;
        _cPossFill.fillOrigin = (int)Image.OriginHorizontal.Left;
        _cPossFill.fillAmount = 0f;
        _cPossFill.sprite = CreateSimpleSprite(Color.white, 10, 10);

        // === Battle log - scrollable, above skill bar ===
        var logGo = new GameObject("Log", typeof(RectTransform), typeof(Image), typeof(Outline));
        logGo.transform.SetParent(rt, false);
        _cLogPanel = logGo.GetComponent<Image>();
        _cLogPanel.color = new Color(0.05f, 0.04f, 0.09f, 0.85f);
        var logOL = logGo.GetComponent<Outline>();
        logOL.effectColor = new Color(0.85f, 0.65f, 0.2f, 0.22f);
        logOL.effectDistance = new Vector2(1, 1);
        var logRT = logGo.GetComponent<RectTransform>();
        logRT.anchorMin = new Vector2(0, 0.34f);
        logRT.anchorMax = new Vector2(1, 0.485f);
        logRT.offsetMin = new Vector2(8, 4); logRT.offsetMax = new Vector2(-8, -4);

        var logAccent = new GameObject("LogAccent", typeof(RectTransform), typeof(Image));
        logAccent.transform.SetParent(logRT, false);
        var laRT = logAccent.GetComponent<RectTransform>();
        laRT.anchorMin = Vector2.zero; laRT.anchorMax = new Vector2(0, 1);
        laRT.offsetMin = Vector2.zero; laRT.offsetMax = new Vector2(4, 0);
        logAccent.GetComponent<Image>().color = new Color(0.85f, 0.65f, 0.2f, 0.4f);

        var logHeader = TxtAnchored(logRT, "◈ 战斗记录", 10, new Color(0.85f, 0.65f, 0.2f, 0.75f),
            new Vector2(0, 0.92f), new Vector2(1, 1), 8, 6);
        logHeader.alignment = TextAnchor.MiddleLeft;
        logHeader.fontStyle = FontStyle.Bold;

        var scrollGo = new GameObject("LogScroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(logRT, false);
        var scrollRT = scrollGo.GetComponent<RectTransform>();
        scrollRT.anchorMin = new Vector2(0, 0);
        scrollRT.anchorMax = new Vector2(1, 0.92f);
        scrollRT.offsetMin = new Vector2(4, 4);
        scrollRT.offsetMax = new Vector2(-4, -4);
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.vertical = true;
        scroll.movementType = ScrollRect.MovementType.Clamped;
        scroll.scrollSensitivity = 28f;
        scroll.inertia = true;
        scroll.decelerationRate = 0.12f;
        _cLogScroll = scroll;

        var vpGo = new GameObject("VP", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(scrollGo.transform, false);
        Stretch(vpGo);
        vpGo.GetComponent<Image>().color = new Color(1f, 1f, 1f, 0.004f);
        vpGo.GetComponent<Mask>().showMaskGraphic = false;
        scroll.viewport = vpGo.GetComponent<RectTransform>();

        var contentGo = new GameObject("Content", typeof(RectTransform));
        contentGo.transform.SetParent(vpGo.transform, false);
        var contentRT = contentGo.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0, 1);
        contentRT.anchorMax = new Vector2(1, 1);
        contentRT.pivot = new Vector2(0.5f, 1);
        contentRT.offsetMin = new Vector2(6, 0);
        contentRT.offsetMax = new Vector2(-6, 0);
        scroll.content = contentRT;
        _cLogContentRT = contentRT;

        var textGO = new GameObject("Text", typeof(RectTransform), typeof(Text), typeof(Shadow));
        textGO.transform.SetParent(contentGo.transform, false);
        var textRT = textGO.GetComponent<RectTransform>();
        textRT.anchorMin = new Vector2(0, 1);
        textRT.anchorMax = new Vector2(1, 1);
        textRT.pivot = new Vector2(0, 1);
        textRT.offsetMin = Vector2.zero;
        textRT.offsetMax = Vector2.zero;

        _cMsg = textGO.GetComponent<Text>();
        _cMsg.font = F(); _cMsg.fontSize = 14; _cMsg.color = new Color(0.8f, 0.75f, 0.9f);
        _cMsg.alignment = TextAnchor.UpperLeft;
        _cMsg.fontStyle = FontStyle.Normal;
        _cMsg.lineSpacing = 1.5f;
        _cMsg.raycastTarget = false;
        _cMsg.verticalOverflow = VerticalWrapMode.Overflow;
        _cMsg.horizontalOverflow = HorizontalWrapMode.Wrap;
        var logShadow = textGO.GetComponent<Shadow>();
        logShadow.effectColor = new Color(0, 0, 0, 0.45f);
        logShadow.effectDistance = new Vector2(1, -1);

        // === Bottom HUD bar (3%) ===
        _cBtmHudGo = new GameObject("BtmHud", typeof(RectTransform), typeof(Image));
        _cBtmHudGo.transform.SetParent(rt, false);
        _cBtmHudGo.GetComponent<Image>().color = new Color(0.05f,0.05f,0.08f,0.92f);
        var hudRT = _cBtmHudGo.GetComponent<RectTransform>();
        hudRT.anchorMin = new Vector2(0, 0.28f);
        hudRT.anchorMax = new Vector2(1, 0.31f);
        hudRT.offsetMin = Vector2.zero; hudRT.offsetMax = Vector2.zero;

        var hudSepLine = new GameObject("HudSep", typeof(RectTransform), typeof(Image));
        hudSepLine.transform.SetParent(_cBtmHudGo.transform, false);
        var hslRT = hudSepLine.GetComponent<RectTransform>();
        hslRT.anchorMin = new Vector2(0.03f, 1); hslRT.anchorMax = new Vector2(0.97f, 1);
        hslRT.offsetMin = new Vector2(0, -1); hslRT.offsetMax = Vector2.zero;
        hudSepLine.GetComponent<Image>().color = new Color(0f, 1f, 0.82f, 0.08f);

        _cBtmHud = TxtAnchored(hudRT, "", 10, Bright, new Vector2(0,0), new Vector2(1,1), 10, 10);
        _cBtmHud.alignment = TextAnchor.MiddleLeft;

        // === Active skill bar (3%) ===
        var skillBar = new GameObject("SkillBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        skillBar.transform.SetParent(rt, false);
        var sbRT = skillBar.GetComponent<RectTransform>();
        sbRT.anchorMin = new Vector2(0, 0.31f);
        sbRT.anchorMax = new Vector2(1, 0.34f);
        sbRT.offsetMin = new Vector2(6, 0);
        sbRT.offsetMax = new Vector2(-6, 0);
        var sbHL = skillBar.GetComponent<HorizontalLayoutGroup>();
        sbHL.spacing = 4; sbHL.childAlignment = TextAnchor.MiddleCenter;
        sbHL.childForceExpandWidth = true; sbHL.childForceExpandHeight = true;
        _skillBarRow = skillBar.transform;
        for (int si = 0; si < 3; si++)
        {
            int idx = si;
            var sb = BtnGo(skillBar.transform, "", 10, Cyan, 0);
            var sbBtnRT = sb.GetComponent<RectTransform>();
            sbBtnRT.sizeDelta = new Vector2(0, 0);
            sb.GetComponent<Image>().color = new Color(0.06f, 0.10f, 0.14f, 0.92f);
            sb.AddComponent<Outline>().effectColor = new Color(0f, 1f, 0.816f, 0.3f);
            _skillBarBtns[si] = sb.GetComponent<Button>();
            _skillBarBtns[si].onClick.AddListener(() => CompleteGameSystem.Instance?.UseActiveSkill(idx));
            var sbVL = AddVL(sb, 1, 1);
            _skillBarLabels[si] = Txt(sbVL, "", 11, Gold, 14).GetComponent<Text>();
            _skillBarLabels[si].alignment = TextAnchor.MiddleCenter;
            _skillBarLabels[si].fontStyle = FontStyle.Bold;
            _skillBarUses[si] = Txt(sbVL, "", 9, Cyan, 12).GetComponent<Text>();
            _skillBarUses[si].alignment = TextAnchor.MiddleCenter;
            sb.SetActive(false);
        }

        // Corruption skill quick-access button
        {
            var csb = BtnGo(skillBar.transform, "☢ 污染技能", 10, new Color(0.85f, 0.55f, 0.2f), 0);
            var csbRT = csb.GetComponent<RectTransform>();
            csbRT.sizeDelta = new Vector2(0, 0);
            csb.GetComponent<Image>().color = new Color(0.10f, 0.07f, 0.05f, 0.92f);
            csb.AddComponent<Outline>().effectColor = new Color(0.85f, 0.45f, 0.15f, 0.3f);
            csb.GetComponent<Button>().onClick.AddListener(() => ShowCorruptionSkillSelector());
        }

        // === Action area (22%) ===
        // Separator: HUD → Actions
        var combatActSep = new GameObject("CombatActSep", typeof(RectTransform), typeof(Image));
        combatActSep.transform.SetParent(rt, false);
        var combatActSepRT = combatActSep.GetComponent<RectTransform>();
        combatActSepRT.anchorMin = new Vector2(0.03f, 0.298f); combatActSepRT.anchorMax = new Vector2(0.97f, 0.30f);
        combatActSepRT.offsetMin = Vector2.zero; combatActSepRT.offsetMax = Vector2.zero;
        combatActSep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.08f);

        var actGo = new GameObject("Actions", typeof(RectTransform), typeof(Image), typeof(Outline));
        actGo.transform.SetParent(rt, false);
        actGo.GetComponent<Image>().color = new Color(0.05f,0.04f,0.09f,0.96f);
        actGo.GetComponent<Image>().raycastTarget = false;
        var actOL = actGo.GetComponent<Outline>();
        actOL.effectColor = new Color(0f, 1f, 0.816f, 0.12f);
        actOL.effectDistance = new Vector2(1, 1);
        var actRT = actGo.GetComponent<RectTransform>();
        actRT.anchorMin = new Vector2(0, 0.08f);
        actRT.anchorMax = new Vector2(1, 0.30f);
        actRT.offsetMin = Vector2.zero;
        actRT.offsetMax = Vector2.zero;

        // 技能特性描述弹窗
        _traitInfoOvl = new GameObject("TraitInfoOvl", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        _traitInfoOvl.transform.SetParent(_root, false);
        var tiBGRoot = _traitInfoOvl.GetComponent<RectTransform>();
        tiBGRoot.anchorMin = Vector2.zero; tiBGRoot.anchorMax = Vector2.one;
        tiBGRoot.offsetMin = Vector2.zero; tiBGRoot.offsetMax = Vector2.zero;
        var tiBGImg = _traitInfoOvl.GetComponent<Image>();
        tiBGImg.color = new Color(0.01f, 0.01f, 0.02f, 0.92f);
        tiBGImg.raycastTarget = true;
        var tiCG = _traitInfoOvl.GetComponent<CanvasGroup>();
        tiCG.alpha = 0f;
        tiCG.blocksRaycasts = true;
        _traitInfoCG = tiCG;
        _traitInfoOvl.SetActive(false);

        // Panel container
        var tiPanel = new GameObject("TraitPanel", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Shadow));
        tiPanel.transform.SetParent(_root, false);
        var tiPRT = tiPanel.GetComponent<RectTransform>();
        tiPRT.anchorMin = new Vector2(0.06f, 0.08f);
        tiPRT.anchorMax = new Vector2(0.94f, 0.92f);
        tiPRT.offsetMin = Vector2.zero; tiPRT.offsetMax = Vector2.zero;
        var tiPanelImg = tiPanel.GetComponent<Image>();
        tiPanelImg.color = new Color(0.025f, 0.02f, 0.06f, 0.985f);
        tiPanelImg.raycastTarget = false;
        var tiOl = tiPanel.GetComponent<Outline>();
        tiOl.effectColor = new Color(0f, 1f, 0.816f, 0.55f);
        tiOl.effectDistance = new Vector2(3, 4);
        var tiShadow = tiPanel.GetComponent<Shadow>();
        tiShadow.effectColor = new Color(0, 0, 0, 0.8f);
        tiShadow.effectDistance = new Vector2(5, -5);

        // Subtle hex pattern
        var tiHexBg = new GameObject("HexPattern", typeof(RectTransform), typeof(Image));
        tiHexBg.transform.SetParent(tiPanel.transform, false);
        var tiHexRT = tiHexBg.GetComponent<RectTransform>();
        tiHexRT.anchorMin = Vector2.zero; tiHexRT.anchorMax = Vector2.one;
        tiHexRT.offsetMin = Vector2.zero; tiHexRT.offsetMax = Vector2.zero;
        var tiHexImg = tiHexBg.GetComponent<Image>();
        tiHexImg.sprite = CreateHexPatternSprite(128);
        tiHexImg.color = new Color(0.25f, 0.85f, 0.7f, 0.035f);
        tiHexImg.raycastTarget = false;

        // Soft radial glow
        var tiGlow = new GameObject("Glow", typeof(RectTransform), typeof(Image));
        tiGlow.transform.SetParent(tiPanel.transform, false);
        var tiGlowRT = tiGlow.GetComponent<RectTransform>();
        tiGlowRT.anchorMin = Vector2.zero; tiGlowRT.anchorMax = Vector2.one;
        tiGlowRT.offsetMin = Vector2.zero; tiGlowRT.offsetMax = Vector2.zero;
        var tiGlowImg = tiGlow.GetComponent<Image>();
        tiGlowImg.sprite = CreateRadialGradientSprite(256, 0.35f);
        tiGlowImg.color = new Color(0.1f, 0.6f, 0.5f, 0.08f);
        tiGlowImg.raycastTarget = false;

        // Top accent gradient strip
        var tiTopGrad = new GameObject("TopGrad", typeof(RectTransform), typeof(Image));
        tiTopGrad.transform.SetParent(tiPanel.transform, false);
        var tiTopGradRT = tiTopGrad.GetComponent<RectTransform>();
        tiTopGradRT.anchorMin = new Vector2(0, 1);
        tiTopGradRT.anchorMax = new Vector2(1, 1);
        tiTopGradRT.pivot = new Vector2(0.5f, 1);
        tiTopGradRT.offsetMin = new Vector2(8, -3);
        tiTopGradRT.offsetMax = new Vector2(-8, -3);
        var tiTopGradImg = tiTopGrad.GetComponent<Image>();
        tiTopGradImg.sprite = CreateRadialGradientSprite(128, 0.05f);
        tiTopGradImg.color = new Color(0f, 1f, 0.816f, 0.18f);
        tiTopGradImg.raycastTarget = false;

        // Inner border
        var tiInner = new GameObject("InnerBorder", typeof(RectTransform), typeof(Image), typeof(Outline));
        tiInner.transform.SetParent(tiPanel.transform, false);
        var tiInnerRT = tiInner.GetComponent<RectTransform>();
        tiInnerRT.anchorMin = Vector2.zero; tiInnerRT.anchorMax = Vector2.one;
        tiInnerRT.offsetMin = new Vector2(3, 3); tiInnerRT.offsetMax = new Vector2(-3, -3);
        tiInner.GetComponent<Image>().color = new Color(0, 0, 0, 0);
        tiInner.GetComponent<Image>().raycastTarget = false;
        var tiInnerOl = tiInner.GetComponent<Outline>();
        tiInnerOl.effectColor = new Color(0f, 1f, 0.816f, 0.18f);
        tiInnerOl.effectDistance = new Vector2(1, 1);

        var tiPanelCG = tiPanel.AddComponent<CanvasGroup>();
        tiPanelCG.alpha = 0f;
        tiPanelCG.blocksRaycasts = false;
        _traitInfoPanelCG = tiPanelCG;

        var tiContent = new GameObject("Content", typeof(RectTransform));
        tiContent.transform.SetParent(tiPanel.transform, false);
        var tiContentRT = tiContent.GetComponent<RectTransform>();
        tiContentRT.anchorMin = Vector2.zero; tiContentRT.anchorMax = Vector2.one;
        tiContentRT.offsetMin = new Vector2(0, 0); tiContentRT.offsetMax = new Vector2(0, 0);

        var tiVL = AddVL(tiContent, 24, 6);

        // Top accent bar - glowing
        var tiTopBar = new GameObject("TopBar", typeof(RectTransform), typeof(Image));
        tiTopBar.transform.SetParent(tiVL.transform, false);
        var tiTopBarImg = tiTopBar.GetComponent<Image>();
        tiTopBarImg.color = new Color(0f, 1f, 0.816f, 0.85f);
        tiTopBarImg.raycastTarget = false;
        tiTopBar.AddComponent<LayoutElement>().preferredHeight = 4;

        Spacer(tiVL, 14);

        // Header label
        _traitInfoHeader = Txt(tiVL, "技能", 13, new Color(0.5f, 0.58f, 0.7f), 18).GetComponent<Text>();
        _traitInfoHeader.alignment = TextAnchor.MiddleCenter;

        // Skill name title
        _traitInfoTitle = Txt(tiVL, "", 24, Gold, 36).GetComponent<Text>();
        _traitInfoTitle.fontStyle = FontStyle.Bold;
        _traitInfoTitle.alignment = TextAnchor.MiddleCenter;

        Spacer(tiVL, 6);

        // Glowing underline
        var tiUnderline = new GameObject("Underline", typeof(RectTransform), typeof(Image));
        tiUnderline.transform.SetParent(tiVL.transform, false);
        var tiUnderRT = tiUnderline.GetComponent<RectTransform>();
        tiUnderRT.anchorMin = new Vector2(0.3f, 1);
        tiUnderRT.anchorMax = new Vector2(0.7f, 1);
        tiUnderRT.pivot = new Vector2(0.5f, 1);
        tiUnderRT.offsetMin = new Vector2(0, -1);
        tiUnderRT.offsetMax = new Vector2(0, -1);
        tiUnderline.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.35f);
        tiUnderline.GetComponent<Image>().raycastTarget = false;
        tiUnderline.AddComponent<LayoutElement>().preferredHeight = 2;

        Spacer(tiVL, 14);

        // Trait cards container
        _traitCardContainer = new GameObject("TraitCards", typeof(RectTransform));
        _traitCardContainer.transform.SetParent(tiVL.transform, false);
        var tiCCRT = _traitCardContainer.GetComponent<RectTransform>();
        tiCCRT.anchorMin = new Vector2(0, 0);
        tiCCRT.anchorMax = new Vector2(1, 1);
        tiCCRT.offsetMin = Vector2.zero; tiCCRT.offsetMax = Vector2.zero;
        var tiCCLE = _traitCardContainer.AddComponent<LayoutElement>();
        tiCCLE.flexibleHeight = 1;
        tiCCLE.minHeight = 0;

        Spacer(tiVL, 16);

        // Close button
        var closeBtn = PrimaryBtn(tiVL.transform, "关闭", 16, 48);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => {
            _traitInfoOvl.SetActive(false);
            if (_traitInfoPanelCG != null) _traitInfoPanelCG.alpha = 0f;
            if (_traitInfoCG != null) _traitInfoCG.alpha = 0f;
        });

        // Click-outside to close
        var tiClickBg = _traitInfoOvl.AddComponent<UnityEngine.UI.Button>();
        tiClickBg.targetGraphic = tiBGImg;
        tiClickBg.onClick.AddListener(() => {
            _traitInfoOvl.SetActive(false);
            if (_traitInfoPanelCG != null) _traitInfoPanelCG.alpha = 0f;
            if (_traitInfoCG != null) _traitInfoCG.alpha = 0f;
        });

        _traitInfoOvl.SetActive(false);

        // Action buttons layout - using anchor-based positioning
        // Attack | Possess | Ultimate (top row), Defend | Flee (bottom row)
        
        _cAtkBtn = CreateCombatButton(actGo.transform, "攻击", new Color(0.95f,0.45f,0.25f), 0.03f, 0.52f, 0.33f, 0.92f);
        var atkLabel = _cAtkBtn.GetComponentInChildren<Text>();
        var atkHold = _cAtkBtn.gameObject.AddComponent<HoldButton>();
        atkHold.onTrigger = () => CompleteGameSystem.Instance?.PlayerAttack();
        atkHold.firstDelay = 1f;
        atkHold.repeatInterval = 0.3f;
        atkHold.onStart = () => { atkLabel.text = "自动攻击"; };
        atkHold.onEnd = () => { atkLabel.text = "攻击"; };

        _cPossBtn = CreateCombatButton(actGo.transform, "附身", new Color(0.65f, 0.35f, 0.95f), 0.35f, 0.52f, 0.62f, 0.92f, () => ShowPossessPanel());

        _cUltBtn = CreateCombatButton(actGo.transform, "终极技", new Color(0.95f,0.75f,0.3f), 0.68f, 0.52f, 0.97f, 0.92f, () => CompleteGameSystem.Instance?.UseUltimate());
        _cUltLabel = _cUltBtn.GetComponentInChildren<Text>();

        _cDefBtn = CreateCombatButton(actGo.transform, "防御", new Color(0.25f, 0.8f, 0.65f), 0.03f, 0.15f, 0.33f, 0.48f, () => CompleteGameSystem.Instance?.PlayerDefend());

        _cFleeBtn = CreateCombatButton(actGo.transform, "逃跑", new Color(0.6f, 0.55f, 0.65f), 0.35f, 0.15f, 0.62f, 0.48f, () => CompleteGameSystem.Instance?.PlayerFlee());

        // Form switch row at very bottom
        var fr = new GameObject("Forms", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        fr.transform.SetParent(rt, false);
        var frRT = fr.GetComponent<RectTransform>();
        frRT.anchorMin = new Vector2(0, 0);
        frRT.anchorMax = new Vector2(1, 0.05f);
        frRT.offsetMin = new Vector2(8, 0);
        frRT.offsetMax = new Vector2(-8, 0);
        var frHL = fr.GetComponent<HorizontalLayoutGroup>();
        frHL.spacing = 4;
        frHL.childAlignment = TextAnchor.MiddleCenter;
        frHL.childForceExpandWidth = true;
        frHL.childForceExpandHeight = true;
        _cFormRow = fr.transform;

        // === Host traits bar (overlay on top of cards area, only shown when possessed) ===
        _cHostBar = new GameObject("HostBar", typeof(RectTransform), typeof(Image), typeof(Outline));
        _cHostBar.transform.SetParent(rt, false);
        _cHostBar.GetComponent<Image>().color = new Color(0.08f, 0.10f, 0.14f, 0.95f);
        var hostOL = _cHostBar.GetComponent<Outline>();
        hostOL.effectColor = new Color(0f, 1f, 0.816f, 0.35f);
        hostOL.effectDistance = new Vector2(1, 1);
        var hostRT = _cHostBar.GetComponent<RectTransform>();
        hostRT.anchorMin = new Vector2(0, 0.86f);
        hostRT.anchorMax = new Vector2(1, 0.90f);
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

    IEnumerator AnimateCombatEntrance()
    {
        if (_cVsRT != null)
            yield return UIAnimationSystem.ScalePulse(_cVsRT, 1.15f, 0.35f);
    }

    void PlayCombatLogPulse()
    {
        if (_cLogPanel != null)
            StartCoroutine(UIAnimationSystem.Flash(_cLogPanel, new Color(0.85f, 0.65f, 0.2f, 0.35f), 0.18f));
    }

    Sprite CreateSimpleSprite(Color color, int width, int height)
    {
        Texture2D tex = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                tex.SetPixel(x, y, color);
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateRadialGradientSprite(int size, float innerRadiusRatio = 0.4f)
    {
        int res = Mathf.Max(64, size);
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        
        float center = res / 2f;
        float maxDist = res / 2f;
        float innerDist = maxDist * innerRadiusRatio;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (dist <= innerDist)
                {
                    pixels[y * res + x] = Color.clear;
                }
                else if (dist <= maxDist)
                {
                    float t = (dist - innerDist) / (maxDist - innerDist);
                    float alpha = Mathf.Lerp(0f, 1f, t * t);
                    pixels[y * res + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[y * res + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateHexPatternSprite(int size)
    {
        int res = Mathf.Max(64, size);
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];

        float hexSize = res / 8f;
        float hexW = hexSize * Mathf.Sqrt(3f);
        float hexH = hexSize * 1.5f;

        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float col = x / (hexW * 1.5f);
                float row = y / hexH;
                int colI = Mathf.FloorToInt(col);
                int rowI = Mathf.FloorToInt(row);

                float offX = (rowI % 2 == 0) ? 0f : hexW * 0.75f;
                float cx = colI * hexW * 1.5f + offX + hexW * 0.5f;
                float cy = rowI * hexH + hexH * 0.5f;

                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);

                float hexThickness = Mathf.Max(1f, hexSize * 0.08f);

                bool onEdge = false;
                float dxA = hexW * 0.5f - dx;
                float dyA = hexSize - dy;
                if (dx <= hexW * 0.5f && dy <= hexSize)
                {
                    if (dyA < hexThickness || dxA < hexThickness) onEdge = true;
                    if (Mathf.Abs(dyA + dxA * 0.5f) < hexThickness) onEdge = true;
                    if (Mathf.Abs(dyA - dxA * 0.5f) < hexThickness) onEdge = true;
                }

                pixels[y * res + x] = onEdge ? new Color(1, 1, 1, 0.6f) : Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    Sprite CreateCircleSprite(int size)
    {
        int res = Mathf.Max(16, size);
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        
        float center = res / 2f;
        float radius = res / 2f - 1f;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (dist <= radius)
                {
                    pixels[y * res + x] = Color.white;
                }
                else
                {
                    pixels[y * res + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
    }

    // 发光光环 sprite: 中心实心圆 + 外围渐变光晕,用于地图元素的发光效果
    static readonly System.Collections.Generic.Dictionary<int, Sprite> _glowSpriteCache = new System.Collections.Generic.Dictionary<int, Sprite>();
    Sprite CreateGlowSprite(int size)
    {
        if (_glowSpriteCache.TryGetValue(size, out var cached)) return cached;
        
        int res = Mathf.Max(64, size);
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        
        float center = res / 2f;
        float maxDist = res / 2f;
        float solidRadius = maxDist * 0.15f;   // 中心实心圆
        float glowStart = maxDist * 0.20f;    // 光晕开始
        float glowEnd = maxDist * 0.85f;      // 光晕结束(边缘淡出)
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (dist <= solidRadius)
                {
                    // 中心实心: 完全不透明
                    pixels[y * res + x] = new Color(1, 1, 1, 1);
                }
                else if (dist <= glowStart)
                {
                    // 实心到光晕的过渡: 快速衰减
                    float t = (dist - solidRadius) / (glowStart - solidRadius);
                    float alpha = 1f - t * t;
                    pixels[y * res + x] = new Color(1, 1, 1, alpha);
                }
                else if (dist <= glowEnd)
                {
                    // 光晕区: 平滑衰减
                    float t = (dist - glowStart) / (glowEnd - glowStart);
                    float alpha = 0.85f * (1f - t * t);
                    pixels[y * res + x] = new Color(1, 1, 1, alpha);
                }
                else
                {
                    pixels[y * res + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        var spr = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        _glowSpriteCache[size] = spr;
        return spr;
    }

    // 玩家专属标记: 双层圆环 + 中心发光, 区分英雄与怪物
    static readonly System.Collections.Generic.Dictionary<int, Sprite> _markerSpriteCache = new System.Collections.Generic.Dictionary<int, Sprite>();
    Sprite CreatePlayerMarkerSprite(int size)
    {
        if (_markerSpriteCache.TryGetValue(size, out var cached)) return cached;
        
        int res = Mathf.Max(64, size);
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        
        float center = res / 2f;
        float maxDist = res / 2f;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (dist > maxDist - 1f)
                {
                    pixels[y * res + x] = Color.clear;
                }
                else
                {
                    // 距离边缘的归一化值 (0=中心, 1=边缘)
                    float t = dist / maxDist;
                    
                    // 外环: 0.78-0.92 范围内的圆环
                    if (t >= 0.78f && t <= 0.92f)
                    {
                        pixels[y * res + x] = new Color(1, 1, 1, 1);
                    }
                    // 内环: 0.45-0.55 范围内的细环
                    else if (t >= 0.45f && t <= 0.55f)
                    {
                        pixels[y * res + x] = new Color(1, 1, 1, 0.9f);
                    }
                    // 中心: 实心圆 + 光晕
                    else if (t < 0.35f)
                    {
                        float centerGlow = 1f - t / 0.35f;
                        pixels[y * res + x] = new Color(1, 1, 1, 0.7f + centerGlow * 0.3f);
                    }
                    // 两环之间: 透明 + 微弱光晕衰减
                    else
                    {
                        float midT = (t - 0.35f) / (0.78f - 0.35f);
                        float fadeT = midT < 0.5f ? midT * 2f : (1f - midT) * 2f;
                        float alpha = 0.15f * fadeT * fadeT;
                        pixels[y * res + x] = new Color(1, 1, 1, alpha);
                    }
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        var spr = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        _markerSpriteCache[size] = spr;
        return spr;
    }

    Sprite CreateScanlineSprite()
    {
        int resX = 540;
        int resY = 960;
        Texture2D tex = new Texture2D(resX, resY, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[resX * resY];
        
        for (int y = 0; y < resY; y++)
        {
            for (int x = 0; x < resX; x++)
            {
                if (y % 6 == 0)
                {
                    pixels[y * resX + x] = new Color(1, 1, 1, 0.2f);
                }
                else if (y % 12 == 6)
                {
                    pixels[y * resX + x] = new Color(1, 1, 1, 0.1f);
                }
                else
                {
                    pixels[y * resX + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        return Sprite.Create(tex, new Rect(0, 0, resX, resY), new Vector2(0.5f, 0.5f));
    }

}
