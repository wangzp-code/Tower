using UnityEngine;

public class MonsterBase : MonoBehaviour
{
    public MonsterData data;
    public MonsterData Data => data;
    public int currentHp;
    public int maxHp;
    public int attack;
    public int defense;
    public EnemyIntent intent;

    // Trait-related fields
    public float damageReduction;
    public float regenRate;
    public float lifesteal;
    public float evasion;
    public int armor;
    public int attackCount = 1;
    public float lowHpThreshold;
    public float lowHpAttackMultiplier;
    public float critChance;
    public float damageReflect;
    public float goldBonus;

    public bool IsDead => currentHp <= 0;

    private void Awake()
    {
        if (data == null)
        {

            return;
        }
        InitializeStats();
    }

    private void InitializeStats()
    {
        int floor = GameManager.Instance.CurrentFloor;
        float scaling = 1f + floor * ConfigManager.Instance.GetGameConfig().enemyHpScaling;

        maxHp = Mathf.RoundToInt(data.baseHp * scaling);
        currentHp = maxHp;
        attack = Mathf.RoundToInt(data.baseAttack * scaling);
        defense = data.baseDefense;

        // Apply trait effects
        TraitManager.Instance?.ApplyTraitEffects(this);
    }

    public void TakeDamage(int damage)
    {
        int actualDamage = Mathf.Max(1, damage);
        currentHp -= actualDamage;

        if (currentHp <= 0)
        {
            currentHp = 0;
            OnDeath();
        }
    }

    public void Heal(int amount)
    {
        currentHp = Mathf.Min(currentHp + amount, maxHp);
    }

    public void ApplyBuff()
    {
        attack += Mathf.RoundToInt(attack * 0.2f);
    }

    public void UpdateIntent()
    {
        if (data.possibleIntents != null && data.possibleIntents.Length > 0)
        {
            intent.SetIntent(data.possibleIntents.RandomElement(), attack);
        }
    }

    private void OnDeath()
    {
        Destroy(gameObject, 0.1f);
    }
}
