using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    public void ShowBestiaryPanel()
    {
        if (_bestOvl != null) Destroy(_bestOvl);
        var content = BuildOverlayScaffold(ref _bestOvl, "BestOvl", "异种图鉴", Gold, OvlBg, "bg_bestiary", "icon_bestiary");

        var seen = GameManager.Instance?.Player?.seenMonsterTypes;
        HashSet<string> seenSet;
        var metaProg = MetaProgressSystem.Instance;
        if (metaProg != null && metaProg.metaData.monstersEncountered?.Count > 0)
            seenSet = new HashSet<string>(metaProg.metaData.monstersEncountered);
        else
            seenSet = seen != null ? new HashSet<string>(seen) : new HashSet<string>();

        int discovered = 0;
        int total = 0;
        foreach (var m in GameDataImporter.MonsterDefinitions)
        {
            if (m.boss) continue;
            total++;
            if (seenSet.Contains(m.id)) discovered++;
        }

        var progressTxt = TxtGo(content.transform, $"已收集 <color=#00ffd0><b>{discovered}</b></color> / {total}", 13, Dim);
        progressTxt.alignment = TextAnchor.MiddleCenter;
        progressTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;

        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceTiny);

        foreach (var m in GameDataImporter.MonsterDefinitions)
        {
            if (m.boss) continue;

            bool found = seenSet.Contains(m.id);

            if (found)
            {
                var (card, cvl) = ClickCard(content.transform, CardBg, 70);

                var nameRow = new GameObject("NameRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
                nameRow.transform.SetParent(cvl.transform, false);
                nameRow.AddComponent<LayoutElement>().preferredHeight = 22;
                var nrHL = nameRow.GetComponent<HorizontalLayoutGroup>();
                nrHL.spacing = 6; nrHL.childAlignment = TextAnchor.MiddleLeft;
                nrHL.childForceExpandWidth = false;
                var nameTxt = TxtGo(nameRow.transform, m.name, 14, Bright);
                nameTxt.fontStyle = FontStyle.Bold;
                nameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
                var zoneTxt = TxtGo(nameRow.transform, $"Z{m.zone}", 11, Dim);
                zoneTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 28;

                Txt(cvl, $"HP {m.hp}  ATK {m.atk}  DEF {m.def}", 12, Dim, 18);

                string traits = m.traits != null ? string.Join(", ", m.traits) : "";
                if (!string.IsNullOrEmpty(traits))
                    Label(cvl, $"特性: {traits}", Purp);

                var monsterRef = m;
                card.GetComponent<Button>().onClick.AddListener(() => ShowMonsterDetail(monsterRef));
            }
            else
            {
                var (card, cvl) = InfoCard(content.transform, LockedCardBg, 70);
                Txt(cvl, "???", 14, new Color(0.3f, 0.3f, 0.4f), 22);
                Label(cvl, LocalizationData.T("未发现"), new Color(0.2f, 0.2f, 0.3f));
            }
        }

        _bestPanelBuilt = true;
        OpenOverlayAnimated(_bestOvl);
    }

    void ShowMonsterDetail((string id, string name, int hp, int atk, int def, int zone, bool boss, Color color, string[] traits, string[] axes) m)
    {
        GameObject detailOvl = null;
        var content = BuildOverlayScaffold(ref detailOvl, "MonsterDetailOvl", m.name, Gold, OvlBgDense, null, "icon_bestiary");
        detailOvl.transform.SetParent(_root, false);
        detailOvl.transform.SetAsLastSibling();

        // --- Archive header: PARASITE.ARCHIVE / T{zone} + zone name ---
        var archRow = new GameObject("ArchRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        archRow.transform.SetParent(content, false);
        archRow.GetComponent<LayoutElement>().preferredHeight = 20;
        var arHL = archRow.GetComponent<HorizontalLayoutGroup>();
        arHL.childForceExpandWidth = false; arHL.childAlignment = TextAnchor.MiddleLeft;
        var archLabel = TxtGo(archRow.transform, $"PARASITE.ARCHIVE / T{m.zone}", 10, Dim);
        archLabel.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;
        var zoneLabel = TxtGo(archRow.transform, GetZoneName(m.zone), 10, Dim);
        zoneLabel.alignment = TextAnchor.MiddleRight;

        // --- Icon + Name card ---
        var topCard = CardGo(content, DarkItemBg, 72);
        var topRow = new GameObject("TopRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        topRow.transform.SetParent(topCard.transform, false);
        Stretch(topRow);
        var trHL = topRow.GetComponent<HorizontalLayoutGroup>();
        trHL.spacing = 12; trHL.childAlignment = TextAnchor.MiddleLeft;
        trHL.childForceExpandWidth = false; trHL.childForceExpandHeight = false;
        trHL.padding = new RectOffset(10, 10, 6, 6);

        var iconWrap = new GameObject("IconWrap", typeof(RectTransform), typeof(Image), typeof(Outline), typeof(LayoutElement));
        iconWrap.transform.SetParent(topRow.transform, false);
        var iconLE = iconWrap.GetComponent<LayoutElement>();
        iconLE.preferredWidth = 60;
        iconLE.preferredHeight = 60;
        iconLE.minWidth = 60;
        iconLE.minHeight = 60;
        iconWrap.GetComponent<Image>().color = new Color(0.10f, 0.08f, 0.16f);
        iconWrap.GetComponent<Outline>().effectColor = new Color(m.color.r, m.color.g, m.color.b, 0.4f);
        iconWrap.GetComponent<Outline>().effectDistance = new Vector2(1, 1);

        var monsterTex = LoadTex($"Icons/Monsters/{m.id}");
        if (monsterTex != null)
        {
            var ri = BuildAspectIcon(iconWrap.transform, monsterTex, 5);
            ri.color = m.color;
        }

        var infoVL = new GameObject("InfoVL", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(LayoutElement));
        infoVL.transform.SetParent(topRow.transform, false);
        infoVL.GetComponent<LayoutElement>().flexibleWidth = 1;
        infoVL.GetComponent<LayoutElement>().preferredHeight = 60;
        var ivl = infoVL.GetComponent<VerticalLayoutGroup>();
        ivl.spacing = 2; ivl.childAlignment = TextAnchor.MiddleLeft;
        ivl.childForceExpandWidth = false; ivl.childForceExpandHeight = false;

        var nameTxt = TxtGo(infoVL.transform, m.name, 18, new Color(m.color.r * 1.2f, m.color.g * 1.2f, m.color.b * 1.2f));
        nameTxt.fontStyle = FontStyle.Bold;
        nameTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var nameLE = nameTxt.gameObject.AddComponent<LayoutElement>();
        nameLE.preferredHeight = 24;
        nameLE.preferredWidth = 120;
        nameLE.minHeight = 24;
        nameLE.minWidth = 60;
        nameLE.flexibleWidth = 1;
        var subTxt = TxtGo(infoVL.transform, $"区域 {m.zone}  |  {(m.boss ? "BOSS" : "普通异种")}", 11, Dim);
        subTxt.horizontalOverflow = HorizontalWrapMode.Overflow;
        var subLE = subTxt.gameObject.AddComponent<LayoutElement>();
        subLE.preferredHeight = 16;
        subLE.preferredWidth = 120;
        subLE.minHeight = 16;
        subLE.minWidth = 60;
        subLE.flexibleWidth = 1;
        var colorBar = new GameObject("ColorBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        colorBar.transform.SetParent(infoVL.transform, false);
        colorBar.GetComponent<LayoutElement>().preferredHeight = 2;
        colorBar.GetComponent<Image>().color = new Color(m.color.r, m.color.g, m.color.b, 0.6f);

        // --- Stats row ---
        var statsRow = new GameObject("StatsRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        statsRow.transform.SetParent(content, false);
        statsRow.GetComponent<LayoutElement>().preferredHeight = 56;
        var sHL = statsRow.GetComponent<HorizontalLayoutGroup>();
        sHL.spacing = 8; sHL.childForceExpandWidth = true; sHL.childForceExpandHeight = true;
        sHL.padding = new RectOffset(4, 4, 0, 0);

        CreateStatCard(statsRow.transform, "❤️", "HP", m.hp.ToString(), new Color(0.8f, 0.2f, 0.2f));
        CreateStatCard(statsRow.transform, "⚔️", "ATK", m.atk.ToString(), new Color(0.2f, 0.6f, 0.8f));
        CreateStatCard(statsRow.transform, "◆", "DEF", m.def.ToString(), new Color(0.4f, 0.7f, 0.4f));

        // --- Lore / Archive description (with orange left bar) ---
        string lore = GetMonsterLore(m.id);
        var loreRow = new GameObject("LoreRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        loreRow.transform.SetParent(content, false);
        loreRow.GetComponent<LayoutElement>().minHeight = 50;
        var lrHL = loreRow.GetComponent<HorizontalLayoutGroup>();
        lrHL.spacing = 8; lrHL.childForceExpandWidth = false; lrHL.childForceExpandHeight = true;
        lrHL.padding = new RectOffset(4, 4, 4, 4);

        var loreBar = new GameObject("LoreBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        loreBar.transform.SetParent(loreRow.transform, false);
        loreBar.GetComponent<LayoutElement>().preferredWidth = 3;
        loreBar.GetComponent<Image>().color = new Color(1f, 0.6f, 0.2f, 0.8f);

        var loreTxt = TxtGo(loreRow.transform, lore, 12, new Color(0.75f, 0.7f, 0.8f));
        loreTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        // --- Traits with detail ---
        if (m.traits != null && m.traits.Length > 0)
        {
            Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceTiny);
            SectionHeader(content.GetComponent<VerticalLayoutGroup>(), "技能 / 特性", Gold, $"x{m.traits.Length}");

            foreach (string traitName in m.traits)
            {
                string traitType = GetTraitType(traitName);
                string traitDesc = GetTraitDescription(traitName);

                var (tc, tcvl) = InfoCard(content, LockedCardBg, 60);

                var nameRowGo = new GameObject("NameRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
                nameRowGo.transform.SetParent(tcvl.transform, false);
                nameRowGo.GetComponent<LayoutElement>().preferredHeight = 20;
                var nrHL = nameRowGo.GetComponent<HorizontalLayoutGroup>();
                nrHL.spacing = 6; nrHL.childAlignment = TextAnchor.MiddleLeft;
                nrHL.childForceExpandWidth = false;

                var arrow = TxtGo(nameRowGo.transform, "▶", 11, Cyan);
                arrow.gameObject.AddComponent<LayoutElement>().preferredWidth = 14;
                var tn = TxtGo(nameRowGo.transform, traitName, 14, Gold);
                tn.fontStyle = FontStyle.Bold;

                if (!string.IsNullOrEmpty(traitType))
                {
                    var tagGo = new GameObject("Tag", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
                    tagGo.transform.SetParent(nameRowGo.transform, false);
                    tagGo.GetComponent<LayoutElement>().preferredWidth = 36;
                    tagGo.GetComponent<LayoutElement>().preferredHeight = 18;
                    tagGo.GetComponent<Image>().color = new Color(0.15f, 0.12f, 0.22f);
                    var tagTxt = TxtGo(tagGo.transform, traitType, 9, Dim);
                    tagTxt.alignment = TextAnchor.MiddleCenter;
                    Stretch(tagTxt.gameObject);
                }

                Txt(tcvl, traitDesc, 11, Bright, 16);
            }
        }

        // --- Axes ---
        if (m.axes != null && m.axes.Length > 0)
        {
            Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceTiny);
            SectionHeader(content.GetComponent<VerticalLayoutGroup>(), "构造轴", Purp);

            foreach (string axis in m.axes)
            {
                var (ac, acvl) = InfoCard(content, new Color(0.08f, 0.05f, 0.12f), 32);
                Txt(acvl, axis, 12, Dim, 18);
            }
        }

        // --- Footer buttons ---
        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceSmall);
        var footRow = new GameObject("FootRow", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        footRow.transform.SetParent(content, false);
        footRow.GetComponent<LayoutElement>().preferredHeight = 44;
        var fHL = footRow.GetComponent<HorizontalLayoutGroup>();
        fHL.spacing = 10; fHL.childForceExpandWidth = true; fHL.childForceExpandHeight = true;

        var backBtn = PrimaryBtn(footRow.transform, "返回图鉴", 14, 44);
        backBtn.GetComponent<Button>().onClick.AddListener(() => { StartCoroutine(CloseOverlayAnimated(detailOvl)); ShowBestiaryPanel(); });

        var closeBtn = DangerBtn(footRow.transform, "关闭", 14, 44);
        closeBtn.GetComponent<Button>().onClick.AddListener(() => StartCoroutine(CloseOverlayAnimated(detailOvl)));

        detailOvl.SetActive(false);
        OpenOverlayAnimated(detailOvl);
    }

    string GetMonsterLore(string id)
    {
        switch (id)
        {
            case "rat": return "实验室逃逸的初代试验体。神经系统被基因改造剂腐蚀，行动反常迅捷。";
            case "spider": return "在废弃通风管道中繁殖的变异蛛类。丝线具有微弱的神经毒性。";
            case "bat": return "栖息于塔内阴暗角落的飞行生物。回声定位系统被污染物增强。";
            case "slime": return "由泄漏的实验液体凝聚而成的不定形体。能缓慢溶解有机物。";
            case "wolf": return "被培育出的战斗型试验体。保留了狼群的狩猎本能，极具攻击性。";
            case "snake": return "基因改造后的爬行类，毒腺经过人工强化，毒液可侵蚀金属。";
            case "boss1": return "实验区域的守护者。由多个试验体融合而成的畸形巨兽。";
            case "beetle": return "甲壳经过矿物质沉积硬化的变异甲虫。防御力极高。";
            case "mantis": return "前肢进化为锋利刃臂的捕食者。一击必杀是它的狩猎哲学。";
            case "worm": return "潜伏在墙壁裂缝中的寄生型生物。能感知宿主的生命力。";
            case "moth": return "翅膀覆盖着致幻鳞粉的夜行生物。靠近时会产生短暂幻觉。";
            case "scorpion": return "尾针注射的不是毒液，而是一种能改写神经信号的纳米机器。";
            case "hydra": return "每次受伤都会刺激细胞分裂的再生型异种。越战越强。";
            case "boss2": return "生物实验室深处沉睡的古老实验体。苏醒后展现出压倒性的力量。";
            case "shade": return "由高浓度污染凝聚成的暗影实体。几乎没有物理形态。";
            case "lurker": return "潜伏在阴影中的捕猎者。受害者往往在被发现时已经太迟。";
            case "wraith": return "失败实验体残留的意识碎片。被怨念驱使着在走廊中游荡。";
            case "voidbeast": return "从虚空裂缝中渗出的未知生物。身体结构不符合已知生物学。";
            case "nightmare": return "能入侵意识的精神寄生体。与它对视会看到自己最深的恐惧。";
            case "watcher": return "漂浮在空中的巨大眼球。似乎在记录着塔内发生的一切。";
            case "voiddragon": return "传说中虚空深处的至高存在。仅仅是它的气息就能扭曲现实。";
            case "boss3": return "污染核心的守卫者。被纯粹的污染能量所驱动。";
            case "titan": return "远古时代遗留的巨型构造体。动力源至今仍在运转。";
            case "chaos": return "因果律崩坏区域自然诞生的混沌实体。行为完全无法预测。";
            case "deathknight": return "曾经的塔内守卫，死后被污染能量复活。保留着生前的战斗技巧。";
            case "horror": return "直视它的人都会失去理智。没有人能准确描述它的外貌。";
            case "colossus": return "由无数小型异种融合而成的巨大集合体。每个细胞都是独立的生命。";
            case "plague": return "体内携带着能改写基因的病毒。靠近它的生物都会开始变异。";
            case "origin": return "最初的寄生体原型。所有异种的基因都源自于它。";
            case "boss4": return "数据中心的核心防御系统。半机械半生物的终极造物。";
            case "boss5": return "塔顶的最终存在。它就是寄生塔本身的意志。";
            default: return "档案数据缺失。该异种的详细信息尚未被记录。";
        }
    }

    string GetTraitType(string traitName)
    {
        switch (traitName)
        {
            case "迅捷": return "机动";
            case "厚皮": case "护甲": case "弹性": return "防御";
            case "再生": case "再生+": case "吸血": case "吸取": return "回复";
            case "狂暴": case "暴击": case "伏击": case "多重攻击": case "蓄力": return "攻击";
            case "电击": case "蛛网": case "恐惧": case "腐蚀": return "控制";
            case "反击": case "爆炸": return "反击";
            case "撕裂": case "毒素": case "剧毒": case "污染光环": return "持续";
            case "不死": case "相位": return "生存";
            case "召唤": case "领袖": return "辅助";
            case "忠诚": case "寄生强化": case "掠夺": return "被动";
            default: return "";
        }
    }

    void CreateStatCard(Transform parent, string icon, string label, string value, Color color)
    {
        var card = new GameObject($"StatCard_{label}", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        card.transform.SetParent(parent, false);
        card.GetComponent<LayoutElement>().flexibleWidth = 1;
        card.GetComponent<Image>().color = color * 0.15f;

        var cardVL = new GameObject("CardVL", typeof(RectTransform), typeof(VerticalLayoutGroup));
        cardVL.transform.SetParent(card.transform, false);
        Stretch(cardVL);
        var cardVLG = cardVL.GetComponent<VerticalLayoutGroup>();
        cardVLG.spacing = 1;
        cardVLG.padding = new RectOffset(4, 4, 4, 4);
        cardVLG.childAlignment = TextAnchor.MiddleCenter;
        cardVLG.childForceExpandWidth = true;
        cardVLG.childForceExpandHeight = false;

        var valTxt = TxtGo(cardVL.transform, value, 18, color);
        valTxt.fontStyle = FontStyle.Bold;
        valTxt.alignment = TextAnchor.MiddleCenter;
        valTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 24;
        var lblTxt = TxtGo(cardVL.transform, label, 10, Dim);
        lblTxt.alignment = TextAnchor.MiddleCenter;
        lblTxt.gameObject.AddComponent<LayoutElement>().preferredHeight = 14;
    }

    string GetTraitTypeLabel(string type)
    {
        switch (type)
        {
            case "passive": return "被动";
            case "active": return "主动";
            case "reaction": return "反应";
            case "move": return "机动";
            default: return type;
        }
    }

    string GetMonsterIcon(string monsterId)
    {
        switch (monsterId)
        {
            case "rat": return "←";
            case "roach": return "→";
            case "slime": return "◯";
            case "dog": return "♠";
            case "gecko": return "~";
            case "drone": return "※";
            case "boss1": return "☠";
            case "wolf": return "♦";
            case "spider": return "×";
            case "bat": return "▼";
            case "wasp": return "►";
            case "guard": return "◆";
            case "vine": return "♣";
            case "boss2": return "☆";
            case "larva": return "·";
            case "mantis": return "†";
            case "beetle": return "‡";
            case "worm": return "÷";
            case "moth": return "◇";
            case "scorpion": return "♜";
            case "hydra": return "♛";
            case "boss3": return "⊕";
            case "shade": return "●";
            case "lurker": return "□";
            case "wraith": return "◎";
            case "voidbeast": return "■";
            case "nightmare": return "▲";
            case "watcher": return "◉";
            case "voiddragon": return "⊙";
            case "boss4": return "◈";
            case "titan": return "↓";
            case "chaos": return "○";
            case "deathknight": return "⚰";
            case "horror": return "♥";
            case "colossus": return "◄";
            case "plague": return "☣";
            case "origin": return "★";
            case "boss5": return "⭐";
            case "human": return "↑";
            default: return "❓";
        }
    }
}
