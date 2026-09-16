using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// 污染叠加层 — 当 ScreenEffectsManager 存在时委托给它，避免重复渲染。
/// </summary>
public class PollutionOverlay : SingletonBase<PollutionOverlay>
{
    Image _overlay;
    Image _vignette;
    float _targetAlpha;
    float _currentAlpha;
    Color _targetColor;

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        if (ScreenEffectsManager.Instance != null)
        {
            enabled = false;
            return;
        }
        CreateOverlay();
    }

    void CreateOverlay()
    {
        var canvas = FindObjectOfType<Canvas>();
        if (canvas == null) return;

        var ovlGo = new GameObject("PollutionOverlay", typeof(RectTransform), typeof(Image), typeof(CanvasGroup));
        ovlGo.transform.SetParent(canvas.transform, false);
        var rt = ovlGo.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;
        _overlay = ovlGo.GetComponent<Image>();
        _overlay.color = new Color(0, 0, 0, 0);
        _overlay.raycastTarget = false;
        ovlGo.GetComponent<CanvasGroup>().blocksRaycasts = false;

        var vigGo = new GameObject("Vignette", typeof(RectTransform), typeof(Image));
        vigGo.transform.SetParent(canvas.transform, false);
        var vrt = vigGo.GetComponent<RectTransform>();
        vrt.anchorMin = Vector2.zero;
        vrt.anchorMax = Vector2.one;
        vrt.offsetMin = Vector2.zero;
        vrt.offsetMax = Vector2.zero;
        _vignette = vigGo.GetComponent<Image>();
        _vignette.color = new Color(0, 0, 0, 0);
        _vignette.raycastTarget = false;
    }

    void Update()
    {
        if (ScreenEffectsManager.Instance != null) return;
        if (GameManager.Instance == null || GameManager.Instance.Player == null) return;

        float poll = GameManager.Instance.Player.pollution;
        UpdatePollutionVisual(poll);

        _currentAlpha = Mathf.Lerp(_currentAlpha, _targetAlpha, Time.deltaTime * 3f);
        if (_overlay != null)
        {
            var c = _targetColor;
            c.a = _currentAlpha;
            _overlay.color = c;
        }
    }

    public void UpdatePollution(float pollution)
    {
        UpdatePollutionVisual(pollution);
    }

    void UpdatePollutionVisual(float pollution)
    {
        if (pollution < 90f)
        {
            _targetAlpha = 0f;
            _targetColor = Color.clear;
        }
        else
        {
            _targetColor = ParasiteTowerEffects.GetPollutionOverlayColor(pollution);
            _targetAlpha = _targetColor.a;
        }

        if (_vignette != null && pollution >= 90f)
        {
            float vigAlpha = Mathf.Clamp01((pollution - 90f) / 10f) * 0.1f;
            _vignette.color = new Color(0, 0, 0, vigAlpha);
        }
        else if (_vignette != null)
        {
            _vignette.color = new Color(0, 0, 0, 0);
        }
    }

    public void FlashDamage()
    {
        if (_overlay != null)
        {
            _overlay.color = new Color(ParasiteTowerColorScheme.CriticalRed.r, 0f, 0f, 0.3f);
            _currentAlpha = 0.3f;
        }
    }

    public void FlashHeal()
    {
        if (_overlay != null)
        {
            _overlay.color = new Color(0f, ParasiteTowerColorScheme.HealthGreen.g, 0.5f, 0.2f);
            _currentAlpha = 0.2f;
        }
    }

    public void FlashPossess()
    {
        if (_overlay != null)
        {
            _overlay.color = new Color(ParasiteTowerColorScheme.ColorParasite.r, 0f, 0.8f, 0.3f);
            _currentAlpha = 0.3f;
        }
    }

    protected override void OnDestroy()
    {
        if (_overlay != null) _overlay.color = Color.clear;
        if (_vignette != null) _vignette.color = Color.clear;
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        if (_overlay != null) _overlay.color = Color.clear;
        if (_vignette != null) _vignette.color = Color.clear;
        base.OnDisable();
    }
}
