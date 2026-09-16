using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    public void ShowAchievementPanel()
    {
        if (_achOvl != null) Destroy(_achOvl);
        var content = BuildOverlayScaffold(ref _achOvl, "AchOvl", "成就回响", Gold, OvlBg, "bg_achievement", "icon_achievement");

        var am = AchievementManager.Instance;
        int unlocked = 0;
        int total = 0;
        if (am != null)
        {
            var all = am.GetAllAchievements();
            total = all.Count;
            foreach (var ach in all)
                if (am.IsUnlocked(ach.achievementId)) unlocked++;
        }

        var (summaryCard, summaryVL) = InfoCard(content.transform, CardBg, 50);
        Txt(summaryVL, $"已解锁 <color=#ffd700><b>{unlocked}</b></color> / {total}", 14, Bright, 22);
        var achBarGo = new GameObject("AchBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        achBarGo.transform.SetParent(summaryVL.transform, false);
        achBarGo.GetComponent<LayoutElement>().preferredHeight = 6;
        achBarGo.GetComponent<Image>().color = new Color(0.1f, 0.15f, 0.2f);
        var achFill = new GameObject("Fill", typeof(RectTransform), typeof(Image));
        achFill.transform.SetParent(achBarGo.transform, false);
        var achFillRT = achFill.GetComponent<RectTransform>();
        achFillRT.anchorMin = Vector2.zero;
        achFillRT.anchorMax = new Vector2(total > 0 ? Mathf.Clamp01((float)unlocked / total) : 0, 1);
        achFillRT.offsetMin = Vector2.zero; achFillRT.offsetMax = Vector2.zero;
        achFill.GetComponent<Image>().color = Gold;

        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceSmall);

        foreach (var ach in am.GetAllAchievements())
        {
            bool done = am.IsUnlocked(ach.achievementId);
            var (card, cvl) = InfoCard(content.transform, done ? CardBg : LockedCardBg, 70);

            var nameRow = new GameObject("NameRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
            nameRow.transform.SetParent(cvl.transform, false);
            nameRow.AddComponent<LayoutElement>().preferredHeight = 22;
            var nrHL = nameRow.GetComponent<HorizontalLayoutGroup>();
            nrHL.spacing = 6; nrHL.childAlignment = TextAnchor.MiddleLeft;
            nrHL.childForceExpandWidth = false;

            var statusTxt = TxtGo(nameRow.transform, done ? "✓" : "○", 14, done ? SuccessGreen : Dim);
            statusTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 20;
            var achNameTxt = TxtGo(nameRow.transform, $"{ach.icon} {ach.name}", 14, done ? Gold : Dim);
            achNameTxt.fontStyle = done ? FontStyle.Bold : FontStyle.Normal;
            achNameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            Txt(cvl, ach.description, 12, done ? Bright : Dim, 18);

            if (!done && ach.target > 1)
            {
                int progress = am.GetProgress(ach.achievementId);
                Txt(cvl, $"进度: {progress}/{ach.target}", 11, Dim, 16);
            }

            if (ach.bonus != null && !string.IsNullOrEmpty(ach.bonus.description))
            {
                Txt(cvl, $"奖励: {ach.bonus.description}", 11, done ? Cyan : Dim, 16);
            }
        }

        _achPanelBuilt = true;
        OpenOverlayAnimated(_achOvl);
    }

    // Achievement Toast notification
    void BuildAchievementToast()
    {
        _achToast = new GameObject("AchToast", typeof(RectTransform), typeof(CanvasGroup), typeof(Image));
        _achToast.transform.SetParent(_root, false);
        var rt = _achToast.GetComponent<RectTransform>();
        rt.anchorMin = new Vector2(0.05f, 0.88f);
        rt.anchorMax = new Vector2(0.95f, 0.96f);
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        _achToast.GetComponent<Image>().color = new Color(0.06f, 0.05f, 0.12f, 0.93f);
        var outline = _achToast.AddComponent<Outline>();
        outline.effectColor = new Color(Gold.r, Gold.g, Gold.b, 0.35f);
        outline.effectDistance = new Vector2(0.5f, -0.5f);

        var hl = _achToast.AddComponent<HorizontalLayoutGroup>();
        hl.spacing = 8; hl.padding = new RectOffset(12, 12, 4, 4);
        hl.childAlignment = TextAnchor.MiddleLeft;
        hl.childForceExpandWidth = false; hl.childForceExpandHeight = true;

        _achToastIcon = Txt(hl, "★", 28, Gold, 40).GetComponent<Text>();
        var le0 = _achToastIcon.gameObject.AddComponent<LayoutElement>();
        le0.preferredWidth = 40;

        var infoGo = new GameObject("Info", typeof(RectTransform), typeof(VerticalLayoutGroup));
        infoGo.transform.SetParent(_achToast.transform, false);
        var ivl = infoGo.GetComponent<VerticalLayoutGroup>();
        ivl.spacing = 2; ivl.childForceExpandWidth = true; ivl.childForceExpandHeight = false;
        ivl.childAlignment = TextAnchor.MiddleLeft;
        var le1 = infoGo.AddComponent<LayoutElement>();
        le1.flexibleWidth = 1;

        _achToastName = Txt(ivl, "", 16, Gold, 22).GetComponent<Text>();
        _achToastName.fontStyle = FontStyle.Bold;
        _achToastDesc = Txt(ivl, "", 11, Bright, 16).GetComponent<Text>();

        _achToast.GetComponent<CanvasGroup>().alpha = 0;
        _achToast.SetActive(false);
    }

    void BuildSoftHintBubble()
    {
        _softHintBubble = new GameObject("SoftHint", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(Button));
        _softHintBubble.transform.SetParent(_root, false);
        
        var hrt = _softHintBubble.GetComponent<RectTransform>();
        hrt.anchorMin = new Vector2(0.04f, 0.285f);
        hrt.anchorMax = new Vector2(0.40f, 0.365f);
        hrt.pivot = new Vector2(0.5f, 0);
        hrt.offsetMin = Vector2.zero;
        hrt.offsetMax = Vector2.zero;
        
        var img = _softHintBubble.GetComponent<Image>();
        img.color = new Color(0.05f, 0.12f, 0.14f, 0.93f);
        img.sprite = CreateRoundedSprite(Color.white, 220, 70, 10);
        
        var outline = _softHintBubble.GetComponent<Outline>();
        outline.effectColor = new Color(0f, 0.75f, 0.65f, 0.30f);
        outline.effectDistance = new Vector2(0.5f, 0.5f);

        var btn = _softHintBubble.GetComponent<Button>();
        btn.targetGraphic = img;
        btn.onClick.AddListener(() => {
            CompleteGameSystem.Instance?.DismissSoftHint();
            HideGuideBubble();
        });

        _softHintText = TxtAnchored(hrt, "", 16, new Color(1f, 0.95f, 0.3f), new Vector2(0, 0), new Vector2(1, 1), 20, 18);
        _softHintText.alignment = TextAnchor.MiddleCenter;
        _softHintText.fontStyle = FontStyle.Bold;
        _softHintText.raycastTarget = false;
        _softHintText.horizontalOverflow = HorizontalWrapMode.Wrap;
        _softHintText.verticalOverflow = VerticalWrapMode.Truncate;
        _softHintText.resizeTextForBestFit = true;
        _softHintText.resizeTextMinSize = 10;
        _softHintText.resizeTextMaxSize = 18;
        _softHintText.supportRichText = true;
        _softHintText.GetComponent<Outline>().effectColor = new Color(0f, 0.5f, 0.4f, 0.6f);
        _softHintText.GetComponent<Outline>().effectDistance = new Vector2(2, 2);

        var arrowGo = new GameObject("Arrow", typeof(RectTransform), typeof(Image), typeof(Outline));
        arrowGo.transform.SetParent(hrt, false);
        var arrowRT = arrowGo.GetComponent<RectTransform>();
        _softHintArrow = arrowRT;
        arrowRT.anchorMin = new Vector2(0.5f, 0); arrowRT.anchorMax = new Vector2(0.5f, 0);
        arrowRT.pivot = new Vector2(0.5f, 1);
        arrowRT.offsetMin = new Vector2(-18, -32);
        arrowRT.offsetMax = new Vector2(18, -2);
        
        var arrowImg = arrowGo.GetComponent<Image>();
        arrowImg.color = new Color(0f, 0.9f, 0.75f, 1f);
        arrowImg.sprite = CreateTriangleSprite(Color.white);
        
        var arrowOutline = arrowGo.GetComponent<Outline>();
        arrowOutline.effectColor = new Color(0f, 1f, 0.816f, 0.5f);
        arrowOutline.effectDistance = new Vector2(1, 1);

        _softHintBubble.SetActive(false);
        _softHintBubble.transform.SetAsLastSibling();
    }
    
    Sprite CreateRoundedSprite(Color color, int width, int height, int radius)
    {
        Texture2D tex = new Texture2D(width, height);
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                float cx = x < radius ? radius : (x > width - radius ? width - radius : x);
                float cy = y < radius ? radius : (y > height - radius ? height - radius : y);
                float dist = Mathf.Sqrt(Mathf.Pow(x - cx, 2) + Mathf.Pow(y - cy, 2));
                if (dist <= radius)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, width, height), new Vector2(0.5f, 0.5f));
    }
    
    Sprite CreateTriangleSprite(Color color)
    {
        Texture2D tex = new Texture2D(32, 32);
        for (int x = 0; x < 32; x++)
        {
            for (int y = 0; y < 32; y++)
            {
                float dx = Mathf.Abs(x - 16);
                float dy = 31 - y;
                if (dy >= dx * 2)
                {
                    tex.SetPixel(x, y, color);
                }
                else
                {
                    tex.SetPixel(x, y, Color.clear);
                }
            }
        }
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, 32, 32), new Vector2(0.5f, 0.5f));
    }

    void OnAchievementUnlocked(AchievementData ach)
    {
        _achToastQueue.Enqueue(ach);
    }

    Coroutine _achToastCo;
    void UpdateAchievementToast()
    {
        if (_achToast == null) return;
        if (_achToastCo != null) return;
        if (_achToastQueue.Count > 0)
        {
            var ach = _achToastQueue.Dequeue();
            ST(_achToastIcon, ach.icon ?? "★");
            ST(_achToastName, $"成就解锁: {ach.name}");
            string desc = ach.description;
            if (ach.bonus != null && !string.IsNullOrEmpty(ach.bonus.description))
                desc += $"  |  奖励: {ach.bonus.description}";
            ST(_achToastDesc, desc);
            _achToast.SetActive(true);
            _achToast.transform.SetAsLastSibling();
            _achToastCo = StartCoroutine(AchToastSequence());
        }
    }

    IEnumerator AchToastSequence()
    {
        var rt = _achToast.GetComponent<RectTransform>();
        yield return StartCoroutine(UIAnimationSystem.FadeIn(rt, 0.3f));
        yield return new WaitForSeconds(2.2f);
        yield return StartCoroutine(UIAnimationSystem.FadeOut(rt, 0.5f));
        _achToast.SetActive(false);
        var cg = _achToast.GetComponent<CanvasGroup>();
        if (cg) cg.alpha = 0;
        _achToastCo = null;
    }
}
