using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public enum MonsterAIType
{
    Idle,
    Patrol,
    Aggressive,
    Ambush
}

public struct MonsterTemplate
{
    public string id;
    public string name;
    public int hp;
    public int atk;
    public int def;
    public int zone;
    public bool boss;
    public Color color;
    public List<string> traits;
    public List<string> axes;
}

public static class MonsterAISystem
{
    public static MonsterAIType GetMonsterAI(MonsterTemplate template)
    {
        if (template.traits == null) return MonsterAIType.Patrol;
        
        if (template.traits.Contains("伏击")) return MonsterAIType.Ambush;
        
        if (template.traits.Any(t => new[] { "狂暴", "撕裂", "多重攻击" }.Contains(t)))
            return MonsterAIType.Aggressive;
        
        if (template.traits.Any(t => new[] { "迅捷", "巡逻", "电击", "毒素", "蛛网" }.Contains(t)))
            return MonsterAIType.Patrol;
        
        if (template.traits.Any(t => new[] { "护甲", "厚皮", "再生", "再生+" }.Contains(t)))
            return MonsterAIType.Idle;
        
        return template.zone >= 3 ? MonsterAIType.Aggressive : MonsterAIType.Patrol;
    }

    public static int GetMonsterDetectRange(MonsterTemplate template)
    {
        if (template.traits == null) return 3;
        
        if (template.traits.Contains("伏击")) return 3;
        if (template.traits.Contains("恐惧")) return 6;
        if (template.zone >= 4) return 5;
        if (template.zone >= 3) return 4;
        return 3;
    }

    private static MonsterTemplate GetMonsterTemplate(string monsterId)
    {
        var def = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == monsterId);
        
        return new MonsterTemplate {
            id = def.id,
            name = def.name,
            hp = def.hp,
            atk = def.atk,
            def = def.def,
            zone = def.zone,
            boss = def.boss,
            color = def.color,
            traits = def.traits != null ? new List<string>(def.traits) : new List<string>(),
            axes = def.axes != null ? new List<string>(def.axes) : new List<string>()
        };
    }

    public static void MoveMonsterToward(MonsterRuntime m, int tx, int ty)
    {
        if (Mathf.Abs(tx - m.x) + Mathf.Abs(ty - m.y) <= 1) return;
        
        int dx = (int)Mathf.Sign(tx - m.x);
        int dy = (int)Mathf.Sign(ty - m.y);
        int adx = Mathf.Abs(tx - m.x);
        int ady = Mathf.Abs(ty - m.y);

        if (adx >= ady)
        {
            TryMoveMonster(m, m.x + dx, m.y);
        }
        else
        {
            TryMoveMonster(m, m.x, m.y + dy);
        }
    }

    public static bool TryMoveMonster(MonsterRuntime m, int nx, int ny)
    {
        if (m.isTutorialPinned) return false;
        if (nx < 1 || ny < 1 || nx > 11 || ny > 11) return false;
        
        m.prevX = m.x;
        m.prevY = m.y;
        m.x = nx;
        m.y = ny;
        return true;
    }
}