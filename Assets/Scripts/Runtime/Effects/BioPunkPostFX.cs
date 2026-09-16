using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 生物朋克轻量后处理叠加层：暗角 + 颗粒 + 扫描线 + 污染红光脉动 + 污染 flash
/// 无需 URP Post Stack, 纯 UI 层实现.
/// </summary>
public class BioPunkPostFX : MonoBehaviour
{
    Image _vignette;
    Image _grain;
    Image _scanlines;
    Image _redPulse;
    float _grainSeed;
    float _pollutionIntensity;
    float _targetIntensity;
    float _pollutionFlashAmount;   // 一次性污染 flash 叠加层 alpha
    float _pollutionFlashDecay;

    Texture2D _scanlineTex;

    public void Initialize(Canvas parentCanvas)
    {
        if (_vignette != null) return;

        _vignette = CreateOverlay(parentCanvas.transform, "BioVignette");
        _grain = CreateOverlay(parentCanvas.transform, "BioGrain");
        _scanlines = CreateOverlay(parentCanvas.transform, "BioScanlines");
        _redPulse = CreateOverlay(parentCanvas.transform, "BioRedPulse");

        // 程序化生成扫描线纹理: 交替明暗水平条纹
        _scanlineTex = CreateScanlineTexture(256, 128, 0.6f);
        if (_scanlines != null)
        {
            _scanlines.sprite = Sprite.Create(_scanlineTex, new Rect(0, 0, _scanlineTex.width, _scanlineTex.height), new Vector2(0.5f, 0.5f));
            _scanlines.color = new Color(0.7f, 0.55f, 0.55f, 0f);
        }

        ApplyQualityTier();
    }

    Image CreateOverlay(Transform parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform), typeof(Image));
        go.transform.SetParent(parent, false);
        var rt = go.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var img = go.GetComponent<Image>();
        img.raycastTarget = false;
        img.color = Color.clear;
        return img;
    }

    Texture2D CreateScanlineTexture(int w, int h, float darkRatio)
    {
        var tex = new Texture2D(w, h, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        var pixels = new Color[w * h];
        for (int y = 0; y < h; y++)
        {
            bool dark = (y % 3) < Mathf.RoundToInt(darkRatio * 3f);
            float a = dark ? 0.35f : 0.05f;
            for (int x = 0; x < w; x++)
                pixels[y * w + x] = new Color(1, 1, 1, a);
        }
        tex.SetPixels(pixels);
        tex.Apply();
        return tex;
    }

    void Update()
    {
        if (_grain == null) return;

        // 1) 污染强度插值
        _pollutionIntensity = Mathf.Lerp(_pollutionIntensity, _targetIntensity, Time.deltaTime * 3f);

        // 2) 污染 flash 衰减
        if (_pollutionFlashAmount > 0.01f)
        {
            _pollutionFlashDecay -= Time.deltaTime;
            if (_pollutionFlashDecay <= 0) _pollutionFlashAmount = 0f;
            else _pollutionFlashAmount = Mathf.Lerp(_pollutionFlashAmount, 0f, Time.deltaTime * 10f);
        }

        // 3) 颗粒层 — 随污染增强
        _grainSeed += Time.deltaTime * 12f;
        float baseGrain = ParasiteTowerArtOptimization.VFXEnhancements.PostProcessing.FilmGrainIntensity;
        float pollutionBoost = _pollutionIntensity * 0.4f;
        float grain = baseGrain + pollutionBoost;
        if (grain <= 0.001f)
        {
            _grain.color = Color.clear;
        }
        else
        {
            float n = Mathf.PerlinNoise(_grainSeed, 0.37f);
            float grainFlicker = n * 0.7f + 0.3f;
            _grain.color = new Color(
                0.5f + _pollutionIntensity * 0.3f,
                0.3f + _pollutionIntensity * 0.2f,
                0.3f + _pollutionIntensity * 0.2f,
                n * grain * grainFlicker);
        }

        // 4) 扫描线层 — 污染 > 0.3 时淡入, 频率随污染加快
        if (_scanlines != null)
        {
            if (_pollutionIntensity <= 0.01f)
            {
                _scanlines.color = Color.clear;
            }
            else
            {
                float scanlineAlpha = Mathf.SmoothStep(0f, 0.6f, _pollutionIntensity) * 0.45f;
                float scanlineColorShift = _pollutionIntensity;
                _scanlines.color = new Color(
                    0.65f + scanlineColorShift * 0.25f,
                    0.45f - scanlineColorShift * 0.1f,
                    0.45f - scanlineColorShift * 0.1f,
                    scanlineAlpha);
            }
        }

        // 5) 红色脉动层 + 污染 flash 叠加
        float combinedPollution = Mathf.Min(1f, _pollutionIntensity + _pollutionFlashAmount);
        if (_redPulse != null)
        {
            if (combinedPollution <= 0.01f)
            {
                _redPulse.color = Color.clear;
            }
            else
            {
                float pulseFreq = 0.5f + combinedPollution * 2.5f;   // 污染越重脉动越快
                float pulse = Mathf.Sin(Time.time * pulseFreq) * 0.5f + 0.5f;
                float pulseAlpha = Mathf.SmoothStep(0f, 0.7f, _pollutionIntensity) * (0.18f + pulse * 0.22f);
                float flashBoost = _pollutionFlashAmount * 0.5f;
                _redPulse.color = new Color(
                    0.8f + combinedPollution * 0.2f,
                    0.15f,
                    0.25f,
                    pulseAlpha + flashBoost);
            }
        }

        // 6) 暗角层 — 基础暗角 + 污染时红光偏移
        if (_vignette != null)
        {
            if (combinedPollution <= 0.01f)
            {
                // 基础暗角 (常亮, 不透明, 用 radial gradient)
                _vignette.color = new Color(0.02f, 0.01f, 0.05f, 0.20f);
            }
            else
            {
                float vigAlpha = 0.20f + combinedPollution * 0.45f;
                if (combinedPollution > 0.3f)
                {
                    float vigPulse = Mathf.Sin(Time.time * (1f + combinedPollution * 3f)) * 0.15f + 0.85f;
                    float redShift = combinedPollution * 0.25f;
                    _vignette.color = new Color(0.02f + redShift, 0.01f, 0.05f + redShift * 0.5f, vigAlpha * vigPulse);
                }
                else
                {
                    _vignette.color = new Color(0.02f, 0.01f, 0.05f, vigAlpha);
                }
            }
        }
    }

    public void ApplyQualityTier(bool highQuality = false)
    {
        if (_vignette == null) return;
        _vignette.color = Color.clear;
        if (_grain != null && !highQuality)
            _grain.color = Color.clear;
    }

    /// <summary>由 ScreenEffectsManager 更新污染强度 (0-1)</summary>
    public void SetPollutionIntensity(float intensity)
    {
        _targetIntensity = Mathf.Clamp01(intensity);
    }

    /// <summary>污染度突然升高时触发一次性红光 flash 反馈.</summary>
    public void TriggerPollutionFlash(float intensity = 0.6f)
    {
        _pollutionFlashAmount = Mathf.Max(_pollutionFlashAmount, Mathf.Clamp01(intensity));
        _pollutionFlashDecay = 0.45f;
    }

    void OnDestroy()
    {
        if (_vignette != null) _vignette.color = Color.clear;
        if (_grain != null) _grain.color = Color.clear;
        if (_scanlines != null) _scanlines.color = Color.clear;
        if (_redPulse != null) _redPulse.color = Color.clear;
        if (_scanlineTex != null) Destroy(_scanlineTex);
    }

    void OnDisable()
    {
        if (_vignette != null) _vignette.color = Color.clear;
        if (_grain != null) _grain.color = Color.clear;
        if (_scanlines != null) _scanlines.color = Color.clear;
        if (_redPulse != null) _redPulse.color = Color.clear;
    }
}
