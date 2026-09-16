using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    void ShowMenuPanel()
    {
        UpdateSolidifyBtn();
        UpdateEvoRedDot();
        OpenOverlayAnimated(_menuOvl);
    }

    void UpdateEvoRedDot()
    {
        bool canEvolve = CanEvolve();

        if (_evoRedDot != null)
        {
            _evoRedDot.SetActive(canEvolve);
        }
        else if (canEvolve && _menuOvl != null)
        {
            // 如果红点不存在但需要显示，尝试找到进化按钮并创建红点
            var evoBtn = _menuOvl.transform.Find("MenuVL/MenuBtn_0");
            if (evoBtn != null)
            {
                CreateEvoRedDot(evoBtn);
            }
        }
    }

    void CreateEvoRedDot(Transform parent)
    {
        _evoRedDot = new GameObject("EvoRedDot", typeof(RectTransform));
        _evoRedDot.transform.SetParent(parent, false);
        var rdRT = _evoRedDot.GetComponent<RectTransform>();
        rdRT.anchorMin = new Vector2(1, 1);
        rdRT.anchorMax = new Vector2(1, 1);
        rdRT.pivot = new Vector2(0.5f, 0.5f);
        rdRT.anchoredPosition = new Vector2(-15, -15);
        rdRT.sizeDelta = new Vector2(16, 16);

        // 创建外圈光晕
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

        // 创建红点
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

        _evoRedDot.SetActive(true);
        if (_evoRedDotCoroutine == null)
            _evoRedDotCoroutine = StartCoroutine(EvoRedDotPulse());
    }

    void OnPlayerStatsChanged()
    {
        UpdateEvoRedDot();
    }

    IEnumerator DelayedUpdateRedDot()
    {
        yield return null; // 延迟一帧
        UpdateEvoRedDot();
    }

    void MenuButtonHover(GameObject btnGo, bool isEnter)
    {
        var btnImg = btnGo.GetComponent<Image>();
        var outline = btnGo.GetComponent<Outline>();
        var txt = btnGo.transform.Find("Txt")?.GetComponent<Text>();

        if (isEnter)
        {
            btnImg.color = new Color(0.12f, 0.10f, 0.20f);
            outline.effectColor = new Color(0f, 1f, 0.82f, 0.6f);
            outline.effectDistance = new Vector2(2, 2);
            if (txt != null)
            {
                txt.fontSize = 17;
                txt.color = new Color(0.3f, 1f, 0.9f);
            }
            btnGo.transform.localScale = Vector3.Lerp(btnGo.transform.localScale, new Vector3(1.02f, 1.02f, 1f), 0.2f);
        }
        else
        {
            btnImg.color = DarkItemBg;
            outline.effectColor = new Color(0f, 1f, 0.82f, 0.3f);
            outline.effectDistance = new Vector2(1, 1);
            if (txt != null)
            {
                txt.fontSize = 16;
                txt.color = ParseColor("00ffd0");
            }
            btnGo.transform.localScale = Vector3.Lerp(btnGo.transform.localScale, Vector3.one, 0.2f);
        }
    }

    IEnumerator MenuButtonClickAnim(GameObject btnGo)
    {
        Vector3 originalScale = btnGo.transform.localScale;

        // 按下缩小
        btnGo.transform.localScale = new Vector3(0.95f, 0.95f, 1f);
        yield return new WaitForSeconds(0.08f);

        // 弹回
        btnGo.transform.localScale = new Vector3(1.05f, 1.05f, 1f);
        yield return new WaitForSeconds(0.05f);

        // 恢复原状
        btnGo.transform.localScale = originalScale;
    }

    void UpdateSolidifyBtn()
    {
        if (_solidifyBtnTxt == null) return;
        var p = GameManager.Instance?.Player;
        bool canAfford = p != null && p.evolutionPoints >= 200;
        _solidifyBtnTxt.text = canAfford ? "◎ 固化记忆 (200EP)" : "◎ 固化(需200EP)";
        _solidifyBtnTxt.color = canAfford ? Cyan : new Color(0.35f, 0.35f, 0.35f);
        if (_solidifyBtnImg != null)
        {
            var ol = _solidifyBtnImg.GetComponent<Outline>();
            if (ol) ol.effectColor = canAfford ? new Color(0f, 1f, 0.82f, 0.4f) : new Color(0.2f, 0.2f, 0.2f, 0.2f);
        }
    }

    void DoSaveGame()
    {
        try
        {
            bool success = SaveSystem.Instance?.Save(0) ?? false;
            if (success)
            {
                CompleteGameSystem.Instance?.AddCombatLog("◆ 游戏已保存");
                ShowFlashBanner("◆ 游戏已保存", new Color(0.3f, 0.85f, 0.55f), 1.5f);
            }
            else
            {
                ShowFlashBanner("保存失败", new Color(1f, 0.3f, 0.3f), 1.5f);
            }
        }
        catch (System.Exception ex)
        {
            ShowFlashBanner("保存失败", new Color(1f, 0.3f, 0.3f), 1.5f);
        }
    }

    void DoSolidifyMemory()
    {
        var p = GameManager.Instance?.Player;
        if (p == null || p.evolutionPoints < 200)
        {
            CompleteGameSystem.Instance?.AddCombatLog("✕ 需要200EP来固化记忆");
            return;
        }
        AnchorSystem.Instance?.TryActivateAltar();
    }

    // Phase 3 panel refs
    GameObject _evoOvl, _shopOvl, _bestOvl, _achOvl, _setOvl, _rankOvl, _dailyOvl, _fragOvl;
    Transform _evoList, _shopList, _bestList, _achList;
    bool _evoPanelBuilt, _shopPanelBuilt, _bestPanelBuilt, _achPanelBuilt;

    // Shared overlay builder: title bar (with close button) + scrollable content area
    Transform BuildOverlayScaffold(ref GameObject ovl, string panelName, string title, Color titleColor, Color bgColor, string bgTexture = null, string titleIcon = null)
    {
        ovl = Panel(panelName, bgColor);
        var panel = ovl;
        var root = panel.GetComponent<RectTransform>();

        if (!string.IsNullOrEmpty(bgTexture))
        {
            var tex = LoadTex("UI/" + bgTexture);
            if (tex != null)
            {
                var bgGo = new GameObject("BgImg", typeof(RectTransform), typeof(RawImage), typeof(CanvasGroup));
                bgGo.transform.SetParent(root, false);
                Stretch(bgGo);
                bgGo.GetComponent<RawImage>().texture = tex;
                ThemeUIHelper.ApplyArtBackgroundTint(bgGo.GetComponent<RawImage>(), 0.55f);
                bgGo.GetComponent<CanvasGroup>().blocksRaycasts = false;
            }
        }

        // Header bar (fixed top 8%)
        var header = new GameObject("Header", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        header.transform.SetParent(root, false);
        var hRT = header.GetComponent<RectTransform>();
        hRT.anchorMin = new Vector2(0, 0.92f); hRT.anchorMax = Vector2.one;
        hRT.offsetMin = new Vector2(15, 0); hRT.offsetMax = new Vector2(-15, -8);
        var hhl = header.GetComponent<HorizontalLayoutGroup>();
        hhl.childAlignment = TextAnchor.MiddleCenter;
        hhl.childForceExpandWidth = false; hhl.childForceExpandHeight = true;
        hhl.spacing = 6;

        if (!string.IsNullOrEmpty(titleIcon))
        {
            var iconTex = LoadTex("UI/" + titleIcon);
            if (iconTex != null)
            {
                var icoWrap = new GameObject("TitleIconWrap", typeof(RectTransform), typeof(LayoutElement));
                icoWrap.transform.SetParent(header.transform, false);
                icoWrap.GetComponent<LayoutElement>().preferredWidth = 36;
                icoWrap.GetComponent<LayoutElement>().preferredHeight = 36;

                BuildAspectIcon(icoWrap.transform, iconTex, 3);
            }
        }

        var titleTxt = TxtGo(header.transform, title, 22, titleColor);
        titleTxt.fontStyle = FontStyle.Bold;
        titleTxt.alignment = TextAnchor.MiddleLeft;
        titleTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var closeGo = new GameObject("X", typeof(RectTransform), typeof(Image), typeof(Button), typeof(Outline), typeof(Outline));
        closeGo.transform.SetParent(header.transform, false);
        var closeLe = closeGo.AddComponent<LayoutElement>();
        closeLe.preferredWidth = 44;
        closeLe.preferredHeight = 36;

        var closeRT = closeGo.GetComponent<RectTransform>();
        closeRT.sizeDelta = new Vector2(36, 36);

        var closeImg = closeGo.GetComponent<Image>();
        closeImg.color = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.NormalBg;
        closeImg.raycastTarget = true;

        var closeOl = closeGo.GetComponent<Outline>();
        closeOl.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.NormalBorder;
        closeOl.effectDistance = new Vector2(2, 2);

        var glowOl = closeGo.GetComponents<Outline>()[1];
        glowOl.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.GlowColor;
        glowOl.effectDistance = new Vector2(4, 4);

        var closeTxt = TxtGo(closeGo.transform, "✕", 22, ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.NormalText);
        closeTxt.alignment = TextAnchor.MiddleCenter;
        closeTxt.fontStyle = FontStyle.Bold;
        Stretch(closeTxt.gameObject);

        var closeBtn = closeGo.GetComponent<Button>();
        closeBtn.targetGraphic = closeImg;
        var closeBtnColors = closeBtn.colors;
        closeBtnColors.normalColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.NormalBg;
        closeBtnColors.highlightedColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.HoverBg;
        closeBtnColors.pressedColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.PressBg;
        closeBtnColors.selectedColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.HoverBg;
        closeBtnColors.disabledColor = new Color(0.1f, 0.08f, 0.1f, 0.6f);
        closeBtnColors.colorMultiplier = 1f;
        closeBtnColors.fadeDuration = 0.1f;
        closeBtn.colors = closeBtnColors;
        closeBtn.onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(panel)));

        var closeShadow = closeGo.AddComponent<Shadow>();
        closeShadow.effectColor = new Color(0.5f, 0f, 0f, 0.35f);
        closeShadow.effectDistance = new Vector2(1, -1);

        var closeFeedback = closeGo.AddComponent<ButtonPressFeedback>();
        closeFeedback.SetCloseButtonStyle(true);

        // Separator line under header
        var sepGo = new GameObject("HSep", typeof(RectTransform), typeof(Image));
        sepGo.transform.SetParent(root, false);
        var sepRT = sepGo.GetComponent<RectTransform>();
        sepRT.anchorMin = new Vector2(0.05f, 0.915f); sepRT.anchorMax = new Vector2(0.95f, 0.917f);
        sepRT.offsetMin = Vector2.zero; sepRT.offsetMax = Vector2.zero;
        sepGo.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.2f);

        // Scrollable content area (92% of screen)
        var scrollGo = new GameObject("Scroll", typeof(RectTransform), typeof(ScrollRect));
        scrollGo.transform.SetParent(root, false);
        var sRT = scrollGo.GetComponent<RectTransform>();
        sRT.anchorMin = Vector2.zero; sRT.anchorMax = new Vector2(1, 0.91f);
        sRT.offsetMin = Vector2.zero; sRT.offsetMax = Vector2.zero;
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
        var cRT = contentGo.GetComponent<RectTransform>();
        cRT.anchorMin = new Vector2(0, 1); cRT.anchorMax = new Vector2(1, 1);
        cRT.pivot = new Vector2(0.5f, 1); cRT.sizeDelta = Vector2.zero;
        var cvl = contentGo.GetComponent<VerticalLayoutGroup>();
        cvl.padding = new RectOffset(15, 15, 10, 30);
        cvl.spacing = 8;
        cvl.childAlignment = TextAnchor.UpperCenter;
        cvl.childForceExpandWidth = true;
        cvl.childForceExpandHeight = false;
        contentGo.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        scroll.content = cRT;

        panel.SetActive(false);
        return contentGo.transform;
    }

    IEnumerator CloseOverlayAnimated(GameObject panel)
    {
        yield return UIAnimationSystem.ScaleOutSmooth(panel.GetComponent<RectTransform>(), 0.9f, 0.15f);
        panel.SetActive(false);
        var cg = panel.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 1f;
        panel.GetComponent<RectTransform>().localScale = Vector3.one;
    }

    void OpenOverlayAnimated(GameObject panel)
    {
        panel.SetActive(true);
        var rt = panel.GetComponent<RectTransform>();
        rt.localScale = Vector3.one;
        StartCoroutine(UIAnimationSystem.ScaleInSmooth(rt, 0.85f, 0.25f));
    }
}
