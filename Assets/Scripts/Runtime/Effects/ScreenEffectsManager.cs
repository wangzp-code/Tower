using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class ScreenEffectsManager : SingletonBase<ScreenEffectsManager>
{
    Image _flashOverlay;
    Image _pollutionOverlay;
    Image _vignetteOverlay;
    Image _glitchOverlay;
    Image _scanlineOverlay;
    Image _noiseOverlay;
    Image _rgbSepR;
    Image _rgbSepB;
    Image _rgbSepG;
    Canvas _fxCanvas;
    Camera _mainCam;
    Vector3 _camOrigPos;
    bool _shaking;
    BioPunkPostFX _postFx;

    // Pollution state
    float _pollGlitchTimer;
    float _pollFlickerTimer;
    float _pollPulsePhase;
    float _pollWavePhase;
    Sprite _scanlineSprite;
    Sprite _noiseSprite;
    bool _glitchActive;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        CreateFXCanvas();
        _mainCam = Camera.main;
        if (_mainCam != null) _camOrigPos = _mainCam.transform.localPosition;
        ClearAllEffects();
    }

    void CreateFXCanvas()
    {
        var go = new GameObject("FXCanvas");
        go.transform.SetParent(transform);
        _fxCanvas = go.AddComponent<Canvas>();
        _fxCanvas.renderMode = RenderMode.ScreenSpaceOverlay;
        _fxCanvas.sortingOrder = 999;
        go.AddComponent<CanvasScaler>();

        _flashOverlay = CreateOverlay("Flash", Color.clear);
        _pollutionOverlay = CreateOverlay("Pollution", Color.clear);
        _vignetteOverlay = CreateOverlay("Vignette", Color.clear);
        _scanlineOverlay = CreateOverlay("Scanlines", Color.clear);
        _noiseOverlay = CreateOverlay("Noise", Color.clear);
        _glitchOverlay = CreateOverlay("Glitch", Color.clear);
        _rgbSepR = CreateOverlay("RGB_SepR", Color.clear);
        _rgbSepG = CreateOverlay("RGB_SepG", Color.clear);
        _rgbSepB = CreateOverlay("RGB_SepB", Color.clear);

        _scanlineSprite = CreateScanlineTexture(540, 960);
        if (_scanlineSprite)
        {
            _scanlineOverlay.sprite = _scanlineSprite;
            _scanlineOverlay.color = Color.clear;
        }

        _noiseSprite = CreateNoiseTexture(540, 960);
        if (_noiseSprite)
        {
            _noiseOverlay.sprite = _noiseSprite;
            _noiseOverlay.color = Color.clear;
        }

        // All non-interactive
        var cg = go.AddComponent<CanvasGroup>();
        cg.blocksRaycasts = false;
        cg.interactable = false;

        _postFx = go.AddComponent<BioPunkPostFX>();
        _postFx.Initialize(_fxCanvas);
        bool highQuality = !Application.isMobilePlatform && SystemInfo.systemMemorySize >= 4096;
        _postFx.ApplyQualityTier(highQuality);
    }

    Image CreateOverlay(string name, Color color)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(_fxCanvas.transform, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero; rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero; rt.offsetMax = Vector2.zero;
        var img = go.GetComponent<Image>();
        img.color = color;
        img.raycastTarget = false;
        return img;
    }

    Sprite CreateScanlineTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float yNorm = (float)y / h;
                float baseAlpha = 0.18f;

                if (y % 3 == 0)
                    pixels[y * w + x] = new Color(1f, 1f, 1f, baseAlpha * 1.4f);
                else if (y % 3 == 1)
                    pixels[y * w + x] = new Color(1f, 1f, 1f, baseAlpha * 0.5f);
                else
                    pixels[y * w + x] = Color.clear;
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    Sprite CreateNoiseTexture(int w, int h)
    {
        Texture2D tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        Color[] pixels = new Color[w * h];

        for (int y = 0; y < h; y++)
        {
            for (int x = 0; x < w; x++)
            {
                float n = Mathf.PerlinNoise(x * 0.02f, y * 0.02f) * 0.5f + 0.5f;
                pixels[y * w + x] = new Color(n, n, n, n * 0.4f);
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, w, h), new Vector2(0.5f, 0.5f), 100f);
    }

    void Update()
    {
        if (CompleteGameSystem.Instance != null)
        {
            var screen = CompleteGameSystem.Instance.CurrentScreen;
            if (screen == CompleteGameSystem.RunScreen.GameOver ||
                screen == CompleteGameSystem.RunScreen.Ending ||
                screen == CompleteGameSystem.RunScreen.MainMenu ||
                screen == CompleteGameSystem.RunScreen.CharacterSelect)
            {
                ClearAllEffects();
                return;
            }
        }
        UpdatePollutionEffects();
    }

    void ClearAllEffects()
    {
        if (_pollutionOverlay) _pollutionOverlay.color = Color.clear;
        if (_vignetteOverlay) _vignetteOverlay.color = Color.clear;
        if (_glitchOverlay) _glitchOverlay.color = Color.clear;
        if (_scanlineOverlay)
        {
            _scanlineOverlay.color = Color.clear;
            var rt = _scanlineOverlay.GetComponent<RectTransform>();
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        if (_noiseOverlay) _noiseOverlay.color = Color.clear;
        if (_rgbSepR) { _rgbSepR.color = Color.clear; ResetOffset(_rgbSepR); }
        if (_rgbSepG) { _rgbSepG.color = Color.clear; ResetOffset(_rgbSepG); }
        if (_rgbSepB) { _rgbSepB.color = Color.clear; ResetOffset(_rgbSepB); }
        if (_postFx != null) _postFx.SetPollutionIntensity(0f);
        if (_mainCam != null) _mainCam.transform.localPosition = _camOrigPos;
        _pollGlitchTimer = 0f;
        _pollFlickerTimer = 0f;
    }

    void ResetOffset(Image img)
    {
        var rt = img.GetComponent<RectTransform>();
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
    }

    // =================== SCREEN SHAKE ===================
    public void Shake(float intensity = 0.15f, float duration = 0.15f)
    {
        if (!_shaking && _mainCam != null)
            StartCoroutine(DoShake(intensity, duration));
    }

    public void ShakeHeavy()
    {
        Shake(0.35f, 0.25f);
    }

    IEnumerator DoShake(float intensity, float duration)
    {
        _shaking = true;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float decay = 1f - (elapsed / duration);
            float x = Random.Range(-1f, 1f) * intensity * decay;
            float y = Random.Range(-1f, 1f) * intensity * decay;
            _mainCam.transform.localPosition = _camOrigPos + new Vector3(x, y, 0);
            elapsed += Time.unscaledDeltaTime;
            yield return null;
        }
        _mainCam.transform.localPosition = _camOrigPos;
        _shaking = false;
    }

    // =================== SCREEN FLASH ===================
    public void FlashDamage()
    {
        StartCoroutine(DoFlash(new Color(
            ParasiteTowerColorScheme.CriticalRed.r,
            ParasiteTowerColorScheme.CriticalRed.g * 0.1f,
            ParasiteTowerColorScheme.CriticalRed.b * 0.2f, 0.35f), 0.15f));
        Shake(0.08f, 0.1f);
    }

    public void FlashCrit()
    {
        StartCoroutine(DoFlash(new Color(
            ParasiteTowerColorScheme.White.r,
            ParasiteTowerColorScheme.HealthGreen.g,
            ParasiteTowerColorScheme.White.b, 0.5f), 0.08f));
        ShakeHeavy();
    }

    public void FlashHeal()
    {
        StartCoroutine(DoFlash(new Color(
            ParasiteTowerColorScheme.HealthGreen.r * 0.2f,
            ParasiteTowerColorScheme.HealthGreen.g,
            ParasiteTowerColorScheme.HealthGreen.b * 0.6f, 0.2f), 0.3f));
    }

    public void FlashPossess()
    {
        StartCoroutine(DoFlashSequence());
        Shake(0.15f, 0.4f);
    }

    public void FlashPossessFail()
    {
        StartCoroutine(DoPossessFailFlash());
        Shake(0.2f, 0.5f);
    }

    IEnumerator DoFlashSequence()
    {
        yield return DoFlash(new Color(1f, 1f, 1f, 0.8f), 0.1f);
        yield return new WaitForSecondsRealtime(0.05f);
        yield return DoFlash(new Color(
            ParasiteTowerColorScheme.ColorParasite.r * 0.6f,
            0.8f,
            1f, 0.5f), 0.4f);
    }

    IEnumerator DoPossessFailFlash()
    {
        yield return DoFlash(new Color(
            ParasiteTowerColorScheme.CriticalRed.r,
            ParasiteTowerColorScheme.CriticalRed.g * 0.1f,
            ParasiteTowerColorScheme.CriticalRed.b * 0.2f, 0.6f), 0.12f);
        yield return new WaitForSecondsRealtime(0.03f);
        yield return DoFlash(new Color(
            ParasiteTowerColorScheme.CriticalRed.r,
            ParasiteTowerColorScheme.CriticalRed.g * 0.05f,
            ParasiteTowerColorScheme.CriticalRed.b * 0.15f, 0.4f), 0.15f);
        yield return new WaitForSecondsRealtime(0.05f);
        yield return DoFlash(new Color(
            ParasiteTowerColorScheme.DangerOrange.r,
            ParasiteTowerColorScheme.DangerOrange.g * 0.1f,
            ParasiteTowerColorScheme.DangerOrange.b * 0.1f, 0.3f), 0.2f);
    }

    public void FlashBossPhase()
    {
        StartCoroutine(DoFlash(new Color(
            ParasiteTowerColorScheme.CriticalRed.r,
            ParasiteTowerColorScheme.CriticalRed.g * 0.05f,
            ParasiteTowerColorScheme.CriticalRed.b * 0.15f, 0.5f), 0.6f));
        ShakeHeavy();
    }

    public void FlashResonance()
    {
        StartCoroutine(DoFlash(new Color(
            0f,
            0.8f,
            1f, 0.35f), 0.4f));
        Shake(0.1f, 0.2f);
    }

    public void FlashScreen(Color color, float duration)
    {
        StartCoroutine(DoFlash(color, duration));
    }

    IEnumerator DoFlash(Color color, float duration)
    {
        if (_flashOverlay == null) yield break;
        _flashOverlay.color = color;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            float t = elapsed / duration;
            var c = color;
            c.a = color.a * (1f - t);
            _flashOverlay.color = c;
            elapsed += Time.deltaTime;
            yield return null;
        }
        _flashOverlay.color = Color.clear;
    }

    // =================== SLOW MOTION ===================
    public void SlowMotion(float scale = 0.15f, float duration = 0.12f)
    {
        StartCoroutine(DoSlowMotion(scale, duration));
    }

    IEnumerator DoSlowMotion(float scale, float duration)
    {
        Time.timeScale = scale;
        Time.fixedDeltaTime = 0.02f * scale;
        yield return new WaitForSecondsRealtime(duration);
        Time.timeScale = 1f;
        Time.fixedDeltaTime = 0.02f;
    }

    // =================== POLLUTION VISUAL ===================
    void UpdatePollutionEffects()
    {
        if (GameManager.Instance == null || GameManager.Instance.Player == null)
        {
            ClearPollutionLayers();
            return;
        }
        float poll = GameManager.Instance.Player.pollution;
        _pollPulsePhase += Time.deltaTime;
        _pollWavePhase += Time.deltaTime;

        if (poll < 90f)
        {
            ClearPollutionLayers();
            return;
        }

        float t = Mathf.InverseLerp(90f, 100f, poll);
        float pulse = 0.5f + 0.5f * Mathf.Sin(_pollPulsePhase * (1f + t * 3f));
        float intensity = Mathf.Clamp01((poll - 90f) / 10f);

        // === 污染叠加色 — 90%+ 才出现 ===
        Color pollColor;
        if (poll < 90f)
        {
            pollColor = Color.clear;
        }
        else if (poll < 95f)
        {
            float highT = Mathf.InverseLerp(90f, 95f, poll);
            float bloodPulse = pulse * 0.5f + 0.5f;
            float a = Mathf.Lerp(0.3f, 0.55f, highT) * bloodPulse;
            pollColor = new Color(0.6f + highT * 0.3f, 0.02f, 0.05f, a);
        }
        else
        {
            float highT = Mathf.InverseLerp(95f, 100f, poll);
            float bloodPulse = pulse * 0.4f + 0.6f;
            float flicker = Mathf.PerlinNoise(Time.time * 15f, 0f) * 0.3f + 0.7f;
            float a = Mathf.Lerp(0.65f, 0.9f, highT) * bloodPulse * flicker;
            float whiteness = (poll - 95f) / 5f;
            pollColor = new Color(1f, whiteness * 0.25f, whiteness * 0.25f, a);
        }

        if (poll >= 90f)
        {
            float hueShift = ParasiteTowerEffects.GetPollutionHueShift(poll, _pollPulsePhase);
            if (hueShift != 0f)
                pollColor = ShiftHue(pollColor, hueShift);
        }

        if (_postFx != null)
        {
            float fxIntensity = poll >= 90f ? intensity : 0f;
            _postFx.SetPollutionIntensity(fxIntensity);
        }

        if (_pollutionOverlay) _pollutionOverlay.color = pollColor;

        // === 暗角 — 90%+ 才出现 ===
        float vigAlpha;
        Color vigColor;
        if (poll < 90f)
        {
            vigAlpha = 0f;
            vigColor = Color.clear;
        }
        else if (poll < 95f)
        {
            vigAlpha = Mathf.Lerp(0.15f, 0.4f, (poll - 90f) / 5f);
            float vigPulse = pulse * 0.3f + 0.7f;
            float flicker = Mathf.PerlinNoise(Time.time * 8f, 0f) * 0.15f + 0.85f;
            vigColor = new Color(0.1f, 0.01f, 0.02f, vigAlpha * vigPulse * flicker);
        }
        else
        {
            vigAlpha = Mathf.Lerp(0.4f, 0.7f, (poll - 95f) / 5f);
            float vigPulse = pulse * 0.4f + 0.6f;
            float flicker = Mathf.PerlinNoise(Time.time * 12f, 0f) * 0.25f + 0.75f;
            vigColor = new Color(0.12f, 0.01f, 0.03f, vigAlpha * vigPulse * flicker);
        }
        if (_vignetteOverlay) _vignetteOverlay.color = vigColor;

        // === 扫描线 — 90%+ 出现并滚动 ===
        if (_scanlineOverlay)
        {
            if (poll >= 90f)
            {
                float scanAlpha;
                if (poll < 95f)
                {
                    scanAlpha = Mathf.Lerp(0.1f, 0.25f, (poll - 90f) / 5f);
                }
                else
                {
                    scanAlpha = Mathf.Lerp(0.25f, 0.5f, (poll - 95f) / 5f);
                }
                float scanPulse = pulse * 0.3f + 0.7f;
                _scanlineOverlay.color = new Color(1f, 0.6f + 0.4f * scanPulse, 0.6f + 0.4f * scanPulse, scanAlpha * scanPulse);

                var scanRT = _scanlineOverlay.GetComponent<RectTransform>();
                float scrollSpeed = (poll - 90f) / 10f * 80f + 20f;
                float scrollY = Mathf.Repeat(_pollWavePhase * scrollSpeed, 100f) - 50f;
                scanRT.offsetMin = new Vector2(0, scrollY);
                scanRT.offsetMax = new Vector2(0, scrollY);
            }
            else
            {
                _scanlineOverlay.color = Color.clear;
                var scanRT = _scanlineOverlay.GetComponent<RectTransform>();
                scanRT.offsetMin = Vector2.zero;
                scanRT.offsetMax = Vector2.zero;
            }
        }

        // === 噪点/颗粒 — 90%+ 出现，随污染增强 ===
        if (_noiseOverlay)
        {
            if (poll >= 90f)
            {
                float noiseAlpha;
                if (poll < 95f)
                {
                    noiseAlpha = Mathf.Lerp(0.04f, 0.15f, (poll - 90f) / 5f);
                }
                else
                {
                    noiseAlpha = Mathf.Lerp(0.15f, 0.28f, (poll - 95f) / 5f);
                }
                float noiseFlicker = Mathf.PerlinNoise(Time.time * 15f, 0f) * 0.3f + 0.7f;
                _noiseOverlay.color = new Color(0.7f, 0.5f, 0.5f, noiseAlpha * noiseFlicker);
            }
            else
            {
                _noiseOverlay.color = Color.clear;
            }
        }

        // === RGB 色差分离 — 75%+ 出现，大幅增强 ===
        UpdateRGBSeparation(poll, pulse);

        // === 屏幕闪烁 — 95%+ 高频随机闪烁 ===
        UpdatePollutionFlicker(poll);

        // === Glitch 触发 — 90% 开始 ===
        UpdateGlitchSystem(poll);

        // === 污染脉动下的持续微抖动 — 90%+ ===
        if (poll >= 90f && !_shaking && _mainCam != null)
        {
            float microShake = (poll - 90f) / 10f * 0.015f + 0.005f;
            float decay = 1f;
            float mx = Random.Range(-1f, 1f) * microShake * decay;
            float my = Random.Range(-1f, 1f) * microShake * decay;
            _mainCam.transform.localPosition = _camOrigPos + new Vector3(mx, my, 0);
        }
        else if (_mainCam != null && !_shaking)
        {
            _mainCam.transform.localPosition = _camOrigPos;
        }
    }

    void ClearPollutionLayers()
    {
        if (_pollutionOverlay) _pollutionOverlay.color = Color.clear;
        if (_vignetteOverlay) _vignetteOverlay.color = Color.clear;
        if (_scanlineOverlay)
        {
            _scanlineOverlay.color = Color.clear;
            var rt = _scanlineOverlay.GetComponent<RectTransform>();
            rt.offsetMin = Vector2.zero;
            rt.offsetMax = Vector2.zero;
        }
        if (_noiseOverlay) _noiseOverlay.color = Color.clear;
        ResetOffset(_rgbSepR); _rgbSepR.color = Color.clear;
        ResetOffset(_rgbSepG); _rgbSepG.color = Color.clear;
        ResetOffset(_rgbSepB); _rgbSepB.color = Color.clear;
        _pollGlitchTimer = 0f;
        _pollFlickerTimer = 0f;
        if (_postFx != null) _postFx.SetPollutionIntensity(0f);
        if (_mainCam != null) _mainCam.transform.localPosition = _camOrigPos;
    }

    void UpdateRGBSeparation(float poll, float pulse)
    {
        if (poll < 90f)
        {
            if (_rgbSepR) { _rgbSepR.color = Color.clear; ResetOffset(_rgbSepR); }
            if (_rgbSepG) { _rgbSepG.color = Color.clear; ResetOffset(_rgbSepG); }
            if (_rgbSepB) { _rgbSepB.color = Color.clear; ResetOffset(_rgbSepB); }
            return;
        }

        float sepIntensity;
        if (poll < 95f)
        {
            sepIntensity = Mathf.Lerp(3f, 12f, (poll - 90f) / 5f);
        }
        else
        {
            sepIntensity = Mathf.Lerp(12f, 22f, (poll - 95f) / 5f);
        }

        float sepPulse = 0.5f + 0.5f * Mathf.Sin(_pollPulsePhase * 6f);
        float waveY = Mathf.Sin(_pollWavePhase * 3f) * sepIntensity * 0.3f;
        float shift = sepIntensity * sepPulse;
        float jitter = poll >= 90f ? Random.Range(-1.5f, 1.5f) * (poll - 90f) / 10f : 0f;

        if (_rgbSepR)
        {
            float rAlpha = poll >= 93f ? 0.55f + pulse * 0.25f : 0.45f + pulse * 0.15f;
            _rgbSepR.color = new Color(1f, 0f, 0f, rAlpha);
            var rt = _rgbSepR.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(-shift + jitter, waveY);
            rt.offsetMax = new Vector2(-shift + jitter, waveY);
        }

        if (_rgbSepG && poll >= 93f)
        {
            float gAlpha = poll >= 95f ? 0.3f + pulse * 0.2f : 0.2f;
            _rgbSepG.color = new Color(0f, 1f, 0f, gAlpha);
            var rt = _rgbSepG.GetComponent<RectTransform>();
            float gShift = shift * 0.3f + jitter * 0.5f;
            rt.offsetMin = new Vector2(-gShift, -waveY * 0.5f);
            rt.offsetMax = new Vector2(-gShift, -waveY * 0.5f);
        }
        else if (_rgbSepG)
        {
            _rgbSepG.color = Color.clear;
            ResetOffset(_rgbSepG);
        }

        if (_rgbSepB)
        {
            float bAlpha = poll >= 93f ? 0.55f + pulse * 0.25f : 0.45f + pulse * 0.15f;
            _rgbSepB.color = new Color(0f, 0f, 1f, bAlpha);
            var rt = _rgbSepB.GetComponent<RectTransform>();
            rt.offsetMin = new Vector2(shift + jitter, -waveY);
            rt.offsetMax = new Vector2(shift + jitter, -waveY);
        }
    }

    void UpdatePollutionFlicker(float poll)
    {
        if (poll < 95f)
        {
            _pollFlickerTimer = 0f;
            return;
        }

        _pollFlickerTimer += Time.deltaTime;
        float flickerInterval = Mathf.Lerp(0.3f, 1.2f, (poll - 95f) / 5f);

        if (_pollFlickerTimer > flickerInterval)
        {
            _pollFlickerTimer = 0f;
            StartCoroutine(DoPollutionFlicker(poll));
        }
    }

    IEnumerator DoPollutionFlicker(float poll)
    {
        float intensity = Mathf.Clamp01((poll - 95f) / 5f);
        int flickerCount = poll >= 98f ? 4 : 2;

        for (int i = 0; i < flickerCount; i++)
        {
            if (_pollutionOverlay)
            {
                var c = _pollutionOverlay.color;
                c.a *= (0.3f + intensity * 0.3f);
                _pollutionOverlay.color = c;
            }
            if (_scanlineOverlay)
            {
                var c = _scanlineOverlay.color;
                c.a *= (0.2f + intensity * 0.2f);
                _scanlineOverlay.color = c;
            }

            if (_mainCam != null)
            {
                float fx = Random.Range(-1f, 1f) * 0.03f * intensity;
                float fy = Random.Range(-1f, 1f) * 0.03f * intensity;
                _mainCam.transform.localPosition = _camOrigPos + new Vector3(fx, fy, 0);
            }

            yield return new WaitForSeconds(Random.Range(0.02f, 0.06f));
        }
    }

    void UpdateGlitchSystem(float poll)
    {
        if (_glitchActive) return;

        if (poll < 90f) return;

        _pollGlitchTimer += Time.deltaTime;
        float glitchInterval;
        if (poll >= 95f) glitchInterval = Random.Range(0.3f, 0.8f);
        else if (poll >= 93f) glitchInterval = Random.Range(0.6f, 1.5f);
        else glitchInterval = Random.Range(1.0f, 2.2f);

        if (_pollGlitchTimer > glitchInterval)
        {
            _pollGlitchTimer = 0f;
            StartCoroutine(DoGlitch(poll));
        }
    }

    static Color ShiftHue(Color c, float degrees)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        h = (h + degrees / 360f) % 1f;
        var shifted = Color.HSVToRGB(h, s, v);
        shifted.a = c.a;
        return shifted;
    }

    IEnumerator DoGlitch(float poll)
    {
        if (_glitchOverlay == null) yield break;
        _glitchActive = true;

        float intensity = Mathf.Clamp01((poll - 90f) / 10f);
        int phases = poll >= 95f ? 6 : poll >= 93f ? 4 : 2;
        float baseAlpha = Mathf.Lerp(0.15f, 0.5f, intensity);
        float duration = Mathf.Lerp(0.15f, 0.45f, intensity);

        for (int i = 0; i < phases; i++)
        {
            float r = Random.value;
            float g = Random.value;
            float b = Random.value;

            float phaseAlpha = baseAlpha * Random.Range(0.5f, 1f);
            _glitchOverlay.color = new Color(r, g, b, phaseAlpha);

            if (_mainCam != null)
            {
                float camIntensity = 0.03f + intensity * 0.07f;
                Vector3 offset = new Vector3(
                    Random.Range(-1f, 1f) * camIntensity,
                    Random.Range(-1f, 1f) * camIntensity,
                    0);
                _mainCam.transform.localPosition = _camOrigPos + offset;
            }

            if (poll >= 90f && _rgbSepR != null && _rgbSepB != null)
            {
                var rtR = _rgbSepR.GetComponent<RectTransform>();
                var rtB = _rgbSepB.GetComponent<RectTransform>();
                float extraShift = Random.Range(-5f, 5f) * intensity;
                rtR.offsetMin = new Vector2(rtR.offsetMin.x + extraShift, rtR.offsetMin.y);
                rtR.offsetMax = new Vector2(rtR.offsetMax.x + extraShift, rtR.offsetMax.y);
                rtB.offsetMin = new Vector2(rtB.offsetMin.x - extraShift, rtB.offsetMin.y);
                rtB.offsetMax = new Vector2(rtB.offsetMax.x - extraShift, rtB.offsetMax.y);
            }

            yield return new WaitForSeconds(duration / phases + Random.Range(-0.01f, 0.02f));
        }

        if (intensity > 0.75f)
        {
            float whiteAlpha = baseAlpha * 0.7f;
            _glitchOverlay.color = new Color(1f, 1f, 1f, whiteAlpha);
            if (_mainCam != null)
            {
                float wf = Random.Range(-1f, 1f) * 0.05f * intensity;
                _mainCam.transform.localPosition = _camOrigPos + new Vector3(wf, wf * 0.5f, 0);
            }
            yield return new WaitForSeconds(Random.Range(0.04f, 0.1f));
        }

        if (_mainCam != null)
            _mainCam.transform.localPosition = _camOrigPos;

        _glitchOverlay.color = Color.clear;
        _glitchActive = false;
    }

    protected override void OnDestroy()
    {
        StopAllCoroutines();
        ClearAllEffects();
        if (_mainCam != null) _mainCam.transform.localPosition = _camOrigPos;
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        StopAllCoroutines();
        ClearAllEffects();
        if (_mainCam != null) _mainCam.transform.localPosition = _camOrigPos;
        base.OnDisable();
    }
}
