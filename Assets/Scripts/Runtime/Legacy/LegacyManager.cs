using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class LegacyManager : MonoBehaviour
{
    private static LegacyManager _instance;
    public static LegacyManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType<LegacyManager>();
                if (_instance == null)
                {
                    var go = new GameObject("LegacyManager");
                    _instance = go.AddComponent<LegacyManager>();
                    DontDestroyOnLoad(go);
                }
            }
            return _instance;
        }
    }

    public const int BASE_MAX_LEGACIES = 2;

    private List<LegacyAbility> _equippedLegacies = new List<LegacyAbility>();
    private Dictionary<string, LegacyAbility> _activeEffects = new Dictionary<string, LegacyAbility>();
    private LegacyTemplate[] _templates;
    private float _pollutionModifier = 1f;

    [Serializable]
    public class LegacyTemplate
    {
        public string sourceTrait;
        public string sourceType;
        public string name;
        public string icon;
        public string effectType;
        public float baseValue;
        public string baseFormula;
        public string desc;
    }

    [Serializable]
    private class TemplateArrayWrapper { public LegacyTemplate[] items; }

    void Awake()
    {
        if (_instance != null && _instance != this)
        {
            Destroy(gameObject);
            return;
        }
        _instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void Initialize()
    {
        LoadTemplates();
        ClearLegacies();

    }

    void LoadTemplates()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Config", "LegacyTemplateConfig.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                string wrapped = "{\"items\":" + json + "}";
                var wrapper = JsonUtility.FromJson<TemplateArrayWrapper>(wrapped);
                _templates = wrapper?.items ?? new LegacyTemplate[0];
            }
            else
            {
                _templates = new LegacyTemplate[0];

            }
        }
        catch (Exception e)
        {
            _templates = new LegacyTemplate[0];

        }
    }

    #region Hooks
    public void SubscribeHooks()
    {
        UnsubscribeHooks();
        EventBus.Register(EventTypes.RunStart, OnRunStart);
        EventBus.Register<string>(EventTypes.FormReplaced, OnFormReplaced);
        EventBus.Register<float>(EventTypes.PollutionChanged, OnPollutionChanged);

    }

    public void UnsubscribeHooks()
    {
        EventBus.Unregister(EventTypes.RunStart, OnRunStart);
        EventBus.Unregister<string>(EventTypes.FormReplaced, OnFormReplaced);
        EventBus.Unregister<float>(EventTypes.PollutionChanged, OnPollutionChanged);
    }

    void OnRunStart()
    {
        ClearLegacies();
        _pollutionModifier = 1f;
    }

    void OnFormReplaced(string oldFormId)
    {
        if (string.IsNullOrEmpty(oldFormId)) return;
        if (oldFormId == "human") return;

        var candidates = ExtractLegaciesFromForm(oldFormId);
        if (candidates.Count > 0)
        {
            if (GetLegacyCount() == 0)
            {
                CompleteGameSystem.Instance?.AddCombatLog("<color=#ffcc00>◆ 旧形态留下了遗产！选择一个强化自己！</color>");
                candidates = FilterFirstLegacyChoice(candidates);
            }
            EventBus.Emit(EventTypes.LegacySelection, candidates, oldFormId);
        }
    }

    List<LegacyAbility> FilterFirstLegacyChoice(List<LegacyAbility> candidates)
    {
        var result = new List<LegacyAbility>();

        bool isFirstRun = PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
        if (isFirstRun)
        {
            var attackBonus = candidates.FirstOrDefault(c => c.effectType == "AttackBonus");
            var defenseBonus = candidates.FirstOrDefault(c => c.effectType == "DefenseBonus");
            var lifesteal = candidates.FirstOrDefault(c => c.effectType == "lifesteal");
            var regen = candidates.FirstOrDefault(c => c.effectType == "regen");
            var hpBonus = candidates.FirstOrDefault(c => c.effectType == "MaxHpBonus");

            if (attackBonus != null)
            {
                attackBonus.name = "锋利爪牙";
                attackBonus.description = $"攻击力 +{Mathf.RoundToInt(attackBonus.effectValue)}";
                result.Add(attackBonus);
            }
            else
            {
                result.Add(CreateSimpleLegacy("攻击", "攻击力+5", "AttackBonus", 5, candidates));
            }

            if (defenseBonus != null)
            {
                defenseBonus.name = "坚硬外皮";
                defenseBonus.description = $"防御力 +{Mathf.RoundToInt(defenseBonus.effectValue)}";
                result.Add(defenseBonus);
            }
            else
            {
                result.Add(CreateSimpleLegacy("防御", "防御力+3", "DefenseBonus", 3, candidates));
            }

            if (regen != null)
            {
                regen.name = "快速愈合";
                regen.description = $"每回合恢复 {Mathf.RoundToInt(regen.effectValue * 100)}% HP";
                result.Add(regen);
            }
            else if (lifesteal != null)
            {
                lifesteal.name = "嗜血本能";
                lifesteal.description = $"攻击恢复 {Mathf.RoundToInt(lifesteal.effectValue * 100)}% 伤害";
                result.Add(lifesteal);
            }
            else if (hpBonus != null)
            {
                hpBonus.name = "强壮体魄";
                hpBonus.description = $"最大生命 +{Mathf.RoundToInt(hpBonus.effectValue)}";
                result.Add(hpBonus);
            }
            else
            {
                result.Add(CreateSimpleLegacy("恢复", "每回合恢复5%HP", "regen", 0.05f, candidates));
            }

            return result;
        }

        var attack = candidates.FirstOrDefault(c => c.effectType == "AttackBonus");
        var defense = candidates.FirstOrDefault(c => c.effectType == "DefenseBonus");
        var life = candidates.FirstOrDefault(c => c.effectType == "lifesteal") ?? candidates.FirstOrDefault(c => c.effectType == "regen");

        if (attack != null)
        {
            attack.name = "攻击强化";
            attack.description = "攻击力+" + Mathf.RoundToInt(attack.effectValue);
            result.Add(attack);
        }

        if (life != null)
        {
            life.name = life.effectType == "regen" ? "生命恢复" : "吸血";
            life.description = life.effectType == "regen" 
                ? "每回合恢复" + Mathf.RoundToInt(life.effectValue * 100) + "%HP"
                : "攻击时恢复" + Mathf.RoundToInt(life.effectValue * 100) + "%伤害";
            result.Add(life);
        }

        if (defense != null)
        {
            defense.name = "防御强化";
            defense.description = "防御力+" + Mathf.RoundToInt(defense.effectValue);
            result.Add(defense);
        }

        if (result.Count == 0)
            return candidates;

        while (result.Count < 3)
        {
            var remaining = candidates.Where(c => !result.Contains(c)).ToList();
            if (remaining.Count > 0)
                result.Add(remaining[0]);
            else
                break;
        }

        return result;
    }

    LegacyAbility CreateSimpleLegacy(string name, string desc, string effectType, float value, List<LegacyAbility> sourceCandidates)
    {
        string sourceMonsterId = sourceCandidates.Count > 0 ? sourceCandidates[0].sourceMonsterId : "unknown";
        string sourceMonsterName = sourceCandidates.Count > 0 ? sourceCandidates[0].sourceMonsterName : "未知";
        return new LegacyAbility(
            $"leg_simple_{effectType}", name, desc, "◆", effectType, value,
            sourceMonsterId, sourceMonsterName, 0
        );
    }

    void OnPollutionChanged(float pollution)
    {
        float oldMod = _pollutionModifier;
        if (pollution >= 85f)
            _pollutionModifier = 1.5f;
        else if (pollution >= 60f)
            _pollutionModifier = 1.25f;
        else
            _pollutionModifier = 1f;

        if (Mathf.Abs(oldMod - _pollutionModifier) > 0.01f)
            RecalculateAllEffects();
    }
    #endregion

    #region Slot Capacity
    public int GetMaxLegacies()
    {
        int evoLevel = CompleteGameSystem.Instance?.EvolutionLevel ?? 0;
        if (evoLevel >= 4) return BASE_MAX_LEGACIES + 2; // 4
        if (evoLevel >= 2) return BASE_MAX_LEGACIES + 1; // 3
        return BASE_MAX_LEGACIES; // 2
    }
    #endregion

    #region Extract Legacies
    public List<LegacyAbility> ExtractLegaciesFromForm(string formId)
    {
        var candidates = new List<LegacyAbility>();
        var monsterData = FindMonsterData(formId);
        if (string.IsNullOrEmpty(monsterData.id)) return candidates;

        int rarity = monsterData.zone >= 3 || monsterData.boss ? 2 :
                     monsterData.zone >= 2 ? 1 : 0;
        float rarityMult = rarity == 2 ? 1.5f : rarity == 1 ? 1.2f : 1f;

        // trait-based candidates from templates
        if (monsterData.traits != null)
        {
            foreach (var trait in monsterData.traits)
            {
                if (string.IsNullOrEmpty(trait)) continue;
                var tmpl = FindTemplate(trait);
                if (tmpl != null)
                {
                    float val = tmpl.baseValue * rarityMult;
                    candidates.Add(new LegacyAbility(
                        $"leg_{trait}_{formId}",
                        tmpl.name, tmpl.desc, tmpl.icon,
                        tmpl.effectType, val,
                        formId, monsterData.name, rarity
                    ));
                }
            }
        }

        // stat-based fallbacks
        if (monsterData.atk > 5)
        {
            float val = Mathf.CeilToInt(monsterData.atk * 0.12f * rarityMult);
            candidates.Add(new LegacyAbility(
                $"leg_atk_{formId}", "攻击传承",
                $"攻击+{val:F0}", "†", "AttackBonus", val,
                formId, monsterData.name, rarity
            ));
        }
        if (monsterData.def > 3)
        {
            float val = Mathf.CeilToInt(monsterData.def * 0.15f * rarityMult);
            candidates.Add(new LegacyAbility(
                $"leg_def_{formId}", "防御传承",
                $"防御+{val:F0}", "◈", "DefenseBonus", val,
                formId, monsterData.name, rarity
            ));
        }
        if (monsterData.hp > 30)
        {
            float val = Mathf.CeilToInt(monsterData.hp * 0.08f * rarityMult);
            candidates.Add(new LegacyAbility(
                $"leg_hp_{formId}", "生命传承",
                $"最大生命+{val:F0}", "♦", "MaxHpBonus", val,
                formId, monsterData.name, rarity
            ));
        }

        if (candidates.Count == 0) return candidates;

        // how many to show: Evo Lv4+ → 3, otherwise 2
        int evoLevel = CompleteGameSystem.Instance?.EvolutionLevel ?? 0;
        int showCount = evoLevel >= 4 ? 3 : 2;
        showCount = Mathf.Min(showCount, candidates.Count);

        // shuffle and pick
        for (int i = candidates.Count - 1; i > 0; i--)
        {
            int j = UnityEngine.Random.Range(0, i + 1);
            var tmp = candidates[i];
            candidates[i] = candidates[j];
            candidates[j] = tmp;
        }

        return candidates.GetRange(0, showCount);
    }

    LegacyTemplate FindTemplate(string traitName)
    {
        if (_templates == null) return null;
        return Array.Find(_templates, t => t.sourceTrait == traitName);
    }

    (string id, string name, int hp, int atk, int def, int zone, bool boss, Color color, string[] traits, string[] axes)
        FindMonsterData(string monsterId)
    {
        if (GameDataImporter.MonsterDefinitions == null)
            return default;
        return GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == monsterId);
    }
    #endregion

    #region Add / Replace / Remove
    public bool AddLegacy(LegacyAbility ability)
    {
        if (_equippedLegacies.Count >= GetMaxLegacies())
            return false;

        _equippedLegacies.Add(ability);
        ApplyLegacyEffect(ability);
        EventBus.Emit(EventTypes.LegacyAdded, ability);

        return true;
    }

    public bool ReplaceLegacy(int index, LegacyAbility newAbility)
    {
        if (index < 0 || index >= _equippedLegacies.Count)
            return false;

        var oldAbility = _equippedLegacies[index];
        RemoveLegacyEffect(oldAbility);
        _equippedLegacies[index] = newAbility;
        ApplyLegacyEffect(newAbility);
        EventBus.Emit(EventTypes.LegacyReplaced, index, newAbility);
        return true;
    }

    public void RemoveLegacy(int index)
    {
        if (index < 0 || index >= _equippedLegacies.Count) return;
        var ability = _equippedLegacies[index];
        RemoveLegacyEffect(ability);
        _equippedLegacies.RemoveAt(index);
        EventBus.Emit(EventTypes.LegacyRemoved, index);
    }
    #endregion

    #region Effects
    float GetEvoMultiplier()
    {
        int evoLevel = CompleteGameSystem.Instance?.EvolutionLevel ?? 0;
        if (evoLevel >= 4) return 1.25f;
        if (evoLevel >= 2) return 1.1f;
        return 1f;
    }

    float GetEffectiveValue(LegacyAbility ability)
    {
        return ability.baseEffectValue * _pollutionModifier * GetEvoMultiplier();
    }

    void ApplyLegacyEffect(LegacyAbility ability)
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return;

        ability.effectValue = GetEffectiveValue(ability);
        float v = ability.effectValue;

        switch (ability.effectType)
        {
            case "AttackBonus": player.attackBonus += v; break;
            case "DefenseBonus": player.defenseBonus += v; break;
            case "MaxHpBonus":
                int hpAdd = Mathf.CeilToInt(v);
                player.maxHp += hpAdd;
                player.hp += hpAdd;
                break;
            case "regen": player.regenPerTurn += v; break;
            case "poison_attack": player.poisonChance += v; break;
            case "lifesteal": player.lifesteal += v; break;
            case "crit_rate": player.critRate += v; break;
            case "DamageReduction": player.defenseBonus += v * 10; break;
            case "DamageReflect": player.defenseBonus += v * 5; break;
            case "Initiative": player.attackBonus += v * 5; break;
            case "LowHpAttackBonus": player.attackBonus += v * 3; break;
            case "Evasion": player.defenseBonus += v * 8; break;
        }

        _activeEffects[ability.id] = ability;
        GameManager.Instance?.NotifyPlayerStatsChanged();
    }

    void RemoveLegacyEffect(LegacyAbility ability)
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return;

        float v = ability.effectValue;

        switch (ability.effectType)
        {
            case "AttackBonus": player.attackBonus -= v; break;
            case "DefenseBonus": player.defenseBonus -= v; break;
            case "MaxHpBonus":
                int hpLoss = Mathf.CeilToInt(v);
                player.maxHp -= hpLoss;
                player.hp = Mathf.Min(player.hp, player.maxHp);
                break;
            case "regen": player.regenPerTurn -= v; break;
            case "poison_attack": player.poisonChance -= v; break;
            case "lifesteal": player.lifesteal -= v; break;
            case "crit_rate": player.critRate -= v; break;
            case "DamageReduction": player.defenseBonus -= v * 10; break;
            case "DamageReflect": player.defenseBonus -= v * 5; break;
            case "Initiative": player.attackBonus -= v * 5; break;
            case "LowHpAttackBonus": player.attackBonus -= v * 3; break;
            case "Evasion": player.defenseBonus -= v * 8; break;
        }

        _activeEffects.Remove(ability.id);
        GameManager.Instance?.NotifyPlayerStatsChanged();
    }

    void RecalculateAllEffects()
    {
        var player = GameManager.Instance?.Player;
        if (player == null) return;

        foreach (var ability in _equippedLegacies)
        {
            RemoveLegacyEffect(ability);
        }
        foreach (var ability in _equippedLegacies)
        {
            ApplyLegacyEffect(ability);
        }
    }

    void ClearLegacies()
    {
        foreach (var ability in _equippedLegacies)
            RemoveLegacyEffect(ability);
        _equippedLegacies.Clear();
        _activeEffects.Clear();
    }
    #endregion

    #region Public Query API
    public List<LegacyAbility> GetEquippedLegacies()
    {
        return new List<LegacyAbility>(_equippedLegacies);
    }

    public int GetLegacyCount() => _equippedLegacies.Count;

    public List<LegacyAbility> GetActiveLegacies() => new List<LegacyAbility>(_equippedLegacies);

    public bool HasEmptySlot() => _equippedLegacies.Count < GetMaxLegacies();

    public float GetActiveLegacyEffectValue(string effectType)
    {
        float total = 0;
        foreach (var leg in _equippedLegacies)
        {
            if (leg.effectType == effectType)
                total += leg.effectValue;
        }
        return total;
    }

    // Compat stubs for old cross-run API calls from UI code
    public void CheckUnlockConditions() { }
    public LegacyConfig[] GetAllConfigs() => new LegacyConfig[0];
    public List<string> GetEquippedIds()
    {
        return _equippedLegacies.Select(l => l.id).ToList();
    }
    public bool IsUnlocked(string id) => _equippedLegacies.Any(l => l.id == id);
    public bool IsEquipped(string id) => _equippedLegacies.Any(l => l.id == id);
    public List<LegacyConfig> GetUnlockedLegacies() => new List<LegacyConfig>();
    #endregion

    #region Save / Load (per-run, in SaveSystem)
    public string SerializeLegacies()
    {
        var wrapper = new LegacyArrayWrapper { legacies = _equippedLegacies.ToArray() };
        return JsonUtility.ToJson(wrapper);
    }

    public void RestoreLegacies(string json)
    {
        ClearLegacies();
        if (string.IsNullOrEmpty(json)) return;
        try
        {
            var wrapper = JsonUtility.FromJson<LegacyArrayWrapper>(json);
            if (wrapper?.legacies != null)
            {
                foreach (var leg in wrapper.legacies)
                {
                    _equippedLegacies.Add(leg);
                    ApplyLegacyEffect(leg);
                }
            }
        }
        catch (Exception e)
        {

        }
    }

    [Serializable]
    private class LegacyArrayWrapper { public LegacyAbility[] legacies; }
    #endregion

    #region Pollution Mutation
    public bool TryMutateLegacy(float pollution)
    {
        if (pollution < 85f || _equippedLegacies.Count == 0) return false;
        if (UnityEngine.Random.value > 0.2f) return false;

        int idx = UnityEngine.Random.Range(0, _equippedLegacies.Count);
        var leg = _equippedLegacies[idx];
        if (leg.isMutated) return false;

        string[] possibleTypes = { "AttackBonus", "DefenseBonus", "lifesteal", "crit_rate", "regen", "poison_attack" };
        string newType = possibleTypes[UnityEngine.Random.Range(0, possibleTypes.Length)];
        if (newType == leg.effectType) return false;

        RemoveLegacyEffect(leg);
        leg.isMutated = true;
        leg.originalEffectType = leg.effectType;
        leg.effectType = newType;
        leg.name = "◈ " + leg.name;
        ApplyLegacyEffect(leg);

        CompleteGameSystem.Instance?.AddCombatLog($"<color=#ff6600>※ 遗产腐蚀: {leg.name} 效果变异!</color>");
        CanvasUIManager.Instance?.ShowFlashBanner($"※ 遗产腐蚀!", new Color(1f, 0.4f, 0f), 1.2f);
        return true;
    }
    #endregion
}
