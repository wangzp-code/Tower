using UnityEngine;
using System.Collections.Generic;

public class AchievementManager : SingletonBase<AchievementManager>
{
    private Dictionary<string, AchievementData> achievementDatabase = new Dictionary<string, AchievementData>();
    private Dictionary<string, bool> unlockedAchievements = new Dictionary<string, bool>();
    private Dictionary<string, int> achievementProgress = new Dictionary<string, int>();

    public delegate void AchievementUnlocked(AchievementData achievement);
    public event AchievementUnlocked OnAchievementUnlocked;

    private void Awake()
    {
        base.Awake();
        InitializeAchievements();
        LoadProgress();
    }

    private void InitializeAchievements()
    {
        achievementDatabase["first_kill"] = CreateAchievement("first_kill", "初猎", "击杀第一只怪物", "⚔", AchievementType.Kill, 1, "atk", 1, "攻击+1");
        achievementDatabase["first_possess"] = CreateAchievement("first_possess", "寄生觉醒", "首次成功附身", "★", AchievementType.Possession, 1, "possessBonus", 0.05f, "附身成功率+5%");
        achievementDatabase["floor10"] = CreateAchievement("floor10", "深入", "到达第10层", "◄", AchievementType.Floor, 10, null, 0, null);
        achievementDatabase["floor25"] = CreateAchievement("floor25", "中途觉醒", "到达第25层", "⚡", AchievementType.Floor, 25, "maxHp", 20, "最大生命+20");
        achievementDatabase["floor50"] = CreateAchievement("floor50", "登顶", "到达第50层", "♛", AchievementType.Floor, 50, "atk", 3, "攻击+3");
        achievementDatabase["possess5"] = CreateAchievement("possess5", "收集者", "附身5种不同生物", "♦", AchievementType.PossessionCount, 5, null, 0, null);
        achievementDatabase["possess10"] = CreateAchievement("possess10", "百变怪", "附身10种不同生物", "↔", AchievementType.PossessionCount, 10, "def", 2, "防御+2");
        achievementDatabase["no_death"] = CreateAchievement("no_death", "不死传说", "不死亡通关25层", "☠", AchievementType.NoDeath, 25, "evoPoints", 100, "起始进化点+100");
        achievementDatabase["titan_end"] = CreateAchievement("titan_end", "不可移动的永恒", "达成泰坦结局", "■", AchievementType.Ending, 1, null, 0, null);
        achievementDatabase["ghost_end"] = CreateAchievement("ghost_end", "不存在的自由", "达成幽灵结局", "◎", AchievementType.Ending, 1, null, 0, null);
        achievementDatabase["swarm_end"] = CreateAchievement("swarm_end", "增殖的混沌", "达成虫群结局", "†", AchievementType.Ending, 1, null, 0, null);
        achievementDatabase["blood_end"] = CreateAchievement("blood_end", "永恒的饥渴", "达成血族结局", "♥", AchievementType.Ending, 1, null, 0, null);
        achievementDatabase["mech_end"] = CreateAchievement("mech_end", "超越肉体", "达成机甲结局", "⚙", AchievementType.Ending, 1, null, 0, null);
        achievementDatabase["hidden_end"] = CreateAchievement("hidden_end", "递归的观察者", "达成隐藏结局", "◈", AchievementType.Ending, 1, "pollution", -10, "起始污染-10");
        achievementDatabase["defend10"] = CreateAchievement("defend10", "铁壁", "单场战斗防御10次", "◆", AchievementType.Defend, 10, null, 0, null);
        achievementDatabase["switch3"] = CreateAchievement("switch3", "形态大师", "单场战斗切换形态3次", "↩", AchievementType.FormSwitch, 3, null, 0, null);
        achievementDatabase["pollution0"] = CreateAchievement("pollution0", "纯净", "通关25层时污染为0", "✨", AchievementType.PureRun, 25, null, 0, null);
    }

    private AchievementData CreateAchievement(string id, string name, string desc, string iconStr, AchievementType type, int target, string bonusStat, float bonusValue, string bonusDesc)
    {
        AchievementData achievement = ScriptableObject.CreateInstance<AchievementData>();
        achievement.achievementId = id;
        achievement.name = name;
        achievement.description = desc;
        achievement.icon = iconStr;
        achievement.type = type;
        achievement.target = target;

        if (bonusStat != null)
        {
            AchievementBonus bonus = new AchievementBonus();
            bonus.stat = bonusStat;
            bonus.value = bonusValue;
            bonus.description = bonusDesc;
            achievement.bonus = bonus;
        }

        return achievement;
    }

    public void UpdateProgress(string achievementId, int progress)
    {
        if (!achievementDatabase.ContainsKey(achievementId)) return;

        achievementProgress[achievementId] = progress;
        AchievementData achievement = achievementDatabase[achievementId];

        if (!IsUnlocked(achievementId) && progress >= achievement.target)
        {
            UnlockAchievement(achievementId);
        }
    }

    public void UnlockAchievement(string achievementId)
    {
        if (!achievementDatabase.ContainsKey(achievementId)) return;
        if (IsUnlocked(achievementId)) return;

        unlockedAchievements[achievementId] = true;
        AchievementData achievement = achievementDatabase[achievementId];

        ApplyBonus(achievement);
        SaveProgress();
        OnAchievementUnlocked?.Invoke(achievement);

        if (CompleteGameSystem.Instance != null)
        {
            CompleteGameSystem.Instance.AddCombatLog($"<color=#ffd700><b>★ 成就解锁: {achievement.name}</b></color>");
            if (achievement.bonus != null && !string.IsNullOrEmpty(achievement.bonus.description))
                CompleteGameSystem.Instance.AddCombatLog($"  奖励: {achievement.bonus.description}");
        }
    }

    public bool IsUnlocked(string achievementId)
    {
        return unlockedAchievements.TryGetValue(achievementId, out bool unlocked) && unlocked;
    }

    public int GetProgress(string achievementId)
    {
        return achievementProgress.TryGetValue(achievementId, out int progress) ? progress : 0;
    }

    public AchievementData GetAchievement(string achievementId)
    {
        if (achievementDatabase.TryGetValue(achievementId, out AchievementData achievement))
        {
            return achievement;
        }
        return null;
    }

    public List<AchievementData> GetAllAchievements()
    {
        return new List<AchievementData>(achievementDatabase.Values);
    }

    public List<AchievementData> GetUnlockedAchievements()
    {
        List<AchievementData> unlocked = new List<AchievementData>();
        foreach (string id in unlockedAchievements.Keys)
        {
            if (unlockedAchievements[id] && achievementDatabase.ContainsKey(id))
            {
                unlocked.Add(achievementDatabase[id]);
            }
        }
        return unlocked;
    }

    private void ApplyBonus(AchievementData achievement)
    {
        if (achievement.bonus == null) return;

        var player = GameManager.Instance.Player;
        if (player == null) return;

        switch (achievement.bonus.stat)
        {
            case "atk":
                player.attack += Mathf.RoundToInt(achievement.bonus.value);
                player.baseAttack += Mathf.RoundToInt(achievement.bonus.value);
                break;
            case "def":
                player.defense += Mathf.RoundToInt(achievement.bonus.value);
                player.baseDefense += Mathf.RoundToInt(achievement.bonus.value);
                break;
            case "maxHp":
                int hpDelta = Mathf.RoundToInt(achievement.bonus.value);
                player.maxHp += hpDelta;
                player.baseMaxHp += hpDelta;
                player.hp += hpDelta;
                break;
            case "possessBonus":
                player.possessionBonus += achievement.bonus.value;
                break;
            case "evoPoints":
                player.evolutionPoints += Mathf.RoundToInt(achievement.bonus.value);
                break;
            case "pollution":
                player.pollution = Mathf.Max(0f, player.pollution + achievement.bonus.value);
                break;
        }
        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    private void SaveProgress()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.SaveAchievements(unlockedAchievements, achievementProgress);
        }
    }

    private void LoadProgress()
    {
        if (SaveSystem.Instance != null)
        {
            SaveSystem.Instance.LoadAchievements(out unlockedAchievements, out achievementProgress);
        }
    }

    public void ResetAll()
    {
        unlockedAchievements.Clear();
        achievementProgress.Clear();
    }

    protected override void CleanupEvents()
    {
        OnAchievementUnlocked = null;
    }

    protected override void OnDestroy()
    {
        CleanupEvents();
        achievementDatabase.Clear();
        unlockedAchievements.Clear();
        achievementProgress.Clear();
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
