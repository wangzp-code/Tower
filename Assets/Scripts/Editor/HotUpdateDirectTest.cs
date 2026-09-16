using UnityEngine;
using UnityEditor;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine.Networking;

public static class HotUpdateDirectTest
{
    [MenuItem("Tools/HotUpdate/Direct HTTP Test")]
    public static void RunDirectHttpTest()
    {
        Debug.Log("=== Hot Update Direct HTTP Test ===");
        
        var runner = new GameObject("HttpTestRunner");
        runner.AddComponent<HttpTestMono>();
    }
    
    private class HttpTestMono : MonoBehaviour
    {
        private void Start()
        {
            StartCoroutine(TestHttp());
        }
        
        private IEnumerator TestHttp()
        {
            string url = "http://127.0.0.1:8888/version.json";
            Debug.Log($"[Test] Requesting: {url}");
            
            using (var request = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                request.timeout = 10;
                
                Debug.Log($"[Test] Sending request...");
                yield return request.SendWebRequest();
                
                Debug.Log($"[Test] Result: {request.result}");
                Debug.Log($"[Test] ResponseCode: {request.responseCode}");
                Debug.Log($"[Test] Error: {request.error}");
                
                if (request.downloadHandler != null)
                {
                    Debug.Log($"[Test] DownloadHandler type: {request.downloadHandler.GetType().Name}");
                    Debug.Log($"[Test] DownloadHandler.data length: {request.downloadHandler.data?.Length ?? -1}");
                    Debug.Log($"[Test] DownloadHandler.text length: {request.downloadHandler.text?.Length ?? -1}");
                    
                    if (request.downloadHandler.data != null && request.downloadHandler.data.Length > 0)
                    {
                        string text = System.Text.Encoding.UTF8.GetString(request.downloadHandler.data);
                        Debug.Log($"[Test] Raw decoded text ({text.Length} chars): {text.Substring(0, Math.Min(200, text.Length))}");
                    }
                    
                    if (!string.IsNullOrEmpty(request.downloadHandler.text))
                    {
                        Debug.Log($"[Test] Text content: {request.downloadHandler.text.Substring(0, Math.Min(200, request.downloadHandler.text.Length))}");
                    }
                }
                else
                {
                    Debug.LogError("[Test] downloadHandler is NULL!");
                }
                
                if (request.result == UnityWebRequest.Result.Success && request.downloadHandler != null && !string.IsNullOrEmpty(request.downloadHandler.text))
                {
                    Debug.Log("[Test] SUCCESS: Got valid response!");
                }
                else
                {
                    Debug.LogError("[Test] FAILED: Could not get response from server");
                }
            }
            
            // Test with DownloadHandlerBuffer explicitly
            Debug.Log("\n--- Test 2: With explicit DownloadHandlerBuffer ---");
            using (var request2 = new UnityWebRequest(url, UnityWebRequest.kHttpVerbGET))
            {
                request2.timeout = 10;
                request2.downloadHandler = new DownloadHandlerBuffer();
                
                yield return request2.SendWebRequest();
                
                Debug.Log($"[Test2] Result: {request2.result}, Code: {request2.responseCode}");
                if (request2.downloadHandler != null)
                {
                    byte[] data = request2.downloadHandler.data;
                    string text = request2.downloadHandler.text;
                    Debug.Log($"[Test2] Data bytes: {data?.Length ?? 0}, Text length: {text?.Length ?? 0}");
                    
                    if (data != null && data.Length > 0)
                    {
                        Debug.Log($"[Test2] Content: {System.Text.Encoding.UTF8.GetString(data).Substring(0, Math.Min(200, data.Length))}");
                    }
                }
            }
            
            // Test with www class
            Debug.Log("\n--- Test 3: Using obsolete UnityWebRequest texture (test) ---");
            using (var request3 = UnityWebRequestTexture.GetTexture(url))
            {
                yield return request3.SendWebRequest();
                Debug.Log($"[Test3] Result: {request3.result}, Code: {request3.responseCode}");
                Debug.Log($"[Test3] DownloadHandler: {request3.downloadHandler?.GetType().Name}");
            }
            
            Debug.Log("\n=== Direct HTTP Test Complete ===");
            Destroy(gameObject);
        }
    }
}