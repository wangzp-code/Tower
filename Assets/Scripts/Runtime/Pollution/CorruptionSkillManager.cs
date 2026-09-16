using UnityEngine;
using System;
using System.Collections.Generic;

public class CorruptionSkillManager : SingletonBase<CorruptionSkillManager>
{
    [Serializable]
    public class SkillRuntimeState
    {
        public string id;
        public int cooldownRemaining;
        public int usesThisCombat;
    }

    private CorruptionSkillConfig[] _configs;
    private Dictionary<string, SkillRuntimeState> _runtimeStates = new Dictionary<string, SkillRuntimeState>();

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        _configs = DataConfigManager.Instance?.GetCorruptionSkillConfigs() ?? new CorruptionSkillConfig[0];

    }

    #region Skill Query
    public CorruptionSkillConfig[] GetAllSkills() => _configs ?? new CorruptionSkillConfig[0];

    public List<CorruptionSkillConfig> GetUnlockedSkills(float currentPollution)
    {
        var result = new List<CorruptionSkillConfig>();
        if (_configs == null) return result;

        foreach (var config in _configs)
        {
            if (currentPollution >= config.threshold)
                result.Add(config);
        }
        return result;
    }

    public bool IsSkillAvailable(string skillId, float currentPollution)
    {
        var config = GetSkillConfig(skillId);
        if (config == null) return false;
        if (currentPollution < config.threshold) return false;
        if (currentPollution < config.cost) return false;

        if (_runtimeStates.TryGetValue(skillId, out var state))
        {
            if (state.cooldownRemaining > 0) return false;
        }

        float costReduction = LegacyManager.Instance?.GetActiveLegacyEffectValue("skill_cost_reduce") ?? 0;
        float actualCost = config.cost * (1f - costReduction);
        if (currentPollution < actualCost) return false;

        return true;
    }

    public CorruptionSkillConfig GetSkillConfig(string id)
    {
        if (_configs == null) return null;
        return Array.Find(_configs, c => c.id == id);
    }

    public int GetCooldownRemaining(string skillId)
    {
        if (_runtimeStates.TryGetValue(skillId, out var state))
            return state.cooldownRemaining;
        return 0;
    }
    #endregion

    #region Skill Usage
    public struct SkillResult
    {
        public bool success;
        public string message;
        public int damageDealt;
        public int healAmount;
        public float pollutionCost;
    }

    public SkillResult UseSkill(string skillId)
    {
        var result = new SkillResult();
        var config = GetSkillConfig(skillId);
        var player = GameManager.Instance?.Player;

        if (config == null || player == null)
        {
            result.message = "技能不可用";
            return result;
        }

        if (!IsSkillAvailable(skillId, player.pollution))
        {
            result.message = "条件不满足";
            return result;
        }

        float costReduction = LegacyManager.Instance?.GetActiveLegacyEffectValue("skill_cost_reduce") ?? 0;
        float actualCost = config.cost * (1f - costReduction);

        PollutionSystem.Instance?.ReducePollution(actualCost);
        result.pollutionCost = actualCost;

        switch (config.effectType)
        {
            case "damage":
                int dmg = Mathf.RoundToInt(player.attack * config.effectValue);
                result.damageDealt = dmg;
                result.message = $"{config.name}: 造成 {dmg} 伤害";
                break;

            case "refresh_skills":
                ResetAllCooldowns();
                int hpCost = Mathf.RoundToInt(player.hp * config.effectValue);
                player.hp = Mathf.Max(1, player.hp - hpCost);
                result.message = $"{config.name}: 消耗 {hpCost}HP，技能冷却重置";
                break;

            case "execute":
                var enemy = CompleteGameSystem.Instance?.CurrentEnemy;
                if (enemy != null)
                {
                    float threshold = config.effectValue;
                    float hpRatio = (float)enemy.hp / enemy.maxHp;
                    if (hpRatio < threshold)
                    {
                        result.damageDealt = enemy.hp;
                        result.message = $"{config.name}: 即杀！{enemy.name} HP低于{threshold * 100}%";
                    }
                    else
                    {
                        int execDmg = Mathf.RoundToInt(player.attack * 1.2f);
                        result.damageDealt = execDmg;
                        result.message = $"{config.name}: 目标HP过高，造成 {execDmg} 伤害";
                    }
                }
                break;

            case "shield":
                int shieldVal = Mathf.RoundToInt(actualCost * config.effectValue);
                player.switchShieldTurns += 2;
                result.message = $"{config.name}: 获得护盾（{shieldVal}）";
                break;

            case "lifesteal":
                var lsEnemy = CompleteGameSystem.Instance?.CurrentEnemy;
                if (lsEnemy != null)
                {
                    int stealHp = Mathf.RoundToInt(lsEnemy.hp * config.effectValue);
                    stealHp = Mathf.Min(stealHp, lsEnemy.hp);
                    result.damageDealt = stealHp;
                    result.healAmount = stealHp;
                    player.hp = Mathf.Min(player.maxHp, player.hp + stealHp);
                    result.message = $"{config.name}: 吸取 {stealHp}HP";
                }
                break;

            case "aoe_damage":
                int aoeDmg = Mathf.RoundToInt(player.attack * config.effectValue);
                result.damageDealt = aoeDmg;
                result.message = $"{config.name}: 对全体造成 {aoeDmg} 伤害";
                break;
        }

        if (!_runtimeStates.ContainsKey(skillId))
            _runtimeStates[skillId] = new SkillRuntimeState { id = skillId };
        _runtimeStates[skillId].cooldownRemaining = config.cooldown;
        _runtimeStates[skillId].usesThisCombat++;

        EventBus.Emit(EventTypes.PlayerUseSkill);
        EventBus.Emit(EventTypes.PlayerStatsChanged);

        result.success = true;

        return result;
    }
    #endregion

    #region Turn / Combat Management
    public void OnTurnEnd()
    {
        foreach (var state in _runtimeStates.Values)
        {
            if (state.cooldownRemaining > 0)
                state.cooldownRemaining--;
        }
    }

    public void OnCombatStart()
    {
        _runtimeStates.Clear();
    }

    public void OnCombatEnd()
    {
        _runtimeStates.Clear();
    }

    void ResetAllCooldowns()
    {
        foreach (var state in _runtimeStates.Values)
            state.cooldownRemaining = 0;
    }
    #endregion

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _runtimeStates.Clear();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _runtimeStates.Clear();
    }
}
