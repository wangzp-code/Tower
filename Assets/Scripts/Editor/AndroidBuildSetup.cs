using UnityEditor;
using UnityEngine;

public class AndroidBuildSetup : EditorWindow
{
    [MenuItem("Tools/Android Setup")]
    public static void ShowWindow()
    {
        GetWindow<AndroidBuildSetup>("Android Build Setup");
    }

    private void OnGUI()
    {
        GUILayout.Label("Unity Android Build Configuration", EditorStyles.boldLabel);
        GUILayout.Space(10);

        if (GUILayout.Button("1. Configure Android Player Settings"))
        {
            ConfigurePlayerSettings();
        }

        if (GUILayout.Button("2. Add Scenes to Build"))
        {
            AddScenesToBuild();
        }

        if (GUILayout.Button("3. Switch to Android Platform"))
        {
            SwitchToAndroidPlatform();
        }

        GUILayout.Space(20);
        GUILayout.Label("Quick Setup (Run All)", EditorStyles.helpBox);
        if (GUILayout.Button("Run Full Android Setup"))
        {
            ConfigurePlayerSettings();
            AddScenesToBuild();
            SwitchToAndroidPlatform();
            EditorUtility.DisplayDialog("Complete", "Android setup complete! You can now build to Android.", "OK");
        }
    }

    private void ConfigurePlayerSettings()
    {
        // Set product name
        PlayerSettings.productName = "你也是我";

        // Set bundle identifier
        PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.parasitetower.game");

        // Set version
        PlayerSettings.bundleVersion = "1.0.0";
        PlayerSettings.Android.bundleVersionCode = 1;

        // Set orientation to portrait or auto-rotate
        PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
        PlayerSettings.allowedAutorotateToPortrait = true;
        PlayerSettings.allowedAutorotateToPortraitUpsideDown = false;
        PlayerSettings.allowedAutorotateToLandscapeLeft = false;
        PlayerSettings.allowedAutorotateToLandscapeRight = false;

        // Set target API level
        PlayerSettings.Android.targetSdkVersion = (AndroidSdkVersions)34;
        PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;

        // Set scripting backend
        PlayerSettings.SetScriptingBackend(BuildTargetGroup.Android, ScriptingImplementation.IL2CPP);

        // Set target architectures
        PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;

        // Set internet access
        PlayerSettings.Android.forceSDCardPermission = false;

        // Disable splash screen for development
        PlayerSettings.SplashScreen.show = false;

        // Keystore configuration — set paths before building release APK
        // PlayerSettings.Android.keystoreName = "parasite-tower.keystore";
        // PlayerSettings.Android.keystorePass = "";
        // PlayerSettings.Android.keyaliasName = "parasitetower";
        // PlayerSettings.Android.keyaliasPass = "";

        // Set icon
        Texture2D defaultIcon = AssetDatabase.GetBuiltinExtraResource<Texture2D>("UI/Skin/Background.psd");
        if (defaultIcon != null)
        {
            PlayerSettings.SetIconsForTargetGroup(BuildTargetGroup.Unknown, new Texture2D[] { defaultIcon });
        }
    }

    private void AddScenesToBuild()
    {
        var scenes = new EditorBuildSettingsScene[2];
        scenes[0] = new EditorBuildSettingsScene("Assets/Scenes/BootstrapScene.unity", true);
        scenes[1] = new EditorBuildSettingsScene("Assets/Scenes/Game.unity", true);
        EditorBuildSettings.scenes = scenes;

        Debug.Log("Scenes added to Build Settings!");
    }

    private void SwitchToAndroidPlatform()
    {
        if (EditorUserBuildSettings.activeBuildTarget != BuildTarget.Android)
        {
            EditorUserBuildSettings.SwitchActiveBuildTargetAsync(BuildTargetGroup.Android, BuildTarget.Android);
        }
        else
        {
            Debug.Log("Already on Android platform!");
        }
    }
}
