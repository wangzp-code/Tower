using UnityEngine;

public class ConfigManager : SingletonBase<ConfigManager>
{

    [Header("Game Config")]
    public GameConfig gameConfig;

    [Header("Fragment Configs")]
    public FragmentData[] fragmentData;

    [Header("Monster Configs")]
    public MonsterData[] monsterData;

    [Header("Form Configs")]
    public FormData[] formData;

    protected override void Awake()
    {
        base.Awake();
    }

    public GameConfig GetGameConfig()
    {
        if (gameConfig == null)
        {
            gameConfig = GameDataInitializer.GetOrCreateGameConfig();
        }
        return gameConfig;
    }

    private void SetDefaultGameConfig(GameConfig config)
    {
        config.startingHp = 100;
        config.startingAttack = 10;
        config.startingDefense = 5;
        config.maxFragmentSlots = 4;
        config.baseCritChance = 0.15f;
        config.critMultiplier = 1.5f;
        config.defendMultiplier = 2f;
        config.fleeBaseChance = 0.5f;
        config.pollutionGainPerTurn = 2f;
        config.pollutionDecayPerFloor = 5f;
        config.pollutionThreshold = 50f;
        config.burstModeMultiplier = 1.5f;
        config.classicModeFloorCount = 50;
        config.shortModeFloorCount = 12;
        config.expeditionModeFloorCount = 20;
        config.enemyHpScaling = 0.05f;
        config.enemyAttackScaling = 0.05f;
        config.goldScaling = 0.03f;
    }

    public FragmentData GetFragmentData(string fragmentId)
    {
        foreach (var data in fragmentData)
        {
            if (data.fragmentId == fragmentId)
            {
                return data;
            }
        }
        return null;
    }

    public MonsterData GetMonsterData(string monsterId)
    {
        foreach (var data in monsterData)
        {
            if (data.monsterId == monsterId)
            {
                return data;
            }
        }
        return null;
    }

    public FormData GetFormData(string formId)
    {
        foreach (var data in formData)
        {
            if (data.formId == formId)
            {
                return data;
            }
        }
        return null;
    }
}
