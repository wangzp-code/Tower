using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class DamageNumberPool : SingletonBase<DamageNumberPool>
{
    Canvas _canvas;
    readonly List<FloatingText> _pool = new List<FloatingText>();

    class FloatingText
    {
        public GameObject go;
        public Text text;
        public RectTransform rt;
        public float timer;
        public float duration;
        public Vector2 velocity;
        public Color startColor;
    }

    protected override void Awake()
    {
        base.Awake();
    }

    void Start()
    {
        _canvas = FindObjectOfType<Canvas>();
    }

    public void Spawn(string text, Color color, bool large = false)
    {
        if (_canvas == null) _canvas = FindObjectOfType<Canvas>();
        if (_canvas == null) return;

        var ft = GetFromPool();
        ft.text.text = text;
        ft.text.fontSize = large ? 28 : 18;
        ft.startColor = color;
        ft.text.color = color;
        ft.timer = 0f;
        ft.duration = 1.2f;
        ft.rt.anchoredPosition = new Vector2(
            Random.Range(-80f, 80f),
            Random.Range(100f, 200f)
        );
        ft.velocity = new Vector2(Random.Range(-20f, 20f), 80f);
        ft.go.SetActive(true);
    }

    public void SpawnDamage(int amount, bool crit)
    {
        Color c = crit ? new Color(1f, 0.3f, 0.1f) : Color.white;
        string txt = crit ? $"-{amount}!" : $"-{amount}";
        Spawn(txt, c, crit);
    }

    public void SpawnHeal(int amount)
    {
        Spawn($"+{amount}", new Color(0f, 1f, 0.6f), false);
    }

    public void SpawnPollution(float amount)
    {
        if (amount > 0)
            Spawn($"+{amount:F0}☢", new Color(0.7f, 0.2f, 0.8f), false);
    }

    FloatingText GetFromPool()
    {
        foreach (var ft in _pool)
        {
            if (!ft.go.activeSelf) return ft;
        }

        var go = new GameObject("FloatTxt", typeof(RectTransform), typeof(Text), typeof(Outline));
        go.transform.SetParent(_canvas.transform, false);
        var t = go.GetComponent<Text>();
        // 统一走 ThemeUIFonts,保证中文字体覆盖一致
        t.font = ThemeUIFonts.Get(18);
        t.alignment = TextAnchor.MiddleCenter;
        t.raycastTarget = false;
        t.horizontalOverflow = HorizontalWrapMode.Overflow;
        go.GetComponent<Outline>().effectColor = new Color(0, 0, 0, 0.8f);
        go.GetComponent<Outline>().effectDistance = new Vector2(1, 1);
        var rt = go.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(150, 40);

        var ft2 = new FloatingText { go = go, text = t, rt = rt };
        _pool.Add(ft2);
        return ft2;
    }

    void Update()
    {
        foreach (var ft in _pool)
        {
            if (!ft.go.activeSelf) continue;

            ft.timer += Time.deltaTime;
            float progress = ft.timer / ft.duration;

            if (progress >= 1f)
            {
                ft.go.SetActive(false);
                continue;
            }

            ft.rt.anchoredPosition += ft.velocity * Time.deltaTime;
            ft.velocity.y -= 120f * Time.deltaTime;

            var c = ft.startColor;
            c.a = 1f - progress;
            ft.text.color = c;

            float scale = progress < 0.1f ? Mathf.Lerp(0.5f, 1.2f, progress / 0.1f) :
                          progress < 0.2f ? Mathf.Lerp(1.2f, 1f, (progress - 0.1f) / 0.1f) : 1f;
            ft.rt.localScale = Vector3.one * scale;
        }
    }

    protected override void OnDestroy()
    {
        foreach (var ft in _pool)
        {
            if (ft.go != null) Destroy(ft.go);
        }
        _pool.Clear();
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        foreach (var ft in _pool)
        {
            if (ft.go != null) ft.go.SetActive(false);
        }
        base.OnDisable();
    }
}
