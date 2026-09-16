# 记忆锚点系统设计文档

## 1. 系统概述

记忆锚点是死亡存续机制，允许玩家在死亡时消耗EP回滚到之前激活的锚点层，而非直接结束游戏。

来源：`AnchorSystem.cs` + `CompleteGameSystem.cs` DeathRollback

---

## 2. 锚点管理

### 2.1 激活锚点

`ActivateAnchor(int floor)`
- 在指定楼层激活一个锚点
- 最多保留 3 个锚点（MAX_ANCHORS = 3），超出时移除最旧的
- 激活代价：200 EP（TryActivateAltar）

### 2.2 锚点命名

按楼层自动命名：

| 层数范围 | 名称 |
|---------|------|
| 1-10 | 浅层锚点 |
| 11-25 | 中层锚点 |
| 26-40 | 深层锚点 |
| 40+ | 深渊锚点 |

### 2.3 锚点数据

```
AnchorData {
  floor         — 锚点所在楼层
  name          — 显示名称
  activated     — 是否已激活
  used          — 是否已使用
  activatedAt   — 激活时间
}
```

---

## 3. 死亡回滚

### 3.1 触发流程

来源：`CompleteGameSystem.cs` HandleDeath

```
玩家HP≤0 → 检查是否有锚点 + 足够EP
  → 有：消耗EP，调用 UseAnchor → ReturnToAnchor(floor, consecutiveDeaths)
  → 无：正常死亡 → GenerateRunReport → GameOver
```

### 3.2 回滚代价

`DeathRollbackEpCost` — EP消耗量（随连续死亡递增）

### 3.3 回滚效果

- 回到锚点层，重新生成该层
- 保留当前形态、遗产、污染值
- 锚点标记为已使用（used=true）
- 日志提示：`⛓ 记忆锚定已激活 -{cost}EP`

### 3.4 EP不足

如果EP不够，显示错误提示并正常死亡：
`EP不足（{current}/{required}），回滚失败`

---

## 4. 锚点UI

### 4.1 游戏内菜单

菜单中"锚点管理"入口可查看已激活锚点列表。

### 4.2 死亡时

死亡画面显示回滚选项（如果有可用锚点且EP足够）。

---

## 5. 查询API

| 方法 | 返回 | 说明 |
|------|------|------|
| HasActiveAnchor() | bool | 是否有可用锚点 |
| GetCurrentAnchorFloor() | int | 当前锚点楼层（0=无） |
| GetAllAnchors() | List | 所有锚点列表 |
| ClearAllAnchors() | void | 清除全部锚点 |

---

**文档版本**: v1.0
**所属模块**: Anchor
**最后更新**: 2026-06-23
