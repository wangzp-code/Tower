using UnityEngine;
using System;
using System.Collections.Generic;

public class CombatManager : SingletonBase<CombatManager>
{
    [Header("Combat Settings")]
    public float turnDelay = 0.25f;
    public int baseDamage = 0;
    public float critMultiplier = 1.5f;
    public float critChance = 0.1f;

    private MonsterBase currentEnemy;
    private readonly List<string> combatLog = new List<string>();
    private bool isDefending;

    public enum CombatState
    {
        Idle,
        PlayerTurn,
        EnemyTurn,
        Animating,
        Victory,
        Defeat
    }

    public CombatState State { get; private set; } = CombatState.Idle;
    public event Action<CombatState> OnStateChanged;

    protected override void Awake()
    {
        base.Awake();
    }

    public void StartCombat(MonsterBase monster)
    {
        currentEnemy = monster;
        combatLog.Clear();
        isDefending = false;
        State = CombatState.PlayerTurn;
        NotifyStateChanged();
        EventBus.Emit(EventTypes.CombatStarted);

        if (monster != null && monster.data != null)
        {
            AddCombatLog($"Encountered {monster.data.monsterName}!");
        }
    }

    public void EndCombat(bool victory)
    {
        State = victory ? CombatState.Victory : CombatState.Defeat;
        NotifyStateChanged();
        EventBus.Emit(victory ? EventTypes.CombatVictory : EventTypes.CombatDefeat);
    }

    public void Attack()
    {
        if (State != CombatState.PlayerTurn || currentEnemy == null)
        {
            return;
        }

        var player = GameManager.Instance.Player;
        State = CombatState.Animating;
        NotifyStateChanged();

        int damage = Mathf.Max(1, player.attack - currentEnemy.defense + baseDamage);
        bool isCrit = UnityEngine.Random.value < critChance;
        if (isCrit)
        {
            damage = Mathf.RoundToInt(damage * critMultiplier);
        }

        currentEnemy.TakeDamage(damage);
        AddCombatLog($"Attack dealt {damage} damage{(isCrit ? " (CRITICAL!)" : "")}");

        if (currentEnemy.IsDead)
        {
            EndCombat(true);
            return;
        }

        State = CombatState.EnemyTurn;
        NotifyStateChanged();
        ExecuteEnemyTurn();
    }

    public void Defend()
    {
        if (State != CombatState.PlayerTurn)
        {
            return;
        }

        isDefending = true;
        State = CombatState.EnemyTurn;
        NotifyStateChanged();
        AddCombatLog("Player defends!");
        ExecuteEnemyTurn();
    }

    public void Flee()
    {
        if (State != CombatState.PlayerTurn)
        {
            return;
        }

        float fleeChance = CalculateFleeChance();
        bool success = UnityEngine.Random.value < fleeChance;
        if (success)
        {
            AddCombatLog("Successfully fled!");
            EndCombat(false);
            return;
        }

        AddCombatLog("Failed to flee!");
        State = CombatState.EnemyTurn;
        NotifyStateChanged();
        ExecuteEnemyTurn();
    }

    private float CalculateFleeChance()
    {
        if (currentEnemy != null && currentEnemy.data != null && currentEnemy.data.isBoss)
        {
            return 0f;
        }
        var config = ConfigManager.Instance?.GetGameConfig();
        return config?.fleeBaseChance ?? 0.3f;
    }

    private void ExecuteEnemyTurn()
    {
        if (currentEnemy == null || currentEnemy.IsDead)
        {
            return;
        }

        // 先计算基础伤害（不防御时的伤害）
        int rawDamage = Mathf.Max(1, currentEnemy.attack - GameManager.Instance.Player.defense);
        // 防御时伤害减半（向下取整，可能为 0），而非仅翻倍防御值后仍保留最小伤害 1
        int damage = isDefending ? Mathf.FloorToInt(rawDamage * 0.5f) : rawDamage;

        GameManager.Instance.Player.hp = Mathf.Max(0, GameManager.Instance.Player.hp - damage);
        AddCombatLog($"{currentEnemy.data.monsterName} attacked for {damage} damage!");
        EventBus.Emit(EventTypes.PlayerDamaged, damage);
        EventBus.Emit(EventTypes.PlayerStatsChanged);

        isDefending = false;
        if (GameManager.Instance.Player.hp <= 0)
        {
            AddCombatLog("You have been defeated!");
            EndCombat(false);
            return;
        }

        State = CombatState.PlayerTurn;
        NotifyStateChanged();
    }

    private void AddCombatLog(string message)
    {
        combatLog.Add(message);
        EventBus.Emit(EventTypes.CombatLogUpdated, message);
    }

    private void NotifyStateChanged()
    {
        OnStateChanged?.Invoke(State);
    }

    public MonsterBase GetCurrentEnemy() => currentEnemy;
    public List<string> GetCombatLog() => combatLog;

    protected override void OnDestroy()
    {
        base.OnDestroy();
        if (this == Instance)
        {
            OnStateChanged = null;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (this == Instance)
        {
            OnStateChanged = null;
        }
    }
}
