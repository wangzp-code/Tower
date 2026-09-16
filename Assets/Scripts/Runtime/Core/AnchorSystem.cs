using UnityEngine;
using System.Collections.Generic;

public class AnchorSystem : SingletonBase<AnchorSystem>
{

    public struct AnchorData
    {
        public int floor;
        public string name;
        public bool activated;
        public bool used;
        public System.DateTime activatedAt;
    }

    private List<AnchorData> anchors = new List<AnchorData>();
    private int currentAnchorFloor = 0;
    private const int MAX_ANCHORS = 3;

    protected override void Awake()
    {
        base.Awake();
        LoadAnchors();
    }

    protected override void OnDestroy()
    {
        base.OnDestroy();
    }

    protected override void OnDisable()
    {
        base.OnDisable();
    }

    void LoadAnchors()
    {
        var data = SaveSystem.Instance.LoadAnchorData();
        if (data != null)
        {
            anchors = data;
            UpdateCurrentAnchor();
        }
    }

    void SaveAnchors()
    {
        SaveSystem.Instance.SaveAnchorData(anchors);
    }

    public void ActivateAnchor(int floor)
    {
        var existing = anchors.Find(a => a.floor == floor);
        if (existing.floor == floor)
        {
            existing.activated = true;
            existing.used = false;
            existing.activatedAt = System.DateTime.Now;
            anchors[anchors.FindIndex(a => a.floor == floor)] = existing;
        }
        else
        {
            anchors.Add(new AnchorData
            {
                floor = floor,
                name = GetAnchorName(floor),
                activated = true,
                used = false,
                activatedAt = System.DateTime.Now
            });
        }

        // Keep only MAX_ANCHORS anchors, oldest first
        if (anchors.Count > MAX_ANCHORS)
        {
            anchors.Sort((a, b) => a.floor.CompareTo(b.floor));
            anchors.RemoveAt(0);
        }

        UpdateCurrentAnchor();
        SaveAnchors();
        CompleteGameSystem.Instance.AddCombatLog($"⛓️ 记忆锚定已激活 · F{floor}");
    }

    public void UseAnchor(int consecutiveDeaths = 0)
    {
        if (currentAnchorFloor <= 0)
        {
            CompleteGameSystem.Instance.AddCombatLog("✕ 没有可用的锚点");
            return;
        }

        var anchor = anchors.Find(a => a.floor == currentAnchorFloor);
        if (anchor.floor > 0)
        {
            anchor.used = true;
            anchors[anchors.FindIndex(a => a.floor == currentAnchorFloor)] = anchor;
            SaveAnchors();

            CompleteGameSystem.Instance.ReturnToAnchor(currentAnchorFloor, consecutiveDeaths);
        }
    }

    public void RemoveAnchor(int floor)
    {
        anchors.RemoveAll(a => a.floor == floor);
        UpdateCurrentAnchor();
        SaveAnchors();
    }

    void UpdateCurrentAnchor()
    {
        var activeAnchors = anchors.FindAll(a => a.activated && !a.used);
        if (activeAnchors.Count > 0)
        {
            activeAnchors.Sort((a, b) => b.floor.CompareTo(a.floor));
            currentAnchorFloor = activeAnchors[0].floor;
        }
        else
        {
            currentAnchorFloor = 0;
        }
    }

    public int GetCurrentAnchorFloor()
    {
        return currentAnchorFloor;
    }

    public bool HasActiveAnchor()
    {
        return currentAnchorFloor > 0;
    }

    public List<AnchorData> GetAllAnchors()
    {
        return new List<AnchorData>(anchors);
    }

    public void ClearAllAnchors()
    {
        anchors.Clear();
        currentAnchorFloor = 0;
        SaveAnchors();
    }

    public void OnDeath()
    {
        // 死亡回滚由 CompleteGameSystem.OnPlayerDefeated 处理（含EP惩罚逻辑）
    }

    public void OnFloorEnter(int floor)
    {
        // Check for existing altar
        if (SpecialFloorSystem.Instance != null && 
            SpecialFloorSystem.Instance.GetSpecialFloorType(floor) == SpecialFloorSystem.SpecialFloorType.Altar)
        {
            CompleteGameSystem.Instance.AddCombatLog("⛫ 发现回声祭坛！消耗200EP可激活锚点");
            CompleteGameSystem.Instance?.CheckTutorial("anchorHint");
        }
    }

    public void TryActivateAltar()
    {
        var p = GameManager.Instance.Player;
        int cost = 200;
        
        if (p.evolutionPoints >= cost)
        {
            p.evolutionPoints -= cost;
            ActivateAnchor(GameManager.Instance.CurrentFloor);
            CompleteGameSystem.Instance.AddCombatLog($"⛓️ 锚点已激活！消耗 {cost}EP");
        }
        else
        {
            CompleteGameSystem.Instance.AddCombatLog($"✕ EP不足！需要 {cost}EP");
        }
    }

    string GetAnchorName(int floor)
    {
        if (floor <= 10) return "浅层锚点";
        if (floor <= 25) return "中层锚点";
        if (floor <= 40) return "深层锚点";
        return "宿命锚点";
    }

    public void OnNewGame()
    {
        ClearAllAnchors();
    }
}
