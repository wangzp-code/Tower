using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    // ========== GAME OVER ==========
    // Report refs
    Text _rptTitle, _rptSubtitle, _rptRank, _rptFloor, _rptPossess, _rptKills;
    Text _rptTime, _rptHost, _rptPollution, _rptForm, _rptScore, _rptEcho, _rptQuote;
    Text _rptBuildLegacy, _rptBuildTier, _rptBuildForms;
    GameObject _rptShareBtn;
    bool _rptBuilt;

    void BuildOver()
    {
        _overPanel = Panel("Over", new Color(0.05f, 0.03f, 0.10f));
        var rt = _overPanel.GetComponent<RectTransform>();

        // 背景图
        var bgTex = LoadTex("UI/death_report_bg");
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
        gtRI.texture = MakeGradientTex(64, new Color(0.05f, 0.03f, 0.10f, 0.9f), Color.clear);
        gtRI.raycastTarget = false;
        var gtRT = gradTop.GetComponent<RectTransform>();
        gtRT.anchorMin = new Vector2(0, 0.75f); gtRT.anchorMax = Vector2.one;
        gtRT.offsetMin = Vector2.zero; gtRT.offsetMax = Vector2.zero;

        // 底部渐变遮罩
        var gradBot = new GameObject("GradBot", typeof(RectTransform), typeof(RawImage));
        gradBot.transform.SetParent(rt, false);
        var gbRI = gradBot.GetComponent<RawImage>();
        gbRI.texture = MakeGradientTex(64, Color.clear, new Color(0.05f, 0.03f, 0.10f, 0.95f));
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

        // 构筑回顾区段
        var sep3 = new GameObject("Sep3", typeof(RectTransform), typeof(Image));
        sep3.transform.SetParent(vl.transform, false);
        sep3.AddComponent<LayoutElement>().preferredHeight = 1;
        sep3.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);

        Txt(vl, "⊞ 构筑回顾", 13, Cyan, 22).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;
        Spacer(vl, 4);
        _rptBuildTier = MakeReportRow(vl, "污染阶段");
        _rptBuildForms = MakeReportRow(vl, "收集形态");
        _rptBuildLegacy = MakeReportRow(vl, "遗产组合");

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

        var retryBtn = PrimaryBtn(row.transform, "再开一局", 16, 50);
        retryBtn.GetComponent<Button>().onClick.AddListener(() => {
            _rptBuilt = false;
            string currentMode = GameManager.Instance?.CurrentMode.ToString() ?? "Classic";
            GameManager.Instance?.StartNewGame(currentMode);
        });

        _rptShareBtn = SecondaryBtn(row.transform, "分享海报", new Color(0.66f, 0.33f, 0.97f), 14, 50);
        _rptShareBtn.GetComponent<Button>().onClick.AddListener(() => {
            var report = CompleteGameSystem.Instance?.LastReport;
            if (report != null)
            {
                CanvasUIManager.Instance?.ShareRunReportPoster(report, "short");
            }
            else
            {
                CanvasUIManager.Instance?.ShowFlashBanner("死亡报告数据不可用", Color.red, 1.2f);
            }
        });

        var homeBtn = SecondaryBtn(row.transform, "返回主页", Dim, 14, 50);
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
        Spacer(vl, SpaceMed);
        var btn = PrimaryBtn(vl.transform, "再来一次", 18, 55);
        btn.GetComponent<Button>().onClick.AddListener(() => CompleteGameSystem.Instance?.ReturnToMenu());
    }

    // ========== STAGE TRANSITION ==========
    Text _stTitle, _stSub, _stKills, _stTime;
    GameObject _stReward1, _stReward2, _stNextBtn;
    Text _stR1Name, _stR1Desc, _stR2Name, _stR2Desc;
    string _stSelectedReward;
    bool _stBuilt;

    void BuildStageTransition()
    {
        _stageTransPanel = Panel("StageTrans", new Color(0.04f, 0.06f, 0.12f));
        var rt = _stageTransPanel.GetComponent<RectTransform>();

        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(rt, false);
        Stretch(scrollGo);
        var scroll = scrollGo.GetComponent<ScrollRect>();
        scroll.horizontal = false;
        scroll.movementType = ScrollRect.MovementType.Elastic;

        var vpGo = new GameObject("VP", typeof(RectTransform), typeof(Image), typeof(Mask));
        vpGo.transform.SetParent(scrollGo.transform, false);
        Stretch(vpGo);
        vpGo.GetComponent<Image>().color = new Color(1, 1, 1, 0.003f);
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
        vlg.spacing = 8;
        vlg.childAlignment = TextAnchor.UpperCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;
        contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = contentRT;

        var vl = vlg;

        Spacer(vl, 60);

        _stTitle = Txt(vl, "", 28, Cyan, 42).GetComponent<Text>();
        _stTitle.fontStyle = FontStyle.Bold;
        _stTitle.alignment = TextAnchor.MiddleCenter;

        _stSub = Txt(vl, "", 12, new Color(0.5f, 0.8f, 0.8f), 20).GetComponent<Text>();
        _stSub.alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 10);

        var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
        sep.transform.SetParent(vl.transform, false);
        sep.AddComponent<LayoutElement>().preferredHeight = 1;
        sep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.25f);

        Spacer(vl, 8);
        _stKills = MakeReportRow(vl, "击杀数");
        _stTime = MakeReportRow(vl, "用时");

        Spacer(vl, 15);

        Txt(vl, "选择跨关奖励", 16, Gold, 26).GetComponent<Text>().alignment = TextAnchor.MiddleCenter;

        Spacer(vl, 8);

        _stReward1 = MakeRewardCard(vl, out _stR1Name, out _stR1Desc, 0);
        Spacer(vl, 6);
        _stReward2 = MakeRewardCard(vl, out _stR2Name, out _stR2Desc, 1);

        Spacer(vl, 20);

        _stNextBtn = PrimaryBtn(vl.transform, "进入下一关", 18, 55);
        _stNextBtn.GetComponent<Button>().onClick.AddListener(OnStageNextClicked);
        _stNextBtn.SetActive(false);

        Spacer(vl, 30);
    }

    GameObject MakeRewardCard(VerticalLayoutGroup vl, out Text nameText, out Text descText, int index)
    {
        var card = new GameObject($"Reward{index}", typeof(RectTransform), typeof(Image), typeof(Button));
        card.transform.SetParent(vl.transform, false);
        card.AddComponent<LayoutElement>().preferredHeight = 70;
        var img = card.GetComponent<Image>();
        img.color = new Color(0.08f, 0.12f, 0.18f);

        var cardVl = card.AddComponent<VerticalLayoutGroup>();
        cardVl.padding = new RectOffset(16, 16, 8, 8);
        cardVl.spacing = 4;
        cardVl.childAlignment = TextAnchor.MiddleCenter;
        cardVl.childForceExpandWidth = true;
        cardVl.childForceExpandHeight = false;

        nameText = Txt(cardVl, "", 16, Cyan, 24).GetComponent<Text>();
        nameText.fontStyle = FontStyle.Bold;
        nameText.alignment = TextAnchor.MiddleCenter;
        descText = Txt(cardVl, "", 13, Bright, 20).GetComponent<Text>();
        descText.alignment = TextAnchor.MiddleCenter;

        int idx = index;
        card.GetComponent<Button>().onClick.AddListener(() => OnRewardCardClicked(idx));
        return card;
    }

    void OnRewardCardClicked(int index)
    {
        var gs = CompleteGameSystem.Instance;
        if (gs?.PendingStageRewards == null || index >= gs.PendingStageRewards.Length) return;

        _stSelectedReward = gs.PendingStageRewards[index].id;

        var c1 = _stReward1?.GetComponent<Image>();
        var c2 = _stReward2?.GetComponent<Image>();
        if (c1) c1.color = index == 0 ? new Color(0f, 0.3f, 0.4f) : new Color(0.08f, 0.12f, 0.18f);
        if (c2) c2.color = index == 1 ? new Color(0f, 0.3f, 0.4f) : new Color(0.08f, 0.12f, 0.18f);

        if (_stNextBtn) _stNextBtn.SetActive(true);
    }

    void OnStageNextClicked()
    {
        if (string.IsNullOrEmpty(_stSelectedReward)) return;
        string reward = _stSelectedReward;
        _stBuilt = false;
        _stSelectedReward = null;
        if (_stNextBtn) _stNextBtn.SetActive(false);
        CompleteGameSystem.Instance?.ApplyExpeditionReward(reward);
    }

    void SyncStageTransition(CompleteGameSystem gs)
    {
        if (_stBuilt) return;
        _stBuilt = true;
        _stSelectedReward = null;
        if (_stNextBtn) _stNextBtn.SetActive(false);

        int stage = GameManager.Instance.CurrentStage;
        ST(_stTitle, $"第 {stage} 关 通关！");
        ST(_stSub, $"STAGE {stage} CLEARED");
        ST(_stKills, $"{gs.StageKills}");

        int mins = Mathf.FloorToInt(gs.StageTime / 60f);
        int secs = Mathf.FloorToInt(gs.StageTime % 60f);
        ST(_stTime, $"{mins}分{secs}秒");

        var rewards = gs.PendingStageRewards;
        if (rewards != null && rewards.Length >= 2)
        {
            ST(_stR1Name, $"{rewards[0].icon} {rewards[0].name}");
            ST(_stR1Desc, rewards[0].desc);
            ST(_stR2Name, $"{rewards[1].icon} {rewards[1].name}");
            ST(_stR2Desc, rewards[1].desc);
        }

        var c1 = _stReward1?.GetComponent<Image>();
        var c2 = _stReward2?.GetComponent<Image>();
        if (c1) c1.color = new Color(0.08f, 0.12f, 0.18f);
        if (c2) c2.color = new Color(0.08f, 0.12f, 0.18f);
    }
}
