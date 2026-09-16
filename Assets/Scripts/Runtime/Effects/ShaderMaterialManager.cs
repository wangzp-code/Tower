using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class ShaderMaterialManager : SingletonBase<ShaderMaterialManager>
{
    Material _glowBtnMat;
    Material _hpBarMat;
    Material _enemyHpBarMat;
    Material _bgMat;
    Shader _bgShader;
    readonly Dictionary<Color, Material> _glowCache = new Dictionary<Color, Material>();
    readonly Dictionary<int, Material> _floorBgCache = new Dictionary<int, Material>();
    readonly Dictionary<int, Sprite> _floorPatternCache = new Dictionary<int, Sprite>();

    // ============= 楼层主题系统 =============
    // 每套主题定义完整的视觉 DNA:
    //   Main/Sec/Tert → ParasiteBackground Shader 三色调
    //   FloorTint     → 地板格子染色 (半透明叠加在 shader 主背景上)
    //   WallTint      → 墙壁染色 (完全不透明)
    //   Accent        → 描边/发光/accent color
    //   Speed/NoiseScale/ParticleDensity → Shader 动态参数
    //   Name          → 楼层氛围横幅显示用

    public class FloorTheme
    {
        public Color Main, Sec, Tert;
        public Color FloorTint, WallTint, Accent;
        public float Speed, NoiseScale, ParticleDensity;
        public string Name;
    }

    // ===== 视觉 DNA 重构: 神秘感 = 更暗 + 去饱和 + 微弱点缀 =====
    // Accent alpha 统一 0.18 (仅微弱轮廓提示, 不喧宾夺主)
    // FloorTint alpha 统一 0.30 (让 Shader 主背景纹理更多透出)
    // Tert 颜色大幅去饱和暗化 (particle 仅做"呼吸微光"而非荧光闪烁)

    // F1 入口通道 — 深青微透 · 冷静的金属感
    static readonly FloorTheme T_F1 = new FloorTheme {
        Main = new Color(0.015f, 0.025f, 0.04f),
        Sec  = new Color(0.00f, 0.14f, 0.18f),
        Tert = new Color(0.05f, 0.30f, 0.28f),
        FloorTint = new Color(0.04f, 0.12f, 0.14f, 0.30f),
        WallTint  = new Color(0.02f, 0.05f, 0.06f, 1.0f),
        Accent    = new Color(0.2f, 0.7f, 0.65f, 0.18f),
        Speed = 0.40f, NoiseScale = 7f, ParticleDensity = 25f,
        Name = "入口通道"
    };

    // F2 生物研究室 — 青蓝深潜 · 暗流涌动
    static readonly FloorTheme T_F2 = new FloorTheme {
        Main = new Color(0.015f, 0.03f, 0.05f),
        Sec  = new Color(0.02f, 0.16f, 0.26f),
        Tert = new Color(0.10f, 0.32f, 0.40f),
        FloorTint = new Color(0.05f, 0.10f, 0.16f, 0.30f),
        WallTint  = new Color(0.03f, 0.04f, 0.08f, 1.0f),
        Accent    = new Color(0.3f, 0.65f, 0.75f, 0.18f),
        Speed = 0.48f, NoiseScale = 8f, ParticleDensity = 30f,
        Name = "生物研究室"
    };

    // F3 变异培养舱 — 紫罗兰低语 · 异样的静谧
    static readonly FloorTheme T_F3 = new FloorTheme {
        Main = new Color(0.02f, 0.012f, 0.05f),
        Sec  = new Color(0.12f, 0.04f, 0.24f),
        Tert = new Color(0.30f, 0.15f, 0.40f),
        FloorTint = new Color(0.07f, 0.04f, 0.14f, 0.30f),
        WallTint  = new Color(0.04f, 0.02f, 0.08f, 1.0f),
        Accent    = new Color(0.55f, 0.30f, 0.70f, 0.18f),
        Speed = 0.58f, NoiseScale = 10f, ParticleDensity = 34f,
        Name = "变异培养舱"
    };

    // F4 神经连接层 — 深紫回路 · 脉动的网络
    static readonly FloorTheme T_F4 = new FloorTheme {
        Main = new Color(0.018f, 0.008f, 0.055f),
        Sec  = new Color(0.16f, 0.03f, 0.32f),
        Tert = new Color(0.38f, 0.20f, 0.48f),
        FloorTint = new Color(0.08f, 0.03f, 0.16f, 0.30f),
        WallTint  = new Color(0.04f, 0.015f, 0.09f, 1.0f),
        Accent    = new Color(0.60f, 0.35f, 0.75f, 0.18f),
        Speed = 0.65f, NoiseScale = 11f, ParticleDensity = 38f,
        Name = "神经连接层"
    };

    // F5 废弃孵化场 — 暗红锈迹 · 被遗忘的温床
    static readonly FloorTheme T_F5 = new FloorTheme {
        Main = new Color(0.03f, 0.008f, 0.015f),
        Sec  = new Color(0.18f, 0.025f, 0.07f),
        Tert = new Color(0.42f, 0.12f, 0.18f),
        FloorTint = new Color(0.11f, 0.03f, 0.05f, 0.30f),
        WallTint  = new Color(0.05f, 0.015f, 0.025f, 1.0f),
        Accent    = new Color(0.70f, 0.22f, 0.30f, 0.18f),
        Speed = 0.72f, NoiseScale = 12f, ParticleDensity = 42f,
        Name = "废弃孵化场"
    };

    // F6 感染核心 — 凝血深红 · 潜伏的心跳
    static readonly FloorTheme T_F6 = new FloorTheme {
        Main = new Color(0.04f, 0.006f, 0.01f),
        Sec  = new Color(0.24f, 0.02f, 0.04f),
        Tert = new Color(0.48f, 0.10f, 0.12f),
        FloorTint = new Color(0.14f, 0.025f, 0.03f, 0.30f),
        WallTint  = new Color(0.06f, 0.01f, 0.02f, 1.0f),
        Accent    = new Color(0.75f, 0.18f, 0.20f, 0.18f),
        Speed = 0.80f, NoiseScale = 13f, ParticleDensity = 46f,
        Name = "感染核心"
    };

    // F7+ 深渊核心 — 纯黑残息 · 最后的微光
    static readonly FloorTheme T_F7p = new FloorTheme {
        Main = new Color(0.05f, 0.00f, 0.005f),
        Sec  = new Color(0.30f, 0.01f, 0.03f),
        Tert = new Color(0.55f, 0.06f, 0.08f),
        FloorTint = new Color(0.16f, 0.02f, 0.02f, 0.30f),
        WallTint  = new Color(0.07f, 0.005f, 0.01f, 1.0f),
        Accent    = new Color(0.80f, 0.12f, 0.14f, 0.18f),
        Speed = 0.88f, NoiseScale = 14f, ParticleDensity = 50f,
        Name = "深渊核心"
    };

    public static FloorTheme GetFloorTheme(int floorId)
    {
        if (floorId <= 0) floorId = 1;
        if (floorId == 1) return T_F1;
        if (floorId == 2) return T_F2;
        if (floorId == 3) return T_F3;
        if (floorId == 4) return T_F4;
        if (floorId == 5) return T_F5;
        if (floorId == 6) return T_F6;
        return T_F7p;
    }

    protected override void Awake()
    {
        base.Awake();
        InitMaterials();
    }

    void InitMaterials()
    {
        var glowShader = Shader.Find("Game/UI/GlowingButton");
        var hpShader = Shader.Find("Game/UI/GradientHealthBar");
        var bgShader = Shader.Find("Game/UI/ParasiteBackground");

        if (glowShader != null)
        {
            _glowBtnMat = new Material(glowShader);
            _glowBtnMat.SetColor("_GlowColor", ParasiteTowerColorScheme.BioGlowCyan);
            _glowBtnMat.SetFloat("_GlowStrength", ParasiteTowerArtOptimization.UIPolish.ButtonStyle.GlowIntensity);
            _glowBtnMat.SetFloat("_GlowSpeed", 2f);
            _glowBtnMat.SetColor("_BackgroundColor", ParasiteTowerColorScheme.BioBgCard);
        }

        if (hpShader != null)
        {
            _hpBarMat = CreateHpBarMaterial(hpShader, false);
            _enemyHpBarMat = CreateHpBarMaterial(hpShader, true);
        }

        if (bgShader != null)
        {
            _bgShader = bgShader;
            _bgMat = new Material(bgShader);
            _bgMat.mainTexture = Texture2D.whiteTexture;
            _bgMat.SetColor("_MainColor", ParasiteTowerColorScheme.NearBlack);
            _bgMat.SetColor("_SecondaryColor", ParasiteTowerColorScheme.AbyssPurple);
            _bgMat.SetColor("_TertiaryColor", ParasiteTowerColorScheme.HealthGreen);
            _bgMat.SetFloat("_Speed", 0.6f);
            _bgMat.SetFloat("_NoiseScale", 8f);
        }
    }

    /// <summary>按楼层返回 ParasiteBackground Material, 每层独特 procedural pattern.</summary>
    public Material GetFloorBackgroundMat(int floorId)
    {
        if (_bgShader == null) return null;

        if (_floorBgCache.TryGetValue(floorId, out var cached) && cached != null)
            return cached;

        var theme = GetFloorTheme(floorId);
        var mat = new Material(_bgShader);
        mat.mainTexture = Texture2D.whiteTexture;

        // 每层参数微扰 — ±10% 浮动, 避免相邻层过于相似
        float perturb = (floorId * 0.137f) % 1f;
        float noiseScale = theme.NoiseScale * (0.9f + perturb * 0.2f);
        float particleDensity = theme.ParticleDensity * (0.88f + perturb * 0.24f);
        float speed = theme.Speed * (0.92f + perturb * 0.16f);
        float viscosity = 0.3f + perturb * 0.3f;
        float parallax = 0.05f + perturb * 0.08f;

        mat.SetColor("_MainColor", theme.Main);
        mat.SetColor("_SecondaryColor", theme.Sec);
        mat.SetColor("_TertiaryColor", theme.Tert);
        mat.SetFloat("_Speed", speed);
        mat.SetFloat("_NoiseScale", noiseScale);
        mat.SetFloat("_ParticleDensity", particleDensity);
        mat.SetFloat("_Viscosity", viscosity);
        mat.SetFloat("_ParallaxStrength", parallax);
        mat.SetFloat("_Seed", floorId * 7.31f + perturb * 100f);

        _floorBgCache[floorId] = mat;
        return mat;
    }

    /// <summary>按楼层返回地板格子微纹理 Sprite — 每层独特的生物膜质感 pattern.</summary>
    public Sprite GetFloorPatternSprite(int floorId)
    {
        if (_floorPatternCache.TryGetValue(floorId, out var cached) && cached != null)
            return cached;

        var theme = GetFloorTheme(floorId);
        const int size = 64;
        var tex = new Texture2D(size, size, TextureFormat.RGBA32, false);
        tex.filterMode = FilterMode.Bilinear;
        tex.wrapMode = TextureWrapMode.Repeat;

        float seed = floorId * 17.37f;
        Color[] pixels = new Color[size * size];

        for (int y = 0; y < size; y++)
        {
            for (int x = 0; x < size; x++)
            {
                float fx = (x - size / 2f) / (size / 2f);
                float fy = (y - size / 2f) / (size / 2f);
                float distFromCenter = Mathf.Sqrt(fx * fx + fy * fy);

                // 基础: 中心微亮, 边缘渐暗 (radial gradient)
                float baseV = Mathf.Lerp(1.0f, 0.65f, Mathf.Clamp01(distFromCenter * 1.2f));

                // 角点暗化 — 让格子有四角阴影
                float cornerDist = Mathf.Max(Mathf.Abs(fx), Mathf.Abs(fy));
                float cornerDark = Mathf.Lerp(0f, 0.35f, Mathf.Clamp01(cornerDist - 0.7f) * 3.3f);
                baseV *= (1f - cornerDark);

                // 主题 Accent 色的微弱噪点 (生物膜颗粒感)
                float n1 = Mathf.PerlinNoise((x + seed) * 0.25f, (y + seed * 1.3f) * 0.25f);
                float n2 = Mathf.PerlinNoise((x + seed * 2.7f) * 0.5f, (y + seed * 3.1f) * 0.5f);
                float noiseDetail = (n1 * 0.6f + n2 * 0.4f) * 0.12f;
                baseV += noiseDetail;

                // 少数亮点 — 模拟生物膜光泽
                float dotNoise = Mathf.PerlinNoise((x + seed * 5.7f) * 1.5f, (y + seed * 7.1f) * 1.5f);
                if (dotNoise > 0.88f)
                {
                    baseV += 0.22f;
                }

                // 边缘柔和渐变 (格子边缘 1px 淡出)
                float edgeFade = Mathf.Lerp(1f, 0.3f,
                    Mathf.Clamp01((distFromCenter - 0.85f) / 0.15f));
                baseV *= edgeFade;

                // 最终颜色: baseV 作为 alpha, 颜色用主题 FloorTint 的亮色版本
                Color tint = theme.FloorTint;
                float v = Mathf.Clamp01(baseV);
                pixels[y * size + x] = new Color(
                    Mathf.Lerp(tint.r, 1f, 0.3f) * v,
                    Mathf.Lerp(tint.g, 1f, 0.3f) * v,
                    Mathf.Lerp(tint.b, 1f, 0.3f) * v,
                    v * 0.6f);  // alpha 让格子有微纹理但不盖 shader 背景
            }
        }

        tex.SetPixels(pixels);
        tex.Apply();

        var sprite = Sprite.Create(tex, new Rect(0, 0, size, size), new Vector2(0.5f, 0.5f), size);
        _floorPatternCache[floorId] = sprite;
        return sprite;
    }

    Material CreateHpBarMaterial(Shader hpShader, bool enemy)
    {
        var mat = new Material(hpShader);
        if (enemy)
        {
            mat.SetColor("_LowHealthColor", ParasiteTowerColorScheme.CriticalRed);
            mat.SetColor("_MidHealthColor", ParasiteTowerColorScheme.DangerOrange);
            mat.SetColor("_HighHealthColor", new Color(0.9f, 0.2f, 0.2f));
            mat.SetColor("_GlowColor", ParasiteTowerColorScheme.GlowRedIntense);
            mat.SetColor("_FlowColor", new Color(1f, 0.4f, 0.4f));
            mat.SetFloat("_FlowSpeed", 1.5f);
        }
        else
        {
            mat.SetColor("_LowHealthColor", ParasiteTowerColorScheme.CriticalRed);
            mat.SetColor("_MidHealthColor", ParasiteTowerColorScheme.WarningYellow);
            mat.SetColor("_HighHealthColor", ParasiteTowerColorScheme.HealthGreen);
            mat.SetColor("_GlowColor", ParasiteTowerColorScheme.BioGlowCyan);
            mat.SetColor("_FlowColor", new Color(0.8f, 1f, 0.9f));
            mat.SetFloat("_FlowSpeed", 1.2f);
        }
        mat.SetColor("_BorderColor", ParasiteTowerColorScheme.BioBorderDim);
        mat.SetFloat("_GlowIntensity", 0.8f);
        mat.SetFloat("_FlowWidth", 0.12f);
        mat.SetFloat("_FlowIntensity", 0.8f);
        return mat;
    }

    public Material GetGlowButtonMat(Color glowColor)
    {
        if (_glowBtnMat == null) return null;
        if (_glowCache.TryGetValue(glowColor, out var cached)) return cached;
        var mat = new Material(_glowBtnMat);
        mat.SetColor("_GlowColor", glowColor);
        _glowCache[glowColor] = mat;
        return mat;
    }

    public void ApplyHpBar(Image img, float fill, bool enemy = false)
    {
        var mat = enemy ? _enemyHpBarMat : _hpBarMat;
        if (mat == null || img == null) return;
        img.material = new Material(mat);
        img.color = Color.white;
        img.material.SetFloat("_FillAmount", fill);
    }

    public void UpdateHpBarFill(Image img, float fill)
    {
        if (img == null || img.material == null) return;
        img.material.SetFloat("_FillAmount", fill);
    }

    public void ApplyGlowButton(Image img, Color glowColor)
    {
        if (img == null) return;
        var mat = GetGlowButtonMat(glowColor);
        if (mat != null) img.material = mat;
    }

    public void ApplyPanelBackground(Image img)
    {
        if (img == null || _bgMat == null) return;
        img.material = _bgMat;
        img.color = Color.white;
    }

    public Material GetBgMat() => _bgMat;

    protected override void OnDestroy()
    {
        foreach (var mat in _glowCache.Values)
        {
            if (mat != null) Destroy(mat);
        }
        _glowCache.Clear();
        foreach (var mat in _floorBgCache.Values)
        {
            if (mat != null) Destroy(mat);
        }
        _floorBgCache.Clear();
        foreach (var spr in _floorPatternCache.Values)
        {
            if (spr != null && spr.texture != null) Destroy(spr.texture);
        }
        _floorPatternCache.Clear();
        if (_glowBtnMat != null) Destroy(_glowBtnMat);
        if (_hpBarMat != null) Destroy(_hpBarMat);
        if (_enemyHpBarMat != null) Destroy(_enemyHpBarMat);
        if (_bgMat != null) Destroy(_bgMat);
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
