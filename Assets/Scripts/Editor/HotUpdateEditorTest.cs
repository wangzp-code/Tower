using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using HotUpdate;

public static class HotUpdateEditorTest
{
    [MenuItem("Tools/HotUpdate/Run Editor Tests")]
    public static void RunEditorTests()
    {
        Debug.Log("========================================");
        Debug.Log("  Hot Update Editor Tests Starting");
        Debug.Log("========================================");

        int passed = 0;
        int failed = 0;

        // Test 1: Config parsing
        Debug.Log("\n--- Test 1: HotUpdateConfig Default Values ---");
        try
        {
            var config = new HotUpdateConfig();
            Assert(config.baseUrl == "https://your-server.com/bundles/", "Default baseUrl");
            Assert(config.versionFileName == "version.json", "Default versionFileName");
            Assert(config.maxDownloadThreads == 3, "Default maxDownloadThreads");
            Assert(config.checkInterval == 3600f, "Default checkInterval");
            Debug.Log("[PASS] Test 1: HotUpdateConfig defaults");
            passed++;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FAIL] Test 1: {e.Message}");
            failed++;
        }

        // Test 2: Version file parse
        Debug.Log("\n--- Test 2: VersionFile Parse ---");
        try
        {
            string json = File.ReadAllText("Tools/HotUpdateMockServer/version.json");
            var versionFile = JsonUtility.FromJson<VersionFile>(json);
            Assert(versionFile != null, "VersionFile not null");
            Assert(versionFile.version == "1.1.0", "Version = 1.1.0");
            Assert(versionFile.minVersion == "1.0.0", "MinVersion = 1.0.0");
            Assert(versionFile.forceUpdate == false, "ForceUpdate = false");
            Assert(versionFile.releaseNotes.Contains("1.1.0"), "ReleaseNotes contains version");
            Debug.Log($"[PASS] Test 2: VersionFile parsed (version={versionFile.version})");
            passed++;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FAIL] Test 2: {e.Message}");
            failed++;
        }

        // Test 3: Manifest data parse
        Debug.Log("\n--- Test 3: ManifestData Parse ---");
        try
        {
            string json = File.ReadAllText("Tools/HotUpdateMockServer/1.1.0/manifest.json");
            var manifest = JsonUtility.FromJson<ManifestData>(json);
            Assert(manifest != null, "Manifest not null");
            Assert(manifest.version == "1.1.0", "Manifest version = 1.1.0");
            Assert(manifest.bundles.Count == 2, "Bundle count = 2");

            var testBundle = manifest.GetBundleInfo("test_assets.bundle");
            Assert(testBundle != null, "test_assets.bundle found");
            Assert(testBundle.size == 51, "test_assets.bundle size = 51");
            Assert(testBundle.hash == "897b11172a7b8bad8859801117c7e9c7", "test_assets.bundle hash");

            var configBundle = manifest.GetBundleInfo("game_configs.bundle");
            Assert(configBundle != null, "game_configs.bundle found");
            Assert(configBundle.size == 52, "game_configs.bundle size = 52");
            Assert(configBundle.hash == "413c2078e136dbb472c4908ab3e99f03", "game_configs.bundle hash");

            var notFound = manifest.GetBundleInfo("nonexistent");
            Assert(notFound == null, "Nonexistent bundle returns null");

            Debug.Log($"[PASS] Test 3: ManifestData parsed ({manifest.bundles.Count} bundles)");
            passed++;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FAIL] Test 3: {e.Message}");
            failed++;
        }

        // Test 4: Version comparison logic
        Debug.Log("\n--- Test 4: Version Comparison ---");
        try
        {
            var go = new GameObject("TestManager");
            var manager = go.AddComponent<HotUpdateManager>();

            var method = typeof(HotUpdateManager).GetMethod("IsNewerVersion",
                System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);

            Assert(method != null, "IsNewerVersion method exists");

            Assert((bool)method.Invoke(manager, new object[] { "1.1.0", "1.0.0" }), "1.1.0 > 1.0.0");
            Assert((bool)method.Invoke(manager, new object[] { "2.0.0", "1.9.9" }), "2.0.0 > 1.9.9");
            Assert((bool)method.Invoke(manager, new object[] { "1.1.1", "1.1.0" }), "1.1.1 > 1.1.0");
            Assert(!(bool)method.Invoke(manager, new object[] { "1.0.0", "1.1.0" }), "1.0.0 not > 1.1.0");
            Assert(!(bool)method.Invoke(manager, new object[] { "1.0.0", "1.0.0" }), "1.0.0 not > 1.0.0");
            Assert(!(bool)method.Invoke(manager, new object[] { "0.9.0", "1.0.0" }), "0.9.0 not > 1.0.0");

            Debug.Log("[PASS] Test 4: Version comparison logic");
            passed++;

            Object.DestroyImmediate(go);
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FAIL] Test 4: {e.Message}");
            failed++;
        }

        // Test 5: DownloadTask structure
        Debug.Log("\n--- Test 5: DownloadTask Structure ---");
        try
        {
            var task = new DownloadTask
            {
                bundleName = "test.bundle",
                hash = "abc123",
                size = 1024,
                downloadUrl = "http://localhost/bundle",
                localPath = "/tmp/test.bundle",
                progress = 0.5f,
                isComplete = false
            };

            Assert(task.bundleName == "test.bundle", "bundleName");
            Assert(task.size == 1024, "size");
            Assert(Mathf.Approximately(task.progress, 0.5f), "progress");

            Debug.Log("[PASS] Test 5: DownloadTask structure");
            passed++;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FAIL] Test 5: {e.Message}");
            failed++;
        }

        // Test 6: HotUpdateConfig serialization
        Debug.Log("\n--- Test 6: Config Serialization ---");
        try
        {
            var config = new HotUpdateConfig
            {
                baseUrl = "http://localhost:8888/",
                versionFileName = "version.json",
                manifestFileName = "manifest.json",
                localBundlePath = "Bundles/",
                maxDownloadThreads = 5,
                checkInterval = 60f
            };

            string json = JsonUtility.ToJson(config);
            var loaded = JsonUtility.FromJson<HotUpdateConfig>(json);

            Assert(loaded.baseUrl == config.baseUrl, "baseUrl roundtrip");
            Assert(loaded.maxDownloadThreads == config.maxDownloadThreads, "maxDownloadThreads roundtrip");
            Assert(loaded.checkInterval == config.checkInterval, "checkInterval roundtrip");

            Debug.Log("[PASS] Test 6: Config serialization roundtrip");
            passed++;
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[FAIL] Test 6: {e.Message}");
            failed++;
        }

        // Summary
        Debug.Log("\n========================================");
        Debug.Log($"  Editor Tests: {passed} passed, {failed} failed");
        Debug.Log("========================================");

        if (failed > 0)
        {
            Debug.LogError("[HotUpdateEditorTest] Some tests FAILED!");
        }
        else
        {
            Debug.Log("[HotUpdateEditorTest] All editor tests PASSED!");
        }
    }

    private static void Assert(bool condition, string message)
    {
        if (!condition)
        {
            throw new System.Exception($"Assertion failed: {message}");
        }
    }
}