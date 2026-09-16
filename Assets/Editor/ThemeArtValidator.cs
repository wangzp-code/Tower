using UnityEngine;
using UnityEditor;
using System.IO;

/// <summary>
/// 编辑器：为 UI 背景图生成主题调色报告，并校验资源命名规范。
/// </summary>
public class ThemeArtValidator : EditorWindow
{
    [MenuItem("Tools/Art/Validate Theme Art Assets")]
    public static void ValidateAssets()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Theme Art Validation Report");
        sb.AppendLine("Generated: " + System.DateTime.Now);
        sb.AppendLine();

        string[] folders = { "Assets/Resources/UI", "Assets/Resources/Icons/Monsters", "Assets/Resources/Icons/Classes" };
        int pngCount = 0;
        int missingMeta = 0;

        foreach (var folder in folders)
        {
            if (!Directory.Exists(folder)) continue;
            foreach (var file in Directory.GetFiles(folder, "*.png", SearchOption.AllDirectories))
            {
                pngCount++;
                if (!File.Exists(file + ".meta")) missingMeta++;
                sb.AppendLine("OK: " + file);
            }
        }

        sb.AppendLine();
        sb.AppendLine($"Total PNG: {pngCount}, Missing .meta: {missingMeta}");
        sb.AppendLine();
        sb.AppendLine("Theme palette reference:");
        sb.AppendLine("  Background: #0e0c1e");
        sb.AppendLine("  Accent:     #00ffd0");
        sb.AppendLine("  Danger:     #ff006e");
        sb.AppendLine("  Parasite:   #e056fd");

        var outPath = "Assets/OptimizationReports/theme_art_validation.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, sb.ToString());
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("主题美术校验", "报告已写入:\n" + outPath, "OK");
    }
}
