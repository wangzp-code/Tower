using UnityEngine;
using System;
using System.Collections.Generic;
using System.Text;

public class PosterShareSystem : MonoBehaviour
{
    public static PosterShareSystem Instance { get; private set; }
    
    private const int W = 720;
    private const int H = 1280;
    
    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    public Texture2D BuildEndingPoster(string playerClass, Dictionary<string, object> data = null)
    {
        Texture2D texture = new Texture2D(W, H, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[W * H];
        
        var palette = GetPalette(playerClass ?? "titan");
        DrawBackground(pixels, palette);
        DrawFrame(pixels, palette.primary);
        
        string tier = "rare";
        DrawTierEffects(pixels, tier, palette.primary);
        
        int y = 100;
        DrawText(pixels, "YOU  ARE  ME", W / 2, y, 20, ColorAlpha(palette.primary, 0.45f), true);
        y += 60;
        
        DrawText(pixels, "达成结局", W / 2, y, 18, ColorAlpha(palette.primary, 0.65f), true);
        y += 50;
        
        string endingTitle = GetEndingTitle(playerClass);
        DrawText(pixels, endingTitle, W / 2, y, 52, Color.white, true, palette.primary, 32);
        y += 50;
        
        string endingEN = GetEndingEN(playerClass);
        DrawText(pixels, endingEN, W / 2, y, 14, ColorAlpha(palette.primary, 0.5f), true);
        y += 30;
        
        DrawTierBadge(pixels, tier, W / 2, y);
        y += 80;
        
        DrawClassGlyph(pixels, playerClass, W / 2, y, 80, palette.primary);
        y += 150;
        
        string punchLine = GetPunchLine(playerClass);
        string[] punchLines = punchLine.Split('\n');
        foreach (var line in punchLines)
        {
            DrawText(pixels, line, W / 2, y, 20, new Color(0.88f, 0.9f, 0.94f), true, palette.primary, 8);
            y += 36;
        }
        
        y += 20;
        DrawFinalReportCard(pixels, y, palette, data);
        y += 100;
        
        DrawSocialFooter(pixels, palette.primary, "塔会把你变成什么？", "#ParasiteTower #你也是我 #" + endingEN);
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    public Texture2D BuildRunReportPoster(CompleteGameSystem.Report report, string mode = "short")
    {
        if (report == null)
        {
            Debug.LogError("[PosterShareSystem] BuildRunReportPoster: report is null");
            return null;
        }

        Texture2D texture = new Texture2D(W, H, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[W * H];
        
        string playerClass = GameManager.Instance?.Player?.selectedClass ?? "titan";
        var palette = GetPalette(playerClass);
        
        DrawBackground(pixels, palette);
        
        string rating = report.rank ?? "C";
        var ratingTheme = GetRatingTheme(rating);
        DrawRatingFrame(pixels, ratingTheme);
        
        string tier = GetPosterTier(report, mode);
        Color modeColor = GetModeColor(mode);
        DrawTierEffects(pixels, tier, modeColor);
        
        int y = 96;
        DrawText(pixels, "YOU  ARE  ME", W / 2, y, 20, ColorAlpha(modeColor, 0.4f), true);
        y += 60;
        
        string modeCN = GetModeCN(mode);
        DrawText(pixels, modeCN, Mathf.RoundToInt(W * 0.3f), y, 18, ColorAlpha(modeColor, 0.8f), true);
        DrawText(pixels, GetClassIcon(playerClass) + " " + GetClassName(playerClass), Mathf.RoundToInt(W * 0.7f), y, 18, new Color(0.80f, 0.84f, 0.88f), true);
        y += 60;
        
        string headline = GetModeHeadline(mode, report);
        DrawText(pixels, headline, W / 2, y, 36, Color.white, true, modeColor, 20);
        y += 40;
        
        string modeEN = mode == "expedition" ? "EXPEDITION" : mode == "classic" ? "CLASSIC" : "SHORT RUN";
        DrawText(pixels, modeEN, W / 2, y, 13, ColorAlpha(modeColor, 0.5f), true);
        y += 120;
        
        int ratingSize = rating.Length >= 3 ? 110 : rating.Length >= 2 ? 120 : 130;
        DrawText(pixels, rating, W / 2, y, ratingSize, ratingTheme.color, true, ratingTheme.glow, ratingTheme.tier >= 1 ? 48 : 32);
        y += 110;
        
        DrawText(pixels, "EVALUATION", W / 2, y, 12, ColorAlpha(ratingTheme.color, 0.6f), true);
        y += 20;
        
        if (tier != "normal") DrawTierBadge(pixels, tier, W / 2, y);
        y += 30;
        
        DrawText(pixels, report.score.ToString(), W / 2, y, 40, new Color(0.11f, 0.85f, 0.69f), true, new Color(0.11f, 0.85f, 0.69f), 12);
        y += 30;
        
        DrawText(pixels, "SCORE", W / 2, y, 11, new Color(0.35f, 0.38f, 0.47f), true);
        y += 50;
        
        string punch = GetRunPunchLine(mode, report);
        DrawText(pixels, punch, W / 2, y, 18, new Color(0.7f, 0.72f, 0.8f), true, modeColor, 6);
        y += 50;
        
        DrawRunReportCard(pixels, y, report, mode, palette, modeColor);
        
        DrawSocialFooter(pixels, modeColor, GetModeCTA(mode), "#ParasiteTower #你也是我 #" + modeCN);
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    public Texture2D BuildAchievementPoster(AchievementDef def)
    {
        if (def == null)
        {
            Debug.LogError("[PosterShareSystem] BuildAchievementPoster: def is null");
            return null;
        }

        Texture2D texture = new Texture2D(W, H, TextureFormat.RGBA32, false);
        Color[] pixels = new Color[W * H];
        
        Color gold = new Color(1f, 0.84f, 0f);
        var palette = new Palette
        {
            primary = gold,
            bgTop = new Color(0.1f, 0.08f, 0.03f),
            bgMid = new Color(0.16f, 0.11f, 0.03f),
            bgBot = new Color(0.03f, 0.02f, 0.01f),
            glow1 = gold,
            glow2 = new Color(1f, 0.53f, 0f),
            particle = gold
        };
        
        DrawBackground(pixels, palette);
        DrawFrame(pixels, gold);
        DrawTierEffects(pixels, "rare", gold);
        
        DrawLogo(pixels, gold);
        
        int y = 240;
        DrawText(pixels, "◈ 成就解锁 ◈", W / 2, y, 22, ColorAlpha(gold, 0.85f), true);
        y += 180;
        
        DrawAchievementBadge(pixels, W / 2, y, def.icon);
        y += 220;
        
        DrawText(pixels, def.name, W / 2, y, 48, Color.white, true, gold, 20);
        y += 70;
        
        if (!string.IsNullOrEmpty(def.description))
        {
            DrawText(pixels, def.description, W / 2, y, 20, new Color(0.80f, 0.84f, 0.88f), true);
        }
        
        y += 200;
        DrawText(pixels, DateTime.Now.ToString("yyyy-MM-dd HH:mm"), W / 2, y, 14, new Color(0.49f, 0.51f, 0.58f), true);
        
        DrawSocialFooter(pixels, gold, "你也是我 · 成就解锁", "#ParasiteTower #你也是我 #成就");
        
        texture.SetPixels(pixels);
        texture.Apply();
        return texture;
    }
    
    private void DrawBackground(Color[] pixels, Palette palette)
    {
        for (int y = 0; y < H; y++)
        {
            float t = (float)y / H;
            Color bg = Color.Lerp(palette.bgTop, palette.bgMid, Mathf.Clamp01(t * 2));
            bg = Color.Lerp(bg, palette.bgBot, Mathf.Clamp01((t - 0.5f) * 2));
            for (int x = 0; x < W; x++)
            {
                pixels[y * W + x] = bg;
            }
        }
        
        DrawRadialGlow(pixels, W * 0.18f, H * 0.22f, 420, palette.glow1, 0.55f);
        DrawRadialGlow(pixels, W * 0.85f, H * 0.45f, 380, palette.glow2, 0.45f);
        DrawRadialGlow(pixels, W * 0.45f, H * 0.85f, 520, palette.glow1, 0.35f);
        
        for (int y = 0; y < H; y += 4)
        {
            for (int x = 0; x < W; x++)
            {
                pixels[y * W + x] += new Color(0.025f, 0.025f, 0.025f);
            }
        }
        
        System.Random rnd = new System.Random(palette.seed.GetHashCode());
        for (int i = 0; i < 60; i++)
        {
            int x = rnd.Next(W);
            int y = rnd.Next(H);
            float alpha = (float)rnd.NextDouble() * 0.6f + 0.2f;
            DrawCircle(pixels, x, y, (float)rnd.NextDouble() * 2.4f + 0.6f, ColorAlpha(palette.particle, alpha));
        }
        
        for (int y = 0; y < H; y++)
        {
            float darken = Mathf.Abs(y - H / 2) / (float)(H / 2);
            darken = darken * darken * 0.55f;
            for (int x = 0; x < W; x++)
            {
                pixels[y * W + x] *= (1 - darken);
            }
        }
    }
    
    private void DrawFrame(Color[] pixels, Color color)
    {
        DrawRectangle(pixels, 24, 24, W - 48, H - 48, 2, ColorAlpha(color, 0.6f));
        DrawRectangle(pixels, 38, 38, W - 76, H - 76, 1, ColorAlpha(color, 0.18f));
        
        int L = 36;
        DrawCorner(pixels, 40, 40, L, color, 1, 1);
        DrawCorner(pixels, W - 40, 40, L, color, -1, 1);
        DrawCorner(pixels, 40, H - 40, L, color, 1, -1);
        DrawCorner(pixels, W - 40, H - 40, L, color, -1, -1);
    }
    
    private void DrawRatingFrame(Color[] pixels, RatingTheme theme)
    {
        DrawFrame(pixels, theme.color);
        
        if (theme.tier >= 1)
        {
            DrawRectangle(pixels, 18, 18, W - 36, H - 36, 4, ColorAlpha(theme.color, 0.35f));
        }
        if (theme.tier >= 2)
        {
            DrawRectangle(pixels, 52, 52, W - 104, H - 104, 1, ColorAlpha(theme.color, 0.5f));
        }
        if (theme.tier >= 3)
        {
            DrawCircle(pixels, 40, 40, 4, theme.color);
            DrawCircle(pixels, W - 40, 40, 4, theme.color);
            DrawCircle(pixels, 40, H - 40, 4, theme.color);
            DrawCircle(pixels, W - 40, H - 40, 4, theme.color);
        }
    }
    
    private void DrawTierEffects(Color[] pixels, string tier, Color color)
    {
        var meta = GetTierMeta(tier);
        if (tier == "normal" || meta.frameLayers == 0) return;
        
        if (meta.frameLayers >= 1)
        {
            DrawRectangle(pixels, 14, 14, W - 28, H - 28, 3, ColorAlpha(meta.color, 0.25f));
        }
        if (meta.frameLayers >= 2)
        {
            DrawDashedRectangle(pixels, 8, 8, W - 16, H - 16, 1, ColorAlpha(meta.color, 0.35f));
            
            DrawDiamond(pixels, W / 2, 6, meta.color);
            DrawDiamond(pixels, W / 2, H - 6, meta.color);
            DrawDiamond(pixels, 6, H / 2, meta.color);
            DrawDiamond(pixels, W - 6, H / 2, meta.color);
        }
        if (meta.frameLayers >= 3)
        {
            DrawRadialGlow(pixels, W / 2, H / 2, H * 0.6f, meta.color, 0.06f);
        }
        
        if (meta.particles > 0)
        {
            System.Random rnd = new System.Random(("tier" + tier).GetHashCode() + 13);
            for (int i = 0; i < meta.particles; i++)
            {
                int x = rnd.Next(W);
                int y = rnd.Next(H);
                float alpha = (float)rnd.NextDouble() * 0.25f + 0.08f;
                DrawCircle(pixels, x, y, (float)rnd.NextDouble() * 2.2f + 0.6f, ColorAlpha(meta.color, alpha));
            }
        }
    }
    
    private void DrawTierBadge(Color[] pixels, string tier, int x, int y)
    {
        if (tier == "normal") return;
        var meta = GetTierMeta(tier);
        
        DrawRoundedRect(pixels, x - 60, y - 16, 120, 34, 6, ColorAlpha(meta.color, 0.1f));
        DrawRoundedRectOutline(pixels, x - 60, y - 16, 120, 34, 6, 1, ColorAlpha(meta.color, 0.5f));
        DrawText(pixels, meta.labelEN, x, y + 6, 12, meta.color, true);
    }
    
    private void DrawLogo(Color[] pixels, Color color)
    {
        DrawText(pixels, "YOU  ARE  ME", W / 2, 108, 22, ColorAlpha(color, 0.55f), true);
        DrawText(pixels, "你也是我", W / 2, 160, 38, Color.white, true, color, 18);
        
        DrawLine(pixels, W / 2 - 100, 180, W / 2 - 40, 180, 1, ColorAlpha(color, 0.5f));
        DrawLine(pixels, W / 2 + 40, 180, W / 2 + 100, 180, 1, ColorAlpha(color, 0.5f));
        DrawCircle(pixels, W / 2, 180, 3, color);
    }
    
    private void DrawClassGlyph(Color[] pixels, string cls, int cx, int cy, int r, Color color)
    {
        DrawRadialGlow(pixels, cx, cy, r * 1.5f, color, 0.55f);
        
        DrawCircleOutline(pixels, cx, cy, r, 3, color);
        
        DrawDashedCircle(pixels, cx, cy, r - 14, 1, ColorAlpha(color, 0.45f));
        
        string icon = GetClassIcon(cls);
        DrawText(pixels, icon, cx, cy + 4, Mathf.RoundToInt(r * 1.2f), Color.white, true);
    }
    
    private void DrawAchievementBadge(Color[] pixels, int cx, int cy, string icon)
    {
        DrawRadialGlow(pixels, cx, cy, 160, new Color(1f, 0.84f, 0f), 0.6f);
        
        DrawPolygon(pixels, cx, cy, 140, 6, 4, new Color(1f, 0.84f, 0f));
        
        DrawText(pixels, icon ?? "🏆", cx, cy + 8, 150, Color.white, true);
    }
    
    private void DrawFinalReportCard(Color[] pixels, int y, Palette palette, Dictionary<string, object> data)
    {
        int cardH = 80;
        DrawRoundedRect(pixels, 60, y, W - 120, cardH, 12, ColorAlpha(Color.white, 0.05f));
        DrawRoundedRectOutline(pixels, 60, y, W - 120, cardH, 12, 1, ColorAlpha(palette.primary, 0.3f));
        DrawLine(pixels, 72, y, W - 72, y, 2, ColorAlpha(palette.primary, 0.4f));
        
        DrawText(pixels, "FINAL REPORT", W / 2, y + 26, 12, ColorAlpha(palette.primary, 0.6f), true);
        
        List<string> reportItems = new List<string>();
        reportItems.Add("F" + (data?["floor"]?.ToString() ?? "?"));
        reportItems.Add("附身 " + (data?["possessions"]?.ToString() ?? "0"));
        reportItems.Add("污染 " + (data?["pollution"]?.ToString() ?? "0") + "%");
        reportItems.Add("击杀 " + (data?["kills"]?.ToString() ?? "0"));
        
        DrawReportItems(pixels, W / 2, y + 58, reportItems.ToArray());
    }
    
    private void DrawRunReportCard(Color[] pixels, int y, CompleteGameSystem.Report report, string mode, Palette palette, Color modeColor)
    {
        int cardH = report.echoReward > 0 ? 380 : 300;
        DrawRoundedRect(pixels, 60, y, W - 120, cardH, 12, ColorAlpha(Color.white, 0.05f));
        DrawRoundedRectOutline(pixels, 60, y, W - 120, cardH, 12, 1, ColorAlpha(modeColor, 0.3f));
        DrawLine(pixels, 72, y, W - 72, y, 2, ColorAlpha(modeColor, 0.4f));
        
        string reportLabel = mode == "expedition" ? "EXPEDITION REPORT" : mode == "classic" ? "CLASSIC REPORT" : "FINAL REPORT";
        DrawText(pixels, "· " + reportLabel + " ·", W / 2, y + 28, 12, ColorAlpha(modeColor, 0.65f), true);
        
        int minutes = Mathf.FloorToInt(report.survivalSeconds / 60);
        int seconds = Mathf.FloorToInt(report.survivalSeconds % 60);
        string timeStr = $"{minutes:D2}:{seconds:D2}";
        
        List<string> reportItems = new List<string>();
        int floorCap = mode == "expedition" ? 20 : mode == "classic" ? 50 : 12;
        reportItems.Add("F" + report.floorsReached + "/" + floorCap);
        reportItems.Add("击杀 " + report.kills);
        reportItems.Add("附身 " + report.possessions);
        reportItems.Add(timeStr);
        
        DrawReportItems(pixels, W / 2, y + 60, reportItems.ToArray());
        
        DrawText(pixels, "☢ 污染峰值 " + Mathf.RoundToInt(report.maxPollution) + "%", W / 2, y + 92, 14, 
            report.maxPollution >= 80 ? new Color(1f, 0f, 0.43f) : report.maxPollution >= 50 ? new Color(1f, 0.55f, 0f) : new Color(0.35f, 0.38f, 0.47f), true);
        
        if (mode == "expedition")
        {
            DrawChapters(pixels, y + 120, report.floorsReached);
        }
        else if (mode == "classic")
        {
            DrawZones(pixels, y + 120, report.floorsReached);
        }
        else
        {
            DrawFloorProgress(pixels, y + 120, report.floorsReached, floorCap);
        }
        
        int dY = y + 186;
        DrawBestHostCard(pixels, dY, report.longestHost, report.longestHostSeconds, palette.primary);
        
        int fx = 92 + (W - 204) / 2 + 20;
        Color causeColor = (report.deathCause?.Contains("污染") ?? false) ? new Color(1f, 0f, 0.43f) : new Color(0.11f, 0.85f, 0.69f);
        DrawFinalFormCard(pixels, fx, dY, report.finalForm, report.deathCause, causeColor);
        
        if (report.echoReward > 0)
        {
            DrawEchoRewardCard(pixels, dY + 102, report.echoReward);
        }
    }
    
    private void DrawReportItems(Color[] pixels, int x, int y, string[] items)
    {
        Color[] colors = { new Color(1f, 0.84f, 0f), new Color(1f, 0f, 0.43f), new Color(0.11f, 0.85f, 0.69f), new Color(0.6f, 0.68f, 0.87f) };
        
        int totalWidth = 0;
        foreach (var item in items)
        {
            totalWidth += item.Length * 12;
        }
        totalWidth += (items.Length - 1) * 24;
        
        int currentX = x - totalWidth / 2;
        for (int i = 0; i < items.Length; i++)
        {
            DrawText(pixels, items[i], currentX, y, 18, colors[i % colors.Length], false);
            currentX += items[i].Length * 12;
            if (i < items.Length - 1)
            {
                DrawText(pixels, "｜", currentX + 12, y, 18, new Color(0.23f, 0.25f, 0.31f), false);
                currentX += 24;
            }
        }
    }
    
    private void DrawChapters(Color[] pixels, int y, int floor)
    {
        Color[] chColors = { new Color(0.16f, 0.83f, 0.70f), new Color(1f, 0.55f, 0f), new Color(0.71f, 0.33f, 1f), new Color(1f, 0f, 0.43f) };
        string[] chNames = { "前厅", "裂变", "深层", "终域" };
        
        DrawText(pixels, "CHAPTERS", 92, y, 11, new Color(0.49f, 0.51f, 0.58f), false);
        DrawText(pixels, "F1 → F20", W - 92, y, 11, new Color(0.35f, 0.38f, 0.47f), true);
        
        int tlW = W - 184;
        int gap = 3;
        int chW = (tlW - gap * 3) / 4;
        
        for (int i = 0; i < 4; i++)
        {
            int chEnd = (i + 1) * 5;
            bool reached = floor >= chEnd;
            bool partial = !reached && floor >= i * 5 + 1;
            Color fillColor = reached ? chColors[i] : (partial ? ColorAlpha(chColors[i], 0.5f) : ColorAlpha(Color.white, 0.06f));
            
            DrawRoundedRect(pixels, 92 + i * (chW + gap), y + 12, chW, 14, 3, fillColor);
            DrawText(pixels, chNames[i], 92 + i * (chW + gap) + chW / 2, y + 42, 10, reached ? chColors[i] : new Color(0.35f, 0.38f, 0.47f), true);
        }
    }
    
    private void DrawZones(Color[] pixels, int y, int floor)
    {
        Color[] zColors = { new Color(0.16f, 0.83f, 0.70f), new Color(1f, 0.55f, 0f), new Color(0.71f, 0.33f, 1f), new Color(1f, 0f, 0.43f), new Color(1f, 0.84f, 0f) };
        string[] zNames = { "I", "II", "III", "IV", "V" };
        
        DrawText(pixels, "ZONES", 92, y, 11, new Color(0.49f, 0.51f, 0.58f), false);
        DrawText(pixels, "F1 → F50", W - 92, y, 11, new Color(0.35f, 0.38f, 0.47f), true);
        
        int tlW = W - 184;
        int gap = 3;
        int zW = (tlW - gap * 4) / 5;
        
        for (int i = 0; i < 5; i++)
        {
            int zEnd = (i + 1) * 10;
            bool reached = floor >= zEnd;
            bool partial = !reached && floor >= i * 10 + 1;
            Color fillColor = reached ? zColors[i] : (partial ? ColorAlpha(zColors[i], 0.5f) : ColorAlpha(Color.white, 0.06f));
            
            DrawRoundedRect(pixels, 92 + i * (zW + gap), y + 12, zW, 14, 3, fillColor);
            DrawText(pixels, zNames[i], 92 + i * (zW + gap) + zW / 2, y + 42, 10, reached ? zColors[i] : new Color(0.35f, 0.38f, 0.47f), true);
        }
    }
    
    private void DrawFloorProgress(Color[] pixels, int y, int floor, int floorCap)
    {
        DrawText(pixels, "FLOOR PROGRESS", 92, y, 10, new Color(0.35f, 0.38f, 0.47f), false);
        DrawText(pixels, "F" + floor + "/" + floorCap, W - 92, y, 10, new Color(1f, 0.84f, 0f), true);
        
        int segGap = 3;
        int segW = (W - 184 - segGap * 11) / 12;
        
        for (int i = 0; i < 12; i++)
        {
            bool reached = floor >= i + 1;
            bool isBoss = (i + 1) % 4 == 0;
            Color fillColor = reached ? (isBoss ? new Color(1f, 0.84f, 0f) : new Color(1f, 0.84f, 0f)) : ColorAlpha(Color.white, 0.06f);
            
            DrawRoundedRect(pixels, 92 + i * (segW + segGap), y + 14, segW, 12, 2, fillColor);
        }
    }
    
    private void DrawBestHostCard(Color[] pixels, int y, string host, float duration, Color color)
    {
        int halfW = (W - 204) / 2;
        DrawRoundedRect(pixels, 92, y, halfW, 86, 8, ColorAlpha(Color.white, 0.03f));
        DrawRoundedRectOutline(pixels, 92, y, halfW, 86, 8, 1, ColorAlpha(color, 0.12f));
        
        DrawText(pixels, "🏆 BEST HOST", 104, y + 20, 10, ColorAlpha(color, 0.7f), false);
        DrawText(pixels, (host ?? "未知").Substring(0, Math.Min(host?.Length ?? 0, 8)), 104, y + 48, 18, Color.white, false);
        DrawText(pixels, Mathf.FloorToInt(duration) + "秒存活", 104, y + 72, 11, new Color(0.49f, 0.51f, 0.58f), false);
    }
    
    private void DrawFinalFormCard(Color[] pixels, int x, int y, string form, string cause, Color color)
    {
        int halfW = (W - 204) / 2;
        DrawRoundedRect(pixels, x, y, halfW, 86, 8, ColorAlpha(Color.white, 0.03f));
        DrawRoundedRectOutline(pixels, x, y, halfW, 86, 8, 1, ColorAlpha(color, 0.12f));
        
        DrawText(pixels, "💀 FINAL FORM", x + 12, y + 20, 10, ColorAlpha(color, 0.8f), false);
        DrawText(pixels, (form ?? "未知").Substring(0, Math.Min(form?.Length ?? 0, 8)), x + 12, y + 48, 18, Color.white, false);
        DrawText(pixels, cause ?? "", x + 12, y + 72, 11, color, false);
    }
    
    private void DrawEchoRewardCard(Color[] pixels, int y, int reward)
    {
        DrawRoundedRect(pixels, 92, y, W - 184, 62, 8, ColorAlpha(new Color(0.65f, 0.36f, 1f), 0.08f));
        DrawRoundedRectOutline(pixels, 92, y, W - 184, 62, 8, 1, ColorAlpha(new Color(0.65f, 0.36f, 1f), 0.4f));
        
        DrawText(pixels, "⛯", 108, y + 38, 26, new Color(0.80f, 0.71f, 1f), false);
        DrawText(pixels, "ECHO · 残响", 140, y + 24, 10, new Color(0.65f, 0.55f, 0.84f), false);
        DrawText(pixels, "+" + reward, 140, y + 48, 20, new Color(0.80f, 0.71f, 1f), false);
    }
    
    private void DrawSocialFooter(Color[] pixels, Color color, string cta, string hashtags)
    {
        DrawLine(pixels, 80, H - 180, W - 80, H - 180, 1, ColorAlpha(Color.white, 0.06f));
        
        DrawText(pixels, cta, W / 2, H - 150, 20, new Color(0.76f, 0.79f, 0.85f), true);
        
        DrawQRCode(pixels, W - 92 - 84, H - 128, 84, color);
        DrawText(pixels, "扫码下载", W - 92 - 84 / 2, H - 30, 10, new Color(0.5f, 0.51f, 0.58f), true);
        
        DrawText(pixels, hashtags, 92, H - 108, 13, ColorAlpha(color, 0.55f), false);
        
        DrawText(pixels, DateTime.Now.ToString("yyyy-MM-dd HH:mm"), 92, H - 60, 11, new Color(0.29f, 0.31f, 0.41f), false);
    }
    
    private void DrawQRCode(Color[] pixels, int x, int y, int size, Color color)
    {
        DrawRoundedRect(pixels, x - 2, y - 2, size + 4, size + 4, 4, Color.white);
        
        int m = size / 21;
        int[] corners = { 0, 0, size - 7 * m, 0, 0, size - 7 * m };
        
        for (int i = 0; i < 3; i++)
        {
            int cx = x + corners[i * 2];
            int cy = y + corners[i * 2 + 1];
            DrawRectangle(pixels, cx, cy, 7 * m, 7 * m, Color.black);
            DrawRectangle(pixels, cx + m, cy + m, 5 * m, 5 * m, Color.white);
            DrawRectangle(pixels, cx + 2 * m, cy + 2 * m, 3 * m, 3 * m, Color.black);
        }
        
        System.Random rnd = new System.Random("qr-pt".GetHashCode());
        for (int gy = 0; gy < 21; gy++)
        {
            for (int gx = 0; gx < 21; gx++)
            {
                bool inCorner = (gx < 8 && gy < 8) || (gx >= 13 && gy < 8) || (gx < 8 && gy >= 13);
                if (inCorner) continue;
                
                if (rnd.NextDouble() > 0.52)
                {
                    DrawRectangle(pixels, x + gx * m, y + gy * m, m, m, Color.black);
                }
            }
        }
        
        DrawRoundedRectOutline(pixels, x - 4, y - 4, size + 8, size + 8, 6, 2, ColorAlpha(color, 0.4f));
    }
    
    private void DrawText(Color[] pixels, string text, int x, int y, int size, Color color, bool centered, Color glowColor = default, int glowSize = 0)
    {
        int charWidth = size / 2;
        int charHeight = size;
        
        if (centered)
        {
            x -= text.Length * charWidth / 2;
        }
        
        for (int i = 0; i < text.Length; i++)
        {
            DrawChar(pixels, text[i], x + i * charWidth, y, size, color, glowColor, glowSize);
        }
    }
    
    private void DrawChar(Color[] pixels, char c, int x, int y, int size, Color color, Color glowColor, int glowSize)
    {
        int halfSize = size / 2;
        
        if (glowSize > 0)
        {
            for (int dx = -glowSize; dx <= glowSize; dx++)
            {
                for (int dy = -glowSize; dy <= glowSize; dy++)
                {
                    float dist = Mathf.Sqrt(dx * dx + dy * dy);
                    if (dist <= glowSize)
                    {
                        float alpha = 1 - dist / glowSize;
                        DrawPixel(pixels, x + halfSize + dx, y, ColorAlpha(glowColor, alpha * 0.5f));
                    }
                }
            }
        }
        
        for (int py = 0; py < size; py++)
        {
            for (int px = 0; px < size; px++)
            {
                float intensity = GetCharPixel(c, px / (float)size, py / (float)size);
                if (intensity > 0)
                {
                    DrawPixel(pixels, x + px, y + py - size / 2, ColorAlpha(color, intensity));
                }
            }
        }
    }
    
    private float GetCharPixel(char c, float x, float y)
    {
        if (char.IsDigit(c) || char.IsLetter(c))
        {
            x = Mathf.Clamp01(x);
            y = Mathf.Clamp01(y);
            
            if (y < 0.2f || y > 0.8f) return 0;
            if (x < 0.1f || x > 0.9f) return 0;
            
            return 1f;
        }
        
        string s = c.ToString();
        if (s == "｜") return (x > 0.4f && x < 0.6f) ? 1f : 0;
        if (s == "·")
        {
            float dist = Mathf.Sqrt((x - 0.5f) * (x - 0.5f) + (y - 0.5f) * (y - 0.5f));
            return dist < 0.4f ? 1f : 0;
        }
        if (s == "\u2622") return Math.Abs(x - 0.5f) < 0.4f && Math.Abs(y - 0.5f) < 0.4f ? 1f : 0;
        if (s == "\u26CF") return (x > 0.2f && x < 0.8f && y > 0.2f && y < 0.8f) ? 1f : 0;
        if (s == "\uD83C\uDFC6") return y > 0.3f ? 1f : 0;
        if (s == "\uD83D\uDC80") return (x > 0.2f && x < 0.8f && y > 0.1f && y < 0.9f) ? 1f : 0;
        
        return (x > 0.1f && x < 0.9f && y > 0.1f && y < 0.9f) ? 0.8f : 0;
    }
    
    private void DrawPixel(Color[] pixels, int x, int y, Color color)
    {
        if (x < 0 || x >= W || y < 0 || y >= H) return;
        pixels[y * W + x] += color;
    }
    
    private void DrawCircle(Color[] pixels, int cx, int cy, float r, Color color)
    {
        for (int dy = -Mathf.CeilToInt(r); dy <= Mathf.CeilToInt(r); dy++)
        {
            for (int dx = -Mathf.CeilToInt(r); dx <= Mathf.CeilToInt(r); dx++)
            {
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= r)
                {
                    DrawPixel(pixels, cx + dx, cy + dy, color);
                }
            }
        }
    }
    
    private void DrawCircleOutline(Color[] pixels, int cx, int cy, int r, int width, Color color)
    {
        for (int w = 0; w < width; w++)
        {
            for (int angle = 0; angle < 360; angle++)
            {
                float rad = Mathf.Deg2Rad * angle;
                int x = cx + Mathf.RoundToInt(Mathf.Cos(rad) * (r + w));
                int y = cy + Mathf.RoundToInt(Mathf.Sin(rad) * (r + w));
                DrawPixel(pixels, x, y, color);
            }
        }
    }
    
    private void DrawDashedCircle(Color[] pixels, int cx, int cy, int r, int width, Color color)
    {
        for (int angle = 0; angle < 360; angle += 14)
        {
            for (int i = 0; i < 6; i++)
            {
                float rad = Mathf.Deg2Rad * (angle + i);
                int x = cx + Mathf.RoundToInt(Mathf.Cos(rad) * r);
                int y = cy + Mathf.RoundToInt(Mathf.Sin(rad) * r);
                DrawPixel(pixels, x, y, color);
            }
        }
    }
    
    private void DrawRectangle(Color[] pixels, int x, int y, int w, int h, Color color)
    {
        for (int py = y; py < y + h; py++)
        {
            for (int px = x; px < x + w; px++)
            {
                DrawPixel(pixels, px, py, color);
            }
        }
    }
    
    private void DrawRectangle(Color[] pixels, int x, int y, int w, int h, int width, Color color)
    {
        for (int ww = 0; ww < width; ww++)
        {
            for (int px = x; px < x + w; px++)
            {
                DrawPixel(pixels, px, y + ww, color);
                DrawPixel(pixels, px, y + h - 1 - ww, color);
            }
            for (int py = y; py < y + h; py++)
            {
                DrawPixel(pixels, x + ww, py, color);
                DrawPixel(pixels, x + w - 1 - ww, py, color);
            }
        }
    }
    
    private void DrawRoundedRect(Color[] pixels, int x, int y, int w, int h, int r, Color color)
    {
        DrawRectangle(pixels, x + r, y, w - 2 * r, h, color);
        DrawRectangle(pixels, x, y + r, r, h - 2 * r, color);
        DrawRectangle(pixels, x + w - r, y + r, r, h - 2 * r, color);
        
        DrawCircleQuadrant(pixels, x + r, y + r, r, 0, color);
        DrawCircleQuadrant(pixels, x + w - r, y + r, r, 1, color);
        DrawCircleQuadrant(pixels, x + r, y + h - r, r, 2, color);
        DrawCircleQuadrant(pixels, x + w - r, y + h - r, r, 3, color);
    }
    
    private void DrawRoundedRectOutline(Color[] pixels, int x, int y, int w, int h, int r, int width, Color color)
    {
        DrawLine(pixels, x + r, y, x + w - r, y, width, color);
        DrawLine(pixels, x + r, y + h - 1, x + w - r, y + h - 1, width, color);
        DrawLine(pixels, x, y + r, x, y + h - r, width, color);
        DrawLine(pixels, x + w - 1, y + r, x + w - 1, y + h - r, width, color);
        
        DrawCircleQuadrantOutline(pixels, x + r, y + r, r, 0, width, color);
        DrawCircleQuadrantOutline(pixels, x + w - r, y + r, r, 1, width, color);
        DrawCircleQuadrantOutline(pixels, x + r, y + h - r, r, 2, width, color);
        DrawCircleQuadrantOutline(pixels, x + w - r, y + h - r, r, 3, width, color);
    }
    
    private void DrawCircleQuadrant(Color[] pixels, int cx, int cy, int r, int quadrant, Color color)
    {
        for (int dy = 0; dy <= r; dy++)
        {
            for (int dx = 0; dx <= r; dx++)
            {
                if (dx * dx + dy * dy <= r * r)
                {
                    int px = cx + (quadrant % 2 == 0 ? -dx : dx);
                    int py = cy + (quadrant < 2 ? -dy : dy);
                    DrawPixel(pixels, px, py, color);
                }
            }
        }
    }
    
    private void DrawCircleQuadrantOutline(Color[] pixels, int cx, int cy, int r, int quadrant, int width, Color color)
    {
        for (int w = 0; w < width; w++)
        {
            for (int angle = quadrant * 90; angle < (quadrant + 1) * 90; angle++)
            {
                float rad = Mathf.Deg2Rad * angle;
                int x = cx + Mathf.RoundToInt(Mathf.Cos(rad) * (r + w));
                int y = cy + Mathf.RoundToInt(Mathf.Sin(rad) * (r + w));
                DrawPixel(pixels, x, y, color);
            }
        }
    }
    
    private void DrawDashedRectangle(Color[] pixels, int x, int y, int w, int h, int width, Color color)
    {
        DrawDashedLine(pixels, x, y, x + w, y, width, color);
        DrawDashedLine(pixels, x, y + h - 1, x + w, y + h - 1, width, color);
        DrawDashedLine(pixels, x, y, x, y + h, width, color);
        DrawDashedLine(pixels, x + w - 1, y, x + w - 1, y + h, width, color);
    }
    
    private void DrawDashedLine(Color[] pixels, int x1, int y1, int x2, int y2, int width, Color color)
    {
        bool dashOn = true;
        int dashLength = 10;
        int gapLength = 8;
        int current = 0;
        
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            if (dashOn)
            {
                for (int w = 0; w < width; w++)
                {
                    DrawPixel(pixels, x1, y1 + w, color);
                }
            }
            
            current++;
            if (current >= (dashOn ? dashLength : gapLength))
            {
                dashOn = !dashOn;
                current = 0;
            }
            
            if (x1 == x2 && y1 == y2) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x1 += sx; }
            if (e2 < dx) { err += dx; y1 += sy; }
        }
    }
    
    private void DrawLine(Color[] pixels, int x1, int y1, int x2, int y2, int width, Color color)
    {
        int dx = Math.Abs(x2 - x1);
        int dy = Math.Abs(y2 - y1);
        int sx = x1 < x2 ? 1 : -1;
        int sy = y1 < y2 ? 1 : -1;
        int err = dx - dy;
        
        while (true)
        {
            for (int w = 0; w < width; w++)
            {
                DrawPixel(pixels, x1, y1 + w, color);
            }
            
            if (x1 == x2 && y1 == y2) break;
            int e2 = 2 * err;
            if (e2 > -dy) { err -= dy; x1 += sx; }
            if (e2 < dx) { err += dx; y1 += sy; }
        }
    }
    
    private void DrawCorner(Color[] pixels, int x, int y, int L, Color color, int dx, int dy)
    {
        DrawLine(pixels, x, y + dy * L, x, y, 3, color);
        DrawLine(pixels, x, y, x + dx * L, y, 3, color);
    }
    
    private void DrawDiamond(Color[] pixels, int x, int y, Color color)
    {
        DrawLine(pixels, x, y - 6, x + 4, y, 1, color);
        DrawLine(pixels, x + 4, y, x, y + 6, 1, color);
        DrawLine(pixels, x, y + 6, x - 4, y, 1, color);
        DrawLine(pixels, x - 4, y, x, y - 6, 1, color);
    }
    
    private void DrawPolygon(Color[] pixels, int cx, int cy, int r, int sides, int width, Color color)
    {
        for (int w = 0; w < width; w++)
        {
            for (int i = 0; i < sides; i++)
            {
                float angle1 = Mathf.Deg2Rad * (i * 360f / sides - 90);
                float angle2 = Mathf.Deg2Rad * ((i + 1) * 360f / sides - 90);
                
                int x1 = cx + Mathf.RoundToInt(Mathf.Cos(angle1) * (r + w));
                int y1 = cy + Mathf.RoundToInt(Mathf.Sin(angle1) * (r + w));
                int x2 = cx + Mathf.RoundToInt(Mathf.Cos(angle2) * (r + w));
                int y2 = cy + Mathf.RoundToInt(Mathf.Sin(angle2) * (r + w));
                
                DrawLine(pixels, x1, y1, x2, y2, 1, color);
            }
        }
    }
    
    private void DrawRadialGlow(Color[] pixels, float cx, float cy, float r, Color color, float maxAlpha)
    {
        for (int dy = -Mathf.CeilToInt(r); dy <= Mathf.CeilToInt(r); dy++)
        {
            for (int dx = -Mathf.CeilToInt(r); dx <= Mathf.CeilToInt(r); dx++)
            {
                float dist = Mathf.Sqrt(dx * dx + dy * dy);
                if (dist <= r)
                {
                    float alpha = (1 - dist / r) * maxAlpha;
                    DrawPixel(pixels, Mathf.RoundToInt(cx + dx), Mathf.RoundToInt(cy + dy), ColorAlpha(color, alpha));
                }
            }
        }
    }
    
    private Color ColorAlpha(Color color, float alpha)
    {
        return new Color(color.r, color.g, color.b, alpha);
    }
    
    private Palette GetPalette(string playerClass)
    {
        Color primary = GetClassColor(playerClass);
        return new Palette
        {
            primary = primary,
            bgTop = new Color(0.04f, 0.03f, 0.09f),
            bgMid = new Color(0.09f, 0.06f, 0.16f),
            bgBot = new Color(0.03f, 0.02f, 0.08f),
            glow1 = primary,
            glow2 = new Color(0.35f, 0.22f, 0.55f),
            particle = primary,
            seed = playerClass.GetHashCode()
        };
    }
    
    private Color GetClassColor(string playerClass)
    {
        switch (playerClass.ToLower())
        {
            case "titan": return new Color(0.11f, 0.85f, 0.69f);
            case "ghost": return new Color(0.71f, 0.33f, 1f);
            case "swarm": return new Color(1f, 0.55f, 0f);
            case "blood": return new Color(1f, 0f, 0.43f);
            case "mech": return new Color(0.6f, 0.68f, 0.87f);
            default: return new Color(0.11f, 0.85f, 0.69f);
        }
    }
    
    private string GetClassIcon(string playerClass)
    {
        switch (playerClass.ToLower())
        {
            case "titan": return "🗿";
            case "ghost": return "👻";
            case "swarm": return "🐜";
            case "blood": return "🩸";
            case "mech": return "🤖";
            default: return "🧬";
        }
    }
    
    private string GetClassName(string playerClass)
    {
        switch (playerClass.ToLower())
        {
            case "titan": return "泰坦";
            case "ghost": return "幽灵";
            case "swarm": return "虫群";
            case "blood": return "血裔";
            case "mech": return "机械";
            default: return playerClass;
        }
    }
    
    private string GetEndingTitle(string playerClass)
    {
        switch (playerClass.ToLower())
        {
            case "titan": return "泰坦结局";
            case "ghost": return "幽灵结局";
            case "swarm": return "虫群结局";
            case "blood": return "血裔结局";
            case "mech": return "机械结局";
            default: return "结局";
        }
    }
    
    private string GetEndingEN(string playerClass)
    {
        switch (playerClass.ToLower())
        {
            case "titan": return "TITAN ENDING";
            case "ghost": return "GHOST ENDING";
            case "swarm": return "SWARM ENDING";
            case "blood": return "BLOOD ENDING";
            case "mech": return "MECH ENDING";
            default: return "ENDING";
        }
    }
    
    private string GetPunchLine(string playerClass)
    {
        switch (playerClass.ToLower())
        {
            case "titan": return "你成为了塔本身。\n保护，就是囚禁。囚禁，就是保护。";
            case "ghost": return "不再被定义。\n以不存在的形式获得自由。";
            case "swarm": return "每一次死亡，都是繁殖。\n你已经是塔的每一个角落。";
            case "blood": return "每一滴血都是永恒的承诺。\n每一次心跳都是我的脉搏。";
            case "mech": return "当肉体不再是限制，\n你便不再是囚徒。";
            default: return "你也是我。";
        }
    }
    
    private RatingTheme GetRatingTheme(string rating)
    {
        string r = rating.ToUpper();
        char t = r[0];
        
        if (t == 'S')
        {
            int tier = r == "SSS" ? 3 : r == "SS" ? 2 : 1;
            return new RatingTheme
            {
                color = new Color(1f, 0.84f, 0f),
                glow = new Color(1f, 0.71f, 0f),
                tier = tier,
                name = "gold"
            };
        }
        if (t == 'A') return new RatingTheme { color = new Color(0.71f, 0.33f, 1f), glow = new Color(0.57f, 0.2f, 0.93f), tier = 0, name = "purple" };
        if (t == 'B') return new RatingTheme { color = new Color(0.23f, 0.66f, 1f), glow = new Color(0.12f, 0.5f, 0.88f), tier = 0, name = "blue" };
        if (t == 'C') return new RatingTheme { color = new Color(0.12f, 0.84f, 0.69f), glow = new Color(0.06f, 0.68f, 0.54f), tier = 0, name = "green" };
        return new RatingTheme { color = new Color(0.53f, 0.53f, 0.53f), glow = new Color(0.33f, 0.33f, 0.33f), tier = 0, name = "gray" };
    }
    
    private string GetPosterTier(CompleteGameSystem.Report report, string mode)
    {
        string rating = report.rank?.ToUpper() ?? "";
        int floorCap = mode == "expedition" ? 20 : mode == "classic" ? 50 : 12;
        bool cleared = report.floorsReached >= floorCap;
        bool noDeath = report.survivalSeconds > 0 && report.deathCause == null;
        
        if (rating == "SSS") return "legendary";
        if (cleared && noDeath && report.survivalSeconds < 180) return "legendary";
        if (rating == "SS") return "epic";
        if (cleared && noDeath) return "epic";
        if (cleared && report.survivalSeconds < 240) return "epic";
        if (rating.StartsWith("S") || cleared) return "rare";
        return "normal";
    }
    
    private TierMeta GetTierMeta(string tier)
    {
        switch (tier)
        {
            case "normal": return new TierMeta { label = "", labelEN = "", color = new Color(0.53f, 0.53f, 0.53f), glow = new Color(0.33f, 0.33f, 0.33f), particles = 0, frameLayers = 0 };
            case "rare": return new TierMeta { label = "稀有", labelEN = "RARE", color = new Color(0.66f, 0.33f, 0.97f), glow = new Color(0.49f, 0.23f, 0.93f), particles = 20, frameLayers = 1 };
            case "epic": return new TierMeta { label = "史诗", labelEN = "EPIC", color = new Color(1f, 0.55f, 0f), glow = new Color(0.9f, 0.47f, 0f), particles = 40, frameLayers = 2 };
            case "legendary": return new TierMeta { label = "传说", labelEN = "LEGENDARY", color = new Color(1f, 0.84f, 0f), glow = new Color(1f, 0.71f, 0f), particles = 60, frameLayers = 3 };
            default: return new TierMeta();
        }
    }
    
    private Color GetModeColor(string mode)
    {
        switch (mode)
        {
            case "expedition": return new Color(0.16f, 0.83f, 0.70f);
            case "classic": return new Color(0.71f, 0.33f, 1f);
            default: return new Color(1f, 0.84f, 0f);
        }
    }
    
    private string GetModeCN(string mode)
    {
        switch (mode)
        {
            case "expedition": return "远征";
            case "classic": return "经典";
            default: return "短局";
        }
    }
    
    private string GetModeHeadline(string mode, CompleteGameSystem.Report report)
    {
        bool cleared = report.floorsReached >= (mode == "expedition" ? 20 : mode == "classic" ? 50 : 12);
        
        if (mode == "short") return "短局评级";
        if (mode == "expedition") return cleared ? "暗塔已征服" : "暗塔探索报告";
        return cleared ? "塔已登顶" : "经典探索报告";
    }
    
    private string GetRunPunchLine(string mode, CompleteGameSystem.Report report)
    {
        bool cleared = report.floorsReached >= (mode == "expedition" ? 20 : mode == "classic" ? 50 : 12);
        bool isPollution = (report.deathCause?.Contains("污染") ?? false);
        
        if (mode == "short")
        {
            return cleared ? "12层速攻完成，你能更快吗？" : "这次到 F" + report.floorsReached + "。下一次呢？";
        }
        
        if (cleared)
        {
            return mode == "expedition" ? "穿越四大章节，暗塔深处无人归来。" : "50层全通，这座塔已被征服。";
        }
        
        if (isPollution) return "被污染吞噬于 F" + report.floorsReached + "。";
        return "止步 F" + report.floorsReached + "，这次还不够。";
    }
    
    private string GetModeCTA(string mode)
    {
        switch (mode)
        {
            case "expedition": return "你敢征服暗塔吗？";
            case "classic": return "你能登顶吗？";
            default: return "你能打出什么评级？";
        }
    }
    
    public void ShareImage(Texture2D texture, string text)
    {
        if (texture == null)
        {
            Debug.LogError("[PosterShareSystem] ShareImage: texture is null");
            return;
        }

        byte[] bytes = texture.EncodeToPNG();
        string base64 = Convert.ToBase64String(bytes);
        
        #if UNITY_ANDROID && !UNITY_EDITOR
        try
        {
            var unityPlayerClass = System.Type.GetType("UnityEngine.AndroidJavaClass, UnityEngine.AndroidJNIModule");
            if (unityPlayerClass != null)
            {
                var unityPlayer = System.Activator.CreateInstance(unityPlayerClass, "com.unity3d.player.UnityPlayer");
                var getStaticMethod = unityPlayerClass.GetMethod("GetStatic");
                var currentActivity = getStaticMethod.MakeGenericMethod(System.Type.GetType("UnityEngine.AndroidJavaObject, UnityEngine.AndroidJNIModule"))
                    .Invoke(unityPlayer, new object[] { "currentActivity" });
                
                var androidJavaObjectClass = System.Type.GetType("UnityEngine.AndroidJavaObject, UnityEngine.AndroidJNIModule");
                var shareBridge = System.Activator.CreateInstance(androidJavaObjectClass, "com.parasite.tower.ShareBridge", currentActivity);
                var callMethod = androidJavaObjectClass.GetMethod("Call");
                callMethod.Invoke(shareBridge, new object[] { "shareImage", "data:image/png;base64," + base64, text });
                
                System.IDisposable bridgeDisposable = shareBridge as System.IDisposable;
                bridgeDisposable?.Dispose();
                System.IDisposable playerDisposable = unityPlayer as System.IDisposable;
                playerDisposable?.Dispose();
            }
        }
        catch { }
        #elif UNITY_IOS
        UnityEngine.iOS.Device.SetNoBackupFlag(Application.persistentDataPath);
        string path = Application.persistentDataPath + "/poster.png";
        System.IO.File.WriteAllBytes(path, bytes);
        UnityEngine.iOS.Device.ShareText(text, path);
        #else
        string path = Application.persistentDataPath + "/poster.png";
        System.IO.File.WriteAllBytes(path, bytes);

        #endif
    }
    
    private struct Palette
    {
        public Color primary;
        public Color bgTop;
        public Color bgMid;
        public Color bgBot;
        public Color glow1;
        public Color glow2;
        public Color particle;
        public int seed;
    }
    
    private struct RatingTheme
    {
        public Color color;
        public Color glow;
        public int tier;
        public string name;
    }
    
    private struct TierMeta
    {
        public string label;
        public string labelEN;
        public Color color;
        public Color glow;
        public int particles;
        public int frameLayers;
    }
    
    [Serializable]
    public class AchievementDef
    {
        public string id;
        public string name;
        public string description;
        public string icon;
    }
}