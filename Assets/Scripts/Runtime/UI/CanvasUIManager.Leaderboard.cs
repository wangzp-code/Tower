using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

public partial class CanvasUIManager
{
    public void ShowLeaderboardPanel()
    {
        // 清理旧的UI
        if (_rankOvl != null)
        {
            DestroyImmediate(_rankOvl);
            _rankOvl = null;
        }
        _rankBody = null;
        _rankTabs = null;
        _rankSubTabs = null;

        BuildLeaderboardOverlay();
        OpenOverlayAnimated(_rankOvl);
    }

    bool _rankBuilt;
    Transform _rankBody;
    Transform _rankTabs;
    Transform _rankSubTabs;
    LeaderboardState _lbState = new LeaderboardState();

    class LeaderboardState
    {
        public string tab = "score";
        public string period = "all";
        public string cls = "swarm";
    }

    void BuildLeaderboardOverlay()
    {
        var content = BuildOverlayScaffold(ref _rankOvl, "RankOvl", "暗塔排行", Cyan, OvlBg, "bg_ranking", "icon_ranking");

        _rankTabs = new GameObject("Tabs", typeof(RectTransform), typeof(HorizontalLayoutGroup)).transform;
        _rankTabs.SetParent(content, false);
        _rankTabs.gameObject.AddComponent<LayoutElement>().preferredHeight = 32;
        var tabsHL = _rankTabs.GetComponent<HorizontalLayoutGroup>();
        tabsHL.spacing = 3;
        tabsHL.childForceExpandWidth = true;
        tabsHL.childForceExpandHeight = true;
        tabsHL.childAlignment = TextAnchor.MiddleCenter;

        _rankSubTabs = new GameObject("SubTabs", typeof(RectTransform), typeof(HorizontalLayoutGroup)).transform;
        _rankSubTabs.SetParent(content, false);
        _rankSubTabs.gameObject.AddComponent<LayoutElement>().preferredHeight = 28;
        var subHL = _rankSubTabs.GetComponent<HorizontalLayoutGroup>();
        subHL.spacing = 5;
        subHL.childForceExpandWidth = false;
        subHL.childForceExpandHeight = true;
        subHL.childAlignment = TextAnchor.MiddleLeft;

        _rankBody = new GameObject("Body", typeof(RectTransform), typeof(VerticalLayoutGroup)).transform;
        _rankBody.SetParent(content, false);
        var bodyLE = _rankBody.gameObject.AddComponent<LayoutElement>();
        bodyLE.preferredHeight = 400;
        bodyLE.flexibleHeight = 1;
        var bodyVL = _rankBody.GetComponent<VerticalLayoutGroup>();
        bodyVL.spacing = 2;
        bodyVL.childForceExpandWidth = true;
        bodyVL.childForceExpandHeight = false;

        BuildLeaderboardTabs();
    }

    void BuildLeaderboardTabs()
    {
        // 清理旧标签（倒序避免跳过）
        for (int i = _rankTabs.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankTabs.GetChild(i).gameObject);
        for (int i = _rankSubTabs.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankSubTabs.GetChild(i).gameObject);
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        string[][] tabs = {
            new[] { "SCORE", "score" },
            new[] { "SPEED", "speed" },
            new[] { "DAILY", "daily" },
            new[] { "CLASS", "class" },
            new[] { "ME", "me" }
        };

        foreach (var tab in tabs)
        {
            bool active = tab[1] == _lbState.tab;
            var btn = CreateLeaderboardTab(_rankTabs, tab[0], tab[1], active);
            btn.GetComponent<Button>().onClick.AddListener(() => { _lbState.tab = tab[1]; BuildLeaderboardTabs(); });
        }

        if (_lbState.tab == "score")
        {
            string[][] periods = {
                new[] { "ALL", "all" },
                new[] { "WEEK", "week" },
                new[] { "DAY", "day" }
            };
            foreach (var period in periods)
            {
                bool active = period[1] == _lbState.period;
                var btn = CreateLeaderboardSubTab(_rankSubTabs, period[0], period[1], active);
                btn.GetComponent<Button>().onClick.AddListener(() => { _lbState.period = period[1]; BuildLeaderboardTabs(); });
            }
        }
        else if (_lbState.tab == "class")
        {
            string[][] classes = {
                new[] { "SWARM", "swarm" },
                new[] { "TITAN", "titan" },
                new[] { "GHOST", "ghost" },
                new[] { "BLOOD", "blood" },
                new[] { "MECH", "mech" }
            };
            foreach (var cls in classes)
            {
                bool active = cls[1] == _lbState.cls;
                var btn = CreateLeaderboardSubTab(_rankSubTabs, cls[0], cls[1], active);
                btn.GetComponent<Button>().onClick.AddListener(() => { _lbState.cls = cls[1]; BuildLeaderboardTabs(); });
            }
        }

        ShowLoadingSkeleton();
        LoadLeaderboardData();
    }

    GameObject CreateLeaderboardTab(Transform parent, string label, string key, bool active)
    {
        var btn = new GameObject("Tab_" + key, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);

        var img = btn.GetComponent<Image>();
        img.color = active ? new Color(0.05f, 0.28f, 0.32f) : DarkItemBg;

        var txt = TxtGo(btn.transform, label, 12, active ? Cyan : SteelBlue);
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);

        if (active)
        {
            var ol = btn.AddComponent<Outline>();
            ol.effectColor = new Color(0f, 1f, 0.82f, 0.45f);
            ol.effectDistance = new Vector2(1, 1);
        }

        var btnComp = btn.GetComponent<Button>();
        btnComp.targetGraphic = img;

        return btn;
    }

    GameObject CreateLeaderboardSubTab(Transform parent, string label, string key, bool active)
    {
        var btn = new GameObject("SubTab_" + key, typeof(RectTransform), typeof(Image), typeof(Button));
        btn.transform.SetParent(parent, false);

        var img = btn.GetComponent<Image>();
        img.color = active ? new Color(0.12f, 0.08f, 0.04f) : Color.clear;

        var txt = TxtGo(btn.transform, label, 11, active ? Gold : SlateText);
        txt.alignment = TextAnchor.MiddleCenter;
        Stretch(txt.gameObject);

        var le = btn.AddComponent<LayoutElement>();
        le.preferredWidth = 70;
        le.preferredHeight = 24;
        le.flexibleWidth = 0;
        le.flexibleHeight = 0;

        var btnComp = btn.GetComponent<Button>();
        btnComp.targetGraphic = img;

        return btn;
    }

    void ShowLoadingSkeleton()
    {
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        var skeleton = new GameObject("Skeleton", typeof(RectTransform));
        skeleton.transform.SetParent(_rankBody, false);
        var rt = skeleton.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;
        rt.offsetMin = Vector2.zero;
        rt.offsetMax = Vector2.zero;

        var vl = skeleton.AddComponent<VerticalLayoutGroup>();
        vl.childAlignment = TextAnchor.MiddleCenter;

        var icon = TxtGo(skeleton.transform, "⌛", 22, SlateText);
        icon.alignment = TextAnchor.MiddleCenter;
        icon.fontStyle = FontStyle.Normal;

        Txt(skeleton.transform, "加载中…", 12, SlateText, 24);
    }

    async void LoadLeaderboardData()
    {
        try
        {
            if (!LeaderboardAPI.Instance.HasAPI())
            {
                ShowLocalFallback("本地记录（未连接服务器）");
                return;
            }

            var reqTab = _lbState.tab;
            var reqSub = _lbState.tab == "score" ? _lbState.period : (_lbState.tab == "class" ? _lbState.cls : "");

            object result = null;
            if (_lbState.tab == "score")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("score", _lbState.period);
            else if (_lbState.tab == "speed")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("speed");
            else if (_lbState.tab == "daily")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("daily");
            else if (_lbState.tab == "class")
                result = await LeaderboardAPI.Instance.FetchLeaderboard("class", cls: _lbState.cls);
            else if (_lbState.tab == "me")
            {
                string uid = PlayerPrefs.GetString("PT_UID", "");
                if (string.IsNullOrEmpty(uid))
                {
                    ShowNoIdentityMessage();
                    return;
                }
                result = await LeaderboardAPI.Instance.FetchMyLeaderboard(uid);
            }

            if (_lbState.tab != reqTab) return;
            if (_lbState.tab == "score" && _lbState.period != reqSub) return;
            if (_lbState.tab == "class" && _lbState.cls != reqSub) return;

            for (int ci = _rankBody.childCount - 1; ci >= 0; ci--)
                DestroyImmediate(_rankBody.GetChild(ci).gameObject);

            if (_lbState.tab == "me")
                RenderMyLeaderboard(result as PlayerLeaderboardData);
            else
                RenderLeaderboardRows((result as LeaderboardResponse)?.rows);

        }
        catch (Exception e)
        {

            ShowLocalFallback("无法连接服务器，显示本地记录");
        }
    }

    void RenderLeaderboardRows(List<LeaderboardRow> rows)
    {
        if (rows == null || rows.Count == 0)
        {
            Txt(_rankBody, "★", 28, new Color(0.3f, 0.3f, 0.3f), 36);
            Txt(_rankBody, "暂无记录", 13, SteelBlue, 24);
            Txt(_rankBody, "完成一次短局即可上榜", 11, DarkStatText, 18);
            return;
        }

        bool isSpeed = _lbState.tab == "speed";
        string myUid = PlayerPrefs.GetString("PT_UID", "");

        // 表头
        var hdr = CreateLBRow(_rankBody, 22);
        CreateLBCell(hdr, "#",  10, SlateText, TextAnchor.MiddleCenter, 24);
        if (!isSpeed) CreateLBCell(hdr, "评级", 10, SlateText, TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "分数", 10, SlateText, TextAnchor.MiddleCenter, 48);
        CreateLBCell(hdr, "层",  10, SlateText, TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "时间", 10, SlateText, TextAnchor.MiddleCenter, 50);
        CreateLBCell(hdr, "玩家", 10, SlateText, TextAnchor.MiddleLeft, 0, 1);

        // 分隔线
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        line.transform.SetParent(_rankBody, false);
        line.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.25f);
        var lineLE = line.GetComponent<LayoutElement>();
        lineLE.preferredHeight = 1; lineLE.flexibleHeight = 0;

        // 数据行
        for (int i = 0; i < rows.Count; i++)
        {
            var data = rows[i];
            bool isMe = !string.IsNullOrEmpty(data.uid) && !string.IsNullOrEmpty(myUid) && data.uid == myUid;

            Color rankColor = i == 0 ? new Color(0.96f,0.77f,0.33f) :
                              i == 1 ? new Color(0.79f,0.82f,0.90f) :
                              i == 2 ? new Color(0.80f,0.50f,0.20f) : new Color(0.36f,0.38f,0.48f);
            Color rowTxt = isMe ? Cyan : new Color(0.80f, 0.85f, 0.93f);

            var row = CreateLBRow(_rankBody, 24);
            CreateLBCell(row, $"{(data.rank > 0 ? data.rank : i + 1)}", 11, rankColor, TextAnchor.MiddleCenter, 24);
            if (!isSpeed) CreateLBCell(row, data.rating ?? "-", 11, GetRatingColor(data.rating), TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, $"{data.score}", 11, rowTxt, TextAnchor.MiddleCenter, 48);
            CreateLBCell(row, $"F{data.floor}", 11, rowTxt, TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, FormatDuration(data.duration), 10, SteelBlue, TextAnchor.MiddleCenter, 50);

            string name = (data.nickname ?? "?").Replace("<","").Replace(">","");
            if (isMe) name += " (我)";
            CreateLBCell(row, name, 11, rowTxt, TextAnchor.MiddleLeft, 0, 1);
        }

        Txt(_rankBody, $"共 {rows.Count} 条记录", 10, DarkStatText, 20);
    }

    Transform CreateLBRow(Transform parent, int height)
    {
        var row = new GameObject("Row", typeof(RectTransform), typeof(HorizontalLayoutGroup), typeof(LayoutElement));
        row.transform.SetParent(parent, false);
        var hl = row.GetComponent<HorizontalLayoutGroup>();
        hl.spacing = 2;
        hl.childForceExpandWidth = false;
        hl.childForceExpandHeight = true;
        hl.childAlignment = TextAnchor.MiddleCenter;
        var le = row.GetComponent<LayoutElement>();
        le.preferredHeight = height;
        le.flexibleHeight = 0;
        return row.transform;
    }

    void CreateLBCell(Transform parent, string text, int fontSize, Color color, TextAnchor align, float width, float flex = 0)
    {
        var cell = new GameObject("Cell", typeof(RectTransform), typeof(LayoutElement));
        cell.transform.SetParent(parent, false);
        var le = cell.GetComponent<LayoutElement>();
        if (width > 0) le.preferredWidth = width;
        le.flexibleWidth = flex;
        var txt = TxtGo(cell.transform, text, fontSize, color);
        txt.alignment = align;
        Stretch(txt.gameObject);
    }

    GameObject CreateLeaderboardRow(bool isSpeed, bool isHeader)
    {
        var row = new GameObject(isHeader ? "Header" : "Row", typeof(RectTransform));
        var rt = row.GetComponent<RectTransform>();
        rt.sizeDelta = isHeader ? new Vector2(0, 28) : new Vector2(0, 34);

        var grid = row.AddComponent<GridLayoutGroup>();
        grid.spacing = new Vector2(5, 0);
        grid.childAlignment = TextAnchor.MiddleCenter;

        if (isSpeed)
            grid.constraintCount = 5;
        else
            grid.constraintCount = 6;

        grid.cellSize = new Vector2(0, isHeader ? 24 : 30);

        if (isHeader)
        {
            CreateHeaderCell(row.transform, "#", true);
            if (!isSpeed)
                CreateHeaderCell(row.transform, "评级", true);
            CreateHeaderCell(row.transform, "分数", true);
            CreateHeaderCell(row.transform, "层", true);
            CreateHeaderCell(row.transform, "时间", true);
            CreateHeaderCell(row.transform, "玩家", true);
        }

        return row;
    }

    void CreateHeaderCell(Transform parent, string text, bool isHeader)
    {
        var cell = new GameObject("Cell", typeof(RectTransform));
        cell.transform.SetParent(parent, false);
        var txt = TxtGo(cell.transform, text, isHeader ? 10 : 12, SlateText);
        txt.alignment = TextAnchor.MiddleCenter;
        txt.fontStyle = FontStyle.Normal;
    }

    void PopulateLeaderboardRow(GameObject row, LeaderboardRow data, int index, string myUid)
    {
        bool isSpeed = _lbState.tab == "speed";
        bool isMe = !string.IsNullOrEmpty(data.uid) && !string.IsNullOrEmpty(myUid) && data.uid == myUid;

        Color rankColor = index == 0 ? Gold : (index == 1 ? Bright : (index == 2 ? new Color(0.8f, 0.5f, 0.2f) : DarkStatText));
        Color ratingColor = GetRatingColor(data.rating);
        string timeStr = FormatDuration(data.duration);

        var cells = row.GetComponent<GridLayoutGroup>();

        CreateLeaderboardCell(row.transform, $"{(data.rank > 0 ? data.rank : (index + 1))}", 12, rankColor, index < 3, TextAnchor.MiddleCenter);

        if (!isSpeed)
        {
            CreateLeaderboardCell(row.transform, data.rating ?? "-", 12, ratingColor, true, TextAnchor.MiddleCenter);
        }

        CreateLeaderboardCell(row.transform, $"{data.score}", 12, new Color(0.91f, 0.95f, 1f), true, TextAnchor.MiddleCenter);
        CreateLeaderboardCell(row.transform, $"{data.floor}", 12, SteelBlue, false, TextAnchor.MiddleCenter);
        CreateLeaderboardCell(row.transform, timeStr, 12, SteelBlue, false, TextAnchor.MiddleCenter);

        string name = (data.nickname ?? "?").Replace("<", "").Replace(">", "").Replace("\"", "").Replace("&", "");
        CreateLeaderboardCell(row.transform, name + (isMe ? " (我)" : ""), 11.5f, isMe ? Cyan : new Color(0.79f, 0.82f, 0.87f), false, TextAnchor.MiddleLeft);
    }

    void CreateLeaderboardCell(Transform parent, string text, float fontSize, Color color, bool bold, TextAnchor alignment)
    {
        var cell = new GameObject("Cell", typeof(RectTransform));
        cell.transform.SetParent(parent, false);
        var txt = TxtGo(cell.transform, text, (int)fontSize, color);
        txt.alignment = alignment;
        txt.fontStyle = bold ? FontStyle.Bold : FontStyle.Normal;
    }

    void RenderMyLeaderboard(PlayerLeaderboardData data)
    {
        if (data == null)
        {
            ShowNoIdentityMessage();
            return;
        }

        string nick = (!string.IsNullOrEmpty(data.nickname) ? data.nickname : PlayerPrefs.GetString("PT_NICKNAME", "")).Replace("<", "").Replace(">", "").Replace("\"", "").Replace("&", "");
        if (string.IsNullOrEmpty(nick)) nick = "玩家";

        var header = Txt(_rankBody, $"玩家：{nick}", 12, SteelBlue, 24).GetComponent<Text>();
        header.alignment = TextAnchor.MiddleLeft;

        var grid = new GameObject("BoardsGrid", typeof(RectTransform), typeof(GridLayoutGroup));
        grid.transform.SetParent(_rankBody, false);
        var glg = grid.GetComponent<GridLayoutGroup>();
        glg.constraintCount = 2;
        glg.spacing = new Vector2(8, 8);
        glg.constraint = GridLayoutGroup.Constraint.FixedColumnCount;

        Dictionary<string, string> labels = new Dictionary<string, string> {
            { "score|all", "综合 · 全时段" }, { "score|week", "综合 · 本周" }, { "score|day", "综合 · 今日" },
            { "speed|", "速通 (F12 通关)" }, { "daily|", "今日 Modifier" },
            { "class|swarm", "职业 · 虫群" }, { "class|titan", "职业 · 巨像" }, { "class|ghost", "职业 · 幽影" },
            { "class|blood", "职业 · 血裔" }, { "class|mech", "职业 · 机械" }
        };

        if (data.boards != null)
        {
            foreach (var board in data.boards)
            {
                string key = $"{board.type}|{board.sub}";
                string label = labels.ContainsKey(key) ? labels[key] : $"{board.type}{(string.IsNullOrEmpty(board.sub) ? "" : $" · {board.sub}")}";

                var (card, vl) = InfoCard(grid.transform, DarkItemBg, 80);

                Txt(vl, label, 11, SlateText, 18);

                if (board.rank > 0)
                {
                    Txt(vl, $"<color={ColorToHex(Gold)}><b>#{board.rank}</b></color> <color={ColorToHex(DarkStatText)}> / {board.total}</color>", 14, Bright, 20);
                    string stat = $"超过 {board.percentile}% · ";
                    if (board.type == "speed")
                        stat += $"最快 {FormatDuration(board.bestDur)}";
                    else
                        stat += $"最高 {board.bestScore}";
                    Txt(vl, stat, 11, SteelBlue, 18);
                }
                else
                {
                    Txt(vl, "<color=" + ColorToHex(DarkStatText) + ">未上榜</color>", 13, Bright, 18);
                    Txt(vl, "完成一次即可", 11, SteelBlue, 18);
                }
            }
        }
    }

    void ShowLocalFallback(string message)
    {
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        // 警告提示
        var warning = new GameObject("Warning", typeof(RectTransform), typeof(Image), typeof(Outline));
        warning.transform.SetParent(_rankBody, false);
        var rt = warning.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(0, 32);

        // 添加 LayoutElement 确保高度固定
        var le = warning.AddComponent<LayoutElement>();
        le.preferredHeight = 32;
        le.flexibleHeight = 0;

        var img = warning.GetComponent<Image>();
        img.color = new Color(0.1f, 0.04f, 0.06f);

        var ol = warning.GetComponent<Outline>();
        ol.effectColor = new Color(1f, 0.5f, 0.56f, 0.32f);
        ol.effectDistance = new Vector2(1, 1);

        var txt = TxtGo(warning.transform, message, 12, new Color(1f, 0.54f, 0.63f));
        txt.alignment = TextAnchor.MiddleCenter;

        // 渲染本地排行榜数据
        RenderLocalLeaderboard();
    }

    void ShowLocalGameRecords()
    {
        var list = new GameObject("LocalRecords", typeof(RectTransform), typeof(VerticalLayoutGroup));
        list.transform.SetParent(_rankBody, false);
        var lg = list.GetComponent<VerticalLayoutGroup>();
        lg.spacing = 6;
        lg.childForceExpandWidth = true;

        Txt(list.transform, "本地游戏记录", 14, Gold, 28);

        var save = SaveSystem.Instance;
        bool hasData = false;

        for (int slot = 0; slot < 3; slot++)
        {
            var data = save?.GetSaveInfo(slot);
            if (data == null) continue;
            if (data.currentFloor <= 0) continue;

            hasData = true;
            string mode = data.gameMode ?? "经典";
            string floor = $"F{data.currentFloor}";
            string cls = data.player?.selectedClass ?? "?";
            string time = data.saveTime.Year > 2000 ? data.saveTime.ToString("MM/dd HH:mm") : "已保存";

            Color rankC = slot == 0 ? Gold : slot == 1 ? Bright : Dim;
            var (card, cvl) = InfoCard(list.transform, CardBg, 60);
            Txt(cvl, $"#{slot + 1}  {mode}  {floor}  [{cls}]", 12, rankC, 20);
            Txt(cvl, $"HP:{data.player?.hp ?? 0}/{data.player?.maxHp ?? 100}  ATK:{data.player?.attack ?? 0}  {time}", 10, Dim, 16);
        }

        var metaRank = MetaProgressSystem.Instance;
        if (metaRank != null && metaRank.metaData.totalGamesPlayed > 0)
        {
            var md = metaRank.metaData;
            var (statsCard, svl) = InfoCard(list.transform, CardBg, 80);
            Txt(svl, $"总闯塔: {md.totalGamesPlayed}   最深: F{md.maxFloorReached}", 11, Bright, 16);
            Txt(svl, $"总击杀: {md.totalKills}   附身: {md.possessions}", 11, Bright, 16);
            Txt(svl, $"完成度: {metaRank.GetCompletionPercent()}%", 11, Cyan, 16);
        }

        if (!hasData)
            Txt(list.transform, "暂无游戏记录", 12, Dim, 24);
    }

    void RenderLocalLeaderboard()
    {
        // Local leaderboard rendering with class/speed filters
        // 表头
        var hdr = CreateLBRow(_rankBody, 22);
        CreateLBCell(hdr, "#",  10, SlateText, TextAnchor.MiddleCenter, 24);
        CreateLBCell(hdr, "评级", 10, SlateText, TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "分数", 10, SlateText, TextAnchor.MiddleCenter, 48);
        CreateLBCell(hdr, "层",  10, SlateText, TextAnchor.MiddleCenter, 36);
        CreateLBCell(hdr, "时间", 10, SlateText, TextAnchor.MiddleCenter, 60);
        CreateLBCell(hdr, "职业", 10, SlateText, TextAnchor.MiddleLeft, 0, 1);

        // 分隔线
        var line = new GameObject("Line", typeof(RectTransform), typeof(Image), typeof(LayoutElement));
        line.transform.SetParent(_rankBody, false);
        line.GetComponent<Image>().color = new Color(0.15f, 0.18f, 0.25f);
        var lineLE = line.GetComponent<LayoutElement>();
        lineLE.preferredHeight = 1; lineLE.flexibleHeight = 0;

        var save = SaveSystem.Instance;
        bool hasData = false;

        for (int slot = 0; slot < 3; slot++)
        {
            var data = save?.GetSaveInfo(slot);
            if (data == null) continue;
            if (data.currentFloor <= 0) continue;

            string cls = data.player?.selectedClass?.ToLower() ?? "";
            
            // CLASS 标签页：按职业筛选
            if (_lbState.tab == "class")
            {
                if (cls != _lbState.cls) continue;
            }
            
            // SPEED 标签页：只显示通关的记录
            if (_lbState.tab == "speed")
            {
                if (data.currentFloor < 12) continue;
            }

            hasData = true;

            Color rankColor = slot == 0 ? new Color(0.96f,0.77f,0.33f) :
                              slot == 1 ? new Color(0.79f,0.82f,0.90f) :
                              new Color(0.80f,0.50f,0.20f);

            int score = (data.currentFloor * 100) + (data.player?.attack ?? 0) * 10;
            string rating = GetLocalRating(data.currentFloor);
            string time = data.saveTime.Year > 2000 ? data.saveTime.ToString("MM/dd HH:mm") : "--:--";
            string clsName = data.player?.selectedClass ?? "?";

            var row = CreateLBRow(_rankBody, 24);
            CreateLBCell(row, $"{slot + 1}", 11, rankColor, TextAnchor.MiddleCenter, 24);
            CreateLBCell(row, rating, 11, GetRatingColor(rating), TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, $"{score}", 11, new Color(0.91f,0.95f,1f), TextAnchor.MiddleCenter, 48);
            CreateLBCell(row, $"F{data.currentFloor}", 11, SteelBlue, TextAnchor.MiddleCenter, 36);
            CreateLBCell(row, time, 10, SteelBlue, TextAnchor.MiddleCenter, 60);
            CreateLBCell(row, clsName, 11, new Color(0.80f,0.85f,0.93f), TextAnchor.MiddleLeft, 0, 1);
        }

        if (!hasData)
        {
            Txt(_rankBody, "暂无本地记录", 12, SlateText, 24);
        }
    }

    void CreateFixedWidthCell(Transform parent, string text, float fontSize, Color color, TextAnchor alignment, float width)
    {
        var cell = new GameObject("Cell", typeof(RectTransform), typeof(LayoutElement));
        cell.transform.SetParent(parent, false);

        var le = cell.GetComponent<LayoutElement>();
        le.preferredWidth = width;
        le.flexibleWidth = width >= 100 ? 1 : 0;

        var txt = TxtGo(cell.transform, text, (int)fontSize, color);
        txt.alignment = alignment;
        txt.fontStyle = FontStyle.Normal;
    }

    string GetLocalRating(int floor)
    {
        if (floor >= 12) return "SSS";
        if (floor >= 10) return "SS";
        if (floor >= 8) return "S";
        if (floor >= 6) return "A";
        if (floor >= 4) return "B";
        return "C";
    }

    void ShowNoIdentityMessage()
    {
        for (int i = _rankBody.childCount - 1; i >= 0; i--)
            DestroyImmediate(_rankBody.GetChild(i).gameObject);

        var empty = new GameObject("Empty", typeof(RectTransform));
        empty.transform.SetParent(_rankBody, false);
        var rt = empty.GetComponent<RectTransform>();
        rt.anchorMin = Vector2.zero;
        rt.anchorMax = Vector2.one;

        var vl = empty.AddComponent<VerticalLayoutGroup>();
        vl.childAlignment = TextAnchor.MiddleCenter;
        vl.padding = new RectOffset(0, 0, 80, 0);

        Txt(empty.transform, "未识别到玩家身份", 13, SteelBlue, 24);
    }

    Color GetRatingColor(string rating)
    {
        switch (rating)
        {
            case "SSS": return new Color(1f, 0f, 0.43f);
            case "SS": return new Color(0.71f, 0.33f, 1f);
            case "S": return Gold;
            case "A": return Cyan;
            case "B": return new Color(1f, 0.55f, 0f);
            default: return SlateText;
        }
    }

    string FormatDuration(float seconds)
    {
        int minutes = Mathf.FloorToInt(seconds / 60);
        int secs = Mathf.FloorToInt(seconds % 60);
        return $"{minutes}:{secs:D2}";
    }

    string ColorToHex(Color c)
    {
        return $"#{Mathf.RoundToInt(c.r * 255):X2}{Mathf.RoundToInt(c.g * 255):X2}{Mathf.RoundToInt(c.b * 255):X2}";
    }
}
// Force recompile
