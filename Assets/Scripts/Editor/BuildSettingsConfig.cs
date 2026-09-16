using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;

public class BuildSettingsConfig : EditorWindow
{
    [MenuItem("Tools/Configure Build Settings")]
    public static void ShowWindow()
    {
        GetWindow<BuildSettingsConfig>("Build Settings Config");
    }

    private void OnGUI()
    {
        GUILayout.Label("Build Settings Configuration", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("Configure Scenes"))
        {
            ConfigureScenes();
        }

        if (GUILayout.Button("Set Player Settings"))
        {
            SetPlayerSettings();
        }

        if (GUILayout.Button("Build Development"))
        {
            BuildDevelopment();
        }

        if (GUILayout.Button("Build Production"))
        {
            BuildProduction();
        }

        GUILayout.Space(20);
        GUILayout.Label("Current Scenes in Build:", EditorStyles.label);
        
        foreach (var scene in EditorBuildSettings.scenes)
        {
            GUILayout.Label($"- {scene.path}");
        }
    }

    private void ConfigureScenes()
    {
        EditorBuildSettings.scenes = new[]
        {
            new EditorBuildSettingsScene("Assets/Scenes/BootstrapScene.unity", true),
            new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true)
        };

        EditorUtility.DisplayDialog("Success", "Build scenes configured!", "OK");
    }

    private void SetPlayerSettings()
    {
        PlayerSettings.productName = "你也是我";
        PlayerSettings.companyName = "GameDev Studio";
        PlayerSettings.applicationIdentifier = "com.parasitetower.game";

        EditorUtility.DisplayDialog("Success", "Player settings configured!", "OK");
    }

    private void BuildDevelopment()
    {
        BuildAndroid(true);
    }

    private void BuildProduction()
    {
        BuildAndroid(false);
    }

    private void BuildAndroid(bool development)
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUtility.DisplayDialog("Switching Target", "Switching to Android build target...", "OK");
            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);
        }

        string buildPath = development ? "Builds/Dev/ParasiteTower.apk" : "Builds/Prod/ParasiteTower.apk";
        System.IO.Directory.CreateDirectory(System.IO.Path.GetDirectoryName(buildPath));

        BuildOptions options = development 
            ? (BuildOptions.Development | BuildOptions.AllowDebugging) 
            : BuildOptions.None;

        BuildReport report = BuildPipeline.BuildPlayer(
            EditorBuildSettings.scenes,
            buildPath,
            BuildTarget.Android,
            options
        );

        if (report.summary.result == BuildResult.Succeeded)
        {
            EditorUtility.DisplayDialog("Success", $"Build completed!\nPath: {buildPath}", "OK");
        }
        else
        {
            EditorUtility.DisplayDialog("Build Failed", $"Errors: {report.summary.totalErrors}\nCheck Console for details", "OK");
        }
    }
}