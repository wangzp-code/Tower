using UnityEngine;
using System;
using System.IO;

public class GuestAuthManager : SingletonBase<GuestAuthManager>
{

    [Serializable]
    public class GuestProfile
    {
        public string guestId;
        public string nickname;
        public int loginCount;
        public string firstLoginTime;
        public string lastLoginTime;
    }

    public GuestProfile Profile { get; private set; }
    public bool IsLoggedIn => Profile != null;

    protected override void Awake()
    {
        base.Awake();
        TryAutoLogin();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    void TryAutoLogin()
    {
        string path = GetProfilePath();
        if (File.Exists(path))
        {
            try
            {
                string json = File.ReadAllText(path);
                Profile = JsonUtility.FromJson<GuestProfile>(json);
                Profile.loginCount++;
                Profile.lastLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
                SaveProfile();
            }
            catch
            {
                Profile = null;
            }
        }
    }

    public const int MAX_NICKNAME_BYTES = 24;

    public void GuestLogin(string customNickname = null)
    {
        int id = UnityEngine.Random.Range(100000, 999999);
        string nick = string.IsNullOrEmpty(customNickname) ? $"寄生体{id}" : TruncateByBytes(customNickname.Trim(), MAX_NICKNAME_BYTES);
        Profile = new GuestProfile
        {
            guestId = id.ToString(),
            nickname = nick,
            loginCount = 1,
            firstLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
            lastLoginTime = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss")
        };
        SaveProfile();
    }

    public void UpdateNickname(string newNick)
    {
        if (Profile == null) return;
        Profile.nickname = TruncateByBytes(newNick.Trim(), MAX_NICKNAME_BYTES);
        SaveProfile();
    }

    static string TruncateByBytes(string s, int maxBytes)
    {
        if (string.IsNullOrEmpty(s)) return s;
        int bytes = 0;
        for (int i = 0; i < s.Length; i++)
        {
            bytes += s[i] > 127 ? 3 : 1;
            if (bytes > maxBytes) return s.Substring(0, i);
        }
        return s;
    }

    public void Logout()
    {
        Profile = null;
        string dir = Path.Combine(Application.persistentDataPath, "Saves");
        if (Directory.Exists(dir))
        {
            foreach (var f in Directory.GetFiles(dir))
                File.Delete(f);
        }

        PlayerPrefs.DeleteKey("pt_daily_last_date");
        PlayerPrefs.DeleteKey("pt_daily_login_days");
        PlayerPrefs.DeleteKey("pt_daily_last_claim");
        PlayerPrefs.DeleteKey("pt_meta_echoes");
        PlayerPrefs.DeleteKey("pt_save_classic");
        PlayerPrefs.DeleteKey("pt_save_short");
        PlayerPrefs.DeleteKey("pt_save_expedition");
        PlayerPrefs.DeleteKey("pt_affinity");
        PlayerPrefs.DeleteKey("PT_UID");
        PlayerPrefs.DeleteKey("PT_NICKNAME");
        PlayerPrefs.Save();

        if (MetaProgressSystem.Instance != null)
            MetaProgressSystem.Instance.ResetProgress();
    }

    void SaveProfile()
    {
        if (Profile == null) return;
        string dir = Path.Combine(Application.persistentDataPath, "Saves");
        if (!Directory.Exists(dir)) Directory.CreateDirectory(dir);
        string json = JsonUtility.ToJson(Profile, true);
        File.WriteAllText(GetProfilePath(), json);
    }

    string GetProfilePath()
    {
        return Path.Combine(Application.persistentDataPath, "Saves", "guest_profile.json");
    }
}
