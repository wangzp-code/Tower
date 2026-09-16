using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using System.Threading.Tasks;

public partial class CanvasUIManager : MonoBehaviour
{
    private GameObject _posterOverlay;
    private RawImage _posterImage;
    private Texture2D _currentPosterTexture;
    private bool _isSharing = false;
    private bool _isSaving = false;
    
    public void ShowPosterPreview(Texture2D posterTexture)
    {
        if (posterTexture == null)
        {
            Debug.LogError("[CanvasUIManager] ShowPosterPreview: posterTexture is null");
            return;
        }

        // Reuse existing overlay if present to avoid allocations.
        if (_posterOverlay != null)
        {
            _currentPosterTexture = posterTexture;
            if (_posterImage != null) _posterImage.texture = posterTexture;
            _posterOverlay.SetActive(true);
            _posterOverlay.transform.SetAsLastSibling();
            return;
        }

        _currentPosterTexture = posterTexture;

        _posterOverlay = new GameObject("PosterOverlay");
        _posterOverlay.transform.SetParent(transform, false);

        var overlayRect = _posterOverlay.AddComponent<RectTransform>();
        overlayRect.anchorMin = Vector2.zero;
        overlayRect.anchorMax = Vector2.one;
        overlayRect.sizeDelta = Vector2.zero;

        var overlayImage = _posterOverlay.AddComponent<Image>();
        overlayImage.color = new Color(0, 0, 0, 0.85f);
        overlayImage.raycastTarget = true;

        var content = new GameObject("PosterContent");
        content.transform.SetParent(_posterOverlay.transform, false);

        var contentRect = content.AddComponent<RectTransform>();
        contentRect.anchorMin = new Vector2(0.5f, 0.5f);
        contentRect.anchorMax = new Vector2(0.5f, 0.5f);
        contentRect.pivot = new Vector2(0.5f, 0.5f);
        contentRect.sizeDelta = new Vector2(400, 711);

        _posterImage = content.AddComponent<RawImage>();
        if (_posterImage == null)
        {
            Debug.LogError("[CanvasUIManager] Failed to add RawImage component");
            return;
        }
        _posterImage.texture = posterTexture;
        _posterImage.uvRect = new Rect(0, 0, 1, 1);

        var border = content.AddComponent<Image>();
        if (border != null)
        {
            border.color = Color.clear;
            var defaultShader = Shader.Find("UI/Default");
            if (defaultShader != null)
            {
                border.material = new Material(defaultShader);
                border.material.SetColor("_Color", new Color(1, 1, 1, 0.1f));
            }
        }

        var buttonContainer = new GameObject("ButtonContainer");
        buttonContainer.transform.SetParent(content.transform, false);

        var buttonRect = buttonContainer.AddComponent<RectTransform>();
        buttonRect.anchorMin = new Vector2(0, 0);
        buttonRect.anchorMax = new Vector2(1, 0);
        buttonRect.pivot = new Vector2(0.5f, 1);
        buttonRect.anchoredPosition = new Vector2(0, -10);
        buttonRect.sizeDelta = new Vector2(0, 60);

        var horizontalLayout = buttonContainer.AddComponent<HorizontalLayoutGroup>();
        horizontalLayout.spacing = 16;
        horizontalLayout.childAlignment = TextAnchor.MiddleCenter;
        horizontalLayout.childForceExpandWidth = true;

        var closeBtn = CreateButton(buttonContainer.transform, "关闭", Color.gray, () => HidePosterPreview());
        var saveBtn = CreateButton(buttonContainer.transform, "保存到相册", new Color(0.11f, 0.85f, 0.69f), () => SavePoster());
        var shareBtn = CreateButton(buttonContainer.transform, "分享海报", new Color(0.66f, 0.33f, 0.97f), () => SharePoster());

        _posterOverlay.SetActive(true);
    }
    
    private GameObject CreateButton(Transform parent, string text, Color accent, System.Action onClick)
    {
        var btn = BtnGo(parent, text, 14, accent, 40);
        btn.GetComponent<Button>().onClick.AddListener(() => onClick?.Invoke());
        return btn;
    }
    
    private void HidePosterPreview()
    {
        DestroyCurrentTexture();
        if (_posterOverlay != null)
        {
            Destroy(_posterOverlay);
            _posterOverlay = null;
            _posterImage = null;
        }
    }
    
    private void SavePoster()
    {
        if (_isSaving)
        {
            ShowNotification("正在保存中...");
            return;
        }
        if (_currentPosterTexture == null)
        {
            ShowNotification("海报数据不可用");
            return;
        }
        
        _isSaving = true;
        ShowNotification("正在保存海报...");
        
        try
        {
            byte[] bytes = _currentPosterTexture.EncodeToPNG();
            string path = Application.persistentDataPath + "/poster_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            
            Task.Run(() =>
            {
                try
                {
                    System.IO.File.WriteAllBytes(path, bytes);
                    Debug.Log("Poster saved: " + path + " (" + bytes.Length + " bytes)");

                    RunOnMainThread(() =>
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                        try
                        {
                            var unityPlayerClass = System.Type.GetType("UnityEngine.AndroidJavaClass, UnityEngine.AndroidJNIModule");
                            if (unityPlayerClass != null)
                            {
                                var unityPlayer = System.Activator.CreateInstance(unityPlayerClass, "com.unity3d.player.UnityPlayer");
                                var getStaticMethod = unityPlayerClass.GetMethod("GetStatic");
                                var currentActivity = getStaticMethod.MakeGenericMethod(System.Type.GetType("UnityEngine.AndroidJavaObject, UnityEngine.AndroidJNIModule"))
                                    .Invoke(unityPlayer, new object[] { "currentActivity" });
                                
                                var mediaScanner = System.Activator.CreateInstance(unityPlayerClass, "android.media.MediaScannerConnection");
                                var callStaticMethod = unityPlayerClass.GetMethod("CallStatic");
                                callStaticMethod.Invoke(mediaScanner, new object[] { "scanFile", currentActivity, new string[] { path }, new string[] { "image/png" }, null });
                                
                                System.IDisposable scannerDisposable = mediaScanner as System.IDisposable;
                                scannerDisposable?.Dispose();
                                System.IDisposable playerDisposable = unityPlayer as System.IDisposable;
                                playerDisposable?.Dispose();
                            }
                        }
                        catch { }
                        ShowNotification("海报已保存到相册");
#elif UNITY_IOS
                        UnityEngine.iOS.Device.SetNoBackupFlag(path);
                        ShowNotification("海报已保存到相册");
#else
                        ShowNotification("海报已保存到: " + path);
#endif
                        _isSaving = false;
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    _isSaving = false;
                    RunOnMainThread(() => ShowNotification("保存海报失败: " + ex.Message));
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            _isSaving = false;
            ShowNotification("保存海报失败: " + ex.Message);
        }
    }
    
    private void SharePoster()
    {
        if (_isSharing)
        {
            ShowNotification("正在分享中...");
            return;
        }
        if (_currentPosterTexture == null)
        {
            ShowNotification("海报数据不可用");
            return;
        }
        
        _isSharing = true;
        string shareText = "我在《你也是我》中达成了新成就！#ParasiteTower #你也是我";
        
        ShowNotification("正在生成分享图片...");
        
        try
        {
            byte[] bytes = _currentPosterTexture.EncodeToPNG();
            string path = Application.persistentDataPath + "/poster_share_" + System.DateTime.Now.ToString("yyyyMMdd_HHmmss") + ".png";
            
            Task.Run(() =>
            {
                try
                {
                    System.IO.File.WriteAllBytes(path, bytes);

                    RunOnMainThread(() =>
                    {
#if UNITY_ANDROID && !UNITY_EDITOR
                        try
                        {
                            string base64 = System.Convert.ToBase64String(bytes);
                            var unityPlayerClass = System.Type.GetType("UnityEngine.AndroidJavaClass, UnityEngine.AndroidJNIModule");
                            if (unityPlayerClass != null)
                            {
                                var unityPlayer = System.Activator.CreateInstance(unityPlayerClass, "com.unity3d.player.UnityPlayer");
                                var getStaticMethod = unityPlayerClass.GetMethod("GetStatic");
                                var currentActivity = getStaticMethod.MakeGenericMethod(System.Type.GetType("UnityEngine.AndroidJavaObject, UnityEngine.AndroidJNIModule"))
                                    .Invoke(unityPlayer, new object[] { "currentActivity" });
                                
                                var androidJavaObjectClass = System.Type.GetType("UnityEngine.AndroidJavaObject, UnityEngine.AndroidJNIModule");
                                var shareBridge = System.Activator.CreateInstance(androidJavaObjectClass, "com.parasite.tower.ShareBridge", currentActivity);
                                var callMethod = androidJavaObjectClass.GetMethod("Call");
                                callMethod.Invoke(shareBridge, new object[] { "shareImage", "data:image/png;base64," + base64, shareText });
                                
                                System.IDisposable bridgeDisposable = shareBridge as System.IDisposable;
                                bridgeDisposable?.Dispose();
                                System.IDisposable playerDisposable = unityPlayer as System.IDisposable;
                                playerDisposable?.Dispose();
                                
                                ShowNotification("分享已启动");
                            }
                            else
                            {
                                FallbackShare(path, shareText);
                            }
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                            FallbackShare(path, shareText);
                        }
#elif UNITY_IOS
                        try
                        {
                            UnityEngine.iOS.Device.ShareText(shareText, path);
                            ShowNotification("分享已启动");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogException(ex);
                            FallbackShare(path, shareText);
                        }
#else
                        FallbackShare(path, shareText);
#endif
#if !UNITY_ANDROID && !UNITY_IOS
                        #else
                        HidePosterPreview();
                        #endif
                        _isSharing = false;
                    });
                }
                catch (Exception ex)
                {
                    Debug.LogException(ex);
                    _isSharing = false;
                    RunOnMainThread(() => ShowNotification("分享失败: " + ex.Message));
                }
            });
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            _isSharing = false;
            ShowNotification("分享失败: " + ex.Message);
        }
    }
    
    private void FallbackShare(string path, string shareText)
    {
        byte[] bytes = System.IO.File.ReadAllBytes(path);
        string base64 = System.Convert.ToBase64String(bytes);
        Debug.Log("Fallback Share - Path: " + path + ", Size: " + bytes.Length);
        ShowNotification("海报已保存，请手动分享");
    }
    
    public void ShareEndingPoster(string playerClass)
    {
        if (PosterShareSystem.Instance == null)
        {
            ShowNotification("海报分享系统未初始化");
            return;
        }

        DestroyCurrentTexture();
        
        Texture2D poster = PosterShareSystem.Instance.BuildEndingPoster(playerClass);
        if (poster == null)
        {
            ShowNotification("海报生成失败");
            return;
        }
        ShowPosterPreview(poster);
    }
    
    public void ShareRunReportPoster(CompleteGameSystem.Report report, string mode = "short")
    {
        if (report == null)
        {
            ShowNotification("死亡报告数据不可用");
            return;
        }
        if (PosterShareSystem.Instance == null)
        {
            ShowNotification("海报分享系统未初始化");
            return;
        }

        DestroyCurrentTexture();

        Texture2D poster = PosterShareSystem.Instance.BuildRunReportPoster(report, mode);
        if (poster == null)
        {
            ShowNotification("海报生成失败");
            return;
        }
        ShowPosterPreview(poster);
    }
    
    public void ShareAchievementPoster(PosterShareSystem.AchievementDef achievement)
    {
        if (PosterShareSystem.Instance == null)
        {
            ShowNotification("海报分享系统未初始化");
            return;
        }

        DestroyCurrentTexture();

        Texture2D poster = PosterShareSystem.Instance.BuildAchievementPoster(achievement);
        if (poster == null)
        {
            ShowNotification("海报生成失败");
            return;
        }
        ShowPosterPreview(poster);
    }
    
    private void DestroyCurrentTexture()
    {
        if (_currentPosterTexture != null)
        {
            Destroy(_currentPosterTexture);
            _currentPosterTexture = null;
        }
    }
    
    private void ShowNotification(string message)
    {
        ShowFlashBanner(message, Color.white, 1.2f);
    }
}