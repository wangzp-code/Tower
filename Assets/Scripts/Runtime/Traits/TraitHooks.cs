using UnityEngine;
using System.Collections.Generic;

public enum TraitHookType
{
    OnAttack,
    OnDefend,
    OnHitTaken,
    OnKill,
    OnDeath,
    OnTurnStart,
    OnTurnEnd,
    OnPossess,
    OnFormSwitch,
    OnFloorEnter,
    OnFloorExit
}

public delegate void TraitHookDelegate(TraitHookContext context);

public class TraitHookContext
{
    public object self;
    public object target;
    public int damage;
    public int hp;
    public int maxHp;
    public int round;
    public string traitName;
    public Dictionary<string, object> data = new Dictionary<string, object>();
    public bool handled;
    public List<string> log = new List<string>();
}

public static class TraitHooks
{
    private static Dictionary<TraitHookType, List<TraitHookDelegate>> hooks =
        new Dictionary<TraitHookType, List<TraitHookDelegate>>();

    // 注册钩子
    public static void RegisterHook(TraitHookType type, TraitHookDelegate handler)
    {
        if (!hooks.ContainsKey(type))
        {
            hooks[type] = new List<TraitHookDelegate>();
        }
        hooks[type].Add(handler);
    }

    // 触发钩子
    public static void TriggerHook(TraitHookType type, TraitHookContext context)
    {
        if (!hooks.ContainsKey(type)) return;

        foreach (var handler in hooks[type])
        {
            handler(context);
            if (context.handled) break;
        }
    }

    // 运行特质管道
    public static void RunTraitPipeline(TraitHookType type, TraitHookContext context, List<string> traits)
    {
        if (traits == null || traits.Count == 0) return;

        foreach (string trait in traits)
        {
            context.traitName = trait;
            TriggerHook(type, context);
            if (context.handled) break;
        }
    }

    /// <summary>
    /// 安全地从 context.data 中获取 bool 值，避免 KeyNotFoundException
    /// </summary>
    private static bool TryGetBool(TraitHookContext context, string key, bool defaultValue = false)
    {
        if (context.data.TryGetValue(key, out object val) && val is bool b)
        {
            return b;
        }
        return defaultValue;
    }

    /// <summary>
    /// 安全地从 context.data 中获取 int 值
    /// </summary>
    private static int TryGetInt(TraitHookContext context, string key, int defaultValue = 0)
    {
        if (context.data.TryGetValue(key, out object val) && val is int i)
        {
            return i;
        }
        return defaultValue;
    }

    /// <summary>
    /// 安全地从 context.data 中获取 float 值
    /// </summary>
    private static float TryGetFloat(TraitHookContext context, string key, float defaultValue = 0f)
    {
        if (context.data.TryGetValue(key, out object val) && val is float f)
        {
            return f;
        }
        return defaultValue;
    }

    // 初始化内置特质钩子
    public static void InitializeBuiltinHooks(CompleteGameSystem gameSystem)
    {
        // 攻击时钩子
        RegisterHook(TraitHookType.OnAttack, (context) => {
            ApplyAttackTraitEffects(context, gameSystem);
        });

        // 受击时钩子
        RegisterHook(TraitHookType.OnHitTaken, (context) => {
            ApplyDefenseTraitEffects(context, gameSystem);
        });

        // 击杀时钩子
        RegisterHook(TraitHookType.OnKill, (context) => {
            ApplyKillTraitEffects(context, gameSystem);
        });

        // 死亡时钩子
        RegisterHook(TraitHookType.OnDeath, (context) => {
            ApplyDeathTraitEffects(context, gameSystem);
        });

        // 回合开始时钩子
        RegisterHook(TraitHookType.OnTurnStart, (context) => {
            ApplyTurnStartTraitEffects(context, gameSystem);
        });

        // 回合结束时钩子
        RegisterHook(TraitHookType.OnTurnEnd, (context) => {
            ApplyTurnEndTraitEffects(context, gameSystem);
        });
    }

    // 应用攻击特质效果
    private static void ApplyAttackTraitEffects(TraitHookContext context, CompleteGameSystem gameSystem)
    {
        string trait = context.traitName;

        switch (trait)
        {
            case "暴击":
                if (Random.value < 0.15f)
                {
                    context.damage = Mathf.FloorToInt(context.damage * 1.5f);
                    context.log.Add("暴击!");
                }
                break;
            case "多重攻击":
                context.damage = Mathf.FloorToInt(context.damage * 0.8f * 2f);
                context.log.Add("多重攻击×2");
                break;
            case "撕裂":
                context.data["bleedApplied"] = true;
                context.log.Add("撕裂!");
                break;
            case "毒素":
                context.data["poisonApplied"] = true;
                context.log.Add("毒素!");
                break;
        }
    }

    // 应用防御特质效果
    private static void ApplyDefenseTraitEffects(TraitHookContext context, CompleteGameSystem gameSystem)
    {
        string trait = context.traitName;

        switch (trait)
        {
            case "闪避":
                if (Random.value < 0.2f)
                {
                    context.damage = 0;
                    context.log.Add("闪避!");
                    context.handled = true;
                }
                break;
            case "铁壁":
                if (Random.value < 0.1f)
                {
                    context.damage = 0;
                    context.log.Add("铁壁格挡!");
                    context.handled = true;
                }
                break;
            case "棘甲":
                if (context.damage > 0)
                {
                    int reflect = Mathf.FloorToInt(context.damage * 0.5f);
                    context.data["counterDamage"] = reflect;
                    context.log.Add($"棘甲反击-{reflect}");
                }
                break;
            case "硬化":
                context.damage = Mathf.FloorToInt(context.damage * 0.8f);
                context.log.Add("硬化减伤");
                break;
        }
    }

    // 应用击杀特质效果
    private static void ApplyKillTraitEffects(TraitHookContext context, CompleteGameSystem gameSystem)
    {
        string trait = context.traitName;

        switch (trait)
        {
            case "吸血":
                int lifesteal = Mathf.FloorToInt(context.damage * 0.1f);
                context.data["healAmount"] = lifesteal;
                context.log.Add($"吸血+{lifesteal}");
                break;
            case "贪婪":
                context.data["extraGold"] = 50;
                context.log.Add("贪婪+50EP");
                break;
        }
    }

    // 应用死亡特质效果
    private static void ApplyDeathTraitEffects(TraitHookContext context, CompleteGameSystem gameSystem)
    {
        string trait = context.traitName;

        switch (trait)
        {
            case "不死":
                // 安全访问字典，避免 KeyNotFoundException
                bool hasRevived = TryGetBool(context, "hasRevived");
                if (!hasRevived)
                {
                    context.data["saved"] = true;
                    context.data["hasRevived"] = true;
                    context.hp = Mathf.Max(1, Mathf.FloorToInt(context.maxHp * 0.3f));
                    context.log.Add("不死复活!");
                    context.handled = true;
                }
                break;
            case "死亡爆炸":
                int blast = Mathf.FloorToInt(context.maxHp * 0.5f);
                context.data["blastDamage"] = blast;
                context.log.Add($"死亡爆炸-{blast}");
                break;
        }
    }

    // 应用回合开始特质效果
    private static void ApplyTurnStartTraitEffects(TraitHookContext context, CompleteGameSystem gameSystem)
    {
        string trait = context.traitName;

        switch (trait)
        {
            case "再生":
                int regen = Mathf.FloorToInt(context.maxHp * 0.05f);
                context.hp = Mathf.Min(context.maxHp, context.hp + regen);
                context.log.Add($"再生+{regen}");
                break;
            case "再生+":
                int regenPlus = Mathf.FloorToInt(context.maxHp * 0.1f);
                context.hp = Mathf.Min(context.maxHp, context.hp + regenPlus);
                context.log.Add($"再生+{regenPlus}");
                break;
            case "狂暴":
                if (context.hp < context.maxHp * 0.5f)
                {
                    context.data["atkBoost"] = 1.5f;
                    context.log.Add("狂暴! ATK+50%");
                }
                break;
        }
    }

    // 应用回合结束特质效果
    private static void ApplyTurnEndTraitEffects(TraitHookContext context, CompleteGameSystem gameSystem)
    {
        string trait = context.traitName;

        switch (trait)
        {
            case "腐蚀":
                int defReduction = 1;
                context.data["defReduction"] = defReduction;
                context.log.Add($"腐蚀 DEF-{defReduction}");
                break;
            case "召唤":
                if (context.round % 4 == 0 && Random.value < 0.2f)
                {
                    context.data["summon"] = true;
                    context.log.Add("召唤!");
                }
                break;
        }
    }
}
