using UnityEngine;
using System.Collections.Generic;

public class FragmentManager : SingletonBase<FragmentManager>
{
    protected override void Awake()
    {
        base.Awake();
    }

    public void EquipFragment(string fragmentId)
    {
        var player = GameManager.Instance.Player;
        var config = ConfigManager.Instance.GetGameConfig();

        if (player.equippedFragments.Count < config.maxFragmentSlots)
        {
            player.equippedFragments.Add(fragmentId);
            ApplyFragmentBonus(fragmentId);
            EventBus.Emit(EventTypes.FragmentEquipped, fragmentId);
        }
    }

    public void UnequipFragment(string fragmentId)
    {
        var player = GameManager.Instance.Player;
        if (player.equippedFragments.Contains(fragmentId))
        {
            RemoveFragmentBonus(fragmentId);
            player.equippedFragments.Remove(fragmentId);
        }
    }

    private void ApplyFragmentBonus(string fragmentId)
    {
        var fragment = ConfigManager.Instance.GetFragmentData(fragmentId);
        if (fragment != null)
        {
            var player = GameManager.Instance.Player;
            player.attack += fragment.attackBonus;
            player.defense += fragment.defenseBonus;
            player.maxHp += fragment.hpBonus;
            player.hp += fragment.hpBonus;
        }
    }

    private void RemoveFragmentBonus(string fragmentId)
    {
        var fragment = ConfigManager.Instance.GetFragmentData(fragmentId);
        if (fragment != null)
        {
            var player = GameManager.Instance.Player;
            player.attack -= fragment.attackBonus;
            player.defense -= fragment.defenseBonus;
            player.maxHp -= fragment.hpBonus;
            // 确保 hp 不超过降低后的 maxHp
            player.hp = Mathf.Min(player.hp, player.maxHp);
        }
    }

    public void UseFragmentSkill(string fragmentId)
    {
        var fragment = ConfigManager.Instance.GetFragmentData(fragmentId);
        if (fragment != null && fragment.hasActiveSkill)
        {
        }
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
