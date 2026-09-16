using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

/// <summary>
/// 编辑器测试运行器
/// 提供菜单命令来运行冒烟测试和性能测试
/// </summary>
public static class EditorTestRunner
{
    [MenuItem("Tools/运行冒烟测试")]
    public static void RunSmokeTest()
    {
        var runner = Object.FindObjectOfType<SmokeTestRunner>();
        if (runner == null)
        {
            var go = new GameObject("[SmokeTestRunner]");
            runner = go.AddComponent<SmokeTestRunner>();
        }
        
        runner.RunTests();
    }

    [MenuItem("Tools/性能诊断")]
    public static void RunDiagnostics()
    {
        var diag = Object.FindObjectOfType<GameDiagnostics>();
        if (diag == null)
        {
            var go = new GameObject("[GameDiagnostics]");
            diag = go.AddComponent<GameDiagnostics>();
        }
        
        diag.CollectDiagnostics();
        var report = diag.GenerateReport();
        Debug.Log(report.ToSummaryString());
    }

    [MenuItem("Tools/查看存档目录")]
    public static void OpenSaveDirectory()
    {
        string saveDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        
        if (!System.IO.Directory.Exists(saveDir))
        {
            System.IO.Directory.CreateDirectory(saveDir);
        }
        
        EditorUtility.RevealInFinder(saveDir);
    }

    [MenuItem("Tools/清理存档")]
    public static void ClearSaveFiles()
    {
        if (!EditorUtility.DisplayDialog("确认", "确定要清理所有存档文件吗？此操作不可恢复！", "确定清理", "取消"))
        {
            return;
        }
        
        string saveDir = System.IO.Path.Combine(Application.persistentDataPath, "Saves");
        
        if (System.IO.Directory.Exists(saveDir))
        {
            var files = System.IO.Directory.GetFiles(saveDir);
            foreach (var file in files)
            {
                System.IO.File.Delete(file);
            }
            Debug.Log($"[EditorTestRunner] 已清理 {files.Length} 个存档文件");
        }
    }

    [MenuItem("Tools/检查项目状态")]
    public static void CheckProjectStatus()
    {
        var status = new List<string>();
        
        // 检查关键文件
        string[] criticalFiles = new[]
        {
            "Assets/Scripts/Runtime/Core/GameManager.cs",
            "Assets/Scripts/Runtime/Core/Bootstrap.cs",
            "Assets/Scripts/Runtime/Core/SaveSystem.cs",
            "Assets/Scripts/Runtime/UI/CanvasUIManager.cs",
            "Assets/Scripts/Runtime/Gameplay/CompleteGameSystem.cs",
            "Assets/Scripts/Runtime/HotUpdate/HotUpdateManager.cs",
        };
        
        foreach (var file in criticalFiles)
        {
            string fullPath = System.IO.Path.Combine(Application.dataPath, file.Replace("Assets/", ""));
            bool exists = System.IO.File.Exists(fullPath);
            status.Add($"{(exists ? "✅" : "❌")} {file}");
        }
        
        // 检查场景
        string[] requiredScenes = new[]
        {
            "Assets/Scenes/MainMenu.unity",
            "Assets/Scenes/Gameplay.unity",
        };
        
        foreach (var scene in requiredScenes)
        {
            string fullPath = System.IO.Path.Combine(Application.dataPath, scene.Replace("Assets/", ""));
            bool exists = System.IO.File.Exists(fullPath);
            status.Add($"{(exists ? "✅" : "❌")} {scene}");
        }
        
        // 检查包依赖
        string manifestPath = System.IO.Path.Combine(Application.dataPath, "../Packages/manifest.json");
        if (System.IO.File.Exists(manifestPath))
        {
            status.Add("✅ Packages/manifest.json");
        }
        else
        {
            status.Add("❌ Packages/manifest.json 不存在");
        }
        
        Debug.Log("========== 项目状态检查 ==========");
        foreach (var line in status)
        {
            Debug.Log(line);
        }
        Debug.Log("===================================");
    }

    [MenuItem("Tools/Android打包检查")]
    public static void CheckAndroidBuild()
    {
        var issues = new List<string>();
        var info = new List<string>();
        
        // 检查AndroidManifest
        string manifestPath = System.IO.Path.Combine(Application.dataPath, "Plugins/Android/AndroidManifest.xml");
        if (System.IO.File.Exists(manifestPath))
        {
            info.Add("✅ AndroidManifest.xml 存在");
            
            string content = System.IO.File.ReadAllText(manifestPath);
            
            // 检查网络权限
            if (content.Contains("INTERNET"))
            {
                info.Add("✅ INTERNET 权限");
            }
            else
            {
                issues.Add("❌ 缺少 INTERNET 权限");
            }
            
            // 检查明文网络
            if (content.Contains("usesCleartextTraffic=\"true\""))
            {
                info.Add("✅ 允许明文HTTP请求（热更新需要）");
            }
            else
            {
                issues.Add("⚠️ 建议添加 usesCleartextTraffic=\"true\" 以支持本地HTTP服务器");
            }
        }
        else
        {
            issues.Add("❌ AndroidManifest.xml 不存在");
        }
        
        // 检查构建设置
        if (PlayerSettings.applicationIdentifier == "")
        {
            issues.Add("❌ 未设置 Application Identifier");
        }
        else
        {
            info.Add($"✅ Application Identifier: {PlayerSettings.applicationIdentifier}");
        }
        
        info.Add($"✅ Bundle Version: {PlayerSettings.bundleVersion}");
        info.Add($"✅ Target SDK: {PlayerSettings.Android.targetSdkVersion}");
        info.Add($"✅ Min SDK: {PlayerSettings.Android.minSdkVersion}");
        
        // 输出结果
        Debug.Log("========== Android打包检查 ==========");
        Debug.Log("信息:");
        foreach (var line in info)
        {
            Debug.Log($"  {line}");
        }
        
        if (issues.Count > 0)
        {
            Debug.Log("\n问题:");
            foreach (var line in issues)
            {
                Debug.Log($"  {line}");
            }
        }
        else
        {
            Debug.Log("\n✅ 所有检查通过，可以进行Android打包");
        }
        Debug.Log("===================================");
    }
}
