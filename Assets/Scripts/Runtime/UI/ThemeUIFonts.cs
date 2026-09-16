using UnityEngine;

/// <summary>
/// 主题字体加载：优先 Resources，其次系统中文字体。
/// </summary>
public static class ThemeUIFonts
{
    static Font _cached;
    static readonly string[] PreferredFonts =
    {
        "PingFang SC", "Hiragino Sans GB", "Microsoft YaHei UI",
        "Noto Sans CJK SC", "Source Han Sans SC", "Arial Unicode MS",
        "Roboto", "Arial"
    };

    public static Font Get(int size = 16)
    {
        if (_cached != null) return _cached;

        var themeFont = Resources.Load<Font>("Fonts/ThemeFont");
        if (themeFont != null)
        {
            _cached = themeFont;
            return _cached;
        }

        foreach (var name in PreferredFonts)
        {
            var f = Font.CreateDynamicFontFromOSFont(name, size);
            if (f != null)
            {
                _cached = f;
                return _cached;
            }
        }

        var fallback = Font.GetOSInstalledFontNames();
        if (fallback.Length > 0)
            _cached = Font.CreateDynamicFontFromOSFont(fallback[0], size);

        return _cached;
    }
}
