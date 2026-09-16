using UnityEngine;
using System.Collections.Generic;
using System.IO;

public class DataConfigManager : MonoBehaviour
{
    public static DataConfigManager Instance;

    private string _configPath;
    private ConfigData _configData;

    private MonsterBaseConfig[] _monsterConfigs;
    private HostProfileConfig[] _hostConfigs;
    private LegacyConfig[] _legacyConfigs;
    private CorruptionSkillConfig[] _corruptionSkillConfigs;
    private FloorEventConfig[] _floorEventConfigs;
    private BossPhaseConfigEntry[] _bossPhaseConfigs;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
            _configPath = Path.Combine(Application.streamingAssetsPath, "Config");
            LoadConfig();
            LoadExtendedConfigs();
        }
        else
        {
            Destroy(gameObject);
        }
    }

    void LoadConfig()
    {
        string mainConfigPath = Path.Combine(_configPath, "GameConfig.json");

        
        if (File.Exists(mainConfigPath))
        {
            try
            {
                string json = File.ReadAllText(mainConfigPath);

                _configData = JsonUtility.FromJson<ConfigData>(json);

            }
            catch (System.Exception e)
            {
                Analytics.Exception(e);
                _configData = CreateDefaultConfig();
            }
        }
        else
        {

            _configData = CreateDefaultConfig();
        }
    }

    ConfigData CreateDefaultConfig()
    {
        return new ConfigData
        {
            dailyLogin = new DailyLoginConfig
            {
                rewards = new int[] { 10, 15, 20, 25, 30, 35, 45, 55, 70 }
            },
            combat = new CombatConfig
            {
                baseEpReward = 15,
                epPerZone = 20,
                epRandomRange = 10,
                eliteEpMultiplier = 1.8f,
                swarmEpBonusL2 = 5,
                swarmEpBonusL5 = 10
            },
            runReport = new RunReportConfig
            {
                echoDivider = 3,
                victoryBonus = 80,
                defeatBonus = 20
            }
        };
    }

    public int[] GetDailyLoginRewards()
    {
        if (_configData == null || _configData.dailyLogin == null || _configData.dailyLogin.rewards == null)
        {

            return new int[] { 10, 15, 20, 25, 30, 35, 45, 55, 70 };
        }
        return _configData.dailyLogin.rewards;
    }

    public CombatConfig GetCombatConfig()
    {
        return _configData?.combat ?? new CombatConfig();
    }

    public RunReportConfig GetRunReportConfig()
    {
        return _configData?.runReport ?? new RunReportConfig();
    }

    #region Extended Config Loading
    void LoadExtendedConfigs()
    {
        _monsterConfigs = LoadJsonArray<MonsterBaseConfig>("MonsterBase.json");
        _hostConfigs = LoadJsonArray<HostProfileConfig>("HostProfile.json");
        _legacyConfigs = LoadJsonArray<LegacyConfig>("LegacyConfig.json");
        _corruptionSkillConfigs = LoadJsonArray<CorruptionSkillConfig>("CorruptionSkill.json");
        _floorEventConfigs = LoadJsonArray<FloorEventConfig>("FloorEvent.json");
        _bossPhaseConfigs = LoadJsonArray<BossPhaseConfigEntry>("BossPhaseConfig.json");


    }

    T[] LoadJsonArray<T>(string filename)
    {
        string path = Path.Combine(_configPath, filename);
        if (!File.Exists(path))
        {

            return new T[0];
        }
        try
        {
            string json = File.ReadAllText(path);
            var wrapper = JsonUtility.FromJson<JsonArrayWrapper<T>>("{\"items\":" + json + "}");
            return wrapper?.items ?? new T[0];
        }
        catch (System.Exception e)
        {

            return new T[0];
        }
    }

    public MonsterBaseConfig[] GetMonsterConfigs() => _monsterConfigs ?? new MonsterBaseConfig[0];
    public HostProfileConfig[] GetHostConfigs() => _hostConfigs ?? new HostProfileConfig[0];
    public LegacyConfig[] GetLegacyConfigs() => _legacyConfigs ?? new LegacyConfig[0];
    public CorruptionSkillConfig[] GetCorruptionSkillConfigs() => _corruptionSkillConfigs ?? new CorruptionSkillConfig[0];
    public FloorEventConfig[] GetFloorEventConfigs() => _floorEventConfigs ?? new FloorEventConfig[0];
    public BossPhaseConfigEntry[] GetBossPhaseConfigs() => _bossPhaseConfigs ?? new BossPhaseConfigEntry[0];

    public MonsterBaseConfig GetMonsterById(string id)
    {
        if (_monsterConfigs == null) return null;
        return System.Array.Find(_monsterConfigs, m => m.id == id);
    }

    public LegacyConfig GetLegacyById(string id)
    {
        if (_legacyConfigs == null) return null;
        return System.Array.Find(_legacyConfigs, l => l.id == id);
    }

    public CorruptionSkillConfig GetCorruptionSkillById(string id)
    {
        if (_corruptionSkillConfigs == null) return null;
        return System.Array.Find(_corruptionSkillConfigs, s => s.id == id);
    }

    public FloorEventConfig[] GetFloorEventsForFloor(int floor)
    {
        if (_floorEventConfigs == null) return new FloorEventConfig[0];
        return System.Array.FindAll(_floorEventConfigs, e => floor >= e.floorMin && floor <= e.floorMax);
    }

    public BossPhaseConfigEntry[] GetBossPhasesById(string bossId)
    {
        if (_bossPhaseConfigs == null) return new BossPhaseConfigEntry[0];
        return System.Array.FindAll(_bossPhaseConfigs, p => p.bossId == bossId);
    }
    #endregion
}

[System.Serializable]
public class ConfigData
{
    public DailyLoginConfig dailyLogin;
    public CombatConfig combat;
    public RunReportConfig runReport;
}

[System.Serializable]
public class DailyLoginConfig
{
    public int[] rewards;
}

[System.Serializable]
public class CombatConfig
{
    public int baseEpReward = 15;
    public int epPerZone = 20;
    public int epRandomRange = 10;
    public float eliteEpMultiplier = 1.8f;
    public int swarmEpBonusL2 = 5;
    public int swarmEpBonusL5 = 10;
}

[System.Serializable]
public class RunReportConfig
{
    public int echoDivider = 3;
    public int victoryBonus = 80;
    public int defeatBonus = 20;
}

[System.Serializable]
public class JsonArrayWrapper<T>
{
    public T[] items;
}

[System.Serializable]
public class MonsterBaseConfig
{
    public string id;
    public string name;
    public int hp;
    public int atk;
    public int def;
    public string[] skills;
    public int zoneMin;
    public int zoneMax;
    public int zone;
    public bool boss;
    public string type;
    public string icon;
    public float possessRate;
    public string desc;
    public float colorR;
    public float colorG;
    public float colorB;
    public string[] traits;
    public string[] axes;
}

[System.Serializable]
public class HostProfileConfig
{
    public string id;
    public string name;
    public string classType;
    public int baseHp;
    public int baseAtk;
    public int baseDef;
    public string[] traits;
    public string desc;
    public string icon;
}

[System.Serializable]
public class LegacyConfig
{
    public string id;
    public string name;
    public string desc;
    public string triggerHook;
    public string effectType;
    public float effectValue;
    public string unlockCondition;
    public string icon;
    public int tier;
}

[System.Serializable]
public class CorruptionSkillConfig
{
    public string id;
    public string name;
    public string desc;
    public float threshold;
    public float cost;
    public int cooldown;
    public string effectType;
    public float effectValue;
    public string icon;
    public int tier;
}

[System.Serializable]
public class FloorEventConfig
{
    public string id;
    public string name;
    public string desc;
    public int floorMin;
    public int floorMax;
    public float probability;
    public string eventType;
    public string rewardType;
    public float rewardValue;
    public string icon;
    public string[] choices;
}

[System.Serializable]
public class BossPhaseConfigEntry
{
    public string bossId;
    public int phase;
    public float hpPercent;
    public int atk;
    public int def;
    public string[] skills;
    public string dialogue;
    public string[] traits;
}
