using UnityEngine;
using System.Collections.Generic;

public class DeathTransferSystem
{
    private CompleteGameSystem gameSystem;
    private bool isDeathChoiceActive;

    public bool IsDeathChoiceActive => isDeathChoiceActive;

    public DeathTransferSystem(CompleteGameSystem system)
    {
        gameSystem = system;
    }

    public void HandlePlayerDeath()
    {
        var player = GameManager.Instance.Player;
        
        List<string> aliveBackups = GetAliveBackupForms(player);
        
        if (aliveBackups.Count > 0)
        {
            isDeathChoiceActive = true;
            gameSystem.AddCombatLog("<color=#00ffd0>意识抽离中...</color>");
        }
        else
        {
            gameSystem.AddCombatLog("<color=#ff006e>// 宿主死亡 //</color>");
        }
    }

    private List<string> GetAliveBackupForms(GameManager.PlayerData player)
    {
        List<string> aliveForms = new List<string>();
        
        foreach (string formId in player.ownedForms)
        {
            if (formId == player.currentFormId) continue;
            aliveForms.Add(formId);
        }
        
        return aliveForms;
    }

    public void SelectNewForm(string formId)
    {
        var player = GameManager.Instance.Player;
        
        gameSystem.PlayerSwitchForm(formId);
        
        isDeathChoiceActive = false;
        
        gameSystem.AddCombatLog($"<color=#00ffd0>意识转移至 {formId}！</color>");
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public void CancelDeathChoice()
    {
        isDeathChoiceActive = false;
    }

    public bool TryRevive(float hpPercent = 0.5f)
    {
        var player = GameManager.Instance.Player;
        
        if (player.extraRevive > 0)
        {
            player.extraRevive--;
            player.hp = Mathf.FloorToInt(player.maxHp * hpPercent);
            
            gameSystem.AddCombatLog($"<color=#ffff00>死亡拒绝! 复活{Mathf.RoundToInt(hpPercent * 100)}%HP!</color>");
            return true;
        }
        
        return false;
    }

    public bool HandleUndeadTrait()
    {
        var player = GameManager.Instance.Player;
        
        if (!player.traits.Contains("不死")) return false;
        
        if (!player.hasRevived)
        {
            player.hasRevived = true;
            player.hp = Mathf.Max(1, Mathf.FloorToInt(player.maxHp * 0.3f));
            
            gameSystem.AddCombatLog("<color=#ffff00>【不死】复活！恢复30%HP</color>");
            return true;
        }
        
        return false;
    }
}