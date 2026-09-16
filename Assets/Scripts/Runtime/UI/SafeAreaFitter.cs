using UnityEngine;

public class SafeAreaFitter : MonoBehaviour
{
    RectTransform _rt;
    Rect _lastSafeArea;

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
        ApplySafeArea();
    }

    void Update()
    {
        if (Screen.safeArea != _lastSafeArea)
            ApplySafeArea();
    }

    void ApplySafeArea()
    {
        var sa = Screen.safeArea;
        _lastSafeArea = sa;

        var canvas = GetComponentInParent<Canvas>().rootCanvas;
        var canvasRT = canvas.GetComponent<RectTransform>();
        var canvasSize = canvasRT.rect.size;

        float sw = Screen.width;
        float sh = Screen.height;
        if (sw <= 0 || sh <= 0) return;

        float scaleX = canvasSize.x / sw;
        float scaleY = canvasSize.y / sh;

        _rt.anchorMin = Vector2.zero;
        _rt.anchorMax = Vector2.one;
        _rt.offsetMin = new Vector2(sa.x * scaleX, sa.y * scaleY);
        _rt.offsetMax = new Vector2((sa.x + sa.width - sw) * scaleX, (sa.y + sa.height - sh) * scaleY);
    }
}
