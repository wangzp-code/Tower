
# 遗产系统设计文档

## 1. 系统概述

### 1.1 设计目的
遗产系统是局内成长机制，当玩家替换形态时，被替换的怪物会留下一个技能/能力作为"遗产"，增强玩家的战斗能力。

### 1.2 核心概念

| 概念 | 说明 |
|------|------|
| 遗产能力 | 从被替换形态提取的被动能力 |
| 遗产槽 | 存储已装备遗产的槽位，最多3个 |
| 效果类型 | 遗产能力的效果分类（攻击、防御、生命等） |
| 遗产提取 | 替换形态时自动提取遗产能力 |

### 1.3 设计原则
- **局内机制**：遗产仅在当前局有效，新局开始时清空
- **策略选择**：玩家可选择保留哪个遗产能力
- **来源关联**：遗产效果来源于被替换怪物的实际属性/特性

---

## 2. 系统架构

### 2.1 数据结构

**遗产能力** - [LegacyAbility.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/Legacy/LegacyAbility.cs)
```csharp
{
    id: string,           // 遗产唯一ID
    name: string,         // 遗产名称
    description: string,  // 遗产描述
    effectType: string,   // 效果类型
    effectValue: float,   // 效果数值
    sourceMonsterId: string // 来源怪物ID
}
```

**效果类型对照表**

| 效果类型 | 说明 | 应用属性 |
|---------|------|---------|
| AttackBonus | 攻击加成 | attackBonus |
| DefenseBonus | 防御加成 | defenseBonus |
| MaxHpBonus | 最大生命加成 | maxHp |
| regen | 每回合恢复 | regenPerTurn |
| poison_attack | 中毒概率 | poisonChance |
| lifesteal | 吸血率 | lifesteal |
| crit_rate | 暴击率 | critRate |

### 2.2 核心组件

| 组件 | 职责 | 文件路径 |
|------|------|---------|
| LegacyManager | 遗产管理核心类 | Assets/Scripts/Runtime/Legacy/LegacyManager.cs |
| TraitManager | 特质数据管理 | Assets/Scripts/Runtime/Traits/TraitManager.cs |
| GameDataImporter | 怪物数据获取 | Assets/Scripts/Runtime/Data/GameDataImporter.cs |

---

## 3. 功能设计

### 3.1 遗产提取

**触发条件：**
- 玩家替换形态时自动触发

**提取规则：**
- 从被替换的怪物形态中提取遗产
- 根据怪物属性生成对应的遗产能力

**提取逻辑** - [LegacyManager.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/Legacy/LegacyManager.cs#L80-147)

```
1. 获取被替换形态的怪物数据
2. 根据攻击属性判断是否生成攻击传承
3. 根据防御属性判断是否生成防御传承
4. 根据生命属性判断是否生成生命传承
5. 遍历怪物特质，生成特质遗产
6. 返回所有可选择的遗产列表
```

### 3.2 遗产选择

**选择流程：**
```
替换形态 → 提取遗产 → 显示遗产选择面板 → 玩家选择一个遗产 → 
装备遗产 → 应用效果
```

**UI面板设计：**
```
┌─────────────────────────────┐
│    获得遗产！               │
│                             │
│  选择要保留的遗产:          │
│                             │
│  ┌─────────────────────┐   │
│  │  ⚔ 攻击传承         │   │
│  │  攻击+5             │   │
│  └─────────────────────┘   │
│                             │
│  ┌─────────────────────┐   │
│  │  🛡 防御传承         │   │
│  │  防御+3             │   │
│  └─────────────────────┘   │
│                             │
│  [选择]      [放弃]         │
└─────────────────────────────┘
```

### 3.3 遗产装备

**装备规则：**
- 最多装备3个遗产
- 相同效果类型可叠加
- 装备后立即应用效果

**装备流程：**
```
选择遗产 → 检查槽位是否已满 → 未满：直接装备 → 
已满：提示替换或放弃 → 应用效果到玩家属性
```

### 3.4 遗产效果应用

**应用时机：**
- 装备遗产时
- 新局开始时（清空所有遗产）
- 移除遗产时（反向应用效果）

**效果应用示例：**
```csharp
// 攻击加成
player.attackBonus += ability.effectValue;

// 最大生命加成
player.maxHp += Mathf.CeilToInt(ability.effectValue);
player.hp += Mathf.CeilToInt(ability.effectValue);

// 防御加成
player.defenseBonus += ability.effectValue;
```

---

## 4. 事件系统

| 事件名称 | 触发时机 | 参数 |
|---------|---------|------|
| LegacySelection | 提取遗产后 | abilities: List<LegacyAbility>, sourceFormId: string |
| LegacyAdded | 遗产装备成功 | ability: LegacyAbility |
| LegacyReplaced | 遗产替换成功 | index: int, newAbility: LegacyAbility |
| LegacyRemoved | 遗产移除成功 | index: int |

---

## 5. 关键代码引用

**遗产提取逻辑** - [LegacyManager.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/Legacy/LegacyManager.cs#L80-147)
```csharp
public List<LegacyAbility> ExtractLegaciesFromForm(string formId)
{
    // 从怪物数据中提取攻击、防御、生命、特质遗产
}
```

**效果应用逻辑** - [LegacyManager.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/Legacy/LegacyManager.cs#L203-236)
```csharp
void ApplyLegacyEffect(LegacyAbility ability)
{
    // 根据effectType应用对应效果
}
```

---

## 6. 与其他系统的交互

### 6.1 与形态系统
- 形态替换触发遗产提取
- 遗产来源与被替换形态直接相关

### 6.2 与战斗系统
- 遗产效果直接影响战斗属性
- 战斗中实时生效

### 6.3 与玩家系统
- 遗产效果存储在 PlayerData 中
- 新局开始时清空所有遗产

---

## 7. 设计决策

### 7.1 局内机制设计
- **理由**：避免跨局成长导致游戏难度失衡
- **实现**：RunStart事件触发时清空所有遗产

### 7.2 选择机制设计
- **理由**：增加玩家决策深度
- **实现**：提取多个遗产供玩家选择

### 7.3 槽位限制设计
- **理由**：限制玩家强度，增加策略取舍
- **当前设定**：最多3个遗产槽

### 7.4 数值平衡
- 遗产效果基于怪物基础属性的百分比（10%-15%）
- 特质遗产效果与原特质保持一致
