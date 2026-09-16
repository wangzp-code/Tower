using UnityEngine;
using UnityEditor;
using System.IO;

public class PerformanceQAReportGenerator : EditorWindow
{
    [MenuItem("Tools/Optimize/Generate Performance QA Report")]
    public static void GeneratePerformanceReport()
    {
        var report = BuildReport();
        var outPath = "Assets/OptimizationReports/performance_qa_report.txt";
        Directory.CreateDirectory(Path.GetDirectoryName(outPath));
        File.WriteAllText(outPath, report);
        AssetDatabase.Refresh();
        EditorUtility.DisplayDialog("性能 QA 报告已生成", "报告已写入: " + outPath, "OK");
    }

    private static string BuildReport()
    {
        var sb = new System.Text.StringBuilder();
        sb.AppendLine("Performance QA Report");
        sb.AppendLine("Generated: " + System.DateTime.Now);
        sb.AppendLine();
        sb.AppendLine("=== Runtime / Static Performance Settings ===");
        sb.AppendLine("Target Frame Rate: " + Application.targetFrameRate);
        sb.AppendLine("Graphics Device: " + SystemInfo.graphicsDeviceName);
        sb.AppendLine("Graphics Memory Size: " + SystemInfo.graphicsMemorySize + " MB");
        sb.AppendLine("System Memory Size: " + SystemInfo.systemMemorySize + " MB");
        sb.AppendLine("Processor Count: " + SystemInfo.processorCount);
        sb.AppendLine("Supports Instancing: " + SystemInfo.supportsInstancing);
        sb.AppendLine("Supports Compute Shaders: " + SystemInfo.supportsComputeShaders);
        sb.AppendLine();
        sb.AppendLine("=== Quality Settings ===");
        sb.AppendLine("Quality Level: " + QualitySettings.names[QualitySettings.GetQualityLevel()]);
        sb.AppendLine("Anti Aliasing: " + QualitySettings.antiAliasing);
        sb.AppendLine("Texture Quality: " + QualitySettings.globalTextureMipmapLimit);
        sb.AppendLine("Anisotropic Textures: " + QualitySettings.anisotropicFiltering);
        sb.AppendLine("Soft Shadows: " + QualitySettings.shadows);
        sb.AppendLine("Shadow Resolution: " + QualitySettings.shadowResolution);
        sb.AppendLine("Shadow Distance: " + QualitySettings.shadowDistance);
        sb.AppendLine();
        sb.AppendLine("=== Suggested QA Checks ===");
        sb.AppendLine("- 运行主战斗场景，检查 60 FPS 是否稳定。");
        sb.AppendLine("- 验证战斗特效与 UI 不造成帧率掉帧。");
        sb.AppendLine("- 检查移动端低端机是否开启移动优化配置。");
        sb.AppendLine("- 验证 `PerformanceManager.ApplyMobileOptimizations()` 在移动配置中生效。");
        sb.AppendLine("- 检查 `Tools -> Optimize` 中的报告是否生成。");
        sb.AppendLine();
        sb.AppendLine("=== Notes ===");
        sb.AppendLine("此报告为静态 QA 参考，建议与实际运行时帧率/内存数据一起验证。");
        return sb.ToString();
    }
}
