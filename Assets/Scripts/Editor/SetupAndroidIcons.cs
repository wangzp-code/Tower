using UnityEditor;
using UnityEngine;
using System.IO;

public class SetupAndroidIcons : EditorWindow
{
    [MenuItem("Tools/Setup Android Icons")]
    public static void ShowWindow()
    {
        GetWindow<SetupAndroidIcons>("Setup Android Icons");
    }

    private void OnGUI()
    {
        GUILayout.Label("Android Icon Setup", EditorStyles.boldLabel);
        GUILayout.Space(10);
        
        GUILayout.Label("Android requires icons in multiple resolutions:", EditorStyles.label);
        GUILayout.Label("- mipmap-ldpi: 36x36", EditorStyles.miniLabel);
        GUILayout.Label("- mipmap-mdpi: 48x48", EditorStyles.miniLabel);
        GUILayout.Label("- mipmap-hdpi: 72x72", EditorStyles.miniLabel);
        GUILayout.Label("- mipmap-xhdpi: 96x96", EditorStyles.miniLabel);
        GUILayout.Label("- mipmap-xxhdpi: 144x144", EditorStyles.miniLabel);
        GUILayout.Label("- mipmap-xxxhdpi: 192x192", EditorStyles.miniLabel);
        GUILayout.Space(15);
        
        GUILayout.Label("Option 1: Use Player Settings (Recommended)", EditorStyles.boldLabel);
        if (GUILayout.Button("Open Player Settings", GUILayout.Height(30)))
        {
            SettingsService.OpenProjectSettings("Project/Player");
        }
        
        GUILayout.Space(15);
        GUILayout.Label("Option 2: Create Icons from Source Image", EditorStyles.boldLabel);
        GUILayout.Label("Drag a source image (512x512+) to: Assets/Plugins/Android/icon_source.png", EditorStyles.label);
        GUILayout.Space(10);
        
        if (GUILayout.Button("Generate Android Icons from Source", GUILayout.Height(40)))
        {
            GenerateAndroidIcons();
        }
        
        GUILayout.Space(15);
        GUILayout.Label("Option 3: Quick Setup with Default Icon", EditorStyles.boldLabel);
        if (GUILayout.Button("Use Unity Default Icon", GUILayout.Height(30)))
        {
            UseDefaultIcon();
        }
    }
    
    private void GenerateAndroidIcons()
    {
        string sourcePath = "Assets/Plugins/Android/icon_source.png";
        
        if (!File.Exists(sourcePath))
        {
            EditorUtility.DisplayDialog(
                "Error",
                "Source icon not found at: " + sourcePath + "\n\n" +
                "Please create a 512x512+ PNG image and place it at:\n" +
                "Assets/Plugins/Android/icon_source.png",
                "OK");
            return;
        }
        
        try
        {
            Texture2D source = AssetDatabase.LoadAssetAtPath<Texture2D>(sourcePath);
            if (source == null)
            {
                EditorUtility.DisplayDialog("Error", "Failed to load source icon.", "OK");
                return;
            }
            
            string outputDir = "Assets/Plugins/Android/res";
            Directory.CreateDirectory(outputDir);
            
            var sizes = new[]
            {
                new { dir = "mipmap-ldpi", size = 36 },
                new { dir = "mipmap-mdpi", size = 48 },
                new { dir = "mipmap-hdpi", size = 72 },
                new { dir = "mipmap-xhdpi", size = 96 },
                new { dir = "mipmap-xxhdpi", size = 144 },
                new { dir = "mipmap-xxxhdpi", size = 192 }
            };
            
            foreach (var s in sizes)
            {
                string dirPath = Path.Combine(outputDir, s.dir);
                Directory.CreateDirectory(dirPath);
                
                Texture2D resized = ResizeTexture(source, s.size);
                string outPath = Path.Combine(dirPath, "app_icon.png");
                File.WriteAllBytes(outPath, resized.EncodeToPNG());
                
                DestroyImmediate(resized);
            }
            
            AssetDatabase.Refresh();
            
            EditorUtility.DisplayDialog(
                "Success",
                "Android icons generated successfully!\n\n" +
                "Note: If you get OBSELETE error during build, use Player Settings instead.\n\n" +
                "For Player Settings approach:\n" +
                "1. Open Edit > Project Settings > Player\n" +
                "2. Select Android tab\n" +
                "3. Go to Icon section\n" +
                "4. Assign the source image (512x512+)",
                "OK");
        }
        catch (System.Exception e)
        {
            EditorUtility.DisplayDialog("Error", "Failed to generate icons: " + e.Message, "OK");
        }
    }
    
    private Texture2D ResizeTexture(Texture2D source, int size)
    {
        var rt = RenderTexture.GetTemporary(size, size, 0, RenderTextureFormat.ARGB32);
        var prev = RenderTexture.active;
        RenderTexture.active = rt;
        GL.Clear(true, true, Color.black);
        Graphics.Blit(source, rt);
        
        var result = new Texture2D(size, size, TextureFormat.ARGB32, false);
        result.ReadPixels(new Rect(0, 0, size, size), 0, 0);
        result.Apply();
        
        RenderTexture.active = prev;
        RenderTexture.ReleaseTemporary(rt);
        
        return result;
    }
    
    private void UseDefaultIcon()
    {
        if (EditorUtility.DisplayDialog(
            "Use Default Icon",
            "This will configure Unity's default icon for Android.\n\n" +
            "The default icon is the Unity logo. You can change it later in:\n" +
            "Edit > Project Settings > Player > Icon > Android",
            "Use Default", "Cancel"))
        {
            Texture2D defaultIcon = AssetDatabase.GetBuiltinExtraResource<Texture2D>("UI/Skin/Background.psd");
            
            PlayerSettings.SetIconsForTargetGroup(
                BuildTargetGroup.Android, 
                new Texture2D[] { defaultIcon });
            
            EditorUtility.DisplayDialog(
                "Success",
                "Default icon configured.\n\n" +
                "To change: Edit > Project Settings > Player > Icon > Android\n" +
                "Click 'Select...' under Android section and assign your icon.",
                "OK");
        }
    }
}
