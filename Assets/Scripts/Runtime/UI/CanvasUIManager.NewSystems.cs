using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public partial class CanvasUIManager
{
    // ========== 遗产面板 ==========
    GameObject _legacyOvl;
    Transform _legacyList;
    bool _legacyBuilt;

    // ========== 楼层事件面板 ==========
    GameObject _floorEventOvl;
    Transform _floorEventContent;

    public void ShowLegacyPanel()
    {
        if (_legacyOvl == null) BuildLegacyOverlay();
        _legacyBuilt = false;
        SyncLegacy();
        OpenOverlayAnimated(_legacyOvl);
    }

    void BuildLegacyOverlay()
    {
        var content = BuildOverlayScaffold(ref _legacyOvl, "LegacyOvl", "★ 附身遗产", Purp, OvlBgDense);
        var listGo = new GameObject("LegacyList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6; lg.padding = new RectOffset(4, 4, 4, 4);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _legacyList = listGo.transform;
    }

    void SyncLegacy()
    {
        if (_legacyOvl == null || !_legacyOvl.activeSelf) return;
        if (_legacyBuilt) return;
        _legacyBuilt = true;

        for (int i = _legacyList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_legacyList.GetChild(i).gameObject);

        var mgr = LegacyManager.Instance;
        if (mgr == null) return;

        int maxSlots = mgr.GetMaxLegacies();
        var equipped = mgr.GetEquippedLegacies();

        Txt(_legacyList.gameObject, $"遗产槽位: {equipped.Count}/{maxSlots}", 13, Cyan, 22);
        Spacer(_legacyList.gameObject, SpaceSmall);

        for (int i = 0; i < maxSlots; i++)
        {
            if (i < equipped.Count)
            {
                var leg = equipped[i];
                Color rarityColor = leg.rarity == 2 ? Purp : leg.rarity == 1 ? Gold : Bright;
                Color cardBg = leg.isMutated
                    ? new Color(0.15f, 0.06f, 0.06f, 0.9f)
                    : new Color(0.08f, 0.08f, 0.15f, 0.9f);

                var card = CardGo(_legacyList, cardBg, 72);
                var cardOutline = card.GetComponent<Outline>();
                if (cardOutline != null)
                {
                    Color borderColor = leg.rarity == 2
                        ? new Color(0.6f, 0.2f, 0.9f, 0.7f)
                        : leg.rarity == 1
                            ? new Color(1f, 0.8f, 0.2f, 0.55f)
                            : new Color(0.5f, 0.5f, 0.55f, 0.25f);
                    cardOutline.effectColor = borderColor;
                    cardOutline.effectDistance = new Vector2(2, 2);
                }
                var vl = AddVL(card, 6, 4);

                var headerGo = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                headerGo.transform.SetParent(vl.transform, false);
                headerGo.AddComponent<LayoutElement>().preferredHeight = 20;
                var hlg = headerGo.GetComponent<HorizontalLayoutGroup>();
                hlg.spacing = 6; hlg.childAlignment = TextAnchor.MiddleLeft;
                hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;

                Txt(headerGo, leg.icon, 14, rarityColor, 20);
                Txt(headerGo, leg.name, 13, rarityColor, 20);
                Txt(headerGo, $"[{leg.RarityLabel()}]", 10, rarityColor, 20);
                var buildTag = leg.BuildTag();
                Txt(headerGo, $"●{buildTag.tag}", 10, buildTag.color, 20);
                if (leg.isMutated)
                    Txt(headerGo, "腐蚀", 10, SoftRed, 20);

                Txt(vl.gameObject, leg.description, 11, Bright, 16);
                Txt(vl.gameObject, $"来源: {leg.sourceMonsterName}  效果值: {leg.effectValue:F1}", 10, Dim, 14);
            }
            else
            {
                var card = CardGo(_legacyList, new Color(0.04f, 0.04f, 0.08f, 0.7f), 50);
                var vl = AddVL(card, 6, 4);
                Txt(vl.gameObject, $"[空槽 {i + 1}]", 13, Dim, 22);
                Txt(vl.gameObject, "替换形态后获得遗产", 10, Dim, 16);
            }
        }
    }

    // ========== 楼层事件面板 ==========
    public void ShowFloorEventPanel()
    {
        if (_floorEventOvl == null) BuildFloorEventOverlay();
        SyncFloorEvent();
        OpenOverlayAnimated(_floorEventOvl);
    }

    public void ShowFloorEventDelayed(string icon, string name)
    {
        ShowFlashBanner($"{icon} {name}", Gold, 1.2f);
        StartCoroutine(DelayedShowFloorEvent());
    }

    System.Collections.IEnumerator DelayedShowFloorEvent()
    {
        yield return new WaitForSeconds(1.4f);
        ShowFloorEventPanel();
    }

    void BuildFloorEventOverlay()
    {
        var content = BuildOverlayScaffold(ref _floorEventOvl, "FloorEventOvl", "📦 发现物品", Gold, OvlBgDense);
        var listGo = new GameObject("EventContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 10; lg.padding = new RectOffset(12, 12, 12, 12);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _floorEventContent = listGo.transform;
    }

    void SyncFloorEvent()
    {
        if (_floorEventContent == null) return;

        for (int i = _floorEventContent.childCount - 1; i >= 0; i--)
            DestroyImmediate(_floorEventContent.GetChild(i).gameObject);

        var mgr = FloorEventManager.Instance;
        if (mgr == null || !mgr.HasActiveEvent()) return;

        var activeEvent = mgr.GetCurrentEvent();
        var config = mgr.GetConfig(activeEvent.configId);
        if (config == null) return;

        // 事件图标和标题
        Txt(_floorEventContent.gameObject, $"{config.icon} {config.name}", 18, Gold, 30);
        Spacer(_floorEventContent.gameObject, SpaceSmall);

        // 描述
        var descCard = CardGo(_floorEventContent, new Color(0.08f, 0.08f, 0.15f, 0.9f), 60);
        var descVl = AddVL(descCard, 8, 4);
        Txt(descVl.gameObject, config.desc, 13, Bright, 0);

        Spacer(_floorEventContent.gameObject, SpaceMed);

        // 选项按钮
        if (config.choices != null)
        {
            for (int i = 0; i < config.choices.Length; i++)
            {
                int choiceIdx = i;
                string choiceText = config.choices[i];

                Color btnBg = i == 0 ? new Color(0.06f, 0.18f, 0.15f, 0.95f) : DarkItemBg;
                Color btnText = i == 0 ? Cyan : Bright;

                var btnGo = new GameObject($"Choice_{i}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
                btnGo.transform.SetParent(_floorEventContent, false);
                btnGo.AddComponent<LayoutElement>().preferredHeight = 48;

                var img = btnGo.GetComponent<Image>();
                img.color = btnBg;

                var ol = btnGo.GetComponent<Outline>();
                ol.effectColor = new Color(btnText.r, btnText.g, btnText.b, 0.3f);
                ol.effectDistance = new Vector2(1, 1);

                var btn = btnGo.GetComponent<Button>();
                btn.targetGraphic = img;
                btn.onClick.AddListener(() =>
                {
                    var result = FloorEventManager.Instance?.ResolveChoice(choiceIdx);
                    if (result.HasValue && result.Value.success)
                    {
                        ShowFloorEventResult(result.Value);
                    }
                    StartCoroutine(DelayedCloseFloorEvent());
                });

                var txt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
                txt.transform.SetParent(btnGo.transform, false);
                Stretch(txt);
                var t = txt.GetComponent<Text>();
                t.font = F(); t.fontSize = 14; t.color = btnText;
                t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
                t.text = choiceText; t.raycastTarget = false;
            }
        }
    }

    void ShowFloorEventResult(FloorEventManager.EventResult result)
    {
        if (!string.IsNullOrEmpty(result.message))
        {
            CompleteGameSystem.Instance?.AddCombatLog($"⚡ {result.message}");
            ShowFlashBanner($"⚡ {result.message}", Gold, 1.5f);
        }
    }

    System.Collections.IEnumerator DelayedCloseFloorEvent()
    {
        yield return new WaitForSeconds(1.0f);
        StartCoroutine(CloseOverlayAnimated(_floorEventOvl));
    }

    // ========== 污染技能战斗集成 ==========
    GameObject _corruptSkillOvl;
    Transform _corruptSkillContent;

    public void ShowCorruptionSkillSelector()
    {
        if (_corruptSkillOvl == null) BuildCorruptionSkillSelector();
        SyncCorruptionSkills();
        OpenOverlayAnimated(_corruptSkillOvl);
    }

    void BuildCorruptionSkillSelector()
    {
        var content = BuildOverlayScaffold(ref _corruptSkillOvl, "CorruptSkillOvl", "☢ 污染技能", new Color(0.9f, 0.5f, 0.1f), OvlBgDense);
        var listGo = new GameObject("SkillList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 8; lg.padding = new RectOffset(6, 6, 6, 6);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _corruptSkillContent = listGo.transform;
    }

    void SyncCorruptionSkills()
    {
        if (_corruptSkillContent == null) return;

        for (int i = _corruptSkillContent.childCount - 1; i >= 0; i--)
            DestroyImmediate(_corruptSkillContent.GetChild(i).gameObject);

        var mgr = CorruptionSkillManager.Instance;
        if (mgr == null) return;

        var player = GameManager.Instance?.Player;
        float poll = player != null ? player.pollution : 0;

        Txt(_corruptSkillContent.gameObject, $"当前污染: {poll:F0}%", 14,
            ParasiteTowerColorScheme.GetPollutionColor(poll / 100f), 24);
        Spacer(_corruptSkillContent.gameObject, SpaceSmall);

        var allSkills = mgr.GetAllSkills();
        if (allSkills == null || allSkills.Length == 0)
        {
            Txt(_corruptSkillContent.gameObject, "暂无技能数据", 13, Dim, 30);
            return;
        }

        bool inCombat = CompleteGameSystem.Instance?.CurrentScreen == CompleteGameSystem.RunScreen.Combat;

        foreach (var config in allSkills)
        {
            bool unlocked = poll >= config.threshold;
            bool available = mgr.IsSkillAvailable(config.id, poll);
            int cd = mgr.GetCooldownRemaining(config.id);

            Color cardBg = available
                ? new Color(0.06f, 0.15f, 0.12f, 0.9f)
                : (unlocked ? new Color(0.08f, 0.08f, 0.15f, 0.9f) : new Color(0.05f, 0.04f, 0.08f, 0.9f));

            var card = CardGo(_corruptSkillContent, cardBg, 90);
            var vl = AddVL(card, 8, 4);

            // 标题行
            var headerGo = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            headerGo.transform.SetParent(vl.transform, false);
            headerGo.AddComponent<LayoutElement>().preferredHeight = 22;
            var hlg = headerGo.GetComponent<HorizontalLayoutGroup>();
            hlg.spacing = 6; hlg.childAlignment = TextAnchor.MiddleLeft;
            hlg.childForceExpandWidth = false; hlg.childForceExpandHeight = true;

            Color nameColor = available ? Cyan : (unlocked ? Bright : Dim);
            string tierStr = config.tier == 1 ? "I" : config.tier == 2 ? "II" : "III";
            Color tierColor = config.tier == 1 ? Bright : config.tier == 2 ? Gold : Purp;

            Txt(headerGo, config.icon, 14, nameColor, 22);
            Txt(headerGo, config.name, 13, nameColor, 22);
            Txt(headerGo, $"[{tierStr}]", 11, tierColor, 22);

            // 描述
            Txt(vl.gameObject, config.desc, 11, unlocked ? Bright : Dim, 16);

            // 消耗/冷却信息
            string costStr = $"消耗: {config.cost}污染  冷却: {config.cooldown}回合";
            if (cd > 0) costStr += $"  (剩余{cd}回合)";
            Txt(vl.gameObject, costStr, 10, MutedPurp, 14);

            if (!unlocked)
            {
                Txt(vl.gameObject, $"解锁条件: 污染≥{config.threshold}%", 10, Dim, 14);
            }
            else if (inCombat && available)
            {
                string skillId = config.id;
                var btn = BtnGo(vl.transform, "使用", 12, Cyan, 32);
                btn.GetComponent<Button>().onClick.AddListener(() =>
                {
                    var result = CorruptionSkillManager.Instance?.UseSkill(skillId);
                    if (result.HasValue && result.Value.success)
                    {
                        CompleteGameSystem.Instance?.AddCombatLog($"☢ {result.Value.message}");
                        if (result.Value.damageDealt > 0)
                        {
                            CompleteGameSystem.Instance?.ApplyDamageToEnemy(result.Value.damageDealt);
                        }
                    }
                    StartCoroutine(CloseOverlayAnimated(_corruptSkillOvl));
                });
            }
            else if (inCombat && !available && unlocked)
            {
                string reason = cd > 0 ? "冷却中" : "污染不足";
                Txt(vl.gameObject, reason, 10, SoftRed, 14);
            }
        }
    }
}
