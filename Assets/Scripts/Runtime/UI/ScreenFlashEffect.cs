using UnityEngine;
using UnityEngine.UI;

public class ScreenFlashEffect : MonoBehaviour
{
    public static ScreenFlashEffect Instance { get; private set; }

    [Header("Flash Settings")]
    public float defaultFlashDuration = 0.1f;
    public float maxFlashIntensity = 1f;
    [Range(0f, 0.5f)]
    [Tooltip("渐入阶段占总时长的比例,其余为衰减")]
    public float fadeInRatio = 0.2f;

    private Image flashImage;
    private Color targetColor;
    private float flashStartTime;
    private float flashDuration;
    private bool isFlashing = false;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            Initialize();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Initialize()
    {
        flashImage = GetComponent<Image>();
        if (flashImage == null)
        {
            flashImage = gameObject.AddComponent<Image>();
        }

        flashImage.color = Color.clear;
        flashImage.raycastTarget = false;
        flashImage.rectTransform.anchorMin = Vector2.zero;
        flashImage.rectTransform.anchorMax = Vector2.one;
        flashImage.rectTransform.sizeDelta = Vector2.zero;
    }

    public void Flash(Color color, float duration = 0.1f)
    {
        targetColor = color;
        flashDuration = duration;
        flashStartTime = Time.time;
        isFlashing = true;
        // 起始透明,通过 Update 渐入,避免突兀
        flashImage.color = new Color(color.r, color.g, color.b, 0f);
    }

    private void Update()
    {
        if (!isFlashing) return;

        float elapsed = Time.time - flashStartTime;
        float fadeInTime = flashDuration * fadeInRatio;

        if (elapsed >= flashDuration)
        {
            flashImage.color = Color.clear;
            isFlashing = false;
        }
        else if (elapsed < fadeInTime)
        {
            // 渐入阶段:0 → maxFlashIntensity
            float t = fadeInTime > 0f ? elapsed / fadeInTime : 1f;
            float alpha = Mathf.Lerp(0f, maxFlashIntensity, t);
            flashImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
        }
        else
        {
            // 衰减阶段:maxFlashIntensity → 0
            float t = (elapsed - fadeInTime) / Mathf.Max(0.0001f, flashDuration - fadeInTime);
            float alpha = Mathf.Lerp(maxFlashIntensity, 0f, t);
            flashImage.color = new Color(targetColor.r, targetColor.g, targetColor.b, alpha);
        }
    }

    public void FlashWhite()
    {
        Flash(Color.white, defaultFlashDuration);
    }

    public void FlashBlack()
    {
        Flash(Color.black, defaultFlashDuration);
    }

    public void FlashRed()
    {
        Flash(Color.red, defaultFlashDuration);
    }

    public void FlashWithIntensity(Color color, float intensity, float duration)
    {
        Color adjustedColor = new Color(color.r, color.g, color.b, intensity);
        Flash(adjustedColor, duration);
    }
}