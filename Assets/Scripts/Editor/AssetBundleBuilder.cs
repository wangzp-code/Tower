using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using HotUpdate;

public class AssetBundleBuilder : EditorWindow
{
    private string outputPath = "Bundles";
    private string version = "1.0.0";
    private BuildTarget buildTarget = BuildTarget.Android;
    private List<string> bundleNames = new List<string>();
    
    [MenuItem("Tools/AssetBundle Builder")]
    public static void ShowWindow()
    {
        GetWindow<AssetBundleBuilder>("AssetBundle Builder");
    }
    
    private void OnGUI()
    {
        GUILayout.Label("AssetBundle 打包工具", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        outputPath = EditorGUILayout.TextField("输出路径", outputPath);
        version = EditorGUILayout.TextField("版本号", version);
        buildTarget = (BuildTarget)EditorGUILayout.EnumPopup("平台", buildTarget);
        
        GUILayout.Space(15);
        GUILayout.Label("要打包的资源目录:", EditorStyles.label);
        
        if (GUILayout.Button("添加 Resources 下的资源", GUILayout.Height(30)))
        {
            AddResourcesBundles();
        }
        
        if (GUILayout.Button("添加指定目录", GUILayout.Height(30)))
        {
            string folder = EditorUtility.OpenFolderPanel("选择要打包的目录", "Assets", "");
            if (!string.IsNullOrEmpty(folder))
            {
                AddFolderBundles(folder);
            }
        }
        
        GUILayout.Space(15);
        
        EditorGUILayout.LabelField("已添加的 Bundles:", EditorStyles.boldLabel);
        for (int i = 0; i < bundleNames.Count; i++)
        {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.LabelField(bundleNames[i]);
            if (GUILayout.Button("删除", GUILayout.Width(50)))
            {
                bundleNames.RemoveAt(i);
                i--;
            }
            EditorGUILayout.EndHorizontal();
        }
        
        GUILayout.Space(20);
        
        if (GUILayout.Button("打包 AssetBundle", GUILayout.Height(50)))
        {
            BuildAssetBundles();
        }
        
        if (GUILayout.Button("生成版本清单", GUILayout.Height(30)))
        {
            GenerateVersionManifest();
        }
    }
    
    private void AddResourcesBundles()
    {
        string resourcesPath = "Assets/Resources";
        if (!Directory.Exists(resourcesPath)) return;
        
        string[] folders = Directory.GetDirectories(resourcesPath);
        foreach (string folder in folders)
        {
            string relativePath = folder.Replace("Assets/", "");
            string bundleName = relativePath.Replace("/", "_").ToLower();
            
            if (!bundleNames.Contains(bundleName))
            {
                bundleNames.Add(bundleName);
                AssignBundleLabel(folder, bundleName);
            }
        }
    }
    
    private void AddFolderBundles(string folderPath)
    {
        string bundleName = folderPath.Replace("Assets/", "").Replace("/", "_").ToLower();
        if (!bundleNames.Contains(bundleName))
        {
            bundleNames.Add(bundleName);
            AssignBundleLabel(folderPath, bundleName);
        }
    }
    
    private void AssignBundleLabel(string folderPath, string bundleName)
    {
        string[] guids = AssetDatabase.FindAssets("", new[] { folderPath });
        foreach (string guid in guids)
        {
            string assetPath = AssetDatabase.GUIDToAssetPath(guid);
            var importer = AssetImporter.GetAtPath(assetPath);
            if (importer != null)
            {
                importer.assetBundleName = bundleName;
                importer.SaveAndReimport();
            }
        }
        Debug.Log($"[AssetBundleBuilder] Assign bundle '{bundleName}' to '{folderPath}'");
    }
    
    private void BuildAssetBundles()
    {
        if (bundleNames.Count == 0)
        {
            EditorUtility.DisplayDialog("Error", "请先添加要打包的资源", "OK");
            return;
        }
        
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputPath, version);
        if (!Directory.Exists(buildPath))
        {
            Directory.CreateDirectory(buildPath);
        }
        
        BuildTarget target = buildTarget;
        if (EditorUserBuildSettings.activeBuildTarget != target)
        {
            if (!EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, target))
            {
                EditorUtility.DisplayDialog("Error", $"无法切换到 {target} 平台", "OK");
                return;
            }
        }
        
        EditorUtility.DisplayProgressBar("打包中", "Building AssetBundles...", 0.5f);
        
        try
        {
            var manifest = BuildPipeline.BuildAssetBundles(
                buildPath,
                BuildAssetBundleOptions.ChunkBasedCompression | BuildAssetBundleOptions.DeterministicAssetBundle,
                target);
            
            if (manifest != null)
            {
                GenerateManifest(manifest, buildPath);
                GenerateVersionFile(buildPath);
                
                EditorUtility.DisplayDialog("Success", 
                    $"打包完成！\n\n输出路径: {buildPath}\nBundle 数量: {manifest.GetAllAssetBundles().Length}", 
                    "OK");
            }
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", $"打包失败: {e.Message}", "OK");
            Debug.LogError(e);
        }
        finally
        {
            EditorUtility.ClearProgressBar();
        }
    }
    
    private void GenerateManifest(AssetBundleManifest manifest, string buildPath)
    {
        var bundles = manifest.GetAllAssetBundles();
        var manifestData = new ManifestData
        {
            version = version,
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
            bundles = new List<BundleInfo>()
        };
        
        foreach (string bundle in bundles)
        {
            string bundlePath = Path.Combine(buildPath, bundle);
            if (File.Exists(bundlePath))
            {
                var info = new FileInfo(bundlePath);
                string hash = CalculateHash(bundlePath);
                
                manifestData.bundles.Add(new BundleInfo
                {
                    name = bundle,
                    size = info.Length,
                    hash = hash,
                    version = version
                });
            }
        }
        
        string json = JsonUtility.ToJson(manifestData, true);
        
        string manifestPath = Path.Combine(buildPath, "manifest.json");
        File.WriteAllText(manifestPath, json);
        Debug.Log($"[AssetBundleBuilder] Manifest saved to: {manifestPath}");
    }
    
    private void GenerateVersionFile(string buildPath)
    {
        var versionData = new VersionFile
        {
            version = version,
            minVersion = "0.0.0",
            updateUrl = "https://your-server.com/update",
            downloadUrl = $"https://your-server.com/bundles/{version}/",
            forceUpdate = false,
            releaseNotes = "请查看更新日志",
            timestamp = System.DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
        
        string json = JsonUtility.ToJson(versionData, true);
        string versionPath = Path.Combine(buildPath, "version.json");
        File.WriteAllText(versionPath, json);
        Debug.Log($"[AssetBundleBuilder] Version file saved to: {versionPath}");
    }
    
    private void GenerateVersionManifest()
    {
        string buildPath = Path.Combine(Directory.GetParent(Application.dataPath).FullName, outputPath, version);
        if (!Directory.Exists(buildPath))
        {
            EditorUtility.DisplayDialog("Error", $"未找到打包目录: {buildPath}", "OK");
            return;
        }
        
        string manifestPath = Path.Combine(buildPath, "manifest.json");
        if (!File.Exists(manifestPath))
        {
            EditorUtility.DisplayDialog("Error", $"未找到 manifest.json，请先打包", "OK");
            return;
        }
        
        EditorUtility.RevealInFinder(manifestPath);
    }
    
    private string CalculateHash(string filePath)
    {
        using (var md5 = MD5.Create())
        using (var stream = File.OpenRead(filePath))
        {
            byte[] hashBytes = md5.ComputeHash(stream);
            var builder = new StringBuilder();
            foreach (byte b in hashBytes)
            {
                builder.Append(b.ToString("x2"));
            }
            return builder.ToString();
        }
    }
}
