using UnityEngine;
using UnityEngine.UI;
using System;

namespace HotUpdate
{
    public class UpdateUI : MonoBehaviour
    {
        [Header("Panel")]
        public GameObject updatePanel;
        public GameObject downloadingPanel;
        public GameObject completePanel;
        public GameObject errorPanel;
        
        [Header("Version Info")]
        public Text currentVersionText;
        public Text latestVersionText;
        public Text releaseNotesText;
        
        [Header("Download Progress")]
        public Slider progressSlider;
        public Text progressText;
        public Text downloadedBundleText;
        public Text totalBundleText;
        public Button pauseButton;
        public Button cancelButton;
        
        [Header("Buttons")]
        public Button downloadButton;
        public Button skipButton;
        public Button retryButton;
        public Button closeButton;
        public Text skipButtonText;
        
        private HotUpdateManager hotUpdateManager;
        private bool isPaused = false;
        
        void Start()
        {
            hotUpdateManager = HotUpdateManager.Instance;
            
            if (hotUpdateManager != null)
            {
                hotUpdateManager.OnStateChanged += HandleStateChanged;
                hotUpdateManager.OnProgressChanged += HandleProgressChanged;
                hotUpdateManager.OnUpdateAvailable += HandleUpdateAvailable;
                hotUpdateManager.OnUpdateComplete += HandleUpdateComplete;
                hotUpdateManager.OnUpdateFailed += HandleUpdateFailed;
            }
            
            InitializeUI();
        }
        
        void InitializeUI()
        {
            if (updatePanel != null) updatePanel.SetActive(false);
            if (downloadingPanel != null) downloadingPanel.SetActive(false);
            if (completePanel != null) completePanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(false);
            
            if (downloadButton != null) downloadButton.onClick.AddListener(StartUpdate);
            if (skipButton != null) skipButton.onClick.AddListener(SkipUpdate);
            if (retryButton != null) retryButton.onClick.AddListener(RetryUpdate);
            if (closeButton != null) closeButton.onClick.AddListener(ClosePanel);
            if (pauseButton != null) pauseButton.onClick.AddListener(TogglePause);
            if (cancelButton != null) cancelButton.onClick.AddListener(CancelUpdate);
        }
        
        private void HandleUpdateAvailable(object sender, bool hasUpdate)
        {
            if (hasUpdate)
            {
                ShowUpdateAvailable();
            }
        }
        
        private void ShowUpdateAvailable()
        {
            if (updatePanel != null) updatePanel.SetActive(true);
            if (downloadingPanel != null) downloadingPanel.SetActive(false);
            if (completePanel != null) completePanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(false);
            
            if (currentVersionText != null)
            {
                currentVersionText.text = $"当前版本: {hotUpdateManager.currentVersion}";
            }
            
            if (latestVersionText != null)
            {
                latestVersionText.text = $"最新版本: {hotUpdateManager.LatestVersion}";
            }
            
            if (releaseNotesText != null)
            {
                releaseNotesText.text = hotUpdateManager.GetReleaseNotes();
            }
            
            if (skipButton != null)
            {
                bool isForce = hotUpdateManager.IsForceUpdate();
                skipButton.gameObject.SetActive(!isForce);
                skipButtonText.text = isForce ? "强制更新" : "跳过";
                skipButton.interactable = !isForce;
            }
        }
        
        private void StartUpdate()
        {
            if (downloadingPanel != null) downloadingPanel.SetActive(true);
            if (updatePanel != null) updatePanel.SetActive(false);
            
            if (progressSlider != null) progressSlider.value = 0;
            if (progressText != null) progressText.text = "准备下载...";
            
            hotUpdateManager?.StartCoroutine(hotUpdateManager.CheckForUpdate());
        }
        
        private void HandleProgressChanged(object sender, HotUpdateEventArgs e)
        {
            if (downloadingPanel == null || !downloadingPanel.activeSelf) return;
            
            if (progressSlider != null)
            {
                progressSlider.value = e.progress;
            }
            
            if (progressText != null)
            {
                progressText.text = $"{(e.progress * 100).ToString("F0")}%";
            }
            
            if (downloadedBundleText != null)
            {
                downloadedBundleText.text = $"已下载: {e.completedBundles}/{e.totalBundles}";
            }
            
            if (totalBundleText != null && !string.IsNullOrEmpty(e.currentBundle))
            {
                totalBundleText.text = $"当前: {e.currentBundle}";
            }
        }
        
        private void HandleStateChanged(object sender, HotUpdateEventArgs e)
        {
            if (hotUpdateManager.State == HotUpdateState.Downloading)
            {
                if (downloadingPanel != null) downloadingPanel.SetActive(true);
            }
        }
        
        private void HandleUpdateComplete(object sender, EventArgs e)
        {
            if (downloadingPanel != null) downloadingPanel.SetActive(false);
            if (completePanel != null) completePanel.SetActive(true);
            
            if (closeButton != null)
            {
                closeButton.gameObject.SetActive(true);
                closeButton.onClick.AddListener(CloseAndRestart);
            }
        }
        
        private void HandleUpdateFailed(object sender, string errorMessage)
        {
            if (downloadingPanel != null) downloadingPanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(true);
            
            Debug.LogError($"[UpdateUI] Update failed: {errorMessage}");
        }
        
        private void SkipUpdate()
        {
            ClosePanel();
        }
        
        private void RetryUpdate()
        {
            if (errorPanel != null) errorPanel.SetActive(false);
            StartUpdate();
        }
        
        private void TogglePause()
        {
            if (isPaused)
            {
                hotUpdateManager?.ResumeDownload();
                pauseButton.GetComponentInChildren<Text>().text = "暂停";
            }
            else
            {
                hotUpdateManager?.PauseDownload();
                pauseButton.GetComponentInChildren<Text>().text = "继续";
            }
            isPaused = !isPaused;
        }
        
        private void CancelUpdate()
        {
            hotUpdateManager?.CancelDownload();
            ClosePanel();
        }
        
        private void ClosePanel()
        {
            if (updatePanel != null) updatePanel.SetActive(false);
            if (downloadingPanel != null) downloadingPanel.SetActive(false);
            if (completePanel != null) completePanel.SetActive(false);
            if (errorPanel != null) errorPanel.SetActive(false);
        }
        
        private void CloseAndRestart()
        {
            ClosePanel();
            #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
            #else
            UnityEngine.SceneManagement.SceneManager.LoadScene(0);
            #endif
        }
        
        void OnDestroy()
        {
            if (hotUpdateManager != null)
            {
                hotUpdateManager.OnStateChanged -= HandleStateChanged;
                hotUpdateManager.OnProgressChanged -= HandleProgressChanged;
                hotUpdateManager.OnUpdateAvailable -= HandleUpdateAvailable;
                hotUpdateManager.OnUpdateComplete -= HandleUpdateComplete;
                hotUpdateManager.OnUpdateFailed -= HandleUpdateFailed;
            }
        }
    }
}
