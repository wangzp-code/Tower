using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    void BuildMenuPanel()
    {
        _menuOvl = Panel("MenuOvl", new Color(0.10f, 0.07f, 0.18f, 0.95f));
        var root = _menuOvl.GetComponent<RectTransform>();

        var bgImg = _menuOvl.GetComponent<Image>();
        bgImg.color = new Color(0.10f, 0.07f, 0.18f, 0.95f);
        bgImg.material = null;

        var menuBgTex = LoadTex("UI/菜单背景图");
        if (menuBgTex != null)
        {
            var bgGo = new GameObject("BgImg", typeof(RectTransform), typeof(RawImage));
            bgGo.transform.SetParent(root, false);
            Stretch(bgGo);
            var bgRI = bgGo.GetComponent<RawImage>();
            bgRI.texture = menuBgTex;
            bgRI.color = new Color(1f, 1f, 1f, 0.7f);
            bgRI.raycastTarget = false;
        }

        var vl = new GameObject("MenuVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        vl.transform.SetParent(root, false);
        var vlRT = vl.GetComponent<RectTransform>();
        vlRT.anchorMin = new Vector2(0.1f, 0.06f);
        vlRT.anchorMax = new Vector2(0.9f, 0.92f);
        vlRT.offsetMin = Vector2.zero;
        vlRT.offsetMax = Vector2.zero;
        var vlg = vl.GetComponent<VerticalLayoutGroup>();
        vlg.spacing = 6;
        vlg.padding = new RectOffset(12, 12, 10, 10);
        vlg.childAlignment = TextAnchor.MiddleCenter;
        vlg.childForceExpandWidth = true;
        vlg.childForceExpandHeight = false;

        var titleGo = Txt(vl, "菜单", 20, Cyan, 28);
        titleGo.GetComponent<Text>().fontStyle = FontStyle.Bold;

        SepLine(vl);

        string[][] menuItems = {
            new string[] { "★ 进化",       "00ffd0" },
            new string[] { "◇ 形态羁绊",   "00ffd0" },
            new string[] { "☢ 污染技能",    "00ffd0" },
            new string[] { "◈ 附身遗产",    "a040f0" },
            new string[] { "⊞ 构筑总览",    "00ffd0" },
            new string[] { "⊕ 锚点管理",    "00ffd0" },
            new string[] { "† 神秘商店",    "ff9933" },
        };

        Action[] menuActions = {
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowEvolutionPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowFormBondPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowPollutionSkillPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowLegacyPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowBuildOverviewPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowAnchorPanel(); },
            () => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ShowItemShopPanel(); },
        };

        for (int i = 0; i < menuItems.Length; i++)
        {
            int idx = i;
            var item = menuItems[i];
            Color itemColor = ParseColor(item[1]);

            var btnGo = new GameObject($"MenuBtn_{i}", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnGo.transform.SetParent(vl.transform, false);
            btnGo.AddComponent<LayoutElement>().preferredHeight = 54;

            var btnImg = btnGo.GetComponent<Image>();
            btnImg.color = DarkItemBg;
            var mat = Resources.Load<Material>("Materials/UI/Default");
            if (mat != null) btnImg.material = mat;

            var ol = btnGo.GetComponent<Outline>();
            ol.effectColor = new Color(0f, 1f, 0.82f, 0.3f);
            ol.effectDistance = new Vector2(1, 1);

            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = btnImg;
            btn.onClick.AddListener(() =>
            {
                StartCoroutine(MenuButtonClickAnim(btnGo));
                menuActions[idx]();
            });

            // 添加悬停效果
            var eventTrigger = btnGo.AddComponent<EventTrigger>();

            var enterTrigger = new EventTrigger.Entry();
            enterTrigger.eventID = EventTriggerType.PointerEnter;
            enterTrigger.callback.AddListener((data) => MenuButtonHover(btnGo, true));
            eventTrigger.triggers.Add(enterTrigger);

            var exitTrigger = new EventTrigger.Entry();
            exitTrigger.eventID = EventTriggerType.PointerExit;
            exitTrigger.callback.AddListener((data) => MenuButtonHover(btnGo, false));
            eventTrigger.triggers.Add(exitTrigger);

            var txt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(btnGo.transform, false);
            Stretch(txt);
            var t = txt.GetComponent<Text>();
            t.font = F(); t.fontSize = 16; t.color = itemColor;
            t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
            t.text = item[0]; t.raycastTarget = false;

            // 进化按钮添加红点提示
            if (i == 0) // 进化按钮是第一个
            {
                _evoRedDot = new GameObject("EvoRedDot", typeof(RectTransform));
                _evoRedDot.transform.SetParent(btnGo.transform, false);
                var rdRT = _evoRedDot.GetComponent<RectTransform>();
                rdRT.anchorMin = new Vector2(1, 1);
                rdRT.anchorMax = new Vector2(1, 1);
                rdRT.pivot = new Vector2(0.5f, 0.5f);
                rdRT.anchoredPosition = new Vector2(-15, -15);
                rdRT.sizeDelta = new Vector2(16, 16);

                // 创建外圈光晕（先创建，放在底层）
                var glow = new GameObject("Glow", typeof(RectTransform), typeof(Text));
                glow.transform.SetParent(_evoRedDot.transform, false);
                var glowRT = glow.GetComponent<RectTransform>();
                glowRT.anchorMin = Vector2.zero;
                glowRT.anchorMax = Vector2.one;
                var glowTxt = glow.GetComponent<Text>();
                glowTxt.font = F();
                glowTxt.fontSize = 24;
                glowTxt.color = new Color(1f, 0.2f, 0.2f, 0.6f);
                glowTxt.alignment = TextAnchor.MiddleCenter;
                glowTxt.text = "●";

                // 创建红点（使用文字●）
                var dot = new GameObject("Dot", typeof(RectTransform), typeof(Text));
                dot.transform.SetParent(_evoRedDot.transform, false);
                var dotRT = dot.GetComponent<RectTransform>();
                dotRT.anchorMin = Vector2.zero;
                dotRT.anchorMax = Vector2.one;
                var dotTxt = dot.GetComponent<Text>();
                dotTxt.font = F();
                dotTxt.fontSize = 16;
                dotTxt.color = LightRed;
                dotTxt.alignment = TextAnchor.MiddleCenter;
                dotTxt.text = "●";

                // 先强制显示红点，验证是否能显示
                _evoRedDot.SetActive(true);
                if (_evoRedDotCoroutine == null)
                    _evoRedDotCoroutine = StartCoroutine(EvoRedDotPulse());

                // 延迟一帧后再根据实际条件更新状态
                StartCoroutine(DelayedUpdateRedDot());
            }
        }

        // 固化记忆 (200EP) — special button with dynamic state
        {
            var btnGo = new GameObject("SolidifyBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnGo.transform.SetParent(vl.transform, false);
            btnGo.AddComponent<LayoutElement>().preferredHeight = 50;
            _solidifyBtnImg = btnGo.GetComponent<Image>();
            _solidifyBtnImg.color = DarkItemBg;
            var ol = btnGo.GetComponent<Outline>();
            ol.effectColor = new Color(0f, 1f, 0.82f, 0.3f);
            ol.effectDistance = new Vector2(1, 1);
            var sh = btnGo.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.24f);
            sh.effectDistance = new Vector2(0, -2);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = _solidifyBtnImg;
            btn.onClick.AddListener(() => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); DoSolidifyMemory(); });
            btnGo.AddComponent<ButtonPressFeedback>();
            var txt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(btnGo.transform, false);
            Stretch(txt);
            _solidifyBtnTxt = txt.GetComponent<Text>();
            _solidifyBtnTxt.font = F(); _solidifyBtnTxt.fontSize = 15; _solidifyBtnTxt.alignment = TextAnchor.MiddleCenter;
            _solidifyBtnTxt.fontStyle = FontStyle.Bold; _solidifyBtnTxt.raycastTarget = false;
        }

        // 退出游戏
        {
            var btnGo = new GameObject("ExitBtn", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline));
            btnGo.transform.SetParent(vl.transform, false);
            btnGo.AddComponent<LayoutElement>().preferredHeight = 50;
            var img = btnGo.GetComponent<Image>();
            img.color = DarkItemBg;
            var ol = btnGo.GetComponent<Outline>();
            ol.effectColor = new Color(0.8f, 0.2f, 0.2f, 0.35f);
            ol.effectDistance = new Vector2(1, 1);
            var sh = btnGo.AddComponent<Shadow>();
            sh.effectColor = new Color(0, 0, 0, 0.24f);
            sh.effectDistance = new Vector2(0, -2);
            var btn = btnGo.GetComponent<Button>();
            btn.targetGraphic = img;
            btn.onClick.AddListener(() => { StartCoroutine(CloseOverlayAnimated(_menuOvl)); ExitGame(); });
            btnGo.AddComponent<ButtonPressFeedback>();
            var txt = new GameObject("Txt", typeof(RectTransform), typeof(Text));
            txt.transform.SetParent(btnGo.transform, false);
            Stretch(txt);
            var t = txt.GetComponent<Text>();
            t.font = F(); t.fontSize = 15; t.color = SoftRed;
            t.alignment = TextAnchor.MiddleCenter; t.fontStyle = FontStyle.Bold;
            t.text = "✕ 退出游戏"; t.raycastTarget = false;
        }

        Spacer(vl, 2);

        // 关闭
        var closeBtnGo = new GameObject("CloseBtn", typeof(RectTransform), typeof(Image), typeof(Button));
        closeBtnGo.transform.SetParent(vl.transform, false);
        closeBtnGo.AddComponent<LayoutElement>().preferredHeight = 44;
        closeBtnGo.GetComponent<Image>().color = new Color(0.06f, 0.04f, 0.10f);
        var closeBtn = closeBtnGo.GetComponent<Button>();
        closeBtn.targetGraphic = closeBtnGo.GetComponent<Image>();
        closeBtn.onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(_menuOvl)));
        var closeTxt = new GameObject("T", typeof(RectTransform), typeof(Text));
        closeTxt.transform.SetParent(closeBtnGo.transform, false);
        Stretch(closeTxt);
        var ct = closeTxt.GetComponent<Text>();
        ct.font = F(); ct.text = "✕ 关闭"; ct.fontSize = 15; ct.color = Dim;
        ct.alignment = TextAnchor.MiddleCenter; ct.raycastTarget = false;

        _menuOvl.SetActive(false);
    }

    Color ParseColor(string hex)
    {
        hex = hex.TrimStart('#');
        float r = int.Parse(hex.Substring(0, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float g = int.Parse(hex.Substring(2, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        float b = int.Parse(hex.Substring(4, 2), System.Globalization.NumberStyles.HexNumber) / 255f;
        return new Color(r, g, b);
    }
}
