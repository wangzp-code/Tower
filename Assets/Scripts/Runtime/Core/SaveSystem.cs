using UnityEngine;
using System;
using System.IO;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public class SaveSystem : SingletonBase<SaveSystem>
{

    private const string SAVE_FOLDER = "Saves";
    private const int MAX_SAVE_SLOTS = 3;
    public const int CURRENT_SAVE_VERSION = 1;

    #region Serializable Dictionary Helpers
    private static List<SerializableKVP_StringInt> DictToList(Dictionary<string, int> dict)
    {
        if (dict == null) return new List<SerializableKVP_StringInt>();
        var list = new List<SerializableKVP_StringInt>(dict.Count);
        foreach (var kvp in dict)
            list.Add(new SerializableKVP_StringInt { key = kvp.Key, value = kvp.Value });
        return list;
    }

    private static Dictionary<string, int> ListToDict(List<SerializableKVP_StringInt> list)
    {
        var dict = new Dictionary<string, int>();
        if (list == null) return dict;
        foreach (var kvp in list)
            dict[kvp.key] = kvp.value;
        return dict;
    }

    private static List<SerializableKVP_StringBool> DictBoolToList(Dictionary<string, bool> dict)
    {
        if (dict == null) return new List<SerializableKVP_StringBool>();
        var list = new List<SerializableKVP_StringBool>(dict.Count);
        foreach (var kvp in dict)
            list.Add(new SerializableKVP_StringBool { key = kvp.Key, value = kvp.Value });
        return list;
    }

    private static Dictionary<string, bool> ListToBoolDict(List<SerializableKVP_StringBool> list)
    {
        var dict = new Dictionary<string, bool>();
        if (list == null) return dict;
        foreach (var kvp in list)
            dict[kvp.key] = kvp.value;
        return dict;
    }
    #endregion

    #region Save Data Classes
    #endregion

    private float autoSaveInterval = 30f;
    private float lastAutoSaveTime;
    private bool _dirty;
    private Coroutine _autoSaveCoroutine;

    protected override void OnDestroy()
    {
        base.OnDestroy();

        if (_autoSaveCoroutine != null)
        {
            StopCoroutine(_autoSaveCoroutine);
            _autoSaveCoroutine = null;
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();

        if (_autoSaveCoroutine != null)
        {
            StopCoroutine(_autoSaveCoroutine);
            _autoSaveCoroutine = null;
        }
    }

    #region Public Methods
    public void AutoSave()
    {
        Save(0);
    }

    /// <summary>
    /// Mark save data as dirty so the next auto-save cycle will persist it.
    /// </summary>
    public void MarkDirty()
    {
        _dirty = true;
    }

    public bool Save(int slotIndex)
    {
        if (slotIndex < 0 || slotIndex >= MAX_SAVE_SLOTS)
        {

            return false;
        }

        try
        {
            var saveData = CreateSaveData();
            var json = JsonUtility.ToJson(saveData, true);
            var path = GetSavePath(slotIndex);

            EnsureSaveDirectory();
            AtomicWrite(path, json);

            _dirty = false;

            EventBus.Emit(EventTypes.GameSaved);
            return true;
        }
        catch (Exception e)
        {
            Analytics.Exception(e);
            return false;
        }
    }

    public bool Load(int slotIndex)
    {
        var path = GetSavePath(slotIndex);

        try
        {
            string json = SafeReadFile(path);
            if (json == null)
            {

                return false;
            }

            var saveData = JsonUtility.FromJson<SaveData>(json);
            saveData = MigrateSaveData(saveData);
            ApplySaveData(saveData);


            EventBus.Emit(EventTypes.GameLoaded);
            return true;
        }
        catch (Exception e)
        {
            Analytics.Exception(e);
            return false;
        }
    }

    // Returns save data for the specified slot
    public SaveData GetSaveInfo(int slotIndex)
    {
        var path = GetSavePath(slotIndex);

        try
        {
            string json = SafeReadFile(path);
            if (json == null) return null;
            return JsonUtility.FromJson<SaveData>(json);
        }
        catch
        {
            return null;
        }
    }

    public bool HasSave(int slotIndex)
    {
        var path = GetSavePath(slotIndex);
        return File.Exists(path);
    }

    public SaveData LoadSaveData(int slotIndex)
    {
        var path = GetSavePath(slotIndex);
        try
        {
            string json = SafeReadFile(path);
            if (json == null) return null;
            var data = JsonUtility.FromJson<SaveData>(json);
            return MigrateSaveData(data);
        }
        catch
        {
            return null;
        }
    }

    public bool DeleteSave(int slotIndex)
    {
        var path = GetSavePath(slotIndex);
        bool deleted = false;
        if (File.Exists(path))
        {
            File.Delete(path);
            deleted = true;
        }
        string bakPath = path + ".bak";
        if (File.Exists(bakPath))
        {
            File.Delete(bakPath);
        }
        return deleted;
    }
    #endregion

    #region Achievement Save Methods
    public void SaveAchievements(Dictionary<string, bool> unlocked, Dictionary<string, int> progress)
    {
        try
        {
            var path = GetAchievementPath();
            EnsureSaveDirectory();

            var data = new AchievementSaveData
            {
                unlockedAchievements = new List<string>(unlocked.Keys).ToArray(),
                achievementProgress = new int[0]
            };

            AtomicWrite(path, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {

        }
    }

    public void LoadAchievements(out Dictionary<string, bool> unlocked, out Dictionary<string, int> progress)
    {
        unlocked = new Dictionary<string, bool>();
        progress = new Dictionary<string, int>();

        var path = GetAchievementPath();
        string json = SafeReadFile(path);
        if (json == null) return;

        try
        {
            var data = JsonUtility.FromJson<AchievementSaveData>(json);

            foreach (string id in data.unlockedAchievements)
            {
                unlocked[id] = true;
            }
        }
        catch (Exception e)
        {

        }
    }

    public void SaveTutorialCompleted(bool completed)
    {
        try
        {
            var path = GetTutorialPath();
            EnsureSaveDirectory();
            AtomicWrite(path, completed.ToString());
        }
        catch (Exception e)
        {

        }
    }

    public bool LoadTutorialCompleted()
    {
        var path = GetTutorialPath();
        string content = SafeReadFile(path);
        if (content == null) return false;

        try
        {
            return bool.Parse(content);
        }
        catch
        {
            return false;
        }
    }

    private string GetAchievementPath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, "achievements.json");
    }

    private string GetTutorialPath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, "tutorial.dat");
    }

    private string GetAnchorPath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, "anchors.json");
    }

    private string GetMetaProgressPath()
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, "meta_progress.json");
    }
    #endregion

    #region Anchor Save Methods
    public void SaveAnchorData(List<AnchorSystem.AnchorData> anchors)
    {
        try
        {
            var path = GetAnchorPath();
            EnsureSaveDirectory();

            var data = new AnchorSaveData
            {
                floors = anchors.Select(a => a.floor).ToArray(),
                activated = anchors.Select(a => a.activated).ToArray(),
                used = anchors.Select(a => a.used).ToArray()
            };

            AtomicWrite(path, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {

        }
    }

    public List<AnchorSystem.AnchorData> LoadAnchorData()
    {
        var path = GetAnchorPath();
        string json = SafeReadFile(path);
        if (json == null) return null;

        try
        {
            var data = JsonUtility.FromJson<AnchorSaveData>(json);

            var anchors = new List<AnchorSystem.AnchorData>();
            for (int i = 0; i < data.floors.Length; i++)
            {
                anchors.Add(new AnchorSystem.AnchorData
                {
                    floor = data.floors[i],
                    activated = data.activated[i],
                    used = data.used[i]
                });
            }
            return anchors;
        }
        catch (Exception e)
        {

            return null;
        }
    }
    #endregion

    #region MetaProgress Save Methods
    public void SaveMetaProgress(MetaProgressSystem.MetaData metaData)
    {
        try
        {
            var path = GetMetaProgressPath();
            EnsureSaveDirectory();

            var data = new MetaProgressSaveData
            {
                totalGamesPlayed = metaData.totalGamesPlayed,
                totalFloorsCleared = metaData.totalFloorsCleared,
                totalKills = metaData.totalKills,
                totalEvoPointsEarned = metaData.totalEvoPointsEarned,
                maxFloorReached = metaData.maxFloorReached,
                maxPollution = metaData.maxPollution,
                deaths = metaData.deaths,
                possessions = metaData.possessions,
                achievementsUnlocked = metaData.achievementsUnlocked,
                endingsAchieved = metaData.endingsAchieved.ToArray(),
                traitsUnlocked = metaData.traitsUnlocked.ToArray(),
                monstersEncountered = metaData.monstersEncountered.ToArray(),
                currentRun = metaData.currentRun,
                noDeathRuns = metaData.noDeathRuns,
                perfectRuns = metaData.perfectRuns,
                classLevelKeys = metaData.classLevel.Keys.ToArray(),
                classLevelValues = metaData.classLevel.Values.ToArray(),
                formBondKeys = metaData.formBond.Keys.ToArray(),
                formBondValues = metaData.formBond.Values.ToArray(),
                newGamePlus = metaData.newGamePlus,
                ngpLevel = metaData.ngpLevel,
                maxExpeditionStage = metaData.maxExpeditionStage,
                expeditionFullClears = metaData.expeditionFullClears
            };

            AtomicWrite(path, JsonUtility.ToJson(data, true));
        }
        catch (Exception e)
        {

        }
    }

    public MetaProgressSystem.MetaData? LoadMetaProgress()
    {
        var path = GetMetaProgressPath();
        string json = SafeReadFile(path);
        if (json == null) return null;

        try
        {
            var data = JsonUtility.FromJson<MetaProgressSaveData>(json);

            var metaData = new MetaProgressSystem.MetaData
            {
                totalGamesPlayed = data.totalGamesPlayed,
                totalFloorsCleared = data.totalFloorsCleared,
                totalKills = data.totalKills,
                totalEvoPointsEarned = data.totalEvoPointsEarned,
                maxFloorReached = data.maxFloorReached,
                maxPollution = data.maxPollution,
                deaths = data.deaths,
                possessions = data.possessions,
                achievementsUnlocked = data.achievementsUnlocked,
                endingsAchieved = new List<string>(data.endingsAchieved ?? new string[0]),
                traitsUnlocked = new List<string>(data.traitsUnlocked ?? new string[0]),
                monstersEncountered = new List<string>(data.monstersEncountered ?? new string[0]),
                currentRun = data.currentRun,
                noDeathRuns = data.noDeathRuns,
                perfectRuns = data.perfectRuns,
                classLevel = new Dictionary<string, int>(),
                formBond = new Dictionary<string, int>(),
                newGamePlus = data.newGamePlus,
                ngpLevel = data.ngpLevel,
                maxExpeditionStage = data.maxExpeditionStage,
                expeditionFullClears = data.expeditionFullClears
            };

            if (data.classLevelKeys != null && data.classLevelValues != null)
            {
                for (int i = 0; i < data.classLevelKeys.Length && i < data.classLevelValues.Length; i++)
                {
                    metaData.classLevel[data.classLevelKeys[i]] = data.classLevelValues[i];
                }
            }

            if (data.formBondKeys != null && data.formBondValues != null)
            {
                for (int i = 0; i < data.formBondKeys.Length && i < data.formBondValues.Length; i++)
                {
                    metaData.formBond[data.formBondKeys[i]] = data.formBondValues[i];
                }
            }

            return metaData;
        }
        catch (Exception e)
        {

            return null;
        }
    }
    #endregion

    #region Private Methods - Save/Load Core

    private SaveData CreateSaveData()
    {
        var gm = GameManager.Instance;
        var p = gm.Player;

        return new SaveData
        {
            version = CURRENT_SAVE_VERSION,
            saveTimeStr = DateTime.Now.ToString("o"),
            gameMode = gm.CurrentMode.ToString(),
            currentFloor = gm.CurrentFloor,
            currentStage = gm.CurrentStage,
            challengeModId = CompleteGameSystem.Instance?.ActiveChallengeModifier?.id,
            expeditionData = gm.ExpeditionData != null ? new ExpeditionRunDataSave
            {
                currentStage = gm.ExpeditionData.currentStage,
                chosenBuffs = gm.ExpeditionData.chosenBuffs?.ToArray() ?? new string[0],
                permAtkBonus = gm.ExpeditionData.permAtkBonus,
                permDefBonus = gm.ExpeditionData.permDefBonus,
                permMaxHpBonus = gm.ExpeditionData.permMaxHpBonus,
                permRegenBonus = gm.ExpeditionData.permRegenBonus,
                startEpBonus = gm.ExpeditionData.startEpBonus,
                possessRateBonus = gm.ExpeditionData.possessRateBonus
            } : null,
            player = new PlayerSaveData
            {
                level = p.level,
                hp = p.hp,
                maxHp = p.maxHp,
                attack = p.attack,
                defense = p.defense,
                pollution = p.pollution,
                selectedClass = p.selectedClass,
                currentForm = p.currentFormId,

                tutorialStage = CompleteGameSystem.Instance?.TutorialStage ?? 0,
                forceTutorial = CompleteGameSystem.Instance?.ForceTutorial ?? false,

                baseMaxHp = p.baseMaxHp,
                baseAttack = p.baseAttack,
                baseDefense = p.baseDefense,
                classFogRadius = p.classFogRadius,

                possessionBonus = p.possessionBonus,
                evolutionPoints = p.evolutionPoints,
                rebirthCount = p.rebirthCount,
                formSlots = p.formSlots,
                collapseResistCharges = p.collapseResistCharges,
                deathReviveCharges = p.deathReviveCharges,
                noDeathRun = p.noDeathRun,

                switchShieldTurns = (int)p.switchShieldTurns,
                switchCooldownTurns = p.switchCooldownTurns,
                defendCountThisCombat = p.defendCountThisCombat,
                switchCountThisCombat = p.switchCountThisCombat,
                possessionCountThisRun = p.possessionCountThisRun,
                totalKillsThisRun = p.totalKillsThisRun,
                turnsInCombat = p.turnsInCombat,
                defendedLastTurn = p.defendedLastTurn,
                formBroken = p.formBroken,

                monstersKilled = p.monstersKilled,
                totalDamageDealt = p.totalDamageDealt,

                runStartTime = p.runStartTime,
                maxPollutionReached = p.maxPollutionReached,
                longestFormId = p.longestFormId,
                longestFormDuration = p.longestFormDuration,
                currentFormStartTime = p.currentFormStartTime,
                runNumber = p.runNumber,

                hasBloodMoon = p.hasBloodMoon,
                tempRegenCombat = p.tempRegenCombat,
                permRegen = p.permRegen,
                extraRevive = p.extraRevive,
                deathBlast = p.deathBlast,
                hasRevived = p.hasRevived,
                formBondCount = p.formBondCount,

                seenMonsterTypes = p.seenMonsterTypes != null ? p.seenMonsterTypes.ToArray() : new string[0],
                deadForms = p.deadForms != null ? p.deadForms.ToArray() : new bool[0],
                traits = p.traits != null ? p.traits.ToArray() : new string[0],

                formResonanceLevels = DictToList(p.formResonanceLevels),
                formHpMap = DictToList(p.formHpMap),
                formBondCounts = DictToList(p.formBondCounts),
                formSlotLevels = DictToList(p.formSlotLevels),
                storyFlags = DictBoolToList(p.storyFlags),
                evolution = DictToList(p.evolution),

                legacyJson = LegacyManager.Instance?.SerializeLegacies() ?? ""
            },
            inventory = new InventorySaveData
            {
                ownedForms = p.ownedForms != null ? p.ownedForms.ToArray() : new string[0],
                equippedFragments = p.equippedFragments != null ? p.equippedFragments.ToArray() : new string[0],
                gold = p.gold,
                fragments = p.fragments
            },
            progress = new ProgressSaveData
            {
                highestFloor = gm.MaxFloor,
                totalWins = 0,
                totalPlays = 1,
                unlockedForms = new string[0],
                unlockedSkills = new string[0]
            },
            achievements = new AchievementSaveData
            {
                unlockedAchievements = new string[0],
                achievementProgress = new int[0]
            },
            floorState = SaveFloorState()
        };
    }

    private FloorStateSave SaveFloorState()
    {
        var gs = CompleteGameSystem.Instance;
        if (gs == null || gs.CurrentFloorState == null) return null;

        var fs = gs.CurrentFloorState;
        var floorSave = new FloorStateSave
        {
            floor = fs.floor,
            zone = fs.zone,
            playerPosX = fs.playerPos.x,
            playerPosY = fs.playerPos.y,
            exitPosX = fs.exitPos.x,
            exitPosY = fs.exitPos.y,
            stepsTaken = fs.stepsTaken,
            discoveredCells = new List<string>(),
            walkableCells = new List<string>(),
            actions = new List<ExploreActionSave>()
        };

        for (int y = 0; y < fs.discovered.GetLength(0); y++)
            for (int x = 0; x < fs.discovered.GetLength(1); x++)
                if (fs.discovered[y, x])
                    floorSave.discoveredCells.Add($"{x},{y}");

        for (int y = 0; y < fs.walkable.GetLength(0); y++)
            for (int x = 0; x < fs.walkable.GetLength(1); x++)
                if (fs.walkable[y, x])
                    floorSave.walkableCells.Add($"{x},{y}");

        foreach (var kvp in fs.actions)
        {
            var actionSave = new ExploreActionSave
            {
                posX = kvp.Key.x,
                posY = kvp.Key.y,
                type = (int)kvp.Value.type,
                title = kvp.Value.title,
                description = kvp.Value.description,
                consumed = kvp.Value.consumed
            };

            if (kvp.Value.monster != null)
            {
                var m = kvp.Value.monster;
                actionSave.monster = new MonsterRuntimeSave
                {
                    id = m.id,
                    name = m.name,
                    hp = m.hp,
                    maxHp = m.maxHp,
                    atk = m.atk,
                    def = m.def,
                    zone = m.zone,
                    isBoss = m.isBoss,
                    traits = m.traits ?? new string[0],
                    axes = m.axes ?? new string[0],
                    possessBaseChance = m.possessBaseChance,
                    colorHex = ColorUtility.ToHtmlStringRGBA(m.color),
                    stairGuard = m.stairGuard
                };
            }

            floorSave.actions.Add(actionSave);
        }

        return floorSave;
    }

    private void ApplySaveData(SaveData data)
    {
        var gm = GameManager.Instance;
        var pd = data.player;

        gm.CurrentMode = ParseGameMode(data.gameMode);
        gm.CurrentFloor = data.currentFloor;
        gm.CurrentStage = data.currentStage > 0 ? data.currentStage : 1;

        // 恢复挑战词条
        if (!string.IsNullOrEmpty(data.challengeModId) && CompleteGameSystem.Instance != null)
        {
            CompleteGameSystem.Instance.RestoreChallengeModifier(data.challengeModId);
        }

        if (data.expeditionData != null)
        {
            gm.ExpeditionData = new ExpeditionRunData
            {
                currentStage = data.expeditionData.currentStage,
                chosenBuffs = data.expeditionData.chosenBuffs != null
                    ? new List<string>(data.expeditionData.chosenBuffs) : new List<string>(),
                permAtkBonus = data.expeditionData.permAtkBonus,
                permDefBonus = data.expeditionData.permDefBonus,
                permMaxHpBonus = data.expeditionData.permMaxHpBonus,
                permRegenBonus = data.expeditionData.permRegenBonus,
                startEpBonus = data.expeditionData.startEpBonus,
                possessRateBonus = data.expeditionData.possessRateBonus
            };
        }
        else
        {
            gm.ExpeditionData = null;
        }

        var p = gm.Player;

        // Core stats
        p.level = pd.level;
        p.hp = pd.hp;
        p.maxHp = pd.maxHp;
        p.attack = pd.attack;
        p.defense = pd.defense;
        p.pollution = pd.pollution;
        p.selectedClass = pd.selectedClass;
        if (p.selectedClass.StartsWith("t_")) p.selectedClass = p.selectedClass.Substring(2);
        p.currentFormId = pd.currentForm ?? "human";

        // Tutorial
        var gs = CompleteGameSystem.Instance;
        if (gs != null)
        {
            gs.AdvanceTutorialStage(pd.tutorialStage);
            gs.SetForceTutorial(pd.forceTutorial);
        }

        // Base stats
        p.baseMaxHp = pd.baseMaxHp > 0 ? pd.baseMaxHp : 100;
        p.baseAttack = pd.baseAttack > 0 ? pd.baseAttack : 10;
        p.baseDefense = pd.baseDefense > 0 ? pd.baseDefense : 5;
        p.classFogRadius = pd.classFogRadius > 0 ? pd.classFogRadius : 5;

        // Progression
        p.possessionBonus = pd.possessionBonus;
        p.evolutionPoints = pd.evolutionPoints;
        p.rebirthCount = pd.rebirthCount;
        p.formSlots = pd.formSlots > 0 ? pd.formSlots : 3;
        p.collapseResistCharges = pd.collapseResistCharges;
        p.deathReviveCharges = pd.deathReviveCharges;
        p.noDeathRun = pd.noDeathRun;

        // Combat counters
        p.switchShieldTurns = pd.switchShieldTurns;
        p.switchCooldownTurns = pd.switchCooldownTurns;
        p.defendCountThisCombat = pd.defendCountThisCombat;
        p.switchCountThisCombat = pd.switchCountThisCombat;
        p.possessionCountThisRun = pd.possessionCountThisRun;
        p.totalKillsThisRun = pd.totalKillsThisRun;
        p.turnsInCombat = pd.turnsInCombat;
        p.defendedLastTurn = pd.defendedLastTurn;
        p.formBroken = pd.formBroken;

        // Death report
        p.monstersKilled = pd.monstersKilled;
        p.totalDamageDealt = pd.totalDamageDealt;

        // Run statistics
        p.runStartTime = pd.runStartTime;
        p.maxPollutionReached = pd.maxPollutionReached;
        p.longestFormId = pd.longestFormId ?? "";
        p.longestFormDuration = pd.longestFormDuration;
        p.currentFormStartTime = pd.currentFormStartTime;
        p.runNumber = pd.runNumber;

        // Extra fields
        p.hasBloodMoon = pd.hasBloodMoon;
        p.tempRegenCombat = pd.tempRegenCombat;
        p.permRegen = pd.permRegen;
        p.extraRevive = pd.extraRevive;
        p.deathBlast = pd.deathBlast;
        p.hasRevived = pd.hasRevived;
        p.formBondCount = pd.formBondCount;

        // Lists
        p.ownedForms = new List<string>(data.inventory.ownedForms ?? new string[] { "human" });
        p.equippedFragments = new List<string>(data.inventory.equippedFragments ?? new string[0]);
        p.gold = data.inventory.gold;
        p.fragments = data.inventory.fragments;
        p.seenMonsterTypes = new List<string>(pd.seenMonsterTypes ?? new string[0]);
        p.deadForms = new List<bool>(pd.deadForms ?? new bool[0]);
        p.traits = new List<string>(pd.traits ?? new string[0]);

        // Dictionaries
        p.formResonanceLevels = ListToDict(pd.formResonanceLevels);
        p.formHpMap = ListToDict(pd.formHpMap);
        p.formBondCounts = ListToDict(pd.formBondCounts);
        p.formSlotLevels = ListToDict(pd.formSlotLevels);
        p.storyFlags = ListToBoolDict(pd.storyFlags);
        p.evolution = ListToDict(pd.evolution);

        // Restore legacy abilities
        if (!string.IsNullOrEmpty(pd.legacyJson))
            LegacyManager.Instance?.RestoreLegacies(pd.legacyJson);

        // Restore floor state
        if (data.floorState != null)
        {
            RestoreFloorState(data.floorState);
        }
    }

    private void RestoreFloorState(FloorStateSave fs)
    {
        var gs = CompleteGameSystem.Instance;
        if (gs == null) return;

        var floorState = new FloorRuntime
        {
            floor = fs.floor,
            zone = fs.zone,
            playerPos = new Vector2Int(fs.playerPosX, fs.playerPosY),
            exitPos = new Vector2Int(fs.exitPosX, fs.exitPosY),
            stepsTaken = fs.stepsTaken,
            discovered = new bool[13, 13],
            walkable = new bool[13, 13],
            actions = new Dictionary<Vector2Int, ExploreAction>()
        };

        foreach (var cell in fs.discoveredCells ?? new List<string>())
        {
            var parts = cell.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            {
                if (x >= 0 && x < 13 && y >= 0 && y < 13)
                    floorState.discovered[y, x] = true;
            }
        }

        foreach (var cell in fs.walkableCells ?? new List<string>())
        {
            var parts = cell.Split(',');
            if (parts.Length == 2 && int.TryParse(parts[0], out int x) && int.TryParse(parts[1], out int y))
            {
                if (x >= 0 && x < 13 && y >= 0 && y < 13)
                    floorState.walkable[y, x] = true;
            }
        }

        foreach (var actionSave in fs.actions ?? new List<ExploreActionSave>())
        {
            var pos = new Vector2Int(actionSave.posX, actionSave.posY);
            var action = new ExploreAction
            {
                type = (ExploreActionType)actionSave.type,
                title = actionSave.title,
                description = actionSave.description,
                consumed = actionSave.consumed
            };

            if (actionSave.monster != null)
            {
                var m = actionSave.monster;
                Color color = Color.white;
                if (!string.IsNullOrEmpty(m.colorHex))
                    ColorUtility.TryParseHtmlString("#" + m.colorHex, out color);

                action.monster = new MonsterRuntime
                {
                    id = m.id,
                    name = m.name,
                    hp = m.hp,
                    maxHp = m.maxHp,
                    atk = m.atk,
                    def = m.def,
                    zone = m.zone,
                    isBoss = m.isBoss,
                    traits = m.traits,
                    axes = m.axes,
                    possessBaseChance = m.possessBaseChance,
                    color = color,
                    stairGuard = m.stairGuard
                };
            }

            floorState.actions[pos] = action;
        }

        gs.SetFloorState(floorState);
    }
    #endregion

    #region Version Migration
    private static readonly string[] VERSION_MIGRATION_NOTES = new string[]
    {
        "v1: Initial version",
        "v2: Added form resonance and boss kill tracking",
        "v3: Added corruption skill runtime persistence",
    };

    private SaveData MigrateSaveData(SaveData data)
    {
        if (data == null) return null;
        if (data.version >= CURRENT_SAVE_VERSION) return data;

        Debug.Log($"[SaveSystem] Migrating save data from v{data.version} to v{CURRENT_SAVE_VERSION}...");

        if (data.version < 1)
        {
            data.version = 1;
        }

        if (data.version < 2)
        {
            if (data.player != null)
            {
                if (data.player.formResonanceLevels == null)
                {
                    data.player.formResonanceLevels = new List<SerializableKVP_StringInt>();
                }
                if (data.player.formHpMap == null)
                {
                    data.player.formHpMap = new List<SerializableKVP_StringInt>();
                }
                if (data.player.formBondCounts == null)
                {
                    data.player.formBondCounts = new List<SerializableKVP_StringInt>();
                }
                if (data.player.formSlotLevels == null)
                {
                    data.player.formSlotLevels = new List<SerializableKVP_StringInt>();
                }
            }
            if (data.progress != null)
            {
                if (data.progress.unlockedForms == null)
                {
                    data.progress.unlockedForms = new string[0];
                }
                if (data.progress.unlockedSkills == null)
                {
                    data.progress.unlockedSkills = new string[0];
                }
            }
            if (data.achievements != null)
            {
                if (data.achievements.unlockedAchievements == null)
                {
                    data.achievements.unlockedAchievements = new string[0];
                }
                if (data.achievements.achievementProgress == null)
                {
                    data.achievements.achievementProgress = new int[0];
                }
            }
            data.version = 2;
            Debug.Log("[SaveSystem] Migrated to v2: Added form resonance and boss kill tracking");
        }

        if (data.version < 3)
        {
            if (data.player != null)
            {
                if (data.player.storyFlags == null)
                {
                    data.player.storyFlags = new List<SerializableKVP_StringBool>();
                }
                if (data.player.evolution == null)
                {
                    data.player.evolution = new List<SerializableKVP_StringInt>();
                }
            }
            if (data.achievements != null && data.achievements.unlockedAchievements != null)
            {
                var list = new List<string>(data.achievements.unlockedAchievements);
                data.achievements.unlockedAchievements = list.ToArray();
            }
            data.version = 3;
            Debug.Log("[SaveSystem] Migrated to v3: Added corruption skill persistence");
        }

        data.version = CURRENT_SAVE_VERSION;
        Debug.Log($"[SaveSystem] Migration complete. Version: v{data.version}");

        return data;
    }
    #endregion

    #region Atomic Write / Safe Read
    /// <summary>
    /// Writes data atomically: write to .tmp, backup old to .bak, rename .tmp to target.
    /// </summary>
    private void AtomicWrite(string path, string content)
    {
        string tempPath = path + ".tmp";
        string bakPath = path + ".bak";

        // 1. Write to temporary file
        File.WriteAllText(tempPath, content);

        // 2. Backup existing file
        if (File.Exists(path))
        {
            File.Copy(path, bakPath, true);
        }

        // 3. Replace target with temp (File.Move does not overwrite on all platforms)
        if (File.Exists(path))
        {
            File.Delete(path);
        }
        File.Move(tempPath, path);
    }

    /// <summary>
    /// Reads a file; if it does not exist or is corrupt, tries the .bak backup.
    /// Returns null if neither is available.
    /// </summary>
    private string SafeReadFile(string path)
    {
        // Try primary file
        if (File.Exists(path))
        {
            try
            {
                string content = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(content))
                    return content;
            }
            catch (Exception e)
            {

            }
        }

        // Try backup
        string bakPath = path + ".bak";
        if (File.Exists(bakPath))
        {
            try
            {
                string content = File.ReadAllText(bakPath);
                if (!string.IsNullOrEmpty(content))
                {

                    try { File.Copy(bakPath, path, true); } catch { }
                    return content;
                }
            }
            catch (Exception e)
            {

            }
        }

        return null;
    }
    #endregion

    #region Path Helpers
    private string GetSavePath(int slotIndex)
    {
        return Path.Combine(Application.persistentDataPath, SAVE_FOLDER, $"save_{slotIndex}.json");
    }

    private void EnsureSaveDirectory()
    {
        var dir = Path.Combine(Application.persistentDataPath, SAVE_FOLDER);
        if (!Directory.Exists(dir))
        {
            Directory.CreateDirectory(dir);
        }
    }

    private GameMode ParseGameMode(string mode)
    {
        if (Enum.TryParse<GameMode>(mode, out var result))
        {
            return result;
        }
        return GameMode.Classic;
    }
    #endregion

    #region Auto-Save (Coroutine)
    private void Update()
    {
        if (GameManager.Instance != null && GameManager.Instance.IsPlaying())
        {
            if (Time.time - lastAutoSaveTime > autoSaveInterval)
            {
                _dirty = true;
            }

            if (_dirty && _autoSaveCoroutine == null)
            {
                _autoSaveCoroutine = StartCoroutine(AutoSaveCoroutine());
            }
        }
    }

    private IEnumerator AutoSaveCoroutine()
    {
        // Yield one frame so we never block Update
        yield return null;

        try
        {
            Save(0);
            lastAutoSaveTime = Time.time;
        }
        catch (Exception e)
        {

        }

        _autoSaveCoroutine = null;
    }
    #endregion
}
