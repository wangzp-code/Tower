using UnityEngine;

public enum CombatResult
{
    Continue,
    Killed,
    Dead
}

[System.Obsolete("Combat logic is handled inline by CompleteGameSystem. This class is retained for enum CombatResult only.")]
public class CombatResultSystem
{
    private CompleteGameSystem gameSystem;

    public CombatResultSystem(CompleteGameSystem system)
    {
        gameSystem = system;
    }

    public CombatResult HandleCombatResult(MonsterRuntime monster, int round)
    {
        var player = GameManager.Instance.Player;

        if (monster.hp <= 0 && monster.HasTrait("不死") && !monster.hasRevived)
        {
            monster.hasRevived = true;
            monster.hp = Mathf.Max(1, Mathf.FloorToInt(monster.maxHp * 0.3f));
            gameSystem.AddCombatLog($"【不死】{monster.name}复活了！恢复30%HP");
            return CombatResult.Continue;
        }

        if (monster.hp <= 0 && monster.HasTrait("爆炸"))
        {
            int boomDamage = Mathf.Max(1, Mathf.FloorToInt(monster.maxHp * 0.3f));
            player.hp = Mathf.Max(1, player.hp - boomDamage);
            gameSystem.AddCombatLog($"【爆炸】{monster.name}自爆! -{boomDamage}HP");
        }

        if (monster.hp <= 0 && monster.HasTrait("毒素"))
        {
            int poisonAdd = Mathf.Min(3, Mathf.FloorToInt((100 - player.pollution) * 0.03f));
            if (poisonAdd > 0)
            {
                player.pollution = Mathf.Min(100, player.pollution + poisonAdd);
                gameSystem.AddCombatLog($"毒素残留 污染+{poisonAdd}");
            }
        }

        if (player.hp <= 0)
        {
            gameSystem.AddCombatLog("// 宿主死亡 //");
            return CombatResult.Dead;
        }

        if (monster.hp <= 0)
        {
            int zoneBonus = 1;
            int evoGain = 10 + zoneBonus * 15 + Random.Range(0, 8);
            
            if (monster.isElite)
                evoGain = Mathf.FloorToInt(evoGain * 1.8f);

            player.evolutionPoints += evoGain;
            gameSystem.AddCombatLog($"胜利！+{evoGain}EP");

            if (!string.IsNullOrEmpty(monster.id) && monster.id.StartsWith("boss"))
            {
                player.hp = player.maxHp;
                gameSystem.AddCombatLog("★ Boss击败！HP完全恢复！");
            }

            return CombatResult.Killed;
        }

        return CombatResult.Continue;
    }

    public void ApplyEndOfRoundEffects(MonsterRuntime monster, int playerDamage, int monsterDamage)
    {
        var player = GameManager.Instance.Player;

        if (player.selectedClass == "blood" && playerDamage > 0)
        {
            float bloodRate = player.hasBloodMoon ? 1.0f : 0.10f;
            int heal = Mathf.Max(1, Mathf.FloorToInt(playerDamage * bloodRate));
            player.hp = Mathf.Min(player.maxHp, player.hp + heal);
            gameSystem.AddCombatLog($"{(bloodRate >= 1 ? "狂宴" : "嗜血")}+{heal}");
        }

        float combatRegen = (player.tempRegenCombat + player.permRegen);
        if (combatRegen > 0)
        {
            int heal = Mathf.Max(1, Mathf.FloorToInt(player.maxHp * combatRegen));
            player.hp = Mathf.Min(player.maxHp, player.hp + heal);
            gameSystem.AddCombatLog($"再生+{heal}");
        }

        if (monster.hp > 0)
        {
            int regen = MonsterTraitSystem.HandleRegeneration(monster);
            if (regen > 0)
            {
                monster.hp = Mathf.Min(monster.maxHp, monster.hp + regen);
                gameSystem.AddCombatLog($"{monster.name} 再生+{regen}");
            }
        }

        if (monster.hasBleedApplied && monster.hp > 0)
        {
            int bleedDamage = Mathf.Max(1, Mathf.FloorToInt(player.maxHp * 0.05f));
            player.hp -= bleedDamage;
            gameSystem.AddCombatLog($"撕裂流血 -{bleedDamage}");
        }

        if (PollutionPassiveSystem.IsPassiveActive("死亡脉冲", player.pollution))
        {
            int pulseHeal = PollutionPassiveSystem.ApplyDeathPulse(player.pollution, player.maxHp);
            if (pulseHeal > 0)
            {
                player.hp = Mathf.Min(player.maxHp, player.hp + pulseHeal);
                gameSystem.AddCombatLog($"死亡脉冲 +{pulseHeal}");
            }
        }
    }
}