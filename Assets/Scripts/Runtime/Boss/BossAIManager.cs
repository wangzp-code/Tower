using UnityEngine;
using System;
using System.Collections.Generic;

public class BossAIManager : SingletonBase<BossAIManager>
{
    private BossPhaseConfigEntry[] _phases;
    private int _currentPhaseIndex;
    private bool _isActive;
    private string _activeBossId;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        _phases = null;
        _currentPhaseIndex = 0;
        _isActive = false;
        _activeBossId = null;
    }

    public MonsterRuntime CreateZeroContainerBoss(int floor)
    {
        var phaseConfigs = DataConfigManager.Instance?.GetBossPhasesById("zero_container");
        if (phaseConfigs == null || phaseConfigs.Length == 0)
        {

            return null;
        }

        _phases = phaseConfigs;
        _currentPhaseIndex = 0;
        _isActive = true;
        _activeBossId = "zero_container";

        var phase1 = _phases[0];
        var curve = ModeFloorCurves.GetParams(GameManager.Instance.CurrentMode, floor, GameManager.Instance.CurrentStage);

        int baseHp = Mathf.RoundToInt(200 * curve.hpMult);
        int hp = Mathf.Max(100, baseHp);

        var boss = new MonsterRuntime
        {
            id = "zero_container",
            name = "零号容器",
            hp = hp,
            maxHp = hp,
            atk = phase1.atk,
            def = phase1.def,
            zone = 3,
            isBoss = true,
            traits = phase1.traits ?? new string[0],
            axes = new string[0],
            color = new Color(0.6f, 0.1f, 0.9f),
            possessBaseChance = 0.05f,
            phaseIndex = 1,
            _intent = null,
            _intentAtkBuff = 0f,
            _stunned = false,
            _netStunTurns = 0,
            _ambush = false,
            _elite = false,
            _possessWindowUsed = false,
            _breakWindowDone = false
        };

        return boss;
    }

    public bool IsActiveBoss() => _isActive;
    public string ActiveBossId => _activeBossId;
    public int CurrentPhase => _currentPhaseIndex + 1;
    public int TotalPhases => _phases?.Length ?? 0;

    public BossPhaseConfigEntry GetCurrentPhaseConfig()
    {
        if (_phases == null || _currentPhaseIndex >= _phases.Length) return null;
        return _phases[_currentPhaseIndex];
    }

    public bool CheckPhaseTransition(MonsterRuntime boss)
    {
        if (!_isActive || _phases == null || boss == null) return false;

        int nextPhaseIdx = _currentPhaseIndex + 1;
        if (nextPhaseIdx >= _phases.Length) return false;

        var nextPhase = _phases[nextPhaseIdx];
        float hpRatio = (float)boss.hp / boss.maxHp;

        if (hpRatio <= nextPhase.hpPercent)
        {
            _currentPhaseIndex = nextPhaseIdx;
            ApplyPhaseStats(boss, nextPhase);
            return true;
        }

        return false;
    }

    void ApplyPhaseStats(MonsterRuntime boss, BossPhaseConfigEntry phase)
    {
        boss.atk = phase.atk;
        boss.def = phase.def;
        boss.traits = phase.traits ?? boss.traits;
        boss.phaseIndex = _currentPhaseIndex + 1;

        CompleteGameSystem.Instance?.AddCombatLog($"<color=#a040f0>--- 阶段 {boss.phaseIndex} ---</color>");
        if (!string.IsNullOrEmpty(phase.dialogue))
        {
            CompleteGameSystem.Instance?.AddCombatLog($"<color=#c8a0ff>「{phase.dialogue}」</color>");
        }

        EventBus.Emit(EventTypes.BossPhaseChange);
    }

    public string GetBossSkillAction(MonsterRuntime boss)
    {
        if (!_isActive || _phases == null) return "attack";

        var phase = GetCurrentPhaseConfig();
        if (phase == null || phase.skills == null || phase.skills.Length == 0) return "attack";

        int roll = UnityEngine.Random.Range(0, phase.skills.Length);
        return phase.skills[roll];
    }

    public int ExecuteBossSkill(string skillName, MonsterRuntime boss, GameManager.PlayerData player)
    {
        if (boss == null || player == null) return 0;

        switch (skillName)
        {
            case "attack":
                return -1;

            case "corrupt":
                float pollGain = 8 + _currentPhaseIndex * 4;
                PollutionSystem.Instance?.GainPollution(pollGain);
                CompleteGameSystem.Instance?.AddCombatLog($"<color=#9900ff>☢ 零号容器释放腐蚀波！污染+{pollGain}</color>");
                return 0;

            case "devour":
                int devourDmg = Mathf.RoundToInt(boss.atk * 1.5f);
                int devourHeal = Mathf.RoundToInt(devourDmg * 0.5f);
                player.hp -= devourDmg;
                boss.hp = Mathf.Min(boss.maxHp, boss.hp + devourHeal);
                CompleteGameSystem.Instance?.AddCombatLog($"<color=#ff006e>◎ 吞噬！对你造成 {devourDmg} 伤害并恢复 {devourHeal} HP</color>");
                if (player.hp <= 0) player.hp = 0;
                return devourDmg;

            case "nova":
                int novaDmg = Mathf.RoundToInt(boss.atk * 2f);
                float novaPoll = 15;
                player.hp -= novaDmg;
                PollutionSystem.Instance?.GainPollution(novaPoll);
                CompleteGameSystem.Instance?.AddCombatLog($"<color=#ff3300>※ 虚空爆发！{novaDmg}伤害 + 污染+{novaPoll}</color>");
                if (player.hp <= 0) player.hp = 0;
                return novaDmg;

            default:
                return -1;
        }
    }

    public void ApplyTraitEffects(MonsterRuntime boss)
    {
        if (boss == null || boss.traits == null) return;

        foreach (var trait in boss.traits)
        {
            switch (trait)
            {
                case "regen":
                    int regen = Mathf.RoundToInt(boss.maxHp * 0.03f);
                    boss.hp = Mathf.Min(boss.maxHp, boss.hp + regen);
                    if (regen > 0)
                        CompleteGameSystem.Instance?.AddCombatLog($"<color=#00ff88>♻ 再生 +{regen}HP</color>");
                    break;

                case "rage":
                    float hpRatio = (float)boss.hp / boss.maxHp;
                    if (hpRatio < 0.3f)
                    {
                        boss.atk = Mathf.RoundToInt(boss.atk * 1.1f);
                    }
                    break;

                case "lifesteal":
                    break;
            }
        }
    }

    public void OnBossDefeated()
    {
        if (!_isActive) return;
        _isActive = false;
        EventBus.Emit(EventTypes.BossDefeat);

    }

    public void Reset()
    {
        _isActive = false;
        _activeBossId = null;
        _phases = null;
        _currentPhaseIndex = 0;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _phases = null;
        _activeBossId = null;
        _isActive = false;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
