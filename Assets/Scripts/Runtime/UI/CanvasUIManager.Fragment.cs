using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    public void ShowFragmentPanel()
    {
        if (_fragOvl != null) Destroy(_fragOvl);
        var p = GameManager.Instance?.Player;
        Dictionary<string, bool> flags = new Dictionary<string, bool>();

        if (p != null && p.storyFlags != null)
        {
            flags = p.storyFlags;
        }
        else
        {
            string saveData = PlayerPrefs.GetString("pt_save_classic", "");
            if (!string.IsNullOrEmpty(saveData))
            {
                try
                {
                    var pd = JsonUtility.FromJson<SaveData>(saveData);
                    if (pd.player != null && pd.player.storyFlags != null)
                    {
                        foreach (var kvp in pd.player.storyFlags)
                        {
                            flags[kvp.key] = kvp.value;
                        }
                    }
                }
                catch { }
            }
        }

        var fragContent = BuildOverlayScaffold(ref _fragOvl, "FragOvl", "记忆档案", new Color(0.85f, 0.35f, 0.85f), OvlBg, "bg_archive", "icon_archive");
        var content = fragContent;

        int[] mainFloors = { 5, 10, 15, 20, 25, 30, 35, 40, 45, 50 };
        string[] mainTitles = { "觉醒", "起源", "裂痕", "深渊", "真相", "终焉", "回归", "超越", "虚空", "重生" };
        string[] hiddenIds = { "echo_corridor", "mirror_whisper", "parasite_resonance", "death_memory" };
        string[] hiddenNames = { "记忆裂隙", "虚空低语", "寄生共鸣", "死亡记忆" };

        int mainCount = 0;
        foreach (int f in mainFloors)
        {
            if (flags.ContainsKey("floor_" + f)) mainCount++;
        }

        int hiddenCount = 0;
        foreach (string id in hiddenIds)
        {
            if (flags.ContainsKey("hidden_" + id)) hiddenCount++;
        }

        int wallGiftCount = flags.Keys.Count(k => k.StartsWith("hidden_wall_gift_"));
        int total = mainFloors.Length + hiddenIds.Length + 1;
        int found = mainCount + hiddenCount + (wallGiftCount > 0 ? 1 : 0);

        Txt(content.GetComponent<VerticalLayoutGroup>(), "收集宿主残留记忆，解锁被遗忘的真相", 12, Dim, 18);

        // 进度条卡片
        var (progressCard, progressVL) = InfoCard(content, CardBg, 50);

        var progressRow = new GameObject("ProgressRow", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        progressRow.transform.SetParent(progressVL.transform, false);
        progressRow.AddComponent<LayoutElement>().preferredHeight = 20;
        var progressHLG = progressRow.GetComponent<HorizontalLayoutGroup>();
        progressHLG.spacing = 10;
        progressHLG.childAlignment = TextAnchor.MiddleLeft;
        progressHLG.childForceExpandWidth = false;

        var progressLabel = TxtGo(progressRow.transform, "收集进度", 12, Dim);
        progressLabel.gameObject.AddComponent<LayoutElement>().preferredWidth = 60;
        var progressNum = TxtGo(progressRow.transform, $"<color=#00ffff><b>{found}</b></color> / {total}", 13, Bright);
        progressNum.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

        var progressBarGo = new GameObject("ProgressBar", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        progressBarGo.transform.SetParent(progressVL.transform, false);
        progressBarGo.GetComponent<LayoutElement>().preferredHeight = 8;
        progressBarGo.GetComponent<Image>().color = new Color(0.1f, 0.15f, 0.2f);

        var progressFill = new GameObject("ProgressFill", typeof(RectTransform), typeof(Image));
        progressFill.transform.SetParent(progressBarGo.transform, false);
        var progressFillRT = progressFill.GetComponent<RectTransform>();
        progressFillRT.anchorMin = Vector2.zero;
        progressFillRT.anchorMax = new Vector2(Mathf.Clamp01((float)found / total), 1);
        progressFillRT.offsetMin = Vector2.zero;
        progressFillRT.offsetMax = Vector2.zero;
        progressFill.GetComponent<Image>().color = new Color(0.2f, 0.8f, 0.8f);

        // 主线剧情 section
        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceSmall);
        SectionHeader(content.GetComponent<VerticalLayoutGroup>(), "主线剧情", Cyan, $"{mainCount}/{mainFloors.Length}");

        for (int i = 0; i < mainFloors.Length; i++)
        {
            int floor = mainFloors[i];
            bool unlocked = flags.ContainsKey("floor_" + floor);

            if (unlocked)
            {
                var (card, cvl) = ClickCard(content, CardBg, 52);

                var row = ListRow(cvl.transform, 26);
                row.GetComponent<HorizontalLayoutGroup>().spacing = 10;

                string idStr = "F" + (floor < 10 ? "0" + floor : floor.ToString());
                var idTxt = TxtGo(row.transform, idStr, 12, Cyan);
                idTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 32;

                var nameTxt = TxtGo(row.transform, mainTitles[i], 14, Bright);
                nameTxt.fontStyle = FontStyle.Bold;
                nameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

                var statusTxt = TxtGo(row.transform, "✓", 14, SuccessGreen);
                statusTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;

                int capturedI = i;
                card.GetComponent<Button>().onClick.AddListener(() => ShowMemoryDetail("主线", idStr, mainTitles[capturedI], floor));
            }
            else
            {
                var (card, cvl) = InfoCard(content, LockedCardBg, 52);

                var row = ListRow(cvl.transform, 26);
                row.GetComponent<HorizontalLayoutGroup>().spacing = 10;

                string idStr = "F" + (floor < 10 ? "0" + floor : floor.ToString());
                var idTxt = TxtGo(row.transform, idStr, 12, Dim);
                idTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 32;

                var nameTxt = TxtGo(row.transform, "???", 14, Dim);
                nameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

                var statusTxt = TxtGo(row.transform, "○", 14, Dim);
                statusTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;
            }
        }

        // 隐藏事件 section
        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceSmall);
        SectionHeader(content.GetComponent<VerticalLayoutGroup>(), "隐藏事件", new Color(0.85f, 0.6f, 0.9f), $"{hiddenCount}/{hiddenIds.Length}");

        for (int i = 0; i < hiddenIds.Length; i++)
        {
            bool unlocked = flags.ContainsKey("hidden_" + hiddenIds[i]);

            if (unlocked)
            {
                var (card, cvl) = ClickCard(content, new Color(0.1f, 0.06f, 0.16f), 52);

                var row = ListRow(cvl.transform, 26);
                row.GetComponent<HorizontalLayoutGroup>().spacing = 10;

                string idStr = "H" + (i + 1 < 10 ? "0" + (i + 1) : (i + 1).ToString());
                var idTxt = TxtGo(row.transform, idStr, 12, new Color(0.85f, 0.4f, 0.85f));
                idTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 32;

                var nameTxt = TxtGo(row.transform, hiddenNames[i], 14, Bright);
                nameTxt.fontStyle = FontStyle.Bold;
                nameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

                var statusTxt = TxtGo(row.transform, "✓", 14, new Color(0.85f, 0.4f, 0.85f));
                statusTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;

                int capturedI = i;
                card.GetComponent<Button>().onClick.AddListener(() => ShowMemoryDetail("隐藏", idStr, hiddenNames[capturedI], capturedI + 1));
            }
            else
            {
                var (card, cvl) = InfoCard(content, LockedCardBg, 52);

                var row = ListRow(cvl.transform, 26);
                row.GetComponent<HorizontalLayoutGroup>().spacing = 10;

                string idStr = "H" + (i + 1 < 10 ? "0" + (i + 1) : (i + 1).ToString());
                var idTxt = TxtGo(row.transform, idStr, 12, Dim);
                idTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 32;

                var nameTxt = TxtGo(row.transform, "???", 14, Dim);
                nameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

                var statusTxt = TxtGo(row.transform, "○", 14, Dim);
                statusTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;
            }
        }

        // 馈赠 section
        Spacer(content.GetComponent<VerticalLayoutGroup>(), SpaceSmall);
        SectionHeader(content.GetComponent<VerticalLayoutGroup>(), "馈赠", new Color(1f, 0.7f, 0.3f));

        bool wallGiftUnlocked = wallGiftCount > 0;

        if (wallGiftUnlocked)
        {
            var (wallCard, wallCvl) = ClickCard(content, new Color(0.12f, 0.08f, 0.1f), 52);

            var wallRow = ListRow(wallCvl.transform, 26);
            wallRow.GetComponent<HorizontalLayoutGroup>().spacing = 10;

            var wallNameTxt = TxtGo(wallRow.transform, "神秘馈赠", 14, Bright);
            wallNameTxt.fontStyle = FontStyle.Bold;
            wallNameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var wallStatusTxt = TxtGo(wallRow.transform, "✓", 14, new Color(1f, 0.7f, 0.3f));
            wallStatusTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;

            wallCard.GetComponent<Button>().onClick.AddListener(() => ShowMemoryDetail("馈赠", "WG", "神秘馈赠", 0));
        }
        else
        {
            var (wallCard, wallCvl) = InfoCard(content, LockedCardBg, 52);

            var wallRow = ListRow(wallCvl.transform, 26);
            wallRow.GetComponent<HorizontalLayoutGroup>().spacing = 10;

            var wallNameTxt = TxtGo(wallRow.transform, "???", 14, Dim);
            wallNameTxt.gameObject.AddComponent<LayoutElement>().flexibleWidth = 1;

            var wallStatusTxt = TxtGo(wallRow.transform, "○", 14, Dim);
            wallStatusTxt.gameObject.AddComponent<LayoutElement>().preferredWidth = 24;
        }

        OpenOverlayAnimated(_fragOvl);
    }

    void ShowMemoryDetail(string type, string id, string name, int index)
    {
        var detailOvl = new GameObject("MemoryDetailOvl", typeof(RectTransform), typeof(Image));
        detailOvl.transform.SetParent(_root, false);
        detailOvl.transform.SetAsLastSibling();
        var rt = detailOvl.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        var img = detailOvl.GetComponent<Image>();
        img.color = new Color(0, 0, 0, 0.95f);

        var content = new GameObject("DetailContent", typeof(RectTransform), typeof(Image), typeof(VerticalLayoutGroup));
        content.transform.SetParent(detailOvl.transform, false);
        var contentRT = content.GetComponent<RectTransform>();
        contentRT.anchorMin = new Vector2(0.08f, 0.15f);
        contentRT.anchorMax = new Vector2(0.92f, 0.88f);

        var contentImg = content.GetComponent<Image>();
        contentImg.color = new Color(0.06f, 0.04f, 0.1f);
        var contentVL = content.GetComponent<VerticalLayoutGroup>();
        contentVL.spacing = 12;
        contentVL.padding = new RectOffset(16, 16, 16, 16);

        var headerHL = new GameObject("HeaderHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        headerHL.transform.SetParent(content.transform, false);
        var headerHLG = headerHL.GetComponent<HorizontalLayoutGroup>();
        headerHLG.spacing = 8;
        headerHLG.childForceExpandWidth = false;

        string icon = type == "主线" ? "►" : type == "隐藏" ? "◈" : "♦";
        TxtGo(headerHL.transform, icon, 18, type == "主线" ? Cyan : type == "隐藏" ? new Color(0.85f, 0.35f, 0.85f) : new Color(1f, 0.7f, 0.3f));
        TxtGo(headerHL.transform, id, 14, Dim);

        var closeBtn = BtnGo(headerHL.transform, "×", 16, Dim, 32);
        closeBtn.GetComponent<LayoutElement>().preferredWidth = 32;
        closeBtn.GetComponent<Button>().onClick.AddListener(() => Destroy(detailOvl));

        TxtGo(content.transform, name, 18, Bright);

        var divider = new GameObject("Divider", typeof(RectTransform), typeof(Image));
        divider.transform.SetParent(content.transform, false);
        var dividerRT = divider.GetComponent<RectTransform>();
        dividerRT.sizeDelta = new Vector2(0, 1);
        divider.GetComponent<Image>().color = new Color(0.2f, 0.2f, 0.35f);

        string[] mainLores = {
            "觉醒：宿主意识开始觉醒，寄生体与宿主初次接触。",
            "起源：实验室的真相逐渐浮出水面，一切开始的地方。",
            "裂痕：现实与虚空的边界开始模糊，异常现象频发。",
            "深渊：深入地下设施，发现更古老的秘密。",
            "真相：揭开寄生体的真正目的与宿主的命运。",
            "终焉：最终的决战即将到来，命运的十字路口。",
            "回归：一切回归原点，但有些东西已经改变。",
            "超越：超越生死，超越寄生，超越一切。",
            "虚空：虚空的真相，万物的尽头。",
            "重生：新的开始，或者是另一个轮回的起点。"
        };

        string[] hiddenLores = {
            "记忆裂隙：在特定条件下出现的时空裂隙，连接着不同的记忆碎片。",
            "虚空低语：来自虚空深处的声音，包含着古老的知识。",
            "寄生共鸣：与寄生体产生共鸣，获得超越凡人的力量。",
            "死亡记忆：记录着无数死亡的记忆，每一个都是警示。"
        };

        string lore = "";
        if (type == "主线" && index >= 1 && index <= mainLores.Length)
        {
            lore = mainLores[index - 1];
        }
        else if (type == "隐藏" && index >= 1 && index <= hiddenLores.Length)
        {
            lore = hiddenLores[index - 1];
        }
        else if (type == "馈赠")
        {
            lore = "神秘的馈赠，来自虚空深处的礼物。它包含着未知的力量与秘密。";
        }

        if (string.IsNullOrEmpty(lore))
        {
            lore = "暂无更多档案记录。";
        }

        TxtGo(content.transform, lore, 13, new Color(0.75f, 0.7f, 0.8f));

        var footerHL = new GameObject("FooterHL", typeof(RectTransform), typeof(HorizontalLayoutGroup));
        footerHL.transform.SetParent(content.transform, false);
        var footerHLG = footerHL.GetComponent<HorizontalLayoutGroup>();
        footerHLG.spacing = 12;
        footerHLG.childForceExpandWidth = true;

        var backBtn = BtnGo(footerHL.transform, "返回", 13, Dim, 40);
        backBtn.GetComponent<Button>().onClick.AddListener(() => { Destroy(detailOvl); ShowFragmentPanel(); });

        var closeBtn2 = BtnGo(footerHL.transform, "关闭", 13, Dim, 40);
        closeBtn2.GetComponent<Button>().onClick.AddListener(() => Destroy(detailOvl));
    }
}
