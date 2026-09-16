using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class PossessionCinematic : SingletonBase<PossessionCinematic>
{
    Canvas _cinema;
    Image _bgOverlay;
    Image _oldFormIcon;
    Image _newFormIcon;
    Text _nameText;
    Text _statsText;
    Text _oldStatsText;
    Text _traitText;
    Image _statsBg;
    Image _statsBorder;
    Image _glowEffect;
    Image _ringEffect;
    Image _scanlineEffect;
    GameObject _particleContainer;
    bool _playing;
    System.Action _pendingCallback;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Play(string oldFormId, string newFormId, string newName, int hp, int atk, int def, int oldHp = 0, int oldAtk = 0, int oldDef = 0, string[] traits = null, System.Action onComplete = null)
    {
        if (_playing)
        {
            StopAllCoroutines();
            _playing = false;
            CleanupState();
        }
        _pendingCallback = onComplete;
        StartCoroutine(DoCinematic(oldFormId, newFormId, newName, hp, atk, def, oldHp, oldAtk, oldDef, traits));
    }

    void CleanupState()
    {
        if (_cinema != null)
        {
            _cinema.gameObject.SetActive(false);
            _cinema.enabled = false;
        }
        ClearParticles();
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        
        // Reset all visual elements to initial state
        if (_bgOverlay != null) _bgOverlay.color = Color.clear;
        if (_oldFormIcon != null) { _oldFormIcon.color = Color.clear; _oldFormIcon.gameObject.SetActive(false); }
        if (_newFormIcon != null) { _newFormIcon.color = Color.clear; _newFormIcon.gameObject.SetActive(false); }
        if (_nameText != null) { _nameText.text = ""; _nameText.color = new Color(0.2f, 0.9f, 1f, 0); }
        if (_statsText != null) { _statsText.text = ""; _statsText.color = new Color(1f, 1f, 1f, 0); }
        if (_oldStatsText != null) { _oldStatsText.text = ""; _oldStatsText.color = new Color(0.4f, 0.4f, 0.5f, 0); }
        if (_traitText != null) { _traitText.text = ""; _traitText.color = new Color(1f, 0.8f, 0.2f, 0); }
        if (_glowEffect != null) _glowEffect.color = new Color(0.3f, 0.8f, 1f, 0);
        if (_ringEffect != null) { _ringEffect.color = new Color(0.3f, 0.8f, 1f, 0); _ringEffect.gameObject.SetActive(false); }
        if (_scanlineEffect != null) { _scanlineEffect.color = new Color(1, 1, 1, 0); _scanlineEffect.gameObject.SetActive(false); }
        if (_statsBg != null) { _statsBg.color = new Color(0.06f, 0.02f, 0.1f, 0); _statsBg.gameObject.SetActive(false); }
        if (_statsBorder != null) { _statsBorder.color = new Color(0.2f, 0.9f, 1f, 0); _statsBorder.gameObject.SetActive(false); }
        
        var oldRT = _oldFormIcon?.GetComponent<RectTransform>();
        if (oldRT != null) { oldRT.localScale = Vector3.one; oldRT.anchoredPosition = Vector2.zero; }
        var newRT = _newFormIcon?.GetComponent<RectTransform>();
        if (newRT != null) { newRT.localScale = Vector3.one; newRT.anchoredPosition = Vector2.zero; }
        var glowRT = _glowEffect?.GetComponent<RectTransform>();
        if (glowRT != null) glowRT.localScale = Vector3.one * 1.3f;
        
        AudioManager.Instance?.RestoreBGM(0.1f);
    }

    IEnumerator DoCinematic(string oldFormId, string newFormId, string newName, int hp, int atk, int def, int oldHp, int oldAtk, int oldDef, string[] traits)
    {
        _playing = true;
        EnsureCanvas();
        CleanupState();
        AudioManager.Instance?.DuckBGM(0.2f);
        _cinema.enabled = true;
        _cinema.gameObject.SetActive(true);

        // === Phase 1: Darken + Slow Motion (0.4s) ===
        ScreenEffectsManager.Instance?.SlowMotion(0.1f, 0.5f);
        yield return FadeOverlay(Color.clear, new Color(0.1f, 0.03f, 0.2f, 0.88f), 0.4f);

        // === Phase 2: Old form intensifies and breaks apart (0.4s) ===
        LoadFormIcon(_oldFormIcon, oldFormId);
        _oldFormIcon.color = Color.white;
        _oldFormIcon.gameObject.SetActive(true);
        _newFormIcon.gameObject.SetActive(false);
        _nameText.text = "";
        _statsText.text = "";
        _ringEffect.gameObject.SetActive(false);
        _scanlineEffect.gameObject.SetActive(false);

        var oldRT = _oldFormIcon.GetComponent<RectTransform>();
        oldRT.localScale = Vector3.one;
        oldRT.anchoredPosition = Vector2.zero;

        // Initial pulse + shake on old form
        float t = 0;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.4f;
            float easeOut = 1f - Mathf.Pow(1f - p, 3f);
            
            oldRT.localScale = Vector3.one * (1f + easeOut * 0.6f + Mathf.Sin(p * Mathf.PI * 4f) * 0.05f);
            float alpha = 1f - p * 0.9f;
            _oldFormIcon.color = new Color(1, 1, 1, alpha);
            
            float shake = (1f - p) * 15f;
            oldRT.anchoredPosition = new Vector2(Random.Range(-shake, shake), Random.Range(-shake, shake));
            
            _bgOverlay.color = new Color(0.1f + p * 0.1f, 0.03f, 0.2f + p * 0.15f, 0.88f);
            
            // Add glitch-like color shift near end
            if (p > 0.6f)
            {
                float glitch = Mathf.Sin(p * 50f) * 0.1f;
                _bgOverlay.color = new Color(0.2f + glitch, 0.05f, 0.35f - glitch, 0.88f);
            }
            
            yield return null;
        }
        
        // === Phase 3: Explosion + transition (0.3s) ===
        // Multi-layer particle burst
        SpawnParticles(30, new Color(0.3f, 0.8f, 1f, 1f));
        SpawnParticles(25, new Color(0.7f, 0.3f, 1f, 1f));
        SpawnParticles(15, new Color(1f, 0.5f, 0.8f, 1f));
        
        ScreenEffectsManager.Instance?.FlashPossess();
        ScreenEffectsManager.Instance?.Shake(0.3f, 0.3f);
        AudioManager.Instance?.PlaySFX("possession_success");
        
        _oldFormIcon.gameObject.SetActive(false);

        // === Phase 4: New form emerges with power-up (0.8s) ===
        LoadFormIcon(_newFormIcon, newFormId);
        _newFormIcon.gameObject.SetActive(true);
        _newFormIcon.color = new Color(0.3f, 0.8f, 1f, 0);

        var newRT = _newFormIcon.GetComponent<RectTransform>();
        newRT.localScale = Vector3.one * 0.05f;
        newRT.anchoredPosition = Vector2.zero;

        _ringEffect.gameObject.SetActive(true);
        _ringEffect.color = new Color(0.3f, 0.8f, 1f, 0);
        var ringRT = _ringEffect.GetComponent<RectTransform>();
        ringRT.localScale = Vector3.one * 0.3f;

        // Add second ring
        SpawnParticles(10, new Color(0.3f, 0.8f, 1f, 0.8f));
        SpawnParticles(8, new Color(1f, 1f, 1f, 0.6f));

        t = 0;
        while (t < 0.8f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.8f;
            // Overshoot ease for dramatic effect
            float easeOutBack = 1f + 1.8f * Mathf.Pow(1f - p, 3f) + (-2.8f) * Mathf.Pow(1f - p, 2f);
            
            newRT.localScale = Vector3.one * Mathf.Lerp(0.05f, 1.25f, easeOutBack);
            float alpha = Mathf.Lerp(0, 1, Mathf.Clamp01(p * 2.5f));
            _newFormIcon.color = new Color(1, 1, 1, alpha);
            
            _bgOverlay.color = new Color(0.15f + p * 0.08f, 0.05f + p * 0.03f, 0.25f + p * 0.1f, 0.88f - p * 0.08f);
            
            float glowAlpha = Mathf.Lerp(0, 0.6f, p);
            _glowEffect.color = new Color(0.3f, 0.8f, 1f, glowAlpha);
            var glowRT = _glowEffect.GetComponent<RectTransform>();
            glowRT.localScale = Vector3.one * Mathf.Lerp(0.3f, 2.2f, p);
            
            float ringAlpha = Mathf.Lerp(0, 0.4f, p) * (1f - p * 0.3f);
            ringRT.localScale = Vector3.one * Mathf.Lerp(0.3f, 2.8f, p);
            _ringEffect.color = new Color(0.3f, 0.8f, 1f, ringAlpha);
            
            if (p > 0.5f)
            {
                _scanlineEffect.gameObject.SetActive(true);
                _scanlineEffect.color = new Color(1, 1, 1, Mathf.Lerp(0, 0.1f, (p - 0.5f) / 0.5f));
            }
            
            yield return null;
        }

        // === Phase 5: Settle into final pose (0.2s) ===
        t = 0;
        while (t < 0.2f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.2f;
            newRT.localScale = Vector3.one * Mathf.Lerp(1.25f, 1f, p);
            var glowRT2 = _glowEffect.GetComponent<RectTransform>();
            glowRT2.localScale = Vector3.one * Mathf.Lerp(2.2f, 1.4f, p);
            ringRT.localScale = Vector3.one * Mathf.Lerp(2.8f, 1.5f, p);
            _ringEffect.color = new Color(0.3f, 0.8f, 1f, 0.3f * (1f - p));
            yield return null;
        }
        newRT.localScale = Vector3.one;

        // === Phase 6: Reveal identity and stats (0.5s) ===
        _nameText.text = newName;
        _nameText.color = new Color(0.2f, 0.9f, 1f, 0);

        // Build stat strings with colored arrows and diffs
        string hpArrowStr, atkArrowStr, defArrowStr;
        string hpDiffStr, atkDiffStr, defDiffStr;
        string hpColorStr, atkColorStr, defColorStr;

        if (hp > oldHp) { hpArrowStr = " <color=#00FF99>↑</color>"; hpColorStr = "#00FF99"; hpDiffStr = "<color=#00FF99>(+" + (hp - oldHp) + ")</color>"; }
        else if (hp < oldHp) { hpArrowStr = " <color=#FF4444>↓</color>"; hpColorStr = "#FF4444"; hpDiffStr = "<color=#FF4444>(-" + (oldHp - hp) + ")</color>"; }
        else { hpArrowStr = " →"; hpColorStr = "#FFFFFF"; hpDiffStr = ""; }

        if (atk > oldAtk) { atkArrowStr = " <color=#FFAA00>↑</color>"; atkColorStr = "#FFAA00"; atkDiffStr = "<color=#FFAA00>(+" + (atk - oldAtk) + ")</color>"; }
        else if (atk < oldAtk) { atkArrowStr = " <color=#FF4444>↓</color>"; atkColorStr = "#FF4444"; atkDiffStr = "<color=#FF4444>(-" + (oldAtk - atk) + ")</color>"; }
        else { atkArrowStr = " →"; atkColorStr = "#FFFFFF"; atkDiffStr = ""; }

        if (def > oldDef) { defArrowStr = " <color=#00CCFF>↑</color>"; defColorStr = "#00CCFF"; defDiffStr = "<color=#00CCFF>(+" + (def - oldDef) + ")</color>"; }
        else if (def < oldDef) { defArrowStr = " <color=#FF4444>↓</color>"; defColorStr = "#FF4444"; defDiffStr = "<color=#FF4444>(-" + (oldDef - def) + ")</color>"; }
        else { defArrowStr = " →"; defColorStr = "#FFFFFF"; defDiffStr = ""; }

        string newStatsStr = "<size=110%>HP</size> <size=140%><color=" + hpColorStr + ">" + hp + "</color></size>" + hpArrowStr + hpDiffStr
            + "     <size=110%>ATK</size> <size=140%><color=" + atkColorStr + ">" + atk + "</color></size>" + atkArrowStr + atkDiffStr
            + "     <size=110%>DEF</size> <size=140%><color=" + defColorStr + ">" + def + "</color></size>" + defArrowStr + defDiffStr;
        string oldStatsStr = oldHp > 0 ? ("HP " + oldHp + "  ATK " + oldAtk + "  DEF " + oldDef) : "";

        _oldStatsText.text = oldStatsStr;
        _oldStatsText.color = new Color(0.4f, 0.4f, 0.5f, 0);
        _statsText.text = newStatsStr;
        _statsText.color = new Color(1f, 1f, 1f, 0);

        _statsBg.color = new Color(0.06f, 0.02f, 0.1f, 0);
        _statsBg.gameObject.SetActive(true);
        _statsBorder.gameObject.SetActive(true);
        _glowEffect.gameObject.SetActive(true);

        t = 0;
        while (t < 0.4f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.4f;
            _nameText.color = new Color(0.2f, 0.9f, 1f, p);
            yield return null;
        }

        if (oldHp > 0)
        {
            t = 0;
            while (t < 0.4f)
            {
                t += Time.unscaledDeltaTime;
                float p = t / 0.4f;
                float easeOut = 1f - Mathf.Pow(1f - p, 3f);
                _oldStatsText.color = new Color(0.4f, 0.4f, 0.5f, Mathf.Clamp01(p * 1.5f));
                var oldStatsRT = _oldStatsText.GetComponent<RectTransform>();
                oldStatsRT.anchoredPosition = new Vector2(0, Mathf.Lerp(75, 105, easeOut));
                yield return null;
            }
        }

        t = 0;
        while (t < 0.45f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.45f;
            float easeOutBack = 1f + 1.2f * Mathf.Pow(1f - p, 3f) + (-2.2f) * Mathf.Pow(1f - p, 2f);
            
            float statAlpha = p;
            _statsText.color = new Color(1f, 1f, 1f, statAlpha);

            _statsBg.color = new Color(0.06f, 0.02f, 0.1f, 0.85f * p);
            _statsBorder.color = new Color(0.2f, 0.9f, 1f, 0.7f * p);

            var statRT = _statsText.GetComponent<RectTransform>();
            statRT.localScale = Vector3.one * Mathf.Lerp(0.5f, 1.1f, easeOutBack);
            
            _glowEffect.color = new Color(0.2f, 0.9f, 1f, 0.5f * p);
            var glowRT3 = _glowEffect.GetComponent<RectTransform>();
            glowRT3.localScale = Vector3.one * Mathf.Lerp(1f, 1.6f, p);
            
            yield return null;
        }

        t = 0;
        while (t < 0.15f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.15f;
            var statRT = _statsText.GetComponent<RectTransform>();
            statRT.localScale = Vector3.one * Mathf.Lerp(1.1f, 1f, p);
            var glowRT4 = _glowEffect.GetComponent<RectTransform>();
            glowRT4.localScale = Vector3.one * Mathf.Lerp(1.6f, 1.3f, p);
            yield return null;
        }

        if (traits != null && traits.Length > 0)
        {
            _traitText.text = "◆ " + string.Join("  ◆ ", traits);
            _traitText.color = new Color(1f, 0.8f, 0.2f, 0);
            t = 0;
            while (t < 0.45f)
            {
                t += Time.unscaledDeltaTime;
                float p = t / 0.45f;
                float easeOut = 1f - Mathf.Pow(1f - p, 3f);
                _traitText.color = new Color(1f, 0.8f, 0.2f, easeOut);
                var traitRT = _traitText.GetComponent<RectTransform>();
                traitRT.anchoredPosition = new Vector2(0, Mathf.Lerp(-20, 20, easeOut));
                yield return null;
            }
        }

        // === Phase 7: Fade out (0.35s) ===
        t = 0;
        while (t < 0.35f)
        {
            t += Time.unscaledDeltaTime;
            float p = t / 0.35f;
            float easeIn = p * p;
            
            _bgOverlay.color = new Color(0.15f, 0.05f, 0.25f, 0.88f * (1f - easeIn));
            _nameText.color = new Color(0.2f, 0.9f, 1f, 1f - easeIn);
            _oldStatsText.color = new Color(0.4f, 0.4f, 0.5f, 1f - easeIn);
            _statsText.color = new Color(1f, 1f, 1f, 1f - easeIn);
            _traitText.color = new Color(1f, 0.8f, 0.2f, 1f - easeIn);
            _statsBg.color = new Color(0.06f, 0.02f, 0.1f, 0.85f * (1f - easeIn));
            _statsBorder.color = new Color(0.2f, 0.9f, 1f, 0.7f * (1f - easeIn));
            _glowEffect.color = new Color(0.2f, 0.9f, 1f, 0.5f * (1f - easeIn));
            _ringEffect.color = new Color(0.2f, 0.9f, 1f, 0.3f * (1f - easeIn));
            _newFormIcon.color = new Color(1, 1, 1, 1f - easeIn);
            
            newRT.localScale = Vector3.one * (1f + easeIn * 0.15f);
            
            yield return null;
        }

        _cinema.enabled = false;
        _cinema.gameObject.SetActive(false);
        ClearParticles();
        _playing = false;
        AudioManager.Instance?.RestoreBGM(0.2f);
        InvokeCallback();
    }

    void InvokeCallback()
    {
        if (_pendingCallback != null)
        {
            var cb = _pendingCallback;
            _pendingCallback = null;
            cb();
        }
    }

    public void ForceComplete()
    {
        if (_playing)
        {
            StopAllCoroutines();
            _playing = false;
            CleanupState();
            ScreenEffectsManager.Instance?.StopAllCoroutines();
            InvokeCallback();
        }
    }

    IEnumerator FadeOverlay(Color from, Color to, float dur)
    {
        float t = 0;
        while (t < dur)
        {
            t += Time.unscaledDeltaTime;
            float p = t / dur;
            float ease = p * p;
            _bgOverlay.color = Color.Lerp(from, to, ease);
            yield return null;
        }
        _bgOverlay.color = to;
    }

    void EnsureCanvas()
    {
        if (_cinema != null) return;

        var go = new GameObject("CinemaCanvas");
        go.transform.SetParent(transform);
        _cinema = go.AddComponent<Canvas>();
        _cinema.renderMode = RenderMode.ScreenSpaceOverlay;
        _cinema.sortingOrder = 500;
        go.AddComponent<CanvasScaler>().uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
        go.GetComponent<CanvasScaler>().referenceResolution = new Vector2(540, 960);
        var cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;

        _bgOverlay = MakeImg(go, "Bg", Color.clear);
        Stretch(_bgOverlay.gameObject);

        var iconSize = 140f;
        _oldFormIcon = MakeImg(go, "OldForm", Color.clear);
        _oldFormIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
        _oldFormIcon.preserveAspect = true;

        _newFormIcon = MakeImg(go, "NewForm", Color.clear);
        _newFormIcon.GetComponent<RectTransform>().sizeDelta = new Vector2(iconSize, iconSize);
        _newFormIcon.preserveAspect = true;

        _glowEffect = MakeImg(go, "Glow", Color.clear);
        var glowRT = _glowEffect.GetComponent<RectTransform>();
        glowRT.sizeDelta = new Vector2(200, 200);
        glowRT.localScale = Vector3.one * 1.3f;
        CreateRadialGradientSprite(_glowEffect, 200);

        _ringEffect = MakeImg(go, "Ring", Color.clear);
        var ringRT = _ringEffect.GetComponent<RectTransform>();
        ringRT.sizeDelta = new Vector2(220, 220);
        ringRT.localScale = Vector3.one;
        CreateRingSprite(_ringEffect, 220, 8f);

        _scanlineEffect = MakeImg(go, "Scanlines", Color.clear);
        var scanRT = _scanlineEffect.GetComponent<RectTransform>();
        scanRT.sizeDelta = new Vector2(540, 960);
        scanRT.anchorMin = Vector2.zero;
        scanRT.anchorMax = Vector2.one;
        scanRT.offsetMin = Vector2.zero;
        scanRT.offsetMax = Vector2.zero;
        CreateScanlineSprite(_scanlineEffect);

        _statsBg = MakeImg(go, "StatsBg", Color.clear);
        var statsBgRT = _statsBg.GetComponent<RectTransform>();
        statsBgRT.sizeDelta = new Vector2(340, 150);
        statsBgRT.anchoredPosition = new Vector2(0, 100);
        _statsBg.color = new Color(0.08f, 0.03f, 0.12f, 0.8f);
        _statsBg.gameObject.SetActive(false);

        _statsBorder = MakeImg(go, "StatsBorder", Color.clear);
        var borderRT = _statsBorder.GetComponent<RectTransform>();
        borderRT.sizeDelta = new Vector2(348, 158);
        borderRT.anchoredPosition = new Vector2(0, 100);
        _statsBorder.color = new Color(0.3f, 0.8f, 1f, 0.6f);
        _statsBorder.gameObject.SetActive(false);
        AddRoundedBorder(_statsBorder, 16f, 3f);

        _nameText = MakeTxt(go, "Name", "", 36, new Color(0.2f, 0.9f, 1f));
        _nameText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 190);

        _oldStatsText = MakeTxt(go, "OldStats", "", 16, new Color(0.4f, 0.4f, 0.5f));
        _oldStatsText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 110);

        _statsText = MakeTxt(go, "Stats", "", 24, new Color(1f, 1f, 1f));
        _statsText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 85);
        _statsText.supportRichText = true;

        _traitText = MakeTxt(go, "Traits", "", 16, new Color(1f, 0.8f, 0.2f));
        _traitText.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 30);

        _particleContainer = new GameObject("Particles");
        _particleContainer.transform.SetParent(go.transform, false);

        _cinema.gameObject.SetActive(false);
    }

    Image MakeImg(GameObject parent, string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent.transform, false);
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    Text MakeTxt(GameObject parent, string name, string text, int size, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Text));
        go.transform.SetParent(parent.transform, false);
        go.GetComponent<RectTransform>().sizeDelta = new Vector2(400, 40);
        var t = go.GetComponent<Text>();
        // 统一走 ThemeUIFonts,保证中文字体覆盖一致
        t.font = ThemeUIFonts.Get(size);
        t.text = text;
        t.fontSize = size;
        t.color = color;
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        return t;
    }

    void Stretch(GameObject go)
    {
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
    }

    void AddRoundedBorder(Image img, float radius, float borderWidth)
    {
        var rt = img.GetComponent<RectTransform>();
        float w = rt.sizeDelta.x;
        float h = rt.sizeDelta.y;
        
        Texture2D tex = new Texture2D(Mathf.CeilToInt(w), Mathf.CeilToInt(h), TextureFormat.RGBA32, false);
        Color[] pixels = new Color[tex.width * tex.height];
        
        for (int x = 0; x < tex.width; x++)
        {
            for (int y = 0; y < tex.height; y++)
            {
                float cx = w / 2;
                float cy = h / 2;
                float distX = Mathf.Abs(x - cx);
                float distY = Mathf.Abs(y - cy);
                
                float maxDistX = cx - radius;
                float maxDistY = cy - radius;
                
                bool inBorder = false;
                bool inCenter = false;
                
                if (distX <= maxDistX && distY <= maxDistY)
                {
                    inCenter = true;
                }
                else if (distX <= cx && distY <= cy)
                {
                    float cornerDistX = distX - maxDistX;
                    float cornerDistY = distY - maxDistY;
                    float cornerDist = Mathf.Sqrt(cornerDistX * cornerDistX + cornerDistY * cornerDistY);
                    
                    if (cornerDist <= radius)
                    {
                        inCenter = true;
                    }
                    else if (cornerDist <= radius + borderWidth)
                    {
                        inBorder = true;
                    }
                }
                else if (distX <= cx + borderWidth && distY <= cy + borderWidth)
                {
                    inBorder = true;
                }
                
                if (inCenter)
                {
                    pixels[y * tex.width + x] = Color.clear;
                }
                else if (inBorder)
                {
                    pixels[y * tex.width + x] = img.color;
                }
                else
                {
                    pixels[y * tex.width + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        img.sprite = Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Simple;
    }

    void CreateRadialGradientSprite(Image img, float size)
    {
        int res = 256;
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
                
                if (dist <= maxDist)
                {
                    float t = dist / maxDist;
                    float alpha = Mathf.Lerp(1f, 0f, t * t);
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
        
        img.sprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Simple;
    }

    void CreateRingSprite(Image img, float size, float thickness)
    {
        int res = 256;
        Texture2D tex = new Texture2D(res, res, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[res * res];
        
        float center = res / 2f;
        float maxDist = res / 2f;
        float innerRatio = (size - thickness) / size;
        
        for (int x = 0; x < res; x++)
        {
            for (int y = 0; y < res; y++)
            {
                float dx = x - center;
                float dy = y - center;
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                
                if (dist <= maxDist && dist >= maxDist * innerRatio)
                {
                    float t = Mathf.Abs(dist - maxDist * 0.5f * (1f + innerRatio)) / (maxDist * (1f - innerRatio) * 0.5f);
                    float alpha = 1f - t * t;
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
        
        img.sprite = Sprite.Create(tex, new Rect(0, 0, res, res), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Simple;
    }

    void CreateScanlineSprite(Image img)
    {
        int resX = 540;
        int resY = 960;
        Texture2D tex = new Texture2D(resX, resY, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[resX * resY];
        
        for (int y = 0; y < resY; y++)
        {
            for (int x = 0; x < resX; x++)
            {
                if (y % 4 == 0)
                {
                    pixels[y * resX + x] = new Color(1, 1, 1, 0.3f);
                }
                else
                {
                    pixels[y * resX + x] = Color.clear;
                }
            }
        }
        
        tex.SetPixels(pixels);
        tex.Apply();
        
        img.sprite = Sprite.Create(tex, new Rect(0, 0, resX, resY), new Vector2(0.5f, 0.5f));
        img.type = Image.Type.Simple;
    }

    void LoadFormIcon(Image img, string id)
    {
        if (img == null) return;

        if (string.IsNullOrEmpty(id) || id == "human") id = "c_" + (GameManager.Instance?.Player?.selectedClass ?? "titan");
        var tex = Resources.Load<Texture2D>("Icons/Monsters/" + id);
        if (tex == null) tex = Resources.Load<Texture2D>("Icons/Classes/" + id);
        if (tex != null)
        {
            var oldSprite = img.sprite;
            img.sprite = Sprite.Create(tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            if (oldSprite != null && oldSprite != img.sprite)
            {
                Destroy(oldSprite);
            }
        }
    }

    void SpawnParticles(int count, Color color)
    {
        if (_particleContainer == null) return;
        for (int i = 0; i < count; i++)
        {
            var go = new GameObject("P", typeof(RectTransform), typeof(Image));
            go.transform.SetParent(_particleContainer.transform, false);
            var rt = go.GetComponent<RectTransform>();
            
            float angle = Random.Range(0f, Mathf.PI * 2f);
            float radius = Random.Range(10f, 60f);
            rt.anchoredPosition = new Vector2(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius);
            
            float baseSize = Random.Range(4f, 18f);
            rt.sizeDelta = new Vector2(baseSize, baseSize);
            rt.localScale = Vector3.one * Random.Range(0.5f, 1.5f);
            
            go.GetComponent<Image>().color = color;
            go.GetComponent<Image>().raycastTarget = false;
            StartCoroutine(AnimateParticle(rt, go.GetComponent<Image>(), angle, baseSize));
        }
    }

    IEnumerator AnimateParticle(RectTransform rt, Image img, float spawnAngle, float baseSize)
    {
        Vector2 vel = new Vector2(Mathf.Cos(spawnAngle) * Random.Range(120f, 280f), Mathf.Sin(spawnAngle) * Random.Range(120f, 280f));
        float life = Random.Range(0.6f, 1.4f);
        float t = 0;
        Color c = img.color;
        float rotationSpeed = Random.Range(-180f, 180f);
        float startScale = rt.localScale.x;
        
        while (t < life)
        {
            t += Time.unscaledDeltaTime;
            float p = Mathf.Clamp01(t / life);
            float easeOut = 1f - Mathf.Pow(1f - p, 2.5f);
            
            rt.anchoredPosition += vel * Time.unscaledDeltaTime;
            vel *= 0.94f;
            vel += new Vector2(Random.Range(-20f, 20f), Random.Range(-20f, 20f)) * Time.unscaledDeltaTime;
            
            c.a = (1f - p) * (1f - p * 0.3f);
            img.color = c;
            
            float s = Mathf.Lerp(startScale, 0.15f, easeOut);
            if (!float.IsNaN(s) && !float.IsInfinity(s))
                rt.localScale = Vector3.one * s;
            
            rt.Rotate(0, 0, rotationSpeed * Time.unscaledDeltaTime);
            
            yield return null;
        }
        Destroy(rt.gameObject);
    }

    void ClearParticles()
    {
        if (_particleContainer == null) return;
        for (int i = _particleContainer.transform.childCount - 1; i >= 0; i--)
            Destroy(_particleContainer.transform.GetChild(i).gameObject);
    }

    protected override void OnDestroy()
    {
        StopAllCoroutines();
        if (_cinema != null)
        {
            _cinema.enabled = false;
            _cinema.gameObject.SetActive(false);
        }
        ClearParticles();
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        _pendingCallback = null;
        _playing = false;
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();
        if (_cinema != null)
        {
            _cinema.enabled = false;
            _cinema.gameObject.SetActive(false);
        }
        ClearParticles();
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
        _playing = false;
        base.OnDisable();
    }
}