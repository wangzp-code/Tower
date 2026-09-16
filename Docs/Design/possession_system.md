# 寄生（附身）系统设计文档

## 1. 系统概述

寄生/附身是游戏核心机制，玩家通过附身敌人获取新形态，触发遗产提取和共鸣系统。

来源：`CompleteGameSystem.cs` TryPossess / FormManager.cs

---

## 2. 附身触发

### 2.1 破防窗口

来源：PlayerAttack 中 breakWindow 逻辑

敌人 HP 首次低于 50% 时触发破防窗口，显示附身按钮。

### 2.2 附身成功率

```
基础概率 = possessBaseChance (通常 0.6 = 60%)
+ possessionBonus (玩家加成，来自祭坛/签名)
+ 楼层签名加成 (如"寄生乐园"+20%, "黑暗降临"+20%)
```

### 2.3 附身结果

**成功：**
1. 获得敌人形态，加入 `ownedForms`
2. 触发 `EventTypes.FormReplaced`
3. LegacyManager 从旧形态提取遗产候选
4. 弹出遗产选择面板
5. 增加污染（按曲线 `possessPollMult`）
6. 统计 `possessionCountThisRun++`

**失败：**
- 受到反伤

---

## 3. 形态管理

详见 [form_system.md](form_system.md)

- 形态列表：`player.ownedForms`
- 当前形态：`player.currentFormId`
- 形态切换触发共鸣：[resonance_system.md](resonance_system.md)

---

## 4. 死亡存续

### 4.1 记忆锚点回滚

详见 [anchor_system.md](anchor_system.md)

死亡时如果有锚点+EP，可消耗EP回滚到锚点层。

### 4.2 正常死亡

无锚点/EP不足时：
1. 触发 `GenerateRunReport(false, cause)`
2. 进入 GameOver 画面
3. 显示结算报告（含构筑回顾）

---

## 5. 短局特殊机制

### 5.1 第2层保底强敌

短局F2保底一个zone2强敌（★标记，possessBaseChance=0.75），作为明确的附身目标。

### 5.2 第3层免费遗产

短局F3如果玩家还没有遗产，自动触发遗产选择事件（从当前形态提取），附赠5%污染。

---

## 6. 相关系统

| 系统 | 关联 |
|------|------|
| [遗产系统](legacy_system.md) | 附身后从旧形态提取遗产 |
| [共鸣系统](resonance_system.md) | 形态切换触发共鸣效果 |
| [污染系统](pollution_system.md) | 附身增加污染 |
| [锚点系统](anchor_system.md) | 死亡回滚替代方案 |

---

**文档版本**: v2.0
**所属模块**: Possession
**最后更新**: 2026-06-23
