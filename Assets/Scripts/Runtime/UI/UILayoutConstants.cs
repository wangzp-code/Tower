using UnityEngine;

/// <summary>
/// UI布局常量集中管理类
/// 所有归一化坐标（0-1范围）的布局值定义在此，便于统一调整
/// </summary>
public static class UILayoutConstants
{
    /// <summary>
    /// 战斗界面布局常量
    /// </summary>
    public static class Combat
    {
        // 顶部信息区域 (0.94 - 1.0)
        public const float TopBarTop = 1.0f;
        public const float TopBarBottom = 0.94f;

        // 卡片区域 (0.54 - 0.88)
        public const float CardAreaTop = 0.88f;
        public const float CardAreaBottom = 0.54f;

        // 附身成功率条 (0.88 - 1.0)
        public const float PossessBarTop = 1.0f;
        public const float PossessBarBottom = 0.88f;

        // 寄主卡片水平范围 (0 - 0.46)
        public const float PlayerCardLeft = 0f;
        public const float PlayerCardRight = 0.46f;

        // 怪物卡片水平范围 (0.54 - 1.0)
        public const float EnemyCardLeft = 0.54f;
        public const float EnemyCardRight = 1.0f;

        // 中间分隔线
        public const float CenterLineX = 0.5f;
        public const float CenterLineWidth = 0.02f;

        // VS标记位置 (0.47 - 0.53, 0.45 - 0.55)
        public const float VsCenterX = 0.5f;
        public const float VsWidth = 0.06f;
        public const float VsCenterY = 0.5f;
        public const float VsHeight = 0.1f;

        // 附身进度区域 (0.15 - 0.65)
        public const float PossessAreaTop = 0.65f;
        public const float PossessAreaBottom = 0.15f;
        public const float PossessAreaLeft = 0.38f;
        public const float PossessAreaRight = 0.92f;

        // 日志区域 (0.34 - 0.49)
        public const float LogAreaTop = 0.49f;
        public const float LogAreaBottom = 0.34f;

        // 日志头区域
        public const float LogHeaderTop = 1.0f;
        public const float LogHeaderBottom = 0.92f;

        // 底部HUD (0.28 - 0.31)
        public const float BottomHudTop = 0.31f;
        public const float BottomHudBottom = 0.28f;

        // 技能栏 (0.31 - 0.34)
        public const float SkillBarTop = 0.34f;
        public const float SkillBarBottom = 0.31f;

        // 动作按钮区域 (0.02 - 0.28)
        public const float ActionAreaTop = 0.28f;
        public const float ActionAreaBottom = 0.02f;

        // 按钮位置
        public static class Buttons
        {
            // 攻击按钮 (左上)
            public const float AttackX = 0.17f;
            public const float AttackY = 0.54f;
            public const float AttackWidth = 0.15f;
            public const float AttackHeight = 0.36f;

            // 附身按钮 (中上)
            public const float PossessX = 0.485f;
            public const float PossessY = 0.54f;
            public const float PossessWidth = 0.15f;
            public const float PossessHeight = 0.36f;

            // 终极技按钮 (右侧，垂直居中)
            public const float UltimateX = 0.815f;
            public const float UltimateY = 0.18f;
            public const float UltimateWidth = 0.16f;
            public const float UltimateHeight = 0.72f;

            // 防御按钮 (左下)
            public const float DefendX = 0.17f;
            public const float DefendY = 0.10f;
            public const float DefendWidth = 0.15f;
            public const float DefendHeight = 0.36f;

            // 逃跑按钮 (中下)
            public const float FleeX = 0.485f;
            public const float FleeY = 0.10f;
            public const float FleeWidth = 0.15f;
            public const float FleeHeight = 0.36f;
        }

        // 卡片内元素位置
        public static class CardElements
        {
            // 卡片内边距
            public const float CardPaddingLeft = 0.04f;
            public const float CardPaddingRight = 0.04f;
            public const float CardPaddingTop = 0.04f;
            public const float CardPaddingBottom = 0.04f;

            // 图标位置
            public const float IconTop = 0.88f;
            public const float IconBottom = 0.50f;
            public const float IconLeft = 0.15f;
            public const float IconRight = 0.85f;

            // 名字位置
            public const float NameTop = 0.50f;
            public const float NameBottom = 0.38f;

            // HP条位置
            public const float HpBarTop = 0.38f;
            public const float HpBarBottom = 0.28f;
            public const float HpBarLeft = 0.08f;
            public const float HpBarRight = 0.92f;

            // 属性文本位置
            public const float StatsTop = 0.28f;
            public const float StatsBottom = 0.18f;

            // 类型标签位置
            public const float TypeTagsTop = 0.18f;
            public const float TypeTagsBottom = 0.12f;

            // 特性按钮位置
            public const float TraitBtnTop = 0.12f;
            public const float TraitBtnBottom = 0f;
            public const float TraitBtnViewWidth = 0.22f;
        }
    }

    /// <summary>
    /// 颜色主题常量
    /// </summary>
    public static class Colors
    {
        // 背景色
        public static readonly Color CombatBg = new Color(0.06f, 0.04f, 0.10f);
        public static readonly Color TopBarBg = new Color(0.10f, 0.06f, 0.16f, 0.95f);
        public static readonly Color PlayerCardBg = new Color(0.06f, 0.16f, 0.18f, 0.95f);
        public static readonly Color EnemyCardBg = new Color(0.14f, 0.05f, 0.10f, 0.95f);
        public static readonly Color LogBg = new Color(0.05f, 0.04f, 0.09f, 0.85f);
        public static readonly Color PossessBarBg = new Color(0f, 0.4f, 0.35f, 0.9f);
        public static readonly Color BottomHudBg = new Color(0.05f, 0.05f, 0.08f, 0.92f);

        // 强调色
        public static readonly Color AccentCyan = new Color(0f, 1f, 0.816f);
        public static readonly Color AccentGold = new Color(1f, 0.85f, 0.3f);
        public static readonly Color AccentPurple = new Color(0.65f, 0.25f, 0.95f);
        public static readonly Color AccentRed = new Color(0.95f, 0.25f, 0.45f);
        public static readonly Color AccentGreen = new Color(0f, 0.85f, 0.45f);

        // 特效颜色
        public static readonly Color PossessFill = new Color(0.7f, 0.3f, 1f);
        public static readonly Color PlayerHpFill = new Color(0f, 0.85f, 0.45f);
        public static readonly Color EnemyHpFill = new Color(0.9f, 0.15f, 0.15f);
    }

    /// <summary>
    /// 字体大小常量
    /// </summary>
    public static class FontSizes
    {
        public const int Tiny = 9;
        public const int Small = 10;
        public const int Normal = 12;
        public const int Medium = 14;
        public const int Large = 18;
        public const int XLarge = 22;
        public const int Title = 26;
    }
}
