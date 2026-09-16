# 设计文档目录

## 概述

《寄生塔》（对外名称：你也是我 / You Are Me）游戏设计文档中心。

---

## 文档列表

### 系统导读（编号文档）

| 文档 | 描述 |
|------|------|
| [00_项目总览](00_项目总览.md) | 项目架构概述 |
| [01_核心框架](01_核心框架.md) | 状态管理、存档、事件、配置、管理器清单 |
| [02_战斗系统](02_战斗系统.md) | 20步伤害流水线、伤害标签、soft cap |
| [03_玩家系统](03_玩家系统.md) | 5职业、进化、属性、跨局进度 |
| [04_怪物系统](04_怪物系统.md) | 怪物AI、特质、Boss战 |
| [05_地图系统](05_地图系统.md) | 3模式楼层、签名、事件、远征抉择 |
| [06_商店系统](06_商店系统.md) | 残响商店、物品购买 |
| [07_成就系统](07_成就系统.md) | 成就解锁、奖励 |
| [08_UI界面](08_UI界面.md) | 界面布局 |
| [09_数据配置](09_数据配置.md) | JSON配置结构 |
| [10_功能模块总览](10_功能模块总览.md) | 全模块索引与玩法循环 |

### 专题文档（深度参考）

| 文档 | 描述 |
|------|------|
| [game_overview](game_overview.md) | 系统架构总览 |
| [form_system](form_system.md) | 形态管理、切换、槽位 |
| [possession_system](possession_system.md) | 附身机制、破防窗口 |
| [legacy_system](legacy_system.md) | 遗产提取、装备、腐蚀变异 |
| [pollution_system](pollution_system.md) | 5阶段污染、被动效果、ATK/DEF倍率 |
| [resonance_system](resonance_system.md) | 共鸣组合、熟练度、持续时间计算 |
| [anchor_system](anchor_system.md) | 记忆锚点、死亡回滚 |
| [mode_curves](mode_curves.md) | Short/Expedition/Classic 难度曲线 |
| [shop_system](shop_system.md) | 商品、礼包、限购 |
| [daily_login_system](daily_login_system.md) | 每日签到奖励 |

---

## 系统架构

```
CompleteGameSystem (核心玩法引擎)
├── 战斗 (PlayerAttack → 20步伤害链 → 伤害标签 → soft cap)
├── 探索 (SeedFloorActions → 签名 → 事件 → 祭坛)
├── 附身 (TryPossess → 形态获取 → 遗产提取 → 共鸣触发)
└── 结算 (GenerateRunReport → 构筑回顾)

构筑子系统
├── PollutionSystem + PollutionPassiveSystem (污染5阶段)
├── FormResonanceSystem (共鸣 + 熟练度)
├── LegacyManager (遗产 + 腐蚀)
└── AnchorSystem (死亡回滚)

数据层
├── ModeFloorCurves (难度曲线)
├── GameDataImporter (怪物定义)
├── FloorSignatureData (21种签名)
└── AltarData (13+2种祭坛选项)
```

---

## 设计原则

1. **策略深度**：每个决策都有权衡和后果
2. **风险收益**：高污染=高攻击+低防御
3. **4层见核心**：短局前4层体验全部核心系统
4. **跨局进度**：成就/结局/图鉴永久保留

---

## 版本记录

| 版本 | 日期 | 更新内容 |
|------|------|---------|
| v1.0 | 2026-06 | 初始文档创建 |
| v1.1 | 2026-06 | 更新修复后的系统设计 |
| v2.0 | 2026-06-23 | 全面更新：新增共鸣/锚点/曲线文档，修正过时内容，添加伤害标签/soft cap/远征抉择/短局节奏压缩等 |
