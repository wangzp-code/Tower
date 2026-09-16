using UnityEngine;
using System.Collections.Generic;
using System.Linq;

public partial class CompleteGameSystem : SingletonBase<CompleteGameSystem>
{
    public class FragSkillEntry
    {
        public string fragName, fragIcon;
        public bool passive;
        public string skillName, skillIcon, skillDesc, effectId;
        public int maxUses;
        public float value;
    }

    [System.Serializable]
    public class SkillFragment { public string type; public string icon; }
    [System.Serializable]
    public class ActiveSkill { public string name; public string desc; public string icon; public string effectId; public int uses; public int maxUses; }
    [System.Serializable]
    public class PassiveEffect { public string name; public string icon; public string desc; public string effectId; public float value; }

    public List<SkillFragment> SkillFragments { get; private set; } = new List<SkillFragment>();
    public List<ActiveSkill> ActiveSkills { get; private set; } = new List<ActiveSkill>();
    public List<PassiveEffect> PassiveEffects { get; private set; } = new List<PassiveEffect>();
    public bool ShowingSynthConfirm { get; private set; }
    public string PendingSynthTrait { get; private set; }

    public FragSkillEntry[] CurrentFragCandidates { get; private set; }
    public string[] CurrentFragTraits { get; private set; }
    int _fragDropCount;

    // 技能效果临时状态
    float _nextAtkMult = 1f;
    bool _nextCrit;
    int _shieldTurns;
    int _dodgeTurns;
    int _enemyStunTurns;
    int _enemyPoisonTurns;
    float _enemyPoisonRate;
    float _enemyBleedRate;
    bool _perfectCounterNext;
    bool _decoyActive;

    static readonly Dictionary<string, FragSkillEntry> FragmentSkillMap = new Dictionary<string, FragSkillEntry>
    {
        {"吸血", new FragSkillEntry{fragName="吸血碎片",fragIcon="♦",skillName="生命汲取",skillIcon="♦",skillDesc="治愈攻击伤害的30%",maxUses=2,effectId="healOnHit"}},
        {"狂暴", new FragSkillEntry{fragName="狂暴碎片",fragIcon="※",skillName="暴怒一击",skillIcon="※",skillDesc="下次攻击伤害x2",maxUses=1,effectId="nextAtkX2"}},
        {"再生", new FragSkillEntry{fragName="再生碎片",fragIcon="♣",skillName="紧急修复",skillIcon="♣",skillDesc="立即回复25%MaxHP",maxUses=2,effectId="healNow25"}},
        {"护甲", new FragSkillEntry{fragName="护甲碎片",fragIcon="◆",skillName="临时护盾",skillIcon="◆",skillDesc="3回合受伤-50%",maxUses=1,effectId="shield"}},
        {"暴击", new FragSkillEntry{fragName="暴击碎片",fragIcon="★",skillName="必杀之心",skillIcon="★",skillDesc="下次攻击必定暴击x2",maxUses=2,effectId="guaranteedCrit"}},
        {"毒素", new FragSkillEntry{fragName="毒素碎片",fragIcon="☠",skillName="剧毒释放",skillIcon="☠",skillDesc="敌每回合-8%HP 持续3回合",maxUses=1,effectId="poisonDot"}},
        {"相位", new FragSkillEntry{fragName="相位碎片",fragIcon="◎",skillName="虚空闪避",skillIcon="◎",skillDesc="2回合完全闪避",maxUses=1,effectId="dodge"}},
        {"电击", new FragSkillEntry{fragName="电击碎片",fragIcon="⚡",skillName="电弧释放",skillIcon="⚡",skillDesc="ATKx50%伤害+眩晕1回合",maxUses=2,effectId="shockStun"}},
        {"恐惧", new FragSkillEntry{fragName="恐惧碎片",fragIcon="▼",skillName="心灵震慑",skillIcon="▼",skillDesc="敌ATK-30%全场",maxUses=1,effectId="fearDebuff"}},
        {"不死", new FragSkillEntry{fragName="不死碎片",fragIcon="☆",skillName="死亡拒绝",skillIcon="☆",skillDesc="本场死亡时50%HP复活",maxUses=1,effectId="extraRevive"}},
        {"撕裂", new FragSkillEntry{fragName="撕裂碎片",fragIcon="×",skillName="致命撕裂",skillIcon="×",skillDesc="敌每回合-10%HP全场",maxUses=1,effectId="heavyBleed"}},
        {"反击", new FragSkillEntry{fragName="反击碎片",fragIcon="↩",skillName="完美格挡",skillIcon="↩",skillDesc="下次受击反弹100%",maxUses=2,effectId="perfectCounter"}},
        {"迅捷", new FragSkillEntry{fragName="迅捷碎片",fragIcon="»",skillName="疾风突刺",skillIcon="»",skillDesc="下次攻击伤害x1.8 先手",maxUses=2,effectId="nextAtkX2"}},
        {"厚皮", new FragSkillEntry{fragName="厚皮碎片",fragIcon="■",skillName="铁壁",skillIcon="■",skillDesc="3回合受伤-50%",maxUses=1,effectId="shield"}},
        {"忠诚", new FragSkillEntry{fragName="忠诚碎片",fragIcon="♠",skillName="忠诚守护",skillIcon="♠",skillDesc="立即回复25%MaxHP",maxUses=2,effectId="healNow25"}},
        {"弹性", new FragSkillEntry{fragName="弹性碎片",fragIcon="~",skillName="弹性闪避",skillIcon="~",skillDesc="2回合完全闪避",maxUses=1,effectId="dodge"}},
        {"蛛网", new FragSkillEntry{fragName="蛛网碎片",fragIcon="※",skillName="蛛网陷阱",skillIcon="※",skillDesc="敌ATK-30%全场",maxUses=1,effectId="fearDebuff"}},
        {"领袖", new FragSkillEntry{fragName="领袖碎片",fragIcon="♛",skillName="鼓舞士气",skillIcon="♛",skillDesc="下次攻击伤害x2",maxUses=1,effectId="nextAtkX2"}},
        {"寄生强化", new FragSkillEntry{fragName="寄生碎片",fragIcon="◉",skillName="寄生吸取",skillIcon="◉",skillDesc="治愈攻击伤害的30%",maxUses=2,effectId="healOnHit"}},
        {"伏击", new FragSkillEntry{fragName="伏击碎片",fragIcon="†",skillName="暗影伏击",skillIcon="†",skillDesc="下次攻击必定暴击x2",maxUses=2,effectId="guaranteedCrit"}},
        {"吸取", new FragSkillEntry{fragName="吸取碎片",fragIcon="●",skillName="灵魂吸取",skillIcon="●",skillDesc="治愈攻击伤害的30%",maxUses=2,effectId="healOnHit"}},
        {"多重攻击", new FragSkillEntry{fragName="多重碎片",fragIcon="⚔",skillName="连击风暴",skillIcon="⚔",skillDesc="下次攻击伤害x2",maxUses=1,effectId="nextAtkX2"}},
        {"召唤", new FragSkillEntry{fragName="召唤碎片",fragIcon="◇",skillName="幻影召唤",skillIcon="◇",skillDesc="召唤分身承受1次伤害",maxUses=1,effectId="summonDecoy"}},
        {"污染光环", new FragSkillEntry{fragName="污染碎片",fragIcon="☢",skillName="污染爆发",skillIcon="☢",skillDesc="敌每回合-5%HP 持续3回合",maxUses=1,effectId="poisonDot"}},
        {"掠夺", new FragSkillEntry{fragName="掠夺碎片",fragIcon="$",skillName="资源掠夺",skillIcon="$",skillDesc="击杀后EP+50",maxUses=2,effectId="epBonus"}},
        {"爆炸", new FragSkillEntry{fragName="爆炸碎片",fragIcon="⊙",skillName="自爆协议",skillIcon="⊙",skillDesc="对敌造成30%MaxHP伤害",maxUses=1,effectId="selfDestruct"}},
        // 被动碎片
        {"护甲被动", new FragSkillEntry{fragName="铁壁碎片",fragIcon="◆",passive=true,skillName="铁壁",skillIcon="◆",skillDesc="永久DEF+3",effectId="passiveDef",value=3}},
        {"迅捷被动", new FragSkillEntry{fragName="疾步碎片",fragIcon="»",passive=true,skillName="疾步",skillIcon="»",skillDesc="永久双步移动",effectId="passiveSpeed"}},
        {"洞察被动", new FragSkillEntry{fragName="先知碎片",fragIcon="◎",passive=true,skillName="先知之眼",skillIcon="◎",skillDesc="永久暴击率+15%",effectId="passiveCrit",value=0.15f}},
        {"掠夺被动", new FragSkillEntry{fragName="掠夺碎片",fragIcon="$",passive=true,skillName="资源掠夺",skillIcon="$",skillDesc="击杀EP+20%",effectId="passiveEP",value=0.2f}},
        {"恐惧被动", new FragSkillEntry{fragName="威慑碎片",fragIcon="▼",passive=true,skillName="威慑光环",skillIcon="▼",skillDesc="怪物ATK-10%",effectId="passiveFear",value=0.1f}},
        {"反击被动", new FragSkillEntry{fragName="反击碎片",fragIcon="↩",passive=true,skillName="完美反击",skillIcon="↩",skillDesc="受击30%概率反弹50%伤害",effectId="passiveCounter",value=0.3f}},
        {"寄生被动", new FragSkillEntry{fragName="寄生碎片",fragIcon="◉",passive=true,skillName="寄生强化",skillIcon="◉",skillDesc="附身率+10%",effectId="passivePossess",value=0.1f}},
        {"生命力被动", new FragSkillEntry{fragName="体质碎片",fragIcon="❤",passive=true,skillName="体质强化",skillIcon="❤",skillDesc="永久MaxHP+30",effectId="passiveHP",value=30f}},
    };

    static readonly string[][] ZoneFragPools = new string[][]
    {
        new[]{"护甲","再生","忠诚","厚皮","护甲被动","生命力被动"},
        new[]{"吸血","狂暴","毒素","撕裂","掠夺","掠夺被动"},
        new[]{"暴击","电击","恐惧","蛛网","伏击","恐惧被动","洞察被动"},
        new[]{"召唤","寄生强化","反击","弹性","不死","反击被动","寄生被动"},
        new[]{"掠夺","迅捷","相位","污染光环","爆炸","迅捷被动"},
    };

    static bool _configLoaded;

    public static void LoadFragmentConfig(FragmentDataConfig config)
    {
        if (config == null || _configLoaded) return;
        _configLoaded = true;

        FragmentSkillMap.Clear();
        foreach (var f in config.fragments)
        {
            FragmentSkillMap[f.id] = new FragSkillEntry
            {
                fragName = f.fragName,
                fragIcon = f.fragIcon,
                passive = f.passive,
                skillName = f.skillName,
                skillIcon = f.skillIcon,
                skillDesc = f.skillDesc,
                effectId = f.effectId,
                maxUses = f.maxUses,
                value = f.value
            };
        }

        foreach (var zp in config.zonePools)
        {
            int idx = zp.zoneId - 1;
            if (idx >= 0 && idx < ZoneFragPools.Length)
            {
                var poolIds = zp.fragmentIds.ToArray();
                var currentPool = ZoneFragPools[idx];
                if (currentPool.Length >= poolIds.Length)
                {
                    for (int i = 0; i < poolIds.Length; i++)
                        currentPool[i] = poolIds[i];
                }
                else
                {
                    var newPool = new string[poolIds.Length];
                    for (int i = 0; i < poolIds.Length; i++)
                        newPool[i] = poolIds[i];
                    ZoneFragPools[idx] = newPool;
                }
            }
        }

        Debug.Log($"[FragmentSystem] Loaded {FragmentSkillMap.Count} fragments from config (zone pools updated)");
    }

    public static void ResetFragmentConfig()
    {
        _configLoaded = false;
    }

    public void TryDropFragment(MonsterRuntime monster)
    {
        if (monster == null) return;
        bool isBoss = monster.isBoss;
        var mode = GameManager.Instance.CurrentMode;
        int floor = GameManager.Instance.CurrentFloor;
        var floorParams = ModeFloorCurves.GetParams(mode, floor);
        
        float dropRate = isBoss ? 0.8f : floorParams.fragRate * 1.2f;
        if (Random.value >= dropRate) return;
        
        int maxDropsPerFloor = mode == GameMode.Short ? (floor <= 5 ? 1 : 2) : (floor <= 10 ? 1 : 2);
        
        if (_fragDropCount >= maxDropsPerFloor) return;
        _fragDropCount++;
        var candidates = GenerateFragCandidates(monster, isBoss);
        if (candidates.Count == 0) return;
        ShowFragmentChoiceInternal(candidates);
    }

    List<string> GenerateFragCandidates(MonsterRuntime monster, bool isBoss)
    {
        int zone = Mathf.Clamp(Mathf.CeilToInt(GameManager.Instance.CurrentFloor / 10f), 1, 5);
        var pool = new List<string>();
        for (int z = 1; z <= zone; z++)
        {
            int w = z == zone ? 3 : (zone - z >= 2 ? 1 : 2);
            foreach (var t in ZoneFragPools[z - 1])
                if (FragmentSkillMap.ContainsKey(t))
                    for (int i = 0; i < w; i++) pool.Add(t);
        }
        var boosted = new HashSet<string>();
        foreach (var f in SkillFragments)
            if (SkillFragments.Count(ff => ff.type == f.type) >= 2 && FragmentSkillMap.ContainsKey(f.type))
                boosted.Add(f.type);
        foreach (var t in boosted) for (int i = 0; i < 6; i++) pool.Add(t);

        var results = new List<string>();
        if (monster.traits != null)
        {
            var valid = monster.traits.Where(t => FragmentSkillMap.ContainsKey(t)).ToList();
            if (valid.Count > 0) results.Add(valid[Random.Range(0, valid.Count)]);
        }
        while (results.Count < 3 && pool.Count > 0)
        {
            int idx = Random.Range(0, pool.Count);
            if (!results.Contains(pool[idx])) results.Add(pool[idx]);
            pool.RemoveAt(idx);
        }
        return results;
    }

    void ShowFragmentChoiceInternal(List<string> traits)
    {
        CurrentFragTraits = traits.ToArray();
        CurrentFragCandidates = new FragSkillEntry[traits.Count];
        FragmentChoices = new string[traits.Count];
        FragmentChoiceDescs = new string[traits.Count];
        for (int i = 0; i < traits.Count; i++)
        {
            if (FragmentSkillMap.TryGetValue(traits[i], out var entry))
            {
                CurrentFragCandidates[i] = entry;
                FragmentChoices[i] = entry.fragName;
                FragmentChoiceDescs[i] = entry.skillDesc;
            }
        }
        ShowingFragmentChoice = true;
        LastMessage = "⚡ 选择碎片奖励 ⚡";
    }

    public void TriggerFragmentChoice()
    {
        int zone = Mathf.Clamp(Mathf.CeilToInt(GameManager.Instance.CurrentFloor / 10f), 1, 5);
        var pool = new List<string>();
        for (int z = 1; z <= zone; z++)
        {
            int w = z == zone ? 3 : (zone - z >= 2 ? 1 : 2);
            foreach (var t in ZoneFragPools[z - 1])
                if (FragmentSkillMap.ContainsKey(t))
                    for (int i = 0; i < w; i++) pool.Add(t);
        }
        var results = new List<string>();
        var used = new HashSet<string>();
        while (results.Count < 3 && pool.Count > 0)
        {
            int idx = Random.Range(0, pool.Count);
            if (!used.Contains(pool[idx])) { used.Add(pool[idx]); results.Add(pool[idx]); }
            pool.RemoveAt(idx);
        }
        ShowFragmentChoiceInternal(results);
    }

    public void SelectFragment(int index)
    {
        if (!ShowingFragmentChoice || CurrentFragTraits == null || index < 0 || index >= CurrentFragTraits.Length) return;
        string trait = CurrentFragTraits[index];
        if (!FragmentSkillMap.TryGetValue(trait, out var frag)) return;
        var player = GameManager.Instance.Player;
        int maxFrags = 12;
        int count = SkillFragments.Count(f => f.type == trait);

        if (SkillFragments.Count >= maxFrags && count < 2)
        {
            player.evolutionPoints += 15;
            AddCombatLog(frag.fragIcon + " 碎片已满，自动分解 +15EP");
        }
        else
        {
            SkillFragments.Add(new SkillFragment { type = trait, icon = frag.fragIcon });
            player.fragments = SkillFragments.Count;
            player.equippedFragments.Add(frag.fragIcon + " " + frag.fragName);
            AddCombatLog(frag.fragIcon + " 获得 " + frag.fragName + "!");
            if (count + 1 >= 3)
            {
                PendingSynthTrait = trait;
                ShowingSynthConfirm = true;
            }
        }
        ShowingFragmentChoice = false;
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public void SkipAllFragments()
    {
        if (!ShowingFragmentChoice) return;
        GameManager.Instance.Player.evolutionPoints += 50;
        ShowingFragmentChoice = false;
        AddCombatLog("放弃碎片 +50EP");
        LastMessage = "全部放弃，获得 +50EP";
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    public void ConfirmSynthesize()
    {
        if (!ShowingSynthConfirm || string.IsNullOrEmpty(PendingSynthTrait)) return;
        DoSynthesize(PendingSynthTrait);
        ShowingSynthConfirm = false;
        PendingSynthTrait = null;
    }

    public void CancelSynthesize()
    {
        ShowingSynthConfirm = false;
        PendingSynthTrait = null;
    }

    void DoSynthesize(string traitType)
    {
        if (!FragmentSkillMap.TryGetValue(traitType, out var frag)) return;
        int removed = 0;
        SkillFragments.RemoveAll(f =>
        {
            if (f.type == traitType && removed < 3) { removed++; return true; }
            return false;
        });
        GameManager.Instance.Player.fragments = SkillFragments.Count;

        if (frag.passive)
        {
            var eff = new PassiveEffect
            {
                name = frag.skillName, icon = frag.skillIcon,
                desc = frag.skillDesc, effectId = frag.effectId, value = frag.value
            };
            PassiveEffects.Add(eff);
            ApplyPassiveImmediate(eff);
            AddCombatLog("◈ 被动效果: " + eff.icon + " " + eff.name + " 永久激活!");
        }
        else
        {
            var skill = new ActiveSkill
            {
                name = frag.skillName, desc = frag.skillDesc, icon = frag.skillIcon,
                effectId = frag.effectId, uses = frag.maxUses, maxUses = frag.maxUses
            };
            if (ActiveSkills.Count >= 3) ActiveSkills.RemoveAt(0);
            ActiveSkills.Add(skill);
            AddCombatLog("⚡ 合成技能: " + skill.icon + " " + skill.name + " (" + skill.uses + "次)");
        }
        GameManager.Instance.NotifyPlayerStatsChanged();
    }

    void ApplyPassiveImmediate(PassiveEffect eff)
    {
        var p = GameManager.Instance.Player;
        switch (eff.effectId)
        {
            case "passiveDef": p.defense += Mathf.RoundToInt(eff.value); p.baseDefense += Mathf.RoundToInt(eff.value); break;
            case "passiveHP": p.maxHp += Mathf.RoundToInt(eff.value); p.hp += Mathf.RoundToInt(eff.value); p.baseMaxHp += Mathf.RoundToInt(eff.value); break;
            case "passivePossess": p.possessionBonus += eff.value; break;
        }
    }

    public bool HasPassiveEffect(string effectId) => PassiveEffects.Any(e => e.effectId == effectId);
    public float GetPassiveValue(string effectId) => PassiveEffects.Where(e => e.effectId == effectId).Sum(e => e.value);

    public FragSkillEntry GetFragSkillEntry(string trait)
    {
        return FragmentSkillMap.TryGetValue(trait, out var e) ? e : null;
    }

#if UNITY_EDITOR
    // 调试：强制触发碎片选择界面，注入混合主动/被动数据
    public void DebugShowFragChoice()
    {
        var debugTraits = new List<string>
        {
            "吸血",       // 主动
            "护甲被动",   // 被动
            "暴击"        // 主动
        };
        ShowFragmentChoiceInternal(debugTraits);
        UnityEngine.Debug.Log("[FragmentSystem] Debug: 显示碎片选择界面 (主动+被动混合)");
    }
#endif

    public void UseActiveSkill(int index)
    {
        if (index < 0 || index >= ActiveSkills.Count) return;
        var skill = ActiveSkills[index];
        if (skill.uses <= 0) { AddCombatLog(skill.icon + " " + skill.name + " 已耗尽!"); return; }
        if (CurrentScreen != RunScreen.Combat || CurrentEnemy == null) { AddCombatLog("只能在战斗中使用技能!"); return; }

        skill.uses--;
        var player = GameManager.Instance.Player;
        switch (skill.effectId)
        {
            case "healOnHit":
                int heal = Mathf.RoundToInt(player.attack * 0.3f);
                player.hp = Mathf.Min(player.maxHp, player.hp + heal);
                AddCombatLog(skill.icon + " 生命汲取! 回复" + heal + "HP");
                break;
            case "nextAtkX2":
                _nextAtkMult = 2f;
                AddCombatLog(skill.icon + " 下次攻击伤害x2!");
                break;
            case "healNow25":
                int h25 = Mathf.RoundToInt(player.maxHp * 0.25f);
                player.hp = Mathf.Min(player.maxHp, player.hp + h25);
                AddCombatLog(skill.icon + " 紧急修复! 回复" + h25 + "HP");
                DamageNumberPool.Instance?.SpawnHeal(h25);
                break;
            case "shield":
                _shieldTurns = 3;
                AddCombatLog(skill.icon + " 临时护盾! 3回合受伤-50%");
                break;
            case "guaranteedCrit":
                _nextCrit = true;
                AddCombatLog(skill.icon + " 必杀之心! 下次攻击必定暴击x2");
                break;
            case "poisonDot":
                _enemyPoisonTurns = 3;
                _enemyPoisonRate = 0.08f;
                AddCombatLog(skill.icon + " 毒素释放! 敌人每回合-8%HP");
                break;
            case "dodge":
                _dodgeTurns = 2;
                AddCombatLog(skill.icon + " 完全闪避! 2回合免疫伤害");
                break;
            case "shockStun":
                int shockDmg = Mathf.RoundToInt(player.attack * 0.5f);
                CurrentEnemy.hp = Mathf.Max(0, CurrentEnemy.hp - shockDmg);
                _enemyStunTurns = 1;
                AddCombatLog(skill.icon + " 电弧释放! " + shockDmg + "伤害+眩晕1回合");
                DamageNumberPool.Instance?.SpawnDamage(shockDmg, true);
                break;
            case "fearDebuff":
                CurrentEnemy.atk = Mathf.RoundToInt(CurrentEnemy.atk * 0.7f);
                AddCombatLog(skill.icon + " 心灵震慑! 敌ATK-30%");
                break;
            case "extraRevive":
                player.extraRevive++;
                AddCombatLog(skill.icon + " 死亡拒绝! 本场死亡时复活");
                break;
            case "heavyBleed":
                _enemyBleedRate = 0.10f;
                AddCombatLog(skill.icon + " 致命撕裂! 敌每回合-10%HP");
                break;
            case "perfectCounter":
                _perfectCounterNext = true;
                AddCombatLog(skill.icon + " 完美格挡! 下次受击反弹100%");
                break;
            case "selfDestruct":
                int boomDmg = Mathf.RoundToInt(player.maxHp * 0.3f);
                CurrentEnemy.hp = Mathf.Max(0, CurrentEnemy.hp - boomDmg);
                AddCombatLog(skill.icon + " 自爆协议! 造成" + boomDmg + "伤害");
                DamageNumberPool.Instance?.SpawnDamage(boomDmg, true);
                break;
            case "summonDecoy":
                _decoyActive = true;
                AddCombatLog(skill.icon + " 幻影召唤! 分身承受下1次伤害");
                break;
            case "epBonus":
                player.evolutionPoints += 50;
                AddCombatLog(skill.icon + " 资源掠夺! EP+50");
                break;
        }
        ScreenEffectsManager.Instance?.FlashCrit();
        GameManager.Instance.NotifyPlayerStatsChanged();
        if (CurrentEnemy != null && CurrentEnemy.hp <= 0) OnEnemyDefeated(false);
    }
}
