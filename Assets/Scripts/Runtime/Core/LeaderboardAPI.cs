using System;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
using UnityEngine.Networking;
using UnityWebRequest = UnityEngine.Networking.UnityWebRequest;
using System.Threading.Tasks;

public class LeaderboardAPI : SingletonBase<LeaderboardAPI>
{

    private const string DEFAULT_BASE = "";

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    private string GetBase()
    {
        if (!string.IsNullOrEmpty(PlayerPrefs.GetString("PT_API_BASE")))
            return PlayerPrefs.GetString("PT_API_BASE").TrimEnd('/');
        return DEFAULT_BASE;
    }

    public async Task<LeaderboardResponse> FetchLeaderboard(string type, string period = null, string cls = null, int limit = 50)
    {
        string baseUrl = GetBase();
        if (string.IsNullOrEmpty(baseUrl))
            throw new Exception("No API base configured");

        string url = $"{baseUrl}/api/v1/leaderboard?type={type}&limit={limit}";
        if (!string.IsNullOrEmpty(period))
            url += $"&period={period}";
        if (!string.IsNullOrEmpty(cls))
            url += $"&class={cls}";

        using (var request = UnityWebRequest.Get(url))
        {
            request.timeout = 6;
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"HTTP {request.responseCode}");

            string json = request.downloadHandler.text;
            try
            {
                return JsonUtility.FromJson<LeaderboardResponse>(json);
            }
            catch
            {
                var response = new LeaderboardResponse();
                try
                {
                    var wrapper = JsonUtility.FromJson<RowsWrapper>(json);
                    response.rows = wrapper.rows;
                }
                catch
                {
                    response.rows = JsonHelper.FromJsonArray<LeaderboardRow>(json);
                }
                return response;
            }
        }
    }

    [Serializable]
    class RowsWrapper
    {
        public List<LeaderboardRow> rows;
    }

    public async Task<PlayerLeaderboardData> FetchMyLeaderboard(string uid)
    {
        string baseUrl = GetBase();
        if (string.IsNullOrEmpty(baseUrl))
            throw new Exception("No API base configured");

        string url = $"{baseUrl}/api/v1/me?uid={UnityWebRequest.EscapeURL(uid)}";

        using (var request = UnityWebRequest.Get(url))
        {
            request.timeout = 6;
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"HTTP {request.responseCode}");

            return JsonUtility.FromJson<PlayerLeaderboardData>(request.downloadHandler.text);
        }
    }

    public async Task<(bool ok, string reason)> CheckNickname(string name, string uid = null)
    {
        string baseUrl = GetBase();
        if (string.IsNullOrEmpty(baseUrl))
            return (false, "offline");

        string url = $"{baseUrl}/api/v1/nickname/check?name={UnityWebRequest.EscapeURL(name ?? "")}";
        if (!string.IsNullOrEmpty(uid))
            url += $"&uid={UnityWebRequest.EscapeURL(uid)}";

        using (var request = UnityWebRequest.Get(url))
        {
            request.timeout = 6;
            var operation = request.SendWebRequest();

            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                return (false, $"http {request.responseCode}");

            var response = JsonUtility.FromJson<NicknameCheckResponse>(request.downloadHandler.text);
            return (response.available, response.reason);
        }
    }

    public async Task<(bool ok, string nickname, string reason)> ClaimNickname(string uid, string name)
    {
        string baseUrl = GetBase();
        if (string.IsNullOrEmpty(baseUrl))
            return (false, "", "offline");

        string url = $"{baseUrl}/api/v1/nickname/claim";
        var body = JsonUtility.ToJson(new { uid = uid, nickname = name });

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 6;

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                return (false, "", $"http {request.responseCode}");

            var response = JsonUtility.FromJson<NicknameClaimResponse>(request.downloadHandler.text);
            if (request.responseCode == 200)
                return (true, response.nickname, "");
            return (false, "", response.error);
        }
    }

    public async Task SubmitRun(RunData run)
    {
        string baseUrl = GetBase();
        if (string.IsNullOrEmpty(baseUrl))
            throw new Exception("No API base configured");

        string url = $"{baseUrl}/api/v1/runs";
        string body = JsonUtility.ToJson(run);

        using (var request = new UnityWebRequest(url, "POST"))
        {
            byte[] bodyRaw = Encoding.UTF8.GetBytes(body);
            request.uploadHandler = new UploadHandlerRaw(bodyRaw);
            request.downloadHandler = new DownloadHandlerBuffer();
            request.SetRequestHeader("Content-Type", "application/json");
            request.timeout = 10;

            var operation = request.SendWebRequest();
            while (!operation.isDone)
                await Task.Yield();

            if (request.result != UnityWebRequest.Result.Success)
                throw new Exception($"HTTP {request.responseCode}");
        }
    }

    public bool HasAPI()
    {
        return !string.IsNullOrEmpty(GetBase());
    }
}

[Serializable]
public class LeaderboardResponse
{
    public List<LeaderboardRow> rows;
}

[Serializable]
public class LeaderboardRow
{
    public int rank;
    public string rating;
    public int score;
    public int floor;
    public float duration;
    public string nickname;
    public string uid;
}

[Serializable]
public class PlayerLeaderboardData
{
    public string nickname;
    public List<PlayerBoardEntry> boards;
}

[Serializable]
public class PlayerBoardEntry
{
    public string type;
    public string sub;
    public int rank;
    public int total;
    public int percentile;
    public int bestScore;
    public float bestDur;
}

[Serializable]
public class NicknameCheckResponse
{
    public bool available;
    public string reason;
}

[Serializable]
public class NicknameClaimResponse
{
    public string nickname;
    public string error;
}

[Serializable]
public class RunData
{
    public string uid;
    public string nickname;
    public string mode;
    public string playerClass;
    public int floor;
    public int score;
    public float duration;
    public string rating;
    public int pollution;
    public int possessions;
    public int kills;
    public string seed;
    public string version;
}