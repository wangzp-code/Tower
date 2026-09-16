using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(fileName = "FragmentDataConfig", menuName = "ParasiteTower/Fragment Data Config", order = 100)]
public class FragmentDataConfig : ScriptableObject
{
    [System.Serializable]
    public class FragmentDefinition
    {
        public string id;
        public string fragName;
        public string fragIcon;
        public bool passive;
        public string skillName;
        public string skillIcon;
        public string skillDesc;
        public string effectId;
        public int maxUses = 1;
        public float value = 0f;
    }

    [System.Serializable]
    public class ZoneFragmentPool
    {
        public int zoneId;
        public List<string> fragmentIds = new List<string>();
    }

    [Header("Fragment Definitions")]
    public List<FragmentDefinition> fragments = new List<FragmentDefinition>();

    [Header("Zone Fragment Pools")]
    public List<ZoneFragmentPool> zonePools = new List<ZoneFragmentPool>();

    private Dictionary<string, FragmentDefinition> _fragmentMap;
    private Dictionary<int, string[]> _zonePoolMap;

    public Dictionary<string, FragmentDefinition> GetFragmentMap()
    {
        if (_fragmentMap == null || _fragmentMap.Count != fragments.Count)
        {
            BuildLookup();
        }
        return _fragmentMap;
    }

    public string[] GetZonePool(int zoneId)
    {
        if (_zonePoolMap == null || _zonePoolMap.Count != zonePools.Count)
        {
            BuildLookup();
        }
        return _zonePoolMap.TryGetValue(zoneId, out var pool) ? pool : System.Array.Empty<string>();
    }

    private void BuildLookup()
    {
        _fragmentMap = new Dictionary<string, FragmentDefinition>();
        foreach (var f in fragments)
        {
            if (!string.IsNullOrEmpty(f.id))
            {
                _fragmentMap[f.id] = f;
            }
        }

        _zonePoolMap = new Dictionary<int, string[]>();
        foreach (var zp in zonePools)
        {
            if (zp != null && zp.fragmentIds != null && zp.fragmentIds.Count > 0)
            {
                _zonePoolMap[zp.zoneId] = zp.fragmentIds.ToArray();
            }
        }
    }

    public FragmentDefinition GetFragment(string id)
    {
        if (_fragmentMap == null) BuildLookup();
        return _fragmentMap.TryGetValue(id, out var def) ? def : null;
    }

    public void ResetToDefaults()
    {
        fragments = new List<FragmentDefinition>
        {
            new FragmentDefinition{id="吸血",fragName="吸血碎片",fragIcon="♦",passive=false,skillName="生命汲取",skillIcon="♦",skillDesc="治愈攻击伤害的30%",maxUses=2,effectId="healOnHit"},
            new FragmentDefinition{id="狂暴",fragName="狂暴碎片",fragIcon="※",passive=false,skillName="暴怒一击",skillIcon="※",skillDesc="下次攻击伤害x2",maxUses=1,effectId="nextAtkX2"},
            new FragmentDefinition{id="再生",fragName="再生碎片",fragIcon="♣",passive=false,skillName="紧急修复",skillIcon="♣",skillDesc="立即回复25%MaxHP",maxUses=2,effectId="healNow25"},
            new FragmentDefinition{id="护甲",fragName="护甲碎片",fragIcon="◆",passive=false,skillName="临时护盾",skillIcon="◆",skillDesc="3回合受伤-50%",maxUses=1,effectId="shield"},
            new FragmentDefinition{id="暴击",fragName="暴击碎片",fragIcon="★",passive=false,skillName="必杀之心",skillIcon="★",skillDesc="下次攻击必定暴击x2",maxUses=2,effectId="guaranteedCrit"},
            new FragmentDefinition{id="毒素",fragName="毒素碎片",fragIcon="☠",passive=false,skillName="剧毒释放",skillIcon="☠",skillDesc="敌每回合-8%HP 持续3回合",maxUses=1,effectId="poisonDot"},
            new FragmentDefinition{id="相位",fragName="相位碎片",fragIcon="◎",passive=false,skillName="虚空闪避",skillIcon="◎",skillDesc="2回合完全闪避",maxUses=1,effectId="dodge"},
            new FragmentDefinition{id="电击",fragName="电击碎片",fragIcon="⚡",passive=false,skillName="电弧释放",skillIcon="⚡",skillDesc="ATKx50%伤害+眩晕1回合",maxUses=2,effectId="shockStun"},
            new FragmentDefinition{id="恐惧",fragName="恐惧碎片",fragIcon="▼",passive=false,skillName="心灵震慑",skillIcon="▼",skillDesc="敌ATK-30%全场",maxUses=1,effectId="fearDebuff"},
            new FragmentDefinition{id="不死",fragName="不死碎片",fragIcon="☆",passive=false,skillName="死亡拒绝",skillIcon="☆",skillDesc="本场死亡时50%HP复活",maxUses=1,effectId="extraRevive"},
            new FragmentDefinition{id="撕裂",fragName="撕裂碎片",fragIcon="×",passive=false,skillName="致命撕裂",skillIcon="×",skillDesc="敌每回合-10%HP全场",maxUses=1,effectId="heavyBleed"},
            new FragmentDefinition{id="反击",fragName="反击碎片",fragIcon="↩",passive=false,skillName="完美格挡",skillIcon="↩",skillDesc="下次受击反弹100%",maxUses=2,effectId="perfectCounter"},
            new FragmentDefinition{id="迅捷",fragName="迅捷碎片",fragIcon="»",passive=false,skillName="疾风突刺",skillIcon="»",skillDesc="下次攻击伤害x1.8 先手",maxUses=2,effectId="nextAtkX2"},
            new FragmentDefinition{id="厚皮",fragName="厚皮碎片",fragIcon="■",passive=false,skillName="铁壁",skillIcon="■",skillDesc="3回合受伤-50%",maxUses=1,effectId="shield"},
            new FragmentDefinition{id="忠诚",fragName="忠诚碎片",fragIcon="♠",passive=false,skillName="忠诚守护",skillIcon="♠",skillDesc="立即回复25%MaxHP",maxUses=2,effectId="healNow25"},
            new FragmentDefinition{id="弹性",fragName="弹性碎片",fragIcon="~",passive=false,skillName="弹性闪避",skillIcon="~",skillDesc="2回合完全闪避",maxUses=1,effectId="dodge"},
            new FragmentDefinition{id="蛛网",fragName="蛛网碎片",fragIcon="※",passive=false,skillName="蛛网陷阱",skillIcon="※",skillDesc="敌ATK-30%全场",maxUses=1,effectId="fearDebuff"},
            new FragmentDefinition{id="领袖",fragName="领袖碎片",fragIcon="♛",passive=false,skillName="鼓舞士气",skillIcon="♛",skillDesc="下次攻击伤害x2",maxUses=1,effectId="nextAtkX2"},
            new FragmentDefinition{id="寄生强化",fragName="寄生碎片",fragIcon="◉",passive=false,skillName="寄生吸取",skillIcon="◉",skillDesc="治愈攻击伤害的30%",maxUses=2,effectId="healOnHit"},
            new FragmentDefinition{id="伏击",fragName="伏击碎片",fragIcon="†",passive=false,skillName="暗影伏击",skillIcon="†",skillDesc="下次攻击必定暴击x2",maxUses=2,effectId="guaranteedCrit"},
            new FragmentDefinition{id="吸取",fragName="吸取碎片",fragIcon="●",passive=false,skillName="灵魂吸取",skillIcon="●",skillDesc="治愈攻击伤害的30%",maxUses=2,effectId="healOnHit"},
            new FragmentDefinition{id="多重攻击",fragName="多重碎片",fragIcon="⚔",passive=false,skillName="连击风暴",skillIcon="⚔",skillDesc="下次攻击伤害x2",maxUses=1,effectId="nextAtkX2"},
            new FragmentDefinition{id="召唤",fragName="召唤碎片",fragIcon="◇",passive=false,skillName="幻影召唤",skillIcon="◇",skillDesc="召唤分身承受1次伤害",maxUses=1,effectId="summonDecoy"},
            new FragmentDefinition{id="污染光环",fragName="污染碎片",fragIcon="☢",passive=false,skillName="污染爆发",skillIcon="☢",skillDesc="敌每回合-5%HP 持续3回合",maxUses=1,effectId="poisonDot"},
            new FragmentDefinition{id="掠夺",fragName="掠夺碎片",fragIcon="$",passive=false,skillName="资源掠夺",skillIcon="$",skillDesc="击杀后EP+50",maxUses=2,effectId="epBonus"},
            new FragmentDefinition{id="爆炸",fragName="爆炸碎片",fragIcon="⊙",passive=false,skillName="自爆协议",skillIcon="⊙",skillDesc="对敌造成30%MaxHP伤害",maxUses=1,effectId="selfDestruct"},
            // 被动
            new FragmentDefinition{id="护甲被动",fragName="铁壁碎片",fragIcon="◆",passive=true,skillName="铁壁",skillIcon="◆",skillDesc="永久DEF+3",effectId="passiveDef",value=3},
            new FragmentDefinition{id="迅捷被动",fragName="疾步碎片",fragIcon="»",passive=true,skillName="疾步",skillIcon="»",skillDesc="永久双步移动",effectId="passiveSpeed"},
            new FragmentDefinition{id="洞察被动",fragName="先知碎片",fragIcon="◎",passive=true,skillName="先知之眼",skillIcon="◎",skillDesc="永久暴击率+15%",effectId="passiveCrit",value=0.15f},
            new FragmentDefinition{id="掠夺被动",fragName="掠夺碎片",fragIcon="$",passive=true,skillName="资源掠夺",skillIcon="$",skillDesc="击杀EP+20%",effectId="passiveEP",value=0.2f},
            new FragmentDefinition{id="恐惧被动",fragName="威慑碎片",fragIcon="▼",passive=true,skillName="威慑光环",skillIcon="▼",skillDesc="怪物ATK-10%",effectId="passiveFear",value=0.1f},
            new FragmentDefinition{id="反击被动",fragName="反击碎片",fragIcon="↩",passive=true,skillName="完美反击",skillIcon="↩",skillDesc="受击30%概率反弹50%伤害",effectId="passiveCounter",value=0.3f},
            new FragmentDefinition{id="寄生被动",fragName="寄生碎片",fragIcon="◉",passive=true,skillName="寄生强化",skillIcon="◉",skillDesc="附身率+10%",effectId="passivePossess",value=0.1f},
            new FragmentDefinition{id="生命力被动",fragName="体质碎片",fragIcon="❤",passive=true,skillName="体质强化",skillIcon="❤",skillDesc="永久MaxHP+30",effectId="passiveHP",value=30f},
        };

        zonePools = new List<ZoneFragmentPool>
        {
            new ZoneFragmentPool{zoneId=1, fragmentIds=new List<string>{"护甲","再生","忠诚","厚皮","护甲被动","生命力被动"}},
            new ZoneFragmentPool{zoneId=2, fragmentIds=new List<string>{"吸血","狂暴","毒素","撕裂","掠夺","掠夺被动"}},
            new ZoneFragmentPool{zoneId=3, fragmentIds=new List<string>{"暴击","电击","恐惧","蛛网","伏击","恐惧被动","洞察被动"}},
            new ZoneFragmentPool{zoneId=4, fragmentIds=new List<string>{"召唤","寄生强化","反击","弹性","不死","反击被动","寄生被动"}},
            new ZoneFragmentPool{zoneId=5, fragmentIds=new List<string>{"掠夺","迅捷","相位","污染光环","爆炸","迅捷被动"}},
        };

        BuildLookup();
    }
}