using UnityEngine;

/// <summary>
/// 原游戏粒子特效和视觉效果（基于styles.css）
/// 生物朋克 × 新艺术运动风格特效
/// </summary>
public static class ParasiteTowerEffects
{
    // 发光效果强度
    public static class GlowIntensities
    {
        public const float Low = 0.3f;
        public const float Medium = 0.6f;
        public const float High = 1.0f;
        public const float Ultra = 1.5f;
    }

    // 粒子系统配置
    public static class ParticleConfigs
    {
        // 污染粒子
        public static class Pollution
        {
            public static readonly Color StartColor = new Color(0.8f, 0.3f, 1f, 0.8f);
            public static readonly Color EndColor = new Color(0.2f, 0f, 0.4f, 0.2f);
            public const float StartSize = 0.1f;
            public const float EndSize = 0.5f;
            public const float StartSpeed = 0.5f;
            public const float Lifetime = 2f;
            public const int MaxParticles = 50;
            public const float EmissionRate = 10f;
        }

        // 附身粒子
        public static class Possess
        {
            public static readonly Color StartColor = new Color(0f, 1f, 0.8f, 1f);
            public static readonly Color EndColor = new Color(0.5f, 0f, 0.8f, 0.3f);
            public const float StartSize = 0.15f;
            public const float EndSize = 0.8f;
            public const float StartSpeed = 1f;
            public const float Lifetime = 1.5f;
            public const int MaxParticles = 80;
            public const float EmissionRate = 20f;
        }

        // 治愈粒子
        public static class Heal
        {
            public static readonly Color StartColor = new Color(0.3f, 1f, 0.5f, 1f);
            public static readonly Color EndColor = new Color(0f, 0.5f, 0.3f, 0.2f);
            public const float StartSize = 0.08f;
            public const float EndSize = 0.4f;
            public const float StartSpeed = 0.8f;
            public const float Lifetime = 1f;
            public const int MaxParticles = 40;
            public const float EmissionRate = 15f;
        }

        // 攻击粒子
        public static class Attack
        {
            public static readonly Color StartColor = new Color(1f, 0.4f, 0.2f, 1f);
            public static readonly Color EndColor = new Color(0.5f, 0.1f, 0f, 0.2f);
            public const float StartSize = 0.12f;
            public const float EndSize = 0.6f;
            public const float StartSpeed = 1.5f;
            public const float Lifetime = 0.8f;
            public const int MaxParticles = 60;
            public const float EmissionRate = 30f;
        }

        // 成就解锁粒子
        public static class Achievement
        {
            public static readonly Color StartColor = new Color(1f, 0.84f, 0f, 1f);
            public static readonly Color EndColor = new Color(1f, 0.5f, 0f, 0.3f);
            public const float StartSize = 0.2f;
            public const float EndSize = 1f;
            public const float StartSpeed = 1.2f;
            public const float Lifetime = 2f;
            public const int MaxParticles = 100;
            public const float EmissionRate = 50f;
        }
    }

    // 屏幕特效配置
    public static class ScreenEffects
    {
        // 污染视觉效果
        public static class PollutionOverlay
        {
            // 30-70%: 紫粉渐变，70%+ 由 ScreenEffectsManager 动态生成血红效果
            public static readonly Color Level30 = new Color(0.5f, 0.2f, 0.7f, 0.18f);
            public static readonly Color Level50 = new Color(0.7f, 0.15f, 0.5f, 0.28f);
            public static readonly Color Level70 = new Color(0.85f, 0.12f, 0.25f, 0.4f);
            public static readonly Color Level85 = new Color(0.9f, 0.08f, 0.15f, 0.55f);
            public static readonly Color Level100 = new Color(1f, 0.95f, 0.95f, 0.9f);
        }

        // 高污染RGB分离参数
        public static class RGBSeparation
        {
            public const float StartPollution = 75f;
            public const float MaxPollution = 100f;
            public const float MinShift = 3f;
            public const float MaxShift = 22f;
            public const float JitterThreshold = 90f;
        }

        // 扫描线参数
        public static class Scanlines
        {
            public const float StartPollution = 70f;
            public const float ScrollThreshold = 90f;
            public const float ScrollSpeedMin = 20f;
            public const float ScrollSpeedMax = 100f;
        }

        // Glitch效果参数
        public static class Glitch
        {
            public const float StartPollution = 65f;
            public const float MaxPollution = 100f;
            public const float MinInterval = 0.3f;
            public const float MaxInterval = 3.2f;
        }

        // 屏幕闪烁参数
        public static class Flicker
        {
            public const float StartPollution = 95f;
            public const float MaxPollution = 100f;
            public const float MinInterval = 0.3f;
            public const float MaxInterval = 1.2f;
        }

        // 闪回效果
        public static class Flashback
        {
            public const float Duration = 0.3f;
            public static readonly Color FlashColor = new Color(1f, 1f, 1f, 0.8f);
        }

        // 屏幕震动
        public static class ScreenShake
        {
            public const float LightIntensity = 0.1f;
            public const float MediumIntensity = 0.3f;
            public const float HeavyIntensity = 0.6f;
            public const float LightDuration = 0.2f;
            public const float MediumDuration = 0.4f;
            public const float HeavyDuration = 0.6f;
        }
    }

    // 动画曲线配置
    public static class AnimationCurves
    {
        // 呼吸式发光
        public static AnimationCurve GlowPulse()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0.8f);
            curve.AddKey(0.5f, 1.2f);
            curve.AddKey(1f, 0.8f);
            return curve;
        }

        // 渐入渐出
        public static AnimationCurve FadeInOut()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(0.2f, 1f);
            curve.AddKey(0.8f, 1f);
            curve.AddKey(1f, 0f);
            return curve;
        }

        // 弹跳效果
        public static AnimationCurve Bounce()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(0.3f, 1.2f);
            curve.AddKey(0.5f, 0.8f);
            curve.AddKey(0.7f, 1.1f);
            curve.AddKey(1f, 1f);
            return curve;
        }

        // 抖动效果
        public static AnimationCurve Jitter()
        {
            AnimationCurve curve = new AnimationCurve();
            curve.AddKey(0f, 0f);
            curve.AddKey(0.1f, 1f);
            curve.AddKey(0.2f, -0.8f);
            curve.AddKey(0.3f, 0.6f);
            curve.AddKey(0.4f, -0.4f);
            curve.AddKey(0.5f, 0.2f);
            curve.AddKey(0.6f, -0.1f);
            curve.AddKey(1f, 0f);
            return curve;
        }
    }

    // 材质Shader配置
    public static class ShaderConfigs
    {
        // 发光材质属性
        public static class GlowMaterial
        {
            public const string EmissionColor = "_EmissionColor";
            public const string EmissionIntensity = "_EmissionIntensity";
            public const string GlowColor = "_GlowColor";
            public const string GlowPower = "_GlowPower";
        }

        // 像素风格渲染
        public static class PixelArt
        {
            public const int PixelPerUnit = 32;
            public const FilterMode DefaultFilterMode = FilterMode.Point;
            public const TextureWrapMode DefaultWrapMode = TextureWrapMode.Clamp;
        }
    }

    // 获取污染视觉效果颜色
    public static Color GetPollutionOverlayColor(float pollution)
    {
        if (pollution < 30f) return Color.clear;
        if (pollution < 50f)
        {
            float t = Mathf.InverseLerp(30f, 50f, pollution);
            return Color.Lerp(ScreenEffects.PollutionOverlay.Level30, ScreenEffects.PollutionOverlay.Level50, t);
        }
        if (pollution < 70f)
        {
            float t = Mathf.InverseLerp(50f, 70f, pollution);
            return Color.Lerp(ScreenEffects.PollutionOverlay.Level50, ScreenEffects.PollutionOverlay.Level70, t);
        }
        // 70%+ 交给 ScreenEffectsManager 动态处理
        return ScreenEffects.PollutionOverlay.Level70;
    }

    // 获取职业主题光晕颜色
    public static Color GetClassGlowColor(string classId)
    {
        return ParasiteTowerColorScheme.GetClassPrimaryColor(classId);
    }

    // 计算发光强度随时间变化
    public static float CalculateGlowIntensity(float time, float baseIntensity = 1f)
    {
        return baseIntensity * (0.8f + 0.4f * Mathf.Sin(time * 2f));
    }

    // 污染虹彩效果色相偏移 — 90% 开始，强度随污染度提升
    public static float GetPollutionHueShift(float pollution, float time)
    {
        if (pollution < 90f) return 0f;
        float intensity = Mathf.InverseLerp(90f, 100f, pollution);
        float baseShift = Mathf.Sin(time * (2f + intensity * 4f)) * 20f * intensity;
        if (pollution >= 95f)
            baseShift += Mathf.Sin(time * 12f) * 8f * intensity;
        return baseShift;
    }

    // 获取污染视觉阶段标签 — 用于UI显示
    public static string GetPollutionStageLabel(float pollution)
    {
        if (pollution < 30f) return "";
        if (pollution < 60f) return "轻微感染";
        if (pollution < 80f) return "深度侵蚀";
        if (pollution < 95f) return "濒临崩溃";
        return "完全异变";
    }

    // 获取污染视觉强度 (0-1)，用于驱动各种效果
    public static float GetPollutionVisualIntensity(float pollution)
    {
        if (pollution < 30f) return 0f;
        if (pollution < 70f)
        {
            return Mathf.Lerp(0.1f, 0.4f, (pollution - 30f) / 40f);
        }
        else if (pollution < 90f)
        {
            return Mathf.Lerp(0.4f, 0.75f, (pollution - 70f) / 20f);
        }
        else
        {
            return Mathf.Lerp(0.75f, 1f, (pollution - 90f) / 10f);
        }
    }
}
