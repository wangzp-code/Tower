using UnityEngine;
using System;
using System.Collections.Generic;

[Serializable]
public class SaveData
{
    public int version = SaveSystem.CURRENT_SAVE_VERSION;
    public string saveTimeStr;
    public string gameMode;
    public int currentFloor;
    public int currentStage = 1;
    public string challengeModId;
    public ExpeditionRunDataSave expeditionData;
    public PlayerSaveData player;
    public InventorySaveData inventory;
    public ProgressSaveData progress;
    public AchievementSaveData achievements;
    
    public FloorStateSave floorState;

    public DateTime saveTime
    {
        get
        {
            if (DateTime.TryParse(saveTimeStr, out var dt)) return dt;
            return DateTime.MinValue;
        }
    }
}

[Serializable]
public class FloorStateSave
{
    public int floor;
    public int zone;
    public int playerPosX;
    public int playerPosY;
    public int exitPosX;
    public int exitPosY;
    public int stepsTaken;
    public List<string> discoveredCells;
    public List<string> walkableCells;
    public List<ExploreActionSave> actions;
}

[Serializable]
public class ExploreActionSave
{
    public int posX;
    public int posY;
    public int type;
    public string title;
    public string description;
    public bool consumed;
    public MonsterRuntimeSave monster;
}

[Serializable]
public class MonsterRuntimeSave
{
    public string id;
    public string name;
    public int hp;
    public int maxHp;
    public int atk;
    public int def;
    public int zone;
    public bool isBoss;
    public string[] traits;
    public string[] axes;
    public float possessBaseChance;
    public string colorHex;
    public bool stairGuard;
}

[Serializable]
public class PlayerSaveData
{
    public int level;
    public int hp, maxHp;
    public int attack, defense;
    public float pollution;
    public string selectedClass;
    public string currentForm;

    public int tutorialStage;
    public bool forceTutorial;

    public int baseMaxHp;
    public int baseAttack;
    public int baseDefense;
    public int classFogRadius;

    public float possessionBonus;
    public int evolutionPoints;
    public int rebirthCount;
    public int formSlots;
    public int collapseResistCharges;
    public int deathReviveCharges;
    public bool noDeathRun;

    public int switchShieldTurns;
    public int switchCooldownTurns;
    public int defendCountThisCombat;
    public int switchCountThisCombat;
    public int possessionCountThisRun;
    public int totalKillsThisRun;
    public int turnsInCombat;
    public bool defendedLastTurn;
    public bool formBroken;

    public int monstersKilled;
    public int totalDamageDealt;

    public float runStartTime;
    public float maxPollutionReached;
    public string longestFormId;
    public float longestFormDuration;
    public float currentFormStartTime;
    public int runNumber;

    public bool hasBloodMoon;
    public float tempRegenCombat;
    public float permRegen;
    public int extraRevive;
    public int deathBlast;
    public bool hasRevived;
    public int formBondCount;

    public string[] seenMonsterTypes;
    public bool[] deadForms;
    public string[] traits;

    public List<SerializableKVP_StringInt> formResonanceLevels;
    public List<SerializableKVP_StringInt> formHpMap;
    public List<SerializableKVP_StringInt> formBondCounts;
    public List<SerializableKVP_StringInt> formSlotLevels;
    public List<SerializableKVP_StringBool> storyFlags;
    public List<SerializableKVP_StringInt> evolution;

    public string legacyJson;
}

[Serializable]
public class InventorySaveData
{
    public string[] ownedForms;
    public string[] equippedFragments;
    public int gold;
    public int fragments;
}

[Serializable]
public class ProgressSaveData
{
    public int highestFloor;
    public int totalWins;
    public int totalPlays;
    public string[] unlockedForms;
    public string[] unlockedSkills;
}

[Serializable]
public class AchievementSaveData
{
    public string[] unlockedAchievements;
    public int[] achievementProgress;
}

[Serializable]
public class AnchorSaveData
{
    public int[] floors;
    public bool[] activated;
    public bool[] used;
}

[Serializable]
public class ExpeditionRunDataSave
{
    public int currentStage = 1;
    public string[] chosenBuffs;
    public int permAtkBonus;
    public int permDefBonus;
    public int permMaxHpBonus;
    public float permRegenBonus;
    public int startEpBonus;
    public float possessRateBonus;
}

[Serializable]
public class MetaProgressSaveData
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
    public string[] endingsAchieved;
    public string[] traitsUnlocked;
    public string[] monstersEncountered;
    public int currentRun;
    public int noDeathRuns;
    public int perfectRuns;
    public string[] classLevelKeys;
    public int[] classLevelValues;
    public string[] formBondKeys;
    public int[] formBondValues;
    public bool newGamePlus;
    public int ngpLevel;
    public int maxExpeditionStage;
    public int expeditionFullClears;
}

[Serializable]
public class SerializableKVP_StringInt
{
    public string key;
    public int value;
}

[Serializable]
public class SerializableKVP_StringBool
{
    public string key;
    public bool value;
}

[Serializable]
public class SerializableKVP_StringString
{
    public string key;
    public string value;
}

[Serializable]
public class SerializableKVP_StringFloat
{
    public string key;
    public float value;
}

[Serializable]
public class DeathReportSaveData
{
    public string deathDate;
    public int floor;
    public int zone;
    public string monsterId;
    public string monsterName;
    public int playerHp;
    public int playerMaxHp;
    public int playerAtk;
    public int playerDef;
    public float pollution;
    public string currentForm;
    public string[] ownedForms;
    public int evolutionPoints;
    public int runNumber;
    public float runDuration;
    public int monstersKilled;
    public int totalDamageDealt;
    public int maxPollutionReached;
    public string legacyJson;
}
