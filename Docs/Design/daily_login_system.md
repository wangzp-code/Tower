
# 每日登录系统设计文档

## 1. 系统概述

### 1.1 设计目的
每日登录系统鼓励玩家每日登录游戏，通过连续登录奖励提升玩家活跃度和留存率。

### 1.2 核心概念

| 概念 | 说明 |
|------|------|
| 登录天数 | 连续登录的天数计数 |
| 登录奖励 | 每日登录可领取的奖励 |
| 奖励领取状态 | 记录当天奖励是否已领取 |
| 重置机制 | 未登录时重置连续天数 |

---

## 2. 系统架构

### 2.1 数据存储

**PlayerPrefs 键值**

| 键名 | 类型 | 说明 |
|------|------|------|
| pt_daily_login_days | int | 连续登录天数 |
| pt_daily_last_claim | string | 上次领取日期（yyyy-MM-dd） |

### 2.2 奖励配置

**9天奖励循环**

| 天数 | 奖励内容 |
|------|---------|
| 1 | 10 EP |
| 2 | 15 EP |
| 3 | 20 EP |
| 4 | 25 EP |
| 5 | 30 EP |
| 6 | 35 EP |
| 7 | 45 EP |
| 8 | 55 EP |
| 9 | 70 EP |

---

## 3. 功能设计

### 3.1 登录检测

**检测流程：**
```
游戏启动 → 检查上次登录日期 → 
判断是否为新的一天 → 更新登录状态
```

### 3.2 奖励领取

**领取条件：**
- 当天未领取过奖励
- 与上次领取日期不同

**领取流程：**
```
打开每日登录面板 → 检查可领取状态 → 
显示奖励 → 点击领取 → 发放奖励 → 
更新登录天数 → 记录领取日期
```

**关键逻辑** - [CanvasUIManager.Panels.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/UI/CanvasUIManager.Panels.cs#L725-741)
```csharp
void ClaimDailyReward()
{
    int days = PlayerPrefs.GetInt("pt_daily_login_days", 0);
    int[] rewards = { 10, 15, 20, 25, 30, 35, 45, 55, 70 };
    int epReward = rewards[Mathf.Min(days, rewards.Length - 1)];
    
    // 发放奖励
    player.evolutionPoints += epReward;
    
    // 更新状态
    days++;
    PlayerPrefs.SetInt("pt_daily_login_days", days);
    PlayerPrefs.SetString("pt_daily_last_claim", DateTime.Now.ToString("yyyy-MM-dd"));
    PlayerPrefs.Save();
}
```

### 3.3 状态检查

**可领取判断** - [CanvasUIManager.Panels.cs](file:///Users/wangzhipeng/tower/Assets/Scripts/Runtime/UI/CanvasUIManager.Panels.cs#L712-717)
```csharp
bool CanClaimDailyReward()
{
    string lastClaim = PlayerPrefs.GetString("pt_daily_last_claim", "");
    string today = DateTime.Now.ToString("yyyy-MM-dd");
    return lastClaim != today;
}
```

### 3.4 重置机制

**重置条件：**
- 超过1天未登录
- 手动重置（设置界面）

**重置方式：**
```csharp
// 清除登录记录
PlayerPrefs.DeleteKey("pt_daily_login_days");
PlayerPrefs.DeleteKey("pt_daily_last_claim");
```

---

## 4. UI设计

### 4.1 每日登录面板

```
┌─────────────────────────────┐
│      每日登录奖励           │
│                             │
│  今日奖励：[10 EP]          │
│                             │
│  已连续登录：[X]天          │
│                             │
│  ┌──┬──┬──┬──┬──┬──┬──┬──┐ │
│  │1 │2 │3 │4 │5 │6 │7 │8 │ │
│  └──┴──┴──┴──┴──┴──┴──┴──┘ │
│     [已领][已领][可领]...   │
│                             │
│     [领取奖励]              │
└─────────────────────────────┘
```

### 4.2 奖励状态显示
- 已领取：灰色标记
- 可领取：高亮显示
- 未达到：锁定状态

---

## 5. 设计决策

### 5.1 天数递增时机
- **问题**：之前实现在打开面板时递增，导致天数不正确
- **修复**：仅在实际领取奖励时递增天数

### 5.2 日期判断
- 使用"yyyy-MM-dd"格式比较
- 确保跨时区正确处理

### 5.3 数据持久化
- 使用 PlayerPrefs 存储
- 领取后立即保存

---

## 6. 版本历史

| 版本 | 日期 | 更新内容 |
|------|------|---------|
| v1.0 | 2026-06 | 初始实现 |
| v1.1 | 2026-06 | 修复天数递增bug，改为领取时递增 |
