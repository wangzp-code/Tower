# 形态共鸣系统设计文档

## 1. 系统概述

形态共鸣是切换形态时触发的临时增益系统，鼓励玩家在战斗中灵活切换形态而非固守单一形态。

来源：`FormResonanceSystem.cs`

---

## 2. 共鸣触发

### 2.1 触发条件

调用 `OnFormSwitch(oldFormId, newFormId)` 时：
1. 获取旧形态的 `axes` 数组和新形态的 `axes` 数组
2. 遍历 `FormResonanceConfig.json` 中的共鸣组合定义
3. 如果旧形态含 `fromAxis` 且新形态含 `toAxis`，触发该共鸣

### 2.2 共鸣组合定义

来源：`StreamingAssets/Config/FormResonanceConfig.json`

```
ResonanceCombo {
  fromAxis    — 来源形态轴标签
  toAxis      — 目标形态轴标签
  name        — 共鸣名称
  effectType  — 效果类型
  value       — 基础效果值
  duration    — 基础持续回合
  desc        — 描述文本
}
```

---

## 3. 效果类型

| effectType | 效果 | 持续 |
|-----------|------|------|
| pollution_damage | 即时伤害 = floor(污染值 × value) | 无 |
| regen_burst | 即时回复 = ceil(maxHP × value) | 无 |
| shield | 获得1回合护盾 | 1回合 |
| atk_boost | ATK加成 +value% | 持续 |
| ignore_def | 无视敌人DEF × value% | 持续 |
| crit_boost | 暴击概率 +value | 持续 |
| double_attack | 二连击，额外 damage × value | 持续 |
| poison_dot | 每回合毒伤 = maxHP × value | 持续 |
| lifesteal_full | 攻击回复 damage × value HP | 持续 |

---

## 4. 持续时间计算

```
baseDuration = combo.duration
durationBonus = 0

进化Lv3+: durationBonus += 1
进化Lv5+: durationBonus += 1, valueMult ×1.5
污染60-84: durationBonus += 1
污染85+:   durationBonus += 1, valueMult ×1.5
           (15%概率反噬 5% maxHP)
遗产匹配:  legacyMult = 1.5

最终持续 = baseDuration + durationBonus
最终效果 = value × valueMult × legacyMult
```

每回合结束 `turnsRemaining--`，归零移除。

---

## 5. 形态熟练度

来源：`_formUseTurns` / `_proficiencyBonus`

| 参数 | 值 |
|------|---|
| 每步累积 | TURNS_PER_STEP = 3回合 |
| 每级加成 | PROFICIENCY_PER_STEP = 5% |
| 最大加成 | PROFICIENCY_MAX = 25% |

连续使用同一形态每3回合获得+5%攻击加成，上限25%。切换形态后该形态的熟练度保留。

查询：`GetProficiencyBonus(formId)` → 0.0 ~ 0.25
查询全部：`GetAllProficiencies()` → Dictionary<string, float>

---

## 6. UI展示

### 6.1 构筑总览面板

`CanvasUIManager.BuildOverview.cs` 中显示：
- **◈ 共鸣效果**：所有激活中的共鸣，显示名称和剩余回合
- **◎ 形态熟练度**：各形态当前熟练度加成

### 6.2 战斗日志

共鸣触发时显示彩色日志：
- `◈ 二连击 +{bonusDmg}` (橙色)
- `※ 共鸣暴击！` (金色)
- `◈ 寄生吸取 +{heal}HP` (绿色)

### 6.3 短局第4层提示

短局F4进入时，如果玩家有≥2形态但从未触发共鸣，自动提示+FlashBanner。

---

**文档版本**: v1.0
**所属模块**: FormResonance
**最后更新**: 2026-06-23
