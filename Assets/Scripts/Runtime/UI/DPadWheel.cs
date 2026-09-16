using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;

public class DPadWheel : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IDragHandler
{
    public Action<Vector2Int> onDirection;
    public float firstDelay = 0.25f;
    public float repeatInterval = 0.1f;
    public float joystickDeadZone = 0.2f;

    RectTransform _rt;

    // Virtual Joystick
    RectTransform _joystickHandle;
    Image _joystickHandleImg;
    Vector2 _joystickCenter;
    float _joystickMaxDist;

    // Arrow highlights
    Image[] _arrows = new Image[4];

    bool _held;
    float _timer;
    bool _firstFired;
    Vector2Int _currentDir;
    int _lastArrowIndex = -1;

    // 静态 sprite 缓存:相同尺寸的圆形/箭头/十字 sprite 复用,避免 Texture2D 内存泄漏
    static readonly System.Collections.Generic.Dictionary<int, Sprite> _circleSpriteCache = new System.Collections.Generic.Dictionary<int, Sprite>();

    void Awake()
    {
        _rt = GetComponent<RectTransform>();
    }

    void OnDestroy()
    {
        // 静态缓存在场景切换时统一释放,避免跨场景泄漏
        // 单个 DPadWheel 销毁时不清理静态缓存(其他实例可能仍在使用)
    }

    // 应用退出或场景切换时调用,清理所有缓存的 sprite
    public static void ClearSpriteCache()
    {
        foreach (var spr in _circleSpriteCache.Values)
        {
            if (spr != null && spr.texture != null) Destroy(spr.texture);
        }
        _circleSpriteCache.Clear();
    }

    public void Setup(RectTransform parent, Vector2 pos, float size)
    {
        _rt = GetComponent<RectTransform>();
        _rt.anchoredPosition = pos;

        float diameter = size;
        float radius = diameter / 2f;
        _rt.sizeDelta = new Vector2(diameter, diameter);

        // Transparent background (for input detection)
        var bg = GetComponent<Image>();
        bg.color = new Color(0, 0, 0, 0);
        bg.raycastTarget = true;

        // Base circle (dark background with glow)
        var baseGo = new GameObject("Base", typeof(RectTransform), typeof(Image));
        baseGo.transform.SetParent(transform, false);
        var baseRT = baseGo.GetComponent<RectTransform>();
        baseRT.anchorMin = Vector2.zero;
        baseRT.anchorMax = Vector2.one;
        baseRT.offsetMin = Vector2.zero;
        baseRT.offsetMax = Vector2.zero;
        var baseImg = baseGo.GetComponent<Image>();
        baseImg.color = new Color(0.12f, 0.10f, 0.18f, 0.95f);
        baseImg.raycastTarget = false;
        
        // Create circle sprite for base
        baseImg.sprite = CreateCircleSprite((int)diameter);
        baseImg.type = Image.Type.Simple;

        // Outer ring glow
        var ringGo = new GameObject("Ring", typeof(RectTransform), typeof(Image));
        ringGo.transform.SetParent(baseGo.transform, false);
        var ringRT = ringGo.GetComponent<RectTransform>();
        ringRT.anchorMin = Vector2.zero;
        ringRT.anchorMax = Vector2.one;
        ringRT.offsetMin = new Vector2(-4, -4);
        ringRT.offsetMax = new Vector2(4, 4);
        var ringImg = ringGo.GetComponent<Image>();
        ringImg.color = new Color(0f, 1f, 0.816f, 0.15f);
        ringImg.raycastTarget = false;
        ringImg.sprite = CreateCircleSprite((int)(diameter + 8));

        // Inner ring
        var innerRingGo = new GameObject("InnerRing", typeof(RectTransform), typeof(Image));
        innerRingGo.transform.SetParent(baseGo.transform, false);
        var innerRingRT = innerRingGo.GetComponent<RectTransform>();
        innerRingRT.anchorMin = new Vector2(0.15f, 0.15f);
        innerRingRT.anchorMax = new Vector2(0.85f, 0.85f);
        innerRingRT.offsetMin = Vector2.zero;
        innerRingRT.offsetMax = Vector2.zero;
        var innerRingImg = innerRingGo.GetComponent<Image>();
        innerRingImg.color = new Color(0.08f, 0.06f, 0.14f, 0.8f);
        innerRingImg.raycastTarget = false;
        innerRingImg.sprite = CreateCircleSprite((int)(diameter * 0.7f));

        // Direction arrows (inside the base)
        string[] arrowTexts = { "▲", "▶", "▼", "◀" };
        Vector2[] arrowAnchors = {
            new Vector2(0.5f, 0.82f),  // Up
            new Vector2(0.82f, 0.5f),  // Right
            new Vector2(0.5f, 0.18f),  // Down
            new Vector2(0.18f, 0.5f)   // Left
        };

        for (int i = 0; i < 4; i++)
        {
            var arrowGo = new GameObject($"Arrow_{i}", typeof(RectTransform), typeof(Image));
            arrowGo.transform.SetParent(baseGo.transform, false);
            var arrowRT = arrowGo.GetComponent<RectTransform>();
            arrowRT.anchorMin = arrowAnchors[i];
            arrowRT.anchorMax = arrowAnchors[i];
            arrowRT.pivot = new Vector2(0.5f, 0.5f);
            arrowRT.sizeDelta = new Vector2(24, 24);
            
            var arrowImg = arrowGo.GetComponent<Image>();
            arrowImg.color = new Color(0f, 1f, 0.816f, 0.3f);
            arrowImg.raycastTarget = false;
            arrowImg.sprite = CreateArrowSprite(arrowTexts[i], (int)(diameter * 0.18f));
            _arrows[i] = arrowImg;
        }

        // Center dot
        var centerDotGo = new GameObject("CenterDot", typeof(RectTransform), typeof(Image));
        centerDotGo.transform.SetParent(baseGo.transform, false);
        var centerDotRT = centerDotGo.GetComponent<RectTransform>();
        centerDotRT.anchorMin = new Vector2(0.5f, 0.5f);
        centerDotRT.anchorMax = new Vector2(0.5f, 0.5f);
        centerDotRT.pivot = new Vector2(0.5f, 0.5f);
        centerDotRT.sizeDelta = new Vector2(8, 8);
        centerDotRT.anchoredPosition = Vector2.zero;
        var centerDotImg = centerDotGo.GetComponent<Image>();
        centerDotImg.color = new Color(0f, 1f, 0.816f, 0.4f);
        centerDotImg.raycastTarget = false;
        centerDotImg.sprite = CreateCircleSprite(16);

        // Joystick handle
        float handleSize = diameter * 0.38f;
        var handleGo = new GameObject("Handle", typeof(RectTransform), typeof(Image));
        handleGo.transform.SetParent(baseGo.transform, false);
        _joystickHandle = handleGo.GetComponent<RectTransform>();
        _joystickHandle.anchorMin = new Vector2(0.5f, 0.5f);
        _joystickHandle.anchorMax = new Vector2(0.5f, 0.5f);
        _joystickHandle.pivot = new Vector2(0.5f, 0.5f);
        _joystickHandle.sizeDelta = new Vector2(handleSize, handleSize);
        _joystickHandle.anchoredPosition = Vector2.zero;
        
        _joystickHandleImg = handleGo.GetComponent<Image>();
        _joystickHandleImg.color = new Color(0.5f, 0.3f, 0.85f, 0.95f);
        _joystickHandleImg.raycastTarget = false;
        _joystickHandleImg.sprite = CreateCircleSprite((int)handleSize);

        // Handle highlight (top-left gradient effect)
        var highlightGo = new GameObject("Highlight", typeof(RectTransform), typeof(Image));
        highlightGo.transform.SetParent(handleGo.transform, false);
        var highlightRT = highlightGo.GetComponent<RectTransform>();
        highlightRT.anchorMin = new Vector2(0, 0);
        highlightRT.anchorMax = new Vector2(1, 1);
        highlightRT.offsetMin = Vector2.zero;
        highlightRT.offsetMax = Vector2.zero;
        var highlightImg = highlightGo.GetComponent<Image>();
        highlightImg.color = new Color(1f, 1f, 1f, 0.15f);
        highlightImg.raycastTarget = false;
        highlightImg.sprite = CreateCircleSprite((int)handleSize);

        // Handle center icon — 程序化绘制十字符号(不再使用字符 "◈")
        var handleIconGo = new GameObject("Icon", typeof(RectTransform), typeof(Image));
        handleIconGo.transform.SetParent(handleGo.transform, false);
        var iconRT = handleIconGo.GetComponent<RectTransform>();
        iconRT.anchorMin = new Vector2(0.5f, 0.5f);
        iconRT.anchorMax = new Vector2(0.5f, 0.5f);
        iconRT.pivot = new Vector2(0.5f, 0.5f);
        float iconSize = handleSize * 0.5f;
        iconRT.sizeDelta = new Vector2(iconSize, iconSize);
        iconRT.anchoredPosition = Vector2.zero;
        var iconImg = handleIconGo.GetComponent<Image>();
        iconImg.color = new Color(1f, 1f, 1f, 0.9f);
        iconImg.raycastTarget = false;
        iconImg.sprite = CreateCrossSprite((int)iconSize);

        _joystickMaxDist = radius * 0.75f;
    }

    // Helper: Create cross/plus sprite — 程序化绘制中心十字符号
    Sprite CreateCrossSprite(int size)
    {
        var tex = new Texture2D(size, size);
        var colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;

        float cx = size / 2f;
        float cy = size / 2f;
        float armLen = size * 0.32f;   // 十字臂长
        float armThick = Mathf.Max(1f, size * 0.08f); // 十字臂宽

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dx = Mathf.Abs(x - cx);
                float dy = Mathf.Abs(y - cy);
                // 横臂或竖臂
                bool inH = dy <= armThick && dx <= armLen;
                bool inV = dx <= armThick && dy <= armLen;
                if (inH || inV)
                {
                    colors[y * size + x] = new Color(1f, 1f, 1f, 0.9f);
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    void Update()
    {
        if (!_held || _currentDir == Vector2Int.zero) return;

        _timer += Time.deltaTime;
        if (!_firstFired)
        {
            if (_timer >= firstDelay)
            {
                _firstFired = true;
                _timer = 0f;
                onDirection?.Invoke(_currentDir);
            }
        }
        else if (_timer >= repeatInterval)
        {
            _timer = 0f;
            onDirection?.Invoke(_currentDir);
        }
    }

    void OnDisable() { ResetJoystick(); }

    public void OnPointerDown(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, eventData.position, eventData.pressEventCamera, out localPoint))
            return;
        
        _joystickCenter = localPoint;
        _held = true;
        _timer = 0f;
        _firstFired = false;
        UpdateJoystick(eventData);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!_held) return;
        UpdateJoystick(eventData);
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        ResetJoystick();
    }

    void UpdateJoystick(PointerEventData eventData)
    {
        Vector2 localPoint;
        if (!RectTransformUtility.ScreenPointToLocalPointInRectangle(
            _rt, eventData.position, eventData.pressEventCamera, out localPoint))
            return;

        Vector2 offset = localPoint - _joystickCenter;
        float dist = offset.magnitude;
        
        if (dist > _joystickMaxDist)
        {
            offset = offset.normalized * _joystickMaxDist;
        }

        if (_joystickHandle != null)
        {
            _joystickHandle.anchoredPosition = offset;
        }

        // Determine direction
        Vector2 normalized = offset / _joystickMaxDist;
        if (normalized.magnitude < joystickDeadZone)
        {
            _currentDir = Vector2Int.zero;
            UpdateArrowHighlight(-1);
            return;
        }

        float absX = Mathf.Abs(normalized.x);
        float absY = Mathf.Abs(normalized.y);

        Vector2Int newDir;
        int arrowIdx;
        
        if (absX > absY)
        {
            if (normalized.x > 0)
            {
                newDir = Vector2Int.right;
                arrowIdx = 1;
            }
            else
            {
                newDir = Vector2Int.left;
                arrowIdx = 3;
            }
        }
        else
        {
            if (normalized.y > 0)
            {
                newDir = Vector2Int.up;
                arrowIdx = 0;
            }
            else
            {
                newDir = Vector2Int.down;
                arrowIdx = 2;
            }
        }

        UpdateArrowHighlight(arrowIdx);

        if (newDir != _currentDir)
        {
            _currentDir = newDir;
            _timer = 0f;
            _firstFired = false;
            onDirection?.Invoke(_currentDir);
        }
    }

    void UpdateArrowHighlight(int activeIdx)
    {
        if (_lastArrowIndex == activeIdx) return;
        
        for (int i = 0; i < 4; i++)
        {
            if (_arrows[i] != null)
            {
                _arrows[i].color = (i == activeIdx) 
                    ? new Color(0f, 1f, 0.816f, 1f) 
                    : new Color(0f, 1f, 0.816f, 0.3f);
            }
        }
        _lastArrowIndex = activeIdx;
    }

    void ResetJoystick()
    {
        _held = false;
        _currentDir = Vector2Int.zero;
        _lastArrowIndex = -1;
        
        UpdateArrowHighlight(-1);
        
        if (_joystickHandle != null)
        {
            _joystickHandle.anchoredPosition = Vector2.zero;
        }
    }

    // Helper: Create circular sprite (带缓存,避免内存泄漏)
    Sprite CreateCircleSprite(int size)
    {
        if (_circleSpriteCache.TryGetValue(size, out var cached)) return cached;

        var tex = new Texture2D(size, size);
        float cx = size / 2f, cy = size / 2f;
        float r = size / 2f - 1;

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float dist = Mathf.Sqrt((x - cx) * (x - cx) + (y - cy) * (y - cy));
                float alpha = dist <= r ? 1f : 0f;
                tex.SetPixel(x, y, new Color(1f, 1f, 1f, alpha));
            }
        }
        tex.Apply();
        var spr = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
        _circleSpriteCache[size] = spr;
        return spr;
    }

    // Helper: Create arrow sprite — 根据方向字符绘制真正的三角形箭头
    Sprite CreateArrowSprite(string text, int size)
    {
        var tex = new Texture2D(size, size);
        var colors = new Color[size * size];
        for (int i = 0; i < colors.Length; i++) colors[i] = Color.clear;

        // 三角形顶点(归一化坐标 0~1),根据方向旋转
        // 默认朝上的三角形:顶点在 (0.5, 0.9),左下 (0.15, 0.15),右下 (0.85, 0.15)
        Vector2 top, bl, br;
        switch (text)
        {
            case "▶": // 朝右
                top = new Vector2(0.9f, 0.5f);
                bl = new Vector2(0.15f, 0.85f);
                br = new Vector2(0.15f, 0.15f);
                break;
            case "▼": // 朝下
                top = new Vector2(0.5f, 0.1f);
                bl = new Vector2(0.85f, 0.85f);
                br = new Vector2(0.15f, 0.85f);
                break;
            case "◀": // 朝左
                top = new Vector2(0.1f, 0.5f);
                bl = new Vector2(0.85f, 0.15f);
                br = new Vector2(0.85f, 0.85f);
                break;
            default: // ▲ 朝上
                top = new Vector2(0.5f, 0.9f);
                bl = new Vector2(0.15f, 0.15f);
                br = new Vector2(0.85f, 0.15f);
                break;
        }

        // 用重心坐标填充三角形
        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float px = (x + 0.5f) / size;
                float py = (y + 0.5f) / size;
                if (PointInTriangle(new Vector2(px, py), top, bl, br))
                {
                    colors[y * size + x] = new Color(1f, 1f, 1f, 0.9f);
                }
            }
        }

        tex.SetPixels(colors);
        tex.Apply();
        return Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f));
    }

    // 重心坐标判断点是否在三角形内
    static bool PointInTriangle(Vector2 p, Vector2 a, Vector2 b, Vector2 c)
    {
        float d1 = Sign(p, a, b);
        float d2 = Sign(p, b, c);
        float d3 = Sign(p, c, a);
        bool hasNeg = (d1 < 0) || (d2 < 0) || (d3 < 0);
        bool hasPos = (d1 > 0) || (d2 > 0) || (d3 > 0);
        return !(hasNeg && hasPos);
    }

    static float Sign(Vector2 p1, Vector2 p2, Vector2 p3)
    {
        return (p1.x - p3.x) * (p2.y - p3.y) - (p2.x - p3.x) * (p1.y - p3.y);
    }
}
