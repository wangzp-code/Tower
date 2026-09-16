using UnityEngine;
using System.Collections.Generic;

public class AchievementCelebrationSystem
{
    private CompleteGameSystem gameSystem;
    private List<AchievementData> pendingAchievements = new List<AchievementData>();
    private bool isCelebrating;
    private float celebrationTimer;

    public bool IsCelebrating => isCelebrating;

    public AchievementCelebrationSystem(CompleteGameSystem system)
    {
        gameSystem = system;
    }

    public void Update()
    {
        if (isCelebrating)
        {
            celebrationTimer -= Time.deltaTime;
            if (celebrationTimer <= 0)
            {
                EndCelebration();
            }
            return;
        }

        if (pendingAchievements.Count > 0)
        {
            StartCelebration(pendingAchievements[0]);
            pendingAchievements.RemoveAt(0);
        }
    }

    private void StartCelebration(AchievementData achievement)
    {
        isCelebrating = true;
        celebrationTimer = 2f;
        
        gameSystem.AddCombatLog($"<color=#ffd700><b>★ {achievement.name} 解锁！</b></color>");
        gameSystem.AddCombatLog($"{achievement.description}");
    }

    private void EndCelebration()
    {
        isCelebrating = false;
        celebrationTimer = 0f;
    }

    public void TriggerAhaMoment(string message, string color = "#00ffd0")
    {
        gameSystem.AddCombatLog($"<color={color}><b>★ {message} ★</b></color>");
    }

    public void TriggerBossDefeat(string bossName)
    {
        TriggerAhaMoment($"BOSS {bossName} 已击败", "#00ffd0");
        gameSystem.AddCombatLog($"<color=#00ffd0><b>★ Boss击败！HP完全恢复！</b></color>");
    }

    public void TriggerMilestone(int floor)
    {
        string message = floor switch {
            10 => "抵达第10层！",
            25 => "抵达第25层！",
            50 => "抵达第50层！",
            100 => "抵达第100层！",
            _ => $"抵达第{floor}层！"
        };
        
        TriggerAhaMoment(message, floor >= 50 ? "#ffd700" : "#00ffd0");
    }

    public void TriggerComboBreakthrough(int comboCount, string tierName)
    {
        string color = comboCount >= 20 ? "#00ffd0" : comboCount >= 12 ? "#b455ff" : comboCount >= 8 ? "#ff8800" : "#ff0";
        TriggerAhaMoment($"{tierName} ({comboCount}连击)", color);
    }

    public void TriggerRareDiscovery(string name)
    {
        TriggerAhaMoment($"发现稀有: {name}", "#ffd700");
    }

    public void ClearPendingAchievements()
    {
        pendingAchievements.Clear();
    }
}