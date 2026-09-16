using UnityEngine;

/// <summary>
/// 美术优化和精美化配置
/// 生物朋克 × 新艺术运动风格全面升级
/// </summary>
public static class ParasiteTowerArtOptimization
{
    // 画面质量配置
    public static class QualityPresets
    {
        public static class Ultra
        {
            public const int PixelLightCount = 4;
            public const int TextureQuality = 0; // Full Res
            public const int ShadowQuality = 2; // Soft Shadows
            public const int ShadowResolution = 2; // High
            public const int ShadowDistance = 50;
            public const int ShadowCascades = 4;
            public const float ShadowCascade2Split = 0.333f;
            public const float ShadowNearPlane = 3f;
            public const float ShadowDistanceScale = 1f;
            public const bool SoftParticles = true;
            public const bool RealtimeReflectionProbes = true;
            public const float LODBias = 1f;
            public const float MaximumLODLevel = 0f;
            public const int AntiAliasing = 4; // MSAA 4x
        }

        public static class High
        {
            public const int PixelLightCount = 3;
            public const int TextureQuality = 0;
            public const int ShadowQuality = 2;
            public const int ShadowResolution = 1;
            public const int ShadowDistance = 40;
            public const int ShadowCascades = 2;
            public const float ShadowCascade2Split = 0.25f;
            public const float ShadowNearPlane = 3f;
            public const float ShadowDistanceScale = 1f;
            public const bool SoftParticles = true;
            public const bool RealtimeReflectionProbes = true;
            public const float LODBias = 1f;
            public const float MaximumLODLevel = 0f;
            public const int AntiAliasing = 2; // MSAA 2x
        }

        public static class Medium
        {
            public const int PixelLightCount = 2;
            public const int TextureQuality = 1; // Half Res
            public const int ShadowQuality = 1; // Hard Shadows
            public const int ShadowResolution = 0; // Low
            public const int ShadowDistance = 25;
            public const int ShadowCascades = 2;
            public const float ShadowCascade2Split = 0.25f;
            public const float ShadowNearPlane = 3f;
            public const float ShadowDistanceScale = 1f;
            public const bool SoftParticles = false;
            public const bool RealtimeReflectionProbes = false;
            public const float LODBias = 0.8f;
            public const float MaximumLODLevel = 1f;
            public const int AntiAliasing = 0; // Off
        }
    }

    // UI精美化配置
    public static class UIPolish
    {
        // 按钮样式
        public static class ButtonStyle
        {
            public const float CornerRadius = 8f;
            public const float BorderWidth = 2f;
            public const float GlowIntensity = 0.8f;
            public const float HoverScale = 1.05f;
            public const float PressScale = 0.95f;
            public const float AnimationDuration = 0.15f;

            public static readonly Color NormalBorder = new Color(0f, 1f, 0.8f, 0.4f);
            public static readonly Color HoverBorder = new Color(0f, 1f, 0.8f, 0.8f);
            public static readonly Color PressBorder = new Color(0.5f, 0f, 1f, 0.8f);
            public static readonly Color DisabledBorder = new Color(0.3f, 0.3f, 0.4f, 0.3f);

            // 关闭按钮专用样式
            public static class CloseButton
            {
                public static readonly Color NormalBg = new Color(0.14f, 0.08f, 0.12f, 0.95f);
                public static readonly Color NormalBorder = new Color(0.9f, 0.3f, 0.3f, 0.45f);
                public static readonly Color HoverBg = new Color(0.35f, 0.12f, 0.15f, 1f);
                public static readonly Color HoverBorder = new Color(1f, 0.4f, 0.4f, 0.9f);
                public static readonly Color PressBg = new Color(0.12f, 0.05f, 0.08f, 1f);
                public static readonly Color PressBorder = new Color(1f, 0.6f, 0.6f, 1f);
                public static readonly Color NormalText = new Color(0.95f, 0.55f, 0.55f);
                public static readonly Color HoverText = new Color(1f, 0.85f, 0.85f);
                public static readonly Color GlowColor = new Color(1f, 0.3f, 0.3f, 0.3f);
            }
        }

        // 面板样式
        public static class PanelStyle
        {
            public const float CornerRadius = 12f;
            public const float BorderWidth = 1f;
            public const float GlowIntensity = 0.5f;
            public const float BackgroundOpacity = 0.95f;

            public static readonly Color BorderColor = new Color(0f, 1f, 0.8f, 0.25f);
            public static readonly Color BackgroundColor = new Color(0.08f, 0.06f, 0.16f, 0.95f);
            public static readonly Color HighlightColor = new Color(0.7f, 0.4f, 1f, 0.15f);
        }

        // 文本样式
        public static class TextStyle
        {
            public const float LineSpacing = 1.2f;
            public const float LetterSpacing = 0.5f;
            public const float GlowIntensity = 0.6f;

            public static readonly Color PrimaryText = new Color(0.94f, 0.91f, 1f, 1f);
            public static readonly Color SecondaryText = new Color(0.53f, 0.47f, 0.67f, 1f);
            public static readonly Color AccentText = new Color(0f, 1f, 0.8f, 1f);
            public static readonly Color WarningText = new Color(1f, 0.8f, 0f, 1f);
            public static readonly Color DangerText = new Color(1f, 0.42f, 0.2f, 1f);
        }

        // 滚动条样式
        public static class ScrollbarStyle
        {
            public const float HandleWidth = 8f;
            public const float HandleHeight = 8f;
            public const float CornerRadius = 4f;
            public const float BackgroundOpacity = 0.3f;
        }
    }

    // 怪物图标美化
    public static class MonsterArt
    {
        // 图标渲染
        public static class IconRendering
        {
            public const float IconSize = 128;
            public const float PaddingSize = 256;
            public const float BorderWidth = 3f;
            public const float GlowSize = 8f;
            public const float OutlineWidth = 2f;
            public const float BossScale = 1.2f;
            public const float EliteScale = 1.1f;
        }

        // 动画参数
        public static class AnimationParams
        {
            public const float IdleBreathMin = 0.98f;
            public const float IdleBreathMax = 1.02f;
            public const float IdleBreathSpeed = 2f;
            public const float HoverScale = 1.1f;
            public const float HoverSpeed = 3f;
            public const float AttackShake = 0.05f;
            public const float DamageFlash = 0.5f;
            public const float DeathScale = 1.5f;
            public const float DeathFade = 0.5f;
        }
    }

    // 特效增强
    public static class VFXEnhancements
    {
        // 屏幕后处理
        public static class PostProcessing
        {
            public const float BloomIntensity = 1.2f;
            public const float BloomThreshold = 0.8f;
            public const float BloomKnee = 0.5f;
            public const float VignetteIntensity = 0.4f;
            public const float VignetteSmoothness = 0.8f;
            public const float ChromaticAberration = 0.2f;
            public const float FilmGrainIntensity = 0.1f;
            public const float FilmGrainSize = 1f;
        }

        // 光晕效果
        public static class LensFlare
        {
            public const float Intensity = 0.5f;
            public const float Threshold = 0.6f;
        }

        // 动态模糊
        public static class MotionBlur
        {
            public const float Intensity = 0.1f;
            public const float ShutterAngle = 180f;
        }
    }

    // 音效可视化
    public static class AudioVisuals
    {
        // 频谱可视化
        public static class Spectrum
        {
            public const int BandCount = 8;
            public const float BarWidth = 20f;
            public const float BarHeight = 100f;
            public const float BarSpacing = 5f;
            public const float AnimationSmoothness = 0.1f;
        }

        // 节拍检测
        public static class BeatDetection
        {
            public const float Threshold = 0.8f;
            public const float MinInterval = 0.1f;
            public const float ReleaseTime = 0.15f;
        }
    }

    // 性能优化
    public static class PerformanceOptimizations
    {
        // 对象池
        public static class ObjectPooling
        {
            public const int ParticlePoolSize = 50;
            public const int UIPoolSize = 20;
            public const int MonsterPoolSize = 30;
        }

        // 加载优化
        public static class Loading
        {
            public const bool AsyncLoading = true;
            public const int MaxAsyncOperations = 2;
            public const float LoadTimeout = 10f;
        }

        // 内存管理
        public static class Memory
        {
            public const int MaxCachedTextures = 50;
            public const int MaxCachedSprites = 100;
            public const float UnloadUnusedTime = 60f;
        }
    }

    // 分辨率适配
    public static class ResponsiveDesign
    {
        // 不同设备适配
        public static class DevicePresets
        {
            // 手机
            public const float PhoneUIScale = 1f;
            public const float PhoneFontScale = 1f;

            // 平板
            public const float TabletUIScale = 1.2f;
            public const float TabletFontScale = 1.1f;

            // 桌面
            public const float DesktopUIScale = 1f;
            public const float DesktopFontScale = 1f;
        }

        // 安全区域
        public static class SafeArea
        {
            public const float TopPadding = 10f;
            public const float BottomPadding = 10f;
            public const float LeftPadding = 10f;
            public const float RightPadding = 10f;
        }
    }

    // 应用高质量画面设置
    public static void ApplyHighQualitySettings()
    {
        QualitySettings.SetQualityLevel(5, true);
        QualitySettings.pixelLightCount = QualityPresets.Ultra.PixelLightCount;
        QualitySettings.globalTextureMipmapLimit = QualityPresets.Ultra.TextureQuality;
        QualitySettings.shadows = (ShadowQuality)QualityPresets.Ultra.ShadowQuality;
        QualitySettings.shadowResolution = (ShadowResolution)QualityPresets.Ultra.ShadowResolution;
        QualitySettings.shadowDistance = QualityPresets.Ultra.ShadowDistance;
        QualitySettings.shadowCascades = QualityPresets.Ultra.ShadowCascades;
        QualitySettings.shadowCascade2Split = QualityPresets.Ultra.ShadowCascade2Split;
        QualitySettings.shadowNearPlaneOffset = QualityPresets.Ultra.ShadowNearPlane;
        QualitySettings.shadowDistance = QualityPresets.Ultra.ShadowDistance * QualityPresets.Ultra.ShadowDistanceScale;
        QualitySettings.softParticles = QualityPresets.Ultra.SoftParticles;
        QualitySettings.realtimeReflectionProbes = QualityPresets.Ultra.RealtimeReflectionProbes;
        QualitySettings.lodBias = QualityPresets.Ultra.LODBias;
        QualitySettings.maximumLODLevel = (int)QualityPresets.Ultra.MaximumLODLevel;
        QualitySettings.antiAliasing = QualityPresets.Ultra.AntiAliasing;
    }

    // 应用中等质量画面设置
    public static void ApplyMediumQualitySettings()
    {
        QualitySettings.SetQualityLevel(2, true);
        QualitySettings.pixelLightCount = QualityPresets.Medium.PixelLightCount;
        QualitySettings.globalTextureMipmapLimit = QualityPresets.Medium.TextureQuality;
        QualitySettings.shadows = (ShadowQuality)QualityPresets.Medium.ShadowQuality;
        QualitySettings.shadowResolution = (ShadowResolution)QualityPresets.Medium.ShadowResolution;
        QualitySettings.shadowDistance = QualityPresets.Medium.ShadowDistance;
        QualitySettings.shadowCascades = QualityPresets.Medium.ShadowCascades;
        QualitySettings.shadowCascade2Split = QualityPresets.Medium.ShadowCascade2Split;
        QualitySettings.shadowNearPlaneOffset = QualityPresets.Medium.ShadowNearPlane;
        QualitySettings.shadowDistance = QualityPresets.Medium.ShadowDistance * QualityPresets.Medium.ShadowDistanceScale;
        QualitySettings.softParticles = QualityPresets.Medium.SoftParticles;
        QualitySettings.realtimeReflectionProbes = QualityPresets.Medium.RealtimeReflectionProbes;
        QualitySettings.lodBias = QualityPresets.Medium.LODBias;
        QualitySettings.maximumLODLevel = (int)QualityPresets.Medium.MaximumLODLevel;
        QualitySettings.antiAliasing = QualityPresets.Medium.AntiAliasing;
    }

    /// <summary>按平台自动选择画质档位。</summary>
    public static void ApplyQualityForPlatform()
    {
        bool mobile = Application.isMobilePlatform;
        bool lowEnd = SystemInfo.systemMemorySize > 0 && SystemInfo.systemMemorySize < 4096;

        if (mobile || lowEnd)
            ApplyMediumQualitySettings();
        else
            ApplyHighQualitySettings();
    }
}
