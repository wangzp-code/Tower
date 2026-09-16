using UnityEngine;
using System;

public static class ModeFloorCurves
{
    [Serializable]
    public struct FloorParams
    {
        public float hpMult;
        public float atkMult;
        public int monsterCount;
        public float eliteRate;
        public float epMult;
        public float fragRate;
        public float pollutionPerFloor;
        public float possessPollMult;
    }

    // 原版 SHORT_FLOOR_CURVE 精确数值 (modes/short.js)
    public static readonly FloorParams[] ShortCurve = new FloorParams[]
    {
        new FloorParams{hpMult=0.55f,atkMult=0.35f,monsterCount=4, eliteRate=0f,   epMult=2.0f,fragRate=0.80f,pollutionPerFloor=0f, possessPollMult=0.3f},
        new FloorParams{hpMult=0.70f,atkMult=0.50f,monsterCount=5, eliteRate=0.05f,epMult=1.8f,fragRate=0.80f,pollutionPerFloor=2f, possessPollMult=0.4f},
        new FloorParams{hpMult=0.85f,atkMult=0.60f,monsterCount=5, eliteRate=0.10f,epMult=1.6f,fragRate=0.70f,pollutionPerFloor=3f, possessPollMult=0.6f},
        new FloorParams{hpMult=1.05f,atkMult=0.80f,monsterCount=6, eliteRate=0.15f,epMult=1.3f,fragRate=0.65f,pollutionPerFloor=4f, possessPollMult=1.0f},
        new FloorParams{hpMult=1.25f,atkMult=0.95f,monsterCount=7, eliteRate=0.22f,epMult=1.1f,fragRate=0.65f,pollutionPerFloor=2f, possessPollMult=1.0f},
        new FloorParams{hpMult=1.45f,atkMult=1.05f,monsterCount=8, eliteRate=0.25f,epMult=1.2f,fragRate=0.70f,pollutionPerFloor=3f, possessPollMult=1.2f},
        new FloorParams{hpMult=1.65f,atkMult=1.15f,monsterCount=8, eliteRate=0.30f,epMult=1.3f,fragRate=0.72f,pollutionPerFloor=4f, possessPollMult=1.3f},
        new FloorParams{hpMult=2.0f, atkMult=1.30f,monsterCount=7, eliteRate=0.25f,epMult=0.9f,fragRate=0.50f,pollutionPerFloor=5f, possessPollMult=1.2f},
        new FloorParams{hpMult=2.3f, atkMult=1.45f,monsterCount=9, eliteRate=0.35f,epMult=1.4f,fragRate=0.75f,pollutionPerFloor=5f, possessPollMult=1.6f},
        new FloorParams{hpMult=2.6f, atkMult=1.60f,monsterCount=10,eliteRate=0.40f,epMult=1.5f,fragRate=0.80f,pollutionPerFloor=6f, possessPollMult=1.8f},
        new FloorParams{hpMult=2.9f, atkMult=1.75f,monsterCount=10,eliteRate=0.45f,epMult=1.8f,fragRate=0.90f,pollutionPerFloor=7f, possessPollMult=2.2f},
        new FloorParams{hpMult=3.5f, atkMult=2.0f, monsterCount=7, eliteRate=0.55f,epMult=2.0f,fragRate=1.00f,pollutionPerFloor=10f,possessPollMult=2.5f}
    };

    // 原版 EXPEDITION_FLOOR_CURVE 精确数值 (modes/expedition.js)
    public static readonly FloorParams[] ExpeditionCurve = new FloorParams[]
    {
        // 章一：前厅与污染边缘 (F1-5)
        new FloorParams{hpMult=0.85f,atkMult=0.80f,monsterCount=4,eliteRate=0f,   epMult=1.5f,fragRate=0.75f,pollutionPerFloor=0f,possessPollMult=0.6f},
        new FloorParams{hpMult=0.90f,atkMult=0.85f,monsterCount=5,eliteRate=0f,   epMult=1.4f,fragRate=0.75f,pollutionPerFloor=0f,possessPollMult=0.6f},
        new FloorParams{hpMult=0.95f,atkMult=0.90f,monsterCount=5,eliteRate=0.08f,epMult=1.4f,fragRate=0.70f,pollutionPerFloor=0f,possessPollMult=0.7f},
        new FloorParams{hpMult=1.00f,atkMult=0.95f,monsterCount=6,eliteRate=0.10f,epMult=1.3f,fragRate=0.70f,pollutionPerFloor=0f,possessPollMult=0.8f},
        new FloorParams{hpMult=1.05f,atkMult=1.00f,monsterCount=4,eliteRate=0.12f,epMult=1.3f,fragRate=0.70f,pollutionPerFloor=0f,possessPollMult=0.8f},
        // 章二：裂变层区 (F6-10)
        new FloorParams{hpMult=1.10f,atkMult=1.05f,monsterCount=6,eliteRate=0.15f,epMult=1.3f,fragRate=0.70f,pollutionPerFloor=1f,possessPollMult=0.9f},
        new FloorParams{hpMult=1.15f,atkMult=1.08f,monsterCount=7,eliteRate=0.18f,epMult=1.2f,fragRate=0.65f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.18f,atkMult=1.10f,monsterCount=5,eliteRate=0.15f,epMult=1.0f,fragRate=0.60f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.22f,atkMult=1.13f,monsterCount=7,eliteRate=0.20f,epMult=1.3f,fragRate=0.70f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.28f,atkMult=1.18f,monsterCount=4,eliteRate=0.22f,epMult=1.2f,fragRate=0.70f,pollutionPerFloor=1f,possessPollMult=1.1f},
        // 章三：深层脉络 (F11-15)
        new FloorParams{hpMult=1.32f,atkMult=1.20f,monsterCount=7,eliteRate=0.22f,epMult=1.3f,fragRate=0.70f,pollutionPerFloor=2f,possessPollMult=1.1f},
        new FloorParams{hpMult=1.36f,atkMult=1.23f,monsterCount=8,eliteRate=0.25f,epMult=1.2f,fragRate=0.65f,pollutionPerFloor=2f,possessPollMult=1.2f},
        new FloorParams{hpMult=1.40f,atkMult=1.25f,monsterCount=8,eliteRate=0.28f,epMult=1.5f,fragRate=0.75f,pollutionPerFloor=2f,possessPollMult=1.2f},
        new FloorParams{hpMult=1.44f,atkMult=1.28f,monsterCount=8,eliteRate=0.28f,epMult=1.3f,fragRate=0.70f,pollutionPerFloor=3f,possessPollMult=1.3f},
        new FloorParams{hpMult=1.48f,atkMult=1.30f,monsterCount=5,eliteRate=0.30f,epMult=1.3f,fragRate=0.70f,pollutionPerFloor=3f,possessPollMult=1.3f},
        // 终章：终域回响 (F16-20)
        new FloorParams{hpMult=1.52f,atkMult=1.33f,monsterCount=8,eliteRate=0.30f,epMult=1.4f,fragRate=0.75f,pollutionPerFloor=3f,possessPollMult=1.4f},
        new FloorParams{hpMult=1.56f,atkMult=1.35f,monsterCount=9,eliteRate=0.32f,epMult=1.4f,fragRate=0.75f,pollutionPerFloor=3f,possessPollMult=1.5f},
        new FloorParams{hpMult=1.58f,atkMult=1.35f,monsterCount=5,eliteRate=0.25f,epMult=1.2f,fragRate=0.70f,pollutionPerFloor=3f,possessPollMult=1.5f},
        new FloorParams{hpMult=1.62f,atkMult=1.38f,monsterCount=9,eliteRate=0.35f,epMult=1.5f,fragRate=0.80f,pollutionPerFloor=4f,possessPollMult=1.6f},
        new FloorParams{hpMult=1.70f,atkMult=1.42f,monsterCount=5,eliteRate=0.38f,epMult=1.8f,fragRate=0.90f,pollutionPerFloor=4f,possessPollMult=1.8f}
    };

    // 原版 FULL_FLOOR_CURVE 精确数值 (core/game-data.js) — 经典50层
    public static readonly FloorParams[] ClassicCurve = new FloorParams[]
    {
        // S1 适应期 (F1-F8)
        new FloorParams{hpMult=1.00f,atkMult=1.00f,monsterCount=5, eliteRate=0f,   epMult=1.3f,fragRate=0.70f,pollutionPerFloor=0f,possessPollMult=0.7f},
        new FloorParams{hpMult=1.00f,atkMult=1.00f,monsterCount=5, eliteRate=0f,   epMult=1.3f,fragRate=0.70f,pollutionPerFloor=0f,possessPollMult=0.7f},
        new FloorParams{hpMult=1.00f,atkMult=1.00f,monsterCount=5, eliteRate=0.05f,epMult=1.2f,fragRate=0.65f,pollutionPerFloor=0f,possessPollMult=0.7f},
        new FloorParams{hpMult=1.00f,atkMult=1.00f,monsterCount=5, eliteRate=0.05f,epMult=1.2f,fragRate=0.65f,pollutionPerFloor=0f,possessPollMult=0.8f},
        new FloorParams{hpMult=1.00f,atkMult=1.00f,monsterCount=6, eliteRate=0.10f,epMult=1.2f,fragRate=0.65f,pollutionPerFloor=0f,possessPollMult=0.8f},
        new FloorParams{hpMult=1.02f,atkMult=1.01f,monsterCount=6, eliteRate=0.10f,epMult=1.1f,fragRate=0.65f,pollutionPerFloor=0f,possessPollMult=0.8f},
        new FloorParams{hpMult=1.04f,atkMult=1.02f,monsterCount=6, eliteRate=0.12f,epMult=1.1f,fragRate=0.60f,pollutionPerFloor=0f,possessPollMult=0.9f},
        new FloorParams{hpMult=1.06f,atkMult=1.03f,monsterCount=6, eliteRate=0.12f,epMult=1.1f,fragRate=0.60f,pollutionPerFloor=0f,possessPollMult=0.9f},
        // F9-F10
        new FloorParams{hpMult=1.08f,atkMult=1.04f,monsterCount=7, eliteRate=0.15f,epMult=1.1f,fragRate=0.60f,pollutionPerFloor=0f,possessPollMult=0.9f},
        new FloorParams{hpMult=1.10f,atkMult=1.05f,monsterCount=5, eliteRate=0.15f,epMult=1.0f,fragRate=0.60f,pollutionPerFloor=0f,possessPollMult=1.0f},
        // S2 稳定成长 (F11-F16)
        new FloorParams{hpMult=1.12f,atkMult=1.06f,monsterCount=7, eliteRate=0.18f,epMult=1.0f,fragRate=0.60f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.14f,atkMult=1.07f,monsterCount=7, eliteRate=0.18f,epMult=1.0f,fragRate=0.60f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.16f,atkMult=1.08f,monsterCount=7, eliteRate=0.20f,epMult=1.0f,fragRate=0.60f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.18f,atkMult=1.09f,monsterCount=7, eliteRate=0.20f,epMult=1.1f,fragRate=0.60f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.20f,atkMult=1.10f,monsterCount=8, eliteRate=0.22f,epMult=1.1f,fragRate=0.60f,pollutionPerFloor=1f,possessPollMult=1.0f},
        new FloorParams{hpMult=1.22f,atkMult=1.11f,monsterCount=8, eliteRate=0.22f,epMult=1.1f,fragRate=0.60f,pollutionPerFloor=1f,possessPollMult=1.0f},
        // S3 污染成为主角 (F17-F24)
        new FloorParams{hpMult=1.24f,atkMult=1.12f,monsterCount=8, eliteRate=0.22f,epMult=1.0f,fragRate=0.55f,pollutionPerFloor=2f,possessPollMult=1.1f},
        new FloorParams{hpMult=1.26f,atkMult=1.13f,monsterCount=8, eliteRate=0.24f,epMult=1.0f,fragRate=0.55f,pollutionPerFloor=2f,possessPollMult=1.1f},
        new FloorParams{hpMult=1.28f,atkMult=1.14f,monsterCount=9, eliteRate=0.24f,epMult=1.0f,fragRate=0.55f,pollutionPerFloor=2f,possessPollMult=1.2f},
        new FloorParams{hpMult=1.30f,atkMult=1.15f,monsterCount=5, eliteRate=0.24f,epMult=1.0f,fragRate=0.55f,pollutionPerFloor=2f,possessPollMult=1.2f},
        new FloorParams{hpMult=1.32f,atkMult=1.16f,monsterCount=9, eliteRate=0.25f,epMult=1.1f,fragRate=0.55f,pollutionPerFloor=2f,possessPollMult=1.2f},
        new FloorParams{hpMult=1.34f,atkMult=1.17f,monsterCount=9, eliteRate=0.25f,epMult=1.1f,fragRate=0.55f,pollutionPerFloor=3f,possessPollMult=1.2f},
        new FloorParams{hpMult=1.36f,atkMult=1.18f,monsterCount=9, eliteRate=0.28f,epMult=1.2f,fragRate=0.60f,pollutionPerFloor=3f,possessPollMult=1.3f},
        new FloorParams{hpMult=1.38f,atkMult=1.19f,monsterCount=9, eliteRate=0.28f,epMult=1.2f,fragRate=0.60f,pollutionPerFloor=3f,possessPollMult=1.3f},
        // S4 系统反转 (F25-F32)
        new FloorParams{hpMult=1.40f,atkMult=1.20f,monsterCount=10,eliteRate=0.28f,epMult=1.2f,fragRate=0.60f,pollutionPerFloor=3f,possessPollMult=1.3f},
        new FloorParams{hpMult=1.42f,atkMult=1.21f,monsterCount=10,eliteRate=0.30f,epMult=1.2f,fragRate=0.60f,pollutionPerFloor=3f,possessPollMult=1.3f},
        new FloorParams{hpMult=1.44f,atkMult=1.22f,monsterCount=10,eliteRate=0.30f,epMult=1.3f,fragRate=0.65f,pollutionPerFloor=3f,possessPollMult=1.4f},
        new FloorParams{hpMult=1.46f,atkMult=1.23f,monsterCount=10,eliteRate=0.30f,epMult=1.3f,fragRate=0.65f,pollutionPerFloor=4f,possessPollMult=1.4f},
        new FloorParams{hpMult=1.48f,atkMult=1.24f,monsterCount=11,eliteRate=0.32f,epMult=1.3f,fragRate=0.65f,pollutionPerFloor=4f,possessPollMult=1.4f},
        new FloorParams{hpMult=1.50f,atkMult=1.25f,monsterCount=5, eliteRate=0.32f,epMult=1.2f,fragRate=0.65f,pollutionPerFloor=4f,possessPollMult=1.5f},
        // S5 高压验证 (F31-F40)
        new FloorParams{hpMult=1.52f,atkMult=1.26f,monsterCount=11,eliteRate=0.32f,epMult=1.3f,fragRate=0.65f,pollutionPerFloor=4f,possessPollMult=1.5f},
        new FloorParams{hpMult=1.54f,atkMult=1.27f,monsterCount=11,eliteRate=0.34f,epMult=1.3f,fragRate=0.65f,pollutionPerFloor=4f,possessPollMult=1.5f},
        new FloorParams{hpMult=1.56f,atkMult=1.28f,monsterCount=11,eliteRate=0.34f,epMult=1.4f,fragRate=0.70f,pollutionPerFloor=4f,possessPollMult=1.5f},
        new FloorParams{hpMult=1.58f,atkMult=1.29f,monsterCount=11,eliteRate=0.34f,epMult=1.4f,fragRate=0.70f,pollutionPerFloor=4f,possessPollMult=1.6f},
        new FloorParams{hpMult=1.60f,atkMult=1.30f,monsterCount=12,eliteRate=0.36f,epMult=1.4f,fragRate=0.70f,pollutionPerFloor=5f,possessPollMult=1.6f},
        new FloorParams{hpMult=1.62f,atkMult=1.31f,monsterCount=12,eliteRate=0.36f,epMult=1.4f,fragRate=0.70f,pollutionPerFloor=5f,possessPollMult=1.6f},
        new FloorParams{hpMult=1.64f,atkMult=1.32f,monsterCount=12,eliteRate=0.38f,epMult=1.5f,fragRate=0.75f,pollutionPerFloor=5f,possessPollMult=1.7f},
        new FloorParams{hpMult=1.66f,atkMult=1.33f,monsterCount=12,eliteRate=0.38f,epMult=1.5f,fragRate=0.75f,pollutionPerFloor=5f,possessPollMult=1.7f},
        new FloorParams{hpMult=1.68f,atkMult=1.34f,monsterCount=13,eliteRate=0.40f,epMult=1.5f,fragRate=0.75f,pollutionPerFloor=5f,possessPollMult=1.8f},
        new FloorParams{hpMult=1.70f,atkMult=1.35f,monsterCount=5, eliteRate=0.40f,epMult=1.4f,fragRate=0.75f,pollutionPerFloor=5f,possessPollMult=1.8f},
        // S6 持续高潮 (F41-F50)
        new FloorParams{hpMult=1.72f,atkMult=1.36f,monsterCount=13,eliteRate=0.40f,epMult=1.5f,fragRate=0.75f,pollutionPerFloor=6f,possessPollMult=1.8f},
        new FloorParams{hpMult=1.74f,atkMult=1.37f,monsterCount=13,eliteRate=0.42f,epMult=1.6f,fragRate=0.80f,pollutionPerFloor=6f,possessPollMult=1.9f},
        new FloorParams{hpMult=1.76f,atkMult=1.38f,monsterCount=13,eliteRate=0.42f,epMult=1.6f,fragRate=0.80f,pollutionPerFloor=6f,possessPollMult=1.9f},
        new FloorParams{hpMult=1.78f,atkMult=1.39f,monsterCount=14,eliteRate=0.44f,epMult=1.7f,fragRate=0.80f,pollutionPerFloor=7f,possessPollMult=2.0f},
        new FloorParams{hpMult=1.80f,atkMult=1.40f,monsterCount=14,eliteRate=0.44f,epMult=1.7f,fragRate=0.85f,pollutionPerFloor=7f,possessPollMult=2.0f},
        new FloorParams{hpMult=1.82f,atkMult=1.41f,monsterCount=14,eliteRate=0.46f,epMult=1.8f,fragRate=0.85f,pollutionPerFloor=7f,possessPollMult=2.0f},
        new FloorParams{hpMult=1.84f,atkMult=1.42f,monsterCount=14,eliteRate=0.46f,epMult=1.8f,fragRate=0.90f,pollutionPerFloor=8f,possessPollMult=2.0f},
        new FloorParams{hpMult=1.86f,atkMult=1.43f,monsterCount=15,eliteRate=0.48f,epMult=2.0f,fragRate=0.90f,pollutionPerFloor=8f,possessPollMult=2.0f},
        new FloorParams{hpMult=1.88f,atkMult=1.44f,monsterCount=15,eliteRate=0.50f,epMult=2.0f,fragRate=1.00f,pollutionPerFloor=8f,possessPollMult=2.0f},
        new FloorParams{hpMult=1.90f,atkMult=1.45f,monsterCount=5, eliteRate=0.50f,epMult=2.0f,fragRate=1.00f,pollutionPerFloor=8f,possessPollMult=2.0f}
    };

    public static FloorParams GetParams(GameMode mode, int floor, int stage = 1)
    {
        FloorParams[] curve;
        switch (mode)
        {
            case GameMode.Short:
                curve = ShortCurve;
                break;
            case GameMode.Expedition:
                curve = ExpeditionCurve;
                break;
            default:
                curve = ClassicCurve;
                break;
        }

        int index = Mathf.Clamp(floor - 1, 0, curve.Length - 1);
        var p = curve[index];

        if (mode == GameMode.Expedition && stage > 1)
        {
            float stageMult = 1f + (stage - 1) * 0.12f;
            p.hpMult *= stageMult;
            p.atkMult *= stageMult;
            p.eliteRate = Mathf.Min(p.eliteRate + (stage - 1) * 0.03f, 0.6f);
            p.pollutionPerFloor += (stage - 1) * 0.5f;
        }

        if (mode == GameMode.Daily || mode == GameMode.Weekly)
        {
            var mod = CompleteGameSystem.Instance?.ActiveChallengeModifier;
            if (mod != null)
            {
                if (mod.monsterHpMult > 0f) p.hpMult *= mod.monsterHpMult;
                if (mod.monsterAtkMult > 0f) p.atkMult *= mod.monsterAtkMult;
                if (mod.epMult > 0f) p.epMult *= mod.epMult;
                if (mod.eliteRateAdd > 0f) p.eliteRate = Mathf.Min(p.eliteRate + mod.eliteRateAdd, 0.8f);
                if (mod.pollutionMult > 0f) p.pollutionPerFloor *= mod.pollutionMult;
            }
        }

        return p;
    }

    public static string GetWaveTransitionText(GameMode mode, int floor)
    {
        if (mode == GameMode.Short)
        {
            switch (floor)
            {
                case 1: return "教学热身 — 熟悉操作";
                case 2: return "探索阶段 — 收集资源";
                case 3: return "首个选择点 — 宿主抉择";
                case 4: return "难度爬升 — 精英出现";
                case 5: return "挑战加剧 — 组合威胁";
                case 6: return "中场休息 — 补给与遗产";
                case 7: return "核心挑战 — 环境效果";
                case 8: return "压力测试 — 组合怪群";
                case 9: return "抉择点 — 高风险高回报";
                case 10: return "Boss前奏 — 资源紧张";
                case 11: return "难度峰值 — 最后准备";
                case 12: return "零号容器 — 最终Boss";
            }
        }
        else if (mode == GameMode.Expedition)
        {
            switch (floor)
            {
                case 1: return "章一: 前厅与污染边缘";
                case 6: return "章二: 裂变层区";
                case 11: return "章三: 深层脉络";
                case 16: return "终章: 终域回响";
            }
        }
        else
        {
            if (floor == 1) return "S1 适应期";
            if (floor == 11) return "S2 稳定成长 — 首次污染";
            if (floor == 17) return "S3 污染成为主角";
            if (floor == 25) return "S4 系统反转 — 净化祭坛";
            if (floor == 33) return "S5 高压验证";
            if (floor == 41) return "S6 持续高潮 — 崩坏冲顶";
            if (floor == 49) return "终章 审判与结局";
        }
        return null;
    }

    // Zone mapping by mode
    public static int GetZone(GameMode mode, int floor)
    {
        switch (mode)
        {
            case GameMode.Short:
                if (floor <= 3) return 1;
                if (floor <= 6) return 2;
                if (floor <= 9) return 3;
                if (floor <= 11) return 4;
                return 5;
            case GameMode.Expedition:
                return Mathf.Clamp((floor - 1) / 5 + 1, 1, 4);
            default:
                return Mathf.Clamp((floor - 1) / 10 + 1, 1, 5);
        }
    }

    // Boss floor check by mode
    public static bool IsBossFloor(GameMode mode, int floor, int maxFloor)
    {
        switch (mode)
        {
            case GameMode.Short:
                return floor >= maxFloor;
            case GameMode.Expedition:
                return floor % 5 == 0;
            default:
                return floor % 10 == 0;
        }
    }

    public enum FloorType { Normal, Event, Elite, Rest, Boss }

    public static FloorType GetShortModeFloorType(int floor)
    {
        switch (floor)
        {
            case 1: return FloorType.Normal;
            case 2: return FloorType.Normal;
            case 3: return FloorType.Event;
            case 4: return FloorType.Elite;
            case 5: return FloorType.Elite;
            case 6: return FloorType.Rest;
            case 7: return FloorType.Normal;
            case 8: return FloorType.Normal;
            case 9: return FloorType.Event;
            case 10: return FloorType.Elite;
            case 11: return FloorType.Elite;
            case 12: return FloorType.Boss;
            default: return FloorType.Normal;
        }
    }
}
