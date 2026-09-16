using UnityEngine;

public class PollutionSystem : SingletonBase<PollutionSystem>
{
    private bool isBurstMode;
    private string _lastTierLabel = "";

    protected override void Awake()
    {
        base.Awake();
    }

    public void ResetState()
    {
        isBurstMode = false;
        _lastTierLabel = "";
    }

    public void GainPollution(float amount)
    {
        var player = GameManager.Instance.Player;
        string oldTier = PollutionPassiveSystem.GetPollutionTier(player.pollution).label;

        player.pollution = Mathf.Clamp(player.pollution + amount, 0f, GetMaxPollution());

        if (player.pollution > player.maxPollutionReached)
            player.maxPollutionReached = player.pollution;

        EventBus.Emit(EventTypes.PollutionChanged, player.pollution);
        EventBus.Emit(EventTypes.PlayerStatsChanged);
        MetaProgressSystem.Instance?.OnPollution(Mathf.RoundToInt(player.pollution));
        CheckBurstMode();
        CheckTierTransition(oldTier, player.pollution);

        if (player.pollution >= GetMaxPollution())
        {
            OnPollutionMaxed();
        }
    }

    public void ReducePollution(float amount)
    {
        var player = GameManager.Instance.Player;
        string oldTier = PollutionPassiveSystem.GetPollutionTier(player.pollution).label;

        player.pollution = Mathf.Clamp(player.pollution - amount, 0f, GetMaxPollution());
        EventBus.Emit(EventTypes.PollutionChanged, player.pollution);
        EventBus.Emit(EventTypes.PlayerStatsChanged);
        CheckTierTransition(oldTier, player.pollution);

        if (isBurstMode && player.pollution < 70f)
        {
            ExitBurstMode();
        }
    }

    void CheckTierTransition(string oldTier, float newPollution)
    {
        var newInfo = PollutionPassiveSystem.GetPollutionTier(newPollution);
        if (oldTier != newInfo.label)
        {
            string powerMsg = "";
            if (newInfo.atkMult > 1f)
                powerMsg += $"攻击提升 {Mathf.RoundToInt((newInfo.atkMult - 1f) * 100)}%";
            if (newInfo.defMult < 1f)
                powerMsg += (powerMsg.Length > 0 ? " | " : "") + $"防御降低 {Mathf.RoundToInt((1f - newInfo.defMult) * 100)}%";

            string msg = $"<color=#{ColorUtility.ToHtmlStringRGB(newInfo.color)}>{newInfo.icon} 污染阶段: {newInfo.label}</color>";
            
            if (newInfo.label == "侵蚀")
                msg += " <color=#ffaa00>污染正在侵蚀你，也在强化你...</color>";
            else if (newInfo.label == "临界")
                msg += " <color=#ff6666>力量正在失控边缘爆发！</color>";
            else if (newInfo.label == "爆发")
                msg += " <color=#ff006e>你离失控更近了一步，但力量也更接近极限！</color>";

            if (powerMsg.Length > 0)
                msg += $" <color=#ffffff>[{powerMsg}]</color>";

            CompleteGameSystem.Instance?.AddCombatLog(msg);
            CanvasUIManager.Instance?.ShowFlashBanner($"{newInfo.icon} {newInfo.label} — {powerMsg}", newInfo.color, 2f);

            if (System.Array.IndexOf(new[] { "侵蚀", "临界", "爆发" }, newInfo.label) >=
                System.Array.IndexOf(new[] { "侵蚀", "临界", "爆发" }, oldTier))
                EventBus.Emit(EventTypes.PollutionTierUp);
            else
                EventBus.Emit(EventTypes.PollutionTierDown);
        }

        ShowPollutionRiskReward(newPollution);
    }

    void ShowPollutionRiskReward(float pollution)
    {
        bool isFirstRun = PlayerPrefs.GetInt("FirstRunComplete", 0) == 0;
        if (!isFirstRun) return;

        float[] thresholds = { 30f, 50f, 70f, 85f };
        string[] messages = {
            "污染30%: 小幅伤害加成开始显现",
            "污染50%: 伤害+15% | 防御-5%",
            "污染70%: 伤害+25% | 防御-10% | 即将进入爆发模式",
            "污染85%: 伤害+40% | 防御-20% | 极度危险！"
        };

        for (int i = 0; i < thresholds.Length; i++)
        {
            if (pollution >= thresholds[i] && pollution < thresholds[i] + 5)
            {
                bool shown = PlayerPrefs.GetInt($"PollutionHint_{i}", 0) == 1;
                if (!shown)
                {
                    PlayerPrefs.SetInt($"PollutionHint_{i}", 1);
                    PlayerPrefs.Save();
                    
                    Color hintColor = i < 2 ? new Color(0.5f, 0.9f, 0.5f) : 
                                      i < 3 ? new Color(0.9f, 0.8f, 0.3f) : 
                                      new Color(0.9f, 0.3f, 0.3f);
                    
                    CompleteGameSystem.Instance?.AddCombatLog($"<color=#{ColorUtility.ToHtmlStringRGB(hintColor)}>◆ 污染提示: {messages[i]}</color>");
                    CanvasUIManager.Instance?.ShowFlashBanner($"◆ {messages[i]}", hintColor, 3f);
                }
            }
        }
    }

    float GetMaxPollution()
    {
        int evoLevel = CompleteGameSystem.Instance?.EvolutionLevel ?? 0;
        var player = GameManager.Instance?.Player;
        string evoClass = player?.selectedClass ?? "";
        if (evoClass == "titan" && evoLevel >= 4)
            return 110f;
        return 100f;
    }

    private void CheckBurstMode()
    {
        var player = GameManager.Instance.Player;
        var config = ConfigManager.Instance.GetGameConfig();

        if (!isBurstMode && player.pollution >= config.pollutionThreshold)
        {
            EnterBurstMode();
        }
    }

    private void EnterBurstMode()
    {
        isBurstMode = true;
    }

    private void ExitBurstMode()
    {
        isBurstMode = false;
    }

    private void OnPollutionMaxed()
    {
        var player = GameManager.Instance.Player;
        if (player.collapseResistCharges > 0)
        {
            player.collapseResistCharges--;
            player.pollution = 70f;
            EventBus.Emit(EventTypes.PollutionChanged, player.pollution);
            EventBus.Emit(EventTypes.PlayerStatsChanged);
            return;
        }

        player.hp = Mathf.Max(1, Mathf.RoundToInt(player.maxHp * 0.3f));
        player.pollution = 70f;
        EventBus.Emit(EventTypes.PollutionChanged, player.pollution);
        EventBus.Emit(EventTypes.PlayerStatsChanged);
    }

    public bool IsBurstModeActive()
    {
        return isBurstMode;
    }

    public void AddPollution(float amount)
    {
        GainPollution(amount);
    }

    public void RemovePollution(float amount)
    {
        ReducePollution(amount);
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }
}
