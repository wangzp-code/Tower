using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
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
        var content = BuildOverlayScaffold(ref _pollSkillOvl, "PollSkillOvl", "☢ 污染技能", new Color(0.9f, 0.5f, 0.1f), OvlBgDense);
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
        Spacer(_pollSkillList.GetComponent<VerticalLayoutGroup>(), SpaceTiny);

        // 主动技能
        SectionHeader(_pollSkillList.GetComponent<VerticalLayoutGroup>(), "⚡ 主动技能", Gold);
        foreach (var skill in PollutionTierData.Skills)
        {
            bool unlocked = poll >= skill.threshold;
            Color cardC = unlocked ? new Color(0.06f, 0.12f, 0.08f) : CardBg;
            Color textC = unlocked ? Cyan : Dim;

            var (card, cvl) = InfoCard(_pollSkillList, cardC, 60);
            string status = unlocked ? "✓ 已解锁" : $"○ 需要{skill.threshold}%污染";
            Txt(cvl, $"☢ {skill.name}  [{status}]", 14, textC, 22);
            Txt(cvl, skill.desc, 11, unlocked ? Bright : Dim, 18);
            Label(cvl, $"解锁条件: 污染≥{skill.threshold}%");
        }

        Spacer(_pollSkillList.GetComponent<VerticalLayoutGroup>(), SpaceSmall);

        // 被动效果
        SectionHeader(_pollSkillList.GetComponent<VerticalLayoutGroup>(), "◈ 被动效果", Purp);
        foreach (var passive in PollutionTierData.Passives)
        {
            bool active = poll >= passive.threshold;
            Color cardC = active ? new Color(0.12f, 0.06f, 0.18f) : CardBg;
            Color textC = active ? Purp : Dim;

            var (card, cvl) = InfoCard(_pollSkillList, cardC, 50);
            string status = active ? "✓ 生效中" : $"○ 需要{passive.threshold}%";
            Txt(cvl, $"◈ {passive.name}  [{status}]", 13, textC, 20);
            Txt(cvl, passive.desc, 11, active ? Bright : Dim, 16);
        }

        Spacer(_pollSkillList.GetComponent<VerticalLayoutGroup>(), SpaceSmall);

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
        var content = BuildOverlayScaffold(ref _bondOvl, "BondOvl", "◇ 形态羁绊", Cyan, OvlBgDense);
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
        Spacer(_bondList.GetComponent<VerticalLayoutGroup>(), SpaceTiny);

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
                Label(infoVl.GetComponent<VerticalLayoutGroup>(), $"加成: {bonus}", Gold);
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
        var content = BuildOverlayScaffold(ref _anchorOvl, "AnchorOvl", "⊕ 锚点管理", Cyan, OvlBgDense);
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
        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), SpaceTiny);

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

                if (a.activated && !a.used)
                {
                    var (card, cvl) = ClickCard(_anchorList, cardC, 55);
                    Txt(cvl, $"⛓ F{a.floor} · {a.name}  [{status}]", 14, statusC, 22);
                    int dist = currentFloor - a.floor;
                    Label(cvl, $"距离当前: {dist}层  建立于: {a.activatedAt:MM/dd HH:mm}");

                    card.GetComponent<Button>().onClick.AddListener(() => {
                        anchor.UseAnchor();
                        _anchorBuilt = false;
                        StartCoroutine(CloseOverlayAnimated(_anchorOvl));
                    });
                }
                else
                {
                    var (card, cvl) = InfoCard(_anchorList, cardC, 55);
                    Txt(cvl, $"⛓ F{a.floor} · {a.name}  [{status}]", 14, statusC, 22);
                }
            }
        }

        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), 8);

        if (anchor.HasActiveAnchor())
        {
            Txt(_anchorList, $"⏪ 当前锚定: F{anchor.GetCurrentAnchorFloor()}", 13, Cyan, 20);
            Txt(_anchorList, "死亡时自动传送至最近锚点", 11, Dim, 16);
        }

        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), SpaceTiny);
        SectionDivider(_anchorList.GetComponent<VerticalLayoutGroup>(), "记忆断层", Dim);
        Spacer(_anchorList.GetComponent<VerticalLayoutGroup>(), SpaceTiny);
        Txt(_anchorList, "锚点之间的楼层记忆将丢失", 11, new Color(0.6f, 0.3f, 0.3f), 16);
    }

    public void ShowSettingsPanel()
    {
        if (_setOvl == null) BuildSettingsOverlay();
        OpenOverlayAnimated(_setOvl);
    }

    void BuildSettingsOverlay()
    {
        var content = BuildOverlayScaffold(ref _setOvl, "SetOvl", LocalizationData.T("终端设置"), Cyan, OvlBg, "bg_settings", "icon_settings");
        var vl = content.GetComponent<VerticalLayoutGroup>();

        Spacer(vl, SpaceSmall);

        // === Audio Section ===
        SectionDivider(vl, "音量控制", Bright);
        Spacer(vl, SpaceTiny);

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
        var privacyBtn = BtnGo(content, "◆ 隐私政策", 13, Dim, 36);
        privacyBtn.GetComponent<Button>().onClick.AddListener(() => {
            ShowPrivacyPolicy();
        });

        Spacer(vl, SpaceSmall);

        // === Info Section (at bottom) ===
        SectionDivider(vl, "信息", Bright);
        Spacer(vl, SpaceTiny);
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
        if (_echoAltarOvl != null) Destroy(_echoAltarOvl);
        var content = BuildOverlayScaffold(ref _echoAltarOvl, "EchoAltarOvl", "残响圣坛", Gold, OvlBg, null, "icon_altar");

        int balance = PlayerPrefs.GetInt("pt_meta_echoes", 0);
        var (altarBalCard, altarBalVL) = InfoCard(content, new Color(0.10f, 0.06f, 0.16f), 40);
        var balTxt = Txt(altarBalVL, $"残响余额: <color=#a855f7><b>{balance}</b></color>", 14, Bright, 24);

        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceTiny);

        string[][] nodes = {
            new string[] {"hp", "生命强化", "永久最大生命+5", "100", "pt_altar_hp", "5"},
            new string[] {"atk", "攻击强化", "永久攻击+1", "100", "pt_altar_atk", "3"},
            new string[] {"evo_rate", "进化加速", "进化点获取+5%", "200", "pt_altar_evo_rate", "5"},
            new string[] {"form_slot", "形态扩展", "形态上限+1", "400", "pt_altar_form_slot", "2"},
        };

        foreach (var node in nodes)
        {
            string nodeId = node[0];
            string nodeName = node[1];
            string nodeDesc = node[2];
            int price = int.Parse(node[3]);
            string prefsKey = node[4];
            int maxLevel = int.Parse(node[5]);
            int curLevel = PlayerPrefs.GetInt(prefsKey, 0);
            bool maxed = curLevel >= maxLevel;
            int scaledPrice = price + curLevel * (price / 2);

            var (card, cvl) = InfoCard(content, CardBg, 100);
            var nameRow = new GameObject("NameRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            nameRow.transform.SetParent(cvl.transform, false);
            nameRow.AddComponent<LayoutElement>().preferredHeight = 22;
            var nrHL = nameRow.GetComponent<HorizontalLayoutGroup>();
            nrHL.spacing = 8; nrHL.childAlignment = TextAnchor.MiddleLeft;
            nrHL.childForceExpandWidth = false;
            var lvlTxt = TxtGo(nameRow.transform, $"Lv.{curLevel}/{maxLevel}", 11, Dim);
            lvlTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 50;
            var nameTxtGo = TxtGo(nameRow.transform, nodeName, 14, Bright);
            nameTxtGo.fontStyle = FontStyle.Bold;
            nameTxtGo.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            Txt(cvl, nodeDesc, 11, Dim, 20);

            if (maxed)
            {
                var doneTxt = Txt(cvl, "已满级", 12, SuccessGreen, 26);
            }
            else
            {
                bool canAfford = balance >= scaledPrice;
                string btnLabel = canAfford ? $"购买 ({scaledPrice} 残响)" : $"残响不足 ({scaledPrice})";
                Color btnColor = canAfford ? Gold : Dim;
                var buyBtn = BtnGo(cvl.transform, btnLabel, 12, btnColor, 32);
                var btn = buyBtn.GetComponent<Button>();
                btn.interactable = canAfford;
                string capturedKey = prefsKey;
                int capturedPrice = scaledPrice;
                btn.onClick.AddListener(() =>
                {
                    int bal = PlayerPrefs.GetInt("pt_meta_echoes", 0);
                    if (bal < capturedPrice) return;
                    PlayerPrefs.SetInt("pt_meta_echoes", bal - capturedPrice);
                    int lv = PlayerPrefs.GetInt(capturedKey, 0);
                    PlayerPrefs.SetInt(capturedKey, lv + 1);
                    PlayerPrefs.Save();
                    ShowFlashBanner($"圣坛强化！{nodeName} Lv.{lv + 1}", Gold, 1.5f);
                    ShowEchoAltarPanel();
                });
            }
        }

        OpenOverlayAnimated(_echoAltarOvl);
    }

    // ========== 游戏内道具商店 ==========
    GameObject _itemShopOvl;

    static readonly Dictionary<string, string> ShopCatLabels = new Dictionary<string, string>
    {
        { "supply", "基础补给" },
        { "survival", "净化保命" },
        { "growth", "形态成长" },
        { "info", "信息优势" }
    };
    static readonly string[] ShopCatOrder = { "supply", "survival", "growth", "info" };

    public void ShowItemShopPanel()
    {
        ShopManager.Instance.GenerateShopItems(GameManager.Instance.CurrentFloor);

        if (_itemShopOvl != null) Destroy(_itemShopOvl);
        var content = BuildOverlayScaffold(ref _itemShopOvl, "ItemShopOvl", "💰 神 秘 商 店", Gold, OvlBg);

        var vl = content.GetComponent<VerticalLayoutGroup>();
        Spacer(vl, SpaceSmall);

        var player = GameManager.Instance.Player;
        int ep = player?.evolutionPoints ?? 0;
        var epRow = Txt(vl, $"当前进化点：{ep}", 14, Cyan, 22);
        epRow.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

        Spacer(vl, SpaceSmall);

        var items = ShopManager.Instance.GetCurrentItems();

        foreach (var catKey in ShopCatOrder)
        {
            var catItems = items.FindAll(it => it.cat == catKey);
            if (catItems.Count == 0) continue;

            string catLabel = ShopCatLabels.ContainsKey(catKey) ? ShopCatLabels[catKey] : catKey;
            var catTitle = Txt(vl, catLabel, 14, Gold, 22);
            catTitle.GetComponent<Text>().fontStyle = FontStyle.Bold;
            catTitle.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;

            var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
            sep.transform.SetParent(vl.transform, false);
            sep.AddComponent<LayoutElement>().preferredHeight = 1;
            sep.GetComponent<Image>().color = new Color(0.3f, 0.3f, 0.4f, 0.5f);

            foreach (var shopItem in catItems)
            {
                var si = shopItem;
                int price = ShopManager.Instance.GetPrice(si);
                bool canBuy = ShopManager.Instance.CanBuy(si);

                int bought = 0;
                bool soldOut = false;
                if (si.maxBuy > 0)
                {
                    var field = typeof(ShopManager).GetField("_purchaseCounts", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                    if (field != null)
                    {
                        var counts = field.GetValue(ShopManager.Instance) as Dictionary<string, int>;
                        if (counts != null && counts.TryGetValue(si.id, out int c)) bought = c;
                    }
                    soldOut = bought >= si.maxBuy;
                }

                var row = new GameObject($"Item_{si.id}", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(Button), typeof(Image));
                row.transform.SetParent(vl.transform, false);
                row.AddComponent<LayoutElement>().preferredHeight = 36;
                var rowHL = row.GetComponent<HorizontalLayoutGroup>();
                rowHL.padding = new RectOffset(12, 12, 4, 4);
                rowHL.spacing = 8;
                rowHL.childForceExpandWidth = false;
                rowHL.childForceExpandHeight = true;
                rowHL.childAlignment = TextAnchor.MiddleLeft;
                var rowImg = row.GetComponent<Image>();
                rowImg.color = new Color(0, 0, 0, 0.01f);

                var nameGo = Txt(rowHL, si.name, 14, canBuy ? Bright : Dim, 24);
                nameGo.GetComponent<Text>().fontStyle = FontStyle.Bold;
                nameGo.GetComponent<LayoutElement>().preferredWidth = 90;

                var descGo = Txt(rowHL, si.desc, 12, canBuy ? new Color(0.6f, 0.6f, 0.7f) : Dim, 24);
                descGo.GetComponent<LayoutElement>().flexibleWidth = 1;

                string statusText;
                Color statusColor;
                if (soldOut)
                {
                    statusText = "已满";
                    statusColor = Dim;
                }
                else if (!canBuy)
                {
                    statusText = $"EP不足({price})";
                    statusColor = new Color(0.5f, 0.4f, 0.3f);
                }
                else
                {
                    statusText = $"{price} EP";
                    statusColor = Gold;
                }

                var statusGo = Txt(rowHL, statusText, 12, statusColor, 24);
                statusGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
                statusGo.GetComponent<LayoutElement>().preferredWidth = 100;

                var btn = row.GetComponent<Button>();
                btn.targetGraphic = rowImg;
                btn.interactable = canBuy && !soldOut;
                btn.onClick.AddListener(() =>
                {
                    if (ShopManager.Instance.Purchase(si))
                    {
                        CompleteGameSystem.Instance?.AddCombatLog($"<color=#ffcc00>† 购买了 {si.name}</color>");
                        ShowItemShopPanel();
                    }
                });
            }

            Spacer(vl, 6);
        }

        Spacer(vl, SpaceMed);

        var closeBtn = PrimaryBtn(vl.transform, "关闭", 16, 50);
        closeBtn.GetComponent<Button>().onClick.AddListener(() =>
        {
            if (_itemShopOvl != null) StartCoroutine(CloseOverlayAnimated(_itemShopOvl));
        });

        Spacer(vl, SpaceMed);
        OpenOverlayAnimated(_itemShopOvl);
    }

    void EnsureShopConfigManager()
    {
        if (ShopConfigManager.Instance == null)
        {
            GameObject managerObj = new GameObject("ShopConfigManager");
            managerObj.AddComponent<ShopConfigManager>();
            DontDestroyOnLoad(managerObj);
        }
    }

    public void ShowShopPanel()
    {
        EnsureShopConfigManager();

        if (CompleteGameSystem.Instance != null)
        {
            var gs = CompleteGameSystem.Instance;
            var hints = gs.ShownSoftHintIds;
            if (hints != null && !hints.Contains("hint_shop_first"))
            {
                hints.Add("hint_shop_first");
                gs.AddCombatLog("<color=#ffcc00>◆ 商店消耗EP购买道具和强化，注意管理EP花销！</color>");
            }
        }

        if (_echoAltarOvl != null) Destroy(_echoAltarOvl);
        var content = BuildOverlayScaffold(ref _echoAltarOvl, "ShopOvl", "残响商店", Purp, OvlBg, null, "icon_shop");

        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceSmall);

        string[] mainCategories = ShopConfigManager.Instance?.GetMainCategories() ?? new string[] { "职业", "皮肤", "功能", "礼包" };
        var tabsHL = new GameObject("TabsHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        tabsHL.transform.SetParent(content, false);
        var tabsHLG = tabsHL.GetComponent<HorizontalLayoutGroup>();
        tabsHLG.spacing = 4;
        tabsHLG.childForceExpandWidth = true;

        GameObject[] mainTabButtons = new GameObject[mainCategories.Length];
        for (int i = 0; i < mainCategories.Length; i++)
        {
            mainTabButtons[i] = TabBtn(tabsHL.transform, mainCategories[i], i == 0, Purp);
            int idx = i;
            mainTabButtons[i].GetComponent<Button>().onClick.AddListener(() => SwitchShopCategory(mainCategories[idx], mainTabButtons, null, content));
        }

        ShowShopCategory("职业", content);

        string[] bottomTabs = ShopConfigManager.Instance?.GetBottomTabs() ?? new string[] { "每日特惠", "月卡", "限购礼包", "闯关礼包" };
        var bottomTabsHL = new GameObject("BottomTabsHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        bottomTabsHL.transform.SetParent(content, false);
        var bottomTabsHLG = bottomTabsHL.GetComponent<HorizontalLayoutGroup>();
        bottomTabsHLG.spacing = 4;
        bottomTabsHLG.childForceExpandWidth = true;

        for (int i = 0; i < bottomTabs.Length; i++)
        {
            var btn = TabBtn(bottomTabsHL.transform, bottomTabs[i], false, BtnBg);
            int idx = i;
            btn.GetComponent<Button>().onClick.AddListener(() => SwitchShopCategory(bottomTabs[idx], mainTabButtons, bottomTabsHL.transform, content));
        }

        OpenOverlayAnimated(_echoAltarOvl);
    }

    void SwitchShopCategory(string category, GameObject[] mainTabs, Transform bottomTabsHL, Transform content)
    {
        if (mainTabs != null)
        {
            for (int i = 0; i < mainTabs.Length; i++)
            {
                var img = mainTabs[i].GetComponent<Image>();
                img.color = mainTabs[i].GetComponentInChildren<Text>().text == category ? Purp : BtnBg;
            }
        }

        if (bottomTabsHL != null)
        {
            for (int i = 0; i < bottomTabsHL.childCount; i++)
            {
                var child = bottomTabsHL.GetChild(i).gameObject;
                var img = child.GetComponent<Image>();
                img.color = child.GetComponentInChildren<Text>().text == category ? Purp : BtnBg;
            }
        }

        for (int i = content.childCount - 1; i >= 0; i--)
        {
            var child = content.GetChild(i);
            if (child.name.StartsWith("ShopItem_") || child.name == "ItemsContainer")
            {
                Destroy(child.gameObject);
            }
        }

        ShowShopCategory(category, content);
    }

    void ShowShopCategory(string category, Transform content)
    {
        var items = EchoShopItemData.GetItemsByCategory(category);
        foreach (var item in items)
        {
            CreateShopItemCard(content, item);
        }
    }

    void CreateShopItemCard(Transform parent, EchoShopItemData item)
    {
        var (card, cvl) = InfoCard(parent, CardBg, 85);
        card.name = $"ShopItem_{item.id}";

        var nameRow = new GameObject("NameRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        nameRow.transform.SetParent(cvl.transform, false);
        nameRow.AddComponent<LayoutElement>().preferredHeight = 24;
        var nrHL = nameRow.GetComponent<HorizontalLayoutGroup>();
        nrHL.spacing = 8; nrHL.childAlignment = TextAnchor.MiddleLeft;
        nrHL.childForceExpandWidth = false;

        var nameTxt = TxtGo(nameRow.transform, item.name, 14, Bright);
        nameTxt.fontStyle = FontStyle.Bold;
        nameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var priceTxt = TxtGo(nameRow.transform, item.price, 12, Gold);
        priceTxt.fontStyle = FontStyle.Bold;
        priceTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 70;

        Txt(cvl, item.description, 11, Dim, 16);

        if (item.isCollection && item.includes != null && item.includes.Count > 0)
        {
            var includesGO = new GameObject("Includes", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            includesGO.transform.SetParent(cvl.transform, false);
            includesGO.AddComponent<LayoutElement>().preferredHeight = 16;
            var incHL = includesGO.GetComponent<HorizontalLayoutGroup>();
            incHL.spacing = 4; incHL.childAlignment = TextAnchor.MiddleLeft;

            TxtGo(includesGO.transform, "包含: ", 10, Dim);
            for (int i = 0; i < item.includes.Count; i++)
            {
                var incTxt = TxtGo(includesGO.transform, item.includes[i], 10, new Color(0.7f, 0.9f, 0.7f));
                if (i < item.includes.Count - 1)
                {
                    TxtGo(includesGO.transform, ",", 10, Dim);
                }
            }
        }

        bool purchased = item.IsPurchased();

        var btn = BtnGo(cvl.transform, purchased ? "已购买" : "购买", 12, purchased ? Dim : Purp, 28);
        btn.GetComponent<LayoutElement>().preferredWidth = 80;
        btn.GetComponent<Button>().interactable = !purchased;
        btn.GetComponent<Button>().onClick.AddListener(() => PurchaseShopItem(item));
    }

    void PurchaseShopItem(EchoShopItemData item)
    {
        if (!item.IsPurchased())
        {
            item.MarkPurchased();
            
            if (_echoAltarOvl != null)
            {
                Destroy(_echoAltarOvl);
                ShowShopPanel();
            }
        }
    }

    public void ShowDailyRewardPanel()
    {
        if (_dailyOvl != null) Destroy(_dailyOvl);
        var content = BuildOverlayScaffold(ref _dailyOvl, "DailyOvl", "每日登录", Gold, OvlBg, null, "icon_daily");

        int currentDay = GetDailyLoginDays();

        Spacer(content.GetComponent<VerticalLayoutGroup>(), 80);

        var (infoCard, infoVL) = InfoCard(content, CardBg, 50);
        Txt(infoVL, "每日登录领取残响，累积奖励更丰厚", 12, Dim, 18);
        Txt(infoVL, $"连续登录 <color=#ffd700><b>{currentDay}</b></color> 天", 15, Bright, 22);

        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceTiny);

        int[] rewards = DataConfigManager.Instance?.GetDailyLoginRewards() ?? new int[] { 10, 15, 20, 25, 30, 35, 45, 55, 70 };
        bool canClaim = CanClaimDailyReward();

        for (int row = 0; row < 3; row++)
        {
            var rowGO = new GameObject($"Row{row}", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
            rowGO.transform.SetParent(content, false);
            rowGO.GetComponent<LayoutElement>().preferredHeight = 140;
            var rowHL = rowGO.GetComponent<HorizontalLayoutGroup>();
            rowHL.spacing = 8;
            rowHL.childForceExpandWidth = true;
            rowHL.childForceExpandHeight = true;
            rowHL.childAlignment = TextAnchor.MiddleCenter;

            for (int col = 0; col < 3; col++)
            {
                int i = row * 3 + col;
                bool claimed = i + 1 < currentDay;
                bool current = i + 1 == currentDay;

                Color cellColor;
                if (current && canClaim) cellColor = new Color(0.20f, 0.12f, 0.30f);
                else if (claimed) cellColor = new Color(0.08f, 0.10f, 0.12f);
                else cellColor = new Color(0.05f, 0.04f, 0.07f);

                var cell = new GameObject($"Day{i + 1}", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
                cell.transform.SetParent(rowGO.transform, false);
                cell.GetComponent<LayoutElement>().flexibleWidth = 1;
                var cellImg = cell.GetComponent<Image>();
                cellImg.color = cellColor;
                var cellOL = cell.GetComponent<Outline>();
                if (current && canClaim)
                {
                    cellOL.effectColor = new Color(Gold.r, Gold.g, Gold.b, 0.6f);
                    cellOL.effectDistance = new Vector2(2, -2);
                }
                else
                {
                    cellOL.effectColor = new Color(0.2f, 0.18f, 0.3f, 0.4f);
                    cellOL.effectDistance = new Vector2(1, -1);
                }

                var cellVL = new GameObject("CellVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
                cellVL.transform.SetParent(cell.transform, false);
                Stretch(cellVL);
                var cellVLG = cellVL.GetComponent<VerticalLayoutGroup>();
                cellVLG.spacing = 4;
                cellVLG.childAlignment = TextAnchor.MiddleCenter;
                cellVLG.padding = new RectOffset(6, 6, 10, 8);
                cellVLG.childForceExpandHeight = false;

                var dayLabel = TxtGo(cellVL.transform, $"Day {i + 1}", 13, current ? Gold : Dim);
                dayLabel.alignment = TextAnchor.MiddleCenter;
                dayLabel.gameObject.AddComponent<LayoutElement>().preferredHeight = 20;

                string icon; Color iconColor; int iconSize;
                if (claimed) { icon = "✓"; iconColor = new Color(0.3f, 0.8f, 0.4f); iconSize = 30; }
                else if (current && canClaim) { icon = "!"; iconColor = Gold; iconSize = 34; }
                else { icon = "○"; iconColor = Dim; iconSize = 24; }
                var iconTxt = TxtGo(cellVL.transform, icon, iconSize, iconColor);
                iconTxt.alignment = TextAnchor.MiddleCenter;
                iconTxt.gameObject.AddComponent<LayoutElement>().flexibleHeight = 1;

                var rewardTxt = TxtGo(cellVL.transform, $"{rewards[Mathf.Min(i, rewards.Length - 1)]} ✦", claimed || current ? 16 : 14, claimed || current ? Gold : Dim);
                rewardTxt.alignment = TextAnchor.MiddleCenter;
                rewardTxt.fontStyle = current ? FontStyle.Bold : FontStyle.Normal;
                rewardTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

                if (current && canClaim)
                {
                    cell.AddComponent<Button>().onClick.AddListener(() => {
                        ClaimDailyReward();
                        ShowDailyRewardPanel();
                    });
                    cell.GetComponent<Button>().targetGraphic = cellImg;
                }
            }
        }

        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceSmall);

        int todayIdx = Mathf.Clamp(currentDay - 1, 0, rewards.Length - 1);
        int todayReward = rewards[todayIdx];
        if (canClaim)
        {
            var claimBtn = PrimaryBtn(content, $"领取今日 {todayReward} 残响", 16, 60);
            claimBtn.GetComponent<Button>().onClick.AddListener(() => {
                ClaimDailyReward();
                ShowDailyRewardPanel();
            });
        }
        else
        {
            var claimBtn = SecondaryBtn(content, "今日已领取", Dim, 16, 60);
            claimBtn.GetComponent<Button>().interactable = false;
        }

        OpenOverlayAnimated(_dailyOvl);
    }

    int GetDailyLoginDays()
    {
        return PlayerPrefs.GetInt("pt_daily_login_days", 0);
    }

    bool CanClaimDailyReward()
    {
        string lastClaim = PlayerPrefs.GetString("pt_daily_last_claim", "");
        string today = System.DateTime.Now.ToString("yyyy-MM-dd");
        return lastClaim != today;
    }

    string GetDailyReward(int days)
    {
        int epReward = 50 + (days - 1) * 10;
        return $"{epReward} EP";
    }

    void ClaimDailyReward()
    {
        int days = PlayerPrefs.GetInt("pt_daily_login_days", 0);
        int[] rewards = { 10, 15, 20, 25, 30, 35, 45, 55, 70 };
        int echoReward = rewards[Mathf.Min(days, rewards.Length - 1)];

        int currentEchoes = PlayerPrefs.GetInt("pt_meta_echoes", 0);
        PlayerPrefs.SetInt("pt_meta_echoes", currentEchoes + echoReward);

        days++;
        PlayerPrefs.SetInt("pt_daily_login_days", days);
        PlayerPrefs.SetString("pt_daily_last_claim", System.DateTime.Now.ToString("yyyy-MM-dd"));
        PlayerPrefs.Save();

        ShowFlashBanner($"领取成功！+{echoReward} 残响", Gold, 1.5f);
    }

    void ShowPrivacyPolicy()
    {
        var privacyOvl = new GameObject("PrivacyOvl", typeof(RectTransform), typeof(Image));
        privacyOvl.transform.SetParent(_root, false);
        var rt = privacyOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = privacyOvl.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.92f);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(privacyOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.1f, 0.1f);
        contentRT.anchorMax = new Vector2(0.9f, 0.9f);
        contentRT.offsetMin = Vector2.zero;
        contentRT.offsetMax = Vector2.zero;

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 8;
        vlg.padding = new RectOffset(12, 12, 12, 12);
        vlg.childForceExpandWidth = true;

        var title = TxtGo(content.transform, "◆ 隐私政策", 18, Gold);
        title.fontStyle = FontStyle.Bold;

        var scrollContent = new GameObject("ScrollContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
        scrollContent.transform.SetParent(content.transform, false);
        var scRT = scrollContent.GetComponent<RectTransform>();
        scRT.sizeDelta = new Vector2(0, 300);
        scrollContent.AddComponent<LayoutElement>().preferredHeight = 300;

        var scVLG = scrollContent.GetComponent<VerticalLayoutGroup>();
        scVLG.spacing = 6;
        scVLG.padding = new RectOffset(8, 8, 8, 8);

        string[] paragraphs = {
            "<b>1. 数据收集</b>\n仅在你完成游戏并选择上传成绩时，本游戏会向服务器发送：你设置的昵称、设备生成的随机ID（不含真实身份信息）、以及该局成绩数据（分数、层数、用时等）。如未设置昵称，则不会上传任何数据。",
            "<b>2. 本地存储</b>\n游戏进度保存在设备本地存储中，卸载应用将清除存档。你可通过\"记录管理→导出存档\"备份数据。",
            "<b>3. 网络使用</b>\n仅用于排行榜上传与查询；如关闭网络可正常游玩，仅本地排行榜可用。",
            "<b>4. 第三方服务</b>\n本游戏不集成任何第三方SDK、广告或分析服务。",
            "<b>5. 儿童隐私</b>\n本游戏不针对儿童收集任何信息。",
            "<b>6. 联系方式</b>\n如有疑问，请通过应用商店页面联系开发者。",
            "<b>7. 数据删除</b>\n你可以联系开发者删除你在服务器上的全部排行榜记录。",
            "<color=#888888>最后更新：2026年5月8日</color>"
        };

        foreach (var para in paragraphs)
        {
            var pTxt = TxtGo(scrollContent.transform, para, 11, Bright);
            pTxt.alignment = TextAnchor.UpperLeft;
        }

        var closeBtn = BtnGo(content.transform, "关闭", 13, Cyan, 40);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => {
            Destroy(privacyOvl);
        });
    }

    // ========== 记录管理面板 ==========
    GameObject _recOvl;
    Transform _recList;

    public void ShowRecordPanel()
    {
        if (_recOvl == null) BuildRecordOverlay();
        _recBuilt = false;
        OpenOverlayAnimated(_recOvl);
    }
    bool _recBuilt;

    void BuildRecordOverlay()
    {
        var content = BuildOverlayScaffold(ref _recOvl, "RecOvl", LocalizationData.T("记录管理"), Cyan, OvlBgDense, null, "icon_records");

        var listGo = new GameObject("RecList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 5; lg.padding = new RectOffset(4,4,4,4);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _recList = listGo.transform;
    }

    void SyncRecord()
    {
        if (_recOvl == null || !_recOvl.activeSelf) return;
        if (_recBuilt) return;
        _recBuilt = true;

        for (int i = _recList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_recList.GetChild(i).gameObject);

        var rvl = _recList.GetComponent<VerticalLayoutGroup>();

        // === Save Management ===
        SectionDivider(rvl, LocalizationData.T("存档管理"), Bright);
        Spacer(rvl, SpaceTiny);

        for (int slot = 0; slot < 3; slot++)
        {
            int s = slot;
            var info = SaveSystem.Instance?.GetSaveInfo(s);
            string label;
            if (info != null)
            {
                string cls = info.player?.selectedClass ?? "?";
                string time = info.saveTime.Year > 2000 ? info.saveTime.ToString("MM/dd HH:mm") : "";
                label = $"{LocalizationData.T("槽位")}{s+1}: F{info.currentFloor} [{cls}] {time}";
            }
            else
            {
                label = $"{LocalizationData.T("槽位")}{s+1}: {LocalizationData.T("空")}";
            }

            var slotCard = CardGo(_recList, CardBg, 40);
            var slotHL = slotCard.AddComponent<HorizontalLayoutGroup>();
            slotHL.spacing = 6; slotHL.padding = new RectOffset(8, 8, 4, 4);
            slotHL.childAlignment = TextAnchor.MiddleLeft;
            slotHL.childForceExpandWidth = false; slotHL.childForceExpandHeight = true;

            var slotTxt = TxtGo(slotCard.transform, label, 11, info != null ? Bright : Dim);
            slotTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            if (info != null)
            {
                var loadBtn = SecondaryBtn(slotCard.transform, LocalizationData.T("加载"), Cyan, 10, 28);
                loadBtn.GetComponent<LayoutElement>().preferredWidth = 50;
                loadBtn.GetComponent<Button>().onClick.AddListener(() => {
                    GameManager.Instance?.LoadGame(s);
                    StartCoroutine(CloseOverlayAnimated(_recOvl));
                });

                var delBtn = DangerBtn(slotCard.transform, LocalizationData.T("删除"), 10, 28);
                delBtn.GetComponent<LayoutElement>().preferredWidth = 50;
                delBtn.GetComponent<Button>().onClick.AddListener(() => {
                    SaveSystem.Instance?.DeleteSave(s);
                    _recBuilt = false;
                    SyncRecord();
                });
            }
            Spacer(rvl, 3);
        }

        Spacer(rvl, 12);

        // === Data Management ===
        SectionDivider(rvl, LocalizationData.T("数据管理"), Bright);
        Spacer(rvl, SpaceTiny);

        var exportBtn = SecondaryBtn(_recList, "↑ 导出存档", Cyan, 13, 42);
        exportBtn.GetComponent<Button>().onClick.AddListener(() => {
            ShowExportPanel();
        });

        Spacer(rvl, 4);

        var importBtn = SecondaryBtn(_recList, "↓ 导入存档", Purp, 13, 42);
        importBtn.GetComponent<Button>().onClick.AddListener(() => {
            ShowImportPanel();
        });

        Spacer(rvl, SpaceSmall);

        var resetProgressBtn = DangerBtn(_recList, "✕ " + LocalizationData.T("重置进度"), 13, 42);
        resetProgressBtn.GetComponent<Button>().onClick.AddListener(() => {
            ShowResetConfirm(LocalizationData.T("重置进度"), LocalizationData.T("将清除所有历史统计数据，此操作不可撤销。"), () => {
                MetaProgressSystem.Instance?.ResetProgress();
                _recBuilt = false;
                SyncRecord();
            });
        });
    }

    void ShowExportPanel()
    {
        var exportOvl = new GameObject("ExportOvl", typeof(RectTransform), typeof(Image));
        exportOvl.transform.SetParent(_root, false);
        var rt = exportOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = exportOvl.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.92f);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(exportOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.1f, 0.25f);
        contentRT.anchorMax = new Vector2(0.9f, 0.75f);

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.padding = new RectOffset(12, 12, 12, 12);

        var title = TxtGo(content.transform, "↑ 导出存档", 18, Gold);
        title.fontStyle = FontStyle.Bold;

        var hint = TxtGo(content.transform, "长按复制下方文本，粘贴到备忘录保存", 11, Dim);
        hint.alignment = TextAnchor.MiddleCenter;

        string exportData = ExportSaveData();

        var textArea = new GameObject("TextArea", typeof(RectTransform), typeof(Text));
        textArea.transform.SetParent(content.transform, false);
        var taRT = textArea.GetComponent<RectTransform>();
        taRT.sizeDelta = new Vector2(0, 120);
        textArea.AddComponent<LayoutElement>().preferredHeight = 120;

        var taTxt = textArea.GetComponent<Text>();
        taTxt.font = F();
        taTxt.fontSize = 9;
        taTxt.color = Cyan;
        taTxt.alignment = TextAnchor.UpperLeft;
        taTxt.text = exportData;

        var taImg = textArea.AddComponent<Image>();
        taImg.color = new Color(0.04f, 0.03f, 0.06f);

        var btn = BtnGo(content.transform, "关闭", 13, Dim, 40);
        btn.GetComponent<Button>().onClick.AddListener(() => Destroy(exportOvl));
    }

    string ExportSaveData()
    {
        try
        {
            GameManager.Instance?.Save?.AutoSave();
            string classic = PlayerPrefs.GetString("pt_save_classic", "");
            string shortSave = PlayerPrefs.GetString("pt_save_short", "");
            string expedition = PlayerPrefs.GetString("pt_save_expedition", "");
            string achievements = PlayerPrefs.GetString("pt_achievements", "");
            string endings = PlayerPrefs.GetString("pt_endings", "");
            string affinity = PlayerPrefs.GetString("pt_affinity", "");

            var exportData = new
            {
                classic = classic,
                @short = shortSave,
                expedition = expedition,
                achievements = achievements,
                endings = endings,
                affinity = affinity,
                version = 1,
                timestamp = System.DateTime.Now.Ticks
            };

            string json = JsonUtility.ToJson(exportData);
            byte[] bytes = System.Text.Encoding.UTF8.GetBytes(json);
            return Convert.ToBase64String(bytes);
        }
        catch (Exception e)
        {
            return "导出失败: " + e.Message;
        }
    }

    void ShowImportPanel()
    {
        var importOvl = new GameObject("ImportOvl", typeof(RectTransform), typeof(Image));
        importOvl.transform.SetParent(_root, false);
        var rt = importOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = importOvl.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.92f);

        var content = new GameObject("Content", typeof(RectTransform), typeof(VerticalLayoutGroup));
        content.transform.SetParent(importOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.1f, 0.25f);
        contentRT.anchorMax = new Vector2(0.9f, 0.75f);

        var vlg = content.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 12;
        vlg.padding = new RectOffset(12, 12, 12, 12);

        var title = TxtGo(content.transform, "↓ 导入存档", 18, Gold);
        title.fontStyle = FontStyle.Bold;

        var hint = TxtGo(content.transform, "粘贴之前导出的存档文本", 11, Dim);
        hint.alignment = TextAnchor.MiddleCenter;

        var inputGo = new GameObject("InputArea", typeof(RectTransform), typeof(Image));
        inputGo.transform.SetParent(content.transform, false);
        var inputRT = inputGo.GetComponent<RectTransform>();
        inputRT.sizeDelta = new Vector2(0, 120);
        inputGo.AddComponent<LayoutElement>().preferredHeight = 120;
        var inputImg = inputGo.GetComponent<Image>();
        inputImg.color = new Color(0.04f, 0.03f, 0.06f);

        var inputText = new GameObject("InputText", typeof(RectTransform), typeof(Text));
        inputText.transform.SetParent(inputGo.transform, false);
        var inputTxtRT = inputText.GetComponent<RectTransform>();
        inputTxtRT.anchorMin = new Vector2(0.03f, 0.03f);
        inputTxtRT.anchorMax = new Vector2(0.97f, 0.97f);
        var inputTxt = inputText.GetComponent<Text>();
        inputTxt.font = F();
        inputTxt.fontSize = 9;
        inputTxt.color = Color.red;
        inputTxt.alignment = TextAnchor.UpperLeft;
        inputTxt.text = "";

        var placeholder = TxtGo(inputGo.transform, "粘贴存档文本...", 9, Dim);
        placeholder.alignment = TextAnchor.UpperLeft;
        var phRT = placeholder.GetComponent<RectTransform>();
        phRT.anchorMin = new Vector2(0.03f, 0.03f);
        phRT.anchorMax = new Vector2(0.97f, 0.97f);

        inputTxt.gameObject.AddComponent<InputField>().textComponent = inputTxt;

        var btnHL = new GameObject("BtnHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnHL.transform.SetParent(content.transform, false);
        var btnHLRT = btnHL.GetComponent<RectTransform>();
        btnHLRT.sizeDelta = new Vector2(0, 40);
        btnHL.AddComponent<LayoutElement>().preferredHeight = 40;
        var btnHLG = btnHL.GetComponent<HorizontalLayoutGroup>();
        btnHLG.spacing = 12;
        btnHLG.childAlignment = TextAnchor.MiddleCenter;

        Text finalInputTxt = inputTxt;
        var confirmBtn = BtnGo(btnHL.transform, "确认导入", 13, Cyan, 80);
        confirmBtn.GetComponent<Button>().onClick.AddListener(() => {
            bool success = DoImportSaveData(finalInputTxt.text);
            if (success)
            {
                ShowMessage("存档已导入，重启游戏生效");
            }
            else
            {
                ShowMessage("导入失败：存档格式无效");
            }
            Destroy(importOvl);
        });

        var cancelBtn = BtnGo(btnHL.transform, "取消", 13, Dim, 60);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => Destroy(importOvl));
    }

    void ShowMessage(string msg)
    {
        var msgGo = new GameObject("Message", typeof(RectTransform), typeof(Image), typeof(Text));
        msgGo.transform.SetParent(_root, false);
        var msgRT = msgGo.GetComponent<RectTransform>();
        msgRT.anchorMin = new Vector2(0.3f, 0.45f);
        msgRT.anchorMax = new Vector2(0.7f, 0.55f);
        msgGo.GetComponent<Image>().color = new Color(0.1f, 0.08f, 0.16f);
        
        var txt = msgGo.GetComponent<Text>();
        txt.font = F();
        txt.fontSize = 14;
        txt.color = Cyan;
        txt.alignment = TextAnchor.MiddleCenter;
        txt.text = msg;

        Destroy(msgGo, 2f);
    }

    bool DoImportSaveData(string data)
    {
        try
        {
            if (string.IsNullOrEmpty(data)) return false;

            byte[] bytes = Convert.FromBase64String(data);
            string json = System.Text.Encoding.UTF8.GetString(bytes);

            var importData = JsonUtility.FromJson<ImportSaveData>(json);
            if (importData == null) return false;

            if (!string.IsNullOrEmpty(importData.classic))
                PlayerPrefs.SetString("pt_save_classic", importData.classic);
            if (!string.IsNullOrEmpty(importData.@short))
                PlayerPrefs.SetString("pt_save_short", importData.@short);
            if (!string.IsNullOrEmpty(importData.expedition))
                PlayerPrefs.SetString("pt_save_expedition", importData.expedition);
            if (!string.IsNullOrEmpty(importData.achievements))
                PlayerPrefs.SetString("pt_achievements", importData.achievements);
            if (!string.IsNullOrEmpty(importData.endings))
                PlayerPrefs.SetString("pt_endings", importData.endings);
            if (!string.IsNullOrEmpty(importData.affinity))
                PlayerPrefs.SetString("pt_affinity", importData.affinity);

            PlayerPrefs.Save();
            return true;
        }
        catch
        {
            return false;
        }
    }

    [System.Serializable]
    class ImportSaveData
    {
        public string classic;
        public string @short;
        public string expedition;
        public string achievements;
        public string endings;
        public string affinity;
    }

    float CycleVolume(float current, float[] steps)
    {
        for (int i = 0; i < steps.Length; i++)
        {
            if (Mathf.Abs(current - steps[i]) < 0.05f)
                return steps[(i + 1) % steps.Length];
        }
        return steps[0];
    }

    void ShowResetConfirm(string title, string message, System.Action onConfirm)
    {
        var bg = Panel("ResetConfirmBG", Mask70);
        var box = new GameObject("ResetConfirmBox", typeof(RectTransform), typeof(Image));
        box.transform.SetParent(bg.transform, false);
        var rt = box.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(300, 200);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        box.GetComponent<Image>().color = DarkItemBg;
        var bvl = box.AddComponent<VerticalLayoutGroup>();
        bvl.padding = new RectOffset(20, 20, 20, 15);
        bvl.spacing = 10;
        bvl.childAlignment = TextAnchor.UpperCenter;
        bvl.childForceExpandWidth = true;
        bvl.childForceExpandHeight = false;

        Txt(box.transform, title, 18, SoftRed, 28);
        Txt(box.transform, message, 12, Bright, 40);
        Spacer(bvl, 10);

        var btnRow = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(box.transform, false);
        btnRow.AddComponent<LayoutElement>().preferredHeight = 42;
        var hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 10; hlg.childForceExpandWidth = true;

        var confirmBtn = BtnGo(btnRow.transform, "确认", 13, SoftRed, 38);
        confirmBtn.GetComponent<Button>().onClick.AddListener(() => {
            onConfirm?.Invoke();
            Destroy(bg);
        });
        var cancelBtn = BtnGo(btnRow.transform, LocalizationData.T("取消"), 13, Dim, 38);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => Destroy(bg));
    }

    void RebuildAllForLanguage()
    {
        // Destroy all overlay panels so they rebuild with new language
        if (_setOvl != null) { Destroy(_setOvl); _setOvl = null; }
        if (_recOvl != null) { Destroy(_recOvl); _recOvl = null; }
        if (_bestOvl != null) { Destroy(_bestOvl); _bestOvl = null; _bestPanelBuilt = false; }
        if (_achOvl != null) { Destroy(_achOvl); _achOvl = null; _achPanelBuilt = false; }
        if (_shopOvl != null) { Destroy(_shopOvl); _shopOvl = null; _shopPanelBuilt = false; }
        if (_rankOvl != null) { Destroy(_rankOvl); _rankOvl = null; _rankBuilt = false; }
        if (_pollSkillOvl != null) { Destroy(_pollSkillOvl); _pollSkillOvl = null; _pollSkillBuilt = false; }
        if (_bondOvl != null) { Destroy(_bondOvl); _bondOvl = null; }
        if (_anchorOvl != null) { Destroy(_anchorOvl); _anchorOvl = null; }
        if (_buildOvl != null) { Destroy(_buildOvl); _buildOvl = null; }
        if (_menuOvl != null) { Destroy(_menuOvl); _menuOvl = null; }

        // Rebuild main menu if on home screen
        if (_menuPanel != null)
        {
            Destroy(_menuPanel);
            _menuPanel = null;
            if (_avatarBtn != null) { Destroy(_avatarBtn); _avatarBtn = null; }
            _menuSyncTime = 0;
            BuildMenu();
            SA(_menuPanel, true);
            if (_avatarBtn) _avatarBtn.transform.SetAsLastSibling();
        }

        // Reopen settings
        ShowSettingsPanel();
    }

    void ExitGame()
    {
        ShowExitConfirmDialog();
    }

    void ShowExitConfirmDialog()
    {
        var bgPanel = Panel("ExitConfirmBG", Mask70);
        
        var confirmPanel = new GameObject("ExitConfirmPanel", typeof(RectTransform), typeof(Image));
        confirmPanel.transform.SetParent(bgPanel.transform, false);
        var rt = confirmPanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(320, 280);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        confirmPanel.GetComponent<Image>().color = DarkItemBg;
        
        var vl = confirmPanel.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(20, 20, 25, 20);
        vl.spacing = 10;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        
        Txt(confirmPanel.transform, "退出游戏", 22, Cyan, 32);
        Spacer(vl, 5);
        Txt(confirmPanel.transform, "确定要退出游戏吗？", 13, Bright, 20);
        
        Spacer(vl, 20);
        
        var btnRow = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(confirmPanel.transform, false);
        btnRow.AddComponent<LayoutElement>().preferredHeight = 48;
        var hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childForceExpandWidth = true;
        
        var saveBtn = BtnGo(btnRow.transform, "保存游戏", 13, Gold, 42);
        saveBtn.GetComponent<Button>().onClick.AddListener(() => {
            try
            {
                SaveSystem.Instance?.Save(0);
                Destroy(bgPanel);
                SA(_setOvl, false);
                SA(_menuOvl, false);
                CompleteGameSystem.Instance?.ReturnToMenu();
            }
            catch (System.Exception ex) { Debug.LogError($"[ExitSave] {ex}"); }
        });

        var noSaveBtn = BtnGo(btnRow.transform, "不保存", 13, SoftRed, 42);
        noSaveBtn.GetComponent<Button>().onClick.AddListener(() => {
            try
            {
                SaveSystem.Instance?.DeleteSave(0);
                Destroy(bgPanel);
                SA(_setOvl, false);
                SA(_menuOvl, false);
                _menuSyncTime = 0;
                CompleteGameSystem.Instance?.ReturnToMenu();
            }
            catch (System.Exception ex) { Debug.LogError($"[ExitNoSave] {ex}"); }
        });
        
        var cancelBtn = BtnGo(btnRow.transform, "取消", 13, Dim, 42);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => {
            Destroy(bgPanel);
        });
        
        Spacer(vl, 10);
    }

    public void ShowFleeConfirm(int fleeCost)
    {
        var bgPanel = Panel("FleeConfirmBG", Mask70);

        var confirmPanel = new GameObject("FleeConfirmPanel", typeof(RectTransform), typeof(Image));
        confirmPanel.transform.SetParent(bgPanel.transform, false);
        var rt = confirmPanel.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        confirmPanel.GetComponent<Image>().color = DarkItemBg;

        var vl = confirmPanel.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(20, 20, 25, 20);
        vl.spacing = 10;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        var csf = confirmPanel.AddComponent<ContentSizeFitter>();
        csf.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        rt.sizeDelta = new Vector2(320, 0);

        Txt(confirmPanel.transform, "逃跑确认", 22, Cyan, 32);
        Spacer(vl, 5);

        var p = GameManager.Instance.Player;
        bool willDie = p.hp <= fleeCost;

        Txt(confirmPanel.transform, $"逃跑需付出代价：", 14, new Color(1f, 0.55f, 0f), 20);
        Txt(confirmPanel.transform, $"HP -{fleeCost}", 20, new Color(1f, 0f, 0.43f), 28);
        Txt(confirmPanel.transform, $"(当前HP: {p.hp}/{p.maxHp})", 12, Dim, 16);

        if (willDie)
        {
            Spacer(vl, 5);
            var warnTxt = Txt(confirmPanel.transform, "⚠ 当前HP不足，逃跑将直接死亡！", 14, LightRed, 22);
            warnTxt.GetComponent<Text>().fontStyle = FontStyle.Bold;
            warnTxt.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        }

        Spacer(vl, 8);

        var bonus1Go = Txt(confirmPanel.transform, "⟐ 污染保护: 免除战后污染增长", 12, new Color(0f, 0.78f, 1f), 18);
        bonus1Go.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        int roundsEP = Mathf.FloorToInt(CompleteGameSystem.Instance?.CombatRound * 3 ?? 0);
        if (roundsEP > 0)
        {
            var bonus2Go = Txt(confirmPanel.transform, $"⟐ 回合补偿: +{roundsEP}EP", 12, new Color(0f, 1f, 0.816f), 18);
            bonus2Go.GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        }

        Spacer(vl, 15);

        var btnRow = new GameObject("BtnRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        btnRow.transform.SetParent(confirmPanel.transform, false);
        btnRow.AddComponent<LayoutElement>().preferredHeight = 48;
        var hlg = btnRow.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = 8;
        hlg.childForceExpandWidth = true;

        string confirmLabel = willDie ? "赴死逃跑" : "确定";
        Color confirmColor = willDie ? LightRed : new Color(0f, 0.85f, 0.45f);
        var confirmBtn = BtnGo(btnRow.transform, confirmLabel, 13, confirmColor, 42);
        confirmBtn.GetComponent<Button>().onClick.AddListener(() => {
            Destroy(bgPanel);
            CompleteGameSystem.Instance?.ExecuteFlee();
        });

        var cancelBtn = BtnGo(btnRow.transform, "取消", 13, Dim, 42);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => {
            Destroy(bgPanel);
            CompleteGameSystem.Instance?.CancelFlee();
        });

        Spacer(vl, 10);
    }

    public void HideFleeConfirm()
    {
        var fleePanel = GameObject.Find("FleeConfirmBG");
        if (fleePanel != null) Destroy(fleePanel);
    }

    public void ShowFormReplaceDialog(string newFormId, string newFormName, List<string> replaceableForms, List<int> replaceableIndices, Action<int> onSelect)
    {
        var bgPanel = Panel("FormReplaceBG", new Color(0, 0, 0, 0.85f));
        
        var confirmPanel = new GameObject("FormReplacePanel", typeof(RectTransform), typeof(Image));
        confirmPanel.transform.SetParent(bgPanel.transform, false);
        var rt = confirmPanel.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(340, 420);
        rt.anchorMin = new Vector2(0.5f, 0.5f);
        rt.anchorMax = new Vector2(0.5f, 0.5f);
        rt.pivot = new Vector2(0.5f, 0.5f);
        rt.anchoredPosition = Vector2.zero;
        confirmPanel.GetComponent<Image>().color = DarkItemBg;
        
        var vl = confirmPanel.AddComponent<VerticalLayoutGroup>();
        vl.padding = new RectOffset(16, 16, 20, 16);
        vl.spacing = 12;
        vl.childAlignment = TextAnchor.UpperCenter;
        vl.childForceExpandWidth = true;
        vl.childForceExpandHeight = false;
        
        // 标题
        Txt(confirmPanel.transform, "选择要覆盖的形态", 20, new Color(1f, 0.9f, 0.35f), 28);
        
        // 新形态信息
        var newInfo = new GameObject("NewFormInfo", typeof(RectTransform), typeof(Image));
        newInfo.transform.SetParent(vl.transform, false);
        newInfo.GetComponent<Image>().color = new Color(0.05f, 0.12f, 0.15f);
        var niLE = newInfo.AddComponent<LayoutElement>();
        niLE.preferredHeight = 60;
        var niVL = newInfo.AddComponent<VerticalLayoutGroup>();
        niVL.padding = new RectOffset(12, 12, 8, 8);
        niVL.childAlignment = TextAnchor.MiddleCenter;
        
        Txt(newInfo.transform, $"★ 新形态：{newFormName}", 15, new Color(0f, 1f, 0.816f), 22);
        Txt(newInfo.transform, "选择一个形态槽位进行覆盖", 12, Dim, 18);
        
        // 可替换形态列表
        var listContainer = new GameObject("SlotList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listContainer.transform.SetParent(vl.transform, false);
        var listVL = listContainer.GetComponent<VerticalLayoutGroup>();
        listVL.spacing = 8;
        listVL.childAlignment = TextAnchor.UpperCenter;
        
        for (int i = 0; i < replaceableForms.Count; i++)
        {
            string formId = replaceableForms[i];
            int originalIndex = replaceableIndices[i];
            
            var card = new GameObject("SlotCard", typeof(RectTransform), typeof(Image), typeof(Button));
            card.transform.SetParent(listContainer.transform, false);
            card.GetComponent<Image>().color = new Color(0.12f, 0.10f, 0.20f);
            var cardLE = card.AddComponent<LayoutElement>();
            cardLE.preferredHeight = 56;
            
            var cardHL = card.AddComponent<HorizontalLayoutGroup>();
            cardHL.padding = new RectOffset(12, 12, 8, 8);
            cardHL.spacing = 12;
            cardHL.childAlignment = TextAnchor.MiddleCenter;
            
            // 图标区域
            var iconGo = Txt(card.transform, "◎", 24, new Color(0.95f, 0.9f, 0.8f), 32);
            iconGo.GetComponent<RectTransform>().sizeDelta = new Vector2(40, 40);
            
            // 信息区域
            var infoVL = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
            infoVL.transform.SetParent(card.transform, false);
            var ivl = infoVL.GetComponent<VerticalLayoutGroup>();
            ivl.spacing = 2;
            ivl.childAlignment = TextAnchor.MiddleLeft;
            
            string formName = CompleteGameSystem.Instance?.GetDisplayName(formId) ?? formId;
            var nameTxtGo = Txt(infoVL.transform, formName, 14, Color.white, 20);
            nameTxtGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            
            // 提示文字
            var hintTxtGo = Txt(infoVL.transform, "点击覆盖此槽位", 11, Dim, 16);
            hintTxtGo.GetComponent<Text>().alignment = TextAnchor.MiddleLeft;
            
            // 点击事件
            int idx = originalIndex;
            card.GetComponent<Button>().onClick.AddListener(() => {
                Destroy(bgPanel);
                onSelect?.Invoke(idx);
                CompleteGameSystem.Instance?.OnFormReplaceSelected(idx);
            });
        }
        
        // 底部提示
        Txt(confirmPanel.transform, "► 本命形态无法被覆盖", 12, new Color(0.6f, 0.6f, 0.8f), 18);
    }

    Text[] _fragChoiceLabels = new Text[3];
    Text[] _fragChoiceDescs = new Text[3];
    Text[] _fragChoiceIcons = new Text[3];
    GameObject[] _fragChoiceGlow = new GameObject[3];
    Text[] _fragChoiceTags = new Text[3];
    Text[] _fragChoiceStars = new Text[3];
    Text[] _fragChoiceProgress = new Text[3];
    RectTransform[] _fragChoiceProgressFill = new RectTransform[3];
    Image[] _fragChoiceProgressFillImg = new Image[3];
    Image[] _fragChoiceTagBg = new Image[3];
    GameObject[] _fragChoiceCards = new GameObject[3];
    bool _fragChoiceBuilt;

    // 碎片卡片自定义边框
    Image[] _fragBorderTop = new Image[3];
    Image[] _fragBorderBottom = new Image[3];
    Image[] _fragBorderLeft = new Image[3];
    Image[] _fragBorderRight = new Image[3];
    Image[] _fragCornerTL = new Image[3];
    Image[] _fragCornerTR = new Image[3];
    Image[] _fragCornerBL = new Image[3];
    Image[] _fragCornerBR = new Image[3];
    Image[] _fragBorderGlow = new Image[3];
    Image[] _fragInnerLine = new Image[3];
    float _fragBorderAnimTime;


}
