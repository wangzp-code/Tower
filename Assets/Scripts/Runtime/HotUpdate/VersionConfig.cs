using UnityEngine;
using System.Collections.Generic;
using System;

namespace HotUpdate
{
    [Serializable]
    public class BundleInfo
    {
        public string name;
        public long size;
        public string hash;
        public string version;
    }
    
    [Serializable]
    public class ManifestData
    {
        public string version;
        public long timestamp;
        public List<BundleInfo> bundles = new List<BundleInfo>();
        
        public BundleInfo GetBundleInfo(string bundleName)
        {
            return bundles.Find(b => b.name == bundleName);
        }
    }
    
    [Serializable]
    public class VersionFile
    {
        public string version;
        public string minVersion;
        public string updateUrl;
        public string downloadUrl;
        public bool forceUpdate;
        public string releaseNotes;
        public long timestamp;
    }
    
    [Serializable]
    public class DownloadTask
    {
        public string bundleName;
        public string hash;
        public long size;
        public string downloadUrl;
        public string localPath;
        public float progress;
        public bool isComplete;
        public bool isFailed;
        public string errorMessage;
        public int completedBundles;
        public int totalBundles;
    }
    
    [Serializable]
    public class HotUpdateConfig
    {
        public string baseUrl = "https://your-server.com/bundles/";
        public string versionFileName = "version.json";
        public string manifestFileName = "manifest.json";
        public string localBundlePath = "Bundles/";
        public int maxDownloadThreads = 3;
        public float checkInterval = 3600f;
        public bool autoDownloadOnWifiOnly = false;
        public bool enabled = false;
    }
    
    public enum HotUpdateState
    {
        Idle,
        Checking,
        Downloading,
        Updating,
        Complete,
        Error
    }
    
    public class HotUpdateEventArgs : EventArgs
    {
        public string message;
        public float progress;
        public int totalBundles;
        public int completedBundles;
        public string currentBundle;
        public string errorMessage;
        
        public static HotUpdateEventArgs CheckComplete(bool hasUpdate, string version)
        {
            return new HotUpdateEventArgs
            {
                message = hasUpdate ? $"发现新版本: {version}" : "已是最新版本",
                progress = hasUpdate ? 0.5f : 1f
            };
        }
        
        public static HotUpdateEventArgs DownloadProgress(string bundle, int completed, int total, float progress)
        {
            return new HotUpdateEventArgs
            {
                message = $"下载中: {bundle} ({completed}/{total})",
                progress = progress,
                currentBundle = bundle,
                completedBundles = completed,
                totalBundles = total
            };
        }
        
        public static HotUpdateEventArgs UpdateComplete()
        {
            return new HotUpdateEventArgs
            {
                message = "热更新完成！",
                progress = 1f
            };
        }
        
        public static HotUpdateEventArgs Error(string error)
        {
            return new HotUpdateEventArgs
            {
                message = $"更新失败: {error}",
                errorMessage = error
            };
        }
    }
}
