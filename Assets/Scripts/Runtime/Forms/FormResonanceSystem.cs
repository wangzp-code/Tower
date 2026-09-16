using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;
using System.Linq;

public class FormResonanceSystem : SingletonBase<FormResonanceSystem>
{
    [Serializable]
    public class ResonanceCombo
    {
        public string fromAxis;
        public string toAxis;
        public string name;
        public string effectType;
        public float value;
        public int duration;
        public string desc;
    }

    [Serializable]
    private class ComboArrayWrapper { public ResonanceCombo[] items; }

    public class ActiveComboEffect
    {
        public string effectType;
        public float value;
        public int turnsRemaining;
        public string comboName;
    }

    private ResonanceCombo[] _combos;
    private List<ActiveComboEffect> _activeEffects = new List<ActiveComboEffect>();
    private Dictionary<string, int> _formUseTurns = new Dictionary<string, int>();
    private Dictionary<string, float> _proficiencyBonus = new Dictionary<string, float>();
    private string _currentFormId;
    private const float PROFICIENCY_PER_STEP = 0.05f;
    private const float PROFICIENCY_MAX = 0.25f;
    private const int TURNS_PER_STEP = 3;

    protected override void Awake()
    {
        base.Awake();
    }

    public void Initialize()
    {
        LoadCombos();
        _activeEffects.Clear();
        _formUseTurns.Clear();
        _proficiencyBonus.Clear();
        _currentFormId = null;
    }

    void LoadCombos()
    {
        try
        {
            string path = Path.Combine(Application.streamingAssetsPath, "Config", "FormResonanceConfig.json");
            if (File.Exists(path))
            {
                string json = File.ReadAllText(path);
                string wrapped = "{\"items\":" + json + "}";
                var wrapper = JsonUtility.FromJson<ComboArrayWrapper>(wrapped);
                _combos = wrapper?.items ?? new ResonanceCombo[0];
            }
            else
            {
                _combos = new ResonanceCombo[0];
            }
        }
        catch (Exception e)
        {
            _combos = new ResonanceCombo[0];

        }
    }

    public void OnFormSwitch(string oldFormId, string newFormId)
    {
        _currentFormId = newFormId;

        bool isFirstRun = PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
        int currentFloor = GameManager.Instance?.CurrentFloor ?? 0;

        if (isFirstRun && currentFloor == 4)
        {
            ForceFirstRunResonance(oldFormId, newFormId);
            return;
        }

        string[] oldAxes = GetFormAxes(oldFormId);
        string[] newAxes = GetFormAxes(newFormId);

        ResonanceCombo bestCombo = null;
        if (oldAxes != null && newAxes != null && _combos != null)
        {
            foreach (var combo in _combos)
            {
                bool fromMatch = Array.Exists(oldAxes, a => a == combo.fromAxis);
                bool toMatch = Array.Exists(newAxes, a => a == combo.toAxis);
                if (fromMatch && toMatch)
                {
                    bestCombo = combo;
                    break;
                }
            }
        }

        if (bestCombo != null)
        {
            ApplyComboEffect(bestCombo);
        }
        else
        {
            ApplyDefaultSwitchShield();
        }
    }

    void ForceFirstRunResonance(string oldFormId, string newFormId)
    {
        if (_combos == null || _combos.Length == 0)
        {
            ApplyDefaultSwitchShield();
            return;
        }

        ResonanceCombo bestCombo = null;
        float bestScore = -1;

        string[] oldAxes = GetFormAxes(oldFormId);
        string[] newAxes = GetFormAxes(newFormId);

        if (oldAxes != null && newAxes != null)
        {
            foreach (var combo in _combos)
            {
                bool fromMatch = Array.Exists(oldAxes, a => a == combo.fromAxis);
                bool toMatch = Array.Exists(newAxes, a => a == combo.toAxis);
                float score = (fromMatch ? 1 : 0) + (toMatch ? 1 : 0);
                if (score > bestScore)
                {
                    bestScore = score;
                    bestCombo = combo;
                }
            }
        }

        if (bestCombo == null)
        {
            bestCombo = _combos[UnityEngine.Random.Range(0, _combos.Length)];
        }

        CompleteGameSystem.Instance?.AddCombatLog("<color=#ffd700>◆◆◆ 暗塔共鸣触发！ ◆◆◆</color>");
        ApplyComboEffect(bestCombo);
    }

    void ApplyComboEffect(ResonanceCombo combo)
    {
        int evoLevel = CompleteGameSystem.Instance?.EvolutionLevel ?? 0;
        float valueMult = evoLevel >= 5 ? 1.5f : 1f;
        int durationBonus = evoLevel >= 3 ? 1 : 0;

        // 污染增强
        float pollution = GameManager.Instance?.Player?.pollution ?? 0;
        if (pollution >= 85f)
        {
            valueMult *= 1.5f;
            durationBonus += 1;
            if (UnityEngine.Random.value < 0.15f)
            {
                int recoil = Mathf.CeilToInt(GameManager.Instance.Player.maxHp * 0.05f);
                GameManager.Instance.Player.hp -= recoil;
                CompleteGameSystem.Instance?.AddCombatLog($"<color=#ff4444>共鸣反噬 -{recoil}HP</color>");
            }
        }
        else if (pollution >= 60f)
        {
            durationBonus += 1;
        }

        // 遗产协同
        float legacyMult = 1f;
        float legacyMatch = LegacyManager.Instance?.GetActiveLegacyEffectValue(combo.effectType) ?? 0;
        if (legacyMatch > 0) legacyMult = 1.5f;

        float finalValue = combo.value * valueMult * legacyMult;
        int finalDuration = combo.duration + durationBonus;

        if (combo.effectType == "pollution_damage")
        {
            int dmg = Mathf.FloorToInt(pollution * finalValue);
            CompleteGameSystem.Instance?.ApplyDamageToEnemy(dmg);
            CompleteGameSystem.Instance?.AddCombatLog($"<color=#00ccff>◈ {combo.name}: 造成{dmg}点污染伤害</color>");
            return;
        }

        if (combo.effectType == "regen_burst")
        {
            var player = GameManager.Instance?.Player;
            if (player != null)
            {
                int heal = Mathf.CeilToInt(player.maxHp * finalValue);
                player.hp = Mathf.Min(player.hp + heal, player.maxHp);
                CompleteGameSystem.Instance?.AddCombatLog($"<color=#88ff88>◈ {combo.name}: 回复{heal}HP</color>");
            }
            return;
        }

        if (finalDuration > 0)
        {
            _activeEffects.Add(new ActiveComboEffect
            {
                effectType = combo.effectType,
                value = finalValue,
                turnsRemaining = finalDuration,
                comboName = combo.name
            });
        }

        string legacyInfo = legacyMult > 1f ? " [遗产共鸣!]" : "";
        CompleteGameSystem.Instance?.AddCombatLog($"<color=#00ccff>◈ {combo.name}: {combo.desc} ({finalDuration}回合){legacyInfo}</color>");
        CompleteGameSystem.Instance?.OnResonanceTriggered(combo.name);
        CanvasUIManager.Instance?.ShowFlashBanner($"◈ {combo.name}", new Color(0f, 0.8f, 1f), 1.0f);
        ScreenEffectsManager.Instance?.FlashResonance();
    }

    void ApplyDefaultSwitchShield()
    {
        _activeEffects.Add(new ActiveComboEffect
        {
            effectType = "shield",
            value = 1,
            turnsRemaining = 1,
            comboName = "切换护盾"
        });
        CompleteGameSystem.Instance?.AddCombatLog("<color=#aaaaff>◇ 形态切换: 1回合护盾</color>");
    }

    public void OnTurnEnd()
    {
        // tick active effects
        for (int i = _activeEffects.Count - 1; i >= 0; i--)
        {
            _activeEffects[i].turnsRemaining--;
            if (_activeEffects[i].turnsRemaining <= 0)
                _activeEffects.RemoveAt(i);
        }

        // proficiency tracking
        if (!string.IsNullOrEmpty(_currentFormId))
        {
            if (!_formUseTurns.ContainsKey(_currentFormId))
                _formUseTurns[_currentFormId] = 0;
            _formUseTurns[_currentFormId]++;

            if (_formUseTurns[_currentFormId] % TURNS_PER_STEP == 0)
            {
                if (!_proficiencyBonus.ContainsKey(_currentFormId))
                    _proficiencyBonus[_currentFormId] = 0;
                if (_proficiencyBonus[_currentFormId] < PROFICIENCY_MAX)
                {
                    _proficiencyBonus[_currentFormId] += PROFICIENCY_PER_STEP;
                    CompleteGameSystem.Instance?.AddCombatLog($"<color=#aaddff>◎ 形态熟练度提升 +{_proficiencyBonus[_currentFormId] * 100:F0}%</color>");
                }
            }
        }
    }

    #region Query API
    public float GetProficiencyBonus(string formId)
    {
        if (string.IsNullOrEmpty(formId)) return 0;
        return _proficiencyBonus.TryGetValue(formId, out float v) ? v : 0;
    }

    public bool HasActiveEffect(string effectType)
    {
        return _activeEffects.Any(e => e.effectType == effectType);
    }

    public float GetActiveEffectValue(string effectType)
    {
        float total = 0;
        foreach (var e in _activeEffects)
        {
            if (e.effectType == effectType)
                total += e.value;
        }
        return total;
    }

    public List<ActiveComboEffect> GetAllActiveEffects()
    {
        return new List<ActiveComboEffect>(_activeEffects);
    }

    public Dictionary<string, float> GetAllProficiencies()
    {
        return new Dictionary<string, float>(_proficiencyBonus);
    }

    public void ClearEffects()
    {
        _activeEffects.Clear();
    }

    public ResonanceCombo GetPreviewResonance(string fromFormId, string toFormId)
    {
        if (_combos == null || _combos.Length == 0) return null;
        
        string[] fromAxes = GetFormAxes(fromFormId);
        string[] toAxes = GetFormAxes(toFormId);
        
        if (fromAxes == null || toAxes == null) return null;
        
        foreach (var combo in _combos)
        {
            bool fromMatch = Array.Exists(fromAxes, a => a == combo.fromAxis);
            bool toMatch = Array.Exists(toAxes, a => a == combo.toAxis);
            if (fromMatch && toMatch)
            {
                return combo;
            }
        }
        
        return null;
    }

    public string GetActiveEffectsSummary()
    {
        if (_activeEffects.Count == 0) return "";
        var effects = new List<string>();
        foreach (var e in _activeEffects)
        {
            string valueStr = e.value >= 1 ? $"{e.value:F0}" : $"{e.value * 100:F0}%";
            effects.Add($"{e.comboName}({valueStr}, {e.turnsRemaining}回合)");
        }
        return string.Join(" · ", effects);
    }
    #endregion

    string[] GetFormAxes(string formId)
    {
        if (GameDataImporter.MonsterDefinitions == null) return null;
        var monster = GameDataImporter.MonsterDefinitions.FirstOrDefault(m => m.id == formId);
        return !string.IsNullOrEmpty(monster.id) ? monster.axes : null;
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
        _activeEffects.Clear();
        _formUseTurns.Clear();
        _proficiencyBonus.Clear();
        _currentFormId = null;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        _activeEffects.Clear();
    }
}
