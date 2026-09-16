using UnityEngine;
using UnityEditor;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;
using HotUpdate;

public static class HotUpdateIntegrationTest
{
    private const string MockServerUrl = "http://127.0.0.1:8888/";

    [MenuItem("Tools/HotUpdate/Integration Test")]
    public static void RunIntegrationTest()
    {
        Debug.Log("========================================");
        Debug.Log("  Hot Update Integration Test Starting");
        Debug.Log("========================================");

        var runner = new GameObject("HotUpdateTestRunner");
        runner.AddComponent<TestRunnerMono>();

        Debug.Log("[IntegrationTest] Test runner created. Running tests...");
    }

    private class TestRunnerMono : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(RunAllTests());
        }

        private IEnumerator RunAllTests()
        {
            int passed = 0;
            int failed = 0;

            // Test 1: Check mock server is accessible
            Debug.Log("\n--- Test 1: Mock Server Accessibility ---");
            using (var req = new UnityWebRequest(MockServerUrl, UnityWebRequest.kHttpVerbGET))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    Debug.Log("[PASS] Mock server is accessible");
                    passed++;
                }
                else
                {
                    Debug.LogError($"[FAIL] Mock server not accessible: {req.error}");
                    failed++;
                }
            }

            // Test 2: Download version.json
            Debug.Log("\n--- Test 2: Download version.json ---");
            string versionUrl = MockServerUrl + "version.json";
            VersionFile versionFile = null;
            using (var req = new UnityWebRequest(versionUrl, UnityWebRequest.kHttpVerbGET))
            {
                yield return req.SendWebRequest();
                if (req.result == UnityWebRequest.Result.Success)
                {
                    try
                    {
                        versionFile = JsonUtility.FromJson<VersionFile>(req.downloadHandler.text);
                        Debug.Log($"[PASS] version.json downloaded: version={versionFile.version}");
                        passed++;
                    }
                    catch (System.Exception e)
                    {
                        Debug.LogError($"[FAIL] Failed to parse version.json: {e.Message}");
                        failed++;
                    }
                }
                else
                {
                    Debug.LogError($"[FAIL] Failed to download version.json: {req.error}");
                    failed++;
                }
            }

            // Test 3: Validate version comparison
            if (versionFile != null)
            {
                Debug.Log("\n--- Test 3: Version Comparison ---");
                bool isNewer = IsNewerVersion(versionFile.version, "1.0.0");
                if (isNewer)
                {
                    Debug.Log($"[PASS] {versionFile.version} is newer than 1.0.0");
                    passed++;
                }
                else
                {
                    Debug.LogError($"[FAIL] {versionFile.version} should be newer than 1.0.0");
                    failed++;
                }
            }

            // Test 4: Download manifest.json for new version
            if (versionFile != null)
            {
                Debug.Log("\n--- Test 4: Download manifest.json ---");
                string manifestUrl = MockServerUrl + versionFile.version + "/manifest.json";
                ManifestData manifest = null;
                using (var req = new UnityWebRequest(manifestUrl, UnityWebRequest.kHttpVerbGET))
                {
                    yield return req.SendWebRequest();
                    if (req.result == UnityWebRequest.Result.Success)
                    {
                        try
                        {
                            manifest = JsonUtility.FromJson<ManifestData>(req.downloadHandler.text);
                            Debug.Log($"[PASS] manifest.json downloaded: {manifest?.bundles?.Count ?? 0} bundles");
                            passed++;
                        }
                        catch (System.Exception e)
                        {
                            Debug.LogError($"[FAIL] Failed to parse manifest.json: {e.Message}");
                            failed++;
                        }
                    }
                    else
                    {
                        Debug.LogError($"[FAIL] Failed to download manifest.json: {req.error}");
                        failed++;
                    }
                }

                // Test 5: Download test bundle
                if (manifest != null && manifest.bundles.Count > 0)
                {
                    Debug.Log("\n--- Test 5: Download Test Bundle ---");
                    var testBundle = manifest.bundles[0];
                    string bundleUrl = MockServerUrl + versionFile.version + "/" + testBundle.name;
                    string tempPath = Path.Combine(Application.persistentDataPath, "test_bundle.tmp");

                    using (var req = new UnityWebRequest(bundleUrl, UnityWebRequest.kHttpVerbGET))
                    {
                        var handler = new DownloadHandlerFile(tempPath);
                        req.downloadHandler = handler;
                        yield return req.SendWebRequest();

                        if (req.result == UnityWebRequest.Result.Success)
                        {
                            Debug.Log($"[PASS] Bundle downloaded: {testBundle.name} ({testBundle.size} bytes)");
                            passed++;

                            // Test 6: Hash verification
                            Debug.Log("\n--- Test 6: Hash Verification ---");
                            string actualHash = CalculateMD5(tempPath);
                            if (actualHash == testBundle.hash)
                            {
                                Debug.Log($"[PASS] Hash verified: {actualHash}");
                                passed++;
                            }
                            else
                            {
                                Debug.LogError($"[FAIL] Hash mismatch. Expected: {testBundle.hash}, Got: {actualHash}");
                                failed++;
                            }

                            if (File.Exists(tempPath))
                            {
                                File.Delete(tempPath);
                            }
                        }
                        else
                        {
                            Debug.LogError($"[FAIL] Failed to download bundle: {req.error}");
                            failed++;
                        }
                    }
                }

                // Test 7: Test manifest save/load
                Debug.Log("\n--- Test 7: Manifest Save/Load ---");
                try
                {
                    string savePath = Path.Combine(Application.persistentDataPath, "Bundles", "manifest.json");
                    string dir = Path.GetDirectoryName(savePath);
                    if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
                    
                    string json = JsonUtility.ToJson(manifest, true);
                    File.WriteAllText(savePath, json);
                    
                    string loadedJson = File.ReadAllText(savePath);
                    var loaded = JsonUtility.FromJson<ManifestData>(loadedJson);
                    
                    if (loaded != null && loaded.version == manifest.version && loaded.bundles.Count == manifest.bundles.Count)
                    {
                        Debug.Log($"[PASS] Manifest save/load roundtrip successful");
                        passed++;
                    }
                    else
                    {
                        Debug.LogError("[FAIL] Manifest save/load roundtrip failed");
                        failed++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[FAIL] Manifest save/load error: {e.Message}");
                    failed++;
                }

                // Test 8: Test version persistence
                Debug.Log("\n--- Test 8: Version Persistence ---");
                try
                {
                    PlayerPrefs.SetString("CurrentVersion", versionFile.version);
                    PlayerPrefs.Save();
                    string savedVersion = PlayerPrefs.GetString("CurrentVersion", "");
                    if (savedVersion == versionFile.version)
                    {
                        Debug.Log($"[PASS] Version persisted: {savedVersion}");
                        passed++;
                    }
                    else
                    {
                        Debug.LogError($"[FAIL] Version persistence failed");
                        failed++;
                    }
                }
                catch (System.Exception e)
                {
                    Debug.LogError($"[FAIL] Version persistence error: {e.Message}");
                    failed++;
                }
            }

            // Summary
            Debug.Log("\n========================================");
            Debug.Log($"  Results: {passed} passed, {failed} failed");
            Debug.Log("========================================");

            if (failed > 0)
            {
                Debug.LogError("[IntegrationTest] Some tests FAILED!");
            }
            else
            {
                Debug.Log("[IntegrationTest] All tests PASSED!");
            }

            Destroy(gameObject);
        }

        private bool IsNewerVersion(string newVersion, string oldVersion)
        {
            var newParts = newVersion.Split('.');
            var oldParts = oldVersion.Split('.');

            for (int i = 0; i < Mathf.Max(newParts.Length, oldParts.Length); i++)
            {
                int newNum = i < newParts.Length ? int.Parse(newParts[i]) : 0;
                int oldNum = i < oldParts.Length ? int.Parse(oldParts[i]) : 0;

                if (newNum > oldNum) return true;
                if (newNum < oldNum) return false;
            }
            return false;
        }

        private string CalculateMD5(string filePath)
        {
            using (var md5 = System.Security.Cryptography.MD5.Create())
            using (var stream = File.OpenRead(filePath))
            {
                byte[] hashBytes = md5.ComputeHash(stream);
                var builder = new System.Text.StringBuilder();
                foreach (byte b in hashBytes)
                {
                    builder.Append(b.ToString("x2"));
                }
                return builder.ToString();
            }
        }
    }
}