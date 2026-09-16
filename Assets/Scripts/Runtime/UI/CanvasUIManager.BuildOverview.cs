using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    GameObject _buildOvl;
    Transform _buildContent;

    public void ShowBuildOverviewPanel()
    {
        if (_buildOvl == null) BuildBuildOverviewOverlay();
        SyncBuildOverview();
        OpenOverlayAnimated(_buildOvl);
    }

    void BuildBuildOverviewOverlay()
    {
        var content = BuildOverlayScaffold(ref _buildOvl, "BuildOvl", "⊞ 构筑总览", Cyan, OvlBgDense);
        var listGo = new GameObject("BuildContent", typeof(RectTransform), typeof(VerticalLayoutGroup));
        listGo.transform.SetParent(content, false);
        var lg = listGo.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 8;
        lg.padding = new RectOffset(4, 4, 4, 4);
        lg.childForceExpandWidth = true;
        lg.childForceExpandHeight = false;
        _buildContent = listGo.transform;
    }

    void SyncBuildOverview()
    {
        if (_buildContent == null) return;

        for (int i = _buildContent.childCount - 1; i >= 0; i--)
            DestroyImmediate(_buildContent.GetChild(i).gameObject);

        var player = GameManager.Instance?.Player;
        if (player == null) return;

        BuildSection_PollutionTier(player);
        BuildSection_ActiveResonances();
        BuildSection_Legacies();
        BuildSection_Proficiency(player);
        BuildSection_EvolutionInfo(player);
    }

    void BuildSection_PollutionTier(GameManager.PlayerData player)
    {
        BuildSectionSep("☢ 污染状态");

        var tier = PollutionPassiveSystem.GetPollutionTier(player.pollution);
        var card = CardGo(_buildContent, new Color(0.08f, 0.06f, 0.14f, 0.9f), 70);
        var vl = AddVL(card, 6, 6);

        var row1 = BuildHL(vl.transform, 6);
        Txt(row1, tier.icon, 18, tier.color, 24);
        Txt(row1, tier.label, 16, tier.color, 24);
        Txt(row1, $"({player.pollution:F0}%)", 13, Dim, 24);

        var row2 = BuildHL(vl.transform, 6);
        Txt(row2, $"ATK x{tier.atkMult:F1}", 12,
            tier.atkMult > 1f ? new Color(1f, 0.6f, 0.3f) : Dim, 18);
        Txt(row2, $"DEF x{tier.defMult:F2}", 12,
            tier.defMult < 1f ? SoftRed : new Color(0.5f, 0.8f, 0.5f), 18);

        if (player.pollution < 30f)
            Txt(vl.gameObject, "净化回血: 每回合+2%HP", 11, new Color(0.5f, 0.8f, 0.5f), 16);
        else if (player.pollution >= 85f)
            Txt(vl.gameObject, "遗产腐蚀风险 / 共鸣增幅+50%", 11, SoftRed, 16);
        else if (player.pollution >= 60f)
            Txt(vl.gameObject, "遗产效果+25% / 附身变异概率", 11, new Color(1f, 0.7f, 0f), 16);
    }

    void BuildSection_ActiveResonances()
    {
        var frs = FormResonanceSystem.Instance;
        if (frs == null) return;

        var effects = frs.GetAllActiveEffects();

        BuildSectionSep("◈ 共鸣效果");

        if (effects.Count == 0)
        {
            Txt(_buildContent.gameObject, "  无激活共鸣 (切换形态触发)", 11, Dim, 20);
            return;
        }

        foreach (var e in effects)
        {
            var card = CardGo(_buildContent, new Color(0.05f, 0.08f, 0.14f, 0.9f), 40);
            var vl = AddVL(card, 4, 4);
            var row = BuildHL(vl.transform, 6);
            Txt(row, "◈", 13, new Color(0f, 0.8f, 1f), 18);
            Txt(row, e.comboName, 12, Bright, 18);
            Txt(row, $"{e.turnsRemaining}回合", 11, new Color(1f, 0.85f, 0.4f), 18);
        }
    }

    void BuildSection_Legacies()
    {
        var mgr = LegacyManager.Instance;
        if (mgr == null) return;

        var equipped = mgr.GetEquippedLegacies();
        int maxSlots = mgr.GetMaxLegacies();

        BuildSectionSep($"★ 附身遗产 ({equipped.Count}/{maxSlots})");

        if (equipped.Count == 0)
        {
            Txt(_buildContent.gameObject, "  无遗产 (替换形态后获得)", 11, Dim, 20);
            return;
        }

        foreach (var leg in equipped)
        {
            Color rc = leg.rarity == 2 ? Purp : leg.rarity == 1 ? Gold : Bright;
            Color bg = leg.isMutated
                ? new Color(0.15f, 0.06f, 0.06f, 0.9f)
                : new Color(0.06f, 0.06f, 0.12f, 0.9f);

            var card = CardGo(_buildContent, bg, 44);
            var cvl = AddVL(card, 4, 2);
            var row = BuildHL(cvl.transform, 6);
            Txt(row, leg.icon, 13, rc, 20);
            Txt(row, leg.name, 12, rc, 20);
            if (leg.isMutated)
                Txt(row, "腐蚀", 9, SoftRed, 20);

            var valGo = Txt(row, $"+{leg.effectValue:F1}", 11, Dim, 20);
            var le = valGo.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            valGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
        }
    }

    void BuildSection_Proficiency(GameManager.PlayerData player)
    {
        var frs = FormResonanceSystem.Instance;
        if (frs == null) return;

        var profs = frs.GetAllProficiencies();
        if (profs.Count == 0 && player.ownedForms.Count <= 1) return;

        BuildSectionSep("◎ 形态熟练度");

        bool any = false;
        foreach (var form in player.ownedForms)
        {
            float bonus = frs.GetProficiencyBonus(form);
            if (bonus <= 0) continue;
            any = true;

            string formName = BuildFormName(form);
            bool isCurrent = form == player.currentFormId;
            Color nameColor = isCurrent ? Cyan : Bright;

            var card = CardGo(_buildContent, new Color(0.06f, 0.08f, 0.10f, 0.9f), 32);
            var pvl = AddVL(card, 4, 2);
            var row = BuildHL(pvl.transform, 6);
            if (isCurrent) Txt(row, "▶", 10, Cyan, 18);
            Txt(row, formName, 12, nameColor, 18);

            var pctGo = Txt(row, $"+{bonus * 100:F0}%", 12, new Color(0.6f, 0.85f, 1f), 18);
            var le = pctGo.AddComponent<LayoutElement>();
            le.flexibleWidth = 1;
            pctGo.GetComponent<Text>().alignment = TextAnchor.MiddleRight;
        }

        if (!any)
            Txt(_buildContent.gameObject, "  连续使用同一形态3回合提升", 11, Dim, 20);
    }

    void BuildSection_EvolutionInfo(GameManager.PlayerData player)
    {
        var cgs = CompleteGameSystem.Instance;
        if (cgs == null) return;

        int evoLv = cgs.EvolutionLevel;
        string cls = player.selectedClass;
        string clsName = cls == "titan" ? "泰坦" : cls == "swarm" ? "虫群" : cls == "ghost" ? "幽灵" : cls == "blood" ? "血族" : cls;

        BuildSectionSep($"⚡ 进化 Lv{evoLv} [{clsName}]");

        var card = CardGo(_buildContent, new Color(0.06f, 0.06f, 0.12f, 0.9f), 50);
        var vl = AddVL(card, 4, 6);

        if (evoLv >= 2)
            Txt(vl.gameObject, $"遗产槽位+{(evoLv >= 4 ? 2 : 1)}  效果x{(evoLv >= 4 ? "1.25" : "1.1")}", 11, Gold, 16);
        if (evoLv >= 3)
            Txt(vl.gameObject, "共鸣持续+1回合", 11, new Color(0f, 0.8f, 1f), 16);
        if (evoLv >= 5)
            Txt(vl.gameObject, "共鸣效果x1.5", 11, new Color(0f, 0.8f, 1f), 16);
        int nextCost = cgs.GetNextEvolutionNode().epCost;
        if (nextCost < 9999)
            Txt(vl.gameObject, $"EP: {player.evolutionPoints}/{nextCost}", 11, Dim, 16);
        else
            Txt(vl.gameObject, "已达最高进化", 11, Gold, 16);
    }

    void BuildSectionSep(string text)
    {
        Spacer(_buildContent.gameObject, 4);
        var sep = new GameObject("Sep", typeof(RectTransform), typeof(Image));
        sep.transform.SetParent(_buildContent, false);
        sep.AddComponent<LayoutElement>().preferredHeight = 1;
        sep.GetComponent<Image>().color = new Color(0f, 1f, 0.816f, 0.15f);
        Txt(_buildContent.gameObject, text, 14, Cyan, 22);
    }

    GameObject BuildHL(Transform parent, float spacing)
    {
        var go = new GameObject("HL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        go.transform.SetParent(parent, false);
        go.AddComponent<LayoutElement>().preferredHeight = 22;
        var hlg = go.GetComponent<HorizontalLayoutGroup>();
        hlg.spacing = spacing;
        hlg.childAlignment = TextAnchor.MiddleLeft;
        hlg.childForceExpandWidth = false;
        hlg.childForceExpandHeight = true;
        hlg.padding = new RectOffset(6, 6, 0, 0);
        return go;
    }

    string BuildFormName(string formId)
    {
        if (formId == "human") return "人类";
        if (GameDataImporter.MonsterDefinitions == null) return formId;
        var m = GameDataImporter.MonsterDefinitions.FirstOrDefault(x => x.id == formId);
        return string.IsNullOrEmpty(m.name) ? formId : m.name;
    }
}
