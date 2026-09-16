using UnityEngine;

public class VFXManager : MonoBehaviour
{
    public static VFXManager Instance { get; private set; }

    [Header("Transform References")]
    public Transform playerTarget;
    public Transform monsterTarget;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this.gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(this.gameObject);
    }

    public void PlayAttackEffect(bool isCritical = false)
    {
        if (isCritical)
        {
            ScreenEffectsManager.Instance?.FlashCrit();
            EffectsManager.Instance?.PlayHitEffect(GetMonsterPosition(), true);
        }
        else
        {
            ScreenEffectsManager.Instance?.FlashDamage();
            EffectsManager.Instance?.PlayHitEffect(GetMonsterPosition(), false);
        }
    }

    public void PlayDefenseEffect()
    {
        ScreenEffectsManager.Instance?.FlashHeal();
        EffectsManager.Instance?.Shake(0.05f, 0.1f);
    }

    public void PlayHealEffect()
    {
        ScreenEffectsManager.Instance?.FlashHeal();
        EffectsManager.Instance?.PlayHealEffect(GetPlayerPosition());
    }

    public void PlayDeathEffect()
    {
        ScreenEffectsManager.Instance?.ShakeHeavy();
        ScreenEffectsManager.Instance?.FlashDamage();
    }

    public void PlayPollutionEffect()
    {
        EffectsManager.Instance?.PlayPollutionEffect(50f);
    }

    public void PlayLevelUpEffect()
    {
        EffectsManager.Instance?.PlayEvolutionEffect(GetPlayerPosition());
    }

    public void SpawnDamageNumber(int damage, bool isCritical = false, bool isPlayer = false)
    {
        Color color = isCritical ? new Color(1f, 0.8f, 0.2f) : new Color(1f, 0.3f, 0.2f);
        EffectsManager.Instance?.FlashScreen(color, 0.1f);
    }

    public void SpawnHealNumber(int heal)
    {
        EffectsManager.Instance?.PlayHealEffect(GetPlayerPosition());
    }

    public void SetPlayerTarget(Transform target)
    {
        playerTarget = target;
    }

    public void SetMonsterTarget(Transform target)
    {
        monsterTarget = target;
    }

    private Vector3 GetPlayerPosition()
    {
        return playerTarget != null ? playerTarget.position : Vector3.zero;
    }

    private Vector3 GetMonsterPosition()
    {
        return monsterTarget != null ? monsterTarget.position : Vector3.zero;
    }
}
