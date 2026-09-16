using UnityEngine;
using System.Collections.Generic;

public class TutorialManager : SingletonBase<TutorialManager>
{
    private List<CompleteGameSystem.TutorialStep> tutorialSteps = new List<CompleteGameSystem.TutorialStep>();
    private int currentStepIndex = 0;
    private bool isTutorialActive = false;
    private bool isTutorialCompleted = false;
    private int currentStage = 0;
    private bool isForceTutorial = true;

    private int _attackCount;
    private int _possessCount;
    private int _defendCount;
    private int _killCount;
    private int _maxComboShown;
    private bool _evolutionHintShown;
    private bool _defendHintShown;
    private bool _pollutionHintShown;
    private readonly Dictionary<string, float> _hintCooldowns = new Dictionary<string, float>();

    public delegate void TutorialStepChanged(CompleteGameSystem.TutorialStep step);
    public event TutorialStepChanged OnTutorialStepChanged;

    public delegate void TutorialCompleted();
    public event TutorialCompleted OnTutorialCompleted;

    public delegate void TutorialStageUp(int stage);
    public event TutorialStageUp OnTutorialStageUp;

    private void Awake()
    {
        base.Awake();
        InitializeTutorialSteps();
        EventBus.Register<float>(EventTypes.PollutionChanged, OnPollutionChangedEvent);
    }

    private void OnPollutionChangedEvent(float pollution)
    {
        OnPollutionChanged(pollution);
    }

    private void InitializeTutorialSteps()
    {
        AddStep("move", "移动探索", "使用方向键探索地图，周围区域会逐渐显露",
            "◎", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 0, true, "");

        AddStep("attack", "战斗", "遭遇怪物时点击攻击按钮开战",
            "⚔", CompleteGameSystem.TutorialType.Action, CompleteGameSystem.TutorialAction.Attack, 0, true, "");

        AddStep("possess", "附身", "击败虚弱怪物可附身获取新身体\n被附身者HP<50%时出现意识裂隙",
            "◆", CompleteGameSystem.TutorialType.Action, CompleteGameSystem.TutorialAction.Possess, 1, true, "");

        AddStep("new_body", "新身体", "你现在拥有新形态了！\n形态栏可随时切换回原形态",
            "★", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 2, true, "");

        AddStep("form", "形态切换", "顶部形态栏可切换已获得形态",
            "◇", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 2, false, "");

        AddStep("defend", "防御", "面对强敌时点击防御\n防御=减半伤害+回少量HP",
            "🛡", CompleteGameSystem.TutorialType.Action, CompleteGameSystem.TutorialAction.Defend, 2, true, "");

        AddStep("inspect", "查看怪物", "长按地图怪物可查看属性",
            "ⓘ", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 2, false, "");

        AddStep("anchor", "记忆锚定", "点击顶部锚点栏查看锚点\n祭坛可消耗200EP固化记忆",
            "⚓", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 2, false, "");

        AddStep("evolution", "进化", "菜单中进化系统已解锁\n消耗EP强化你的能力",
            "✦", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.Blessing, 3, true, "");

        AddStep("ultimate", "终极技能", "右下技能球已就绪\n危急时刻使用可扭转战局",
            "💫", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 3, false, "");

        AddStep("shop", "商店", "残雪商店已开放\n消耗EP购买补给和强化",
            "🛒", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 4, false, "");

        AddStep("pollution", "污染", ">80%异变 / 100%失控\n用净化道具压制",
            "☢", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 4, false, "");

        AddStep("signature", "楼层签名", "每层有特殊规则\n注意状态栏的签名提示",
            "📜", CompleteGameSystem.TutorialType.Info, CompleteGameSystem.TutorialAction.None, 4, false, "");
    }

    private void AddStep(string id, string title, string desc, string icon,
        CompleteGameSystem.TutorialType type, CompleteGameSystem.TutorialAction action, int stage, bool required, string btnText)
    {
        tutorialSteps.Add(new CompleteGameSystem.TutorialStep
        {
            id = id, title = title, desc = desc, icon = icon,
            type = type, actionType = action, stage = stage,
            required = required, nextButtonText = btnText
        });
    }

    public void StartTutorial()
    {
        if (isTutorialCompleted) return;
        isTutorialActive = true;
        currentStepIndex = 0;
        currentStage = 0;
        ShowStepAsToast(GetCurrentStep());
    }

    public void NotifyTutorialShown(int stage, string stepId)
    {
        if (isTutorialCompleted) return;
        isTutorialActive = true;
        if (stage > currentStage) currentStage = stage;
        int idx = tutorialSteps.FindIndex(s => s.id == stepId);
        if (idx >= 0 && idx > currentStepIndex) currentStepIndex = idx;
    }

    public void NotifyStageAdvanced(int stage)
    {
        if (stage > currentStage)
        {
            currentStage = stage;
            if (isTutorialActive)
                OnTutorialStageUp?.Invoke(currentStage);
        }
    }

    public void NotifyTutorialDismissed()
    {
        if (!isTutorialActive && isTutorialCompleted) return;
        isTutorialActive = false;
    }

    public void NotifyTutorialCompleted()
    {
        isTutorialActive = false;
        isTutorialCompleted = true;
        OnTutorialCompleted?.Invoke();
        SaveTutorialProgress();
    }

    public void ShowTutorialToast(CompleteGameSystem.TutorialStep step)
    {
        if (step == null || isTutorialCompleted) return;
        isTutorialActive = true;
        
        string message = $"【{step.title}】{step.desc.Replace("\n", " ")}";
        ShowContextHint(message, 3f);
        
        // 显示按钮引导
        ShowButtonGuide(step);
        
        OnTutorialStepChanged?.Invoke(step);
    }
    
    private void ShowButtonGuide(CompleteGameSystem.TutorialStep step)
    {
        if (step == null) return;
        
        // 映射步骤到按钮
        string buttonId = GetButtonIdForStep(step.id);
        if (string.IsNullOrEmpty(buttonId)) return;
        
        string guideMessage = GetGuideMessageForStep(step);
        
        // 通过事件触发UI引导
        EventBus.Emit(EventTypes.ShowButtonGuide, buttonId, guideMessage);
    }
    
    private string GetButtonIdForStep(string stepId)
    {
        switch (stepId)
        {
            case "move":
                return "dpad";
            case "attack":
                return "attack";
            case "possess":
                return "possess";
            case "new_body":
                return "attack";
            case "form":
                return "fslot-0";
            case "defend":
                return "defend";
            case "evolution":
                return "evolution";
            case "ultimate":
                return "btn-ultimate";
            case "shop":
                return "save";
            case "anchor":
                return "anchor-bar";
            case "signature":
                return "menu";
            default:
                return null;
        }
    }
    
    private string GetGuideMessageForStep(CompleteGameSystem.TutorialStep step)
    {
        switch (step.id)
        {
            case "move":
                return "使用方向键移动探索";
            case "attack":
                return "点击此处进行攻击";
            case "possess":
                return "虚弱时可附身获取新身体";
            case "new_body":
                return "你现在拥有新形态了！";
            case "form":
                return "点击形态栏切换身体";
            case "defend":
                return "防御可减半伤害+回HP";
            case "evolution":
                return "菜单中可打开进化系统";
            case "ultimate":
                return "危急时使用终极技能";
            case "shop":
                return "存档可回到基地补给";
            case "anchor":
                return "点击锚点栏查看记忆锚定";
            case "signature":
                return "菜单查看楼层规则";
            default:
                return step.title;
        }
    }

    private void ShowStepAsToast(CompleteGameSystem.TutorialStep step)
    {
        if (step == null || isTutorialCompleted) return;
        
        string message = $"【{step.title}】{step.desc.Replace("\n", " ")}";
        ShowContextHint(message, 3f);
        OnTutorialStepChanged?.Invoke(step);
    }

    public void AdvanceToNextStep()
    {
        if (!isTutorialActive) return;

        CompleteGameSystem.TutorialStep currentStep = GetCurrentStep();
        if (currentStep != null && currentStep.required)
        {
            int nextStage = currentStep.stage + 1;
            if (nextStage > currentStage)
            {
                currentStage = nextStage;
                OnTutorialStageUp?.Invoke(currentStage);
                AnnounceStageUp(currentStage);
            }
        }

        currentStepIndex++;

        if (currentStepIndex >= tutorialSteps.Count)
        {
            CompleteTutorial();
        }
        else
        {
            ShowStepAsToast(GetCurrentStep());
        }
    }

    public void AdvanceToStep(string stepId)
    {
        int idx = tutorialSteps.FindIndex(s => s.id == stepId);
        if (idx < 0) return;
        if (idx <= currentStepIndex) return;

        int targetStage = tutorialSteps[idx].stage;
        if (targetStage > currentStage)
        {
            currentStage = targetStage;
            OnTutorialStageUp?.Invoke(currentStage);
        }
        currentStepIndex = idx;
        ShowStepAsToast(GetCurrentStep());
    }

    private void AnnounceStageUp(int stage)
    {
        string[] stageMessages = {
            "",
            "✓ 觉醒阶段完成",
            "✓ 寄生阶段完成",
            "✓ 战术阶段完成",
            "✓ 成长阶段完成"
        };
        if (stage > 0 && stage < stageMessages.Length)
            ShowContextHint(stageMessages[stage], 2.5f);
    }

    public void SkipTutorial()
    {
        CompleteTutorial();
    }

    private void CompleteTutorial()
    {
        isTutorialActive = false;
        isTutorialCompleted = true;
        OnTutorialCompleted?.Invoke();
        SaveTutorialProgress();
        ShowContextHint("✓ 新手引导完成！", 3f);
    }

    public CompleteGameSystem.TutorialStep GetCurrentStep()
    {
        if (currentStepIndex >= 0 && currentStepIndex < tutorialSteps.Count)
            return tutorialSteps[currentStepIndex];
        return null;
    }

    public void HandleAction(CompleteGameSystem.TutorialAction action)
    {
        if (!isTutorialActive) return;

        CompleteGameSystem.TutorialStep currentStep = GetCurrentStep();
        if (currentStep != null && currentStep.type == CompleteGameSystem.TutorialType.Action)
        {
            if (currentStep.actionType == action)
                AdvanceToNextStep();
        }
    }

    public void RecordAction(CompleteGameSystem.TutorialAction action)
    {
        switch (action)
        {
            case CompleteGameSystem.TutorialAction.Attack: _attackCount++; break;
            case CompleteGameSystem.TutorialAction.Possess: _possessCount++; break;
            case CompleteGameSystem.TutorialAction.Defend: _defendCount++; break;
        }

        if (isTutorialActive || isTutorialCompleted)
            CheckActionHint(action);
    }

    public void RecordKill()
    {
        _killCount++;
    }

    public void RecordCombo(int combo)
    {
        if (combo > _maxComboShown && combo >= 3)
        {
            _maxComboShown = combo;
            if (isTutorialActive || isTutorialCompleted)
                ShowContextHint($"连击 {combo}！继续保持！", 1.5f);
        }
    }

    public void OnFloorEnter(int floor)
    {
        if (isTutorialCompleted) return;

        if (floor >= 2 && currentStage < 2 && !_defendHintShown)
        {
            _defendHintShown = true;
            if (isTutorialActive)
                AdvanceToStep("defend");
        }
        if (floor >= 3 && currentStage < 3)
        {
            if (isTutorialActive)
                AdvanceToStep("evolution");
        }
        if (floor >= 5 && currentStage < 4)
        {
            if (isTutorialActive)
                AdvanceToStep("signature");
        }
    }

    public void OnEvolutionPointsGained(int totalEp)
    {
        if (!_evolutionHintShown && totalEp >= 20)
        {
            _evolutionHintShown = true;
            if (isTutorialActive && currentStage < 3)
            {
                AdvanceToStep("evolution");
            }
        }
    }

    public void OnPollutionChanged(float pollution)
    {
        if (!_pollutionHintShown && pollution >= 50f)
        {
            _pollutionHintShown = true;
            if (isTutorialCompleted || !isTutorialActive || currentStage >= 2)
                ShowContextHint("污染警告！注意控制腐蚀值", 3f);
        }
    }

    public void ShowContextHint(string message, float duration = 2f)
    {
        if (string.IsNullOrEmpty(message)) return;
        if (_hintCooldowns.TryGetValue(message, out float lastTime) && Time.time < lastTime)
            return;
        _hintCooldowns[message] = Time.time + 10f;
        EventBus.Emit(EventTypes.ShowHint, message);
    }

    private void CheckActionHint(CompleteGameSystem.TutorialAction action)
    {
        switch (action)
        {
            case CompleteGameSystem.TutorialAction.Defend:
                ShowContextHint("防御成功！善用防御可减半伤害", 2f);
                break;
        }
    }

    public float GetStageProgress(int stage)
    {
        int stageSteps = 0;
        int completedInStage = 0;
        for (int i = 0; i < tutorialSteps.Count; i++)
        {
            if (tutorialSteps[i].stage == stage)
            {
                stageSteps++;
                if (i < currentStepIndex) completedInStage++;
            }
        }
        return stageSteps > 0 ? (float)completedInStage / stageSteps : 0f;
    }

    public int GetTotalSteps => tutorialSteps.Count;
    public int GetCurrentStepIndex => currentStepIndex;

    public void CheckAndShowTutorial(CompleteGameSystem.TutorialAction triggerAction)
    {
        if (!isTutorialActive || isTutorialCompleted) return;

        for (int i = currentStepIndex; i < tutorialSteps.Count; i++)
        {
            CompleteGameSystem.TutorialStep step = tutorialSteps[i];
            if (step.stage > currentStage) continue;
            if (step.type == CompleteGameSystem.TutorialType.Action && step.actionType == triggerAction)
            {
                currentStepIndex = i;
                ShowStepAsToast(step);
                return;
            }
        }
    }

    public bool IsTutorialActive() => isTutorialActive;
    public bool IsTutorialCompleted() => isTutorialCompleted;
    public int GetCurrentStage() => currentStage;
    public bool IsForceTutorial() => isForceTutorial;

    public void SetForceTutorial(bool value) => isForceTutorial = value;

    public void LoadTutorialProgress()
    {
        isTutorialCompleted = SaveSystem.Instance.LoadTutorialCompleted();
    }

    private void SaveTutorialProgress()
    {
        SaveSystem.Instance.SaveTutorialCompleted(true);
    }

    public void ResetTutorialProgress()
    {
        isTutorialCompleted = false;
        isTutorialActive = false;
        currentStepIndex = 0;
        currentStage = 0;
        _attackCount = 0;
        _possessCount = 0;
        _defendCount = 0;
        _killCount = 0;
        _maxComboShown = 0;
        _evolutionHintShown = false;
        _defendHintShown = false;
        _pollutionHintShown = false;
        _hintCooldowns.Clear();
        SaveSystem.Instance.SaveTutorialCompleted(false);
    }

    protected override void CleanupEvents()
    {
        OnTutorialStepChanged = null;
        OnTutorialCompleted = null;
        OnTutorialStageUp = null;
    }

    protected override void OnDestroy()
    {
        EventBus.Unregister<float>(EventTypes.PollutionChanged, OnPollutionChangedEvent);
        base.OnDestroy();
    }
}
