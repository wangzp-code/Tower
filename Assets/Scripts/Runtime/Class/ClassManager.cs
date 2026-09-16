using UnityEngine;

public class ClassManager : SingletonBase<ClassManager>
{
    public enum PlayerClass
    {
        Titan,
        Ghost,
        Swarm,
        Blood,
        Mech
    }

    private PlayerClass currentClass;
    private bool titanUltimateUsedThisFloor;

    /// <summary>
    /// 每层/每场战斗开始时调用，重置终极技能使用状态
    /// </summary>
    public void ResetUltimateUsage()
    {
        titanUltimateUsedThisFloor = false;
    }

    private void Awake()
    {
        base.Awake();
    }

    public void SelectClass(PlayerClass playerClass)
    {
        currentClass = playerClass;
        GameManager.Instance.Player.selectedClass = playerClass.ToString().ToLowerInvariant();
    }

    public PlayerClass GetCurrentClass()
    {
        return currentClass;
    }

    public void UseUltimate()
    {
        var player = GameManager.Instance.Player;
        switch (currentClass)
        {
            case PlayerClass.Titan:
                // 每层只能使用一次，防止 maxHp 指数爆炸
                if (titanUltimateUsedThisFloor)
                {
                    return;
                }
                titanUltimateUsedThisFloor = true;
                // 基于基础值的固定增量，而非当前值的乘法
                int hpBonus = Mathf.RoundToInt(player.baseMaxHp * 0.5f);
                player.maxHp += hpBonus;
                player.hp = Mathf.Min(player.maxHp, player.hp + 20);
                player.attack += 15;
                player.defense += 15;
                break;
            case PlayerClass.Ghost:
                player.attack += 20;
                break;
            case PlayerClass.Swarm:
                player.evolutionPoints += 30;
                break;
            case PlayerClass.Blood:
                player.hp = Mathf.Min(player.maxHp, player.hp + 10);
                player.attack += 8;
                break;
            case PlayerClass.Mech:
                player.defense += 8;
                break;
        }
        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
