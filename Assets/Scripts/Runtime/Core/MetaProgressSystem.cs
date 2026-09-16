using UnityEngine;
using System.Collections.Generic;

public class MetaProgressSystem : SingletonBase<MetaProgressSystem>
{

    public struct MetaData
    {
        public int totalGamesPlayed;
        public int totalFloorsCleared;
        public int totalKills;
        public int totalEvoPointsEarned;
        public int maxFloorReached;
        public int maxPollution;
        public int deaths;
        public int possessions;
        public int achievementsUnlocked;
        public List<string> endingsAchieved;
        public List<string> traitsUnlocked;
        public List<string> monstersEncountered;
        public int currentRun;
        public int noDeathRuns;
        public int perfectRuns;
        public Dictionary<string, int> classLevel;
        public Dictionary<string, int> formBond;
        public bool newGamePlus;
        public int ngpLevel;
        public int maxExpeditionStage;
        public int expeditionFullClears;
    }

    public MetaData metaData = new MetaData();

    /// <summary>
    /// 脏标记：数据变更后置 true，定时器或生命周期事件统一写盘
    /// </summary>
    private bool isDirty;

    /// <summary>
    /// 自动保存间隔（秒）
    /// </summary>
    private const float AutoSaveInterval = 60f;
    private float lastSaveTime;

    protected override void Awake()
    {
        base.Awake();
        LoadMetaProgress();
        lastSaveTime = Time.unscaledTime;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        FlushIfDirty();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        FlushIfDirty();
    }

    private void Update()
    {
        // 定时自动保存（每 AutoSaveInterval 秒）
        if (isDirty && Time.unscaledTime - lastSaveTime >= AutoSaveInterval)
        {
            FlushIfDirty();
        }
    }

    private void OnApplicationPause(bool pauseStatus)
    {
        if (pauseStatus)
        {
            FlushIfDirty();
        }
    }

    private void OnApplicationQuit()
    {
        FlushIfDirty();
    }

    /// <summary>
    /// 如果有脏数据，执行同步写盘
    /// </summary>
    private void FlushIfDirty()
    {
        if (!isDirty) return;
        isDirty = false;
        lastSaveTime = Time.unscaledTime;
        SaveSystem.Instance.SaveMetaProgress(metaData);
    }

    /// <summary>
    /// 标记数据已变更，等待下一个写盘时机
    /// </summary>
    private void MarkDirty()
    {
        isDirty = true;
    }

    void LoadMetaProgress()
    {
        var data = SaveSystem.Instance.LoadMetaProgress();
        if (data.HasValue && data.Value.totalGamesPlayed > 0)
        {
            metaData = data.Value;
        }
        else
        {
            InitializeDefault();
        }
    }

    void InitializeDefault()
    {
        metaData = new MetaData
        {
            totalGamesPlayed = 0,
            totalFloorsCleared = 0,
            totalKills = 0,
            totalEvoPointsEarned = 0,
            maxFloorReached = 0,
            maxPollution = 0,
            deaths = 0,
            possessions = 0,
            achievementsUnlocked = 0,
            endingsAchieved = new List<string>(),
            traitsUnlocked = new List<string>(),
            monstersEncountered = new List<string>(),
            currentRun = 1,
            noDeathRuns = 0,
            perfectRuns = 0,
            classLevel = new Dictionary<string, int>(),
            formBond = new Dictionary<string, int>(),
            newGamePlus = false,
            ngpLevel = 0,
            maxExpeditionStage = 0,
            expeditionFullClears = 0
        };
    }

    public void ResetAll()
    {
        InitializeDefault();
    }

    /// <summary>
    /// 立即同步写盘（仅供外部强制保存场景使用，如切换场景前）
    /// </summary>
    public void SaveMetaProgress()
    {
        isDirty = true;
        FlushIfDirty();
    }

    public void OnNewGame()
    {
        metaData.totalGamesPlayed++;
        metaData.currentRun++;
        MarkDirty();
    }

    public void OnFloorClear(int floor)
    {
        metaData.totalFloorsCleared++;
        if (floor > metaData.maxFloorReached)
        {
            metaData.maxFloorReached = floor;
        }
        MarkDirty();
    }

    public void OnKill()
    {
        metaData.totalKills++;
        MarkDirty();
    }

    public void OnEvoPointsEarned(int amount)
    {
        metaData.totalEvoPointsEarned += amount;
        MarkDirty();
    }

    public void OnPollution(int pollution)
    {
        if (pollution > metaData.maxPollution)
        {
            metaData.maxPollution = pollution;
        }
        MarkDirty();
    }

    public void OnDeath()
    {
        metaData.deaths++;
        MarkDirty();
    }

    public void OnPossession()
    {
        metaData.possessions++;
        MarkDirty();
    }

    public void OnAchievementUnlock()
    {
        metaData.achievementsUnlocked++;
        MarkDirty();
    }

    public void OnEndingAchieved(string endingId)
    {
        if (!metaData.endingsAchieved.Contains(endingId))
        {
            metaData.endingsAchieved.Add(endingId);
        }
        MarkDirty();
    }

    public void OnTraitUnlock(string traitId)
    {
        if (!metaData.traitsUnlocked.Contains(traitId))
        {
            metaData.traitsUnlocked.Add(traitId);
        }
        MarkDirty();
    }

    public void OnMonsterEncounter(string monsterId)
    {
        if (!metaData.monstersEncountered.Contains(monsterId))
        {
            metaData.monstersEncountered.Add(monsterId);
        }
        MarkDirty();
    }

    public void OnNoDeathRun()
    {
        metaData.noDeathRuns++;
        MarkDirty();
    }

    public void OnPerfectRun()
    {
        metaData.perfectRuns++;
        MarkDirty();
    }

    public void OnClassLevelUp(string className)
    {
        if (!metaData.classLevel.ContainsKey(className))
        {
            metaData.classLevel[className] = 0;
        }
        metaData.classLevel[className]++;
        MarkDirty();
    }

    public void OnFormBondIncrease(string formId, int amount)
    {
        if (!metaData.formBond.ContainsKey(formId))
        {
            metaData.formBond[formId] = 0;
        }
        metaData.formBond[formId] += amount;
        MarkDirty();
    }

    public void OnNewGamePlus()
    {
        metaData.newGamePlus = true;
        metaData.ngpLevel++;
        MarkDirty();
    }

    public void OnExpeditionStageCleared(int stage)
    {
        if (stage > metaData.maxExpeditionStage)
            metaData.maxExpeditionStage = stage;
        MarkDirty();
    }

    public void OnExpeditionFullClear()
    {
        metaData.expeditionFullClears++;
        MarkDirty();
    }

    public void ResetProgress()
    {
        InitializeDefault();
        // ResetProgress 需要立即写盘
        SaveMetaProgress();
    }

    public int GetClassLevel(string className)
    {
        return metaData.classLevel.TryGetValue(className, out int level) ? level : 0;
    }

    public int GetFormBond(string formId)
    {
        return metaData.formBond.TryGetValue(formId, out int bond) ? bond : 0;
    }

    public bool HasEnding(string endingId)
    {
        return metaData.endingsAchieved.Contains(endingId);
    }

    public bool HasTrait(string traitId)
    {
        return metaData.traitsUnlocked.Contains(traitId);
    }

    public bool HasMonster(string monsterId)
    {
        return metaData.monstersEncountered.Contains(monsterId);
    }

    public int GetCompletionPercent()
    {
        int total = 0;
        int max = 0;

        // Achievements
        max += 50;
        total += Mathf.Min(50, metaData.achievementsUnlocked);

        // Endings
        max += 20;
        total += Mathf.Min(20, metaData.endingsAchieved.Count * 5);

        // Monsters
        max += 15;
        int monsterPercent = Mathf.RoundToInt((float)metaData.monstersEncountered.Count / 50 * 15);
        total += monsterPercent;

        // Traits
        max += 15;
        int traitPercent = Mathf.RoundToInt((float)metaData.traitsUnlocked.Count / 30 * 15);
        total += traitPercent;

        return Mathf.RoundToInt((float)total / max * 100);
    }
}
