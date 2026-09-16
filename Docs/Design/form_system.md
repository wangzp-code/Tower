
# 形态系统设计文档

## 1. 系统概述

### 1.1 设计目的
形态系统是游戏的核心玩法之一，允许玩家通过附身怪物获取不同的战斗形态，每个形态拥有独特的属性和技能。

### 1.2 核心概念

| 概念 | 说明 |
|------|------|
| 形态 | 玩家可切换的战斗形态，每个形态有独特的HP、攻击、防御属性 |
| 形态槽 | 存储已获取形态的槽位，最多3个 |
| 本命职业 | 玩家选择的初始职业，无法替换，始终占据槽位0 |
| 形态切换 | 在战斗中切换不同形态应对不同敌人 |

---

## 2. 系统架构

### 2.1 数据结构

**形态数据（来自 GameDataImporter.MonsterDefinitions）**
```
{
    id: string,           // 形态ID
    name: string,         // 形态名称
    hp: int,              // 基础生命值
    atk: int,             // 基础攻击力
    def: int,             // 基础防御力
    zone: int,            // 出现楼层
    boss: bool,           // 是否BOSS形态
    traits: string[],     // 特质列表
    axes: string[]        // 攻击轴
}
```

**玩家形态状态**
```
{
    ownedForms: string[],    // 已拥有的形态ID列表
    currentFormId: string,   // 当前使用的形态
    formSlots: int           // 形态槽数量（默认3）
}
```

### 2.2 核心组件

| 组件 | 职责 | 文件路径 |
|------|------|---------|
| FormManager | 形态管理核心类 | Assets/Scripts/Runtime/Forms/FormManager.cs |
| CanvasUIManager.FormReplacement | 形态替换UI | Assets/Scripts/Runtime/UI/CanvasUIManager.FormReplacement.cs |

---

## 3. 功能设计

### 3.1 形态获取

**触发条件：**
- 战斗中成功附身怪物
- 探索事件奖励
- 商店购买

**获取流程：**
```
检测形态槽是否已满 → 未满：直接添加 → 已满：触发形态替换流程
```

### 3.2 形态替换

**限制规则：**
- 本命职业（槽位0）**无法替换**
- 只有非本命槽位可以被替换

**替换流程：**
```
形态槽已满 → 弹出替换面板 → 选择要替换的槽位 → 确认替换 → 
触发遗产提取 → 应用新形态
```

### 3.3 形态切换

**切换规则：**
- 战斗中可随时切换
- 切换有冷却时间（3秒）
- 切换后立即应用新形态属性

**切换流程：**
```
选择目标形态 → 检查冷却 → 切换形态 → 应用属性 → 通知UI更新
```

### 3.4 形态属性

每个形态拥有以下属性：
- **生命值 (HP)**: 形态的血量
- **攻击力 (ATK)**: 基础攻击伤害
- **防御力 (DEF)**: 减少受到的伤害
- **特质 (Traits)**: 特殊被动效果

---

## 4. UI设计

### 4.1 形态替换面板

```
┌─────────────────────────────┐
│    形态槽已满！             │
│                             │
│  当前形态: [A] [B] [C]      │
│                             │
│  选择要替换的形态:          │
│  ┌─────┐  ┌─────┐  ┌─────┐ │
│  │  A* │  │  B  │  │  C  │ │  (*表示本命)
│  └─────┘  └─────┘  └─────┘ │
│     锁定    可替换   可替换  │
│                             │
│     [取消]                  │
└─────────────────────────────┘
```

### 4.2 形态切换快捷栏

- 显示当前拥有的所有形态
- 显示形态头像和HP条
- 点击即可切换（受冷却限制）

---

## 5. 事件系统

| 事件名称 | 触发时机 | 参数 |
|---------|---------|------|
| FormAdded | 形态添加成功 | formId: string |
| FormSlotFull | 形态槽已满 | newFormId: string |
| FormReplaced | 形态替换成功 | oldFormId: string |
| FormSwitched | 形态切换成功 | formId: string |

---

## 6. 关键代码引用

**形态替换限制** - [FormManager.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/Forms/FormManager.cs#L88-96)
```csharp
public bool ReplaceForm(int slotIndex, string newFormId)
{
    if (slotIndex == PRIMARY_FORM_SLOT)
    {
        Debug.Log("[FormManager] Cannot replace primary form slot");
        return false;
    }
    // ...
}
```

**形态添加逻辑** - [FormManager.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/Forms/FormManager.cs#L54-77)
```csharp
public bool TryAddForm(string newFormId)
{
    // 检查是否已拥有 → 检查槽位 → 添加或触发替换流程
}
```

---

## 7. 设计决策

### 7.1 本命职业保护
- **设计理由**：确保玩家始终有一个核心职业作为基础，避免完全随机化带来的挫败感
- **实现方式**：在 FormManager 中定义 PRIMARY_FORM_SLOT = 0，替换时检查

### 7.2 形态槽数量限制
- **设计理由**：限制玩家同时拥有的形态数量，增加策略决策深度
- **当前设定**：最多3个形态槽

### 7.3 切换冷却
- **设计理由**：防止玩家在战斗中频繁切换形态，增加战术思考
- **当前设定**：3秒冷却时间
