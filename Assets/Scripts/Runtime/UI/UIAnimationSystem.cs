using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class ButtonPressFeedback : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerEnterHandler, IPointerExitHandler
{
    RectTransform _rt;
    Outline _outline;
    Outline _glowOutline;
    Coroutine _anim;
    bool _hovering;
    bool _isCloseButton;

    // 自定义强调色 - 用于侧边栏等需要特殊配色的按钮
    Color? _accentColor;
    Color _normalOutlineColor;
    Color _normalGlowColor;
    Color _normalBgColor;
    Image _bgImage;

    public void SetCloseButtonStyle(bool isClose)
    {
        _isCloseButton = isClose;
    }

    /// <summary>
    /// 设置自定义强调色，用于hover/press时的边框和背景反馈
    /// </summary>
    public void SetAccentColor(Color accent)
    {
        _accentColor = accent;
        _normalOutlineColor = _outline != null ? _outline.effectColor : Color.white;
        _normalGlowColor = _glowOutline != null ? _glowOutline.effectColor : Color.white;
        _bgImage = GetComponent<Image>();
        _normalBgColor = _bgImage != null ? _bgImage.color : Color.white;
    }

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        var outlines = GetComponents<Outline>();
        if (outlines.Length > 0) _outline = outlines[0];
        if (outlines.Length > 1) _glowOutline = outlines[1];
        _bgImage = GetComponent<Image>();
        _normalBgColor = _bgImage != null ? _bgImage.color : Color.white;
        _normalOutlineColor = _outline != null ? _outline.effectColor : Color.white;
        _normalGlowColor = _glowOutline != null ? _glowOutline.effectColor : Color.white;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        _hovering = true;
        if (_rt != null)
            _rt.localScale = Vector3.one * ParasiteTowerArtOptimization.UIPolish.ButtonStyle.HoverScale;

        // 自定义强调色模式（侧边栏按钮等）
        if (_accentColor.HasValue)
        {
            Color accent = _accentColor.Value;
            if (_outline != null)
                _outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.5f);
            if (_glowOutline != null)
                _glowOutline.effectColor = new Color(accent.r, accent.g, accent.b, 0.15f);
            if (_bgImage != null)
                _bgImage.color = Color.Lerp(_normalBgColor, new Color(accent.r * 0.3f, accent.g * 0.3f, accent.b * 0.3f, 0.8f), 1f);
            return;
        }

        if (_isCloseButton)
        {
            if (_outline != null)
                _outline.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.HoverBorder;
            if (_glowOutline != null)
                _glowOutline.effectColor = new Color(1f, 0.4f, 0.4f, 0.5f);
        }
        else
        {
            if (_outline != null)
                _outline.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.HoverBorder;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        _hovering = false;
        if (_anim != null) StopCoroutine(_anim);
        if (_rt != null)
            _rt.localScale = Vector3.one;

        // 恢复正常状态颜色
        if (_accentColor.HasValue)
        {
            if (_outline != null) _outline.effectColor = _normalOutlineColor;
            if (_glowOutline != null) _glowOutline.effectColor = _normalGlowColor;
            if (_bgImage != null) _bgImage.color = _normalBgColor;
            return;
        }

        if (_isCloseButton)
        {
            if (_outline != null)
                _outline.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.NormalBorder;
            if (_glowOutline != null)
                _glowOutline.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.GlowColor;
        }
        else
        {
            if (_outline != null)
                _outline.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.NormalBorder;
        }
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        if (_anim != null) StopCoroutine(_anim);
        if (_rt != null)
            _rt.localScale = Vector3.one * ParasiteTowerArtOptimization.UIPolish.ButtonStyle.PressScale;

        // 自定义强调色模式
        if (_accentColor.HasValue)
        {
            Color accent = _accentColor.Value;
            if (_outline != null)
                _outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.7f);
            if (_glowOutline != null)
                _glowOutline.effectColor = new Color(accent.r, accent.g, accent.b, 0.25f);
            if (_bgImage != null)
                _bgImage.color = Color.Lerp(_normalBgColor, new Color(accent.r * 0.15f, accent.g * 0.15f, accent.b * 0.15f, 0.9f), 1f);
            return;
        }

        if (_isCloseButton)
        {
            if (_outline != null)
                _outline.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.PressBorder;
            if (_glowOutline != null)
                _glowOutline.effectColor = new Color(1f, 0.6f, 0.6f, 0.6f);
        }
        else
        {
            if (_outline != null)
                _outline.effectColor = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.PressBorder;
        }
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        if (_rt == null) return;
        if (_anim != null) StopCoroutine(_anim);
        _anim = StartCoroutine(ReleaseAnim());
    }

    IEnumerator ReleaseAnim()
    {
        float duration = ParasiteTowerArtOptimization.UIPolish.ButtonStyle.AnimationDuration;
        Vector3 target = _hovering
            ? Vector3.one * ParasiteTowerArtOptimization.UIPolish.ButtonStyle.HoverScale
            : Vector3.one;
        Vector3 from = _rt.localScale;
        float elapsed = 0f;
        while (elapsed < duration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float ease = 1f + (t - 1f) * (2.70158f * t + 1.70158f);
            _rt.localScale = Vector3.LerpUnclamped(from, target, ease);
            yield return null;
        }
        _rt.localScale = target;

        // 恢复到hover或normal状态
        if (_accentColor.HasValue)
        {
            Color accent = _accentColor.Value;
            if (_hovering)
            {
                if (_outline != null) _outline.effectColor = new Color(accent.r, accent.g, accent.b, 0.5f);
                if (_glowOutline != null) _glowOutline.effectColor = new Color(accent.r, accent.g, accent.b, 0.15f);
                if (_bgImage != null)
                    _bgImage.color = Color.Lerp(_normalBgColor, new Color(accent.r * 0.3f, accent.g * 0.3f, accent.b * 0.3f, 0.8f), 1f);
            }
            else
            {
                if (_outline != null) _outline.effectColor = _normalOutlineColor;
                if (_glowOutline != null) _glowOutline.effectColor = _normalGlowColor;
                if (_bgImage != null) _bgImage.color = _normalBgColor;
            }
            _anim = null;
            yield break;
        }

        if (_isCloseButton)
        {
            if (_outline != null)
                _outline.effectColor = _hovering
                    ? ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.HoverBorder
                    : ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.NormalBorder;
            if (_glowOutline != null)
                _glowOutline.effectColor = _hovering
                    ? new Color(1f, 0.4f, 0.4f, 0.5f)
                    : ParasiteTowerArtOptimization.UIPolish.ButtonStyle.CloseButton.GlowColor;
        }
        else
        {
            if (_outline != null)
                _outline.effectColor = _hovering
                    ? ParasiteTowerArtOptimization.UIPolish.ButtonStyle.HoverBorder
                    : ParasiteTowerArtOptimization.UIPolish.ButtonStyle.NormalBorder;
        }
        _anim = null;
    }
}

public class UIAnimationSystem : MonoBehaviour
{
    public static UIAnimationSystem Instance { get; private set; }

    void Awake()
    {
        if (Instance != null && Instance != this) { Destroy(gameObject); return; }
        Instance = this;
    }

    public static IEnumerator FadeIn(RectTransform rect, float duration = 0.3f)
    {
        if (rect == null) yield break;
        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(0f, 1f, elapsed / duration);
            yield return null;
        }
        if (rect == null) yield break;
        cg.alpha = 1f;
    }

    public static IEnumerator FadeOut(RectTransform rect, float duration = 0.3f)
    {
        if (rect == null) yield break;
        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
        float startAlpha = cg.alpha;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            cg.alpha = Mathf.Lerp(startAlpha, 0f, elapsed / duration);
            yield return null;
        }
        if (rect == null) yield break;
        cg.alpha = 0f;
    }

    public static IEnumerator SlideInFromBottom(RectTransform rect, float duration = 0.4f)
    {
        if (rect == null) yield break;
        // 按屏幕高度比例计算滑入距离,避免不同分辨率下视觉不一致
        float slideDistance = Mathf.Max(500f, Screen.height * 0.5f);
        Vector2 startPos = rect.anchoredPosition + Vector2.down * slideDistance;
        Vector2 endPos = rect.anchoredPosition;
        rect.anchoredPosition = startPos;

        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = EaseOutBack(t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
            cg.alpha = Mathf.Lerp(0f, 1f, easeT);
            yield return null;
        }
        if (rect == null) yield break;
        rect.anchoredPosition = endPos;
        cg.alpha = 1f;
    }

    public static IEnumerator SlideOutToBottom(RectTransform rect, float duration = 0.4f)
    {
        if (rect == null) yield break;
        // 按屏幕高度比例计算滑出距离
        float slideDistance = Mathf.Max(500f, Screen.height * 0.5f);
        Vector2 startPos = rect.anchoredPosition;
        Vector2 endPos = rect.anchoredPosition + Vector2.down * slideDistance;

        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = EaseInBack(t);
            rect.anchoredPosition = Vector2.Lerp(startPos, endPos, easeT);
            cg.alpha = Mathf.Lerp(1f, 0f, easeT);
            yield return null;
        }
        if (rect == null) yield break;
        rect.anchoredPosition = endPos;
        cg.alpha = 0f;
    }

    public static IEnumerator ScaleIn(RectTransform rect, float duration = 0.25f)
    {
        if (rect == null) yield break;
        Vector3 startScale = Vector3.one * 0.5f;
        Vector3 endScale = Vector3.one;
        rect.localScale = startScale;

        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float easeT = EaseOutElastic(t);
            rect.localScale = Vector3.Lerp(startScale, endScale, easeT);
            cg.alpha = Mathf.Lerp(0f, 1f, easeT);
            yield return null;
        }
        if (rect == null) yield break;
        rect.localScale = endScale;
        cg.alpha = 1f;
    }

    public static IEnumerator ScalePulse(RectTransform rect, float scale = 1.1f, float duration = 0.2f)
    {
        if (rect == null) yield break;
        Vector3 startScale = Vector3.one;
        Vector3 endScale = Vector3.one * scale;
        rect.localScale = startScale;

        float elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            rect.localScale = Vector3.Lerp(startScale, endScale, EaseOutQuad(t));
            yield return null;
        }

        elapsed = 0f;
        while (elapsed < duration / 2f)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / (duration / 2f);
            rect.localScale = Vector3.Lerp(endScale, startScale, EaseInQuad(t));
            yield return null;
        }

        if (rect == null) yield break;
        rect.localScale = startScale;
    }

    public static IEnumerator Bounce(RectTransform rect, float height = 30f, float duration = 0.4f)
    {
        if (rect == null) yield break;
        Vector2 startPos = rect.anchoredPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float bounce = Mathf.Abs(Mathf.Sin(t * Mathf.PI * 2f)) * height;
            rect.anchoredPosition = startPos + new Vector2(0, bounce);
            yield return null;
        }

        if (rect == null) yield break;
        rect.anchoredPosition = startPos;
    }

    public static IEnumerator Flash(Graphic graphic, Color color, float duration = 0.2f, int loops = 2)
    {
        if (graphic == null) yield break;
        Color originalColor = graphic.color;

        for (int i = 0; i < loops; i++)
        {
            float elapsed = 0f;
            while (elapsed < duration / (loops * 2f))
            {
                if (graphic == null) yield break;
                elapsed += Time.deltaTime;
                graphic.color = Color.Lerp(originalColor, color, elapsed / (duration / (loops * 2f)));
                yield return null;
            }

            elapsed = 0f;
            while (elapsed < duration / (loops * 2f))
            {
                if (graphic == null) yield break;
                elapsed += Time.deltaTime;
                graphic.color = Color.Lerp(color, originalColor, elapsed / (duration / (loops * 2f)));
                yield return null;
            }
        }

        if (graphic == null) yield break;
        graphic.color = originalColor;
    }

    public static IEnumerator Pulse(RectTransform rect, float scale = 1.05f, float interval = 1.5f, float maxDuration = 0f)
    {
        if (rect == null) yield break;
        float elapsed = 0f;
        while (maxDuration <= 0f || elapsed < maxDuration)
        {
            if (rect == null) yield break;
            yield return ScalePulse(rect, scale, 0.3f);
            if (rect == null) yield break;
            yield return new WaitForSeconds(interval);
            elapsed += 0.3f + interval;
        }
    }

    private static float EaseOutQuad(float t) => 1f - (1f - t) * (1f - t);
    private static float EaseInQuad(float t) => t * t;
    private static float EaseOutBack(float t) => 1f + (t - 1f) * (2.70158f * t + 1.70158f);
    private static float EaseInBack(float t) => t * t * (2.70158f * t - 1.70158f);

    private static float EaseOutElastic(float t)
    {
        if (t == 0f) return 0f;
        if (t == 1f) return 1f;
        float c4 = (2f * Mathf.PI) / 3f;
        return Mathf.Pow(2f, -10f * t) * Mathf.Sin((t * 10f - 0.75f) * c4) + 1f;
    }

    public static IEnumerator ScaleInSmooth(RectTransform rect, float startScale = 0.85f, float duration = 0.25f)
    {
        Vector3 from = Vector3.one * startScale;
        Vector3 to = Vector3.one;
        rect.localScale = from;

        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
        cg.alpha = 0f;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            float easeT = EaseOutBack(t);
            rect.localScale = Vector3.LerpUnclamped(from, to, easeT);
            cg.alpha = Mathf.Lerp(0f, 1f, t);
            yield return null;
        }
        if (rect == null) yield break;
        rect.localScale = to;
        cg.alpha = 1f;
    }

    public static IEnumerator ScaleOutSmooth(RectTransform rect, float endScale = 0.9f, float duration = 0.15f)
    {
        Vector3 from = rect.localScale;
        Vector3 to = Vector3.one * endScale;

        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
        float startAlpha = cg.alpha;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = Mathf.Clamp01(elapsed / duration);
            rect.localScale = Vector3.Lerp(from, to, EaseInQuad(t));
            cg.alpha = Mathf.Lerp(startAlpha, 0f, t);
            yield return null;
        }
        if (rect == null) yield break;
        rect.localScale = to;
        cg.alpha = 0f;
    }

    public static IEnumerator Sequence(params IEnumerator[] animations)
    {
        foreach (var animation in animations)
        {
            yield return Instance.StartCoroutine(animation);
        }
    }

    public static IEnumerator Parallel(params IEnumerator[] animations)
    {
        int running = animations.Length;
        foreach (var animation in animations)
        {
            Instance.StartCoroutine(RunAndDecrement(animation, () => running--));
        }
        while (running > 0)
            yield return null;
    }

    private static IEnumerator RunAndDecrement(IEnumerator routine, System.Action onDone)
    {
        yield return Instance.StartCoroutine(routine);
        onDone();
    }

    public static IEnumerator DamageShake(RectTransform rect, float intensity, float duration = 0.15f)
    {
        if (rect == null) yield break;
        Vector3 originalPos = rect.localPosition;
        float elapsed = 0f;

        while (elapsed < duration)
        {
            if (rect == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            float shakeX = (Random.value - 0.5f) * intensity * 20f * (1f - t);
            float shakeY = (Random.value - 0.5f) * intensity * 20f * (1f - t);
            rect.localPosition = originalPos + new Vector3(shakeX, shakeY, 0);
            yield return null;
        }

        if (rect == null) yield break;
        rect.localPosition = originalPos;
    }

    public static IEnumerator DamageFlash(RectTransform rect, Color flashColor, float duration = 0.2f)
    {
        if (rect == null) yield break;
        CanvasGroup cg = rect.GetComponent<CanvasGroup>() ?? rect.gameObject.AddComponent<CanvasGroup>();
        float originalAlpha = cg.alpha;
        cg.alpha = 1f;

        yield return Flash(cg.GetComponent<Graphic>() ?? rect.GetComponent<Graphic>(), flashColor, duration, 1);

        if (rect == null) yield break;
        cg.alpha = originalAlpha;
    }

    public static IEnumerator ScreenFlash(Color color, float duration = 0.1f)
    {
        // 统一转发到 ScreenFlashEffect 单例,避免两套闪烁系统并存
        if (ScreenFlashEffect.Instance != null)
        {
            ScreenFlashEffect.Instance.Flash(color, duration);
            yield return new WaitForSeconds(duration);
            yield break;
        }

        // Fallback:ScreenFlashEffect 不可用时使用独立 GameObject
        if (Instance == null) yield break;

        var flashGo = new GameObject("ScreenFlash", typeof(RectTransform), typeof(Image));
        flashGo.transform.SetParent(Instance.transform, false);
        var flashRT = flashGo.GetComponent<RectTransform>();
        flashRT.anchorMin = Vector2.zero;
        flashRT.anchorMax = Vector2.one;
        flashRT.offsetMin = Vector2.zero;
        flashRT.offsetMax = Vector2.zero;
        flashRT.SetAsLastSibling();

        var flashImg = flashGo.GetComponent<Image>();
        flashImg.color = new Color(color.r, color.g, color.b, 0f);
        flashImg.raycastTarget = false;

        float elapsed = 0f;
        while (elapsed < duration)
        {
            if (flashGo == null) yield break;
            elapsed += Time.deltaTime;
            float t = elapsed / duration;
            // 先渐入(0~0.2)再衰减(0.2~1)
            float alpha = t < 0.2f ? Mathf.Lerp(0f, 0.5f, t / 0.2f) : Mathf.Lerp(0.5f, 0f, (t - 0.2f) / 0.8f);
            flashImg.color = new Color(color.r, color.g, color.b, alpha);
            yield return null;
        }

        if (flashGo != null)
            Object.Destroy(flashGo);
    }
}
