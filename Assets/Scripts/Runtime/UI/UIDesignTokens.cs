using UnityEngine;

/// <summary>
/// UI设计令牌系统 - 统一管理设计变量
/// 基于参考分辨率 540x960，所有尺寸以像素为单位，通过CanvasScaler自动适配
/// 遵循UICraft设计原则：间距系统、排版层级、色彩语义、动效时长
/// </summary>
public static class UIDesignTokens
{
    // 参考分辨率（与CanvasScaler一致）
    public const float RefWidth = 540f;
    public const float RefHeight = 960f;

    // ============ 间距系统 (Spacing Scale) ============
    // 基于 4px 基础单位，按 1.5x 递增
    public static class Space
    {
        public const float XS = 2f;    // 超细 - 图标内边距
        public const float S = 4f;     // 细 - 紧凑间距
        public const float M = 8f;     // 中 - 标准间距
        public const float L = 12f;    // 大 - 分组间距
        public const float XL = 16f;   // 超大 - 段落间距
        public const float XXL = 24f;  // 特大 - 区块间距
    }

    // ============ 字体排版系统 (Typography Scale) ============
    // 基于 8px 基础，按 1.25x 递增（符合视觉节奏）
    public static class FontSize
    {
        public const float Caption = 9f;      // 标注 - 微小文字（标签、提示）
        public const float CaptionLg = 10f;   // 大标注 - 次重要信息
        public const float Body = 12f;        // 正文 - 主要阅读文本
        public const float BodyLg = 14f;      // 大正文 - 重要文本
        public const float Subheading = 16f;  // 副标题 - 区块标题
        public const float Heading = 20f;     // 标题 - 页面主标题
        public const float Display = 24f;     // 展示 - Hero元素
    }

    // ============ 图标尺寸系统 (Icon Sizes) ============
    public static class Icon
    {
        // 导航栏图标
        public const float NavNormal = 13f;    // 普通导航图标 (原22 ×0.6)
        public const float NavCenter = 17f;    // 中心导航图标（闯塔）(原28 ×0.6)
        
        // 功能按钮图标
        public const float SideBtn = 22f;      // 侧边栏按钮 (原36 ×0.6)
        public const float Avatar = 38f;       // 头像 (保持原值)
        
        // 弹窗/对话框图标
        public const float DialogIcon = 36f;   // 弹窗标题图标 (保持原值)
        public const float BestiaryIcon = 56f; // 图鉴图标 (保持原值)
        
        // 模式选择图标
        public const float ClassIcon = 52f;    // 职业图标 (保持原值)
    }

    // ============ 组件尺寸系统 (Component Sizes) ============
    public static class Component
    {
        // 底部导航栏
        public const float NavBarHeight = 68f;      // 导航栏高度 (自适应基准: 格子/图标均按此比例推导)
        public const float NavLabelH = 7f;          // 导航标签高度 (原11 ×0.6)
        public const float NavLabelFont = 5f;       // 导航标签字号 (原8 ×0.6)
        
        // 侧边栏按钮
        public const float SideBtnSize = 26f;       // 侧边按钮尺寸 (原44 ×0.6)
        public const float SideLabelFont = 7f;      // 侧边标签字号 (原11 ×0.6)
        public const float SideLabelH = 8f;        // 侧边标签高度 (原14 ×0.6)
        
        // 顶部栏
        public const float TopBarHeight = 70f;      // 顶部栏高度
        public const float TopBarFont = 14f;        // 顶部栏主字号
        public const float TopBarSmallFont = 9f;    // 顶部栏小字号
        
        // 头像
        public const float AvatarSize = 38f;        // 头像尺寸 (保持原值)
        public const float AvatarFont = 22f;        // 头像后备字号 (保持原值)
        
        // 弹窗
        public const float DialogIconSize = 32f;    // 弹窗图标尺寸 (保持原值)
        public const float DialogIconPad = 3f;       // 弹窗图标内边距 (保持原值)
    }

    // ============ 布局常量 (Layout Constants) ============
    public static class Layout
    {
        // 底部导航栏位置
        public const float NavBarBottom = 0f;
        public const float NavBarTop = Component.NavBarHeight;
        
        // 滚动区域偏移（底部留出导航栏空间）
        public const float ScrollBottomOffset = Component.NavBarHeight + 8f;
        
        // 顶部栏位置
        public const float TopBarTop = 0f;
        public const float TopBarBottom = Component.TopBarHeight;
        
        // 侧边栏锚点位置（屏幕百分比）
        public static class SideAnchors
        {
            // 左侧按钮 (屏幕左侧 12%)
            public const float LeftX = 0.14f;
            // 右侧按钮 (屏幕右侧 12%)
            public const float RightX = 0.86f;
            
            // 垂直位置（从上到下）
            public const float Row1Y = 0.72f;  // 第一行
            public const float Row2Y = 0.52f;  // 第二行
            public const float Row3Y = 0.32f;  // 第三行
        }
    }

    // ============ 动效时长系统 (Motion Duration) ============
    public static class Motion
    {
        // 瞬时反馈 (100-150ms)
        public const float Instant = 0.12f;       // 按钮按下
        public const float Hover = 0.15f;         // 悬停响应
        
        // 状态变化 (200-300ms)
        public const float StateChange = 0.25f;   // 开关切换
        public const float OverlayFade = 0.3f;    // 遮罩淡入淡出
        
        // 布局变化 (300-500ms)
        public const float PanelSlide = 0.35f;    // 面板滑入
        public const float DialogScale = 0.3f;    // 弹窗缩放
        
        // 入场动画 (500-800ms)
        public const float PageEnter = 0.5f;      // 页面进入
        public const float StaggerDelay = 0.06f;  // 错落延迟
    }

    // ============ 色彩语义系统 (Color Semantics) ============
    // 委托至 ParasiteTowerColorScheme，保持单一视觉源
    // 注意：类名用 Colors 而非 Color，避免遮蔽 UnityEngine.Color
    public static class Colors
    {
        // 背景色 - 生物朋克暗黑风格
        public static readonly UnityEngine.Color ScreenBg = ParasiteTowerColorScheme.NearBlack;
        public static readonly UnityEngine.Color PanelBg = ParasiteTowerColorScheme.UiPanelBg;
        public static readonly UnityEngine.Color CardBg = ParasiteTowerColorScheme.UiCardBg;
        public static readonly UnityEngine.Color NavBarBg = new UnityEngine.Color(0.06f, 0.04f, 0.10f, 0.94f);
        public static readonly UnityEngine.Color TopBarBg = new UnityEngine.Color(0.06f, 0.04f, 0.10f, 0.90f);
        public static readonly UnityEngine.Color OverlayBg = ParasiteTowerColorScheme.UiOverlayBg;
        
        // 主强调色 - 生物青色 (Bio-Cyan)
        public static readonly UnityEngine.Color Primary = ParasiteTowerColorScheme.HealthGreen;  // #00ffd0
        public static readonly UnityEngine.Color PrimaryDim = new UnityEngine.Color(0f, 1f, 0.816f, 0.15f);
        public static readonly UnityEngine.Color PrimaryBorder = ParasiteTowerColorScheme.BioBorder;
        public static readonly UnityEngine.Color PrimaryGlow = ParasiteTowerColorScheme.BioGlowCyan;
        
        // 次强调色 - 深渊紫 (Abyss Purple)
        public static readonly UnityEngine.Color Secondary = ParasiteTowerColorScheme.AbyssPurple;
        public static readonly UnityEngine.Color SecondaryDim = new UnityEngine.Color(0.65f, 0.25f, 0.95f, 0.12f);
        public static readonly UnityEngine.Color SecondaryBorder = ParasiteTowerColorScheme.Membrane;
        
        // 功能色
        public static readonly UnityEngine.Color Success = ParasiteTowerColorScheme.HealthGreen;
        public static readonly UnityEngine.Color Warning = ParasiteTowerColorScheme.WarningYellow;
        public static readonly UnityEngine.Color Danger = ParasiteTowerColorScheme.DangerOrange;
        public static readonly UnityEngine.Color Critical = ParasiteTowerColorScheme.CriticalRed;
        public static readonly UnityEngine.Color Info = ParasiteTowerColorScheme.InfoBlue;
        
        // 文字颜色 - 保持高对比度可读性
        public static readonly UnityEngine.Color TextBright = ParasiteTowerColorScheme.White;
        public static readonly UnityEngine.Color TextPrimary = ParasiteTowerColorScheme.LightGray;
        public static readonly UnityEngine.Color TextSecondary = ParasiteTowerColorScheme.MidGray;
        public static readonly UnityEngine.Color TextDim = ParasiteTowerColorScheme.DarkGray;
        
        // 按钮状态色
        public static readonly UnityEngine.Color BtnNormal = ParasiteTowerColorScheme.UiBtnBg;
        public static readonly UnityEngine.Color BtnNormalBorder = new UnityEngine.Color(0f, 1f, 0.816f, 0.25f);
        public static readonly UnityEngine.Color BtnHover = ParasiteTowerColorScheme.UiBtnBgHover;
        public static readonly UnityEngine.Color BtnHoverBorder = new UnityEngine.Color(0f, 1f, 0.816f, 0.5f);
        public static readonly UnityEngine.Color BtnPressed = new UnityEngine.Color(0.04f, 0.08f, 0.06f, 0.95f);
        public static readonly UnityEngine.Color BtnCenterBg = new UnityEngine.Color(0.04f, 0.12f, 0.10f, 0.95f);
        public static readonly UnityEngine.Color BtnCenterBorder = new UnityEngine.Color(0f, 1f, 0.816f, 0.6f);
        
        // 侧边按钮色彩语义
        public static readonly UnityEngine.Color SidePurple = ParasiteTowerColorScheme.AbyssPurple;
        public static readonly UnityEngine.Color SideCyan = ParasiteTowerColorScheme.HealthGreen;
        public static readonly UnityEngine.Color SideBtnBg = new UnityEngine.Color(0.07f, 0.05f, 0.12f, 0.55f);
        
        // 面板边框发光色
        public static readonly UnityEngine.Color BorderGlowCyan = new UnityEngine.Color(0f, 1f, 0.816f, 0.35f);
        public static readonly UnityEngine.Color BorderGlowPurple = new UnityEngine.Color(0.65f, 0.25f, 0.95f, 0.35f);
        
        // 分隔线
        public static readonly UnityEngine.Color Separator = new UnityEngine.Color(0f, 1f, 0.816f, 0.15f);
    }

    // ============ 边框与发光 (Border & Glow) ============
    public static class Effect
    {
        // Outline效果
        public const float OutlineSmall = 0.5f;
        public const float OutlineNormal = 1f;
        public const float OutlineLarge = 2f;
        
        // 阴影偏移
        public const float ShadowSmall = 1f;
        public const float ShadowNormal = 2f;
        public const float ShadowLarge = 3f;
    }
}
