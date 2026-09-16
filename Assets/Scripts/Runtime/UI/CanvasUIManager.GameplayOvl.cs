using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
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

        var (aggCard, avl) = InfoCard(row.transform, new Color(0.2f,0.06f,0.06f,0.9f), 200);
        _alAggN = Txt(avl, "", 15, BrightRed, 24).GetComponent<Text>();
        _alAggD = Txt(avl, "", 11, Bright, 80).GetComponent<Text>();
        _alAggD.alignment = TextAnchor.UpperCenter;
        var abtn = DangerBtn(avl.transform, "选择激进", 13, 38);
        abtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ChooseAltar(true));

        var (conCard, cvl) = InfoCard(row.transform, new Color(0.06f,0.06f,0.2f,0.9f), 200);
        _alConN = Txt(cvl, "", 15, Cyan, 24).GetComponent<Text>();
        _alConD = Txt(cvl, "", 11, Bright, 80).GetComponent<Text>();
        _alConD.alignment = TextAnchor.UpperCenter;
        var cbtn = PrimaryBtn(cvl.transform, "选择保守", 13, 38);
        cbtn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ChooseAltar(false));

        _altarOvl.SetActive(false);
    }

    void BuildCollapse()
    {
        _collapseOvl = Panel("Collapse", new Color(0.15f,0,0,0.8f));
        var vl = AddVL(_collapseOvl, 25, 10);
        Spacer(vl, 220);
        Txt(vl, "⚠ 污染崩溃 ⚠", 26, BrightRed, 40);
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

        // Main card — 居中紧凑, 不再撑满屏幕
        var card = new GameObject("Card", typeof(RectTransform), typeof(Image), typeof(Outline));
        card.transform.SetParent(rt, false);
        card.GetComponent<Image>().color = new Color(0.06f,0.04f,0.14f,0.95f);
        card.GetComponent<Outline>().effectColor = new Color(0.71f,0.33f,1f,0.25f);
        card.GetComponent<Outline>().effectDistance = new Vector2(0.5f,0.5f);
        var cardRT = card.GetComponent<RectTransform>();
        // 居中锚点 + pivot — ContentSizeFitter 可自由控制高度, 不被拉伸
        cardRT.anchorMin = new Vector2(0.08f, 0.5f);
        cardRT.anchorMax = new Vector2(0.92f, 0.5f);
        cardRT.pivot = new Vector2(0.5f, 0.5f);
        cardRT.anchoredPosition = Vector2.zero;
        // 先给一个合理的初始高度, ContentSizeFitter 会在布局后自动校正
        cardRT.sizeDelta = new Vector2(0, 480);
        // 让卡片根据子元素 preferredHeight 自动收缩, 不撑满锚点范围
        var csf = card.AddComponent<UnityEngine.UI.ContentSizeFitter>();
        csf.verticalFit = UnityEngine.UI.ContentSizeFitter.FitMode.PreferredSize;
        csf.horizontalFit = UnityEngine.UI.ContentSizeFitter.FitMode.Unconstrained;

        var cardVL = AddVL(card, 14, 6);
        cardVL.childControlHeight = false;
        cardVL.childControlWidth = true;
        cardVL.childForceExpandHeight = false;   // 不强制均分, 让卡片按 preferredHeight 走
        cardVL.childForceExpandWidth = true;

        // Title area — 固定高度保证不被挤
        var titleArea = new GameObject("TitleArea", typeof(RectTransform), typeof(LayoutElement));
        titleArea.transform.SetParent(cardVL.transform, false);
        titleArea.GetComponent<LayoutElement>().preferredHeight = 88;
        var titleVL = AddVL(titleArea, 0, 2);
        titleVL.childForceExpandHeight = false;
        titleVL.childForceExpandWidth = true;
        titleVL.childAlignment = TextAnchor.MiddleCenter;

        Txt(titleVL, "【意识交涉】", 28, Purp, 36);
        _possEnemyName = Txt(titleVL, "", 18, Gold, 28).GetComponent<Text>();
        _possRate = Txt(titleVL, "", 18, Purp, 28).GetComponent<Text>();

        // 4 strategy cards — 用 childForceExpandHeight 均分剩余空间
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
            optGo.AddComponent<LayoutElement>().preferredHeight = 72;

            var optVL = AddVL(optGo, 8, 2);
            optVL.childForceExpandHeight = false;
            optVL.childForceExpandWidth = true;
            optVL.childAlignment = TextAnchor.MiddleCenter;

            var lbl = Txt(optVL, defaultLabels[i], 20, optColors[i], 24);
            var lblText = lbl.GetComponent<Text>();
            lblText.fontStyle = FontStyle.Bold;
            _possOptLabels[i] = lblText;

            var desc = Txt(optVL, defaultDescs[i], 12, Dim, 34);
            var descText = desc.GetComponent<Text>();
            descText.alignment = TextAnchor.MiddleCenter;
            descText.color = new Color(0.85f, 0.8f, 0.9f);
            descText.lineSpacing = 1.0f;
            descText.horizontalOverflow = HorizontalWrapMode.Wrap;
            descText.verticalOverflow = VerticalWrapMode.Overflow;
            _possOptDescs[i] = descText;

            var btn = optGo.GetComponent<Button>();
            btn.targetGraphic = optGo.GetComponent<Image>();
            var colors = btn.colors;
            colors.normalColor = new Color(0.10f,0.08f,0.18f,0.9f);
            colors.highlightedColor = new Color(0.14f,0.10f,0.24f);
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

        // Cancel button — 固定底部, 不参与均分
        var cancelArea = new GameObject("CancelArea", typeof(RectTransform), typeof(LayoutElement));
        cancelArea.transform.SetParent(cardVL.transform, false);
        cancelArea.GetComponent<LayoutElement>().preferredHeight = 50;
        var cancelVL = AddVL(cancelArea, 0, 0);
        cancelVL.childForceExpandHeight = false;
        cancelVL.childForceExpandWidth = true;
        cancelVL.childAlignment = TextAnchor.MiddleCenter;

        var cancelBtn = SecondaryBtn(cancelVL.transform, "✕ 取消", Dim, 15, 40);
        cancelBtn.GetComponent<Button>().onClick.AddListener(() => _possessOvl.SetActive(false));

        _possessOvl.SetActive(false);
    }

    bool _shownPossessGuide;

    void ShowPossessPanel()
    {
        var gs = CompleteGameSystem.Instance;
        if (gs == null || gs.CurrentEnemy == null) return;
        gs.DismissSoftHint("possess");

        // 形态破裂检查 - 无法寄生
        if (GameManager.Instance?.Player != null && GameManager.Instance.Player.formBroken)
        {
            gs.AddCombatLog("♥ 形态破裂，无法寄生");
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
        bool tutDog = enemy.id == "dog" && (GameManager.Instance?.CurrentFloor ?? 0) == 1;
        if (tutDog) baseRate = 1f;

        ST(_possEnemyName, $"目标: {enemy.name}  HP:{enemy.hp}/{enemy.maxHp}");
        ST(_possRate, $"基础成功率: {Mathf.RoundToInt(baseRate * 100)}%");

        // 计算实际污染消耗（原版逻辑：基础10 + 额外 + 楼层乘数 + 特性减免）
        int CalcPollutionPreview(int basePol, int extraPol)
        {
            int totalPol = basePol + extraPol;
            // 楼层乘数
            var curveP = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, GameManager.Instance.CurrentFloor, GameManager.Instance.CurrentStage);
            totalPol = Mathf.RoundToInt(totalPol * curveP.possessPollMult);
            // 特性减免
            float polResist = 0f;
            if (GameManager.Instance.Player.traits != null)
            {
                foreach (var trait in GameManager.Instance.Player.traits)
                {
                    if (trait == "净化") polResist += 0.5f;
                    else if (trait == "寄生强化") polResist += 0.15f;
                }
            }
            if (polResist > 0) totalPol = Mathf.FloorToInt(totalPol * (1 - polResist));
            return Mathf.Max(0, totalPol);
        }

        // 获取怪物类型倾向
        string GetMonsterNegBias(string type)
        {
            var biases = new Dictionary<string, string>
            {
                {"rat", "resonate"}, {"roach", "threat"}, {"slime", "resonate"}, {"dog", "resonate"},
                {"gecko", "trade"}, {"drone", "trade"}, {"wolf", "threat"}, {"spider", "trade"},
                {"bat", "resonate"}, {"wasp", "threat"}, {"guard", "trade"}, {"vine", "resonate"},
                {"larva", "threat"}, {"mantis", "threat"}, {"beetle", "trade"}, {"worm", "resonate"},
                {"moth", "resonate"}, {"scorpion", "threat"}, {"hydra", "threat"}
            };
            return biases.TryGetValue(type, out var bias) ? bias : null;
        }

        string bias = !tutDog ? GetMonsterNegBias(enemy.id) : null;
        float biasBonus = bias != null ? 0.15f : 0f;

        // Update each option with actual rates and pollution
        string[] ids = {"intimidate","trade","resonance","join"};
        float[] mods = {-0.2f, 0.2f, 0.1f, 0f};
        bool[] guaranteed = {false, false, false, true};
        string[] labels = {"威压","交易","共鸣","加入"};
        
        int polThreat = CalcPollutionPreview(10, 0);
        int polTrade = CalcPollutionPreview(10, 10);
        int polResonate = CalcPollutionPreview(10, -5);
        int polJoin = CalcPollutionPreview(10, 15);

        float threatRate = Mathf.Max(1, Mathf.RoundToInt((baseRate - 0.2f + (bias == "threat" ? biasBonus : 0f)) * 100));
        float tradeRate = Mathf.Min(95, Mathf.RoundToInt((baseRate + 0.2f + (bias == "trade" ? biasBonus : 0f)) * 100));
        float resonateRate = Mathf.Min(95, Mathf.RoundToInt((baseRate + 0.1f + (bias == "resonate" ? biasBonus : 0f)) * 100));

        string[] descs = {
            $"成功率 {threatRate}%\n成功后满血继承\n失败: 目标ATK+2\n污染+{polThreat}",
            $"成功率 {tradeRate}%\n污染+{polTrade}\n高效但有代价{(bias == "trade" ? "\n✨对此类有效" : "")}",
            $"成功率 {resonateRate}%\n进化点+2\n失败也得EP\n污染+{polResonate}{(bias == "resonate" ? "\n✨对此类有效" : "")}",
            $"成功率 100%\n污染+{polJoin}\n需70%+污染才出现"
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
            // Hide "join" if pollution < 70 (原版逻辑)
            SA(_possOptBtns[3], (GameManager.Instance?.Player?.pollution ?? 0f) >= 70f);
        }

        if (!_shownPossessGuide && !tutDog)
        {
            _shownPossessGuide = true;
            gs.AddCombatLog("<color=#ffcc00>◆ 附身策略：威压(高风险满血) / 交易(高率+污染) / 共鸣(稳健+EP)</color>");
        }

        _possessOvl.SetActive(true);
        _possessOvl.transform.SetAsLastSibling();
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
        Txt(vl, "⚠ 宿主死亡 ⚠", 26, BrightRed, 40);
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
        Txt(vl, "☠ 意识崩坏", 28, BrightRed, 40);
        Txt(vl, "检测到记忆锚点信号...", 14, Dim, 24);
        Spacer(vl, 20);

        var infoGo = Txt(vl, "", 15, Cyan, 80);
        _rollbackInfoTxt = infoGo.GetComponent<Text>();
        _rollbackInfoTxt.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, SpaceMed);

        var rollBtn = PrimaryBtn(vl.transform, "⛓ 回滚至锚点", 16, 56);
        _rollbackBtn = rollBtn.GetComponent<Button>();
        _rollbackBtn.onClick.AddListener(() => {
            _rollbackBuilt = false;
            CompleteGameSystem.Instance?.ChooseDeathRollback(true);
        });

        Spacer(vl, SpaceMed);

        var giveUpBtn = DangerBtn(vl.transform, "放弃挣扎...", 14, 48);
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
        Txt(vl, "↩ 卡槽已满", 24, Gold, 36);
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

        Spacer(vl, SpaceMed);
        var cancelBtn = DangerBtn(vl.transform, "放弃附身", 14, 48);
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
    bool _storyFadeAnimating;
    float _storyFadeTime;
    string _storyPendingTitle;
    string _storyPendingBody;
    Text _storyTypewriterText;
    int _storyTypewriterChar;
    float _storyTypewriterDelay;
    string _storyTypewriterFull;
    bool _storyTypewriterActive;

    // Mini-map refs
    Image[,] _miniMapCells;

    void BuildMiniMap(RectTransform parent)
    {
        // MiniMap 地形指示已移除 — 用户偏好背景图完全可见,无地形标记
    }

    void SyncMiniMap(CompleteGameSystem gs)
    {
        // MiniMap 地形指示已移除 — no-op
    }

    // Menu panel
    GameObject _menuOvl;

    Text _solidifyBtnTxt;
    Image _solidifyBtnImg;
    GameObject _evoRedDot;
    GameObject _hudMenuRedDot;

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
        while (_evoRedDot != null && _evoRedDot.activeSelf)
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
        _evoRedDotCoroutine = null;
    }
}
