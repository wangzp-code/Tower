using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    void ShowEvolutionPanel()
    {
        if (_evoOvl == null) BuildEvolutionOverlay();
        _evoPanelBuilt = false;
        OpenOverlayAnimated(_evoOvl);
    }

    void BuildEvolutionOverlay()
    {
        var content = BuildOverlayScaffold(ref _evoOvl, "EvoOvl", "⚡ 进化树", Gold, OvlBg, "bg_evolution");
        var listGo = new GameObject("EvoList", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6; lg.padding = new RectOffset(4,4,4,4);
        lg.childForceExpandWidth = true; lg.childForceExpandHeight = false;
        _evoList = listGo.transform;
    }

    void SyncEvolution()
    {
        if (_evoOvl == null || !_evoOvl.activeSelf) return;
        if (_evoPanelBuilt) return;
        _evoPanelBuilt = true;

        for (int i = _evoList.childCount - 1; i >= 0; i--)
            DestroyImmediate(_evoList.GetChild(i).gameObject);

        var gs = CompleteGameSystem.Instance;
        var player = GameManager.Instance?.Player;
        if (gs == null || player == null) return;

        if (!EvolutionData.Trees.TryGetValue(player.selectedClass, out var tree)) return;

        // 创建EP显示栏
        var epBar = new GameObject("EPBar", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        epBar.transform.SetParent(_evoList, false);
        var epHL = epBar.GetComponent<HorizontalLayoutGroup>();
        epHL.spacing = 8;
        epHL.childAlignment = TextAnchor.MiddleCenter;
        epHL.childForceExpandWidth = false;
        epBar.AddComponent<LayoutElement>().preferredHeight = 32;

        TxtGo(epBar.transform, "$", 20, Gold);
        var epTxt = TxtGo(epBar.transform, player.evolutionPoints.ToString(), 18, Gold);
        epTxt.fontStyle = FontStyle.Bold;

        SepLine(_evoList);

        for (int i = 0; i < tree.Count; i++)
        {
            var node = tree[i];
            bool unlocked = i < gs.EvolutionLevel;
            bool canUnlock = i == gs.EvolutionLevel && player.evolutionPoints >= node.epCost;
            bool isNext = i == gs.EvolutionLevel;

            // 卡片背景颜色
            Color cardColor = unlocked ? UnlockedCardBg :
                              canUnlock ? CanUnlockCardBg :
                              new Color(0.08f, 0.06f, 0.12f);

            // 文字颜色
            Color titleColor = unlocked ? Cyan : canUnlock ? Gold : Dim;
            Color descColor = unlocked ? Bright : canUnlock ? new Color(0.9f, 0.85f, 0.7f) : new Color(0.5f, 0.48f, 0.55f);

            var card = new GameObject($"EvoCard_{i}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
            card.transform.SetParent(_evoList, false);
            var cardRT = card.GetComponent<RectTransform>();
            cardRT.sizeDelta = new Vector2(0, 75);
            card.GetComponent<LayoutElement>().preferredHeight = 75;

            var cardImg = card.GetComponent<Image>();
            cardImg.color = cardColor;

            // 可升级时添加边框发光效果
            if (canUnlock)
            {
                var outline = card.AddComponent<Outline>();
                outline.effectColor = new Color(1f, 0.7f, 0.2f, 0.6f);
                outline.effectDistance = new Vector2(2, 2);
            }

            // 创建内部布局
            var cvl = new GameObject("Content", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            cvl.transform.SetParent(card.transform, false);
            var cvlRT = cvl.GetComponent<RectTransform>();
            cvlRT.anchorMin = Vector2.zero;
            cvlRT.anchorMax = Vector2.one;
            cvlRT.offsetMin = new Vector2(12, 8);
            cvlRT.offsetMax = new Vector2(-12, -8);
            var cvlHL = cvl.GetComponent<HorizontalLayoutGroup>();
            cvlHL.spacing = 12;
            cvlHL.childForceExpandWidth = true;
            cvlHL.childAlignment = TextAnchor.MiddleLeft;

            // 左侧：等级和名称
            var leftVL = new GameObject("Left", typeof(RectTransform), typeof(VerticalLayoutGroup));
            leftVL.transform.SetParent(cvl.transform, false);
            var leftVLg = leftVL.GetComponent<VerticalLayoutGroup>();
            leftVLg.spacing = 4;
            leftVLg.childForceExpandWidth = true;

            string status = unlocked ? "✓ 已解锁" : canUnlock ? $"花费 {node.epCost} EP" : $"○ 需要 {node.epCost} EP";
            var title = TxtGo(leftVL.transform, $"Lv{node.level} {node.name}", 16, titleColor);
            title.fontStyle = FontStyle.Bold;
            var statusTxt = TxtGo(leftVL.transform, status, 12, canUnlock ? Gold : Dim);

            // 中间：描述
            var descVL = new GameObject("Desc", typeof(RectTransform), typeof(VerticalLayoutGroup));
            descVL.transform.SetParent(cvl.transform, false);
            descVL.AddComponent<LayoutElement>().flexibleWidth = 2;
            var descVLg = descVL.GetComponent<VerticalLayoutGroup>();
            descVLg.spacing = 2;

            TxtGo(descVL.transform, node.description, 11, descColor);

            // 右侧：解锁按钮或状态图标
            var rightVL = new GameObject("Right", typeof(RectTransform), typeof(VerticalLayoutGroup));
            rightVL.transform.SetParent(cvl.transform, false);
            rightVL.AddComponent<LayoutElement>().flexibleWidth = 0;
            var rightVLg = rightVL.GetComponent<VerticalLayoutGroup>();
            rightVLg.spacing = 4;
            rightVLg.childAlignment = TextAnchor.MiddleRight;

            if (unlocked)
            {
                var check = TxtGo(rightVL.transform, "✓", 24, new Color(0.3f, 1f, 0.6f));
            }
            else if (canUnlock)
            {
                var btn = new GameObject("UnlockBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
                btn.transform.SetParent(rightVL.transform, false);
                var btnRT = btn.GetComponent<RectTransform>();
                btnRT.sizeDelta = new Vector2(80, 32);

                var btnImg = btn.GetComponent<Image>();
                btnImg.color = new Color(0.25f, 0.2f, 0.08f);

                var btnOutline = btn.GetComponent<Outline>();
                btnOutline.effectColor = new Color(1f, 0.8f, 0.3f, 0.8f);
                btnOutline.effectDistance = new Vector2(1, 1);

                var btnTxt = TxtGo(btn.transform, "解锁", 14, new Color(1f, 0.9f, 0.6f));
                btnTxt.alignment = TextAnchor.MiddleCenter;

                var button = btn.GetComponent<Button>();
                button.targetGraphic = btnImg;
                button.onClick.AddListener(() => {
                    gs.Evolve();
                    _evoPanelBuilt = false;
                    UpdateEvoRedDot();
                });
            }
            else
            {
                var lockIcon = TxtGo(rightVL.transform, "○", 20, Dim);
            }
        }

        // 底部提示
        if (gs.EvolutionLevel >= tree.Count)
        {
            Txt(_evoList, "✨ 所有进化已解锁！", 14, new Color(0.9f, 0.7f, 0.3f), 28);
        }
        else
        {
            var nextCost = tree[gs.EvolutionLevel].epCost;
            int needed = nextCost - player.evolutionPoints;
            if (needed > 0)
            {
                Txt(_evoList, $"还差 {needed} EP 可解锁下一级进化", 12, new Color(0.8f, 0.5f, 0.5f), 22);
            }
        }
    }
}
